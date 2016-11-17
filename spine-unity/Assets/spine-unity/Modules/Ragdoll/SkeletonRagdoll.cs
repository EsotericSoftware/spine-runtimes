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

// Contributed by: Mitch Thompson

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Spine.Unity.Modules {
	[RequireComponent(typeof(SkeletonRenderer))]
	public class SkeletonRagdoll : MonoBehaviour {
		static Transform parentSpaceHelper;

		#region Inspector
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
		[Space(18)]
		[Tooltip("Set RootRigidbody IsKinematic to true when Apply is called.")]
		public bool pinStartBone;
		[Tooltip("Enable Collision between adjacent ragdoll elements (IE: Neck and Head)")]
		public bool enableJointCollision;
		public bool useGravity = true;
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
		#endregion

		ISkeletonAnimation targetSkeletonComponent;
		Skeleton skeleton;
		Dictionary<Bone, Transform> boneTable = new Dictionary<Bone, Transform>();
		Transform ragdollRoot;
		public Rigidbody RootRigidbody { get; private set; }
		public Bone StartingBone { get; private set; }
		Vector3 rootOffset;
		public Vector3 RootOffset { get { return this.rootOffset; } }
		bool isActive;
		public bool IsActive { get { return this.isActive; } }

		IEnumerator Start () {
			if (parentSpaceHelper == null) {
				parentSpaceHelper = (new GameObject("Parent Space Helper")).transform;
				parentSpaceHelper.hideFlags = HideFlags.HideInHierarchy;
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
		public Rigidbody[] RigidbodyArray {
			get {
				if (!isActive)
					return new Rigidbody[0];

				var rigidBodies = new Rigidbody[boneTable.Count];
				int i = 0;
				foreach (Transform t in boneTable.Values) {
					rigidBodies[i] = t.GetComponent<Rigidbody>();
					i++;
				}

				return rigidBodies;
			}
		}

		public Vector3 EstimatedSkeletonPosition {
			get { return RootRigidbody.position - rootOffset; }
		}

		/// <summary>Instantiates the ragdoll simulation and applies its transforms to the skeleton.</summary>
		public void Apply () {
			isActive = true;
			mix = 1;

			StartingBone = skeleton.FindBone(startingBoneName);
			RecursivelyCreateBoneProxies(StartingBone);

			RootRigidbody = boneTable[StartingBone].GetComponent<Rigidbody>();
			RootRigidbody.isKinematic = pinStartBone;
			RootRigidbody.mass = rootMass;
			var boneColliders = new List<Collider>();
			foreach (var pair in boneTable) {
				var b = pair.Key;
				var t = pair.Value;
				Transform parentTransform;
				boneColliders.Add(t.GetComponent<Collider>());
				if (b == StartingBone) {
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
				var rbParent = parentTransform.GetComponent<Rigidbody>();
				if (rbParent != null) {
					var joint = t.gameObject.AddComponent<HingeJoint>();
					joint.connectedBody = rbParent;
					Vector3 localPos = parentTransform.InverseTransformPoint(t.position);
					localPos.x *= 1;
					joint.connectedAnchor = localPos;
					joint.axis = Vector3.forward;

					joint.GetComponent<Rigidbody>().mass = joint.connectedBody.mass * massFalloffFactor;
					joint.limits = new JointLimits {
						min = -rotationLimit,
						max = rotationLimit,
					};
					joint.useLimits = true;
					joint.enableCollision = enableJointCollision;
				}
			}

			// Ignore collisions among bones.
			for (int x = 0; x < boneColliders.Count; x++) {
				for (int y = 0; y < boneColliders.Count; y++) {
					if (x == y) continue;
					Physics.IgnoreCollision(boneColliders[x], boneColliders[y]);
				}
			}

			// Destroy existing override-mode SkeletonUtilityBones.
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

		public Rigidbody GetRigidbody (string boneName) {
			var bone = skeleton.FindBone(boneName);
			return (bone != null && boneTable.ContainsKey(bone)) ? boneTable[bone].GetComponent<Rigidbody>() : null;
		}
		#endregion

		void RecursivelyCreateBoneProxies (Bone b) {
			string boneName = b.data.name;
			if (stopBoneNames.Contains(boneName))
				return;

			var boneGameObject = new GameObject(boneName);
			boneGameObject.layer = colliderLayer;
			Transform t = boneGameObject.transform;
			boneTable.Add(b, t);

			t.parent = transform;
			t.localPosition = new Vector3(b.WorldX, b.WorldY, 0);
			t.localRotation = Quaternion.Euler(0, 0, b.WorldRotationX - b.shearX);
			t.localScale = new Vector3(b.WorldScaleX, b.WorldScaleY, 1);

			// MITCH: You left "todo: proper ragdoll branching"
			var colliders = AttachBoundingBoxRagdollColliders(b);
			if (colliders.Count == 0) {
				float length = b.Data.Length;
				if (length == 0) {
					var ball = boneGameObject.AddComponent<SphereCollider>();
					ball.radius = thickness * 0.5f;
				} else {					
					var box = boneGameObject.AddComponent<BoxCollider>();
					box.size = new Vector3(length, thickness, thickness);
					box.center = new Vector3(length * 0.5f, 0);
				}
			}
			var rb = boneGameObject.AddComponent<Rigidbody>();
			rb.constraints = RigidbodyConstraints.FreezePositionZ;

			foreach (Bone child in b.Children)
				RecursivelyCreateBoneProxies(child);
		}

		void UpdateSpineSkeleton (ISkeletonAnimation skeletonRenderer) {
			bool flipX = skeleton.flipX;
			bool flipY = skeleton.flipY;
			bool flipXOR = flipX ^ flipY;
			bool flipOR = flipX || flipY;

			foreach (var pair in boneTable) {
				var b = pair.Key;
				var t = pair.Value;
				bool isStartingBone = b == StartingBone;
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
				//b.AppliedRotation = Mathf.Lerp(b.AppliedRotation, boneLocalRotation, mix);
			}
		}

		List<Collider> AttachBoundingBoxRagdollColliders (Bone b) {
			const string AttachmentNameMarker = "ragdoll";
			var colliders = new List<Collider>();

			Transform t = boneTable[b];
			GameObject go = t.gameObject;
			var skin = skeleton.Skin ?? skeleton.Data.DefaultSkin;

			var attachments = new List<Attachment>();
			foreach (Slot s in skeleton.Slots) {
				if (s.Bone == b) {
					skin.FindAttachmentsForSlot(skeleton.Slots.IndexOf(s), attachments);
					foreach (var a in attachments) {
						var bbAttachment = a as BoundingBoxAttachment;
						if (bbAttachment != null) {
							if (!a.Name.ToLower().Contains(AttachmentNameMarker))
								continue;

							var bbCollider = go.AddComponent<BoxCollider>();
							var bounds = SkeletonUtility.GetBoundingBoxBounds(bbAttachment, thickness);
							bbCollider.center = bounds.center;
							bbCollider.size = bounds.size;
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

		public class LayerFieldAttribute : PropertyAttribute {}
	}

}
