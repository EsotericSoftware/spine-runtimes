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

namespace Spine.Unity.Examples {
	public class HandleEventWithAudioExample : MonoBehaviour {

		public SkeletonAnimation skeletonAnimation;
		[SpineEvent(dataField: "skeletonAnimation", fallbackToTextField: true)]
		public string eventName;

		[Space]
		public AudioSource audioSource;
		public AudioClip audioClip;
		public float basePitch = 1f;
		public float randomPitchOffset = 0.1f;

		[Space]
		public bool logDebugMessage = false;

		Spine.EventData eventData;

		void OnValidate () {
			if (skeletonAnimation == null) GetComponent<SkeletonAnimation>();
			if (audioSource == null) GetComponent<AudioSource>();
		}

		void Start () {
			if (audioSource == null) return;
			if (skeletonAnimation == null) return;
			skeletonAnimation.Initialize(false);
			if (!skeletonAnimation.valid) return;

			eventData = skeletonAnimation.Skeleton.Data.FindEvent(eventName);
			skeletonAnimation.AnimationState.Event += HandleAnimationStateEvent;
		}

		private void HandleAnimationStateEvent (TrackEntry trackEntry, Event e) {
			if (logDebugMessage) Debug.Log("Event fired! " + e.Data.Name);
			//bool eventMatch = string.Equals(e.Data.Name, eventName, System.StringComparison.Ordinal); // Testing recommendation: String compare.
			bool eventMatch = (eventData == e.Data); // Performance recommendation: Match cached reference instead of string.
			if (eventMatch) {
				Play();
			}
		}

		public void Play () {
			audioSource.pitch = basePitch + Random.Range(-randomPitchOffset, randomPitchOffset);
			audioSource.clip = audioClip;
			audioSource.Play();
		}
	}

}
