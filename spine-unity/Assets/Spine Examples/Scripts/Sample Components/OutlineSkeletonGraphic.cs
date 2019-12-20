/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

using UnityEngine;
using Spine.Unity;

namespace Spine.Unity.Examples {

	public class OutlineSkeletonGraphic : MonoBehaviour {

		public SkeletonGraphic skeletonGraphic;
		public Material materialWithoutOutline;
		public Material materialWithOutline;

		#if UNITY_EDITOR
		void Reset () {
			skeletonGraphic = GetComponent<SkeletonGraphic>();

			// Add normal material as default
			if (skeletonGraphic != null && skeletonGraphic.skeletonDataAsset != null) {
				var atlasAssets = skeletonGraphic.skeletonDataAsset.atlasAssets;

				if (atlasAssets.Length > 0 && atlasAssets[0].PrimaryMaterial) {
					materialWithoutOutline = atlasAssets[0].PrimaryMaterial;
				}
			}
		}
		#endif

		void OnEnable () {
			if (skeletonGraphic == null)
				skeletonGraphic = GetComponent<SkeletonGraphic>();
		}

		public void EnableOutlineRendering () {
			skeletonGraphic.material = materialWithOutline;
		}

		public void  DisableOutlineRendering () {
			skeletonGraphic.material = materialWithoutOutline;
		}
	}
}
