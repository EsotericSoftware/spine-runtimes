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

using UnityEngine;
using System.Collections;
using Spine;

namespace Spine.Unity {
	/// <summary>
	/// Use this as a condition-blocking yield instruction for Unity Coroutines.
	/// The routine will pause until the AnimationState.TrackEntry fires its Complete event.
	/// It can be configured to trigger on the End event as well to cover interruption.
	/// <p/>
	/// See the <see cref="http://esotericsoftware.com/spine-unity-events">Spine Unity Events documentation page</see>
	/// and <see cref="http://esotericsoftware.com/spine-api-reference#AnimationStateListener"/>
	/// for more information on when track events will be triggered.</summary>
	public class WaitForSpineAnimationComplete : WaitForSpineAnimation, IEnumerator {

		public WaitForSpineAnimationComplete (Spine.TrackEntry trackEntry, bool includeEndEvent = false) :
			base(trackEntry,
				includeEndEvent ? (AnimationEventTypes.Complete | AnimationEventTypes.End) : AnimationEventTypes.Complete)
		{
		}

		#region Reuse
		/// <summary>
		/// One optimization high-frequency YieldInstruction returns is to cache instances to minimize GC pressure.
		/// Use NowWaitFor to reuse the same instance of WaitForSpineAnimationComplete.</summary>
		public WaitForSpineAnimationComplete NowWaitFor (Spine.TrackEntry trackEntry, bool includeEndEvent = false) {
			SafeSubscribe(trackEntry,
				includeEndEvent ? (AnimationEventTypes.Complete | AnimationEventTypes.End) : AnimationEventTypes.Complete);
			return this;
		}
		#endregion
	}
}
