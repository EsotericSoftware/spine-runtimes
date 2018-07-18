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

// Contributed by: Mitch Thompson

using UnityEngine;
using System.Collections;
using Spine.Unity;

namespace Spine.Unity.Examples {
	public class FootSoldierExample : MonoBehaviour {
		[SpineAnimation("Idle")]
		public string idleAnimation;

		[SpineAnimation]
		public string attackAnimation;

		[SpineAnimation]
		public string moveAnimation;

		[SpineSlot]
		public string eyesSlot;

		[SpineAttachment(currentSkinOnly: true, slotField: "eyesSlot")]
		public string eyesOpenAttachment;

		[SpineAttachment(currentSkinOnly: true, slotField: "eyesSlot")]
		public string blinkAttachment;

		[Range(0, 0.2f)]
		public float blinkDuration = 0.05f;

		public KeyCode attackKey = KeyCode.Mouse0;
		public KeyCode rightKey = KeyCode.D;
		public KeyCode leftKey = KeyCode.A;

		public float moveSpeed = 3;

		SkeletonAnimation skeletonAnimation;

		void Awake () {
			skeletonAnimation = GetComponent<SkeletonAnimation>();
			skeletonAnimation.OnRebuild += Apply;
		}

		void Apply (SkeletonRenderer skeletonRenderer) {
			StartCoroutine("Blink");
		}

		void Update () {
			if (Input.GetKey(attackKey)) {
				skeletonAnimation.AnimationName = attackAnimation;
			} else {
				if (Input.GetKey(rightKey)) {
					skeletonAnimation.AnimationName = moveAnimation;
					skeletonAnimation.Skeleton.ScaleX = 1;
					transform.Translate(moveSpeed * Time.deltaTime, 0, 0);
				} else if(Input.GetKey(leftKey)) {
					skeletonAnimation.AnimationName = moveAnimation;
					skeletonAnimation.Skeleton.ScaleX = -1;
					transform.Translate(-moveSpeed * Time.deltaTime, 0, 0);
				} else {
					skeletonAnimation.AnimationName = idleAnimation;
				}
			}
		}

		IEnumerator Blink() {
			while (true) {
				yield return new WaitForSeconds(Random.Range(0.25f, 3f));
				skeletonAnimation.Skeleton.SetAttachment(eyesSlot, blinkAttachment);
				yield return new WaitForSeconds(blinkDuration);
				skeletonAnimation.Skeleton.SetAttachment(eyesSlot, eyesOpenAttachment);
			}
		}
	}
}