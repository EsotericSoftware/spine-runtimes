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

#include "SpineAnimationStateDataResource.h"

SpineAnimationStateDataResource::SpineAnimationStateDataResource() : animation_state_data(NULL), animation_state_data_created(false), default_mix(0.5f) {
}
SpineAnimationStateDataResource::~SpineAnimationStateDataResource() {
	if (animation_state_data) {
		delete animation_state_data;
		animation_state_data = NULL;
	}
}

void SpineAnimationStateDataResource::_bind_methods() {
	ClassDB::bind_method(D_METHOD("set_skeleton", "skeleton"), &SpineAnimationStateDataResource::set_skeleton);
	ClassDB::bind_method(D_METHOD("get_spine_object"), &SpineAnimationStateDataResource::get_skeleton);
	ClassDB::bind_method(D_METHOD("_on_skeleton_data_loaded"), &SpineAnimationStateDataResource::_on_skeleton_data_loaded);
	ClassDB::bind_method(D_METHOD("is_animation_state_data_created"), &SpineAnimationStateDataResource::is_animation_state_data_created);
	ClassDB::bind_method(D_METHOD("_on_skeleton_data_changed"), &SpineAnimationStateDataResource::_on_skeleton_data_changed);
	ClassDB::bind_method(D_METHOD("set_default_mix", "mix"), &SpineAnimationStateDataResource::set_default_mix);
	ClassDB::bind_method(D_METHOD("get_default_mix"), &SpineAnimationStateDataResource::get_default_mix);
	ClassDB::bind_method(D_METHOD("get_mix", "from", "to"), &SpineAnimationStateDataResource::get_mix);
	ClassDB::bind_method(D_METHOD("set_mix", "from", "to", "mix"), &SpineAnimationStateDataResource::set_mix);

	ADD_SIGNAL(MethodInfo("animation_state_data_created"));
	ADD_SIGNAL(MethodInfo("skeleton_data_res_changed"));
	ADD_SIGNAL(MethodInfo("animation_state_data_changed"));

	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "skeleton", PropertyHint::PROPERTY_HINT_RESOURCE_TYPE, "SpineSkeletonDataResource"), "set_skeleton", "get_spine_object");
	ADD_PROPERTY(PropertyInfo(Variant::REAL, "default_mix", PropertyHint::PROPERTY_HINT_EXP_RANGE, "0,1,0.01"), "set_default_mix", "get_default_mix");
}

void SpineAnimationStateDataResource::set_skeleton(const Ref<SpineSkeletonDataResource> &s) {
	skeleton = s;

	_on_skeleton_data_changed();
	if (skeleton.is_valid()) {
		skeleton->connect("skeleton_data_loaded", this, "_on_skeleton_data_loaded");
		skeleton->connect("atlas_res_changed", this, "_on_skeleton_data_changed");
		skeleton->connect("skeleton_file_res_changed", this, "_on_skeleton_data_changed");

		if (skeleton->is_skeleton_data_loaded()) {
			_on_skeleton_data_loaded();
		}
	} else {
		if (animation_state_data) {
			delete animation_state_data;
			animation_state_data = NULL;
			animation_state_data_created = false;
		}
	}
}
Ref<SpineSkeletonDataResource> SpineAnimationStateDataResource::get_skeleton() const {
	return skeleton;
}

void SpineAnimationStateDataResource::set_default_mix(float m) {
	default_mix = m;
	if (!is_animation_state_data_created()) return;
	animation_state_data->setDefaultMix(m);
}
float SpineAnimationStateDataResource::get_default_mix() {
	return default_mix;
}

void SpineAnimationStateDataResource::set_mix(const String &from, const String &to, float mix_duration) {
	if (!is_animation_state_data_created()) {
		ERR_PRINT("'set_mix' fail. Animation state data is not created!");
		return;
	}
	auto anim_from = get_skeleton()->find_animation(from);
	auto anim_to = get_skeleton()->find_animation(to);
	if (!anim_from.is_valid()) {
		ERR_PRINT("'set_mix' fail. From animation animation not found!");
		return;
	}
	if (!anim_to.is_valid()) {
		ERR_PRINT("'set_mix' fail. To animation animation not found!");
		return;
	}
	animation_state_data->setMix(anim_from->get_spine_object(), anim_to->get_spine_object(), mix_duration);
}
float SpineAnimationStateDataResource::get_mix(const String &from, const String &to) {
	if (!is_animation_state_data_created()) {
		ERR_PRINT("'set_mix' fail. Animation state data is not created!");
		return 0;
	}
	auto anim_from = get_skeleton()->find_animation(from);
	auto anim_to = get_skeleton()->find_animation(to);
	if (!anim_from.is_valid()) {
		ERR_PRINT("'set_mix' fail. From animation animation not found!");
		return 0;
	}
	if (!anim_to.is_valid()) {
		ERR_PRINT("'set_mix' fail. To animation animation not found!");
		return 0;
	}
	return animation_state_data->getMix(anim_from->get_spine_object(), anim_to->get_spine_object());
}

void SpineAnimationStateDataResource::_on_skeleton_data_loaded() {
	animation_state_data = new spine::AnimationStateData(skeleton->get_skeleton_data());
	//	print_line("Animation state data created.");


	emit_signal("animation_state_data_created");
	animation_state_data->setDefaultMix(default_mix);
	animation_state_data_created = true;
}

void SpineAnimationStateDataResource::_on_skeleton_data_changed() {
	animation_state_data_created = false;
	if (animation_state_data) {
		delete animation_state_data;
		animation_state_data = NULL;
		//		print_line("Animation state data deleted.");
	}

	//	print_line("skeleton_data_res_changed emitted");
	emit_signal("skeleton_data_res_changed");
}

bool SpineAnimationStateDataResource::is_animation_state_data_created() {
	return animation_state_data_created;
}
