#include <iostream>
#include <spine-cc/Skeleton.h>
#include <spine/SkeletonData.h>
#include <spine/Slot.h>
#include <spine/Attachment.h>


namespace spine {

Skeleton::Skeleton (SkeletonData *skeletonData) :
				BaseSkeleton(skeletonData),
                texAtlas(0),
				texture(0) {
                    
                    
}

void Skeleton::draw ()  {
	const_cast<Skeleton*>(this)->vertexArray.clear();
	for (int i = 0, n = slots.size(); i < n; i++)
		if (slots[i]->attachment) slots[i]->attachment->draw(slots[i]);
    
    if (texture == 0)
        return;
    
    if (texAtlas == 0)
    {
        texAtlas = CCTextureAtlas::createWithTexture(texture, vertexArray.size());
        texAtlas->retain();
    }
    
    for (int i=0; i<vertexArray.size(); i++) {
        texAtlas->updateQuad(&vertexArray[i], i);
    }
    
    texAtlas->drawQuads();
}

} /* namespace spine */
