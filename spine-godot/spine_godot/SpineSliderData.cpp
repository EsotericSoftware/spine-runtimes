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

#include "SpineSliderData.h"
#include "SpineSliderPose.h"
#include "SpineAnimation.h"
#include "SpineCommon.h"

void SpineSliderData::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_animation"), &SpineSliderData::get_animation);
	ClassDB::bind_method(D_METHOD("get_additive"), &SpineSliderData::get_additive);
	ClassDB::bind_method(D_METHOD("set_additive", "value"), &SpineSliderData::set_additive);
	ClassDB::bind_method(D_METHOD("get_loop"), &SpineSliderData::get_loop);
	ClassDB::bind_method(D_METHOD("set_loop", "value"), &SpineSliderData::set_loop);
	ClassDB::bind_method(D_METHOD("get_bone"), &SpineSliderData::get_bone);
	ClassDB::bind_method(D_METHOD("set_bone", "value"), &SpineSliderData::set_bone);
	ClassDB::bind_method(D_METHOD("get_scale"), &SpineSliderData::get_scale);
	ClassDB::bind_method(D_METHOD("set_scale", "value"), &SpineSliderData::set_scale);
	ClassDB::bind_method(D_METHOD("get_offset"), &SpineSliderData::get_offset);
	ClassDB::bind_method(D_METHOD("set_offset", "value"), &SpineSliderData::set_offset);
	ClassDB::bind_method(D_METHOD("get_max"), &SpineSliderData::get_max);
	ClassDB::bind_method(D_METHOD("set_max", "value"), &SpineSliderData::set_max);
	ClassDB::bind_method(D_METHOD("get_local"), &SpineSliderData::get_local);
	ClassDB::bind_method(D_METHOD("set_local", "value"), &SpineSliderData::set_local);
	ClassDB::bind_method(D_METHOD("get_setup_pose"), &SpineSliderData::get_setup_pose);
}

Ref<SpineAnimation> SpineSliderData::get_animation() {
	SPINE_CHECK(get_spine_constraint_data(), nullptr)
	auto &animation = get_spine_constraint_data()->getAnimation();
	Ref<SpineAnimation> animation_ref(memnew(SpineAnimation));
	animation_ref->set_spine_object(get_spine_owner(), &animation);
	return animation_ref;
}

bool SpineSliderData::get_additive() {
	SPINE_CHECK(get_spine_constraint_data(), false)
	return get_spine_constraint_data()->getAdditive();
}

void SpineSliderData::set_additive(bool value) {
	SPINE_CHECK(get_spine_constraint_data(), )
	get_spine_constraint_data()->setAdditive(value);
}

bool SpineSliderData::get_loop() {
	SPINE_CHECK(get_spine_constraint_data(), false)
	return get_spine_constraint_data()->getLoop();
}

void SpineSliderData::set_loop(bool value) {
	SPINE_CHECK(get_spine_constraint_data(), )
	get_spine_constraint_data()->setLoop(value);
}

Ref<SpineBoneData> SpineSliderData::get_bone() {
	SPINE_CHECK(get_spine_constraint_data(), nullptr)
	auto bone = get_spine_constraint_data()->getBone();
	if (!bone) return nullptr;
	Ref<SpineBoneData> bone_ref(memnew(SpineBoneData));
	bone_ref->set_spine_object(get_spine_owner(), bone);
	return bone_ref;
}

void SpineSliderData::set_bone(Ref<SpineBoneData> value) {
	SPINE_CHECK(get_spine_constraint_data(), )
	if (value.is_valid() && value->get_spine_object()) {
		get_spine_constraint_data()->setBone(value->get_spine_object());
	}
}

float SpineSliderData::get_scale() {
	SPINE_CHECK(get_spine_constraint_data(), 0)
	return get_spine_constraint_data()->getScale();
}

void SpineSliderData::set_scale(float value) {
	SPINE_CHECK(get_spine_constraint_data(), )
	get_spine_constraint_data()->setScale(value);
}

float SpineSliderData::get_offset() {
	SPINE_CHECK(get_spine_constraint_data(), 0)
	return get_spine_constraint_data()->getOffset();
}

void SpineSliderData::set_offset(float value) {
	SPINE_CHECK(get_spine_constraint_data(), )
	get_spine_constraint_data()->setOffset(value);
}

float SpineSliderData::get_max() {
	SPINE_CHECK(get_spine_constraint_data(), 0)
	return get_spine_constraint_data()->getMax();
}

void SpineSliderData::set_max(float value) {
	SPINE_CHECK(get_spine_constraint_data(), )
	get_spine_constraint_data()->setMax(value);
}

bool SpineSliderData::get_local() {
	SPINE_CHECK(get_spine_constraint_data(), false)
	return get_spine_constraint_data()->getLocal();
}

void SpineSliderData::set_local(bool value) {
	SPINE_CHECK(get_spine_constraint_data(), )
	get_spine_constraint_data()->setLocal(value);
}

Ref<SpineSliderPose> SpineSliderData::get_setup_pose() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto &pose = get_spine_constraint_data()->getSetupPose();
	Ref<SpineSliderPose> pose_ref(memnew(SpineSliderPose));
	pose_ref->set_spine_object(get_spine_owner(), &pose);
	return pose_ref;
}