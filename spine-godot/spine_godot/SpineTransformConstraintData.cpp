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

#include "SpineTransformConstraintData.h"
#include "SpineTransformConstraintPose.h"
#include "SpineCommon.h"

void SpineTransformConstraintData::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_bones"), &SpineTransformConstraintData::get_bones);
	ClassDB::bind_method(D_METHOD("get_source"), &SpineTransformConstraintData::get_source);
	ClassDB::bind_method(D_METHOD("get_offset_rotation"), &SpineTransformConstraintData::get_offset_rotation);
	ClassDB::bind_method(D_METHOD("get_offset_x"), &SpineTransformConstraintData::get_offset_x);
	ClassDB::bind_method(D_METHOD("get_offset_y"), &SpineTransformConstraintData::get_offset_y);
	ClassDB::bind_method(D_METHOD("get_offset_scale_x"), &SpineTransformConstraintData::get_offset_scale_x);
	ClassDB::bind_method(D_METHOD("get_offset_scale_y"), &SpineTransformConstraintData::get_offset_scale_y);
	ClassDB::bind_method(D_METHOD("get_offset_shear_y"), &SpineTransformConstraintData::get_offset_shear_y);
	ClassDB::bind_method(D_METHOD("is_local_source"), &SpineTransformConstraintData::is_local_source);
	ClassDB::bind_method(D_METHOD("is_local_target"), &SpineTransformConstraintData::is_local_target);
	ClassDB::bind_method(D_METHOD("get_setup_pose"), &SpineTransformConstraintData::get_setup_pose);

	ADD_PROPERTY(PropertyInfo(Variant::ARRAY, "bones", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "", "get_bones");
	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "source", PROPERTY_HINT_RESOURCE_TYPE, "SpineBoneData", PROPERTY_USAGE_NONE), "", "get_source");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "offset_rotation", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "", "get_offset_rotation");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "offset_x", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "", "get_offset_x");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "offset_y", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "", "get_offset_y");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "offset_scale_x", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "", "get_offset_scale_x");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "offset_scale_y", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "", "get_offset_scale_y");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "offset_shear_y", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "", "get_offset_shear_y");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "local_source", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "", "is_local_source");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "local_target", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "", "is_local_target");
	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "setup_pose", PROPERTY_HINT_RESOURCE_TYPE, "SpineTransformConstraintPose", PROPERTY_USAGE_NONE), "",
				 "get_setup_pose");
}

Array SpineTransformConstraintData::get_bones() {
	Array result;
	SPINE_CHECK(get_spine_constraint_data(), result)
	auto &bones = get_spine_constraint_data()->getBones();
	result.resize((int) bones.size());
	for (int i = 0; i < (int) bones.size(); ++i) {
		Ref<SpineBoneData> bone_ref(memnew(SpineBoneData));
		bone_ref->set_spine_object(get_spine_owner(), bones[i]);
		result[i] = bone_ref;
	}
	return result;
}

Ref<SpineBoneData> SpineTransformConstraintData::get_source() {
	SPINE_CHECK(get_spine_constraint_data(), nullptr)
	auto bone = &get_spine_constraint_data()->getSource();
	if (!bone) return nullptr;
	Ref<SpineBoneData> slot_ref(memnew(SpineBoneData));
	slot_ref->set_spine_object(get_spine_owner(), bone);
	return slot_ref;
}

float SpineTransformConstraintData::get_offset_rotation() {
	SPINE_CHECK(get_spine_constraint_data(), 0)
	return get_spine_constraint_data()->getOffsetRotation();
}

float SpineTransformConstraintData::get_offset_x() {
	SPINE_CHECK(get_spine_constraint_data(), 0)
	return get_spine_constraint_data()->getOffsetX();
}

float SpineTransformConstraintData::get_offset_y() {
	SPINE_CHECK(get_spine_constraint_data(), 0)
	return get_spine_constraint_data()->getOffsetY();
}

float SpineTransformConstraintData::get_offset_scale_x() {
	SPINE_CHECK(get_spine_constraint_data(), 0)
	return get_spine_constraint_data()->getOffsetScaleX();
}

float SpineTransformConstraintData::get_offset_scale_y() {
	SPINE_CHECK(get_spine_constraint_data(), 0)
	return get_spine_constraint_data()->getOffsetScaleY();
}

float SpineTransformConstraintData::get_offset_shear_y() {
	SPINE_CHECK(get_spine_constraint_data(), 0)
	return get_spine_constraint_data()->getOffsetShearY();
}

bool SpineTransformConstraintData::is_local_source() {
	SPINE_CHECK(get_spine_constraint_data(), false)
	return get_spine_constraint_data()->getLocalSource();
}

bool SpineTransformConstraintData::is_local_target() {
	SPINE_CHECK(get_spine_constraint_data(), false)
	return get_spine_constraint_data()->getLocalTarget();
}

Ref<SpineTransformConstraintPose> SpineTransformConstraintData::get_setup_pose() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto &pose = get_spine_constraint_data()->getSetupPose();
	Ref<SpineTransformConstraintPose> pose_ref(memnew(SpineTransformConstraintPose));
	pose_ref->set_spine_object(get_spine_owner(), &pose);
	return pose_ref;
}
