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

package spine.attachments {
	import spine.Skin;
	import spine.atlas.Atlas;
	import spine.atlas.AtlasRegion;

	public class AtlasAttachmentLoader implements AttachmentLoader {
		private var atlas : Atlas;

		public function AtlasAttachmentLoader(atlas : Atlas) {
			if (atlas == null)
				throw new ArgumentError("atlas cannot be null.");
			this.atlas = atlas;
		}

		public function newRegionAttachment(skin : Skin, name : String, path : String) : RegionAttachment {
			var region : AtlasRegion = atlas.findRegion(path);
			if (region == null)
				throw new Error("Region not found in atlas: " + path + " (region attachment: " + name + ")");
			var attachment : RegionAttachment = new RegionAttachment(name);
			attachment.rendererObject = region;
			var scaleX : Number = region.page.width / nextPOT(region.page.width);
			var scaleY : Number = region.page.height / nextPOT(region.page.height);
			attachment.setUVs(region.u * scaleX, region.v * scaleY, region.u2 * scaleX, region.v2 * scaleY, region.rotate);
			attachment.regionOffsetX = region.offsetX;
			attachment.regionOffsetY = region.offsetY;
			attachment.regionWidth = region.width;
			attachment.regionHeight = region.height;
			attachment.regionOriginalWidth = region.originalWidth;
			attachment.regionOriginalHeight = region.originalHeight;
			return attachment;
		}

		public function newMeshAttachment(skin : Skin, name : String, path : String) : MeshAttachment {
			var region : AtlasRegion = atlas.findRegion(path);
			if (region == null)
				throw new Error("Region not found in atlas: " + path + " (mesh attachment: " + name + ")");
			var attachment : MeshAttachment = new MeshAttachment(name);
			attachment.rendererObject = region;
			var scaleX : Number = region.page.width / nextPOT(region.page.width);
			var scaleY : Number = region.page.height / nextPOT(region.page.height);
			attachment.regionU = region.u * scaleX;
			attachment.regionV = region.v * scaleY;
			attachment.regionU2 = region.u2 * scaleX;
			attachment.regionV2 = region.v2 * scaleY;
			attachment.regionRotate = region.rotate;
			attachment.regionOffsetX = region.offsetX;
			attachment.regionOffsetY = region.offsetY;
			attachment.regionWidth = region.width;
			attachment.regionHeight = region.height;
			attachment.regionOriginalWidth = region.originalWidth;
			attachment.regionOriginalHeight = region.originalHeight;
			return attachment;
		}

		public function newBoundingBoxAttachment(skin : Skin, name : String) : BoundingBoxAttachment {
			return new BoundingBoxAttachment(name);
		}

		public function newPathAttachment(skin : Skin, name : String) : PathAttachment {
			return new PathAttachment(name);
		}

		public function newPointAttachment(skin : Skin, name : String) : PointAttachment {
			return new PointAttachment(name);
		}
		
		public function newClippingAttachment(skin : Skin, name : String) : ClippingAttachment {
			return new ClippingAttachment(name);
		}

		static public function nextPOT(value : int) : int {
			value--;
			value |= value >> 1;
			value |= value >> 2;
			value |= value >> 4;
			value |= value >> 8;
			value |= value >> 16;
			return value + 1;
		}
	}
}