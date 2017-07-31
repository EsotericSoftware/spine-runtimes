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

#include "RaptorExample.h"
#include "TankExample.h"
#include <spine/extension.h>

USING_NS_CC;
using namespace spine;

spSwirlVertexEffect* effect = spSwirlVertexEffect_create(400);

Scene* RaptorExample::scene () {
	Scene *scene = Scene::create();
	scene->addChild(RaptorExample::create());
	return scene;
}

bool RaptorExample::init () {
	if (!LayerColor::initWithColor(Color4B(128, 128, 128, 255))) return false;

	skeletonNode = SkeletonAnimation::createWithJsonFile("raptor-pro.json", "raptor.atlas", 0.5f);
	skeletonNode->setAnimation(0, "walk", true);
	skeletonNode->setAnimation(1, "empty", false);
	skeletonNode->addAnimation(1, "gungrab", false, 2);
	skeletonNode->setTwoColorTint(true);
	
	effect->centerY = 200;
	swirlTime = 0;
	
	skeletonNode->setVertexEffect(&effect->super);

	skeletonNode->setPosition(Vec2(_contentSize.width / 2, 20));
	addChild(skeletonNode);

	scheduleUpdate();
	
	EventListenerTouchOneByOne* listener = EventListenerTouchOneByOne::create();
	listener->onTouchBegan = [this] (Touch* touch, Event* event) -> bool {
		if (!skeletonNode->getDebugBonesEnabled()) {
			skeletonNode->setDebugBonesEnabled(true);
			skeletonNode->setDebugMeshesEnabled(true);
		} else if (skeletonNode->getTimeScale() == 1)
			skeletonNode->setTimeScale(0.3f);
		else
			Director::getInstance()->replaceScene(TankExample::scene());
		return true;
	};
	_eventDispatcher->addEventListenerWithSceneGraphPriority(listener, this);

	return true;
}

void RaptorExample::update(float fDelta) {
	swirlTime += fDelta;
	float percent = fmod(swirlTime, 2);
	if (percent > 1) percent = 1 - (percent - 1);
	effect->angle = _spMath_interpolate(_spMath_pow2_apply, -60, 60, percent);
}
