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

#include "SpineSkeleton.h"

void SpineSkeleton::_bind_methods() {
	//void update_world_transform();
	//
	//	void set_to_setup_pose();
	//
	//	void set_bones_to_setup_pose();
	//
	//	void set_slots_to_setup_pose();
	ClassDB::bind_method(D_METHOD("update_world_transform"), &SpineSkeleton::update_world_transform);
	ClassDB::bind_method(D_METHOD("set_to_setup_pose"), &SpineSkeleton::set_to_setup_pose);
	ClassDB::bind_method(D_METHOD("set_bones_to_setup_pose"), &SpineSkeleton::set_bones_to_setup_pose);
	ClassDB::bind_method(D_METHOD("set_slots_to_setup_pose"), &SpineSkeleton::set_slots_to_setup_pose);
	//
	//	Ref<SpineBone> find_bone(const String &name);
	//	int find_bone_index(const String &name);
	//
	//	Ref<SpineSlot> find_slot(const String &name);
	//	int find_slot_index(const String &name);
	//
	//	void set_skin_by_name(const String &skin_name);
	//	void set_skin(Ref<SpineSkin> new_skin);
	//
	//	Ref<SpineAttachment> get_attachment_by_slot_name(const String &slot_name, const String &attachment_name);
	//	Ref<SpineAttachment> get_attachment_by_slot_index(int slot_index, const String &attachment_name);
	ClassDB::bind_method(D_METHOD("find_bone", "bone_name"), &SpineSkeleton::find_bone);
	ClassDB::bind_method(D_METHOD("find_slot", "slot_name"), &SpineSkeleton::find_slot);
	ClassDB::bind_method(D_METHOD("set_skin_by_name", "skin_name"), &SpineSkeleton::set_skin_by_name);
	ClassDB::bind_method(D_METHOD("set_skin", "new_skin"), &SpineSkeleton::set_skin);
	ClassDB::bind_method(D_METHOD("get_attachment_by_slot_name", "slot_name", "attachment_name"), &SpineSkeleton::get_attachment_by_slot_name);
	ClassDB::bind_method(D_METHOD("get_attachment_by_slot_index", "slot_index", "attachment_name"), &SpineSkeleton::get_attachment_by_slot_index);
	//
	//	void set_attachment(const String &slot_name, const String &attachment_name);
	//
	//	Ref<SpineIkConstraint> find_ik_constraint(const String &constraint_name);
	//	Ref<SpineTransformConstraint> find_transform_constraint(const String &constraint_name);
	//	Ref<SpinePathConstraint> find_path_constraint(const String &constraint_name);
	//
	//	void update(float delta);
	//
	//	Dictionary get_bounds();
	//
	//	Ref<SpineBone> get_root_bone();
	//
	//	Ref<SpineSkeletonDataResource> get_data();
	ClassDB::bind_method(D_METHOD("set_attachment", "slot_name", "attachment_name"), &SpineSkeleton::set_attachment);
	ClassDB::bind_method(D_METHOD("find_ik_constraint", "constraint_name"), &SpineSkeleton::find_ik_constraint);
	ClassDB::bind_method(D_METHOD("find_transform_constraint", "constraint_name"), &SpineSkeleton::find_transform_constraint);
	ClassDB::bind_method(D_METHOD("find_path_constraint", "constraint_name"), &SpineSkeleton::find_path_constraint);
	ClassDB::bind_method(D_METHOD("update", "delta"), &SpineSkeleton::update);
	ClassDB::bind_method(D_METHOD("get_bounds"), &SpineSkeleton::get_bounds);
	ClassDB::bind_method(D_METHOD("get_root_bone"), &SpineSkeleton::get_root_bone);
	ClassDB::bind_method(D_METHOD("get_data"), &SpineSkeleton::get_data);
	//
	//	Array get_bones();
	//	Array get_slots();
	//	Array get_draw_orders();
	//	Array get_ik_constraints();
	//	Array get_path_constraints();
	//	Array get_transform_constraints();
	//
	//	Ref<SpineSkin> get_skin();
	ClassDB::bind_method(D_METHOD("get_bones"), &SpineSkeleton::get_bones);
	ClassDB::bind_method(D_METHOD("get_slots"), &SpineSkeleton::get_slots);
	ClassDB::bind_method(D_METHOD("get_draw_orders"), &SpineSkeleton::get_draw_orders);
	ClassDB::bind_method(D_METHOD("get_ik_constraints"), &SpineSkeleton::get_ik_constraints);
	ClassDB::bind_method(D_METHOD("get_path_constraints"), &SpineSkeleton::get_path_constraints);
	ClassDB::bind_method(D_METHOD("get_transform_constraints"), &SpineSkeleton::get_transform_constraints);
	ClassDB::bind_method(D_METHOD("get_skin"), &SpineSkeleton::get_skin);
	//
	//	Color get_color();
	//	void set_color(Color v);
	//
	//	float get_time();
	//	void set_time(float v);
	//
	//	void set_position(Vector2 pos);
	ClassDB::bind_method(D_METHOD("get_color"), &SpineSkeleton::get_color);
	ClassDB::bind_method(D_METHOD("set_color", "v"), &SpineSkeleton::set_color);
	ClassDB::bind_method(D_METHOD("get_time"), &SpineSkeleton::get_time);
	ClassDB::bind_method(D_METHOD("set_time", "v"), &SpineSkeleton::set_time);
	ClassDB::bind_method(D_METHOD("set_position", "pos"), &SpineSkeleton::set_position);
	//
	//	float get_x();
	//	void set_x(float v);
	//
	//	float get_y();
	//	void set_y(float v);
	//
	//	float get_scale_x();
	//	void set_scale_x(float v);
	//
	//	float get_scale_y();
	//	void set_scale_y(float v);
	ClassDB::bind_method(D_METHOD("get_x"), &SpineSkeleton::get_x);
	ClassDB::bind_method(D_METHOD("set_x", "v"), &SpineSkeleton::set_x);
	ClassDB::bind_method(D_METHOD("get_y"), &SpineSkeleton::get_y);
	ClassDB::bind_method(D_METHOD("set_y", "v"), &SpineSkeleton::set_y);
	ClassDB::bind_method(D_METHOD("get_scale_x"), &SpineSkeleton::get_scale_x);
	ClassDB::bind_method(D_METHOD("set_scale_x", "v"), &SpineSkeleton::set_scale_x);
	ClassDB::bind_method(D_METHOD("get_scale_y"), &SpineSkeleton::get_scale_y);
	ClassDB::bind_method(D_METHOD("set_scale_y", "v"), &SpineSkeleton::set_scale_y);
}

