#ifndef SPINE_SLOT_H_
#define SPINE_SLOT_H_

namespace spine {

class BaseSkeleton;
class SlotData;
class Bone;
class Attachment;

class Slot {
	friend class BaseSkeleton;

private:
	float attachmentTime;

	void setToBindPose (int slotIndex);

public:
	SlotData *data;
	BaseSkeleton *skeleton;
	Bone *bone;
	float r, g, b, a;
	Attachment *attachment;

	Slot (SlotData *data, BaseSkeleton *skeleton, Bone *bone);

	void setAttachment (Attachment *attachment);

	void setAttachmentTime (float time);
	float getAttachmentTime () const;

	void setToBindPose ();
};

} /* namespace spine */
#endif /* SPINE_SLOT_H_ */
