/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using UnityEngine;
using System.Collections.Generic;

namespace Spine.Unity {
	[RequireComponent(typeof(Animator))]
	public class SkeletonMecanim : SkeletonRenderer, ISkeletonAnimation {

		[SerializeField] protected MecanimTranslator translator;
		public MecanimTranslator Translator { get { return translator; } }

		#region Bone Callbacks (ISkeletonAnimation)
		protected event UpdateBonesDelegate _UpdateLocal;
		protected event UpdateBonesDelegate _UpdateWorld;
		protected event UpdateBonesDelegate _UpdateComplete;

		/// <summary>
		/// Occurs after the animations are applied and before world space values are resolved.
		/// Use this callback when you want to set bone local values.</summary>
		public event UpdateBonesDelegate UpdateLocal { add { _UpdateLocal += value; } remove { _UpdateLocal -= value; } }

		/// <summary>
		/// Occurs after the Skeleton's bone world space values are resolved (including all constraints).
		/// Using this callback will cause the world space values to be solved an extra time.
		/// Use this callback if want to use bone world space values, and also set bone local values.</summary>
		public event UpdateBonesDelegate UpdateWorld { add { _UpdateWorld += value; } remove { _UpdateWorld -= value; } }

		/// <summary>
		/// Occurs after the Skeleton's bone world space values are resolved (including all constraints).
		/// Use this callback if you want to use bone world space values, but don't intend to modify bone local values.
		/// This callback can also be used when setting world position and the bone matrix.</summary>
		public event UpdateBonesDelegate UpdateComplete { add { _UpdateComplete += value; } remove { _UpdateComplete -= value; } }
		#endregion

		public override void Initialize (bool overwrite) {
			if (valid && !overwrite) return;
			base.Initialize(overwrite);
			if (!valid) return;

			if (translator == null) translator = new MecanimTranslator();
			translator.Initialize(GetComponent<Animator>(), this.skeletonDataAsset);
		}

		public void Update () {
			if (!valid) return;

			#if UNITY_EDITOR
			var translatorAnimator = translator.Animator;
			if (translatorAnimator != null && !translatorAnimator.isInitialized)
				translatorAnimator.Rebind();

			if (Application.isPlaying) {
				translator.Apply(skeleton);
			} else {
				if (translatorAnimator != null && translatorAnimator.isInitialized &&
					translatorAnimator.isActiveAndEnabled && translatorAnimator.runtimeAnimatorController != null) {
					// Note: Rebind is required to prevent warning "Animator is not playing an AnimatorController" with prefabs
					translatorAnimator.Rebind();
					translator.Apply(skeleton);
				}
			}
			#else
			translator.Apply(skeleton);
			#endif

			// UpdateWorldTransform and Bone Callbacks
			{
				if (_UpdateLocal != null)
					_UpdateLocal(this);

				skeleton.UpdateWorldTransform();

				if (_UpdateWorld != null) {
					_UpdateWorld(this);
					skeleton.UpdateWorldTransform();
				}

				if (_UpdateComplete != null)
					_UpdateComplete(this);
			}
		}

		[System.Serializable]
		public class MecanimTranslator {
			#region Inspector
			public bool autoReset = true;
			public MixMode[] layerMixModes = new MixMode[0];
			public MixBlend[] layerBlendModes = new MixBlend[0];
			#endregion

			public enum MixMode { AlwaysMix, MixNext, Hard }

			readonly Dictionary<int, Spine.Animation> animationTable = new Dictionary<int, Spine.Animation>(IntEqualityComparer.Instance);
			readonly Dictionary<AnimationClip, int> clipNameHashCodeTable = new Dictionary<AnimationClip, int>(AnimationClipEqualityComparer.Instance);
			readonly List<Animation> previousAnimations = new List<Animation>();

			protected class ClipInfos {
				public bool isInterruptionActive = false;
				public bool isLastFrameOfInterruption = false;

				public int clipInfoCount = 0;
				public int nextClipInfoCount = 0;
				public int interruptingClipInfoCount = 0;
				public readonly List<AnimatorClipInfo> clipInfos = new List<AnimatorClipInfo>();
				public readonly List<AnimatorClipInfo> nextClipInfos = new List<AnimatorClipInfo>();
				public readonly List<AnimatorClipInfo> interruptingClipInfos = new List<AnimatorClipInfo>();

				public AnimatorStateInfo stateInfo;
				public AnimatorStateInfo nextStateInfo;
				public AnimatorStateInfo interruptingStateInfo;

