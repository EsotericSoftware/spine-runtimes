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

#include <spine/SkeletonData.h>
#include <spine/BoneData.h>
#include <spine/SlotData.h>
#include <spine/Skin.h>

using std::string;

namespace spine {

SkeletonData::SkeletonData () :
				defaultSkin(0) {
}

SkeletonData::~SkeletonData () {
	for (int i = 0, n = bones.size(); i < n; i++)
		delete bones[i];
	for (int i = 0, n = slots.size(); i < n; i++)
		delete slots[i];
	for (int i = 0, n = skins.size(); i < n; i++)
		delete skins[i];
}

BoneData* SkeletonData::findBone (const string &boneName) const {
	for (int i = 0, n = bones.size(); i < n; i++)
		if (bones[i]->name == boneName) return bones[i];
	return 0;
}

int SkeletonData::findBoneIndex (const string &boneName) const {
	for (int i = 0, n = bones.size(); i < n; i++)
		if (bones[i]->name == boneName) return i;
	return -1;
}

SlotData* SkeletonData::findSlot (const string &slotName) const {
	for (int i = 0, n = slots.size(); i < n; i++)
		if (slots[i]->name == slotName) return slots[i];
	return 0;
}

int SkeletonData::findSlotIndex (const string &slotName) const {
	for (int i = 0, n = slots.size(); i < n; i++)
		if (slots[i]->name == slotName) return i;
	return -1;
}

Skin* SkeletonData::findSkin (const string &skinName) const {
	for (int i = 0, n = skins.size(); i < n; i++)
		if (skins[i]->name == skinName) return skins[i];
	return 0;
}

} /* namespace spine */
