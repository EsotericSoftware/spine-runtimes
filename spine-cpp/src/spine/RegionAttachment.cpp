/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include <spine/RegionAttachment.h>

#include <spine/Atlas.h>
#include <spine/Bone.h>
#include <spine/MathUtil.h>
#include <spine/Slot.h>
#include <spine/SlotPose.h>

#include <assert.h>

using namespace spine;

RTTI_IMPL(RegionAttachment, Attachment)

const int RegionAttachment::BLX = 0;
const int RegionAttachment::BLY = 1;
const int RegionAttachment::ULX = 2;
const int RegionAttachment::ULY = 3;
const int RegionAttachment::URX = 4;
const int RegionAttachment::URY = 5;
const int RegionAttachment::BRX = 6;
const int RegionAttachment::BRY = 7;

RegionAttachment::RegionAttachment(const String &name, Sequence *sequence)
	: Attachment(name), _sequence(sequence), _x(0), _y(0), _scaleX(1), _scaleY(1), _rotation(0), _width(0), _height(0), _path(), _color(1, 1, 1, 1) {
	assert(sequence);
}

RegionAttachment::~RegionAttachment() {
	delete _sequence;
}

void RegionAttachment::computeWorldVertices(Slot &slot, Array<float> &vertexOffsets, Array<float> &worldVertices, size_t offset, size_t stride) {
	assert(worldVertices.size() >= (offset + 8));
	computeWorldVertices(slot, vertexOffsets.buffer(), worldVertices.buffer(), offset, stride);
}

void RegionAttachment::computeWorldVertices(Slot &slot, float *vertexOffsets, float *worldVertices, size_t offset, size_t stride) {
	BonePose &bone = slot.getBone().getAppliedPose();
	float x = bone.getWorldX(), y = bone.getWorldY();
	float a = bone.getA(), b = bone.getB(), c = bone.getC(), d = bone.getD();

	float offsetX = vertexOffsets[BRX];
	float offsetY = vertexOffsets[BRY];
	worldVertices[offset] = offsetX * a + offsetY * b + x;
	worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
	offset += stride;

	offsetX = vertexOffsets[BLX];
	offsetY = vertexOffsets[BLY];
	worldVertices[offset] = offsetX * a + offsetY * b + x;
	worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
	offset += stride;

	offsetX = vertexOffsets[ULX];
	offsetY = vertexOffsets[ULY];
	worldVertices[offset] = offsetX * a + offsetY * b + x;
	worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
	offset += stride;

	offsetX = vertexOffsets[URX];
	offsetY = vertexOffsets[URY];
	worldVertices[offset] = offsetX * a + offsetY * b + x;
	worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
}

Array<float> &RegionAttachment::getOffsets(SlotPose &pose) {
	return _sequence->getOffsets(_sequence->resolveIndex(pose));
}

float RegionAttachment::getX() {
	return _x;
}

void RegionAttachment::setX(float inValue) {
	_x = inValue;
}

float RegionAttachment::getY() {
	return _y;
}

void RegionAttachment::setY(float inValue) {
	_y = inValue;
}

float RegionAttachment::getScaleX() {
	return _scaleX;
}

void RegionAttachment::setScaleX(float inValue) {
	_scaleX = inValue;
}

float RegionAttachment::getScaleY() {
	return _scaleY;
}

void RegionAttachment::setScaleY(float inValue) {
	_scaleY = inValue;
}

float RegionAttachment::getRotation() {
	return _rotation;
}

void RegionAttachment::setRotation(float inValue) {
	_rotation = inValue;
}

float RegionAttachment::getWidth() {
	return _width;
}

void RegionAttachment::setWidth(float inValue) {
	_width = inValue;
}

float RegionAttachment::getHeight() {
	return _height;
}

void RegionAttachment::setHeight(float inValue) {
	_height = inValue;
}

Sequence &RegionAttachment::getSequence() {
	return *_sequence;
}

