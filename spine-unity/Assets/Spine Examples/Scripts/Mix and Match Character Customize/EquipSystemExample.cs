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

