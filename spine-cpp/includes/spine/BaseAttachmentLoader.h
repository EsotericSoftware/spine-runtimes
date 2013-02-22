#ifndef SPINE_BASEATTACHMENTLOADER_H_
#define SPINE_BASEATTACHMENTLOADER_H_

#include <spine/Attachment.h>

namespace spine {

enum AttachmentType {
	region, regionSequence
};

class BaseAttachmentLoader {
public:
	virtual ~BaseAttachmentLoader () {
	}

	virtual Attachment* newAttachment (AttachmentType type, const std::string &name) = 0;
};

} /* namespace spine */
#endif /* SPINE_BASEATTACHMENTLOADER_H_ */
