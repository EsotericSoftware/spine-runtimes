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

using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Examples {

	// This is an example of an animation handle. This is implemented with strings as state names.
	// Strings can serve as the identifier when Mecanim is used as the state machine and state source.
	// If you don't use Mecanim, using custom ScriptableObjects may be a more efficient way to store information about the state and its connection with specific Spine animations.

	// This animation handle implementation also comes with a dummy implementation of transition-handling.
	public class SkeletonAnimationHandleExample : MonoBehaviour {
		public SkeletonAnimation skeletonAnimation;
		public List<StateNameToAnimationReference> statesAndAnimations = new List<StateNameToAnimationReference>();
		public List<AnimationTransition> transitions = new List<AnimationTransition>(); // Alternately, an AnimationPair-Animation Dictionary (commented out) can be used for more efficient lookups.

		[System.Serializable]
		public class StateNameToAnimationReference {
			public string stateName;
			public AnimationReferenceAsset animation;
		}

		[System.Serializable]
		public class AnimationTransition {
			public AnimationReferenceAsset from;
			public AnimationReferenceAsset to;
			public AnimationReferenceAsset transition;
		}

		//readonly Dictionary<Spine.AnimationStateData.AnimationPair, Spine.Animation> transitionDictionary = new Dictionary<AnimationStateData.AnimationPair, Animation>(Spine.AnimationStateData.AnimationPairComparer.Instance);

		public Spine.Animation TargetAnimation { get; private set; }

		void Awake () {
			// Initialize AnimationReferenceAssets
			foreach (var entry in statesAndAnimations) {
				entry.animation.Initialize();
			}
			foreach (var entry in transitions) {
				entry.from.Initialize();
				entry.to.Initialize();
				entry.transition.Initialize();
			}

			// Build Dictionary
			//foreach (var entry in transitions) {
			//	transitionDictionary.Add(new AnimationStateData.AnimationPair(entry.from.Animation, entry.to.Animation), entry.transition.Animation);
			//}
		}

		/// <summary>Sets the horizontal flip state of the skeleton based on a nonzero float. If negative, the skeleton is flipped. If positive, the skeleton is not flipped.</summary>
		public void SetFlip (float horizontal) {
			if (horizontal != 0) {
				skeletonAnimation.Skeleton.ScaleX = horizontal > 0 ? 1f : -1f;
			}
		}

		/// <summary>Plays an animation based on the state name.</summary>
		public void PlayAnimationForState (string stateShortName, int layerIndex) {
			PlayAnimationForState(StringToHash(stateShortName), layerIndex);
		}

		/// <summary>Plays an animation based on the hash of the state name.</summary>
		public void PlayAnimationForState (int shortNameHash, int layerIndex) {
			var foundAnimation = GetAnimationForState(shortNameHash);
			if (foundAnimation == null)
				return;

			PlayNewAnimation(foundAnimation, layerIndex);
		}

		/// <summary>Gets a Spine Animation based on the state name.</summary>
		public Spine.Animation GetAnimationForState (string stateShortName) {
			return GetAnimationForState(StringToHash(stateShortName));
		}

		/// <summary>Gets a Spine Animation based on the hash of the state name.</summary>
		public Spine.Animation GetAnimationForState (int shortNameHash) {
			var foundState = statesAndAnimations.Find(entry => StringToHash(entry.stateName) == shortNameHash);
			return (foundState == null) ? null : foundState.animation;
		}

		/// <summary>Play an animation. If a transition animation is defined, the transition is played before the target animation being passed.</summary>
		public void PlayNewAnimation (Spine.Animation target, int layerIndex) {
			Spine.Animation transition = null;
			Spine.Animation current = null;

			current = GetCurrentAnimation(layerIndex);
			if (current != null)
				transition = TryGetTransition(current, target);

			if (transition != null) {
				skeletonAnimation.AnimationState.SetAnimation(layerIndex, transition, false);
				skeletonAnimation.AnimationState.AddAnimation(layerIndex, target, true, 0f);
			} else {
				skeletonAnimation.AnimationState.SetAnimation(layerIndex, target, true);
			}

			this.TargetAnimation = target;
		}

		/// <summary>Play a non-looping animation once then continue playing the state animation.</summary>
		public void PlayOneShot (Spine.Animation oneShot, int layerIndex) {
			var state = skeletonAnimation.AnimationState;
			state.SetAnimation(0, oneShot, false);

			var transition = TryGetTransition(oneShot, TargetAnimation);
			if (transition != null)
				state.AddAnimation(0, transition, false, 0f);

			state.AddAnimation(0, this.TargetAnimation, true, 0f);
		}

		Spine.Animation TryGetTransition (Spine.Animation from, Spine.Animation to) {
			foreach (var transition in transitions) {
				if (transition.from.Animation == from && transition.to.Animation == to) {
					return transition.transition.Animation;
				}
			}
			return null;

			//Spine.Animation foundTransition = null;
			//transitionDictionary.TryGetValue(new AnimationStateData.AnimationPair(from, to), out foundTransition);
			//return foundTransition;
		}

		Spine.Animation GetCurrentAnimation (int layerIndex) {
			var currentTrackEntry = skeletonAnimation.AnimationState.GetCurrent(layerIndex);
			return (currentTrackEntry != null) ? currentTrackEntry.Animation : null;
		}

		int StringToHash (string s) {
			return Animator.StringToHash(s);
		}
	}
}
