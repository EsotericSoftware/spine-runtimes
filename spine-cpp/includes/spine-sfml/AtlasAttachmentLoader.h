#ifndef SPINE_ATLASATTACHMENTLOADER_H_
#define SPINE_ATLASATTACHMENTLOADER_H_

#include <spine/BaseAttachmentLoader.h>

namespace spine {

class Atlas;

class AtlasAttachmentLoader: public BaseAttachmentLoader {
public:
	Atlas *atlas;

	AtlasAttachmentLoader (Atlas *atlas);

	virtual Attachment* newAttachment (AttachmentType type, const std::string &name);
}
;

} /* namespace spine */
#endif /* SPINE_ATLASATTACHMENTLOADER_H_ */
