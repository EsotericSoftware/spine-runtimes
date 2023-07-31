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
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Examples {
	public class MaterialReplacementExample : MonoBehaviour {

		public Material originalMaterial;
		public Material replacementMaterial;
		public bool replacementEnabled = true;
		public SkeletonAnimation skeletonAnimation;

		[Space]
		public string phasePropertyName = "_FillPhase";
		[Range(0f, 1f)] public float phase = 1f;

		bool previousEnabled;
		MaterialPropertyBlock mpb;

		void Start () {
			// Use the code below to programmatically query the original material.
			// Note: using MeshRenderer.material will fail since it creates an instance copy of the Material,
			// MeshRenderer.sharedMaterial might also fail when called too early or when no Attachments
			// are visible in the initial first frame.
			if (originalMaterial == null)
				originalMaterial = skeletonAnimation.SkeletonDataAsset.atlasAssets[0].PrimaryMaterial;

			previousEnabled = replacementEnabled;
			SetReplacementEnabled(replacementEnabled);
			mpb = new MaterialPropertyBlock();
		}

		void Update () {
			mpb.SetFloat(phasePropertyName, phase);
			GetComponent<MeshRenderer>().SetPropertyBlock(mpb);

			if (previousEnabled != replacementEnabled)
				SetReplacementEnabled(replacementEnabled);

			previousEnabled = replacementEnabled;

		}

		void SetReplacementEnabled (bool active) {
			if (replacementEnabled) {
				skeletonAnimation.CustomMaterialOverride[originalMaterial] = replacementMaterial;
			} else {
				skeletonAnimation.CustomMaterialOverride.Remove(originalMaterial);
			}
		}

	}
}
