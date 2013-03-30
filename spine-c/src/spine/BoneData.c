#include <spine/BoneData.h>
#include <spine/util.h>

BoneData* BoneData_create (const char* name, BoneData* parent) {
	BoneData* self = CALLOC(BoneData, 1)
	MALLOC_STR(self->name, name)
	CAST(BoneData*, self->parent) = parent;
	self->scaleX = 1;
	self->scaleY = 1;
	return self;
}

void BoneData_dispose (BoneData* self) {
	FREE(self->name)
	FREE(self)
}
