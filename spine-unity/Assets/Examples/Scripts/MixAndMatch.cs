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
	public class MixAndMatch : MonoBehaviour {

		#region Inspector
		[Header("Visor")]
		public Sprite visorSprite;
		[SpineSlot] public string visorSlot;
		[SpineAttachment(slotField:"visorSlot")] public string visorKey = "goggles";

		[Header("Gun")]
		public Sprite gunSprite;
		[SpineSlot] public string gunSlot;
		[SpineAttachment(slotField:"gunSlot")] public string gunKey = "gun";

		[Header("Runtime Repack")]
		public bool repack = true;
		public Material sourceMaterial;

		[Header("Do not assign")]
		public Texture2D runtimeAtlas;
		public Material runtimeMaterial;
		#endregion

		void OnValidate () {
			if (sourceMaterial == null) {
				var skeletonAnimation = GetComponent<SkeletonAnimation>();
				if (skeletonAnimation != null)
					sourceMaterial = skeletonAnimation.SkeletonDataAsset.atlasAssets[0].materials[0];
			}
		}

		void Start () {
			var skeletonAnimation = GetComponent<SkeletonAnimation>();
			var skeleton = skeletonAnimation.Skeleton;

			// All attachment changes will be applied to the skin. We use a clone so other instances will not be affected.
			var newSkin = skeleton.UnshareSkin(true, false, skeletonAnimation.AnimationState);

			// NOTE: sprite textures need to be full rect.
			var originalSkin = skeleton.Data.DefaultSkin;

			int visorSlotIndex = skeleton.FindSlotIndex(visorSlot);
			RegionAttachment originalVisor = originalSkin.GetAttachment(visorSlotIndex, visorKey) as RegionAttachment;
			RegionAttachment newVisor = visorSprite.ToRegionAttachmentPMAClone(sourceMaterial);
			newVisor.X = originalVisor.X;
			newVisor.Y = originalVisor.Y;
			newVisor.Rotation = originalVisor.Rotation;
			newSkin.AddAttachment(visorSlotIndex, visorKey, newVisor);


			int gunSlotIndex = skeleton.FindSlotIndex(gunSlot);
			RegionAttachment originalGun = originalSkin.GetAttachment(gunSlotIndex, gunKey) as RegionAttachment;
			RegionAttachment newGun = gunSprite.ToRegionAttachmentPMAClone(sourceMaterial);
			newGun.x = originalGun.x;
			newGun.y = originalGun.y;
			newGun.Rotation = originalGun.Rotation;
			newSkin.AddAttachment(gunSlotIndex, gunKey, newGun);

			// Case 3: Change an existing attachment's backing region.
//			if (applyHeadRegion) {
//				AtlasRegion spineBoyHead = headSource.GetAtlas().FindRegion(headRegion);
//				int headSlotIndex = skeleton.FindSlotIndex(headSlot);
//				var newHead = newSkin.GetAttachment(headSlotIndex, headAttachmentName).GetClone(true);
//				newHead.SetRegion(spineBoyHead);
//				newSkin.AddAttachment(headSlotIndex, headAttachmentName, newHead);
//			}

			// Case 4: Repacking a mixed-and-matched skin to minimize draw calls.
			// Repacking requires that you set all source textures/sprites/atlases to be Read/Write enabled in the inspector.
			if (repack) {
				newSkin = newSkin.GetRepackedSkin("repacked", sourceMaterial, out runtimeMaterial, out runtimeAtlas);
			}
			
			skeleton.SetSkin(newSkin);
			skeleton.SetSlotsToSetupPose();
			skeletonAnimation.Update(0);

			Resources.UnloadUnusedAssets();
		}

	}
}
