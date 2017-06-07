#include <spine-cc/SkeletonJson.h>
#include <spine-cc/AtlasAttachmentLoader.h>

namespace spine {

SkeletonJson::SkeletonJson (BaseAttachmentLoader *attachmentLoader) :
				BaseSkeletonJson(attachmentLoader) {
	yDown = false;
}

SkeletonJson::SkeletonJson (Atlas *atlas) :
				BaseSkeletonJson(new AtlasAttachmentLoader(atlas)) {
	yDown = false;
}

} /* namespace spine */
