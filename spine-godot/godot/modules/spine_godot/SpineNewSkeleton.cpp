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

#include "SpineNewSkeleton.h"

void SpineNewSkeleton::_bind_methods() {
	ClassDB::bind_method(D_METHOD("update_world_transform"), &SpineNewSkeleton::update_world_transform);
	ClassDB::bind_method(D_METHOD("set_to_setup_pose"), &SpineNewSkeleton::set_to_setup_pose);
	ClassDB::bind_method(D_METHOD("set_bones_to_setup_pose"), &SpineNewSkeleton::set_bones_to_setup_pose);
	ClassDB::bind_method(D_METHOD("set_slots_to_setup_pose"), &SpineNewSkeleton::set_slots_to_setup_pose);
	ClassDB::bind_method(D_METHOD("find_bone", "bone_name"), &SpineNewSkeleton::find_bone);
	ClassDB::bind_method(D_METHOD("find_slot", "slot_name"), &SpineNewSkeleton::find_slot);
	ClassDB::bind_method(D_METHOD("set_skin_by_name", "skin_name"), &SpineNewSkeleton::set_skin_by_name);
	ClassDB::bind_method(D_METHOD("set_skin", "new_skin"), &SpineNewSkeleton::set_skin);
	ClassDB::bind_method(D_METHOD("get_attachment_by_slot_name", "slot_name", "attachment_name"), &SpineNewSkeleton::get_attachment_by_slot_name);
	ClassDB::bind_method(D_METHOD("get_attachment_by_slot_index", "slot_index", "attachment_name"), &SpineNewSkeleton::get_attachment_by_slot_index);
	ClassDB::bind_method(D_METHOD("set_attachment", "slot_name", "attachment_name"), &SpineNewSkeleton::set_attachment);
	ClassDB::bind_method(D_METHOD("find_ik_constraint", "constraint_name"), &SpineNewSkeleton::find_ik_constraint);
	ClassDB::bind_method(D_METHOD("find_transform_constraint", "constraint_name"), &SpineNewSkeleton::find_transform_constraint);
	ClassDB::bind_method(D_METHOD("find_path_constraint", "constraint_name"), &SpineNewSkeleton::find_path_constraint);
	ClassDB::bind_method(D_METHOD("get_bounds"), &SpineNewSkeleton::get_bounds);
	ClassDB::bind_method(D_METHOD("get_root_bone"), &SpineNewSkeleton::get_root_bone);
	ClassDB::bind_method(D_METHOD("get_data"), &SpineNewSkeleton::get_data);
	ClassDB::bind_method(D_METHOD("get_bones"), &SpineNewSkeleton::get_bones);
	ClassDB::bind_method(D_METHOD("get_slots"), &SpineNewSkeleton::get_slots);
	ClassDB::bind_method(D_METHOD("get_draw_orders"), &SpineNewSkeleton::get_draw_orders);
	ClassDB::bind_method(D_METHOD("get_ik_constraints"), &SpineNewSkeleton::get_ik_constraints);
	ClassDB::bind_method(D_METHOD("get_path_constraints"), &SpineNewSkeleton::get_path_constraints);
	ClassDB::bind_method(D_METHOD("get_transform_constraints"), &SpineNewSkeleton::get_transform_constraints);
	ClassDB::bind_method(D_METHOD("get_skin"), &SpineNewSkeleton::get_skin);
	ClassDB::bind_method(D_METHOD("get_color"), &SpineNewSkeleton::get_color);
	ClassDB::bind_method(D_METHOD("set_color", "v"), &SpineNewSkeleton::set_color);
	ClassDB::bind_method(D_METHOD("set_position", "pos"), &SpineNewSkeleton::set_position);
	ClassDB::bind_method(D_METHOD("get_x"), &SpineNewSkeleton::get_x);
	ClassDB::bind_method(D_METHOD("set_x", "v"), &SpineNewSkeleton::set_x);
	ClassDB::bind_method(D_METHOD("get_y"), &SpineNewSkeleton::get_y);
	ClassDB::bind_method(D_METHOD("set_y", "v"), &SpineNewSkeleton::set_y);
	ClassDB::bind_method(D_METHOD("get_scale_x"), &SpineNewSkeleton::get_scale_x);
	ClassDB::bind_method(D_METHOD("set_scale_x", "v"), &SpineNewSkeleton::set_scale_x);
	ClassDB::bind_method(D_METHOD("get_scale_y"), &SpineNewSkeleton::get_scale_y);
	ClassDB::bind_method(D_METHOD("set_scale_y", "v"), &SpineNewSkeleton::set_scale_y);
}

SpineNewSkeleton::SpineNewSkeleton() : skeleton(nullptr), sprite(nullptr), skeleton_data_res(nullptr) {
}

SpineNewSkeleton::~SpineNewSkeleton() {
	delete skeleton;
}

