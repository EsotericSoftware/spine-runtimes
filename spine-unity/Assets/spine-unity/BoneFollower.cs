/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/
using System;
using UnityEngine;

namespace Spine.Unity {
	/// <summary>Sets a GameObject's transform to match a bone on a Spine skeleton.</summary>
	[ExecuteInEditMode]
	[AddComponentMenu("Spine/BoneFollower")]
	public class BoneFollower : MonoBehaviour {

		#region Inspector
		public SkeletonRenderer skeletonRenderer;
		public SkeletonRenderer SkeletonRenderer {
			get { return skeletonRenderer; }
			set {
				skeletonRenderer = value;
				Initialize();
			}
		}

		/// <summary>If a bone isn't set, boneName is used to find the bone.</summary>
		[SpineBone(dataField: "skeletonRenderer")]
		public String boneName;

		public bool followZPosition = true;
		public bool followBoneRotation = true;
		[UnityEngine.Serialization.FormerlySerializedAs("resetOnAwake")]
		public bool initializeOnAwake = true;
		#endregion

		[NonSerialized] public bool valid;
		[NonSerialized] public Bone bone;
		Transform skeletonTransform;

		public void Awake () {
			if (initializeOnAwake) Initialize();
		}

		public void HandleRebuildRenderer (SkeletonRenderer skeletonRenderer) {
			Initialize();
		}

		public void Initialize () {
			bone = null;
			valid = skeletonRenderer != null && skeletonRenderer.valid;
			if (!valid) return;

			skeletonTransform = skeletonRenderer.transform;
			skeletonRenderer.OnRebuild -= HandleRebuildRenderer;
			skeletonRenderer.OnRebuild += HandleRebuildRenderer;

			#if UNITY_EDITOR
			if (Application.isEditor)
				LateUpdate();
			#endif
		}

		void OnDestroy () {
			if (skeletonRenderer != null)
				skeletonRenderer.OnRebuild -= HandleRebuildRenderer;
		}

		public void LateUpdate () {
			if (!valid) {
				Initialize();
				return;
			}

			if (bone == null) {
				if (string.IsNullOrEmpty(boneName)) return;
				
				bone = skeletonRenderer.skeleton.FindBone(boneName);
				if (bone == null) {
					Debug.LogError("Bone not found: " + boneName, this);
					return;
				}
			}

			Transform thisTransform = this.transform;
			if (thisTransform.parent == skeletonTransform) {
				// Recommended setup: Use local transform properties if Spine GameObject is the immediate parent
				thisTransform.localPosition = new Vector3(bone.worldX, bone.worldY, followZPosition ? 0f : thisTransform.localPosition.z);
				if (followBoneRotation) thisTransform.localRotation = Quaternion.Euler(0f, 0f, bone.WorldRotationX);
			
			} else {
				// For special cases: Use transform world properties if transform relationship is complicated
				Vector3 targetWorldPosition = skeletonTransform.TransformPoint(new Vector3(bone.worldX, bone.worldY, 0f));
				if (!followZPosition) targetWorldPosition.z = thisTransform.position.z;
				thisTransform.position = targetWorldPosition;

				if (followBoneRotation) {
					Vector3 worldRotation = skeletonTransform.rotation.eulerAngles;
					thisTransform.rotation = Quaternion.Euler(worldRotation.x, worldRotation.y, skeletonTransform.rotation.eulerAngles.z + bone.WorldRotationX);
				}
			}
		}
	}

}
