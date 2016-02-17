/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine {

public class IkConstraint implements Updatable {
	internal var _data:IkConstraintData;
	public var bones:Vector.<Bone>;
	public var target:Bone;
	public var bendDirection:int;
	public var mix:Number;

	public function IkConstraint (data:IkConstraintData, skeleton:Skeleton) {
		if (data == null) throw new ArgumentError("data cannot be null.");
		if (skeleton == null) throw new ArgumentError("skeleton cannot be null.");
		_data = data;
		mix = data.mix;
		bendDirection = data.bendDirection;

		bones = new Vector.<Bone>();
		for each (var boneData:BoneData in data.bones)
			bones[bones.length] = skeleton.findBone(boneData.name);
		target = skeleton.findBone(data.target._name);
	}

	public function apply () : void {
		update();
	}

	public function update () : void {
		switch (bones.length) {
		case 1:
			apply1(bones[0], target._worldX, target._worldY, mix);
			break;
		case 2:
			apply2(bones[0], bones[1], target._worldX, target._worldY, bendDirection, mix);
			break;
		}
	}

	public function get data () : IkConstraintData {
		return _data;
	}

	public function toString () : String {
		return _data._name;
	}
	
	/** Adjusts the bone rotation so the tip is as close to the target position as possible. The target is specified in the world
	 * coordinate system. */
	static public function apply1 (bone:Bone, targetX:Number, targetY:Number, alpha:Number) : void {
		var parentRotation:Number = bone.parent == null ? 0 : bone.parent.worldRotationX;
		var rotation:Number = bone.rotation;
		var rotationIK:Number = Math.atan2(targetY - bone.worldY, targetX - bone.worldX) * MathUtils.radDeg - parentRotation;
		if (bone.worldSignX != bone.worldSignY) rotationIK = 360 - rotationIK;
		if (rotationIK > 180) rotationIK -= 360;
		else if (rotationIK < -180) rotationIK += 360;
		bone.updateWorldTransformWith(bone.x, bone.y, rotation + (rotationIK - rotation) * alpha, bone.scaleX, bone.scaleY);
	}

	/** Adjusts the parent and child bone rotations so the tip of the child is as close to the target position as possible. The
	 * target is specified in the world coordinate system.
	 * @param child Any descendant bone of the parent. */
	static public function apply2 (parent:Bone, child:Bone, targetX:Number, targetY:Number, bendDir:int, alpha:Number) : void {
		if (alpha == 0) return;
		var px:Number = parent.x, py:Number = parent.y, psx:Number = parent.scaleX, psy:Number = parent.scaleY;
		var csx:Number = child.scaleX, cy:Number = child.y;
		var offset1:int, offset2:int, sign2:int;
		if (psx < 0) {
			psx = -psx;
			offset1 = 180;
			sign2 = -1;
		} else {
			offset1 = 0;
			sign2 = 1;
		}
		if (psy < 0) {
			psy = -psy;
			sign2 = -sign2;
		}
		if (csx < 0) {
			csx = -csx;
			offset2 = 180;
		} else
			offset2 = 0;
		var pp:Bone = parent.parent;
		var tx:Number, ty:Number, dx:Number, dy:Number;
		if (!pp) {
			tx = targetX - px;
			ty = targetY - py;
			dx = child.worldX - px;
			dy = child.worldY - py;
		} else {
			var ppa:Number = pp.a, ppb:Number = pp.b, ppc:Number = pp.c, ppd:Number = pp.d;
			var invDet:Number = 1 / (ppa * ppd - ppb * ppc);
			var wx:Number = pp.worldX, wy:Number = pp.worldY, twx:Number = targetX - wx, twy:Number = targetY - wy;
			tx = (twx * ppd - twy * ppb) * invDet - px;
			ty = (twy * ppa - twx * ppc) * invDet - py;
			twx = child.worldX - wx;
			twy = child.worldY - wy;
			dx = (twx * ppd - twy * ppb) * invDet - px;
			dy = (twy * ppa - twx * ppc) * invDet - py;
		}
		var l1:Number = Math.sqrt(dx * dx + dy * dy), l2:Number = child.data.length * csx, a1:Number, a2:Number;
		outer:
		if (Math.abs(psx - psy) <= 0.0001) {
			l2 *= psx;
			var cos:Number = (tx * tx + ty * ty - l1 * l1 - l2 * l2) / (2 * l1 * l2);
			if (cos < -1) cos = -1;
			else if (cos > 1) cos = 1;
			a2 = Math.acos(cos) * bendDir;
			var ad:Number = l1 + l2 * cos, o:Number = l2 * Math.sin(a2);
			a1 = Math.atan2(ty * ad - tx * o, tx * ad + ty * o);
		} else {
			cy = 0;
			var a:Number = psx * l2, b:Number = psy * l2, ta:Number = Math.atan2(ty, tx);
			var aa:Number = a * a, bb:Number = b * b, ll:Number = l1 * l1, dd:Number = tx * tx + ty * ty;
			var c0:Number = bb * ll + aa * dd - aa * bb, c1:Number = -2 * bb * l1, c2:Number = bb - aa;
			var d:Number = c1 * c1 - 4 * c2 * c0;
			if (d >= 0) {
				var q:Number = Math.sqrt(d);
				if (c1 < 0) q = -q;
				q = -(c1 + q) / 2;
				var r0:Number = q / c2, r1:Number = c0 / q;
				var r:Number = Math.abs(r0) < Math.abs(r1) ? r0 : r1;
				if (r * r <= dd) {
					var y1:Number = Math.sqrt(dd - r * r) * bendDir;
					a1 = ta - Math.atan2(y1, r);
					a2 = Math.atan2(y1 / psy, (r - l1) / psx);
					break outer;
				}
			}
			var minAngle:Number = 0, minDist:Number = Number.MAX_VALUE, minX:Number = 0, minY:Number = 0;
			var maxAngle:Number = 0, maxDist:Number = 0, maxX:Number = 0, maxY:Number = 0;
			var x:Number = l1 + a, dist:Number = x * x;
			if (dist > maxDist) {
				maxAngle = 0;
				maxDist = dist;
				maxX = x;
			}
			x = l1 - a;
			dist = x * x;
			if (dist < minDist) {
				minAngle = Math.PI;
				minDist = dist;
				minX = x;
			}
			var angle:Number = Math.acos(-a * l1 / (aa - bb));
			x = a * Math.cos(angle) + l1;
			var y:Number = b * Math.sin(angle);
			dist = x * x + y * y;
			if (dist < minDist) {
				minAngle = angle;
				minDist = dist;
				minX = x;
				minY = y;
			}
			if (dist > maxDist) {
				maxAngle = angle;
				maxDist = dist;
				maxX = x;
				maxY = y;
			}
			if (dd <= (minDist + maxDist) / 2) {
				a1 = ta - Math.atan2(minY * bendDir, minX);
				a2 = minAngle * bendDir;
			} else {
				a1 = ta - Math.atan2(maxY * bendDir, maxX);
				a2 = maxAngle * bendDir;
			}
		}
		var offset:Number = Math.atan2(cy, child.x) * sign2;
		a1 = (a1 - offset) * MathUtils.radDeg + offset1;
		a2 = (a2 + offset) * MathUtils.radDeg * sign2 + offset2;
		if (a1 > 180) a1 -= 360;
		else if (a1 < -180) a1 += 360;
		if (a2 > 180) a2 -= 360;
		else if (a2 < -180) a2 += 360;
		var rotation:Number = parent.rotation;
		parent.updateWorldTransformWith(parent.x, parent.y, rotation + (a1 - rotation) * alpha, parent.scaleX, parent.scaleY);
		rotation = child.rotation;
		child.updateWorldTransformWith(child.x, cy, rotation + (a2 - rotation) * alpha, child.scaleX, child.scaleY);
	}
}

}
