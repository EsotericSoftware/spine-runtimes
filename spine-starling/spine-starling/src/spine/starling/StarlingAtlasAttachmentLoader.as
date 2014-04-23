/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software (typically granted by licensing Spine), you
 * may not (a) modify, translate, adapt or otherwise create derivative works,
 * improvements of the Software or develop new applications using the Software
 * or (b) remove, delete, alter or obscure any trademarks or any copyright,
 * trademark, patent or other intellectual property or proprietary rights
 * notices on or in the Software, including any copy thereof. Redistributions
 * in binary or source form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine.starling {
import flash.geom.Rectangle;

import spine.Bone;
import spine.Skin;
import spine.attachments.Attachment;
import spine.attachments.AttachmentLoader;
import spine.attachments.AttachmentType;
import spine.attachments.BoundingBoxAttachment;
import spine.attachments.RegionAttachment;

import starling.textures.Texture;
import starling.textures.TextureAtlas;

public class StarlingAtlasAttachmentLoader implements AttachmentLoader {
	private var atlas:TextureAtlas;

	public function StarlingAtlasAttachmentLoader (atlas:TextureAtlas) {
		this.atlas = atlas;

		Bone.yDown = true;
	}

	public function newAttachment (skin:Skin, type:AttachmentType, name:String) : Attachment {
		switch (type) {
		case AttachmentType.region:
			var regionAttachment:RegionAttachment = new RegionAttachment(name);
			var texture:Texture = atlas.getTexture(name);
			var frame:Rectangle = texture.frame;
			texture = Texture.fromTexture(texture); // Discard frame.
			regionAttachment.rendererObject = new SkeletonImage(texture);
			regionAttachment.regionOffsetX = frame ? -frame.x : 0;
			regionAttachment.regionOffsetY = frame ? -frame.y : 0;
			regionAttachment.regionWidth = texture.width;
			regionAttachment.regionHeight = texture.height;
			regionAttachment.regionOriginalWidth = frame ? frame.width : texture.width;
			regionAttachment.regionOriginalHeight = frame ? frame.height : texture.height;
			return regionAttachment;
		case AttachmentType.boundingbox:
			return new BoundingBoxAttachment(name);
		}

		throw new Error("Unknown attachment type: " + type);
	}
}

}
