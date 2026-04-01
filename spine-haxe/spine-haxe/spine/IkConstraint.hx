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

/** Stores the current pose for an IK constraint. An IK constraint adjusts the rotation of 1 or 2 constrained bones so the tip of
 * the last bone is as close to the target bone as possible.
 *
 * @see https://esotericsoftware.com/spine-ik-constraints IK constraints in the Spine User Guide */
class IkConstraint extends Constraint<IkConstraint, IkConstraintData, IkConstraintPose> {
	/** The 1 or 2 bones that will be modified by this IK constraint. */
	public final bones:Array<BonePose>;

	/** The bone that is the IK target. */
	public var target(default, set):Bone;

	public function new(data:IkConstraintData, skeleton:Skeleton) {
		super(data, new IkConstraintPose(), new IkConstraintPose());
		if (skeleton == null)
			throw new SpineException("skeleton cannot be null.");

		bones = new Array<BonePose>();
		for (boneData in data.bones)
			bones.push(skeleton.bones[boneData.index].constrainedPose);
		target = skeleton.bones[data.target.index];
	}

	public function copy(skeleton:Skeleton) {
		var copy = new IkConstraint(data, skeleton);
		copy.pose.set(pose);
		return copy;
	}

	/** Applies the constraint to the constrained bones. */
	public function update(skeleton:Skeleton, physics:Physics):Void {
		var p = appliedPose;
		if (p.mix == 0)
			return;
		var target = target.appliedPose;
		switch (bones.length) {
			case 1:
				apply1(skeleton, bones[0], target.worldX, target.worldY, p.compress, p.stretch, data.uniform, p.mix);
			case 2:
				apply2(skeleton, bones[0], bones[1], target.worldX, target.worldY, p.bendDirection, p.stretch, data.uniform, p.softness, p.mix);
		}
	}

	public function sort(skeleton:Skeleton) {
		skeleton.sortBone(target);
		var parent = bones[0].bone;
		skeleton.sortBone(parent);
		skeleton._updateCache.push(this);
		parent.sorted = false;
		skeleton.sortReset(parent.children);
		skeleton.constrained(parent);
		if (bones.length > 1)
			skeleton.constrained(bones[1].bone);
	}

	override public function isSourceActive() {
		return target.active;
	}

	public function set_target(target:Bone):Bone {
		if (target == null)
			throw new SpineException("target cannot be null.");
		this.target = target;
		return target;
	}

	/** Applies 1 bone IK. The target is specified in the world coordinate system. */
	static public function apply1(skeleton:Skeleton, bone:BonePose, targetX:Float, targetY:Float, compress:Bool, stretch:Bool, uniform:Bool, mix:Float) {
		if (bone == null)
			throw new SpineException("bone cannot be null.");
		bone.modifyLocal(skeleton);
		var p = bone.bone.parent.appliedPose;
		var pa = p.a, pb = p.b, pc = p.c, pd = p.d;
		var rotationIK = -bone.shearX - bone.rotation, tx = 0., ty = 0.;

		function switchDefault() {
			var x = targetX - p.worldX, y = targetY - p.worldY;
			var d = pa * pd - pb * pc;
			if (Math.abs(d) <= 0.0001) {
				tx = 0;
				ty = 0;
			} else {
				tx = (x * pd - y * pb) / d - bone.x;
				ty = (y * pa - x * pc) / d - bone.y;
			}
		}

		switch (bone.inherit) {
			case Inherit.onlyTranslation:
				tx = (targetX - bone.worldX) * MathUtils.signum(skeleton.scaleX);
				ty = (targetY - bone.worldY) * MathUtils.signum(skeleton.scaleY);
			case Inherit.noRotationOrReflection:
				var s = Math.abs(pa * pd - pb * pc) / Math.max(0.0001, pa * pa + pc * pc);
				var sa:Float = pa / skeleton.scaleX;
				var sc:Float = pc / skeleton.scaleY;
				pb = -sc * s * skeleton.scaleX;
				pd = sa * s * skeleton.scaleY;
				rotationIK += MathUtils.atan2Deg(sc, sa);
				switchDefault(); // Fall through.
			default:
				switchDefault();
		}
		rotationIK += MathUtils.atan2Deg(ty, tx);
		if (bone.scaleX < 0)
			rotationIK += 180;
		if (rotationIK > 180)
			rotationIK -= 360;
		else if (rotationIK < -180) //
			rotationIK += 360;
		bone.rotation += rotationIK * mix;
		if (compress || stretch) {
			switch (bone.inherit) {
				case Inherit.noScale, Inherit.noScaleOrReflection:
					tx = targetX - bone.worldX;
					ty = targetY - bone.worldY;
			}
			var b = bone.bone.data.length * bone.scaleX;
			if (b > 0.0001) {
				var dd = tx * tx + ty * ty;
				if ((compress && dd < b * b) || (stretch && dd > b * b)) {
					var s = (Math.sqrt(dd) / b - 1) * mix + 1;
					bone.scaleX *= s;
					if (uniform)
						bone.scaleY *= s;
				}
			}
		}
	}

