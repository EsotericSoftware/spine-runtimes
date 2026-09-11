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

#include "SpineSlot.h"
#include "SpineBone.h"
#include "SpineCommon.h"
#include "SpineController.h"
#include "SpineSprite.h"
#include "SpineSkeletonDataResource.h"

void SpineSlot::_bind_methods() {
	ClassDB::bind_method(D_METHOD("set_to_setup_pose"), &SpineSlot::set_to_setup_pose);
	ClassDB::bind_method(D_METHOD("get_data"), &SpineSlot::get_data);
	ClassDB::bind_method(D_METHOD("get_bone"), &SpineSlot::get_bone);
	ClassDB::bind_method(D_METHOD("get_pose"), &SpineSlot::get_pose);
	ClassDB::bind_method(D_METHOD("get_applied_pose"), &SpineSlot::get_applied_pose);
}

void SpineSlot::set_to_setup_pose() {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setupPose();
}

Ref<SpineSlotData> SpineSlot::get_data() {
	SPINE_CHECK(get_spine_object(), nullptr)
	if (_data.is_valid()) {
		return _data;
	} else {
		auto &slot_data = get_spine_object()->getData();
		Ref<SpineSlotData> slot_data_ref(memnew(SpineSlotData));
		slot_data_ref->set_spine_object(*get_spine_controller()->get_skeleton_data_res(), &slot_data);
		_data = slot_data_ref;
		return slot_data_ref;
	}
}

Ref<SpineBone> SpineSlot::get_bone() {
	SPINE_CHECK(get_spine_object(), nullptr)
	if (_bone.is_valid()) {
		return _bone;
	} else {
		auto &bone = get_spine_object()->getBone();
		Ref<SpineBone> bone_ref(memnew(SpineBone));
		bone_ref->set_spine_object(get_spine_controller(), &bone);
		_bone = bone_ref;
		return bone_ref;
	}
}

Ref<SpineSlotPose> SpineSlot::get_pose() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto &pose = get_spine_object()->getPose();
	Ref<SpineSlotPose> pose_ref(memnew(SpineSlotPose));
	pose_ref->set_spine_object(get_spine_controller(), &pose);
	return pose_ref;
}

Ref<SpineSlotPose> SpineSlot::get_applied_pose() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto &applied_pose = get_spine_object()->getAppliedPose();
	Ref<SpineSlotPose> pose_ref(memnew(SpineSlotPose));
	pose_ref->set_spine_object(get_spine_controller(), &applied_pose);
	return pose_ref;
}
