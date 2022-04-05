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

void SpineSkeletonDataResource::_bind_methods() {
	ClassDB::bind_method(D_METHOD("set_atlas_res", "atlas_res"), &SpineSkeletonDataResource::set_atlas_res);
	ClassDB::bind_method(D_METHOD("get_atlas_res"), &SpineSkeletonDataResource::get_atlas_res);
	ClassDB::bind_method(D_METHOD("set_skeleton_json_res", "skeleton_json_res"), &SpineSkeletonDataResource::set_skeleton_json_res);
	ClassDB::bind_method(D_METHOD("get_skeleton_json_res"), &SpineSkeletonDataResource::get_skeleton_json_res);
	ClassDB::bind_method(D_METHOD("is_skeleton_data_loaded"), &SpineSkeletonDataResource::is_skeleton_data_loaded);
	ClassDB::bind_method(D_METHOD("find_animation", "animation_name"), &SpineSkeletonDataResource::find_animation);
	ClassDB::bind_method(D_METHOD("get_sk_name"), &SpineSkeletonDataResource::get_sk_name);
	ClassDB::bind_method(D_METHOD("set_sk_name", "sk_name"), &SpineSkeletonDataResource::set_sk_name);
	ClassDB::bind_method(D_METHOD("get_x"), &SpineSkeletonDataResource::get_x);
	ClassDB::bind_method(D_METHOD("set_x", "v"), &SpineSkeletonDataResource::set_x);
	ClassDB::bind_method(D_METHOD("get_y"), &SpineSkeletonDataResource::get_y);
	ClassDB::bind_method(D_METHOD("set_y", "v"), &SpineSkeletonDataResource::set_y);
	ClassDB::bind_method(D_METHOD("get_width"), &SpineSkeletonDataResource::get_width);
	ClassDB::bind_method(D_METHOD("get_height"), &SpineSkeletonDataResource::get_height);
	ClassDB::bind_method(D_METHOD("get_version"), &SpineSkeletonDataResource::get_version);
	ClassDB::bind_method(D_METHOD("get_fps"), &SpineSkeletonDataResource::get_fps);
	ClassDB::bind_method(D_METHOD("set_fps", "v"), &SpineSkeletonDataResource::set_fps);

	ClassDB::bind_method(D_METHOD("find_bone", "bone_name"), &SpineSkeletonDataResource::find_bone);
	ClassDB::bind_method(D_METHOD("find_slot", "slot_name"), &SpineSkeletonDataResource::find_slot);
	ClassDB::bind_method(D_METHOD("find_skin", "skin_name"), &SpineSkeletonDataResource::find_skin);
	ClassDB::bind_method(D_METHOD("find_event", "event_data_name"), &SpineSkeletonDataResource::find_event);
	ClassDB::bind_method(D_METHOD("find_ik_constraint_data", "constraint_name"), &SpineSkeletonDataResource::find_ik_constraint);
	ClassDB::bind_method(D_METHOD("find_transform_constraint_data", "constraint_name"), &SpineSkeletonDataResource::find_transform_constraint);
	ClassDB::bind_method(D_METHOD("find_path_constraint_data", "constraint_name"), &SpineSkeletonDataResource::find_path_constraint);
	ClassDB::bind_method(D_METHOD("get_all_bone_data"), &SpineSkeletonDataResource::get_bones);
	ClassDB::bind_method(D_METHOD("get_all_slot_data"), &SpineSkeletonDataResource::get_slots);
	ClassDB::bind_method(D_METHOD("get_skins"), &SpineSkeletonDataResource::get_skins);
	ClassDB::bind_method(D_METHOD("get_default_skin"), &SpineSkeletonDataResource::get_default_skin);
	ClassDB::bind_method(D_METHOD("set_default_skin", "v"), &SpineSkeletonDataResource::set_default_skin);
	ClassDB::bind_method(D_METHOD("get_all_event_data"), &SpineSkeletonDataResource::get_events);
	ClassDB::bind_method(D_METHOD("get_animations"), &SpineSkeletonDataResource::get_animations);
	ClassDB::bind_method(D_METHOD("get_all_ik_constraint_data"), &SpineSkeletonDataResource::get_ik_constraints);
	ClassDB::bind_method(D_METHOD("get_all_transform_constraint_data"), &SpineSkeletonDataResource::get_transform_constraints);
	ClassDB::bind_method(D_METHOD("get_all_path_constraint_data"), &SpineSkeletonDataResource::get_path_constraints);

	ADD_SIGNAL(MethodInfo("skeleton_data_loaded"));
	ADD_SIGNAL(MethodInfo("atlas_res_changed"));
	ADD_SIGNAL(MethodInfo("skeleton_json_res_changed"));

	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "atlas_res", PropertyHint::PROPERTY_HINT_RESOURCE_TYPE, "SpineAtlasResource"), "set_atlas_res", "get_atlas_res");
	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "skeleton_json_res", PropertyHint::PROPERTY_HINT_RESOURCE_TYPE, "SpineSkeletonJsonDataResource"), "set_skeleton_json_res", "get_skeleton_json_res");
}

