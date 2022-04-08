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

#include "SpineNewSkeletonDataResource.h"

void SpineNewSkeletonDataResource::_bind_methods() {
	ClassDB::bind_method(D_METHOD("set_atlas_res", "atlas_res"), &SpineNewSkeletonDataResource::set_atlas_res);
	ClassDB::bind_method(D_METHOD("get_atlas_res"), &SpineNewSkeletonDataResource::get_atlas_res);
	ClassDB::bind_method(D_METHOD("set_skeleton_file_res", "skeleton_file_res"), &SpineNewSkeletonDataResource::set_skeleton_file_res);
	ClassDB::bind_method(D_METHOD("get_skeleton_file_res"), &SpineNewSkeletonDataResource::get_skeleton_file_res);
	ClassDB::bind_method(D_METHOD("is_skeleton_data_loaded"), &SpineNewSkeletonDataResource::is_skeleton_data_loaded);
	ClassDB::bind_method(D_METHOD("find_animation", "animation_name"), &SpineNewSkeletonDataResource::find_animation);
	ClassDB::bind_method(D_METHOD("get_skeleton_name"), &SpineNewSkeletonDataResource::get_skeleton_name);
	ClassDB::bind_method(D_METHOD("set_skeleton_name", "skeleton_name"), &SpineNewSkeletonDataResource::set_skeleton_name);
	ClassDB::bind_method(D_METHOD("get_x"), &SpineNewSkeletonDataResource::get_x);
	ClassDB::bind_method(D_METHOD("set_x", "v"), &SpineNewSkeletonDataResource::set_x);
	ClassDB::bind_method(D_METHOD("get_y"), &SpineNewSkeletonDataResource::get_y);
	ClassDB::bind_method(D_METHOD("set_y", "v"), &SpineNewSkeletonDataResource::set_y);
	ClassDB::bind_method(D_METHOD("get_width"), &SpineNewSkeletonDataResource::get_width);
	ClassDB::bind_method(D_METHOD("get_height"), &SpineNewSkeletonDataResource::get_height);
	ClassDB::bind_method(D_METHOD("get_version"), &SpineNewSkeletonDataResource::get_version);
	ClassDB::bind_method(D_METHOD("get_fps"), &SpineNewSkeletonDataResource::get_fps);
	ClassDB::bind_method(D_METHOD("set_fps", "v"), &SpineNewSkeletonDataResource::set_fps);

	ClassDB::bind_method(D_METHOD("find_bone", "bone_name"), &SpineNewSkeletonDataResource::find_bone);
	ClassDB::bind_method(D_METHOD("find_slot", "slot_name"), &SpineNewSkeletonDataResource::find_slot);
	ClassDB::bind_method(D_METHOD("find_skin", "skin_name"), &SpineNewSkeletonDataResource::find_skin);
	ClassDB::bind_method(D_METHOD("find_event", "event_data_name"), &SpineNewSkeletonDataResource::find_event);
	ClassDB::bind_method(D_METHOD("find_ik_constraint_data", "constraint_name"), &SpineNewSkeletonDataResource::find_ik_constraint);
	ClassDB::bind_method(D_METHOD("find_transform_constraint_data", "constraint_name"), &SpineNewSkeletonDataResource::find_transform_constraint);
	ClassDB::bind_method(D_METHOD("find_path_constraint_data", "constraint_name"), &SpineNewSkeletonDataResource::find_path_constraint);
	ClassDB::bind_method(D_METHOD("get_all_bone_data"), &SpineNewSkeletonDataResource::get_bones);
	ClassDB::bind_method(D_METHOD("get_all_slot_data"), &SpineNewSkeletonDataResource::get_slots);
	ClassDB::bind_method(D_METHOD("get_skins"), &SpineNewSkeletonDataResource::get_skins);
	ClassDB::bind_method(D_METHOD("get_default_skin"), &SpineNewSkeletonDataResource::get_default_skin);
	ClassDB::bind_method(D_METHOD("set_default_skin", "v"), &SpineNewSkeletonDataResource::set_default_skin);
	ClassDB::bind_method(D_METHOD("get_all_event_data"), &SpineNewSkeletonDataResource::get_events);
	ClassDB::bind_method(D_METHOD("get_animations"), &SpineNewSkeletonDataResource::get_animations);
	ClassDB::bind_method(D_METHOD("get_all_ik_constraint_data"), &SpineNewSkeletonDataResource::get_ik_constraints);
	ClassDB::bind_method(D_METHOD("get_all_transform_constraint_data"), &SpineNewSkeletonDataResource::get_transform_constraints);
	ClassDB::bind_method(D_METHOD("get_all_path_constraint_data"), &SpineNewSkeletonDataResource::get_path_constraints);

	ADD_SIGNAL(MethodInfo("skeleton_data_loaded"));
	ADD_SIGNAL(MethodInfo("atlas_res_changed"));
	ADD_SIGNAL(MethodInfo("skeleton_file_res_changed"));

	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "atlas_res", PropertyHint::PROPERTY_HINT_RESOURCE_TYPE, "SpineAtlasResource"), "set_atlas_res", "get_atlas_res");
	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "skeleton_file_res", PropertyHint::PROPERTY_HINT_RESOURCE_TYPE, "SpineSkeletonFileResource"), "set_skeleton_file_res", "get_skeleton_file_res");
}

