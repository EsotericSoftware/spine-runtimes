package spine.starling {
import flash.geom.Rectangle;

import spine.Bone;
import spine.Skin;
import spine.attachments.Attachment;
import spine.attachments.AttachmentLoader;
import spine.attachments.AttachmentType;
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
		if (type == AttachmentType.region) {
			var regionAttachment:RegionAttachment = new RegionAttachment(name);
			var texture:Texture = atlas.getTexture(name);
			var frame:Rectangle = texture.frame;
			texture = Texture.fromTexture(texture); // Discard frame.
			regionAttachment.rendererObject = new SkeletonImage(texture);
			regionAttachment.regionOffsetX = frame.x;
			regionAttachment.regionOffsetY = frame.y;
			regionAttachment.regionWidth = frame.width;
			regionAttachment.regionHeight = frame.height;
			regionAttachment.regionOriginalWidth = texture.width;
			regionAttachment.regionOriginalHeight = texture.height;
			return regionAttachment;
		}

		throw new Error("Unknown attachment type: " + type);
	}
}

}
