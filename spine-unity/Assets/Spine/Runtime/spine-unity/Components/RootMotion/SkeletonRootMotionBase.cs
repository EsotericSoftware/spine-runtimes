/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2022, Esoteric Software LLC
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

using Spine.Unity.AnimationTools;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity {

	/// <summary>
	/// Base class for skeleton root motion components.
	/// </summary>
	[DefaultExecutionOrder(1)]
	abstract public class SkeletonRootMotionBase : MonoBehaviour {

		#region Inspector
		[SpineBone]
		[SerializeField]
		protected string rootMotionBoneName = "root";
		public bool transformPositionX = true;
		public bool transformPositionY = true;
		public bool transformRotation = false;

		public float rootMotionScaleX = 1;
		public float rootMotionScaleY = 1;
		public float rootMotionScaleRotation = 1;
		/// <summary>Skeleton space X translation per skeleton space Y translation root motion.</summary>
		public float rootMotionTranslateXPerY = 0;
		/// <summary>Skeleton space Y translation per skeleton space X translation root motion.</summary>
		public float rootMotionTranslateYPerX = 0;

		[Header("Optional")]
		public Rigidbody2D rigidBody2D;
		public bool applyRigidbody2DGravity = false;
		public Rigidbody rigidBody;

		/// <summary>Delegate type for customizing application of rootmotion.
		public delegate void RootMotionDelegate (SkeletonRootMotionBase component, Vector2 translation, float rotation);
		/// <summary>This callback can be used to apply root-motion in a custom way. It is raised after evaluating
		/// this animation frame's root-motion, before it is potentially applied (see <see cref="disableOnOverride"/>)
		/// to either Transform or Rigidbody.
		/// When <see cref="SkeletonAnimation.UpdateTiming"/> is set to <see cref="UpdateTiming.InUpdate"/>, multiple
		/// animation frames might take place before <c>FixedUpdate</c> is called once.
		/// The callback parameters <c>translation</c> and <c>rotation</c> are filled out with
		/// this animation frame's skeleton-space root-motion (not cumulated). You can use
		/// e.g. <c>transform.TransformVector()</c> to transform skeleton-space root-motion to world space.
		/// </summary>
		/// <seealso cref="PhysicsUpdateRootMotionOverride"/>
		public event RootMotionDelegate ProcessRootMotionOverride;
		/// <summary>This callback can be used to apply root-motion in a custom way. It is raised in FixedUpdate
		/// after (when <see cref="disableOnOverride"/> is set to false) or instead of when root-motion
		/// would be applied at the Rigidbody.
		/// When <see cref="SkeletonAnimation.UpdateTiming"/> is set to <see cref="UpdateTiming.InUpdate"/>, multiple
		/// animation frames might take place before before <c>FixedUpdate</c> is called once.
		/// The callback parameters <c>translation</c> and <c>rotation</c> are filled out with the
		/// (cumulated) skeleton-space root-motion since the the last <c>FixedUpdate</c> call. You can use
		/// e.g. <c>transform.TransformVector()</c> to transform skeleton-space root-motion to world space.
		/// </summary>
		/// <seealso cref="ProcessRootMotionOverride"/>
		public event RootMotionDelegate PhysicsUpdateRootMotionOverride;
		/// <summary>When true, root-motion is not applied to the Transform or Rigidbody.
		/// Otherwise the delegate callbacks are issued additionally.</summary>
		public bool disableOnOverride = true;

		public Bone RootMotionBone { get { return rootMotionBone; } }

		public bool UsesRigidbody {
			get { return rigidBody != null || rigidBody2D != null; }
		}

		/// <summary>Root motion translation that has been applied in the preceding <c>FixedUpdate</c> call
		/// if a rigidbody is assigned at either <c>rigidbody</c> or <c>rigidbody2D</c>.
		/// Returns <c>Vector2.zero</c> when <c>rigidbody</c> and <c>rigidbody2D</c> are null.
		/// This can be necessary when multiple scripts call <c>Rigidbody2D.MovePosition</c>,
		/// where the last call overwrites the effect of preceding ones.</summary>
		public Vector2 PreviousRigidbodyRootMotion2D {
			get { return new Vector2(previousRigidbodyRootMotion.x, previousRigidbodyRootMotion.y); }
		}

		/// <summary>Root motion translation that has been applied in the preceding <c>FixedUpdate</c> call
		/// if a rigidbody is assigned at either <c>rigidbody</c> or <c>rigidbody2D</c>.
		/// Returns <c>Vector3.zero</c> when <c>rigidbody</c> and <c>rigidbody2D</c> are null.</summary>
		public Vector3 PreviousRigidbodyRootMotion3D {
			get { return previousRigidbodyRootMotion; }
		}

		/// <summary>Additional translation to add to <c>Rigidbody2D.MovePosition</c>
		/// called in FixedUpdate. This can be necessary when multiple scripts call
		/// <c>MovePosition</c>, where the last call overwrites the effect of preceding ones.
		/// Has no effect if <c>rigidBody2D</c> is null.</summary>
		public Vector2 AdditionalRigidbody2DMovement {
			get { return additionalRigidbody2DMovement; }
			set { additionalRigidbody2DMovement = value; }
		}
		#endregion

		protected bool SkeletonAnimationUsesFixedUpdate {
			get {
				ISkeletonAnimation skeletonAnimation = skeletonComponent as ISkeletonAnimation;
				if (skeletonAnimation != null) {
					return skeletonAnimation.UpdateTiming == UpdateTiming.InFixedUpdate;
				}
				return false;
			}
		}

		protected ISkeletonComponent skeletonComponent;
		protected Bone rootMotionBone;
		protected int rootMotionBoneIndex;
		protected List<int> transformConstraintIndices = new List<int>();
		protected List<Vector2> transformConstraintLastPos = new List<Vector2>();
		protected List<float> transformConstraintLastRotation = new List<float>();
		protected List<Bone> topLevelBones = new List<Bone>();
		protected Vector2 initialOffset = Vector2.zero;
		protected bool accumulatedUntilFixedUpdate = false;
		protected Vector2 tempSkeletonDisplacement;
		protected Vector3 rigidbodyDisplacement;
		protected Vector3 previousRigidbodyRootMotion = Vector2.zero;
		protected Vector2 additionalRigidbody2DMovement = Vector2.zero;

		protected Quaternion rigidbodyLocalRotation = Quaternion.identity;
		protected float rigidbody2DRotation;
		protected float initialOffsetRotation;
		protected float tempSkeletonRotation;

		protected virtual void Reset () {
			FindRigidbodyComponent();
		}

		protected virtual void Start () {
			skeletonComponent = GetComponent<ISkeletonComponent>();
			GatherTopLevelBones();
			SetRootMotionBone(rootMotionBoneName);
			if (rootMotionBone != null) {
				initialOffset = new Vector2(rootMotionBone.X, rootMotionBone.Y);
				initialOffsetRotation = rootMotionBone.Rotation;
			}

			ISkeletonAnimation skeletonAnimation = skeletonComponent as ISkeletonAnimation;
			if (skeletonAnimation != null) {
				skeletonAnimation.UpdateLocal -= HandleUpdateLocal;
				skeletonAnimation.UpdateLocal += HandleUpdateLocal;
			}
		}

		protected virtual void FixedUpdate () {
			// Root motion is only applied when component is enabled.
			if (!this.isActiveAndEnabled)
				return;
			// When SkeletonAnimation component uses UpdateTiming.InFixedUpdate,
			// we directly call PhysicsUpdate in HandleUpdateLocal instead of here.
			if (!SkeletonAnimationUsesFixedUpdate)
				PhysicsUpdate(false);
		}

		protected virtual void PhysicsUpdate (bool skeletonAnimationUsesFixedUpdate) {
			Vector2 callbackDisplacement = tempSkeletonDisplacement;
			float callbackRotation = tempSkeletonRotation;

			bool isApplyAtRigidbodyAllowed = PhysicsUpdateRootMotionOverride == null || !disableOnOverride;
			if (isApplyAtRigidbodyAllowed) {
				if (rigidBody2D != null) {
					Vector2 gravityAndVelocityMovement = Vector2.zero;
					if (applyRigidbody2DGravity) {
						float deltaTime = Time.fixedDeltaTime;
						float deltaTimeSquared = (deltaTime * deltaTime);

						rigidBody2D.velocity += rigidBody2D.gravityScale * Physics2D.gravity * deltaTime;
						gravityAndVelocityMovement = 0.5f * rigidBody2D.gravityScale * Physics2D.gravity * deltaTimeSquared +
							rigidBody2D.velocity * deltaTime;
					}

					Vector2 rigidbodyDisplacement2D = new Vector2(rigidbodyDisplacement.x, rigidbodyDisplacement.y);
					rigidBody2D.MovePosition(gravityAndVelocityMovement + new Vector2(rigidBody2D.position.x, rigidBody2D.position.y)
						+ rigidbodyDisplacement2D + additionalRigidbody2DMovement);
					rigidBody2D.MoveRotation(rigidbody2DRotation + rigidBody2D.rotation);
				} else if (rigidBody != null) {
					rigidBody.MovePosition(rigidBody.position
						+ new Vector3(rigidbodyDisplacement.x, rigidbodyDisplacement.y, rigidbodyDisplacement.z));
					rigidBody.MoveRotation(rigidBody.rotation * rigidbodyLocalRotation);
				}
			}

			previousRigidbodyRootMotion = rigidbodyDisplacement;
			if (accumulatedUntilFixedUpdate) {
				Vector2 parentBoneScale;
				GetScaleAffectingRootMotion(out parentBoneScale);
				ClearEffectiveBoneOffsets(parentBoneScale);
				skeletonComponent.Skeleton.UpdateWorldTransform();
			}
			ClearRigidbodyTempMovement();

			if (PhysicsUpdateRootMotionOverride != null)
				PhysicsUpdateRootMotionOverride(this, callbackDisplacement, callbackRotation);
		}

		protected virtual void OnDisable () {
			ClearRigidbodyTempMovement();
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
		protected virtual float CalculateAnimationsRotationDelta () { return 0; }
		abstract public Vector2 GetRemainingRootMotion (int trackIndex = 0);

		public struct RootMotionInfo {
			public Vector2 start;
			public Vector2 current;
			public Vector2 mid;
			public Vector2 end;
			public bool timeIsPastMid;
		};
		abstract public RootMotionInfo GetRootMotionInfo (int trackIndex = 0);

		public ISkeletonComponent TargetSkeletonComponent {
			get {
				if (skeletonComponent == null)
					skeletonComponent = GetComponent<ISkeletonComponent>();
				return skeletonComponent;
			}
		}

		public ISkeletonAnimation TargetSkeletonAnimationComponent {
			get { return TargetSkeletonComponent as ISkeletonAnimation; }
		}

		public void SetRootMotionBone (string name) {
			Skeleton skeleton = skeletonComponent.Skeleton;
			Bone bone = skeleton.FindBone(name);
			if (bone != null) {
				this.rootMotionBoneIndex = bone.Data.Index;
				this.rootMotionBone = bone;
				FindTransformConstraintsAffectingBone();
			} else {
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
			return GetAnimationRootMotion(0, animation.Duration, animation);
		}

		public Vector2 GetAnimationRootMotion (float startTime, float endTime,
			Animation animation) {

			if (startTime == endTime)
				return Vector2.zero;

			TranslateTimeline translateTimeline = animation.FindTranslateTimelineForBone(rootMotionBoneIndex);
			TranslateXTimeline xTimeline = animation.FindTimelineForBone<TranslateXTimeline>(rootMotionBoneIndex);
			TranslateYTimeline yTimeline = animation.FindTimelineForBone<TranslateYTimeline>(rootMotionBoneIndex);

			// Non-looped base
			Vector2 endPos = Vector2.zero;
			Vector2 startPos = Vector2.zero;
			if (translateTimeline != null) {
				endPos = translateTimeline.Evaluate(endTime);
				startPos = translateTimeline.Evaluate(startTime);
			} else if (xTimeline != null || yTimeline != null) {
				endPos = TimelineExtensions.Evaluate(xTimeline, yTimeline, endTime);
				startPos = TimelineExtensions.Evaluate(xTimeline, yTimeline, startTime);
			}
			TransformConstraint[] transformConstraintsItems = skeletonComponent.Skeleton.TransformConstraints.Items;
			foreach (int constraintIndex in this.transformConstraintIndices) {
				TransformConstraint constraint = transformConstraintsItems[constraintIndex];
				ApplyConstraintToPos(animation, constraint, constraintIndex, endTime, false, ref endPos);
				ApplyConstraintToPos(animation, constraint, constraintIndex, startTime, true, ref startPos);
			}
			Vector2 currentDelta = endPos - startPos;

			// Looped additions
			if (startTime > endTime) {
				Vector2 loopPos = Vector2.zero;
				Vector2 zeroPos = Vector2.zero;
				if (translateTimeline != null) {
					loopPos = translateTimeline.Evaluate(animation.Duration);
					zeroPos = translateTimeline.Evaluate(0);
				} else if (xTimeline != null || yTimeline != null) {
					loopPos = TimelineExtensions.Evaluate(xTimeline, yTimeline, animation.Duration);
					zeroPos = TimelineExtensions.Evaluate(xTimeline, yTimeline, 0);
				}
				foreach (int constraintIndex in this.transformConstraintIndices) {
					TransformConstraint constraint = transformConstraintsItems[constraintIndex];
					ApplyConstraintToPos(animation, constraint, constraintIndex, animation.Duration, false, ref loopPos);
					ApplyConstraintToPos(animation, constraint, constraintIndex, 0, false, ref zeroPos);
				}
				currentDelta += loopPos - zeroPos;
			}
			UpdateLastConstraintPos(transformConstraintsItems);
			return currentDelta;
		}

		public float GetAnimationRootMotionRotation (Animation animation) {
			return GetAnimationRootMotionRotation(0, animation.Duration, animation);
		}

		public float GetAnimationRootMotionRotation (float startTime, float endTime,
			Animation animation) {

			if (startTime == endTime)
				return 0;

			RotateTimeline rotateTimeline = animation.FindTimelineForBone<RotateTimeline>(rootMotionBoneIndex);

			// Non-looped base
			float endRotation = 0;
			float startRotation = 0;
			if (rotateTimeline != null) {
				endRotation = rotateTimeline.Evaluate(endTime);
				startRotation = rotateTimeline.Evaluate(startTime);
			}
			TransformConstraint[] transformConstraintsItems = skeletonComponent.Skeleton.TransformConstraints.Items;
			foreach (int constraintIndex in this.transformConstraintIndices) {
				TransformConstraint constraint = transformConstraintsItems[constraintIndex];
				ApplyConstraintToRotation(animation, constraint, constraintIndex, endTime, false, ref endRotation);
				ApplyConstraintToRotation(animation, constraint, constraintIndex, startTime, true, ref startRotation);
			}
			float currentDelta = endRotation - startRotation;

			// Looped additions
			if (startTime > endTime) {
				float loopRotation = 0;
				float zeroPos = 0;
				if (rotateTimeline != null) {
					loopRotation = rotateTimeline.Evaluate(animation.Duration);
					zeroPos = rotateTimeline.Evaluate(0);
				}
				foreach (int constraintIndex in this.transformConstraintIndices) {
					TransformConstraint constraint = transformConstraintsItems[constraintIndex];
					ApplyConstraintToRotation(animation, constraint, constraintIndex, animation.Duration, false, ref loopRotation);
					ApplyConstraintToRotation(animation, constraint, constraintIndex, 0, false, ref zeroPos);
				}
				currentDelta += loopRotation - zeroPos;
			}
			UpdateLastConstraintRotation(transformConstraintsItems);
			return currentDelta;
		}

		void ApplyConstraintToPos (Animation animation, TransformConstraint constraint,
			int constraintIndex, float time, bool useLastConstraintPos, ref Vector2 pos) {
			TransformConstraintTimeline timeline = animation.FindTransformConstraintTimeline(constraintIndex);
			if (timeline == null)
				return;
			Vector2 mixXY = timeline.EvaluateTranslateXYMix(time);
			Vector2 invMixXY = timeline.EvaluateTranslateXYMix(time);
			Vector2 constraintPos;
			if (useLastConstraintPos)
				constraintPos = transformConstraintLastPos[GetConstraintLastPosIndex(constraintIndex)];
			else {
				Bone targetBone = constraint.Target;
				constraintPos = new Vector2(targetBone.X, targetBone.Y);
			}
			pos = new Vector2(
				pos.x * invMixXY.x + constraintPos.x * mixXY.x,
				pos.y * invMixXY.y + constraintPos.y * mixXY.y);
		}

		void ApplyConstraintToRotation (Animation animation, TransformConstraint constraint,
			int constraintIndex, float time, bool useLastConstraintRotation, ref float rotation) {
			TransformConstraintTimeline timeline = animation.FindTransformConstraintTimeline(constraintIndex);
			if (timeline == null)
				return;
			float mixRotate = timeline.EvaluateRotateMix(time);
			float invMixRotate = timeline.EvaluateRotateMix(time);
			float constraintRotation;
			if (useLastConstraintRotation)
				constraintRotation = transformConstraintLastRotation[GetConstraintLastPosIndex(constraintIndex)];
			else {
				Bone targetBone = constraint.Target;
				constraintRotation = targetBone.Rotation;
			}
			rotation = rotation * invMixRotate + constraintRotation * mixRotate;
		}

		void UpdateLastConstraintPos (TransformConstraint[] transformConstraintsItems) {
			foreach (int constraintIndex in this.transformConstraintIndices) {
				TransformConstraint constraint = transformConstraintsItems[constraintIndex];
				Bone targetBone = constraint.Target;
				transformConstraintLastPos[GetConstraintLastPosIndex(constraintIndex)] = new Vector2(targetBone.X, targetBone.Y);
			}
		}

		void UpdateLastConstraintRotation (TransformConstraint[] transformConstraintsItems) {
			foreach (int constraintIndex in this.transformConstraintIndices) {
				TransformConstraint constraint = transformConstraintsItems[constraintIndex];
				Bone targetBone = constraint.Target;
				transformConstraintLastRotation[GetConstraintLastPosIndex(constraintIndex)] = targetBone.Rotation;
			}
		}

		public RootMotionInfo GetAnimationRootMotionInfo (Animation animation, float currentTime) {
			RootMotionInfo rootMotion = new RootMotionInfo();
			float duration = animation.Duration;
			float mid = duration * 0.5f;
			rootMotion.timeIsPastMid = currentTime > mid;
			TranslateTimeline timeline = animation.FindTranslateTimelineForBone(rootMotionBoneIndex);
			if (timeline != null) {
				rootMotion.start = timeline.Evaluate(0);
				rootMotion.current = timeline.Evaluate(currentTime);
				rootMotion.mid = timeline.Evaluate(mid);
				rootMotion.end = timeline.Evaluate(duration);
				return rootMotion;
			}
			TranslateXTimeline xTimeline = animation.FindTimelineForBone<TranslateXTimeline>(rootMotionBoneIndex);
			TranslateYTimeline yTimeline = animation.FindTimelineForBone<TranslateYTimeline>(rootMotionBoneIndex);
			if (xTimeline != null || yTimeline != null) {
				rootMotion.start = TimelineExtensions.Evaluate(xTimeline, yTimeline, 0);
				rootMotion.current = TimelineExtensions.Evaluate(xTimeline, yTimeline, currentTime);
				rootMotion.mid = TimelineExtensions.Evaluate(xTimeline, yTimeline, mid);
				rootMotion.end = TimelineExtensions.Evaluate(xTimeline, yTimeline, duration);
				return rootMotion;
			}
			return rootMotion;
		}

		int GetConstraintLastPosIndex (int constraintIndex) {
			ExposedList<TransformConstraint> constraints = skeletonComponent.Skeleton.TransformConstraints;
			TransformConstraint targetConstraint = constraints.Items[constraintIndex];
			return transformConstraintIndices.FindIndex(addedIndex => addedIndex == constraintIndex);
		}

		void FindTransformConstraintsAffectingBone () {
			ExposedList<TransformConstraint> constraints = skeletonComponent.Skeleton.TransformConstraints;
			TransformConstraint[] constraintsItems = constraints.Items;
			for (int i = 0, n = constraints.Count; i < n; ++i) {
				TransformConstraint constraint = constraintsItems[i];
				if (constraint.Bones.Contains(rootMotionBone)) {
					transformConstraintIndices.Add(i);
					Bone targetBone = constraint.Target;
					Vector2 constraintPos = new Vector2(targetBone.X, targetBone.Y);
					transformConstraintLastPos.Add(constraintPos);
					transformConstraintLastRotation.Add(targetBone.Rotation);
				}
			}
		}

		Vector2 GetTimelineMovementDelta (float startTime, float endTime,
			TranslateXTimeline xTimeline, TranslateYTimeline yTimeline, Animation animation) {

			Vector2 currentDelta;
			if (startTime > endTime) // Looped
				currentDelta =
					(TimelineExtensions.Evaluate(xTimeline, yTimeline, animation.Duration)
					- TimelineExtensions.Evaluate(xTimeline, yTimeline, startTime))
					+ (TimelineExtensions.Evaluate(xTimeline, yTimeline, endTime)
					- TimelineExtensions.Evaluate(xTimeline, yTimeline, 0));
			else if (startTime != endTime) // Non-looped
				currentDelta = TimelineExtensions.Evaluate(xTimeline, yTimeline, endTime)
					- TimelineExtensions.Evaluate(xTimeline, yTimeline, startTime);
			else
				currentDelta = Vector2.zero;
			return currentDelta;
		}

		void GatherTopLevelBones () {
			topLevelBones.Clear();
			Skeleton skeleton = skeletonComponent.Skeleton;
			foreach (Bone bone in skeleton.Bones) {
				if (bone.Parent == null)
					topLevelBones.Add(bone);
			}
		}

		void HandleUpdateLocal (ISkeletonAnimation animatedSkeletonComponent) {
			if (!this.isActiveAndEnabled)
				return; // Root motion is only applied when component is enabled.

			Vector2 boneLocalDelta = CalculateAnimationsMovementDelta();
			Vector2 parentBoneScale;
			Vector2 totalScale;
			Vector2 skeletonTranslationDelta = GetSkeletonSpaceMovementDelta(boneLocalDelta, out parentBoneScale, out totalScale);
			float skeletonRotationDelta = 0;
			if (transformRotation) {
				float boneLocalDeltaRotation = CalculateAnimationsRotationDelta();
				boneLocalDeltaRotation *= rootMotionScaleRotation;
				skeletonRotationDelta = GetSkeletonSpaceRotationDelta(boneLocalDeltaRotation, totalScale);
			}

			bool usesFixedUpdate = SkeletonAnimationUsesFixedUpdate;
			ApplyRootMotion(skeletonTranslationDelta, skeletonRotationDelta, parentBoneScale, usesFixedUpdate);

			if (usesFixedUpdate)
				PhysicsUpdate(usesFixedUpdate);
		}

		void ApplyRootMotion (Vector2 skeletonTranslationDelta, float skeletonRotationDelta, Vector2 parentBoneScale,
			bool skeletonAnimationUsesFixedUpdate) {

			// Accumulated displacement is applied on the next Physics update in FixedUpdate.
			// Until the next Physics update, tempSkeletonDisplacement and tempSkeletonRotation
			// are offsetting bone locations to prevent stutter which would otherwise occur if
			// we don't move every Update.
			bool usesRigidbody = this.UsesRigidbody;
			bool applyToTransform = !usesRigidbody && (ProcessRootMotionOverride == null || !disableOnOverride);
			accumulatedUntilFixedUpdate = !applyToTransform && !skeletonAnimationUsesFixedUpdate;

			if (ProcessRootMotionOverride != null)
				ProcessRootMotionOverride(this, skeletonTranslationDelta, skeletonRotationDelta);

			// Apply root motion to Transform or update values applied to RigidBody later (must happen in FixedUpdate).
			if (usesRigidbody) {
				rigidbodyDisplacement += transform.TransformVector(skeletonTranslationDelta);
				if (skeletonRotationDelta != 0.0f) {
					if (rigidBody != null) {
						Quaternion addedWorldRotation = Quaternion.Euler(0, 0, skeletonRotationDelta);
						rigidbodyLocalRotation = rigidbodyLocalRotation * addedWorldRotation;
					} else if (rigidBody2D != null) {
						Vector3 lossyScale = transform.lossyScale;
						float rotationSign = lossyScale.x * lossyScale.y > 0 ? 1 : -1;
						rigidbody2DRotation += rotationSign * skeletonRotationDelta;
					}
				}
			} else if (applyToTransform) {
				transform.position += transform.TransformVector(skeletonTranslationDelta);
				if (skeletonRotationDelta != 0.0f) {
					Vector3 lossyScale = transform.lossyScale;
					float rotationSign = lossyScale.x * lossyScale.y > 0 ? 1 : -1;
					transform.Rotate(0, 0, rotationSign * skeletonRotationDelta);
				}
			}

			tempSkeletonDisplacement += skeletonTranslationDelta;
			tempSkeletonRotation += skeletonRotationDelta;
			if (accumulatedUntilFixedUpdate) {
				SetEffectiveBoneOffsetsTo(tempSkeletonDisplacement, tempSkeletonRotation, parentBoneScale);
			} else {
				ClearEffectiveBoneOffsets(parentBoneScale);
			}
		}

		void ApplyTransformConstraints () {
			rootMotionBone.AX = rootMotionBone.X;
			rootMotionBone.AY = rootMotionBone.Y;
			rootMotionBone.AppliedRotation = rootMotionBone.Rotation;
			TransformConstraint[] transformConstraintsItems = skeletonComponent.Skeleton.TransformConstraints.Items;
			foreach (int constraintIndex in this.transformConstraintIndices) {
				TransformConstraint constraint = transformConstraintsItems[constraintIndex];
				// apply the constraint and sets Bone.ax, Bone.ay and Bone.arotation values.
				/// Update is based on Bone.x, Bone.y and Bone.rotation, so skeleton.UpdateWorldTransform()
				/// can be called afterwards without having a different starting point.
				constraint.Update();
			}
		}

		Vector2 GetScaleAffectingRootMotion () {
			Vector2 parentBoneScale;
			return GetScaleAffectingRootMotion(out parentBoneScale);
		}

		Vector2 GetScaleAffectingRootMotion (out Vector2 parentBoneScale) {
			Skeleton skeleton = skeletonComponent.Skeleton;
			Vector2 totalScale = Vector2.one;
			totalScale.x *= skeleton.ScaleX;
			totalScale.y *= skeleton.ScaleY;

			parentBoneScale = Vector2.one;
			Bone scaleBone = rootMotionBone;
			while ((scaleBone = scaleBone.Parent) != null) {
				parentBoneScale.x *= scaleBone.ScaleX;
				parentBoneScale.y *= scaleBone.ScaleY;
			}
			totalScale = Vector2.Scale(totalScale, parentBoneScale);
			totalScale *= AdditionalScale;
			return totalScale;
		}

		Vector2 GetSkeletonSpaceMovementDelta (Vector2 boneLocalDelta, out Vector2 parentBoneScale, out Vector2 totalScale) {
			Vector2 skeletonDelta = boneLocalDelta;
			totalScale = GetScaleAffectingRootMotion(out parentBoneScale);
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

		float GetSkeletonSpaceRotationDelta (float boneLocalDelta, Vector2 totalScaleAffectingRootMotion) {
			float rotationSign = totalScaleAffectingRootMotion.x * totalScaleAffectingRootMotion.y > 0 ? 1 : -1;
			return rotationSign * boneLocalDelta;
		}

		void SetEffectiveBoneOffsetsTo (Vector2 displacementSkeletonSpace, float rotationSkeletonSpace, Vector2 parentBoneScale) {

			ApplyTransformConstraints();

			// Move top level bones in opposite direction of the root motion bone
			Skeleton skeleton = skeletonComponent.Skeleton;
			foreach (Bone topLevelBone in topLevelBones) {
				if (topLevelBone == rootMotionBone) {
					if (transformPositionX) topLevelBone.X = displacementSkeletonSpace.x / skeleton.ScaleX;
					if (transformPositionY) topLevelBone.Y = displacementSkeletonSpace.y / skeleton.ScaleY;
					if (transformRotation) {
						float rotationSign = skeleton.ScaleX * skeleton.ScaleY > 0 ? 1 : -1;
						topLevelBone.Rotation = rotationSign * rotationSkeletonSpace;
					}
				} else {
					bool useAppliedTransform = transformConstraintIndices.Count > 0;
					float rootMotionBoneX = useAppliedTransform ? rootMotionBone.AX : rootMotionBone.X;
					float rootMotionBoneY = useAppliedTransform ? rootMotionBone.AY : rootMotionBone.Y;

					float offsetX = (initialOffset.x - rootMotionBoneX) * parentBoneScale.x;
					float offsetY = (initialOffset.y - rootMotionBoneY) * parentBoneScale.y;

					if (transformPositionX) topLevelBone.X = (displacementSkeletonSpace.x / skeleton.ScaleX) + offsetX;
					if (transformPositionY) topLevelBone.Y = (displacementSkeletonSpace.y / skeleton.ScaleY) + offsetY;

					if (transformRotation) {
						float rootMotionBoneRotation = useAppliedTransform ? rootMotionBone.AppliedRotation : rootMotionBone.Rotation;

						float parentBoneRotationSign = (parentBoneScale.x * parentBoneScale.y > 0 ? 1 : -1);
						float offsetRotation = (initialOffsetRotation - rootMotionBoneRotation) * parentBoneRotationSign;

						float skeletonRotationSign = skeleton.ScaleX * skeleton.ScaleY > 0 ? 1 : -1;
						topLevelBone.Rotation = (rotationSkeletonSpace * skeletonRotationSign) + offsetRotation;
					}
				}
			}
		}

		void ClearEffectiveBoneOffsets (Vector2 parentBoneScale) {
			SetEffectiveBoneOffsetsTo(Vector2.zero, 0, parentBoneScale);
		}

		void ClearRigidbodyTempMovement () {
			rigidbodyDisplacement = Vector2.zero;
			tempSkeletonDisplacement = Vector2.zero;
			rigidbodyLocalRotation = Quaternion.identity;
			rigidbody2DRotation = 0;
			tempSkeletonRotation = 0;
		}
	}
}
