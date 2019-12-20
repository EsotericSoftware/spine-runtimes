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

#include "IKExample.h"
#include "SpineboyExample.h"

USING_NS_CC;
using namespace spine;

// This example demonstrates how to set the position
// of a bone based on the touch position, which in
// turn will make an IK chain follow that bone
// smoothly.
Scene* IKExample::scene () {
    Scene *scene = Scene::create();
    scene->addChild(IKExample::create());
    return scene;
}

bool IKExample::init () {
    if (!LayerColor::initWithColor(Color4B(128, 128, 128, 255))) return false;
	
	// Load the Spineboy skeleton and create a SkeletonAnimation node from it
	// centered on the screen.
    skeletonNode = SkeletonAnimation::createWithJsonFile("spineboy-pro.json", "spineboy.atlas", 0.6f);
    skeletonNode->setPosition(Vec2(_contentSize.width / 2, 20));
    addChild(skeletonNode);
    
	// Queue the "walk" animation on the first track.
	skeletonNode->setAnimation(0, "walk", true);
	
	// Queue the "aim" animation on a higher track.
	// It consists of a single frame that positions
	// the back arm and gun such that they point at
	// the "crosshair" bone. By setting this
	// animation on a higher track, it overrides
	// any changes to the back arm and gun made
	// by the walk animation, allowing us to
	// mix the two. The mouse position following
	// is performed in the lambda below.
	skeletonNode->setAnimation(1, "aim", true);

	// Next we setup a listener that receives and stores
	// the current mouse location. The location is converted
	// to the skeleton's coordinate system.
	EventListenerMouse* mouseListener = EventListenerMouse::create();
	mouseListener->onMouseMove = [this] (cocos2d::Event* event) -> void {
		// convert the mosue location to the skeleton's coordinate space
		// and store it.
		EventMouse* mouseEvent = dynamic_cast<EventMouse*>(event);
		position = skeletonNode->convertToNodeSpace(mouseEvent->getLocationInView());
	};
	_eventDispatcher->addEventListenerWithSceneGraphPriority(mouseListener, this);
	
	// Position the "crosshair" bone at the mouse
	// location.
	//
	// When setting the crosshair bone position
	// to the mouse position, we need to translate
	// from "skeleton space" to "local bone space".
	// Note that the local bone space is calculated
	// using the bone's parent worldToLocal() function!
	//
	// After updating the bone position based on the
	// converted mouse location, we call updateWorldTransforms()
	// again so the change of the IK target position is
	// applied to the rest of the skeleton.
	skeletonNode->setPostUpdateWorldTransformsListener([this] (SkeletonAnimation* node) -> void {
		Bone* crosshair = node->findBone("crosshair"); // The bone should be cached
		float localX = 0, localY = 0;
		crosshair->getParent()->worldToLocal(position.x, position.y, localX, localY);
		crosshair->setX(localX);
		crosshair->setY(localY);
		crosshair->setAppliedValid(false);
		
		node->getSkeleton()->updateWorldTransform();
	});
	
	EventListenerTouchOneByOne* listener = EventListenerTouchOneByOne::create();
	listener->onTouchBegan = [this] (Touch* touch, cocos2d::Event* event) -> bool {
        Director::getInstance()->replaceScene(SpineboyExample::scene());
        return true;
    };
	
    _eventDispatcher->addEventListenerWithSceneGraphPriority(listener, this);
	
    scheduleUpdate();

    return true;
}

void IKExample::update (float deltaTime) {
    
}
