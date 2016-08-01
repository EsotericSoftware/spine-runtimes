﻿/*****************************************************************************
 * SkeletonRagdoll2D added by Mitch Thompson
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/
//#define FLIPDEBUG

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine.Assertions;

namespace Spine.Unity.Modules {
	[RequireComponent(typeof(SkeletonRenderer))]
	public class SkeletonRagdoll2D : MonoBehaviour {
		static Transform parentSpaceHelper;

		#region Inspector
		#if FLIPDEBUG
		[Header("DEBUG")]
		public bool flipXInitially;
		public bool flipYInitially;
		public bool spawnKinematic;
		public bool disableUpdateBones;
		#endif

		[Header("Hierarchy")]
		[SpineBone]
		public string startingBoneName = "";
		[SpineBone]
		public List<string> stopBoneNames = new List<string>();

		[Header("Parameters")]
		public bool applyOnStart;
		[Tooltip("Warning!  You will have to re-enable and tune mix values manually if attempting to remove the ragdoll system.")]
		public bool disableIK = true;
		public bool disableOtherConstraints = false;
		[Space]
		[Tooltip("Set RootRigidbody IsKinematic to true when Apply is called.")]
		public bool pinStartBone;
		public float gravityScale = 1;
		[Tooltip("If no BoundingBox Attachment is attached to a bone, this becomes the default Width or Radius of a Bone's ragdoll Rigidbody")]
		public float thickness = 0.125f;
		[Tooltip("Default rotational limit value.  Min is negative this value, Max is this value.")]
		public float rotationLimit = 20;
		public float rootMass = 20;
		[Tooltip("If your ragdoll seems unstable or uneffected by limits, try lowering this value.")]
		[Range(0.01f, 1f)]
		public float massFalloffFactor = 0.4f;
		[Tooltip("The layer assigned to all of the rigidbody parts.")]
		[SkeletonRagdoll.LayerField]
		public int colliderLayer = 0;
		[Range(0, 1)]
		public float mix = 1;
		#endregion

		ISkeletonAnimation targetSkeletonComponent;
		Skeleton skeleton;
		Dictionary<Bone, Transform> boneTable = new Dictionary<Bone, Transform>();
		Transform ragdollRoot;
		public Rigidbody2D RootRigidbody { get; private set; }
		public Bone StartingBone { get; private set; }
		Vector2 rootOffset;
		public Vector3 RootOffset { get { return this.rootOffset; } }
		bool isActive;
		public bool IsActive { get { return this.isActive; } }

		IEnumerator Start () {
			if (parentSpaceHelper == null) {
				parentSpaceHelper = (new GameObject("Parent Space Helper")).transform;
				#if !FLIPDEBUG
				parentSpaceHelper.hideFlags = HideFlags.HideInHierarchy;
				#endif
			}

			targetSkeletonComponent = GetComponent<SkeletonRenderer>() as ISkeletonAnimation;
			if (targetSkeletonComponent == null) Debug.LogError("Attached Spine component does not implement ISkeletonAnimation. This script is not compatible.");
			skeleton = targetSkeletonComponent.Skeleton;

			#if FLIPDEBUG
			skeleton.flipX = flipXInitially;
			skeleton.flipY = flipYInitially;
			#endif

			if (applyOnStart) {
				yield return null;
				Apply();
			}
		}

		#region API
		public Rigidbody2D[] RigidbodyArray {
			get {
				if (!isActive)
					return new Rigidbody2D[0];

				var rigidBodies = new Rigidbody2D[boneTable.Count];
				int i = 0;
				foreach (Transform t in boneTable.Values) {
					rigidBodies[i] = t.GetComponent<Rigidbody2D>();
					i++;
				}

				return rigidBodies;
			}
		}

		public Vector3 EstimatedSkeletonPosition {
			get { return this.RootRigidbody.position - rootOffset; }
		}

		/// <summary>Instantiates the ragdoll simulation and applies its transforms to the skeleton.</summary>
		public void Apply () {
			isActive = true;
			mix = 1;

			Bone startingBone = this.StartingBone = skeleton.FindBone(startingBoneName);
			RecursivelyCreateBoneProxies(startingBone);

			RootRigidbody = boneTable[startingBone].GetComponent<Rigidbody2D>();
			#if FLIPDEBUG
			if (!RootRigidbody.isKinematic)
			#endif
			RootRigidbody.isKinematic = pinStartBone;
			RootRigidbody.mass = rootMass;
			var boneColliders = new List<Collider2D>();
			foreach (var pair in boneTable) {
				var b = pair.Key;
				var t = pair.Value;
				Transform parentTransform;
				boneColliders.Add(t.GetComponent<Collider2D>());
				if (b == startingBone) {
					ragdollRoot = new GameObject("RagdollRoot").transform;
					ragdollRoot.SetParent(transform, false);
					if (b == skeleton.RootBone) { // RagdollRoot is skeleton root.
						ragdollRoot.localPosition = new Vector3(b.WorldX, b.WorldY, 0);
						ragdollRoot.localRotation = Quaternion.Euler(0, 0, GetPropagatedRotation(b));
					} else {
						ragdollRoot.localPosition = new Vector3(b.Parent.WorldX, b.Parent.WorldY, 0);
						ragdollRoot.localRotation = Quaternion.Euler(0, 0, GetPropagatedRotation(b.Parent));
					}
					parentTransform = ragdollRoot;
					rootOffset = t.position - transform.position;
				} else {
					parentTransform = boneTable[b.Parent];
				}

				// Add joint and attach to parent.
				var rbParent = parentTransform.GetComponent<Rigidbody2D>();
				if (rbParent != null) {
					var joint = t.gameObject.AddComponent<HingeJoint2D>();
					joint.connectedBody = rbParent;
					Vector3 localPos = parentTransform.InverseTransformPoint(t.position);
					joint.connectedAnchor = localPos;

					joint.GetComponent<Rigidbody2D>().mass = joint.connectedBody.mass * massFalloffFactor;
					joint.limits = new JointAngleLimits2D {
						min = -rotationLimit,
						max = rotationLimit
					};
					joint.useLimits = true;
				}
			}

			// Ignore collisions among bones.
			for (int x = 0; x < boneColliders.Count; x++) {
				for (int y = 0; y < boneColliders.Count; y++) {
					if (x == y) continue;
					Physics2D.IgnoreCollision(boneColliders[x], boneColliders[y]);
				}
			}

			// Destroy existing override-mode SkeletonUtility bones.
			var utilityBones = GetComponentsInChildren<SkeletonUtilityBone>();
			if (utilityBones.Length > 0) {
				var destroyedUtilityBoneNames = new List<string>();
				foreach (var ub in utilityBones) {
					if (ub.mode == SkeletonUtilityBone.Mode.Override) {
						destroyedUtilityBoneNames.Add(ub.gameObject.name);
						Destroy(ub.gameObject);
					}
				}
				if (destroyedUtilityBoneNames.Count > 0) {
					string msg = "Destroyed Utility Bones: ";
					for (int i = 0; i < destroyedUtilityBoneNames.Count; i++) {
						msg += destroyedUtilityBoneNames[i];
						if (i != destroyedUtilityBoneNames.Count - 1) {
							msg += ",";
						}
					}
					Debug.LogWarning(msg);
				}
			}

			// Disable skeleton constraints.
			if (disableIK) {
				var ikConstraints = skeleton.IkConstraints;
				for (int i = 0, n = ikConstraints.Count; i < n; i++)
					ikConstraints.Items[i].mix = 0;
			}

			if (disableOtherConstraints) {
				var transformConstraints = skeleton.transformConstraints;
				for (int i = 0, n = transformConstraints.Count; i < n; i++) {
					transformConstraints.Items[i].rotateMix = 0;
					transformConstraints.Items[i].scaleMix = 0;
					transformConstraints.Items[i].shearMix = 0;
					transformConstraints.Items[i].translateMix = 0;
				}

				var pathConstraints = skeleton.pathConstraints;
				for (int i = 0, n = pathConstraints.Count; i < n; i++) {
					pathConstraints.Items[i].rotateMix = 0;
					pathConstraints.Items[i].translateMix = 0;
				}
			}

			targetSkeletonComponent.UpdateWorld += UpdateSpineSkeleton;
		}

		/// <summary>Transitions the mix value from the current value to a target value.</summary>
		public Coroutine SmoothMix (float target, float duration) {
			return StartCoroutine(SmoothMixCoroutine(target, duration));
		}

		IEnumerator SmoothMixCoroutine (float target, float duration) {
			float startTime = Time.time;
			float startMix = mix;
			while (mix > 0) {
				mix = Mathf.SmoothStep(startMix, target, (Time.time - startTime) / duration);
				yield return null;
			}
		}

		/// <summary>Set the transform world position while preserving the ragdoll parts world position.</summary>
		public void SetSkeletonPosition (Vector3 worldPosition) {
			if (!isActive) {
				Debug.LogWarning("Can't call SetSkeletonPosition while Ragdoll is not active!");
				return;
			}

			Vector3 offset = worldPosition - transform.position;
			transform.position = worldPosition;
			foreach (Transform t in boneTable.Values)
				t.position -= offset;

			UpdateSpineSkeleton(null);
			skeleton.UpdateWorldTransform();
		}

		/// <summary>Removes the ragdoll instance and effect from the animated skeleton.</summary>
		public void Remove () {
			isActive = false;
			foreach (var t in boneTable.Values)
				Destroy(t.gameObject);

			Destroy(ragdollRoot.gameObject);
			boneTable.Clear();
			targetSkeletonComponent.UpdateWorld -= UpdateSpineSkeleton;
		}

		public Rigidbody2D GetRigidbody (string boneName) {
			var bone = skeleton.FindBone(boneName);
			return (bone != null && boneTable.ContainsKey(bone)) ? boneTable[bone].GetComponent<Rigidbody2D>() : null;
		}
		#endregion

		/// <summary>Generates the ragdoll simulation's Transform and joint setup.</summary>
		void RecursivelyCreateBoneProxies (Bone b) {
			string boneName = b.data.name;
			if (stopBoneNames.Contains(boneName))
				return;

			var boneGameObject = new GameObject(boneName);
			boneGameObject.layer = this.colliderLayer;
			Transform t = boneGameObject.transform;
			boneTable.Add(b, t);

			t.parent = transform;
			t.localPosition = new Vector3(b.WorldX, b.WorldY, 0);
			t.localRotation = Quaternion.Euler(0, 0, b.WorldRotationX - b.shearX);
			t.localScale = new Vector3(b.WorldScaleX, b.WorldScaleY, 0);

			// MITCH: You left "todo: proper ragdoll branching"
			var colliders = AttachBoundingBoxRagdollColliders(b, boneGameObject, skeleton);
			if (colliders.Count == 0) {
				float length = b.data.length;
				if (length == 0) {
					var circle = boneGameObject.AddComponent<CircleCollider2D>();
					circle.radius = thickness * 0.5f;
				} else {				
					var box = boneGameObject.AddComponent<BoxCollider2D>();
					box.size = new Vector2(length, thickness);
					box.offset = new Vector2(length * 0.5f, 0); // box.center in UNITY_4
				}
			}
			var rb = boneGameObject.AddComponent<Rigidbody2D>();
			rb.gravityScale = this.gravityScale;

			#if FLIPDEBUG
			rb.isKinematic = spawnKinematic;
			#endif

			foreach (Bone child in b.Children)
				RecursivelyCreateBoneProxies(child);
		}

		/// <summary>Performed every skeleton animation update to translate Unity Transforms positions into Spine bone transforms.</summary>
		void UpdateSpineSkeleton (ISkeletonAnimation animatedSkeleton) {
			#if FLIPDEBUG
			if (disableUpdateBones) return;
			#endif

			bool flipX = skeleton.flipX;
			bool flipY = skeleton.flipY;
			bool flipXOR = flipX ^ flipY;
			bool flipOR = flipX || flipY;
			var startingBone = this.StartingBone;

			foreach (var pair in boneTable) {
				var b = pair.Key;
				var t = pair.Value;
				bool isStartingBone = (b == startingBone);
				Transform parentTransform = isStartingBone ? ragdollRoot : boneTable[b.Parent];
				Vector3 parentTransformWorldPosition = parentTransform.position;
				Quaternion parentTransformWorldRotation = parentTransform.rotation;

				parentSpaceHelper.position = parentTransformWorldPosition;
				parentSpaceHelper.rotation = parentTransformWorldRotation;
				parentSpaceHelper.localScale = parentTransform.localScale;

				Vector3 boneWorldPosition = t.position;
				Vector3 right = parentSpaceHelper.InverseTransformDirection(t.right);

				Vector3 boneLocalPosition = parentSpaceHelper.InverseTransformPoint(boneWorldPosition);
				float boneLocalRotation = Mathf.Atan2(right.y, right.x) * Mathf.Rad2Deg;
				if (flipOR) {
					if (isStartingBone) {
						if (flipX) boneLocalPosition.x *= -1f;
						if (flipY) boneLocalPosition.y *= -1f;

						boneLocalRotation = boneLocalRotation * (flipXOR ? -1f : 1f);
						if (flipX) boneLocalRotation += 180;
					} else {
						if (flipXOR) {
							boneLocalRotation *= -1f;
							boneLocalPosition.y *= -1f; // wtf??
						}
					}
				}

				b.x = Mathf.Lerp(b.x, boneLocalPosition.x, mix);
				b.y = Mathf.Lerp(b.y, boneLocalPosition.y, mix);
				b.rotation = Mathf.Lerp(b.rotation, boneLocalRotation, mix);
				b.appliedRotation = Mathf.Lerp(b.appliedRotation, boneLocalRotation, mix);
			}
		}

		static List<Collider2D> AttachBoundingBoxRagdollColliders (Bone b, GameObject go, Skeleton skeleton) {
			const string AttachmentNameMarker = "ragdoll";
			var colliders = new List<Collider2D>();
			var skin = skeleton.Skin ?? skeleton.Data.DefaultSkin;

			var attachments = new List<Attachment>();
			foreach (Slot s in skeleton.Slots) {
				if (s.bone == b) {
					skin.FindAttachmentsForSlot(skeleton.Slots.IndexOf(s), attachments);
					foreach (var a in attachments) {
						var bbAttachment = a as BoundingBoxAttachment;
						if (bbAttachment != null) {
							if (!a.Name.ToLower().Contains(AttachmentNameMarker))
								continue;

							var bbCollider = SkeletonUtility.AddBoundingBoxAsComponent(bbAttachment, go, false);
							colliders.Add(bbCollider);
						}
					}
				}
			}

			return colliders;
		}
			
		static float GetPropagatedRotation (Bone b) {
			Bone parent = b.Parent;
			float a = b.AppliedRotation;
			while (parent != null) {
				a += parent.AppliedRotation;
				parent = parent.parent;
			}
			return a;
		}

		static Vector3 FlipScale (bool flipX, bool flipY) {
			return new Vector3(flipX ? -1f : 1f, flipY ? -1f : 1f, 1f);
		}

		#if UNITY_EDITOR
		void OnDrawGizmosSelected () {
			if (isActive) {
				Gizmos.DrawWireSphere(transform.position, thickness * 1.2f);
				Vector3 newTransformPos = RootRigidbody.position - rootOffset;
				Gizmos.DrawLine(transform.position, newTransformPos);
				Gizmos.DrawWireSphere(newTransformPos, thickness * 1.2f);
			}
		}
		#endif
	}

}
