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

using System;
using System.Collections.Generic;

namespace Spine {
	public class IkConstraint {
		private const float radDeg = 180 / (float)Math.PI;

		internal IkConstraintData data;
		internal ExposedList<Bone> bones = new ExposedList<Bone>();
		internal Bone target;
		internal int bendDirection;
		internal float mix;

		public IkConstraintData Data { get { return data; } }
		public ExposedList<Bone> Bones { get { return bones; } }
		public Bone Target { get { return target; } set { target = value; } }
		public int BendDirection { get { return bendDirection; } set { bendDirection = value; } }
		public float Mix { get { return mix; } set { mix = value; } }

		public IkConstraint (IkConstraintData data, Skeleton skeleton) {
			if (data == null) throw new ArgumentNullException("data cannot be null.");
			if (skeleton == null) throw new ArgumentNullException("skeleton cannot be null.");
			this.data = data;
			mix = data.mix;
			bendDirection = data.bendDirection;

			bones = new ExposedList<Bone>(data.bones.Count);
			foreach (BoneData boneData in data.bones)
				bones.Add(skeleton.FindBone(boneData.name));
			target = skeleton.FindBone(data.target.name);
		}

		public void apply () {
			Bone target = this.target;
			ExposedList<Bone> bones = this.bones;
			switch (bones.Count) {
			case 1:
				apply(bones.Items[0], target.worldX, target.worldY, mix);
				break;
			case 2:
				apply(bones.Items[0], bones.Items[1], target.worldX, target.worldY, bendDirection, mix);
				break;
			}
		}

		override public String ToString () {
			return data.name;
		}

		/// <summary>Adjusts the bone rotation so the tip is as close to the target position as possible. The target is specified
		/// in the world coordinate system.</summary>
		static public void apply (Bone bone, float targetX, float targetY, float alpha) {
			float parentRotation = (!bone.data.inheritRotation || bone.parent == null) ? 0 : bone.parent.worldRotation;
			float rotation = bone.rotation;
			float rotationIK = (float)Math.Atan2(targetY - bone.worldY, targetX - bone.worldX) * radDeg;
			if (bone.worldFlipX != (bone.worldFlipY != Bone.yDown)) rotationIK = -rotationIK;
			rotationIK -= parentRotation;
			bone.rotationIK = rotation + (rotationIK - rotation) * alpha;
		}

		/// <summary>Adjusts the parent and child bone rotations so the tip of the child is as close to the target position as
		/// possible. The target is specified in the world coordinate system.</summary>
		/// <param name="child">Any descendant bone of the parent.</param>
		static public void apply (Bone parent, Bone child, float targetX, float targetY, int bendDirection, float alpha) {
			float childRotation = child.rotation, parentRotation = parent.rotation;
			if (alpha == 0) {
				child.rotationIK = childRotation;
				parent.rotationIK = parentRotation;
				return;
			}
			float positionX, positionY;
			Bone parentParent = parent.parent;
			if (parentParent != null) {
				parentParent.worldToLocal(targetX, targetY, out positionX, out positionY);
				targetX = (positionX - parent.x) * parentParent.worldScaleX;
				targetY = (positionY - parent.y) * parentParent.worldScaleY;
			} else {
				targetX -= parent.x;
				targetY -= parent.y;
			}
			if (child.parent == parent) {
				positionX = child.x;
				positionY = child.y;
			} else {
				child.parent.localToWorld(child.x, child.y, out positionX, out positionY);
				parent.worldToLocal(positionX, positionY, out positionX, out positionY);
			}
			float childX = positionX * parent.worldScaleX, childY = positionY * parent.worldScaleY;
			float offset = (float)Math.Atan2(childY, childX);
			float len1 = (float)Math.Sqrt(childX * childX + childY * childY), len2 = child.data.length * child.worldScaleX;
			// Based on code by Ryan Juckett with permission: Copyright (c) 2008-2009 Ryan Juckett, http://www.ryanjuckett.com/
			float cosDenom = 2 * len1 * len2;
			if (cosDenom < 0.0001f) {
				child.rotationIK = childRotation + ((float)Math.Atan2(targetY, targetX) * radDeg - parentRotation - childRotation)
					* alpha;
				return;
			}
			float cos = (targetX * targetX + targetY * targetY - len1 * len1 - len2 * len2) / cosDenom;
			if (cos < -1)
				cos = -1;
			else if (cos > 1)
				cos = 1;
			float childAngle = (float)Math.Acos(cos) * bendDirection;
			float adjacent = len1 + len2 * cos, opposite = len2 * (float)Math.Sin(childAngle);
			float parentAngle = (float)Math.Atan2(targetY * adjacent - targetX * opposite, targetX * adjacent + targetY * opposite);
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
}
