/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System.Collections;
using UnityEngine;

namespace Spine.Unity.Examples {
	public class SkeletonUtilityEyeConstraint : SkeletonUtilityConstraint {
		public Transform[] eyes;
		public float radius = 0.5f;
		public Transform target;
		public Vector3 targetPosition;
		public float speed = 10;
		Vector3[] origins;
		Vector3 centerPoint;

		protected override void OnEnable () {
			if (!Application.isPlaying) return;
			base.OnEnable();

			Bounds centerBounds = new Bounds(eyes[0].localPosition, Vector3.zero);
			origins = new Vector3[eyes.Length];
			for (int i = 0; i < eyes.Length; i++) {
				origins[i] = eyes[i].localPosition;
				centerBounds.Encapsulate(origins[i]);
			}

			centerPoint = centerBounds.center;
		}

		protected override void OnDisable () {
			if (!Application.isPlaying) return;

			for (int i = 0; i < eyes.Length; i++) {
				eyes[i].localPosition = origins[i];
			}
			base.OnDisable();
		}

		public override void DoUpdate () {
			if (target != null) targetPosition = target.position;

			Vector3 goal = targetPosition;
			Vector3 center = transform.TransformPoint(centerPoint);
			Vector3 dir = goal - center;

			if (dir.magnitude > 1)
				dir.Normalize();

			for (int i = 0; i < eyes.Length; i++) {
				center = transform.TransformPoint(origins[i]);
				eyes[i].position = Vector3.MoveTowards(eyes[i].position, center + (dir * radius * hierarchy.PositionScale),
					speed * hierarchy.PositionScale * Time.deltaTime);
			}

		}
	}
}
