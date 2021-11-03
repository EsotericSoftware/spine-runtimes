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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Spine.Unity.Prototyping {

	public class SpineEventUnityHandler : MonoBehaviour {

		[System.Serializable]
		public class EventPair {
			[SpineEvent] public string spineEvent;
			public UnityEvent unityHandler;
			public AnimationState.TrackEntryEventDelegate eventDelegate;
		}

		public List<EventPair> events = new List<EventPair>();

		ISkeletonComponent skeletonComponent;
		IAnimationStateComponent animationStateComponent;

		void Start () {
			if (skeletonComponent == null)
				skeletonComponent = GetComponent<ISkeletonComponent>();
			if (skeletonComponent == null) return;
			if (animationStateComponent == null)
				animationStateComponent = skeletonComponent as IAnimationStateComponent;
			if (animationStateComponent == null) return;
			var skeleton = skeletonComponent.Skeleton;
			if (skeleton == null) return;


			var skeletonData = skeleton.Data;
			var state = animationStateComponent.AnimationState;
			foreach (var ep in events) {
				var eventData = skeletonData.FindEvent(ep.spineEvent);
				ep.eventDelegate = ep.eventDelegate ?? delegate (TrackEntry trackEntry, Event e) { if (e.Data == eventData) ep.unityHandler.Invoke(); };
				state.Event += ep.eventDelegate;
			}
		}

		void OnDestroy () {
			if (animationStateComponent == null) animationStateComponent = GetComponent<IAnimationStateComponent>();
			if (animationStateComponent.IsNullOrDestroyed()) return;

			var state = animationStateComponent.AnimationState;
			foreach (var ep in events) {
				if (ep.eventDelegate != null) state.Event -= ep.eventDelegate;
				ep.eventDelegate = null;
			}
		}

	}
}
