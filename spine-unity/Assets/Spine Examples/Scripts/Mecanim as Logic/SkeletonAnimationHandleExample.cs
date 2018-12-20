using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Spine.Unity.Examples {
	public class SkeletonAnimationHandleExample : MonoBehaviour {

		public SkeletonAnimation skeletonAnimation;
		public List<StateNameToAnimationReference> statesAndAnimations = new List<StateNameToAnimationReference>();
		public List<AnimationTransition> transitions = new List<AnimationTransition>();

		[System.Serializable]
		public class StateNameToAnimationReference {
			public string stateName;
			public AnimationReferenceAsset animation;
		}

		[System.Serializable]
		public struct AnimationTransition {
			public AnimationReferenceAsset from;
			public AnimationReferenceAsset to;
			public AnimationReferenceAsset transition;
		}

		readonly Dictionary<Spine.AnimationStateData.AnimationPair, Spine.Animation> transitionDictionary = new Dictionary<AnimationStateData.AnimationPair, Animation>(Spine.AnimationStateData.AnimationPairComparer.Instance);

		void Awake () {
			foreach (var entry in transitions) {
				// If uninitialized
				entry.from.Initialize();
				entry.to.Initialize();
				entry.transition.Initialize();

				transitionDictionary.Add(new AnimationStateData.AnimationPair(entry.from.Animation, entry.to.Animation), entry.transition.Animation);
			}
		}

		public void SetFlip (float horizontal) {
			if (horizontal != 0) {
				skeletonAnimation.Skeleton.ScaleX = horizontal > 0 ? 1f : -1f;
			}
		}

		public void PlayAnimationForState (int shortNameHash, int layerIndex) {
			var foundAnimation = GetAnimationForState(shortNameHash);
			if (foundAnimation == null)
				return;

			PlayNewAnimation(foundAnimation, layerIndex);
		}

		public Spine.Animation GetAnimationForState (int shortNameHash) {
			var foundState = statesAndAnimations.Find(entry => Animator.StringToHash(entry.stateName) == shortNameHash);
			return (foundState == null) ? null : foundState.animation;
		}

		void PlayNewAnimation (Spine.Animation target, int layerIndex) {
			Spine.Animation transition = null;
			Spine.Animation current = null;

			var currentTrackEntry = skeletonAnimation.AnimationState.GetCurrent(layerIndex);
			if (currentTrackEntry != null) {
				current = currentTrackEntry.Animation;
				if (current != null)
					transitionDictionary.TryGetValue(new AnimationStateData.AnimationPair(current, target), out transition);
			}

			if (transition != null) {
				skeletonAnimation.AnimationState.SetAnimation(layerIndex, transition, false);
				skeletonAnimation.AnimationState.AddAnimation(layerIndex, target, true, 0f);
			} else {
				skeletonAnimation.AnimationState.SetAnimation(layerIndex, target, true);
			}
		}
	}
}