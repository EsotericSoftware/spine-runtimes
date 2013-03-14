
package com.esotericsoftware.spine;

public interface AttachmentLoader {
	/** @return May be null to not load any attachment. */
	public Attachment newAttachment (AttachmentType type, String name);
}
