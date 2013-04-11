using System;

namespace Spine {
	public interface AttachmentLoader {
		/** @return May be null to not load any attachment. */
		Attachment NewAttachment (AttachmentType type, String name);
	}
}
