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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Playables;

using Spine;
using Spine.Unity;
using Spine.Unity.Playables;

namespace Spine.Unity.Playables {

	[AddComponentMenu("Spine/Playables/SkeletonAnimation Playable Handle (Playables)")]
	public class SkeletonAnimationPlayableHandle : SpinePlayableHandleBase {
		#region Inspector
		public SkeletonAnimation skeletonAnimation;
		//public float fadeOutDuration = 0.5f;

		#if UNITY_EDITOR
		void OnValidate () {
			if (this.skeletonAnimation == null)
				skeletonAnimation = GetComponent<SkeletonAnimation>();
		}
		#endif

		#endregion

		//readonly HashSet<int> frameAppliedProperties = new HashSet<int>();

		public override Skeleton Skeleton {	get { return skeletonAnimation.Skeleton; } }
		public override SkeletonData SkeletonData { get { return skeletonAnimation.Skeleton.data; } }

		#if UNITY_2017 || UNITY_2018 || (UNITY_2019_1_OR_NEWER && SPINE_TIMELINE_PACKAGE_DOWNLOADED)
		void Awake () {
			if (skeletonAnimation == null)
				skeletonAnimation = GetComponent<SkeletonAnimation>();

			//frameAppliedProperties.Clear();
		}

		//Skeleton skeleton;
		//int frameTrackCount = 0;
		//int frameCurrentInputs = 0;
		//bool firstCleared = false;
		//int lastApplyFrame = 0;
		//public override void ProcessFrame (Playable playable, FrameData info, SpineAnimationMixerBehaviour mixer) {
		//	if (skeletonAnimation == null) return;
		//	if (skeleton == null) skeleton = skeletonAnimation.Skeleton;

		//	// New frame.
		//	if (lastApplyFrame != Time.frameCount) {
		//		if (frameTrackCount > 0)
		//			frameAppliedProperties.Clear();					

		//		frameCurrentInputs = 0;
		//		frameTrackCount = 0;	
		//	}
		//	lastApplyFrame = Time.frameCount;

		//	int currentInputs = mixer.ApplyPlayableFrame(playable, skeleton, frameAppliedProperties, frameTrackCount);
		//	frameCurrentInputs += currentInputs;

		//	// EXPERIMENTAL: Handle overriding SkeletonAnimation.AnimationState.
		//	if (frameCurrentInputs > 0) {
		//		var state = skeletonAnimation.AnimationState;

		//		if (!firstCleared) {
		//			firstCleared = true;
		//			for (int i = 0; i < 4; i++) {
		//				if (state.GetCurrent(i) != null) state.SetEmptyAnimation(i, fadeOutDuration);
		//			}
		//		}

		//		// Update again whenever an animation is playing in the AnimationState. Quite wasteful.
		//		//if (state.GetCurrent(0) != null) {
		//			skeleton.UpdateWorldTransform();
		//		//}
		//	}

		//	frameTrackCount++;
		//}
#endif
	}

}
