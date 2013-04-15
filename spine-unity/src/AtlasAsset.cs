using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using Spine;

public class AtlasAsset : ScriptableObject {
	public TextAsset atlasFile;
	public Material material;
	private Atlas atlas;

	public void Clear () {
		atlas = null;
	}

	/// <returns>The atlas or null if it could not be loaded.</returns>
	public Atlas GetAtlas () {
		if (atlasFile == null) {
			Debug.LogWarning("Atlas file not set for atlas asset: " + name, this);
			Clear();
			return null;
		}

		if (material == null) {
			Debug.LogWarning("Material not set for atlas asset: " + name, this);
			Clear();
			return null;
		}

		if (atlas != null)
			return atlas;

		try {
			atlas = new Atlas(new StringReader(atlasFile.text), material, material.mainTexture.width, material.mainTexture.height);
			return atlas;
		} catch (Exception) {
			Debug.LogException(new Exception("Error reading atlas file for atlas asset: " + name), this);
			return null;
		}
	}
}
