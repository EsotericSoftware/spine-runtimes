

#ifndef _EXAMPLELAYER_H_
#define _EXAMPLELAYER_H_

#include "cocos2d.h"
#include <spine/spine-cocos2dx.h>

class ExampleLayer: public cocos2d::CCLayer {
public:
	static cocos2d::CCScene* scene ();

	virtual bool init ();
	virtual void update (float deltaTime);

	CREATE_FUNC (ExampleLayer);
private:
	spine::CCSkeletonAnimation* skeletonNode;
	
	void animationStateEvent (spine::CCSkeletonAnimation* node, int trackIndex, EventType type, Event* event, int loopCount);
};

#endif // _EXAMPLELAYER_H_