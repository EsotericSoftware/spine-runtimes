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
	public var mix:Number;
	public var bendDirection:int;
	
	public var level:int;	

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
		var pp:Bone = bone.parent;
		var id:Number = 1 / (pp.a * pp.d - pp.b * pp.c);
		var x:Number = targetX - pp.worldX, y:Number = targetY - pp.worldY;
		var tx:Number = (x * pp.d - y * pp.b) * id - bone.x, ty:Number = (y * pp.a - x * pp.c) * id - bone.y;
		var rotationIK:Number = Math.atan2(ty, tx) * MathUtils.radDeg - bone.shearX - bone.rotation;
		if (bone.scaleX < 0) rotationIK += 180;
		if (rotationIK > 180)
			rotationIK -= 360;
		else if (rotationIK < -180) rotationIK += 360;
		bone.updateWorldTransformWith(bone.x, bone.y, bone.rotation + rotationIK * alpha, bone.scaleX, bone.scaleY, bone.shearX,
			bone.shearY);
	}

	/** Adjusts the parent and child bone rotations so the tip of the child is as close to the target position as possible. The
	 * target is specified in the world coordinate system.
	 * @param child Any descendant bone of the parent. */
	static public function apply2 (parent:Bone, child:Bone, targetX:Number, targetY:Number, bendDir:int, alpha:Number) : void {
		if (alpha == 0) {
			child.updateWorldTransform();
			return;
		}
		var px:Number = parent.x, py:Number = parent.y, psx:Number = parent.scaleX, psy:Number = parent.scaleY, csx:Number = child.scaleX;;
		var os1:int, os2:int, s2:int;
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
		var cx:Number = child.x, cy:Number, cwx:Number, cwy:Number, a:Number = parent.a, b:Number = parent.b, c:Number = parent.c, d:Number = parent.d;
		var u:Boolean = Math.abs(psx - psy) <= 0.0001;
		if (!u) {
			cy = 0;
			cwx = a * cx + parent.worldX;
			cwy = c * cx + parent.worldY;
		} else {
			cy = child.y;
			cwx = a * cx + b * cy + parent.worldX;
			cwy = c * cx + d * cy + parent.worldY;
		}
		var pp:Bone = parent.parent;
		a = pp.a;
		b = pp.b;
		c = pp.c;
		d = pp.d;
		var id:Number = 1 / (a * d - b * c), x:Number = targetX - pp.worldX, y:Number = targetY - pp.worldY;
		var tx:Number = (x * d - y * b) * id - px, ty:Number = (y * a - x * c) * id - py;
		x = cwx - pp.worldX;
		y = cwy - pp.worldY;
		var dx:Number = (x * d - y * b) * id - px, dy:Number = (y * a - x * c) * id - py;
		var l1:Number = Math.sqrt(dx * dx + dy * dy), l2:Number = child.data.length * csx, a1:Number, a2:Number;
		outer:
		if (u) {
			l2 *= psx;
			var cos:Number = (tx * tx + ty * ty - l1 * l1 - l2 * l2) / (2 * l1 * l2);
			if (cos < -1) 
				cos = -1;
			else if (cos > 1) cos = 1;
			a2 = Math.acos(cos) * bendDir;
			a = l1 + l2 * cos;
			b = l2 * Math.sin(a2);
			a1 = Math.atan2(ty * a - tx * b, tx * a + ty * b);
		} else {
			a = psx * l2;
			b = psy * l2;
			var aa:Number = a * a, bb:Number = b * b, dd:Number = tx * tx + ty * ty, ta:Number = Math.atan2(ty, tx);
			c = bb * l1 * l1 + aa * dd - aa * bb;
			var c1:Number = -2 * bb * l1, c2:Number = bb - aa;
			d = c1 * c1 - 4 * c2 * c;
			if (d >= 0) {
				var q:Number = Math.sqrt(d);
				if (c1 < 0) q = -q;
				q = -(c1 + q) / 2;
				var r0:Number = q / c2, r1:Number = c / q;
				var r:Number = Math.abs(r0) < Math.abs(r1) ? r0 : r1;
				if (r * r <= dd) {
					y = Math.sqrt(dd - r * r) * bendDir;
					a1 = ta - Math.atan2(y, r);
					a2 = Math.atan2(y / psy, (r - l1) / psx);
					break outer;
				}
			}
			var minAngle:Number = 0, minDist:Number = Number.MAX_VALUE, minX:Number = 0, minY:Number = 0;
			var maxAngle:Number = 0, maxDist:Number = 0, maxX:Number = 0, maxY:Number = 0;
			x = l1 + a;
			d = x * x;
			if (d > maxDist) {
				maxAngle = 0;
				maxDist = d;
				maxX = x;
			}
			x = l1 - a;
			d = x * x;
			if (d < minDist) {
				minAngle = Math.PI;
				minDist = d;
				minX = x;
			}
			var angle:Number = Math.acos(-a * l1 / (aa - bb));
			x = a * Math.cos(angle) + l1;
			y = b * Math.sin(angle);
			d = x * x + y * y;
			if (d < minDist) {
				minAngle = angle;
				minDist = d;
				minX = x;
				minY = y;
			}
			if (d > maxDist) {
				maxAngle = angle;
				maxDist = d;
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
		var os:Number = Math.atan2(cy, cx) * s2;
		var rotation:Number = parent.rotation;
		a1 = (a1 - os) * MathUtils.radDeg + os1 - rotation;
		if (a1 > 180)
			a1 -= 360;
		else if (a1 < -180) a1 += 360;
		parent.updateWorldTransformWith(px, py, rotation + a1 * alpha, parent.scaleX, parent.scaleY, 0, 0);
		rotation = child.rotation;
		a2 = ((a2 + os) * MathUtils.radDeg - child.shearX) * s2 + os2 - rotation;
		if (a2 > 180)
			a2 -= 360;
		else if (a2 < -180) a2 += 360;
		child.updateWorldTransformWith(cx, cy, rotation + a2 * alpha, child.scaleX, child.scaleY, child.shearX, child.shearY);
	}
}

}
