﻿/******************************************************************************
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
			var regionAttachment = attachment as RegionAttachment;
			if (regionAttachment != null)
				return regionAttachment.RendererObject as AtlasRegion;

			var meshAttachment = attachment as MeshAttachment;
			if (meshAttachment != null)
				return meshAttachment.RendererObject as AtlasRegion;

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
		public static RegionAttachment ToRegionAttachment (this Sprite sprite, Material material) {
			return sprite.ToRegionAttachment(material.ToSpineAtlasPage());
		}

		/// <summary>
		/// Creates a RegionAttachment based on a sprite. This method creates a real, usable AtlasRegion. That AtlasRegion uses the AtlasPage provided./// </summary>
		public static RegionAttachment ToRegionAttachment (this Sprite sprite, AtlasPage page) {
			if (sprite == null) throw new System.ArgumentNullException("sprite");
			if (page == null) throw new System.ArgumentNullException("page");
			var region = sprite.ToAtlasRegion(page);
			var unitsPerPixel = 1f / sprite.pixelsPerUnit;
			return region.ToRegionAttachment(sprite.name, unitsPerPixel);
		}

		/// <summary>
		/// Creates a Spine.AtlasRegion that uses a premultiplied alpha duplicate texture of the Sprite's texture data. Returns a RegionAttachment that uses it. Use this if you plan to use a premultiply alpha shader such as "Spine/Skeleton"</summary>
		public static RegionAttachment ToRegionAttachmentPMAClone (this Sprite sprite, Shader shader) {
			if (sprite == null) throw new System.ArgumentNullException("sprite");
			if (shader == null) throw new System.ArgumentNullException("shader");
			var region = sprite.ToAtlasRegionPMAClone(shader);
			var unitsPerPixel = 1f / sprite.pixelsPerUnit;
			return region.ToRegionAttachment(sprite.name, unitsPerPixel);
		}

		/// <summary>
		/// Creates a new RegionAttachment from a given AtlasRegion.</summary>
		public static RegionAttachment ToRegionAttachment (this AtlasRegion region, string attachmentName, float scale = 0.01f) {
			if (string.IsNullOrEmpty(attachmentName)) throw new System.ArgumentException("attachmentName can't be null or empty.", "attachmentName");
			if (region == null) throw new System.ArgumentNullException("region");

			// (AtlasAttachmentLoader.cs)
			var attachment = new RegionAttachment(attachmentName);

			attachment.scaleX = 1;
			attachment.scaleY = 1;
			attachment.SetColor(Color.white);
			attachment.width = region.width * scale;
			attachment.height = region.height * scale;

			attachment.RendererObject = region;
			attachment.SetUVs(region.u, region.v, region.u2, region.v2, region.rotate);
			attachment.regionOffsetX = region.offsetX;
			attachment.regionOffsetY = region.offsetY;
			attachment.regionWidth = region.width;
			attachment.regionHeight = region.height;
			attachment.regionOriginalWidth = region.originalWidth;
			attachment.regionOriginalHeight = region.originalHeight;

			attachment.UpdateOffset();
			return attachment;
		}
		#endregion
	}

	public static class SpriteAtlasRegionExtensions {
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
		public static AtlasRegion ToAtlasRegionPMAClone (this Sprite s, Shader shader) {
			var material = new Material(shader);
			var tex = s.ToTexture(false);
			tex.ApplyPMA(true);

			tex.name = s.name + "-pma-";
			material.name = tex.name + shader.name;

			material.mainTexture = tex;
			var page = material.ToSpineAtlasPage();

			var region = s.ToAtlasRegion(true);
			region.page = page;

			return region;
		}

		static AtlasRegion ToAtlasRegion (this Sprite s, bool isolatedTexture = false) {
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
		/// Creates and populates a duplicate skin with cloned attachments that are backed by a new packed texture atlas comprised of all the regions from the original skin.</summary>
		/// <remarks>No Spine.Atlas object is created so there is no way to find AtlasRegions except through the Attachments using them.</remarks>
		public static Skin GetRepackedSkin (this Skin o, string skinName, Shader shader, out Material m, out Texture2D t, int maxAtlasSize = 1024, int padding = 2) {
			var skinAttachments = o.Attachments;
			var newSkin = new Skin(skinName);

			var repackedAttachments = new List<Attachment>();
			var texturesToPack = new List<Texture2D>();
			foreach (var kvp in skinAttachments) {
				var newAttachment = kvp.Value.GetClone(true);
				if (IsRenderable(newAttachment)) {
					texturesToPack.Add(newAttachment.GetAtlasRegion().ToTexture());
					repackedAttachments.Add(newAttachment);
				}
				var key = kvp.Key;
				newSkin.AddAttachment(key.slotIndex, key.name, newAttachment);
			}

			var newTexture = new Texture2D(maxAtlasSize, maxAtlasSize);
			newTexture.name = skinName;
			var rects = newTexture.PackTextures(texturesToPack.ToArray(), padding, maxAtlasSize);

			var newMaterial = new Material(shader);
			newMaterial.name = skinName;
			newMaterial.mainTexture = newTexture;
			var page = newMaterial.ToSpineAtlasPage();
			page.name = skinName;

			for (int i = 0, n = repackedAttachments.Count; i < n; i++) {
				var a = repackedAttachments[i];
				var r = rects[i];
				var oldRegion = a.GetAtlasRegion();
				var newRegion = UVRectToAtlasRegion(r, oldRegion.name, page, oldRegion.offsetX, oldRegion.offsetY, oldRegion.rotate);
				a.SetRegion(newRegion);
			}

			t = newTexture;
			m = newMaterial;
			return newSkin;
		}

		public static Sprite ToSprite (this AtlasRegion ar, float pixelsPerUnit = 100) {
			return Sprite.Create(ar.GetMainTexture(), ar.GetUnityRect(), new Vector2(0.5f, 0.5f), pixelsPerUnit);
		}

		static Texture2D ToTexture (this AtlasRegion ar, bool applyImmediately = true) {
			Texture2D sourceTexture = ar.GetMainTexture();

			Texture2D output = new Texture2D(ar.width, ar.height);
			output.name = ar.name;

			Rect r = ar.GetUnityRect(sourceTexture.height);
			Color[] pixelBuffer = sourceTexture.GetPixels((int)r.x, (int)r.y, (int)r.width, (int)r.height);
			output.SetPixels(pixelBuffer);

			if (applyImmediately)
				output.Apply();

			return output;
		}

		static Texture2D ToTexture (this Sprite s, bool applyImmediately = true) {
			var spriteTexture = s.texture;
			var r = s.textureRect;
			var spritePixels = spriteTexture.GetPixels((int)r.x, (int)r.y, (int)r.width, (int)r.height); // TODO: Test
			var newTexture = new Texture2D((int)r.width, (int)r.height);
			newTexture.SetPixels(spritePixels);

			if (applyImmediately)
				newTexture.Apply();

			return newTexture;
		}

		static bool IsRenderable (Attachment a) {
			return a is RegionAttachment || a is MeshAttachment;
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
		static Rect GetSpineAtlasRect (this AtlasRegion region) {
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
			int w = (int)rr.width, h = (int)rr.height, x = (int)rr.x, y = (int)rr.y;
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
		/// Tries to get the backing AtlasRegion of an attachment if it is renderable. Returns null for non-renderable attachments.</summary>
		static AtlasRegion GetAtlasRegion (this Attachment a) {
			var regionAttachment = a as RegionAttachment;
			if (regionAttachment != null)
				return (regionAttachment.RendererObject) as AtlasRegion;

			var meshAttachment = a as MeshAttachment;
			if (meshAttachment != null)
				return (meshAttachment.RendererObject) as AtlasRegion;

			return null;
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

		private static float InverseLerp (float a, float b, float value) {
			return (value - a) / (b - a);
		}
	}

	public static class SkinExtensions {
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
			activeSkin.CopyTo(newSkin, true, cloneAttachments, cloneMeshesAsLinked);

			return newSkin;
		}

		/// <summary>
		/// Gets a shallow copy of the skin. The cloned skin's attachments are shared with the original skin.</summary>
		public static Skin GetClone (this Skin original) {
			var newSkin = new Skin(original.name + " clone");
			var newSkinAttachments = newSkin.Attachments;

			foreach (var a in original.Attachments)
				newSkinAttachments[a.Key] = a.Value;

			return newSkin;
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

		public static BoundingBoxAttachment GetClone (this BoundingBoxAttachment o) {
			var ba = new BoundingBoxAttachment(o.Name);
			CloneVertexAttachment(o, ba);
			return o;
		}

		public static MeshAttachment GetLinkedClone (this MeshAttachment o, bool inheritDeform = true) {
			return o.GetLinkedMesh(o.Name, o.RendererObject as AtlasRegion, inheritDeform);
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
			if (o.parentMesh != null) {
				// bones, vertices, worldVerticesLength, regionUVs, triangles, HullLength, Edges, Width, Height
				ma.ParentMesh = o.parentMesh;
			} else {
				CloneVertexAttachment(o, ma); // bones, vertices, worldVerticesLength
				ma.regionUVs = o.regionUVs.Clone() as float[];
				ma.triangles = o.triangles.Clone() as int[];
				ma.hulllength = o.hulllength;

				// Nonessential.
				ma.Edges = o.Edges.Clone() as int[];
				ma.Width = o.Width;
				ma.Height = o.Height;
			}

			return o;
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
				dest.bones = src.vertices.Clone() as int[];

			if (src.vertices != null)
				dest.vertices = src.vertices.Clone() as float[];
		}


		#region Runtime Linked MeshAttachments
		/// <summary>
		/// Returns a new linked mesh linked to this MeshAttachment. It will be mapped to the AtlasRegion provided.</summary>
		public static MeshAttachment GetLinkedMesh (this MeshAttachment o, string newLinkedMeshName, AtlasRegion region, bool inheritDeform = true) {
			//if (string.IsNullOrEmpty(attachmentName)) throw new System.ArgumentException("attachmentName cannot be null or empty", "attachmentName");
			if (region == null) throw new System.ArgumentNullException("region");

			// If parentMesh is a linked mesh, create a link to its parent. Preserves Deform animations.
			if (o.parentMesh != null)
				o = o.parentMesh;

			// 1. NewMeshAttachment (AtlasAttachmentLoader.cs)
			var mesh = new MeshAttachment(newLinkedMeshName);
			mesh.SetRegion(region, false);

			// 2. (SkeletonJson.cs::ReadAttachment. case: LinkedMesh)
			mesh.Path = newLinkedMeshName;
			mesh.r = 1f;
			mesh.g = 1f;
			mesh.b = 1f;
			mesh.a = 1f;
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
		public static MeshAttachment GetLinkedMesh (this MeshAttachment o, Sprite sprite, Shader shader, bool inheritDeform = true) {
			return o.GetLinkedMesh(sprite.name, sprite.ToAtlasRegion(new Material(shader)), inheritDeform);
		}
		#endregion
	}
}
