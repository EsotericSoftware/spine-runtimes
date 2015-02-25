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
	MeshFilter filter;
	Material[] sharedMaterials;
	MeshFilter parentFilter;

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
		Debug.Log("Mesh: " + mesh);
	}

	public void Update () {
		if (mesh == null || mesh != parentFilter.sharedMesh) {
			mesh = parentFilter.sharedMesh;
			filter.sharedMesh = mesh;
		}

		if (cachedRenderer == null)
			cachedRenderer = GetComponent<Renderer>();

		if (mesh == null || submeshIndex > mesh.subMeshCount - 1) {
			cachedRenderer.enabled = false;
			return;
		} else {
			GetComponent<Renderer>().enabled = true;
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
