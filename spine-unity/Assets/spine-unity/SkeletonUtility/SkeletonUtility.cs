

/*****************************************************************************
 * Skeleton Utility created by Mitch Thompson
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Spine;

namespace Spine.Unity {
	[RequireComponent(typeof(ISkeletonAnimation))]
	[ExecuteInEditMode]
	public class SkeletonUtility : MonoBehaviour {

		public static T GetInParent<T> (Transform origin) where T : Component {
			#if UNITY_4_3
			Transform parent = origin.parent;
			while(parent.GetComponent<T>() == null){
			parent = parent.parent;
			if(parent == null)
			return default(T);
			}

			return parent.GetComponent<T>();
			#else
			return origin.GetComponentInParent<T>();
			#endif
		}

		public static PolygonCollider2D AddBoundingBox (Skeleton skeleton, string skinName, string slotName, string attachmentName, Transform parent, bool isTrigger = true) {
			// List<Attachment> attachments = new List<Attachment>();
			Skin skin;

			if (skinName == "")
				skinName = skeleton.Data.DefaultSkin.Name;

			skin = skeleton.Data.FindSkin(skinName);

			if (skin == null) {
				Debug.LogError("Skin " + skinName + " not found!");
				return null;
			}

			var attachment = skin.GetAttachment(skeleton.FindSlotIndex(slotName), attachmentName);
			if (attachment is BoundingBoxAttachment) {
				GameObject go = new GameObject("[BoundingBox]" + attachmentName);
				go.transform.parent = parent;
				go.transform.localPosition = Vector3.zero;
				go.transform.localRotation = Quaternion.identity;
				go.transform.localScale = Vector3.one;
				var collider = go.AddComponent<PolygonCollider2D>();
				collider.isTrigger = isTrigger;
				var boundingBox = (BoundingBoxAttachment)attachment;
				float[] floats = boundingBox.Vertices;
				int floatCount = floats.Length;
				int vertCount = floatCount / 2;

				Vector2[] verts = new Vector2[vertCount];
				int v = 0;
				for (int i = 0; i < floatCount; i += 2, v++) {
					verts[v].x = floats[i];
					verts[v].y = floats[i + 1];
				}

				collider.SetPath(0, verts);

				return collider;

			}

			return null;
		}

		public static PolygonCollider2D AddBoundingBoxAsComponent (BoundingBoxAttachment boundingBox, GameObject gameObject, bool isTrigger = true) {
			if (boundingBox == null)
				return null;

			var collider = gameObject.AddComponent<PolygonCollider2D>();
			collider.isTrigger = isTrigger;
			float[] floats = boundingBox.Vertices;
			int floatCount = floats.Length;
			int vertCount = floatCount / 2;

			Vector2[] verts = new Vector2[vertCount];
			int v = 0;
			for (int i = 0; i < floatCount; i += 2, v++) {
				verts[v].x = floats[i];
				verts[v].y = floats[i + 1];
			}

			collider.SetPath(0, verts);

			return collider;
		}

		public static Bounds GetBoundingBoxBounds (BoundingBoxAttachment boundingBox, float depth = 0) {
			float[] floats = boundingBox.Vertices;
			int floatCount = floats.Length;

			Bounds bounds = new Bounds();

			bounds.center = new Vector3(floats[0], floats[1], 0);
			for (int i = 2; i < floatCount; i += 2) {
				bounds.Encapsulate(new Vector3(floats[i], floats[i + 1], 0));
			}
			Vector3 size = bounds.size;
			size.z = depth;
			bounds.size = size;

			return bounds;
		}

		public delegate void SkeletonUtilityDelegate ();

		public event SkeletonUtilityDelegate OnReset;

		public Transform boneRoot;

		void Update () {
			if (boneRoot != null && skeletonRenderer.skeleton != null) {
				Vector3 flipScale = Vector3.one;
				if (skeletonRenderer.skeleton.FlipX)
					flipScale.x = -1;

				if (skeletonRenderer.skeleton.FlipY)
					flipScale.y = -1;

				boneRoot.localScale = flipScale;
			}
		}

		[HideInInspector]
		public SkeletonRenderer skeletonRenderer;
		[HideInInspector]
		public ISkeletonAnimation skeletonAnimation;
		[System.NonSerialized]
		public List<SkeletonUtilityBone> utilityBones = new List<SkeletonUtilityBone>();
		[System.NonSerialized]
		public List<SkeletonUtilityConstraint> utilityConstraints = new List<SkeletonUtilityConstraint>();
		//	Dictionary<Bone, SkeletonUtilityBone> utilityBoneTable;

		protected bool hasTransformBones;
		protected bool hasUtilityConstraints;
		protected bool needToReprocessBones;

		void OnEnable () {
			if (skeletonRenderer == null) {
				skeletonRenderer = GetComponent<SkeletonRenderer>();
			}

			if (skeletonAnimation == null) {
				skeletonAnimation = GetComponent<SkeletonAnimation>();
				if (skeletonAnimation == null)
					skeletonAnimation = GetComponent<SkeletonAnimator>();
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
			//		CollectBones();
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
			if (OnReset != null)
				OnReset();

			CollectBones();
		}

		public void RegisterBone (SkeletonUtilityBone bone) {
			if (utilityBones.Contains(bone))
				return;
			else {
				utilityBones.Add(bone);
				needToReprocessBones = true;
			}
		}

		public void UnregisterBone (SkeletonUtilityBone bone) {
			utilityBones.Remove(bone);
		}

		public void RegisterConstraint (SkeletonUtilityConstraint constraint) {

			if (utilityConstraints.Contains(constraint))
				return;
			else {
				utilityConstraints.Add(constraint);
				needToReprocessBones = true;
			}
		}

		public void UnregisterConstraint (SkeletonUtilityConstraint constraint) {
			utilityConstraints.Remove(constraint);
		}

		public void CollectBones () {
			if (skeletonRenderer.skeleton == null)
				return;

			if (boneRoot != null) {
				List<string> constraintTargetNames = new List<string>();

				ExposedList<IkConstraint> ikConstraints = skeletonRenderer.skeleton.IkConstraints;
				for (int i = 0, n = ikConstraints.Count; i < n; i++)
					constraintTargetNames.Add(ikConstraints.Items[i].Target.Data.Name);

				var utilityBones = this.utilityBones;
				for (int i = 0, n = utilityBones.Count; i < n; i++) {
					var b = utilityBones[i];
					if (b.bone == null) return;
					if (b.mode == SkeletonUtilityBone.Mode.Override)
						hasTransformBones = true;

					if (constraintTargetNames.Contains(b.bone.Data.Name))
						hasUtilityConstraints = true;
				}

				if (utilityConstraints.Count > 0)
					hasUtilityConstraints = true;

				if (skeletonAnimation != null) {
					skeletonAnimation.UpdateWorld -= UpdateWorld;
					skeletonAnimation.UpdateComplete -= UpdateComplete;

					if (hasTransformBones || hasUtilityConstraints) {
						skeletonAnimation.UpdateWorld += UpdateWorld;
					}

					if (hasUtilityConstraints) {
						skeletonAnimation.UpdateComplete += UpdateComplete;
					}
				}

				needToReprocessBones = false;
			} else {
				utilityBones.Clear();
				utilityConstraints.Clear();
			}

		}

		void UpdateLocal (ISkeletonAnimation anim) {
			if (needToReprocessBones)
				CollectBones();

			var utilityBones = this.utilityBones;
			if (utilityBones == null) return;
			for (int i = 0, n = utilityBones.Count; i < n; i++)
				utilityBones[i].transformLerpComplete = false;

			UpdateAllBones();
		}

		void UpdateWorld (ISkeletonAnimation anim) {
			UpdateAllBones();
			for (int i = 0, n = utilityConstraints.Count; i < n; i++)
				utilityConstraints[i].DoUpdate();
		}

		void UpdateComplete (ISkeletonAnimation anim) {
			UpdateAllBones();
		}

		void UpdateAllBones () {
			if (boneRoot == null)
				CollectBones();
				
			var utilityBones = this.utilityBones;
			if (utilityBones == null) return;
			for (int i = 0, n = utilityBones.Count; i < n; i++)
				utilityBones[i].DoUpdate();
		}

		public Transform GetBoneRoot () {
			if (boneRoot != null)
				return boneRoot;

			boneRoot = new GameObject("SkeletonUtility-Root").transform;
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
			go.transform.parent = parent;

			SkeletonUtilityBone b = go.AddComponent<SkeletonUtilityBone>();
			b.skeletonUtility = this;
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
				if (rot)
					go.transform.localRotation = Quaternion.Euler(0, 0, b.bone.AppliedRotation);

				if (pos)
					go.transform.localPosition = new Vector3(b.bone.X, b.bone.Y, 0);

				go.transform.localScale = new Vector3(b.bone.scaleX, b.bone.scaleY, 0);
			}

			return go;
		}

	}

}
