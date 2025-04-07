/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

using Spine.Unity;
using UnityEngine;

namespace Spine.Unity.Examples {

	public class RootMotionDeltaCompensation : MonoBehaviour {

		[SerializeField] protected SkeletonRootMotionBase rootMotion;
		public Transform targetPosition;
		public int trackIndex = 0;
		public bool adjustX = true;
		public bool adjustY = true;
		public float minScaleX = -999;
		public float minScaleY = -999;
		public float maxScaleX = 999;
		public float maxScaleY = 999;

		public bool allowXTranslation = false;
		public bool allowYTranslation = true;

		void Start () {
			if (rootMotion == null)
				rootMotion = this.GetComponent<SkeletonRootMotionBase>();
		}

		void Update () {
			AdjustDelta();
		}

		void OnDisable () {
			if (adjustX)
				rootMotion.rootMotionScaleX = 1;
			if (adjustY)
				rootMotion.rootMotionScaleY = 1;
			if (allowXTranslation)
				rootMotion.rootMotionTranslateXPerY = 0;
			if (allowYTranslation)
				rootMotion.rootMotionTranslateYPerX = 0;
		}

		void AdjustDelta () {
			Vector3 toTarget = targetPosition.position - this.transform.position;
			rootMotion.AdjustRootMotionToDistance(toTarget, trackIndex, adjustX, adjustY,
				minScaleX, maxScaleX, minScaleY, maxScaleY,
				allowXTranslation, allowYTranslation);
		}
	}
}
