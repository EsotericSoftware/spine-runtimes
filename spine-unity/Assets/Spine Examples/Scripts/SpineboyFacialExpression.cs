using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Examples {
	public class SpineboyFacialExpression : MonoBehaviour {

		public SpineboyFootplanter footPlanter;

		[SpineSlot]
		public string eyeSlotName, mouthSlotName;

		[SpineAttachment(slotField:"eyeSlotName")]
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
			var skeletonAnimation = GetComponent<SkeletonAnimation>();
			var skeleton = skeletonAnimation.Skeleton;
			eyeSlot = skeleton.FindSlot(eyeSlotName);
			mouthSlot = skeleton.FindSlot(mouthSlotName);

			int eyeSlotIndex = skeleton.FindSlotIndex(eyeSlotName);
			shockEye = skeleton.GetAttachment(eyeSlotIndex, shockEyeName);
			normalEye = skeleton.GetAttachment(eyeSlotIndex, normalEyeName);

			int mouthSlotIndex = skeleton.FindSlotIndex(mouthSlotName);
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
