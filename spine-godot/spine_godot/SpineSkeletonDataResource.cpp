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

#include "SpineSkeletonDataResource.h"
#include "SpineCommon.h"

#ifdef SPINE_GODOT_EXTENSION
#include <godot_cpp/classes/encoded_object_as_id.hpp>
#endif

void SpineAnimationMix::_bind_methods() {
	ClassDB::bind_method(D_METHOD("set_from", "from"),
						 &SpineAnimationMix::set_from);
	ClassDB::bind_method(D_METHOD("get_from"), &SpineAnimationMix::get_from);
	ClassDB::bind_method(D_METHOD("set_to", "to"), &SpineAnimationMix::set_to);
	ClassDB::bind_method(D_METHOD("get_to"), &SpineAnimationMix::get_to);
	ClassDB::bind_method(D_METHOD("set_mix", "mix"), &SpineAnimationMix::set_mix);
	ClassDB::bind_method(D_METHOD("get_mix"), &SpineAnimationMix::get_mix);

	ADD_PROPERTY(PropertyInfo(Variant::STRING, "from"), "set_from", "get_from");
	ADD_PROPERTY(PropertyInfo(Variant::STRING, "to"), "set_to", "get_to");
#if VERSION_MAJOR > 3
	ADD_PROPERTY(PropertyInfo(Variant::FLOAT, "mix"), "set_mix", "get_mix");
#else
	ADD_PROPERTY(PropertyInfo(Variant::REAL, "mix"), "set_mix", "get_mix");
#endif
}

SpineAnimationMix::SpineAnimationMix() : from(""), to(""), mix(0) {}

void SpineAnimationMix::set_from(const String &_from) { this->from = _from; }

String SpineAnimationMix::get_from() { return from; }

void SpineAnimationMix::set_to(const String &_to) { this->to = _to; }

String SpineAnimationMix::get_to() { return to; }

void SpineAnimationMix::set_mix(float _mix) { this->mix = _mix; }

float SpineAnimationMix::get_mix() { return mix; }

