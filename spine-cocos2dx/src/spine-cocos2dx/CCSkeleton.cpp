/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
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
 ******************************************************************************/

#include <spine-cocos2dx/CCSkeleton.h>
#include <stdexcept>
#include <spine-cocos2dx/Skeleton.h>
#include <spine-cocos2dx/RegionAttachment.h>
#include <spine/SkeletonData.h>
#include <spine/AnimationState.h>
#include <spine/AnimationStateData.h>
#include <spine/Slot.h>
#include <spine/BoneData.h>
#include <spine/Bone.h>

using namespace spine;
USING_NS_CC;

CCSkeleton* CCSkeleton::create (SkeletonData* skeletonData) {
	CCSkeleton* skeleton = new CCSkeleton(skeletonData);
	skeleton->autorelease();
	return skeleton;
}

CCSkeleton::CCSkeleton (SkeletonData *skeletonData, AnimationStateData *stateData) :
				debug(false) {
	if (!skeletonData) throw std::invalid_argument("skeletonData cannot be null.");
	skeleton = new Skeleton(skeletonData);
	state = new AnimationState(stateData);

	blendFunc.src = GL_SRC_ALPHA;
	blendFunc.dst = GL_ONE_MINUS_SRC_ALPHA;

	setShaderProgram(CCShaderCache::sharedShaderCache()->programForKey(kCCShader_PositionTextureColor));
	scheduleUpdate();
}

CCSkeleton::~CCSkeleton () {
	delete skeleton;
	delete state;
}

void CCSkeleton::update (float deltaTime) {
	skeleton->update(deltaTime);
	state->update(deltaTime);
	state->apply(skeleton);
	skeleton->updateWorldTransform();
}

void CCSkeleton::draw () {
	CC_NODE_DRAW_SETUP();

	ccGLBlendFunc(blendFunc.src, blendFunc.dst);
	ccColor3B color = getColor();
	skeleton->r = color.r / (float)255;
	skeleton->g = color.g / (float)255;
	skeleton->b = color.b / (float)255;
	skeleton->a = getOpacity() / (float)255;
	skeleton->draw();

	if (debug) {
		// Slots.
		ccDrawColor4B(0, 0, 255, 10);
		glLineWidth(1);
		CCPoint points[4];
		for (int i = 0, n = skeleton->slots.size(); i < n; i++) {
			if (!skeleton->slots[i]->attachment) continue;
			ccV3F_C4B_T2F_Quad quad = ((RegionAttachment*)skeleton->slots[i]->attachment)->quad;
			points[0] = ccp(quad.bl.vertices.x, quad.bl.vertices.y);
			points[1] = ccp(quad.br.vertices.x, quad.br.vertices.y);
			points[2] = ccp(quad.tr.vertices.x, quad.tr.vertices.y);
			points[3] = ccp(quad.tl.vertices.x, quad.tl.vertices.y);
			ccDrawPoly(points, 4, true);
		}
		// Bone lengths.
		glLineWidth(2);
		ccDrawColor4B(255, 0, 0, 255);
		for (int i = 0, n = skeleton->bones.size(); i < n; i++) {
			Bone *bone = skeleton->bones[i];
			float x = bone->data->length * bone->m00 + bone->worldX;
			float y = bone->data->length * bone->m10 + bone->worldY;
			ccDrawLine(ccp(bone->worldX, bone->worldY), ccp(x, y));
		}
		// Bone origins.
		ccPointSize(4);
		ccDrawColor4B(0, 0, 255, 255); // Root bone is blue.
		for (int i = 0, n = skeleton->bones.size(); i < n; i++) {
			Bone *bone = skeleton->bones[i];
			ccDrawPoint(ccp(bone->worldX, bone->worldY));
			if (i == 0) ccDrawColor4B(0, 255, 0, 255);
		}
	}
}

// CCBlendProtocol

ccBlendFunc CCSkeleton::getBlendFunc () {
    return blendFunc;
}

void CCSkeleton::setBlendFunc (ccBlendFunc blendFunc) {
    this->blendFunc = blendFunc;
}
