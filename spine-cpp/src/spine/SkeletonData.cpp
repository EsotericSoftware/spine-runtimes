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
	for (int i = 0; i < bones.size(); i++)
		if (bones[i]->name == boneName) return bones[i];
	return 0;
}

int SkeletonData::findBoneIndex (const string &boneName) const {
	for (int i = 0; i < bones.size(); i++)
		if (bones[i]->name == boneName) return i;
	return -1;
}

SlotData* SkeletonData::findSlot (const string &slotName) const {
	for (int i = 0; i < slots.size(); i++)
		if (slots[i]->name == slotName) return slots[i];
	return 0;
}

int SkeletonData::findSlotIndex (const string &slotName) const {
	for (int i = 0; i < slots.size(); i++)
		if (slots[i]->name == slotName) return i;
	return -1;
}

Skin* SkeletonData::findSkin (const string &skinName) {
	for (int i = 0; i < skins.size(); i++)
		if (skins[i]->name == skinName) return skins[i];
	return 0;
}

} /* namespace spine */
