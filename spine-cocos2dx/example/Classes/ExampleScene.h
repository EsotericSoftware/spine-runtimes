#ifndef _EXAMPLESCENE_H_
#define _EXAMPLESCENE_H_

#include "cocos2d.h"

class ExampleScene : public cocos2d::CCLayer {
public:
	static cocos2d::CCScene* scene();

	virtual bool init();

	CREATE_FUNC(ExampleScene);
};

#endif // SPINE_EXAMPLESCENE_H_
