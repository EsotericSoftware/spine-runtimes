using UnityEngine;
using Spine.Unity;

namespace Spine.Unity.Examples {

	public class RootMotionDeltaCompensation : MonoBehaviour {

		[SerializeField] protected SkeletonRootMotionBase rootMotion;
		public Transform targetPosition;
		public int trackIndex = 0;
		public bool adjustX = true;
		public bool adjustY = true;
		public float minScaleX = -999;
		public float minScaleY = -999;
		public float maxScaleX = 999;
		public float maxScaleY = 999;

		public bool allowXTranslation = false;
		public bool allowYTranslation = true;

		void Start () {
			if (rootMotion == null)
				rootMotion = this.GetComponent<SkeletonRootMotionBase>();
		}

		void Update () {
			AdjustDelta();
		}

		void OnDisable () {
			if (adjustX)
				rootMotion.rootMotionScaleX = 1;
			if (adjustY)
				rootMotion.rootMotionScaleY = 1;
			if (allowXTranslation)
				rootMotion.rootMotionTranslateXPerY = 0;
			if (allowYTranslation)
				rootMotion.rootMotionTranslateYPerX = 0;
		}

		void AdjustDelta() {
			Vector3 toTarget = targetPosition.position - this.transform.position;
			rootMotion.AdjustRootMotionToDistance(toTarget, trackIndex, adjustX, adjustY,
				minScaleX, maxScaleX, minScaleY, maxScaleY,
				allowXTranslation, allowYTranslation);
		}
	}
}
