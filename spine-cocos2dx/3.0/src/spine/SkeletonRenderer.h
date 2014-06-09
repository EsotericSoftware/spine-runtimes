/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software (typically granted by licensing Spine), you
 * may not (a) modify, translate, adapt or otherwise create derivative works,
 * improvements of the Software or develop new applications using the Software
 * or (b) remove, delete, alter or obscure any trademarks or any copyright,
 * trademark, patent or other intellectual property or proprietary rights
 * notices on or in the Software, including any copy thereof. Redistributions
 * in binary or source form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
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
class SkeletonRenderer: public cocos2d::Node, public cocos2d::BlendProtocol {
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

	virtual void update (float deltaTime) override;
	virtual void draw(cocos2d::Renderer* renderer, const kmMat4& transform, bool transformUpdated) override;
	virtual void drawSkeleton (const kmMat4& transform, bool transformUpdated);
	virtual cocos2d::Rect getBoundingBox () const override;

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
	 * attached if the corresponding attachment from the old skin was attached. Returns false if the skin was not found.
	 * @param skin May be 0.*/
	bool setSkin (const char* skinName);
	
	/* Returns 0 if the slot or attachment was not found. */
	spAttachment* getAttachment (const char* slotName, const char* attachmentName) const;
	/* Returns false if the slot or attachment was not found. */
	bool setAttachment (const char* slotName, const char* attachmentName);

	// --- BlendProtocol
	virtual void setBlendFunc (const cocos2d::BlendFunc& blendFunc);
	virtual const cocos2d::BlendFunc& getBlendFunc () const;
	virtual void setOpacityModifyRGB (bool value);
	virtual bool isOpacityModifyRGB ();

protected:
	SkeletonRenderer ();
	void setSkeletonData (spSkeletonData* skeletonData, bool ownsSkeletonData);
	virtual cocos2d::Texture2D* getTexture (spRegionAttachment* attachment) const;
	virtual cocos2d::Texture2D* getTexture (spMeshAttachment* attachment) const;
	virtual cocos2d::Texture2D* getTexture (spSkinnedMeshAttachment* attachment) const;

private:
	bool ownsSkeletonData;
	spAtlas* atlas;
	cocos2d::CustomCommand drawCommand;
	cocos2d::BlendFunc blendFunc;
	PolygonBatch* batch;
	float* worldVertices;
	void initialize ();
};

}

#endif /* SPINE_SKELETONRENDERER_H_ */
