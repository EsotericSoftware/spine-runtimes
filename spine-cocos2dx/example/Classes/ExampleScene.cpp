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

	Atlas *atlas = new Atlas("spineboy.txt");
	SkeletonJson json(atlas);
  json.scale = 0.5;
	SkeletonData *skeletonData = json.readSkeletonData("spineboy-skeleton.json");
	Animation *animation = json.readAnimation("spineboy-walk.json", skeletonData);

	CCSkeleton* skeletonNode = new CCSkeleton(skeletonData);
	skeletonNode->state->setAnimation(animation, true);
	skeletonNode->debug = true;

	CCSize windowSize = CCDirector::sharedDirector()->getWinSize();
	skeletonNode->setPosition(ccp(windowSize.width / 2, 20));
	addChild(skeletonNode);

	return true;
}
