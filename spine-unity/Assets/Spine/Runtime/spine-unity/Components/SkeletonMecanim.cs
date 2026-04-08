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

#if !SPINE_AUTO_UPGRADE_COMPONENTS_OFF
#define AUTO_UPGRADE_TO_43_COMPONENTS
#endif

#if UNITY_2017_1_OR_NEWER
#define BUILT_IN_SPRITE_MASK_COMPONENT
#endif

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

namespace Spine.Unity {

#if NEW_PREFAB_SYSTEM
	[ExecuteAlways]
#else
	[ExecuteInEditMode]
#endif
	[RequireComponent(typeof(Animator))]
	[HelpURL("https://esotericsoftware.com/spine-unity-main-components#SkeletonMecanim-Component")]
	public class SkeletonMecanim : SkeletonAnimationBase, IUpgradable {

		public SkeletonMecanim.MecanimTranslator translator;
		public MecanimTranslator Translator { get { return translator; } }

		public UnityEngine.Animator AnimatorComponent {
			get { return this.GetComponent<Animator>(); }
		}

		public override bool IsValid {
			get {
				return skeletonRenderer != null && skeletonRenderer.IsValid && translator != null &&
					translator.Animator && translator.Animator.isInitialized;
			}
		}

		public override void InitializeAnimationComponent () {
			base.InitializeAnimationComponent();
			if (!skeletonRenderer.IsValid)
				return;

			if (translator == null) translator = new MecanimTranslator();
			translator.Initialize(this.AnimatorComponent, skeletonRenderer.SkeletonDataAsset);
		}

		protected override void UpdateAnimationStatus (float deltaTime) {
			// Note: main animation status is updated by Mecanim Animator component

			skeleton.Update(deltaTime);
		}

		public override void MainThreadBeforeUpdateInternal () {
			base.MainThreadBeforeUpdateInternal();
			translator.GatherAnimatorState();
		}

		protected override void ApplyStateToSkeleton (bool calledFromMainThread) {
#if UNITY_EDITOR
			if (calledFromMainThread)
				EditorRebindAnimator();

			if (ApplicationIsPlaying || !calledFromMainThread) {
				translator.Apply(skeletonRenderer.Skeleton);
			} else {
				Animator translatorAnimator = translator.Animator;
				if (translatorAnimator != null && translatorAnimator.isInitialized &&
					translatorAnimator.isActiveAndEnabled && translatorAnimator.runtimeAnimatorController != null) {
					// Note: Rebind is required to prevent warning "Animator is not playing an AnimatorController" with prefabs
					translatorAnimator.Rebind();
					translator.Apply(skeletonRenderer.Skeleton);
				}
			}
#else
			translator.Apply(skeletonRenderer.Skeleton);
#endif
		}

#if UNITY_EDITOR
		private void EditorRebindAnimator () {
			Animator translatorAnimator = translator.Animator;
			if (translatorAnimator != null && !translatorAnimator.isInitialized)
				translatorAnimator.Rebind();
		}
#endif

		#region Transfer of Deprecated Fields
#if UNITY_EDITOR && AUTO_UPGRADE_TO_43_COMPONENTS
		// compatibility layer between 4.1 and 4.2, automatically transfer serialized attributes.
		public override void UpgradeTo43 () {
			if (!Application.isPlaying && !wasDeprecatedTransferred) {
				UpgradeTo43Components();
				TransferDeprecatedFields();
				InitializeAnimationComponent();
			}
		}

		protected void UpgradeTo43Components () {
			if (gameObject.GetComponent<SkeletonRenderer>() == null &&
				gameObject.GetComponent<SkeletonGraphic>() == null) {
				gameObject.AddComponent<SkeletonRenderer>();
				Debug.Log(string.Format("{0}: Auto-migrated old SkeletonMecanim component to split SkeletonMecanim + SkeletonRenderer components.",
					gameObject.name), gameObject);
				EditorBridge.RequestMarkDirty(gameObject);
			}
		}