void SpineSkeletonDataResource::_bind_methods() {
	ClassDB::bind_method(D_METHOD("is_skeleton_data_loaded"),
						 &SpineSkeletonDataResource::is_skeleton_data_loaded);
	ClassDB::bind_method(D_METHOD("set_atlas_res", "atlas_res"),
						 &SpineSkeletonDataResource::set_atlas_res);
	ClassDB::bind_method(D_METHOD("get_atlas_res"),
						 &SpineSkeletonDataResource::get_atlas_res);
	ClassDB::bind_method(D_METHOD("set_skeleton_file_res", "skeleton_file_res"),
						 &SpineSkeletonDataResource::set_skeleton_file_res);
	ClassDB::bind_method(D_METHOD("get_skeleton_file_res"),
						 &SpineSkeletonDataResource::get_skeleton_file_res);
	ClassDB::bind_method(D_METHOD("set_default_mix", "default_mix"),
						 &SpineSkeletonDataResource::set_default_mix);
	ClassDB::bind_method(D_METHOD("get_default_mix"),
						 &SpineSkeletonDataResource::get_default_mix);
	ClassDB::bind_method(D_METHOD("set_animation_mixes", "mixes"),
						 &SpineSkeletonDataResource::set_animation_mixes);
	ClassDB::bind_method(D_METHOD("get_animation_mixes"),
						 &SpineSkeletonDataResource::get_animation_mixes);

	// Spine API
	ClassDB::bind_method(D_METHOD("find_bone", "bone_name"),
						 &SpineSkeletonDataResource::find_bone);
	ClassDB::bind_method(D_METHOD("find_slot", "slot_name"),
						 &SpineSkeletonDataResource::find_slot);
	ClassDB::bind_method(D_METHOD("find_skin", "skin_name"),
						 &SpineSkeletonDataResource::find_skin);
	ClassDB::bind_method(D_METHOD("find_event", "event_data_name"),
						 &SpineSkeletonDataResource::find_event);
	ClassDB::bind_method(D_METHOD("find_animation", "animation_name"),
						 &SpineSkeletonDataResource::find_animation);
	ClassDB::bind_method(D_METHOD("find_ik_constraint_data", "constraint_name"),
						 &SpineSkeletonDataResource::find_ik_constraint);
	ClassDB::bind_method(
			D_METHOD("find_transform_constraint_data", "constraint_name"),
			&SpineSkeletonDataResource::find_transform_constraint);
	ClassDB::bind_method(D_METHOD("find_path_constraint_data", "constraint_name"),
						 &SpineSkeletonDataResource::find_path_constraint);
	ClassDB::bind_method(D_METHOD("find_physics_constraint_data", "constraint_name"),
						 &SpineSkeletonDataResource::find_physics_constraint);
	ClassDB::bind_method(D_METHOD("get_skeleton_name"),
						 &SpineSkeletonDataResource::get_skeleton_name);
	ClassDB::bind_method(D_METHOD("get_bones"),
						 &SpineSkeletonDataResource::get_bones);
	ClassDB::bind_method(D_METHOD("get_slots"),
						 &SpineSkeletonDataResource::get_slots);
	ClassDB::bind_method(D_METHOD("get_skins"),
						 &SpineSkeletonDataResource::get_skins);
	ClassDB::bind_method(D_METHOD("get_default_skin"),
						 &SpineSkeletonDataResource::get_default_skin);
	ClassDB::bind_method(D_METHOD("set_default_skin", "skin"),
						 &SpineSkeletonDataResource::set_default_skin);
	ClassDB::bind_method(D_METHOD("get_events"),
						 &SpineSkeletonDataResource::get_events);
	ClassDB::bind_method(D_METHOD("get_animations"),
						 &SpineSkeletonDataResource::get_animations);
	ClassDB::bind_method(D_METHOD("get_ik_constraints"),
						 &SpineSkeletonDataResource::get_ik_constraints);
	ClassDB::bind_method(D_METHOD("get_transform_constraints"),
						 &SpineSkeletonDataResource::get_transform_constraints);
	ClassDB::bind_method(D_METHOD("get_path_constraints"),
						 &SpineSkeletonDataResource::get_path_constraints);
	ClassDB::bind_method(D_METHOD("get_physics_constraints"),
						 &SpineSkeletonDataResource::get_physics_constraints);
	ClassDB::bind_method(D_METHOD("get_x"), &SpineSkeletonDataResource::get_x);
	ClassDB::bind_method(D_METHOD("get_y"), &SpineSkeletonDataResource::get_y);
	ClassDB::bind_method(D_METHOD("get_width"),
						 &SpineSkeletonDataResource::get_width);
	ClassDB::bind_method(D_METHOD("get_height"),
						 &SpineSkeletonDataResource::get_height);
	ClassDB::bind_method(D_METHOD("get_version"),
						 &SpineSkeletonDataResource::get_version);
	ClassDB::bind_method(D_METHOD("get_hash"),
						 &SpineSkeletonDataResource::get_hash);
	ClassDB::bind_method(D_METHOD("get_images_path"),
						 &SpineSkeletonDataResource::get_images_path);
	ClassDB::bind_method(D_METHOD("get_audio_path"),
						 &SpineSkeletonDataResource::get_audio_path);
	ClassDB::bind_method(D_METHOD("get_fps"),
						 &SpineSkeletonDataResource::get_fps);
	ClassDB::bind_method(D_METHOD("get_reference_scale"),
						 &SpineSkeletonDataResource::get_reference_scale);
	ClassDB::bind_method(D_METHOD("set_reference_scale", "reference_scale"),
						 &SpineSkeletonDataResource::set_reference_scale);
	ClassDB::bind_method(D_METHOD("update_skeleton_data"),
						 &SpineSkeletonDataResource::update_skeleton_data);

	ADD_SIGNAL(MethodInfo("skeleton_data_changed"));
	ADD_SIGNAL(MethodInfo("_internal_spine_objects_invalidated"));

	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "atlas_res",
							  PropertyHint::PROPERTY_HINT_RESOURCE_TYPE,
							  "SpineAtlasResource"),
				 "set_atlas_res", "get_atlas_res");
	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "skeleton_file_res",
							  PropertyHint::PROPERTY_HINT_RESOURCE_TYPE,
							  "SpineSkeletonFileResource"),
				 "set_skeleton_file_res", "get_skeleton_file_res");
