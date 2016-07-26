/*****************************************************************************
 * SkeletonAnimator created by Mitch Thompson
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/
//#define USE_SPINE_EVENTS // Uncomment this define to use C# events to handle Spine events. (Does not disable Unity AnimationClip Events)

using UnityEngine;
using System.Collections.Generic;

namespace Spine.Unity {
	[RequireComponent(typeof(Animator))]
	public class SkeletonAnimator : SkeletonRenderer, ISkeletonAnimation {

		public enum MixMode { AlwaysMix, MixNext, SpineStyle }
		public MixMode[] layerMixModes = new MixMode[0];

		public event UpdateBonesDelegate UpdateLocal {
			add { _UpdateLocal += value; }
			remove { _UpdateLocal -= value; }
		}

		public event UpdateBonesDelegate UpdateWorld {
			add { _UpdateWorld += value; }
			remove { _UpdateWorld -= value; }
		}

		public event UpdateBonesDelegate UpdateComplete {
			add { _UpdateComplete += value; }
			remove { _UpdateComplete -= value; }
		}

		protected event UpdateBonesDelegate _UpdateLocal;
		protected event UpdateBonesDelegate _UpdateWorld;
		protected event UpdateBonesDelegate _UpdateComplete;

		readonly Dictionary<int, Spine.Animation> animationTable = new Dictionary<int, Spine.Animation>();
		readonly Dictionary<AnimationClip, int> clipNameHashCodeTable = new Dictionary<AnimationClip, int>();
		Animator animator;
		float lastTime;

		#if USE_SPINE_EVENTS
		public delegate void SkeletonAnimatorEventDelegate (Spine.Event firedEvent, float weight);
		public event SkeletonAnimatorEventDelegate AnimationEvent;
		public readonly ExposedList<Spine.Event> events = new ExposedList<Spine.Event>();
		#else
		public readonly ExposedList<Spine.Event> events = null;
		#endif

		public override void Initialize (bool overwrite) {
			if (valid && !overwrite)
				return;

			base.Initialize(overwrite);

			if (!valid)
				return;

			animationTable.Clear();
			clipNameHashCodeTable.Clear();

			var data = skeletonDataAsset.GetSkeletonData(true);

			foreach (var a in data.Animations) {
				animationTable.Add(a.Name.GetHashCode(), a);
			}

			animator = GetComponent<Animator>();

			lastTime = Time.time;
		}

		void Update () {
			if (!valid)
				return;

			if (layerMixModes.Length != animator.layerCount) {
				System.Array.Resize<MixMode>(ref layerMixModes, animator.layerCount);
			}
			float deltaTime = Time.time - lastTime;

			skeleton.Update(Time.deltaTime);

			//apply
			int layerCount = animator.layerCount;

			for (int i = 0; i < layerCount; i++) {

				float layerWeight = animator.GetLayerWeight(i);
				if (i == 0)
					layerWeight = 1;

				var stateInfo = animator.GetCurrentAnimatorStateInfo(i);
				var nextStateInfo = animator.GetNextAnimatorStateInfo(i);

				#if UNITY_5
				var clipInfo = animator.GetCurrentAnimatorClipInfo(i);
				var nextClipInfo = animator.GetNextAnimatorClipInfo(i);
				#else
				var clipInfo = animator.GetCurrentAnimationClipState(i);
				var nextClipInfo = animator.GetNextAnimationClipState(i);
				#endif
				MixMode mode = layerMixModes[i];

				if (mode == MixMode.AlwaysMix) {
					//always use Mix instead of Applying the first non-zero weighted clip
					for (int c = 0; c < clipInfo.Length; c++) {
						var info = clipInfo[c];
						float weight = info.weight * layerWeight;
						if (weight == 0)
							continue;

						float time = stateInfo.normalizedTime * info.clip.length;
						animationTable[GetAnimationClipNameHashCode(info.clip)].Mix(skeleton, Mathf.Max(0, time - deltaTime), time, stateInfo.loop, events, weight);
						#if USE_SPINE_EVENTS
						FireEvents(events, weight, this.AnimationEvent);
						#endif
					}
					#if UNITY_5
					if (nextStateInfo.fullPathHash != 0) {
					#else
					if (nextStateInfo.nameHash != 0) {
					#endif
						for (int c = 0; c < nextClipInfo.Length; c++) {
							var info = nextClipInfo[c];
							float weight = info.weight * layerWeight;
							if (weight == 0)
								continue;

							float time = nextStateInfo.normalizedTime * info.clip.length;
							animationTable[GetAnimationClipNameHashCode(info.clip)].Mix(skeleton, Mathf.Max(0, time - deltaTime), time, nextStateInfo.loop, events, weight);
							#if USE_SPINE_EVENTS
							FireEvents(events, weight, this.AnimationEvent);
							#endif
						}
					}
				} else if (mode >= MixMode.MixNext) {
					//apply first non-zero weighted clip
					int c = 0;

					for (; c < clipInfo.Length; c++) {
						var info = clipInfo[c];
						float weight = info.weight * layerWeight;
						if (weight == 0)
							continue;

						float time = stateInfo.normalizedTime * info.clip.length;
						animationTable[GetAnimationClipNameHashCode(info.clip)].Apply(skeleton, Mathf.Max(0, time - deltaTime), time, stateInfo.loop, events);
						#if USE_SPINE_EVENTS
						FireEvents(events, weight, this.AnimationEvent);
						#endif
						break;
					}

					//mix the rest
					for (; c < clipInfo.Length; c++) {
						var info = clipInfo[c];
						float weight = info.weight * layerWeight;
						if (weight == 0)
							continue;

						float time = stateInfo.normalizedTime * info.clip.length;
						animationTable[GetAnimationClipNameHashCode(info.clip)].Mix(skeleton, Mathf.Max(0, time - deltaTime), time, stateInfo.loop, events, weight);
						#if USE_SPINE_EVENTS
						FireEvents(events, weight, this.AnimationEvent);
						#endif
					}

					c = 0;
					#if UNITY_5
					if (nextStateInfo.fullPathHash != 0) {
					#else
					if (nextStateInfo.nameHash != 0) {
					#endif
						//apply next clip directly instead of mixing (ie:  no crossfade, ignores mecanim transition weights)
						if (mode == MixMode.SpineStyle) {
							for (; c < nextClipInfo.Length; c++) {
								var info = nextClipInfo[c];
								float weight = info.weight * layerWeight;
								if (weight == 0)
									continue;

								float time = nextStateInfo.normalizedTime * info.clip.length;
								animationTable[GetAnimationClipNameHashCode(info.clip)].Apply(skeleton, Mathf.Max(0, time - deltaTime), time, nextStateInfo.loop, events);
								#if USE_SPINE_EVENTS
								FireEvents(events, weight, this.AnimationEvent);
								#endif
								break;
							}
						}

						//mix the rest
						for (; c < nextClipInfo.Length; c++) {
							var info = nextClipInfo[c];
							float weight = info.weight * layerWeight;
							if (weight == 0)
								continue;

							float time = nextStateInfo.normalizedTime * info.clip.length;
							animationTable[GetAnimationClipNameHashCode(info.clip)].Mix(skeleton, Mathf.Max(0, time - deltaTime), time, nextStateInfo.loop, events, weight);
							#if USE_SPINE_EVENTS
							FireEvents(events, weight, this.AnimationEvent);
							#endif
						}
					}
				}
			}

			if (_UpdateLocal != null)
				_UpdateLocal(this);

			skeleton.UpdateWorldTransform();

			if (_UpdateWorld != null) {
				_UpdateWorld(this);
				skeleton.UpdateWorldTransform();
			}

			if (_UpdateComplete != null) {
				_UpdateComplete(this);
			}

			lastTime = Time.time;
		}

		private int GetAnimationClipNameHashCode (AnimationClip clip) {
			int clipNameHashCode;
			if (!clipNameHashCodeTable.TryGetValue(clip, out clipNameHashCode)) {
				clipNameHashCode = clip.name.GetHashCode();
				clipNameHashCodeTable.Add(clip, clipNameHashCode);
			}

			return clipNameHashCode;
		}

		#if USE_SPINE_EVENTS
		static void FireEvents (ExposedList<Spine.Event> eventList, float weight, SkeletonAnimatorEventDelegate callback) {
			int eventsCount = eventList.Count;
			if (eventsCount > 0) {
				var eventListItems = eventList.Items;
				for (int i = 0; i < eventsCount; i++) {
					if (callback != null)
						callback(eventListItems[i], weight);
				}

				eventList.Clear(false);
			}
		}
		#endif
	}
}
