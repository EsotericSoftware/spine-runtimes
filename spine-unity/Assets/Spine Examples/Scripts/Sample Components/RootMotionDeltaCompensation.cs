using UnityEngine;
using Spine.Unity;

namespace Spine.Unity.Examples {

	public class RootMotionDeltaCompensation : MonoBehaviour {

		protected SkeletonRootMotionBase rootMotion;
		public Transform targetPosition;
		public int trackIndex = 0;

		void Start () {
			rootMotion = this.GetComponent<SkeletonRootMotionBase>();
		}

		void Update () {
			AdjustDelta();
		}

		void OnDisable () {
			rootMotion.rootMotionScaleX = rootMotion.rootMotionScaleY = 1;
		}

		void AdjustDelta() {
			Vector3 toTarget = targetPosition.position - this.transform.position;
			rootMotion.AdjustRootMotionToDistance(toTarget, trackIndex);
		}
	}
}
