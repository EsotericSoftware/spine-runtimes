using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Spine.Unity.Modules.AttachmentTools;

namespace Spine.Unity.Examples {
	public class EquipsVisualsComponentExample : MonoBehaviour {

		public SkeletonAnimation skeletonAnimation;

		[SpineSkin]
		public string templateSkinName;

		Spine.Skin equipsSkin;
		Spine.Skin collectedSkin;

		public Material runtimeMaterial;
		public Texture2D runtimeAtlas;

		void Start () {
			equipsSkin = new Skin("Equips");

			// OPTIONAL: Add all the attachments from the template skin.
			var templateSkin = skeletonAnimation.Skeleton.Data.FindSkin(templateSkinName);
			if (templateSkin != null)
				equipsSkin.AddAttachments(templateSkin);

			skeletonAnimation.Skeleton.Skin = equipsSkin;
			RefreshSkeletonAttachments();
		}

		public void Equip (int slotIndex, string attachmentName, Attachment attachment) {
			equipsSkin.AddAttachment(slotIndex, attachmentName, attachment);
			skeletonAnimation.Skeleton.SetSkin(equipsSkin);
			RefreshSkeletonAttachments();
		}

		public void OptimizeSkin () {
			// 1. Collect all the attachments of all active skins.
			collectedSkin = collectedSkin ?? new Skin("Collected skin");
			collectedSkin.Clear();
			collectedSkin.AddAttachments(skeletonAnimation.Skeleton.Data.DefaultSkin);
			collectedSkin.AddAttachments(equipsSkin);

			// 2. Create a repacked skin.
			var repackedSkin = collectedSkin.GetRepackedSkin("Repacked skin", skeletonAnimation.SkeletonDataAsset.atlasAssets[0].PrimaryMaterial, out runtimeMaterial, out runtimeAtlas);
			collectedSkin.Clear();

			// 3. Use the repacked skin.
			skeletonAnimation.Skeleton.Skin = repackedSkin;
			RefreshSkeletonAttachments();
		}

		void RefreshSkeletonAttachments () {
			skeletonAnimation.Skeleton.SetSlotsToSetupPose();
			skeletonAnimation.AnimationState.Apply(skeletonAnimation.Skeleton); //skeletonAnimation.Update(0);
		}

	}

}
