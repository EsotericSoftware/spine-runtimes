#ifndef _EXAMPLELAYER_H_
#define _EXAMPLELAYER_H_

#include "cocos2d.h"
#include <spine-cocos2dx/spine.h>

class ExampleLayer: public cocos2d::CCLayer {
private:
	spine::Atlas *atlas;
	spine::SkeletonData *skeletonData;
	spine::Animation *animation;

public:
	static cocos2d::CCScene* scene ();
	~ExampleLayer ();

	virtual bool init ();

	CREATE_FUNC (ExampleLayer);
};

#endif // _EXAMPLELAYER_H_