SpineNewSkeletonDataResource::SpineNewSkeletonDataResource() : valid(false), spine_object(false), skeleton_data(NULL) {
}

SpineNewSkeletonDataResource::~SpineNewSkeletonDataResource() {
	if (skeleton_data && !spine_object) {
		delete skeleton_data;
		skeleton_data = NULL;
	}
}

bool SpineNewSkeletonDataResource::is_skeleton_data_loaded() const {
	return valid || spine_object;
}

void SpineNewSkeletonDataResource::load_res(spine::Atlas *atlas, const String &json, const Vector<uint8_t> &binary) {
	valid = false;
	if (skeleton_data) {
		delete skeleton_data;
		skeleton_data = NULL;
	}

	if ((json.empty() && binary.empty()) || atlas == NULL) return;

	spine::SkeletonData *skeletonData = NULL;
	if (!json.empty()) {
		spine::SkeletonJson skeletonJson(atlas);
		skeletonData = skeletonJson.readSkeletonData(json.utf8());
		if (!skeletonData) {
			print_error(String("Error while loading skeleton data: ") + get_path());
			print_error(String("Error message: ") + skeletonJson.getError().buffer());
			return;
		}
	} else {
		spine::SkeletonBinary skeletonBinary(atlas);
		skeletonData = skeletonBinary.readSkeletonData(binary.ptr(), binary.size());
		if (!skeletonData) {
			print_error(String("Error while loading skeleton data: ") + get_path());
			print_error(String("Error message: ") + skeletonBinary.getError().buffer());
			return;
		}
	}
	skeleton_data = skeletonData;
	valid = true;
}

void SpineNewSkeletonDataResource::update_skeleton_data() {
	if (atlas_res.is_valid() && skeleton_file_res.is_valid()) {
		load_res(atlas_res->get_spine_atlas(), skeleton_file_res->get_json(), skeleton_file_res->get_binary());
		if (valid) {
			emit_signal("skeleton_data_loaded");
		}
	}
}

void SpineNewSkeletonDataResource::set_atlas_res(const Ref<SpineAtlasResource> &a) {
	atlas_res = a;
	valid = false;
	emit_signal("atlas_res_changed");
	update_skeleton_data();
}
Ref<SpineAtlasResource> SpineNewSkeletonDataResource::get_atlas_res() {
	return atlas_res;
}

void SpineNewSkeletonDataResource::set_skeleton_file_res(const Ref<SpineSkeletonFileResource> &s) {
	skeleton_file_res = s;
	valid = false;
	emit_signal("skeleton_file_res_changed");
	update_skeleton_data();
}
Ref<SpineSkeletonFileResource> SpineNewSkeletonDataResource::get_skeleton_file_res() {
	return skeleton_file_res;
}

