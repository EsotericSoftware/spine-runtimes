#include <spine-sfml/SkeletonJson.h>
#include <spine-sfml/AtlasAttachmentLoader.h>

namespace spine {

SkeletonJson::SkeletonJson (BaseAttachmentLoader *attachmentLoader) :
				BaseSkeletonJson(attachmentLoader) {
	yDown = true;
}

SkeletonJson::SkeletonJson (Atlas *atlas) :
				BaseSkeletonJson(new AtlasAttachmentLoader(atlas)) {
	yDown = true;
}

} /* namespace spine */
