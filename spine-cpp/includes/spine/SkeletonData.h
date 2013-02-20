#ifndef SPINE_SKELETONDATA_H_
#define SPINE_SKELETONDATA_H_

#include <string>
#include <vector>

namespace spine {

class BoneData;
class SlotData;
class Skin;

class SkeletonData {
public:
	std::vector<BoneData*> bones;
	std::vector<SlotData*> slots;
	std::vector<Skin*> skins;
	Skin *defaultSkin;

	SkeletonData ();
	~SkeletonData ();

	BoneData* findBone (const std::string &boneName) const;
	int findBoneIndex (const std::string &boneName) const;

	SlotData* findSlot (const std::string &slotName) const;
	int findSlotIndex (const std::string &slotName) const;

	Skin* findSkin (const std::string &skinName);
};

} /* namespace spine */
#endif /* SPINE_SKELETONDATA_H_ */
