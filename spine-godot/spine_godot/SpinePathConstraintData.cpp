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

#include "SpinePathConstraintData.h"
#include "SpinePathConstraintPose.h"
#include "SpineCommon.h"
#include "SpineSkeletonDataResource.h"

void SpinePathConstraintData::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_bones"), &SpinePathConstraintData::get_bones);
	ClassDB::bind_method(D_METHOD("get_slot"), &SpinePathConstraintData::get_slot);
	ClassDB::bind_method(D_METHOD("set_slot", "v"), &SpinePathConstraintData::set_slot);
	ClassDB::bind_method(D_METHOD("get_position_mode"), &SpinePathConstraintData::get_position_mode);
	ClassDB::bind_method(D_METHOD("set_position_mode", "v"), &SpinePathConstraintData::set_position_mode);
	ClassDB::bind_method(D_METHOD("get_spacing_mode"), &SpinePathConstraintData::get_spacing_mode);
	ClassDB::bind_method(D_METHOD("set_spacing_mode", "v"), &SpinePathConstraintData::set_spacing_mode);
	ClassDB::bind_method(D_METHOD("get_rotate_mode"), &SpinePathConstraintData::get_rotate_mode);
	ClassDB::bind_method(D_METHOD("set_rotate_mode", "v"), &SpinePathConstraintData::set_rotate_mode);
	ClassDB::bind_method(D_METHOD("get_offset_rotation"), &SpinePathConstraintData::get_offset_rotation);
	ClassDB::bind_method(D_METHOD("set_offset_rotation", "v"), &SpinePathConstraintData::set_offset_rotation);
	ClassDB::bind_method(D_METHOD("get_setup_pose"), &SpinePathConstraintData::get_setup_pose);

	ADD_PROPERTY(PropertyInfo(Variant::ARRAY, "bones", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "", "get_bones");
	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "slot", PROPERTY_HINT_RESOURCE_TYPE, "SpineSlotData", PROPERTY_USAGE_NONE), "set_slot", "get_slot");
	ADD_PROPERTY(PropertyInfo(Variant::INT, "position_mode", PROPERTY_HINT_ENUM, "Fixed,Percent", PROPERTY_USAGE_NONE), "set_position_mode",
				 "get_position_mode");
	ADD_PROPERTY(PropertyInfo(Variant::INT, "spacing_mode", PROPERTY_HINT_ENUM, "Length,Fixed,Percent,Proportional", PROPERTY_USAGE_NONE),
				 "set_spacing_mode", "get_spacing_mode");
	ADD_PROPERTY(PropertyInfo(Variant::INT, "rotate_mode", PROPERTY_HINT_ENUM, "Tangent,Chain,Chain Scale", PROPERTY_USAGE_NONE), "set_rotate_mode",
				 "get_rotate_mode");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "offset_rotation", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_offset_rotation",
				 "get_offset_rotation");
	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "setup_pose", PROPERTY_HINT_RESOURCE_TYPE, "SpinePathConstraintPose", PROPERTY_USAGE_NONE), "",
				 "get_setup_pose");
}

Array SpinePathConstraintData::get_bones() {
	Array result;
	SPINE_CHECK(get_spine_constraint_data(), result)
	auto &bones = get_spine_constraint_data()->getBones();
	result.resize((int) bones.size());
	for (int i = 0; i < bones.size(); ++i) {
		Ref<SpineBoneData> bone_ref(memnew(SpineBoneData));
		bone_ref->set_spine_object(get_spine_owner(), bones[i]);
		result[i] = bone_ref;
	}
	return result;
}

Ref<SpineSlotData> SpinePathConstraintData::get_slot() {
	SPINE_CHECK(get_spine_constraint_data(), nullptr)
	auto slot = &get_spine_constraint_data()->getSlot();
	if (!slot) return nullptr;
	Ref<SpineSlotData> slot_ref(memnew(SpineSlotData));
	slot_ref->set_spine_object(get_spine_owner(), slot);
	return slot_ref;
}

void SpinePathConstraintData::set_slot(Ref<SpineSlotData> v) {
	SPINE_CHECK(get_spine_constraint_data(), )
	if (v.is_valid() && v->get_spine_object()) {
		get_spine_constraint_data()->setSlot(*v->get_spine_object());
	}
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

Ref<SpinePathConstraintPose> SpinePathConstraintData::get_setup_pose() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto &pose = get_spine_constraint_data()->getSetupPose();
	Ref<SpinePathConstraintPose> pose_ref(memnew(SpinePathConstraintPose));
	pose_ref->set_spine_object(get_spine_owner(), &pose);
	return pose_ref;
}
