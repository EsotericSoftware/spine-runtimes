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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Examples {
	public class SpineboyFacialExpression : MonoBehaviour {

		public SpineboyFootplanter footPlanter;

		[SpineSlot]
		public string eyeSlotName, mouthSlotName;

		[SpineAttachment(slotField: "eyeSlotName")]
		public string shockEyeName, normalEyeName;

		[SpineAttachment(slotField: "mouthSlotName")]
		public string shockMouthName, normalMouthName;

		public Slot eyeSlot, mouthSlot;
		public Attachment shockEye, normalEye, shockMouth, normalMouth;

		public float balanceThreshold = 2.5f;
		public float shockDuration = 1f;

		[Header("Debug")]
		public float shockTimer = 0f;

		void Start () {
			SkeletonAnimation skeletonAnimation = GetComponent<SkeletonAnimation>();
			Skeleton skeleton = skeletonAnimation.Skeleton;
			eyeSlot = skeleton.FindSlot(eyeSlotName);
			mouthSlot = skeleton.FindSlot(mouthSlotName);

			int eyeSlotIndex = skeleton.Data.FindSlot(eyeSlotName).Index;
			shockEye = skeleton.GetAttachment(eyeSlotIndex, shockEyeName);
			normalEye = skeleton.GetAttachment(eyeSlotIndex, normalEyeName);

			int mouthSlotIndex = skeleton.Data.FindSlot(mouthSlotName).Index;
			shockMouth = skeleton.GetAttachment(mouthSlotIndex, shockMouthName);
			normalMouth = skeleton.GetAttachment(mouthSlotIndex, normalMouthName);
		}

		void Update () {
			if (Mathf.Abs(footPlanter.Balance) > balanceThreshold)
				shockTimer = shockDuration;

			if (shockTimer > 0)
				shockTimer -= Time.deltaTime;

			if (shockTimer > 0) {
				eyeSlot.Attachment = shockEye;
				mouthSlot.Attachment = shockMouth;
			} else {
				eyeSlot.Attachment = normalEye;
				mouthSlot.Attachment = normalMouth;
			}
		}
	}

}