				public float interruptingClipTimeAddition = 0;
			}
			protected ClipInfos[] layerClipInfos = new ClipInfos[0];

			Animator animator;
			public Animator Animator { get { return this.animator; } }

			public void Initialize(Animator animator, SkeletonDataAsset skeletonDataAsset) {
				this.animator = animator;

				previousAnimations.Clear();

				animationTable.Clear();
				var data = skeletonDataAsset.GetSkeletonData(true);
				foreach (var a in data.Animations)
					animationTable.Add(a.Name.GetHashCode(), a);

				clipNameHashCodeTable.Clear();
				ClearClipInfosForLayers();
			}

			public void Apply (Skeleton skeleton) {
				if (layerMixModes.Length < animator.layerCount) {
					System.Array.Resize<MixMode>(ref layerMixModes, animator.layerCount);
					layerMixModes[animator.layerCount-1] = MixMode.MixNext;
				}

			#if UNITY_EDITOR
				if (!Application.isPlaying) {
					GetLayerBlendModes();
				}
			#endif
				InitClipInfosForLayers();
				for (int layer = 0, n = animator.layerCount; layer < n; layer++) {
					GetStateUpdatesFromAnimator(layer);
				}

				//skeleton.Update(Time.deltaTime); // Doesn't actually do anything, currently. (Spine 3.6).

				// Clear Previous
				if (autoReset) {
					var previousAnimations = this.previousAnimations;
					for (int i = 0, n = previousAnimations.Count; i < n; i++)
						previousAnimations[i].SetKeyedItemsToSetupPose(skeleton);

					previousAnimations.Clear();
					for (int layer = 0, n = animator.layerCount; layer < n; layer++) {
						float layerWeight = (layer == 0) ? 1 : animator.GetLayerWeight(layer); // Animator.GetLayerWeight always returns 0 on the first layer. Should be interpreted as 1.
						if (layerWeight <= 0) continue;

						AnimatorStateInfo nextStateInfo = animator.GetNextAnimatorStateInfo(layer);

						bool hasNext = nextStateInfo.fullPathHash != 0;

						int clipInfoCount, nextClipInfoCount, interruptingClipInfoCount;
						IList<AnimatorClipInfo> clipInfo, nextClipInfo, interruptingClipInfo;
						bool isInterruptionActive, shallInterpolateWeightTo1;
						GetAnimatorClipInfos(layer, out isInterruptionActive, out clipInfoCount, out nextClipInfoCount, out interruptingClipInfoCount,
											out clipInfo, out nextClipInfo, out interruptingClipInfo, out shallInterpolateWeightTo1);

						for (int c = 0; c < clipInfoCount; c++) {
							var info = clipInfo[c];
							float weight = info.weight * layerWeight; if (weight == 0) continue;
							previousAnimations.Add(GetAnimation(info.clip));
						}

						if (hasNext) {
							for (int c = 0; c < nextClipInfoCount; c++) {
								var info = nextClipInfo[c];
								float weight = info.weight * layerWeight; if (weight == 0) continue;
								previousAnimations.Add(GetAnimation(info.clip));
							}
						}

						if (isInterruptionActive) {
							for (int c = 0; c < interruptingClipInfoCount; c++)
							{
								var info = interruptingClipInfo[c];
								float clipWeight = shallInterpolateWeightTo1 ? (info.weight + 1.0f) * 0.5f : info.weight;
								float weight = clipWeight * layerWeight; if (weight == 0) continue;
								previousAnimations.Add(GetAnimation(info.clip));
							}
						}
					}
				}

				// Apply
				for (int layer = 0, n = animator.layerCount; layer < n; layer++) {
					float layerWeight = (layer == 0) ? 1 : animator.GetLayerWeight(layer); // Animator.GetLayerWeight always returns 0 on the first layer. Should be interpreted as 1.

					bool isInterruptionActive;
					AnimatorStateInfo stateInfo;
					AnimatorStateInfo nextStateInfo;
					AnimatorStateInfo interruptingStateInfo;
					float interruptingClipTimeAddition;
					GetAnimatorStateInfos(layer, out isInterruptionActive, out stateInfo, out nextStateInfo, out interruptingStateInfo, out interruptingClipTimeAddition);

					bool hasNext = nextStateInfo.fullPathHash != 0;

					int clipInfoCount, nextClipInfoCount, interruptingClipInfoCount;
					IList<AnimatorClipInfo> clipInfo, nextClipInfo, interruptingClipInfo;
					bool shallInterpolateWeightTo1;
					GetAnimatorClipInfos(layer, out isInterruptionActive, out clipInfoCount, out nextClipInfoCount, out interruptingClipInfoCount,
										out clipInfo, out nextClipInfo, out interruptingClipInfo, out shallInterpolateWeightTo1);

					MixMode mode = layerMixModes[layer];
					MixBlend layerBlendMode = (layer < layerBlendModes.Length) ? layerBlendModes[layer] : MixBlend.Replace;
					if (mode == MixMode.AlwaysMix) {
						// Always use Mix instead of Applying the first non-zero weighted clip.
						for (int c = 0; c < clipInfoCount; c++) {
							var info = clipInfo[c]; float weight = info.weight * layerWeight; if (weight == 0) continue;
							GetAnimation(info.clip).Apply(skeleton, 0, AnimationTime(stateInfo.normalizedTime, info.clip.length, stateInfo.loop, stateInfo.speed < 0), stateInfo.loop, null, weight, layerBlendMode, MixDirection.In);
						}
						if (hasNext) {
							for (int c = 0; c < nextClipInfoCount; c++) {
								var info = nextClipInfo[c]; float weight = info.weight * layerWeight; if (weight == 0) continue;
								GetAnimation(info.clip).Apply(skeleton, 0, AnimationTime(nextStateInfo.normalizedTime, info.clip.length, nextStateInfo.speed < 0), nextStateInfo.loop, null, weight, layerBlendMode, MixDirection.In);
							}
						}
						if (isInterruptionActive) {
							for (int c = 0; c < interruptingClipInfoCount; c++)
							{
								var info = interruptingClipInfo[c];
								float clipWeight = shallInterpolateWeightTo1 ? (info.weight + 1.0f) * 0.5f : info.weight;
								float weight = clipWeight * layerWeight; if (weight == 0) continue;
								GetAnimation(info.clip).Apply(skeleton, 0, AnimationTime(interruptingStateInfo.normalizedTime + interruptingClipTimeAddition, info.clip.length, interruptingStateInfo.speed < 0),
																interruptingStateInfo.loop, null, weight, layerBlendMode, MixDirection.In);
							}
						}
					} else { // case MixNext || Hard
						// Apply first non-zero weighted clip
						int c = 0;
						for (; c < clipInfoCount; c++) {
							var info = clipInfo[c]; float weight = info.weight * layerWeight; if (weight == 0) continue;
							GetAnimation(info.clip).Apply(skeleton, 0, AnimationTime(stateInfo.normalizedTime, info.clip.length, stateInfo.loop, stateInfo.speed < 0), stateInfo.loop, null, 1f, layerBlendMode, MixDirection.In);
							break;
						}
						// Mix the rest
						for (; c < clipInfoCount; c++) {
							var info = clipInfo[c]; float weight = info.weight * layerWeight; if (weight == 0) continue;
							GetAnimation(info.clip).Apply(skeleton, 0, AnimationTime(stateInfo.normalizedTime, info.clip.length, stateInfo.loop, stateInfo.speed < 0), stateInfo.loop, null, weight, layerBlendMode, MixDirection.In);
						}

						c = 0;
						if (hasNext) {
							// Apply next clip directly instead of mixing (ie: no crossfade, ignores mecanim transition weights)
							if (mode == MixMode.Hard) {
								for (; c < nextClipInfoCount; c++) {
									var info = nextClipInfo[c]; float weight = info.weight * layerWeight; if (weight == 0) continue;
									GetAnimation(info.clip).Apply(skeleton, 0, AnimationTime(nextStateInfo.normalizedTime, info.clip.length, nextStateInfo.speed < 0), nextStateInfo.loop, null, 1f, layerBlendMode, MixDirection.In);
									break;
								}
							}
							// Mix the rest
							for (; c < nextClipInfoCount; c++) {
								var info = nextClipInfo[c]; float weight = info.weight * layerWeight; if (weight == 0) continue;
								GetAnimation(info.clip).Apply(skeleton, 0, AnimationTime(nextStateInfo.normalizedTime, info.clip.length, nextStateInfo.speed < 0), nextStateInfo.loop, null, weight, layerBlendMode, MixDirection.In);
							}
						}

						c = 0;
						if (isInterruptionActive) {
							// Apply next clip directly instead of mixing (ie: no crossfade, ignores mecanim transition weights)
							if (mode == MixMode.Hard) {
								for (; c < interruptingClipInfoCount; c++) {
									var info = interruptingClipInfo[c];
									float clipWeight = shallInterpolateWeightTo1 ? (info.weight + 1.0f) * 0.5f : info.weight;
									float weight = clipWeight * layerWeight; if (weight == 0) continue;
									GetAnimation(info.clip).Apply(skeleton, 0, AnimationTime(interruptingStateInfo.normalizedTime + interruptingClipTimeAddition, info.clip.length, interruptingStateInfo.speed < 0), interruptingStateInfo.loop, null, 1f, layerBlendMode, MixDirection.In);
									break;
								}
							}
							// Mix the rest
							for (; c < interruptingClipInfoCount; c++) {
								var info = interruptingClipInfo[c];
								float clipWeight = shallInterpolateWeightTo1 ? (info.weight + 1.0f) * 0.5f : info.weight;
								float weight = clipWeight * layerWeight; if (weight == 0) continue;
								GetAnimation(info.clip).Apply(skeleton, 0, AnimationTime(interruptingStateInfo.normalizedTime + interruptingClipTimeAddition, info.clip.length, interruptingStateInfo.speed < 0), interruptingStateInfo.loop, null, weight, layerBlendMode, MixDirection.In);
							}
						}
					}
				}
			}

