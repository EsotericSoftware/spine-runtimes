#include <spine/Slot.h>
#include <spine/util.h>
#include <spine/Skeleton.h>

typedef struct {
	Slot slot;
	float attachmentTime;
} Internal;

Slot* Slot_create (SlotData* data, Skeleton* skeleton, Bone* bone) {
	Internal* internal = CALLOC(Internal, 1)
	Slot* self = &internal->slot;
	CAST(SlotData*, self->data) = data;
	CAST(Skeleton*, self->skeleton) = skeleton;
	CAST(Bone*, self->bone) = bone;
	self->r = 1;
	self->g = 1;
	self->b = 1;
	self->a = 1;
	return self;
}

void Slot_dispose (Slot* self) {
	FREE(self);
}

/* @param attachment May be null. */
void Slot_setAttachment (Slot* self, Attachment* attachment) {
	CAST(Attachment*, self->attachment) = attachment;
	((Internal*)self)->attachmentTime = self->skeleton->time;
}

void Slot_setAttachmentTime (Slot* self, float time) {
	((Internal*)self)->attachmentTime = self->skeleton->time - time;
}

float Slot_getAttachmentTime (const Slot* self) {
	return self->skeleton->time - ((Internal*)self)->attachmentTime;
}

void Slot_setToBindPose (Slot* self) {
	self->r = self->data->r;
	self->g = self->data->g;
	self->b = self->data->b;
	self->a = self->data->a;

	Attachment* attachment = 0;
	if (self->data->attachmentName) {
		int i;
		for (i = 0; i < self->skeleton->data->slotCount; ++i) {
			if (self->data == self->skeleton->data->slots[i]) {
				attachment = Skeleton_getAttachmentForSlotIndex(self->skeleton, i, self->data->attachmentName);
				break;
			}
		}
	}
	Slot_setAttachment(self, attachment);
}
