/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Spine {
	public class XnaTextureLoader : TextureLoader {
		GraphicsDevice device;
		string[] textureLayerSuffixes = null;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="device">The graphics device to be used.</param>
		/// <param name="loadMultipleTextureLayers">If <c>true</c> multiple textures layers
		/// (e.g. a diffuse/albedo texture and a normal map) are loaded instead of a single texture.
		/// Names are constructed based on suffixes added according to the <c>textureSuffixes</c> parameter.</param>
		/// <param name="textureSuffixes">If <c>loadMultipleTextureLayers</c> is <c>true</c>, the strings of this array
		/// define the path name suffix of each layer to be loaded. Array size must be equal to the number of layers to be loaded.
		/// The first array entry is the suffix to be <c>replaced</c> (e.g. "_albedo", or "" for a first layer without a suffix),
		/// subsequent array entries contain the suffix to replace the first entry with (e.g. "_normals").
		///
		/// An example would be:
		/// <code>new string[] { "", "_normals" }</code> for loading a base diffuse texture named "skeletonname.png" and
		/// a normalmap named "skeletonname_normals.png".</param>
		public XnaTextureLoader (GraphicsDevice device, bool loadMultipleTextureLayers = false, string[] textureSuffixes = null) {
			this.device = device;
			if (loadMultipleTextureLayers)
				this.textureLayerSuffixes = textureSuffixes;
		}

		public void Load (AtlasPage page, String path) {
			Texture2D texture = Util.LoadTexture(device, path);
			page.width = texture.Width;
			page.height = texture.Height;

			if (textureLayerSuffixes == null) {
				page.rendererObject = texture;
			}
			else {
				Texture2D[] textureLayersArray = new Texture2D[textureLayerSuffixes.Length];
				textureLayersArray[0] = texture;
				for (int layer = 1; layer < textureLayersArray.Length; ++layer) {
					string layerPath = GetLayerName(path, textureLayerSuffixes[0], textureLayerSuffixes[layer]);
					textureLayersArray[layer] = Util.LoadTexture(device, layerPath);
				}
				page.rendererObject = textureLayersArray;
			}
		}

		public void Unload (Object texture) {
			((Texture2D)texture).Dispose();
		}

		private string GetLayerName (string firstLayerPath, string firstLayerSuffix, string replacementSuffix) {

			int suffixLocation = firstLayerPath.LastIndexOf(firstLayerSuffix + ".");
			if (suffixLocation == -1) {
				throw new Exception(string.Concat("Error composing texture layer name: first texture layer name '", firstLayerPath,
								"' does not contain suffix to be replaced: '", firstLayerSuffix, "'"));
			}
			return firstLayerPath.Remove(suffixLocation, firstLayerSuffix.Length).Insert(suffixLocation, replacementSuffix);
		}
	}
}