			static float AnimationTime (float normalizedTime, float clipLength, bool loop, bool reversed) {
				if (reversed)
					normalizedTime = (1 - normalizedTime + (int)normalizedTime) + (int)normalizedTime;
				float time = normalizedTime * clipLength;
				if (loop) return time;
				const float EndSnapEpsilon = 1f / 30f; // Workaround for end-duration keys not being applied.
				return (clipLength - time < EndSnapEpsilon) ? clipLength : time; // return a time snapped to clipLength;
			}

			static float AnimationTime (float normalizedTime, float clipLength, bool reversed) {
				if (reversed)
					normalizedTime = (1 - normalizedTime + (int)normalizedTime) + (int)normalizedTime;

				return normalizedTime * clipLength;
			}

			void InitClipInfosForLayers () {
				if (layerClipInfos.Length < animator.layerCount) {
					System.Array.Resize<ClipInfos>(ref layerClipInfos, animator.layerCount);
					for (int layer = 0, n = animator.layerCount; layer < n; ++layer) {
						if (layerClipInfos[layer] == null)
							layerClipInfos[layer] = new ClipInfos();
					}
				}
			}

			void ClearClipInfosForLayers () {
				for (int layer = 0, n = layerClipInfos.Length; layer < n; ++layer) {
					if (layerClipInfos[layer] == null)
						layerClipInfos[layer] = new ClipInfos();
					else {
						layerClipInfos[layer].isInterruptionActive = false;
						layerClipInfos[layer].isLastFrameOfInterruption = false;
						layerClipInfos[layer].clipInfos.Clear();
						layerClipInfos[layer].nextClipInfos.Clear();
						layerClipInfos[layer].interruptingClipInfos.Clear();
					}
				}
			}

