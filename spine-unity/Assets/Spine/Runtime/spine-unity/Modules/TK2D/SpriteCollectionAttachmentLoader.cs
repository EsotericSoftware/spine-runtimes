/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#if SPINE_TK2D
using Spine;
using System;
using UnityEngine;

// MITCH: handle TPackerCW flip mode (probably not swap uv horizontaly)
namespace Spine.Unity.TK2D {
	public class SpriteCollectionAttachmentLoader : AttachmentLoader {
		private tk2dSpriteCollectionData sprites;
		private float u, v, u2, v2;
		private bool regionRotated;
		private float regionOriginalWidth, regionOriginalHeight;
		private float regionWidth, regionHeight;
		private float regionOffsetX, regionOffsetY;
		private Material material;

		public SpriteCollectionAttachmentLoader (tk2dSpriteCollectionData sprites) {
			if (sprites == null)
				throw new ArgumentNullException("sprites cannot be null.");
			this.sprites = sprites;
		}

		private AtlasRegion ProcessSpriteDefinition (String name) {
			// Strip folder names.
			int index = name.LastIndexOfAny(new char[] { '/', '\\' });
			if (index != -1)
				name = name.Substring(index + 1);

			tk2dSpriteDefinition def = sprites.inst.GetSpriteDefinition(name);

			if (def == null) {
				Debug.Log("Sprite not found in atlas: " + name, sprites);
				throw new Exception("Sprite not found in atlas: " + name);
			}
			if (def.complexGeometry)
				throw new NotImplementedException("Complex geometry is not supported: " + name);
			if (def.flipped == tk2dSpriteDefinition.FlipMode.TPackerCW)
				throw new NotImplementedException("Only 2D Toolkit atlases are supported: " + name);

			Vector2 minTexCoords = Vector2.one, maxTexCoords = Vector2.zero;
			for (int i = 0; i < def.uvs.Length; ++i) {
				Vector2 uv = def.uvs[i];
				minTexCoords = Vector2.Min(minTexCoords, uv);
				maxTexCoords = Vector2.Max(maxTexCoords, uv);
			}
			regionRotated = def.flipped == tk2dSpriteDefinition.FlipMode.Tk2d;
			if (regionRotated) {
				float temp = minTexCoords.x;
				minTexCoords.x = maxTexCoords.x;
				maxTexCoords.x = temp;
			}
			u = minTexCoords.x;
			v = maxTexCoords.y;
			u2 = maxTexCoords.x;
			v2 = minTexCoords.y;

			regionOriginalWidth = (int)(def.untrimmedBoundsData[1].x / def.texelSize.x);
			regionOriginalHeight = (int)(def.untrimmedBoundsData[1].y / def.texelSize.y);

			regionWidth = (int)(def.boundsData[1].x / def.texelSize.x);
			regionHeight = (int)(def.boundsData[1].y / def.texelSize.y);
			if (regionRotated) {
				float tempSwap = regionWidth;
				regionWidth = regionHeight;
				regionHeight = tempSwap;
			}

			float x0 = def.untrimmedBoundsData[0].x - def.untrimmedBoundsData[1].x / 2;
			float x1 = def.boundsData[0].x - def.boundsData[1].x / 2;
			regionOffsetX = (int)((x1 - x0) / def.texelSize.x);

			float y0 = def.untrimmedBoundsData[0].y - def.untrimmedBoundsData[1].y / 2;
			float y1 = def.boundsData[0].y - def.boundsData[1].y / 2;
			regionOffsetY = (int)((y1 - y0) / def.texelSize.y);

			material = def.materialInst;

			AtlasRegion region = new AtlasRegion();
			region.name = name;
			AtlasPage page = new AtlasPage();
			page.rendererObject = material;
			region.page = page;
			region.u = u;
			region.v = v;
			region.u2 = u2;
			region.v2 = v2;
			region.rotate = regionRotated;
			region.degrees = regionRotated ? 90 : 0;
			region.originalWidth = (int)regionOriginalWidth;
			region.originalHeight = (int)regionOriginalHeight;
			region.width = (int)regionWidth;
			region.height = (int)regionHeight;
			region.offsetX = regionOffsetX;
			region.offsetY = regionOffsetY;
			return region;
		}

		private void LoadSequence (string name, string basePath, Sequence sequence) {
			TextureRegion[] regions = sequence.Regions;
			for (int i = 0, n = regions.Length; i < n; i++) {
				string path = sequence.GetPath(basePath, i);
				regions[i] = ProcessSpriteDefinition(path);
				if (regions[i] == null) throw new ArgumentException(string.Format("Region not found in atlas: {0} (region attachment: {1})", path, name));
			}
		}

		public RegionAttachment NewRegionAttachment (Skin skin, String name, String path, Sequence sequence) {
			RegionAttachment attachment = new RegionAttachment(name);
			if (sequence != null)
				LoadSequence(name, path, sequence);
			else {
				AtlasRegion region = ProcessSpriteDefinition(path);
				if (region == null)
					throw new ArgumentException(string.Format("Region not found in atlas: {0} (region attachment: {1})", path, name));
				attachment.Region = region;
				attachment.Path = path;
			}
			return attachment;
		}

		public MeshAttachment NewMeshAttachment (Skin skin, String name, String path, Sequence sequence) {
			MeshAttachment attachment = new MeshAttachment(name);
			if (sequence != null)
				LoadSequence(name, path, sequence);
			else {
				AtlasRegion region = ProcessSpriteDefinition(path);
				if (region == null)
					throw new ArgumentException(string.Format("Region not found in atlas: {0} (region attachment: {1})", path, name));
				attachment.Region = region;
				attachment.Path = path;
			}
			return attachment;
		}

		public BoundingBoxAttachment NewBoundingBoxAttachment (Skin skin, String name) {
			return new BoundingBoxAttachment(name);
		}

		public PathAttachment NewPathAttachment (Skin skin, string name) {
			return new PathAttachment(name);
		}

		public PointAttachment NewPointAttachment (Skin skin, string name) {
			return new PointAttachment(name);
		}

		public ClippingAttachment NewClippingAttachment (Skin skin, string name) {
			return new ClippingAttachment(name);
		}
	}
}
#endif
