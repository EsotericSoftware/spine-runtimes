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
using Spine.Unity;

namespace Spine.Unity.Examples {
	public class RaggedySpineboy : MonoBehaviour {

		public LayerMask groundMask;
		public float restoreDuration = 0.5f;
		public Vector2 launchVelocity = new Vector2(50,100);

		Spine.Unity.Modules.SkeletonRagdoll2D ragdoll;
		Collider2D naturalCollider;

		void Start () {
			ragdoll = GetComponent<Spine.Unity.Modules.SkeletonRagdoll2D>();
			naturalCollider = GetComponent<Collider2D>();
		}

		void AddRigidbody () {
			var rb = gameObject.AddComponent<Rigidbody2D>();
			#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5
	        rb.freezeRotation = true;
			#else
			rb.fixedAngle = true;
			#endif
			naturalCollider.enabled = true;
		}

		void RemoveRigidbody () {
			Destroy(GetComponent<Rigidbody2D>());
			naturalCollider.enabled = false;
		}

		void OnMouseUp () {
			if (naturalCollider.enabled)
				Launch();
		}

		void Launch () {
			RemoveRigidbody();
			ragdoll.Apply();
			ragdoll.RootRigidbody.velocity = new Vector2(Random.Range(-launchVelocity.x, launchVelocity.x), launchVelocity.y);
			StartCoroutine(WaitUntilStopped());
		}

		IEnumerator Restore () {
			Vector3 estimatedPos = ragdoll.EstimatedSkeletonPosition;
			Vector3 rbPosition = ragdoll.RootRigidbody.position;

			Vector3 skeletonPoint = estimatedPos;
			RaycastHit2D hit = Physics2D.Raycast((Vector2)rbPosition, (Vector2)(estimatedPos - rbPosition), Vector3.Distance(estimatedPos, rbPosition), groundMask);
			if (hit.collider != null)
				skeletonPoint = hit.point;
			
			ragdoll.RootRigidbody.isKinematic = true;
			ragdoll.SetSkeletonPosition(skeletonPoint);

			yield return ragdoll.SmoothMix(0, restoreDuration);
			ragdoll.Remove();

			AddRigidbody();
		}

		IEnumerator WaitUntilStopped () {
			yield return new WaitForSeconds(0.5f);

			float t = 0;
			while (t < 0.5f) {
				t = (ragdoll.RootRigidbody.velocity.magnitude > 0.09f) ? 0 : t + Time.deltaTime;
				yield return null;
			}

			StartCoroutine(Restore());
		}
	}
}
