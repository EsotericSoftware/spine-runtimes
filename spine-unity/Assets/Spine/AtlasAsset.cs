/******************************************************************************
 * Spine Runtime Software License - Version 1.0
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Single User License or Spine Professional License must be
 *    purchased from Esoteric Software and the license must remain valid:
 *    http://esotericsoftware.com/
 * 2. Redistributions of source code must retain this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer.
 * 3. Redistributions in binary form must reproduce this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer, in the documentation and/or other materials provided with the
 *    distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using System.IO;
using UnityEngine;
using Spine;

public class AtlasAsset : ScriptableObject {
	public TextAsset atlasFile;
	public Material[] materials;
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

		if (materials == null || materials.Length == 0) {
			Debug.LogWarning("Materials not set for atlas asset: " + name, this);
			Clear();
			return null;
		}

		if (atlas != null)
			return atlas;

		try {
			atlas = new Atlas(new StringReader(atlasFile.text), "", new MaterialsTextureLoader(this));
			return atlas;
		} catch (Exception ex) {
			Debug.Log("Error reading atlas file for atlas asset: " + name + "\n" + ex.Message + "\n" + ex.StackTrace, this);
			return null;
		}
	}
}

public class MaterialsTextureLoader : TextureLoader {
	AtlasAsset atlasAsset;
	
	public MaterialsTextureLoader (AtlasAsset atlasAsset) {
		this.atlasAsset = atlasAsset;
	}
	
	public void Load (AtlasPage page, String path) {
		String name = Path.GetFileNameWithoutExtension(path);
		Material material = null;
		foreach (Material other in atlasAsset.materials) {
			if (other.mainTexture.name == name) {
				material = other;
				break;
			}
		}
		if (material == null) {
			Debug.LogWarning("Material with texture name \"" + name + "\" not found for atlas asset: " + atlasAsset.name, atlasAsset);
			return;
		}
		page.rendererObject = material;
		page.width = material.mainTexture.width;
		page.height = material.mainTexture.height;
	}

	public void Unload (object texture) {
	}
}