		/// <summary>Transfer of former base class SkeletonRenderer parameters.</summary>
		protected void TransferDeprecatedFields () {
			wasDeprecatedTransferred = true;

			SkeletonRenderer skeletonRenderer = gameObject.GetComponent<SkeletonRenderer>();
			if (skeletonRenderer == null)
				return;

			skeletonRenderer.skeletonDataAsset = this.skeletonDataAssetDeprecated;
			skeletonRenderer.initialSkinName = this.initialSkinNameDeprecated;
			skeletonRenderer.EditorSkipSkinSync = this.editorSkipSkinSyncDeprecated;
			skeletonRenderer.initialFlipX = this.initialFlipXDeprecated;
			skeletonRenderer.initialFlipY = this.initialFlipYDeprecated;
			skeletonRenderer.UpdateMode = this.updateModeDeprecated;
			skeletonRenderer.updateWhenInvisible = this.updateWhenInvisibleDeprecated;
			skeletonRenderer.separatorSlotNames = this.separatorSlotNamesDeprecated;

			skeletonRenderer.MeshSettings.zSpacing = this.zSpacingDeprecated;
			skeletonRenderer.MeshSettings.useClipping = this.useClippingDeprecated;
			skeletonRenderer.MeshSettings.immutableTriangles = this.immutableTrianglesDeprecated;
			skeletonRenderer.MeshSettings.pmaVertexColors = this.pmaVertexColorsDeprecated;
			skeletonRenderer.MeshSettings.tintBlack = this.tintBlackDeprecated;
			skeletonRenderer.MeshSettings.addNormals = this.addNormalsDeprecated;
			skeletonRenderer.MeshSettings.calculateTangents = this.calculateTangentsDeprecated;

			skeletonRenderer.clearStateOnDisable = this.clearStateOnDisableDeprecated;
			skeletonRenderer.singleSubmesh = this.singleSubmeshDeprecated;
			skeletonRenderer.MaskInteraction = this.maskInteractionDeprecated;

			translator.TransferDeprecatedFields();
		}

		[SerializeField] protected bool wasDeprecatedTransferred = false;
		// SkeletonRenderer former base class parameters
		[FormerlySerializedAs("skeletonDataAsset")] [SerializeField] private SkeletonDataAsset skeletonDataAssetDeprecated;

		[FormerlySerializedAs("initialSkinName")] [SpineSkin(defaultAsEmptyString: true)] [SerializeField] private string initialSkinNameDeprecated;
		[FormerlySerializedAs("editorSkipSkinSync")] [SerializeField] private bool editorSkipSkinSyncDeprecated = false;
		[FormerlySerializedAs("initialFlipX")] [SerializeField] private bool initialFlipXDeprecated = false;
		[FormerlySerializedAs("initialFlipY")] [SerializeField] private bool initialFlipYDeprecated = false;
		[FormerlySerializedAs("updateMode")] [SerializeField] private UpdateMode updateModeDeprecated = UpdateMode.FullUpdate;
		[FormerlySerializedAs("updateWhenInvisible")] [SerializeField] private UpdateMode updateWhenInvisibleDeprecated = UpdateMode.FullUpdate;
		[UnityEngine.Serialization.FormerlySerializedAs("submeshSeparators"),
			UnityEngine.Serialization.FormerlySerializedAs("separatorSlotNames")]
		[SerializeField] private string[] separatorSlotNamesDeprecated = new string[0];

		[FormerlySerializedAs("zSpacing")] [SerializeField] private float zSpacingDeprecated = 0f;
		[FormerlySerializedAs("useClipping")] [SerializeField] private bool useClippingDeprecated = true;
		[FormerlySerializedAs("immutableTriangles")] [SerializeField] private bool immutableTrianglesDeprecated = false;
		[FormerlySerializedAs("pmaVertexColors")] [SerializeField] private bool pmaVertexColorsDeprecated = true;
		[FormerlySerializedAs("clearStateOnDisable")] [SerializeField] private bool clearStateOnDisableDeprecated = false;
		[FormerlySerializedAs("tintBlack")] [SerializeField] private bool tintBlackDeprecated = false;
		[FormerlySerializedAs("singleSubmesh")] [SerializeField] private bool singleSubmeshDeprecated = false;
		[FormerlySerializedAs("calculateNormals"),
			FormerlySerializedAs("addNormals")]
		[SerializeField] private bool addNormalsDeprecated = false;
		[FormerlySerializedAs("calculateTangents")] [SerializeField] private bool calculateTangentsDeprecated = false;

#if BUILT_IN_SPRITE_MASK_COMPONENT
		[FormerlySerializedAs("maskInteraction")] [SerializeField] private SpriteMaskInteraction maskInteractionDeprecated = SpriteMaskInteraction.None;
#endif // BUILT_IN_SPRITE_MASK_COMPONENT
#endif // UNITY_EDITOR && AUTO_UPGRADE_TO_43_COMPONENTS
		#endregion

