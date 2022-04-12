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

#include "SpineSkeletonDataResource.h"

void SpineAnimationMix::_bind_methods() {
	ClassDB::bind_method(D_METHOD("set_from", "from"), &SpineAnimationMix::set_from);
	ClassDB::bind_method(D_METHOD("get_from"), &SpineAnimationMix::get_from);
	ClassDB::bind_method(D_METHOD("set_to", "to"), &SpineAnimationMix::set_to);
	ClassDB::bind_method(D_METHOD("get_to"), &SpineAnimationMix::get_to);
	ClassDB::bind_method(D_METHOD("set_mix", "mix"), &SpineAnimationMix::set_mix);
	ClassDB::bind_method(D_METHOD("get_mix"), &SpineAnimationMix::get_mix);

	ADD_PROPERTY(PropertyInfo(Variant::STRING, "from"), "set_from", "get_from");
	ADD_PROPERTY(PropertyInfo(Variant::STRING, "to"), "set_to", "get_to");
	ADD_PROPERTY(PropertyInfo(Variant::REAL, "mix"), "set_mix", "get_mix");
}

SpineAnimationMix::SpineAnimationMix() {
}

SpineAnimationMix::~SpineAnimationMix() {
}

void SpineAnimationMix::set_from(const StringName &from) {
	this->from = from;
}

String SpineAnimationMix::get_from() {
	return from;
}

void SpineAnimationMix::set_to(const StringName &to) {
	this->to = to;
}

String SpineAnimationMix::get_to() {
	return to;
}

void SpineAnimationMix::set_mix(float mix) {
	this->mix = mix;
}

float SpineAnimationMix::get_mix() {
	return mix;
}

