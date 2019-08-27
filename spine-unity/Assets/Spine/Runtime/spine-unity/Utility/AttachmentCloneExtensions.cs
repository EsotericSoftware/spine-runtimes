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

	public static class AttachmentCloneExtensions {
		/// <summary>
		/// Clones the attachment.</summary>
		public static Attachment GetCopy (this Attachment o, bool cloneMeshesAsLinked) {
			var meshAttachment = o as MeshAttachment;
			if (meshAttachment != null && cloneMeshesAsLinked)
				return meshAttachment.NewLinkedMesh();
			return o.Copy();
		}

		#region Runtime Linked MeshAttachments
		/// <summary>
		/// Returns a new linked mesh linked to this MeshAttachment. It will be mapped to the AtlasRegion provided.</summary>
		public static MeshAttachment GetLinkedMesh (this MeshAttachment o, string newLinkedMeshName, AtlasRegion region) {
			if (region == null) throw new System.ArgumentNullException("region");
			MeshAttachment mesh = o.NewLinkedMesh();
			mesh.SetRegion(region, false);
			return mesh;
		}

		/// <summary>
		/// Returns a new linked mesh linked to this MeshAttachment. It will be mapped to an AtlasRegion generated from a Sprite. The AtlasRegion will be mapped to a new Material based on the shader.
		/// For better caching and batching, use GetLinkedMesh(string, AtlasRegion, bool)</summary>
		public static MeshAttachment GetLinkedMesh (this MeshAttachment o, Sprite sprite, Shader shader, Material materialPropertySource = null) {
			var m = new Material(shader);
			if (materialPropertySource != null) {
				m.CopyPropertiesFromMaterial(materialPropertySource);
				m.shaderKeywords = materialPropertySource.shaderKeywords;
			}
			return o.GetLinkedMesh(sprite.name, sprite.ToAtlasRegion());
		}

		/// <summary>
		/// Returns a new linked mesh linked to this MeshAttachment. It will be mapped to an AtlasRegion generated from a Sprite. The AtlasRegion will be mapped to a new Material based on the shader.
		/// For better caching and batching, use GetLinkedMesh(string, AtlasRegion, bool)</summary>
		public static MeshAttachment GetLinkedMesh (this MeshAttachment o, Sprite sprite, Material materialPropertySource) {
			return o.GetLinkedMesh(sprite, materialPropertySource.shader, materialPropertySource);
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
			var atlasRegion = premultiplyAlpha ? sprite.ToAtlasRegionPMAClone(sourceMaterial) : sprite.ToAtlasRegion(new Material(sourceMaterial) { mainTexture = sprite.texture } );
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
				RegionAttachment newAttachment = (RegionAttachment)regionAttachment.Copy();
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
					MeshAttachment newAttachment = cloneMeshAsLinked ? meshAttachment.NewLinkedMesh() : (MeshAttachment)meshAttachment.Copy();
					newAttachment.SetRegion(atlasRegion);
					return newAttachment;
				}
			}

			return o.GetCopy(true); // Non-renderable Attachments will return as normal cloned attachments.
		}
		#endregion
	}
}
