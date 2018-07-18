using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Spine;
using Spine.Unity;

public class AnimationStateMecanimState : StateMachineBehaviour {

	#region Inspector
	public AnimationReferenceAsset animation;

	[System.Serializable]
	public struct AnimationTransition {
		public AnimationReferenceAsset from;
		public AnimationReferenceAsset transition;
	}

	[UnityEngine.Serialization.FormerlySerializedAs("transitions")]
	public List<AnimationTransition> fromTransitions = new List<AnimationTransition>();
	#endregion

	Spine.AnimationState state;

	public void Initialize (Animator animator) {
		if (state == null) {
			var animationStateComponent = (animator.GetComponent(typeof(IAnimationStateComponent))) as IAnimationStateComponent;
			state = animationStateComponent.AnimationState;
		}
	}

	override public void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (state == null) {
			Initialize(animator);
		}
		
		float timeScale = stateInfo.speed;
		var current = state.GetCurrent(layerIndex);

		bool transitionPlayed = false;
		if (current != null && fromTransitions.Count > 0) {
			foreach (var t in fromTransitions) {
				if (t.from.Animation == current.Animation) {
					var transitionEntry = state.SetAnimation(layerIndex, t.transition.Animation, false);
					transitionEntry.TimeScale = timeScale;
					transitionPlayed = true;
					break;
				}
			}
		}

		TrackEntry trackEntry;
		if (transitionPlayed) {
			trackEntry = state.AddAnimation(layerIndex, animation.Animation, stateInfo.loop, 0);
		} else {
			trackEntry = state.SetAnimation(layerIndex, animation.Animation, stateInfo.loop);
		}
		trackEntry.TimeScale = timeScale;

	}

}
