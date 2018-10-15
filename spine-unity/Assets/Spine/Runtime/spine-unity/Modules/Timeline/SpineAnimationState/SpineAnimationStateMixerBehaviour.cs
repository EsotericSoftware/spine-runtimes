/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

 #define SPINE_EDITMODEPOSE

#if UNITY_2017 || UNITY_2018
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Spine.Unity.Playables {
	public class SpineAnimationStateMixerBehaviour : PlayableBehaviour {

		float[] lastInputWeights;
		public int trackIndex;

		// NOTE: This function is called at runtime and edit time. Keep that in mind when setting the values of properties.
		public override void ProcessFrame (Playable playable, FrameData info, object playerData) {
			var spineComponent = playerData as SkeletonAnimation;
			if (spineComponent == null) return;

			var skeleton = spineComponent.Skeleton;
			var state = spineComponent.AnimationState;

			if (!Application.isPlaying) {
				#if SPINE_EDITMODEPOSE
				PreviewEditModePose(playable, spineComponent);
				#endif
				return;
			}

			int inputCount = playable.GetInputCount();

			// Ensure correct buffer size.
			if (this.lastInputWeights == null || this.lastInputWeights.Length < inputCount) {
				this.lastInputWeights = new float[inputCount];

				for (int i = 0; i < inputCount; i++)
					this.lastInputWeights[i] = default(float);				
			}
			var lastInputWeights = this.lastInputWeights;

			// Check all clips. If a clip that was weight 0 turned into weight 1, call SetAnimation.
			for (int i = 0; i < inputCount; i++) {
				float lastInputWeight = lastInputWeights[i];
				float inputWeight = playable.GetInputWeight(i);
				bool trackStarted = inputWeight > lastInputWeight;
				lastInputWeights[i] = inputWeight;

				if (trackStarted) {
					ScriptPlayable<SpineAnimationStateBehaviour> inputPlayable = (ScriptPlayable<SpineAnimationStateBehaviour>)playable.GetInput(i);
					SpineAnimationStateBehaviour clipData = inputPlayable.GetBehaviour();

					if (clipData.animationReference == null) {
						float mixDuration = clipData.customDuration ? clipData.mixDuration : state.Data.DefaultMix;
						state.SetEmptyAnimation(trackIndex, mixDuration);
					} else {
						if (clipData.animationReference.Animation != null) {
							Spine.TrackEntry trackEntry = state.SetAnimation(trackIndex, clipData.animationReference.Animation, clipData.loop);

							//trackEntry.TrackTime = (float)inputPlayable.GetTime(); // More accurate time-start?
							trackEntry.EventThreshold = clipData.eventThreshold;
							trackEntry.DrawOrderThreshold = clipData.drawOrderThreshold;
							trackEntry.AttachmentThreshold = clipData.attachmentThreshold;

							if (clipData.customDuration)
								trackEntry.MixDuration = clipData.mixDuration;
						}
						//else Debug.LogWarningFormat("Animation named '{0}' not found", clipData.animationName);
					}

					// Ensure that the first frame ends with an updated mesh.
					spineComponent.Update(0);
					spineComponent.LateUpdate();
				}
			}
		}

		#if SPINE_EDITMODEPOSE

		AnimationState dummyAnimationState;

		public void PreviewEditModePose (Playable playable, SkeletonAnimation spineComponent) {
			if (Application.isPlaying) return;
			if (spineComponent == null) return;

			int inputCount = playable.GetInputCount();
			int lastOneWeight = -1;

			for (int i = 0; i < inputCount; i++) {
				float inputWeight = playable.GetInputWeight(i);
				if (inputWeight >= 1) lastOneWeight = i;
			}

			if (lastOneWeight != -1) {
				ScriptPlayable<SpineAnimationStateBehaviour> inputPlayableClip = (ScriptPlayable<SpineAnimationStateBehaviour>)playable.GetInput(lastOneWeight);
				SpineAnimationStateBehaviour clipData = inputPlayableClip.GetBehaviour();

				var skeleton = spineComponent.Skeleton;

				bool skeletonDataMismatch = clipData.animationReference != null && spineComponent.SkeletonDataAsset.GetSkeletonData(true) != clipData.animationReference.SkeletonDataAsset.GetSkeletonData(true);
				if (skeletonDataMismatch) {
					Debug.LogWarningFormat("SpineAnimationStateMixerBehaviour tried to apply an animation for the wrong skeleton. Expected {0}. Was {1}", spineComponent.SkeletonDataAsset, clipData.animationReference.SkeletonDataAsset);
				}

				// Getting the from-animation here because it's required to get the mix information from AnimationStateData.
				Animation fromAnimation = null;
				float fromClipTime = 0;
				bool fromClipLoop = false;
				if (lastOneWeight != 0 && inputCount > 1) {
					var fromClip = (ScriptPlayable<SpineAnimationStateBehaviour>)playable.GetInput(lastOneWeight - 1);
					var fromClipData = fromClip.GetBehaviour();
					fromAnimation = fromClipData.animationReference.Animation;
					fromClipTime = (float)fromClip.GetTime();
					fromClipLoop = fromClipData.loop;
				}

				Animation toAnimation = clipData.animationReference.Animation;
				float toClipTime = (float)inputPlayableClip.GetTime();
				float mixDuration = clipData.mixDuration;

				if (!clipData.customDuration && fromAnimation != null) {
					mixDuration = spineComponent.AnimationState.Data.GetMix(fromAnimation, toAnimation);
				}

				// Approximate what AnimationState might do at runtime.
				if (fromAnimation != null && mixDuration > 0 && toClipTime < mixDuration) {
					dummyAnimationState = dummyAnimationState ?? new AnimationState(spineComponent.skeletonDataAsset.GetAnimationStateData());

					var toTrack = dummyAnimationState.GetCurrent(0);
					var fromTrack = toTrack != null ? toTrack.mixingFrom : null;
					bool isAnimationTransitionMatch = (toTrack != null && toTrack.animation == toAnimation && fromTrack != null && fromTrack.animation == fromAnimation);
					
					if (!isAnimationTransitionMatch) {
						dummyAnimationState.ClearTracks();
						fromTrack = dummyAnimationState.SetAnimation(0, fromAnimation, fromClipLoop);
						fromTrack.AllowImmediateQueue();
						toTrack = dummyAnimationState.SetAnimation(0, toAnimation, clipData.loop);
					}

					// Update track times.
					fromTrack.trackTime = fromClipTime;
					toTrack.trackTime = toClipTime;
					toTrack.mixTime = toClipTime;
					
					// Apply Pose
					skeleton.SetToSetupPose();
					dummyAnimationState.Update(0);
					dummyAnimationState.Apply(skeleton);
				} else {
					skeleton.SetToSetupPose();
					toAnimation.PoseSkeleton(skeleton, toClipTime, clipData.loop);
				}

			}
			// Do nothing outside of the first clip and the last clip.

		}
		#endif

	}

}
#endif
