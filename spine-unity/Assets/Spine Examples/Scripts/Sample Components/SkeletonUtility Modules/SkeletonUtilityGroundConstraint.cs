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

#if UNITY_2018_3 || UNITY_2019 || UNITY_2018_3_OR_NEWER
#define NEW_PREFAB_SYSTEM
#endif

using UnityEngine;

namespace Spine.Unity.Examples {

#if NEW_PREFAB_SYSTEM
	[ExecuteAlways]
#else
	[ExecuteInEditMode]
#endif
	[RequireComponent(typeof(SkeletonUtilityBone))]
	public class SkeletonUtilityGroundConstraint : SkeletonUtilityConstraint {

		[Tooltip("LayerMask for what objects to raycast against")]
		public LayerMask groundMask;
		[Tooltip("Use 2D")]
		public bool use2D = false;
		[Tooltip("Uses SphereCast for 3D mode and CircleCast for 2D mode")]
		public bool useRadius = false;
		[Tooltip("The Radius")]
		public float castRadius = 0.1f;
		[Tooltip("How high above the target bone to begin casting from")]
		public float castDistance = 5f;
		[Tooltip("X-Axis adjustment")]
		public float castOffset = 0;
		[Tooltip("Y-Axis adjustment")]
		public float groundOffset = 0;
		[Tooltip("How fast the target IK position adjusts to the ground. Use smaller values to prevent snapping")]
		public float adjustSpeed = 5;

		Vector3 rayOrigin;
		Vector3 rayDir = new Vector3(0, -1, 0);
		float hitY;
		float lastHitY;

		protected override void OnEnable () {
			base.OnEnable();
			lastHitY = transform.position.y;
		}

		public override void DoUpdate () {
			rayOrigin = transform.position + new Vector3(castOffset, castDistance, 0);

			float positionScale = hierarchy.PositionScale;
			float adjustDistanceThisFrame = adjustSpeed * positionScale * Time.deltaTime;
			hitY = float.MinValue;
			if (use2D) {
				RaycastHit2D hit;

				if (useRadius)
					hit = Physics2D.CircleCast(rayOrigin, castRadius, rayDir, castDistance + groundOffset, groundMask);
				else
					hit = Physics2D.Raycast(rayOrigin, rayDir, castDistance + groundOffset, groundMask);

				if (hit.collider != null) {
					hitY = hit.point.y + groundOffset;
					if (Application.isPlaying)
						hitY = Mathf.MoveTowards(lastHitY, hitY, adjustDistanceThisFrame);
				} else {
					if (Application.isPlaying)
						hitY = Mathf.MoveTowards(lastHitY, transform.position.y, adjustDistanceThisFrame);
				}
			} else {
				RaycastHit hit;
				bool validHit = false;

				if (useRadius)
					validHit = Physics.SphereCast(rayOrigin, castRadius, rayDir, out hit, castDistance + groundOffset, groundMask);
				else
					validHit = Physics.Raycast(rayOrigin, rayDir, out hit, castDistance + groundOffset, groundMask);

				if (validHit) {
					hitY = hit.point.y + groundOffset;
					if (Application.isPlaying)
						hitY = Mathf.MoveTowards(lastHitY, hitY, adjustDistanceThisFrame);

				} else {
					if (Application.isPlaying)
						hitY = Mathf.MoveTowards(lastHitY, transform.position.y, adjustDistanceThisFrame);
				}
			}

			Vector3 v = transform.position;
			v.y = Mathf.Clamp(v.y, Mathf.Min(lastHitY, hitY), float.MaxValue);
			transform.position = v;

			bone.bone.X = transform.localPosition.x / hierarchy.PositionScale;
			bone.bone.Y = transform.localPosition.y / hierarchy.PositionScale;

			lastHitY = hitY;
		}

		void OnDrawGizmos () {
			Vector3 hitEnd = rayOrigin + (rayDir * Mathf.Min(castDistance, rayOrigin.y - hitY));
			Vector3 clearEnd = rayOrigin + (rayDir * castDistance);
			Gizmos.DrawLine(rayOrigin, hitEnd);

			if (useRadius) {
				Gizmos.DrawLine(new Vector3(hitEnd.x - castRadius, hitEnd.y - groundOffset, hitEnd.z), new Vector3(hitEnd.x + castRadius, hitEnd.y - groundOffset, hitEnd.z));
				Gizmos.DrawLine(new Vector3(clearEnd.x - castRadius, clearEnd.y, clearEnd.z), new Vector3(clearEnd.x + castRadius, clearEnd.y, clearEnd.z));
			}

			Gizmos.color = Color.red;
			Gizmos.DrawLine(hitEnd, clearEnd);
		}
	}

}
