/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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
