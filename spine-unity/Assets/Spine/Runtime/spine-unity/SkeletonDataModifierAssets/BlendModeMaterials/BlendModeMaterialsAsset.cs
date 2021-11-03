/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

using Spine;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity {
	[CreateAssetMenu(menuName = "Spine/SkeletonData Modifiers/Blend Mode Materials", order = 200)]
	public class BlendModeMaterialsAsset : SkeletonDataModifierAsset {
		public Material multiplyMaterialTemplate;
		public Material screenMaterialTemplate;
		public Material additiveMaterialTemplate;

		public bool applyAdditiveMaterial = true;

		public override void Apply (SkeletonData skeletonData) {
			ApplyMaterials(skeletonData, multiplyMaterialTemplate, screenMaterialTemplate, additiveMaterialTemplate, applyAdditiveMaterial);
		}

		public static void ApplyMaterials (SkeletonData skeletonData, Material multiplyTemplate, Material screenTemplate, Material additiveTemplate, bool includeAdditiveSlots) {
			if (skeletonData == null) throw new ArgumentNullException("skeletonData");

			using (var materialCache = new AtlasMaterialCache()) {
				var entryBuffer = new List<Skin.SkinEntry>();
				var slotsItems = skeletonData.Slots.Items;
				for (int slotIndex = 0, slotCount = skeletonData.Slots.Count; slotIndex < slotCount; slotIndex++) {
					var slot = slotsItems[slotIndex];
					if (slot.BlendMode == BlendMode.Normal) continue;
					if (!includeAdditiveSlots && slot.BlendMode == BlendMode.Additive) continue;

					entryBuffer.Clear();
					foreach (var skin in skeletonData.Skins)
						skin.GetAttachments(slotIndex, entryBuffer);

					Material templateMaterial = null;
					switch (slot.BlendMode) {
					case BlendMode.Multiply:
						templateMaterial = multiplyTemplate;
						break;
					case BlendMode.Screen:
						templateMaterial = screenTemplate;
						break;
					case BlendMode.Additive:
						templateMaterial = additiveTemplate;
						break;
					}
					if (templateMaterial == null) continue;

					foreach (var entry in entryBuffer) {
						var renderableAttachment = entry.Attachment as IHasTextureRegion;
						if (renderableAttachment != null) {
							renderableAttachment.Region = materialCache.CloneAtlasRegionWithMaterial(
								(AtlasRegion)renderableAttachment.Region, templateMaterial);
						}
					}
				}

			}
			//attachmentBuffer.Clear();
		}

		class AtlasMaterialCache : IDisposable {
			readonly Dictionary<KeyValuePair<AtlasPage, Material>, AtlasPage> cache = new Dictionary<KeyValuePair<AtlasPage, Material>, AtlasPage>();

			/// <summary>Creates a clone of an AtlasRegion that uses different Material settings, while retaining the original texture.</summary>
			public AtlasRegion CloneAtlasRegionWithMaterial (AtlasRegion originalRegion, Material materialTemplate) {
				var newRegion = originalRegion.Clone();
				newRegion.page = GetAtlasPageWithMaterial(originalRegion.page, materialTemplate);
				return newRegion;
			}

			AtlasPage GetAtlasPageWithMaterial (AtlasPage originalPage, Material materialTemplate) {
				if (originalPage == null) throw new ArgumentNullException("originalPage");

				AtlasPage newPage = null;
				var key = new KeyValuePair<AtlasPage, Material>(originalPage, materialTemplate);
				cache.TryGetValue(key, out newPage);

				if (newPage == null) {
					newPage = originalPage.Clone();
					var originalMaterial = originalPage.rendererObject as Material;
					newPage.rendererObject = new Material(materialTemplate) {
						name = originalMaterial.name + " " + materialTemplate.name,
						mainTexture = originalMaterial.mainTexture
					};
					cache.Add(key, newPage);
				}

				return newPage;
			}

			public void Dispose () {
				cache.Clear();
			}
		}

	}

}