void RegionAttachment::updateSequence() {
	_sequence->update(*this);
}

const String &RegionAttachment::getPath() {
	return _path;
}

void RegionAttachment::setPath(const String &inValue) {
	_path = inValue;
}

Color &RegionAttachment::getColor() {
	return _color;
}

Attachment &RegionAttachment::copy() {
	RegionAttachment *copy = new (__FILE__, __LINE__) RegionAttachment(getName(), new (__FILE__, __LINE__) Sequence(*_sequence));
	copy->setTimelineAttachment(getTimelineAttachment());
	copy->setTimelineSlots(getTimelineSlots());
	copy->_path = _path;
	copy->_x = _x;
	copy->_y = _y;
	copy->_scaleX = _scaleX;
	copy->_scaleY = _scaleY;
	copy->_rotation = _rotation;
	copy->_width = _width;
	copy->_height = _height;
	copy->_color.set(_color);
	return *copy;
}

void RegionAttachment::computeUVs(TextureRegion *region, float x, float y, float scaleX, float scaleY, float rotation, float width, float height,
								  Array<float> &offset, Array<float> &uvs) {
	float localX2 = width / 2, localY2 = height / 2;
	float localX = -localX2, localY = -localY2;
	bool rotated = false;
	if (region != NULL && region->getRTTI().instanceOf(AtlasRegion::rtti)) {
		AtlasRegion *r = static_cast<AtlasRegion *>(region);
		localX += r->_offsetX / r->_originalWidth * width;
		localY += r->_offsetY / r->_originalHeight * height;
		if (r->_degrees == 90) {
			rotated = true;
			localX2 -= (r->_originalWidth - r->_offsetX - r->_packedHeight) / r->_originalWidth * width;
			localY2 -= (r->_originalHeight - r->_offsetY - r->_packedWidth) / r->_originalHeight * height;
		} else {
			localX2 -= (r->_originalWidth - r->_offsetX - r->_packedWidth) / r->_originalWidth * width;
			localY2 -= (r->_originalHeight - r->_offsetY - r->_packedHeight) / r->_originalHeight * height;
		}
	}
	localX *= scaleX;
	localY *= scaleY;
	localX2 *= scaleX;
	localY2 *= scaleY;
	float cos = MathUtil::cosDeg(rotation);
	float sin = MathUtil::sinDeg(rotation);
	float localXCos = localX * cos + x;
	float localXSin = localX * sin;
	float localYCos = localY * cos + y;
	float localYSin = localY * sin;
	float localX2Cos = localX2 * cos + x;
	float localX2Sin = localX2 * sin;
	float localY2Cos = localY2 * cos + y;
	float localY2Sin = localY2 * sin;
	offset[BLX] = localXCos - localYSin;
	offset[BLY] = localYCos + localXSin;
	offset[ULX] = localXCos - localY2Sin;
	offset[ULY] = localY2Cos + localXSin;
	offset[URX] = localX2Cos - localY2Sin;
	offset[URY] = localY2Cos + localX2Sin;
	offset[BRX] = localX2Cos - localYSin;
	offset[BRY] = localYCos + localX2Sin;
	if (region == NULL) {
		uvs[BLX] = 0;
		uvs[BLY] = 0;
		uvs[ULX] = 0;
		uvs[ULY] = 1;
		uvs[URX] = 1;
		uvs[URY] = 1;
		uvs[BRX] = 1;
		uvs[BRY] = 0;
	} else {
		uvs[BLX] = region->_u2;
		uvs[ULY] = region->_v2;
		uvs[URX] = region->_u;
		uvs[BRY] = region->_v;
		if (rotated) {
			uvs[BLY] = region->_v;
			uvs[ULX] = region->_u2;
			uvs[URY] = region->_v2;
			uvs[BRX] = region->_u;
		} else {
			uvs[BLY] = region->_v2;
			uvs[ULX] = region->_u;
			uvs[URY] = region->_v;
			uvs[BRX] = region->_u2;
		}
	}
}
