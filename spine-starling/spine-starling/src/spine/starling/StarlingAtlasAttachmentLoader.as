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

package spine.starling {
	import spine.Bone;
	import spine.Skin;
	import spine.attachments.AttachmentLoader;
	import spine.attachments.BoundingBoxAttachment;
	import spine.attachments.ClippingAttachment;
	import spine.attachments.MeshAttachment;
	import spine.attachments.PathAttachment;
	import spine.attachments.PointAttachment;
	import spine.attachments.RegionAttachment;

	import starling.display.Image;
	import starling.textures.SubTexture;
	import starling.textures.Texture;
	import starling.textures.TextureAtlas;

	import flash.geom.Rectangle;

	public class StarlingAtlasAttachmentLoader implements AttachmentLoader {
		private var atlas : TextureAtlas;

		public function StarlingAtlasAttachmentLoader(atlas : TextureAtlas) {
			this.atlas = atlas;
			Bone.yDown = true;
		}

		protected function getTexture(path : String) : Texture {
			return atlas.getTexture(path);
		}

		public function newRegionAttachment(skin : Skin, name : String, path : String) : RegionAttachment {
			var texture : SubTexture = getTexture(path) as SubTexture;
			if (texture == null)
				throw new Error("Region not found in Starling atlas: " + path + " (region attachment: " + name + ")");
			var attachment : RegionAttachment = new RegionAttachment(name);
			var rotated : Boolean = texture.rotated;
			attachment.rendererObject = new Image(Texture.fromTexture(texture)); // Discard frame.
			var frame : Rectangle = texture.frame;
			attachment.regionOffsetX = frame ? -frame.x : 0;
			attachment.regionOffsetY = frame ? -frame.y : 0;
			attachment.regionWidth = texture.width;
			attachment.regionHeight = texture.height;
			attachment.regionOriginalWidth = frame ? frame.width : texture.width;
			attachment.regionOriginalHeight = frame ? frame.height : texture.height;
			if (rotated) {
				var tmp : Number = attachment.regionOriginalWidth;
				attachment.regionOriginalWidth = attachment.regionOriginalHeight;
				attachment.regionOriginalHeight = tmp;
				tmp = attachment.regionWidth;
				attachment.regionWidth = attachment.regionHeight;
				attachment.regionHeight = tmp;
				attachment["regionU2"] = 0;
				attachment["regionV2"] = 1;
				attachment["regionU"] = 1;
				attachment["regionV"] = 0;
			} else {
				attachment["regionU"] = 0;
				attachment["regionV"] = 0;
				attachment["regionU2"] = 1;
				attachment["regionV2"] = 1;
			}
			attachment.setUVs(attachment["regionU"], attachment["regionV"], attachment["regionU2"], attachment["regionV2"], rotated);
			return attachment;
		}

		public function newMeshAttachment(skin : Skin, name : String, path : String) : MeshAttachment {
			var texture : SubTexture = getTexture(path) as SubTexture;
			if (texture == null)
				throw new Error("Region not found in Starling atlas: " + path + " (mesh attachment: " + name + ")");
			var rotated : Boolean = texture.rotated;
			var attachment : MeshAttachment = new MeshAttachment(name);
			attachment.regionRotate = rotated;
			attachment.rendererObject = new Image(Texture.fromTexture(texture)); // Discard frame.

			var root : Texture = texture.root;
			var rectRegion : Rectangle = texture.region;
			if (!rotated) {
				attachment.regionU = rectRegion.x / root.width;
				attachment.regionV = rectRegion.y / root.height;
				attachment.regionU2 = (rectRegion.x + texture.width) / root.width;
				attachment.regionV2 = (rectRegion.y + texture.height) / root.height;
			} else {
				attachment.regionU2 = rectRegion.x / root.width;
				attachment.regionV2 = rectRegion.y / root.height;
				attachment.regionU = (rectRegion.x + texture.height) / root.width;
				attachment.regionV = (rectRegion.y + texture.width) / root.height;
			}
			attachment.rendererObject = new Image(root);

			var frame : Rectangle = texture.frame;
			attachment.regionOffsetX = frame ? -frame.x : 0;
			attachment.regionOffsetY = frame ? -frame.y : 0;
			attachment.regionWidth = texture.width;
			attachment.regionHeight = texture.height;
			attachment.regionOriginalWidth = frame ? frame.width : texture.width;
			attachment.regionOriginalHeight = frame ? frame.height : texture.height;
			if (rotated) {
				var tmp : Number = attachment.regionOriginalWidth;
				attachment.regionOriginalWidth = attachment.regionOriginalHeight;
				attachment.regionOriginalHeight = tmp;
				tmp = attachment.regionWidth;
				attachment.regionWidth = attachment.regionHeight;
				attachment.regionHeight = tmp;
			}
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
	}
}