#define CHECK(x)                                      \
	if (!is_skeleton_data_loaded()) {                   \
		ERR_PRINT("skeleton data has not loaded yet!"); \
		return x;                                       \
	}

#define S_T(x) (spine::String(x.utf8()))
Ref<SpineAnimation> SpineNewSkeletonDataResource::find_animation(const String &animation_name) {
	CHECK(NULL);
	if (animation_name.empty()) {
		return NULL;
	}
	auto a = skeleton_data->findAnimation(S_T(animation_name));
	if (!a) return NULL;
	Ref<SpineAnimation> sa(memnew(SpineAnimation));
	sa->set_spine_object(a);
	return sa;
}
String SpineNewSkeletonDataResource::get_skeleton_name() {
	CHECK("");
	return skeleton_data->getName().buffer();
}

void SpineNewSkeletonDataResource::set_skeleton_name(const String &v) {
	CHECK();
	skeleton_data->setName(S_T(v));
}

float SpineNewSkeletonDataResource::get_x() {
	CHECK(0);
	return skeleton_data->getX();
}

void SpineNewSkeletonDataResource::set_x(float v) {
	CHECK();
	skeleton_data->setX(v);
}
float SpineNewSkeletonDataResource::get_y() {
	CHECK(0);
	return skeleton_data->getY();
}
void SpineNewSkeletonDataResource::set_y(float v) {
	CHECK();
	skeleton_data->setY(v);
}
float SpineNewSkeletonDataResource::get_width() {
	CHECK(0);
	return skeleton_data->getWidth();
}
float SpineNewSkeletonDataResource::get_height() {
	CHECK(0);
	return skeleton_data->getHeight();
}
String SpineNewSkeletonDataResource::get_version() {
	CHECK("error");
	return skeleton_data->getVersion().buffer();
}
float SpineNewSkeletonDataResource::get_fps() {
	CHECK(0);
	return skeleton_data->getFps();
}
void SpineNewSkeletonDataResource::set_fps(float v) {
	CHECK();
	skeleton_data->setFps(v);
}

Ref<SpineBoneData> SpineNewSkeletonDataResource::find_bone(const String &bone_name) {
	if (bone_name.empty()) return NULL;
	auto b = skeleton_data->findBone(S_T(bone_name));
	if (b == NULL) return NULL;
	Ref<SpineBoneData> gd_b(memnew(SpineBoneData));
	gd_b->set_spine_object(b);
	return gd_b;
}

Ref<SpineSlotData> SpineNewSkeletonDataResource::find_slot(const String &slot_name) {
	if (slot_name.empty()) return NULL;
	auto b = skeleton_data->findSlot(S_T(slot_name));
	if (b == NULL) return NULL;
	Ref<SpineSlotData> gd_b(memnew(SpineSlotData));
	gd_b->set_spine_object(b);
	return gd_b;
}

Ref<SpineSkin> SpineNewSkeletonDataResource::find_skin(const String &skin_name) {
	if (skin_name.empty()) return NULL;
	auto b = skeleton_data->findSkin(S_T(skin_name));
	if (b == NULL) return NULL;
	Ref<SpineSkin> gd_b(memnew(SpineSkin));
	gd_b->set_spine_object(b);
	return gd_b;
}

Ref<SpineEventData> SpineNewSkeletonDataResource::find_event(const String &event_data_name) {
	if (event_data_name.empty()) return NULL;
	auto b = skeleton_data->findEvent(S_T(event_data_name));
	if (b == NULL) return NULL;
	Ref<SpineEventData> gd_b(memnew(SpineEventData));
	gd_b->set_spine_object(b);
	return gd_b;
}

