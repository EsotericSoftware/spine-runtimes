/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include "SpineboyExample.h"
#include "SkeletonRendererSeparatorExample.h"

USING_NS_CC;
using namespace spine;

Scene *SpineboyExample::scene() {
	Scene *scene = Scene::create();
	scene->addChild(SpineboyExample::create());
	return scene;
}

bool SpineboyExample::init() {
	if (!LayerColor::initWithColor(Color4B(128, 128, 128, 255))) return false;

	skeletonNode = SkeletonAnimation::createWithJsonFile("spineboy-pro.json", "spineboy.atlas", 0.6f);

	skeletonNode->setStartListener([](TrackEntry *entry) {
		log("%d start: %s", entry->getTrackIndex(), entry->getAnimation()->getName().buffer());
	});
	skeletonNode->setInterruptListener([](TrackEntry *entry) {
		log("%d interrupt", entry->getTrackIndex());
	});
	skeletonNode->setEndListener([](TrackEntry *entry) {
		log("%d end", entry->getTrackIndex());
	});
	skeletonNode->setCompleteListener([](TrackEntry *entry) {
		log("%d complete", entry->getTrackIndex());
	});
	skeletonNode->setDisposeListener([](TrackEntry *entry) {
		log("%d dispose", entry->getTrackIndex());
	});
	skeletonNode->setEventListener([](TrackEntry *entry, spine::Event *event) {
		log("%d event: %s, %d, %f, %s", entry->getTrackIndex(), event->getData().getName().buffer(), event->getIntValue(), event->getFloatValue(), event->getStringValue().buffer());
	});

	skeletonNode->setMix("walk", "jump", 0.4);
	skeletonNode->setMix("jump", "run", 0.4);
	skeletonNode->setAnimation(0, "walk", true);
	TrackEntry *jumpEntry = skeletonNode->addAnimation(0, "jump", false, 1);
	skeletonNode->addAnimation(0, "run", true);

	skeletonNode->setTrackStartListener(jumpEntry, [](TrackEntry *entry) {
		log("jumped!");
	});

	// skeletonNode->addAnimation(1, "test", true);
	// skeletonNode->runAction(RepeatForever::create(Sequence::create(FadeOut::create(1), FadeIn::create(1), DelayTime::create(5), NULL)));

	skeletonNode->setPosition(Vec2(_contentSize.width / 2, 20));
	addChild(skeletonNode);

	scheduleUpdate();

	EventListenerTouchOneByOne *listener = EventListenerTouchOneByOne::create();
	listener->onTouchBegan = [this](Touch *touch, cocos2d::Event *event) -> bool {
		if (!skeletonNode->getDebugBonesEnabled())
			skeletonNode->setDebugBonesEnabled(true);
		else if (skeletonNode->getTimeScale() == 1)
			skeletonNode->setTimeScale(0.3f);
		else
			Director::getInstance()->replaceScene(SkeletonRendererSeparatorExample::scene());
		return true;
	};
	_eventDispatcher->addEventListenerWithSceneGraphPriority(listener, this);

	return true;
}

void SpineboyExample::update(float deltaTime) {
	// Test releasing memory.
	// Director::getInstance()->replaceScene(SpineboyExample::scene());
}
