#include <spine/BoneData.h>
#include <spine/util.h>

BoneData* BoneData_create (const char* name, BoneData* parent) {
	BoneData* this = calloc(1, sizeof(BoneData));
	MALLOC_STR(this->name, name)
	CAST(BoneData*, this->parent) = parent;
	this->scaleX = 1;
	this->scaleY = 1;
	return this;
}

void BoneData_dispose (BoneData* this) {
	FREE(this->name)
	FREE(this)
}
