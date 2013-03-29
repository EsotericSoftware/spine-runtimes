#include <spine/Slot.h>
#include <spine/util.h>
#include <spine/Skeleton.h>

typedef struct {
	Slot slot;
	float attachmentTime;
} Private;

Slot* Slot_create (SlotData* data, Skeleton* skeleton, Bone* bone) {
	Private* private = calloc(1, sizeof(Private));
	Slot* this = &private->slot;
	CAST(SlotData*, this->data) = data;
	CAST(Skeleton*, this->skeleton) = skeleton;
	CAST(Bone*, this->bone) = bone;
	this->r = 1;
	this->g = 1;
	this->b = 1;
	this->a = 1;
	return this;
}

void Slot_dispose (Slot* this) {
	FREE(this);
}

/* @param attachment May be null. */
void Slot_setAttachment (Slot* this, Attachment* attachment) {
	CAST(Attachment*, this->attachment) = attachment;
	((Private*)this)->attachmentTime = this->skeleton->time;
}

void Slot_setAttachmentTime (Slot* this, float time) {
	((Private*)this)->attachmentTime = this->skeleton->time - time;
}

float Slot_getAttachmentTime (const Slot* this) {
	return this->skeleton->time - ((Private*)this)->attachmentTime;
}

void Slot_setToBindPose (Slot* this) {
	this->r = this->data->r;
	this->g = this->data->g;
	this->b = this->data->b;
	this->a = this->data->a;

	Attachment* attachment = 0;
	if (this->data->attachmentName) {
		int i;
		for (i = 0; i < this->skeleton->data->slotCount; ++i) {
			if (this->data == this->skeleton->data->slots[i]) {
				attachment = Skeleton_getAttachmentForSlotIndex(this->skeleton, i, this->data->attachmentName);
				break;
			}
		}
	}
	Slot_setAttachment(this, attachment);
}
