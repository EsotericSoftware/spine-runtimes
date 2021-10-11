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

using Spine.Unity.AttachmentTools;
using System.Collections;
using UnityEngine;

namespace Spine.Unity.Examples {

	// This is an example script that shows you how to change images on your skeleton using UnityEngine.Sprites.
	public class MixAndMatch : MonoBehaviour {

		#region Inspector
		[SpineSkin]
		public string templateAttachmentsSkin = "base";
		public Material sourceMaterial; // This will be used as the basis for shader and material property settings.

		[Header("Visor")]
		public Sprite visorSprite;
		[SpineSlot] public string visorSlot;
		[SpineAttachment(slotField: "visorSlot", skinField: "baseSkinName")] public string visorKey = "goggles";

		[Header("Gun")]
		public Sprite gunSprite;
		[SpineSlot] public string gunSlot;
		[SpineAttachment(slotField: "gunSlot", skinField: "baseSkinName")] public string gunKey = "gun";

		[Header("Runtime Repack")]
		public bool repack = true;
		public BoundingBoxFollower bbFollower;

		[Header("Do not assign")]
		public Texture2D runtimeAtlas;
		public Material runtimeMaterial;
		#endregion

		Skin customSkin;

		void OnValidate () {
			if (sourceMaterial == null) {
				var skeletonAnimation = GetComponent<SkeletonAnimation>();
				if (skeletonAnimation != null)
					sourceMaterial = skeletonAnimation.SkeletonDataAsset.atlasAssets[0].PrimaryMaterial;
			}
		}

		IEnumerator Start () {
			yield return new WaitForSeconds(1f); // Delay for one second before applying. For testing.
			Apply();
		}

		void Apply () {
			var skeletonAnimation = GetComponent<SkeletonAnimation>();
			var skeleton = skeletonAnimation.Skeleton;

			// STEP 0: PREPARE SKINS
			// Let's prepare a new skin to be our custom skin with equips/customizations. We get a clone so our original skins are unaffected.
			customSkin = customSkin ?? new Skin("custom skin"); // This requires that all customizations are done with skin placeholders defined in Spine.
			var templateSkin = skeleton.Data.FindSkin(templateAttachmentsSkin);

			// STEP 1: "EQUIP" ITEMS USING SPRITES
			// STEP 1.1 Find the original/template attachment.
			// Step 1.2 Get a clone of the original/template attachment.
			// Step 1.3 Apply the Sprite image to the clone.
			// Step 1.4 Add the remapped clone to the new custom skin.

			// Let's do this for the visor.
			int visorSlotIndex = skeleton.Data.FindSlot(visorSlot).Index; // You can access GetAttachment and SetAttachment via string, but caching the slotIndex is faster.
			Attachment templateAttachment = templateSkin.GetAttachment(visorSlotIndex, visorKey); // STEP 1.1

			// Note: Each call to `GetRemappedClone()` with parameter `premultiplyAlpha` set to `true` creates
			// a cached Texture copy which can be cleared by calling AtlasUtilities.ClearCache() as done in the method below.
			Attachment newAttachment = templateAttachment.GetRemappedClone(visorSprite, sourceMaterial, pivotShiftsMeshUVCoords: false); // STEP 1.2 - 1.3
			customSkin.SetAttachment(visorSlotIndex, visorKey, newAttachment); // STEP 1.4

			// And now for the gun.
			int gunSlotIndex = skeleton.Data.FindSlot(gunSlot).Index;
			Attachment templateGun = templateSkin.GetAttachment(gunSlotIndex, gunKey); // STEP 1.1
			Attachment newGun = templateGun.GetRemappedClone(gunSprite, sourceMaterial, pivotShiftsMeshUVCoords: false); // STEP 1.2 - 1.3
			if (newGun != null) customSkin.SetAttachment(gunSlotIndex, gunKey, newGun); // STEP 1.4

			// customSkin.RemoveAttachment(gunSlotIndex, gunKey); // To remove an item.
			// customSkin.Clear()
			// Use skin.Clear() To remove all customizations.
			// Customizations will fall back to the value in the default skin if it was defined there.
			// To prevent fallback from happening, make sure the key is not defined in the default skin.

			// STEP 3: APPLY AND CLEAN UP.
			// Recommended, preferably at level-load-time: REPACK THE CUSTOM SKIN TO MINIMIZE DRAW CALLS
			// 				IMPORTANT NOTE: the GetRepackedSkin() operation is expensive - if multiple characters
			// 				need to call it every few seconds the overhead will outweigh the draw call benefits.
			//
			// 				Repacking requires that you set all source textures/sprites/atlases to be Read/Write enabled in the inspector.
			// 				Combine all the attachment sources into one skin. Usually this means the default skin and the custom skin.
			// 				call Skin.GetRepackedSkin to get a cloned skin with cloned attachments that all use one texture.
			if (repack) {
				var repackedSkin = new Skin("repacked skin");
				repackedSkin.AddSkin(skeleton.Data.DefaultSkin); // Include the "default" skin. (everything outside of skin placeholders)
				repackedSkin.AddSkin(customSkin); // Include your new custom skin.

				// Note: materials and textures returned by GetRepackedSkin() behave like 'new Texture2D()' and need to be destroyed
				if (runtimeMaterial)
					Destroy(runtimeMaterial);
				if (runtimeAtlas)
					Destroy(runtimeAtlas);
				repackedSkin = repackedSkin.GetRepackedSkin("repacked skin", sourceMaterial, out runtimeMaterial, out runtimeAtlas); // Pack all the items in the skin.
				skeleton.SetSkin(repackedSkin); // Assign the repacked skin to your Skeleton.
				if (bbFollower != null) bbFollower.Initialize(true);
			} else {
				skeleton.SetSkin(customSkin); // Just use the custom skin directly.
			}

			skeleton.SetSlotsToSetupPose(); // Use the pose from setup pose.
			skeletonAnimation.Update(0); // Use the pose in the currently active animation.

			// `GetRepackedSkin()` and each call to `GetRemappedClone()` with parameter `premultiplyAlpha` set to `true`
			// cache necessarily created Texture copies which can be cleared by calling AtlasUtilities.ClearCache().
			// You can optionally clear the textures cache after multiple repack operations.
			// Just be aware that while this cleanup frees up memory, it is also a costly operation
			// and will likely cause a spike in the framerate.
			AtlasUtilities.ClearCache();
			Resources.UnloadUnusedAssets();
		}
	}
}
