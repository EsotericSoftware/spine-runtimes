/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#if (UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7)
#define PREUNITY_5_3
#endif

using UnityEngine;
using System.Collections;
using Spine;

namespace Spine.Unity {
	/// <summary>
	/// Use this as a condition-blocking yield instruction for Unity Coroutines. 
	/// The routine will pause until the AnimationState.TrackEntry fires its Complete event.</summary>
	public class WaitForSpineAnimationComplete : IEnumerator {
		
		bool m_WasFired = false;

		public WaitForSpineAnimationComplete (Spine.TrackEntry trackEntry) {
			#if PREUNITY_5_3
			Debug.LogWarning("Unity 5.3 or later is required for Spine Unity custom yield instructions to function correctly.");
			#endif

			SafeSubscribe(trackEntry);
		}

		void HandleComplete (AnimationState state, int trackIndex, int loopCount) {
			m_WasFired = true;
		}

		void SafeSubscribe (Spine.TrackEntry trackEntry) {
			if (trackEntry == null) {
				// Break immediately if trackEntry is null.
				Debug.LogWarning("TrackEntry was null. Coroutine will continue immediately.");
				m_WasFired = true;
			} else {
				// Function normally.
				trackEntry.Complete += HandleComplete;
			}
		}

		#region Reuse
		/// <summary>
		/// One optimization high-frequency YieldInstruction returns is to cache instances to minimize pressure. 
		/// Use NowWaitFor to reuse the same instance of WaitForSpineAnimationComplete.</summary>
		public WaitForSpineAnimationComplete NowWaitFor (Spine.TrackEntry trackEntry) {
			SafeSubscribe(trackEntry);
			return this;
		}
		#endregion

		#region IEnumerator
		bool IEnumerator.MoveNext () {
			if (m_WasFired) {
				((IEnumerator)this).Reset();	// auto-reset for YieldInstruction reuse
				return false;
			}

			return true;
		}
		void IEnumerator.Reset () { m_WasFired = false; }
		object IEnumerator.Current { get { return null; } }
		#endregion

	}

}