SpineSkeletonDataResource::SpineSkeletonDataResource() : valid(false), spine_object(false), skeleton_data(NULL) {
}
SpineSkeletonDataResource::~SpineSkeletonDataResource() {
	if (skeleton_data && !spine_object) {
		delete skeleton_data;
		skeleton_data = NULL;
	}
}

bool SpineSkeletonDataResource::is_skeleton_data_loaded() const {
	return valid || spine_object;
}

void SpineSkeletonDataResource::load_res(spine::Atlas *a, const String &json_string) {
	if (json_string.empty()) return;
	auto path = get_path();
	spine::SkeletonJson json(a);
	auto temp_skeleton_data = json.readSkeletonData(json_string.utf8());
	if (!temp_skeleton_data) {
		print_error(String("Error happened while loading skeleton json data: ") + path);
		print_error(String("Error msg: ") + json.getError().buffer());
		return;
	}
	if (skeleton_data) {
		delete skeleton_data;
		skeleton_data = NULL;
	}
	skeleton_data = temp_skeleton_data;

	valid = true;
	//	print_line("Skeleton json data loaded!");
}

void SpineSkeletonDataResource::update_skeleton_data() {
	if (atlas_res.is_valid() && skeleton_json_res.is_valid()) {
		load_res(atlas_res->get_spine_atlas(), skeleton_json_res->get_json_string());
		if (valid) {
			emit_signal("skeleton_data_loaded");
		}
	}
}

void SpineSkeletonDataResource::set_atlas_res(const Ref<SpineAtlasResource> &a) {
	atlas_res = a;
	valid = false;
	emit_signal("atlas_res_changed");
	update_skeleton_data();
}
Ref<SpineAtlasResource> SpineSkeletonDataResource::get_atlas_res() {
	if (spine_object) {
		print_line("Getting atlas res from a spine_object skeleton! The result may be NULL!");
	}
	return atlas_res;
}

void SpineSkeletonDataResource::set_skeleton_json_res(const Ref<SpineSkeletonJsonDataResource> &s) {
	skeleton_json_res = s;
	valid = false;
	//	print_line("skeleton_json_res_changed emitted");
	emit_signal("skeleton_json_res_changed");
	update_skeleton_data();
}
Ref<SpineSkeletonJsonDataResource> SpineSkeletonDataResource::get_skeleton_json_res() {
	if (spine_object) {
		print_line("Getting atlas res from a spine_object skeleton! The result may be NULL!");
	}
	return skeleton_json_res;
}

#define CHECK_V                                         \
	if (!is_skeleton_data_loaded()) {                   \
		ERR_PRINT("skeleton data has not loaded yet!"); \
		return;                                         \
	}
#define CHECK_X(x)                                      \
	if (!is_skeleton_data_loaded()) {                   \
		ERR_PRINT("skeleton data has not loaded yet!"); \
		return x;                                       \
	}
#define S_T(x) (spine::String(x.utf8()))
Ref<SpineAnimation> SpineSkeletonDataResource::find_animation(const String &animation_name) {
	CHECK_X(NULL);
	if (animation_name.empty()) {
		return NULL;
	}
	auto a = skeleton_data->findAnimation(S_T(animation_name));
	if (!a) return NULL;
	Ref<SpineAnimation> sa(memnew(SpineAnimation));
	sa->set_spine_object(a);
	return sa;
}
String SpineSkeletonDataResource::get_sk_name() {
	CHECK_X("error");
	return skeleton_data->getName().buffer();
}
void SpineSkeletonDataResource::set_sk_name(const String &v) {
	CHECK_V;
	skeleton_data->setName(S_T(v));
}
float SpineSkeletonDataResource::get_x() {
	CHECK_X(0);
	return skeleton_data->getX();
}
void SpineSkeletonDataResource::set_x(float v) {
	CHECK_V;
	skeleton_data->setX(v);
}
float SpineSkeletonDataResource::get_y() {
	CHECK_X(0);
	return skeleton_data->getY();
}
void SpineSkeletonDataResource::set_y(float v) {
	CHECK_V;
	skeleton_data->setY(v);
}
float SpineSkeletonDataResource::get_width() {
	CHECK_X(0);
	return skeleton_data->getWidth();
}
float SpineSkeletonDataResource::get_height() {
	CHECK_X(0);
	return skeleton_data->getHeight();
}
String SpineSkeletonDataResource::get_version() {
	CHECK_X("error");
	return skeleton_data->getVersion().buffer();
}
float SpineSkeletonDataResource::get_fps() {
	CHECK_X(0);
	return skeleton_data->getFps();
}
void SpineSkeletonDataResource::set_fps(float v) {
	CHECK_V;
	skeleton_data->setFps(v);
}

