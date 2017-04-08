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

// Contributed by: Mitch Thompson

using UnityEngine;
using System.Collections.Generic;
using Spine.Unity.Modules.AttachmentTools;

namespace Spine.Unity.Modules {
	public class SpriteAttacher : MonoBehaviour {
		public const string DefaultPMAShader = "Spine/Skeleton";
		public const string DefaultStraightAlphaShader = "Sprites/Default";

		#region Inspector
		public bool attachOnStart = true;
		public Sprite sprite;
		[SpineSlot] public string slot;
		#endregion

		#if UNITY_EDITOR
		void OnValidate () {
			var skeletonComponent = GetComponent<ISkeletonComponent>();
			var skeletonRenderer = skeletonComponent as SkeletonRenderer;
			bool apma;

			if (skeletonRenderer != null) {
				apma = skeletonRenderer.pmaVertexColors;
			} else {
				var skeletonGraphic = skeletonComponent as SkeletonGraphic;
				apma = skeletonGraphic != null && skeletonGraphic.SpineMeshGenerator.PremultiplyVertexColors;
			}

			if (apma) {
				try {
					sprite.texture.GetPixel(0, 0);
				} catch (UnityException e) {
					Debug.LogFormat("Texture of {0} ({1}) is not read/write enabled. SpriteAttacher requires this in order to work with a SkeletonRenderer that renders premultiplied alpha. Please check the texture settings.", sprite.name, sprite.texture.name);
					UnityEditor.EditorGUIUtility.PingObject(sprite.texture);
					throw e;
				}
			}
		}
		#endif

		RegionAttachment attachment;
		bool applyPMA;

		static Dictionary<Texture, AtlasPage> atlasPageCache;
		static AtlasPage GetPageFor (Texture texture, Shader shader) {
			if (atlasPageCache == null) atlasPageCache = new Dictionary<Texture, AtlasPage>();
			AtlasPage atlasPage;
			atlasPageCache.TryGetValue(texture, out atlasPage);
			if (atlasPage == null) {
				var newMaterial = new Material(shader);
				atlasPage = newMaterial.ToSpineAtlasPage();
				atlasPageCache[texture] = atlasPage;
			}
			return atlasPage;
		}

		void Start () {
			if (attachOnStart) Attach();
		}

		public void Attach () {
			var skeletonComponent = GetComponent<ISkeletonComponent>();
			var skeletonRenderer = skeletonComponent as SkeletonRenderer;
			if (skeletonRenderer != null)
				this.applyPMA = skeletonRenderer.pmaVertexColors;
			else {
				var skeletonGraphic = skeletonComponent as SkeletonGraphic;
				if (skeletonGraphic != null)
					this.applyPMA = skeletonGraphic.SpineMeshGenerator.PremultiplyVertexColors;
			}

			Shader attachmentShader = applyPMA ? Shader.Find(DefaultPMAShader) : Shader.Find(DefaultStraightAlphaShader);
			attachment = applyPMA ? sprite.ToRegionAttachmentPMAClone(attachmentShader) : sprite.ToRegionAttachment(SpriteAttacher.GetPageFor(sprite.texture, attachmentShader));
			skeletonComponent.Skeleton.FindSlot(slot).Attachment = attachment;
		}
	}

	public static class SpriteAttachmentExtensions {
		public static RegionAttachment AttachUnitySprite (this Skeleton skeleton, string slotName, Sprite sprite, string shaderName = SpriteAttacher.DefaultPMAShader, bool applyPMA = true) {
			return skeleton.AttachUnitySprite(slotName, sprite, Shader.Find(shaderName), applyPMA);
		}

		public static RegionAttachment AddUnitySprite (this SkeletonData skeletonData, string slotName, Sprite sprite, string skinName = "", string shaderName = SpriteAttacher.DefaultPMAShader, bool applyPMA = true) {
			return skeletonData.AddUnitySprite(slotName, sprite, skinName, Shader.Find(shaderName), applyPMA);
		}

		public static RegionAttachment AttachUnitySprite (this Skeleton skeleton, string slotName, Sprite sprite, Shader shader, bool applyPMA) {
			RegionAttachment att = applyPMA ? sprite.ToRegionAttachmentPMAClone(shader) : sprite.ToRegionAttachment(new Material(shader));
			skeleton.FindSlot(slotName).Attachment = att;
			return att;
		}

		public static RegionAttachment AddUnitySprite (this SkeletonData skeletonData, string slotName, Sprite sprite, string skinName, Shader shader, bool applyPMA) {
			RegionAttachment att = applyPMA ? sprite.ToRegionAttachmentPMAClone(shader) : sprite.ToRegionAttachment(new Material(shader));

			var slotIndex = skeletonData.FindSlotIndex(slotName);
			Skin skin = skeletonData.defaultSkin;
			if (skinName != "")
				skin = skeletonData.FindSkin(skinName);

			skin.AddAttachment(slotIndex, att.Name, att);

			return att;
		}
	}
}
