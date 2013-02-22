#include <iostream>
#include <spine/BaseSkeleton.h>
#include <spine/SkeletonData.h>
#include <spine/SlotData.h>
#include <spine/Slot.h>
#include <spine/BoneData.h>
#include <spine/Bone.h>
#include <spine/Skin.h>

using std::string;
using std::invalid_argument;

namespace spine {

BaseSkeleton::BaseSkeleton (SkeletonData *data) :
				data(data),
				skin(0),
				r(1),
				g(1),
				b(1),
				a(1),
				time(0),
				flipX(false),
				flipY(false) {
	if (!data) throw invalid_argument("data cannot be null.");

	int boneCount = data->bones.size();
	bones.reserve(boneCount);
	for (int i = 0; i < boneCount; i++) {
		BoneData *boneData = data->bones[i];
		Bone *bone = new Bone(boneData);
		if (boneData->parent) {
			for (int ii = 0; ii < boneCount; ii++) {
				if (data->bones[ii] == boneData->parent) {
					bone->parent = bones[ii];
					break;
				}
			}
		}
		bones.push_back(bone);
	}

	int slotCount = data->slots.size();
	slots.reserve(slotCount);
	drawOrder.reserve(slotCount);
	for (int i = 0; i < slotCount; i++) {
		SlotData *slotData = data->slots[i];
		Bone *bone;
		for (int ii = 0; ii < boneCount; ii++) {
			if (data->bones[ii] == slotData->boneData) {
				bone = bones[ii];
				break;
			}
		}
		Slot *slot = new Slot(slotData, this, bone);
		slots.push_back(slot);
		drawOrder.push_back(slot);
	}
}

BaseSkeleton::~BaseSkeleton () {
	for (int i = 0, n = bones.size(); i < n; i++)
		delete bones[i];
	for (int i = 0, n = slots.size(); i < n; i++)
		delete slots[i];
}

void BaseSkeleton::updateWorldTransform () {
	for (int i = 0, n = bones.size(); i < n; i++)
		bones[i]->updateWorldTransform(flipX, flipY);
}

void BaseSkeleton::setToBindPose () {
	setBonesToBindPose();
	setSlotsToBindPose();
}

void BaseSkeleton::setBonesToBindPose () {
	for (int i = 0, n = bones.size(); i < n; i++)
		bones[i]->setToBindPose();
}

void BaseSkeleton::setSlotsToBindPose () {
	for (int i = 0, n = slots.size(); i < n; i++)
		slots[i]->setToBindPose(i);
}

Bone* BaseSkeleton::getRootBone () const {
	if (bones.size() == 0) return 0;
	return bones[0];
}

Bone* BaseSkeleton::findBone (const string &boneName) const {
	for (int i = 0; i < bones.size(); i++)
		if (data->bones[i]->name == boneName) return bones[i];
	return 0;
}

int BaseSkeleton::findBoneIndex (const string &boneName) const {
	for (int i = 0; i < bones.size(); i++)
		if (data->bones[i]->name == boneName) return i;
	return -1;
}

Slot* BaseSkeleton::findSlot (const string &slotName) const {
	for (int i = 0; i < slots.size(); i++)
		if (data->slots[i]->name == slotName) return slots[i];
	return 0;
}

int BaseSkeleton::findSlotIndex (const string &slotName) const {
	for (int i = 0; i < slots.size(); i++)
		if (data->slots[i]->name == slotName) return i;
	return -1;
}

void BaseSkeleton::setSkin (const string &skinName) {
	Skin *skin = data->findSkin(skinName);
	if (!skin) throw invalid_argument("Skin not found: " + skinName);
	setSkin(skin);
}

void BaseSkeleton::setSkin (Skin *newSkin) {
	if (skin && newSkin) newSkin->attachAll(this, skin);
	skin = newSkin;
}

Attachment* BaseSkeleton::getAttachment (const string &slotName, const string &attachmentName) {
	return getAttachment(data->findSlotIndex(slotName), attachmentName);
}

Attachment* BaseSkeleton::getAttachment (int slotIndex, const string &attachmentName) {
	if (data->defaultSkin) {
		Attachment *attachment = data->defaultSkin->getAttachment(slotIndex, attachmentName);
		if (attachment) return attachment;
	}
	if (skin) return skin->getAttachment(slotIndex, attachmentName);
	return 0;
}

void BaseSkeleton::setAttachment (const string &slotName, const string &attachmentName) {
	for (int i = 0, n = slots.size(); i < n; i++) {
		Slot *slot = slots[i];
		if (slot->data->name == slotName) {
			slot->setAttachment(getAttachment(i, attachmentName));
			return;
		}
	}
	throw invalid_argument("Slot not found: " + slotName);
}

} /* namespace spine */
