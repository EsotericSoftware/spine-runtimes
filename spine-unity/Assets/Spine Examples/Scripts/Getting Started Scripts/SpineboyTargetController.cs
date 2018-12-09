using UnityEngine;

namespace Spine.Unity.Examples {
	public class SpineboyTargetController : MonoBehaviour {

		public SkeletonAnimation skeletonAnimation;

		[SpineBone(dataField:"skeletonAnimation")]
		public string boneName;
		public new Camera camera;

		Bone bone;

		void OnValidate () {
			if (skeletonAnimation == null) skeletonAnimation = GetComponent<SkeletonAnimation>();
		}

		void Start () {
			bone = skeletonAnimation.Skeleton.FindBone(boneName);
		}

		void Update () {
			var mousePosition = Input.mousePosition;
			var worldMousePosition = camera.ScreenToWorldPoint(mousePosition);
			var skeletonSpacePoint = skeletonAnimation.transform.InverseTransformPoint(worldMousePosition);
			skeletonSpacePoint.x *= skeletonAnimation.Skeleton.ScaleX;
			skeletonSpacePoint.y *= skeletonAnimation.Skeleton.ScaleY;
			bone.SetLocalPosition(skeletonSpacePoint);
		}
	}

}
