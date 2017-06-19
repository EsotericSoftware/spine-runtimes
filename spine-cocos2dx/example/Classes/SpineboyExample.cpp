/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include "SpineboyExample.h"
#include "GoblinsExample.h"

USING_NS_CC;
using namespace spine;

Scene* SpineboyExample::scene () {
	Scene *scene = Scene::create();
	scene->addChild(SpineboyExample::create());
	return scene;
}

bool SpineboyExample::init () {
	if (!LayerColor::initWithColor(Color4B(128, 128, 128, 255))) return false;

	skeletonNode = SkeletonAnimation::createWithJsonFile("spineboy-ess.json", "spineboy.atlas", 0.6f);

    skeletonNode->setStartListener( [] (spTrackEntry* entry) {
		log("%d start: %s", entry->trackIndex, entry->animation->name);
	});
    skeletonNode->setInterruptListener( [] (spTrackEntry* entry) {
        log("%d interrupt", entry->trackIndex);
    });
	skeletonNode->setEndListener( [] (spTrackEntry* entry) {
		log("%d end", entry->trackIndex);
	});
	skeletonNode->setCompleteListener( [] (spTrackEntry* entry) {
		log("%d complete", entry->trackIndex);
	});
    skeletonNode->setDisposeListener( [] (spTrackEntry* entry) {
        log("%d dispose", entry->trackIndex);
    });
	skeletonNode->setEventListener( [] (spTrackEntry* entry, spEvent* event) {
		log("%d event: %s, %d, %f, %s", entry->trackIndex, event->data->name, event->intValue, event->floatValue, event->stringValue);
	});

	skeletonNode->setMix("walk", "jump", 0.4);
	skeletonNode->setMix("jump", "run", 0.4);
	skeletonNode->setAnimation(0, "walk", true);
	spTrackEntry* jumpEntry = skeletonNode->addAnimation(0, "jump", false, 1);
	skeletonNode->addAnimation(0, "run", true);    

	skeletonNode->setTrackStartListener(jumpEntry, [] (spTrackEntry* entry) {
		log("jumped!");
	});

	// skeletonNode->addAnimation(1, "test", true);
	// skeletonNode->runAction(RepeatForever::create(Sequence::create(FadeOut::create(1), FadeIn::create(1), DelayTime::create(5), NULL)));

	skeletonNode->setPosition(Vec2(_contentSize.width / 2, 20));
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
