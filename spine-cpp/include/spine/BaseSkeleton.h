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

#ifndef SPINE_BASESKELETON_H_
#define SPINE_BASESKELETON_H_

#include <string>
#include <vector>

namespace spine {

class Skin;
class SkeletonData;
class Slot;
class Bone;
class Attachment;

class BaseSkeleton {
public:
	SkeletonData *data;
	std::vector<Bone*> bones;
	std::vector<Slot*> slots;
	std::vector<Slot*> drawOrder;
	Skin *skin;
	float r, g, b, a;
	float time;
	bool flipX, flipY;

	/** The BaseSkeleton owns the SkeletonData. */
	BaseSkeleton (SkeletonData *data);
	virtual ~BaseSkeleton ();

	void updateWorldTransform ();

	void setToBindPose ();
	void setBonesToBindPose ();
	void setSlotsToBindPose ();

	Bone *getRootBone () const;
	Bone* findBone (const std::string &boneName) const;
	int findBoneIndex (const std::string &boneName) const;

	Slot* findSlot (const std::string &slotName) const;
	int findSlotIndex (const std::string &slotName) const;

	void setSkin (const std::string &skinName);
	/** @param skin May be null. */
	void setSkin (Skin *skin);

	Attachment* getAttachment (const std::string &slotName, const std::string &attachmentName) const;
	Attachment* getAttachment (int slotIndex, const std::string &attachmentName) const;
	void setAttachment (const std::string &slotName, const std::string &attachmentName);

	void update (float deltaTime);
};

} /* namespace spine */
#endif /* SPINE_BASESKELETON_H_ */
