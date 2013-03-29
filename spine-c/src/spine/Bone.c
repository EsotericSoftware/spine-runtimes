#include <spine/Bone.h>
#include <math.h>
#include <spine/util.h>

static int yDown;

void Bone_setYDown (int value) {
	yDown = value;
}

Bone* Bone_create (BoneData* data, Bone* parent) {
	Bone* this = calloc(1, sizeof(Bone));
	CAST(BoneData*, this->data) = data;
	CAST(Bone*, this->parent) = parent;
	this->scaleX = 1;
	this->scaleY = 1;
	return this;
}

void Bone_dispose (Bone* this) {
	FREE(this)
}

void Bone_setToBindPose (Bone* this) {
	this->x = this->data->x;
	this->y = this->data->y;
	this->rotation = this->data->rotation;
	this->scaleX = this->data->scaleX;
	this->scaleY = this->data->scaleY;
}

void Bone_updateWorldTransform (Bone* this, int flipX, int flipY) {
	if (this->parent) {
		CAST(float, this->worldX) = this->x * this->parent->m00 + this->y * this->parent->m01 + this->parent->worldX;
		CAST(float, this->worldY) = this->x * this->parent->m10 + this->y * this->parent->m11 + this->parent->worldY;
		CAST(float, this->worldScaleX) = this->parent->worldScaleX * this->scaleX;
		CAST(float, this->worldScaleY) = this->parent->worldScaleY * this->scaleY;
		CAST(float, this->worldRotation) = this->parent->worldRotation + this->rotation;
	} else {
		CAST(float, this->worldX) = this->x;
		CAST(float, this->worldY) = this->y;
		CAST(float, this->worldScaleX) = this->scaleX;
		CAST(float, this->worldScaleY) = this->scaleY;
		CAST(float, this->worldRotation) = this->rotation;
	}
	float radians = (float)(this->worldRotation * 3.1415926535897932385 / 180);
	float cosine = cos(radians);
	float sine = sin(radians);
	CAST(float, this->m00) = cosine * this->worldScaleX;
	CAST(float, this->m10) = sine * this->worldScaleX;
	CAST(float, this->m01) = -sine * this->worldScaleY;
	CAST(float, this->m11) = cosine * this->worldScaleY;
	if (flipX) {
		CAST(float, this->m00) = -this->m00;
		CAST(float, this->m01) = -this->m01;
	}
	if (flipY) {
		CAST(float, this->m10) = -this->m10;
		CAST(float, this->m11) = -this->m11;
	}
	if (yDown) {
		CAST(float, this->m10) = -this->m10;
		CAST(float, this->m11) = -this->m11;
	}
}
