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

#if UNITY_2018_3 || UNITY_2019 || UNITY_2018_3_OR_NEWER
#define NEW_PREFAB_SYSTEM
#endif

using UnityEngine;
using System.Collections.Generic;

namespace Spine.Unity {

	#if NEW_PREFAB_SYSTEM
	[ExecuteAlways]
	#else
	[ExecuteInEditMode]
	#endif
	[RequireComponent(typeof(ISkeletonAnimation))]
	public sealed class SkeletonUtility : MonoBehaviour {

		#region BoundingBoxAttachment
		public static PolygonCollider2D AddBoundingBoxGameObject (Skeleton skeleton, string skinName, string slotName, string attachmentName, Transform parent, bool isTrigger = true) {
			Skin skin = string.IsNullOrEmpty(skinName) ? skeleton.data.defaultSkin : skeleton.data.FindSkin(skinName);
			if (skin == null) {
				Debug.LogError("Skin " + skinName + " not found!");
				return null;
			}

			var attachment = skin.GetAttachment(skeleton.FindSlotIndex(slotName), attachmentName);
			if (attachment == null) {
				Debug.LogFormat("Attachment in slot '{0}' named '{1}' not found in skin '{2}'.", slotName, attachmentName, skin.name);
				return null;
			}

			var box = attachment as BoundingBoxAttachment;
			if (box != null) {
				var slot = skeleton.FindSlot(slotName);
				return AddBoundingBoxGameObject(box.Name, box, slot, parent, isTrigger);
			} else {
				Debug.LogFormat("Attachment '{0}' was not a Bounding Box.", attachmentName);
				return null;
			}
		}

		public static PolygonCollider2D AddBoundingBoxGameObject (string name, BoundingBoxAttachment box, Slot slot, Transform parent, bool isTrigger = true) {
			var go = new GameObject("[BoundingBox]" + (string.IsNullOrEmpty(name) ? box.Name : name));
			var got = go.transform;
			got.parent = parent;
			got.localPosition = Vector3.zero;
			got.localRotation = Quaternion.identity;
			got.localScale = Vector3.one;
			return AddBoundingBoxAsComponent(box, slot, go, isTrigger);
		}

		public static PolygonCollider2D AddBoundingBoxAsComponent (BoundingBoxAttachment box, Slot slot, GameObject gameObject, bool isTrigger = true) {
			if (box == null) return null;
			var collider = gameObject.AddComponent<PolygonCollider2D>();
			collider.isTrigger = isTrigger;
			SetColliderPointsLocal(collider, slot, box);
			return collider;
		}

		public static void SetColliderPointsLocal (PolygonCollider2D collider, Slot slot, BoundingBoxAttachment box) {
			if (box == null) return;
			if (box.IsWeighted()) Debug.LogWarning("UnityEngine.PolygonCollider2D does not support weighted or animated points. Collider points will not be animated and may have incorrect orientation. If you want to use it as a collider, please remove weights and animations from the bounding box in Spine editor.");
			var verts = box.GetLocalVertices(slot, null);
			collider.SetPath(0, verts);
		}

		public static Bounds GetBoundingBoxBounds (BoundingBoxAttachment boundingBox, float depth = 0) {
			float[] floats = boundingBox.Vertices;
			int floatCount = floats.Length;

			Bounds bounds = new Bounds();

			bounds.center = new Vector3(floats[0], floats[1], 0);
			for (int i = 2; i < floatCount; i += 2)
				bounds.Encapsulate(new Vector3(floats[i], floats[i + 1], 0));

			Vector3 size = bounds.size;
			size.z = depth;
			bounds.size = size;

			return bounds;
		}

		public static Rigidbody2D AddBoneRigidbody2D (GameObject gameObject, bool isKinematic = true, float gravityScale = 0f) {
			var rb = gameObject.GetComponent<Rigidbody2D>();
			if (rb == null) {
				rb = gameObject.AddComponent<Rigidbody2D>();
				rb.isKinematic = isKinematic;
				rb.gravityScale = gravityScale;
			}
			return rb;
		}
		#endregion

		public delegate void SkeletonUtilityDelegate ();
		public event SkeletonUtilityDelegate OnReset;
		public Transform boneRoot;

