#include <spine/Bone.h>
#include <math.h>
#include <spine/util.h>

static int yDown;

void Bone_setYDown (int value) {
	yDown = value;
}

Bone* Bone_create (BoneData* data, Bone* parent) {
	Bone* self = CALLOC(Bone, 1)
	CAST(BoneData*, self->data) = data;
	CAST(Bone*, self->parent) = parent;
	self->scaleX = 1;
	self->scaleY = 1;
	return self;
}

void Bone_dispose (Bone* self) {
	FREE(self)
}

void Bone_setToBindPose (Bone* self) {
	self->x = self->data->x;
	self->y = self->data->y;
	self->rotation = self->data->rotation;
	self->scaleX = self->data->scaleX;
	self->scaleY = self->data->scaleY;
}

void Bone_updateWorldTransform (Bone* self, int flipX, int flipY) {
	if (self->parent) {
		CAST(float, self->worldX) = self->x * self->parent->m00 + self->y * self->parent->m01 + self->parent->worldX;
		CAST(float, self->worldY) = self->x * self->parent->m10 + self->y * self->parent->m11 + self->parent->worldY;
		CAST(float, self->worldScaleX) = self->parent->worldScaleX * self->scaleX;
		CAST(float, self->worldScaleY) = self->parent->worldScaleY * self->scaleY;
		CAST(float, self->worldRotation) = self->parent->worldRotation + self->rotation;
	} else {
		CAST(float, self->worldX) = self->x;
		CAST(float, self->worldY) = self->y;
		CAST(float, self->worldScaleX) = self->scaleX;
		CAST(float, self->worldScaleY) = self->scaleY;
		CAST(float, self->worldRotation) = self->rotation;
	}
	float radians = (float)(self->worldRotation * 3.1415926535897932385 / 180);
	float cosine = cosf(radians);
	float sine = sinf(radians);
	CAST(float, self->m00) = cosine * self->worldScaleX;
	CAST(float, self->m10) = sine * self->worldScaleX;
	CAST(float, self->m01) = -sine * self->worldScaleY;
	CAST(float, self->m11) = cosine * self->worldScaleY;
	if (flipX) {
		CAST(float, self->m00) = -self->m00;
		CAST(float, self->m01) = -self->m01;
	}
	if (flipY) {
		CAST(float, self->m10) = -self->m10;
		CAST(float, self->m11) = -self->m11;
	}
	if (yDown) {
		CAST(float, self->m10) = -self->m10;
		CAST(float, self->m11) = -self->m11;
	}
}
