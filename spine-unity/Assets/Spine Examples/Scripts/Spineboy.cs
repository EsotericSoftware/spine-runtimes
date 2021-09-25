/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

using Spine;
using Spine.Unity;
using UnityEngine;

namespace Spine.Unity.Examples {
	public class Spineboy : MonoBehaviour {
		SkeletonAnimation skeletonAnimation;

		public void Start () {
			skeletonAnimation = GetComponent<SkeletonAnimation>(); // Get the SkeletonAnimation component for the GameObject this script is attached to.
			var animationState = skeletonAnimation.AnimationState;

			animationState.Event += HandleEvent; ; // Call our method any time an animation fires an event.
			animationState.End += (entry) => Debug.Log("start: " + entry.TrackIndex); // A lambda can be used for the callback instead of a method.

			animationState.AddAnimation(0, "jump", false, 2);   // Queue jump to be played on track 0 two seconds after the starting animation.
			animationState.AddAnimation(0, "run", true, 0); // Queue walk to be looped on track 0 after the jump animation.
		}

		void HandleEvent (TrackEntry trackEntry, Spine.Event e) {
			Debug.Log(trackEntry.TrackIndex + " " + trackEntry.Animation.Name + ": event " + e + ", " + e.Int);
		}

		public void OnMouseDown () {
			skeletonAnimation.AnimationState.SetAnimation(0, "jump", false); // Set jump to be played on track 0 immediately.
			skeletonAnimation.AnimationState.AddAnimation(0, "run", true, 0); // Queue walk to be looped on track 0 after the jump animation.
		}
	}

}
