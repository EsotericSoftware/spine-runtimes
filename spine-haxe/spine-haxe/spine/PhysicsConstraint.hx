/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*****************************************************************************/

package spine;

/** Stores the current pose for a physics constraint. A physics constraint applies physics to bones.
 *
 *
 * @see https://esotericsoftware.com/spine-physics-constraints Physics constraints in the Spine User Guide
 */
class PhysicsConstraint extends Constraint<PhysicsConstraint, PhysicsConstraintData, PhysicsConstraintPose> {
	/** The bone constrained by this physics constraint. */
	public var bone:BonePose = null;

	public var _reset = true;

	public var ux = 0.;
	public var uy = 0.;
	public var cx = 0.;
	public var cy = 0.;
	public var tx = 0.;
	public var ty = 0.;
	public var xOffset = 0.;
	public var xLag = 0.;
	public var xVelocity = 0.;
	public var yOffset = 0.;
	public var yLag = 0.;
	public var yVelocity = 0.;
	public var rotateOffset = 0.;
	public var rotateLag = 0.;
	public var rotateVelocity = 0.;
	public var scaleOffset = 0.;
	public var scaleLag = 0.;
	public var scaleVelocity = 0.;
	public var remaining = 0.;
	public var lastTime = 0.;

	public function new(data:PhysicsConstraintData, skeleton:Skeleton) {
		super(data, new PhysicsConstraintPose(), new PhysicsConstraintPose());
		if (skeleton == null)
			throw new SpineException("skeleton cannot be null.");

		bone = skeleton.bones[data.bone.index].constrainedPose;
	}

	public function copy(skeleton:Skeleton) {
		var copy = new PhysicsConstraint(data, skeleton);
		copy.pose.set(pose);
		return copy;
	}

	public function reset(skeleton:Skeleton) {
		remaining = 0;
		lastTime = skeleton.time;
		_reset = true;
		xOffset = 0;
		xLag = 0;
		xVelocity = 0;
		yOffset = 0;
		yLag = 0;
		yVelocity = 0;
		rotateOffset = 0;
		rotateLag = 0;
		rotateVelocity = 0;
		scaleOffset = 0;
		scaleLag = 0;
		scaleVelocity = 0;
	}

	/** Translates the physics constraint so next update(Physics) forces are applied as if the bone moved an additional
	 * amount in world space. */
	public function translate(x:Float, y:Float):Void {
		ux -= x;
		uy -= y;
		cx -= x;
		cy -= y;
	}

	/** Rotates the physics constraint so next update(Physics) forces are applied as if the bone rotated around the
	 * specified point in world space. */
	public function rotate(x:Float, y:Float, degrees:Float):Void {
		var r = degrees * MathUtils.degRad,
			cos = Math.cos(r),
			sin = Math.sin(r);
		var dx = cx - x, dy = cy - y;
		translate(dx * cos - dy * sin - dx, dx * sin + dy * cos - dy);
	}

