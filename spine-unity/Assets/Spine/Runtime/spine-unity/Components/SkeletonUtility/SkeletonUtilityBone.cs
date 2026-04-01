/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2026, Esoteric Software LLC
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

#if UNITY_2018_3 || UNITY_2019 || UNITY_2018_3_OR_NEWER
#define NEW_PREFAB_SYSTEM
#endif

using UnityEngine;
using UnityEngine.Serialization;

namespace Spine.Unity {
	/// <summary>Sets a GameObject's transform to match a bone on a Spine skeleton.</summary>
#if NEW_PREFAB_SYSTEM
	[ExecuteAlways]
#else
	[ExecuteInEditMode]
#endif
	[AddComponentMenu("Spine/SkeletonUtilityBone")]
	[HelpURL("https://esotericsoftware.com/spine-unity-utility-components#SkeletonUtilityBone")]
	public class SkeletonUtilityBone : MonoBehaviour {
		public enum Mode {
			Follow,
			Override
		}

		public enum UpdatePhase {
			Local,
			World,
			Complete
		}

		#region Inspector
		/// <summary>If a bone isn't set, boneName is used to find the bone.</summary>
		public string boneName;
		public Transform parentReference;
		[SerializeField, FormerlySerializedAs("mode")] Mode boneMode;
		public Mode mode {
			get { return boneMode; }
			set {
				if (boneMode != value) {
					boneMode = value;
					if (hierarchy != null)
						hierarchy.OnUtilityBoneChanged();
				}
			}
		}
		public bool position, rotation, scale, zPosition = true;
		[Range(0f, 1f)]
		public float overrideAlpha = 1;
		#endregion

		public SkeletonUtility hierarchy;
		[System.NonSerialized] public Bone bone;
		[System.NonSerialized] public bool valid;
		Transform cachedTransform;
		Transform skeletonTransform;

		Vector3 TransformLocalPosition { get { return cachedTransform.localPosition; } }
		Quaternion TransformLocalRotation { get { return cachedTransform.localRotation; } }
		Vector3 TransformLocalScale { get { return cachedTransform.localScale; } }

#if UNITY_EDITOR
		bool incompatibleTransformMode;
		public bool IncompatibleTransformMode { get { return incompatibleTransformMode; } }
#endif

		public void Reset () {
			bone = null;
			cachedTransform = transform;
			valid = hierarchy != null && hierarchy.IsValid;
			if (!valid)
				return;
			skeletonTransform = hierarchy.transform;
			hierarchy.OnReset -= HandleOnReset;
			hierarchy.OnReset += HandleOnReset;
			DoUpdate(UpdatePhase.Local);
		}

		void OnEnable () {
			if (hierarchy == null) hierarchy = transform.GetComponentInParent<SkeletonUtility>();
			if (hierarchy == null) return;

			hierarchy.RegisterBone(this);
			hierarchy.OnReset += HandleOnReset;
		}

		void HandleOnReset () {
			Reset();
		}

		void OnDisable () {
			if (hierarchy != null) {
				hierarchy.OnReset -= HandleOnReset;
				hierarchy.UnregisterBone(this);
			}
		}

