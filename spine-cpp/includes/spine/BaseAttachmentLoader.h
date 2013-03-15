#ifndef SPINE_BASEATTACHMENTLOADER_H_
#define SPINE_BASEATTACHMENTLOADER_H_

namespace spine {

class Attachment;

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
