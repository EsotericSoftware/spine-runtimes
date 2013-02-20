#ifndef SPINE_ATTACHMENT_H_
#define SPINE_ATTACHMENT_H_

#include <string>

namespace spine {

class BaseSkeleton;

class Attachment {
public:
	std::string name;
	float x, y, scaleX, scaleY, rotation, width, height;

	virtual ~Attachment () {
	}

	virtual void draw (const BaseSkeleton *skeleton) = 0;
};

} /* namespace spine */
#endif /* SPINE_ATTACHMENT_H_ */
