
#if UNITY_2018_3 || UNITY_2019 || UNITY_2018_3_OR_NEWER
#define NEW_PREFAB_SYSTEM
#endif

using UnityEngine;

namespace Spine.Unity {
	
	#if NEW_PREFAB_SYSTEM
	[ExecuteAlways]
	#else
	[ExecuteInEditMode]
	#endif
	[DisallowMultipleComponent]
	[AddComponentMenu("Spine/UI/BoneFollowerGraphic")]
	public class BoneFollowerGraphic : MonoBehaviour {
		public SkeletonGraphic skeletonGraphic;
		public SkeletonGraphic SkeletonGraphic {
			get { return skeletonGraphic; }
			set {
				skeletonGraphic = value;
				Initialize();
			}
		}

		public bool initializeOnAwake = true;

		/// <summary>If a bone isn't set in code, boneName is used to find the bone at the beginning. For runtime switching by name, use SetBoneByName. You can also set the BoneFollower.bone field directly.</summary>
		[SpineBone(dataField: "skeletonGraphic")]
		[SerializeField] public string boneName;

		public bool followBoneRotation = true;
		[Tooltip("Follows the skeleton's flip state by controlling this Transform's local scale.")]
		public bool followSkeletonFlip = true;
		[Tooltip("Follows the target bone's local scale. BoneFollower cannot inherit world/skewed scale because of UnityEngine.Transform property limitations.")]
		public bool followLocalScale = false;
		public bool followZPosition = true;

		[System.NonSerialized] public Bone bone;

		Transform skeletonTransform;
		bool skeletonTransformIsParent;

		[System.NonSerialized] public bool valid;

		/// <summary>
		/// Sets the target bone by its bone name. Returns false if no bone was found.</summary>
		public bool SetBone (string name) {
			bone = skeletonGraphic.Skeleton.FindBone(name);
			if (bone == null) {
				Debug.LogError("Bone not found: " + name, this);
				return false;
			}
			boneName = name;
			return true;
		}

		public void Awake () {
			if (initializeOnAwake) Initialize();
		}

		public void Initialize () {
			bone = null;
			valid = skeletonGraphic != null && skeletonGraphic.IsValid;
			if (!valid) return;

			skeletonTransform = skeletonGraphic.transform;
//			skeletonGraphic.OnRebuild -= HandleRebuildRenderer;
//			skeletonGraphic.OnRebuild += HandleRebuildRenderer;
			skeletonTransformIsParent = Transform.ReferenceEquals(skeletonTransform, transform.parent);

			if (!string.IsNullOrEmpty(boneName))
				bone = skeletonGraphic.Skeleton.FindBone(boneName);

			#if UNITY_EDITOR
			if (Application.isEditor) {
				LateUpdate();
			}
			#endif
		}

		public void LateUpdate () {
			if (!valid) {
				Initialize();
				return;
			}

			#if UNITY_EDITOR
			if (!Application.isPlaying)
				skeletonTransformIsParent = Transform.ReferenceEquals(skeletonTransform, transform.parent);
			#endif

			if (bone == null) {
				if (string.IsNullOrEmpty(boneName)) return;
				bone = skeletonGraphic.Skeleton.FindBone(boneName);
				if (!SetBone(boneName)) return;
			}

			var thisTransform = this.transform as RectTransform;
			if (thisTransform == null) return;

			var canvas = skeletonGraphic.canvas;
			if (canvas == null) canvas = skeletonGraphic.GetComponentInParent<Canvas>();
			float scale = canvas.referencePixelsPerUnit;

			if (skeletonTransformIsParent) {
				// Recommended setup: Use local transform properties if Spine GameObject is the immediate parent
				thisTransform.localPosition = new Vector3(bone.worldX * scale, bone.worldY * scale, followZPosition ? 0f : thisTransform.localPosition.z);
				if (followBoneRotation) thisTransform.localRotation = bone.GetQuaternion();
			} else {
				// For special cases: Use transform world properties if transform relationship is complicated
				Vector3 targetWorldPosition = skeletonTransform.TransformPoint(new Vector3(bone.worldX * scale, bone.worldY * scale, 0f));
				if (!followZPosition) targetWorldPosition.z = thisTransform.position.z;

				float boneWorldRotation = bone.WorldRotationX;

				Transform transformParent = thisTransform.parent;
				if (transformParent != null) {
					Matrix4x4 m = transformParent.localToWorldMatrix;
					if (m.m00 * m.m11 - m.m01 * m.m10 < 0) // Determinant2D is negative
						boneWorldRotation = -boneWorldRotation;
				}

				if (followBoneRotation) {
					Vector3 worldRotation = skeletonTransform.rotation.eulerAngles;
					thisTransform.SetPositionAndRotation(targetWorldPosition, Quaternion.Euler(worldRotation.x, worldRotation.y, skeletonTransform.rotation.eulerAngles.z + boneWorldRotation));
				} else {
					thisTransform.position = targetWorldPosition;
				}
			}

			Vector3 localScale = followLocalScale ? new Vector3(bone.scaleX, bone.scaleY, 1f) : new Vector3(1f, 1f, 1f);
			if (followSkeletonFlip) localScale.y *= Mathf.Sign(bone.skeleton.scaleX * bone.skeleton.scaleY);
			thisTransform.localScale = localScale;
		}

	}
}