Ref<SpineIkConstraintData> SpineNewSkeletonDataResource::find_ik_constraint(const String &constraint_name) {
	if (constraint_name.empty()) return NULL;
	auto b = skeleton_data->findIkConstraint(S_T(constraint_name));
	if (b == NULL) return NULL;
	Ref<SpineIkConstraintData> gd_b(memnew(SpineIkConstraintData));
	gd_b->set_spine_object(b);
	return gd_b;
}
Ref<SpineTransformConstraintData> SpineNewSkeletonDataResource::find_transform_constraint(const String &constraint_name) {
	if (constraint_name.empty()) return NULL;
	auto b = skeleton_data->findTransformConstraint(S_T(constraint_name));
	if (b == NULL) return NULL;
	Ref<SpineTransformConstraintData> gd_b(memnew(SpineTransformConstraintData));
	gd_b->set_spine_object(b);
	return gd_b;
}
Ref<SpinePathConstraintData> SpineNewSkeletonDataResource::find_path_constraint(const String &constraint_name) {
	if (constraint_name.empty()) return NULL;
	auto b = skeleton_data->findPathConstraint(S_T(constraint_name));
	if (b == NULL) return NULL;
	Ref<SpinePathConstraintData> gd_b(memnew(SpinePathConstraintData));
	gd_b->set_spine_object(b);
	return gd_b;
}

Array SpineNewSkeletonDataResource::get_bones() {
	auto bs = skeleton_data->getBones();
	Array gd_bs;
	gd_bs.resize(bs.size());
	for (size_t i = 0; i < bs.size(); ++i) {
		if (bs[i] == NULL) gd_bs[i] = Ref<SpineBoneData>(NULL);
		else {
			Ref<SpineBoneData> gd_b(memnew(SpineBoneData));
			gd_b->set_spine_object(bs[i]);
			gd_bs[i] = gd_b;
		}
	}
	return gd_bs;
}
Array SpineNewSkeletonDataResource::get_slots() {
	auto bs = skeleton_data->getSlots();
	Array gd_bs;
	gd_bs.resize(bs.size());
	for (size_t i = 0; i < bs.size(); ++i) {
		if (bs[i] == NULL) gd_bs[i] = Ref<SpineSlotData>(NULL);
		else {
			Ref<SpineSlotData> gd_b(memnew(SpineSlotData));
			gd_b->set_spine_object(bs[i]);
			gd_bs[i] = gd_b;
		}
	}
	return gd_bs;
}
Array SpineNewSkeletonDataResource::get_skins() const {
	auto bs = skeleton_data->getSkins();
	Array gd_bs;
	gd_bs.resize(bs.size());
	for (size_t i = 0; i < bs.size(); ++i) {
		if (bs[i] == NULL) gd_bs[i] = Ref<SpineSkin>(NULL);
		else {
			Ref<SpineSkin> gd_b(memnew(SpineSkin));
			gd_b->set_spine_object(bs[i]);
			gd_bs[i] = gd_b;
		}
	}
	return gd_bs;
}

Ref<SpineSkin> SpineNewSkeletonDataResource::get_default_skin() {
	auto b = skeleton_data->getDefaultSkin();
	if (b == NULL) return NULL;
	Ref<SpineSkin> gd_b(memnew(SpineSkin));
	gd_b->set_spine_object(b);
	return gd_b;
}
void SpineNewSkeletonDataResource::set_default_skin(Ref<SpineSkin> v) {
	if (v.is_valid()) {
		skeleton_data->setDefaultSkin(v->get_spine_object());
	} else
		skeleton_data->setDefaultSkin(NULL);
}

