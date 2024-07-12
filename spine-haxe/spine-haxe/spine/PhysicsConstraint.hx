/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*****************************************************************************/

package spine;

class PhysicsConstraint implements Updatable {
	private var _data:PhysicsConstraintData;
	private var _bone:Bone = null;

	public var inertia:Float = 0;
	public var strength:Float = 0;
	public var damping:Float = 0;
	public var massInverse:Float = 0;
	public var wind:Float = 0;
	public var gravity:Float = 0;
	public var mix:Float = 0;

	private var _reset:Bool = true;

	public var ux:Float = 0;
	public var uy:Float = 0;
	public var cx:Float = 0;
	public var cy:Float = 0;
	public var tx:Float = 0;
	public var ty:Float = 0;
	public var xOffset:Float = 0;
	public var xVelocity:Float = 0;
	public var yOffset:Float = 0;
	public var yVelocity:Float = 0;
	public var rotateOffset:Float = 0;
	public var rotateVelocity:Float = 0;
	public var scaleOffset:Float = 0;
	public var scaleVelocity:Float = 0;

	public var active:Bool = false;

	private var _skeleton:Skeleton;
	public var remaining:Float = 0;
	public var lastTime:Float = 0;

	public function new(data: PhysicsConstraintData, skeleton: Skeleton) {
		_data = data;
		_skeleton = skeleton;

		_bone = skeleton.bones[data.bone.index];

		inertia = data.inertia;
		strength = data.strength;
		damping = data.damping;
		massInverse = data.massInverse;
		wind = data.wind;
		gravity = data.gravity;
		mix = data.mix;
	}

	public function reset () {
		remaining = 0;
		lastTime = skeleton.time;
		_reset = true;
		xOffset = 0;
		xVelocity = 0;
		yOffset = 0;
		yVelocity = 0;
		rotateOffset = 0;
		rotateVelocity = 0;
		scaleOffset = 0;
		scaleVelocity = 0;
	}

	public function setToSetupPose () {
		var data:PhysicsConstraintData = _data;
		inertia = data.inertia;
		strength = data.strength;
		damping = data.damping;
		massInverse = data.massInverse;
		wind = data.wind;
		gravity = data.gravity;
		mix = data.mix;
	}

	public function isActive():Bool {
		return active;
	}