		[System.Serializable]
		public class MecanimTranslator {

			const float WeightEpsilon = 0.0001f;

			#region Inspector
			public bool autoReset = true;
			public bool useCustomMixMode = true;
			public MixMode[] layerMixModes = new MixMode[0];
			public bool[] layerIsAdditive = new bool[0];

			// Migration from removed MixBlend
			[FormerlySerializedAs("layerBlendModes")] [SerializeField] private int[] layerBlendModesDeprecated = new int[0];
			#endregion

			public delegate void OnClipAppliedDelegate (Spine.Animation clip, int layerIndex, float weight,
				float time, float lastTime, bool playsBackward);
			protected event OnClipAppliedDelegate _OnClipApplied;

			public event OnClipAppliedDelegate OnClipApplied { add { _OnClipApplied += value; } remove { _OnClipApplied -= value; } }

			public enum MixMode { AlwaysMix, MixNext, Hard, Match }

			readonly Dictionary<int, Spine.Animation> animationTable = new Dictionary<int, Spine.Animation>(IntEqualityComparer.Instance);
			readonly Dictionary<AnimationClip, int> clipNameHashCodeTable = new Dictionary<AnimationClip, int>(AnimationClipEqualityComparer.Instance);
			readonly List<Animation> previousAnimations = new List<Animation>();

			protected struct ClipInfo {
				public Spine.Animation animation;
				public float weight;
				public float length;
				public bool isLooping;
			}

			protected class ClipInfos {
				public bool isInterruptionActive = false;
				public bool isLastFrameOfInterruption = false;

				public int clipInfoCount = 0;
				public int nextClipInfoCount = 0;
				public int interruptingClipInfoCount = 0;
				public readonly List<ClipInfo> clipInfos = new List<ClipInfo>();
				public readonly List<ClipInfo> nextClipInfos = new List<ClipInfo>();
				public readonly List<ClipInfo> interruptingClipInfos = new List<ClipInfo>();
				public float[] clipResolvedWeights = new float[0];
				public float[] nextClipResolvedWeights = new float[0];
				public float[] interruptingClipResolvedWeights = new float[0];

				public AnimatorStateInfo stateInfo;
				public AnimatorStateInfo nextStateInfo;
				public AnimatorStateInfo interruptingStateInfo;

				public float interruptingClipTimeAddition = 0;
				public float layerWeight = 0;
			}
			protected ClipInfos[] layerClipInfos = new ClipInfos[0];
			protected int layerCount = 0;

			Animator animator;
			public Animator Animator { get { return this.animator; } }

			public int MecanimLayerCount {
				get {
					if (!animator)
						return 0;
					if (!animator.isInitialized || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null)
						return 0;
					return animator.layerCount;
				}
			}

			public string[] MecanimLayerNames {
				get {
					if (!animator)
						return new string[0];
					string[] layerNames = new string[animator.layerCount];
					for (int i = 0; i < animator.layerCount; ++i) {
						layerNames[i] = animator.GetLayerName(i);
					}
					return layerNames;
				}
			}

			public void Initialize (Animator animator, SkeletonDataAsset skeletonDataAsset) {
				this.animator = animator;

				previousAnimations.Clear();

				animationTable.Clear();
				SkeletonData data = skeletonDataAsset.GetSkeletonData(true);
				foreach (Animation a in data.Animations)
					animationTable.Add(a.Name.GetHashCode(), a);

				clipNameHashCodeTable.Clear();
				ClearClipInfosForLayers();
			}

