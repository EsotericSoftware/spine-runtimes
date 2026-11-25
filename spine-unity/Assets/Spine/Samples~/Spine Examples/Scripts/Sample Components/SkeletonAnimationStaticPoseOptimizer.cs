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

using UnityEngine;

namespace Spine.Unity.Examples {

	/// <summary>
	/// This component automatically skips animation and mesh updates while the SkeletonAnimation's AnimationState
	/// retains a static pose. Modifies <see cref="ISkeletonAnimation.UpdateMode">skeletonAnimation.UpdateMode</see>.
	/// Compatible with threaded animation and mesh updates.
	/// <para>
	/// Call <see cref="RequireUpdate"/> before changing bones, slots, skins, attachments or TrackEntry properties
	/// directly.
	/// Optimization remains disabled while an active physics constraint has an applied mix greater than zero.
	/// Do not use this component with other components which modify the skeleton every frame, and do not combine it
	/// with other components which control <see cref="ISkeletonAnimation.UpdateMode"/>.
	/// </para></summary>
	[RequireComponent(typeof(SkeletonAnimation))]
	[DisallowMultipleComponent]
	public class SkeletonAnimationStaticPoseOptimizer : MonoBehaviour {
		[Tooltip("Update mode used while all tracks retain a static pose." +
			"Set to OnlyAnimationStatus or OnlyEventTimelines to allow " +
			"later removal due to TrackEnd to continue without updating the skeleton or mesh.")]
		[SerializeField] UpdateMode updateModeWhenStatic = UpdateMode.OnlyAnimationStatus;

		SkeletonAnimation skeletonAnimation;
		AnimationState animationState;
		UpdateMode savedUpdateMode;
		bool isOptimized;
		bool requiresFullUpdate;

		void Awake () {
			skeletonAnimation = GetComponent<SkeletonAnimation>();
		}

		void OnEnable () {
			requiresFullUpdate = true;
			skeletonAnimation.DeltaTimeOverride += UpdateOptimizationState;
			skeletonAnimation.OnAnimationRebuild += HandleAnimationRebuild;
			SubscribeToAnimationState();
		}

		void OnDisable () {
			StopOptimization();

			if (skeletonAnimation != null) {
				skeletonAnimation.DeltaTimeOverride -= UpdateOptimizationState;
				skeletonAnimation.OnAnimationRebuild -= HandleAnimationRebuild;
			}
			UnsubscribeFromAnimationState();
		}

		/// <summary>
		/// Ensures that one normal full update is performed before optimization.
		/// Call this before modifying the skeleton or current TrackEntry directly.
		/// </summary>
		public void RequireUpdate () {
			requiresFullUpdate = true;
			StopOptimization();
		}

		void UpdateOptimizationState (SkeletonAnimationBase animation, ref float deltaTime) {
			if (isOptimized) {
				if (animation.UpdateMode == updateModeWhenStatic && // otherwise another component changed UpdateMode
					AreAllTracksHoldingPose()) {
					return;
				}
				StopOptimization();
				requiresFullUpdate = true;
			}
			// Another system set a reduced update mode, wait until it restores FullUpdate.
			if (animation.UpdateMode != UpdateMode.FullUpdate) {
				requiresFullUpdate = true;
				return;
			}
			if (requiresFullUpdate) {
				requiresFullUpdate = false;
				return;
			}

			if (!AreAllTracksHoldingPose())
				return;

			StartOptimization(animation);
		}

		bool AreAllTracksHoldingPose () {
			if (skeletonAnimation == null || !skeletonAnimation.IsValid)
				return false;

			ExposedList<PhysicsConstraint> physicsConstraints = skeletonAnimation.Skeleton.PhysicsConstraints;
			for (int i = 0, n = physicsConstraints.Count; i < n; i++) {
				PhysicsConstraint constraint = physicsConstraints.Items[i];
				if (constraint.Active && constraint.AppliedPose.Mix > 0)
					return false;
			}

			ExposedList<TrackEntry> tracks = skeletonAnimation.AnimationState.Tracks;
			for (int i = 0, n = tracks.Count; i < n; i++) {
				TrackEntry entry = tracks.Items[i];
				if (entry == null) continue;

				// Existing empty animation means mixing has not yet finished.
				if (entry.IsEmptyAnimation || entry.Loop) return false;
				if (!entry.IsComplete || !entry.WasApplied || entry.Delay > 0) return false;
				if (entry.Next != null || entry.MixingFrom != null || entry.MixingTo != null) return false;
			}
			return true;
		}

		void StartOptimization (SkeletonAnimationBase animation) {
			savedUpdateMode = animation.UpdateMode;
			isOptimized = true;
			animation.UpdateMode = updateModeWhenStatic;
		}

		void StopOptimization () {
			if (!isOptimized)
				return;

			if (skeletonAnimation != null && skeletonAnimation.UpdateMode == updateModeWhenStatic)
				skeletonAnimation.UpdateMode = savedUpdateMode;

			isOptimized = false;
		}

		void HandleAnimationStateChange (TrackEntry trackEntry) {
			if (isOptimized)
				RequireUpdate();
		}

		void HandleAnimationRebuild (ISkeletonAnimation animation) {
			RequireUpdate();
			SubscribeToAnimationState();
		}

		void SubscribeToAnimationState () {
			AnimationState animationState = skeletonAnimation.AnimationState;
			if (this.animationState == animationState)
				return;

			UnsubscribeFromAnimationState();
			this.animationState = animationState;
			if (this.animationState != null) {
				this.animationState.Start -= HandleAnimationStateChange;
				this.animationState.Dispose -= HandleAnimationStateChange;
				this.animationState.Start += HandleAnimationStateChange;
				this.animationState.Dispose += HandleAnimationStateChange;
			}
		}

		void UnsubscribeFromAnimationState () {
			if (animationState != null) {
				animationState.Start -= HandleAnimationStateChange;
				animationState.Dispose -= HandleAnimationStateChange;
			}
			animationState = null;
		}
	}
}
