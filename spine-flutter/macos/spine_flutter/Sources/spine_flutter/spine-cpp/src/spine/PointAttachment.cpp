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

#include <spine/PointAttachment.h>

#include <spine/Bone.h>

#include <spine/MathUtil.h>

using namespace spine;

RTTI_IMPL(PointAttachment, Attachment)

PointAttachment::PointAttachment(const String &name) : Attachment(name), _x(0), _y(0), _rotation(0), _color(0.9451f, 0.9451f, 0, 1) {
}

float PointAttachment::getX() {
	return _x;
}

void PointAttachment::setX(float inValue) {
	_x = inValue;
}

float PointAttachment::getY() {
	return _y;
}

void PointAttachment::setY(float inValue) {
	_y = inValue;
}

float PointAttachment::getRotation() {
	return _rotation;
}

void PointAttachment::setRotation(float inValue) {
	_rotation = inValue;
}

Color &PointAttachment::getColor() {
	return _color;
}

void PointAttachment::computeWorldPosition(BonePose &bone, float &ox, float &oy) {
	ox = _x * bone.getA() + _y * bone.getB() + bone.getWorldX();
	oy = _x * bone.getC() + _y * bone.getD() + bone.getWorldY();
}

float PointAttachment::computeWorldRotation(BonePose &bone) {
	float r = _rotation * MathUtil::Deg_Rad, cosine = MathUtil::cos(r), sine = MathUtil::sin(r);
	float x = cosine * bone.getA() + sine * bone.getB();
	float y = cosine * bone.getC() + sine * bone.getD();
	return MathUtil::atan2Deg(y, x);
}

Attachment &PointAttachment::copy() {
	PointAttachment *copy = new (__FILE__, __LINE__) PointAttachment(getName());
	copy->setTimelineAttachment(getTimelineAttachment());
	copy->setTimelineSlots(getTimelineSlots());
	copy->_x = _x;
	copy->_y = _y;
	copy->_rotation = _rotation;
	copy->_color.set(_color);
	return *copy;
}
