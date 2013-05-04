package spine.attachments {
import spine.Skin;
import spine.atlas.Atlas;
import spine.atlas.AtlasRegion;

public class AtlasAttachmentLoader implements AttachmentLoader {
	private var atlas:Atlas;

	public function AtlasAttachmentLoader (atlas:Atlas) {
		if (atlas == null)
			throw new ArgumentError("atlas cannot be null.");
		this.atlas = atlas;
	}

	public function newAttachment (skin:Skin, type:AttachmentType, name:String) : Attachment {
		switch (type) {
		case AttachmentType.region:
			var region:AtlasRegion  = atlas.findRegion(name);
			if (region == null)
				throw new Error("Region not found in atlas: " + name + " (" + type + ")");
			var attachment:RegionAttachment = new RegionAttachment(name);
			attachment.rendererObject = region;
			attachment.setUVs(region.u, region.v, region.u2, region.v2, region.rotate);
			attachment.regionOffsetX = region.offsetX;
			attachment.regionOffsetY = region.offsetY;
			attachment.regionWidth = region.width;
			attachment.regionHeight = region.height;
			attachment.regionOriginalWidth = region.originalWidth;
			attachment.regionOriginalHeight = region.originalHeight;
			return attachment;
		}
		throw new Error("Unknown attachment type: " + type);
	}
}

}
