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
	public class DummyMecanimControllerExample : MonoBehaviour {

		public Animator logicAnimator;
		public SkeletonAnimationHandleExample animationHandle;

		[Header("Controls")]
		public KeyCode walkButton = KeyCode.LeftShift;
		public KeyCode jumpButton = KeyCode.Space;

		[Header("Animator Properties")]
		public string horizontalSpeedProperty = "Speed";
		public string verticalSpeedProperty = "VerticalSpeed";
		public string groundedProperty = "Grounded";

		[Header("Fake Physics")]
		public float jumpDuration = 1.5f;
		public Vector2 speed;
		public bool isGrounded;

		void Awake () {
			isGrounded = true;
		}

		void Update () {
			float x = Input.GetAxisRaw("Horizontal");
			if (Input.GetKey(walkButton)) {
				x *= 0.4f;
			}

			speed.x = x;

			// Flip skeleton.
			if (x != 0) {
				animationHandle.SetFlip(x);
			}

			if (Input.GetKeyDown(jumpButton)) {
				if (isGrounded)
					StartCoroutine(FakeJump());
			}

			logicAnimator.SetFloat(horizontalSpeedProperty, Mathf.Abs(speed.x));
			logicAnimator.SetFloat(verticalSpeedProperty, speed.y);
			logicAnimator.SetBool(groundedProperty, isGrounded);
		}

		IEnumerator FakeJump () {
			// Rise
			isGrounded = false;
			speed.y = 10f;
			float durationLeft = jumpDuration * 0.5f;
			while (durationLeft > 0) {
				durationLeft -= Time.deltaTime;
				if (!Input.GetKey(jumpButton)) break;
				yield return null;
			}

			// Fall
			speed.y = -10f;
			float fallDuration = (jumpDuration * 0.5f) - durationLeft;
			yield return new WaitForSeconds(fallDuration);

			// Land
			speed.y = 0f;
			isGrounded = true;
			yield return null;
		}
	}

}
