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

#include "SpineTransformConstraint.h"
#include "SpineTransformConstraintPose.h"
#include "SpineCommon.h"
#include "SpineController.h"
#include "SpineSkeleton.h"
#include "SpineSprite.h"

void SpineTransformConstraint::_bind_methods() {
	ClassDB::bind_method(D_METHOD("update", "skeleton"), &SpineTransformConstraint::update);
	ClassDB::bind_method(D_METHOD("get_data"), &SpineTransformConstraint::get_data);
	ClassDB::bind_method(D_METHOD("get_bones"), &SpineTransformConstraint::get_bones);
	ClassDB::bind_method(D_METHOD("get_source"), &SpineTransformConstraint::get_source);
	ClassDB::bind_method(D_METHOD("set_source", "v"), &SpineTransformConstraint::set_source);
	ClassDB::bind_method(D_METHOD("get_pose"), &SpineTransformConstraint::get_pose);
	ClassDB::bind_method(D_METHOD("get_applied_pose"), &SpineTransformConstraint::get_applied_pose);
	ClassDB::bind_method(D_METHOD("is_active"), &SpineTransformConstraint::is_active);
	ClassDB::bind_method(D_METHOD("set_active", "v"), &SpineTransformConstraint::set_active);
}

void SpineTransformConstraint::update(Ref<SpineSkeleton> skeleton) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->update(*skeleton->get_spine_object(), spine::Physics_Update);
}

Ref<SpineTransformConstraintData> SpineTransformConstraint::get_data() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto &data = get_spine_object()->getData();
	Ref<SpineTransformConstraintData> data_ref(memnew(SpineTransformConstraintData));
	data_ref->set_spine_object(*get_spine_controller()->get_skeleton_data_res(), &data);
	return data_ref;
}

Array SpineTransformConstraint::get_bones() {
	Array result;
	SPINE_CHECK(get_spine_object(), result)
	auto &bones = get_spine_object()->getBones();
	result.resize((int) bones.size());
	for (int i = 0; i < bones.size(); ++i) {
		auto bone = bones[i];
		Ref<SpineBonePose> bone_ref(memnew(SpineBonePose));
		bone_ref->set_spine_object(get_spine_controller(), bone);
		result[i] = bone_ref;
	}
	return result;
}

Ref<SpineBone> SpineTransformConstraint::get_source() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto source = &get_spine_object()->getSource();
	if (!source) return nullptr;
	Ref<SpineBone> target_ref(memnew(SpineBone));
	target_ref->set_spine_object(get_spine_controller(), source);
	return target_ref;
}

void SpineTransformConstraint::set_source(Ref<SpineBone> v) {
	SPINE_CHECK(get_spine_object(), )
	if (v.is_valid() && v->get_spine_object()) {
		get_spine_object()->setSource(*v->get_spine_object());
	}
}

Ref<SpineTransformConstraintPose> SpineTransformConstraint::get_pose() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto &pose = get_spine_object()->getPose();
	Ref<SpineTransformConstraintPose> pose_ref(memnew(SpineTransformConstraintPose));
	pose_ref->set_spine_object(get_spine_controller(), &pose);
	return pose_ref;
}

Ref<SpineTransformConstraintPose> SpineTransformConstraint::get_applied_pose() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto &pose = get_spine_object()->getAppliedPose();
	Ref<SpineTransformConstraintPose> pose_ref(memnew(SpineTransformConstraintPose));
	pose_ref->set_spine_object(get_spine_controller(), &pose);
	return pose_ref;
}

bool SpineTransformConstraint::is_active() {
	SPINE_CHECK(get_spine_object(), false)
	return get_spine_object()->isActive();
}

void SpineTransformConstraint::set_active(bool v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setActive(v);
}
