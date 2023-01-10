/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity {

	/// <summary>
	/// Add this component to a SkeletonAnimation or SkeletonGraphic GameObject
	/// to turn motion of a selected root bone into Transform or RigidBody motion.
	/// Local bone translation movement is used as motion.
	/// All top-level bones of the skeleton are moved to compensate the root
	/// motion bone location, keeping the distance relationship between bones intact.
	/// </summary>
	/// <remarks>
	/// Only compatible with SkeletonAnimation (or other components that implement
	/// ISkeletonComponent, ISkeletonAnimation and IAnimationStateComponent).
	/// For <c>SkeletonMecanim</c> please use
	/// <see cref="SkeletonMecanimRootMotion">SkeletonMecanimRootMotion</see> instead.
	/// </remarks>
	[HelpURL("http://esotericsoftware.com/spine-unity#SkeletonRootMotion")]
	public class SkeletonRootMotion : SkeletonRootMotionBase {
		#region Inspector
		const int DefaultAnimationTrackFlags = -1;
		public int animationTrackFlags = DefaultAnimationTrackFlags;
		#endregion

		AnimationState animationState;
		Canvas canvas;

		public override Vector2 GetRemainingRootMotion (int trackIndex) {
			TrackEntry track = animationState.GetCurrent(trackIndex);
			if (track == null)
				return Vector2.zero;

			Animation animation = track.Animation;
			float start = track.AnimationTime;
			float end = animation.Duration;
			return GetAnimationRootMotion(start, end, animation);
		}

		public override RootMotionInfo GetRootMotionInfo (int trackIndex) {
			TrackEntry track = animationState.GetCurrent(trackIndex);
			if (track == null)
				return new RootMotionInfo();

			Animation animation = track.Animation;
			float time = track.AnimationTime;
			return GetAnimationRootMotionInfo(track.Animation, time);
		}

		protected override float AdditionalScale {
			get {
				return canvas ? canvas.referencePixelsPerUnit : 1.0f;
			}
		}

		protected override void Reset () {
			base.Reset();
			animationTrackFlags = DefaultAnimationTrackFlags;
		}

		protected override void Start () {
			base.Start();
			IAnimationStateComponent animstateComponent = skeletonComponent as IAnimationStateComponent;
			this.animationState = (animstateComponent != null) ? animstateComponent.AnimationState : null;

			if (this.GetComponent<CanvasRenderer>() != null) {
				canvas = this.GetComponentInParent<Canvas>();
			}
		}

		protected override Vector2 CalculateAnimationsMovementDelta () {
			Vector2 localDelta = Vector2.zero;
			int trackCount = animationState.Tracks.Count;

			for (int trackIndex = 0; trackIndex < trackCount; ++trackIndex) {
				// note: animationTrackFlags != -1 below covers trackIndex >= 32,
				// with -1 corresponding to entry "everything" of the dropdown list.
				if (animationTrackFlags != -1 && (animationTrackFlags & 1 << trackIndex) == 0)
					continue;

				TrackEntry track = animationState.GetCurrent(trackIndex);
				TrackEntry next = null;
				while (track != null) {
					Animation animation = track.Animation;
					float start = track.AnimationLast;
					float end = track.AnimationTime;
					Vector2 currentDelta = GetAnimationRootMotion(start, end, animation);
					if (currentDelta != Vector2.zero) {
						ApplyMixAlphaToDelta(ref currentDelta, next, track);
						localDelta += currentDelta;
					}

					// Traverse mixingFrom chain.
					next = track;
					track = track.MixingFrom;
				}
			}
			return localDelta;
		}

		protected override float CalculateAnimationsRotationDelta () {
			float localDelta = 0;
			int trackCount = animationState.Tracks.Count;

			for (int trackIndex = 0; trackIndex < trackCount; ++trackIndex) {
				// note: animationTrackFlags != -1 below covers trackIndex >= 32,
				// with -1 corresponding to entry "everything" of the dropdown list.
				if (animationTrackFlags != -1 && (animationTrackFlags & 1 << trackIndex) == 0)
					continue;

				TrackEntry track = animationState.GetCurrent(trackIndex);
				TrackEntry next = null;
				while (track != null) {
					Animation animation = track.Animation;
					float start = track.AnimationLast;
					float end = track.AnimationTime;
					float currentDelta = GetAnimationRootMotionRotation(start, end, animation);
					if (currentDelta != 0) {
						ApplyMixAlphaToDelta(ref currentDelta, next, track);
						localDelta += currentDelta;
					}

					// Traverse mixingFrom chain.
					next = track;
					track = track.MixingFrom;
				}
			}
			return localDelta;
		}

		void ApplyMixAlphaToDelta (ref Vector2 currentDelta, TrackEntry next, TrackEntry track) {
			float mixAlpha = 1;
			GetMixAlpha(ref mixAlpha, next, track);
			currentDelta *= mixAlpha;
		}

		void ApplyMixAlphaToDelta (ref float currentDelta, TrackEntry next, TrackEntry track) {
			float mixAlpha = 1;
			GetMixAlpha(ref mixAlpha, next, track);
			currentDelta *= mixAlpha;
		}

		void GetMixAlpha (ref float cumulatedMixAlpha, TrackEntry next, TrackEntry track) {
			// code below based on AnimationState.cs
			float mix;
			if (next != null) {
				if (next.MixDuration == 0) { // Single frame mix to undo mixingFrom changes.
					mix = 1;
				} else {
					mix = next.MixTime / next.MixDuration;
					if (mix > 1) mix = 1;
				}
				float mixAndAlpha = track.Alpha * next.InterruptAlpha * (1 - mix);
				cumulatedMixAlpha *= mixAndAlpha;
			} else {
				if (track.MixDuration == 0) {
					mix = 1;
				} else {
					mix = track.Alpha * (track.MixTime / track.MixDuration);
					if (mix > 1) mix = 1;
				}
				cumulatedMixAlpha *= mix;
			}
		}
	}
}