SpineSkeleton::SpineSkeleton() : skeleton(NULL), spine_object(false), the_sprite(nullptr) {
}

SpineSkeleton::~SpineSkeleton() {
	if (skeleton && !spine_object) {
		delete skeleton;
		skeleton = NULL;
	}
}

void SpineSkeleton::load_skeleton(Ref<SpineSkeletonDataResource> sd) {
	if (skeleton && !spine_object) {
		delete skeleton;
		skeleton = NULL;
	}
	skeleton = new spine::Skeleton(sd->get_skeleton_data());
	spine_object = false;
}

#define S_T(x) (spine::String(x.utf8()))
void SpineSkeleton::update_world_transform() {
	skeleton->updateWorldTransform();
}

void SpineSkeleton::set_to_setup_pose() {
	skeleton->setToSetupPose();
}

void SpineSkeleton::set_bones_to_setup_pose() {
	skeleton->setBonesToSetupPose();
}

void SpineSkeleton::set_slots_to_setup_pose() {
	skeleton->setSlotsToSetupPose();
}

Ref<SpineBone> SpineSkeleton::find_bone(const String &name) {
	if (name.empty()) return NULL;
	auto b = skeleton->findBone(S_T(name));
	if (b == NULL) return NULL;
	Ref<SpineBone> gd_b(memnew(SpineBone));
	gd_b->set_spine_object(b);
	gd_b->set_spine_sprite(the_sprite);
	return gd_b;
}

Ref<SpineSlot> SpineSkeleton::find_slot(const String &name) {
	if (name.empty()) return NULL;
	auto s = skeleton->findSlot(S_T(name));
	if (s == NULL) return NULL;
	Ref<SpineSlot> gd_s(memnew(SpineSlot));
	gd_s->set_spine_object(s);
	return gd_s;
}

void SpineSkeleton::set_skin_by_name(const String &skin_name) {
	skeleton->setSkin(S_T(skin_name));
}
void SpineSkeleton::set_skin(Ref<SpineSkin> new_skin) {
	if (new_skin.is_valid()) {
		skeleton->setSkin(new_skin->get_spine_object());
	} else {
		skeleton->setSkin(NULL);
	}
}