			private bool ApplyAnimation (Skeleton skeleton, ClipInfo info, AnimatorStateInfo stateInfo,
										int layerIndex, float layerWeight, bool layerIsAdditive,
										bool useCustomClipWeight = false, float customClipWeight = 1.0f) {
				float weight = info.weight * layerWeight;
				if (weight < WeightEpsilon)
					return false;

				if (info.animation == null)
					return false;

				float time = AnimationTime(stateInfo.normalizedTime, info.length,
										info.isLooping, stateInfo.speed < 0);
				weight = useCustomClipWeight ? layerWeight * customClipWeight : weight;
				bool fromSetup = layerIndex == 0;
				info.animation.Apply(skeleton, 0, time, info.isLooping, null,
						weight, fromSetup, layerIsAdditive, false, false);
				if (_OnClipApplied != null)
					OnClipAppliedCallback(info.animation, stateInfo, layerIndex, time, info.isLooping, weight);
				return true;
			}

			private bool ApplyInterruptionAnimation (Skeleton skeleton,
				bool interpolateWeightTo1, ClipInfo info, AnimatorStateInfo stateInfo,
				int layerIndex, float layerWeight, bool layerIsAdditive, float interruptingClipTimeAddition,
				bool useCustomClipWeight = false, float customClipWeight = 1.0f) {

				float clipWeight = interpolateWeightTo1 ? (info.weight + 1.0f) * 0.5f : info.weight;
				float weight = clipWeight * layerWeight;
				if (weight < WeightEpsilon)
					return false;

				if (info.animation == null)
					return false;

				float time = AnimationTime(stateInfo.normalizedTime + interruptingClipTimeAddition,
										info.length, stateInfo.speed < 0);
				weight = useCustomClipWeight ? layerWeight * customClipWeight : weight;
				bool fromSetup = layerIndex == 0;
				info.animation.Apply(skeleton, 0, time, info.isLooping, null,
							weight, fromSetup, layerIsAdditive, false, false);
				if (_OnClipApplied != null) {
					OnClipAppliedCallback(info.animation, stateInfo, layerIndex, time, info.isLooping, weight);
				}
				return true;
			}

			private void OnClipAppliedCallback (Spine.Animation animation, AnimatorStateInfo stateInfo,
				int layerIndex, float time, bool isLooping, float weight) {

				float speedFactor = stateInfo.speedMultiplier * stateInfo.speed;
				float lastTime = time - (Time.deltaTime * speedFactor);
				if (isLooping && animation.Duration != 0) {
					time %= animation.Duration;
					lastTime %= animation.Duration;
				}
				_OnClipApplied(animation, layerIndex, weight, time, lastTime, speedFactor < 0);
			}

			public void GatherAnimatorState () {
				layerCount = animator.layerCount;
				MigrateLayerBlendModes();
#if UNITY_EDITOR
				GetLayerBlendModes();
#endif
				if (layerIsAdditive.Length < layerCount)
					System.Array.Resize<bool>(ref layerIsAdditive, layerCount);
				if (layerMixModes.Length < layerCount) {
					int oldSize = layerMixModes.Length;
					System.Array.Resize<MixMode>(ref layerMixModes, layerCount);
					for (int layer = oldSize; layer < layerCount; ++layer) {
						layerMixModes[layer] = layerIsAdditive[layer] ? MixMode.AlwaysMix : MixMode.MixNext;
					}
				}

				InitClipInfosForLayers();
				for (int layer = 0, n = layerCount; layer < n; layer++) {
					GetStateUpdatesFromAnimator(layer);
				}
			}

