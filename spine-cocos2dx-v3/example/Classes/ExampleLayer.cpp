

#include "ExampleLayer.h"
#include <iostream>
#include <fstream>
#include <string.h>

using namespace cocos2d;
using namespace spine;
using namespace std;

Scene* ExampleLayer::scene () {
	Scene *scene = CCScene::create();
	scene->addChild(ExampleLayer::create());
	return scene;
}

bool ExampleLayer::init () {
	if (!CCLayer::init()) return false;
	
	skeletonNode = CCSkeletonAnimation::createWithFile("spineboy.json", "spineboy.atlas");
	skeletonNode->setMix("walk", "jump", 0.2f);
	skeletonNode->setMix("jump", "walk", 0.4f);
	
	skeletonNode->setAnimationListener(this, animationStateEvent_selector(ExampleLayer::animationStateEvent));
	skeletonNode->setAnimation(0, "walk", false);
	skeletonNode->addAnimation(0, "jump", false);
	skeletonNode->addAnimation(0, "walk", true);
	skeletonNode->addAnimation(0, "jump", true, 4);
	skeletonNode->addAnimation(1, "drawOrder", true);

	// skeletonNode->timeScale = 0.3f;
	skeletonNode->debugBones = true;

	skeletonNode->runAction(CCRepeatForever::create(CCSequence::create(CCFadeOut::create(1),
		CCFadeIn::create(1),
		CCDelayTime::create(5),
		NULL)));

	Size windowSize = CCDirector::getInstance()->getWinSize();
	skeletonNode->setPosition(Point(windowSize.width / 2, 20));
	addChild(skeletonNode);

	scheduleUpdate();

	return true;
}

void ExampleLayer::update (float deltaTime) {
	// Test releasing memory.
	// if (entry->time > 0.1) CCDirector::sharedDirector()->replaceScene(ExampleLayer::scene());
}

void ExampleLayer::animationStateEvent (CCSkeletonAnimation* node, int trackIndex, spEventType type, spEvent* event, int loopCount) {
	spTrackEntry* entry = spAnimationState_getCurrent(node->state, trackIndex);
	const char* animationName = (entry && entry->animation) ? entry->animation->name : 0;

	switch (type) {
	case ANIMATION_START:
		log("%d start: %s", trackIndex, animationName);
		break;
	case ANIMATION_END:
		log("%d end: %s", trackIndex, animationName);
		break;
	case ANIMATION_COMPLETE:
		log("%d complete: %s, %d", trackIndex, animationName, loopCount);
		break;
	case ANIMATION_EVENT:
		log("%d event: %s, %s: %d, %f, %s", trackIndex, animationName, event->data->name, event->intValue, event->floatValue, event->stringValue);
		break;
	}
	fflush(stdout);
}
