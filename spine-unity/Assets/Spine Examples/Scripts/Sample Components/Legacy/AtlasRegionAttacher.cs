/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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
using Spine.Unity.AttachmentTools;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Examples {
	/// <summary>
	/// Example code for a component that replaces the default attachment of a slot with an image from a Spine atlas.</summary>
	public class AtlasRegionAttacher : MonoBehaviour {

		[System.Serializable]
		public class SlotRegionPair {
			[SpineSlot] public string slot;
			[SpineAtlasRegion] public string region;
		}

		[SerializeField] protected SpineAtlasAsset atlasAsset;
		[SerializeField] protected bool inheritProperties = true;
		[SerializeField] protected List<SlotRegionPair> attachments = new List<SlotRegionPair>();

		Atlas atlas;

		void Awake () {
			SkeletonRenderer skeletonRenderer = GetComponent<SkeletonRenderer>();
			skeletonRenderer.OnRebuild += Apply;
			if (skeletonRenderer.valid) Apply(skeletonRenderer);
		}

		void Apply (SkeletonRenderer skeletonRenderer) {
			if (!this.enabled) return;

			atlas = atlasAsset.GetAtlas();
			if (atlas == null) return;
			float scale = skeletonRenderer.skeletonDataAsset.scale;

			foreach (SlotRegionPair entry in attachments) {
				Slot slot = skeletonRenderer.Skeleton.FindSlot(entry.slot);
				Attachment originalAttachment = slot.Attachment;
				AtlasRegion region = atlas.FindRegion(entry.region);

				if (region == null) {
					slot.Attachment = null;
				} else if (inheritProperties && originalAttachment != null) {
					slot.Attachment = originalAttachment.GetRemappedClone(region, true, true, scale);
				} else {
					RegionAttachment newRegionAttachment = region.ToRegionAttachment(region.name, scale);
					slot.Attachment = newRegionAttachment;
				}
			}
		}
	}
}