		public void DoUpdate (UpdatePhase phase) {
			if (!valid) {
				Reset();
				return;
			}

			Skeleton skeleton = hierarchy.Skeleton;

			if (bone == null) {
				if (string.IsNullOrEmpty(boneName)) return;
				bone = skeleton.FindBone(boneName);
				if (bone == null) {
					Debug.LogError("Bone not found: " + boneName, this);
					return;
				}
			}
			if (!bone.Active) return;

			float positionScale = hierarchy.PositionScale;

			Transform thisTransform = cachedTransform;
			float skeletonFlipRotation = Mathf.Sign(skeleton.ScaleX * skeleton.ScaleY);
			if (mode == Mode.Follow) {
				var bonePose = bone.Pose;
				switch (phase) {
				case UpdatePhase.Local:
					if (position)
						thisTransform.localPosition = new Vector3(bonePose.X * positionScale, bonePose.Y * positionScale,
							zPosition ? 0 : thisTransform.localPosition.z);

					if (rotation) {
						if (bone.Data.GetSetupPose().Inherit.InheritsRotation()) {
							thisTransform.localRotation = Quaternion.Euler(0, 0, bonePose.Rotation);
						} else {
							Vector3 euler = skeletonTransform.rotation.eulerAngles;
							thisTransform.rotation = Quaternion.Euler(euler.x, euler.y, euler.z + (bone.AppliedPose.WorldRotationX * skeletonFlipRotation));
						}
					}

					if (scale) {
						thisTransform.localScale = new Vector3(bonePose.ScaleX, bonePose.ScaleY, 1f);
#if UNITY_EDITOR
						incompatibleTransformMode = BoneTransformModeIncompatible(bone);
#endif
					}
					break;
				case UpdatePhase.World:
				case UpdatePhase.Complete:
					var appliedPose = bone.AppliedPose;
					appliedPose.ValidateLocalTransform(skeleton);
					if (position)
						thisTransform.localPosition = new Vector3(appliedPose.X * positionScale, appliedPose.Y * positionScale,
							zPosition ? 0 : thisTransform.localPosition.z);

					if (rotation) {
						if (bone.Data.GetSetupPose().Inherit.InheritsRotation()) {
							thisTransform.localRotation = Quaternion.Euler(0, 0, appliedPose.Rotation);
						} else {
							Vector3 euler = skeletonTransform.rotation.eulerAngles;
							thisTransform.rotation = Quaternion.Euler(euler.x, euler.y, euler.z + (appliedPose.WorldRotationX * skeletonFlipRotation));
						}
					}

					if (scale) {
						thisTransform.localScale = new Vector3(appliedPose.ScaleX, appliedPose.ScaleY, 1f);
#if UNITY_EDITOR
						incompatibleTransformMode = BoneTransformModeIncompatible(bone);
#endif
					}
					break;
				}
			} else if (mode == Mode.Override) {
				if (phase != UpdatePhase.Local)
					return;
				var bonePose = bone.Pose;
				if (parentReference == null) {
					if (position) {
						Vector3 clp = TransformLocalPosition / positionScale;
						bonePose.X = Mathf.Lerp(bonePose.X, clp.x, overrideAlpha);
						bonePose.Y = Mathf.Lerp(bonePose.Y, clp.y, overrideAlpha);
					}

					if (rotation) {
						float angle = Mathf.LerpAngle(bonePose.Rotation, TransformLocalRotation.eulerAngles.z, overrideAlpha);
						bonePose.Rotation = angle;
						bone.AppliedPose.Rotation = angle;
					}

					if (scale) {
						Vector3 cls = TransformLocalScale;
						bonePose.ScaleX = Mathf.Lerp(bonePose.ScaleX, cls.x, overrideAlpha);
						bonePose.ScaleY = Mathf.Lerp(bonePose.ScaleY, cls.y, overrideAlpha);
					}

				} else {
					if (position) {
						Vector3 pos = parentReference.InverseTransformPoint(thisTransform.position) / positionScale;
						bonePose.X = Mathf.Lerp(bonePose.X, pos.x, overrideAlpha);
						bonePose.Y = Mathf.Lerp(bonePose.Y, pos.y, overrideAlpha);
					}

					if (rotation) {
						float angle = Mathf.LerpAngle(bonePose.Rotation, Quaternion.LookRotation(Vector3.forward, parentReference.InverseTransformDirection(thisTransform.up)).eulerAngles.z, overrideAlpha);
						bonePose.Rotation = angle;
						bone.AppliedPose.Rotation = angle;
					}

					if (scale) {
						Vector3 cls = TransformLocalScale;
						bonePose.ScaleX = Mathf.Lerp(bonePose.ScaleX, cls.x, overrideAlpha);
						bonePose.ScaleY = Mathf.Lerp(bonePose.ScaleY, cls.y, overrideAlpha);
					}
#if UNITY_EDITOR
					incompatibleTransformMode = BoneTransformModeIncompatible(bone);
#endif
				}
			}
		}

		public static bool BoneTransformModeIncompatible (Bone bone) {
			return !bone.Data.GetSetupPose().Inherit.InheritsScale();
		}

		public void AddBoundingBox (Skeleton skeleton, string skinName, string slotName, string attachmentName) {
			SkeletonUtility.AddBoneRigidbody2D(transform.gameObject);
			SkeletonUtility.AddBoundingBoxGameObject(skeleton, skinName, slotName, attachmentName, transform);
		}

#if UNITY_EDITOR
		void OnDrawGizmos () {
			if (IncompatibleTransformMode)
				Gizmos.DrawIcon(transform.position + new Vector3(0, 0.128f, 0), "icon-warning");
		}
#endif
	}
}
