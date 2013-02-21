#ifndef SPINE_BASESKELETONJSON_H_
#define SPINE_BASESKELETONJSON_H_

#include <istream>

namespace spine {

class SkeletonData;
class BaseAttachmentLoader;

class BaseSkeletonJson {
public:
	BaseAttachmentLoader *attachmentLoader;
	float scale;

	BaseSkeletonJson (BaseAttachmentLoader *attachmentLoader);
	virtual ~BaseSkeletonJson ();

	SkeletonData* readSkeletonData (const char *begin, const char *end) const;
	SkeletonData* readSkeletonData (const std::string &json) const;
	SkeletonData* readSkeletonData (std::istream &file) const;
};

} /* namespace spine */
#endif /* SPINE_BASESKELETONJSON_H_ */
