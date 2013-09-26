

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

	skeletonNode = CCSkeletonAnimation::createWithFile("spineboy.json", "spineboy.atlas");
	skeletonNode->setMix("walk", "jump", 0.2f);
	skeletonNode->setMix("jump", "walk", 0.4f);

	skeletonNode->setAnimation(0, "walk", true);
	// This shows how to setup animations to play back to back.
	//skeletonNode->addAnimation(0, "jump", false);
	//skeletonNode->addAnimation(0, "walk", false);
	//skeletonNode->addAnimation(0, "jump", true);

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
	TrackEntry* entry = skeletonNode->getCurrent(0);
	if (entry->loop) {
        if (entry->time > 2) skeletonNode->setAnimation(0, "jump", false);
    } else {
        if (entry->time > 1) skeletonNode->setAnimation(0, "walk", true);
    }
    // if (entry->time > 0.1) CCDirector::sharedDirector()->replaceScene(ExampleLayer::scene());
}