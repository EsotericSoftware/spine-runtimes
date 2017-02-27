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

#define SPINE_OPTIONAL_MATERIALOVERRIDE

// Contributed by: Lost Polygon

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Modules {
	[ExecuteInEditMode]
	public class SkeletonRendererCustomMaterials : MonoBehaviour {

		#region Inspector
		public SkeletonRenderer skeletonRenderer;
		[SerializeField] protected List<SlotMaterialOverride> customSlotMaterials = new List<SlotMaterialOverride>();
		[SerializeField] protected List<AtlasMaterialOverride> customMaterialOverrides = new List<AtlasMaterialOverride>();

		#if UNITY_EDITOR
		void Reset () {
			skeletonRenderer = GetComponent<SkeletonRenderer>();

			// Populate atlas list 
			if (skeletonRenderer != null && skeletonRenderer.skeletonDataAsset != null) {
				AtlasAsset[] atlasAssets = skeletonRenderer.skeletonDataAsset.atlasAssets;

				var initialAtlasMaterialOverrides = new List<AtlasMaterialOverride>();
				foreach (AtlasAsset atlasAsset in atlasAssets) {
					foreach (Material atlasMaterial in atlasAsset.materials) {
						var atlasMaterialOverride = new AtlasMaterialOverride();
						atlasMaterialOverride.overrideDisabled = true;
						atlasMaterialOverride.originalMaterial = atlasMaterial;

						initialAtlasMaterialOverrides.Add(atlasMaterialOverride);
					}
				}

				customMaterialOverrides = initialAtlasMaterialOverrides;
			}
		}
		#endif
		#endregion

		void SetCustomSlotMaterials () {
			if (skeletonRenderer == null) {
				Debug.LogError("skeletonRenderer == null");
				return;
			}

			for (int i = 0; i < customSlotMaterials.Count; i++) {
				SlotMaterialOverride slotMaterialOverride = customSlotMaterials[i];
				if (slotMaterialOverride.overrideDisabled || string.IsNullOrEmpty(slotMaterialOverride.slotName))
					continue;

				Slot slotObject = skeletonRenderer.skeleton.FindSlot(slotMaterialOverride.slotName);
				skeletonRenderer.CustomSlotMaterials[slotObject] = slotMaterialOverride.material;
			}
		}

		void RemoveCustomSlotMaterials () {
			if (skeletonRenderer == null) {
				Debug.LogError("skeletonRenderer == null");
				return;
			}

			for (int i = 0; i < customSlotMaterials.Count; i++) {
				SlotMaterialOverride slotMaterialOverride = customSlotMaterials[i];
				if (string.IsNullOrEmpty(slotMaterialOverride.slotName))
					continue;

				Slot slotObject = skeletonRenderer.skeleton.FindSlot(slotMaterialOverride.slotName);

				Material currentMaterial;
				if (!skeletonRenderer.CustomSlotMaterials.TryGetValue(slotObject, out currentMaterial))
					continue;

				// Do not revert the material if it was changed by something else
				if (currentMaterial != slotMaterialOverride.material)
					continue;

				skeletonRenderer.CustomSlotMaterials.Remove(slotObject);
			}
		}

		void SetCustomMaterialOverrides () {
			if (skeletonRenderer == null) {
				Debug.LogError("skeletonRenderer == null");
				return;
			}

			#if SPINE_OPTIONAL_MATERIALOVERRIDE
			for (int i = 0; i < customMaterialOverrides.Count; i++) {
				AtlasMaterialOverride atlasMaterialOverride = customMaterialOverrides[i];
				if (atlasMaterialOverride.overrideDisabled)
					continue;

				skeletonRenderer.CustomMaterialOverride[atlasMaterialOverride.originalMaterial] = atlasMaterialOverride.replacementMaterial;
			}
			#endif
		}

		void RemoveCustomMaterialOverrides () {
			if (skeletonRenderer == null) {
				Debug.LogError("skeletonRenderer == null");
				return;
			}

			#if SPINE_OPTIONAL_MATERIALOVERRIDE
			for (int i = 0; i < customMaterialOverrides.Count; i++) {
				AtlasMaterialOverride atlasMaterialOverride = customMaterialOverrides[i];
				Material currentMaterial;

				if (!skeletonRenderer.CustomMaterialOverride.TryGetValue(atlasMaterialOverride.originalMaterial, out currentMaterial))
					continue;
				
				// Do not revert the material if it was changed by something else
				if (currentMaterial != atlasMaterialOverride.replacementMaterial)
					continue;

				skeletonRenderer.CustomMaterialOverride.Remove(atlasMaterialOverride.originalMaterial);
			}
			#endif
		}
			
		// OnEnable applies the overrides at runtime, and when the editor loads.
		void OnEnable () {
			if (skeletonRenderer == null)
				skeletonRenderer = GetComponent<SkeletonRenderer>();

			if (skeletonRenderer == null) {
				Debug.LogError("skeletonRenderer == null");
				return;
			}

			skeletonRenderer.Initialize(false);
			SetCustomMaterialOverrides();
			SetCustomSlotMaterials();
		}

		// OnDisable removes the overrides at runtime, and in the editor when the component is disabled or destroyed.
		void OnDisable () {
			if (skeletonRenderer == null) {
				Debug.LogError("skeletonRenderer == null");
				return;
			}

			RemoveCustomMaterialOverrides();
			RemoveCustomSlotMaterials();
		}

		[Serializable]
		public struct SlotMaterialOverride : IEquatable<SlotMaterialOverride> {
			public bool overrideDisabled;

			[SpineSlot]
			public string slotName;
			public Material material;

			public bool Equals (SlotMaterialOverride other) {
				return overrideDisabled == other.overrideDisabled && slotName == other.slotName && material == other.material;
			}
		}

		[Serializable]
		public struct AtlasMaterialOverride : IEquatable<AtlasMaterialOverride> {
			public bool overrideDisabled;
			public Material originalMaterial;
			public Material replacementMaterial;

			public bool Equals (AtlasMaterialOverride other) {
				return overrideDisabled == other.overrideDisabled && originalMaterial == other.originalMaterial && replacementMaterial == other.replacementMaterial;
			}
		}
	}
}
