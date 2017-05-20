using UnityEngine;
using Spine;
using Spine.Unity;

namespace Spine.Unity.Examples {	
	public class BoneLocalOverride : MonoBehaviour {
		[SpineBone]
		public string boneName;

		[Space]
		[Range(0, 1)] public float alpha = 1;

		[Space]
		public bool overridePosition = true;
		public Vector2 localPosition;

		[Space]
		public bool overrideRotation = true;
		[Range(0, 360)] public float rotation = 0;

		ISkeletonAnimation spineComponent;
		Bone bone;

		#if UNITY_EDITOR
		void OnValidate () {
			if (Application.isPlaying) return;
			spineComponent = spineComponent ?? GetComponent<ISkeletonAnimation>();
			if (spineComponent == null) return;
			if (bone != null) bone.SetToSetupPose();
			OverrideLocal(spineComponent);
		}
		#endif

		void Awake () {
			spineComponent = GetComponent<ISkeletonAnimation>();
			if (spineComponent == null) { this.enabled = false; return; }
			spineComponent.UpdateLocal += OverrideLocal;

			if (bone == null) {	this.enabled = false; return; }
		}

		void OverrideLocal (ISkeletonAnimation animated) {
			if (bone == null || bone.Data.Name != boneName) {
				if (string.IsNullOrEmpty(boneName)) return;
				bone = spineComponent.Skeleton.FindBone(boneName);
				if (bone == null) {
					Debug.LogFormat("Cannot find bone: '{0}'", boneName);
					return;
				}
			}

			if (overridePosition) {
				bone.X = Mathf.Lerp(bone.X, localPosition.x, alpha);
				bone.Y = Mathf.Lerp(bone.Y, localPosition.y, alpha);
			}

			if (overrideRotation)
				bone.Rotation = Mathf.Lerp(bone.Rotation, rotation, alpha);
		}

	}
}

