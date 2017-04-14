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
using System.Collections;

namespace Spine.Unity.Modules {
	[RequireComponent(typeof(SkeletonUtilityBone)), ExecuteInEditMode]
	public class SkeletonUtilityGroundConstraint : SkeletonUtilityConstraint {

		[Tooltip("LayerMask for what objects to raycast against")]
		public LayerMask groundMask;
		[Tooltip("The 2D")]
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
		[Tooltip("How fast the target IK position adjusts to the ground.  Use smaller values to prevent snapping")]
		public float adjustSpeed = 5;

		Vector3 rayOrigin;
		Vector3 rayDir = new Vector3(0, -1, 0);
		float hitY;
		float lastHitY;

		protected override void OnEnable () {
			base.OnEnable();
			lastHitY = transform.position.y;
		}

		protected override void OnDisable () {
			base.OnDisable();
		}

		public override void DoUpdate () {
			rayOrigin = transform.position + new Vector3(castOffset, castDistance, 0);

			hitY = float.MinValue;
			if (use2D) {
				RaycastHit2D hit;

				if (useRadius) {
					hit = Physics2D.CircleCast(rayOrigin, castRadius, rayDir, castDistance + groundOffset, groundMask);
				} else {
					hit = Physics2D.Raycast(rayOrigin, rayDir, castDistance + groundOffset, groundMask);
				}

				if (hit.collider != null) {
					hitY = hit.point.y + groundOffset;
					if (Application.isPlaying) {
						hitY = Mathf.MoveTowards(lastHitY, hitY, adjustSpeed * Time.deltaTime);
					}
				} else {
					if (Application.isPlaying)
						hitY = Mathf.MoveTowards(lastHitY, transform.position.y, adjustSpeed * Time.deltaTime);
				}
			} else {
				RaycastHit hit;
				bool validHit = false;

				if (useRadius) {
					validHit = Physics.SphereCast(rayOrigin, castRadius, rayDir, out hit, castDistance + groundOffset, groundMask);
				} else {
					validHit = Physics.Raycast(rayOrigin, rayDir, out hit, castDistance + groundOffset, groundMask);
				}

				if (validHit) {
					hitY = hit.point.y + groundOffset;
					if (Application.isPlaying) {
						hitY = Mathf.MoveTowards(lastHitY, hitY, adjustSpeed * Time.deltaTime);
					}
				} else {
					if (Application.isPlaying)
						hitY = Mathf.MoveTowards(lastHitY, transform.position.y, adjustSpeed * Time.deltaTime);
				}
			}

			Vector3 v = transform.position;
			v.y = Mathf.Clamp(v.y, Mathf.Min(lastHitY, hitY), float.MaxValue);
			transform.position = v;

			utilBone.bone.X = transform.localPosition.x;
			utilBone.bone.Y = transform.localPosition.y;

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
