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
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Spine.Unity {
	[System.Serializable]
	public class BlendModeMaterials {

		[System.Serializable]
		public class ReplacementMaterial {
			public string pageName;
			public Material material;
		}

		[SerializeField, HideInInspector] protected bool requiresBlendModeMaterials = false;
		public bool applyAdditiveMaterial = false;

		public List<ReplacementMaterial> additiveMaterials = new List<ReplacementMaterial>();
		public List<ReplacementMaterial> multiplyMaterials = new List<ReplacementMaterial>();
		public List<ReplacementMaterial> screenMaterials = new List<ReplacementMaterial>();

		public bool RequiresBlendModeMaterials { get { return requiresBlendModeMaterials; } set { requiresBlendModeMaterials = value; } }

		public BlendMode BlendModeForMaterial (Material material) {
			foreach (var pair in multiplyMaterials)
				if (pair.material == material) return BlendMode.Multiply;
			foreach (var pair in additiveMaterials)
				if (pair.material == material) return BlendMode.Additive;
			foreach (var pair in screenMaterials)
				if (pair.material == material) return BlendMode.Screen;
			return BlendMode.Normal;
		}

#if UNITY_EDITOR
		public void TransferSettingsFrom (BlendModeMaterialsAsset modifierAsset) {
			applyAdditiveMaterial = modifierAsset.applyAdditiveMaterial;
		}

		public bool UpdateBlendmodeMaterialsRequiredState (SkeletonData skeletonData) {
			requiresBlendModeMaterials = false;

			if (skeletonData == null) return false;

			var skinEntries = new List<Skin.SkinEntry>();
			var slotsItems = skeletonData.Slots.Items;
			for (int slotIndex = 0, slotCount = skeletonData.Slots.Count; slotIndex < slotCount; slotIndex++) {
				var slot = slotsItems[slotIndex];
				if (slot.BlendMode == BlendMode.Normal) continue;
				if (!applyAdditiveMaterial && slot.BlendMode == BlendMode.Additive) continue;

				skinEntries.Clear();
				foreach (var skin in skeletonData.Skins)
					skin.GetAttachments(slotIndex, skinEntries);

				foreach (var entry in skinEntries) {
					if (entry.Attachment is IHasTextureRegion) {
						requiresBlendModeMaterials = true;
						return true;
					}
				}
			}
			return false;
		}
#endif
		public void ApplyMaterials (SkeletonData skeletonData) {
			if (skeletonData == null) throw new ArgumentNullException("skeletonData");
			if (!requiresBlendModeMaterials)
				return;

			var skinEntries = new List<Skin.SkinEntry>();
			var slotsItems = skeletonData.Slots.Items;
			for (int slotIndex = 0, slotCount = skeletonData.Slots.Count; slotIndex < slotCount; slotIndex++) {
				var slot = slotsItems[slotIndex];
				if (slot.BlendMode == BlendMode.Normal) continue;
				if (!applyAdditiveMaterial && slot.BlendMode == BlendMode.Additive) continue;

				List<ReplacementMaterial> replacementMaterials = null;
				switch (slot.BlendMode) {
				case BlendMode.Multiply:
					replacementMaterials = multiplyMaterials;
					break;
				case BlendMode.Screen:
					replacementMaterials = screenMaterials;
					break;
				case BlendMode.Additive:
					replacementMaterials = additiveMaterials;
					break;
				}
				if (replacementMaterials == null)
					continue;

				skinEntries.Clear();
				foreach (var skin in skeletonData.Skins)
					skin.GetAttachments(slotIndex, skinEntries);

				foreach (var entry in skinEntries) {
					var renderableAttachment = entry.Attachment as IHasTextureRegion;
					if (renderableAttachment != null) {
						if (renderableAttachment.Region != null) {
							renderableAttachment.Region = CloneAtlasRegionWithMaterial(
							(AtlasRegion)renderableAttachment.Region, replacementMaterials);
						} else {
							if (renderableAttachment.Sequence != null) {
								var regions = renderableAttachment.Sequence.Regions;
								for (int i = 0; i < regions.Length; ++i) {
									regions[i] = CloneAtlasRegionWithMaterial(
										(AtlasRegion)regions[i], replacementMaterials);
								}
							}
						}
					}
				}
			}
		}

		protected AtlasRegion CloneAtlasRegionWithMaterial (AtlasRegion originalRegion, List<ReplacementMaterial> replacementMaterials) {
			var newRegion = originalRegion.Clone();
			Material material = null;
			foreach (var replacement in replacementMaterials) {
				if (replacement.pageName == originalRegion.page.name) {
					material = replacement.material;
					break;
				}
			}

			AtlasPage originalPage = originalRegion.page;
			var newPage = originalPage.Clone();
			newPage.rendererObject = material;
			newRegion.page = newPage;
			return newRegion;
		}
	}
}
