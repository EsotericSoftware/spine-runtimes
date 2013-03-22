/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ******************************************************************************/

#include <math.h>
#include <stdexcept>
#include <spine/Bone.h>
#include <spine/BoneData.h>

#ifndef M_PI
#define M_PI 3.1415926535897932385
#endif

namespace spine {

Bone::Bone (BoneData *data) :
				data(data),
				parent(0),
				x(data->x),
				y(data->y),
				rotation(data->rotation),
				scaleX(data->scaleX),
				scaleY(data->scaleY),
				m00(0),
				m01(0),
				worldX(0),
				m10(0),
				m11(0),
				worldY(0),
				worldRotation(0),
				worldScaleX(0),
				worldScaleY(0) {
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
	float radians = (float)(worldRotation * M_PI / 180);
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
	if (data->yDown) {
		m10 = -m10;
		m11 = -m11;
	}
}

} /* namespace spine */
