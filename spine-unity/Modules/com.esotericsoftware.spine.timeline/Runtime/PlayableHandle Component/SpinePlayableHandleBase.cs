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

namespace Spine.Unity.Playables {

	public delegate void SpineEventDelegate (Spine.Event e);

	/// <summary>Base class for Spine Playable Handle components, commonly for integrating with UnityEngine Timeline.</summary>
	public abstract class SpinePlayableHandleBase : MonoBehaviour {

		/// <summary>Gets the SkeletonData of the targeted Spine component.</summary>
		public abstract SkeletonData SkeletonData { get; }

		public abstract Skeleton Skeleton { get; }

		/// <summary>Subscribe to this to handle user events played by the Unity playable</summary>
		public event SpineEventDelegate AnimationEvents;

		public virtual void HandleEvents (ExposedList<Event> eventBuffer) {
			if (eventBuffer == null || AnimationEvents == null) return;
			for (int i = 0, n = eventBuffer.Count; i < n; i++)
				AnimationEvents.Invoke(eventBuffer.Items[i]);
		}

	}
}
