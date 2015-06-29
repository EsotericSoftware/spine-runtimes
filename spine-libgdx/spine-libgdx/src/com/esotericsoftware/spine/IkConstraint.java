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

package com.esotericsoftware.spine;

import static com.badlogic.gdx.math.MathUtils.*;

import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.utils.Array;

public class IkConstraint {
	static private final Vector2 temp = new Vector2();

	final IkConstraintData data;
	final Array<Bone> bones;
	Bone target;
	float mix = 1;
	int bendDirection;

	public IkConstraint (IkConstraintData data, Skeleton skeleton) {
		this.data = data;
		mix = data.mix;
		bendDirection = data.bendDirection;

		bones = new Array(data.bones.size);
		if (skeleton != null) {
			for (BoneData boneData : data.bones)
				bones.add(skeleton.findBone(boneData.name));
			target = skeleton.findBone(data.target.name);
		}
	}

	/** Copy constructor. */
	public IkConstraint (IkConstraint ikConstraint, Array<Bone> bones, Bone target) {
		data = ikConstraint.data;
		this.bones = bones;
		this.target = target;
		mix = ikConstraint.mix;
		bendDirection = ikConstraint.bendDirection;
	}

	public void apply () {
		Bone target = this.target;
		Array<Bone> bones = this.bones;
		switch (bones.size) {
		case 1:
			apply(bones.first(), target.worldX, target.worldY, mix);
			break;
		case 2:
			apply(bones.first(), bones.get(1), target.worldX, target.worldY, bendDirection, mix);
			break;
		}
	}

	public Array<Bone> getBones () {
		return bones;
	}

	public Bone getTarget () {
		return target;
	}

	public void setTarget (Bone target) {
		this.target = target;
	}

	public float getMix () {
		return mix;
	}

	public void setMix (float mix) {
		this.mix = mix;
	}

	public int getBendDirection () {
		return bendDirection;
	}

	public void setBendDirection (int bendDirection) {
		this.bendDirection = bendDirection;
	}

	public IkConstraintData getData () {
		return data;
	}

	public String toString () {
		return data.name;
	}

	/** Adjusts the bone rotation so the tip is as close to the target position as possible. The target is specified in the world
	 * coordinate system. */
	static public void apply (Bone bone, float targetX, float targetY, float alpha) {
		float parentRotation = (!bone.data.inheritRotation || bone.parent == null) ? 0 : bone.parent.worldRotation;
		float rotation = bone.rotation;
		float rotationIK = (float)Math.atan2(targetY - bone.worldY, targetX - bone.worldX) * radDeg - parentRotation;
		bone.rotationIK = rotation + (rotationIK - rotation) * alpha;
	}

	/** Adjusts the parent and child bone rotations so the tip of the child is as close to the target position as possible. The
	 * target is specified in the world coordinate system.
	 * @param child Any descendant bone of the parent. */
	static public void apply (Bone parent, Bone child, float targetX, float targetY, int bendDirection, float alpha) {
		float childRotation = child.rotation, parentRotation = parent.rotation;
		if (alpha == 0) {
			child.rotationIK = childRotation;
			parent.rotationIK = parentRotation;
			return;
		}
		Vector2 position = temp;
		Bone parentParent = parent.parent;
		if (parentParent != null) {
			parentParent.worldToLocal(position.set(targetX, targetY));
			targetX = (position.x - parent.x) * parentParent.worldScaleX;
			targetY = (position.y - parent.y) * parentParent.worldScaleY;
		} else {
			targetX -= parent.x;
			targetY -= parent.y;
		}
		if (child.parent == parent)
			position.set(child.x, child.y);
		else
			parent.worldToLocal(child.parent.localToWorld(position.set(child.x, child.y)));
		float childX = position.x * parent.worldScaleX, childY = position.y * parent.worldScaleY;
		float offset = (float)Math.atan2(childY, childX);
		float len1 = (float)Math.sqrt(childX * childX + childY * childY), len2 = child.data.length * child.worldScaleX;
		// Based on code by Ryan Juckett with permission: Copyright (c) 2008-2009 Ryan Juckett, http://www.ryanjuckett.com/
		float cosDenom = 2 * len1 * len2;
		if (cosDenom < 0.0001f) {
			child.rotationIK = childRotation + ((float)Math.atan2(targetY, targetX) * radDeg - parentRotation - childRotation)
				* alpha;
			return;
		}
		float cos = clamp((targetX * targetX + targetY * targetY - len1 * len1 - len2 * len2) / cosDenom, -1, 1);
		float childAngle = (float)Math.acos(cos) * bendDirection;
		float adjacent = len1 + len2 * cos, opposite = len2 * sin(childAngle);
		float parentAngle = (float)Math.atan2(targetY * adjacent - targetX * opposite, targetX * adjacent + targetY * opposite);
		float rotation = (parentAngle - offset) * radDeg - parentRotation;
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
		child.rotationIK = childRotation + (rotation + parent.worldRotation - child.parent.worldRotation) * alpha;
	}
}
