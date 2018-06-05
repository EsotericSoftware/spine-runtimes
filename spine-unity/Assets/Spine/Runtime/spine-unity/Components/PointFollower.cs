/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/
 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity {

	[ExecuteInEditMode]
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
				var skeleton = skeletonRenderer.skeleton;

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
				localScale.y = Mathf.Abs(localScale.y) * (bone.skeleton.flipX ^ bone.skeleton.flipY ? -1f : 1f);
				thisTransform.localScale = localScale;
			}
		}
	}
}