void SpineSkeletonDataResource::_bind_methods() {
	ClassDB::bind_method(D_METHOD("is_skeleton_data_loaded"), &SpineSkeletonDataResource::is_skeleton_data_loaded);
	ClassDB::bind_method(D_METHOD("set_atlas_res", "atlas_res"), &SpineSkeletonDataResource::set_atlas_res);
	ClassDB::bind_method(D_METHOD("get_atlas_res"), &SpineSkeletonDataResource::get_atlas_res);
	ClassDB::bind_method(D_METHOD("set_skeleton_file_res", "skeleton_file_res"), &SpineSkeletonDataResource::set_skeleton_file_res);
	ClassDB::bind_method(D_METHOD("get_skeleton_file_res"), &SpineSkeletonDataResource::get_skeleton_file_res);
	ClassDB::bind_method(D_METHOD("set_default_mix", "default_mix"), &SpineSkeletonDataResource::set_default_mix);
	ClassDB::bind_method(D_METHOD("get_default_mix"), &SpineSkeletonDataResource::get_default_mix);
	ClassDB::bind_method(D_METHOD("set_animation_mixes", "mixes"), &SpineSkeletonDataResource::set_animation_mixes);
	ClassDB::bind_method(D_METHOD("get_animation_mixes"), &SpineSkeletonDataResource::get_animation_mixes);

	// Spine API
	ClassDB::bind_method(D_METHOD("find_bone", "bone_name"), &SpineSkeletonDataResource::find_bone);
	ClassDB::bind_method(D_METHOD("find_slot", "slot_name"), &SpineSkeletonDataResource::find_slot);
	ClassDB::bind_method(D_METHOD("find_skin", "skin_name"), &SpineSkeletonDataResource::find_skin);
	ClassDB::bind_method(D_METHOD("find_event", "event_data_name"), &SpineSkeletonDataResource::find_event);
	ClassDB::bind_method(D_METHOD("find_animation", "animation_name"), &SpineSkeletonDataResource::find_animation);
	ClassDB::bind_method(D_METHOD("find_ik_constraint_data", "constraint_name"), &SpineSkeletonDataResource::find_ik_constraint);
	ClassDB::bind_method(D_METHOD("find_transform_constraint_data", "constraint_name"), &SpineSkeletonDataResource::find_transform_constraint);
	ClassDB::bind_method(D_METHOD("find_path_constraint_data", "constraint_name"), &SpineSkeletonDataResource::find_path_constraint);
	ClassDB::bind_method(D_METHOD("get_skeleton_name"), &SpineSkeletonDataResource::get_skeleton_name);
	ClassDB::bind_method(D_METHOD("get_bones"), &SpineSkeletonDataResource::get_bones);
	ClassDB::bind_method(D_METHOD("get_slots"), &SpineSkeletonDataResource::get_slots);
	ClassDB::bind_method(D_METHOD("get_skins"), &SpineSkeletonDataResource::get_skins);
	ClassDB::bind_method(D_METHOD("get_default_skin"), &SpineSkeletonDataResource::get_default_skin);
	ClassDB::bind_method(D_METHOD("set_default_skin", "skin"), &SpineSkeletonDataResource::set_default_skin);
	ClassDB::bind_method(D_METHOD("get_events"), &SpineSkeletonDataResource::get_events);
	ClassDB::bind_method(D_METHOD("get_animations"), &SpineSkeletonDataResource::get_animations);
	ClassDB::bind_method(D_METHOD("get_ik_constraints"), &SpineSkeletonDataResource::get_ik_constraints);
	ClassDB::bind_method(D_METHOD("get_transform_constraints"), &SpineSkeletonDataResource::get_transform_constraints);
	ClassDB::bind_method(D_METHOD("get_path_constraints"), &SpineSkeletonDataResource::get_path_constraints);
	ClassDB::bind_method(D_METHOD("get_x"), &SpineSkeletonDataResource::get_x);
	ClassDB::bind_method(D_METHOD("get_y"), &SpineSkeletonDataResource::get_y);
	ClassDB::bind_method(D_METHOD("get_width"), &SpineSkeletonDataResource::get_width);
	ClassDB::bind_method(D_METHOD("get_height"), &SpineSkeletonDataResource::get_height);
	ClassDB::bind_method(D_METHOD("get_version"), &SpineSkeletonDataResource::get_version);
	ClassDB::bind_method(D_METHOD("get_hash"), &SpineSkeletonDataResource::get_hash);
	ClassDB::bind_method(D_METHOD("get_images_path"), &SpineSkeletonDataResource::get_images_path);
	ClassDB::bind_method(D_METHOD("get_audio_path"), &SpineSkeletonDataResource::get_audio_path);
	ClassDB::bind_method(D_METHOD("get_fps"), &SpineSkeletonDataResource::get_fps);

	ADD_SIGNAL(MethodInfo("skeleton_data_changed"));

	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "atlas_res", PropertyHint::PROPERTY_HINT_RESOURCE_TYPE, "SpineAtlasResource"), "set_atlas_res", "get_atlas_res");
	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "skeleton_file_res", PropertyHint::PROPERTY_HINT_RESOURCE_TYPE, "SpineSkeletonFileResource"), "set_skeleton_file_res", "get_skeleton_file_res");
	ADD_PROPERTY(PropertyInfo(Variant::REAL, "default_mix"), "set_default_mix", "get_default_mix");
	ADD_GROUP("Animation mixes", "");
	ADD_PROPERTY(PropertyInfo(Variant::ARRAY, "animation_mixes"), "set_animation_mixes", "get_animation_mixes");
}

SpineSkeletonDataResource::SpineSkeletonDataResource() : skeleton_data(nullptr), animation_state_data(nullptr), default_mix(0) {
}

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

	if (atlas_res.is_valid() && skeleton_file_res.is_valid()) {
		load_res(atlas_res->get_spine_atlas(), skeleton_file_res->get_json(), skeleton_file_res->get_binary());
	}
	emit_signal("skeleton_data_changed");
#ifdef TOOLS_ENABLED
	property_list_changed_notify();
#endif
}

