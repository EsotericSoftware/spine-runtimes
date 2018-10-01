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

using UnityEngine;
using System.Collections.Generic;

namespace Spine.Unity.Modules.AttachmentTools {
	public static class AttachmentRegionExtensions {
		#region GetRegion
		/// <summary>
		/// Tries to get the region (image) of a renderable attachment. If the attachment is not renderable, it returns null.</summary>
		public static AtlasRegion GetRegion (this Attachment attachment) {
			var renderableAttachment = attachment as IHasRendererObject;
			if (renderableAttachment != null)
				return renderableAttachment.RendererObject as AtlasRegion;

			return null;
		}

		/// <summary>Gets the region (image) of a RegionAttachment</summary>
		public static AtlasRegion GetRegion (this RegionAttachment regionAttachment) {
			return regionAttachment.RendererObject as AtlasRegion;
		}

		/// <summary>Gets the region (image) of a MeshAttachment</summary>
		public static AtlasRegion GetRegion (this MeshAttachment meshAttachment) {
			return meshAttachment.RendererObject as AtlasRegion;
		}
		#endregion
		#region SetRegion
		/// <summary>
		/// Tries to set the region (image) of a renderable attachment. If the attachment is not renderable, nothing is applied.</summary>
		public static void SetRegion (this Attachment attachment, AtlasRegion region, bool updateOffset = true) {
			var regionAttachment = attachment as RegionAttachment;
			if (regionAttachment != null)
				regionAttachment.SetRegion(region, updateOffset);

			var meshAttachment = attachment as MeshAttachment;
			if (meshAttachment != null)
				meshAttachment.SetRegion(region, updateOffset);
		}

		/// <summary>Sets the region (image) of a RegionAttachment</summary>
		public static void SetRegion (this RegionAttachment attachment, AtlasRegion region, bool updateOffset = true) {
			if (region == null) throw new System.ArgumentNullException("region"); 

			// (AtlasAttachmentLoader.cs)
			attachment.RendererObject = region;
			attachment.SetUVs(region.u, region.v, region.u2, region.v2, region.rotate);
			attachment.regionOffsetX = region.offsetX;
			attachment.regionOffsetY = region.offsetY;
			attachment.regionWidth = region.width;
			attachment.regionHeight = region.height;
			attachment.regionOriginalWidth = region.originalWidth;
			attachment.regionOriginalHeight = region.originalHeight;

			if (updateOffset) attachment.UpdateOffset();
		}

		/// <summary>Sets the region (image) of a MeshAttachment</summary>
		public static void SetRegion (this MeshAttachment attachment, AtlasRegion region, bool updateUVs = true) {
			if (region == null) throw new System.ArgumentNullException("region"); 

			// (AtlasAttachmentLoader.cs)
			attachment.RendererObject = region;
			attachment.RegionU = region.u;
			attachment.RegionV = region.v;
			attachment.RegionU2 = region.u2;
			attachment.RegionV2 = region.v2;
			attachment.RegionRotate = region.rotate;
			attachment.regionOffsetX = region.offsetX;
			attachment.regionOffsetY = region.offsetY;
			attachment.regionWidth = region.width;
			attachment.regionHeight = region.height;
			attachment.regionOriginalWidth = region.originalWidth;
			attachment.regionOriginalHeight = region.originalHeight;

			if (updateUVs) attachment.UpdateUVs();
		}
		#endregion

		#region Runtime RegionAttachments
		/// <summary>
		/// Creates a RegionAttachment based on a sprite. This method creates a real, usable AtlasRegion. That AtlasRegion uses a new AtlasPage with the Material provided./// </summary>
		public static RegionAttachment ToRegionAttachment (this Sprite sprite, Material material, float rotation = 0f) {
			return sprite.ToRegionAttachment(material.ToSpineAtlasPage(), rotation);
		}

		/// <summary>
		/// Creates a RegionAttachment based on a sprite. This method creates a real, usable AtlasRegion. That AtlasRegion uses the AtlasPage provided./// </summary>
		public static RegionAttachment ToRegionAttachment (this Sprite sprite, AtlasPage page, float rotation = 0f) {
			if (sprite == null) throw new System.ArgumentNullException("sprite");
			if (page == null) throw new System.ArgumentNullException("page");
			var region = sprite.ToAtlasRegion(page);
			var unitsPerPixel = 1f / sprite.pixelsPerUnit;
			return region.ToRegionAttachment(sprite.name, unitsPerPixel, rotation);
		}