		void Update () {
			var skeleton = skeletonRenderer.skeleton;
			if (skeleton != null && boneRoot != null) {
				boneRoot.localScale = new Vector3(skeleton.scaleX, skeleton.scaleY, 1f);
			}
		}

		[HideInInspector] public SkeletonRenderer skeletonRenderer;
		[HideInInspector] public ISkeletonAnimation skeletonAnimation;
		[System.NonSerialized] public List<SkeletonUtilityBone> boneComponents = new List<SkeletonUtilityBone>();
		[System.NonSerialized] public List<SkeletonUtilityConstraint> constraintComponents = new List<SkeletonUtilityConstraint>();

		bool hasOverrideBones;
		bool hasConstraints;
		bool needToReprocessBones;

		public void ResubscribeEvents () {
			OnDisable();
			OnEnable();
		}
		
		void OnEnable () {
			if (skeletonRenderer == null) {
				skeletonRenderer = GetComponent<SkeletonRenderer>();
			}

			if (skeletonAnimation == null) {
				skeletonAnimation = GetComponent(typeof(ISkeletonAnimation)) as ISkeletonAnimation;
			}

			skeletonRenderer.OnRebuild -= HandleRendererReset;
			skeletonRenderer.OnRebuild += HandleRendererReset;

			if (skeletonAnimation != null) {
				skeletonAnimation.UpdateLocal -= UpdateLocal;
				skeletonAnimation.UpdateLocal += UpdateLocal;
			}

			CollectBones();
		}

		void Start () {
			//recollect because order of operations failure when switching between game mode and edit mode...
			CollectBones();
		}

		void OnDisable () {
			skeletonRenderer.OnRebuild -= HandleRendererReset;

			if (skeletonAnimation != null) {
				skeletonAnimation.UpdateLocal -= UpdateLocal;
				skeletonAnimation.UpdateWorld -= UpdateWorld;
				skeletonAnimation.UpdateComplete -= UpdateComplete;
			}
		}

		void HandleRendererReset (SkeletonRenderer r) {
			if (OnReset != null) OnReset();
			CollectBones();
		}

		public void RegisterBone (SkeletonUtilityBone bone) {
			if (boneComponents.Contains(bone)) {
				return;
			} else {
				boneComponents.Add(bone);
				needToReprocessBones = true;
			}
		}

		public void UnregisterBone (SkeletonUtilityBone bone) {
			boneComponents.Remove(bone);
		}

		public void RegisterConstraint (SkeletonUtilityConstraint constraint) {
			if (constraintComponents.Contains(constraint))
				return;
			else {
				constraintComponents.Add(constraint);
				needToReprocessBones = true;
			}
		}

		public void UnregisterConstraint (SkeletonUtilityConstraint constraint) {
			constraintComponents.Remove(constraint);
		}

		public void CollectBones () {
			var skeleton = skeletonRenderer.skeleton;
			if (skeleton == null) return;

			if (boneRoot != null) {
				var constraintTargets = new List<System.Object>();
				var ikConstraints = skeleton.IkConstraints;
				for (int i = 0, n = ikConstraints.Count; i < n; i++)
					constraintTargets.Add(ikConstraints.Items[i].target);

				var transformConstraints = skeleton.TransformConstraints;
				for (int i = 0, n = transformConstraints.Count; i < n; i++)
					constraintTargets.Add(transformConstraints.Items[i].target);

				var boneComponents = this.boneComponents;
				for (int i = 0, n = boneComponents.Count; i < n; i++) {
					var b = boneComponents[i];
					if (b.bone == null) continue;
					hasOverrideBones |= (b.mode == SkeletonUtilityBone.Mode.Override);
					hasConstraints |= constraintTargets.Contains(b.bone);
				}

				hasConstraints |= constraintComponents.Count > 0;

				if (skeletonAnimation != null) {
					skeletonAnimation.UpdateWorld -= UpdateWorld;
					skeletonAnimation.UpdateComplete -= UpdateComplete;

					if (hasOverrideBones || hasConstraints)
						skeletonAnimation.UpdateWorld += UpdateWorld;

					if (hasConstraints)
						skeletonAnimation.UpdateComplete += UpdateComplete;
				}

				needToReprocessBones = false;
			} else {
				boneComponents.Clear();
				constraintComponents.Clear();
			}
		}

