/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

#include "SpinePathConstraintData.h"


void SpinePathConstraintData::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_all_bone_data"), &SpinePathConstraintData::get_bones);
	ClassDB::bind_method(D_METHOD("get_target"), &SpinePathConstraintData::get_target);
	ClassDB::bind_method(D_METHOD("set_target", "V"), &SpinePathConstraintData::set_target);
	ClassDB::bind_method(D_METHOD("get_position_mode"), &SpinePathConstraintData::get_position_mode);
	ClassDB::bind_method(D_METHOD("set_position_mode", "V"), &SpinePathConstraintData::set_position_mode);
	ClassDB::bind_method(D_METHOD("get_spacing_mode"), &SpinePathConstraintData::get_spacing_mode);
	ClassDB::bind_method(D_METHOD("set_spacing_mode", "V"), &SpinePathConstraintData::set_spacing_mode);
	ClassDB::bind_method(D_METHOD("get_rotate_mode"), &SpinePathConstraintData::get_rotate_mode);
	ClassDB::bind_method(D_METHOD("set_rotate_mode", "V"), &SpinePathConstraintData::set_rotate_mode);
	ClassDB::bind_method(D_METHOD("get_offset_rotation"), &SpinePathConstraintData::get_offset_rotation);
	ClassDB::bind_method(D_METHOD("set_offset_rotation", "V"), &SpinePathConstraintData::set_offset_rotation);
	ClassDB::bind_method(D_METHOD("get_position"), &SpinePathConstraintData::get_position);
	ClassDB::bind_method(D_METHOD("set_position", "V"), &SpinePathConstraintData::set_position);
	ClassDB::bind_method(D_METHOD("get_spacing"), &SpinePathConstraintData::get_spacing);
	ClassDB::bind_method(D_METHOD("set_spacing", "V"), &SpinePathConstraintData::set_spacing);
	ClassDB::bind_method(D_METHOD("get_mix_rotate"), &SpinePathConstraintData::get_mix_rotate);
	ClassDB::bind_method(D_METHOD("set_mix_rotate", "V"), &SpinePathConstraintData::set_mix_rotate);
	ClassDB::bind_method(D_METHOD("get_mix_x"), &SpinePathConstraintData::get_mix_x);
	ClassDB::bind_method(D_METHOD("set_mix_x", "V"), &SpinePathConstraintData::set_mix_x);
	ClassDB::bind_method(D_METHOD("get_mix_y"), &SpinePathConstraintData::get_mix_y);
	ClassDB::bind_method(D_METHOD("set_mix_y", "V"), &SpinePathConstraintData::set_mix_y);

	BIND_ENUM_CONSTANT(POSITIONMODE_FIXED);
	BIND_ENUM_CONSTANT(POSITIONMODE_PERCENT);

	BIND_ENUM_CONSTANT(SPACINGMODE_LENGTH);
	BIND_ENUM_CONSTANT(SPACINGMODE_FIXED);
	BIND_ENUM_CONSTANT(SPACINGMODE_PERCENT);

	BIND_ENUM_CONSTANT(ROTATEMODE_TANGENT);
	BIND_ENUM_CONSTANT(ROTATEMODE_CHAIN);
	BIND_ENUM_CONSTANT(ROTATEMODE_CHAINSCALE);
}

SpinePathConstraintData::SpinePathConstraintData() {}
SpinePathConstraintData::~SpinePathConstraintData() {}

Array SpinePathConstraintData::get_bones() {
	auto bs = get_spine_constraint_data()->getBones();
	Array gd_bs;
	gd_bs.resize(bs.size());
	for (size_t i = 0; i < bs.size(); ++i) {
		if (bs[i] == NULL) gd_bs[i] = Ref<SpineBoneData>(NULL);
		else {
			Ref<SpineBoneData> gd_b(memnew(SpineBoneData));
			gd_b->set_spine_object(bs[i]);
			gd_bs[i] = gd_b;
		}
	}
	return gd_bs;
}

Ref<SpineSlotData> SpinePathConstraintData::get_target() {
	auto s = get_spine_constraint_data()->getTarget();
	if (s == NULL) return NULL;
	Ref<SpineSlotData> gd_s(memnew(SpineSlotData));
	gd_s->set_spine_object(s);
	return gd_s;
}
void SpinePathConstraintData::set_target(Ref<SpineSlotData> v) {
	if (v.is_valid()) {
		get_spine_constraint_data()->setTarget(v->get_spine_object());
	} else {
		get_spine_constraint_data()->setTarget(NULL);
	}
}

SpinePathConstraintData::PositionMode SpinePathConstraintData::get_position_mode() {
	auto m = (int) get_spine_constraint_data()->getPositionMode();
	return (PositionMode) m;
}
void SpinePathConstraintData::set_position_mode(PositionMode v) {
	auto m = (int) v;
	get_spine_constraint_data()->setPositionMode((spine::PositionMode) m);
}

SpinePathConstraintData::SpacingMode SpinePathConstraintData::get_spacing_mode() {
	auto m = (int) get_spine_constraint_data()->getSpacingMode();
	return (SpacingMode) m;
}
void SpinePathConstraintData::set_spacing_mode(SpacingMode v) {
	auto m = (int) v;
	get_spine_constraint_data()->setSpacingMode((spine::SpacingMode) m);
}

SpinePathConstraintData::RotateMode SpinePathConstraintData::get_rotate_mode() {
	auto m = (int) get_spine_constraint_data()->getRotateMode();
	return (RotateMode) m;
}
void SpinePathConstraintData::set_rotate_mode(RotateMode v) {
	auto m = (int) v;
	get_spine_constraint_data()->setRotateMode((spine::RotateMode) m);
}

float SpinePathConstraintData::get_offset_rotation() {
	return get_spine_constraint_data()->getOffsetRotation();
}
void SpinePathConstraintData::set_offset_rotation(float v) {
	get_spine_constraint_data()->setOffsetRotation(v);
}

float SpinePathConstraintData::get_position() {
	return get_spine_constraint_data()->getPosition();
}
void SpinePathConstraintData::set_position(float v) {
	get_spine_constraint_data()->setPosition(v);
}

float SpinePathConstraintData::get_spacing() {
	return get_spine_constraint_data()->getSpacing();
}
void SpinePathConstraintData::set_spacing(float v) {
	get_spine_constraint_data()->setSpacing(v);
}

float SpinePathConstraintData::get_mix_rotate() {
	return get_spine_constraint_data()->getMixRotate();
}
void SpinePathConstraintData::set_mix_rotate(float v) {
	get_spine_constraint_data()->setMixRotate(v);
}

float SpinePathConstraintData::get_mix_x() {
	return get_spine_constraint_data()->getMixX();
}
void SpinePathConstraintData::set_mix_x(float v) {
	get_spine_constraint_data()->setMixX(v);
}

float SpinePathConstraintData::get_mix_y() {
	return get_spine_constraint_data()->getMixY();
}
void SpinePathConstraintData::set_mix_y(float v) {
	get_spine_constraint_data()->setMixY(v);
}