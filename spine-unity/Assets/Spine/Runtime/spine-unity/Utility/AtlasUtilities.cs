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

#if UNITY_2019_3_OR_NEWER
#define CONFIGURABLE_ENTER_PLAY_MODE
#endif

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.AttachmentTools {

	public static class AtlasUtilities {
		internal const TextureFormat SpineTextureFormat = TextureFormat.RGBA32;
		internal const float DefaultMipmapBias = -0.5f;
		internal const bool UseMipMaps = false;
		internal const float DefaultScale = 0.01f;

		const int NonrenderingRegion = -1;

#if CONFIGURABLE_ENTER_PLAY_MODE
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void Init () {
			// handle disabled domain reload
			AtlasUtilities.ClearCache();
		}
#endif

		public static AtlasRegion ToAtlasRegion (this Texture2D t, Material materialPropertySource, float scale = DefaultScale) {
			return t.ToAtlasRegion(materialPropertySource.shader, scale, materialPropertySource);
		}

		public static AtlasRegion ToAtlasRegion (this Texture2D t, Shader shader, float scale = DefaultScale, Material materialPropertySource = null) {
			Material material = new Material(shader);
			if (materialPropertySource != null) {
				material.CopyPropertiesFromMaterial(materialPropertySource);
				material.shaderKeywords = materialPropertySource.shaderKeywords;
			}

			material.mainTexture = t;
			AtlasPage page = material.ToSpineAtlasPage();

			float width = t.width;
			float height = t.height;

			AtlasRegion region = new AtlasRegion();
			region.name = t.name;

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
		public static AtlasRegion ToAtlasRegionWithNewPMATexture (this Texture2D t, Material materialPropertySource, TextureFormat textureFormat = SpineTextureFormat, bool mipmaps = UseMipMaps) {
			return t.ToAtlasRegionWithNewPMATexture(materialPropertySource.shader, textureFormat, mipmaps, materialPropertySource);
		}

		/// <summary>
		/// Creates a Spine.AtlasRegion that uses a premultiplied alpha duplicate of the Sprite's texture data.</summary>
		public static AtlasRegion ToAtlasRegionWithNewPMATexture (this Texture2D t, Shader shader, TextureFormat textureFormat = SpineTextureFormat, bool mipmaps = UseMipMaps, Material materialPropertySource = null) {
			Material material = new Material(shader);
			if (materialPropertySource != null) {
				material.CopyPropertiesFromMaterial(materialPropertySource);
				material.shaderKeywords = materialPropertySource.shaderKeywords;
			}
			Texture2D newTexture = t.GetClone(textureFormat, mipmaps, applyPMA: true);

			newTexture.name = t.name + "-pma-";
			material.name = t.name + shader.name;

			material.mainTexture = newTexture;
			AtlasPage page = material.ToSpineAtlasPage();

			AtlasRegion region = newTexture.ToAtlasRegion(shader);
			region.page = page;

			return region;
		}

		/// <summary>
		/// Creates a new Spine.AtlasPage from a UnityEngine.Material. If the material has a preassigned texture, the page width and height will be set.</summary>
		public static AtlasPage ToSpineAtlasPage (this Material m) {
			AtlasPage newPage = new AtlasPage {
				rendererObject = m,
				name = m.name
			};

			Texture t = m.mainTexture;
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
			AtlasRegion region = s.ToAtlasRegion();
			region.page = page;
			return region;
		}

		/// <summary>
		/// Creates a Spine.AtlasRegion from a UnityEngine.Sprite. This creates a new AtlasPage object for every AtlasRegion you create. You can centralize Material control by creating a shared atlas page using Material.ToSpineAtlasPage and using the sprite.ToAtlasRegion(AtlasPage) overload.</summary>
		public static AtlasRegion ToAtlasRegion (this Sprite s, Material material) {
			AtlasRegion region = s.ToAtlasRegion();
			region.page = material.ToSpineAtlasPage();
			return region;
		}

		public static AtlasRegion ToAtlasRegionWithNewPMATexture (this Sprite s, Material materialPropertySource, TextureFormat textureFormat = SpineTextureFormat, bool mipmaps = UseMipMaps) {
			return s.ToAtlasRegionWithNewPMATexture(materialPropertySource.shader, textureFormat, mipmaps, materialPropertySource);
		}

		/// <summary>
		/// Creates a Spine.AtlasRegion that uses a premultiplied alpha duplicate of the Sprite's texture data.</summary>
		public static AtlasRegion ToAtlasRegionWithNewPMATexture (this Sprite s, Shader shader, TextureFormat textureFormat = SpineTextureFormat, bool mipmaps = UseMipMaps, Material materialPropertySource = null) {
			Material material = new Material(shader);
			if (materialPropertySource != null) {
				material.CopyPropertiesFromMaterial(materialPropertySource);
				material.shaderKeywords = materialPropertySource.shaderKeywords;
			}

			Texture2D tex = s.ToTexture(textureFormat, mipmaps, applyPMA: true);
			tex.name = s.name + "-pma-";
			material.name = tex.name + shader.name;

			material.mainTexture = tex;
			AtlasPage page = material.ToSpineAtlasPage();

			AtlasRegion region = s.ToAtlasRegion(true);
			region.page = page;

			return region;
		}

		internal static AtlasRegion ToAtlasRegion (this Sprite s, bool isolatedTexture = false) {
			AtlasRegion region = new AtlasRegion();
			region.name = s.name;
			region.index = -1;
			region.degrees = s.packed && s.packingRotation != SpritePackingRotation.None ? 90 : 0;

			// World space units
			Bounds bounds = s.bounds;
			Vector2 boundsMin = bounds.min, boundsMax = bounds.max;

			// Texture space/pixel units
			Rect spineRect = s.textureRect.SpineUnityFlipRect(s.texture.height);
			Rect originalRect = s.rect;
			region.width = (int)spineRect.width;
			region.originalWidth = (int)originalRect.width;
			region.height = (int)spineRect.height;
			region.originalHeight = (int)originalRect.height;
			region.offsetX = s.textureRectOffset.x + spineRect.width * (0.5f - InverseLerp(boundsMin.x, boundsMax.x, 0));
			region.offsetY = s.textureRectOffset.y + spineRect.height * (0.5f - InverseLerp(boundsMin.y, boundsMax.y, 0));

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
		static readonly Dictionary<AtlasRegion, int> existingRegions = new Dictionary<AtlasRegion, int>();
		static readonly List<int> regionIndices = new List<int>();
		static readonly List<AtlasRegion> originalRegions = new List<AtlasRegion>();
		static readonly List<AtlasRegion> repackedRegions = new List<AtlasRegion>();
		static List<Texture2D>[] texturesToPackAtParam = new List<Texture2D>[1];
		static List<Attachment> inoutAttachments = new List<Attachment>();

		/// <summary>
		/// Repack output struct for <see cref="GetRepackedAttachments"/> and <see cref="GetRepackedSkin"/>.
		/// Returned <c>Material</c> and <c>Texture</c> behave like <c>new Texture2D()</c>, thus you need to
		/// call <c>Destroy()</c> to free resources.
		/// </summary>
		public struct RepackAttachmentsOutput {
			/// <summary>The newly generated Material for the normal blend mode, holding the repacked texture
			/// as main texture.
			/// Materials and textures returned behave like <c>new Texture2D()</c> and need to be destroyed.</summary>
			public Material outputMaterial;
			/// <summary>The newly generated main repacked texture, assigned at <c>outputMaterial.mainTexture</c>.
			/// Materials and textures returned behave like <c>new Texture2D()</c> and need to be destroyed.</summary>
			public Texture2D outputTexture;
			/// <summary>When non-null, this array will be filled with the resulting repacked texture for every property,
			/// just as the main repacked texture is assigned to <c>outputTexture</c>. This serves to avoid unnecessary
			/// allocations.
			/// Materials and textures returned behave like <c>new Texture2D()</c> and need to be destroyed.</summary>
			public Texture2D[] additionalOutputTextures;
			/// <summary>The optional generated Material for additive blend mode, holding the repacked texture
			/// as main texture.
			/// Materials and textures returned behave like <c>new Texture2D()</c> and need to be destroyed.</summary>
			public Material outputAdditiveMaterial;
			/// <summary>The optional generated Material for multiply blend mode, holding the repacked texture
			/// as main texture.
			/// Materials and textures returned behave like <c>new Texture2D()</c> and need to be destroyed.</summary>
			public Material outputMultiplyMaterial;
			/// <summary>The optional generated Material for screen blend mode, holding the repacked texture
			/// as main texture.
			/// Materials and textures returned behave like <c>new Texture2D()</c> and need to be destroyed.</summary>
			public Material outputScreenMaterial;

			/// <summary>
			/// Destroys any assigned previously generated assets. If you decide to store
			/// <see cref="RepackAttachmentsOutput"/> in a MonoBehaviour for re-use, call this method before each
			/// <see cref="GetRepackedSkin"/> or <see cref="GetRepackedAttachments"/> call, and also once in OnDestroy.
			/// </summary>
			public void DestroyGeneratedAssets () {
				if (outputMaterial) { UnityEngine.Object.Destroy(outputMaterial); outputMaterial = null; }
				if (outputTexture) { UnityEngine.Object.Destroy(outputTexture); outputTexture = null; }
				if (additionalOutputTextures != null) {
					for (int i = 0; i < additionalOutputTextures.Length; ++i) {
						if (additionalOutputTextures[i]) {
							UnityEngine.Object.Destroy(additionalOutputTextures[i]);
							additionalOutputTextures[i] = null;
						}
					}
				}
				if (outputAdditiveMaterial) { UnityEngine.Object.Destroy(outputAdditiveMaterial); outputAdditiveMaterial = null; }
				if (outputMultiplyMaterial) { UnityEngine.Object.Destroy(outputMultiplyMaterial); outputMultiplyMaterial = null; }
				if (outputScreenMaterial) { UnityEngine.Object.Destroy(outputScreenMaterial); outputScreenMaterial = null; }
			}
		}

		/// <summary>
		/// Repack configuration settings for <see cref="GetRepackedAttachments"/> and <see cref="GetRepackedSkin"/>.
		/// </summary>
		public struct RepackAttachmentsSettings {
			public static readonly string DefaultTextureName = "Repacked Attachments";

			/// <summary>
			/// Name used for newly created textures, materials and atlas pages.
			/// </summary>
			public string newAssetName;

			/// <summary>
			/// Optional shader to be used, overrides the shader of <c>materialPropertySource.shader</c>.
			/// </summary>
			public Shader shader;

			/// <summary>
			/// Normal blend mode Material reference. The generated normal blend mode Material copies these settings.
			/// </summary>
			public Material materialPropertySource;
			/// <summary>
			/// Additive blend mode Material reference. Optional, assign when repacking any additive blend mode
			/// attachments. The generated additive blend mode Material copies these settings.
			/// If all three blend mode materials <c>additiveMaterialSource</c>, <c>multiplyMaterialSource</c> and
			/// <c>screenMaterialSource</c> are null, blend mode attachments are treated as normal blend mode and
			/// repacked using a single material and atlas page.
			/// </summary>
			public Material additiveMaterialSource;
			/// <summary>
			/// Multiply blend mode Material reference. Optional, assign when repacking any multiply blend mode
			/// attachments. The generated multiply blend mode Material copies these settings.
			/// If all three blend mode materials <c>additiveMaterialSource</c>, <c>multiplyMaterialSource</c> and
			/// <c>screenMaterialSource</c> are null, blend mode attachments are treated as normal blend mode and
			/// repacked using a single material and atlas page.
			/// </summary>
			public Material multiplyMaterialSource;
			/// <summary>
			/// Screen blend mode Material reference. Optional, assign when repacking any screen blend mode attachments.
			/// The generated screen blend mode Material copies these settings.
			/// If all three blend mode materials <c>additiveMaterialSource</c>, <c>multiplyMaterialSource</c> and
			/// <c>screenMaterialSource</c> are null, blend mode attachments are treated as normal blend mode and
			/// repacked using a single material and atlas page.
			/// </summary>
			public Material screenMaterialSource;

			/// <summary>Max atlas size of a repacked texture. Packed attachments are scaled down to fit into a single
			/// texture, not using multiple repacked texture pages.</summary>
			public int maxAtlasSize;
			/// <summary>Padding between packed texture regions in pixels.</summary>
			public int padding;
			/// <summary>Texture format of the main repacked texture.</summary>
			public TextureFormat textureFormat;
			/// <summary>
			/// Whether mip-maps shall be generated for newly created repacked textures.
			/// </summary>
			public bool mipmaps;
			/// <summary>When set to <c>true</c>, <see cref="AtlasUtilities.ClearCache()"/> is called after
			/// repacking to clear the texture cache. See remarks in <see cref="GetRepackedAttachments"/> and
			/// <see cref="GetRepackedSkin"/> for additional info.</summary>
			public bool clearCache;
			/// <summary>
			/// When <c>true</c>, original non-texture-region attachments (e.g. bounding box or point attachments) are
			/// attached to the new skin as-is.
			/// </summary>
			public bool useOriginalNonrenderables;

			/// <summary>Optional additional textures (such as normal maps) to copy while repacking.
			/// To copy e.g. the main texture and normal maps, set it to
			/// <c>new int[] { Shader.PropertyToID("_BumpMap") }</c>.
			/// </summary>
			public int[] additionalTexturePropertyIDsToCopy;
			/// <summary>When non-null, this array will be used as <c>TextureFormat</c> at the Texture at the respective property.
			/// When set to <c>null</c> or when its array size is smaller, <c>textureFormat</c> is used where there
			/// exists no corresponding array item.</summary>
			public TextureFormat[] additionalTextureFormats;
			/// <summary>When non-null, this array will be used to determine whether <c>linear</c> or <c>sRGB</c> color space is
			/// used at the Texture at the respective property.
			/// When set to <c>null</c>, <c>linear</c> color space is assumed at every additional Texture element.
			/// When e.g. packing the main texture and normal maps, assign <c>new bool[] { true }</c> at this parameters,
			/// because normal maps use linear color space.</summary>
			public bool[] additionalTextureIsLinear;

			/// <summary>Default settings providing reasonable parameters, modify according to your needs.</summary>
			public static RepackAttachmentsSettings Default = new RepackAttachmentsSettings(true);

			/// <summary>Hidden pseudo-default ctor, use <see cref="Default"/> instead.</summary>
			private RepackAttachmentsSettings (bool _) {
				newAssetName = DefaultTextureName;

				maxAtlasSize = 1024;
				padding = 2;
				textureFormat = SpineTextureFormat;
				mipmaps = UseMipMaps;
				clearCache = false;
				useOriginalNonrenderables = true;

				shader = null;
				materialPropertySource = null;
				additiveMaterialSource = null;
				multiplyMaterialSource = null;
				screenMaterialSource = null;

				additionalTexturePropertyIDsToCopy = null;
				additionalTextureFormats = null;
				additionalTextureIsLinear = null;
			}

			/// <summary>
			/// Default settings providing reasonable parameters, with source materials assigned according to the
			/// provided <paramref name="skeletonDataAsset"/>. Modify according to your needs.
			/// </summary>
			/// <param name="skeletonDataAsset">Reference <see cref="SkeletonDataAsset"/> used to provide source
			/// materials for all blend modes.</param>
			public RepackAttachmentsSettings (SkeletonDataAsset skeletonDataAsset)
				: this(true) {
				UseSourceMaterialsFrom(skeletonDataAsset);
			}

			/// <summary>
			/// Assigns source materials from the provided <paramref name="skeletonDataAsset"/> for all blend modes
			/// including normal blend mode.
			/// </summary>
			public void UseSourceMaterialsFrom (SkeletonDataAsset skeletonDataAsset) {
				materialPropertySource = skeletonDataAsset.atlasAssets[0].PrimaryMaterial;
				UseBlendModeMaterialsFrom(skeletonDataAsset);
			}

			/// <summary>
			/// Assigns source materials from the provided <paramref name="skeletonDataAsset"/> for
			/// additive, multiply and screen blend modes.
			/// </summary>
			public void UseBlendModeMaterialsFrom (SkeletonDataAsset skeletonDataAsset) {
				BlendModeMaterials materials = skeletonDataAsset.blendModeMaterials;
				if (materials.additiveMaterials.Count > 0)
					additiveMaterialSource = materials.additiveMaterials[0].material;
				if (materials.multiplyMaterials.Count > 0)
					multiplyMaterialSource = materials.multiplyMaterials[0].material;
				if (materials.screenMaterials.Count > 0)
					screenMaterialSource = materials.screenMaterials[0].material;
			}
		}

		private struct BlendModeAtlasPages {
			public AtlasPage normalPage;
			public AtlasPage additivePage;
			public AtlasPage multiplyPage;
			public AtlasPage screenPage;
		}

		/// <summary>
		/// Fills the outputAttachments list with new attachment objects based on the attachments in sourceAttachments,
		/// but mapped to a new single texture using the same material.</summary>
		/// <remarks>
		/// This variant of <c>GetRepackedAttachments</c> supports repacking blend mode attachments.
		/// To enable blend mode repacking, assign a reference material at either
		/// <see cref="RepackAttachmentsSettings.additiveMaterialSource"/>,
		/// <see cref="RepackAttachmentsSettings.multiplyMaterialSource"/> or
		/// <see cref="RepackAttachmentsSettings.screenMaterialSource"/> of parameter <paramref name="settings"/>.
		/// Otherwise any blend mode attachments are treated as normal blend mode and repacked using a single material
		/// and atlas page.
		/// </remarks>
		/// <remarks>Returned <c>Material</c> and <c>Texture</c> behave like <c>new Texture2D()</c>, thus you need to
		/// call <c>Destroy()</c> to free resources.
		/// This method caches necessary Texture copies for later re-use, which might steadily increase the texture
		/// memory footprint when used excessively.
		/// Set <see cref="RepackAttachmentsSettings.clearCache"/> to <c>true</c> at the argument
		/// <paramref name="settings"/> or call <see cref="AtlasUtilities.ClearCache()"/> to clear this texture cache.
		/// You may want to call <c>Resources.UnloadUnusedAssets()</c> after that.
		/// </remarks>
		/// <param name="sourceAttachments">The list of attachments to be repacked.</param>
		/// <param name = "outputAttachments">The List(Attachment) to populate with the newly created Attachment objects.
		/// May be equal to <c>sourceAttachments</c> for in-place operation.</param>
		/// <param name="settings">Repack configuration settings, see <see cref="RepackAttachmentsSettings"/>.</param>
		/// <param name="output">Repack output struct holding generated material and texture references for
		/// potential later cleanup.</param>
		public static void GetRepackedAttachments (List<Attachment> sourceAttachments, List<Attachment> outputAttachments,
			RepackAttachmentsSettings settings, ref RepackAttachmentsOutput output) {

			if (sourceAttachments == null) throw new System.ArgumentNullException("sourceAttachments");
			if (outputAttachments == null) throw new System.ArgumentNullException("outputAttachments");

			if (settings.shader == null)
				settings.shader = settings.materialPropertySource == null ?
					Shader.Find("Spine/Skeleton") : settings.materialPropertySource.shader;

			output.outputTexture = null;
			if (settings.additionalTexturePropertyIDsToCopy != null && settings.additionalTextureIsLinear == null) {
				settings.additionalTextureIsLinear = new bool[settings.additionalTexturePropertyIDsToCopy.Length];
				for (int i = 0; i < settings.additionalTextureIsLinear.Length; ++i) {
					settings.additionalTextureIsLinear[i] = true;
				}
			}

			// Use these to detect and use shared regions.
			existingRegions.Clear();
			regionIndices.Clear();

			// Collect all textures from original attachments.
			int numTextureParamsToRepack = 1 + (settings.additionalTexturePropertyIDsToCopy == null ?
				0 : settings.additionalTexturePropertyIDsToCopy.Length);

			if (texturesToPackAtParam.Length < numTextureParamsToRepack)
				Array.Resize(ref texturesToPackAtParam, numTextureParamsToRepack);
			for (int i = 0; i < numTextureParamsToRepack; ++i) {
				if (texturesToPackAtParam[i] != null)
					texturesToPackAtParam[i].Clear();
				else
					texturesToPackAtParam[i] = new List<Texture2D>();
			}
			originalRegions.Clear();

			bool isInPlaceOperation = object.ReferenceEquals(sourceAttachments, outputAttachments);
			if (!isInPlaceOperation) {
				outputAttachments.Clear();
				outputAttachments.AddRange(sourceAttachments);
			}

			int newRegionIndex = 0;
			for (int attachmentIndex = 0, n = sourceAttachments.Count; attachmentIndex < n; attachmentIndex++) {
				Attachment originalAttachment = sourceAttachments[attachmentIndex];

				if (originalAttachment is IHasSequence) {
					IHasSequence originalTextureAttachment = (IHasSequence)originalAttachment;
					Attachment newAttachment = originalAttachment.Copy();
					AtlasRegion firstRegion = (AtlasRegion)originalTextureAttachment.Sequence.Regions[0];
					int existingIndex;
					if (existingRegions.TryGetValue(firstRegion, out existingIndex)) {
						regionIndices.Add(existingIndex);
					} else {
						existingRegions.Add(firstRegion, newRegionIndex);
						Sequence originalSequence = originalTextureAttachment.Sequence;
						for (int i = 0, regionCount = originalSequence.Regions.Length; i < regionCount; ++i) {
							AtlasRegion sequenceRegion = (AtlasRegion)originalSequence.Regions[i];
							AddRegionTexturesToPack(numTextureParamsToRepack, sequenceRegion,
								settings.textureFormat, settings.mipmaps, settings.additionalTextureFormats,
								settings.additionalTexturePropertyIDsToCopy, settings.additionalTextureIsLinear);
							originalRegions.Add(sequenceRegion);
							regionIndices.Add(newRegionIndex);
							newRegionIndex++;
						}
					}
					outputAttachments[attachmentIndex] = newAttachment;
				} else {
					outputAttachments[attachmentIndex] = settings.useOriginalNonrenderables ?
						originalAttachment : originalAttachment.Copy();
					regionIndices.Add(NonrenderingRegion); // Output attachments pairs with regionIndices list 1:1. Pad with a sentinel if the attachment doesn't have a region.
				}
			}

			// Rehydrate the repacked textures as a Material, Spine atlas and Spine.AtlasAttachments
			Material newMaterial = new Material(settings.shader);
			if (settings.materialPropertySource != null) {
				newMaterial.CopyPropertiesFromMaterial(settings.materialPropertySource);
				newMaterial.shaderKeywords = settings.materialPropertySource.shaderKeywords;
			}
			newMaterial.name = settings.newAssetName;

			Rect[] rects = null;
			for (int i = 0; i < numTextureParamsToRepack; ++i) {
				// Fill a new texture with the collected attachment textures.
				TextureFormat format = (i > 0 &&
					settings.additionalTextureFormats != null &&
					i - 1 < settings.additionalTextureFormats.Length) ?
						settings.additionalTextureFormats[i - 1] : settings.textureFormat;
				bool linear = (i > 0) ? settings.additionalTextureIsLinear[i - 1] : false;
				Texture2D newTexture = new Texture2D(settings.maxAtlasSize, settings.maxAtlasSize,
					format, settings.mipmaps, linear);
				newTexture.mipMapBias = AtlasUtilities.DefaultMipmapBias;

				List<Texture2D> texturesToPack = texturesToPackAtParam[i];
				if (texturesToPack.Count > 0) {
					Texture2D sourceTexture = texturesToPack[0];
					newTexture.CopyTextureAttributesFrom(sourceTexture);
				}
				newTexture.name = settings.newAssetName;
				Rect[] rectsForTexParam = newTexture.PackTextures(texturesToPack.ToArray(),
					settings.padding, settings.maxAtlasSize);
				if (i == 0) {
					rects = rectsForTexParam;
					newMaterial.mainTexture = newTexture;
					output.outputTexture = newTexture;
				} else {
					newMaterial.SetTexture(settings.additionalTexturePropertyIDsToCopy[i - 1], newTexture);
					if (output.additionalOutputTextures != null && output.additionalOutputTextures.Length > i - 1)
						output.additionalOutputTextures[i - 1] = newTexture;
				}
			}

			AtlasPage page = newMaterial.ToSpineAtlasPage();
			page.name = settings.newAssetName;

			BlendModeAtlasPages blendModePages = new BlendModeAtlasPages();
			blendModePages.normalPage = page;

			repackedRegions.Clear();
			for (int i = 0, n = originalRegions.Count; i < n; i++) {
				AtlasRegion oldRegion = originalRegions[i];
				AtlasRegion newRegion = UVRectToAtlasRegion(rects[i], oldRegion, page);
				repackedRegions.Add(newRegion);
			}

			Material additiveMaterialSource = settings.additiveMaterialSource;
			Material multiplyMaterialSource = settings.multiplyMaterialSource;
			Material screenMaterialSource = settings.screenMaterialSource;
			bool enableBlendModes =
				(additiveMaterialSource != null) ||
				(multiplyMaterialSource != null) ||
				(screenMaterialSource != null);
			Shader normalShader = settings.shader;

			// Map the cloned attachments to the repacked atlas.
			for (int attachmentIndex = 0, repackedIndex = 0, n = outputAttachments.Count;
				attachmentIndex < n; ++attachmentIndex, ++repackedIndex) {

				Attachment attachment = outputAttachments[attachmentIndex];
				IHasSequence textureAttachment = attachment as IHasSequence;
				if (textureAttachment != null) {
					TextureRegion[] regions = textureAttachment.Sequence.Regions;
					for (int r = 0, regionCount = regions.Length; r < regionCount; ++r) {
						TextureRegion originalRegion = regions[r];
						TextureRegion repackedRegion = repackedRegions[regionIndices[repackedIndex++]];
						if (enableBlendModes) {
							AssignBlendMode(ref repackedRegion, originalRegion, normalShader, ref blendModePages,
								additiveMaterialSource, multiplyMaterialSource, screenMaterialSource);
						}
						regions[r] = repackedRegion;
					}
					--repackedIndex;
					
					textureAttachment.UpdateSequence();
				}
			}

			// Clean up.
			if (settings.clearCache)
				AtlasUtilities.ClearCache();

			output.outputMaterial = newMaterial;

			output.outputAdditiveMaterial = blendModePages.additivePage != null ? (Material)blendModePages.additivePage.rendererObject : null;
			output.outputMultiplyMaterial = blendModePages.multiplyPage != null ? (Material)blendModePages.multiplyPage.rendererObject : null;
			output.outputScreenMaterial = blendModePages.screenPage != null ? (Material)blendModePages.screenPage.rendererObject : null;
		}

		/// <summary>
		/// Fills the outputAttachments list with new attachment objects based on the attachments in sourceAttachments,
		/// but mapped to a new single texture using the same material. All blend modes are treated as normal blend mode.
		/// Use <see cref="GetRepackedAttachments(List{Attachment}, List{Attachment}, RepackAttachmentsSettings,
		/// ref RepackAttachmentsOutput)"/> if blend modes shall be retained.
		/// </summary>
		/// <remarks>Returned <c>Material</c> and <c>Texture</c> behave like <c>new Texture2D()</c>, thus you need to call <c>Destroy()</c>
		/// to free resources.
		/// This method caches necessary Texture copies for later re-use, which might steadily increase the texture memory
		/// footprint when used excessively. Set <paramref name="clearCache"/> to <c>true</c>
		/// or call <see cref="AtlasUtilities.ClearCache()"/> to clear this texture cache.
		/// You may want to call <c>Resources.UnloadUnusedAssets()</c> after that.
		/// </remarks>
		/// <param name="sourceAttachments">The list of attachments to be repacked.</param>
		/// <param name = "outputAttachments">The List(Attachment) to populate with the newly created Attachment objects.
		/// May be equal to <c>sourceAttachments</c> for in-place operation.</param>
		/// <param name="materialPropertySource">May be null. If no Material property source is provided, a material with
		/// default parameters using the provided <c>shader</c> will be created.</param>
		/// <param name="clearCache">When set to <c>true</c>, <see cref="AtlasUtilities.ClearCache()"/> is called after
		/// repacking to clear the texture cache. See remarks for additional info.</param>
		/// <param name="additionalTexturePropertyIDsToCopy">Optional additional textures (such as normal maps) to copy while repacking.
		/// To copy e.g. the main texture and normal maps, pass 'new int[] { Shader.PropertyToID("_BumpMap") }' at this parameter.</param>
		/// <param name="additionalOutputTextures">When <c>additionalTexturePropertyIDsToCopy</c> is non-null,
		/// this array will be filled with the resulting repacked texture for every property,
		/// just as the main repacked texture is assigned to <c>outputTexture</c>.</param>
		/// <param name="additionalTextureFormats">When <c>additionalTexturePropertyIDsToCopy</c> is non-null,
		/// this array will be used as <c>TextureFormat</c> at the Texture at the respective property.
		/// When <c>additionalTextureFormats</c> is <c>null</c> or when its array size is smaller,
		/// <c>textureFormat</c> is used where there exists no corresponding array item.</param>
		/// <param name="additionalTextureIsLinear">When <c>additionalTexturePropertyIDsToCopy</c> is non-null,
		/// this array will be used to determine whether <c>linear</c> or <c>sRGB</c> color space is used at the
		/// Texture at the respective property. When <c>additionalTextureIsLinear</c> is <c>null</c>, <c>linear</c> color space
		/// is assumed at every additional Texture element.
		/// When e.g. packing the main texture and normal maps, pass 'new bool[] { true }' at this parameter, because normal maps use
		/// linear color space.</param>
		public static void GetRepackedAttachments (List<Attachment> sourceAttachments, List<Attachment> outputAttachments, Material materialPropertySource,
			out Material outputMaterial, out Texture2D outputTexture,
			int maxAtlasSize = 1024, int padding = 2, TextureFormat textureFormat = SpineTextureFormat, bool mipmaps = UseMipMaps,
			string newAssetName = "Repacked Attachments", bool clearCache = false, bool useOriginalNonrenderables = true,
			int[] additionalTexturePropertyIDsToCopy = null, Texture2D[] additionalOutputTextures = null,
			TextureFormat[] additionalTextureFormats = null, bool[] additionalTextureIsLinear = null) {

			Shader shader = materialPropertySource == null ? Shader.Find("Spine/Skeleton") : materialPropertySource.shader;
			GetRepackedAttachments(sourceAttachments, outputAttachments, shader, out outputMaterial, out outputTexture,
				maxAtlasSize, padding, textureFormat, mipmaps, newAssetName,
				materialPropertySource, clearCache, useOriginalNonrenderables,
				additionalTexturePropertyIDsToCopy, additionalOutputTextures,
				additionalTextureFormats, additionalTextureIsLinear);
		}

		/// <summary>
		/// Fills the outputAttachments list with new attachment objects based on the attachments in sourceAttachments,
		/// but mapped to a new single texture using the same material. All blend modes are treated as normal blend mode.
		/// Use <see cref="GetRepackedAttachments(List{Attachment}, List{Attachment}, RepackAttachmentsSettings,
		/// ref RepackAttachmentsOutput)"/> if blend modes shall be retained.
		/// </summary>
		/// <remarks>Returned <c>Material</c> and <c>Texture</c> behave like <c>new Texture2D()</c>, thus you need to call <c>Destroy()</c>
		/// to free resources.</remarks>
		/// <param name="sourceAttachments">The list of attachments to be repacked.</param>
		/// <param name = "outputAttachments">The List(Attachment) to populate with the newly created Attachment objects.
		/// May be equal to <c>sourceAttachments</c> for in-place operation.</param>
		/// <param name="materialPropertySource">May be null. If no Material property source is provided, a material with
		/// default parameters using the provided <c>shader</c> will be created.</param>
		/// <param name="additionalTexturePropertyIDsToCopy">Optional additional textures (such as normal maps) to copy while repacking.
		/// To copy e.g. the main texture and normal maps, pass 'new int[] { Shader.PropertyToID("_BumpMap") }' at this parameter.</param>
		/// <param name="additionalOutputTextures">When <c>additionalTexturePropertyIDsToCopy</c> is non-null,
		/// this array will be filled with the resulting repacked texture for every property,
		/// just as the main repacked texture is assigned to <c>outputTexture</c>.</param>
		/// <param name="additionalTextureFormats">When <c>additionalTexturePropertyIDsToCopy</c> is non-null,
		/// this array will be used as <c>TextureFormat</c> at the Texture at the respective property.
		/// When <c>additionalTextureFormats</c> is <c>null</c> or when its array size is smaller,
		/// <c>textureFormat</c> is used where there exists no corresponding array item.</param>
		/// <param name="additionalTextureIsLinear">When <c>additionalTexturePropertyIDsToCopy</c> is non-null,
		/// this array will be used to determine whether <c>linear</c> or <c>sRGB</c> color space is used at the
		/// Texture at the respective property. When <c>additionalTextureIsLinear</c> is <c>null</c>, <c>linear</c> color space
		/// is assumed at every additional Texture element.
		/// When e.g. packing the main texture and normal maps, pass 'new bool[] { true }' at this parameter, because normal maps use
		/// linear color space.</param>
		public static void GetRepackedAttachments (List<Attachment> sourceAttachments, List<Attachment> outputAttachments, Shader shader,
			out Material outputMaterial, out Texture2D outputTexture,
			int maxAtlasSize = 1024, int padding = 2, TextureFormat textureFormat = SpineTextureFormat, bool mipmaps = UseMipMaps,
			string newAssetName = "Repacked Attachments",
			Material materialPropertySource = null, bool clearCache = false, bool useOriginalNonrenderables = true,
			int[] additionalTexturePropertyIDsToCopy = null, Texture2D[] additionalOutputTextures = null,
			TextureFormat[] additionalTextureFormats = null, bool[] additionalTextureIsLinear = null) {

			RepackAttachmentsSettings settings = new RepackAttachmentsSettings {
				shader = shader,
				maxAtlasSize = maxAtlasSize,
				padding = padding,
				textureFormat = textureFormat,
				mipmaps = mipmaps,
				newAssetName = newAssetName,
				materialPropertySource = materialPropertySource,
				clearCache = clearCache,
				useOriginalNonrenderables = useOriginalNonrenderables,
				additionalTexturePropertyIDsToCopy = additionalTexturePropertyIDsToCopy,
				additionalTextureFormats = additionalTextureFormats,
				additionalTextureIsLinear = additionalTextureIsLinear
			};
			RepackAttachmentsOutput output = new RepackAttachmentsOutput();
			output.additionalOutputTextures = additionalOutputTextures;
			GetRepackedAttachments(sourceAttachments, outputAttachments, settings, ref output);

			outputMaterial = output.outputMaterial;
			outputTexture = output.outputTexture;
		}

		private static void AssignBlendMode (ref TextureRegion repackedRegion, TextureRegion originalRegion,
			Shader normalShader, ref BlendModeAtlasPages blendModePages,
			Material additiveMaterialSource, Material multiplyMaterialSource, Material screenMaterialSource) {

			Material material = ((AtlasRegion)originalRegion).page.rendererObject as Material;
			if (material == null) return;

			if (material.shader != normalShader) {
				if (material.shader.name.Contains("Multiply")) {
					((AtlasRegion)repackedRegion).page = GetBlendModePage(ref blendModePages.multiplyPage,
						blendModePages.normalPage, multiplyMaterialSource, "-Multiply");
				} else if (material.shader.name.Contains("Screen")) {
					((AtlasRegion)repackedRegion).page = GetBlendModePage(ref blendModePages.screenPage,
						blendModePages.normalPage, screenMaterialSource, "-Screen");
				} else if (material.shader.name.Contains("Additive")) {
					((AtlasRegion)repackedRegion).page = GetBlendModePage(ref blendModePages.additivePage,
						blendModePages.normalPage, additiveMaterialSource, "-Additive");
				}
			}
		}

		private static AtlasPage GetBlendModePage (ref AtlasPage targetPage, AtlasPage normalReferencePage,
			Material materialSource, string nameSuffix) {

			if (targetPage == null) {
				targetPage = normalReferencePage.Clone();
				Material material = new Material(materialSource);
				material.mainTexture = ((Material)normalReferencePage.rendererObject).mainTexture;
				material.name = material.mainTexture.name + nameSuffix;
				targetPage.rendererObject = material;
			}
			return targetPage;
		}

		private static void AddRegionTexturesToPack (int numTextureParamsToRepack, AtlasRegion region,
			TextureFormat textureFormat, bool mipmaps, TextureFormat[] additionalTextureFormats,
			int[] additionalTexturePropertyIDsToCopy, bool[] additionalTextureIsLinear) {

			for (int i = 0; i < numTextureParamsToRepack; ++i) {
				Texture2D regionTexture = (i == 0 ?
					region.ToTexture(textureFormat, mipmaps) :
					region.ToTexture((additionalTextureFormats != null && i - 1 < additionalTextureFormats.Length) ?
						additionalTextureFormats[i - 1] : textureFormat,
						mipmaps, additionalTexturePropertyIDsToCopy[i - 1], additionalTextureIsLinear[i - 1]));
				texturesToPackAtParam[i].Add(regionTexture);
			}
		}

		/// <summary>
		/// Creates and populates a duplicate skin with cloned attachments that are backed by a new packed texture atlas
		/// comprised of all the regions from the original skin. Supports blend modes if configured accordingly.
		/// </summary>
		/// <remarks>
		/// <para>GetRepackedSkin is an expensive operation, preferably call it at level load time.
		/// No Spine.Atlas object is created so there is no way to find AtlasRegions except through the Attachments
		/// using them. Returned <c>Material</c> and <c>Texture</c> behave like <c>new Texture2D()</c>, thus you
		/// need to call <c>Destroy()</c> to free resources.
		/// This method caches necessary Texture copies for later re-use, which might steadily increase the texture memory
		/// footprint when used excessively.
		/// Set <see cref="RepackAttachmentsSettings.clearCache"/> to <c>true</c> at the argument
		/// <paramref name="settings"/> or call <see cref="AtlasUtilities.ClearCache()"/> to clear this texture cache.
		/// You may want to call <c>Resources.UnloadUnusedAssets()</c> after that.
		/// </para><para>
		/// This variant of <c>GetRepackedSkin</c> supports repacking blend mode attachments.
		/// To enable blend mode repacking, assign a reference material at either
		/// <see cref="RepackAttachmentsSettings.additiveMaterialSource"/>,
		/// <see cref="RepackAttachmentsSettings.multiplyMaterialSource"/> or
		/// <see cref="RepackAttachmentsSettings.screenMaterialSource"/> of parameter <paramref name="settings"/>.
		/// Otherwise any blend mode attachments are treated as normal blend mode and repacked using a single material
		/// and atlas page.</para>
		/// </remarks>
		/// <param name="settings">Repack configuration settings, see <see cref="RepackAttachmentsSettings"/>.</param>
		/// <param name="output">Repack output struct holding generated material and texture references for
		/// potential later cleanup.</param>
		public static Skin GetRepackedSkin (this Skin o, string newName,
			RepackAttachmentsSettings settings, ref RepackAttachmentsOutput output) {

			if (o == null) throw new System.NullReferenceException("Skin was null");
			ICollection<Skin.SkinEntry> skinAttachments = o.Attachments;
			Skin newSkin = new Skin(newName);

			newSkin.Bones.AddRange(o.Bones);
			newSkin.Constraints.AddRange(o.Constraints);

			inoutAttachments.Clear();
			foreach (Skin.SkinEntry entry in skinAttachments) {
				inoutAttachments.Add(entry.Attachment);
			}
			GetRepackedAttachments(inoutAttachments, inoutAttachments, settings, ref output);
			int i = 0;
			foreach (Skin.SkinEntry originalSkinEntry in skinAttachments) {
				Attachment newAttachment = inoutAttachments[i++];
				newSkin.SetAttachment(originalSkinEntry.SlotIndex, originalSkinEntry.PlaceholderName, newAttachment);
			}
			return newSkin;
		}

		/// <summary>
		/// Creates and populates a duplicate skin with cloned attachments that are backed by a new packed texture atlas
		/// comprised of all the regions from the original skin. All blend modes are treated as normal blend mode.
		/// Use <see cref="GetRepackedSkin(Skin, string, RepackAttachmentsSettings, ref RepackAttachmentsOutput)"/> if
		/// blend modes shall be retained.
		/// </summary>
		/// <remarks>GetRepackedSkin is an expensive operation, preferably call it at level load time.
		/// No Spine.Atlas object is created so there is no way to find AtlasRegions except through the Attachments using them.
		/// Returned <c>Material</c> and <c>Texture</c> behave like <c>new Texture2D()</c>, thus you need to call <c>Destroy()</c>
		/// to free resources.
		/// This method caches necessary Texture copies for later re-use, which might steadily increase the texture memory
		/// footprint when used excessively. Set <paramref name="clearCache"/> to <c>true</c>
		/// or call <see cref="AtlasUtilities.ClearCache()"/> to clear this texture cache.
		/// You may want to call <c>Resources.UnloadUnusedAssets()</c> after that.
		/// </remarks>
		/// <param name="clearCache">When set to <c>true</c>, <see cref="AtlasUtilities.ClearCache()"/> is called after
		/// repacking to clear the texture cache. See remarks for additional info.</param>
		/// <param name="additionalTexturePropertyIDsToCopy">Optional additional textures (such as normal maps) to copy while repacking.
		/// To copy e.g. the main texture and normal maps, pass 'new int[] { Shader.PropertyToID("_BumpMap") }' at this parameter.</param>
		/// <param name="additionalOutputTextures">When <c>additionalTexturePropertyIDsToCopy</c> is non-null,
		/// this array will be filled with the resulting repacked texture for every property,
		/// just as the main repacked texture is assigned to <c>outputTexture</c>.</param>
		/// <param name="additionalTextureFormats">When <c>additionalTexturePropertyIDsToCopy</c> is non-null,
		/// this array will be used as <c>TextureFormat</c> at the Texture at the respective property.
		/// When <c>additionalTextureFormats</c> is <c>null</c> or when its array size is smaller,
		/// <c>textureFormat</c> is used where there exists no corresponding array item.</param>
		/// <param name="additionalTextureIsLinear">When <c>additionalTexturePropertyIDsToCopy</c> is non-null,
		/// this array will be used to determine whether <c>linear</c> or <c>sRGB</c> color space is used at the
		/// Texture at the respective property. When <c>additionalTextureIsLinear</c> is <c>null</c>, <c>linear</c> color space
		/// is assumed at every additional Texture element.
		/// When e.g. packing the main texture and normal maps, pass 'new bool[] { true }' at this parameter, because normal maps use
		/// linear color space.</param>
		public static Skin GetRepackedSkin (this Skin o, string newName, Material materialPropertySource, out Material outputMaterial, out Texture2D outputTexture,
			int maxAtlasSize = 1024, int padding = 2, TextureFormat textureFormat = SpineTextureFormat, bool mipmaps = UseMipMaps,
			bool useOriginalNonrenderables = true, bool clearCache = false,
			int[] additionalTexturePropertyIDsToCopy = null, Texture2D[] additionalOutputTextures = null,
			TextureFormat[] additionalTextureFormats = null, bool[] additionalTextureIsLinear = null) {

			return GetRepackedSkin(o, newName, materialPropertySource.shader, out outputMaterial, out outputTexture,
				maxAtlasSize, padding, textureFormat, mipmaps, materialPropertySource,
				clearCache, useOriginalNonrenderables, additionalTexturePropertyIDsToCopy, additionalOutputTextures,
				additionalTextureFormats, additionalTextureIsLinear);
		}

		/// <summary>
		/// Creates and populates a duplicate skin with cloned attachments that are backed by a new packed texture atlas
		/// comprised of all the regions from the original skin. All blend modes are treated as normal blend mode.
		/// </summary>
		/// <remarks>See documentation of <see cref="GetRepackedSkin"/> for details.</remarks>
		public static Skin GetRepackedSkin (this Skin o, string newName, Shader shader, out Material outputMaterial, out Texture2D outputTexture,
			int maxAtlasSize = 1024, int padding = 2, TextureFormat textureFormat = SpineTextureFormat, bool mipmaps = UseMipMaps,
			Material materialPropertySource = null, bool clearCache = false, bool useOriginalNonrenderables = true,
			int[] additionalTexturePropertyIDsToCopy = null, Texture2D[] additionalOutputTextures = null,
			TextureFormat[] additionalTextureFormats = null, bool[] additionalTextureIsLinear = null) {

			RepackAttachmentsSettings settings = new RepackAttachmentsSettings {
				shader = shader,
				maxAtlasSize = maxAtlasSize,
				padding = padding,
				textureFormat = textureFormat,
				mipmaps = mipmaps,
				newAssetName = newName,
				materialPropertySource = materialPropertySource,
				clearCache = clearCache,
				useOriginalNonrenderables = useOriginalNonrenderables,
				additionalTexturePropertyIDsToCopy = additionalTexturePropertyIDsToCopy,
				additionalTextureFormats = additionalTextureFormats,
				additionalTextureIsLinear = additionalTextureIsLinear
			};
			RepackAttachmentsOutput output = new RepackAttachmentsOutput();
			output.additionalOutputTextures = additionalOutputTextures;

			Skin repackedSkin = o.GetRepackedSkin(newName, settings, ref output);
			outputMaterial = output.outputMaterial;
			outputTexture = output.outputTexture;
			return repackedSkin;
		}

		public static Sprite ToSprite (this AtlasRegion ar, float pixelsPerUnit = 100) {
			return Sprite.Create(ar.GetMainTexture(), ar.GetUnityRect(), new Vector2(0.5f, 0.5f), pixelsPerUnit);
		}

		struct IntAndAtlasRegionKey {
			int i;
			AtlasRegion region;

			public IntAndAtlasRegionKey (int i, AtlasRegion region) {
				this.i = i;
				this.region = region;
			}

			public override int GetHashCode () {
				return i.GetHashCode() * 23 ^ region.GetHashCode();
			}
		}
		static Dictionary<IntAndAtlasRegionKey, Texture2D> CachedRegionTextures = new Dictionary<IntAndAtlasRegionKey, Texture2D>();
		static List<Texture2D> CachedRegionTexturesList = new List<Texture2D>();

		/// <summary>
		/// Frees up textures cached by repacking and remapping operations.
		///
		/// Calling <see cref="AttachmentRegionExtensions.SetRegion(Attachment, Sprite, Material, bool, bool, bool, bool, TextureFormat, bool)"/>
		/// with parameter <c>premultiplyAlpha=true</c>,
		/// <see cref="GetRepackedAttachments"/> or <see cref="GetRepackedSkin"/> will cache textures for later re-use,
		///	which might steadily increase the texture memory footprint when used excessively.
		///	You can clear this Texture cache by calling <see cref="AtlasUtilities.ClearCache()"/>.
		/// You may also want to call <c>Resources.UnloadUnusedAssets()</c> after that. Be aware that while this cleanup
		/// frees up memory, it is also a costly operation and will likely cause a spike in the framerate.
		/// Thus it is recommended to perform costly repacking and cleanup operations after e.g. a character customization
		/// screen has been exited, and if required additionally after a certain number of <c>SetRegion()</c> calls.
		/// </summary>
		public static void ClearCache () {
			foreach (Texture2D t in CachedRegionTexturesList) {
				UnityEngine.Object.Destroy(t);
			}
			CachedRegionTextures.Clear();
			CachedRegionTexturesList.Clear();
		}

		/// <summary>Creates a new Texture2D object based on an AtlasRegion.
		/// If applyImmediately is true, Texture2D.Apply is called immediately after the Texture2D is filled with data.</summary>
		public static Texture2D ToTexture (this AtlasRegion ar, TextureFormat textureFormat = SpineTextureFormat, bool mipmaps = UseMipMaps,
			int texturePropertyId = 0, bool linear = false, bool applyPMA = false) {

			Texture2D output;

			IntAndAtlasRegionKey cacheKey = new IntAndAtlasRegionKey(texturePropertyId, ar);
			CachedRegionTextures.TryGetValue(cacheKey, out output);
			if (output == null) {
				Texture2D sourceTexture = texturePropertyId == 0 ? ar.GetMainTexture() : ar.GetTexture(texturePropertyId);
				Rect r = ar.GetUnityRect();
				int width = (int)r.width;
				int height = (int)r.height;
				output = new Texture2D(width, height, textureFormat, mipmaps, linear) { name = ar.name };
				output.CopyTextureAttributesFrom(sourceTexture);
				if (applyPMA)
					AtlasUtilities.CopyTextureApplyPMA(sourceTexture, r, output);
				else
					AtlasUtilities.CopyTexture(sourceTexture, r, output);
				CachedRegionTextures.Add(cacheKey, output);
				CachedRegionTexturesList.Add(output);
			}

			return output;
		}

		static Texture2D ToTexture (this Sprite s, TextureFormat textureFormat = SpineTextureFormat,
			bool mipmaps = UseMipMaps, bool linear = false, bool applyPMA = false) {

			Texture2D spriteTexture = s.texture;
			Rect r;
			if (!s.packed || s.packingMode == SpritePackingMode.Rectangle) {
				r = s.textureRect;
			} else {
				r = new Rect();
				r.xMin = Math.Min(s.uv[0].x, s.uv[1].x) * spriteTexture.width;
				r.xMax = Math.Max(s.uv[0].x, s.uv[1].x) * spriteTexture.width;
				r.yMin = Math.Min(s.uv[0].y, s.uv[2].y) * spriteTexture.height;
				r.yMax = Math.Max(s.uv[0].y, s.uv[2].y) * spriteTexture.height;
#if UNITY_EDITOR
				if (s.uv.Length > 4) {
					Debug.LogError("When using a tightly packed SpriteAtlas with Spine, you may only access Sprites that are packed as 'FullRect' from it! " +
						"You can either disable 'Tight Packing' at the whole SpriteAtlas, or change the single Sprite's TextureImporter Setting 'MeshType' to 'Full Rect'." +
						"Sprite Asset: " + s.name, s);
				}
#endif
			}
			Texture2D newTexture = new Texture2D((int)r.width, (int)r.height, textureFormat, mipmaps, linear);
			newTexture.CopyTextureAttributesFrom(spriteTexture);
			if (applyPMA)
				AtlasUtilities.CopyTextureApplyPMA(spriteTexture, r, newTexture);
			else
				AtlasUtilities.CopyTexture(spriteTexture, r, newTexture);
			return newTexture;
		}

		static Texture2D GetClone (this Texture2D t, TextureFormat textureFormat = SpineTextureFormat,
			bool mipmaps = UseMipMaps, bool linear = false, bool applyPMA = false) {

			Texture2D newTexture = new Texture2D((int)t.width, (int)t.height, textureFormat, mipmaps, linear);
			newTexture.CopyTextureAttributesFrom(t);
			if (applyPMA)
				AtlasUtilities.CopyTextureApplyPMA(t, new Rect(0, 0, t.width, t.height), newTexture);
			else
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

		static void CopyTextureApplyPMA (Texture2D source, Rect sourceRect, Texture2D destination) {
			Color[] pixelBuffer = source.GetPixels((int)sourceRect.x, (int)sourceRect.y, (int)sourceRect.width, (int)sourceRect.height);
			for (int i = 0, n = pixelBuffer.Length; i < n; i++) {
				Color p = pixelBuffer[i];
				float a = p.a;
				p.r = p.r * a;
				p.g = p.g * a;
				p.b = p.b * a;
				pixelBuffer[i] = p;
			}
			destination.SetPixels(pixelBuffer);
			destination.Apply();
		}

		static bool IsRenderable (Attachment a) {
			return a is IHasSequence;
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
			float width = region.packedWidth;
			float height = region.packedHeight;
			if (includeRotate && region.degrees == 270) {
				width = region.packedHeight;
				height = region.packedWidth;
			}
			return new Rect(region.x, region.y, width, height);
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
			Rect tr = UVRectToTextureRect(uvRect, page.width, page.height);
			Rect rr = tr.SpineUnityFlipRect(page.height);

			int x = (int)rr.x;
			int y = (int)rr.y;
			int w = (int)rr.width;
			int h = (int)rr.height;

			if (referenceRegion.degrees == 270) {
				int tempW = w;
				w = h;
				h = tempW;
			}

			// Note: originalW and originalH need to be scaled according to the
			// repacked width and height, repacking can mess with aspect ratio, etc.
			int originalW = Mathf.RoundToInt((float)w * ((float)referenceRegion.originalWidth / (float)referenceRegion.width));
			int originalH = Mathf.RoundToInt((float)h * ((float)referenceRegion.originalHeight / (float)referenceRegion.height));

			int offsetX = Mathf.RoundToInt((float)referenceRegion.offsetX * ((float)w / (float)referenceRegion.width));
			int offsetY = Mathf.RoundToInt((float)referenceRegion.offsetY * ((float)h / (float)referenceRegion.height));

			float u = uvRect.xMin;
			float u2 = uvRect.xMax;
			float v = uvRect.yMax;
			float v2 = uvRect.yMin;

			if (referenceRegion.degrees == 270) {
				// at a 270 degree region, u2/v2 deltas and atlas width/height are swapped, and delta-v is negative.
				float du = uvRect.width; // u2 - u;
				float dv = uvRect.height; // v - v2;
				float atlasAspectRatio = (float)page.width / (float)page.height;
				u2 = u + (dv / atlasAspectRatio);
				v2 = v - (du * atlasAspectRatio);
			}

			return new AtlasRegion {
				page = page,
				name = referenceRegion.name,

				u = u,
				u2 = u2,
				v = v,
				v2 = v2,

				index = -1,

				width = w,
				originalWidth = originalW,
				height = h,
				originalHeight = originalH,
				offsetX = offsetX,
				offsetY = offsetY,
				x = x,
				y = y,

				rotate = referenceRegion.rotate,
				degrees = referenceRegion.degrees
			};
		}

		/// <summary>
		/// Convenience method for getting the main texture of the material of the page of the region.</summary>
		static Texture2D GetMainTexture (this AtlasRegion region) {
			Material material = (region.page.rendererObject as Material);
			return material.mainTexture as Texture2D;
		}

		/// <summary>
		/// Convenience method for getting any texture of the material of the page of the region by texture property name.</summary>
		static Texture2D GetTexture (this AtlasRegion region, string texturePropertyName) {
			Material material = (region.page.rendererObject as Material);
			return material.GetTexture(texturePropertyName) as Texture2D;
		}

		/// <summary>
		/// Convenience method for getting any texture of the material of the page of the region by texture property id.</summary>
		static Texture2D GetTexture (this AtlasRegion region, int texturePropertyId) {
			Material material = (region.page.rendererObject as Material);
			return material.GetTexture(texturePropertyId) as Texture2D;
		}

		static void CopyTextureAttributesFrom (this Texture2D destination, Texture2D source) {
			destination.filterMode = source.filterMode;
			destination.anisoLevel = source.anisoLevel;
#if UNITY_EDITOR
			destination.alphaIsTransparency = source.alphaIsTransparency;
#endif
			destination.wrapModeU = source.wrapModeU;
			destination.wrapModeV = source.wrapModeV;
			destination.wrapModeW = source.wrapModeW;
		}
		#endregion

		static float InverseLerp (float a, float b, float value) {
			return (value - a) / (b - a);
		}
	}
}
