#include <spine-sfml/Skeleton.h>
#include <spine/SkeletonData.h>
#include <spine/Slot.h>
#include <spine/Attachment.h>
#include <SFML/Graphics/RenderTarget.hpp>
#include <SFML/Graphics/RenderStates.hpp>

using namespace sf;

namespace spine {

Skeleton::Skeleton (SkeletonData *skeletonData) :
				BaseSkeleton(skeletonData),
				vertexArray(Quads, skeletonData->bones.size() * 4) {
}

void Skeleton::draw (RenderTarget& target, RenderStates states) const {
	for (int i = 0, n = slots.size(); i < n; i++)
		if (slots[i]->attachment) slots[i]->attachment->draw(this);
	target.draw(vertexArray, states);
}

} /* namespace spine */
