#ifndef _EXAMPLESCENE_H_
#define _EXAMPLESCENE_H_

#include "cocos2d.h"
#include <spine-cocos2dx/spine.h>

class ExampleScene: public cocos2d::CCLayer {
private:
	spine::Atlas *atlas;
	spine::SkeletonData *skeletonData;
	spine::Animation *animation;

public:
	static cocos2d::CCScene* scene ();
	~ExampleScene ();

	virtual bool init ();

	CREATE_FUNC (ExampleScene);
};

#endif // _EXAMPLESCENE_H_
