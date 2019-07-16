/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using UnityEngine;
using System.Collections.Generic;

namespace Spine.Unity.Examples {
	
	// SkeletonUtilityKinematicShadow allows hinge chains to inherit a velocity interpreted from changes in parent transform position or from unrelated rigidbodies.
	// Note: Uncheck "useRootTransformIfNull
	public class SkeletonUtilityKinematicShadow : MonoBehaviour {
		#region Inspector
		[Tooltip("If checked, the hinge chain can inherit your root transform's velocity or position/rotation changes.")]
		public bool detachedShadow = false;
		public Transform parent;
		public bool hideShadow = true;
		public PhysicsSystem physicsSystem = PhysicsSystem.Physics3D;
		#endregion

		GameObject shadowRoot;
		readonly List<TransformPair> shadowTable = new List<TransformPair>();
		struct TransformPair {
			public Transform dest, src;
		}

		public enum PhysicsSystem {
			Physics2D,
			Physics3D
		};

		void Start () {
			// Duplicate this gameObject as the "shadow" with a different parent.
			shadowRoot = Instantiate<GameObject>(this.gameObject);
			Destroy(shadowRoot.GetComponent<SkeletonUtilityKinematicShadow>());

			// Prepare shadow gameObject's properties.
			var shadowRootTransform = shadowRoot.transform;
			shadowRootTransform.position = transform.position;
			shadowRootTransform.rotation = transform.rotation;

			Vector3 scaleRef = transform.TransformPoint(Vector3.right);
			float scale = Vector3.Distance(transform.position, scaleRef);
			shadowRootTransform.localScale = Vector3.one;

			if (!detachedShadow) {
				// Do not change to null coalescing operator (??). Unity overloads null checks for UnityEngine.Objects but not the ?? operator.
				if (parent == null)
					shadowRootTransform.parent = transform.root;  
				else
					shadowRootTransform.parent = parent;
			}

			if (hideShadow)
				shadowRoot.hideFlags = HideFlags.HideInHierarchy;
			
			var shadowJoints = shadowRoot.GetComponentsInChildren<Joint>();
			foreach (Joint j in shadowJoints)
				j.connectedAnchor *= scale;

			// Build list of bone pairs (matches shadow transforms with bone transforms)
			var bones = GetComponentsInChildren<SkeletonUtilityBone>();
			var shadowBones = shadowRoot.GetComponentsInChildren<SkeletonUtilityBone>();
			foreach (var b in bones) {
				if (b.gameObject == this.gameObject)
					continue;

				System.Type checkType = (physicsSystem == PhysicsSystem.Physics2D) ? typeof(Rigidbody2D) : typeof(Rigidbody);
				foreach (var sb in shadowBones) {
					if (sb.GetComponent(checkType) != null && sb.boneName == b.boneName) {
						shadowTable.Add(new TransformPair {
							dest = b.transform,
							src = sb.transform
						});
						break;
					}
				}

			}

			// Destroy conflicting and unneeded components
			DestroyComponents(shadowBones);

			DestroyComponents(GetComponentsInChildren<Joint>());
			DestroyComponents(GetComponentsInChildren<Rigidbody>());
			DestroyComponents(GetComponentsInChildren<Collider>());
		}

		static void DestroyComponents (Component[] components) {
			for (int i = 0, n = components.Length; i < n; i++)
				Destroy(components[i]);
		}

		void FixedUpdate () {
			if (physicsSystem == PhysicsSystem.Physics2D) {
				var shadowRootRigidbody = shadowRoot.GetComponent<Rigidbody2D>();
				shadowRootRigidbody.MovePosition(transform.position);
				shadowRootRigidbody.MoveRotation(transform.rotation.eulerAngles.z);
			} else {
				var shadowRootRigidbody = shadowRoot.GetComponent<Rigidbody>();
				shadowRootRigidbody.MovePosition(transform.position);
				shadowRootRigidbody.MoveRotation(transform.rotation);
			}

			for (int i = 0, n = shadowTable.Count; i < n; i++) {
				var pair = shadowTable[i];
				pair.dest.localPosition = pair.src.localPosition;
				pair.dest.localRotation = pair.src.localRotation;
			}
		}
	}
}
