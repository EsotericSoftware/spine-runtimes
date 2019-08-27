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
using Spine;
using Spine.Unity;

namespace Spine.Unity.Examples {
	public class BoneLocalOverride : MonoBehaviour {
		[SpineBone]
		public string boneName;

		[Space]
		[Range(0, 1)] public float alpha = 1;

		[Space]
		public bool overridePosition = true;
		public Vector2 localPosition;

		[Space]
		public bool overrideRotation = true;
		[Range(0, 360)] public float rotation = 0;

		ISkeletonAnimation spineComponent;
		Bone bone;

		#if UNITY_EDITOR
		void OnValidate () {
			if (Application.isPlaying) return;
			spineComponent = spineComponent ?? GetComponent<ISkeletonAnimation>();
			if (spineComponent == null) return;
			if (bone != null) bone.SetToSetupPose();
			OverrideLocal(spineComponent);
		}
		#endif

		void Awake () {
			spineComponent = GetComponent<ISkeletonAnimation>();
			if (spineComponent == null) { this.enabled = false; return; }
			spineComponent.UpdateLocal += OverrideLocal;

			if (bone == null) {	this.enabled = false; return; }
		}

		void OverrideLocal (ISkeletonAnimation animated) {
			if (bone == null || bone.Data.Name != boneName) {
				if (string.IsNullOrEmpty(boneName)) return;
				bone = spineComponent.Skeleton.FindBone(boneName);
				if (bone == null) {
					Debug.LogFormat("Cannot find bone: '{0}'", boneName);
					return;
				}
			}

			if (overridePosition) {
				bone.X = Mathf.Lerp(bone.X, localPosition.x, alpha);
				bone.Y = Mathf.Lerp(bone.Y, localPosition.y, alpha);
			}

			if (overrideRotation)
				bone.Rotation = Mathf.Lerp(bone.Rotation, rotation, alpha);
		}

	}
}
