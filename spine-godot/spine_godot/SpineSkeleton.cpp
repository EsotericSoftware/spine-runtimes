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

#include "SpineSkeleton.h"
#include "SpineCommon.h"
#include "SpineSprite.h"
#include <spine/SkeletonClipping.h>

void SpineSkeleton::_bind_methods() {
	ClassDB::bind_method(D_METHOD("update_world_transform", "physics"), &SpineSkeleton::update_world_transform);
	ClassDB::bind_method(D_METHOD("set_to_setup_pose"), &SpineSkeleton::set_to_setup_pose);
	ClassDB::bind_method(D_METHOD("set_bones_to_setup_pose"), &SpineSkeleton::set_bones_to_setup_pose);
	ClassDB::bind_method(D_METHOD("set_slots_to_setup_pose"), &SpineSkeleton::set_slots_to_setup_pose);
	ClassDB::bind_method(D_METHOD("find_bone", "bone_name"), &SpineSkeleton::find_bone);
	ClassDB::bind_method(D_METHOD("find_slot", "slot_name"), &SpineSkeleton::find_slot);
	ClassDB::bind_method(D_METHOD("set_skin_by_name", "skin_name"), &SpineSkeleton::set_skin_by_name);
	ClassDB::bind_method(D_METHOD("set_skin", "new_skin"), &SpineSkeleton::set_skin);
	ClassDB::bind_method(D_METHOD("get_attachment_by_slot_name", "slot_name", "attachment_name"), &SpineSkeleton::get_attachment_by_slot_name);
	ClassDB::bind_method(D_METHOD("get_attachment_by_slot_index", "slot_index", "attachment_name"), &SpineSkeleton::get_attachment_by_slot_index);
	ClassDB::bind_method(D_METHOD("set_attachment", "slot_name", "attachment_name"), &SpineSkeleton::set_attachment);
	ClassDB::bind_method(D_METHOD("find_ik_constraint", "constraint_name"), &SpineSkeleton::find_ik_constraint);
	ClassDB::bind_method(D_METHOD("find_transform_constraint", "constraint_name"), &SpineSkeleton::find_transform_constraint);
	ClassDB::bind_method(D_METHOD("find_path_constraint", "constraint_name"), &SpineSkeleton::find_path_constraint);
	ClassDB::bind_method(D_METHOD("find_physics_constraint", "constraint_name"), &SpineSkeleton::find_physics_constraint);
	ClassDB::bind_method(D_METHOD("get_bounds"), &SpineSkeleton::get_bounds);
	ClassDB::bind_method(D_METHOD("get_root_bone"), &SpineSkeleton::get_root_bone);
	ClassDB::bind_method(D_METHOD("get_data"), &SpineSkeleton::get_skeleton_data_res);
	ClassDB::bind_method(D_METHOD("get_bones"), &SpineSkeleton::get_bones);
	ClassDB::bind_method(D_METHOD("get_slots"), &SpineSkeleton::get_slots);
	ClassDB::bind_method(D_METHOD("get_draw_order"), &SpineSkeleton::get_draw_order);
	ClassDB::bind_method(D_METHOD("get_ik_constraints"), &SpineSkeleton::get_ik_constraints);
	ClassDB::bind_method(D_METHOD("get_path_constraints"), &SpineSkeleton::get_path_constraints);
	ClassDB::bind_method(D_METHOD("get_transform_constraints"), &SpineSkeleton::get_transform_constraints);
	ClassDB::bind_method(D_METHOD("get_skin"), &SpineSkeleton::get_skin);
	ClassDB::bind_method(D_METHOD("get_color"), &SpineSkeleton::get_color);
	ClassDB::bind_method(D_METHOD("set_color", "v"), &SpineSkeleton::set_color);
	ClassDB::bind_method(D_METHOD("set_position", "position"), &SpineSkeleton::set_position);
	ClassDB::bind_method(D_METHOD("get_x"), &SpineSkeleton::get_x);
	ClassDB::bind_method(D_METHOD("set_x", "v"), &SpineSkeleton::set_x);
	ClassDB::bind_method(D_METHOD("get_y"), &SpineSkeleton::get_y);
	ClassDB::bind_method(D_METHOD("set_y", "v"), &SpineSkeleton::set_y);
	ClassDB::bind_method(D_METHOD("get_scale_x"), &SpineSkeleton::get_scale_x);
	ClassDB::bind_method(D_METHOD("set_scale_x", "v"), &SpineSkeleton::set_scale_x);
	ClassDB::bind_method(D_METHOD("get_scale_y"), &SpineSkeleton::get_scale_y);
	ClassDB::bind_method(D_METHOD("set_scale_y", "v"), &SpineSkeleton::set_scale_y);
	ClassDB::bind_method(D_METHOD("get_time"), &SpineSkeleton::get_time);
	ClassDB::bind_method(D_METHOD("set_time", "time"), &SpineSkeleton::set_time);
	ClassDB::bind_method(D_METHOD("update", "delta"), &SpineSkeleton::update);
	ClassDB::bind_method(D_METHOD("physics_translate", "x", "y"), &SpineSkeleton::physics_translate);
	ClassDB::bind_method(D_METHOD("physics_rotate", "x", "y", "degrees"), &SpineSkeleton::physics_rotate);
}

