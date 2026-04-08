/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2026, Esoteric Software LLC
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

using System;

namespace Spine {
	using FromProperty = TransformConstraintData.FromProperty;
	using ToProperty = TransformConstraintData.ToProperty;

	/// <summary>
	/// <para>
	/// Adjusts the world transform of the constrained bones to match that of the source bone.</para>
	/// <para>
	/// See <a href="http://esotericsoftware.com/spine-transform-constraints">Transform constraints</a> in the Spine User Guide.</para>
	/// </summary>
	public class TransformConstraint : Constraint<TransformConstraint, TransformConstraintData, TransformConstraintPose> {
		internal readonly ExposedList<BonePose> bones;
		internal Bone source;

		public TransformConstraint (TransformConstraintData data, Skeleton skeleton)
			: base(data, new TransformConstraintPose(), new TransformConstraintPose()) {
			if (skeleton == null) throw new ArgumentNullException("skeleton", "skeleton cannot be null.");

			bones = new ExposedList<BonePose>(data.bones.Count);
			foreach (BoneData boneData in data.bones)
				bones.Add(skeleton.bones.Items[boneData.index].constrainedPose);

			source = skeleton.bones.Items[data.source.index];
		}

		override public IConstraint Copy (Skeleton skeleton) {
			var copy = new TransformConstraint(data, skeleton);
			copy.pose.Set(pose);
			return copy;
		}

		/// <summary>Applies the constraint to the constrained bones.</summary>
		override public void Update (Skeleton skeleton, Physics physics) {
			TransformConstraintPose p = appliedPose;
			if (p.mixRotate == 0 && p.mixX == 0 && p.mixY == 0 && p.mixScaleX == 0 && p.mixScaleY == 0 && p.mixShearY == 0) return;

			TransformConstraintData data = this.data;
			bool localSource = data.localSource, localTarget = data.localTarget, additive = data.additive, clamp = data.clamp;
			float[] offsets = data.offsets;
			BonePose source = this.source.appliedPose;
			if (localSource) source.ValidateLocalTransform(skeleton);
			FromProperty[] fromItems = data.properties.Items;
			int fn = data.properties.Count, update = skeleton.update;
			BonePose[] bones = this.bones.Items;
			for (int i = 0, n = this.bones.Count; i < n; i++) {
				BonePose bone = bones[i];
				if (localTarget)
					bone.ModifyLocal(skeleton);
				else
					bone.ModifyWorld(update);
				for (int f = 0; f < fn; f++) {
					FromProperty from = fromItems[f];
					float value = from.Value(skeleton, source, localSource, offsets) - from.offset;
					ToProperty[] toItems = from.to.Items;
					for (int t = 0, tn = from.to.Count; t < tn; t++) {
						var to = (ToProperty)toItems[t];
						if (to.Mix(p) != 0) {
							float clamped = to.offset + value * to.scale;
							if (clamp) {
								if (to.offset < to.max)
									clamped = MathUtils.Clamp(clamped, to.offset, to.max);
								else
									clamped = MathUtils.Clamp(clamped, to.max, to.offset);
							}
							to.Apply(skeleton, p, bone, clamped, localTarget, additive);
						}
					}
				}
			}
		}

		override public void Sort (Skeleton skeleton) {
			if (!data.localSource) skeleton.SortBone(source);
			BonePose[] bones = this.bones.Items;
			int boneCount = this.bones.Count;
			bool worldTarget = !data.localTarget;
			if (worldTarget) {
				for (int i = 0; i < boneCount; i++)
					skeleton.SortBone(bones[i].bone);
			}
			skeleton.updateCache.Add(this);
			for (int i = 0; i < boneCount; i++) {
				Bone bone = bones[i].bone;
				skeleton.SortReset(bone.children);
				skeleton.Constrained(bone);
			}
			for (int i = 0; i < boneCount; i++)
				bones[i].bone.sorted = worldTarget;
		}

		override public bool IsSourceActive { get { return source.active; } }

		/// <summary>The bones that will be modified by this transform constraint.</summary>
		public ExposedList<BonePose> Bones { get { return bones; } }
		/// <summary>The bone whose world transform will be copied to the constrained bones.</summary>
		public Bone Source { get { return source; } set { source = value; } }
	}
}
