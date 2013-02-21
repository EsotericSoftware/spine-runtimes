#ifndef SPINE_BASEREGIONATTACHMENT_H_
#define SPINE_BASEREGIONATTACHMENT_H_

#include <spine/Attachment.h>

namespace spine {

class Bone;

class BaseRegionAttachment: public Attachment {
public:
	float offset[8];

	void updateOffset ();

	virtual void updateWorldVertices (Bone *bone) = 0;
};

} /* namespace spine */
#endif /* SPINE_BASEREGIONATTACHMENT_H_ */
