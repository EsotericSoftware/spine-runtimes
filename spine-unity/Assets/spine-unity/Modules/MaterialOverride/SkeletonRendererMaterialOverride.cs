using System;
using System.Collections.Generic;
using Spine;
using UnityEngine;

[ExecuteInEditMode]
public class SkeletonRendererMaterialOverride : MonoBehaviour {
	public SkeletonRenderer skeletonRenderer;

	[SerializeField]
	private List<SlotMaterialOverride> slotMaterialOverrides = new List<SlotMaterialOverride>();

	[SerializeField]
	private List<AtlasMaterialOverride> atlasMaterialOverrides = new List<AtlasMaterialOverride>();

	public List<SlotMaterialOverride> SlotMaterialOverrides {
		get { return slotMaterialOverrides; }
	}

	public List<AtlasMaterialOverride> AtlasMaterialOverrides {
		get { return atlasMaterialOverrides; }
	}

	public void SetSlotMaterialOverrides() {
		if (skeletonRenderer == null) {
			Debug.LogError("skeletonRenderer == null");
			return;
		}

		for (int i = 0; i < slotMaterialOverrides.Count; i++) {
			SlotMaterialOverride slotMaterialOverride = slotMaterialOverrides[i];
			if (slotMaterialOverride.overrideDisabled || string.IsNullOrEmpty(slotMaterialOverride.slotName))
				continue;

			Slot slotObject = skeletonRenderer.skeleton.FindSlot(slotMaterialOverride.slotName);
			skeletonRenderer.CustomSlotMaterials[slotObject] = slotMaterialOverride.material;
		}
	}

	public void RemoveSlotMaterialOverrides() {
		if (skeletonRenderer == null) {
			Debug.LogError("skeletonRenderer == null");
			return;
		}

		for (int i = 0; i < slotMaterialOverrides.Count; i++) {
			SlotMaterialOverride slotMaterialOverride = slotMaterialOverrides[i];
			if (string.IsNullOrEmpty(slotMaterialOverride.slotName))
				continue;

			Slot slotObject = skeletonRenderer.skeleton.FindSlot(slotMaterialOverride.slotName);

			Material currentMaterial;
			if (!skeletonRenderer.CustomSlotMaterials.TryGetValue(slotObject, out currentMaterial))
				continue;

			if (currentMaterial != slotMaterialOverride.material)
				continue;

			skeletonRenderer.CustomSlotMaterials.Remove(slotObject);
		}
	}

	public void SetAtlasMaterialOverrides() {
		if (skeletonRenderer == null) {
			Debug.LogError("skeletonRenderer == null");
			return;
		}

		for (int i = 0; i < atlasMaterialOverrides.Count; i++) {
			AtlasMaterialOverride atlasMaterialOverride = atlasMaterialOverrides[i];
			if (atlasMaterialOverride.overrideDisabled)
				continue;

			skeletonRenderer.CustomAtlasMaterials[atlasMaterialOverride.originalMaterial] = atlasMaterialOverride.replacementMaterial;
		}
	}

	public void RemoveAtlasMaterialOverrides() {
		if (skeletonRenderer == null) {
			Debug.LogError("skeletonRenderer == null");
			return;
		}

		for (int i = 0; i < atlasMaterialOverrides.Count; i++) {
			AtlasMaterialOverride atlasMaterialOverride = atlasMaterialOverrides[i];
			Material currentMaterial;
			if (!skeletonRenderer.CustomAtlasMaterials.TryGetValue(atlasMaterialOverride.originalMaterial, out currentMaterial))
				continue;

			if (currentMaterial != atlasMaterialOverride.replacementMaterial)
				continue;

			skeletonRenderer.CustomAtlasMaterials.Remove(atlasMaterialOverride.originalMaterial);
		}
	}

	private void OnEnable() {
		SetAtlasMaterialOverrides();
		SetSlotMaterialOverrides();
	}

	private void OnDisable() {
		RemoveAtlasMaterialOverrides();
		RemoveSlotMaterialOverrides();
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

			atlasMaterialOverrides = initialAtlasMaterialOverrides;
		}
	}

	[Serializable]
	public class MaterialOverride {
		public bool overrideDisabled;
	}

	[Serializable]
	public class SlotMaterialOverride : MaterialOverride {
		[SpineSlot] public string slotName;
		public Material material;
	}

	[Serializable]
	public class AtlasMaterialOverride : MaterialOverride {
		public Material originalMaterial;
		public Material replacementMaterial;
	}
}