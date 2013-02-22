#include <spine-sfml/SkeletonJson.h>
#include <spine-sfml/AtlasAttachmentLoader.h>

namespace spine {

SkeletonJson::SkeletonJson (BaseAttachmentLoader *attachmentLoader) :
				BaseSkeletonJson(attachmentLoader) {
}

SkeletonJson::SkeletonJson (Atlas *atlas) :
				BaseSkeletonJson(new AtlasAttachmentLoader(atlas)) {
}

} /* namespace spine */
