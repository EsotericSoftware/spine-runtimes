#include <spine/SlotData.h>
#include <spine/util.h>

SlotData* SlotData_create (const char* name, BoneData* boneData) {
	SlotData* this = calloc(1, sizeof(SlotData));
	MALLOC_STR(this->name, name)
	CAST(BoneData*, this->boneData) = boneData;
	this->r = 1;
	this->g = 1;
	this->b = 1;
	this->a = 1;
	return this;
}

void SlotData_dispose (SlotData* this) {
	FREE(this->name);
	FREE(this->attachmentName);
	FREE(this);
}

void SlotData_setAttachmentName (SlotData* this, const char* attachmentName) {
	FREE(this->attachmentName);
	if (attachmentName)
		MALLOC_STR(this->attachmentName, attachmentName)
	else
		CAST(char*, this->attachmentName) = 0;
}
