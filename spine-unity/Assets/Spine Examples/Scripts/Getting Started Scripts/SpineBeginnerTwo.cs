/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

using Spine.Unity;
using System.Collections;
using UnityEngine;

namespace Spine.Unity.Examples {
	public class SpineBeginnerTwo : MonoBehaviour {

		#region Inspector
		// [SpineAnimation] attribute allows an Inspector dropdown of Spine animation names coming form SkeletonAnimation.
		[SpineAnimation]
		public string runAnimationName;

		[SpineAnimation]
		public string idleAnimationName;

		[SpineAnimation]
		public string walkAnimationName;

		[SpineAnimation]
		public string shootAnimationName;

		[Header("Transitions")]
		[SpineAnimation]
		public string idleTurnAnimationName;

		[SpineAnimation]
		public string runToIdleAnimationName;

		public float runWalkDuration = 1.5f;
		#endregion

		SkeletonAnimation skeletonAnimation;

		// Spine.AnimationState and Spine.Skeleton are not Unity-serialized objects. You will not see them as fields in the inspector.
		public Spine.AnimationState spineAnimationState;
		public Spine.Skeleton skeleton;

		void Start () {
			// Make sure you get these AnimationState and Skeleton references in Start or Later.
			// Getting and using them in Awake is not guaranteed by default execution order.
			skeletonAnimation = GetComponent<SkeletonAnimation>();
			spineAnimationState = skeletonAnimation.AnimationState;
			skeleton = skeletonAnimation.Skeleton;

			StartCoroutine(DoDemoRoutine());
		}

		/// This is an infinitely repeating Unity Coroutine. Read the Unity documentation on Coroutines to learn more.
		IEnumerator DoDemoRoutine () {
			while (true) {
				// SetAnimation is the basic way to set an animation.
				// SetAnimation sets the animation and starts playing it from the beginning.
				// Common Mistake: If you keep calling it in Update, it will keep showing the first pose of the animation, do don't do that.

				spineAnimationState.SetAnimation(0, walkAnimationName, true);
				yield return new WaitForSeconds(runWalkDuration);

				spineAnimationState.SetAnimation(0, runAnimationName, true);
				yield return new WaitForSeconds(runWalkDuration);

				// AddAnimation queues up an animation to play after the previous one ends.
				spineAnimationState.SetAnimation(0, runToIdleAnimationName, false);
				spineAnimationState.AddAnimation(0, idleAnimationName, true, 0);
				yield return new WaitForSeconds(1f);

				skeleton.ScaleX = -1;       // skeleton allows you to flip the skeleton.
				spineAnimationState.SetAnimation(0, idleTurnAnimationName, false);
				spineAnimationState.AddAnimation(0, idleAnimationName, true, 0);
				yield return new WaitForSeconds(0.5f);
				skeleton.ScaleX = 1;
				spineAnimationState.SetAnimation(0, idleTurnAnimationName, false);
				spineAnimationState.AddAnimation(0, idleAnimationName, true, 0);
				yield return new WaitForSeconds(0.5f);

			}
		}

	}

}
