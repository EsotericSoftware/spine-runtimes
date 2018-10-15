using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Examples {
	public class SpineboyBodyTilt : MonoBehaviour {

		[Header("Settings")]
		public SpineboyFootplanter planter;

		[SpineBone]
		public string hip = "hip", head = "head";
		public float hipTiltScale = 7;
		public float headTiltScale = 0.7f;
		public float hipRotationMoveScale = 60f;

		[Header("Debug")]
		public float hipRotationTarget;
		public float hipRotationSmoothed;
		public float baseHeadRotation;

		Bone hipBone, headBone;

		void Start () {
			var skeletonAnimation = GetComponent<SkeletonAnimation>();
			var skeleton = skeletonAnimation.Skeleton;

			hipBone = skeleton.FindBone(hip);
			headBone = skeleton.FindBone(head);
			baseHeadRotation = headBone.Rotation;

			skeletonAnimation.UpdateLocal += UpdateLocal;
		}

		private void UpdateLocal (ISkeletonAnimation animated) {
			hipRotationTarget = planter.Balance * hipTiltScale;
			hipRotationSmoothed = Mathf.MoveTowards(hipRotationSmoothed, hipRotationTarget, Time.deltaTime * hipRotationMoveScale * Mathf.Abs(2f * planter.Balance / planter.offBalanceThreshold));
			hipBone.Rotation = hipRotationSmoothed;
			headBone.Rotation = baseHeadRotation + (-hipRotationSmoothed * headTiltScale);
		}
	}

}
