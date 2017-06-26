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

package spine.starling {
	import spine.attachments.ClippingAttachment;
	import spine.attachments.PointAttachment;
	import spine.attachments.PathAttachment;

	import starling.display.Image;

	import spine.Bone;
	import spine.Skin;
	import spine.attachments.AttachmentLoader;
	import spine.attachments.BoundingBoxAttachment;
	import spine.attachments.MeshAttachment;
	import spine.attachments.RegionAttachment;

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

        protected function getTexture(path:String):Texture {
            return atlas.getTexture(path);
        }

        public function newRegionAttachment(skin : Skin, name : String, path : String) : RegionAttachment {
            var subTexture:SubTexture = getTexture(path) as SubTexture;
            if (subTexture == null)
                throw new Error("Region not found in Starling atlas: " + path + " (region attachment: " + name + ")");
            var attachment:RegionAttachment = new RegionAttachment(name);
            var rotated:Boolean = subTexture.rotated;
            attachment.rendererObject = new Image(Texture.fromTexture(subTexture)); // Discard frame.
            var frame:Rectangle = subTexture.frame;
            attachment.regionOffsetX = frame ? -frame.x : 0;
            attachment.regionOffsetY = frame ? -frame.y : 0;
            attachment.regionWidth = subTexture.width;
            attachment.regionHeight = subTexture.height;
            attachment.regionOriginalWidth = frame ? frame.width : subTexture.width;
            attachment.regionOriginalHeight = frame ? frame.height : subTexture.height;
            var root:Texture = subTexture.root;
            var rectRegion:Rectangle = subTexture.region;
            if (rotated) {
                var tmp:Number = attachment.regionOriginalWidth;
                attachment.regionOriginalWidth = attachment.regionOriginalHeight;
                attachment.regionOriginalHeight = tmp;
                tmp = attachment.regionWidth;
                attachment.regionWidth = attachment.regionHeight;
                attachment.regionHeight = tmp;
                attachment["regionU2"] = rectRegion.x / root.width;
                attachment["regionV2"] = rectRegion.y / root.height;
                attachment["regionU"] = (rectRegion.x + subTexture.width) / root.width;
                attachment["regionV"] = (rectRegion.y + subTexture.height) / root.height;
            } else {
                attachment["regionU"] = rectRegion.x / root.width;
                attachment["regionV"] = rectRegion.y / root.height;
                attachment["regionU2"] = (rectRegion.x + subTexture.width) / root.width;
                attachment["regionV2"] = (rectRegion.y + subTexture.height) / root.height;
            }
            attachment.setUVs(attachment["regionU"], attachment["regionV"], attachment["regionU2"], attachment["regionV2"], subTexture.rotated);
            return attachment;
        }

        public function newMeshAttachment(skin:Skin, name:String, path:String):MeshAttachment {
            var subTexture:SubTexture = getTexture(path) as SubTexture;
            if (subTexture== null)
                throw new Error("Region not found in Starling atlas: " + path + " (mesh attachment: " + name + ")");
            var rotated:Boolean = subTexture.rotated;
            var attachment:MeshAttachment = new MeshAttachment(name);
            attachment.regionRotate = rotated;
            attachment.rendererObject = new Image(Texture.fromTexture(subTexture)); // Discard frame.
            var root:Texture = subTexture.root;
            var rectRegion:Rectangle = subTexture.region;
            if (!rotated) {
                attachment.regionU = rectRegion.x / root.width;
                attachment.regionV = rectRegion.y / root.height;
                attachment.regionU2 = (rectRegion.x + subTexture.width) / root.width;
                attachment.regionV2 = (rectRegion.y + subTexture.height) / root.height;
            } else {
                attachment.regionU2 = rectRegion.x / root.width;
                attachment.regionV2 = rectRegion.y / root.height;
                attachment.regionU = (rectRegion.x + subTexture.height) / root.width;
                attachment.regionV = (rectRegion.y + subTexture.width) / root.height;
            }
            attachment.rendererObject = new Image(root);

            var frame:Rectangle = subTexture.frame;
            attachment.regionOffsetX = frame ? -frame.x : 0;
            attachment.regionOffsetY = frame ? -frame.y : 0;
            attachment.regionWidth = subTexture.width;
            attachment.regionHeight = subTexture.height;
            attachment.regionOriginalWidth = frame ? frame.width : subTexture.width;
            attachment.regionOriginalHeight = frame ? frame.height : subTexture.height;
            if (rotated) {
                var tmp:Number = attachment.regionOriginalWidth;
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