			public void Apply (Skeleton skeleton) {
				// Clear Previous
				if (autoReset) {
					List<Animation> previousAnimations = this.previousAnimations;
					for (int i = 0, n = previousAnimations.Count; i < n; i++)
						previousAnimations[i].Apply(skeleton, 0, 0, false, null, 0, true, false, true, false); // SetKeyedItemsToSetupPose

					previousAnimations.Clear();
					for (int layer = 0, n = layerCount; layer < n; layer++) {
						ClipInfos layerInfos = layerClipInfos[layer];
						float layerWeight = layerInfos.layerWeight;
						if (layerWeight <= 0) continue;

						AnimatorStateInfo nextStateInfo = layerInfos.nextStateInfo;

						bool hasNext = nextStateInfo.fullPathHash != 0;

						int clipInfoCount, nextClipInfoCount, interruptingClipInfoCount;
						IList<ClipInfo> clipInfo, nextClipInfo, interruptingClipInfo;
						bool isInterruptionActive, shallInterpolateWeightTo1;
						GetAnimatorClipInfos(layer, out isInterruptionActive, out clipInfoCount, out nextClipInfoCount, out interruptingClipInfoCount,
											out clipInfo, out nextClipInfo, out interruptingClipInfo, out shallInterpolateWeightTo1);

						for (int c = 0; c < clipInfoCount; c++) {
							ClipInfo info = clipInfo[c];
							float weight = info.weight * layerWeight; if (weight < WeightEpsilon) continue;
							if (info.animation != null)
								previousAnimations.Add(info.animation);
						}

						if (hasNext) {
							for (int c = 0; c < nextClipInfoCount; c++) {
								ClipInfo info = nextClipInfo[c];
								float weight = info.weight * layerWeight; if (weight < WeightEpsilon) continue;
								if (info.animation != null)
									previousAnimations.Add(info.animation);
							}
						}

						if (isInterruptionActive) {
							for (int c = 0; c < interruptingClipInfoCount; c++) {
								ClipInfo info = interruptingClipInfo[c];
								float clipWeight = shallInterpolateWeightTo1 ? (info.weight + 1.0f) * 0.5f : info.weight;
								float weight = clipWeight * layerWeight; if (weight < WeightEpsilon) continue;
								if (info.animation != null)
									previousAnimations.Add(info.animation);
							}
						}
					}
				}

				// Apply
				for (int layer = 0, n = layerCount; layer < n; layer++) {
					ClipInfos layerInfos = layerClipInfos[layer];
					float layerWeight = layerInfos.layerWeight;

					bool isInterruptionActive;
					AnimatorStateInfo stateInfo;
					AnimatorStateInfo nextStateInfo;
					AnimatorStateInfo interruptingStateInfo;
					float interruptingClipTimeAddition;
					GetAnimatorStateInfos(layer, out isInterruptionActive, out stateInfo, out nextStateInfo, out interruptingStateInfo, out interruptingClipTimeAddition);

					bool hasNext = nextStateInfo.fullPathHash != 0;

					int clipInfoCount, nextClipInfoCount, interruptingClipInfoCount;
					IList<ClipInfo> clipInfo, nextClipInfo, interruptingClipInfo;
					bool interpolateWeightTo1;
					GetAnimatorClipInfos(layer, out isInterruptionActive, out clipInfoCount, out nextClipInfoCount, out interruptingClipInfoCount,
										out clipInfo, out nextClipInfo, out interruptingClipInfo, out interpolateWeightTo1);

					bool add = layerIsAdditive[layer];
					MixMode mode = GetMixMode(layer, add);
					if (mode == MixMode.AlwaysMix) {
						// Always use Mix instead of Applying the first non-zero weighted clip.
						for (int c = 0; c < clipInfoCount; c++) {
							ApplyAnimation(skeleton, clipInfo[c], stateInfo, layer, layerWeight, add);
						}
						if (hasNext) {
							for (int c = 0; c < nextClipInfoCount; c++) {
								ApplyAnimation(skeleton, nextClipInfo[c], nextStateInfo, layer, layerWeight, add);
							}
						}
						if (isInterruptionActive) {
							for (int c = 0; c < interruptingClipInfoCount; c++) {
								ApplyInterruptionAnimation(skeleton, interpolateWeightTo1,
									interruptingClipInfo[c], interruptingStateInfo,
									layer, layerWeight, add, interruptingClipTimeAddition);
							}
						}
					} else if (mode == MixMode.Match) {
						// Calculate matching Spine lerp(lerp(A, B, w2), C, w3) weights
						// from Unity's absolute weights A*W1 + B*W2 + C*W3.
						MatchWeights(layerClipInfos[layer], hasNext, isInterruptionActive, clipInfoCount,
							nextClipInfoCount, interruptingClipInfoCount, clipInfo, nextClipInfo, interruptingClipInfo);

						float[] customWeights = layerClipInfos[layer].clipResolvedWeights;
						for (int c = 0; c < clipInfoCount; c++) {
							ApplyAnimation(skeleton, clipInfo[c], stateInfo, layer, layerWeight, add, true,
								customWeights[c]);
						}
						if (hasNext) {
							customWeights = layerClipInfos[layer].nextClipResolvedWeights;
							for (int c = 0; c < nextClipInfoCount; c++) {
								ApplyAnimation(skeleton, nextClipInfo[c], nextStateInfo, layer, layerWeight, add,
									true, customWeights[c]);
							}
						}
						if (isInterruptionActive) {
							customWeights = layerClipInfos[layer].interruptingClipResolvedWeights;
							for (int c = 0; c < interruptingClipInfoCount; c++) {
								ApplyInterruptionAnimation(skeleton, interpolateWeightTo1,
									interruptingClipInfo[c], interruptingStateInfo,
									layer, layerWeight, add, interruptingClipTimeAddition,
									true, customWeights[c]);
							}
						}
					} else { // case MixNext || Hard
							 // Apply first non-zero weighted clip
						int c = 0;
						for (; c < clipInfoCount; c++) {
							if (!ApplyAnimation(skeleton, clipInfo[c], stateInfo, layer, layerWeight, add,
								true, 1.0f))
								continue;
							++c; break;
						}
						// Mix the rest
						for (; c < clipInfoCount; c++) {
							ApplyAnimation(skeleton, clipInfo[c], stateInfo, layer, layerWeight, add);
						}

						c = 0;
						if (hasNext) {
							// Apply next clip directly instead of mixing (ie: no crossfade, ignores mecanim transition weights)
							if (mode == MixMode.Hard) {
								for (; c < nextClipInfoCount; c++) {
									if (!ApplyAnimation(skeleton, nextClipInfo[c], nextStateInfo, layer, layerWeight, add,
										true, 1.0f))
										continue;
									++c; break;
								}
							}
							// Mix the rest
							for (; c < nextClipInfoCount; c++) {
								if (!ApplyAnimation(skeleton, nextClipInfo[c], nextStateInfo, layer, layerWeight, add))
									continue;
							}
						}

						c = 0;
						if (isInterruptionActive) {
							// Apply next clip directly instead of mixing (ie: no crossfade, ignores mecanim transition weights)
							if (mode == MixMode.Hard) {
								for (; c < interruptingClipInfoCount; c++) {
									if (ApplyInterruptionAnimation(skeleton, interpolateWeightTo1,
										interruptingClipInfo[c], interruptingStateInfo,
										layer, layerWeight, add, interruptingClipTimeAddition, true, 1.0f)) {

										++c; break;
									}
								}
							}
							// Mix the rest
							for (; c < interruptingClipInfoCount; c++) {
								ApplyInterruptionAnimation(skeleton, interpolateWeightTo1,
									interruptingClipInfo[c], interruptingStateInfo,
									layer, layerWeight, add, interruptingClipTimeAddition);
							}
						}
					}
				}
			}