void SpineSkeletonDataResource::load_res(spine::Atlas *atlas, const String &json, const Vector<uint8_t> &binary) {
	if ((json.empty() && binary.empty()) || atlas == nullptr) return;

	spine::SkeletonData *data;
	if (!json.empty()) {
		spine::SkeletonJson skeletonJson(atlas);
		data = skeletonJson.readSkeletonData(json.utf8());
		if (!data) {
			print_error(String("Error while loading skeleton data: ") + get_path());
			print_error(String("Error message: ") + skeletonJson.getError().buffer());
			return;
		}
	} else {
		spine::SkeletonBinary skeletonBinary(atlas);
		data = skeletonBinary.readSkeletonData(binary.ptr(), binary.size());
		if (!data) {
			print_error(String("Error while loading skeleton data: ") + get_path());
			print_error(String("Error message: ") + skeletonBinary.getError().buffer());
			return;
		}
	}
	skeleton_data = data;
	animation_state_data = new spine::AnimationStateData(data);
}

bool SpineSkeletonDataResource::is_skeleton_data_loaded() const {
	return skeleton_data != nullptr;
}

void SpineSkeletonDataResource::set_atlas_res(const Ref<SpineAtlasResource> &atlas) {
	atlas_res = atlas;
	update_skeleton_data();
}

Ref<SpineAtlasResource> SpineSkeletonDataResource::get_atlas_res() {
	return atlas_res;
}

void SpineSkeletonDataResource::set_skeleton_file_res(const Ref<SpineSkeletonFileResource> &skeleton_file) {
	skeleton_file_res = skeleton_file;
	update_skeleton_data();
}

Ref<SpineSkeletonFileResource> SpineSkeletonDataResource::get_skeleton_file_res() {
	return skeleton_file_res;
}

void SpineSkeletonDataResource::get_animation_names(Vector<String> &animation_names) const {
	animation_names.clear();
	if (!is_skeleton_data_loaded()) return;
	auto animations = skeleton_data->getAnimations();
	for (size_t i = 0; i < animations.size(); ++i) {
		auto animation = animations[i];
		animation_names.push_back(animation->getName().buffer());
	}
}

void SpineSkeletonDataResource::get_skin_names(Vector<String> &skin_names) const {
	skin_names.clear();
	if (!is_skeleton_data_loaded()) return;
	auto skins = skeleton_data->getSkins();
	for (size_t i = 0; i < skins.size(); ++i) {
		auto skin = skins[i];
		skin_names.push_back(skin->getName().buffer());
	}
}

void SpineSkeletonDataResource::set_default_mix(float default_mix) {
	this->default_mix = default_mix;
	if (animation_state_data) animation_state_data->setDefaultMix(default_mix);
}

float SpineSkeletonDataResource::get_default_mix() {
	return default_mix;
}

void SpineSkeletonDataResource::set_animation_mixes(Array animation_mixes) {
	this->animation_mixes = animation_mixes;
}

Array SpineSkeletonDataResource::get_animation_mixes() {
	return animation_mixes;
}

#define CHECK(x)                                      \
	if (!is_skeleton_data_loaded()) {                   \
		ERR_PRINT("skeleton data has not loaded yet!"); \
		return x;                                       \
	}

#define S_T(x) (spine::String((x).utf8()))
Ref<SpineAnimation> SpineSkeletonDataResource::find_animation(const String &animation_name) const {
	CHECK(nullptr)
	if (animation_name.empty()) return nullptr;
	auto animation = skeleton_data->findAnimation(S_T(animation_name));
	if (!animation) return nullptr;
	Ref<SpineAnimation> animation_ref(memnew(SpineAnimation));
	animation_ref->set_spine_object(animation);
	return animation_ref;
}

Ref<SpineBoneData> SpineSkeletonDataResource::find_bone(const String &bone_name) const {
	CHECK(nullptr)
	if (bone_name.empty()) return nullptr;
	auto bone = skeleton_data->findBone(S_T(bone_name));
	if (bone == nullptr) return nullptr;
	Ref<SpineBoneData> bone_ref(memnew(SpineBoneData));
	bone_ref->set_spine_object(bone);
	return bone_ref;
}

Ref<SpineSlotData> SpineSkeletonDataResource::find_slot(const String &slot_name) const {
	CHECK(nullptr)
	if (slot_name.empty()) return nullptr;
	auto slot = skeleton_data->findSlot(S_T(slot_name));
	if (slot == nullptr) return nullptr;
	Ref<SpineSlotData> slot_ref(memnew(SpineSlotData));
	slot_ref->set_spine_object(slot);
	return slot_ref;
}

