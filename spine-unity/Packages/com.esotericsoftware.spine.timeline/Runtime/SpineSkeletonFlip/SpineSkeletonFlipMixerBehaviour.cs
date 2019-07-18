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

using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

using Spine.Unity;

namespace Spine.Unity.Playables {
	public class SpineSkeletonFlipMixerBehaviour : PlayableBehaviour {
		float originalScaleX, originalScaleY;
		float baseScaleX, baseScaleY;

		SpinePlayableHandleBase playableHandle;
		bool m_FirstFrameHappened;

		public override void ProcessFrame (Playable playable, FrameData info, object playerData) {
			playableHandle = playerData as SpinePlayableHandleBase;

			if (playableHandle == null)
				return;

			var skeleton = playableHandle.Skeleton;

			if (!m_FirstFrameHappened) {
				originalScaleX = skeleton.ScaleX;
				originalScaleY = skeleton.ScaleY;
				baseScaleX = Mathf.Abs(originalScaleX);
				baseScaleY = Mathf.Abs(originalScaleY);
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
					SetSkeletonScaleFromFlip(skeleton, input.flipX, input.flipY);
					greatestWeight = inputWeight;
				}

				if (!Mathf.Approximately(inputWeight, 0f))
					currentInputs++;
			}

			if (currentInputs != 1 && 1f - totalWeight > greatestWeight) {
				skeleton.ScaleX = originalScaleX;
				skeleton.ScaleY = originalScaleY;
			}
		}

		public void SetSkeletonScaleFromFlip (Skeleton skeleton, bool flipX, bool flipY) {
			skeleton.ScaleX = flipX ? -baseScaleX : baseScaleX;
			skeleton.ScaleY = flipY ? -baseScaleY : baseScaleY;
		}

		public override void OnGraphStop (Playable playable) {
			m_FirstFrameHappened = false;

			if (playableHandle == null)
				return;

			var skeleton = playableHandle.Skeleton;
			skeleton.ScaleX = originalScaleX;
			skeleton.ScaleY = originalScaleY;
		}
	}

}