	public function update(physics:Physics):Void {
		var mix:Float = this.mix;
		if (mix == 0) return;

		var x:Bool = _data.x > 0, y:Bool = _data.y > 0,
			rotateOrShearX:Bool = _data.rotate > 0 || _data.shearX > 0,
			scaleX:Bool = _data.scaleX > 0;
		var bone:Bone = _bone;
		var l:Float = bone.data.length;

		switch (physics) {
			case Physics.none:
				return;
			case Physics.reset, Physics.update:
				if (physics == Physics.reset) reset();

				var delta:Float = Math.max(skeleton.time - lastTime, 0);
				remaining += delta;
				lastTime = _skeleton.time;

				var bx:Float = bone.worldX, by:Float = bone.worldY;
				if (_reset) {
					_reset = false;
					ux = bx;
					uy = by;
				} else {
					var a:Float = remaining,
						i:Float = inertia,
						t:Float = _data.step,
						f:Float = skeleton.data.referenceScale,
						d:Float = -1;

					var qx:Float = _data.limit * delta,
						qy:Float = qx * Math.abs(skeleton.scaleY);
					qx *= Math.abs(skeleton.scaleX);
					if (x || y) {
						if (x) {
							var u:Float = (ux - bx) * i;
							xOffset += u > qx ? qx : u < -qx ? -qx : u;
							ux = bx;
						}
						if (y) {
							var u:Float = (uy - by) * i;
							yOffset += u > qy ? qy : u < -qy ? -qy : u;
							uy = by;
						}
						if (a >= t) {
							d = Math.pow(damping, 60 * t);
							var m:Float = massInverse * t,
								e:Float = strength,
								w:Float = wind * f,
								g:Float = (Bone.yDown ? -gravity : gravity) * f;
							do {
								if (x) {
									xVelocity += (w - xOffset * e) * m;
									xOffset += xVelocity * t;
									xVelocity *= d;
								}
								if (y) {
									yVelocity -= (g + yOffset * e) * m;
									yOffset += yVelocity * t;
									yVelocity *= d;
								}
								a -= t;
							}  while (a >= t);
						}
						if (x) bone.worldX += xOffset * mix * data.x;
						if (y) bone.worldY += yOffset * mix * data.y;
					}
					if (rotateOrShearX || scaleX) {
						var ca:Float = Math.atan2(bone.c, bone.a),
							c:Float = 0,
							s:Float = 0,
							mr:Float = 0;
						var dx:Float = cx - bone.worldX,
							dy:Float = cy - bone.worldY;
						if (dx > qx)
							dx = qx;
						else if (dx < -qx) //
							dx = -qx;
						if (dy > qy)
							dy = qy;
						else if (dy < -qy) //
							dy = -qy;
						if (rotateOrShearX) {
							mr = (_data.rotate + _data.shearX) * mix;
							var r:Float = Math.atan2(dy + ty, dx + tx) - ca - rotateOffset * mr;
							rotateOffset += (r - Math.ceil(r * MathUtils.invPI2 - 0.5) * MathUtils.PI2) * i;
							r = rotateOffset * mr + ca;
							c = Math.cos(r);
							s = Math.sin(r);
							if (scaleX) {
								r = l * bone.worldScaleX;
								if (r > 0) scaleOffset += (dx * c + dy * s) * i / r;
							}
						} else {
							c = Math.cos(ca);
							s = Math.sin(ca);
							var r:Float = l * bone.worldScaleX;
							if (r > 0) scaleOffset += (dx * c + dy * s) * i / r;
						}
						a = remaining;
						if (a >= t) {
							if (d == -1) d = Math.pow(damping, 60 * t);
							var m:Float = massInverse * t,
							e:Float = strength,
							w:Float = wind,
							g:Float = (Bone.yDown ? -gravity : gravity),
							h:Float = l / f;
							while (true) {
								a -= t;
								if (scaleX) {
									scaleVelocity += (w * c - g * s - scaleOffset * e) * m;
									scaleOffset += scaleVelocity * t;
									scaleVelocity *= d;
								}
								if (rotateOrShearX) {
									rotateVelocity -= ((w * s + g * c) * h + rotateOffset * e) * m;
									rotateOffset += rotateVelocity * t;
									rotateVelocity *= d;
									if (a < t) break;
									var r:Float = rotateOffset * mr + ca;
									c = Math.cos(r);
									s = Math.sin(r);
								} else if (a < t) //
									break;
							}
						}
					}
					remaining = a;
				}
				cx = bone.worldX;
				cy = bone.worldY;
			case Physics.pose:
				if (x) bone.worldX += xOffset * mix * data.x;
				if (y) bone.worldY += yOffset * mix * data.y;
		}

		if (rotateOrShearX) {
			var o:Float = rotateOffset * mix,
				s:Float = 0,
				c:Float = 0,
				a:Float = 0;
			if (_data.shearX > 0) {
				var r:Float = 0;
				if (_data.rotate > 0) {
					r = o * _data.rotate;
					s = Math.sin(r);
					c = Math.cos(r);
					a = bone.b;
					bone.b = c * a - s * bone.d;
					bone.d = s * a + c * bone.d;
				}
				r += o * _data.shearX;
				s = Math.sin(r);
				c = Math.cos(r);
				a = bone.a;
				bone.a = c * a - s * bone.c;
				bone.c = s * a + c * bone.c;
			} else {
				o *= _data.rotate;
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
			var s:Float = 1 + scaleOffset * mix * data.scaleX;
			bone.a *= s;
			bone.c *= s;
		}
		if (physics != Physics.pose) {
			tx = l * bone.a;
			ty = l * bone.c;
		}
		bone.updateAppliedTransform();
	}

	public function translate (x:Float, y:Float):Void {
		ux -= x;
		uy -= y;
		cx -= x;
		cy -= y;
	}

	public function rotate (x:Float, y:Float, degrees:Float):Void {
		var r:Float = degrees * MathUtils.degRad, cos:Float = Math.cos(r), sin:Float = Math.sin(r);
		var dx:Float = cx - x, dy:Float = cy - y;
		translate(dx * cos - dy * sin - dx, dx * sin + dy * cos - dy);
	}

	public var bone(get, never):Bone;

	private function get_bone():Bone {
		if (_bone == null)
			throw new SpineException("Bone not set.")
		else return _bone;
	}

	public var data(get, never):PhysicsConstraintData;

	private function get_data():PhysicsConstraintData {
		return _data;
	}

	public var skeleton(get, never):Skeleton;

	private function get_skeleton():Skeleton {
		return _skeleton;
	}

}