		#if UNITY_EDITOR
			void GetLayerBlendModes() {
				if (layerBlendModes.Length < animator.layerCount) {
					System.Array.Resize<MixBlend>(ref layerBlendModes, animator.layerCount);
				}
				for (int layer = 0, n = animator.layerCount; layer < n; ++layer) {
					var controller = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
					if (controller != null) {
						layerBlendModes[layer] = MixBlend.First;
						if (layer > 0) {
							layerBlendModes[layer] = controller.layers[layer].blendingMode == UnityEditor.Animations.AnimatorLayerBlendingMode.Additive ?
								MixBlend.Add : MixBlend.Replace;
						}
					}
				}
			}
		#endif

			void GetStateUpdatesFromAnimator (int layer) {

				var layerInfos = layerClipInfos[layer];
				int clipInfoCount = animator.GetCurrentAnimatorClipInfoCount(layer);
				int nextClipInfoCount = animator.GetNextAnimatorClipInfoCount(layer);

				var clipInfos = layerInfos.clipInfos;
				var nextClipInfos = layerInfos.nextClipInfos;
				var interruptingClipInfos = layerInfos.interruptingClipInfos;

				layerInfos.isInterruptionActive = (clipInfoCount == 0 && clipInfos.Count != 0 &&
													nextClipInfoCount == 0 && nextClipInfos.Count != 0);

				// Note: during interruption, GetCurrentAnimatorClipInfoCount and GetNextAnimatorClipInfoCount
				// are returning 0 in calls above. Therefore we keep previous clipInfos and nextClipInfos
				// until the interruption is over.
				if (layerInfos.isInterruptionActive) {

					// Note: The last frame of a transition interruption
					// will have fullPathHash set to 0, therefore we have to use previous
					// frame's infos about interruption clips and correct some values
					// accordingly (normalizedTime and weight).
					var interruptingStateInfo = animator.GetNextAnimatorStateInfo(layer);
					layerInfos.isLastFrameOfInterruption = interruptingStateInfo.fullPathHash == 0;
					if (!layerInfos.isLastFrameOfInterruption) {
						layerInfos.interruptingClipInfoCount = interruptingClipInfos.Count;

						animator.GetNextAnimatorClipInfo(layer, interruptingClipInfos);
						float oldTime = layerInfos.interruptingStateInfo.normalizedTime;
						float newTime = interruptingStateInfo.normalizedTime;
						layerInfos.interruptingClipTimeAddition = newTime - oldTime;
						layerInfos.interruptingStateInfo = interruptingStateInfo;
					}
				} else {
					layerInfos.clipInfoCount = clipInfoCount;
					layerInfos.nextClipInfoCount = nextClipInfoCount;
					layerInfos.interruptingClipInfoCount = 0;
					layerInfos.isLastFrameOfInterruption = false;

					if (clipInfos.Capacity < clipInfoCount) clipInfos.Capacity = clipInfoCount;
					if (nextClipInfos.Capacity < nextClipInfoCount) nextClipInfos.Capacity = nextClipInfoCount;

					animator.GetCurrentAnimatorClipInfo(layer, clipInfos);
					animator.GetNextAnimatorClipInfo(layer, nextClipInfos);

					layerInfos.stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
					layerInfos.nextStateInfo = animator.GetNextAnimatorStateInfo(layer);
				}
			}

