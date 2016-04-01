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

#include "BatchingExample.h"
#include "SpineboyExample.h"

USING_NS_CC;
using namespace spine;

Scene* BatchingExample::scene () {
	Scene *scene = Scene::create();
	scene->addChild(BatchingExample::create());
	return scene;
}

bool BatchingExample::init () {
	if (!LayerColor::initWithColor(Color4B(128, 128, 128, 255))) return false;

	// Load the texture atlas.
	spAtlas* atlas = spAtlas_createFromFile("spineboy.atlas", 0);
	CCASSERT(atlas, "Error reading atlas file.");

	// Load the skeleton data.
	spSkeletonJson* json = spSkeletonJson_create(atlas);
	json->scale = 0.6f;
	spSkeletonData* skeletonData = spSkeletonJson_readSkeletonDataFile(json, "spineboy.json");
	CCASSERT(skeletonData, json->error ? json->error : "Error reading skeleton data file.");
	spSkeletonJson_dispose(json);

	// Setup mix times.
	spAnimationStateData* stateData = spAnimationStateData_create(skeletonData);
	spAnimationStateData_setMixByName(stateData, "walk", "jump", 0.2f);
	spAnimationStateData_setMixByName(stateData, "jump", "run", 0.2f);

	Size windowSize = Director::getInstance()->getWinSize();
	int xMin = (int)(windowSize.width * 0.10f), xMax = (int)windowSize.width - xMin;
	int yMin = 20, yMax = windowSize.height - 350;
	for (int i = 0; i < 50; i++) {
		// Each skeleton node shares the same atlas, skeleton data, and mix times.
		SkeletonAnimation* skeletonNode = SkeletonAnimation::createWithData(skeletonData, false);
		skeletonNode->setAnimationStateData(stateData);

		skeletonNode->setAnimation(0, "walk", true);
		skeletonNode->addAnimation(0, "jump", false, 3);
		skeletonNode->addAnimation(0, "run", true);

		skeletonNode->setPosition(Vec2(
			RandomHelper::random_int(xMin, xMax),
			RandomHelper::random_int(yMin, yMax)
		));
		addChild(skeletonNode);
	}

	scheduleUpdate();

	EventListenerTouchOneByOne* listener = EventListenerTouchOneByOne::create();
	listener->onTouchBegan = [this] (Touch* touch, Event* event) -> bool {
		Director::getInstance()->replaceScene(SpineboyExample::scene());
		return true;
	};
	_eventDispatcher->addEventListenerWithSceneGraphPriority(listener, this);

	return true;
}
