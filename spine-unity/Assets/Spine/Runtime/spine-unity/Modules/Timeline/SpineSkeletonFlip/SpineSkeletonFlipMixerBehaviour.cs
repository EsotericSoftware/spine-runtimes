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

#if UNITY_2017 || UNITY_2018
 using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

using Spine.Unity;

namespace Spine.Unity.Playables {
	public class SpineSkeletonFlipMixerBehaviour : PlayableBehaviour {
		bool defaultFlipX, defaultFlipY;

		SpinePlayableHandleBase playableHandle;
		bool m_FirstFrameHappened;

		public override void ProcessFrame (Playable playable, FrameData info, object playerData) {
			playableHandle = playerData as SpinePlayableHandleBase;

			if (playableHandle == null)
				return;

			var skeleton = playableHandle.Skeleton;

			if (!m_FirstFrameHappened) {
				defaultFlipX = skeleton.flipX;
				defaultFlipY = skeleton.flipY;
				m_FirstFrameHappened = true;
			}

			int inputCount = playable.GetInputCount();

			float totalWeight = 0f;
			float greatestWeight = 0f;
			int currentInputs = 0;

			for (int i = 0; i < inputCount; i++) {
				float inputWeight = playable.GetInputWeight(i);
				ScriptPlayable<SpineSkeletonFlipBehaviour> inputPlayable = (ScriptPlayable<SpineSkeletonFlipBehaviour>)playable.GetInput(i);
				SpineSkeletonFlipBehaviour input = inputPlayable.GetBehaviour();

				totalWeight += inputWeight;

				if (inputWeight > greatestWeight) {
					skeleton.flipX = input.flipX;
					skeleton.flipY = input.flipY;
					greatestWeight = inputWeight;
				}

				if (!Mathf.Approximately(inputWeight, 0f))
					currentInputs++;
			}

			if (currentInputs != 1 && 1f - totalWeight > greatestWeight) {
				skeleton.flipX = defaultFlipX;
				skeleton.flipY = defaultFlipY;
			}
		}

		public override void OnGraphStop (Playable playable) {
			m_FirstFrameHappened = false;

			if (playableHandle == null)
				return;

			var skeleton = playableHandle.Skeleton;
			skeleton.flipX = defaultFlipX;
			skeleton.flipY = defaultFlipY;
		}
	}

}
#endif
