#ifndef ATTACHMENTLOADER_H_
#define ATTACHMENTLOADER_H_

#include <stdexcept>
#include <spine/BaseAttachmentLoader.h>
#include <spine-sfml/RegionAttachment.h>

namespace spine {

class AttachmentLoader: public BaseAttachmentLoader {
public:
	virtual Attachment* newAttachment (AttachmentType type) {
		switch (type) {
		case region:
			return new RegionAttachment();
		default:
			throw std::runtime_error("Unknown attachment type: " + type);
		}
	}
};

} /* namespace spine */
#endif /* ATTACHMENTLOADER_H_ */