void SpineNewSkeleton::set_skeleton_data_res(Ref<SpineNewSkeletonDataResource> data_res) {
	delete skeleton;
	skeleton = nullptr;
	if (!data_res.is_valid()) return;
	skeleton = new spine::Skeleton(data_res->get_skeleton_data());
	skeleton_data_res = data_res;
}

#define S_T(x) (spine::String((x).utf8()))
void SpineNewSkeleton::update_world_transform() {
	skeleton->updateWorldTransform();
}

void SpineNewSkeleton::set_to_setup_pose() {
	skeleton->setToSetupPose();
}

void SpineNewSkeleton::set_bones_to_setup_pose() {
	skeleton->setBonesToSetupPose();
}

void SpineNewSkeleton::set_slots_to_setup_pose() {
	skeleton->setSlotsToSetupPose();
}

Ref<SpineNewBone> SpineNewSkeleton::find_bone(const String &name) {
	if (name.empty()) return nullptr;
	auto bone = skeleton->findBone(S_T(name));
	if (!bone) return nullptr;
	Ref<SpineNewBone> bone_ref(memnew(SpineNewBone));
	bone_ref->set_spine_object(bone);
	bone_ref->set_spine_sprite(sprite);
	return bone_ref;
}

Ref<SpineSlot> SpineNewSkeleton::find_slot(const String &name) {
	if (name.empty()) return nullptr;
	auto slot = skeleton->findSlot(S_T(name));
	if (!slot) return nullptr;
	Ref<SpineSlot> slot_ref(memnew(SpineSlot));
	slot_ref->set_spine_object(slot);
	return slot_ref;
}

void SpineNewSkeleton::set_skin_by_name(const String &skin_name) {
	skeleton->setSkin(S_T(skin_name));
}
void SpineNewSkeleton::set_skin(Ref<SpineSkin> new_skin) {
	if (new_skin.is_valid())
		skeleton->setSkin(new_skin->get_spine_object());
	else
		skeleton->setSkin(nullptr);
}

Ref<SpineAttachment> SpineNewSkeleton::get_attachment_by_slot_name(const String &slot_name, const String &attachment_name) {
	auto a = skeleton->getAttachment(S_T(slot_name), S_T(attachment_name));
	if (a == nullptr) return nullptr;
	Ref<SpineAttachment> gd_a(memnew(SpineAttachment));
	gd_a->set_spine_object(a);
	return gd_a;
}

Ref<SpineAttachment> SpineNewSkeleton::get_attachment_by_slot_index(int slot_index, const String &attachment_name) {
	auto a = skeleton->getAttachment(slot_index, S_T(attachment_name));
	if (a == nullptr) return nullptr;
	Ref<SpineAttachment> gd_a(memnew(SpineAttachment));
	gd_a->set_spine_object(a);
	return gd_a;
}

void SpineNewSkeleton::set_attachment(const String &slot_name, const String &attachment_name) {
	ERR_FAIL_COND(slot_name.empty());
	ERR_FAIL_COND(get_attachment_by_slot_name(slot_name, attachment_name) == nullptr);
	skeleton->setAttachment(S_T(slot_name), S_T(attachment_name));
}

Ref<SpineIkConstraint> SpineNewSkeleton::find_ik_constraint(const String &constraint_name) {
	if (constraint_name.empty()) return nullptr;
	auto c = skeleton->findIkConstraint(S_T(constraint_name));
	if (c == nullptr) return nullptr;
	Ref<SpineIkConstraint> gd_c(memnew(SpineIkConstraint));
	gd_c->set_spine_object(c);
	return gd_c;
}
Ref<SpineTransformConstraint> SpineNewSkeleton::find_transform_constraint(const String &constraint_name) {
	if (constraint_name.empty()) return nullptr;
	auto c = skeleton->findTransformConstraint(S_T(constraint_name));
	if (c == nullptr) return nullptr;
	Ref<SpineTransformConstraint> gd_c(memnew(SpineTransformConstraint));
	gd_c->set_spine_object(c);
	return gd_c;
}
Ref<SpinePathConstraint> SpineNewSkeleton::find_path_constraint(const String &constraint_name) {
	if (constraint_name.empty()) return nullptr;
	auto c = skeleton->findPathConstraint(S_T(constraint_name));
	if (c == nullptr) return nullptr;
	Ref<SpinePathConstraint> gd_c(memnew(SpinePathConstraint));
	gd_c->set_spine_object(c);
	return gd_c;
}

