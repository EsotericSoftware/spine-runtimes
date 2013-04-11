using System;

namespace Spine {
	public class AtlasAttachmentLoader : AttachmentLoader {
		private BaseAtlas atlas;

		public AtlasAttachmentLoader (BaseAtlas atlas) {
			if (atlas == null) throw new ArgumentNullException("atlas cannot be null.");
			this.atlas = atlas;
		}

		public Attachment NewAttachment (AttachmentType type, String name) {
			switch (type) {
			case AttachmentType.region:
				AtlasRegion region = atlas.FindRegion(name);
				if (region == null) throw new Exception("Region not found in atlas: " + name + " (" + type + ")");
				RegionAttachment attachment = new RegionAttachment(name);
				attachment.Region = region;
				return attachment;
			}
			throw new Exception("Unknown attachment type: " + type);
		}
	}
}
