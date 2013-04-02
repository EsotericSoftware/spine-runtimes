#include "ExampleLayer.h"
#include <iostream>
#include <fstream>

using namespace cocos2d;
using namespace spine;
using namespace std;

CCScene* ExampleLayer::scene () {
	CCScene *scene = CCScene::create();
	scene->addChild(ExampleLayer::create());
	return scene;
}

bool ExampleLayer::init () {
	if (!CCLayer::init()) return false;

	atlas = Atlas_readAtlasFile("spineboy.atlas");
	SkeletonJson* json = SkeletonJson_create(atlas);
	json->scale = 0.75;
	skeletonData = SkeletonJson_readSkeletonDataFile(json, "spineboy-skeleton.json");
	animation = SkeletonJson_readAnimationFile(json, "spineboy-walk.json", skeletonData);
	SkeletonJson_dispose(json);

	CCSkeleton* skeletonNode = CCSkeleton::create(skeletonData);
	Skeleton_setToBindPose(skeletonNode->skeleton);
	AnimationState_setAnimation(skeletonNode->state, animation, true);
	skeletonNode->debugBones = true;

	/*skeletonNode->runAction(CCRepeatForever::create(CCSequence::create(CCFadeOut::create(1),
		CCFadeIn::create(1),
		CCDelayTime::create(5),
		NULL)));*/

	CCSize windowSize = CCDirector::sharedDirector()->getWinSize();
	skeletonNode->setPosition(ccp(windowSize.width / 2, 20));
	addChild(skeletonNode);

	return true;
}

ExampleLayer::~ExampleLayer () {
	SkeletonData_dispose(skeletonData);
	Animation_dispose(animation);
	Atlas_dispose(atlas);
}
