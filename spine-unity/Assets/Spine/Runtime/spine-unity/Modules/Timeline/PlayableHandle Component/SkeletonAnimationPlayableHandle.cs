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

		#if UNITY_2017 || UNITY_2018
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
