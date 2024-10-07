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

class IkConstraint implements Updatable {
	private var _data:IkConstraintData;

	public var bones:Array<Bone>;
	public var target:Bone;
	public var bendDirection:Int = 0;
	public var compress:Bool = false;
	public var stretch:Bool = false;
	public var mix:Float = 0;
	public var softness:Float = 0;
	public var active:Bool = false;

	public function new(data:IkConstraintData, skeleton:Skeleton) {
		if (data == null)
			throw new SpineException("data cannot be null.");
		if (skeleton == null)
			throw new SpineException("skeleton cannot be null.");
		_data = data;

		bones = new Array<Bone>();
		for (boneData in data.bones) {
			bones.push(skeleton.findBone(boneData.name));
		}
		target = skeleton.findBone(data.target.name);

		mix = data.mix;
		softness = data.softness;
		bendDirection = data.bendDirection;
		compress = data.compress;
		stretch = data.stretch;
	}

	public function isActive():Bool {
		return active;
	}

	public function setToSetupPose () {
		var data:IkConstraintData = _data;
		mix = data.mix;
		softness = data.softness;
		bendDirection = data.bendDirection;
		compress = data.compress;
		stretch = data.stretch;
	}

	public function update(physics:Physics):Void {
		if (mix == 0)
			return;
		switch (bones.length) {
			case 1:
				apply1(bones[0], target.worldX, target.worldY, compress, stretch, _data.uniform, mix);
			case 2:
				apply2(bones[0], bones[1], target.worldX, target.worldY, bendDirection, stretch, _data.uniform, softness, mix);
		}
	}

	public var data(get, never):IkConstraintData;

	private function get_data():IkConstraintData {
		return _data;
	}

	public function toString():String {
		return _data.name != null ? _data.name : "IkContstraint?";
	}

	/** Adjusts the bone rotation so the tip is as close to the target position as possible. The target is specified in the world
	 * coordinate system. */
	static public function apply1(bone:Bone, targetX:Float, targetY:Float, compress:Bool, stretch:Bool, uniform:Bool, alpha:Float):Void {
		var p:Bone = bone.parent;
		var pa:Float = p.a, pb:Float = p.b, pc:Float = p.c, pd:Float = p.d;
		var rotationIK:Float = -bone.ashearX - bone.arotation,
			tx:Float = 0,
			ty:Float = 0;

		function switchDefault() {
			var x:Float = targetX - p.worldX, y:Float = targetY - p.worldY;
			var d:Float = pa * pd - pb * pc;
			if (Math.abs(d) <= 0.0001) {
				tx = 0;
				ty = 0;
			} else {
				tx = (x * pd - y * pb) / d - bone.ax;
				ty = (y * pa - x * pc) / d - bone.ay;
			}
		}

		switch (bone.inherit) {
			case Inherit.onlyTranslation:
				tx = (targetX - bone.worldX) * MathUtils.signum(bone.skeleton.scaleX);
				ty = (targetY - bone.worldY) * MathUtils.signum(bone.skeleton.scaleY);
			case Inherit.noRotationOrReflection:
				var s = Math.abs(pa * pd - pb * pc) / Math.max(0.0001, pa * pa + pc * pc);
				var sa:Float = pa / bone.skeleton.scaleX;
				var sc:Float = pc / bone.skeleton.scaleY;
				pb = -sc * s * bone.skeleton.scaleX;
				pd = sa * s * bone.skeleton.scaleY;
				rotationIK += Math.atan2(sc, sa) * MathUtils.radDeg;
				var x:Float = targetX - p.worldX, y:Float = targetY - p.worldY;
				var d:Float = pa * pd - pb * pc;
				tx = (x * pd - y * pb) / d - bone.ax;
				ty = (y * pa - x * pc) / d - bone.ay;
				switchDefault(); // Fall through.
			default:
				switchDefault();
		}

		rotationIK += Math.atan2(ty, tx) * MathUtils.radDeg;
		if (bone.ascaleX < 0)
			rotationIK += 180;
		if (rotationIK > 180)
			rotationIK -= 360;
		else if (rotationIK < -180)
			rotationIK += 360;
		var sx:Float = bone.ascaleX;
		var sy:Float = bone.ascaleY;
		if (compress || stretch) {
			switch (bone.inherit) {
				case Inherit.noScale, Inherit.noScaleOrReflection:
					tx = targetX - bone.worldX;
					ty = targetY - bone.worldY;
			}
			var b:Float = bone.data.length * sx;
			if (b > 0.0001) {
				var	dd:Float = tx * tx + ty * ty;
				if ((compress && dd < b * b) || (stretch && dd > b * b)) {
					var s:Float = (Math.sqrt(dd) / b - 1) * alpha + 1;
					sx *= s;
					if (uniform) sy *= s;
				}
			}
		}
		bone.updateWorldTransformWith(bone.ax, bone.ay, bone.arotation + rotationIK * alpha, sx, sy, bone.ashearX, bone.ashearY);
	}

