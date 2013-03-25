#include "ExampleScene.h"
#include <iostream>
#include <fstream>
#include <spine-cocos2dx/spine.h>

using namespace cocos2d;
using namespace spine;
using namespace std;

CCScene* ExampleScene::scene() {
	CCScene *scene = CCScene::create();
	ExampleScene *layer = ExampleScene::create();
	scene->addChild(layer);
	return scene;
}

bool ExampleScene::init() {
	if (!CCLayer::init()) return false;

	Atlas *atlas = new Atlas("data/spineboy.atlas");
	SkeletonJson json(atlas);
	SkeletonData *skeletonData = json.readSkeletonDataFile("data/spineboy-skeleton.json");
	Animation *animation = json.readAnimationFile("data/spineboy-walk.json", skeletonData);

	CCSkeleton* skeletonNode = new CCSkeleton(skeletonData);
	skeletonNode->state->setAnimation(animation, true);

	CCSize winSize = CCDirector::sharedDirector()->getWinSize();
	skeletonNode->setPosition(ccp(winSize.width / 2, 20));
	addChild(skeletonNode);

	return true;
}
