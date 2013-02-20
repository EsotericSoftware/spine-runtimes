#include <spine-sfml/SkeletonJson.h>
#include "AttachmentLoader.h"

namespace spine {

SkeletonJson::SkeletonJson () : BaseSkeletonJson(new AttachmentLoader()) {
}

} /* namespace spine */
