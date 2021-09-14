/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

namespace Spine.Unity {
	/// <summary>Sets a GameObject's transform to match a bone on a Spine skeleton.</summary>
#if NEW_PREFAB_SYSTEM
	[ExecuteAlways]
#else
	[ExecuteInEditMode]
#endif
	[AddComponentMenu("Spine/SkeletonUtilityBone")]
	[HelpURL("http://esotericsoftware.com/spine-unity#SkeletonUtilityBone")]
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
		public Mode mode;
		public bool position, rotation, scale, zPosition = true;
		[Range(0f, 1f)]
		public float overrideAlpha = 1;
		#endregion

		public SkeletonUtility hierarchy;
		[System.NonSerialized] public Bone bone;
		[System.NonSerialized] public bool transformLerpComplete;
		[System.NonSerialized] public bool valid;
		Transform cachedTransform;
		Transform skeletonTransform;
		bool incompatibleTransformMode;
		public bool IncompatibleTransformMode { get { return incompatibleTransformMode; } }

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

			var skeleton = hierarchy.Skeleton;

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

			var thisTransform = cachedTransform;
			float skeletonFlipRotation = Mathf.Sign(skeleton.ScaleX * skeleton.ScaleY);
			if (mode == Mode.Follow) {
				switch (phase) {
				case UpdatePhase.Local:
					if (position)
						thisTransform.localPosition = new Vector3(bone.X * positionScale, bone.Y * positionScale, 0);

					if (rotation) {
						if (bone.Data.TransformMode.InheritsRotation()) {
							thisTransform.localRotation = Quaternion.Euler(0, 0, bone.Rotation);
						} else {
							Vector3 euler = skeletonTransform.rotation.eulerAngles;
							thisTransform.rotation = Quaternion.Euler(euler.x, euler.y, euler.z + (bone.WorldRotationX * skeletonFlipRotation));
						}
					}

					if (scale) {
						thisTransform.localScale = new Vector3(bone.ScaleX, bone.ScaleY, 1f);
						incompatibleTransformMode = BoneTransformModeIncompatible(bone);
					}
					break;
				case UpdatePhase.World:
				case UpdatePhase.Complete:
					if (position)
						thisTransform.localPosition = new Vector3(bone.AX * positionScale, bone.AY * positionScale, 0);

					if (rotation) {
						if (bone.Data.TransformMode.InheritsRotation()) {
							thisTransform.localRotation = Quaternion.Euler(0, 0, bone.AppliedRotation);
						} else {
							Vector3 euler = skeletonTransform.rotation.eulerAngles;
							thisTransform.rotation = Quaternion.Euler(euler.x, euler.y, euler.z + (bone.WorldRotationX * skeletonFlipRotation));
						}
					}

					if (scale) {
						thisTransform.localScale = new Vector3(bone.AScaleX, bone.AScaleY, 1f);
						incompatibleTransformMode = BoneTransformModeIncompatible(bone);
					}
					break;
				}

			} else if (mode == Mode.Override) {
				if (transformLerpComplete)
					return;

				if (parentReference == null) {
					if (position) {
						Vector3 clp = thisTransform.localPosition / positionScale;
						bone.X = Mathf.Lerp(bone.X, clp.x, overrideAlpha);
						bone.Y = Mathf.Lerp(bone.Y, clp.y, overrideAlpha);
					}

					if (rotation) {
						float angle = Mathf.LerpAngle(bone.Rotation, thisTransform.localRotation.eulerAngles.z, overrideAlpha);
						bone.Rotation = angle;
						bone.AppliedRotation = angle;
					}

					if (scale) {
						Vector3 cls = thisTransform.localScale;
						bone.ScaleX = Mathf.Lerp(bone.ScaleX, cls.x, overrideAlpha);
						bone.ScaleY = Mathf.Lerp(bone.ScaleY, cls.y, overrideAlpha);
					}

				} else {
					if (transformLerpComplete)
						return;

					if (position) {
						Vector3 pos = parentReference.InverseTransformPoint(thisTransform.position) / positionScale;
						bone.X = Mathf.Lerp(bone.X, pos.x, overrideAlpha);
						bone.Y = Mathf.Lerp(bone.Y, pos.y, overrideAlpha);
					}

					if (rotation) {
						float angle = Mathf.LerpAngle(bone.Rotation, Quaternion.LookRotation(Vector3.forward, parentReference.InverseTransformDirection(thisTransform.up)).eulerAngles.z, overrideAlpha);
						bone.Rotation = angle;
						bone.AppliedRotation = angle;
					}

					if (scale) {
						Vector3 cls = thisTransform.localScale;
						bone.ScaleX = Mathf.Lerp(bone.ScaleX, cls.x, overrideAlpha);
						bone.ScaleY = Mathf.Lerp(bone.ScaleY, cls.y, overrideAlpha);
					}

					incompatibleTransformMode = BoneTransformModeIncompatible(bone);
				}

				transformLerpComplete = true;
			}
		}

		public static bool BoneTransformModeIncompatible (Bone bone) {
			return !bone.Data.TransformMode.InheritsScale();
		}

		public void AddBoundingBox (string skinName, string slotName, string attachmentName) {
			SkeletonUtility.AddBoneRigidbody2D(transform.gameObject);
			SkeletonUtility.AddBoundingBoxGameObject(bone.Skeleton, skinName, slotName, attachmentName, transform);
		}

#if UNITY_EDITOR
		void OnDrawGizmos () {
			if (IncompatibleTransformMode)
				Gizmos.DrawIcon(transform.position + new Vector3(0, 0.128f, 0), "icon-warning");
		}
#endif
	}
}
