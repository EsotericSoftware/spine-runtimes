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

using Spine.Unity.Modules;

namespace Spine.Unity.Examples {
	public class SpineboyPole : MonoBehaviour {
		public SkeletonAnimation skeletonAnimation;
		public SkeletonRenderSeparator separator;

		[Space(18)]
		[SpineAnimation]
		public string run;
		[SpineAnimation]
		public string pole;
		public float startX;
		public float endX;

		const float Speed = 18f;
		const float RunTimeScale = 1.5f;

		IEnumerator Start () {
			var state = skeletonAnimation.state;

			while (true) {
				// Run phase
				SetXPosition(startX);
				separator.enabled = false; // Disable Separator during run.
				state.SetAnimation(0, run, true);
				state.TimeScale = RunTimeScale;

				while (transform.localPosition.x < endX) {
					transform.Translate(Vector3.right * Speed * Time.deltaTime);
					yield return null;
				}

				// Hit phase
				SetXPosition(endX);
				separator.enabled = true; // Enable Separator when hit
				var poleTrack = state.SetAnimation(0, pole, false);
				yield return new WaitForSpineAnimationComplete(poleTrack);
				yield return new WaitForSeconds(1f);
			}
		}

		void SetXPosition (float x) {
			var tp = transform.localPosition;
			tp.x = x;
			transform.localPosition = tp;
		}
	}

}
