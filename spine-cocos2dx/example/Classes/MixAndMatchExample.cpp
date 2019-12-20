/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

#include "MixAndMatchExample.h"
#include "BatchingExample.h"

USING_NS_CC;
using namespace spine;

Scene* MixAndMatchExample::scene () {
	Scene *scene = Scene::create();
	scene->addChild(MixAndMatchExample::create());
	return scene;
}

MixAndMatchExample::~MixAndMatchExample() {
	delete skin;
}

bool MixAndMatchExample::init () {
	if (!LayerColor::initWithColor(Color4B(128, 128, 128, 255))) return false;

	skeletonNode = SkeletonAnimation::createWithBinaryFile("mix-and-match-pro.skel", "mix-and-match.atlas", 0.5);
	skeletonNode->setAnimation(0, "dance", true);
	
	// Create a new skin, by mixing and matching other skins
	// that fit together. Items making up the girl are individual
	// skins. Using the skin API, a new skin is created which is
	// a combination of all these individual item skins.
	SkeletonData* skeletonData = skeletonNode->getSkeleton()->getData();
	skin = new (__FILE__, __LINE__) Skin("mix-and-match");
	skin->addSkin(skeletonData->findSkin("skin-base"));
	skin->addSkin(skeletonData->findSkin("nose/short"));
	skin->addSkin(skeletonData->findSkin("eyelids/girly"));
	skin->addSkin(skeletonData->findSkin("eyes/violet"));
	skin->addSkin(skeletonData->findSkin("hair/brown"));
	skin->addSkin(skeletonData->findSkin("clothes/hoodie-orange"));
	skin->addSkin(skeletonData->findSkin("legs/pants-jeans"));
	skin->addSkin(skeletonData->findSkin("accessories/bag"));
	skin->addSkin(skeletonData->findSkin("accessories/hat-red-yellow"));
	skeletonNode->getSkeleton()->setSkin(skin);

	skeletonNode->setPosition(Vec2(_contentSize.width / 2, _contentSize.height / 2 - 200));
	addChild(skeletonNode);

	scheduleUpdate();

	EventListenerTouchOneByOne* listener = EventListenerTouchOneByOne::create();
	listener->onTouchBegan = [this] (Touch* touch, cocos2d::Event* event) -> bool {
		if (!skeletonNode->getDebugBonesEnabled())
			skeletonNode->setDebugBonesEnabled(true);
		else if (skeletonNode->getTimeScale() == 1)
			skeletonNode->setTimeScale(0.3f);
		else
			Director::getInstance()->replaceScene(BatchingExample::scene());
		return true;
	};
	_eventDispatcher->addEventListenerWithSceneGraphPriority(listener, this);

	return true;
}
