#include "ExampleScene.h"
#include <iostream>
#include <fstream>

using namespace cocos2d;
using namespace spine;
using namespace std;

CCScene* ExampleScene::scene () {
	CCScene *scene = CCScene::create();
	scene->addChild(ExampleScene::create());
	return scene;
}

bool ExampleScene::init () {
	if (!CCLayer::init()) return false;

	atlas = new Atlas("spineboy.txt");
	SkeletonJson json(atlas);
	json.scale = 0.5;
	skeletonData = json.readSkeletonData("spineboy-skeleton.json");
	animation = json.readAnimation("spineboy-walk.json", skeletonData);

	CCSkeleton* skeletonNode = CCSkeleton::create(skeletonData);
	skeletonNode->state->setAnimation(animation, true);
	skeletonNode->debug = true;

	CCSize windowSize = CCDirector::sharedDirector()->getWinSize();
	skeletonNode->setPosition(ccp(windowSize.width / 2, 20));
	addChild(skeletonNode);

	return true;
}

ExampleScene::~ExampleScene () {
	delete atlas;
	delete skeletonData;
	delete animation;
}
