﻿/******************************************************************************
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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Spine.Unity.Modules {

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
			skeletonComponent = skeletonComponent ?? GetComponent<ISkeletonComponent>();
			if (skeletonComponent == null) return;
			animationStateComponent = animationStateComponent ?? skeletonComponent as IAnimationStateComponent;
			if (animationStateComponent == null) return;
			var skeleton = skeletonComponent.Skeleton;
			if (skeleton == null) return;


			var skeletonData = skeleton.Data;
			var state = animationStateComponent.AnimationState;
			foreach (var ep in events) {
				var eventData = skeletonData.FindEvent(ep.spineEvent);
				ep.eventDelegate = ep.eventDelegate ?? delegate(TrackEntry trackEntry, Event e) { if (e.Data == eventData) ep.unityHandler.Invoke(); };
				state.Event += ep.eventDelegate;
			}
		}

		void OnDestroy () {
			animationStateComponent = animationStateComponent ?? GetComponent<IAnimationStateComponent>();
			if (animationStateComponent == null) return;

			var state = animationStateComponent.AnimationState;
			foreach (var ep in events) {
				if (ep.eventDelegate != null) state.Event -= ep.eventDelegate;
				ep.eventDelegate = null;
			}
		}

	}
}