		void UpdateLocal (ISkeletonAnimation anim) {
			if (needToReprocessBones)
				CollectBones();

			var boneComponents = this.boneComponents;
			if (boneComponents == null) return;
			for (int i = 0, n = boneComponents.Count; i < n; i++)
				boneComponents[i].transformLerpComplete = false;

			UpdateAllBones(SkeletonUtilityBone.UpdatePhase.Local);
		}

		void UpdateWorld (ISkeletonAnimation anim) {
			UpdateAllBones(SkeletonUtilityBone.UpdatePhase.World);
			for (int i = 0, n = constraintComponents.Count; i < n; i++)
				constraintComponents[i].DoUpdate();
		}

		void UpdateComplete (ISkeletonAnimation anim) {
			UpdateAllBones(SkeletonUtilityBone.UpdatePhase.Complete);
		}

		void UpdateAllBones (SkeletonUtilityBone.UpdatePhase phase) {
			if (boneRoot == null)
				CollectBones();

			var boneComponents = this.boneComponents;
			if (boneComponents == null) return;
			for (int i = 0, n = boneComponents.Count; i < n; i++)
				boneComponents[i].DoUpdate(phase);
		}

		public Transform GetBoneRoot () {
			if (boneRoot != null)
				return boneRoot;

			boneRoot = new GameObject("SkeletonUtility-SkeletonRoot").transform;
			boneRoot.parent = transform;
			boneRoot.localPosition = Vector3.zero;
			boneRoot.localRotation = Quaternion.identity;
			boneRoot.localScale = Vector3.one;

			return boneRoot;
		}

		public GameObject SpawnRoot (SkeletonUtilityBone.Mode mode, bool pos, bool rot, bool sca) {
			GetBoneRoot();
			Skeleton skeleton = this.skeletonRenderer.skeleton;

			GameObject go = SpawnBone(skeleton.RootBone, boneRoot, mode, pos, rot, sca);
			CollectBones();
			return go;
		}

		public GameObject SpawnHierarchy (SkeletonUtilityBone.Mode mode, bool pos, bool rot, bool sca) {
			GetBoneRoot();
			Skeleton skeleton = this.skeletonRenderer.skeleton;
			GameObject go = SpawnBoneRecursively(skeleton.RootBone, boneRoot, mode, pos, rot, sca);
			CollectBones();
			return go;
		}

		public GameObject SpawnBoneRecursively (Bone bone, Transform parent, SkeletonUtilityBone.Mode mode, bool pos, bool rot, bool sca) {
			GameObject go = SpawnBone(bone, parent, mode, pos, rot, sca);

			ExposedList<Bone> childrenBones = bone.Children;
			for (int i = 0, n = childrenBones.Count; i < n; i++) {
				Bone child = childrenBones.Items[i];
				SpawnBoneRecursively(child, go.transform, mode, pos, rot, sca);
			}

			return go;
		}

		public GameObject SpawnBone (Bone bone, Transform parent, SkeletonUtilityBone.Mode mode, bool pos, bool rot, bool sca) {
			GameObject go = new GameObject(bone.Data.Name);
			var goTransform = go.transform;
			goTransform.parent = parent;

			SkeletonUtilityBone b = go.AddComponent<SkeletonUtilityBone>();
			b.hierarchy = this;
			b.position = pos;
			b.rotation = rot;
			b.scale = sca;
			b.mode = mode;
			b.zPosition = true;
			b.Reset();
			b.bone = bone;
			b.boneName = bone.Data.Name;
			b.valid = true;

			if (mode == SkeletonUtilityBone.Mode.Override) {
				if (rot) goTransform.localRotation = Quaternion.Euler(0, 0, b.bone.AppliedRotation);
				if (pos) goTransform.localPosition = new Vector3(b.bone.X, b.bone.Y, 0);
				goTransform.localScale = new Vector3(b.bone.scaleX, b.bone.scaleY, 0);
			}

			return go;
		}

	}

}
