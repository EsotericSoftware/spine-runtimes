#ifndef SPINE_SLOT_H_
#define SPINE_SLOT_H_

#include <spine/Bone.h>
#include <spine/Attachment.h>
#include <spine/SlotData.h>

#ifdef __cplusplus
namespace spine {
extern "C" {
#endif

struct Skeleton;

typedef struct Slot {
	SlotData* const data;
	struct Skeleton* const skeleton;
	Bone* const bone;
	float r, g, b, a;
	Attachment* const attachment;
} Slot;

Slot* Slot_create (SlotData* data, struct Skeleton* skeleton, Bone* bone);
void Slot_dispose (Slot* slot);

/* @param attachment May be null. */
void Slot_setAttachment (Slot* slot, Attachment* attachment);

void Slot_setAttachmentTime (Slot* slot, float time);
float Slot_getAttachmentTime (const Slot* slot);

void Slot_setToBindPose (Slot* slot);

#ifdef __cplusplus
}
}
#endif

#endif /* SPINE_SLOT_H_ */
