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

#ifndef SPINE_CCSKELETON_H_
#define SPINE_CCSKELETON_H_

#include <spine/spine.h>
#include "cocos2d.h"

namespace spine {

/**
Draws a skeleton.
*/
class CCSkeleton: public cocos2d::CCNodeRGBA, public cocos2d::CCBlendProtocol {
public:
	Skeleton* skeleton;
	float timeScale;
	bool debugSlots;
	bool debugBones;

	static CCSkeleton* createWithData (SkeletonData* skeletonData);
	static CCSkeleton* createWithFile (const char* skeletonDataFile, Atlas* atlas, float scale = 1);
	static CCSkeleton* createWithFile (const char* skeletonDataFile, const char* atlasFile, float scale = 1);

	CCSkeleton (SkeletonData* skeletonData);
	CCSkeleton (const char* skeletonDataFile, Atlas* atlas, float scale = 1);
	CCSkeleton (const char* skeletonDataFile, const char* atlasFile, float scale = 1);

	virtual ~CCSkeleton ();

	virtual void update (float deltaTime);
	virtual void draw ();
	virtual cocos2d::CCRect boundingBox ();

	// CCBlendProtocol
	CC_PROPERTY(cocos2d::ccBlendFunc, blendFunc, BlendFunc);

private:
	bool ownsSkeletonData;
	Atlas* atlas;
	void initialize (SkeletonData *skeletonData);
};

}

#endif /* SPINE_CCSKELETON_H_ */
