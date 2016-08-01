/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using Spine;

namespace Spine.Unity.Modules {
	public class SpriteAttacher : MonoBehaviour {
		public bool attachOnStart = true;
		public bool keepLoaderInMemory = true;

		public Sprite sprite;

		[SpineSlot]
		public string slot;

		private SpriteAttachmentLoader loader;
		private RegionAttachment attachment;

		void Start () {
			if (attachOnStart)
				Attach();
		}

		public void Attach () {
			var skeletonRenderer = GetComponent<SkeletonRenderer>();

			if (loader == null)
				//create loader instance, tell it what sprite and shader to use
				loader = new SpriteAttachmentLoader(sprite, Shader.Find("Spine/Skeleton"));

			if (attachment == null)
				attachment = loader.NewRegionAttachment(null, sprite.name, "");

			skeletonRenderer.skeleton.FindSlot(slot).Attachment = attachment;

			if (!keepLoaderInMemory)
				loader = null;
		}
	}

	public class SpriteAttachmentLoader : AttachmentLoader {

		//MITCH: left a todo:  Memory cleanup functions

		//IMPORTANT:  Make sure you clear this when you don't need it anymore. Goodluck.
		public static Dictionary<int, AtlasRegion> atlasTable = new Dictionary<int, AtlasRegion>();

		//Shouldn't need to clear this, should just prevent redoing premultiply alpha pass on packed atlases
		public static List<int> premultipliedAtlasIds = new List<int>();

		Sprite sprite;
		Shader shader;

		public SpriteAttachmentLoader (Sprite sprite, Shader shader) {

			if (sprite.packed && sprite.packingMode == SpritePackingMode.Tight) {
				Debug.LogError("Tight Packer Policy not supported yet!");
				return;
			}

			this.sprite = sprite;
			this.shader = shader;

			Texture2D tex = sprite.texture;
			//premultiply texture if it hasn't been yet
			int instanceId = tex.GetInstanceID();
			if (!premultipliedAtlasIds.Contains(instanceId)) {
				try {
					var colors = tex.GetPixels();
					Color c;
					float a;
					for (int i = 0; i < colors.Length; i++) {
						c = colors[i];
						a = c.a;
						c.r *= a;
						c.g *= a;
						c.b *= a;
						colors[i] = c;
					}

					tex.SetPixels(colors);
					tex.Apply();

					premultipliedAtlasIds.Add(instanceId);
				} catch {
					//texture is not readable!  Can't pre-multiply it, you're on your own.
				}
			}
		}

		public RegionAttachment NewRegionAttachment (Skin skin, string name, string path) {
			RegionAttachment attachment = new RegionAttachment(name);

			Texture2D tex = sprite.texture;
			int instanceId = tex.GetInstanceID();
			AtlasRegion atlasRegion;

			// Check cache first
			if (atlasTable.ContainsKey(instanceId)) {
				atlasRegion = atlasTable[instanceId];
			} else {
				// Setup new material.
				var material = new Material(shader);
				if (sprite.packed)
					material.name = "Unity Packed Sprite Material";
				else
					material.name = sprite.name + " Sprite Material";
				material.mainTexture = tex;

				// Create faux-region to play nice with SkeletonRenderer.
				atlasRegion = new AtlasRegion();
				AtlasPage page = new AtlasPage();
				page.rendererObject = material;
				atlasRegion.page = page;

				// Cache it.
				atlasTable[instanceId] = atlasRegion;
			}

			Rect texRect = sprite.textureRect;

			//normalize rect to UV space of packed atlas
			texRect.x = Mathf.InverseLerp(0, tex.width, texRect.x);
			texRect.y = Mathf.InverseLerp(0, tex.height, texRect.y);
			texRect.width = Mathf.InverseLerp(0, tex.width, texRect.width);
			texRect.height = Mathf.InverseLerp(0, tex.height, texRect.height);

			Bounds bounds = sprite.bounds;
			Vector3 size = bounds.size;

			//MITCH: left todo: make sure this rotation thing actually works
			bool rotated = false;
			if (sprite.packed)
				rotated = sprite.packingRotation == SpritePackingRotation.Any;

			attachment.SetUVs(texRect.xMin, texRect.yMax, texRect.xMax, texRect.yMin, rotated);
			attachment.RendererObject = atlasRegion;
			attachment.SetColor(Color.white);
			attachment.ScaleX = 1;
			attachment.ScaleY = 1;
			attachment.RegionOffsetX = sprite.rect.width * (0.5f - InverseLerp(bounds.min.x, bounds.max.x, 0)) / sprite.pixelsPerUnit;
			attachment.RegionOffsetY = sprite.rect.height * (0.5f - InverseLerp(bounds.min.y, bounds.max.y, 0)) / sprite.pixelsPerUnit;
			attachment.Width = size.x;
			attachment.Height = size.y;
			attachment.RegionWidth = size.x;
			attachment.RegionHeight = size.y;
			attachment.RegionOriginalWidth = size.x;
			attachment.RegionOriginalHeight = size.y;
			attachment.UpdateOffset();

			return attachment;
		}

		public MeshAttachment NewMeshAttachment (Skin skin, string name, string path) {
			//MITCH : Left todo: Unity 5 only
			return null;
		}

		public BoundingBoxAttachment NewBoundingBoxAttachment (Skin skin, string name) {			
			return null;
		}

		public PathAttachment NewPathAttachment (Skin skin, string name) {
			return null;
		}
		
		private float InverseLerp(float a, float b, float value)
		{
			return (value - a) / (b - a);
		}
	}

	public static class SpriteAttachmentExtensions {
		public static Attachment AttachUnitySprite (this Skeleton skeleton, string slotName, Sprite sprite, string shaderName = "Spine/Skeleton") {
			var att = sprite.ToRegionAttachment(shaderName);
			skeleton.FindSlot(slotName).Attachment = att;
			return att;
		}

		public static Attachment AddUnitySprite (this SkeletonData skeletonData, string slotName, Sprite sprite, string skinName = "", string shaderName = "Spine/Skeleton") {
			var att = sprite.ToRegionAttachment(shaderName);

			var slotIndex = skeletonData.FindSlotIndex(slotName);
			Skin skin = skeletonData.defaultSkin;
			if (skinName != "")
				skin = skeletonData.FindSkin(skinName);

			skin.AddAttachment(slotIndex, att.Name, att);

			return att;
		}

		public static RegionAttachment ToRegionAttachment (this Sprite sprite, string shaderName = "Spine/Skeleton") {
			var loader = new SpriteAttachmentLoader(sprite, Shader.Find(shaderName));
			var att = loader.NewRegionAttachment(null, sprite.name, "");
			loader = null;
			return att;
		}
	}
}

