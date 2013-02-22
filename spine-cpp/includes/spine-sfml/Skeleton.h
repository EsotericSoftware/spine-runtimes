#ifndef SPINE_SKELETON_H_
#define SPINE_SKELETON_H_

#include <spine/BaseSkeleton.h>
#include <SFML/Graphics/VertexArray.hpp>

namespace spine {

class Skeleton: public BaseSkeleton, public sf::Drawable {
public:
	sf::VertexArray vertexArray;
	sf::Texture *texture; // BOZO - This is ugly. Support multiple textures?

	Skeleton (SkeletonData *skeletonData);

	virtual void draw (sf::RenderTarget& target, sf::RenderStates states) const;
};

} /* namespace spine */
#endif /* SPINE_SKELETON_H_ */
