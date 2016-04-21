/*****************************************************************************
 * SkeletonRagdoll2D added by Mitch Thompson
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;

namespace Spine.Unity.Modules {
	[RequireComponent(typeof(SkeletonRenderer))]
	public class SkeletonRagdoll2D : MonoBehaviour {
		private static Transform helper;

		[Header("Hierarchy")]
		[SpineBone]
		public string startingBoneName = "";
		[SpineBone]
		public List<string> stopBoneNames = new List<string>();

		[Header("Parameters")]
		public bool applyOnStart;
		[Tooltip("Set RootRigidbody IsKinematic to true when Apply is called.")]
		public bool pinStartBone;
		public float gravityScale = 1;
		[Tooltip("Warning!  You will have to re-enable and tune mix values manually if attempting to remove the ragdoll system.")]
		public bool disableIK = true;
		[Tooltip("If no BoundingBox Attachment is attached to a bone, this becomes the default Width or Radius of a Bone's ragdoll Rigidbody")]
		public float thickness = 0.125f;
		[Tooltip("Default rotational limit value.  Min is negative this value, Max is this value.")]
		public float rotationLimit = 20;
		public float rootMass = 20;
		[Tooltip("If your ragdoll seems unstable or uneffected by limits, try lowering this value.")]
		[Range(0.01f, 1f)]
		public float massFalloffFactor = 0.4f;
		[Tooltip("The layer assigned to all of the rigidbody parts.")]
		public int colliderLayer = 0;
		[Range(0, 1)]
		public float mix = 1;

		ISkeletonAnimation targetSkeletonComponent;
		Skeleton skeleton;

		Dictionary<Bone, Transform> boneTable = new Dictionary<Bone, Transform>();
		Transform ragdollRoot;

		Rigidbody2D rootRigidbody;
		Vector2 rootOffset;
		bool isActive;
		public Rigidbody2D RootRigidbody { get { return this.rootRigidbody; } }
		public Vector3 RootOffset { get { return this.rootOffset; } }
		public bool IsActive { get { return this.isActive; } }
		public Bone RagdollRootBone { get; private set; }

		IEnumerator Start () {
			if (helper == null) {
				helper = (Transform)(new GameObject("Helper")).transform;
				helper.hideFlags = HideFlags.HideInHierarchy;
			}

			targetSkeletonComponent = GetComponent<SkeletonRenderer>() as ISkeletonAnimation;
			if (targetSkeletonComponent == null) Debug.LogError("Attached Spine component does not implement ISkeletonAnimation. This script is not compatible.");

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

				var returnArray = new Rigidbody2D[boneTable.Count];
				int i = 0;
				foreach (Transform t in boneTable.Values) {
					returnArray[i] = t.GetComponent<Rigidbody2D>();
					i++;
				}
				return returnArray;
			}
		}

		public Vector3 EstimatedSkeletonPosition {
			get { return this.rootRigidbody.position - rootOffset; }
		}

		/// <summary>Instantiates the ragdoll simulation and applies its transforms to the skeleton.</summary>
		public void Apply () {
			isActive = true;
			skeleton = targetSkeletonComponent.Skeleton;
			mix = 1;

			var ragdollRootBone = this.RagdollRootBone = skeleton.FindBone(startingBoneName);

			RecursivelyCreateBoneProxies(ragdollRootBone);

			rootRigidbody = boneTable[ragdollRootBone].GetComponent<Rigidbody2D>();
			rootRigidbody.isKinematic = pinStartBone;
			rootRigidbody.mass = rootMass;

			List<Collider2D> boneColliders = new List<Collider2D>();

			foreach (var pair in boneTable) {
				var b = pair.Key;
				var t = pair.Value;
				Bone parentBone = null;
				Transform parentTransform = transform;

				boneColliders.Add(t.GetComponent<Collider2D>());

				if (b == ragdollRootBone) {
					ragdollRoot = new GameObject("RagdollRoot").transform;
					ragdollRoot.parent = transform;

					if (b == skeleton.RootBone) {
						ragdollRoot.localPosition = new Vector3(b.WorldX, b.WorldY, 0);
						ragdollRoot.localRotation = Quaternion.Euler(0, 0, GetCompensatedRotationIK(b));
						parentTransform = ragdollRoot;
					} else {
						ragdollRoot.localPosition = new Vector3(b.Parent.WorldX, b.Parent.WorldY, 0);
						ragdollRoot.localRotation = Quaternion.Euler(0, 0, GetCompensatedRotationIK(b.Parent));
						parentTransform = ragdollRoot;
					}

					rootOffset = t.position - transform.position;

				} else {
					parentBone = b.Parent;
					parentTransform = boneTable[parentBone];

				}

				var rbParent = parentTransform.GetComponent<Rigidbody2D>();

				// Add joint and attach to parent
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
				List<string> destroyedUtilityBoneNames = new List<string>();
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

			if (disableIK) {
				foreach (IkConstraint ik in skeleton.IkConstraints)
					ik.Mix = 0;
			}

			targetSkeletonComponent.UpdateWorld += UpdateWorld;
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

		/// <summary>Sets the world position of the SkeletonRenderer transform and applies the resulting world position offset to the ragdoll parts (effectively making it stay in place).</summary>
		public void SetSkeletonPosition (Vector3 worldPosition) {
			if (!isActive) {
				Debug.LogWarning("Can't call SetSkeletonPosition while Ragdoll is not active!");
				return;
			}

			Vector3 offset = worldPosition - transform.position;
			transform.position = worldPosition;
			foreach (Transform t in boneTable.Values) {
				t.position -= offset;
			}

			UpdateWorld(null);
			skeleton.UpdateWorldTransform();
		}

		/// <summary>Removes the ragdoll instance and effect from the animated skeleton.</summary>
		public void Remove () {
			isActive = false;
			foreach (var t in boneTable.Values)
				Destroy(t.gameObject);
			
			Destroy(ragdollRoot.gameObject);

			boneTable.Clear();
			targetSkeletonComponent.UpdateWorld -= UpdateWorld;
		}

		public Rigidbody2D GetRigidbody (string boneName) {
			var bone = skeleton.FindBone(boneName);
			if (bone == null)
				return null;

			if (boneTable.ContainsKey(bone))
				return boneTable[bone].GetComponent<Rigidbody2D>();

			return null;
		}
		#endregion

		/// <summary>Generates the ragdoll simulation's Transform and joint setup.</summary>
		void RecursivelyCreateBoneProxies (Bone b) {
			if (stopBoneNames.Contains(b.Data.Name))
				return;

			var boneGameObject = new GameObject(b.Data.Name);
			boneGameObject.layer = colliderLayer;
			Transform t = boneGameObject.transform;
			boneTable.Add(b, t);

			t.parent = transform;
			t.localPosition = new Vector3(b.WorldX, b.WorldY, 0);
			t.localRotation = Quaternion.Euler(0, 0, b.WorldRotationX);
			t.localScale = new Vector3(b.WorldScaleX, b.WorldScaleY, 0);

			// MITCH: You left a todo saying "todo: proper ragdoll branching"
			var colliders = AttachBoundingBoxRagdollColliders(b, boneGameObject, skeleton);
			if (colliders.Count == 0) {

				float length = b.Data.Length;
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
			rb.gravityScale = gravityScale;

			foreach (Bone child in b.Children) {
				RecursivelyCreateBoneProxies(child);
			}
		}

		/// <summary>Performed every skeleton animation update.</summary>
		void UpdateWorld (ISkeletonAnimation skeletonRenderer) {
			var ragdollRootBone = this.RagdollRootBone;

			foreach (var pair in boneTable) {
				var b = pair.Key;
				var t = pair.Value;
				//bool flip = false;
				bool flipX = false;  //TODO:  deal with negative scale instead of Flip Key
				bool flipY = false;  //TODO:  deal with negative scale instead of Flip Key
				Bone parentBone = null;
				Transform parentTransform = transform;

				if (b == ragdollRootBone) {
					parentBone = b.Parent;
					parentTransform = ragdollRoot;
					if (b.Parent == null) {
						flipX = b.Skeleton.FlipX;
						flipY = b.Skeleton.FlipY;
					}

				} else {
					parentBone = b.Parent;
					parentTransform = boneTable[parentBone];

				}

				//flip = flipX ^ flipY;

				helper.position = parentTransform.position;
				helper.rotation = parentTransform.rotation;
				helper.localScale = new Vector3(flipX ? -parentTransform.localScale.x : parentTransform.localScale.x, flipY ? -parentTransform.localScale.y : parentTransform.localScale.y, 1);


				Vector3 pos = t.position;
				pos = helper.InverseTransformPoint(pos);
				b.X = Mathf.Lerp(b.X, pos.x, mix);
				b.Y = Mathf.Lerp(b.Y, pos.y, mix);

				Vector3 right = helper.InverseTransformDirection(t.right);

				float a = Mathf.Atan2(right.y, right.x) * Mathf.Rad2Deg;

				// MITCH
				//if (b.WorldFlipX ^ b.WorldFlipY) {
				//	a *= -1;
				//}

				if (parentBone != null) {
					// MITCH
					//if ((b.WorldFlipX ^ b.WorldFlipY) != flip) {
					//	a -= GetCompensatedRotationIK(parentBone) * 2;
					//}
				}

				b.Rotation = Mathf.Lerp(b.Rotation, a, mix);
				// MITCH
				// b.RotationIK = Mathf.Lerp(b.rotationIK, a, mix);
			}
		}

		static List<Collider2D> AttachBoundingBoxRagdollColliders (Bone b, GameObject go, Skeleton skeleton) {
			var colliders = new List<Collider2D>();
			var skin = skeleton.Skin ?? skeleton.Data.DefaultSkin;

			var attachments = new List<Attachment>();
			foreach (Slot s in skeleton.Slots) {
				if (s.Bone == b) {
					skin.FindAttachmentsForSlot(skeleton.Slots.IndexOf(s), attachments);
					foreach (var a in attachments) {
						if (a is BoundingBoxAttachment) {
							if (!a.Name.ToLower().Contains("ragdoll"))
								continue;

							var bbCollider = SkeletonUtility.AddBoundingBoxAsComponent((BoundingBoxAttachment)a, go, false);
							colliders.Add(bbCollider);
						}
					}
				}
			}

			return colliders;
		}

		static float GetCompensatedRotationIK (Bone b) {
			Bone parent = b.Parent;
			// MITCH
			float a = b.AppliedRotation;
			while (parent != null) {
				a += parent.AppliedRotation;
				parent = parent.parent;
			}

			return a;
		}



		#if UNITY_EDITOR
		void OnDrawGizmosSelected () {
			if (isActive) {
				Gizmos.DrawWireSphere(transform.position, thickness * 1.2f);
				Vector3 newTransformPos = rootRigidbody.position - rootOffset;
				Gizmos.DrawLine(transform.position, newTransformPos);
				Gizmos.DrawWireSphere(newTransformPos, thickness * 1.2f);
			}
		}
		#endif

	}

}
