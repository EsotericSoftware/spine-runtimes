/*****************************************************************************
 * SkeletonRendererCustomMaterials created by Lost Polygon
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity {
	[ExecuteInEditMode]
	public class SkeletonRendererCustomMaterials : MonoBehaviour {
		public SkeletonRenderer skeletonRenderer;

		[SerializeField]
		private List<SlotMaterialOverride> customSlotMaterials = new List<SlotMaterialOverride>();

		[SerializeField]
		private List<AtlasMaterialOverride> customMaterialOverrides = new List<AtlasMaterialOverride>();

		public List<SlotMaterialOverride> CustomSlotMaterials {
			get { return customSlotMaterials; }
		}

		public List<AtlasMaterialOverride> CustomMaterialOverrides {
			get { return customMaterialOverrides; }
		}

		public void SetCustomSlotMaterials() {
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

		public void RemoveCustomSlotMaterials() {
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

		public void SetCustomMaterialOverrides() {
			if (skeletonRenderer == null) {
				Debug.LogError("skeletonRenderer == null");
				return;
			}

			for (int i = 0; i < customMaterialOverrides.Count; i++) {
				AtlasMaterialOverride atlasMaterialOverride = customMaterialOverrides[i];
				if (atlasMaterialOverride.overrideDisabled)
					continue;

				skeletonRenderer.CustomMaterialOverride[atlasMaterialOverride.originalMaterial] = atlasMaterialOverride.replacementMaterial;
			}
		}

		public void RemoveCustomMaterialOverrides() {
			if (skeletonRenderer == null) {
				Debug.LogError("skeletonRenderer == null");
				return;
			}

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
		}

		private void OnEnable() {
			if (skeletonRenderer == null) {
				skeletonRenderer = GetComponent<SkeletonRenderer>();
			}

            skeletonRenderer.Initialize(false);
			SetCustomMaterialOverrides();
			SetCustomSlotMaterials();
		}

		private void OnDisable() {
			RemoveCustomMaterialOverrides();
			RemoveCustomSlotMaterials();
		}

		private void Reset() {
			skeletonRenderer = GetComponent<SkeletonRenderer>();

			// Populate atlas list
			if (skeletonRenderer != null && skeletonRenderer.skeletonDataAsset != null) {
				AtlasAsset[] atlasAssets = skeletonRenderer.skeletonDataAsset.atlasAssets;

				List<AtlasMaterialOverride> initialAtlasMaterialOverrides = new List<AtlasMaterialOverride>();
				foreach (AtlasAsset atlasAsset in atlasAssets) {
					foreach (Material atlasMaterial in atlasAsset.materials) {
						AtlasMaterialOverride atlasMaterialOverride = new AtlasMaterialOverride();
						atlasMaterialOverride.overrideDisabled = true;
						atlasMaterialOverride.originalMaterial = atlasMaterial;

						initialAtlasMaterialOverrides.Add(atlasMaterialOverride);
					}
				}

				customMaterialOverrides = initialAtlasMaterialOverrides;
			}
		}

		[Serializable]
		public class MaterialOverride {
			public bool overrideDisabled;
		}

		[Serializable]
		public class SlotMaterialOverride : MaterialOverride {
			[SpineSlot]
			public string slotName;

			public Material material;
		}

		[Serializable]
		public class AtlasMaterialOverride : MaterialOverride {
			public Material originalMaterial;
			public Material replacementMaterial;
		}
	}
}