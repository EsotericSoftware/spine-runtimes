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

using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity {

#if NEW_PREFAB_SYSTEM
	[ExecuteAlways]
#else
	[ExecuteInEditMode]
#endif
	[RequireComponent(typeof(ISkeletonAnimation))]
	[HelpURL("http://esotericsoftware.com/spine-unity#SkeletonUtility")]
	public sealed class SkeletonUtility : MonoBehaviour {

		#region BoundingBoxAttachment
		public static PolygonCollider2D AddBoundingBoxGameObject (Skeleton skeleton, string skinName, string slotName, string attachmentName, Transform parent, bool isTrigger = true) {
			Skin skin = string.IsNullOrEmpty(skinName) ? skeleton.Data.DefaultSkin : skeleton.Data.FindSkin(skinName);
			if (skin == null) {
				Debug.LogError("Skin " + skinName + " not found!");
				return null;
			}

			Slot slot = skeleton.FindSlot(slotName);
			var attachment = slot != null ? skin.GetAttachment(slot.Data.Index, attachmentName) : null;
			if (attachment == null) {
				Debug.LogFormat("Attachment in slot '{0}' named '{1}' not found in skin '{2}'.", slotName, attachmentName, skin.Name);
				return null;
			}

			var box = attachment as BoundingBoxAttachment;
			if (box != null) {
				return AddBoundingBoxGameObject(box.Name, box, slot, parent, isTrigger);
			} else {
				Debug.LogFormat("Attachment '{0}' was not a Bounding Box.", attachmentName);
				return null;
			}
		}

		public static PolygonCollider2D AddBoundingBoxGameObject (string name, BoundingBoxAttachment box, Slot slot, Transform parent, bool isTrigger = true) {
			var go = new GameObject("[BoundingBox]" + (string.IsNullOrEmpty(name) ? box.Name : name));
#if UNITY_EDITOR
			if (!Application.isPlaying)
				UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Spawn BoundingBox");
#endif
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

		public static void SetColliderPointsLocal (PolygonCollider2D collider, Slot slot, BoundingBoxAttachment box, float scale = 1.0f) {
			if (box == null) return;
			if (box.IsWeighted()) Debug.LogWarning("UnityEngine.PolygonCollider2D does not support weighted or animated points. Collider points will not be animated and may have incorrect orientation. If you want to use it as a collider, please remove weights and animations from the bounding box in Spine editor.");
			var verts = box.GetLocalVertices(slot, null);
			if (scale != 1.0f) {
				for (int i = 0, n = verts.Length; i < n; ++i)
					verts[i] *= scale;
			}
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
		/// <summary>
		/// If true, <see cref="Skeleton.ScaleX"/> and <see cref="Skeleton.ScaleY"/> are followed
		/// by 180 degree rotation. If false, negative Transform scale is used.
		/// Note that using negative scale is consistent with previous behaviour (hence the default),
		/// however causes serious problems with rigidbodies and physics. Therefore, it is recommended to
		/// enable this parameter where possible. When creating hinge chains for a chain of skeleton bones
		/// via <see cref="SkeletonUtilityBone"/>, it is mandatory to have <c>flipBy180DegreeRotation</c> enabled.
		/// </summary>
		public bool flipBy180DegreeRotation = false;

		void Update () {
			var skeleton = skeletonComponent.Skeleton;
			if (skeleton != null && boneRoot != null) {

				if (flipBy180DegreeRotation) {
					boneRoot.localScale = new Vector3(Mathf.Abs(skeleton.ScaleX), Mathf.Abs(skeleton.ScaleY), 1f);
					boneRoot.eulerAngles = new Vector3(skeleton.ScaleY > 0 ? 0 : 180,
																	skeleton.ScaleX > 0 ? 0 : 180,
																	0);
				} else {
					boneRoot.localScale = new Vector3(skeleton.ScaleX, skeleton.ScaleY, 1f);
				}
			}

			if (canvas != null) {
				positionScale = canvas.referencePixelsPerUnit;
			}
		}

		[HideInInspector] public SkeletonRenderer skeletonRenderer;
		[HideInInspector] public SkeletonGraphic skeletonGraphic;
		private Canvas canvas;
		[System.NonSerialized] public ISkeletonAnimation skeletonAnimation;

		private ISkeletonComponent skeletonComponent;
		[System.NonSerialized] public List<SkeletonUtilityBone> boneComponents = new List<SkeletonUtilityBone>();
		[System.NonSerialized] public List<SkeletonUtilityConstraint> constraintComponents = new List<SkeletonUtilityConstraint>();


		public ISkeletonComponent SkeletonComponent {
			get {
				if (skeletonComponent == null) {
					skeletonComponent = skeletonRenderer != null ? skeletonRenderer.GetComponent<ISkeletonComponent>() :
										skeletonGraphic != null ? skeletonGraphic.GetComponent<ISkeletonComponent>() :
										GetComponent<ISkeletonComponent>();
				}
				return skeletonComponent;
			}
		}
		public Skeleton Skeleton {
			get {
				if (SkeletonComponent == null)
					return null;
				return skeletonComponent.Skeleton;
			}
		}

		public bool IsValid {
			get {
				return (skeletonRenderer != null && skeletonRenderer.valid) ||
					(skeletonGraphic != null && skeletonGraphic.IsValid);
			}
		}

		public float PositionScale { get { return positionScale; } }

		float positionScale = 1.0f;
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
			if (skeletonGraphic == null) {
				skeletonGraphic = GetComponent<SkeletonGraphic>();
			}
			if (skeletonAnimation == null) {
				skeletonAnimation = skeletonRenderer != null ? skeletonRenderer.GetComponent<ISkeletonAnimation>() :
									skeletonGraphic != null ? skeletonGraphic.GetComponent<ISkeletonAnimation>() :
									GetComponent<ISkeletonAnimation>();
			}
			if (skeletonComponent == null) {
				skeletonComponent = skeletonRenderer != null ? skeletonRenderer.GetComponent<ISkeletonComponent>() :
									skeletonGraphic != null ? skeletonGraphic.GetComponent<ISkeletonComponent>() :
									GetComponent<ISkeletonComponent>();
			}

			if (skeletonRenderer != null) {
				skeletonRenderer.OnRebuild -= HandleRendererReset;
				skeletonRenderer.OnRebuild += HandleRendererReset;
			} else if (skeletonGraphic != null) {
				skeletonGraphic.OnRebuild -= HandleRendererReset;
				skeletonGraphic.OnRebuild += HandleRendererReset;
				canvas = skeletonGraphic.canvas;
				if (canvas == null)
					canvas = skeletonGraphic.GetComponentInParent<Canvas>();
				if (canvas == null)
					positionScale = 100.0f;
			}

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
			if (skeletonRenderer != null)
				skeletonRenderer.OnRebuild -= HandleRendererReset;
			if (skeletonGraphic != null)
				skeletonGraphic.OnRebuild -= HandleRendererReset;

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

		void HandleRendererReset (SkeletonGraphic g) {
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
			var skeleton = skeletonComponent.Skeleton;
			if (skeleton == null) return;

			if (boneRoot != null) {
				var constraintTargets = new List<System.Object>();
				var ikConstraints = skeleton.IkConstraints;
				for (int i = 0, n = ikConstraints.Count; i < n; i++)
					constraintTargets.Add(ikConstraints.Items[i].Target);

				var transformConstraints = skeleton.TransformConstraints;
				for (int i = 0, n = transformConstraints.Count; i < n; i++)
					constraintTargets.Add(transformConstraints.Items[i].Target);

				var boneComponents = this.boneComponents;
				for (int i = 0, n = boneComponents.Count; i < n; i++) {
					var b = boneComponents[i];
					if (b.bone == null) {
						b.DoUpdate(SkeletonUtilityBone.UpdatePhase.Local);
						if (b.bone == null) continue;
					}
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

			var boneRootObject = new GameObject("SkeletonUtility-SkeletonRoot");
#if UNITY_EDITOR
			if (!Application.isPlaying)
				UnityEditor.Undo.RegisterCreatedObjectUndo(boneRootObject, "Spawn Bone");
#endif
			if (skeletonGraphic != null)
				boneRootObject.AddComponent<RectTransform>();

			boneRoot = boneRootObject.transform;
			boneRoot.SetParent(transform);
			boneRoot.localPosition = Vector3.zero;
			boneRoot.localRotation = Quaternion.identity;
			boneRoot.localScale = Vector3.one;

			return boneRoot;
		}

		public GameObject SpawnRoot (SkeletonUtilityBone.Mode mode, bool pos, bool rot, bool sca) {
			GetBoneRoot();
			Skeleton skeleton = this.skeletonComponent.Skeleton;

			GameObject go = SpawnBone(skeleton.RootBone, boneRoot, mode, pos, rot, sca);
			CollectBones();
			return go;
		}

		public GameObject SpawnHierarchy (SkeletonUtilityBone.Mode mode, bool pos, bool rot, bool sca) {
			GetBoneRoot();
			Skeleton skeleton = this.skeletonComponent.Skeleton;
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
#if UNITY_EDITOR
			if (!Application.isPlaying)
				UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Spawn Bone");
#endif
			if (skeletonGraphic != null)
				go.AddComponent<RectTransform>();

			var goTransform = go.transform;
			goTransform.SetParent(parent);

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
				if (pos) goTransform.localPosition = new Vector3(b.bone.X * positionScale, b.bone.Y * positionScale, 0);
				goTransform.localScale = new Vector3(b.bone.ScaleX, b.bone.ScaleY, 0);
			}

			return go;
		}

	}

}
