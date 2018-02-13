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
using System.Collections.Generic;
using Spine;
using Spine.Unity.Modules.AttachmentTools;

namespace Spine.Unity.Modules {
	/// <summary>
	/// Example code for a component that replaces the default attachment of a slot with an image from a Spine atlas.</summary>
	public class AtlasRegionAttacher : MonoBehaviour {

		[System.Serializable]
		public class SlotRegionPair {
			[SpineSlot] public string slot;
			[SpineAtlasRegion] public string region;
		}

		[SerializeField] protected AtlasAsset atlasAsset;
		[SerializeField] protected bool inheritProperties = true;
		[SerializeField] protected List<SlotRegionPair> attachments = new List<SlotRegionPair>();

		Atlas atlas;

		void Awake () {
			var skeletonRenderer = GetComponent<SkeletonRenderer>();
			skeletonRenderer.OnRebuild += Apply;
			if (skeletonRenderer.valid) Apply(skeletonRenderer);
		}

		void Start () { } // Allow checkbox in inspector

		void Apply (SkeletonRenderer skeletonRenderer) {
			if (!this.enabled) return;

			atlas = atlasAsset.GetAtlas();
			if (atlas == null) return;
			float scale = skeletonRenderer.skeletonDataAsset.scale;

			foreach (var entry in attachments) {
				Slot slot = skeletonRenderer.Skeleton.FindSlot(entry.slot);
				Attachment originalAttachment = slot.Attachment;
				AtlasRegion region = atlas.FindRegion(entry.region);

				if (region == null) {
					slot.Attachment = null;
				} else if (inheritProperties && originalAttachment != null) {
					slot.Attachment = originalAttachment.GetRemappedClone(region, true, true, scale);
				} else {
					var newRegionAttachment = region.ToRegionAttachment(region.name, scale);
					slot.Attachment = newRegionAttachment;
				}
			}
		}
	}
}