	/** Applies the constraint to the constrained bones. */
	public function update(skeleton:Skeleton, physics:Physics):Void {
		var p = appliedPose;
		var mix = p.mix;
		if (mix == 0)
			return;

		var x = data.x > 0,
			y = data.y > 0,
			rotateOrShearX = data.rotate > 0 || data.shearX > 0,
			scaleX = data.scaleX > 0;
		var l = bone.bone.data.length, t = data.step, z = 0.;

		switch (physics) {
			case Physics.none:
				return;
			case Physics.reset, Physics.update:
				if (physics == Physics.reset)
					reset(skeleton);

				var delta = Math.max(skeleton.time - lastTime, 0), aa = remaining;
				remaining += delta;
				lastTime = skeleton.time;

				var bx = bone.worldX, by = bone.worldY;
				if (_reset) {
					_reset = false;
					ux = bx;
					uy = by;
				} else {
					var a = remaining, i = p.inertia, f = skeleton.data.referenceScale, d = -1., m = 0., e = 0., qx = data.limit * delta,
						qy = qx * Math.abs(skeleton.scaleY);
					qx *= Math.abs(skeleton.scaleX);
					if (x || y) {
						if (x) {
							var u = (ux - bx) * i;
							xOffset += u > qx ? qx : u < -qx ? -qx : u;
							ux = bx;
						}
						if (y) {
							var u = (uy - by) * i;
							yOffset += u > qy ? qy : u < -qy ? -qy : u;
							uy = by;
						}
						if (a >= t) {
							var xs = xOffset, ys = yOffset;
							d = Math.pow(p.damping, 60 * t);
							m = t * p.massInverse;
							e = p.strength;
							var w = f * p.wind, g = f * p.gravity;
							var ax = (w * skeleton.windX + g * skeleton.gravityX) * skeleton.scaleX;
							var ay = (w * skeleton.windY + g * skeleton.gravityY) * skeleton.scaleY;
							do {
								if (x) {
									xVelocity += (ax - xOffset * e) * m;
									xOffset += xVelocity * t;
									xVelocity *= d;
								}
								if (y) {
									yVelocity -= (ay + yOffset * e) * m;
									yOffset += yVelocity * t;
									yVelocity *= d;
								}
								a -= t;
							} while (a >= t);
							xLag = xOffset - xs;
							yLag = yOffset - ys;
						}
						z = Math.max(0, 1 - a / t);
						if (x)
							bone.worldX += (xOffset - xLag * z) * mix * data.x;
						if (y)
							bone.worldY += (yOffset - yLag * z) * mix * data.y;
					}
					if (rotateOrShearX || scaleX) {
						var ca = Math.atan2(bone.c, bone.a), c = 0., s = 0., mr = 0., dx = cx - bone.worldX, dy = cy - bone.worldY;
						if (dx > qx)
							dx = qx;
						else if (dx < -qx) //
							dx = -qx;
						if (dy > qy)
							dy = qy;
						else if (dy < -qy) //
							dy = -qy;
						a = remaining;
						if (rotateOrShearX) {
							mr = (data.rotate + data.shearX) * mix;
							z = rotateLag * Math.max(0, 1 - aa / t);
							var r = Math.atan2(dy + ty, dx + tx) - ca - (rotateOffset - z) * mr;
							rotateOffset += (r - Math.ceil(r * MathUtils.invPI2 - 0.5) * MathUtils.PI2) * i;
							r = (rotateOffset - z) * mr + ca;
							c = Math.cos(r);
							s = Math.sin(r);
							if (scaleX) {
								r = l * bone.worldScaleX;
								if (r > 0)
									scaleOffset += (dx * c + dy * s) * i / r;
							}
						} else {
							c = Math.cos(ca);
							s = Math.sin(ca);
							var r = l * bone.worldScaleX - scaleLag * Math.max(0, 1 - aa / t);
							if (r > 0)
								scaleOffset += (dx * c + dy * s) * i / r;
						}
						if (a >= t) {
							if (d == -1) {
								d = Math.pow(p.damping, 60 * t);
								m = t * p.massInverse;
								e = p.strength;
							}
							var ax = p.wind * skeleton.windX + p.gravity * skeleton.gravityX;
							var ay = (p.wind * skeleton.windY + p.gravity * skeleton.gravityY) * Bone.yDir;
							var rs = rotateOffset, ss = scaleOffset, h = l / f;
							while (true) {
								a -= t;
								if (scaleX) {
									scaleVelocity += (ax * c - ay * s - scaleOffset * e) * m;
									scaleOffset += scaleVelocity * t;
									scaleVelocity *= d;
								}
								if (rotateOrShearX) {
									rotateVelocity -= ((ax * s + ay * c) * h + rotateOffset * e) * m;
									rotateOffset += rotateVelocity * t;
									rotateVelocity *= d;
									if (a < t)
										break;
									var r:Float = rotateOffset * mr + ca;
									c = Math.cos(r);
									s = Math.sin(r);
								} else if (a < t) //
									break;
							}
							rotateLag = rotateOffset - rs;
							scaleLag = scaleOffset - ss;
						}
						z = Math.max(0, 1 - a / t);
					}
					remaining = a;
				}
				cx = bone.worldX;
				cy = bone.worldY;
			case Physics.pose:
				z = Math.max(0, 1 - remaining / t);
				if (x)
					bone.worldX += (xOffset - xLag * z) * mix * data.x;
				if (y)
					bone.worldY += (yOffset - yLag * z) * mix * data.y;
		}

		if (rotateOrShearX) {
			var o = (rotateOffset - rotateLag * z) * mix, s = 0., c = 0., a = 0.;
			if (data.shearX > 0) {
				var r = 0.;
				if (data.rotate > 0) {
					r = o * data.rotate;
					s = Math.sin(r);
					c = Math.cos(r);
					a = bone.b;
					bone.b = c * a - s * bone.d;
					bone.d = s * a + c * bone.d;
				}
				r += o * data.shearX;
				s = Math.sin(r);
				c = Math.cos(r);
				a = bone.a;
				bone.a = c * a - s * bone.c;
				bone.c = s * a + c * bone.c;
			} else {
				o *= data.rotate;
				s = Math.sin(o);
				c = Math.cos(o);
				a = bone.a;
				bone.a = c * a - s * bone.c;
				bone.c = s * a + c * bone.c;
				a = bone.b;
				bone.b = c * a - s * bone.d;
				bone.d = s * a + c * bone.d;
			}
		}
		if (scaleX) {
			var s = 1 + (scaleOffset - scaleLag * z) * mix * data.scaleX;
			bone.a *= s;
			bone.c *= s;
		}
		if (physics != Physics.pose) {
			tx = l * bone.a;
			ty = l * bone.c;
		}
		bone.modifyWorld(skeleton._update);
	}

	public function sort(skeleton:Skeleton) {
		var bone = bone.bone;
		skeleton.sortBone(bone);
		skeleton._updateCache.push(this);
		skeleton.sortReset(bone.children);
		skeleton.constrained(bone);
	}

	override public function isSourceActive() {
		return bone.bone.active;
	}
}
