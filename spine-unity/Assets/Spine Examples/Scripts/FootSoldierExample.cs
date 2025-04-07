/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

// Contributed by: Mitch Thompson

using Spine.Unity;
using System.Collections;
using UnityEngine;

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
			StartCoroutine(Blink());
		}

		void Update () {
			if (Input.GetKey(attackKey)) {
				skeletonAnimation.AnimationName = attackAnimation;
			} else {
				if (Input.GetKey(rightKey)) {
					skeletonAnimation.AnimationName = moveAnimation;
					skeletonAnimation.Skeleton.ScaleX = 1;
					transform.Translate(moveSpeed * Time.deltaTime, 0, 0);
				} else if (Input.GetKey(leftKey)) {
					skeletonAnimation.AnimationName = moveAnimation;
					skeletonAnimation.Skeleton.ScaleX = -1;
					transform.Translate(-moveSpeed * Time.deltaTime, 0, 0);
				} else {
					skeletonAnimation.AnimationName = idleAnimation;
				}
			}
		}

		IEnumerator Blink () {
			while (true) {
				yield return new WaitForSeconds(Random.Range(0.25f, 3f));
				skeletonAnimation.Skeleton.SetAttachment(eyesSlot, blinkAttachment);
				yield return new WaitForSeconds(blinkDuration);
				skeletonAnimation.Skeleton.SetAttachment(eyesSlot, eyesOpenAttachment);
			}
		}
	}
}