#if VERSION_MAJOR > 3
	ADD_PROPERTY(PropertyInfo(Variant::FLOAT, "default_mix"), "set_default_mix",
				 "get_default_mix");
#else
	ADD_PROPERTY(PropertyInfo(Variant::REAL, "default_mix"), "set_default_mix",
				 "get_default_mix");
#endif
	ADD_PROPERTY(PropertyInfo(Variant::ARRAY, "animation_mixes"),
				 "set_animation_mixes", "get_animation_mixes");
}

SpineSkeletonDataResource::SpineSkeletonDataResource()
	: default_mix(0), skeleton_data(nullptr), animation_state_data(nullptr) {}

SpineSkeletonDataResource::~SpineSkeletonDataResource() {
	delete skeleton_data;
	delete animation_state_data;
}

void SpineSkeletonDataResource::update_skeleton_data() {
	if (skeleton_data) {
		delete skeleton_data;
		skeleton_data = nullptr;
	}
	if (animation_state_data) {
		delete animation_state_data;
		animation_state_data = nullptr;
	}

	emit_signal(SNAME("_internal_spine_objects_invalidated"));

	if (atlas_res.is_valid() && skeleton_file_res.is_valid()) {
		load_resources(atlas_res->get_spine_atlas(), skeleton_file_res->get_json(),
					   skeleton_file_res->get_binary());
	}
	emit_signal(SNAME("skeleton_data_changed"));
#ifdef TOOLS_ENABLED
	NOTIFY_PROPERTY_LIST_CHANGED();
#endif
}