Ref<SpineSkin> SpineSkeletonDataResource::find_skin(const String &skin_name) const {
	CHECK(nullptr)
	if (skin_name.empty()) return nullptr;
	auto skin = skeleton_data->findSkin(S_T(skin_name));
	if (skin == nullptr) return nullptr;
	Ref<SpineSkin> skin_ref(memnew(SpineSkin));
	skin_ref->set_spine_object(skin);
	return skin_ref;
}

Ref<SpineEventData> SpineSkeletonDataResource::find_event(const String &event_data_name) const {
	CHECK(nullptr)
	if (event_data_name.empty()) return nullptr;
	auto event = skeleton_data->findEvent(S_T(event_data_name));
	if (event == nullptr) return nullptr;
	Ref<SpineEventData> event_ref(memnew(SpineEventData));
	event_ref->set_spine_object(event);
	return event_ref;
}

Ref<SpineIkConstraintData> SpineSkeletonDataResource::find_ik_constraint(const String &constraint_name) const {
	CHECK(nullptr)
	if (constraint_name.empty()) return nullptr;
	auto constraint = skeleton_data->findIkConstraint(S_T(constraint_name));
	if (constraint == nullptr) return nullptr;
	Ref<SpineIkConstraintData> constraint_ref(memnew(SpineIkConstraintData));
	constraint_ref->set_spine_object(constraint);
	return constraint_ref;
}
Ref<SpineTransformConstraintData> SpineSkeletonDataResource::find_transform_constraint(const String &constraint_name) const {
	CHECK(nullptr)
	if (constraint_name.empty()) return nullptr;
	auto constraint = skeleton_data->findTransformConstraint(S_T(constraint_name));
	if (constraint == nullptr) return nullptr;
	Ref<SpineTransformConstraintData> constraint_ref(memnew(SpineTransformConstraintData));
	constraint_ref->set_spine_object(constraint);
	return constraint_ref;
}
Ref<SpinePathConstraintData> SpineSkeletonDataResource::find_path_constraint(const String &constraint_name) const {
	CHECK(nullptr)
	if (constraint_name.empty()) return nullptr;
	auto constraint = skeleton_data->findPathConstraint(S_T(constraint_name));
	if (constraint == nullptr) return nullptr;
	Ref<SpinePathConstraintData> constraint_ref(memnew(SpinePathConstraintData));
	constraint_ref->set_spine_object(constraint);
	return constraint_ref;
}

String SpineSkeletonDataResource::get_skeleton_name() const{
	CHECK("")
	return skeleton_data->getName().buffer();
}

Array SpineSkeletonDataResource::get_bones() const {
	Array bone_refs;
	CHECK(bone_refs)
	auto bones = skeleton_data->getBones();
	bone_refs.resize((int)bones.size());
	for (int i = 0; i < bones.size(); ++i) {
		Ref<SpineBoneData> bone_ref(memnew(SpineBoneData));
		bone_ref->set_spine_object(bones[i]);
		bone_refs[i] = bone_ref;
	}
	return bone_refs;
}

Array SpineSkeletonDataResource::get_slots() const {
	Array slot_refs;
	CHECK(slot_refs)
	auto slots = skeleton_data->getSlots();
	slot_refs.resize((int)slots.size());
	for (int i = 0; i < slots.size(); ++i) {
		Ref<SpineSlotData> slot_ref(memnew(SpineSlotData));
		slot_ref->set_spine_object(slots[i]);
		slot_refs[i] = slot_ref;
	}
	return slot_refs;
}

Array SpineSkeletonDataResource::get_skins() const {
	Array skin_refs;
	CHECK(skin_refs)
	auto skins = skeleton_data->getSkins();
	skin_refs.resize((int)skins.size());
	for (int i = 0; i < skins.size(); ++i) {
		Ref<SpineSkin> skin_ref(memnew(SpineSkin));
		skin_ref->set_spine_object(skins[i]);
		skin_refs[i] = skin_ref;
	}
	return skin_refs;
}

