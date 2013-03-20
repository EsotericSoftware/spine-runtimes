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
	/** The SkeletonData owns the bones. */
	std::vector<BoneData*> bones;
	/** The SkeletonData owns the slots. */
	std::vector<SlotData*> slots;
	/** The SkeletonData owns the skins. */
	std::vector<Skin*> skins;
	/** May be null. */
	Skin *defaultSkin;

	SkeletonData ();
	~SkeletonData ();

	BoneData* findBone (const std::string &boneName) const;
	int findBoneIndex (const std::string &boneName) const;

	SlotData* findSlot (const std::string &slotName) const;
	int findSlotIndex (const std::string &slotName) const;

	Skin* findSkin (const std::string &skinName) const;
};

} /* namespace spine */
#endif /* SPINE_SKELETONDATA_H_ */
