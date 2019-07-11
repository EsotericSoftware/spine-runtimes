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
			var scaleX : Number = 1;
			var scaleY : Number = 1;
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
			var scaleX : Number = 1;
			var scaleY : Number = 1;
			attachment.regionU = region.u * scaleX;
			attachment.regionV = region.v * scaleY;
			attachment.regionU2 = region.u2 * scaleX;
			attachment.regionV2 = region.v2 * scaleY;
			attachment.regionRotate = region.rotate;
			attachment.regionDegrees = region.degrees;
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