Ref<SpineSkin> SpineSkeletonDataResource::get_default_skin() const {
	CHECK(nullptr)
	auto skin = skeleton_data->getDefaultSkin();
	if (skin == nullptr) return nullptr;
	Ref<SpineSkin> skin_ref(memnew(SpineSkin));
	skin_ref->set_spine_object(skin);
	return skin_ref;
}

void SpineSkeletonDataResource::set_default_skin(Ref<SpineSkin> skin) {
	CHECK()
	if (skin.is_valid())
		skeleton_data->setDefaultSkin(skin->get_spine_object());
	else
		skeleton_data->setDefaultSkin(nullptr);
}

Array SpineSkeletonDataResource::get_events() const {
	Array event_refs;
	CHECK(event_refs)
	auto events = skeleton_data->getEvents();
	event_refs.resize((int)events.size());
	for (int i = 0; i < events.size(); ++i) {
		Ref<SpineEventData> event_ref(memnew(SpineEventData));
		event_ref->set_spine_object(events[i]);
		event_refs[i] = event_ref;
	}
	return event_refs;
}

Array SpineSkeletonDataResource::get_animations() const {
	Array animation_refs;
	CHECK(animation_refs)
	auto animations = skeleton_data->getAnimations();
	animation_refs.resize((int)animations.size());
	for (int i = 0; i < animations.size(); ++i) {
		Ref<SpineAnimation> animation_ref(memnew(SpineAnimation));
		animation_ref->set_spine_object(animations[i]);
		animation_refs[i] = animation_ref;
	}
	return animation_refs;
}

Array SpineSkeletonDataResource::get_ik_constraints() const {
	Array constraint_refs;
	CHECK(constraint_refs)
	auto constraints = skeleton_data->getIkConstraints();
	constraint_refs.resize((int)constraints.size());
	for (int i = 0; i < constraints.size(); ++i) {
		Ref<SpineIkConstraintData> constraint_ref(memnew(SpineIkConstraintData));
		constraint_ref->set_spine_object(constraints[i]);
		constraint_refs[i] = constraint_ref;
	}
	return constraint_refs;
}

Array SpineSkeletonDataResource::get_transform_constraints() const {
	Array constraint_refs;
	CHECK(constraint_refs)
	auto constraints = skeleton_data->getTransformConstraints();
	constraint_refs.resize((int)constraints.size());
	for (int i = 0; i < constraints.size(); ++i) {
		Ref<SpineTransformConstraintData> constraint_ref(memnew(SpineTransformConstraintData));
		constraint_ref->set_spine_object(constraints[i]);
		constraint_refs[i] = constraint_ref;
	}
	return constraint_refs;
}

Array SpineSkeletonDataResource::get_path_constraints() const {
	Array constraint_refs;
	CHECK(constraint_refs)
	auto constraints = skeleton_data->getPathConstraints();
	constraint_refs.resize((int)constraints.size());
	for (int i = 0; i < constraints.size(); ++i) {
		Ref<SpinePathConstraintData> constraint_ref(memnew(SpinePathConstraintData));
		constraint_ref->set_spine_object(constraints[i]);
		constraint_refs[i] = constraint_ref;
	}
	return constraint_refs;
}

float SpineSkeletonDataResource::get_x() const{
	CHECK(0)
	return skeleton_data->getX();
}

float SpineSkeletonDataResource::get_y() const {
	CHECK(0)
	return skeleton_data->getY();
}

float SpineSkeletonDataResource::get_width() const{
	CHECK(0)
	return skeleton_data->getWidth();
}

float SpineSkeletonDataResource::get_height() const {
	CHECK(0)
	return skeleton_data->getHeight();
}

String SpineSkeletonDataResource::get_version() const {
	CHECK("")
	return skeleton_data->getVersion().buffer();
}

String SpineSkeletonDataResource::get_hash() const {
	CHECK("")
	return skeleton_data->getHash().buffer();
}


String SpineSkeletonDataResource::get_images_path() const {
	CHECK("")
	return skeleton_data->getImagesPath().buffer();
}

String SpineSkeletonDataResource::get_audio_path() const {
	CHECK("")
	return skeleton_data->getAudioPath().buffer();
}

float SpineSkeletonDataResource::get_fps() const {
	CHECK(0)
	return skeleton_data->getFps();
}
