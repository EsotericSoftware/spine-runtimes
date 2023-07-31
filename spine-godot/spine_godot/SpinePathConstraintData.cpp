/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include "SpinePathConstraintData.h"
#include "SpineCommon.h"
#include "SpineSkeletonDataResource.h"

void SpinePathConstraintData::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_bones"), &SpinePathConstraintData::get_bones);
	ClassDB::bind_method(D_METHOD("get_target"), &SpinePathConstraintData::get_target);
	ClassDB::bind_method(D_METHOD("set_target", "v"), &SpinePathConstraintData::set_target);
	ClassDB::bind_method(D_METHOD("get_position_mode"), &SpinePathConstraintData::get_position_mode);
	ClassDB::bind_method(D_METHOD("set_position_mode", "v"), &SpinePathConstraintData::set_position_mode);
	ClassDB::bind_method(D_METHOD("get_spacing_mode"), &SpinePathConstraintData::get_spacing_mode);
	ClassDB::bind_method(D_METHOD("set_spacing_mode", "v"), &SpinePathConstraintData::set_spacing_mode);
	ClassDB::bind_method(D_METHOD("get_rotate_mode"), &SpinePathConstraintData::get_rotate_mode);
	ClassDB::bind_method(D_METHOD("set_rotate_mode", "v"), &SpinePathConstraintData::set_rotate_mode);
	ClassDB::bind_method(D_METHOD("get_offset_rotation"), &SpinePathConstraintData::get_offset_rotation);
	ClassDB::bind_method(D_METHOD("set_offset_rotation", "v"), &SpinePathConstraintData::set_offset_rotation);
	ClassDB::bind_method(D_METHOD("get_position"), &SpinePathConstraintData::get_position);
	ClassDB::bind_method(D_METHOD("set_position", "v"), &SpinePathConstraintData::set_position);
	ClassDB::bind_method(D_METHOD("get_spacing"), &SpinePathConstraintData::get_spacing);
	ClassDB::bind_method(D_METHOD("set_spacing", "v"), &SpinePathConstraintData::set_spacing);
	ClassDB::bind_method(D_METHOD("get_mix_rotate"), &SpinePathConstraintData::get_mix_rotate);
	ClassDB::bind_method(D_METHOD("set_mix_rotate", "v"), &SpinePathConstraintData::set_mix_rotate);
	ClassDB::bind_method(D_METHOD("get_mix_x"), &SpinePathConstraintData::get_mix_x);
	ClassDB::bind_method(D_METHOD("set_mix_x", "v"), &SpinePathConstraintData::set_mix_x);
	ClassDB::bind_method(D_METHOD("get_mix_y"), &SpinePathConstraintData::get_mix_y);
	ClassDB::bind_method(D_METHOD("set_mix_y", "v"), &SpinePathConstraintData::set_mix_y);
}

Array SpinePathConstraintData::get_bones() {
	Array result;
	SPINE_CHECK(get_spine_constraint_data(), result)
	auto bones = get_spine_constraint_data()->getBones();
	result.resize((int) bones.size());
	for (int i = 0; i < bones.size(); ++i) {
		Ref<SpineBoneData> bone_ref(memnew(SpineBoneData));
		bone_ref->set_spine_object(get_spine_owner(), bones[i]);
		result[i] = bone_ref;
	}
	return result;
}

Ref<SpineSlotData> SpinePathConstraintData::get_target() {
	SPINE_CHECK(get_spine_constraint_data(), nullptr)
	auto slot = get_spine_constraint_data()->getTarget();
	if (!slot) return nullptr;
	Ref<SpineSlotData> slot_ref(memnew(SpineSlotData));
	slot_ref->set_spine_object(get_spine_owner(), slot);
	return slot_ref;
}

void SpinePathConstraintData::set_target(Ref<SpineSlotData> v) {
	SPINE_CHECK(get_spine_constraint_data(), )
	get_spine_constraint_data()->setTarget(v.is_valid() && v->get_spine_object() ? v->get_spine_object() : nullptr);
}

