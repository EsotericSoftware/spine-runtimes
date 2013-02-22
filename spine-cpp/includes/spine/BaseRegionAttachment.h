#ifndef SPINE_BASEREGIONATTACHMENT_H_
#define SPINE_BASEREGIONATTACHMENT_H_

#include <spine/Attachment.h>

namespace spine {

class Bone;
class Slot;

class BaseRegionAttachment: public Attachment {
public:
	float x, y, scaleX, scaleY, rotation, width, height;
	float offset[8];

	BaseRegionAttachment ();

	void updateOffset ();

	virtual void updateWorldVertices (Bone *bone) = 0;
};

} /* namespace spine */
#endif /* SPINE_BASEREGIONATTACHMENT_H_ */