		/// <summary>
		/// Creates a Spine.AtlasRegion that uses a premultiplied alpha duplicate texture of the Sprite's texture data. Returns a RegionAttachment that uses it. Use this if you plan to use a premultiply alpha shader such as "Spine/Skeleton"</summary>
		public static RegionAttachment ToRegionAttachmentPMAClone (this Sprite sprite, Shader shader, TextureFormat textureFormat = AtlasUtilities.SpineTextureFormat, bool mipmaps = AtlasUtilities.UseMipMaps, Material materialPropertySource = null, float rotation = 0f) {
			if (sprite == null) throw new System.ArgumentNullException("sprite");
			if (shader == null) throw new System.ArgumentNullException("shader");
			var region = sprite.ToAtlasRegionPMAClone(shader, textureFormat, mipmaps, materialPropertySource);
			var unitsPerPixel = 1f / sprite.pixelsPerUnit;
			return region.ToRegionAttachment(sprite.name, unitsPerPixel, rotation);
		}

		public static RegionAttachment ToRegionAttachmentPMAClone (this Sprite sprite, Material materialPropertySource, TextureFormat textureFormat = AtlasUtilities.SpineTextureFormat, bool mipmaps = AtlasUtilities.UseMipMaps, float rotation = 0f) {
			return sprite.ToRegionAttachmentPMAClone(materialPropertySource.shader, textureFormat, mipmaps, materialPropertySource, rotation);
		}

		/// <summary>
		/// Creates a new RegionAttachment from a given AtlasRegion.</summary>
		public static RegionAttachment ToRegionAttachment (this AtlasRegion region, string attachmentName, float scale = 0.01f, float rotation = 0f) {
			if (string.IsNullOrEmpty(attachmentName)) throw new System.ArgumentException("attachmentName can't be null or empty.", "attachmentName");
			if (region == null) throw new System.ArgumentNullException("region");

			// (AtlasAttachmentLoader.cs)
			var attachment = new RegionAttachment(attachmentName);

			attachment.RendererObject = region;
			attachment.SetUVs(region.u, region.v, region.u2, region.v2, region.rotate);
			attachment.regionOffsetX = region.offsetX;
			attachment.regionOffsetY = region.offsetY;
			attachment.regionWidth = region.width;
			attachment.regionHeight = region.height;
			attachment.regionOriginalWidth = region.originalWidth;
			attachment.regionOriginalHeight = region.originalHeight;

			attachment.Path = region.name;
			attachment.scaleX = 1;
			attachment.scaleY = 1;
			attachment.rotation = rotation;

			attachment.r = 1;
			attachment.g = 1;
			attachment.b = 1;
			attachment.a = 1;

			// pass OriginalWidth and OriginalHeight because UpdateOffset uses it in its calculation.
			attachment.width = attachment.regionOriginalWidth * scale;
			attachment.height = attachment.regionOriginalHeight * scale;

			attachment.SetColor(Color.white);
			attachment.UpdateOffset();
			return attachment;
		}

		/// <summary> Sets the scale. Call regionAttachment.UpdateOffset to apply the change.</summary>
		public static void SetScale (this RegionAttachment regionAttachment, Vector2 scale) {
			regionAttachment.scaleX = scale.x;
			regionAttachment.scaleY = scale.y;
		}

		/// <summary> Sets the scale. Call regionAttachment.UpdateOffset to apply the change.</summary>
		public static void SetScale (this RegionAttachment regionAttachment, float x, float y) {
			regionAttachment.scaleX = x;
			regionAttachment.scaleY = y;
		}

		/// <summary> Sets the position offset. Call regionAttachment.UpdateOffset to apply the change.</summary>
		public static void SetPositionOffset (this RegionAttachment regionAttachment, Vector2 offset) {
			regionAttachment.x = offset.x;
			regionAttachment.y = offset.y;
		}

		/// <summary> Sets the position offset. Call regionAttachment.UpdateOffset to apply the change.</summary>
		public static void SetPositionOffset (this RegionAttachment regionAttachment, float x, float y) {
			regionAttachment.x = x;
			regionAttachment.y = y;
		}

		/// <summary> Sets the rotation. Call regionAttachment.UpdateOffset to apply the change.</summary>
		public static void SetRotation (this RegionAttachment regionAttachment, float rotation) {
			regionAttachment.rotation = rotation;
		}
		#endregion
	}

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
			var newTexture = t.GetClone(false, textureFormat, mipmaps);
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

