#include <spine/SlotData.h>
#include <spine/util.h>

SlotData* SlotData_create (const char* name, BoneData* boneData) {
	SlotData* self = CALLOC(SlotData, 1)
	MALLOC_STR(self->name, name)
	CAST(BoneData*, self->boneData) = boneData;
	self->r = 1;
	self->g = 1;
	self->b = 1;
	self->a = 1;
	return self;
}

void SlotData_dispose (SlotData* self) {
	FREE(self->name);
	FREE(self->attachmentName);
	FREE(self);
}

void SlotData_setAttachmentName (SlotData* self, const char* attachmentName) {
	FREE(self->attachmentName);
	if (attachmentName)
		MALLOC_STR(self->attachmentName, attachmentName)
	else
		CAST(char*, self->attachmentName) = 0;
}