			/// <summary>
			/// Resolve matching weights from Unity's absolute weights A*w1 + B*w2 + C*w3 to
			/// Spine's lerp(lerp(A, B, x), C, y) weights, in reverse order of clips.
			/// </summary>
			protected void MatchWeights (ClipInfos clipInfos, bool hasNext, bool isInterruptionActive,
				int clipInfoCount, int nextClipInfoCount, int interruptingClipInfoCount,
				IList<ClipInfo> clipInfo, IList<ClipInfo> nextClipInfo, IList<ClipInfo> interruptingClipInfo) {

				if (clipInfos.clipResolvedWeights.Length < clipInfoCount) {
					System.Array.Resize<float>(ref clipInfos.clipResolvedWeights, clipInfoCount);
				}
				if (hasNext && clipInfos.nextClipResolvedWeights.Length < nextClipInfoCount) {
					System.Array.Resize<float>(ref clipInfos.nextClipResolvedWeights, nextClipInfoCount);
				}
				if (isInterruptionActive && clipInfos.interruptingClipResolvedWeights.Length < interruptingClipInfoCount) {
					System.Array.Resize<float>(ref clipInfos.interruptingClipResolvedWeights, interruptingClipInfoCount);
				}

				float inverseWeight = 1.0f;
				if (isInterruptionActive) {
					for (int c = interruptingClipInfoCount - 1; c >= 0; c--) {
						float unityWeight = interruptingClipInfo[c].weight;
						clipInfos.interruptingClipResolvedWeights[c] = interruptingClipInfo[c].weight * inverseWeight;
						inverseWeight /= (1.0f - unityWeight);
					}
				}
				if (hasNext) {
					for (int c = nextClipInfoCount - 1; c >= 0; c--) {
						float unityWeight = nextClipInfo[c].weight;
						clipInfos.nextClipResolvedWeights[c] = nextClipInfo[c].weight * inverseWeight;
						inverseWeight /= (1.0f - unityWeight);
					}
				}
				for (int c = clipInfoCount - 1; c >= 0; c--) {
					float unityWeight = clipInfo[c].weight;
					clipInfos.clipResolvedWeights[c] = (c == 0) ? 1f : clipInfo[c].weight * inverseWeight;
					inverseWeight /= (1.0f - unityWeight);
				}
			}