SpineSkeleton::SpineSkeleton() : skeleton(nullptr), sprite(nullptr), last_skin(nullptr) {
}

SpineSkeleton::~SpineSkeleton() {
	if (last_skin.is_valid()) last_skin.unref();
	delete skeleton;
}

void SpineSkeleton::set_spine_sprite(SpineSprite *_sprite) {
	delete skeleton;
	skeleton = nullptr;
	sprite = _sprite;
	if (!sprite || !sprite->get_skeleton_data_res().is_valid() || !sprite->get_skeleton_data_res()->is_skeleton_data_loaded()) return;
	skeleton = new spine::Skeleton(sprite->get_skeleton_data_res()->get_skeleton_data());
}

Ref<SpineSkeletonDataResource> SpineSkeleton::get_skeleton_data_res() const {
	if (!sprite) return nullptr;
	return sprite->get_skeleton_data_res();
}

void SpineSkeleton::update_world_transform(SpineConstant::Physics physics) {
	SPINE_CHECK(skeleton, )
	skeleton->updateWorldTransform((spine::Physics) physics);
}

void SpineSkeleton::set_to_setup_pose() {
	SPINE_CHECK(skeleton, )
	skeleton->setToSetupPose();
}

void SpineSkeleton::set_bones_to_setup_pose() {
	SPINE_CHECK(skeleton, )
	skeleton->setBonesToSetupPose();
}

void SpineSkeleton::set_slots_to_setup_pose() {
	SPINE_CHECK(skeleton, )
	skeleton->setSlotsToSetupPose();
}

Ref<SpineBone> SpineSkeleton::find_bone(const String &name) {
	SPINE_CHECK(skeleton, nullptr)
	if (EMPTY(name)) return nullptr;
	auto bone = skeleton->findBone(SPINE_STRING_TMP(name));
	if (!bone) return nullptr;
	if (_cached_bones.count(bone) > 0) {
		return _cached_bones[bone];
	}
	Ref<SpineBone> bone_ref(memnew(SpineBone));
	bone_ref->set_spine_object(sprite, bone);
	_cached_bones[bone] = bone_ref;
	return bone_ref;
}

Ref<SpineSlot> SpineSkeleton::find_slot(const String &name) {
	SPINE_CHECK(skeleton, nullptr)
	if (EMPTY(name)) return nullptr;
	auto slot = skeleton->findSlot(SPINE_STRING_TMP(name));
	if (!slot) return nullptr;
	if (_cached_slots.count(slot) > 0) {
		return _cached_slots[slot];
	}
	Ref<SpineSlot> slot_ref(memnew(SpineSlot));
	slot_ref->set_spine_object(sprite, slot);
	_cached_slots[slot] = slot_ref;
	return slot_ref;
}

void SpineSkeleton::set_skin_by_name(const String &skin_name) {
	SPINE_CHECK(skeleton, )
	skeleton->setSkin(SPINE_STRING_TMP(skin_name));
}

void SpineSkeleton::set_skin(Ref<SpineSkin> new_skin) {
	SPINE_CHECK(skeleton, )
	if (last_skin.is_valid()) last_skin.unref();
	last_skin = new_skin;
	skeleton->setSkin(new_skin.is_valid() && new_skin->get_spine_object() ? new_skin->get_spine_object() : nullptr);
}

Ref<SpineAttachment> SpineSkeleton::get_attachment_by_slot_name(const String &slot_name, const String &attachment_name) {
	SPINE_CHECK(skeleton, nullptr)
	auto attachment = skeleton->getAttachment(SPINE_STRING_TMP(slot_name), SPINE_STRING_TMP(attachment_name));
	if (!attachment) return nullptr;
	Ref<SpineAttachment> attachment_ref(memnew(SpineAttachment));
	attachment_ref->set_spine_object(*sprite->get_skeleton_data_res(), attachment);
	return attachment_ref;
}