			void GetAnimatorClipInfos (
				int layer,
				out bool isInterruptionActive,
				out int clipInfoCount,
				out int nextClipInfoCount,
				out int interruptingClipInfoCount,
				out IList<AnimatorClipInfo> clipInfo,
				out IList<AnimatorClipInfo> nextClipInfo,
				out IList<AnimatorClipInfo> interruptingClipInfo,
				out bool shallInterpolateWeightTo1) {

				var layerInfos = layerClipInfos[layer];
				isInterruptionActive = layerInfos.isInterruptionActive;

				clipInfoCount = layerInfos.clipInfoCount;
				nextClipInfoCount = layerInfos.nextClipInfoCount;
				interruptingClipInfoCount = layerInfos.interruptingClipInfoCount;

				clipInfo = layerInfos.clipInfos;
				nextClipInfo = layerInfos.nextClipInfos;
				interruptingClipInfo = isInterruptionActive ? layerInfos.interruptingClipInfos : null;
				shallInterpolateWeightTo1 = layerInfos.isLastFrameOfInterruption;
			}

			void GetAnimatorStateInfos (
				int layer,
				out bool isInterruptionActive,
				out AnimatorStateInfo stateInfo,
				out AnimatorStateInfo nextStateInfo,
				out AnimatorStateInfo interruptingStateInfo,
				out float interruptingClipTimeAddition) {

				var layerInfos = layerClipInfos[layer];
				isInterruptionActive = layerInfos.isInterruptionActive;

				stateInfo = layerInfos.stateInfo;
				nextStateInfo = layerInfos.nextStateInfo;
				interruptingStateInfo = layerInfos.interruptingStateInfo;
				interruptingClipTimeAddition = layerInfos.isLastFrameOfInterruption ? layerInfos.interruptingClipTimeAddition : 0;
			}

			Spine.Animation GetAnimation (AnimationClip clip) {
				int clipNameHashCode;
				if (!clipNameHashCodeTable.TryGetValue(clip, out clipNameHashCode)) {
					clipNameHashCode = clip.name.GetHashCode();
					clipNameHashCodeTable.Add(clip, clipNameHashCode);
				}
				Spine.Animation animation;
				animationTable.TryGetValue(clipNameHashCode, out animation);
				return animation;
			}

			class AnimationClipEqualityComparer : IEqualityComparer<AnimationClip> {
				internal static readonly IEqualityComparer<AnimationClip> Instance = new AnimationClipEqualityComparer();
				public bool Equals (AnimationClip x, AnimationClip y) { return x.GetInstanceID() == y.GetInstanceID(); }
				public int GetHashCode (AnimationClip o) { return o.GetInstanceID(); }
			}

			class IntEqualityComparer : IEqualityComparer<int> {
				internal static readonly IEqualityComparer<int> Instance = new IntEqualityComparer();
				public bool Equals (int x, int y) { return x == y; }
				public int GetHashCode(int o) { return o; }
			}
		}

	}
}
