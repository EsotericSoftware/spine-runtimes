/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

	public static class AttachmentCloneExtensions {

		#region RemappedClone Convenience Methods
		/// <summary>
		/// Gets a clone of the attachment remapped with a sprite image.</summary>
		/// <returns>The remapped clone.</returns>
		/// <param name="o">The original attachment.</param>
		/// <param name="sprite">The sprite whose texture to use.</param>
		/// <param name="sourceMaterial">The source material used to copy the shader and material properties from.</param>
		/// <param name="premultiplyAlpha">If <c>true</c>, a premultiply alpha clone of the original texture will be created.
		/// See remarks below for additional info.</param>
		/// <param name="cloneMeshAsLinked">If <c>true</c> MeshAttachments will be cloned as linked meshes and will inherit animation from the original attachment.</param>
		/// <param name="useOriginalRegionSize">If <c>true</c> the size of the original attachment will be followed, instead of using the Sprite size.</param>
		/// <param name="pivotShiftsMeshUVCoords">If <c>true</c> and the original Attachment is a MeshAttachment, then
		///	a non-central sprite pivot will shift uv coords in the opposite direction. Vertices will not be offset in
		///	any case when the original Attachment is a MeshAttachment.</param>
		///	<param name="useOriginalRegionScale">If <c>true</c> and the original Attachment is a RegionAttachment, then
		///	the original region's scale value is used instead of the Sprite's pixels per unit property. Since uniform scale is used,
		///	x scale of the original attachment (width scale) is used, scale in y direction (height scale) is ignored.</param>
		///	<param name="pmaCloneTextureFormat">If <c>premultiplyAlpha</c> is <c>true></c>, the TextureFormat of the
		///	newly created PMA attachment Texture.</param>
		///	<param name="pmaCloneMipmaps">If <c>premultiplyAlpha</c> is <ctrue></c>, whether the newly created
		///	PMA attachment Texture has mipmaps enabled.</param>
		///	<remarks>When parameter <c>premultiplyAlpha</c> is set to <c>true</c>, a premultiply alpha clone of the
		///	original texture will be created. Additionally, this PMA Texture clone is cached for later re-use,
		///	which might steadily increase the Texture memory footprint when used excessively.
		///	See <see cref="AtlasUtilities.ClearCache()"/> on how to clear these cached textures.</remarks>
		public static Attachment GetRemappedClone (this Attachment o, Sprite sprite, Material sourceMaterial,
			bool premultiplyAlpha = true, bool cloneMeshAsLinked = true, bool useOriginalRegionSize = false,
			bool pivotShiftsMeshUVCoords = true, bool useOriginalRegionScale = false,
			TextureFormat pmaCloneTextureFormat = AtlasUtilities.SpineTextureFormat,
			bool pmaCloneMipmaps = AtlasUtilities.UseMipMaps) {

			AtlasRegion atlasRegion = premultiplyAlpha ?
				sprite.ToAtlasRegionPMAClone(sourceMaterial, pmaCloneTextureFormat, pmaCloneMipmaps) :
				sprite.ToAtlasRegion(new Material(sourceMaterial) { mainTexture = sprite.texture });
			if (!pivotShiftsMeshUVCoords && o is MeshAttachment) {
				// prevent non-central sprite pivot setting offsetX/Y and shifting uv coords out of mesh bounds
				atlasRegion.offsetX = 0;
				atlasRegion.offsetY = 0;
			}
			float scale = 1f / sprite.pixelsPerUnit;
			if (useOriginalRegionScale) {
				RegionAttachment regionAttachment = o as RegionAttachment;
				if (regionAttachment != null)
					scale = regionAttachment.Width / regionAttachment.Region.OriginalWidth;
			}
			return o.GetRemappedClone(atlasRegion, cloneMeshAsLinked, useOriginalRegionSize, scale);
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
			RegionAttachment regionAttachment = o as RegionAttachment;
			if (regionAttachment != null) {
				RegionAttachment newAttachment = (RegionAttachment)regionAttachment.Copy();
				newAttachment.Region = atlasRegion;
				if (!useOriginalRegionSize) {
					newAttachment.Width = atlasRegion.width * scale;
					newAttachment.Height = atlasRegion.height * scale;
				}
				newAttachment.UpdateRegion();
				return newAttachment;
			} else {
				MeshAttachment meshAttachment = o as MeshAttachment;
				if (meshAttachment != null) {
					MeshAttachment newAttachment = cloneMeshAsLinked ? meshAttachment.NewLinkedMesh() : (MeshAttachment)meshAttachment.Copy();
					newAttachment.Region = atlasRegion;
					newAttachment.UpdateRegion();
					return newAttachment;
				}
			}
			return o.Copy();
		}
		#endregion
	}
}
