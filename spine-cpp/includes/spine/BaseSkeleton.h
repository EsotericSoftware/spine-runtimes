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
	void setSkin (Skin *newSkin);

	Attachment* getAttachment (const std::string &slotName, const std::string &attachmentName);
	Attachment* getAttachment (int slotIndex, const std::string &attachmentName);
	void setAttachment (const std::string &slotName, const std::string &attachmentName);
};

} /* namespace spine */
#endif /* SPINE_BASESKELETON_H_ */
