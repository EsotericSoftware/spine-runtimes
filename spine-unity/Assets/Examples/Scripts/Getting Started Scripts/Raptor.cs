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
	public class Raptor : MonoBehaviour {

		#region Inspector
		[SpineAnimation]
		public string walk = "walk";

		[SpineAnimation]
		public string gungrab = "gungrab";

		[SpineAnimation]
		public string gunkeep = "gunkeep";

		[SpineEvent]
		public string footstepEvent = "footstep";

		public AudioSource footstepAudioSource;
		#endregion

		SkeletonAnimation skeletonAnimation;

		void Start () {
			skeletonAnimation = GetComponent<SkeletonAnimation>();
			skeletonAnimation.AnimationState.Event += HandleEvent;
			StartCoroutine(GunGrabRoutine());
		}

		void HandleEvent (Spine.TrackEntry trackEntry, Spine.Event e) {
			if (e.Data.Name == footstepEvent) {
				footstepAudioSource.pitch = 0.5f + Random.Range(-0.2f, 0.2f);
				footstepAudioSource.Play();
			}
		}

		IEnumerator GunGrabRoutine () {		
			// Play the walk animation on track 0.
			skeletonAnimation.AnimationState.SetAnimation(0, walk, true);

			// Repeatedly play the gungrab and gunkeep animation on track 1.
			while (true) {
				yield return new WaitForSeconds(Random.Range(0.5f, 3f));
				skeletonAnimation.AnimationState.SetAnimation(1, gungrab, false);

				yield return new WaitForSeconds(Random.Range(0.5f, 3f));
				skeletonAnimation.AnimationState.SetAnimation(1, gunkeep, false);
			}

		}

	}
}