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

#include "SpineSlider.h"
#include "SpineSliderPose.h"
#include "SpineCommon.h"
#include "SpineSkeleton.h"
#include "SpineSpriteCommon.h"

void SpineSlider::_bind_methods() {
	ClassDB::bind_method(D_METHOD("update", "skeleton", "physics"), &SpineSlider::update);
	ClassDB::bind_method(D_METHOD("get_data"), &SpineSlider::get_data);
	ClassDB::bind_method(D_METHOD("get_bone"), &SpineSlider::get_bone);
	ClassDB::bind_method(D_METHOD("set_bone", "v"), &SpineSlider::set_bone);
	ClassDB::bind_method(D_METHOD("get_pose"), &SpineSlider::get_pose);
	ClassDB::bind_method(D_METHOD("get_applied_pose"), &SpineSlider::get_applied_pose);
	ClassDB::bind_method(D_METHOD("is_active"), &SpineSlider::is_active);
	ClassDB::bind_method(D_METHOD("set_active", "v"), &SpineSlider::set_active);
}

void SpineSlider::update(Ref<SpineSkeleton> skeleton, SpineConstant::Physics physics) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->update(*skeleton->get_spine_object(), (spine::Physics) physics);
}

Ref<SpineSliderData> SpineSlider::get_data() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto &data = get_spine_object()->getData();
	Ref<SpineSliderData> data_ref(memnew(SpineSliderData));
	data_ref->set_spine_object(*spine_sprite_get_skeleton_data_res(get_spine_owner()), &data);
	return data_ref;
}

Ref<SpineBone> SpineSlider::get_bone() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto &bone = get_spine_object()->getBone();
	Ref<SpineBone> bone_ref(memnew(SpineBone));
	bone_ref->set_spine_object(get_spine_owner(), &bone);
	return bone_ref;
}

void SpineSlider::set_bone(Ref<SpineBone> v) {
	SPINE_CHECK(get_spine_object(), )
	if (v.is_valid() && v->get_spine_object()) {
		get_spine_object()->setBone(*v->get_spine_object());
	}
}

Ref<SpineSliderPose> SpineSlider::get_pose() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto &pose = get_spine_object()->getPose();
	Ref<SpineSliderPose> pose_ref(memnew(SpineSliderPose));
	pose_ref->set_spine_object(get_spine_owner(), &pose);
	return pose_ref;
}

Ref<SpineSliderPose> SpineSlider::get_applied_pose() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto &pose = get_spine_object()->getAppliedPose();
	Ref<SpineSliderPose> pose_ref(memnew(SpineSliderPose));
	pose_ref->set_spine_object(get_spine_owner(), &pose);
	return pose_ref;
}

bool SpineSlider::is_active() {
	SPINE_CHECK(get_spine_object(), false)
	return get_spine_object()->isActive();
}

void SpineSlider::set_active(bool v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setActive(v);
}