SpineConstant::PositionMode SpinePathConstraintData::get_position_mode() {
	SPINE_CHECK(get_spine_constraint_data(), SpineConstant::PositionMode_Fixed)
	return (SpineConstant::PositionMode) get_spine_constraint_data()->getPositionMode();
}

void SpinePathConstraintData::set_position_mode(SpineConstant::PositionMode v) {
	SPINE_CHECK(get_spine_constraint_data(), )
	get_spine_constraint_data()->setPositionMode((spine::PositionMode) v);
}

SpineConstant::SpacingMode SpinePathConstraintData::get_spacing_mode() {
	SPINE_CHECK(get_spine_constraint_data(), SpineConstant::SpacingMode_Fixed)
	return (SpineConstant::SpacingMode) get_spine_constraint_data()->getSpacingMode();
}

void SpinePathConstraintData::set_spacing_mode(SpineConstant::SpacingMode v) {
	SPINE_CHECK(get_spine_constraint_data(), )
	get_spine_constraint_data()->setSpacingMode((spine::SpacingMode) v);
}

SpineConstant::RotateMode SpinePathConstraintData::get_rotate_mode() {
	SPINE_CHECK(get_spine_constraint_data(), SpineConstant::RotateMode_Tangent)
	return (SpineConstant::RotateMode) get_spine_constraint_data()->getRotateMode();
}

void SpinePathConstraintData::set_rotate_mode(SpineConstant::RotateMode v) {
	SPINE_CHECK(get_spine_constraint_data(), )
	get_spine_constraint_data()->setRotateMode((spine::RotateMode) v);
}

float SpinePathConstraintData::get_offset_rotation() {
	SPINE_CHECK(get_spine_constraint_data(), 0)
	return get_spine_constraint_data()->getOffsetRotation();
}

void SpinePathConstraintData::set_offset_rotation(float v) {
	SPINE_CHECK(get_spine_constraint_data(), )
	get_spine_constraint_data()->setOffsetRotation(v);
}

float SpinePathConstraintData::get_position() {
	SPINE_CHECK(get_spine_constraint_data(), 0)
	return get_spine_constraint_data()->getPosition();
}

void SpinePathConstraintData::set_position(float v) {
	SPINE_CHECK(get_spine_constraint_data(), )
	get_spine_constraint_data()->setPosition(v);
}

float SpinePathConstraintData::get_spacing() {
	SPINE_CHECK(get_spine_constraint_data(), 0)
	return get_spine_constraint_data()->getSpacing();
}

void SpinePathConstraintData::set_spacing(float v) {
	SPINE_CHECK(get_spine_constraint_data(), )
	get_spine_constraint_data()->setSpacing(v);
}

float SpinePathConstraintData::get_mix_rotate() {
	SPINE_CHECK(get_spine_constraint_data(), 0)
	return get_spine_constraint_data()->getMixRotate();
}

void SpinePathConstraintData::set_mix_rotate(float v) {
	SPINE_CHECK(get_spine_constraint_data(), )
	get_spine_constraint_data()->setMixRotate(v);
}

float SpinePathConstraintData::get_mix_x() {
	SPINE_CHECK(get_spine_constraint_data(), 0)
	return get_spine_constraint_data()->getMixX();
}

void SpinePathConstraintData::set_mix_x(float v) {
	SPINE_CHECK(get_spine_constraint_data(), )
	get_spine_constraint_data()->setMixX(v);
}

float SpinePathConstraintData::get_mix_y() {
	SPINE_CHECK(get_spine_constraint_data(), 0)
	return get_spine_constraint_data()->getMixY();
}

void SpinePathConstraintData::set_mix_y(float v) {
	SPINE_CHECK(get_spine_constraint_data(), )
	get_spine_constraint_data()->setMixY(v);
}
