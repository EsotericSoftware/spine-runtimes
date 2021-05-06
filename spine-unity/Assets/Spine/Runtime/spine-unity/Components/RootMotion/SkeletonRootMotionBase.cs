/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

using UnityEngine;
using System.Collections.Generic;
using Spine.Unity.AnimationTools;
using System;

namespace Spine.Unity {

	/// <summary>
	/// Base class for skeleton root motion components.
	/// </summary>
	abstract public class SkeletonRootMotionBase : MonoBehaviour {

		#region Inspector
		[SpineBone]
		[SerializeField]
		protected string rootMotionBoneName = "root";
		public bool transformPositionX = true;
		public bool transformPositionY = true;

		public float rootMotionScaleX = 1;
		public float rootMotionScaleY = 1;
		/// <summary>Skeleton space X translation per skeleton space Y translation root motion.</summary>
		public float rootMotionTranslateXPerY = 0;
		/// <summary>Skeleton space Y translation per skeleton space X translation root motion.</summary>
		public float rootMotionTranslateYPerX = 0;

		[Header("Optional")]
		public Rigidbody2D rigidBody2D;
		public Rigidbody rigidBody;

		public bool UsesRigidbody {
			get { return rigidBody != null || rigidBody2D != null; }
		}
		#endregion

		protected ISkeletonComponent skeletonComponent;
		protected Bone rootMotionBone;
		protected int rootMotionBoneIndex;
		protected List<Bone> topLevelBones = new List<Bone>();
		protected Vector2 initialOffset = Vector2.zero;
		protected Vector2 tempSkeletonDisplacement;
		protected Vector2 rigidbodyDisplacement;

		protected virtual void Reset () {
			FindRigidbodyComponent();
		}

		protected virtual void Start () {
			skeletonComponent = GetComponent<ISkeletonComponent>();
			GatherTopLevelBones();
			SetRootMotionBone(rootMotionBoneName);
			if (rootMotionBone != null)
				initialOffset = new Vector2(rootMotionBone.x, rootMotionBone.y);

			var skeletonAnimation = skeletonComponent as ISkeletonAnimation;
			if (skeletonAnimation != null) {
				skeletonAnimation.UpdateLocal -= HandleUpdateLocal;
				skeletonAnimation.UpdateLocal += HandleUpdateLocal;
			}
		}

		protected virtual void FixedUpdate () {
			if (!this.isActiveAndEnabled)
				return; // Root motion is only applied when component is enabled.

			if (rigidBody2D != null) {
				rigidBody2D.MovePosition(new Vector2(transform.position.x, transform.position.y)
					+ rigidbodyDisplacement);
			}
			if (rigidBody != null) {
				rigidBody.MovePosition(transform.position
					+ new Vector3(rigidbodyDisplacement.x, rigidbodyDisplacement.y, 0));
			}
			Vector2 parentBoneScale;
			GetScaleAffectingRootMotion(out parentBoneScale);
			ClearEffectiveBoneOffsets(parentBoneScale);
			rigidbodyDisplacement = Vector2.zero;
			tempSkeletonDisplacement = Vector2.zero;
		}

		protected virtual void OnDisable () {
			rigidbodyDisplacement = Vector2.zero;
			tempSkeletonDisplacement = Vector2.zero;
		}

		protected void FindRigidbodyComponent () {
			rigidBody2D = this.GetComponent<Rigidbody2D>();
			if (!rigidBody2D)
				rigidBody = this.GetComponent<Rigidbody>();

			if (!rigidBody2D && !rigidBody) {
				rigidBody2D = this.GetComponentInParent<Rigidbody2D>();
				if (!rigidBody2D)
					rigidBody = this.GetComponentInParent<Rigidbody>();
			}
		}

		protected virtual float AdditionalScale { get { return 1.0f; } }
		abstract protected Vector2 CalculateAnimationsMovementDelta ();
		abstract public Vector2 GetRemainingRootMotion (int trackIndex = 0);

		public struct RootMotionInfo {
			public Vector2 start;
			public Vector2 current;
			public Vector2 mid;
			public Vector2 end;
			public bool timeIsPastMid;
		};
		abstract public RootMotionInfo GetRootMotionInfo (int trackIndex = 0);

