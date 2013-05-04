package spine.starling {
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
			regionAttachment.rendererObject = new SkeletonImage(texture);
			regionAttachment.regionOffsetX = texture.frame.x;
			regionAttachment.regionOffsetY = texture.frame.y;
			regionAttachment.regionWidth = texture.width;
			regionAttachment.regionHeight = texture.height;
			regionAttachment.regionOriginalWidth = texture.width;
			regionAttachment.regionOriginalHeight = texture.height;
			return regionAttachment;
		}

		throw new Error("Unknown attachment type: " + type);
	}
}

}
