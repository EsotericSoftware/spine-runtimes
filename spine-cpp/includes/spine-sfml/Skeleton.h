#ifndef SPINE_SKELETON_H_
#define SPINE_SKELETON_H_

#include <spine/BaseSkeleton.h>
#include <SFML/Graphics/VertexArray.hpp>

namespace spine {

class Skeleton: public BaseSkeleton, private sf::Drawable {
public:
	sf::VertexArray vertexArray;

	Skeleton (SkeletonData *skeletonData);

	virtual void draw (sf::RenderTarget& target, sf::RenderStates states) const;
};

} /* namespace spine */
#endif /* SPINE_SKELETON_H_ */
