#ifndef SKELETONJSON_H_
#define SKELETONJSON_H_

#include <spine/BaseSkeletonJson.h>

namespace spine {

class Atlas;

class SkeletonJson: public BaseSkeletonJson {
public:
	SkeletonJson (Atlas *atlas);
	/** The SkeletonJson owns the attachmentLoader */
	SkeletonJson (BaseAttachmentLoader *attachmentLoader);
};

} /* namespace spine */
#endif /* SKELETONJSON_H_ */
