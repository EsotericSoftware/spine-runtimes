/******************************************************************************
 * Spine Runtime Software License - Version 1.0
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Single User License or Spine Professional License must be
 *    purchased from Esoteric Software and the license must remain valid:
 *    http://esotericsoftware.com/
 * 2. Redistributions of source code must retain this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer.
 * 3. Redistributions in binary form must reproduce this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer, in the documentation and/or other materials provided with the
 *    distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include <spine/CCSkeletonAnimation.h>
#include <spine/extension.h>
#include <spine/spine-cocos2dx.h>

USING_NS_CC;
using std::min;
using std::max;
using std::vector;

namespace spine {

CCSkeletonAnimation* CCSkeletonAnimation::createWithData (SkeletonData* skeletonData) {
	CCSkeletonAnimation* node = new CCSkeletonAnimation(skeletonData);
	node->autorelease();
	return node;
}

CCSkeletonAnimation* CCSkeletonAnimation::createWithFile (const char* skeletonDataFile, Atlas* atlas, float scale) {
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
	state = AnimationState_create(AnimationStateData_create(skeleton->data));
}

CCSkeletonAnimation::CCSkeletonAnimation (SkeletonData *skeletonData)
		: CCSkeleton(skeletonData) {
	initialize();
}

CCSkeletonAnimation::CCSkeletonAnimation (const char* skeletonDataFile, Atlas* atlas, float scale)
		: CCSkeleton(skeletonDataFile, atlas, scale) {
	initialize();
}

CCSkeletonAnimation::CCSkeletonAnimation (const char* skeletonDataFile, const char* atlasFile, float scale)
		: CCSkeleton(skeletonDataFile, atlasFile, scale) {
	initialize();
}

CCSkeletonAnimation::~CCSkeletonAnimation () {
	if (ownsAnimationStateData) AnimationStateData_dispose(state->data);
	AnimationState_dispose(state);
}

void CCSkeletonAnimation::update (float deltaTime) {
	super::update(deltaTime);

	deltaTime *= timeScale;
	AnimationState_update(state, deltaTime);
	AnimationState_apply(state, skeleton);
	Skeleton_updateWorldTransform(skeleton);
}

void CCSkeletonAnimation::setAnimationStateData (AnimationStateData* stateData) {
	CCAssert(stateData, "stateData cannot be null.");

	if (ownsAnimationStateData) AnimationStateData_dispose(state->data);
	AnimationState_dispose(state);

	ownsAnimationStateData = true;
	state = AnimationState_create(stateData);
}

void CCSkeletonAnimation::setMix (const char* fromAnimation, const char* toAnimation, float duration) {
	AnimationStateData_setMixByName(state->data, fromAnimation, toAnimation, duration);
}

TrackEntry* CCSkeletonAnimation::setAnimation (int trackIndex, const char* name, bool loop) {
	return AnimationState_setAnimationByName(state, trackIndex, name, loop);
}

TrackEntry* CCSkeletonAnimation::addAnimation (int trackIndex, const char* name, bool loop, float delay) {
	return AnimationState_addAnimationByName(state, trackIndex, name, loop, delay);
}

TrackEntry* CCSkeletonAnimation::getCurrent (int trackIndex) { 
	return AnimationState_getCurrent(state, trackIndex);
}

void CCSkeletonAnimation::clearAnimation () {
	AnimationState_clear(state);
}

void CCSkeletonAnimation::clearAnimation (int trackIndex) {
	AnimationState_clearTrack(state, trackIndex);
}

}