Ref<SpineBoneData> SpineSkeletonDataResource::find_bone(const String &bone_name) {
	if (bone_name.empty()) return NULL;
	auto b = skeleton_data->findBone(S_T(bone_name));
	if (b == NULL) return NULL;
	Ref<SpineBoneData> gd_b(memnew(SpineBoneData));
	gd_b->set_spine_object(b);
	return gd_b;
}

Ref<SpineSlotData> SpineSkeletonDataResource::find_slot(const String &slot_name) {
	if (slot_name.empty()) return NULL;
	auto b = skeleton_data->findSlot(S_T(slot_name));
	if (b == NULL) return NULL;
	Ref<SpineSlotData> gd_b(memnew(SpineSlotData));
	gd_b->set_spine_object(b);
	return gd_b;
}

Ref<SpineSkin> SpineSkeletonDataResource::find_skin(const String &skin_name) {
	if (skin_name.empty()) return NULL;
	auto b = skeleton_data->findSkin(S_T(skin_name));
	if (b == NULL) return NULL;
	Ref<SpineSkin> gd_b(memnew(SpineSkin));
	gd_b->set_spine_object(b);
	return gd_b;
}

Ref<SpineEventData> SpineSkeletonDataResource::find_event(const String &event_data_name) {
	if (event_data_name.empty()) return NULL;
	auto b = skeleton_data->findEvent(S_T(event_data_name));
	if (b == NULL) return NULL;
	Ref<SpineEventData> gd_b(memnew(SpineEventData));
	gd_b->set_spine_object(b);
	return gd_b;
}

Ref<SpineIkConstraintData> SpineSkeletonDataResource::find_ik_constraint(const String &constraint_name) {
	if (constraint_name.empty()) return NULL;
	auto b = skeleton_data->findIkConstraint(S_T(constraint_name));
	if (b == NULL) return NULL;
	Ref<SpineIkConstraintData> gd_b(memnew(SpineIkConstraintData));
	gd_b->set_spine_object(b);
	return gd_b;
}
Ref<SpineTransformConstraintData> SpineSkeletonDataResource::find_transform_constraint(const String &constraint_name) {
	if (constraint_name.empty()) return NULL;
	auto b = skeleton_data->findTransformConstraint(S_T(constraint_name));
	if (b == NULL) return NULL;
	Ref<SpineTransformConstraintData> gd_b(memnew(SpineTransformConstraintData));
	gd_b->set_spine_object(b);
	return gd_b;
}
Ref<SpinePathConstraintData> SpineSkeletonDataResource::find_path_constraint(const String &constraint_name) {
	if (constraint_name.empty()) return NULL;
	auto b = skeleton_data->findPathConstraint(S_T(constraint_name));
	if (b == NULL) return NULL;
	Ref<SpinePathConstraintData> gd_b(memnew(SpinePathConstraintData));
	gd_b->set_spine_object(b);
	return gd_b;
}

Array SpineSkeletonDataResource::get_bones() {
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
Array SpineSkeletonDataResource::get_slots() {
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
Array SpineSkeletonDataResource::get_skins() const {
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

Ref<SpineSkin> SpineSkeletonDataResource::get_default_skin() {
	auto b = skeleton_data->getDefaultSkin();
	if (b == NULL) return NULL;
	Ref<SpineSkin> gd_b(memnew(SpineSkin));
	gd_b->set_spine_object(b);
	return gd_b;
}
void SpineSkeletonDataResource::set_default_skin(Ref<SpineSkin> v) {
	if (v.is_valid()) {
		skeleton_data->setDefaultSkin(v->get_spine_object());
	} else
		skeleton_data->setDefaultSkin(NULL);
}

Array SpineSkeletonDataResource::get_events() {
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
Array SpineSkeletonDataResource::get_animations() {
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
Array SpineSkeletonDataResource::get_ik_constraints() {
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
Array SpineSkeletonDataResource::get_transform_constraints() {
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
Array SpineSkeletonDataResource::get_path_constraints() {
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
#undef CHECK_X

//External feature functions
void SpineSkeletonDataResource::get_animation_names(Vector<String> &res) const {
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
void SpineSkeletonDataResource::get_skin_names(Vector<String> &res) const {
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

void SpineSkeletonDataResource::_get_property_list(List<PropertyInfo> *p_list) const {
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
