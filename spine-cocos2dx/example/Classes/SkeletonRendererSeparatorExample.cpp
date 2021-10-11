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

#include "SkeletonRendererSeparatorExample.h"
#include "GoblinsExample.h"

USING_NS_CC;
using namespace spine;

Scene *SkeletonRendererSeparatorExample::scene() {
	Scene *scene = Scene::create();
	scene->addChild(SkeletonRendererSeparatorExample::create());
	return scene;
}

bool SkeletonRendererSeparatorExample::init() {
	if (!LayerColor::initWithColor(Color4B(128, 128, 128, 255))) return false;

	// Spineboy's back, which will manage the animation and GPU resources
	// will render only the front slots of Spineboy
	backNode = SkeletonAnimation::createWithJsonFile("spineboy-pro.json", "spineboy.atlas", 0.6f);
	backNode->setMix("walk", "jump", 0.4);
	backNode->setAnimation(0, "walk", true);
	backNode->setSlotsRange(backNode->findSlot("rear-upper-arm")->getData().getIndex(), backNode->findSlot("rear-shin")->getData().getIndex());
	backNode->setPosition(Vec2(_contentSize.width / 2, 20));

	// A simple rectangle to go between the front and back slots of Spineboy
	betweenNode = DrawNode::create();
	Vec2 rect[4];
	rect[0] = Vec2(0, 0);
	rect[1] = Vec2(40, 0);
	rect[2] = Vec2(40, 200);
	rect[3] = Vec2(0, 200);
	betweenNode->drawPolygon(rect, 4, Color4F(1, 0, 0, 1), 1, Color4F(1, 0, 0, 1));
	betweenNode->setPosition(Vec2(_contentSize.width / 2 + 30, 20));
	// Spineboy's front, doesn't manage any skeleton, animation or GPU resources, but simply
	// renders the back slots of Spineboy. The skeleton, animatio state and GPU resources
	// are shared with the front node!
	frontNode = SkeletonRenderer::createWithSkeleton(backNode->getSkeleton());
	frontNode->setSlotsRange(frontNode->findSlot("neck")->getData().getIndex(), -1);
	frontNode->setPosition(Vec2(_contentSize.width / 2, 20));

	// Add the front, between and back node in the correct order to this scene
	addChild(backNode);
	addChild(betweenNode);
	addChild(frontNode);

	scheduleUpdate();

	EventListenerTouchOneByOne *listener = EventListenerTouchOneByOne::create();
	listener->onTouchBegan = [this](Touch *touch, cocos2d::Event *event) -> bool {
		if (!backNode->getDebugBonesEnabled())
			backNode->setDebugBonesEnabled(true);
		else if (backNode->getTimeScale() == 1)
			backNode->setTimeScale(0.3f);
		else
			Director::getInstance()->replaceScene(GoblinsExample::scene());
		return true;
	};
	_eventDispatcher->addEventListenerWithSceneGraphPriority(listener, this);

	return true;
}

void SkeletonRendererSeparatorExample::update(float deltaTime) {
	// Test releasing memory.
	// Director::getInstance()->replaceScene(SpineboyExample::scene());
}
