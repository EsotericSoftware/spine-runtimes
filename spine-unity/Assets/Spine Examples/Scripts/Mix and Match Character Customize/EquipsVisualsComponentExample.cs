/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Spine.Unity.AttachmentTools;

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
			equipsSkin.SetAttachment(slotIndex, attachmentName, attachment);
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
			// Note: materials and textures returned by GetRepackedSkin() behave like 'new Texture2D()' and need to be destroyed
			if (runtimeMaterial)
				Destroy(runtimeMaterial);
			if (runtimeAtlas)
				Destroy(runtimeAtlas);
			var repackedSkin = collectedSkin.GetRepackedSkin("Repacked skin", skeletonAnimation.SkeletonDataAsset.atlasAssets[0].PrimaryMaterial,
				out runtimeMaterial, out runtimeAtlas, maxAtlasSize : 1024, clearCache: false);
			collectedSkin.Clear();

			// You can optionally clear the textures cache after each ore multiple repack operations are done.
			//AtlasUtilities.ClearCache();
			//Resources.UnloadUnusedAssets();

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
