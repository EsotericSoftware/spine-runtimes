#ifndef SPINE_REGIONATTACHMENT_H_
#define SPINE_REGIONATTACHMENT_H_

#include <spine/BaseRegionAttachment.h>
#include <SFML/Graphics/Vertex.hpp>
#include <SFML/Graphics/Texture.hpp>

namespace spine {

class Bone;
class AtlasRegion;

class RegionAttachment: public BaseRegionAttachment {
public:
	sf::Vertex vertices[4];
	sf::Texture *texture;

	RegionAttachment (AtlasRegion *region);

	virtual void updateWorldVertices (Bone *bone);
	virtual void draw (Slot *slot);
};

} /* namespace spine */
#endif /* SPINE_REGIONATTACHMENT_H_ */
