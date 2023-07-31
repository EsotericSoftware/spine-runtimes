/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using System.Collections;
using UnityEngine;

namespace Spine.Unity {
	/// <summary>
	/// Use this as a condition-blocking yield instruction for Unity Coroutines.
	/// The routine will pause until the AnimationState.TrackEntry fires any of the
	/// configured events.
	/// <p/>
	/// See the <see cref="http://esotericsoftware.com/spine-unity-events">Spine Unity Events documentation page</see>
	/// and <see cref="http://esotericsoftware.com/spine-api-reference#AnimationStateListener"/>
	/// for more information on when track events will be triggered.</summary>
	public class WaitForSpineAnimation : IEnumerator {

		[Flags]
		public enum AnimationEventTypes {
			Start = 1,
			Interrupt = 2,
			End = 4,
			Dispose = 8,
			Complete = 16
		}

		bool m_WasFired = false;

		public WaitForSpineAnimation (Spine.TrackEntry trackEntry, AnimationEventTypes eventsToWaitFor) {
			SafeSubscribe(trackEntry, eventsToWaitFor);
		}

		#region Reuse
		/// <summary>
		/// One optimization high-frequency YieldInstruction returns is to cache instances to minimize GC pressure.
		/// Use NowWaitFor to reuse the same instance of WaitForSpineAnimationComplete.</summary>
		public WaitForSpineAnimation NowWaitFor (Spine.TrackEntry trackEntry, AnimationEventTypes eventsToWaitFor) {
			SafeSubscribe(trackEntry, eventsToWaitFor);
			return this;
		}
		#endregion

		#region IEnumerator
		bool IEnumerator.MoveNext () {
			if (m_WasFired) {
				((IEnumerator)this).Reset();    // auto-reset for YieldInstruction reuse
				return false;
			}

			return true;
		}
		void IEnumerator.Reset () { m_WasFired = false; }
		object IEnumerator.Current { get { return null; } }
		#endregion

		protected void SafeSubscribe (Spine.TrackEntry trackEntry, AnimationEventTypes eventsToWaitFor) {
			if (trackEntry == null) {
				// Break immediately if trackEntry is null.
				Debug.LogWarning("TrackEntry was null. Coroutine will continue immediately.");
				m_WasFired = true;
			} else {
				if ((eventsToWaitFor & AnimationEventTypes.Start) != 0)
					trackEntry.Start += HandleComplete;
				if ((eventsToWaitFor & AnimationEventTypes.Interrupt) != 0)
					trackEntry.Interrupt += HandleComplete;
				if ((eventsToWaitFor & AnimationEventTypes.End) != 0)
					trackEntry.End += HandleComplete;
				if ((eventsToWaitFor & AnimationEventTypes.Dispose) != 0)
					trackEntry.Dispose += HandleComplete;
				if ((eventsToWaitFor & AnimationEventTypes.Complete) != 0)
					trackEntry.Complete += HandleComplete;
			}
		}

		void HandleComplete (TrackEntry trackEntry) {
			m_WasFired = true;
		}
	}
}
