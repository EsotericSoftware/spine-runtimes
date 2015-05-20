using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SkeletonUtilitySubmeshRenderer : MonoBehaviour {
	public Renderer parentRenderer;
	[System.NonSerialized]
	public Mesh mesh;
	public int submeshIndex = 0;
	public int sortingOrder = 0;
	public int sortingLayerID = 0;
	public Material hiddenPassMaterial;
	Renderer cachedRenderer;
	SkeletonRenderer parentSkeletonRenderer;
	MeshFilter filter;
	Material[] sharedMaterials;
	MeshFilter parentFilter;
	bool isFirstUpdate = true;

	void Awake () {
		cachedRenderer = GetComponent<Renderer>();
		sharedMaterials = cachedRenderer.sharedMaterials;
		filter = GetComponent<MeshFilter>();

		if (parentRenderer != null)
			Initialize(parentRenderer);
	}

	void OnEnable () {
		parentRenderer = transform.parent.GetComponent<Renderer>();
		parentRenderer.GetComponent<SkeletonRenderer>().OnReset += HandleSkeletonReset;
	}

	void OnDisable () {
		parentRenderer.GetComponent<SkeletonRenderer>().OnReset -= HandleSkeletonReset;
	}

	void HandleSkeletonReset (SkeletonRenderer r) {
		if (parentRenderer != null)
			Initialize(parentRenderer);
	}

	public void Initialize (Renderer parentRenderer) {
		this.parentRenderer = parentRenderer;
		parentFilter = parentRenderer.GetComponent<MeshFilter>();
		mesh = parentFilter.sharedMesh;
		filter.sharedMesh = mesh;
		parentSkeletonRenderer = parentRenderer.GetComponent<SkeletonRenderer>();
		Debug.Log("Mesh: " + (mesh == null ? "null" : mesh.name), parentFilter);
	}

	public void Update () {
		if (cachedRenderer == null)
			cachedRenderer = GetComponent<Renderer>();

		if (isFirstUpdate || cachedRenderer.enabled) {
			parentSkeletonRenderer.RequestMeshUpdate();
			isFirstUpdate = false;
		}

		if (mesh == null || mesh != parentFilter.sharedMesh) {
			mesh = parentFilter.sharedMesh;
			filter.sharedMesh = mesh;
		}

		if (mesh == null || submeshIndex > mesh.subMeshCount - 1) {
			cachedRenderer.enabled = false;
			return;
		} else {
			cachedRenderer.enabled = true;
		}

		bool changed = false;

		if (sharedMaterials.Length != parentRenderer.sharedMaterials.Length) {
			sharedMaterials = parentRenderer.sharedMaterials;
			changed = true;
		}



		for (int i = 0; i < GetComponent<Renderer>().sharedMaterials.Length; i++) {
			if (i == submeshIndex)
				continue;

			if (sharedMaterials[i] != hiddenPassMaterial) {
				sharedMaterials[i] = hiddenPassMaterial;
				changed = true;
			}
		}

		if (sharedMaterials[submeshIndex] != parentRenderer.sharedMaterials[submeshIndex]) {
			sharedMaterials[submeshIndex] = parentRenderer.sharedMaterials[submeshIndex];
			changed = true;
		}

		if (changed) {
			cachedRenderer.sharedMaterials = sharedMaterials;
		}

		cachedRenderer.sortingLayerID = sortingLayerID;
		cachedRenderer.sortingOrder = sortingOrder;
	}
}
