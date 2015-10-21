/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include "SpineboyExample.h"
#include "GoblinsExample.h"
#include <iostream>
#include <fstream>
#include <string.h>

USING_NS_CC;
using namespace spine;
using namespace std;

Scene* SpineboyExample::scene () {
	Scene *scene = Scene::create();
	scene->addChild(SpineboyExample::create());
	return scene;
}

bool SpineboyExample::init () {
	if (!LayerColor::initWithColor(Color4B(128, 128, 128, 255))) return false;

	skeletonNode = SkeletonAnimation::createWithFile("spineboy.json", "spineboy.atlas", 0.6f);

	skeletonNode->setStartListener( [this] (int trackIndex) {
		spTrackEntry* entry = spAnimationState_getCurrent(skeletonNode->getState(), trackIndex);
		const char* animationName = (entry && entry->animation) ? entry->animation->name : 0;
		log("%d start: %s", trackIndex, animationName);
	});
	skeletonNode->setEndListener( [] (int trackIndex) {
		log("%d end", trackIndex);
	});
	skeletonNode->setCompleteListener( [] (int trackIndex, int loopCount) {
		log("%d complete: %d", trackIndex, loopCount);
	});
	skeletonNode->setEventListener( [] (int trackIndex, spEvent* event) {
		log("%d event: %s, %d, %f, %s", trackIndex, event->data->name, event->intValue, event->floatValue, event->stringValue);
	});

	skeletonNode->setMix("walk", "jump", 0.2f);
	skeletonNode->setMix("jump", "run", 0.2f);
	skeletonNode->setAnimation(0, "walk", true);
	spTrackEntry* jumpEntry = skeletonNode->addAnimation(0, "jump", false, 3);
	skeletonNode->addAnimation(0, "run", true);

	skeletonNode->setTrackStartListener(jumpEntry, [] (int trackIndex) {
		log("jumped!");
	});

	// skeletonNode->addAnimation(1, "test", true);
	// skeletonNode->runAction(RepeatForever::create(Sequence::create(FadeOut::create(1), FadeIn::create(1), DelayTime::create(5), NULL)));

	Size windowSize = Director::getInstance()->getWinSize();
	skeletonNode->setPosition(Vec2(windowSize.width / 2, 20));
	addChild(skeletonNode);

	scheduleUpdate();
	
	EventListenerTouchOneByOne* listener = EventListenerTouchOneByOne::create();
	listener->onTouchBegan = [this] (Touch* touch, Event* event) -> bool {
		if (!skeletonNode->getDebugBonesEnabled())
			skeletonNode->setDebugBonesEnabled(true);
		else if (skeletonNode->getTimeScale() == 1)
			skeletonNode->setTimeScale(0.3f);
		else
			Director::getInstance()->replaceScene(GoblinsExample::scene());
		return true;
	};
	_eventDispatcher->addEventListenerWithSceneGraphPriority(listener, this);

	return true;
}

void SpineboyExample::update (float deltaTime) {
	// Test releasing memory.
	// Director::getInstance()->replaceScene(SpineboyExample::scene());
}
