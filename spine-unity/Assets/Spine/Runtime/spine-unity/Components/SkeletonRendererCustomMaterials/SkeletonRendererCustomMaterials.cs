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

#if UNITY_2018_3 || UNITY_2019 || UNITY_2018_3_OR_NEWER
#define NEW_PREFAB_SYSTEM
#endif
#define SPINE_OPTIONAL_MATERIALOVERRIDE

// Contributed by: Lost Polygon

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity {
#if NEW_PREFAB_SYSTEM
	[ExecuteAlways]
#else
	[ExecuteInEditMode]
#endif
	[HelpURL("http://esotericsoftware.com/spine-unity#SkeletonRendererCustomMaterials")]
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
				AtlasAssetBase[] atlasAssets = skeletonRenderer.skeletonDataAsset.atlasAssets;

				List<AtlasMaterialOverride> initialAtlasMaterialOverrides = new List<AtlasMaterialOverride>();
				foreach (AtlasAssetBase atlasAsset in atlasAssets) {
					foreach (Material atlasMaterial in atlasAsset.Materials) {
						AtlasMaterialOverride atlasMaterialOverride = new AtlasMaterialOverride {
							overrideDisabled = true,
							originalMaterial = atlasMaterial
						};

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
				if (slotObject != null)
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
				if (slotObject == null)
					continue;
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