Ref<SpineAttachment> SpineSkeleton::get_attachment_by_slot_name(const String &slot_name, const String &attachment_name) {
	auto a = skeleton->getAttachment(S_T(slot_name), S_T(attachment_name));
	if (a == NULL) return NULL;
	Ref<SpineAttachment> gd_a(memnew(SpineAttachment));
	gd_a->set_spine_object(a);
	return gd_a;
}
Ref<SpineAttachment> SpineSkeleton::get_attachment_by_slot_index(int slot_index, const String &attachment_name) {
	auto a = skeleton->getAttachment(slot_index, S_T(attachment_name));
	if (a == NULL) return NULL;
	Ref<SpineAttachment> gd_a(memnew(SpineAttachment));
	gd_a->set_spine_object(a);
	return gd_a;
}

void SpineSkeleton::set_attachment(const String &slot_name, const String &attachment_name) {
	ERR_FAIL_COND(slot_name.empty());
	ERR_FAIL_COND(get_attachment_by_slot_name(slot_name, attachment_name) == NULL);
	skeleton->setAttachment(S_T(slot_name), S_T(attachment_name));
}

Ref<SpineIkConstraint> SpineSkeleton::find_ik_constraint(const String &constraint_name) {
	if (constraint_name.empty()) return NULL;
	auto c = skeleton->findIkConstraint(S_T(constraint_name));
	if (c == NULL) return NULL;
	Ref<SpineIkConstraint> gd_c(memnew(SpineIkConstraint));
	gd_c->set_spine_object(c);
	return gd_c;
}
Ref<SpineTransformConstraint> SpineSkeleton::find_transform_constraint(const String &constraint_name) {
	if (constraint_name.empty()) return NULL;
	auto c = skeleton->findTransformConstraint(S_T(constraint_name));
	if (c == NULL) return NULL;
	Ref<SpineTransformConstraint> gd_c(memnew(SpineTransformConstraint));
	gd_c->set_spine_object(c);
	return gd_c;
}
Ref<SpinePathConstraint> SpineSkeleton::find_path_constraint(const String &constraint_name) {
	if (constraint_name.empty()) return NULL;
	auto c = skeleton->findPathConstraint(S_T(constraint_name));
	if (c == NULL) return NULL;
	Ref<SpinePathConstraint> gd_c(memnew(SpinePathConstraint));
	gd_c->set_spine_object(c);
	return gd_c;
}

void SpineSkeleton::update(float delta) {
	skeleton->update(delta);
}

Dictionary SpineSkeleton::get_bounds() {
	float x, y, w, h;
	spine::Vector<float> vertex_buffer;
	skeleton->getBounds(x, y, w, h, vertex_buffer);

	Dictionary res;
	res["x"] = x;
	res["y"] = y;
	res["w"] = w;
	res["h"] = h;

	Array gd_a;
	gd_a.resize(vertex_buffer.size());
	for (size_t i = 0; i < gd_a.size(); ++i) {
		gd_a[i] = vertex_buffer[i];
	}
	res["vertex_buffer"] = gd_a;

	return res;
}

Ref<SpineBone> SpineSkeleton::get_root_bone() {
	auto b = skeleton->getRootBone();
	if (b == NULL) return NULL;
	Ref<SpineBone> gd_b(memnew(SpineBone));
	gd_b->set_spine_object(b);
	gd_b->set_spine_sprite(the_sprite);
	return gd_b;
}

Ref<SpineSkeletonDataResource> SpineSkeleton::get_data() const {
	auto sd = skeleton->getData();
	if (sd == NULL) return NULL;
	Ref<SpineSkeletonDataResource> gd_sd(memnew(SpineSkeletonDataResource));
	gd_sd->set_spine_object(sd);
	return gd_sd;
}

