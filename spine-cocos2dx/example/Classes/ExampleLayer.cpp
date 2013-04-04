#include "ExampleLayer.h"
#include <iostream>
#include <fstream>
#include <string.h>

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

	skeletonNode = CCSkeleton::create("spineboy.json", "spineboy.atlas");
	skeletonNode->setMix("walk", "jump", 0.4f);
	skeletonNode->setMix("jump", "walk", 0.4f);
	skeletonNode->setAnimation("walk", true);
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
    if (strcmp(skeletonNode->state->animation->name, "walk") == 0) {
        if (skeletonNode->state->time > 2) skeletonNode->setAnimation("jump", false);
    } else {
        if (skeletonNode->state->time > 1) skeletonNode->setAnimation("walk", true);
    }
}