#ifdef SPINE_GODOT_EXTENSION
void SpineSkeletonDataResource::load_resources(spine::Atlas *atlas,
											   const String &json,
											   const PackedByteArray &binary) {
#else
void SpineSkeletonDataResource::load_resources(spine::Atlas *atlas,
											   const String &json,
											   const Vector<uint8_t> &binary) {
#endif
	if ((EMPTY(json) && EMPTY(binary)) || atlas == nullptr)
		return;

	spine::SkeletonData *data;
	if (!EMPTY(json)) {
		spine::SkeletonJson skeletonJson(atlas);
		data = skeletonJson.readSkeletonData(json.utf8());
		if (!data) {
			ERR_PRINT(String("Error while loading skeleton data: ") + get_path());
			ERR_PRINT(String("Error message: ") + skeletonJson.getError().buffer());
			return;
		}
	} else {
		spine::SkeletonBinary skeletonBinary(atlas);
		data = skeletonBinary.readSkeletonData(binary.ptr(), binary.size());
		if (!data) {
			ERR_PRINT(String("Error while loading skeleton data: ") + get_path());
			ERR_PRINT(String("Error message: ") + skeletonBinary.getError().buffer());
			return;
		}
	}
	skeleton_data = data;
	animation_state_data = new spine::AnimationStateData(data);
	update_mixes();
}

bool SpineSkeletonDataResource::is_skeleton_data_loaded() const {
	return skeleton_data != nullptr;
}

void SpineSkeletonDataResource::set_atlas_res(
		const Ref<SpineAtlasResource> &atlas) {
	atlas_res = atlas;
	if (atlas_res.is_valid()) {
#if VERSION_MAJOR > 3
		if (!atlas_res->is_connected(
					SNAME("skeleton_atlas_changed"),
					callable_mp(this,
								&SpineSkeletonDataResource::update_skeleton_data)))
			atlas_res->connect(
					SNAME("skeleton_atlas_changed"),
					callable_mp(this, &SpineSkeletonDataResource::update_skeleton_data));
#else
		if (!atlas_res->is_connected(SNAME("skeleton_atlas_changed"), this,
									 SNAME("update_skeleton_data")))
			atlas_res->connect(SNAME("skeleton_atlas_changed"), this,
							   SNAME("update_skeleton_data"));
#endif
	}
	update_skeleton_data();
}

Ref<SpineAtlasResource> SpineSkeletonDataResource::get_atlas_res() {
	return atlas_res;
}

void SpineSkeletonDataResource::set_skeleton_file_res(
		const Ref<SpineSkeletonFileResource> &skeleton_file) {
	skeleton_file_res = skeleton_file;
	if (skeleton_file_res.is_valid()) {
#if VERSION_MAJOR > 3
		if (!skeleton_file_res->is_connected(
					SNAME("skeleton_file_changed"),
					callable_mp(this,
								&SpineSkeletonDataResource::update_skeleton_data)))
			skeleton_file_res->connect(
					SNAME("skeleton_file_changed"),
					callable_mp(this, &SpineSkeletonDataResource::update_skeleton_data));
#else
		if (!skeleton_file_res->is_connected(SNAME("skeleton_file_changed"), this,
											 SNAME("update_skeleton_data")))
			skeleton_file_res->connect(SNAME("skeleton_file_changed"), this,
									   SNAME("update_skeleton_data"));
#endif
	}
	update_skeleton_data();
}

Ref<SpineSkeletonFileResource>
SpineSkeletonDataResource::get_skeleton_file_res() {
	return skeleton_file_res;
}

#ifdef SPINE_GODOT_EXTENSION
void SpineSkeletonDataResource::get_animation_names(
		PackedStringArray &animation_names) const {
#else
void SpineSkeletonDataResource::get_animation_names(
		Vector<String> &animation_names) const {
#endif
	animation_names.clear();
	if (!is_skeleton_data_loaded())
		return;
	auto animations = skeleton_data->getAnimations();
	for (size_t i = 0; i < animations.size(); ++i) {
		auto animation = animations[i];
		animation_names.push_back(animation->getName().buffer());
	}
}

#ifdef SPINE_GODOT_EXTENSION
void SpineSkeletonDataResource::get_skin_names(
		PackedStringArray &skin_names) const {
#else
void SpineSkeletonDataResource::get_skin_names(
		Vector<String> &skin_names) const {
#endif
	skin_names.clear();
	if (!is_skeleton_data_loaded())
		return;
	auto skins = skeleton_data->getSkins();
	for (size_t i = 0; i < skins.size(); ++i) {
		auto skin = skins[i];
		skin_names.push_back(skin->getName().buffer());
	}
}

#ifdef SPINE_GODOT_EXTENSION
void SpineSkeletonDataResource::get_slot_names(PackedStringArray &slot_names) {
#else
void SpineSkeletonDataResource::get_slot_names(Vector<String> &slot_names) {
#endif
	slot_names.clear();
	if (!is_skeleton_data_loaded())
		return;
	auto slots = skeleton_data->getSlots();
	for (size_t i = 0; i < slots.size(); ++i) {
		auto slot = slots[i];
		slot_names.push_back(slot->getName().buffer());
	}
}

#ifdef SPINE_GODOT_EXTENSION
void SpineSkeletonDataResource::get_bone_names(PackedStringArray &bone_names) {
#else
void SpineSkeletonDataResource::get_bone_names(Vector<String> &bone_names) {
#endif
	bone_names.clear();
	if (!is_skeleton_data_loaded())
		return;
	auto bones = skeleton_data->getBones();
	for (size_t i = 0; i < bones.size(); ++i) {
		auto bone = bones[i];
		bone_names.push_back(bone->getName().buffer());
	}
}

void SpineSkeletonDataResource::set_default_mix(float _default_mix) {
	this->default_mix = _default_mix;
	update_mixes();
}

float SpineSkeletonDataResource::get_default_mix() { return default_mix; }

void SpineSkeletonDataResource::set_animation_mixes(Array _animation_mixes) {
	for (int i = 0; i < _animation_mixes.size(); i++) {
		auto objectId = Object::cast_to<EncodedObjectAsID>(_animation_mixes[0]);
		if (objectId) {
			ERR_PRINT("Live-editing of animation mixes is not supported.");
			return;
		}
	}

	this->animation_mixes = _animation_mixes;
	update_mixes();
}

Array SpineSkeletonDataResource::get_animation_mixes() {
	return animation_mixes;
}

void SpineSkeletonDataResource::update_mixes() {
	if (!is_skeleton_data_loaded())
		return;
	animation_state_data->clear();
	animation_state_data->setDefaultMix(default_mix);
	for (int i = 0; i < animation_mixes.size(); i++) {
		Ref<SpineAnimationMix> mix = animation_mixes[i];
		spine::Animation *from =
				skeleton_data->findAnimation(mix->get_from().utf8().ptr());
		spine::Animation *to =
				skeleton_data->findAnimation(mix->get_to().utf8().ptr());
		if (!from) {
			ERR_PRINT(vformat("Failed to set animation mix %s->%s. Animation %s does "
							  "not exist in skeleton.",
							  from, to, from));
			continue;
		}
		if (!to) {
			ERR_PRINT(vformat("Failed to set animation mix %s->%s. Animation %s does "
							  "not exist in skeleton.",
							  from, to, to));
			continue;
		}
		animation_state_data->setMix(from, to, mix->get_mix());
	}
}

Ref<SpineAnimation>
SpineSkeletonDataResource::find_animation(const String &animation_name) const {
	SPINE_CHECK(skeleton_data, nullptr)
	if (EMPTY(animation_name))
		return nullptr;
	auto animation =
			skeleton_data->findAnimation(SPINE_STRING_TMP(animation_name));
	if (!animation)
		return nullptr;
	Ref<SpineAnimation> animation_ref(memnew(SpineAnimation));
	animation_ref->set_spine_object(this, animation);
	return animation_ref;
}

Ref<SpineBoneData>
SpineSkeletonDataResource::find_bone(const String &bone_name) const {
	SPINE_CHECK(skeleton_data, nullptr)
	if (EMPTY(bone_name))
		return nullptr;
	auto bone = skeleton_data->findBone(SPINE_STRING_TMP(bone_name));
	if (!bone)
		return nullptr;
	Ref<SpineBoneData> bone_ref(memnew(SpineBoneData));
	bone_ref->set_spine_object(this, bone);
	return bone_ref;
}

Ref<SpineSlotData>
SpineSkeletonDataResource::find_slot(const String &slot_name) const {
	SPINE_CHECK(skeleton_data, nullptr)
	if (EMPTY(slot_name))
		return nullptr;
	auto slot = skeleton_data->findSlot(SPINE_STRING_TMP(slot_name));
	if (!slot)
		return nullptr;
	Ref<SpineSlotData> slot_ref(memnew(SpineSlotData));
	slot_ref->set_spine_object(this, slot);
	return slot_ref;
}

Ref<SpineSkin>
SpineSkeletonDataResource::find_skin(const String &skin_name) const {
	SPINE_CHECK(skeleton_data, nullptr)
	if (EMPTY(skin_name))
		return nullptr;
	auto skin = skeleton_data->findSkin(SPINE_STRING_TMP(skin_name));
	if (!skin)
		return nullptr;
	Ref<SpineSkin> skin_ref(memnew(SpineSkin));
	skin_ref->set_spine_object(this, skin);
	return skin_ref;
}

Ref<SpineEventData>
SpineSkeletonDataResource::find_event(const String &event_data_name) const {
	SPINE_CHECK(skeleton_data, nullptr)
	if (EMPTY(event_data_name))
		return nullptr;
	auto event = skeleton_data->findEvent(SPINE_STRING_TMP(event_data_name));
	if (!event)
		return nullptr;
	Ref<SpineEventData> event_ref(memnew(SpineEventData));
	event_ref->set_spine_object(this, event);
	return event_ref;
}

Ref<SpineIkConstraintData> SpineSkeletonDataResource::find_ik_constraint(
		const String &constraint_name) const {
	SPINE_CHECK(skeleton_data, nullptr)
	if (EMPTY(constraint_name))
		return nullptr;
	auto constraint =
			skeleton_data->findIkConstraint(SPINE_STRING_TMP(constraint_name));
	if (!constraint)
		return nullptr;
	Ref<SpineIkConstraintData> constraint_ref(memnew(SpineIkConstraintData));
	constraint_ref->set_spine_object(this, constraint);
	return constraint_ref;
}

Ref<SpineTransformConstraintData>
SpineSkeletonDataResource::find_transform_constraint(
		const String &constraint_name) const {
	SPINE_CHECK(skeleton_data, nullptr)
	if (EMPTY(constraint_name))
		return nullptr;
	auto constraint =
			skeleton_data->findTransformConstraint(SPINE_STRING_TMP(constraint_name));
	if (!constraint)
		return nullptr;
	Ref<SpineTransformConstraintData> constraint_ref(
			memnew(SpineTransformConstraintData));
	constraint_ref->set_spine_object(this, constraint);
	return constraint_ref;
}

Ref<SpinePathConstraintData> SpineSkeletonDataResource::find_path_constraint(
		const String &constraint_name) const {
	SPINE_CHECK(skeleton_data, nullptr)
	if (EMPTY(constraint_name))
		return nullptr;
	auto constraint =
			skeleton_data->findPathConstraint(SPINE_STRING_TMP(constraint_name));
	if (constraint == nullptr)
		return nullptr;
	Ref<SpinePathConstraintData> constraint_ref(memnew(SpinePathConstraintData));
	constraint_ref->set_spine_object(this, constraint);
	return constraint_ref;
}

Ref<SpinePhysicsConstraintData>
SpineSkeletonDataResource::find_physics_constraint(
		const String &constraint_name) const {
	SPINE_CHECK(skeleton_data, nullptr)
	if (EMPTY(constraint_name))
		return nullptr;
	auto constraint =
			skeleton_data->findPhysicsConstraint(SPINE_STRING_TMP(constraint_name));
	if (constraint == nullptr)
		return nullptr;
	Ref<SpinePhysicsConstraintData> constraint_ref(
			memnew(SpinePhysicsConstraintData));
	constraint_ref->set_spine_object(this, constraint);
	return constraint_ref;
}

String SpineSkeletonDataResource::get_skeleton_name() const {
	SPINE_CHECK(skeleton_data, "")
	return skeleton_data->getName().buffer();
}

Array SpineSkeletonDataResource::get_bones() const {
	Array result;
	SPINE_CHECK(skeleton_data, result)
	auto bones = skeleton_data->getBones();
	result.resize((int) bones.size());
	for (int i = 0; i < bones.size(); ++i) {
		Ref<SpineBoneData> bone_ref(memnew(SpineBoneData));
		bone_ref->set_spine_object(this, bones[i]);
		result[i] = bone_ref;
	}
	return result;
}

Array SpineSkeletonDataResource::get_slots() const {
	Array result;
	SPINE_CHECK(skeleton_data, result)
	auto slots = skeleton_data->getSlots();
	result.resize((int) slots.size());
	for (int i = 0; i < slots.size(); ++i) {
		Ref<SpineSlotData> slot_ref(memnew(SpineSlotData));
		slot_ref->set_spine_object(this, slots[i]);
		result[i] = slot_ref;
	}
	return result;
}

Array SpineSkeletonDataResource::get_skins() const {
	Array result;
	SPINE_CHECK(skeleton_data, result)
	auto skins = skeleton_data->getSkins();
	result.resize((int) skins.size());
	for (int i = 0; i < skins.size(); ++i) {
		Ref<SpineSkin> skin_ref(memnew(SpineSkin));
		skin_ref->set_spine_object(this, skins[i]);
		result[i] = skin_ref;
	}
	return result;
}

Ref<SpineSkin> SpineSkeletonDataResource::get_default_skin() const {
	SPINE_CHECK(skeleton_data, nullptr)
	auto skin = skeleton_data->getDefaultSkin();
	if (skin)
		return nullptr;
	Ref<SpineSkin> skin_ref(memnew(SpineSkin));
	skin_ref->set_spine_object(this, skin);
	return skin_ref;
}

void SpineSkeletonDataResource::set_default_skin(Ref<SpineSkin> skin) {
	SPINE_CHECK(skeleton_data, )
	skeleton_data->setDefaultSkin(skin.is_valid() && skin->get_spine_object()
										  ? skin->get_spine_object()
										  : nullptr);
}

Array SpineSkeletonDataResource::get_events() const {
	Array result;
	SPINE_CHECK(skeleton_data, result)
	auto events = skeleton_data->getEvents();
	result.resize((int) events.size());
	for (int i = 0; i < events.size(); ++i) {
		Ref<SpineEventData> event_ref(memnew(SpineEventData));
		event_ref->set_spine_object(this, events[i]);
		result[i] = event_ref;
	}
	return result;
}

Array SpineSkeletonDataResource::get_animations() const {
	Array result;
	SPINE_CHECK(skeleton_data, result)
	auto animations = skeleton_data->getAnimations();
	result.resize((int) animations.size());
	for (int i = 0; i < animations.size(); ++i) {
		Ref<SpineAnimation> animation_ref(memnew(SpineAnimation));
		animation_ref->set_spine_object(this, animations[i]);
		result[i] = animation_ref;
	}
	return result;
}

Array SpineSkeletonDataResource::get_ik_constraints() const {
	Array result;
	SPINE_CHECK(skeleton_data, result)
	auto constraints = skeleton_data->getIkConstraints();
	result.resize((int) constraints.size());
	for (int i = 0; i < constraints.size(); ++i) {
		Ref<SpineIkConstraintData> constraint_ref(memnew(SpineIkConstraintData));
		constraint_ref->set_spine_object(this, constraints[i]);
		result[i] = constraint_ref;
	}
	return result;
}

Array SpineSkeletonDataResource::get_transform_constraints() const {
	Array result;
	SPINE_CHECK(skeleton_data, result)
	auto constraints = skeleton_data->getTransformConstraints();
	result.resize((int) constraints.size());
	for (int i = 0; i < constraints.size(); ++i) {
		Ref<SpineTransformConstraintData> constraint_ref(
				memnew(SpineTransformConstraintData));
		constraint_ref->set_spine_object(this, constraints[i]);
		result[i] = constraint_ref;
	}
	return result;
}

Array SpineSkeletonDataResource::get_path_constraints() const {
	Array result;
	SPINE_CHECK(skeleton_data, result)
	auto constraints = skeleton_data->getPathConstraints();
	result.resize((int) constraints.size());
	for (int i = 0; i < constraints.size(); ++i) {
		Ref<SpinePathConstraintData> constraint_ref(
				memnew(SpinePathConstraintData));
		constraint_ref->set_spine_object(this, constraints[i]);
		result[i] = constraint_ref;
	}
	return result;
}

Array SpineSkeletonDataResource::get_physics_constraints() const {
	Array result;
	SPINE_CHECK(skeleton_data, result)
	auto constraints = skeleton_data->getPhysicsConstraints();
	result.resize((int) constraints.size());
	for (int i = 0; i < constraints.size(); ++i) {
		Ref<SpinePhysicsConstraintData> constraint_ref(
				memnew(SpinePhysicsConstraintData));
		constraint_ref->set_spine_object(this, constraints[i]);
		result[i] = constraint_ref;
	}
	return result;
}

float SpineSkeletonDataResource::get_x() const {
	SPINE_CHECK(skeleton_data, 0)
	return skeleton_data->getX();
}

float SpineSkeletonDataResource::get_y() const {
	SPINE_CHECK(skeleton_data, 0)
	return skeleton_data->getY();
}

float SpineSkeletonDataResource::get_width() const {
	SPINE_CHECK(skeleton_data, 0)
	return skeleton_data->getWidth();
}

float SpineSkeletonDataResource::get_height() const {
	SPINE_CHECK(skeleton_data, 0)
	return skeleton_data->getHeight();
}

String SpineSkeletonDataResource::get_version() const {
	SPINE_CHECK(skeleton_data, "")
	return skeleton_data->getVersion().buffer();
}

String SpineSkeletonDataResource::get_hash() const {
	SPINE_CHECK(skeleton_data, "")
	return skeleton_data->getHash().buffer();
}

String SpineSkeletonDataResource::get_images_path() const {
	SPINE_CHECK(skeleton_data, "")
	return skeleton_data->getImagesPath().buffer();
}

String SpineSkeletonDataResource::get_audio_path() const {
	SPINE_CHECK(skeleton_data, "")
	return skeleton_data->getAudioPath().buffer();
}

float SpineSkeletonDataResource::get_fps() const {
	SPINE_CHECK(skeleton_data, 0)
	return skeleton_data->getFps();
}

float SpineSkeletonDataResource::get_reference_scale() const {
	SPINE_CHECK(skeleton_data, 100);
	return skeleton_data->getReferenceScale();
}

void SpineSkeletonDataResource::set_reference_scale(float reference_scale) {
	SPINE_CHECK(skeleton_data, )
	skeleton_data->setReferenceScale(reference_scale);
}
