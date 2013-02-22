#include <math.h>
#include <stdexcept>
#include <spine/Bone.h>
#include <spine/BoneData.h>

namespace spine {

Bone::Bone (BoneData *data) :
				data(data),
				parent(0),
				x(data->x),
				y(data->y),
				rotation(data->rotation),
				scaleX(data->scaleX),
				scaleY(data->scaleY) {
	if (!data) throw std::invalid_argument("data cannot be null.");
}

void Bone::setToBindPose () {
	x = data->x;
	y = data->y;
	rotation = data->rotation;
	scaleX = data->scaleX;
	scaleY = data->scaleY;
}

void Bone::updateWorldTransform (bool flipX, bool flipY) {
	if (parent) {
		worldX = x * parent->m00 + y * parent->m01 + parent->worldX;
		worldY = x * parent->m10 + y * parent->m11 + parent->worldY;
		worldScaleX = parent->worldScaleX * scaleX;
		worldScaleY = parent->worldScaleY * scaleY;
		worldRotation = parent->worldRotation + rotation;
	} else {
		worldX = x;
		worldY = y;
		worldScaleX = scaleX;
		worldScaleY = scaleY;
		worldRotation = rotation;
	}
	float radians = worldRotation * M_PI / 180;
	float cos = cosf(radians);
	float sin = sinf(radians);
	m00 = cos * worldScaleX;
	m10 = sin * worldScaleX;
	m01 = -sin * worldScaleY;
	m11 = cos * worldScaleY;
	if (flipX) {
		m00 = -m00;
		m01 = -m01;
	}
	if (flipY) {
		m10 = -m10;
		m11 = -m11;
	}
	if (data->flipY) {
		m10 = -m10;
		m11 = -m11;
	}
}

} /* namespace spine */
