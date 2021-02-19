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

// Contributed by: Mitch Thompson

#if UNITY_2019_2_OR_NEWER
#define HINGE_JOINT_NEW_BEHAVIOUR
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Spine.Unity.Examples {
	[RequireComponent(typeof(SkeletonRenderer))]
	public class SkeletonRagdoll2D : MonoBehaviour {
		static Transform parentSpaceHelper;

		#region Inspector
		[Header("Hierarchy")]
		[SpineBone]
		public string startingBoneName = "";
		[SpineBone]
		public List<string> stopBoneNames = new List<string>();

		[Header("Parameters")]
		public bool applyOnStart;
		[Tooltip("Warning! You will have to re-enable and tune mix values manually if attempting to remove the ragdoll system.")]
		public bool disableIK = true;
		public bool disableOtherConstraints = false;
		[Space]
		[Tooltip("Set RootRigidbody IsKinematic to true when Apply is called.")]
		public bool pinStartBone;
		public float gravityScale = 1;
		[Tooltip("If no BoundingBox Attachment is attached to a bone, this becomes the default Width or Radius of a Bone's ragdoll Rigidbody")]
		public float thickness = 0.125f;
		[Tooltip("Default rotational limit value. Min is negative this value, Max is this value.")]
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
		public bool oldRagdollBehaviour = true;
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
			}

			targetSkeletonComponent = GetComponent<SkeletonRenderer>() as ISkeletonAnimation;
			if (targetSkeletonComponent == null) Debug.LogError("Attached Spine component does not implement ISkeletonAnimation. This script is not compatible.");
			skeleton = targetSkeletonComponent.Skeleton;

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

				#if HINGE_JOINT_NEW_BEHAVIOUR
					float referenceAngle = (rbParent.transform.eulerAngles.z - t.eulerAngles.z + 360f) % 360f;
					float minAngle = referenceAngle - rotationLimit;
					float maxAngle = referenceAngle + rotationLimit;
					if (maxAngle > 180f) {
						minAngle -= 360f;
						maxAngle -= 360f;
					}
				#else
					float minAngle = - rotationLimit;
					float maxAngle = rotationLimit;
				#endif
					joint.limits = new JointAngleLimits2D {
						min = minAngle,
						max = maxAngle
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
					ikConstraints.Items[i].Mix = 0;
			}

			if (disableOtherConstraints) {
				var transformConstraints = skeleton.TransformConstraints;
				for (int i = 0, n = transformConstraints.Count; i < n; i++) {
					transformConstraints.Items[i].RotateMix = 0;
					transformConstraints.Items[i].ScaleMix = 0;
					transformConstraints.Items[i].ShearMix = 0;
					transformConstraints.Items[i].TranslateMix = 0;
				}

				var pathConstraints = skeleton.PathConstraints;
				for (int i = 0, n = pathConstraints.Count; i < n; i++) {
					pathConstraints.Items[i].RotateMix = 0;
					pathConstraints.Items[i].TranslateMix = 0;
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
				skeleton.SetBonesToSetupPose();
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
			string boneName = b.Data.Name;
			if (stopBoneNames.Contains(boneName))
				return;

			var boneGameObject = new GameObject(boneName);
			boneGameObject.layer = this.colliderLayer;
			Transform t = boneGameObject.transform;
			boneTable.Add(b, t);

			t.parent = transform;
			t.localPosition = new Vector3(b.WorldX, b.WorldY, 0);
			t.localRotation = Quaternion.Euler(0, 0, b.WorldRotationX - b.ShearX);
			t.localScale = new Vector3(b.WorldScaleX, b.WorldScaleY, 1);

			// MITCH: You left "todo: proper ragdoll branching"
			var colliders = AttachBoundingBoxRagdollColliders(b, boneGameObject, skeleton, this.gravityScale);
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

			var rb = boneGameObject.GetComponent<Rigidbody2D>();
			if (rb == null) rb = boneGameObject.AddComponent<Rigidbody2D>();
			rb.gravityScale = this.gravityScale;

			foreach (Bone child in b.Children)
				RecursivelyCreateBoneProxies(child);
		}

		/// <summary>Performed every skeleton animation update to translate Unity Transforms positions into Spine bone transforms.</summary>
		void UpdateSpineSkeleton (ISkeletonAnimation animatedSkeleton) {
			bool flipX = skeleton.ScaleX < 0;
			bool flipY = skeleton.ScaleY < 0;
			bool flipXOR = flipX ^ flipY;
			bool flipOR = flipX || flipY;
			var startingBone = this.StartingBone;

			foreach (var pair in boneTable) {
				var b = pair.Key;
				var t = pair.Value;
				bool isStartingBone = (b == startingBone);
				Transform parentTransform = isStartingBone ? ragdollRoot : boneTable[b.Parent];
				if (!oldRagdollBehaviour && isStartingBone) {
					if (b != skeleton.RootBone) { // RagdollRoot is not skeleton root.
						ragdollRoot.localPosition = new Vector3(b.Parent.WorldX, b.Parent.WorldY, 0);
						ragdollRoot.localRotation = Quaternion.Euler(0, 0, GetPropagatedRotation(b.Parent));
					}
				}

				Vector3 parentTransformWorldPosition = parentTransform.position;
				Quaternion parentTransformWorldRotation = parentTransform.rotation;

				parentSpaceHelper.position = parentTransformWorldPosition;
				parentSpaceHelper.rotation = parentTransformWorldRotation;
				parentSpaceHelper.localScale = parentTransform.lossyScale;

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

				b.X = Mathf.Lerp(b.X, boneLocalPosition.x, mix);
				b.Y = Mathf.Lerp(b.Y, boneLocalPosition.y, mix);
				b.Rotation = Mathf.Lerp(b.Rotation, boneLocalRotation, mix);
				//b.AppliedRotation = Mathf.Lerp(b.AppliedRotation, boneLocalRotation, mix);
			}
		}

		static List<Collider2D> AttachBoundingBoxRagdollColliders (Bone b, GameObject go, Skeleton skeleton, float gravityScale) {
			const string AttachmentNameMarker = "ragdoll";
			var colliders = new List<Collider2D>();
			var skin = skeleton.Skin ?? skeleton.Data.DefaultSkin;

			var skinEntries = new List<Skin.SkinEntry>();
			foreach (Slot slot in skeleton.Slots) {
				if (slot.Bone == b) {
					skin.GetAttachments(skeleton.Slots.IndexOf(slot), skinEntries);

					bool bbAttachmentAdded = false;
					foreach (var entry in skinEntries) {
						var bbAttachment = entry.Attachment as BoundingBoxAttachment;
						if (bbAttachment != null) {
							if (!entry.Name.ToLower().Contains(AttachmentNameMarker))
								continue;

							bbAttachmentAdded = true;
							var bbCollider = SkeletonUtility.AddBoundingBoxAsComponent(bbAttachment, slot, go, isTrigger: false);
							colliders.Add(bbCollider);
						}
					}

					if (bbAttachmentAdded)
						SkeletonUtility.AddBoneRigidbody2D(go, isKinematic: false, gravityScale: gravityScale);
				}
			}

			return colliders;
		}

		static float GetPropagatedRotation (Bone b) {
			Bone parent = b.Parent;
			float a = b.AppliedRotation;
			while (parent != null) {
				a += parent.AppliedRotation;
				parent = parent.Parent;
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
