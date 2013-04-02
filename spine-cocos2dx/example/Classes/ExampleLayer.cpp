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

    // Load atlas, skeleton, and animations.
	atlas = Atlas_readAtlasFile("spineboy.atlas");
	SkeletonJson* json = SkeletonJson_create(atlas);
	json->scale = 0.75;
	skeletonData = SkeletonJson_readSkeletonDataFile(json, "spineboy-skeleton.json");
	walkAnimation = SkeletonJson_readAnimationFile(json, "spineboy-walk.json", skeletonData);
	jumpAnimation = SkeletonJson_readAnimationFile(json, "spineboy-jump.json", skeletonData);
	SkeletonJson_dispose(json);

	// Configure mixing.
	AnimationStateData* stateData = AnimationStateData_create();
	AnimationStateData_setMix(stateData, walkAnimation, jumpAnimation, 0.4f);
	AnimationStateData_setMix(stateData, jumpAnimation, walkAnimation, 0.4f);

	skeletonNode = CCSkeleton::create(skeletonData, stateData);
	Skeleton_setToBindPose(skeletonNode->skeleton);
	AnimationState_setAnimation(skeletonNode->state, walkAnimation, true);
	skeletonNode->timeScale = 0.3f;
	skeletonNode->debugBones = true;

	skeletonNode->runAction(CCRepeatForever::create(CCSequence::create(CCFadeOut::create(1),
		CCFadeIn::create(1),
		CCDelayTime::create(5),
		NULL)));

	CCSize windowSize = CCDirector::sharedDirector()->getWinSize();
	skeletonNode->setPosition(ccp(windowSize.width / 2, 20));
	addChild(skeletonNode);

	scheduleUpdate();

	return true;
}

void ExampleLayer::update (float deltaTime) {
    if (skeletonNode->state->animation == walkAnimation) {
        if (skeletonNode->state->time > 2) AnimationState_setAnimation(skeletonNode->state, jumpAnimation, false);
    } else {
        if (skeletonNode->state->time > 1) AnimationState_setAnimation(skeletonNode->state, walkAnimation, true);
    }
}

ExampleLayer::~ExampleLayer () {
	SkeletonData_dispose(skeletonData);
	Animation_dispose(walkAnimation);
	Animation_dispose(jumpAnimation);
	Atlas_dispose(atlas);
}