Ref<SpineAttachment> SpineSkeleton::get_attachment_by_slot_index(int slot_index, const String &attachment_name) {
	SPINE_CHECK(skeleton, nullptr)
	auto attachment = skeleton->getAttachment(slot_index, SPINE_STRING_TMP(attachment_name));
	if (!attachment) return nullptr;
	Ref<SpineAttachment> attachment_ref(memnew(SpineAttachment));
	attachment_ref->set_spine_object(*sprite->get_skeleton_data_res(), attachment);
	return attachment_ref;
}

void SpineSkeleton::set_attachment(const String &slot_name, const String &attachment_name) {
	SPINE_CHECK(skeleton, )
	skeleton->setAttachment(SPINE_STRING(slot_name), SPINE_STRING(attachment_name));
}

Ref<SpineIkConstraint> SpineSkeleton::find_ik_constraint(const String &constraint_name) {
	SPINE_CHECK(skeleton, nullptr)
	if (EMPTY(constraint_name)) return nullptr;
	auto constraint = skeleton->findIkConstraint(SPINE_STRING_TMP(constraint_name));
	if (!constraint) return nullptr;
	Ref<SpineIkConstraint> constraint_ref(memnew(SpineIkConstraint));
	constraint_ref->set_spine_object(sprite, constraint);
	return constraint_ref;
}

Ref<SpineTransformConstraint> SpineSkeleton::find_transform_constraint(const String &constraint_name) {
	SPINE_CHECK(skeleton, nullptr)
	if (EMPTY(constraint_name)) return nullptr;
	auto constraint = skeleton->findTransformConstraint(SPINE_STRING_TMP(constraint_name));
	if (!constraint) return nullptr;
	Ref<SpineTransformConstraint> constraint_ref(memnew(SpineTransformConstraint));
	constraint_ref->set_spine_object(sprite, constraint);
	return constraint_ref;
}

Ref<SpinePathConstraint> SpineSkeleton::find_path_constraint(const String &constraint_name) {
	SPINE_CHECK(skeleton, nullptr)
	if (EMPTY(constraint_name)) return nullptr;
	auto constraint = skeleton->findPathConstraint(SPINE_STRING_TMP(constraint_name));
	if (!constraint) return nullptr;
	Ref<SpinePathConstraint> constraint_ref(memnew(SpinePathConstraint));
	constraint_ref->set_spine_object(sprite, constraint);
	return constraint_ref;
}


Ref<SpinePhysicsConstraint> SpineSkeleton::find_physics_constraint(const String &constraint_name) {
	SPINE_CHECK(skeleton, nullptr)
	if (EMPTY(constraint_name)) return nullptr;
	auto constraint = skeleton->findPhysicsConstraint(SPINE_STRING_TMP(constraint_name));
	if (!constraint) return nullptr;
	Ref<SpinePhysicsConstraint> constraint_ref(memnew(SpinePhysicsConstraint));
	constraint_ref->set_spine_object(sprite, constraint);
	return constraint_ref;
}

Rect2 SpineSkeleton::get_bounds() {
	SPINE_CHECK(skeleton, Rect2(0, 0, 0, 0))
	float x, y, w, h;
	spine::SkeletonClipping clipper;
	skeleton->getBounds(x, y, w, h, bounds_vertex_buffer, &clipper);
	return Rect2(x, y, w, h);
}

Ref<SpineBone> SpineSkeleton::get_root_bone() {
	SPINE_CHECK(skeleton, nullptr)
	auto bone = skeleton->getRootBone();
	if (!bone) return nullptr;
	Ref<SpineBone> bone_ref(memnew(SpineBone));
	bone_ref->set_spine_object(sprite, bone);
	return bone_ref;
}

Array SpineSkeleton::get_bones() {
	Array result;
	SPINE_CHECK(skeleton, result)
	auto &bones = skeleton->getBones();
	result.resize((int) bones.size());
	for (int i = 0; i < result.size(); ++i) {
		auto bone = bones[i];
		Ref<SpineBone> bone_ref(memnew(SpineBone));
		bone_ref->set_spine_object(sprite, bone);
		result[i] = bone_ref;
	}
	return result;
}

Array SpineSkeleton::get_slots() {
	Array result;
	SPINE_CHECK(skeleton, result)
	auto &slots = skeleton->getSlots();
	result.resize((int) slots.size());
	for (int i = 0; i < result.size(); ++i) {
		auto slot = slots[i];
		Ref<SpineSlot> slot_ref(memnew(SpineSlot));
		slot_ref->set_spine_object(sprite, slot);
		result[i] = slot_ref;
	}
	return result;
}

Array SpineSkeleton::get_draw_order() {
	Array result;
	SPINE_CHECK(skeleton, result)
	auto &slots = skeleton->getDrawOrder();
	result.resize((int) slots.size());
	for (int i = 0; i < result.size(); ++i) {
		auto slot = slots[i];
		Ref<SpineSlot> slot_ref(memnew(SpineSlot));
		slot_ref->set_spine_object(sprite, slot);
		result[i] = slot_ref;
	}
	return result;
}