		public void SetRootMotionBone (string name) {
			var skeleton = skeletonComponent.Skeleton;
			int index = skeleton.FindBoneIndex(name);
			if (index >= 0) {
				this.rootMotionBoneIndex = index;
				this.rootMotionBone = skeleton.bones.Items[index];
			}
			else {
				Debug.Log("Bone named \"" + name + "\" could not be found.");
				this.rootMotionBoneIndex = 0;
				this.rootMotionBone = skeleton.RootBone;
			}
		}

		public void AdjustRootMotionToDistance (Vector2 distanceToTarget, int trackIndex = 0, bool adjustX = true, bool adjustY = true,
			float minX = 0, float maxX = float.MaxValue, float minY = 0, float maxY = float.MaxValue,
			bool allowXTranslation = false, bool allowYTranslation = false) {

			Vector2 distanceToTargetSkeletonSpace = (Vector2)transform.InverseTransformVector(distanceToTarget);
			Vector2 scaleAffectingRootMotion = GetScaleAffectingRootMotion();
			if (UsesRigidbody)
				distanceToTargetSkeletonSpace -= tempSkeletonDisplacement;

			Vector2 remainingRootMotionSkeletonSpace = GetRemainingRootMotion(trackIndex);
			remainingRootMotionSkeletonSpace.Scale(scaleAffectingRootMotion);
			if (remainingRootMotionSkeletonSpace.x == 0)
				remainingRootMotionSkeletonSpace.x = 0.0001f;
			if (remainingRootMotionSkeletonSpace.y == 0)
				remainingRootMotionSkeletonSpace.y = 0.0001f;

			if (adjustX)
				rootMotionScaleX = Math.Min(maxX, Math.Max(minX, distanceToTargetSkeletonSpace.x / remainingRootMotionSkeletonSpace.x));
			if (adjustY)
				rootMotionScaleY = Math.Min(maxY, Math.Max(minY, distanceToTargetSkeletonSpace.y / remainingRootMotionSkeletonSpace.y));

			if (allowXTranslation)
				rootMotionTranslateXPerY = (distanceToTargetSkeletonSpace.x - remainingRootMotionSkeletonSpace.x * rootMotionScaleX) / remainingRootMotionSkeletonSpace.y;
			if (allowYTranslation)
				rootMotionTranslateYPerX = (distanceToTargetSkeletonSpace.y - remainingRootMotionSkeletonSpace.y * rootMotionScaleY) / remainingRootMotionSkeletonSpace.x;
		}

		public Vector2 GetAnimationRootMotion (Animation animation) {
			return GetAnimationRootMotion(0, animation.duration, animation);
		}

		public Vector2 GetAnimationRootMotion (float startTime, float endTime,
			Animation animation) {

			var timeline = animation.FindTranslateTimelineForBone(rootMotionBoneIndex);
			if (timeline != null) {
				return GetTimelineMovementDelta(startTime, endTime, timeline, animation);
			}
			return Vector2.zero;
		}

		public RootMotionInfo GetAnimationRootMotionInfo (Animation animation, float currentTime) {
			RootMotionInfo rootMotion = new RootMotionInfo();
			var timeline = animation.FindTranslateTimelineForBone(rootMotionBoneIndex);
			if (timeline != null) {
				float duration = animation.duration;
				float mid = duration * 0.5f;
				rootMotion.start = timeline.Evaluate(0);
				rootMotion.current = timeline.Evaluate(currentTime);
				rootMotion.mid = timeline.Evaluate(mid);
				rootMotion.end = timeline.Evaluate(duration);
				rootMotion.timeIsPastMid = currentTime > mid;
			}
			return rootMotion;
		}

		Vector2 GetTimelineMovementDelta (float startTime, float endTime,
			TranslateTimeline timeline, Animation animation) {

			Vector2 currentDelta;
			if (startTime > endTime) // Looped
				currentDelta = (timeline.Evaluate(animation.duration) - timeline.Evaluate(startTime))
					+ (timeline.Evaluate(endTime) - timeline.Evaluate(0));
			else if (startTime != endTime) // Non-looped
				currentDelta = timeline.Evaluate(endTime) - timeline.Evaluate(startTime);
			else
				currentDelta = Vector2.zero;
			return currentDelta;
		}

		void GatherTopLevelBones () {
			topLevelBones.Clear();
			var skeleton = skeletonComponent.Skeleton;
			foreach (var bone in skeleton.Bones) {
				if (bone.Parent == null)
					topLevelBones.Add(bone);
			}
		}

