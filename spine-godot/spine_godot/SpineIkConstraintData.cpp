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

#include "SpineIkConstraintData.h"
#include "SpineIkConstraintPose.h"
#include "SpineCommon.h"

void SpineIkConstraintData::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_bones"), &SpineIkConstraintData::get_bones);
	ClassDB::bind_method(D_METHOD("get_target"), &SpineIkConstraintData::get_target);
	ClassDB::bind_method(D_METHOD("set_target", "v"), &SpineIkConstraintData::set_target);
	ClassDB::bind_method(D_METHOD("get_scale_y_mode"), &SpineIkConstraintData::get_scale_y_mode);
	ClassDB::bind_method(D_METHOD("set_scale_y_mode", "v"), &SpineIkConstraintData::set_scale_y_mode);
	ClassDB::bind_method(D_METHOD("get_setup_pose"), &SpineIkConstraintData::get_setup_pose);
}

Array SpineIkConstraintData::get_bones() {
	Array result;
	SPINE_CHECK(get_spine_object(), result)
	auto &bones = get_spine_constraint_data()->getBones();
	result.resize((int) bones.size());
	for (int i = 0; i < bones.size(); ++i) {
		Ref<SpineBoneData> bone_ref(memnew(SpineBoneData));
		bone_ref->set_spine_object(get_spine_owner(), bones[i]);
		result[i] = bone_ref;
	}
	return result;
}

Ref<SpineBoneData> SpineIkConstraintData::get_target() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto target = &get_spine_constraint_data()->getTarget();
	if (!target) return nullptr;
	Ref<SpineBoneData> target_ref(memnew(SpineBoneData));
	target_ref->set_spine_object(get_spine_owner(), target);
	return target_ref;
}

void SpineIkConstraintData::set_target(Ref<SpineBoneData> v) {
	SPINE_CHECK(get_spine_object(), )
	if (v.is_valid() && v->get_spine_object()) {
		get_spine_constraint_data()->setTarget(*v->get_spine_object());
	}
}

SpineConstant::ScaleYMode SpineIkConstraintData::get_scale_y_mode() {
	SPINE_CHECK(get_spine_object(), SpineConstant::ScaleYMode_None)
	return (SpineConstant::ScaleYMode) get_spine_constraint_data()->getScaleYMode();
}

void SpineIkConstraintData::set_scale_y_mode(SpineConstant::ScaleYMode v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_constraint_data()->setScaleYMode((spine::ScaleYMode) v);
}

Ref<SpineIkConstraintPose> SpineIkConstraintData::get_setup_pose() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto &pose = get_spine_constraint_data()->getSetupPose();
	Ref<SpineIkConstraintPose> pose_ref(memnew(SpineIkConstraintPose));
	pose_ref->set_spine_object(get_spine_owner(), &pose);
	return pose_ref;
}
