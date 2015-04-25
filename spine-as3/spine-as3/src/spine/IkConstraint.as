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

public class IkConstraint {
	static private const tempPosition:Vector.<Number> = new Vector.<Number>(2, true);
	static private const radDeg:Number = 180 / Math.PI;

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
		var parentRotation:Number = (!bone._data.inheritRotation || bone._parent == null) ? 0 : bone._parent._worldRotation;
		var rotation:Number = bone.rotation;
		var rotationIK:Number = Math.atan2(targetY - bone._worldY, targetX - bone._worldX) * radDeg;
		if (bone._worldFlipX != (bone._worldFlipY != Bone.yDown)) rotationIK = -rotationIK;
		rotationIK -= parentRotation;
		bone.rotationIK = rotation + (rotationIK - rotation) * alpha;
	}

	/** Adjusts the parent and child bone rotations so the tip of the child is as close to the target position as possible. The
	 * target is specified in the world coordinate system.
	 * @param child Any descendant bone of the parent. */
	static public function apply2 (parent:Bone, child:Bone, targetX:Number, targetY:Number, bendDirection:int, alpha:Number) : void {
		var childRotation:Number = child.rotation, parentRotation:Number = parent.rotation;
		if (alpha == 0) {
			child.rotationIK = childRotation;
			parent.rotationIK = parentRotation;
			return;
		}
		var positionX:Number, positionY:Number;
		var parentParent:Bone = parent._parent;
		if (parentParent) {
			tempPosition[0] = targetX;
			tempPosition[1] = targetY;
			parentParent.worldToLocal(tempPosition);
			targetX = (tempPosition[0] - parent.x) * parentParent._worldScaleX;
			targetY = (tempPosition[1] - parent.y) * parentParent._worldScaleY;
		} else {
			targetX -= parent.x;
			targetY -= parent.y;
		}
		if (child._parent == parent) {
			positionX = child.x;
			positionY = child.y;
		} else {
			tempPosition[0] = child.x;
			tempPosition[1] = child.y;
			child._parent.localToWorld(tempPosition);
			parent.worldToLocal(tempPosition);
			positionX = tempPosition[0];
			positionY = tempPosition[1];
		}
		var childX:Number = positionX * parent._worldScaleX, childY:Number = positionY * parent._worldScaleY;
		var offset:Number = Math.atan2(childY, childX);
		var len1:Number = Math.sqrt(childX * childX + childY * childY), len2:Number = child.data.length * child._worldScaleX;
		// Based on code by Ryan Juckett with permission: Copyright (c) 2008-2009 Ryan Juckett, http://www.ryanjuckett.com/
		var cosDenom:Number = 2 * len1 * len2;
		if (cosDenom < 0.0001) {
			child.rotationIK = childRotation + (Math.atan2(targetY, targetX) * radDeg - parentRotation - childRotation) * alpha;
			return;
		}
		var cos:Number = (targetX * targetX + targetY * targetY - len1 * len1 - len2 * len2) / cosDenom;
		if (cos < -1)
			cos = -1;
		else if (cos > 1)
			cos = 1;
		var childAngle:Number = Math.acos(cos) * bendDirection;
		var adjacent:Number = len1 + len2 * cos, opposite:Number = len2 * Math.sin(childAngle);
		var parentAngle:Number = Math.atan2(targetY * adjacent - targetX * opposite, targetX * adjacent + targetY * opposite);
		var rotation:Number = (parentAngle - offset) * radDeg - parentRotation;
		if (rotation > 180)
			rotation -= 360;
		else if (rotation < -180) //
			rotation += 360;
		parent.rotationIK = parentRotation + rotation * alpha;
		rotation = (childAngle + offset) * radDeg - childRotation;
		if (rotation > 180)
			rotation -= 360;
		else if (rotation < -180) //
			rotation += 360;
		child.rotationIK = childRotation + (rotation + parent._worldRotation - child._parent._worldRotation) * alpha;
	}
}

}
