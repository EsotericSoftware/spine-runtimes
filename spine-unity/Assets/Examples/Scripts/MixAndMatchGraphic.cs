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

using UnityEngine;
using Spine.Unity.Modules.AttachmentTools;

namespace Spine.Unity.Examples {

	// This is an example script that shows you how to change images on your skeleton using UnityEngine.Sprites.
	public class MixAndMatchGraphic : MonoBehaviour {

		#region Inspector
		[SpineSkin]
		public string baseSkinName = "base";
		public Material sourceMaterial; // This will be used as the basis for shader and material property settings.

		[Header("Visor")]
		public Sprite visorSprite;
		[SpineSlot] public string visorSlot;
		[SpineAttachment(slotField:"visorSlot", skinField:"baseSkinName")] public string visorKey = "goggles";

		[Header("Gun")]
		public Sprite gunSprite;
		[SpineSlot] public string gunSlot;
		[SpineAttachment(slotField:"gunSlot", skinField:"baseSkinName")] public string gunKey = "gun";

		[Header("Runtime Repack Required!!")]
		public bool repack = true;

		[Header("Do not assign")]
		public Texture2D runtimeAtlas;
		public Material runtimeMaterial;
		#endregion

		Skin customSkin;

		void OnValidate () {
			if (sourceMaterial == null) {
				var skeletonGraphic = GetComponent<SkeletonGraphic>();
				if (skeletonGraphic != null)
					sourceMaterial = skeletonGraphic.SkeletonDataAsset.atlasAssets[0].materials[0];
			}
		}

		void Start () {
			Apply();
		}

		void Apply () {
			var skeletonGraphic = GetComponent<SkeletonGraphic>();
			var skeleton = skeletonGraphic.Skeleton;

			// STEP 0: PREPARE SKINS
			// Let's prepare a new skin to be our custom skin with equips/customizations. We get a clone so our original skins are unaffected.
			customSkin = customSkin ?? new Skin("custom skin"); // This requires that all customizations are done with skin placeholders defined in Spine.
			//customSkin = customSkin ?? skeleton.UnshareSkin(true, false, skeletonAnimation.AnimationState); // use this if you are not customizing on the default skin and don't plan to remove 
			// Next let's 
			var baseSkin = skeleton.Data.FindSkin(baseSkinName);

			// STEP 1: "EQUIP" ITEMS USING SPRITES
			// STEP 1.1 Find the original attachment.
			// Step 1.2 Get a clone of the original attachment.
			// Step 1.3 Apply the Sprite image to it.
			// Step 1.4 Add the remapped clone to the new custom skin.

			// Let's do this for the visor.
			int visorSlotIndex = skeleton.FindSlotIndex(visorSlot); // You can access GetAttachment and SetAttachment via string, but caching the slotIndex is faster.
			Attachment baseAttachment = baseSkin.GetAttachment(visorSlotIndex, visorKey);  // STEP 1.1
			Attachment newAttachment = baseAttachment.GetRemappedClone(visorSprite, sourceMaterial); // STEP 1.2 - 1.3
			customSkin.SetAttachment(visorSlotIndex, visorKey, newAttachment); // STEP 1.4

			// And now for the gun.
			int gunSlotIndex = skeleton.FindSlotIndex(gunSlot);
			Attachment baseGun = baseSkin.GetAttachment(gunSlotIndex, gunKey); // STEP 1.1
			Attachment newGun = baseGun.GetRemappedClone(gunSprite, sourceMaterial); // STEP 1.2 - 1.3
			if (newGun != null) customSkin.SetAttachment(gunSlotIndex, gunKey, newGun); // STEP 1.4

			// customSkin.RemoveAttachment(gunSlotIndex, gunKey); // To remove an item.
			// customSkin.Clear()
			// Use skin.Clear() To remove all customizations.
			// Customizations will fall back to the value in the default skin if it was defined there.
			// To prevent fallback from happening, make sure the key is not defined in the default skin.

			// STEP 3: APPLY AND CLEAN UP.
			// Recommended: REPACK THE CUSTOM SKIN TO MINIMIZE DRAW CALLS
			// 				Repacking requires that you set all source textures/sprites/atlases to be Read/Write enabled in the inspector.
			// 				Combine all the attachment sources into one skin. Usually this means the default skin and the custom skin.
			// 				call Skin.GetRepackedSkin to get a cloned skin with cloned attachments that all use one texture.
			//				Under the hood, this relies on 
			if (repack)	{
				var repackedSkin = new Skin("repacked skin");
				repackedSkin.Append(skeleton.Data.DefaultSkin);
				repackedSkin.Append(customSkin);
				repackedSkin = repackedSkin.GetRepackedSkin("repacked skin", sourceMaterial, out runtimeMaterial, out runtimeAtlas);
				skeleton.SetSkin(repackedSkin);
			} else {
				skeleton.SetSkin(customSkin);
			}

			skeleton.SetSlotsToSetupPose();
			skeletonGraphic.Update(0);
			skeletonGraphic.OverrideTexture = runtimeAtlas;

			Resources.UnloadUnusedAssets();
		}
	}
}
