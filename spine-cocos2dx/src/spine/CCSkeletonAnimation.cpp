/******************************************************************************
 * Spine Runtimes Software License
 * Version 2
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software, you may not (a) modify, translate, adapt or
 * otherwise create derivative works, improvements of the Software or develop
 * new applications using the Software or (b) remove, delete, alter or obscure
 * any trademarks or any copyright, trademark, patent or other intellectual
 * property or proprietary rights notices on or in the Software, including
 * any copy thereof. Redistributions in binary or source form must include
 * this license and terms. THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
 * TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include <spine/CCSkeletonAnimation.h>
#include <spine/extension.h>
#include <spine/spine-cocos2dx.h>
#include <algorithm>

USING_NS_CC;
using std::min;
using std::max;
using std::vector;

namespace spine {

static void callback (spAnimationState* state, int trackIndex, spEventType type, spEvent* event, int loopCount) {
	((CCSkeletonAnimation*)state->context)->onAnimationStateEvent(trackIndex, type, event, loopCount);
}

CCSkeletonAnimation* CCSkeletonAnimation::createWithData (spSkeletonData* skeletonData) {
	CCSkeletonAnimation* node = new CCSkeletonAnimation(skeletonData);
	node->autorelease();
	return node;
}

CCSkeletonAnimation* CCSkeletonAnimation::createWithFile (const char* skeletonDataFile, spAtlas* atlas, float scale) {
	CCSkeletonAnimation* node = new CCSkeletonAnimation(skeletonDataFile, atlas, scale);
	node->autorelease();
	return node;
}

CCSkeletonAnimation* CCSkeletonAnimation::createWithFile (const char* skeletonDataFile, const char* atlasFile, float scale) {
	CCSkeletonAnimation* node = new CCSkeletonAnimation(skeletonDataFile, atlasFile, scale);
	node->autorelease();
	return node;
}

void CCSkeletonAnimation::initialize () {
	listenerInstance = 0;
	listenerMethod = 0;

	ownsAnimationStateData = true;
	state = spAnimationState_create(spAnimationStateData_create(skeleton->data));
	state->context = this;
	state->listener = callback;
}

CCSkeletonAnimation::CCSkeletonAnimation (spSkeletonData *skeletonData)
		: CCSkeleton(skeletonData) {
	initialize();
}

CCSkeletonAnimation::CCSkeletonAnimation (const char* skeletonDataFile, spAtlas* atlas, float scale)
		: CCSkeleton(skeletonDataFile, atlas, scale) {
	initialize();
}

CCSkeletonAnimation::CCSkeletonAnimation (const char* skeletonDataFile, const char* atlasFile, float scale)
		: CCSkeleton(skeletonDataFile, atlasFile, scale) {
	initialize();
}

CCSkeletonAnimation::~CCSkeletonAnimation () {
	if (ownsAnimationStateData) spAnimationStateData_dispose(state->data);
	spAnimationState_dispose(state);
}

void CCSkeletonAnimation::update (float deltaTime) {
	super::update(deltaTime);

	deltaTime *= timeScale;
	spAnimationState_update(state, deltaTime);
	spAnimationState_apply(state, skeleton);
	spSkeleton_updateWorldTransform(skeleton);
}

void CCSkeletonAnimation::setAnimationStateData (spAnimationStateData* stateData) {
	CCAssert(stateData, "stateData cannot be null.");

	if (ownsAnimationStateData) spAnimationStateData_dispose(state->data);
	spAnimationState_dispose(state);

	ownsAnimationStateData = false;
	state = spAnimationState_create(stateData);
	state->context = this;
	state->listener = callback;
}

void CCSkeletonAnimation::setMix (const char* fromAnimation, const char* toAnimation, float duration) {
	spAnimationStateData_setMixByName(state->data, fromAnimation, toAnimation, duration);
}

void CCSkeletonAnimation::setAnimationListener (CCObject* instance, SEL_AnimationStateEvent method) {
	listenerInstance = instance;
	listenerMethod = method;
}

spTrackEntry* CCSkeletonAnimation::setAnimation (int trackIndex, const char* name, bool loop) {
	spAnimation* animation = spSkeletonData_findAnimation(skeleton->data, name);
	if (!animation) {
		CCLog("Spine: Animation not found: %s", name);
		return 0;
	}
	return spAnimationState_setAnimation(state, trackIndex, animation, loop);
}

spTrackEntry* CCSkeletonAnimation::addAnimation (int trackIndex, const char* name, bool loop, float delay) {
	spAnimation* animation = spSkeletonData_findAnimation(skeleton->data, name);
	if (!animation) {
		CCLog("Spine: Animation not found: %s", name);
		return 0;
	}
	return spAnimationState_addAnimation(state, trackIndex, animation, loop, delay);
}

spTrackEntry* CCSkeletonAnimation::getCurrent (int trackIndex) { 
	return spAnimationState_getCurrent(state, trackIndex);
}

void CCSkeletonAnimation::clearTracks () {
	spAnimationState_clearTracks(state);
}

void CCSkeletonAnimation::clearTrack (int trackIndex) {
	spAnimationState_clearTrack(state, trackIndex);
}

void CCSkeletonAnimation::onAnimationStateEvent (int trackIndex, spEventType type, spEvent* event, int loopCount) {
	if (listenerInstance) (listenerInstance->*listenerMethod)(this, trackIndex, type, event, loopCount);
}

}
