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

	#if NEW_PREFAB_SYSTEM
	[ExecuteAlways]
	#else
	[ExecuteInEditMode]
	#endif
	[AddComponentMenu("Spine/Point Follower")]
	public class PointFollower : MonoBehaviour, IHasSkeletonRenderer, IHasSkeletonComponent {

		[SerializeField] public SkeletonRenderer skeletonRenderer;
		public SkeletonRenderer SkeletonRenderer { get { return this.skeletonRenderer; } }
		public ISkeletonComponent SkeletonComponent { get { return skeletonRenderer as ISkeletonComponent; } }

		[SpineSlot(dataField:"skeletonRenderer", includeNone: true)]
		public string slotName;

		[SpineAttachment(slotField:"slotName", dataField: "skeletonRenderer", fallbackToTextField:true, includeNone: true)]
		public string pointAttachmentName;

		public bool followRotation = true;
		public bool followSkeletonFlip = true;
		public bool followSkeletonZPosition = false;

		Transform skeletonTransform;
		bool skeletonTransformIsParent;
		PointAttachment point;
		Bone bone;
		bool valid;
		public bool IsValid { get { return valid; } }

		public void Initialize () {
			valid = skeletonRenderer != null && skeletonRenderer.valid;
			if (!valid)
				return;

			UpdateReferences();

			#if UNITY_EDITOR
			if (Application.isEditor) LateUpdate();
			#endif
		}

		private void HandleRebuildRenderer (SkeletonRenderer skeletonRenderer) {
			Initialize();
		}

		void UpdateReferences () {
			skeletonTransform = skeletonRenderer.transform;
			skeletonRenderer.OnRebuild -= HandleRebuildRenderer;
			skeletonRenderer.OnRebuild += HandleRebuildRenderer;
			skeletonTransformIsParent = Transform.ReferenceEquals(skeletonTransform, transform.parent);

			bone = null;
			point = null;
			if (!string.IsNullOrEmpty(pointAttachmentName)) {
				var skeleton = skeletonRenderer.Skeleton;

				int slotIndex = skeleton.FindSlotIndex(slotName);
				if (slotIndex >= 0) {
					var slot = skeleton.slots.Items[slotIndex];
					bone = slot.bone;
					point = skeleton.GetAttachment(slotIndex, pointAttachmentName) as PointAttachment;
				}
			}
		}

		public void LateUpdate () {
			#if UNITY_EDITOR
			if (!Application.isPlaying) skeletonTransformIsParent = Transform.ReferenceEquals(skeletonTransform, transform.parent);
			#endif

			if (point == null) {
				if (string.IsNullOrEmpty(pointAttachmentName)) return;
				UpdateReferences();
				if (point == null) return;
			}

			Vector2 worldPos;
			point.ComputeWorldPosition(bone, out worldPos.x, out worldPos.y);
			float rotation = point.ComputeWorldRotation(bone);

			Transform thisTransform = this.transform;
			if (skeletonTransformIsParent) {
				// Recommended setup: Use local transform properties if Spine GameObject is the immediate parent
				thisTransform.localPosition = new Vector3(worldPos.x, worldPos.y, followSkeletonZPosition ? 0f : thisTransform.localPosition.z);
				if (followRotation) {
					float halfRotation = rotation * 0.5f * Mathf.Deg2Rad;

					var q = default(Quaternion);
					q.z = Mathf.Sin(halfRotation);
					q.w = Mathf.Cos(halfRotation);
					thisTransform.localRotation = q;
				}
			} else {
				// For special cases: Use transform world properties if transform relationship is complicated
				Vector3 targetWorldPosition = skeletonTransform.TransformPoint(new Vector3(worldPos.x, worldPos.y, 0f));
				if (!followSkeletonZPosition)
					targetWorldPosition.z = thisTransform.position.z;

				Transform transformParent = thisTransform.parent;
				if (transformParent != null) {
					Matrix4x4 m = transformParent.localToWorldMatrix;
					if (m.m00 * m.m11 - m.m01 * m.m10 < 0) // Determinant2D is negative
						rotation = -rotation;
				}

				if (followRotation) {
					Vector3 transformWorldRotation = skeletonTransform.rotation.eulerAngles;
					thisTransform.SetPositionAndRotation(targetWorldPosition, Quaternion.Euler(transformWorldRotation.x, transformWorldRotation.y, transformWorldRotation.z + rotation));
				} else {
					thisTransform.position = targetWorldPosition;
				}
			}

			if (followSkeletonFlip) {
				Vector3 localScale = thisTransform.localScale;
				localScale.y = Mathf.Abs(localScale.y) * Mathf.Sign(bone.skeleton.ScaleX * bone.skeleton.ScaleY);
				thisTransform.localScale = localScale;
			}
		}
	}
}
