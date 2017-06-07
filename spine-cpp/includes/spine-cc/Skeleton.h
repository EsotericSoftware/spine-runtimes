#ifndef SPINE_SKELETON_H_
#define SPINE_SKELETON_H_

#include "cocos2d.h"
#include <spine/BaseSkeleton.h>

USING_NS_CC;

namespace spine {

class Skeleton: public BaseSkeleton {
public:
    std::vector<ccV3F_C4B_T2F_Quad> vertexArray;
	CCTexture2D *texture; // This is a bit ugly and means all region attachments must use the same textures.
    CCTextureAtlas* texAtlas;
    
	Skeleton (SkeletonData *skeletonData);

	virtual void draw () ;
};

} /* namespace spine */
#endif /* SPINE_SKELETON_H_ */
