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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Spine.Unity.Modules.AttachmentTools;

namespace Spine.Unity.Examples {
	public class EquipSystemExample : MonoBehaviour, IHasSkeletonDataAsset {

		// Implementing IHasSkeletonDataAsset allows Spine attribute drawers to automatically detect this component as a skeleton data source.
		public SkeletonDataAsset skeletonDataAsset;
		SkeletonDataAsset IHasSkeletonDataAsset.SkeletonDataAsset { get { return this.skeletonDataAsset; } }
		
		public Material sourceMaterial;
		public bool applyPMA = true;
		public List<EquipHook> equippables = new List<EquipHook>();

		public EquipsVisualsComponentExample target;
		public Dictionary<EquipAssetExample, Attachment> cachedAttachments = new Dictionary<EquipAssetExample, Attachment>();

		[System.Serializable]
		public class EquipHook {
			public EquipType type;
			[SpineSlot]
			public string slot;
			[SpineSkin]
			public string templateSkin;
			[SpineAttachment(skinField:"templateSkin")]
			public string templateAttachment;
		}
		
		public enum EquipType {
			Gun,
			Goggles
		}

		public void Equip (EquipAssetExample asset) {
			var equipType = asset.equipType;
			EquipHook howToEquip = equippables.Find(x => x.type == equipType);

			var skeletonData = skeletonDataAsset.GetSkeletonData(true);
			int slotIndex = skeletonData.FindSlotIndex(howToEquip.slot);
			var attachment = GenerateAttachmentFromEquipAsset(asset, slotIndex, howToEquip.templateSkin, howToEquip.templateAttachment);
			target.Equip(slotIndex, howToEquip.templateAttachment, attachment);
		}

		Attachment GenerateAttachmentFromEquipAsset (EquipAssetExample asset, int slotIndex, string templateSkinName, string templateAttachmentName) {
			Attachment attachment;
			cachedAttachments.TryGetValue(asset, out attachment);

			if (attachment == null) {
				var skeletonData = skeletonDataAsset.GetSkeletonData(true);
				var templateSkin = skeletonData.FindSkin(templateSkinName);
				Attachment templateAttachment = templateSkin.GetAttachment(slotIndex, templateAttachmentName);
				attachment = templateAttachment.GetRemappedClone(asset.sprite, sourceMaterial, premultiplyAlpha: this.applyPMA);

				cachedAttachments.Add(asset, attachment); // Cache this value for next time this asset is used.
			}

			return attachment;
		}

		public void Done () {
			target.OptimizeSkin();
		}

	}

}