			public KeyValuePair<Spine.Animation, float> GetActiveAnimationAndTime (int layer) {
				if (layer >= layerClipInfos.Length)
					return new KeyValuePair<Spine.Animation, float>(null, 0);

				ClipInfos layerInfos = layerClipInfos[layer];
				bool isInterruptionActive = layerInfos.isInterruptionActive;
				ClipInfo clipInfo;
				Spine.Animation animation = null;
				AnimatorStateInfo stateInfo;
				if (isInterruptionActive && layerInfos.interruptingClipInfoCount > 0) {
					clipInfo = layerInfos.interruptingClipInfos[0];
					stateInfo = layerInfos.interruptingStateInfo;
				} else if (layerInfos.clipInfoCount > 0) {
					clipInfo = layerInfos.clipInfos[0];
					stateInfo = layerInfos.stateInfo;
				} else {
					return new KeyValuePair<Spine.Animation, float>(null, 0);
				}
				animation = clipInfo.animation;
				float time = AnimationTime(stateInfo.normalizedTime, clipInfo.length,
										clipInfo.isLooping, stateInfo.speed < 0);
				return new KeyValuePair<Animation, float>(animation, time);
			}

			static float AnimationTime (float normalizedTime, float clipLength, bool loop, bool reversed) {
				float time = AnimationTime(normalizedTime, clipLength, reversed);
				if (loop) return time;
				const float EndSnapEpsilon = 1f / 30f; // Workaround for end-duration keys not being applied.
				return (clipLength - time < EndSnapEpsilon) ? clipLength : time; // return a time snapped to clipLength;
			}

			static float AnimationTime (float normalizedTime, float clipLength, bool reversed) {
				if (reversed)
					normalizedTime = (1 - normalizedTime);
				if (normalizedTime < 0.0f)
					normalizedTime = (normalizedTime % 1.0f) + 1.0f;
				return normalizedTime * clipLength;
			}

