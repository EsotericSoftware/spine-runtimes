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

#define SPINE_EDITMODEPOSE

using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Spine.Unity.Playables {
	public class SpineAnimationStateMixerBehaviour : PlayableBehaviour {

		float[] lastInputWeights;
		bool lastAnyClipPlaying = false;
		public int trackIndex;
		public bool unscaledTime;
		ScriptPlayable<SpineAnimationStateBehaviour>[] startingClips
			= new ScriptPlayable<SpineAnimationStateBehaviour>[2];

		IAnimationStateComponent animationStateComponent;
		bool pauseWithDirector = true;
		bool endAtClipEnd = true;
		float endMixOutDuration = 0.1f;
		bool isPaused = false;
		TrackEntry pausedTrackEntry;
		float previousTimeScale = 1;

		TrackEntry timelineStartedTrackEntry;

		public override void OnBehaviourPause (Playable playable, FrameData info) {
			if (pauseWithDirector) {
				if (!isPaused)
					HandlePause(playable);
				isPaused = true;
			}
		}

		public override void OnGraphStop (Playable playable) {
			if (!isPaused && endAtClipEnd)
				HandleClipEnd();
		}

		public override void OnBehaviourPlay (Playable playable, FrameData info) {
			if (isPaused)
				HandleResume(playable);
			isPaused = false;
		}

		protected void HandlePause (Playable playable) {
			if (animationStateComponent.IsNullOrDestroyed()) return;

			TrackEntry current = animationStateComponent.AnimationState.GetCurrent(trackIndex);
			if (current != null && current == timelineStartedTrackEntry) {
				previousTimeScale = current.TimeScale;
				current.TimeScale = 0;
				pausedTrackEntry = current;
			}
		}

		protected void HandleResume (Playable playable) {
			if (animationStateComponent.IsNullOrDestroyed()) return;

			TrackEntry current = animationStateComponent.AnimationState.GetCurrent(trackIndex);
			if (current != null && current == pausedTrackEntry) {
				current.TimeScale = previousTimeScale;
			}
		}

		protected void HandleClipEnd () {
			if (animationStateComponent.IsNullOrDestroyed()) return;

			var state = animationStateComponent.AnimationState;
			if (endAtClipEnd &&
				timelineStartedTrackEntry != null &&
				timelineStartedTrackEntry == state.GetCurrent(trackIndex)) {

				if (endMixOutDuration >= 0)
					state.SetEmptyAnimation(trackIndex, endMixOutDuration);
				else // pause if endMixOutDuration < 0
					timelineStartedTrackEntry.TimeScale = 0;
				timelineStartedTrackEntry = null;
			}
		}

		// NOTE: This function is called at runtime and edit time. Keep that in mind when setting the values of properties.
		public override void ProcessFrame (Playable playable, FrameData info, object playerData) {
			var skeletonAnimation = playerData as SkeletonAnimation;
			var skeletonGraphic = playerData as SkeletonGraphic;
			animationStateComponent = playerData as IAnimationStateComponent;
			var skeletonComponent = playerData as ISkeletonComponent;
			if (animationStateComponent.IsNullOrDestroyed() || skeletonComponent == null) return;

			var skeleton = skeletonComponent.Skeleton;
			var state = animationStateComponent.AnimationState;

			if (!Application.isPlaying) {
#if SPINE_EDITMODEPOSE
				PreviewEditModePose(playable, skeletonComponent, animationStateComponent,
					skeletonAnimation, skeletonGraphic);
#endif
				return;
			}

			int inputCount = playable.GetInputCount();
			float rootSpeed = GetRootPlayableSpeed(playable);

			// Ensure correct buffer size.
			if (this.lastInputWeights == null || this.lastInputWeights.Length < inputCount) {
				this.lastInputWeights = new float[inputCount];

				for (int i = 0; i < inputCount; i++)
					this.lastInputWeights[i] = default(float);
			}
			var lastInputWeights = this.lastInputWeights;
			int numStartingClips = 0;
			bool anyClipPlaying = false;

			// Check all clips. If a clip that was weight 0 turned into weight 1, call SetAnimation.
			for (int i = 0; i < inputCount; i++) {
				float lastInputWeight = lastInputWeights[i];
				float inputWeight = playable.GetInputWeight(i);
				bool clipStarted = lastInputWeight == 0 && inputWeight > 0;
				if (inputWeight > 0)
					anyClipPlaying = true;
				lastInputWeights[i] = inputWeight;

				if (clipStarted && numStartingClips < 2) {
					ScriptPlayable<SpineAnimationStateBehaviour> clipPlayable = (ScriptPlayable<SpineAnimationStateBehaviour>)playable.GetInput(i);
					startingClips[numStartingClips++] = clipPlayable;
				}
			}
			// unfortunately order of clips can be wrong when two start at the same time, we have to sort clips
			if (numStartingClips == 2) {
				ScriptPlayable<SpineAnimationStateBehaviour> clipPlayable0 = startingClips[0];
				ScriptPlayable<SpineAnimationStateBehaviour> clipPlayable1 = startingClips[1];
				if (clipPlayable0.GetDuration() > clipPlayable1.GetDuration()) { // swap, clip 0 ends after clip 1
					startingClips[0] = clipPlayable1;
					startingClips[1] = clipPlayable0;
				}
			}

			for (int j = 0; j < numStartingClips; ++j) {
				ScriptPlayable<SpineAnimationStateBehaviour> clipPlayable = startingClips[j];
				SpineAnimationStateBehaviour clipData = clipPlayable.GetBehaviour();
				pauseWithDirector = !clipData.dontPauseWithDirector;
				endAtClipEnd = !clipData.dontEndWithClip;
				endMixOutDuration = clipData.endMixOutDuration;

				if (clipData.animationReference == null) {
					float mixDuration = clipData.customDuration ? GetCustomMixDuration(clipData) : state.Data.DefaultMix;
					state.SetEmptyAnimation(trackIndex, mixDuration);
				} else {
					if (clipData.animationReference.Animation != null) {
						animationStateComponent.UnscaledTime = this.unscaledTime;

						TrackEntry currentEntry = state.GetCurrent(trackIndex);
						Spine.TrackEntry trackEntry;
						float customMixDuration = clipData.customDuration ? GetCustomMixDuration(clipData) : 0.0f;
						if (currentEntry == null && customMixDuration > 0) {
							state.SetEmptyAnimation(trackIndex, 0); // ease in requires empty animation
							trackEntry = state.AddAnimation(trackIndex, clipData.animationReference.Animation, clipData.loop, 0);
						} else
							trackEntry = state.SetAnimation(trackIndex, clipData.animationReference.Animation, clipData.loop);

						float clipSpeed = (float)clipPlayable.GetSpeed();
						trackEntry.EventThreshold = clipData.eventThreshold;
						trackEntry.DrawOrderThreshold = clipData.drawOrderThreshold;
						trackEntry.TrackTime = (float)clipPlayable.GetTime() * clipSpeed * rootSpeed;
						trackEntry.TimeScale = clipSpeed * rootSpeed;
						trackEntry.AttachmentThreshold = clipData.attachmentThreshold;
						trackEntry.HoldPrevious = clipData.holdPrevious;
						trackEntry.Alpha = clipData.alpha;

						if (clipData.customDuration)
							trackEntry.MixDuration = customMixDuration / rootSpeed;

						timelineStartedTrackEntry = trackEntry;
					}
					//else Debug.LogWarningFormat("Animation named '{0}' not found", clipData.animationName);
				}

				// Ensure that the first frame ends with an updated mesh.
				if (skeletonAnimation) {
					skeletonAnimation.Update(0);
					skeletonAnimation.LateUpdate();
				} else if (skeletonGraphic) {
					skeletonGraphic.Update(0);
					skeletonGraphic.LateUpdate();
				}
			}
			startingClips[0] = startingClips[1] = ScriptPlayable<SpineAnimationStateBehaviour>.Null;
			if (lastAnyClipPlaying && !anyClipPlaying)
				HandleClipEnd();
			this.lastAnyClipPlaying = anyClipPlaying;
		}

#if SPINE_EDITMODEPOSE

		AnimationState dummyAnimationState;

		public void PreviewEditModePose (Playable playable,
			ISkeletonComponent skeletonComponent, IAnimationStateComponent animationStateComponent,
			SkeletonAnimation skeletonAnimation, SkeletonGraphic skeletonGraphic) {

			if (Application.isPlaying) return;
			if (animationStateComponent.IsNullOrDestroyed() || skeletonComponent == null) return;

			int inputCount = playable.GetInputCount();
			float rootSpeed = GetRootPlayableSpeed(playable);
			int lastNonZeroWeightTrack = -1;

			for (int i = 0; i < inputCount; i++) {
				float inputWeight = playable.GetInputWeight(i);
				if (inputWeight > 0) lastNonZeroWeightTrack = i;
			}

			if (lastNonZeroWeightTrack != -1) {
				ScriptPlayable<SpineAnimationStateBehaviour> inputPlayableClip =
					(ScriptPlayable<SpineAnimationStateBehaviour>)playable.GetInput(lastNonZeroWeightTrack);
				SpineAnimationStateBehaviour clipData = inputPlayableClip.GetBehaviour();

				var skeleton = skeletonComponent.Skeleton;

				bool skeletonDataMismatch = clipData.animationReference != null && clipData.animationReference.SkeletonDataAsset &&
					skeletonComponent.SkeletonDataAsset.GetSkeletonData(true) != clipData.animationReference.SkeletonDataAsset.GetSkeletonData(true);
				if (skeletonDataMismatch) {
					Debug.LogWarningFormat("SpineAnimationStateMixerBehaviour tried to apply an animation for the wrong skeleton. Expected {0}. Was {1}",
						skeletonComponent.SkeletonDataAsset, clipData.animationReference.SkeletonDataAsset);
				}

				// Getting the from-animation here because it's required to get the mix information from AnimationStateData.
				Animation fromAnimation = null;
				float fromClipTime = 0;
				bool fromClipLoop = false;
				if (lastNonZeroWeightTrack != 0 && inputCount > 1) {
					var fromClip = (ScriptPlayable<SpineAnimationStateBehaviour>)playable.GetInput(lastNonZeroWeightTrack - 1);
					var fromClipData = fromClip.GetBehaviour();
					fromAnimation = fromClipData.animationReference != null ? fromClipData.animationReference.Animation : null;
					fromClipTime = (float)fromClip.GetTime() * (float)fromClip.GetSpeed() * rootSpeed;
					fromClipLoop = fromClipData.loop;
				}

				Animation toAnimation = clipData.animationReference != null ? clipData.animationReference.Animation : null;
				float toClipTime = (float)inputPlayableClip.GetTime() * (float)inputPlayableClip.GetSpeed() * rootSpeed;
				float mixDuration = clipData.mixDuration;

				if (!clipData.customDuration && fromAnimation != null && toAnimation != null) {
					mixDuration = animationStateComponent.AnimationState.Data.GetMix(fromAnimation, toAnimation);
				}

				if (trackIndex == 0)
					skeleton.SetToSetupPose();

				// Approximate what AnimationState might do at runtime.
				if (fromAnimation != null && mixDuration > 0 && toClipTime < mixDuration) {
					dummyAnimationState = dummyAnimationState ?? new AnimationState(skeletonComponent.SkeletonDataAsset.GetAnimationStateData());

					var toEntry = dummyAnimationState.GetCurrent(0);
					var fromEntry = toEntry != null ? toEntry.MixingFrom : null;
					bool isAnimationTransitionMatch = (toEntry != null && toEntry.Animation == toAnimation && fromEntry != null && fromEntry.Animation == fromAnimation);

					if (!isAnimationTransitionMatch) {
						dummyAnimationState.ClearTracks();
						fromEntry = dummyAnimationState.SetAnimation(0, fromAnimation, fromClipLoop);
						fromEntry.AllowImmediateQueue();
						if (toAnimation != null) {
							toEntry = dummyAnimationState.SetAnimation(0, toAnimation, clipData.loop);
							toEntry.HoldPrevious = clipData.holdPrevious;
							toEntry.Alpha = clipData.alpha;
						}
					}

					// Update track times.
					fromEntry.TrackTime = fromClipTime;
					if (toEntry != null) {
						toEntry.TrackTime = toClipTime;
						toEntry.MixTime = toClipTime;
					}

					// Apply Pose
					dummyAnimationState.Update(0);
					dummyAnimationState.Apply(skeleton);
				} else {
					if (toAnimation != null)
						toAnimation.Apply(skeleton, 0, toClipTime, clipData.loop, null, clipData.alpha, MixBlend.Setup, MixDirection.In);
				}

				if (skeletonAnimation) {
					skeletonAnimation.Update(0);
					skeletonAnimation.LateUpdate();
				} else if (skeletonGraphic) {
					skeletonGraphic.Update(0);
					skeletonGraphic.LateUpdate();
				}
			}
			// Do nothing outside of the first clip and the last clip.

		}
#endif
		float GetRootPlayableSpeed (Playable playable) {
			PlayableGraph graph = playable.GetGraph();
			int rootPlayableCount = graph.GetRootPlayableCount();
			if (rootPlayableCount == 1)
				return (float)graph.GetRootPlayable(0).GetSpeed();
			else {
				for (int rootIndex = 0; rootIndex < rootPlayableCount; ++rootIndex) {
					var rootPlayable = graph.GetRootPlayable(rootIndex);
					for (int i = 0, n = rootPlayable.GetInputCount(); i < n; ++i) {
						var playableChild = rootPlayable.GetInput(i);
						if (playableChild.Equals(playable)) {
							return (float)rootPlayable.GetSpeed();
						}
					}
				}
			}
			return 1.0f;
		}

		float GetCustomMixDuration (SpineAnimationStateBehaviour clipData) {
			if (clipData.useBlendDuration) {
				TimelineClip clip = clipData.timelineClip;
				return (float)Math.Max(clip.blendInDuration, clip.easeInDuration);
			} else {
				return clipData.mixDuration;
			}
		}
	}

}