	/** Adjusts the parent and child bone rotations so the tip of the child is as close to the target position as possible. The
	 * target is specified in the world coordinate system.
	 * @param child Any descendant bone of the parent. */
	static public function apply2(parent:Bone, child:Bone, targetX:Float, targetY:Float, bendDir:Int, stretch:Bool, uniform:Bool, softness:Float,
			alpha:Float):Void {
		if (parent.inherit != Inherit.normal || child.inherit != Inherit.normal) return;
		var px:Float = parent.ax;
		var py:Float = parent.ay;
		var psx:Float = parent.ascaleX;
		var sx:Float = psx;
		var psy:Float = parent.ascaleY;
		var sy:Float = psy;
		var csx:Float = child.ascaleX;
		var os1:Int;
		var os2:Int;
		var s2:Int;
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
		} else {
			os2 = 0;
		}
		var cx:Float = child.ax;
		var cy:Float;
		var cwx:Float;
		var cwy:Float;
		var a:Float = parent.a;
		var b:Float = parent.b;
		var c:Float = parent.c;
		var d:Float = parent.d;
		var u:Bool = Math.abs(psx - psy) <= 0.0001;
		if (!u || stretch) {
			cy = 0;
			cwx = a * cx + parent.worldX;
			cwy = c * cx + parent.worldY;
		} else {
			cy = child.ay;
			cwx = a * cx + b * cy + parent.worldX;
			cwy = c * cx + d * cy + parent.worldY;
		}
		var pp:Bone = parent.parent;
		a = pp.a;
		b = pp.b;
		c = pp.c;
		d = pp.d;
		var id = a * d - b * c, x = cwx - pp.worldX, y = cwy - pp.worldY;
		id = Math.abs(id) <= 0.0001 ? 0 : 1 / id;
		var dx:Float = (x * d - y * b) * id - px,
			dy:Float = (y * a - x * c) * id - py;
		var l1:Float = Math.sqrt(dx * dx + dy * dy);
		var l2:Float = child.data.length * csx;
		var a1:Float = 0;
		var a2:Float = 0;
		if (l1 < 0.0001) {
			apply1(parent, targetX, targetY, false, stretch, false, alpha);
			child.updateWorldTransformWith(cx, cy, 0, child.ascaleX, child.ascaleY, child.ashearX, child.ashearY);
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
					a = (Math.sqrt(dd) / (l1 + l2) - 1) * alpha + 1;
					sx *= a;
					if (uniform)
						sy *= a;
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

		var os:Float = Math.atan2(cy, cx) * s2;
		var rotation:Float = parent.arotation;
		a1 = (a1 - os) * MathUtils.radDeg + os1 - rotation;
		if (a1 > 180) {
			a1 -= 360;
		} else if (a1 < -180) {
			a1 += 360;
		}
		parent.updateWorldTransformWith(px, py, rotation + a1 * alpha, sx, sy, 0, 0);
		rotation = child.arotation;
		a2 = ((a2 + os) * MathUtils.radDeg - child.ashearX) * s2 + os2 - rotation;
		if (a2 > 180) {
			a2 -= 360;
		} else if (a2 < -180) {
			a2 += 360;
		}
		child.updateWorldTransformWith(cx, cy, rotation + a2 * alpha, child.ascaleX, child.ascaleY, child.ashearX, child.ashearY);
	}
}