	/** Applies 2 bone IK. The target is specified in the world coordinate system.
	 * @param child A direct descendant of the parent bone. */
	static public function apply2(skeleton:Skeleton, parent:BonePose, child:BonePose, targetX:Float, targetY:Float, bendDir:Int, stretch:Bool, uniform:Bool,
			softness:Float, mix:Float):Void {
		if (parent == null)
			throw new SpineException("parent cannot be null.");
		if (child == null)
			throw new SpineException("child cannot be null.");
		if (parent.inherit != Inherit.normal || child.inherit != Inherit.normal)
			return;
		parent.modifyLocal(skeleton);
		child.modifyLocal(skeleton);
		var px = parent.x, py = parent.y, psx = parent.scaleX, psy = parent.scaleY, csx = child.scaleX;
		var os1 = 0, os2 = 0, s2 = 0;
		if (psx < 0) {
			psx = -psx;
			os1 = 180;
			s2 = -1;
		} else {
			os1 = 0;
			s2 = 1;
		}
		if (psy < 0) {
			psy = -psy;
			s2 = -s2;
		}
		if (csx < 0) {
			csx = -csx;
			os2 = 180;
		} else
			os2 = 0;
		var cwx = 0., cwy = 0., a = parent.a, b = parent.b, c = parent.c, d = parent.d;
		var u = Math.abs(psx - psy) <= 0.0001;
		if (!u || stretch) {
			child.y = 0;
			cwx = a * child.x + parent.worldX;
			cwy = c * child.x + parent.worldY;
		} else {
			cwx = a * child.x + b * child.y + parent.worldX;
			cwy = c * child.x + d * child.y + parent.worldY;
		}
		var pp = parent.bone.parent.appliedPose;
		a = pp.a;
		b = pp.b;
		c = pp.c;
		d = pp.d;
		var id = a * d - b * c, x = cwx - pp.worldX, y = cwy - pp.worldY;
		id = Math.abs(id) <= 0.0001 ? 0 : 1 / id;
		var dx = (x * d - y * b) * id - px, dy = (y * a - x * c) * id - py;
		var l1 = Math.sqrt(dx * dx + dy * dy), l2 = child.bone.data.length * csx, a1 = 0., a2 = 0.;
		if (l1 < 0.0001) {
			apply1(skeleton, parent, targetX, targetY, false, stretch, false, mix);
			child.rotation = 0;
			return;
		}
		x = targetX - pp.worldX;
		y = targetY - pp.worldY;
		var tx:Float = (x * d - y * b) * id - px;
		var ty:Float = (y * a - x * c) * id - py;
		var dd:Float = tx * tx + ty * ty;
		if (softness != 0) {
			softness *= psx * (csx + 1) / 2;
			var td:Float = Math.sqrt(dd);
			var sd:Float = td - l1 - l2 * psx + softness;
			if (sd > 0) {
				var p:Float = Math.min(1, sd / (softness * 2)) - 1;
				p = (sd - softness * (1 - p * p)) / td;
				tx -= p * tx;
				ty -= p * ty;
				dd = tx * tx + ty * ty;
			}
		}

		var breakOuter:Bool = false;
		if (u) {
			l2 *= psx;
			var cos:Float = (dd - l1 * l1 - l2 * l2) / (2 * l1 * l2);
			if (cos < -1) {
				cos = -1;
			} else if (cos > 1) {
				cos = 1;
				if (stretch) {
					a = (Math.sqrt(dd) / (l1 + l2) - 1) * mix + 1;
					parent.scaleX *= a;
					if (uniform)
						parent.scaleY *= a;
				}
			}
			a2 = Math.acos(cos) * bendDir;
			a = l1 + l2 * cos;
			b = l2 * Math.sin(a2);
			a1 = Math.atan2(ty * a - tx * b, tx * a + ty * b);
		} else {
			a = psx * l2;
			b = psy * l2;
			var aa:Float = a * a;
			var bb:Float = b * b;
			var ta:Float = Math.atan2(ty, tx);
			c = bb * l1 * l1 + aa * dd - aa * bb;
			var c1:Float = -2 * bb * l1;
			var c2:Float = bb - aa;
			d = c1 * c1 - 4 * c2 * c;
			if (d >= 0) {
				var q:Float = Math.sqrt(d);
				if (c1 < 0)
					q = -q;
				q = -(c1 + q) / 2;
				var r0:Float = q / c2, r1:Float = c / q;
				var r:Float = Math.abs(r0) < Math.abs(r1) ? r0 : r1;
				r0 = dd - r * r;
				if (r0 >= 0) {
					y = Math.sqrt(r0) * bendDir;
					a1 = ta - Math.atan2(y, r);
					a2 = Math.atan2(y / psy, (r - l1) / psx);
					breakOuter = true;
				}
			}

			if (!breakOuter) {
				var minAngle:Float = Math.PI;
				var minX:Float = l1 - a;
				var minDist:Float = minX * minX;
				var minY:Float = 0;
				var maxAngle:Float = 0;
				var maxX:Float = l1 + a;
				var maxDist:Float = maxX * maxX;
				var maxY:Float = 0;
				c = -a * l1 / (aa - bb);
				if (c >= -1 && c <= 1) {
					c = Math.acos(c);
					x = a * Math.cos(c) + l1;
					y = b * Math.sin(c);
					d = x * x + y * y;
					if (d < minDist) {
						minAngle = c;
						minDist = d;
						minX = x;
						minY = y;
					}
					if (d > maxDist) {
						maxAngle = c;
						maxDist = d;
						maxX = x;
						maxY = y;
					}
				}
				if (dd <= (minDist + maxDist) / 2) {
					a1 = ta - Math.atan2(minY * bendDir, minX);
					a2 = minAngle * bendDir;
				} else {
					a1 = ta - Math.atan2(maxY * bendDir, maxX);
					a2 = maxAngle * bendDir;
				}
			}
		}
		var os:Float = Math.atan2(child.y, child.x) * s2;
		a1 = (a1 - os) * MathUtils.radDeg + os1 - parent.rotation;
		if (a1 > 180)
			a1 -= 360;
		else if (a1 < -180) //
			a1 += 360;
		parent.rotation += a1 * mix;
		a2 = ((a2 + os) * MathUtils.radDeg - child.shearX) * s2 + os2 - child.rotation;
		if (a2 > 180)
			a2 -= 360;
		else if (a2 < -180) //
			a2 += 360;
		child.rotation += a2 * mix;
	}
}
