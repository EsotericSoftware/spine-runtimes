#ifndef SPINE_BASESKELETONJSON_H_
#define SPINE_BASESKELETONJSON_H_

#include <istream>

namespace spine {

class BaseAttachmentLoader;
class SkeletonData;
class Animation;

class BaseSkeletonJson {
public:
	BaseAttachmentLoader *attachmentLoader;
	float scale;
	bool flipY;

	BaseSkeletonJson (BaseAttachmentLoader *attachmentLoader);
	virtual ~BaseSkeletonJson ();

	SkeletonData* readSkeletonData (std::ifstream &file) const;
	SkeletonData* readSkeletonData (std::istream &file) const;
	SkeletonData* readSkeletonData (const std::string &json) const;
	SkeletonData* readSkeletonData (const char *begin, const char *end) const;

	Animation* readAnimation (std::ifstream &file, const SkeletonData *skeletonData) const;
	Animation* readAnimation (std::istream &file, const SkeletonData *skeletonData) const;
	Animation* readAnimation (const std::string &json, const SkeletonData *skeletonData) const;
	Animation* readAnimation (const char *begin, const char *end, const SkeletonData *skeletonData) const;
};

} /* namespace spine */
#endif /* SPINE_BASESKELETONJSON_H_ */