			void InitClipInfosForLayers () {
				if (layerClipInfos.Length < layerCount) {
					System.Array.Resize<ClipInfos>(ref layerClipInfos, layerCount);
					for (int layer = 0, n = layerCount; layer < n; ++layer) {
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

			private MixMode GetMixMode (int layer, bool layerIsAdditive) {
				if (useCustomMixMode) {
					MixMode mode = layerMixModes[layer];
					// Note: at additive blending it makes no sense to use constant weight 1 at a fadeout anim add1 as
					// with override layers, so we use AlwaysMix instead to use the proper weights.
					// AlwaysMix leads to the expected result = lower_layer + lerp(add1, add2, transition_weight).
					if (layerIsAdditive && mode == MixMode.MixNext) {
						mode = MixMode.AlwaysMix;
						layerMixModes[layer] = mode;
					}
					return mode;
				} else {
					return layerIsAdditive ? MixMode.AlwaysMix : MixMode.MixNext;
				}
			}

#if UNITY_EDITOR
			public void TransferDeprecatedFields () {
				MigrateLayerBlendModes();
			}
#endif
			void MigrateLayerBlendModes () {
				if (layerIsAdditive.Length == 0 && layerBlendModesDeprecated.Length > 0) {
					layerIsAdditive = new bool[layerBlendModesDeprecated.Length];
					for (int i = 0; i < layerBlendModesDeprecated.Length; i++)
						layerIsAdditive[i] = layerBlendModesDeprecated[i] == 3; // MixBlend.Add was 3.
					layerBlendModesDeprecated = new int[0];
				}
			}

#if UNITY_EDITOR
			void GetLayerBlendModes () {
				if (layerIsAdditive.Length < layerCount) {
					System.Array.Resize<bool>(ref layerIsAdditive, layerCount);
				}
				AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;
				for (int layer = 0, n = layerCount; layer < n; ++layer) {
					if (controller != null)
						layerIsAdditive[layer] = controller.layers[layer].blendingMode == AnimatorLayerBlendingMode.Additive;
				}
			}
#endif

			void GetStateUpdatesFromAnimator (int layer) {

				ClipInfos layerInfos = layerClipInfos[layer];

				// Note: Animator.GetLayerWeight always returns 0 on the first layer. Should be interpreted as 1.
				layerInfos.layerWeight = (layer == 0) ? 1 : animator.GetLayerWeight(layer);

				int clipInfoCount = animator.GetCurrentAnimatorClipInfoCount(layer);
				int nextClipInfoCount = animator.GetNextAnimatorClipInfoCount(layer);

				List<ClipInfo> clipInfos = layerInfos.clipInfos;
				List<ClipInfo> nextClipInfos = layerInfos.nextClipInfos;
				List<ClipInfo> interruptingClipInfos = layerInfos.interruptingClipInfos;

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
					AnimatorStateInfo interruptingStateInfo = animator.GetNextAnimatorStateInfo(layer);
					layerInfos.isLastFrameOfInterruption = interruptingStateInfo.fullPathHash == 0;
					if (!layerInfos.isLastFrameOfInterruption) {
						List<AnimatorClipInfo> tempInterruptingClipInfos = new List<AnimatorClipInfo>();
						animator.GetNextAnimatorClipInfo(layer, tempInterruptingClipInfos);

						interruptingClipInfos.Clear();
						for (int i = 0; i < tempInterruptingClipInfos.Count; i++) {
							AnimatorClipInfo animatorInfo = tempInterruptingClipInfos[i];
							ClipInfo info = new ClipInfo();
							info.animation = GetAnimation(animatorInfo.clip);
							info.weight = animatorInfo.weight;
							info.length = animatorInfo.clip.length;
							info.isLooping = animatorInfo.clip.isLooping;
							interruptingClipInfos.Add(info);
						}

						layerInfos.interruptingClipInfoCount = interruptingClipInfos.Count;
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

					// Get current clip infos and extract data
					List<AnimatorClipInfo> tempClipInfos = new List<AnimatorClipInfo>();
					animator.GetCurrentAnimatorClipInfo(layer, tempClipInfos);
					clipInfos.Clear();
					for (int i = 0; i < tempClipInfos.Count; i++) {
						AnimatorClipInfo animatorInfo = tempClipInfos[i];
						ClipInfo info = new ClipInfo();
						info.animation = GetAnimation(animatorInfo.clip);
						info.weight = animatorInfo.weight;
						info.length = animatorInfo.clip.length;
						info.isLooping = animatorInfo.clip.isLooping;
						clipInfos.Add(info);
					}

					// Get next clip infos and extract data
					List<AnimatorClipInfo> tempNextClipInfos = new List<AnimatorClipInfo>();
					animator.GetNextAnimatorClipInfo(layer, tempNextClipInfos);
					nextClipInfos.Clear();
					for (int i = 0; i < tempNextClipInfos.Count; i++) {
						AnimatorClipInfo animatorInfo = tempNextClipInfos[i];
						ClipInfo info = new ClipInfo();
						info.animation = GetAnimation(animatorInfo.clip);
						info.weight = animatorInfo.weight;
						info.length = animatorInfo.clip.length;
						info.isLooping = animatorInfo.clip.isLooping;
						nextClipInfos.Add(info);
					}

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
				out IList<ClipInfo> clipInfo,
				out IList<ClipInfo> nextClipInfo,
				out IList<ClipInfo> interruptingClipInfo,
				out bool shallInterpolateWeightTo1) {

				ClipInfos layerInfos = layerClipInfos[layer];
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

				ClipInfos layerInfos = layerClipInfos[layer];
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
				public int GetHashCode (int o) { return o; }
			}
		}
	}
}