Array SpineSkeleton::get_bones() {
	auto &as = skeleton->getBones();
	Array gd_as;
	gd_as.resize(as.size());
	for (size_t i = 0; i < gd_as.size(); ++i) {
		auto b = as[i];
		if (b == NULL) gd_as[i] = Ref<SpineBone>(NULL);
		Ref<SpineBone> gd_a(memnew(SpineBone));
		gd_a->set_spine_object(b);
		gd_a->set_spine_sprite(the_sprite);
		gd_as[i] = gd_a;
	}
	return gd_as;
}
Array SpineSkeleton::get_slots() {
	auto &as = skeleton->getSlots();
	Array gd_as;
	gd_as.resize(as.size());
	for (size_t i = 0; i < gd_as.size(); ++i) {
		auto b = as[i];
		if (b == NULL) gd_as[i] = Ref<SpineSlot>(NULL);
		Ref<SpineSlot> gd_a(memnew(SpineSlot));
		gd_a->set_spine_object(b);
		gd_as[i] = gd_a;
	}
	return gd_as;
}
Array SpineSkeleton::get_draw_orders() {
	auto &as = skeleton->getDrawOrder();
	Array gd_as;
	gd_as.resize(as.size());
	for (size_t i = 0; i < gd_as.size(); ++i) {
		auto b = as[i];
		if (b == NULL) gd_as[i] = Ref<SpineSlot>(NULL);
		Ref<SpineSlot> gd_a(memnew(SpineSlot));
		gd_a->set_spine_object(b);
		gd_as[i] = gd_a;
	}
	return gd_as;
}
Array SpineSkeleton::get_ik_constraints() {
	auto &as = skeleton->getIkConstraints();
	Array gd_as;
	gd_as.resize(as.size());
	for (size_t i = 0; i < gd_as.size(); ++i) {
		auto b = as[i];
		if (b == NULL) gd_as[i] = Ref<SpineIkConstraint>(NULL);
		Ref<SpineIkConstraint> gd_a(memnew(SpineIkConstraint));
		gd_a->set_spine_object(b);
		gd_as[i] = gd_a;
	}
	return gd_as;
}
Array SpineSkeleton::get_path_constraints() {
	auto &as = skeleton->getPathConstraints();
	Array gd_as;
	gd_as.resize(as.size());
	for (size_t i = 0; i < gd_as.size(); ++i) {
		auto b = as[i];
		if (b == NULL) gd_as[i] = Ref<SpinePathConstraint>(NULL);
		Ref<SpinePathConstraint> gd_a(memnew(SpinePathConstraint));
		gd_a->set_spine_object(b);
		gd_as[i] = gd_a;
	}
	return gd_as;
}
Array SpineSkeleton::get_transform_constraints() {
	auto &as = skeleton->getTransformConstraints();
	Array gd_as;
	gd_as.resize(as.size());
	for (size_t i = 0; i < gd_as.size(); ++i) {
		auto b = as[i];
		if (b == NULL) gd_as[i] = Ref<SpineTransformConstraint>(NULL);
		Ref<SpineTransformConstraint> gd_a(memnew(SpineTransformConstraint));
		gd_a->set_spine_object(b);
		gd_as[i] = gd_a;
	}
	return gd_as;
}

Ref<SpineSkin> SpineSkeleton::get_skin() {
	auto s = skeleton->getSkin();
	if (s == NULL) return NULL;
	Ref<SpineSkin> gd_s(memnew(SpineSkin));
	gd_s->set_spine_object(s);
	return gd_s;
}

Color SpineSkeleton::get_color() {
	auto &c = skeleton->getColor();
	return Color(c.r, c.g, c.b, c.a);
}
void SpineSkeleton::set_color(Color v) {
	auto &c = skeleton->getColor();
	c.set(v.r, v.g, v.b, v.a);
}

float SpineSkeleton::get_time() {
	return skeleton->getTime();
}
void SpineSkeleton::set_time(float v) {
	skeleton->setTime(v);
}

void SpineSkeleton::set_position(Vector2 pos) {
	skeleton->setPosition(pos.x, pos.y);
}

float SpineSkeleton::get_x() {
	return skeleton->getX();
}
void SpineSkeleton::set_x(float v) {
	skeleton->setX(v);
}

float SpineSkeleton::get_y() {
	return skeleton->getY();
}
void SpineSkeleton::set_y(float v) {
	skeleton->setY(v);
}

float SpineSkeleton::get_scale_x() {
	return skeleton->getScaleX();
}
void SpineSkeleton::set_scale_x(float v) {
	skeleton->setScaleX(v);
}

float SpineSkeleton::get_scale_y() {
	return skeleton->getScaleY();
}
void SpineSkeleton::set_scale_y(float v) {
	skeleton->setScaleY(v);
}

void SpineSkeleton::set_spine_sprite(SpineSprite *s) {
	the_sprite = s;
}
