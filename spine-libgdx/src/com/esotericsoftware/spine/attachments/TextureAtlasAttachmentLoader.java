
package com.esotericsoftware.spine.attachments;

import com.esotericsoftware.spine.Attachment;
import com.esotericsoftware.spine.AttachmentLoader;
import com.esotericsoftware.spine.AttachmentType;

import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.graphics.g2d.TextureAtlas.AtlasRegion;

public class TextureAtlasAttachmentLoader implements AttachmentLoader {
	private TextureAtlas atlas;

	public TextureAtlasAttachmentLoader (TextureAtlas atlas) {
		if (atlas == null) throw new IllegalArgumentException("atlas cannot be null.");
		this.atlas = atlas;
	}

	public Attachment newAttachment (AttachmentType type, String name) {
		Attachment attachment = null;
		switch (type) {
		case region:
			attachment = new RegionAttachment(name);
			break;
		case regionSequence:
			attachment = new RegionAttachment(name);
			break;
		default:
			throw new IllegalArgumentException("Unknown attachment type: " + type);
		}

		if (attachment instanceof RegionAttachment) {
			AtlasRegion region = atlas.findRegion(attachment.getName());
			if (region == null)
				throw new RuntimeException("Region not found in atlas: " + attachment + " (" + type + " attachment: " + name + ")");
			((RegionAttachment)attachment).setRegion(region);
		}

		return attachment;
	}
}
