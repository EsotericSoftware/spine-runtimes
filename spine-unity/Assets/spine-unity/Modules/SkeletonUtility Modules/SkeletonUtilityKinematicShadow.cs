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

using UnityEngine;
using System.Collections.Generic;

namespace Spine.Unity.Modules {
	
	// SkeletonUtilityKinematicShadow allows hinge chains to inherit a velocity interpreted from changes in parent transform position or from unrelated rigidbodies.
	// Note: Uncheck "useRootTransformIfNull
	public class SkeletonUtilityKinematicShadow : MonoBehaviour {
		#region Inspector
		[Tooltip("If checked, the hinge chain can inherit your root transform's velocity or position/rotation changes.")]
		public bool detachedShadow = false;
		public Transform parent;
		public bool hideShadow = true;
		#endregion

		GameObject shadowRoot;
		readonly List<TransformPair> shadowTable = new List<TransformPair>();
		struct TransformPair {
			public Transform dest, src;
		}

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
				
				foreach (var sb in shadowBones) {
					if (sb.GetComponent<Rigidbody>() != null && sb.boneName == b.boneName) {
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
			var shadowRootRigidbody = shadowRoot.GetComponent<Rigidbody>();
			shadowRootRigidbody.MovePosition(transform.position);
			shadowRootRigidbody.MoveRotation(transform.rotation);

			for (int i = 0, n = shadowTable.Count; i < n; i++) {
				var pair = shadowTable[i];
				pair.dest.localPosition = pair.src.localPosition;
				pair.dest.localRotation = pair.src.localRotation;
			}
		}
	}
}