		/// <summary>
		/// Creates a Spine.AtlasRegion that uses a premultiplied alpha duplicate of the Sprite's texture data.</summary>
		public static AtlasRegion ToAtlasRegionPMAClone (this Sprite s, Shader shader, TextureFormat textureFormat = SpineTextureFormat, bool mipmaps = UseMipMaps, Material materialPropertySource = null) {
			var material = new Material(shader);
			if (materialPropertySource != null) {
				material.CopyPropertiesFromMaterial(materialPropertySource);
				material.shaderKeywords = materialPropertySource.shaderKeywords;
			}

			var tex = s.ToTexture(false, textureFormat, mipmaps);
			tex.ApplyPMA(true);

			tex.name = s.name + "-pma-";
			material.name = tex.name + shader.name;

			material.mainTexture = tex;
			var page = material.ToSpineAtlasPage();

			var region = s.ToAtlasRegion(true);
			region.page = page;

			return region;
		}

		public static AtlasRegion ToAtlasRegionPMAClone (this Sprite s, Material materialPropertySource, TextureFormat textureFormat = SpineTextureFormat, bool mipmaps = UseMipMaps) {
			return s.ToAtlasRegionPMAClone(materialPropertySource.shader, textureFormat, mipmaps, materialPropertySource);
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
					var newAttachment = originalAttachment.GetClone(true);
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
					outputAttachments[i] = useOriginalNonrenderables ? originalAttachment : originalAttachment.GetClone(true);
					regionIndexes.Add(NonrenderingRegion); // Output attachments pairs with regionIndexes list 1:1. Pad with a sentinel if the attachment doesn't have a region.
				}
			}

			// Fill a new texture with the collected attachment textures.
			var newTexture = new Texture2D(maxAtlasSize, maxAtlasSize, textureFormat, mipmaps);
			newTexture.mipMapBias = AtlasUtilities.DefaultMipmapBias;
			newTexture.anisoLevel = texturesToPack[0].anisoLevel;
			newTexture.name = newAssetName;
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
				var newRegion = UVRectToAtlasRegion(rects[i], oldRegion.name, page, oldRegion.offsetX, oldRegion.offsetY, oldRegion.rotate);
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
		/// Creates and populates a duplicate skin with cloned attachments that are backed by a new packed texture atlas comprised of all the regions from the original skin.</summary>
		/// <remarks>No Spine.Atlas object is created so there is no way to find AtlasRegions except through the Attachments using them.</remarks>
		public static Skin GetRepackedSkin (this Skin o, string newName, Material materialPropertySource, out Material outputMaterial, out Texture2D outputTexture, int maxAtlasSize = 1024, int padding = 2, TextureFormat textureFormat = SpineTextureFormat, bool mipmaps = UseMipMaps, bool useOriginalNonrenderables = true) {
			return GetRepackedSkin(o, newName, materialPropertySource.shader, out outputMaterial, out outputTexture, maxAtlasSize, padding, textureFormat, mipmaps, materialPropertySource, useOriginalNonrenderables);
		}

		/// <summary>
		/// Creates and populates a duplicate skin with cloned attachments that are backed by a new packed texture atlas comprised of all the regions from the original skin.</summary>
		/// <remarks>No Spine.Atlas object is created so there is no way to find AtlasRegions except through the Attachments using them.</remarks>
		public static Skin GetRepackedSkin (this Skin o, string newName, Shader shader, out Material outputMaterial, out Texture2D outputTexture, int maxAtlasSize = 1024, int padding = 2, TextureFormat textureFormat = SpineTextureFormat, bool mipmaps = UseMipMaps, Material materialPropertySource = null, bool clearCache = false, bool useOriginalNonrenderables = true) {
			if (o == null) throw new System.NullReferenceException("Skin was null");
			var skinAttachments = o.Attachments;
			var newSkin = new Skin(newName);

			// Use these to detect and use shared regions.
			var existingRegions = new Dictionary<AtlasRegion, int>();
			var regionIndexes = new List<int>();

			// Collect all textures from the attachments of the original skin.
			var repackedAttachments = new List<Attachment>();
			var texturesToPack = new List<Texture2D>();
			var originalRegions = new List<AtlasRegion>();
			int newRegionIndex = 0;
			foreach (var skinEntry in skinAttachments) {
				var originalKey = skinEntry.Key;
				var originalAttachment = skinEntry.Value;

				Attachment newAttachment;
				if (IsRenderable(originalAttachment)) {
					newAttachment = originalAttachment.GetClone(true);
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

					repackedAttachments.Add(newAttachment);
					newSkin.AddAttachment(originalKey.slotIndex, originalKey.name, newAttachment);
				} else {
					newSkin.AddAttachment(originalKey.slotIndex, originalKey.name, useOriginalNonrenderables ? originalAttachment : originalAttachment.GetClone(true));
				}	
			}

			// Fill a new texture with the collected attachment textures.
			var newTexture = new Texture2D(maxAtlasSize, maxAtlasSize, textureFormat, mipmaps);
			newTexture.mipMapBias = AtlasUtilities.DefaultMipmapBias;
			newTexture.anisoLevel = texturesToPack[0].anisoLevel;
			newTexture.name = newName;
			var rects = newTexture.PackTextures(texturesToPack.ToArray(), padding, maxAtlasSize);

			// Rehydrate the repacked textures as a Material, Spine atlas and Spine.AtlasAttachments
			var newMaterial = new Material(shader);
			if (materialPropertySource != null) {
				newMaterial.CopyPropertiesFromMaterial(materialPropertySource);
				newMaterial.shaderKeywords = materialPropertySource.shaderKeywords;
			}

			newMaterial.name = newName;
			newMaterial.mainTexture = newTexture;
			var page = newMaterial.ToSpineAtlasPage();
			page.name = newName;

			var repackedRegions = new List<AtlasRegion>();
			for (int i = 0, n = originalRegions.Count; i < n; i++) {
				var oldRegion = originalRegions[i];
				var newRegion = UVRectToAtlasRegion(rects[i], oldRegion.name, page, oldRegion.offsetX, oldRegion.offsetY, oldRegion.rotate);
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

			outputTexture = newTexture;
			outputMaterial = newMaterial;
			return newSkin;
		}

		public static Sprite ToSprite (this AtlasRegion ar, float pixelsPerUnit = 100) {
			return Sprite.Create(ar.GetMainTexture(), ar.GetUnityRect(), new Vector2(0.5f, 0.5f), pixelsPerUnit);
		}

		static Dictionary<AtlasRegion, Texture2D> CachedRegionTextures = new Dictionary<AtlasRegion, Texture2D>();
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
		public static Texture2D ToTexture (this AtlasRegion ar, bool applyImmediately = true, TextureFormat textureFormat = SpineTextureFormat, bool mipmaps = UseMipMaps) {
			Texture2D output;

			CachedRegionTextures.TryGetValue(ar, out output);
			if (output == null) {
				Texture2D sourceTexture = ar.GetMainTexture();
				Rect r = ar.GetUnityRect(sourceTexture.height);
				int width = (int)r.width;
				int height = (int)r.height;
				output = new Texture2D(width, height, textureFormat, mipmaps);
				output.name = ar.name;
				Color[] pixelBuffer = sourceTexture.GetPixels((int)r.x, (int)r.y, width, height);
				output.SetPixels(pixelBuffer);
				CachedRegionTextures.Add(ar, output);
				CachedRegionTexturesList.Add(output);

				if (applyImmediately)
					output.Apply();
			}

			return output;
		}

		static Texture2D ToTexture (this Sprite s, bool applyImmediately = true, TextureFormat textureFormat = SpineTextureFormat, bool mipmaps = UseMipMaps) {
			var spriteTexture = s.texture;
			var r = s.textureRect;
			var spritePixels = spriteTexture.GetPixels((int)r.x, (int)r.y, (int)r.width, (int)r.height);
			var newTexture = new Texture2D((int)r.width, (int)r.height, textureFormat, mipmaps);
			newTexture.SetPixels(spritePixels);

			if (applyImmediately)
				newTexture.Apply();

			return newTexture;
		}

		static Texture2D GetClone (this Texture2D t, bool applyImmediately = true, TextureFormat textureFormat = SpineTextureFormat, bool mipmaps = UseMipMaps) {
			var spritePixels = t.GetPixels(0, 0, (int)t.width, (int)t.height);
			var newTexture = new Texture2D((int)t.width, (int)t.height, textureFormat, mipmaps);
			newTexture.SetPixels(spritePixels);

			if (applyImmediately)
				newTexture.Apply();

			return newTexture;
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
		static AtlasRegion UVRectToAtlasRegion (Rect uvRect, string name, AtlasPage page, float offsetX, float offsetY, bool rotate) {			
			var tr  = UVRectToTextureRect(uvRect, page.width, page.height);
			var rr = tr.SpineUnityFlipRect(page.height);

			int x = (int)rr.x, y = (int)rr.y;
			int w, h;
			if (rotate) {
				w = (int)rr.height;
				h = (int)rr.width;
			} else {
				w = (int)rr.width;
				h = (int)rr.height;
			}

			return new AtlasRegion {
				page = page,
				name = name,

				u = uvRect.xMin,
				u2 = uvRect.xMax,
				v = uvRect.yMax,
				v2 = uvRect.yMin,

				index = -1,

				width = w,
				originalWidth = w,
				height = h,
				originalHeight = h,
				offsetX = offsetX,
				offsetY = offsetY,
				x = x,
				y = y,

				rotate = rotate
			};
		}

		/// <summary>
		/// Convenience method for getting the main texture of the material of the page of the region.</summary>
		static Texture2D GetMainTexture (this AtlasRegion region) {
			var material = (region.page.rendererObject as Material);
			return material.mainTexture as Texture2D;
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

	public static class SkinUtilities {

		#region Skeleton Skin Extensions
		/// <summary>
		/// Convenience method for duplicating a skeleton's current active skin so changes to it will not affect other skeleton instances. .</summary>
		public static Skin UnshareSkin (this Skeleton skeleton, bool includeDefaultSkin, bool unshareAttachments, AnimationState state = null) {
			// 1. Copy the current skin and set the skeleton's skin to the new one.
			var newSkin = skeleton.GetClonedSkin("cloned skin", includeDefaultSkin, unshareAttachments, true);
			skeleton.SetSkin(newSkin);

			// 2. Apply correct attachments: skeleton.SetToSetupPose + animationState.Apply
			if (state != null) {
				skeleton.SetToSetupPose();
				state.Apply(skeleton);
			}

			// 3. Return unshared skin.
			return newSkin;
		}

		public static Skin GetClonedSkin (this Skeleton skeleton, string newSkinName, bool includeDefaultSkin = false, bool cloneAttachments = false, bool cloneMeshesAsLinked = true) {
			var newSkin = new Skin(newSkinName); // may have null name. Harmless.
			var defaultSkin = skeleton.data.DefaultSkin;
			var activeSkin = skeleton.skin;

			if (includeDefaultSkin)
				defaultSkin.CopyTo(newSkin, true, cloneAttachments, cloneMeshesAsLinked);

			if (activeSkin != null)
				activeSkin.CopyTo(newSkin, true, cloneAttachments, cloneMeshesAsLinked);

			return newSkin;
		}
		#endregion

		/// <summary>
		/// Gets a shallow copy of the skin. The cloned skin's attachments are shared with the original skin.</summary>
		public static Skin GetClone (this Skin original) {
			var newSkin = new Skin(original.name + " clone");
			var newSkinAttachments = newSkin.Attachments;

			foreach (var a in original.Attachments)
				newSkinAttachments[a.Key] = a.Value;

			return newSkin;
		}

		/// <summary>Adds an attachment to the skin for the specified slot index and name. If the name already exists for the slot, the previous value is replaced.</summary>
		public static void SetAttachment (this Skin skin, string slotName, string keyName, Attachment attachment, Skeleton skeleton) {
			int slotIndex = skeleton.FindSlotIndex(slotName);
			if (skeleton == null) throw new System.ArgumentNullException("skeleton", "skeleton cannot be null.");
			if (slotIndex == -1) throw new System.ArgumentException(string.Format("Slot '{0}' does not exist in skeleton.", slotName), "slotName");
			skin.AddAttachment(slotIndex, keyName, attachment);
		}

		/// <summary>Gets an attachment from the skin for the specified slot index and name.</summary>
		public static Attachment GetAttachment (this Skin skin, string slotName, string keyName, Skeleton skeleton) {
			int slotIndex = skeleton.FindSlotIndex(slotName);
			if (skeleton == null) throw new System.ArgumentNullException("skeleton", "skeleton cannot be null.");
			if (slotIndex == -1) throw new System.ArgumentException(string.Format("Slot '{0}' does not exist in skeleton.", slotName), "slotName");
			return skin.GetAttachment(slotIndex, keyName);
		}

		/// <summary>Adds an attachment to the skin for the specified slot index and name. If the name already exists for the slot, the previous value is replaced.</summary>
		public static void SetAttachment (this Skin skin, int slotIndex, string keyName, Attachment attachment) {
			skin.AddAttachment(slotIndex, keyName, attachment);
		}

		/// <summary>Removes the attachment. Returns true if the element is successfully found and removed; otherwise, false.</summary>
		public static bool RemoveAttachment (this Skin skin, string slotName, string keyName, Skeleton skeleton) {
			int slotIndex = skeleton.FindSlotIndex(slotName);
			if (skeleton == null) throw new System.ArgumentNullException("skeleton", "skeleton cannot be null.");
			if (slotIndex == -1) throw new System.ArgumentException(string.Format("Slot '{0}' does not exist in skeleton.", slotName), "slotName");
			return skin.RemoveAttachment(slotIndex, keyName);
		}

		/// <summary>Removes the attachment. Returns true if the element is successfully found and removed; otherwise, false.</summary>
		public static bool RemoveAttachment (this Skin skin, int slotIndex, string keyName) {
			return skin.Attachments.Remove(new Skin.AttachmentKeyTuple(slotIndex, keyName));
		}

		public static void Clear (this Skin skin) {
			skin.Attachments.Clear();
		}

		public static void Append (this Skin destination, Skin source) {
			source.CopyTo(destination, true, false);
		}

		public static void CopyTo (this Skin source, Skin destination, bool overwrite, bool cloneAttachments, bool cloneMeshesAsLinked = true) {
			var sourceAttachments = source.Attachments;
			var destinationAttachments = destination.Attachments;

			if (cloneAttachments) {
				if (overwrite) {
					foreach (var e in sourceAttachments)
						destinationAttachments[e.Key] = e.Value.GetClone(cloneMeshesAsLinked);
				} else {
					foreach (var e in sourceAttachments) {
						if (destinationAttachments.ContainsKey(e.Key)) continue;
						destinationAttachments.Add(e.Key, e.Value.GetClone(cloneMeshesAsLinked));
					}
				}
			} else {
				if (overwrite) {
					foreach (var e in sourceAttachments)
						destinationAttachments[e.Key] = e.Value;
				} else {
					foreach (var e in sourceAttachments) {
						if (destinationAttachments.ContainsKey(e.Key)) continue;
						destinationAttachments.Add(e.Key, e.Value);
					}
				}
			}
		}


	}

	public static class AttachmentCloneExtensions {
		/// <summary>
		/// Clones the attachment.</summary>
		public static Attachment GetClone (this Attachment o, bool cloneMeshesAsLinked) {
			var regionAttachment = o as RegionAttachment;
			if (regionAttachment != null)
				return regionAttachment.GetClone();			

			var meshAttachment = o as MeshAttachment;
			if (meshAttachment != null)
				return cloneMeshesAsLinked ? meshAttachment.GetLinkedClone() : meshAttachment.GetClone();

			var boundingBoxAttachment = o as BoundingBoxAttachment;
			if (boundingBoxAttachment != null)
				return boundingBoxAttachment.GetClone();

			var pathAttachment = o as PathAttachment;
			if (pathAttachment != null)
				return pathAttachment.GetClone();

			var pointAttachment = o as PointAttachment;
			if (pointAttachment != null)
				return pointAttachment.GetClone();

			var clippingAttachment = o as ClippingAttachment;
			if (clippingAttachment != null)
				return clippingAttachment.GetClone();

			return null;
		}

		public static RegionAttachment GetClone (this RegionAttachment o) {
			return new RegionAttachment(o.Name + "clone") {
				x = o.x,
				y = o.y,
				rotation = o.rotation,
				scaleX = o.scaleX,
				scaleY = o.scaleY,
				width = o.width,
				height = o.height,

				r = o.r,
				g = o.g,
				b = o.b,
				a = o.a,

				Path = o.Path,
				RendererObject = o.RendererObject,
				regionOffsetX = o.regionOffsetX,
				regionOffsetY = o.regionOffsetY,
				regionWidth = o.regionWidth,
				regionHeight = o.regionHeight,
				regionOriginalWidth = o.regionOriginalWidth,
				regionOriginalHeight = o.regionOriginalHeight,
				uvs = o.uvs.Clone() as float[],
				offset = o.offset.Clone() as float[]
			};
		}

		public static ClippingAttachment GetClone (this ClippingAttachment o) {
			var ca = new ClippingAttachment(o.Name) {
				endSlot = o.endSlot
			};
			CloneVertexAttachment(o, ca);
			return ca;
		}

		public static PointAttachment GetClone (this PointAttachment o) {
			var pa = new PointAttachment(o.Name) {
				rotation = o.rotation,
				x = o.x,
				y = o.y
			};
			return pa;
		}

		public static BoundingBoxAttachment GetClone (this BoundingBoxAttachment o) {
			var ba = new BoundingBoxAttachment(o.Name);
			CloneVertexAttachment(o, ba);
			return ba;
		}

		public static MeshAttachment GetLinkedClone (this MeshAttachment o, bool inheritDeform = true) {
			return o.GetLinkedMesh(o.Name, o.RendererObject as AtlasRegion, inheritDeform, copyOriginalProperties: true);
		}

		/// <summary>
		/// Returns a clone of the MeshAttachment. This will cause Deform animations to stop working unless you explicity set the .parentMesh to the original.</summary>
		public static MeshAttachment GetClone (this MeshAttachment o) {
			var ma = new MeshAttachment(o.Name) {
				r = o.r,
				g = o.g,
				b = o.b,
				a = o.a,

				inheritDeform = o.inheritDeform,

				Path = o.Path,
				RendererObject = o.RendererObject,

				regionOffsetX = o.regionOffsetX,
				regionOffsetY = o.regionOffsetY,
				regionWidth = o.regionWidth,
				regionHeight = o.regionHeight,
				regionOriginalWidth = o.regionOriginalWidth,
				regionOriginalHeight = o.regionOriginalHeight,
				RegionU = o.RegionU,
				RegionV = o.RegionV,
				RegionU2 = o.RegionU2,
				RegionV2 = o.RegionV2,
				RegionRotate = o.RegionRotate,
				uvs = o.uvs.Clone() as float[]
			};

			// Linked mesh
			if (o.ParentMesh != null) {
				// bones, vertices, worldVerticesLength, regionUVs, triangles, HullLength, Edges, Width, Height
				ma.ParentMesh = o.ParentMesh;
			} else {
				CloneVertexAttachment(o, ma); // bones, vertices, worldVerticesLength
				ma.regionUVs = o.regionUVs.Clone() as float[];
				ma.triangles = o.triangles.Clone() as int[];
				ma.hulllength = o.hulllength;

				// Nonessential.
				ma.Edges = (o.Edges == null) ? null : o.Edges.Clone() as int[]; // Allow absence of Edges array when nonessential data is not exported.
				ma.Width = o.Width;
				ma.Height = o.Height;
			}

			return ma;
		}

		public static PathAttachment GetClone (this PathAttachment o) {
			var newPathAttachment = new PathAttachment(o.Name) {
				lengths = o.lengths.Clone() as float[],
				closed = o.closed,
				constantSpeed = o.constantSpeed
			};
			CloneVertexAttachment(o, newPathAttachment);

			return newPathAttachment;
		}

		static void CloneVertexAttachment (VertexAttachment src, VertexAttachment dest) {
			dest.worldVerticesLength = src.worldVerticesLength;
			if (src.bones != null)
				dest.bones = src.bones.Clone() as int[];

			if (src.vertices != null)
				dest.vertices = src.vertices.Clone() as float[];
		}


		#region Runtime Linked MeshAttachments
		/// <summary>
		/// Returns a new linked mesh linked to this MeshAttachment. It will be mapped to the AtlasRegion provided.</summary>
		public static MeshAttachment GetLinkedMesh (this MeshAttachment o, string newLinkedMeshName, AtlasRegion region, bool inheritDeform = true, bool copyOriginalProperties = false) {
			//if (string.IsNullOrEmpty(attachmentName)) throw new System.ArgumentException("attachmentName cannot be null or empty", "attachmentName");
			if (region == null) throw new System.ArgumentNullException("region");

			// If parentMesh is a linked mesh, create a link to its parent. Preserves Deform animations.
			if (o.ParentMesh != null)
				o = o.ParentMesh;

			// 1. NewMeshAttachment (AtlasAttachmentLoader.cs)
			var mesh = new MeshAttachment(newLinkedMeshName);
			mesh.SetRegion(region, false);

			// 2. (SkeletonJson.cs::ReadAttachment. case: LinkedMesh)
			mesh.Path = newLinkedMeshName;
			if (copyOriginalProperties) {
				mesh.r = o.r;
				mesh.g = o.g;
				mesh.b = o.b;
				mesh.a = o.a;
			} else {
				mesh.r = 1f;
				mesh.g = 1f;
				mesh.b = 1f;
				mesh.a = 1f;
			}
			//mesh.ParentMesh property call below sets mesh.Width and mesh.Height

			// 3. Link mesh with parent. (SkeletonJson.cs)
			mesh.inheritDeform = inheritDeform;
			mesh.ParentMesh = o;
			mesh.UpdateUVs();

			return mesh;
		}

		/// <summary>
		/// Returns a new linked mesh linked to this MeshAttachment. It will be mapped to an AtlasRegion generated from a Sprite. The AtlasRegion will be mapped to a new Material based on the shader.
		/// For better caching and batching, use GetLinkedMesh(string, AtlasRegion, bool)</summary>
		public static MeshAttachment GetLinkedMesh (this MeshAttachment o, Sprite sprite, Shader shader, bool inheritDeform = true, Material materialPropertySource = null) {
			var m = new Material(shader);
			if (materialPropertySource != null) {
				m.CopyPropertiesFromMaterial(materialPropertySource);
				m.shaderKeywords = materialPropertySource.shaderKeywords;
			}
			return o.GetLinkedMesh(sprite.name, sprite.ToAtlasRegion(), inheritDeform);
		}

		/// <summary>
		/// Returns a new linked mesh linked to this MeshAttachment. It will be mapped to an AtlasRegion generated from a Sprite. The AtlasRegion will be mapped to a new Material based on the shader.
		/// For better caching and batching, use GetLinkedMesh(string, AtlasRegion, bool)</summary>
		public static MeshAttachment GetLinkedMesh (this MeshAttachment o, Sprite sprite, Material materialPropertySource, bool inheritDeform = true) {
			return o.GetLinkedMesh(sprite, materialPropertySource.shader, inheritDeform, materialPropertySource);
		}
		#endregion

		#region RemappedClone Convenience Methods
		/// <summary>
		/// Gets a clone of the attachment remapped with a sprite image.</summary>
		/// <returns>The remapped clone.</returns>
		/// <param name="o">The original attachment.</param>
		/// <param name="sprite">The sprite whose texture to use.</param>
		/// <param name="sourceMaterial">The source material used to copy the shader and material properties from.</param>
		/// <param name="premultiplyAlpha">If <c>true</c>, a premultiply alpha clone of the original texture will be created.</param>
		/// <param name="cloneMeshAsLinked">If <c>true</c> MeshAttachments will be cloned as linked meshes and will inherit animation from the original attachment.</param>
		/// <param name="useOriginalRegionSize">If <c>true</c> the size of the original attachment will be followed, instead of using the Sprite size.</param>
		public static Attachment GetRemappedClone (this Attachment o, Sprite sprite, Material sourceMaterial, bool premultiplyAlpha = true, bool cloneMeshAsLinked = true, bool useOriginalRegionSize = false) {
			var atlasRegion = premultiplyAlpha ? sprite.ToAtlasRegionPMAClone(sourceMaterial) : sprite.ToAtlasRegion(new Material(sourceMaterial) { mainTexture = sprite.texture });
			return o.GetRemappedClone(atlasRegion, cloneMeshAsLinked, useOriginalRegionSize, 1f/sprite.pixelsPerUnit);
		}

		/// <summary>
		/// Gets a clone of the attachment remapped with an atlasRegion image.</summary>
		/// <returns>The remapped clone.</returns>
		/// <param name="o">The original attachment.</param>
		/// <param name="atlasRegion">Atlas region.</param>
		/// <param name="cloneMeshAsLinked">If <c>true</c> MeshAttachments will be cloned as linked meshes and will inherit animation from the original attachment.</param>
		/// <param name="useOriginalRegionSize">If <c>true</c> the size of the original attachment will be followed, instead of using the Sprite size.</param>
		/// <param name="scale">Unity units per pixel scale used to scale the atlas region size when not using the original region size.</param>
		public static Attachment GetRemappedClone (this Attachment o, AtlasRegion atlasRegion, bool cloneMeshAsLinked = true, bool useOriginalRegionSize = false, float scale = 0.01f) {
			var regionAttachment = o as RegionAttachment;
			if (regionAttachment != null) {
				RegionAttachment newAttachment = regionAttachment.GetClone();
				newAttachment.SetRegion(atlasRegion, false);
				if (!useOriginalRegionSize) {
					newAttachment.width = atlasRegion.width * scale;
					newAttachment.height = atlasRegion.height * scale;
				}
				newAttachment.UpdateOffset();
				return newAttachment;
			} else {
				var meshAttachment = o as MeshAttachment;
				if (meshAttachment != null) {
					MeshAttachment newAttachment = cloneMeshAsLinked ? meshAttachment.GetLinkedClone(cloneMeshAsLinked) : meshAttachment.GetClone();
					newAttachment.SetRegion(atlasRegion);
					return newAttachment;
				}
			}

			return o.GetClone(true); // Non-renderable Attachments will return as normal cloned attachments.
		}
		#endregion
	}
}
