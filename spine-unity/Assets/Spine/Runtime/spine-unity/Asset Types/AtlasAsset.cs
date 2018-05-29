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

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Spine;

namespace Spine.Unity {
	/// <summary>Loads and stores a Spine atlas and list of materials.</summary>
	public class AtlasAsset : ScriptableObject {
		public TextAsset atlasFile;
		public Material[] materials;
		protected Atlas atlas;

		public bool IsLoaded { get { return this.atlas != null; } }

		#region Runtime Instantiation
		/// <summary>
		/// Creates a runtime AtlasAsset</summary>
		public static AtlasAsset CreateRuntimeInstance (TextAsset atlasText, Material[] materials, bool initialize) {
			AtlasAsset atlasAsset = ScriptableObject.CreateInstance<AtlasAsset>();
			atlasAsset.Reset();
			atlasAsset.atlasFile = atlasText;
			atlasAsset.materials = materials;

			if (initialize)
				atlasAsset.GetAtlas();

			return atlasAsset;
		}

		/// <summary>
		/// Creates a runtime AtlasAsset. Only providing the textures is slower because it has to search for atlas page matches. <seealso cref="Spine.Unity.AtlasAsset.CreateRuntimeInstance(TextAsset, Material[], bool)"/></summary>
		public static AtlasAsset CreateRuntimeInstance (TextAsset atlasText, Texture2D[] textures, Material materialPropertySource, bool initialize) {
			// Get atlas page names.
			string atlasString = atlasText.text;
			atlasString = atlasString.Replace("\r", "");
			string[] atlasLines = atlasString.Split('\n');
			var pages = new List<string>();
			for (int i = 0; i < atlasLines.Length - 1; i++) {
				if (atlasLines[i].Trim().Length == 0)
					pages.Add(atlasLines[i + 1].Trim().Replace(".png", ""));
			}

			// Populate Materials[] by matching texture names with page names.
			var materials = new Material[pages.Count];
			for (int i = 0, n = pages.Count; i < n; i++) {
				Material mat = null;

				// Search for a match.
				string pageName = pages[i];
				for (int j = 0, m = textures.Length; j < m; j++) {
					if (string.Equals(pageName, textures[j].name, System.StringComparison.OrdinalIgnoreCase)) {
						// Match found.
						mat = new Material(materialPropertySource);
						mat.mainTexture = textures[j];
						break;
					}
				}

				if (mat != null)
					materials[i] = mat;
				else
					throw new ArgumentException("Could not find matching atlas page in the texture array.");
			}

			// Create AtlasAsset normally
			return CreateRuntimeInstance(atlasText, materials, initialize);
		}

		/// <summary>
		/// Creates a runtime AtlasAsset. Only providing the textures is slower because it has to search for atlas page matches. <seealso cref="Spine.Unity.AtlasAsset.CreateRuntimeInstance(TextAsset, Material[], bool)"/></summary>
		public static AtlasAsset CreateRuntimeInstance (TextAsset atlasText, Texture2D[] textures, Shader shader, bool initialize) {
			if (shader == null)
				shader = Shader.Find("Spine/Skeleton");

			Material materialProperySource = new Material(shader);
			var oa = CreateRuntimeInstance(atlasText, textures, materialProperySource, initialize);

			return oa;
		}
		#endregion

		void Reset () {
			Clear();
		}

		public virtual void Clear () {
			atlas = null;
		}

		/// <returns>The atlas or null if it could not be loaded.</returns>
		public virtual Atlas GetAtlas () {
			if (atlasFile == null) {
				Debug.LogError("Atlas file not set for atlas asset: " + name, this);
				Clear();
				return null;
			}

			if (materials == null || materials.Length == 0) {
				Debug.LogError("Materials not set for atlas asset: " + name, this);
				Clear();
				return null;
			}

			if (atlas != null) return atlas;

			try {
				atlas = new Atlas(new StringReader(atlasFile.text), "", new MaterialsTextureLoader(this));
				atlas.FlipV();
				return atlas;
			} catch (Exception ex) {
				Debug.LogError("Error reading atlas file for atlas asset: " + name + "\n" + ex.Message + "\n" + ex.StackTrace, this);
				return null;
			}
		}

		public Mesh GenerateMesh (string name, Mesh mesh, out Material material, float scale = 0.01f) {
			AtlasRegion region = atlas.FindRegion(name);
			material = null;
			if (region != null) {
				if (mesh == null) {
					mesh = new Mesh();
					mesh.name = name;
				}

				Vector3[] verts = new Vector3[4];
				Vector2[] uvs = new Vector2[4];
				Color[] colors = { Color.white, Color.white, Color.white, Color.white };
				int[] triangles = { 0, 1, 2, 2, 3, 0 };

				float left, right, top, bottom;
				left = region.width / -2f;
				right = left * -1f;
				top = region.height / 2f;
				bottom = top * -1;

				verts[0] = new Vector3(left, bottom, 0) * scale;
				verts[1] = new Vector3(left, top, 0) * scale;
				verts[2] = new Vector3(right, top, 0) * scale;
				verts[3] = new Vector3(right, bottom, 0) * scale;
				float u, v, u2, v2;
				u = region.u;
				v = region.v;
				u2 = region.u2;
				v2 = region.v2;

				if (!region.rotate) {
					uvs[0] = new Vector2(u, v2);
					uvs[1] = new Vector2(u, v);
					uvs[2] = new Vector2(u2, v);
					uvs[3] = new Vector2(u2, v2);
				} else {
					uvs[0] = new Vector2(u2, v2);
					uvs[1] = new Vector2(u, v2);
					uvs[2] = new Vector2(u, v);
					uvs[3] = new Vector2(u2, v);
				}

				mesh.triangles = new int[0];
				mesh.vertices = verts;
				mesh.uv = uvs;
				mesh.colors = colors;
				mesh.triangles = triangles;
				mesh.RecalculateNormals();
				mesh.RecalculateBounds();

				material = (Material)region.page.rendererObject;
			} else {
				mesh = null;
			}

			return mesh;
		}
	}

	public class MaterialsTextureLoader : TextureLoader {
		AtlasAsset atlasAsset;

		public MaterialsTextureLoader (AtlasAsset atlasAsset) {
			this.atlasAsset = atlasAsset;
		}

		public void Load (AtlasPage page, string path) {
			String name = Path.GetFileNameWithoutExtension(path);
			Material material = null;
			foreach (Material other in atlasAsset.materials) {
				if (other.mainTexture == null) {
					Debug.LogError("Material is missing texture: " + other.name, other);
					return;
				}
				if (other.mainTexture.name == name) {
					material = other;
					break;
				}
			}
			if (material == null) {
				Debug.LogError("Material with texture name \"" + name + "\" not found for atlas asset: " + atlasAsset.name, atlasAsset);
				return;
			}
			page.rendererObject = material;

			// Very old atlas files expected the texture's actual size to be used at runtime.
			if (page.width == 0 || page.height == 0) {
				page.width = material.mainTexture.width;
				page.height = material.mainTexture.height;
			}
		}

		public void Unload (object texture) { }
	}
}
