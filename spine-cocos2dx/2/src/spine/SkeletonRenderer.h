/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#ifndef SPINE_SKELETONRENDERER_H_
#define SPINE_SKELETONRENDERER_H_

#include <spine/spine.h>
#include "cocos2d.h"

namespace spine {

class PolygonBatch;

/** Draws a skeleton. */
class SkeletonRenderer: public cocos2d::CCNodeRGBA, public cocos2d::CCBlendProtocol {
public:
	spSkeleton* skeleton;
	spBone* rootBone;
	float timeScale;
	bool debugSlots;
	bool debugBones;
	bool premultipliedAlpha;

	static SkeletonRenderer* createWithData (spSkeletonData* skeletonData, bool ownsSkeletonData = false);
	static SkeletonRenderer* createWithFile (const char* skeletonDataFile, spAtlas* atlas, float scale = 0);
	static SkeletonRenderer* createWithFile (const char* skeletonDataFile, const char* atlasFile, float scale = 0);

	SkeletonRenderer (spSkeletonData* skeletonData, bool ownsSkeletonData = false);
	SkeletonRenderer (const char* skeletonDataFile, spAtlas* atlas, float scale = 0);
	SkeletonRenderer (const char* skeletonDataFile, const char* atlasFile, float scale = 0);

	virtual ~SkeletonRenderer ();

	virtual void update (float deltaTime);
	virtual void draw ();
	virtual cocos2d::CCRect boundingBox ();

	// --- Convenience methods for common Skeleton_* functions.
	void updateWorldTransform ();

	void setToSetupPose ();
	void setBonesToSetupPose ();
	void setSlotsToSetupPose ();

	/* Returns 0 if the bone was not found. */
	spBone* findBone (const char* boneName) const;
	/* Returns 0 if the slot was not found. */
	spSlot* findSlot (const char* slotName) const;
	
	/* Sets the skin used to look up attachments not found in the SkeletonData defaultSkin. Attachments from the new skin are
	 * attached if the corresponding attachment from the old skin was attached. If there was no old skin, each slot's setup mode
	 * attachment is attached from the new skin. Returns false if the skin was not found.
	 * @param skin May be 0.*/
	bool setSkin (const char* skinName);

	/* Returns 0 if the slot or attachment was not found. */
	spAttachment* getAttachment (const char* slotName, const char* attachmentName) const;
	/* Returns false if the slot or attachment was not found. */
	bool setAttachment (const char* slotName, const char* attachmentName);

	// --- BlendProtocol
	CC_PROPERTY(cocos2d::ccBlendFunc, blendFunc, BlendFunc);
	virtual void setOpacityModifyRGB (bool value);
	virtual bool isOpacityModifyRGB ();

protected:
	SkeletonRenderer ();
	void setSkeletonData (spSkeletonData* skeletonData, bool ownsSkeletonData);

	virtual cocos2d::CCTexture2D* getTexture (spRegionAttachment* attachment) const;
	virtual cocos2d::CCTexture2D* getTexture (spMeshAttachment* attachment) const;
	virtual cocos2d::CCTexture2D* getTexture (spSkinnedMeshAttachment* attachment) const;

private:
	bool ownsSkeletonData;
	spAtlas* atlas;
	PolygonBatch* batch;
	float* worldVertices;
	void initialize ();
};

}

#endif /* SPINE_SKELETONRENDERER_H_ */