Array SpineSkeleton::get_ik_constraints() {
	Array result;
	SPINE_CHECK(skeleton, result)
	auto &constraints = skeleton->getIkConstraints();
	result.resize((int) constraints.size());
	for (int i = 0; i < result.size(); ++i) {
		auto constraint = constraints[i];
		Ref<SpineIkConstraint> constraint_ref(memnew(SpineIkConstraint));
		constraint_ref->set_spine_object(sprite, constraint);
		result[i] = constraint_ref;
	}
	return result;
}

Array SpineSkeleton::get_transform_constraints() {
	Array result;
	SPINE_CHECK(skeleton, result)
	auto &constraints = skeleton->getTransformConstraints();
	result.resize((int) constraints.size());
	for (int i = 0; i < result.size(); ++i) {
		auto constraint = constraints[i];
		Ref<SpineTransformConstraint> constraint_ref(memnew(SpineTransformConstraint));
		constraint_ref->set_spine_object(sprite, constraint);
		result[i] = constraint_ref;
	}
	return result;
}

Array SpineSkeleton::get_path_constraints() {
	Array result;
	SPINE_CHECK(skeleton, result)
	auto &constraints = skeleton->getPathConstraints();
	result.resize((int) constraints.size());
	for (int i = 0; i < result.size(); ++i) {
		auto constraint = constraints[i];
		Ref<SpinePathConstraint> constraint_ref(memnew(SpinePathConstraint));
		constraint_ref->set_spine_object(sprite, constraint);
		result[i] = constraint_ref;
	}
	return result;
}

Array SpineSkeleton::get_physics_constraints() {
	Array result;
	SPINE_CHECK(skeleton, result)
	auto &constraints = skeleton->getPhysicsConstraints();
	result.resize((int) constraints.size());
	for (int i = 0; i < result.size(); ++i) {
		auto constraint = constraints[i];
		Ref<SpinePhysicsConstraint> constraint_ref(memnew(SpinePhysicsConstraint));
		constraint_ref->set_spine_object(sprite, constraint);
		result[i] = constraint_ref;
	}
	return result;
}

Ref<SpineSkin> SpineSkeleton::get_skin() {
	SPINE_CHECK(skeleton, nullptr)
	auto skin = skeleton->getSkin();
	if (!skin) return nullptr;
	Ref<SpineSkin> skin_ref(memnew(SpineSkin));
	skin_ref->set_spine_object(*sprite->get_skeleton_data_res(), skin);
	return skin_ref;
}

Color SpineSkeleton::get_color() {
	SPINE_CHECK(skeleton, Color(0, 0, 0, 0))
	auto &color = skeleton->getColor();
	return Color(color.r, color.g, color.b, color.a);
}

void SpineSkeleton::set_color(Color v) {
	SPINE_CHECK(skeleton, )
	auto &color = skeleton->getColor();
	color.set(v.r, v.g, v.b, v.a);
}

void SpineSkeleton::set_position(Vector2 position) {
	SPINE_CHECK(skeleton, )
	skeleton->setPosition(position.x, position.y);
}

float SpineSkeleton::get_x() {
	SPINE_CHECK(skeleton, 0)
	return skeleton->getX();
}

void SpineSkeleton::set_x(float v) {
	SPINE_CHECK(skeleton, )
	skeleton->setX(v);
}

float SpineSkeleton::get_y() {
	SPINE_CHECK(skeleton, 0)
	return skeleton->getY();
}

void SpineSkeleton::set_y(float v) {
	SPINE_CHECK(skeleton, )
	skeleton->setY(v);
}

float SpineSkeleton::get_scale_x() {
	SPINE_CHECK(skeleton, 1)
	return skeleton->getScaleX();
}

void SpineSkeleton::set_scale_x(float v) {
	SPINE_CHECK(skeleton, )
	skeleton->setScaleX(v);
}

float SpineSkeleton::get_scale_y() {
	SPINE_CHECK(skeleton, 1)
	return -skeleton->getScaleY();
}

void SpineSkeleton::set_scale_y(float v) {
	SPINE_CHECK(skeleton, )
	skeleton->setScaleY(v);
}

float SpineSkeleton::get_time() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getTime();
}

void SpineSkeleton::set_time(float time) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setTime(time);
}

void SpineSkeleton::update(float delta) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->update(delta);
}

void SpineSkeleton::physics_translate(float x, float y) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->physicsTranslate(x, y);
}

void SpineSkeleton::physics_rotate(float x, float y, float degrees) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->physicsRotate(x, y, degrees);
}
