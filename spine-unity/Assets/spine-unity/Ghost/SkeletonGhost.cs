/*****************************************************************************
 * SkeletonGhost created by Mitch Thompson
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SkeletonRenderer))]
public class SkeletonGhost : MonoBehaviour {
	public bool ghostingEnabled = true;
	public float spawnRate = 0.05f;
	public Color32 color = new Color32(0xFF, 0xFF, 0xFF, 0x00);
	[Tooltip("Remember to set color alpha to 0 if Additive is true")]
	public bool additive = true;
	public int maximumGhosts = 10;
	public float fadeSpeed = 10;
	public Shader ghostShader;
	[Tooltip("0 is Color and Alpha, 1 is Alpha only.")]
	[Range(0, 1)]
	public float textureFade = 1;

	float nextSpawnTime;
	SkeletonGhostRenderer[] pool;
	int poolIndex = 0;
	SkeletonRenderer skeletonRenderer;
	MeshRenderer meshRenderer;
	MeshFilter meshFilter;


	Dictionary<Material, Material> materialTable = new Dictionary<Material, Material>();

	void Start () {
		if (ghostShader == null)
			ghostShader = Shader.Find("Spine/SkeletonGhost");

		skeletonRenderer = GetComponent<SkeletonRenderer>();
		meshFilter = GetComponent<MeshFilter>();
		meshRenderer = GetComponent<MeshRenderer>();
		nextSpawnTime = Time.time + spawnRate;
		pool = new SkeletonGhostRenderer[maximumGhosts];
		for (int i = 0; i < maximumGhosts; i++) {
			GameObject go = new GameObject(gameObject.name + " Ghost", typeof(SkeletonGhostRenderer));
			pool[i] = go.GetComponent<SkeletonGhostRenderer>();
			go.SetActive(false);
			go.hideFlags = HideFlags.HideInHierarchy;
		}

		if (skeletonRenderer is SkeletonAnimation)
			((SkeletonAnimation)skeletonRenderer).state.Event += OnEvent;

	}

	//SkeletonAnimation
	/*
	 *	Int Value:		0 sets ghostingEnabled to false, 1 sets ghostingEnabled to true
	 *	Float Value:	Values greater than 0 set the spawnRate equal the float value
	 *	String Value:	Pass RGBA hex color values in to set the color property.  IE:   "A0FF8BFF"
	 */
	void OnEvent (Spine.AnimationState state, int trackIndex, Spine.Event e) {
		if (e.Data.Name == "Ghosting") {
			ghostingEnabled = e.Int > 0;
			if (e.Float > 0)
				spawnRate = e.Float;
			if (e.String != null) {
				this.color = HexToColor(e.String);
			}
		}
	}

	//SkeletonAnimator
	//SkeletonAnimator or Mecanim based animations only support toggling ghostingEnabled.  Be sure not to set anything other than the Int param in Spine or String will take priority.
	void Ghosting (float val) {
		ghostingEnabled = val > 0;
	}

	void Update () {
		if (!ghostingEnabled)
			return;

		if (Time.time >= nextSpawnTime) {
			GameObject go = pool[poolIndex].gameObject;

			Material[] materials = meshRenderer.sharedMaterials;
			for (int i = 0; i < materials.Length; i++) {
				var originalMat = materials[i];
				Material ghostMat;
				if (!materialTable.ContainsKey(originalMat)) {
					ghostMat = new Material(originalMat);
					ghostMat.shader = ghostShader;
					ghostMat.color = Color.white;
					if (ghostMat.HasProperty("_TextureFade"))
						ghostMat.SetFloat("_TextureFade", textureFade);
					materialTable.Add(originalMat, ghostMat);
				} else {
					ghostMat = materialTable[originalMat];
				}

				materials[i] = ghostMat;
			}

			pool[poolIndex].Initialize(meshFilter.sharedMesh, materials, color, additive, fadeSpeed, meshRenderer.sortingOrder - 1);
			go.transform.parent = transform;

			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;
			go.transform.localScale = Vector3.one;

			go.transform.parent = null;

			poolIndex++;

			if (poolIndex == pool.Length)
				poolIndex = 0;

			nextSpawnTime = Time.time + spawnRate;
		}
	}

	void OnDestroy () {
		for (int i = 0; i < maximumGhosts; i++) {
			if (pool[i] != null)
				pool[i].Cleanup();
		}

		foreach (var mat in materialTable.Values)
			Destroy(mat);
	}


	//based on UnifyWiki  http://wiki.unity3d.com/index.php?title=HexConverter
	static Color32 HexToColor (string hex) {
		if (hex.Length < 6)
			return Color.magenta;

		hex = hex.Replace("#", "");
		byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
		byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
		byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
		byte a = 0xFF;
		if (hex.Length == 8)
			a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);

		return new Color32(r, g, b, a);
	}
}
