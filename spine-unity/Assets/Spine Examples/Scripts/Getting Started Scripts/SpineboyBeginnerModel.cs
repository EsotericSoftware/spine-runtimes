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

namespace Spine.Unity.Examples {
	[SelectionBase]
	public class SpineboyBeginnerModel : MonoBehaviour {

		#region Inspector
		[Header("Current State")]
		public SpineBeginnerBodyState state;
		public bool facingLeft;
		[Range(-1f, 1f)]
		public float currentSpeed;

		[Header("Balance")]
		public float shootInterval = 0.12f;
		#endregion

		float lastShootTime;
		public event System.Action ShootEvent;	// Lets other scripts know when Spineboy is shooting. Check C# Documentation to learn more about events and delegates.

		#region API
		public void TryJump () {
			StartCoroutine(JumpRoutine());
		}

		public void TryShoot () {
			float currentTime = Time.time;

			if (currentTime - lastShootTime > shootInterval) {
				lastShootTime = currentTime;
				if (ShootEvent != null) ShootEvent();	// Fire the "ShootEvent" event.
			}
		}

		public void TryMove (float speed) {
			currentSpeed = speed; // show the "speed" in the Inspector.

			if (speed != 0) {
				bool speedIsNegative = (speed < 0f);
				facingLeft = speedIsNegative; // Change facing direction whenever speed is not 0.
			}

			if (state != SpineBeginnerBodyState.Jumping) {
				state = (speed == 0) ? SpineBeginnerBodyState.Idle : SpineBeginnerBodyState.Running;
			}

		}
		#endregion

		IEnumerator JumpRoutine () {
			if (state == SpineBeginnerBodyState.Jumping) yield break;	// Don't jump when already jumping.

			state = SpineBeginnerBodyState.Jumping;

			// Fake jumping.
			{
				var pos = transform.localPosition;
				const float jumpTime = 1.2f;
				const float half = jumpTime * 0.5f;
				const float jumpPower = 20f;
				for (float t = 0; t < half; t += Time.deltaTime) {
					float d = jumpPower * (half - t);
					transform.Translate((d * Time.deltaTime) * Vector3.up);
					yield return null;
				}
				for (float t = 0; t < half; t += Time.deltaTime) {
					float d = jumpPower * t;
					transform.Translate((d * Time.deltaTime) * Vector3.down);
					yield return null;
				}
				transform.localPosition = pos;
			}

			state = SpineBeginnerBodyState.Idle;
		}

	}

	public enum SpineBeginnerBodyState {
		Idle,
		Running,
		Jumping
	}
}