		void HandleUpdateLocal (ISkeletonAnimation animatedSkeletonComponent) {
			if (!this.isActiveAndEnabled)
				return; // Root motion is only applied when component is enabled.

			var boneLocalDelta = CalculateAnimationsMovementDelta();
			Vector2 parentBoneScale;
			Vector2 skeletonDelta = GetSkeletonSpaceMovementDelta(boneLocalDelta, out parentBoneScale);
			ApplyRootMotion(skeletonDelta, parentBoneScale);
		}

		void ApplyRootMotion (Vector2 skeletonDelta, Vector2 parentBoneScale) {
			// Apply root motion to Transform or RigidBody;
			if (UsesRigidbody) {
				rigidbodyDisplacement += (Vector2)transform.TransformVector(skeletonDelta);

				// Accumulated displacement is applied on the next Physics update in FixedUpdate.
				// Until the next Physics update, tempBoneDisplacement is offsetting bone locations
				// to prevent stutter which would otherwise occur if we don't move every Update.
				tempSkeletonDisplacement += skeletonDelta;
				SetEffectiveBoneOffsetsTo(tempSkeletonDisplacement, parentBoneScale);
			}
			else {
				transform.position += transform.TransformVector(skeletonDelta);
				ClearEffectiveBoneOffsets(parentBoneScale);
			}
		}

		Vector2 GetScaleAffectingRootMotion () {
			Vector2 parentBoneScale;
			return GetScaleAffectingRootMotion(out parentBoneScale);
		}

		Vector2 GetScaleAffectingRootMotion (out Vector2 parentBoneScale) {
			var skeleton = skeletonComponent.Skeleton;
			Vector2 totalScale = Vector2.one;
			totalScale.x *= skeleton.ScaleX;
			totalScale.y *= skeleton.ScaleY;

			parentBoneScale = Vector2.one;
			Bone scaleBone = rootMotionBone;
			while ((scaleBone = scaleBone.parent) != null) {
				parentBoneScale.x *= scaleBone.ScaleX;
				parentBoneScale.y *= scaleBone.ScaleY;
			}
			totalScale = Vector2.Scale(totalScale, parentBoneScale);
			totalScale *= AdditionalScale;
			return totalScale;
		}

		Vector2 GetSkeletonSpaceMovementDelta (Vector2 boneLocalDelta, out Vector2 parentBoneScale) {
			Vector2 skeletonDelta = boneLocalDelta;
			Vector2 totalScale = GetScaleAffectingRootMotion(out parentBoneScale);
			skeletonDelta.Scale(totalScale);

			Vector2 rootMotionTranslation = new Vector2(
				rootMotionTranslateXPerY * skeletonDelta.y,
				rootMotionTranslateYPerX * skeletonDelta.x);

			skeletonDelta.x *= rootMotionScaleX;
			skeletonDelta.y *= rootMotionScaleY;
			skeletonDelta.x += rootMotionTranslation.x;
			skeletonDelta.y += rootMotionTranslation.y;

			if (!transformPositionX) skeletonDelta.x = 0f;
			if (!transformPositionY) skeletonDelta.y = 0f;
			return skeletonDelta;
		}

		void SetEffectiveBoneOffsetsTo (Vector2 displacementSkeletonSpace, Vector2 parentBoneScale) {
			// Move top level bones in opposite direction of the root motion bone
			var skeleton = skeletonComponent.Skeleton;
			foreach (var topLevelBone in topLevelBones) {
				if (topLevelBone == rootMotionBone) {
					if (transformPositionX) topLevelBone.x = displacementSkeletonSpace.x / skeleton.ScaleX;
					if (transformPositionY) topLevelBone.y = displacementSkeletonSpace.y / skeleton.ScaleY;
				}
				else {
					float offsetX = (initialOffset.x - rootMotionBone.x) * parentBoneScale.x;
					float offsetY = (initialOffset.y - rootMotionBone.y) * parentBoneScale.y;
					if (transformPositionX) topLevelBone.x = (displacementSkeletonSpace.x / skeleton.ScaleX) + offsetX;
					if (transformPositionY) topLevelBone.y = (displacementSkeletonSpace.y / skeleton.ScaleY) + offsetY;
				}
			}
		}

		void ClearEffectiveBoneOffsets (Vector2 parentBoneScale) {
			SetEffectiveBoneOffsetsTo(Vector2.zero, parentBoneScale);
		}
	}
}
