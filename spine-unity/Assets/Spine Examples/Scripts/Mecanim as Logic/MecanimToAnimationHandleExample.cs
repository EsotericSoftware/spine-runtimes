using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Examples {
	
	// This StateMachineBehaviour handles sending the Mecanim state information to the component that handles playing the Spine animations.
	public class MecanimToAnimationHandleExample : StateMachineBehaviour {
		SkeletonAnimationHandleExample animationHandle;
		bool initialized;

		override public void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			if (!initialized) {
				animationHandle = animator.GetComponent<SkeletonAnimationHandleExample>();
				initialized = true;
			}

			animationHandle.PlayAnimationForState(stateInfo.shortNameHash, layerIndex);
		}
	}

}