Array SpineNewSkeletonDataResource::get_events() {
	auto bs = skeleton_data->getEvents();
	Array gd_bs;
	gd_bs.resize(bs.size());
	for (size_t i = 0; i < bs.size(); ++i) {
		if (bs[i] == NULL) gd_bs[i] = Ref<SpineEventData>(NULL);
		else {
			Ref<SpineEventData> gd_b(memnew(SpineEventData));
			gd_b->set_spine_object(bs[i]);
			gd_bs[i] = gd_b;
		}
	}
	return gd_bs;
}
Array SpineNewSkeletonDataResource::get_animations() {
	auto bs = skeleton_data->getAnimations();
	Array gd_bs;
	gd_bs.resize(bs.size());
	for (size_t i = 0; i < bs.size(); ++i) {
		if (bs[i] == NULL) gd_bs[i] = Ref<SpineAnimation>(NULL);
		else {
			Ref<SpineAnimation> gd_b(memnew(SpineAnimation));
			gd_b->set_spine_object(bs[i]);
			gd_bs[i] = gd_b;
		}
	}
	return gd_bs;
}
Array SpineNewSkeletonDataResource::get_ik_constraints() {
	auto bs = skeleton_data->getIkConstraints();
	Array gd_bs;
	gd_bs.resize(bs.size());
	for (size_t i = 0; i < bs.size(); ++i) {
		if (bs[i] == NULL) gd_bs[i] = Ref<SpineIkConstraintData>(NULL);
		else {
			Ref<SpineIkConstraintData> gd_b(memnew(SpineIkConstraintData));
			gd_b->set_spine_object(bs[i]);
			gd_bs[i] = gd_b;
		}
	}
	return gd_bs;
}
Array SpineNewSkeletonDataResource::get_transform_constraints() {
	auto bs = skeleton_data->getTransformConstraints();
	Array gd_bs;
	gd_bs.resize(bs.size());
	for (size_t i = 0; i < bs.size(); ++i) {
		if (bs[i] == NULL) gd_bs[i] = Ref<SpineTransformConstraintData>(NULL);
		else {
			Ref<SpineTransformConstraintData> gd_b(memnew(SpineTransformConstraintData));
			gd_b->set_spine_object(bs[i]);
			gd_bs[i] = gd_b;
		}
	}
	return gd_bs;
}
Array SpineNewSkeletonDataResource::get_path_constraints() {
	auto bs = skeleton_data->getPathConstraints();
	Array gd_bs;
	gd_bs.resize(bs.size());
	for (size_t i = 0; i < bs.size(); ++i) {
		if (bs[i] == NULL) gd_bs[i] = Ref<SpinePathConstraintData>(NULL);
		else {
			Ref<SpinePathConstraintData> gd_b(memnew(SpinePathConstraintData));
			gd_b->set_spine_object(bs[i]);
			gd_bs[i] = gd_b;
		}
	}
	return gd_bs;
}
#undef S_T
#undef CHECK_V
#undef CHECK

//External feature functions
void SpineNewSkeletonDataResource::get_animation_names(Vector<String> &res) const {
	res.clear();
	if (!is_skeleton_data_loaded()) {
		return;
	}
	auto as = skeleton_data->getAnimations();
	for (size_t i = 0; i < as.size(); ++i) {
		auto a = as[i];
		if (a) {
			res.push_back(a->getName().buffer());
		} else {
			res.push_back("");
		}
	}
}
void SpineNewSkeletonDataResource::get_skin_names(Vector<String> &res) const {
	res.clear();
	if (!is_skeleton_data_loaded()) return;
	auto as = get_skins();
	res.resize(as.size());
	for (size_t i = 0; i < as.size(); ++i) {
		auto a = Ref<SpineSkin>(as[i]);
		if (a.is_valid()) {
			res.set(i, a->get_skin_name());
		} else {
			res.set(i, "");
		}
	}
}

void SpineNewSkeletonDataResource::_get_property_list(List<PropertyInfo> *p_list) const {
	PropertyInfo p;
	Vector<String> res;

	p.name = "animations";
	p.type = Variant::STRING;
	get_animation_names(res);
	p.hint_string = String(",").join(res);
	p.hint = PROPERTY_HINT_ENUM;
	p_list->push_back(p);

	p.name = "skins";
	p.type = Variant::STRING;
	get_skin_names(res);
	p.hint_string = String(",").join(res);
	p.hint = PROPERTY_HINT_ENUM;
	p_list->push_back(p);
}
