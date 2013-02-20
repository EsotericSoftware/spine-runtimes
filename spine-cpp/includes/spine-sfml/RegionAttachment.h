#ifndef SPINE_REGIONATTACHMENT_H_
#define SPINE_REGIONATTACHMENT_H_

#include <spine/BaseRegionAttachment.h>
#include <SFML/Graphics/Vertex.hpp>

namespace spine {

class Bone;

class RegionAttachment: public BaseRegionAttachment {
public:
	sf::Vertex vertices[4];

	RegionAttachment ();

	virtual void updateWorldVertices (Bone *bone);

	virtual void draw (const BaseSkeleton *skeleton);
};

} /* namespace spine */
#endif /* SPINE_REGIONATTACHMENT_H_ */