Dictionary SpineNewSkeleton::get_bounds() {
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

Ref<SpineNewBone> SpineNewSkeleton::get_root_bone() {
	auto b = skeleton->getRootBone();
	if (b == nullptr) return nullptr;
	Ref<SpineNewBone> gd_b(memnew(SpineNewBone));
	gd_b->set_spine_object(b);
	gd_b->set_spine_sprite(sprite);
	return gd_b;
}

Ref<SpineNewSkeletonDataResource> SpineNewSkeleton::get_data() const {
	return skeleton_data_res;
}

Array SpineNewSkeleton::get_bones() {
	auto &as = skeleton->getBones();
	Array gd_as;
	gd_as.resize(as.size());
	for (size_t i = 0; i < gd_as.size(); ++i) {
		auto b = as[i];
		if (b == nullptr) gd_as[i] = Ref<SpineNewBone>(nullptr);
		Ref<SpineNewBone> gd_a(memnew(SpineNewBone));
		gd_a->set_spine_object(b);
		gd_a->set_spine_sprite(sprite);
		gd_as[i] = gd_a;
	}
	return gd_as;
}
Array SpineNewSkeleton::get_slots() {
	auto &as = skeleton->getSlots();
	Array gd_as;
	gd_as.resize(as.size());
	for (size_t i = 0; i < gd_as.size(); ++i) {
		auto b = as[i];
		if (b == nullptr) gd_as[i] = Ref<SpineSlot>(nullptr);
		Ref<SpineSlot> gd_a(memnew(SpineSlot));
		gd_a->set_spine_object(b);
		gd_as[i] = gd_a;
	}
	return gd_as;
}
Array SpineNewSkeleton::get_draw_orders() {
	auto &as = skeleton->getDrawOrder();
	Array gd_as;
	gd_as.resize(as.size());
	for (size_t i = 0; i < gd_as.size(); ++i) {
		auto b = as[i];
		if (b == nullptr) gd_as[i] = Ref<SpineSlot>(nullptr);
		Ref<SpineSlot> gd_a(memnew(SpineSlot));
		gd_a->set_spine_object(b);
		gd_as[i] = gd_a;
	}
	return gd_as;
}
Array SpineNewSkeleton::get_ik_constraints() {
	auto &as = skeleton->getIkConstraints();
	Array gd_as;
	gd_as.resize(as.size());
	for (size_t i = 0; i < gd_as.size(); ++i) {
		auto b = as[i];
		if (b == nullptr) gd_as[i] = Ref<SpineIkConstraint>(nullptr);
		Ref<SpineIkConstraint> gd_a(memnew(SpineIkConstraint));
		gd_a->set_spine_object(b);
		gd_as[i] = gd_a;
	}
	return gd_as;
}
Array SpineNewSkeleton::get_path_constraints() {
	auto &as = skeleton->getPathConstraints();
	Array gd_as;
	gd_as.resize(as.size());
	for (size_t i = 0; i < gd_as.size(); ++i) {
		auto b = as[i];
		if (b == nullptr) gd_as[i] = Ref<SpinePathConstraint>(nullptr);
		Ref<SpinePathConstraint> gd_a(memnew(SpinePathConstraint));
		gd_a->set_spine_object(b);
		gd_as[i] = gd_a;
	}
	return gd_as;
}
Array SpineNewSkeleton::get_transform_constraints() {
	auto &as = skeleton->getTransformConstraints();
	Array gd_as;
	gd_as.resize(as.size());
	for (size_t i = 0; i < gd_as.size(); ++i) {
		auto b = as[i];
		if (b == nullptr) gd_as[i] = Ref<SpineTransformConstraint>(nullptr);
		Ref<SpineTransformConstraint> gd_a(memnew(SpineTransformConstraint));
		gd_a->set_spine_object(b);
		gd_as[i] = gd_a;
	}
	return gd_as;
}

Ref<SpineSkin> SpineNewSkeleton::get_skin() {
	auto s = skeleton->getSkin();
	if (s == nullptr) return nullptr;
	Ref<SpineSkin> gd_s(memnew(SpineSkin));
	gd_s->set_spine_object(s);
	return gd_s;
}

Color SpineNewSkeleton::get_color() {
	auto &c = skeleton->getColor();
	return Color(c.r, c.g, c.b, c.a);
}
void SpineNewSkeleton::set_color(Color v) {
	auto &c = skeleton->getColor();
	c.set(v.r, v.g, v.b, v.a);
}

void SpineNewSkeleton::set_position(Vector2 pos) {
	skeleton->setPosition(pos.x, pos.y);
}

float SpineNewSkeleton::get_x() {
	return skeleton->getX();
}
void SpineNewSkeleton::set_x(float v) {
	skeleton->setX(v);
}

float SpineNewSkeleton::get_y() {
	return skeleton->getY();
}
void SpineNewSkeleton::set_y(float v) {
	skeleton->setY(v);
}

float SpineNewSkeleton::get_scale_x() {
	return skeleton->getScaleX();
}
void SpineNewSkeleton::set_scale_x(float v) {
	skeleton->setScaleX(v);
}

float SpineNewSkeleton::get_scale_y() {
	return skeleton->getScaleY();
}
void SpineNewSkeleton::set_scale_y(float v) {
	skeleton->setScaleY(v);
}

void SpineNewSkeleton::set_spine_sprite(SpineNewSprite *s) {
	sprite = s;
}
