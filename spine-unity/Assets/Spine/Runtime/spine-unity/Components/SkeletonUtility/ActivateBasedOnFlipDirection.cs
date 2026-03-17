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

#if !SPINE_AUTO_UPGRADE_COMPONENTS_OFF
#define AUTO_UPGRADE_TO_43_COMPONENTS
#endif

using UnityEngine;
using UnityEngine.Serialization;

namespace Spine.Unity {

	/// <summary>
	/// Utility component to support flipping of 2D hinge chains (chains of HingeJoint2D objects) along
	/// with the parent skeleton by activating the respective mirrored versions of the hinge chain.
	/// Note: This component is automatically attached when calling "Create Hinge Chain 2D" at <see cref="SkeletonUtilityBone"/>,
	/// do not attempt to use this component for other purposes.
	/// </summary>
	public class ActivateBasedOnFlipDirection : MonoBehaviour, IUpgradable {

		public SkeletonRenderer skeletonRenderer;
		public SkeletonGraphic skeletonGraphic;
		public GameObject activeOnNormalX;
		public GameObject activeOnFlippedX;
		HingeJoint2D[] jointsNormalX;
		HingeJoint2D[] jointsFlippedX;
		ISkeletonComponent skeletonComponent;

		bool wasFlippedXBefore = false;

#if UNITY_EDITOR && AUTO_UPGRADE_TO_43_COMPONENTS
		protected void Awake () {
			if (!Application.isPlaying && !wasUpgradedTo43) {
				UpgradeTo43();
			}
		}
#endif

		private void Start () {
			jointsNormalX = activeOnNormalX.GetComponentsInChildren<HingeJoint2D>();
			jointsFlippedX = activeOnFlippedX.GetComponentsInChildren<HingeJoint2D>();
			skeletonComponent = skeletonRenderer != null ? (ISkeletonComponent)skeletonRenderer : (ISkeletonComponent)skeletonGraphic;
		}

		private void FixedUpdate () {
			bool isFlippedX = (skeletonComponent.Skeleton.ScaleX < 0);
			if (isFlippedX != wasFlippedXBefore) {
				HandleFlip(isFlippedX);
			}
			wasFlippedXBefore = isFlippedX;
		}

		void HandleFlip (bool isFlippedX) {
			GameObject gameObjectToActivate = isFlippedX ? activeOnFlippedX : activeOnNormalX;
			GameObject gameObjectToDeactivate = isFlippedX ? activeOnNormalX : activeOnFlippedX;

			gameObjectToActivate.SetActive(true);
			gameObjectToDeactivate.SetActive(false);

			ResetJointPositions(isFlippedX ? jointsFlippedX : jointsNormalX);
			ResetJointPositions(isFlippedX ? jointsNormalX : jointsFlippedX);
			CompensateMovementAfterFlipX(gameObjectToActivate.transform, gameObjectToDeactivate.transform);
		}

		void ResetJointPositions (HingeJoint2D[] joints) {
			for (int i = 0; i < joints.Length; ++i) {
				HingeJoint2D joint = joints[i];
				Transform parent = joint.connectedBody.transform;
				joint.transform.position = parent.TransformPoint(joint.connectedAnchor);
			}
		}

		void CompensateMovementAfterFlipX (Transform toActivate, Transform toDeactivate) {
			Transform targetLocation = toDeactivate.GetChild(0);
			Transform currentLocation = toActivate.GetChild(0);
			toActivate.position += targetLocation.position - currentLocation.position;
		}

		#region Transfer of Deprecated Fields
#if UNITY_EDITOR && AUTO_UPGRADE_TO_43_COMPONENTS
		public virtual void UpgradeTo43 () {
			wasUpgradedTo43 = true;
			if (skeletonRenderer == null && skeletonGraphic == null) {
				Component previousReference = previousSkeletonRenderer != null ? previousSkeletonRenderer : this;
				skeletonRenderer = previousReference.GetComponent<SkeletonRenderer>();
				if (skeletonRenderer == null)
					Debug.LogError("Please manually re-assign SkeletonRenderer at ActivateBasedOnFlipDirection, " +
						"automatic upgrade failed.", this);
			}
		}
		[SerializeField, HideInInspector, FormerlySerializedAs("skeletonRenderer")] Component previousSkeletonRenderer;
		[SerializeField] protected bool wasUpgradedTo43 = false;
#endif
		#endregion
	}
}
