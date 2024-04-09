/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include "PhysicsExample.h"
#include "SpineboyExample.h"

USING_NS_CC;
using namespace spine;

Scene *PhysicsExample::scene() {
	Scene *scene = Scene::create();
	scene->addChild(PhysicsExample::create());
	return scene;
}

bool PhysicsExample::init() {
	if (!LayerColor::initWithColor(Color4B(128, 128, 128, 255))) return false;

	// Load the Spineboy skeleton and create a SkeletonAnimation node from it
	// centered on the screen.
	skeletonNode = SkeletonAnimation::createWithBinaryFile("celestial-circus-pro.skel", "celestial-circus.atlas", 0.2f);
	skeletonNode->setPosition(Vec2(_contentSize.width / 2, 200));
	addChild(skeletonNode);

	// Queue the "walk" animation on the first track.
	// skeletonNode->setAnimation(0, "walk", true);

	// Next we setup a listener that receives and stores
	// the current mouse location and updates the skeleton position
    // accordingly.
	EventListenerMouse *mouseListener = EventListenerMouse::create();
	mouseListener->onMouseMove = [this](cocos2d::Event *event) -> void {
		// convert the mosue location to the skeleton's coordinate space
		// and store it.
		EventMouse *mouseEvent = dynamic_cast<EventMouse *>(event);
		Vec2 mousePosition = skeletonNode->convertToNodeSpace(mouseEvent->getLocationInView());
        if (firstUpdate) {
            firstUpdate = false;
            lastMousePosition = mousePosition;
            return;
        }
        Vec2 delta = mousePosition - lastMousePosition;
        skeletonNode->getSkeleton()->physicsTranslate(-delta.x, -delta.y);
        lastMousePosition = mousePosition;
	};
	_eventDispatcher->addEventListenerWithSceneGraphPriority(mouseListener, this);

	EventListenerTouchOneByOne *listener = EventListenerTouchOneByOne::create();
	listener->onTouchBegan = [this](Touch *touch, cocos2d::Event *event) -> bool {
		Director::getInstance()->replaceScene(SpineboyExample::scene());
		return true;
	};

	_eventDispatcher->addEventListenerWithSceneGraphPriority(listener, this);

	scheduleUpdate();

	return true;
}

void PhysicsExample::update(float deltaTime) {
}
