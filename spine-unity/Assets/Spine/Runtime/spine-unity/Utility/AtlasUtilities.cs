/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Spine.Unity.AttachmentTools {

	public static class AtlasUtilities {
		internal const TextureFormat SpineTextureFormat = TextureFormat.RGBA32;
		internal const float DefaultMipmapBias = -0.5f;
		internal const bool UseMipMaps = false;
		internal const float DefaultScale = 0.01f;

		const int NonrenderingRegion = -1;

		public static AtlasRegion ToAtlasRegion (this Texture2D t, Material materialPropertySource, float scale = DefaultScale) {
			return t.ToAtlasRegion(materialPropertySource.shader, scale, materialPropertySource);
		}

		public static AtlasRegion ToAtlasRegion (this Texture2D t, Shader shader, float scale = DefaultScale, Material materialPropertySource = null) {
			var material = new Material(shader);
			if (materialPropertySource != null) {
				material.CopyPropertiesFromMaterial(materialPropertySource);
				material.shaderKeywords = materialPropertySource.shaderKeywords;
			}

			material.mainTexture = t;
			var page = material.ToSpineAtlasPage();

			float width = t.width;
			float height = t.height;

			var region = new AtlasRegion();
			region.name = t.name;
			region.index = -1;
			region.rotate = false;

			// World space units
			Vector2 boundsMin = Vector2.zero, boundsMax = new Vector2(width, height) * scale;

			// Texture space/pixel units
			region.width = (int)width;
			region.originalWidth = (int)width;
			region.height = (int)height;
			region.originalHeight = (int)height;
			region.offsetX = width * (0.5f - InverseLerp(boundsMin.x, boundsMax.x, 0));
			region.offsetY = height * (0.5f - InverseLerp(boundsMin.y, boundsMax.y, 0));

			// Use the full area of the texture.
			region.u = 0;
			region.v = 1;
			region.u2 = 1;
			region.v2 = 0;
			region.x = 0;
			region.y = 0;

			region.page = page;

			return region;
		}

		/// <summary>
		/// Creates a Spine.AtlasRegion that uses a premultiplied alpha duplicate of the Sprite's texture data.</summary>
		public static AtlasRegion ToAtlasRegionPMAClone (this Texture2D t, Material materialPropertySource, TextureFormat textureFormat = SpineTextureFormat, bool mipmaps = UseMipMaps) {
			return t.ToAtlasRegionPMAClone(materialPropertySource.shader, textureFormat, mipmaps, materialPropertySource);
		}

		/// <summary>
		/// Creates a Spine.AtlasRegion that uses a premultiplied alpha duplicate of the Sprite's texture data.</summary>
		public static AtlasRegion ToAtlasRegionPMAClone (this Texture2D t, Shader shader, TextureFormat textureFormat = SpineTextureFormat, bool mipmaps = UseMipMaps, Material materialPropertySource = null) {
			var material = new Material(shader);
			if (materialPropertySource != null) {
				material.CopyPropertiesFromMaterial(materialPropertySource);
				material.shaderKeywords = materialPropertySource.shaderKeywords;
			}
			var newTexture = t.GetClone(textureFormat, mipmaps);
			newTexture.ApplyPMA(true);

			newTexture.name = t.name + "-pma-";
			material.name = t.name + shader.name;

			material.mainTexture = newTexture;
			var page = material.ToSpineAtlasPage();

			var region = newTexture.ToAtlasRegion(shader);
			region.page = page;

			return region;
		}

		/// <summary>
		/// Creates a new Spine.AtlasPage from a UnityEngine.Material. If the material has a preassigned texture, the page width and height will be set.</summary>
		public static AtlasPage ToSpineAtlasPage (this Material m) {
			var newPage = new AtlasPage {
				rendererObject = m,
				name = m.name
			};

			var t = m.mainTexture;
			if (t != null) {
				newPage.width = t.width;
				newPage.height = t.height;
			}

			return newPage;
		}

		/// <summary>
		/// Creates a Spine.AtlasRegion from a UnityEngine.Sprite.</summary>
		public static AtlasRegion ToAtlasRegion (this Sprite s, AtlasPage page) {
			if (page == null) throw new System.ArgumentNullException("page", "page cannot be null. AtlasPage determines which texture region belongs and how it should be rendered. You can use material.ToSpineAtlasPage() to get a shareable AtlasPage from a Material, or use the sprite.ToAtlasRegion(material) overload.");
			var region = s.ToAtlasRegion();
			region.page = page;
			return region;
		}

		/// <summary>
		/// Creates a Spine.AtlasRegion from a UnityEngine.Sprite. This creates a new AtlasPage object for every AtlasRegion you create. You can centralize Material control by creating a shared atlas page using Material.ToSpineAtlasPage and using the sprite.ToAtlasRegion(AtlasPage) overload.</summary>
		public static AtlasRegion ToAtlasRegion (this Sprite s, Material material) {
			var region = s.ToAtlasRegion();
			region.page = material.ToSpineAtlasPage();
			return region;
		}

		public static AtlasRegion ToAtlasRegionPMAClone (this Sprite s, Material materialPropertySource, TextureFormat textureFormat = SpineTextureFormat, bool mipmaps = UseMipMaps) {
			return s.ToAtlasRegionPMAClone(materialPropertySource.shader, textureFormat, mipmaps, materialPropertySource);
		}

		/// <summary>
		/// Creates a Spine.AtlasRegion that uses a premultiplied alpha duplicate of the Sprite's texture data.</summary>
		public static AtlasRegion ToAtlasRegionPMAClone (this Sprite s, Shader shader, TextureFormat textureFormat = SpineTextureFormat, bool mipmaps = UseMipMaps, Material materialPropertySource = null) {
			var material = new Material(shader);
			if (materialPropertySource != null) {
				material.CopyPropertiesFromMaterial(materialPropertySource);
				material.shaderKeywords = materialPropertySource.shaderKeywords;
			}

			var tex = s.ToTexture(textureFormat, mipmaps);
			tex.ApplyPMA(true);

			tex.name = s.name + "-pma-";
			material.name = tex.name + shader.name;

			material.mainTexture = tex;
			var page = material.ToSpineAtlasPage();

			var region = s.ToAtlasRegion(true);
			region.page = page;

			return region;
		}

		internal static AtlasRegion ToAtlasRegion (this Sprite s, bool isolatedTexture = false) {
			var region = new AtlasRegion();
			region.name = s.name;
			region.index = -1;
			region.rotate = s.packed && s.packingRotation != SpritePackingRotation.None;

			// World space units
			Bounds bounds = s.bounds;
			Vector2 boundsMin = bounds.min, boundsMax = bounds.max;

			// Texture space/pixel units
			Rect spineRect = s.rect.SpineUnityFlipRect(s.texture.height);
			region.width = (int)spineRect.width;
			region.originalWidth = (int)spineRect.width;
			region.height = (int)spineRect.height;
			region.originalHeight = (int)spineRect.height;
			region.offsetX = spineRect.width * (0.5f - InverseLerp(boundsMin.x, boundsMax.x, 0));
			region.offsetY = spineRect.height * (0.5f - InverseLerp(boundsMin.y, boundsMax.y, 0));

			if (isolatedTexture) {
				region.u = 0;
				region.v = 1;
				region.u2 = 1;
				region.v2 = 0;
				region.x = 0;
				region.y = 0;
			} else {
				Texture2D tex = s.texture;
				Rect uvRect = TextureRectToUVRect(s.textureRect, tex.width, tex.height);
				region.u = uvRect.xMin;
				region.v = uvRect.yMax;
				region.u2 = uvRect.xMax;
				region.v2 = uvRect.yMin;
				region.x = (int)spineRect.x;
				region.y = (int)spineRect.y;
			}

			return region;
		}

		#region Runtime Repacking
		/// <summary>
		/// Fills the outputAttachments list with new attachment objects based on the attachments in sourceAttachments, but mapped to a new single texture using the same material.</summary>
		/// <param name="sourceAttachments">The list of attachments to be repacked.</param>
		/// <param name = "outputAttachments">The List(Attachment) to populate with the newly created Attachment objects.</param>
		///
		/// <param name="materialPropertySource">May be null. If no Material property source is provided, no special </param>
		public static void GetRepackedAttachments (List<Attachment> sourceAttachments, List<Attachment> outputAttachments, Material materialPropertySource, out Material outputMaterial, out Texture2D outputTexture, int maxAtlasSize = 1024, int padding = 2, TextureFormat textureFormat = SpineTextureFormat, bool mipmaps = UseMipMaps, string newAssetName = "Repacked Attachments", bool clearCache = false, bool useOriginalNonrenderables = true) {
			if (sourceAttachments == null) throw new System.ArgumentNullException("sourceAttachments");
			if (outputAttachments == null) throw new System.ArgumentNullException("outputAttachments");

			// Use these to detect and use shared regions.
			var existingRegions = new Dictionary<AtlasRegion, int>();
			var regionIndexes = new List<int>();
			var texturesToPack = new List<Texture2D>();
			var originalRegions = new List<AtlasRegion>();

			outputAttachments.Clear();
			outputAttachments.AddRange(sourceAttachments);

			int newRegionIndex = 0;
			for (int i = 0, n = sourceAttachments.Count; i < n; i++) {
				var originalAttachment = sourceAttachments[i];

				if (IsRenderable(originalAttachment)) {
					var newAttachment = originalAttachment.GetCopy(true);
					var region = newAttachment.GetRegion();
					int existingIndex;
					if (existingRegions.TryGetValue(region, out existingIndex)) {
						regionIndexes.Add(existingIndex); // Store the region index for the eventual new attachment.
					} else {
						originalRegions.Add(region);
						texturesToPack.Add(region.ToTexture()); // Add the texture to the PackTextures argument
						existingRegions.Add(region, newRegionIndex); // Add the region to the dictionary of known regions
						regionIndexes.Add(newRegionIndex); // Store the region index for the eventual new attachment.
						newRegionIndex++;
					}

					outputAttachments[i] = newAttachment;
				} else {
					outputAttachments[i] = useOriginalNonrenderables ? originalAttachment : originalAttachment.GetCopy(true);
					regionIndexes.Add(NonrenderingRegion); // Output attachments pairs with regionIndexes list 1:1. Pad with a sentinel if the attachment doesn't have a region.
				}
			}

			// Fill a new texture with the collected attachment textures.
			var newTexture = new Texture2D(maxAtlasSize, maxAtlasSize, textureFormat, mipmaps);
			newTexture.mipMapBias = AtlasUtilities.DefaultMipmapBias;
			newTexture.name = newAssetName;
			// Copy settings
			if (texturesToPack.Count > 0) {
				var sourceTexture = texturesToPack[0];
				newTexture.CopyTextureAttributesFrom(sourceTexture);
			}
			var rects = newTexture.PackTextures(texturesToPack.ToArray(), padding, maxAtlasSize);

			// Rehydrate the repacked textures as a Material, Spine atlas and Spine.AtlasAttachments
			Shader shader = materialPropertySource == null ? Shader.Find("Spine/Skeleton") : materialPropertySource.shader;
			var newMaterial = new Material(shader);
			if (materialPropertySource != null) {
				newMaterial.CopyPropertiesFromMaterial(materialPropertySource);
				newMaterial.shaderKeywords = materialPropertySource.shaderKeywords;
			}

			newMaterial.name = newAssetName;
			newMaterial.mainTexture = newTexture;
			var page = newMaterial.ToSpineAtlasPage();
			page.name = newAssetName;

			var repackedRegions = new List<AtlasRegion>();
			for (int i = 0, n = originalRegions.Count; i < n; i++) {
				var oldRegion = originalRegions[i];
				var newRegion = UVRectToAtlasRegion(rects[i], oldRegion, page);
				repackedRegions.Add(newRegion);
			}

			// Map the cloned attachments to the repacked atlas.
			for (int i = 0, n = outputAttachments.Count; i < n; i++) {
				var a = outputAttachments[i];
				if (IsRenderable(a))
					a.SetRegion(repackedRegions[regionIndexes[i]]);
			}

			// Clean up.
			if (clearCache)
				AtlasUtilities.ClearCache();

			outputTexture = newTexture;
			outputMaterial = newMaterial;
		}

		/// <summary>
		/// Creates and populates a duplicate skin with cloned attachments that are backed by a new packed texture atlas
		/// comprised of all the regions from the original skin.</summary>
		/// <remarks>GetRepackedSkin is an expensive operation, preferably call it at level load time.
		/// No Spine.Atlas object is created so there is no way to find AtlasRegions except through the Attachments using them.</remarks>
		/// <param name="additionalTexturePropertyIDsToCopy">Optional additional textures (such as normal maps) to copy while repacking.
		/// To copy e.g. the main texture and normal maps, pass 'new int[] { Shader.PropertyToID("_BumpMap") }' at this parameter.</param>
		/// <param name="additionalOutputTextures">When <c>additionalTexturePropertyIDsToCopy</c> is non-null,
		/// this array will be filled with the resulting repacked texture for every property,
		/// just as the main repacked texture is assigned to <c>outputTexture</c>.</param>
		public static Skin GetRepackedSkin (this Skin o, string newName, Material materialPropertySource, out Material outputMaterial, out Texture2D outputTexture,
			int maxAtlasSize = 1024, int padding = 2, TextureFormat textureFormat = SpineTextureFormat, bool mipmaps = UseMipMaps,
			bool useOriginalNonrenderables = true, bool clearCache = false,
			int[] additionalTexturePropertyIDsToCopy = null, Texture2D[] additionalOutputTextures = null) {

			return GetRepackedSkin(o, newName, materialPropertySource.shader, out outputMaterial, out outputTexture,
				maxAtlasSize, padding, textureFormat, mipmaps, materialPropertySource,
				clearCache, useOriginalNonrenderables, additionalTexturePropertyIDsToCopy, additionalOutputTextures);
		}

		/// <summary>
		/// Creates and populates a duplicate skin with cloned attachments that are backed by a new packed texture atlas
		/// comprised of all the regions from the original skin.</summary>
		/// <remarks>GetRepackedSkin is an expensive operation, preferably call it at level load time.
		/// No Spine.Atlas object is created so there is no way to find AtlasRegions except through the Attachments using them.</remarks>
		public static Skin GetRepackedSkin (this Skin o, string newName, Shader shader, out Material outputMaterial, out Texture2D outputTexture,
			int maxAtlasSize = 1024, int padding = 2, TextureFormat textureFormat = SpineTextureFormat, bool mipmaps = UseMipMaps,
			Material materialPropertySource = null, bool clearCache = false, bool useOriginalNonrenderables = true,
			int[] additionalTexturePropertyIDsToCopy = null, Texture2D[] additionalOutputTextures = null) {

			outputTexture = null;

			if (o == null) throw new System.NullReferenceException("Skin was null");
			var skinAttachments = o.Attachments;
			var newSkin = new Skin(newName);

			newSkin.bones.AddRange(o.bones);
			newSkin.constraints.AddRange(o.constraints);

			// Use these to detect and use shared regions.
			var existingRegions = new Dictionary<AtlasRegion, int>();
			var regionIndexes = new List<int>();

			// Collect all textures from the attachments of the original skin.
			var repackedAttachments = new List<Attachment>();
			int numTextureParamsToRepack = 1 + (additionalTexturePropertyIDsToCopy == null ? 0 : additionalTexturePropertyIDsToCopy.Length);
			additionalOutputTextures = (additionalTexturePropertyIDsToCopy == null ? null : new Texture2D[additionalTexturePropertyIDsToCopy.Length]);
			List<Texture2D>[] texturesToPackAtParam = new List<Texture2D>[numTextureParamsToRepack];
			for (int i = 0; i < numTextureParamsToRepack; ++i) {
				texturesToPackAtParam[i] = new List<Texture2D>();
			}
			var originalRegions = new List<AtlasRegion>();
			int newRegionIndex = 0;

			foreach (var skinEntry in skinAttachments) {
				var originalKey = skinEntry.Key;
				var originalAttachment = skinEntry.Value;

				Attachment newAttachment;
				if (IsRenderable(originalAttachment)) {
					newAttachment = originalAttachment.GetCopy(true);
					var region = newAttachment.GetRegion();
					int existingIndex;
					if (existingRegions.TryGetValue(region, out existingIndex)) {
						regionIndexes.Add(existingIndex); // Store the region index for the eventual new attachment.
					} else {
						originalRegions.Add(region);
						for (int i = 0; i < numTextureParamsToRepack; ++i) {
							Texture2D regionTexture = (i == 0 ? region.ToTexture() : region.ToTexture(texturePropertyId : additionalTexturePropertyIDsToCopy[i - 1]));
							texturesToPackAtParam[i].Add(regionTexture); // Add the texture to the PackTextures argument
						}
						existingRegions.Add(region, newRegionIndex); // Add the region to the dictionary of known regions
						regionIndexes.Add(newRegionIndex); // Store the region index for the eventual new attachment.
						newRegionIndex++;
					}

					repackedAttachments.Add(newAttachment);
					newSkin.SetAttachment(originalKey.SlotIndex, originalKey.Name, newAttachment);
				} else {
					newSkin.SetAttachment(originalKey.SlotIndex, originalKey.Name, useOriginalNonrenderables ? originalAttachment : originalAttachment.GetCopy(true));
				}
			}

			// Rehydrate the repacked textures as a Material, Spine atlas and Spine.AtlasAttachments
			var newMaterial = new Material(shader);
			if (materialPropertySource != null) {
				newMaterial.CopyPropertiesFromMaterial(materialPropertySource);
				newMaterial.shaderKeywords = materialPropertySource.shaderKeywords;
			}
			newMaterial.name = newName;

			Rect[] rects = null;
			for (int i = 0; i < numTextureParamsToRepack; ++i) {
				// Fill a new texture with the collected attachment textures.
				var newTexture = new Texture2D(maxAtlasSize, maxAtlasSize, textureFormat, mipmaps);
				newTexture.mipMapBias = AtlasUtilities.DefaultMipmapBias;
				var texturesToPack = texturesToPackAtParam[i];
				if (texturesToPack.Count > 0) {
					var sourceTexture = texturesToPack[0];
					newTexture.CopyTextureAttributesFrom(sourceTexture);
				}
				newTexture.name = newName;
				var rectsForTexParam = newTexture.PackTextures(texturesToPack.ToArray(), padding, maxAtlasSize);
				if (i == 0) {
					rects = rectsForTexParam;
					newMaterial.mainTexture = newTexture;
					outputTexture = newTexture;
				}
				else {
					newMaterial.SetTexture(additionalTexturePropertyIDsToCopy[i - 1], newTexture);
					additionalOutputTextures[i - 1] = newTexture;
				}
			}

			var page = newMaterial.ToSpineAtlasPage();
			page.name = newName;

			var repackedRegions = new List<AtlasRegion>();
			for (int i = 0, n = originalRegions.Count; i < n; i++) {
				var oldRegion = originalRegions[i];
				var newRegion = UVRectToAtlasRegion(rects[i], oldRegion, page);
				repackedRegions.Add(newRegion);
			}

			// Map the cloned attachments to the repacked atlas.
			for (int i = 0, n = repackedAttachments.Count; i < n; i++) {
				var a = repackedAttachments[i];
				if (IsRenderable(a))
					a.SetRegion(repackedRegions[regionIndexes[i]]);
			}

			// Clean up.
			if (clearCache)
				AtlasUtilities.ClearCache();

			outputMaterial = newMaterial;
			return newSkin;
		}

		public static Sprite ToSprite (this AtlasRegion ar, float pixelsPerUnit = 100) {
			return Sprite.Create(ar.GetMainTexture(), ar.GetUnityRect(), new Vector2(0.5f, 0.5f), pixelsPerUnit);
		}

		struct IntAndAtlasRegionKey {
			int i;
			AtlasRegion region;

			public IntAndAtlasRegionKey(int i, AtlasRegion region) {
				this.i = i;
				this.region = region;
			}
		}
		static Dictionary<IntAndAtlasRegionKey, Texture2D> CachedRegionTextures = new Dictionary<IntAndAtlasRegionKey, Texture2D>();
		static List<Texture2D> CachedRegionTexturesList = new List<Texture2D>();

		public static void ClearCache () {
			foreach (var t in CachedRegionTexturesList) {
				UnityEngine.Object.Destroy(t);
			}
			CachedRegionTextures.Clear();
			CachedRegionTexturesList.Clear();
		}

		/// <summary>Creates a new Texture2D object based on an AtlasRegion.
		/// If applyImmediately is true, Texture2D.Apply is called immediately after the Texture2D is filled with data.</summary>
		public static Texture2D ToTexture (this AtlasRegion ar, TextureFormat textureFormat = SpineTextureFormat, bool mipmaps = UseMipMaps,
			int texturePropertyId = 0) {

			Texture2D output;

			IntAndAtlasRegionKey cacheKey = new IntAndAtlasRegionKey(texturePropertyId, ar);
			CachedRegionTextures.TryGetValue(cacheKey, out output);
			if (output == null) {
				Texture2D sourceTexture = texturePropertyId == 0 ? ar.GetMainTexture() : ar.GetTexture(texturePropertyId);
				Rect r = ar.GetUnityRect();
				int width = (int)r.width;
				int height = (int)r.height;
				output = new Texture2D(width, height, textureFormat, mipmaps) { name = ar.name };
				output.CopyTextureAttributesFrom(sourceTexture);
				AtlasUtilities.CopyTexture(sourceTexture, r, output);
				CachedRegionTextures.Add(cacheKey, output);
				CachedRegionTexturesList.Add(output);
			}

			return output;
		}

		static Texture2D ToTexture (this Sprite s, TextureFormat textureFormat = SpineTextureFormat, bool mipmaps = UseMipMaps) {
			var spriteTexture = s.texture;
			var r = s.textureRect;
			var newTexture = new Texture2D((int)r.width, (int)r.height, textureFormat, mipmaps);
			newTexture.CopyTextureAttributesFrom(spriteTexture);
			AtlasUtilities.CopyTexture(spriteTexture, r, newTexture);
			return newTexture;
		}

		static Texture2D GetClone (this Texture2D t, TextureFormat textureFormat = SpineTextureFormat, bool mipmaps = UseMipMaps) {
			var newTexture = new Texture2D((int)t.width, (int)t.height, textureFormat, mipmaps);
			newTexture.CopyTextureAttributesFrom(t);
			AtlasUtilities.CopyTexture(t, new Rect(0, 0, t.width, t.height), newTexture);
			return newTexture;
		}

		static void CopyTexture (Texture2D source, Rect sourceRect, Texture2D destination) {
			if (SystemInfo.copyTextureSupport == UnityEngine.Rendering.CopyTextureSupport.None) {
				// GetPixels fallback for old devices.
				Color[] pixelBuffer = source.GetPixels((int)sourceRect.x, (int)sourceRect.y, (int)sourceRect.width, (int)sourceRect.height);
				destination.SetPixels(pixelBuffer);
				destination.Apply();
			} else {
				Graphics.CopyTexture(source, 0, 0, (int)sourceRect.x, (int)sourceRect.y, (int)sourceRect.width, (int)sourceRect.height, destination, 0, 0, 0, 0);
			}
		}

		static bool IsRenderable (Attachment a) {
			return a is IHasRendererObject;
		}

		/// <summary>
		/// Get a rect with flipped Y so that a Spine atlas rect gets converted to a Unity Sprite rect and vice versa.</summary>
		static Rect SpineUnityFlipRect (this Rect rect, int textureHeight) {
			rect.y = textureHeight - rect.y - rect.height;
			return rect;
		}

		/// <summary>
		/// Gets the Rect of an AtlasRegion according to Unity texture coordinates (x-right, y-up).
		/// This overload relies on region.page.height being correctly set.</summary>
		static Rect GetUnityRect (this AtlasRegion region) {
			return region.GetSpineAtlasRect().SpineUnityFlipRect(region.page.height);
		}

		/// <summary>
		/// Gets the Rect of an AtlasRegion according to Unity texture coordinates (x-right, y-up).</summary>
		static Rect GetUnityRect (this AtlasRegion region, int textureHeight) {
			return region.GetSpineAtlasRect().SpineUnityFlipRect(textureHeight);
		}

		/// <summary>
		/// Returns a Rect of the AtlasRegion according to Spine texture coordinates. (x-right, y-down)</summary>
		static Rect GetSpineAtlasRect (this AtlasRegion region, bool includeRotate = true) {
			if (includeRotate && region.rotate)
				return new Rect(region.x, region.y, region.height, region.width);
			else
				return new Rect(region.x, region.y, region.width, region.height);
		}

		/// <summary>
		/// Denormalize a uvRect into a texture-space Rect.</summary>
		static Rect UVRectToTextureRect (Rect uvRect, int texWidth, int texHeight) {
			uvRect.x *= texWidth;
			uvRect.width *= texWidth;
			uvRect.y *= texHeight;
			uvRect.height *= texHeight;
			return uvRect;
		}

		/// <summary>
		/// Normalize a texture Rect into UV coordinates.</summary>
		static Rect TextureRectToUVRect (Rect textureRect, int texWidth, int texHeight) {
			textureRect.x = Mathf.InverseLerp(0, texWidth, textureRect.x);
			textureRect.y = Mathf.InverseLerp(0, texHeight, textureRect.y);
			textureRect.width = Mathf.InverseLerp(0, texWidth, textureRect.width);
			textureRect.height = Mathf.InverseLerp(0, texHeight, textureRect.height);
			return textureRect;
		}

		/// <summary>
		/// Creates a new Spine AtlasRegion according to a Unity UV Rect (x-right, y-up, uv-normalized).</summary>
		static AtlasRegion UVRectToAtlasRegion (Rect uvRect, AtlasRegion referenceRegion, AtlasPage page) {
			var tr = UVRectToTextureRect(uvRect, page.width, page.height);
			var rr = tr.SpineUnityFlipRect(page.height);

			int x = (int)rr.x, y = (int)rr.y;
			int w, h;
			if (referenceRegion.rotate) {
				w = (int)rr.height;
				h = (int)rr.width;
			} else {
				w = (int)rr.width;
				h = (int)rr.height;
			}

			int originalW = Mathf.RoundToInt((float)w * ((float)referenceRegion.originalWidth / (float)referenceRegion.width));
			int originalH = Mathf.RoundToInt((float)h * ((float)referenceRegion.originalHeight / (float)referenceRegion.height));
			int offsetX = Mathf.RoundToInt((float)referenceRegion.offsetX * ((float)w / (float)referenceRegion.width));
			int offsetY = Mathf.RoundToInt((float)referenceRegion.offsetY * ((float)h / (float)referenceRegion.height));

			return new AtlasRegion {
				page = page,
				name = referenceRegion.name,

				u = uvRect.xMin,
				u2 = uvRect.xMax,
				v = uvRect.yMax,
				v2 = uvRect.yMin,

				index = -1,

				width = w,
				originalWidth = originalW,
				height = h,
				originalHeight = originalH,
				offsetX = offsetX,
				offsetY = offsetY,
				x = x,
				y = y,

				rotate = referenceRegion.rotate
			};
		}

		/// <summary>
		/// Convenience method for getting the main texture of the material of the page of the region.</summary>
		static Texture2D GetMainTexture (this AtlasRegion region) {
			var material = (region.page.rendererObject as Material);
			return material.mainTexture as Texture2D;
		}

		/// <summary>
		/// Convenience method for getting any texture of the material of the page of the region by texture property name.</summary>
		static Texture2D GetTexture (this AtlasRegion region, string texturePropertyName) {
			var material = (region.page.rendererObject as Material);
			return material.GetTexture(texturePropertyName) as Texture2D;
		}

		/// <summary>
		/// Convenience method for getting any texture of the material of the page of the region by texture property id.</summary>
		static Texture2D GetTexture (this AtlasRegion region, int texturePropertyId) {
			var material = (region.page.rendererObject as Material);
			return material.GetTexture(texturePropertyId) as Texture2D;
		}

		static void CopyTextureAttributesFrom(this Texture2D destination, Texture2D source) {
			destination.filterMode = source.filterMode;
			destination.anisoLevel = source.anisoLevel;
		#if UNITY_EDITOR
			destination.alphaIsTransparency = source.alphaIsTransparency;
		#endif
			destination.wrapModeU = source.wrapModeU;
			destination.wrapModeV = source.wrapModeV;
			destination.wrapModeW = source.wrapModeW;
		}

		static void ApplyPMA (this Texture2D texture, bool applyImmediately = true) {
			var pixels = texture.GetPixels();
			for (int i = 0, n = pixels.Length; i < n; i++) {
				Color p = pixels[i];
				float a = p.a;
				p.r = p.r * a;
				p.g = p.g * a;
				p.b = p.b * a;
				pixels[i] = p;
			}
			texture.SetPixels(pixels);
			if (applyImmediately)
				texture.Apply();
		}
		#endregion

		static float InverseLerp (float a, float b, float value) {
			return (value - a) / (b - a);
		}
	}
}
