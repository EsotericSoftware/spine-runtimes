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
	public class SkeletonUtilityEyeConstraint : SkeletonUtilityConstraint {
		public Transform[] eyes;
		public float radius = 0.5f;
		public Transform target;
		public Vector3 targetPosition;
		public float speed = 10;
		Vector3[] origins;
		Vector3 centerPoint;

		protected override void OnEnable () {
			if (!Application.isPlaying)
				return;

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
			if (!Application.isPlaying)
				return;

			base.OnDisable();
		}

		public override void DoUpdate () {

			if (target != null)
				targetPosition = target.position;

			Vector3 goal = targetPosition;

			Vector3 center = transform.TransformPoint(centerPoint);
			Vector3 dir = goal - center;

			if (dir.magnitude > 1)
				dir.Normalize();

			for (int i = 0; i < eyes.Length; i++) {
				center = transform.TransformPoint(origins[i]);
				eyes[i].position = Vector3.MoveTowards(eyes[i].position, center + (dir * radius), speed * Time.deltaTime);
			}

		}
	}
}
