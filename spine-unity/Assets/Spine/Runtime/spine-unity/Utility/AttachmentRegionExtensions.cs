/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2026, Esoteric Software LLC
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.AttachmentTools {
	public static class AttachmentRegionExtensions {
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
			AtlasRegion region = sprite.ToAtlasRegion(page);
			float unitsPerPixel = 1f / sprite.pixelsPerUnit;
			return region.ToRegionAttachment(sprite.name, unitsPerPixel, rotation);
		}

		/// <summary>
		/// Creates a Spine.AtlasRegion that uses a premultiplied alpha duplicate texture of the Sprite's texture data.
		/// Returns a RegionAttachment that uses it. Use this if you plan to use a premultiply alpha shader such as "Spine/Skeleton".</summary>
		/// <remarks>The duplicate texture is cached for later re-use. See documentation of
		/// <see cref="SetRegion(Attachment, Sprite, Material, bool, bool, bool, bool, TextureFormat, bool)"/> for additional details.</remarks>
		public static RegionAttachment ToRegionAttachmentWithNewPMATexture (this Sprite sprite, Shader shader, TextureFormat textureFormat = AtlasUtilities.SpineTextureFormat, bool mipmaps = AtlasUtilities.UseMipMaps, Material materialPropertySource = null, float rotation = 0f) {
			if (sprite == null) throw new System.ArgumentNullException("sprite");
			if (shader == null) throw new System.ArgumentNullException("shader");
			AtlasRegion region = sprite.ToAtlasRegionWithNewPMATexture(shader, textureFormat, mipmaps, materialPropertySource);
			float unitsPerPixel = 1f / sprite.pixelsPerUnit;
			return region.ToRegionAttachment(sprite.name, unitsPerPixel, rotation);
		}

		public static RegionAttachment ToRegionAttachmentWithNewPMATexture (this Sprite sprite, Material materialPropertySource, TextureFormat textureFormat = AtlasUtilities.SpineTextureFormat, bool mipmaps = AtlasUtilities.UseMipMaps, float rotation = 0f) {
			return sprite.ToRegionAttachmentWithNewPMATexture(materialPropertySource.shader, textureFormat, mipmaps, materialPropertySource, rotation);
		}

		/// <summary>
		/// Creates a new RegionAttachment from a given AtlasRegion.</summary>
		public static RegionAttachment ToRegionAttachment (this AtlasRegion region, string attachmentName, float scale = 0.01f, float rotation = 0f) {
			if (string.IsNullOrEmpty(attachmentName)) throw new System.ArgumentException("attachmentName can't be null or empty.", "attachmentName");
			if (region == null) throw new System.ArgumentNullException("region");

			// (AtlasAttachmentLoader.cs)
			Sequence sequence = new Sequence(1, false);
			sequence.Regions[0] = region;
			RegionAttachment attachment = new RegionAttachment(attachmentName, sequence);

			attachment.Path = region.name;
			attachment.ScaleX = 1;
			attachment.ScaleY = 1;
			attachment.Rotation = rotation;
			attachment.SetColor(Color.white);

			// pass OriginalWidth and OriginalHeight because UpdateOffset uses it in its calculation.
			attachment.Width = region.originalWidth * scale;
			attachment.Height = region.originalHeight * scale;

			attachment.SetColor(Color.white);
			attachment.UpdateSequence();
			return attachment;
		}

		/// <summary> Sets the scale. Call regionAttachment.UpdateOffset to apply the change.</summary>
		public static void SetScale (this RegionAttachment regionAttachment, Vector2 scale) {
			regionAttachment.ScaleX = scale.x;
			regionAttachment.ScaleY = scale.y;
		}

		/// <summary> Sets the scale. Call regionAttachment.UpdateOffset to apply the change.</summary>
		public static void SetScale (this RegionAttachment regionAttachment, float x, float y) {
			regionAttachment.ScaleX = x;
			regionAttachment.ScaleY = y;
		}

		/// <summary> Sets the position offset. Call regionAttachment.UpdateOffset to apply the change.</summary>
		public static void SetPositionOffset (this RegionAttachment regionAttachment, Vector2 offset) {
			regionAttachment.X = offset.x;
			regionAttachment.Y = offset.y;
		}

		/// <summary> Sets the position offset. Call regionAttachment.UpdateOffset to apply the change.</summary>
		public static void SetPositionOffset (this RegionAttachment regionAttachment, float x, float y) {
			regionAttachment.X = x;
			regionAttachment.Y = y;
		}

		/// <summary> Sets the rotation. Call regionAttachment.UpdateOffset to apply the change.</summary>
		public static void SetRotation (this RegionAttachment regionAttachment, float rotation) {
			regionAttachment.Rotation = rotation;
		}
		#endregion

		#region SetRegion
		/// <summary>
		/// Sets the region of an attachment to match a Sprite image.</summary>
		/// <param name="attachment">The attachment to modify.</param>
		/// <param name="sprite">The sprite whose texture to use.</param>
		/// <param name="sourceMaterial">The source material used to copy the shader and material properties from.</param>
		/// <param name="premultiplyAlpha">If <c>true</c>, a premultiply alpha duplicate of the original texture will be created.
		/// See remarks below for additional info.</param>
		/// <param name="useOriginalRegionSize">If <c>true</c> the size of the original attachment will be followed, instead of using the Sprite size.</param>
		/// <param name="pivotShiftsMeshUVCoords">If <c>true</c> and the Attachment is a MeshAttachment, then
		///	a non-central sprite pivot will shift uv coords in the opposite direction. Vertices will not be offset in
		///	any case when the Attachment is a MeshAttachment.</param>
		///	<param name="useOriginalRegionScale">If <c>true</c> and the Attachment is a RegionAttachment, then
		///	the original region's scale value is used instead of the Sprite's pixels per unit property. Since uniform scale is used,
		///	x scale of the original attachment (width scale) is used, scale in y direction (height scale) is ignored.</param>
		///	<param name="pmaTextureFormat">If <c>premultiplyAlpha</c> is <c>true</c>, the TextureFormat of the
		///	newly created PMA attachment Texture.</param>
		///	<param name="pmaMipmaps">If <c>premultiplyAlpha</c> is <c>true</c>, whether the newly created
		///	PMA attachment Texture has mipmaps enabled.</param>
		///	<remarks>When parameter <c>premultiplyAlpha</c> is set to <c>true</c>, a premultiply alpha duplicate of the
		///	original texture will be created. Additionally, this PMA Texture duplicate is cached for later re-use,
		///	which might steadily increase the Texture memory footprint when used excessively.
		///	See <see cref="AtlasUtilities.ClearCache()"/> on how to clear these cached textures.</remarks>
		public static void SetRegion (this Attachment attachment, Sprite sprite, Material sourceMaterial,
			bool premultiplyAlpha = true, bool useOriginalRegionSize = false,
			bool pivotShiftsMeshUVCoords = true, bool useOriginalRegionScale = false,
			TextureFormat pmaTextureFormat = AtlasUtilities.SpineTextureFormat,
			bool pmaMipmaps = AtlasUtilities.UseMipMaps) {

			AtlasRegion atlasRegion = premultiplyAlpha ?
				sprite.ToAtlasRegionWithNewPMATexture(sourceMaterial, pmaTextureFormat, pmaMipmaps) :
				sprite.ToAtlasRegion(new Material(sourceMaterial) { mainTexture = sprite.texture });
			if (!pivotShiftsMeshUVCoords && attachment is MeshAttachment) {
				// prevent non-central sprite pivot setting offsetX/Y and shifting uv coords out of mesh bounds
				atlasRegion.offsetX = 0;
				atlasRegion.offsetY = 0;
			}
			float scale = 1f / sprite.pixelsPerUnit;
			if (useOriginalRegionScale) {
				RegionAttachment regionAttachment = attachment as RegionAttachment;
				if (regionAttachment != null) {
					var firstRegion = regionAttachment.Sequence.GetRegion(0);
					scale = regionAttachment.Width / firstRegion.OriginalWidth;
				}
			}
			attachment.SetRegion(atlasRegion, useOriginalRegionSize, scale);
		}

		/// <summary>
		/// Sets the region of an attachment to use a new AtlasRegion.</summary>
		/// <param name="attachment">The attachment to modify.</param>
		/// <param name="atlasRegion">Atlas region.</param>
		/// <param name="useOriginalRegionSize">If <c>true</c> the size of the original attachment will be followed, instead of using the atlas region size.</param>
		/// <param name="scale">Unity units per pixel scale used to scale the atlas region size when not using the original region size.</param>
		public static void SetRegion (this Attachment attachment, AtlasRegion atlasRegion, bool useOriginalRegionSize = false, float scale = 0.01f) {
			RegionAttachment regionAttachment = attachment as RegionAttachment;
			if (regionAttachment != null) {

				regionAttachment.Sequence.Regions[0] = atlasRegion;
				if (!useOriginalRegionSize) {
					regionAttachment.Width = atlasRegion.width * scale;
					regionAttachment.Height = atlasRegion.height * scale;
				}
				regionAttachment.UpdateSequence();
			} else {
				MeshAttachment meshAttachment = attachment as MeshAttachment;
				if (meshAttachment != null) {
					meshAttachment.Sequence.Regions[0] = atlasRegion;
					meshAttachment.UpdateSequence();
				}
			}
		}
		#endregion
	}
}
