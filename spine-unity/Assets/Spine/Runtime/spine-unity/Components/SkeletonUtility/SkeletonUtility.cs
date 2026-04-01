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

#if UNITY_6000_0_OR_NEWER
#define USE_RIGIDBODY_BODY_TYPE
#endif

#if !SPINE_AUTO_UPGRADE_COMPONENTS_OFF
#define AUTO_UPGRADE_TO_43_COMPONENTS
#endif

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Spine.Unity {

#if NEW_PREFAB_SYSTEM
	[ExecuteAlways]
#else
	[ExecuteInEditMode]
#endif
	[RequireComponent(typeof(ISkeletonRenderer))]
	[HelpURL("https://esotericsoftware.com/spine-unity-utility-components#SkeletonUtility")]
	public sealed class SkeletonUtility : MonoBehaviour, IUpgradable {

		#region BoundingBoxAttachment
		public static PolygonCollider2D AddBoundingBoxGameObject (Skeleton skeleton, string skinName, string slotName, string attachmentName, Transform parent, bool isTrigger = true) {
			Skin skin = string.IsNullOrEmpty(skinName) ? skeleton.Data.DefaultSkin : skeleton.Data.FindSkin(skinName);
			if (skin == null) {
				Debug.LogError("Skin " + skinName + " not found!");
				return null;
			}

			Slot slot = skeleton.FindSlot(slotName);
			Attachment attachment = slot != null ? skin.GetAttachment(slot.Data.Index, attachmentName) : null;
			if (attachment == null) {
				Debug.LogFormat("Attachment in slot '{0}' named '{1}' not found in skin '{2}'.", slotName, attachmentName, skin.Name);
				return null;
			}

			BoundingBoxAttachment box = attachment as BoundingBoxAttachment;
			if (box != null) {
				return AddBoundingBoxGameObject(box.Name, box, skeleton, slot, parent, isTrigger);
			} else {
				Debug.LogFormat("Attachment '{0}' was not a Bounding Box.", attachmentName);
				return null;
			}
		}

		public static PolygonCollider2D AddBoundingBoxGameObject (string name, BoundingBoxAttachment box, Skeleton skeleton, Slot slot, Transform parent, bool isTrigger = true) {
			GameObject go = new GameObject("[BoundingBox]" + (string.IsNullOrEmpty(name) ? box.Name : name));
#if UNITY_EDITOR
			if (!Application.isPlaying)
				UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Spawn BoundingBox");
#endif
			Transform got = go.transform;
			got.parent = parent;
			got.localPosition = Vector3.zero;
			got.localRotation = Quaternion.identity;
			got.localScale = Vector3.one;
			return AddBoundingBoxAsComponent(box, skeleton, slot, go, isTrigger);
		}

		public static PolygonCollider2D AddBoundingBoxAsComponent (BoundingBoxAttachment box, Skeleton skeleton, Slot slot, GameObject gameObject, bool isTrigger = true) {
			if (box == null) return null;
			PolygonCollider2D collider = gameObject.AddComponent<PolygonCollider2D>();
			collider.isTrigger = isTrigger;
			SetColliderPointsLocal(collider, skeleton, slot, box);
			return collider;
		}

		public static void SetColliderPointsLocal (PolygonCollider2D collider, Skeleton skeleton, Slot slot, BoundingBoxAttachment box, float scale = 1.0f) {
			if (box == null) return;
			if (box.IsWeighted()) Debug.LogWarning("UnityEngine.PolygonCollider2D does not support weighted or animated points. Collider points will not be animated and may have incorrect orientation. If you want to use it as a collider, please remove weights and animations from the bounding box in Spine editor.");
			Vector2[] verts = box.GetLocalVertices(skeleton, slot, null);
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
			Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
			if (rb == null) {
				rb = gameObject.AddComponent<Rigidbody2D>();
#if USE_RIGIDBODY_BODY_TYPE
				rb.bodyType = isKinematic ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
#else
				rb.isKinematic = isKinematic;
#endif
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
			Skeleton skeleton = skeletonComponent.Skeleton;
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

			if (skeletonGraphic != null) {
				positionScale = skeletonGraphic.MeshScale;
				lastPositionScale = positionScale;
				if (boneRoot) {
					positionOffset = skeletonGraphic.MeshOffset;
					if (positionOffset != Vector2.zero) {
						boneRoot.localPosition = positionOffset;
					}
				}
			}
		}

		void UpdateToMeshScaleAndOffset (MeshGeneratorBuffers ignoredParameter) {
			if (skeletonGraphic == null) return;

			positionScale = skeletonGraphic.MeshScale;
			if (boneRoot) {
				positionOffset = skeletonGraphic.MeshOffset;
				if (positionOffset != Vector2.zero) {
					boneRoot.localPosition = positionOffset;
				}
			}

			// Note: skeletonGraphic.MeshScale and MeshOffset can be one frame behind in Update() above.
			// Unfortunately update order is:
			// 1. SkeletonGraphic.Update updating skeleton bones and calling UpdateWorld callback,
			//    calling SkeletonUtilityBone.DoUpdate() reading hierarchy.PositionScale.
			// 2. Layout change triggers SkeletonGraphic.Rebuild, updating MeshScale and MeshOffset.
			// Thus to prevent a one-frame-behind offset after a layout change affecting mesh scale,
			// we have to re-evaluate the callbacks via the lines below.
			if (lastPositionScale != positionScale) {
				UpdateLocal(skeletonGraphic);
				UpdateWorld(skeletonGraphic);
				UpdateComplete(skeletonGraphic);
			}
		}

		[HideInInspector] public SkeletonRenderer skeletonRenderer;
		[HideInInspector] public SkeletonGraphic skeletonGraphic;

		private ISkeletonRenderer skeletonComponent;
		[System.NonSerialized] public List<SkeletonUtilityBone> boneComponents = new List<SkeletonUtilityBone>();
		[System.NonSerialized] public List<SkeletonUtilityConstraint> constraintComponents = new List<SkeletonUtilityConstraint>();


		public ISkeletonComponent SkeletonComponent { get { return this.SkeletonRenderer; } }

		public ISkeletonRenderer SkeletonRenderer {
			get {
				if (skeletonComponent == null) {
					skeletonComponent = skeletonRenderer != null ? skeletonRenderer :
										skeletonGraphic != null ? skeletonGraphic :
										GetComponent<ISkeletonRenderer>();
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
				ISkeletonRenderer skeletonComponent = this.SkeletonRenderer;
				return (skeletonComponent != null && skeletonComponent.IsValid);
			}
		}

		public float PositionScale { get { return positionScale; } }
		public Vector2 PositionOffset { get { return positionOffset; } }

		float positionScale = 1.0f;
		float lastPositionScale = 1.0f;
		Vector2 positionOffset = Vector2.zero;
		bool hasOverrideBones;
		bool hasConstraintTargetBones;
		bool needToReprocessBones;

		public void OnUtilityBoneChanged () {
			needToReprocessBones = true;
		}

		public void ResubscribeEvents () {
			ResubscribeIndependentEvents();
			ResubscribeDependentEvents();
		}

		void ResubscribeIndependentEvents () {
			ISkeletonRenderer skeletonComponent = this.SkeletonRenderer;
			if (skeletonComponent != null) {
				skeletonComponent.OnRebuild -= HandleRendererReset;
				skeletonComponent.OnRebuild += HandleRendererReset;
			}
			if (skeletonGraphic != null) {
				skeletonGraphic.OnPostProcessVertices -= UpdateToMeshScaleAndOffset;
				skeletonGraphic.OnPostProcessVertices += UpdateToMeshScaleAndOffset;
			}
		}

		void ResubscribeDependentEvents () {
			ISkeletonRenderer skeletonComponent = this.SkeletonRenderer;
			if (skeletonComponent != null) {
				skeletonComponent.UpdateLocal -= UpdateLocal;
				skeletonComponent.UpdateWorld -= UpdateWorld;
				skeletonComponent.UpdateComplete -= UpdateComplete;

				bool hasConstraintComponents = constraintComponents.Count > 0;
				if (hasOverrideBones || !hasConstraintTargetBones)
					skeletonComponent.UpdateLocal += UpdateLocal;
				if (hasOverrideBones || hasConstraintComponents)
					skeletonComponent.UpdateWorld += UpdateWorld;
				if (hasConstraintTargetBones)
					skeletonComponent.UpdateComplete += UpdateComplete;
			}
		}

		void OnEnable () {
			if (skeletonRenderer == null) {
				skeletonRenderer = GetComponent<SkeletonRenderer>();
			}
			if (skeletonGraphic == null) {
				skeletonGraphic = GetComponent<SkeletonGraphic>();
			}
			CollectBones();
			ResubscribeEvents();
		}

#if UNITY_EDITOR && AUTO_UPGRADE_TO_43_COMPONENTS
		void Awake () {
			if (!Application.isPlaying && !wasUpgradedTo43) {
				UpgradeTo43();
			}
		}
#endif

		void Start () {
			//recollect because order of operations failure when switching between game mode and edit mode...
			CollectBones();
		}

		void OnDisable () {
			ISkeletonRenderer skeletonComponent = this.SkeletonRenderer;
			if (skeletonComponent != null) {
				skeletonComponent.OnRebuild -= HandleRendererReset;
				skeletonComponent.UpdateLocal -= UpdateLocal;
				skeletonComponent.UpdateWorld -= UpdateWorld;
				skeletonComponent.UpdateComplete -= UpdateComplete;
			}
			if (skeletonGraphic) {
				skeletonGraphic.OnPostProcessVertices -= UpdateToMeshScaleAndOffset;
			}
		}

		void HandleRendererReset (ISkeletonRenderer r) {
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
			ISkeletonRenderer skeletonComponent = this.SkeletonRenderer;
			if (skeletonComponent == null) return;
			Skeleton skeleton = skeletonComponent.Skeleton;
			if (skeleton == null) return;

			if (boneRoot != null) {
				hasOverrideBones = false;
				hasConstraintTargetBones = false;

				List<Bone> constrainedBones = new List<Bone>();
				ExposedList<IConstraint> constraints = skeleton.Constraints;
				for (int i = 0, n = constraints.Count; i < n; i++) {
					IConstraint constraint = constraints.Items[i];
					ExposedList<BonePose> bones = null;
					if (constraint is IkConstraint)
						bones = ((IkConstraint)constraint).Bones;
					else if (constraint is TransformConstraint)
						bones = ((TransformConstraint)constraint).Bones;
					else if (constraint is PathConstraint)
						bones = ((PathConstraint)constraint).Bones;
					if (bones != null) {
						for (int j = 0, m = bones.Count; j < m; j++)
							constrainedBones.Add(bones.Items[j].bone);
					}
				}

				List<SkeletonUtilityBone> boneComponents = this.boneComponents;
				for (int i = 0, n = boneComponents.Count; i < n; i++) {
					SkeletonUtilityBone b = boneComponents[i];
					if (b.bone == null) {
						b.DoUpdate(SkeletonUtilityBone.UpdatePhase.Local);
						if (b.bone == null) continue;
					}
					hasOverrideBones |= (b.mode == SkeletonUtilityBone.Mode.Override);
					hasConstraintTargetBones |= constrainedBones.Contains(b.bone);
				}

				needToReprocessBones = false;
			} else {
				boneComponents.Clear();
				constraintComponents.Clear();
			}
			ResubscribeDependentEvents();
		}

		void UpdateLocal (ISkeletonRenderer skeletonRenderer) {
			UpdateAllBones(SkeletonUtilityBone.UpdatePhase.Local);
		}

		void UpdateWorld (ISkeletonRenderer skeletonRenderer) {
			UpdateAllBones(SkeletonUtilityBone.UpdatePhase.World);
			for (int i = 0, n = constraintComponents.Count; i < n; i++)
				constraintComponents[i].DoUpdate();
		}

		void UpdateComplete (ISkeletonRenderer skeletonRenderer) {
			UpdateAllBones(SkeletonUtilityBone.UpdatePhase.Complete);
		}

		void UpdateAllBones (SkeletonUtilityBone.UpdatePhase phase) {
			if (boneRoot == null || needToReprocessBones)
				CollectBones();

			List<SkeletonUtilityBone> boneComponents = this.boneComponents;
			if (boneComponents == null) return;
			for (int i = 0, n = boneComponents.Count; i < n; i++)
				boneComponents[i].DoUpdate(phase);
		}

		public Transform GetBoneRoot () {
			if (boneRoot != null)
				return boneRoot;

			GameObject boneRootObject = new GameObject("SkeletonUtility-SkeletonRoot");
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

			Transform goTransform = go.transform;
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
				var bonePose = b.bone.AppliedPose;
				if (rot) goTransform.localRotation = Quaternion.Euler(0, 0, bonePose.Rotation);
				if (pos) goTransform.localPosition = new Vector3(
					bonePose.X * positionScale + positionOffset.x, bonePose.Y * positionScale + positionOffset.y, 0);
				goTransform.localScale = new Vector3(bonePose.ScaleX, bonePose.ScaleY, 0);
			}

			return go;
		}

		#region Transfer of Deprecated Fields
#if UNITY_EDITOR && AUTO_UPGRADE_TO_43_COMPONENTS
		public void UpgradeTo43 () {
			wasUpgradedTo43 = true;
			if (skeletonRenderer == null && skeletonGraphic == null) {
				Component previousReference = previousSkeletonRenderer != null ? previousSkeletonRenderer : this;
				skeletonRenderer = previousReference.GetComponent<SkeletonRenderer>();
				if (skeletonRenderer == null)
					Debug.LogError("Please manually re-assign SkeletonRenderer at SkeletonUtility, " +
						"automatic upgrade failed.", this);
			}
		}
		[SerializeField, HideInInspector, FormerlySerializedAs("skeletonRenderer")] Component previousSkeletonRenderer;
		[SerializeField] bool wasUpgradedTo43 = false;
#endif
		#endregion
	}
}
