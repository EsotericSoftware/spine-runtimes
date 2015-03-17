using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SkeletonUtilitySubmeshRenderer : MonoBehaviour {
	[System.NonSerialized]
	public Mesh mesh;
	public int submeshIndex = 0;
	public Material hiddenPassMaterial;
	Renderer cachedRenderer;
	MeshFilter filter;
	Material[] sharedMaterials;

	void Awake () {
		cachedRenderer = GetComponent<Renderer>();
		filter = GetComponent<MeshFilter>();
		sharedMaterials = new Material[0];
	}

	public void SetMesh (Renderer parentRenderer, Mesh mesh, Material mat) {
		if (cachedRenderer == null)
			return;

		cachedRenderer.enabled = true;
		filter.sharedMesh = mesh;
		if (cachedRenderer.sharedMaterials.Length != parentRenderer.sharedMaterials.Length) {
			sharedMaterials = parentRenderer.sharedMaterials;
		}

		for (int i = 0; i < sharedMaterials.Length; i++) {
			if (i == submeshIndex)
				sharedMaterials[i] = mat;
			else
				sharedMaterials[i] = hiddenPassMaterial;
		}

		cachedRenderer.sharedMaterials = sharedMaterials;
	}
}
