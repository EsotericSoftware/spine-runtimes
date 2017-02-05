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

using UnityEngine;
using System.Collections;
using Spine.Unity;

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
		#endregion

		SkeletonAnimation skeletonAnimation;

		// Spine.AnimationState and Spine.Skeleton are not Unity-serialized objects. You will not see them as fields in the inspector.
		public Spine.AnimationState spineAnimationState;
		public Spine.Skeleton skeleton;

		void Start () {
			// Make sure you get these AnimationState and Skeleton references in Start or Later. Getting and using them in Awake is not guaranteed by default execution order.
			skeletonAnimation = GetComponent<SkeletonAnimation>();
			spineAnimationState = skeletonAnimation.AnimationState;
			skeleton = skeletonAnimation.Skeleton;

			StartCoroutine(DoDemoRoutine());
		}

		/// <summary>This is an infinitely repeating Unity Coroutine. Read the Unity documentation on Coroutines to learn more.</summary>
		IEnumerator DoDemoRoutine () {

			while (true) {
				// SetAnimation is the basic way to set an animation.
				// SetAnimation sets the animation and starts playing it from the beginning.
				// Common Mistake: If you keep calling it in Update, it will keep showing the first pose of the animation, do don't do that.

				spineAnimationState.SetAnimation(0, walkAnimationName, true);
				yield return new WaitForSeconds(1.5f);

				// skeletonAnimation.AnimationName = runAnimationName; // this line also works for quick testing/simple uses.
				spineAnimationState.SetAnimation(0, runAnimationName, true);
				yield return new WaitForSeconds(1.5f);

				spineAnimationState.SetAnimation(0, idleAnimationName, true);
				yield return new WaitForSeconds(1f);

				skeleton.FlipX = true;		// skeleton allows you to flip the skeleton.
				yield return new WaitForSeconds(0.5f);
				skeleton.FlipX = false;
				yield return new WaitForSeconds(0.5f);

			}
		}
	}

}
