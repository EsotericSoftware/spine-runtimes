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

#include "BatchingExample.h"
#include "IKExample.h"

USING_NS_CC;
using namespace spine;

#define NUM_SKELETONS 50

Cocos2dTextureLoader textureLoader;

Scene *BatchingExample::scene() {
	Scene *scene = Scene::create();
	scene->addChild(BatchingExample::create());
	return scene;
}

bool BatchingExample::init() {
	if (!LayerColor::initWithColor(Color4B(128, 128, 128, 255))) return false;

	// Load the texture atlas. Note that the texture loader has to live
	// as long as the Atlas, as the Atlas destructor will call TextureLoader::unload.
	_atlas = new (__FILE__, __LINE__) Atlas("spineboy.atlas", &textureLoader, true);
	CCASSERT(_atlas, "Error reading atlas file.");

	// This attachment loader configures attachments with data needed for cocos2d-x rendering.
	// Do not dispose the attachment loader until the skeleton data is disposed!
	_attachmentLoader = new (__FILE__, __LINE__) Cocos2dAtlasAttachmentLoader(_atlas);

	// Load the skeleton data.
	SkeletonJson *json = new (__FILE__, __LINE__) SkeletonJson(_attachmentLoader);
	json->setScale(0.6f);// Resizes skeleton data to 60% of the size it was in Spine.
	_skeletonData = json->readSkeletonDataFile("spineboy-pro.json");
	CCASSERT(_skeletonData, json->getError().isEmpty() ? json->getError().buffer() : "Error reading skeleton data file.");
	delete json;

	// Setup mix times.
	_stateData = new (__FILE__, __LINE__) AnimationStateData(_skeletonData);
	_stateData->setMix("walk", "jump", 0.2f);
	_stateData->setMix("jump", "run", 0.2f);

	int xMin = _contentSize.width * 0.10f, xMax = _contentSize.width * 0.90f;
	int yMin = 0, yMax = _contentSize.height * 0.7f;
	for (int i = 0; i < NUM_SKELETONS; i++) {
		// Each skeleton node shares the same atlas, skeleton data, and mix times.
		SkeletonAnimation *skeletonNode = SkeletonAnimation::createWithData(_skeletonData, false);
		skeletonNode->setAnimationStateData(_stateData);

		skeletonNode->setAnimation(0, "walk", true);
		skeletonNode->addAnimation(0, "jump", true, RandomHelper::random_int(0, 300) / 100.0f);
		skeletonNode->addAnimation(0, "run", true);

		// alternative setting two color tint for groups of 10 skeletons
		// should end up with #skeletons / 10 batches
		// if (j++ < 10)
		//			skeletonNode->setTwoColorTint(true);
		//		if (j == 20) j = 0;
		// skeletonNode->setTwoColorTint(true);

		skeletonNode->setPosition(Vec2(
				RandomHelper::random_int(xMin, xMax),
				RandomHelper::random_int(yMin, yMax)));
		addChild(skeletonNode);
	}

	scheduleUpdate();

	EventListenerTouchOneByOne *listener = EventListenerTouchOneByOne::create();
	listener->onTouchBegan = [this](Touch *touch, cocos2d::Event *event) -> bool {
		Director::getInstance()->replaceScene(IKExample::scene());
		return true;
	};
	_eventDispatcher->addEventListenerWithSceneGraphPriority(listener, this);

	return true;
}

BatchingExample::~BatchingExample() {
	// SkeletonAnimation instances are cocos2d-x nodes and are disposed of automatically as normal, but the data created
	// manually to be shared across multiple SkeletonAnimations needs to be disposed of manually.

	delete _skeletonData;
	delete _stateData;
	delete _attachmentLoader;
	delete _atlas;
}
