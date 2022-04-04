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

#include "SpineBone.h"

#include "SpineSprite.h"
#include "SpineSkeleton.h"

void SpineBone::_bind_methods() {
	ClassDB::bind_method(D_METHOD("update_world_transform"), &SpineBone::update_world_transform);
	//	void set_to_setup_pose();
	//
	//	Vector2 world_to_local(Vector2 world_position);
	//
	//	Vector2 local_to_world(Vector2 local_position);
	//
	//	float world_to_local_rotation(float world_rotation);
	//
	//	float local_to_world_rotation(float local_rotation);
	//
	//	void rotate_world(float degrees);
	ClassDB::bind_method(D_METHOD("set_to_setup_pose"), &SpineBone::set_to_setup_pose);
	ClassDB::bind_method(D_METHOD("world_to_local", "world_position"), &SpineBone::world_to_local);
	ClassDB::bind_method(D_METHOD("local_to_world", "local_position"), &SpineBone::local_to_world);
	ClassDB::bind_method(D_METHOD("world_to_local_rotation", "world_rotation"), &SpineBone::world_to_local_rotation);
	ClassDB::bind_method(D_METHOD("local_to_world_rotation", "local_rotation"), &SpineBone::local_to_world_rotation);
	ClassDB::bind_method(D_METHOD("rotate_world"), &SpineBone::rotate_world);
	//
	//	float get_world_to_local_rotation_x();
	//	float get_world_to_local_rotation_y();
	//
	//	Ref<SpineBoneData> get_data();
	//
	//	Ref<SpineSkeleton> get_skeleton();
	//
	//	Ref<SpineBone> get_parent();
	//
	//	Array get_children();
	ClassDB::bind_method(D_METHOD("get_world_to_local_rotation_x"), &SpineBone::get_world_to_local_rotation_x);
	ClassDB::bind_method(D_METHOD("get_world_to_local_rotation_y"), &SpineBone::get_world_to_local_rotation_y);
	ClassDB::bind_method(D_METHOD("get_data"), &SpineBone::get_data);
	ClassDB::bind_method(D_METHOD("get_skeleton"), &SpineBone::get_skeleton);
	ClassDB::bind_method(D_METHOD("get_parent"), &SpineBone::get_parent);
	ClassDB::bind_method(D_METHOD("get_children"), &SpineBone::get_children);
	//
	//	float get_x();
	//	void set_x(float v);
	//
	//	float get_y();
	//	void set_y(float v);
	//
	//	float get_rotation();
	//	void set_rotation(float v);
	//
	//	float get_scale_x();
	//	void set_scale_x(float v);
	ClassDB::bind_method(D_METHOD("get_x"), &SpineBone::get_x);
	ClassDB::bind_method(D_METHOD("set_x", "v"), &SpineBone::set_x);
	ClassDB::bind_method(D_METHOD("get_y"), &SpineBone::get_y);
	ClassDB::bind_method(D_METHOD("set_y", "v"), &SpineBone::set_y);
	ClassDB::bind_method(D_METHOD("get_rotation"), &SpineBone::get_rotation);
	ClassDB::bind_method(D_METHOD("set_rotation", "v"), &SpineBone::set_rotation);
	ClassDB::bind_method(D_METHOD("get_scale_x"), &SpineBone::get_scale_x);
	ClassDB::bind_method(D_METHOD("set_scale_x", "v"), &SpineBone::set_scale_x);
	//
	//	float get_scale_y();
	//	void set_scale_y(float v);
	//
	//	float get_shear_x();
	//	void set_shear_x(float v);
	//
	//	float get_shear_y();
	//	void set_shear_y(float v);
	//
	//	float get_applied_rotation();
	//	void set_applied_rotation(float v);
	ClassDB::bind_method(D_METHOD("get_scale_y"), &SpineBone::get_scale_y);
	ClassDB::bind_method(D_METHOD("set_scale_y", "v"), &SpineBone::set_scale_y);
	ClassDB::bind_method(D_METHOD("get_shear_x"), &SpineBone::get_shear_x);
	ClassDB::bind_method(D_METHOD("set_shear_x", "v"), &SpineBone::set_shear_x);
	ClassDB::bind_method(D_METHOD("get_shear_y"), &SpineBone::get_shear_y);
	ClassDB::bind_method(D_METHOD("set_shear_y", "v"), &SpineBone::set_shear_y);
	ClassDB::bind_method(D_METHOD("get_applied_rotation"), &SpineBone::get_applied_rotation);
	ClassDB::bind_method(D_METHOD("set_applied_rotation", "v"), &SpineBone::set_applied_rotation);
	//
	//	float get_a_x();
	//	void set_a_x(float v);
	//
	//	float get_a_y();
	//	void set_a_y(float v);
	//
	//	float get_a_scale_x();
	//	void set_a_scale_x(float v);
	//
	//	float get_a_scale_y();
	//	void set_a_scale_y(float v);
	ClassDB::bind_method(D_METHOD("get_a_x"), &SpineBone::get_a_x);
	ClassDB::bind_method(D_METHOD("set_a_x", "v"), &SpineBone::set_a_x);
	ClassDB::bind_method(D_METHOD("get_a_y"), &SpineBone::get_a_y);
	ClassDB::bind_method(D_METHOD("set_a_y", "v"), &SpineBone::set_a_y);
	ClassDB::bind_method(D_METHOD("get_a_scale_x"), &SpineBone::get_a_scale_x);
	ClassDB::bind_method(D_METHOD("set_a_scale_x", "v"), &SpineBone::set_a_scale_x);
	ClassDB::bind_method(D_METHOD("get_a_scale_y"), &SpineBone::get_a_scale_y);
	ClassDB::bind_method(D_METHOD("set_a_scale_y", "v"), &SpineBone::set_a_scale_y);
	//
	//	float get_a_shear_x();
	//	void set_a_shear_x(float v);
	//
	//	float get_a_shear_y();
	//	void set_a_shear_y(float v);
	//
	//	float get_a();
	//	void set_a(float v);
	//
	//	float get_b();
	//	void set_b(float v);
	ClassDB::bind_method(D_METHOD("get_a_shear_x"), &SpineBone::get_a_shear_x);
	ClassDB::bind_method(D_METHOD("set_a_shear_x", "v"), &SpineBone::set_a_shear_x);
	ClassDB::bind_method(D_METHOD("get_a_shear_y"), &SpineBone::get_a_shear_y);
	ClassDB::bind_method(D_METHOD("set_a_shear_y", "v"), &SpineBone::set_a_shear_y);
	ClassDB::bind_method(D_METHOD("get_a"), &SpineBone::get_a);
	ClassDB::bind_method(D_METHOD("set_a", "v"), &SpineBone::set_a);
	ClassDB::bind_method(D_METHOD("get_b"), &SpineBone::get_b);
	ClassDB::bind_method(D_METHOD("set_b", "v"), &SpineBone::set_b);
	//
	//	float get_c();
	//	void set_c(float v);
	//
	//	float get_d();
	//	void set_d(float v);
	//
	//	float get_world_x();
	//	void set_world_x(float v);
	//
	//	float get_world_y();
	//	void set_world_y(float v);
	ClassDB::bind_method(D_METHOD("get_c"), &SpineBone::get_c);
	ClassDB::bind_method(D_METHOD("set_c", "v"), &SpineBone::set_c);
	ClassDB::bind_method(D_METHOD("get_d"), &SpineBone::get_d);
	ClassDB::bind_method(D_METHOD("set_d", "v"), &SpineBone::set_d);
	ClassDB::bind_method(D_METHOD("get_world_x"), &SpineBone::get_world_x);
	ClassDB::bind_method(D_METHOD("set_world_x", "v"), &SpineBone::set_world_x);
	ClassDB::bind_method(D_METHOD("get_world_y"), &SpineBone::get_world_y);
	ClassDB::bind_method(D_METHOD("set_world_y", "v"), &SpineBone::set_world_y);
	//
	//	float get_world_rotation_x();
	//	float get_world_rotation_y();
	//
	//	float get_world_scale_x();
	//	float get_world_scale_y();
	//
	//	bool is_applied_valid();
	//	void set_applied_valid(bool v);
	//
	//	bool is_active();
	//	void set_active(bool v);
	ClassDB::bind_method(D_METHOD("get_world_rotation_x"), &SpineBone::get_world_rotation_x);
	ClassDB::bind_method(D_METHOD("get_world_rotation_y"), &SpineBone::get_world_rotation_y);
	ClassDB::bind_method(D_METHOD("get_world_scale_x"), &SpineBone::get_world_scale_x);
	ClassDB::bind_method(D_METHOD("get_world_scale_y"), &SpineBone::get_world_scale_y);
	ClassDB::bind_method(D_METHOD("is_active"), &SpineBone::is_active);
	ClassDB::bind_method(D_METHOD("set_active", "v"), &SpineBone::set_active);

	ClassDB::bind_method(D_METHOD("get_godot_transform"), &SpineBone::get_godot_transform);
	ClassDB::bind_method(D_METHOD("set_godot_transform", "local_transform"), &SpineBone::set_godot_transform);
	ClassDB::bind_method(D_METHOD("get_godot_global_transform"), &SpineBone::get_godot_global_transform);
	ClassDB::bind_method(D_METHOD("set_godot_global_transform", "global_transform"), &SpineBone::set_godot_global_transform);

	ClassDB::bind_method(D_METHOD("apply_world_transform_2d", "node2d"), &SpineBone::apply_world_transform_2d);
}

SpineBone::SpineBone() : bone(NULL), the_sprite(nullptr) {}
SpineBone::~SpineBone() {}

void SpineBone::update_world_transform() {
	bone->updateWorldTransform();
}

void SpineBone::set_to_setup_pose() {
	bone->setToSetupPose();
}

Vector2 SpineBone::world_to_local(Vector2 world_position) {
	float x, y;
	bone->worldToLocal(world_position.x, world_position.y, x, y);
	return Vector2(x, y);
}

Vector2 SpineBone::local_to_world(Vector2 local_position) {
	float x, y;
	bone->localToWorld(local_position.x, local_position.y, x, y);
	return Vector2(x, y);
}

float SpineBone::world_to_local_rotation(float world_rotation) {
	return bone->worldToLocalRotation(world_rotation);
}

float SpineBone::local_to_world_rotation(float local_rotation) {
	return bone->localToWorldRotation(local_rotation);
}

void SpineBone::rotate_world(float degrees) {
	bone->rotateWorld(degrees);
}

float SpineBone::get_world_to_local_rotation_x() {
	return bone->getWorldToLocalRotationX();
}
float SpineBone::get_world_to_local_rotation_y() {
	return bone->getWorldToLocalRotationY();
}

Ref<SpineBoneData> SpineBone::get_data() {
	auto &bd = bone->getData();
	Ref<SpineBoneData> gd_bd(memnew(SpineBoneData));
	gd_bd->set_spine_object(&bd);
	return gd_bd;
}

Ref<SpineSkeleton> SpineBone::get_skeleton() {
	auto &s = bone->getSkeleton();
	Ref<SpineSkeleton> gd_s(memnew(SpineSkeleton));
	gd_s->set_spine_object(&s);
	gd_s->set_spine_sprite(the_sprite);
	return gd_s;
}

Ref<SpineBone> SpineBone::get_parent() {
	auto b = bone->getParent();
	if (b == NULL) return NULL;
	Ref<SpineBone> gd_b(memnew(SpineBone));
	gd_b->set_spine_object(b);
	gd_b->set_spine_sprite(the_sprite);
	return gd_b;
}

Array SpineBone::get_children() {
	auto bs = bone->getChildren();
	Array gd_bs;
	gd_bs.resize(bs.size());
	for (size_t i = 0; i < bs.size(); ++i) {
		auto b = bs[i];
		if (b == NULL) gd_bs[i] = Ref<SpineBone>(NULL);
		Ref<SpineBone> gd_b(memnew(SpineBone));
		gd_b->set_spine_object(b);
		gd_b->set_spine_sprite(the_sprite);
		gd_bs[i] = gd_b;
	}
	return gd_bs;
}

float SpineBone::get_x() {
	return bone->getX();
}
void SpineBone::set_x(float v) {
	bone->setX(v);
}

float SpineBone::get_y() {
	return bone->getY();
}
void SpineBone::set_y(float v) {
	bone->setY(v);
}

float SpineBone::get_rotation() {
	return bone->getRotation();
}
void SpineBone::set_rotation(float v) {
	bone->setRotation(v);
}

float SpineBone::get_scale_x() {
	return bone->getScaleX();
}
void SpineBone::set_scale_x(float v) {
	bone->setScaleX(v);
}

float SpineBone::get_scale_y() {
	return bone->getScaleY();
}
void SpineBone::set_scale_y(float v) {
	bone->setScaleY(v);
}

float SpineBone::get_shear_x() {
	return bone->getShearX();
}
void SpineBone::set_shear_x(float v) {
	bone->setShearX(v);
}

float SpineBone::get_shear_y() {
	return bone->getShearY();
}
void SpineBone::set_shear_y(float v) {
	bone->setShearY(v);
}

float SpineBone::get_applied_rotation() {
	return bone->getAppliedRotation();
}
void SpineBone::set_applied_rotation(float v) {
	bone->setAppliedRotation(v);
}

float SpineBone::get_a_x() {
	return bone->getAX();
}
void SpineBone::set_a_x(float v) {
	bone->setAX(v);
}

float SpineBone::get_a_y() {
	return bone->getAY();
}
void SpineBone::set_a_y(float v) {
	bone->setAY(v);
}

float SpineBone::get_a_scale_x() {
	return bone->getAScaleX();
}
void SpineBone::set_a_scale_x(float v) {
	bone->setAScaleX(v);
}

float SpineBone::get_a_scale_y() {
	return bone->getAScaleY();
}
void SpineBone::set_a_scale_y(float v) {
	bone->setAScaleY(v);
}

float SpineBone::get_a_shear_x() {
	return bone->getAShearX();
}
void SpineBone::set_a_shear_x(float v) {
	bone->setAShearX(v);
}

float SpineBone::get_a_shear_y() {
	return bone->getAShearY();
}
void SpineBone::set_a_shear_y(float v) {
	bone->setAShearY(v);
}

float SpineBone::get_a() {
	return bone->getA();
}
void SpineBone::set_a(float v) {
	bone->setA(v);
}

float SpineBone::get_b() {
	return bone->getB();
}
void SpineBone::set_b(float v) {
	bone->setB(v);
}

float SpineBone::get_c() {
	return bone->getC();
}
void SpineBone::set_c(float v) {
	bone->setC(v);
}

float SpineBone::get_d() {
	return bone->getD();
}
void SpineBone::set_d(float v) {
	bone->setD(v);
}

float SpineBone::get_world_x() {
	return bone->getWorldX();
}
void SpineBone::set_world_x(float v) {
	bone->setWorldX(v);
}

float SpineBone::get_world_y() {
	return bone->getWorldY();
}
void SpineBone::set_world_y(float v) {
	bone->setWorldY(v);
}

float SpineBone::get_world_rotation_x() {
	return bone->getWorldRotationX();
}
float SpineBone::get_world_rotation_y() {
	return bone->getWorldRotationY();
}

float SpineBone::get_world_scale_x() {
	return bone->getWorldScaleX();
}
float SpineBone::get_world_scale_y() {
	return bone->getWorldScaleY();
}

bool SpineBone::is_active() {
	return bone->isActive();
}
void SpineBone::set_active(bool v) {
	bone->setActive(v);
}

// External feature functions
void SpineBone::apply_world_transform_2d(Variant o) {
	if (o.get_type() == Variant::OBJECT) {
		auto node = (Node *) o;
		if (node->is_class("Node2D")) {
			auto node2d = (Node2D *) node;
			// In godot the y-axis is nag to spine
			node2d->set_transform(Transform2D(
					get_a(), get_c(),
					get_b(), get_d(),
					get_world_x(), -get_world_y()));
			// Fix the rotation
			auto pos = node2d->get_position();
			node2d->translate(-pos);
			node2d->set_rotation(-node2d->get_rotation());
			node2d->translate(pos);
		}
	}
}

Transform2D SpineBone::get_godot_transform() {
	if (get_spine_object() == nullptr)
		return Transform2D();
	Transform2D trans;
	trans.translate(get_x(), -get_y());
	// It seems that spine uses degree for rotation
	trans.rotate(Math::deg2rad(-get_rotation()));
	trans.scale(Size2(get_scale_x(), get_scale_y()));
	return trans;
}

void SpineBone::set_godot_transform(Transform2D trans) {
	if (get_spine_object() == nullptr)
		return;
	Vector2 position = trans.get_origin();
	position.y *= -1;
	real_t rotation = trans.get_rotation();
	rotation = Math::rad2deg(-rotation);
	Vector2 scale = trans.get_scale();

	set_x(position.x);
	set_y(position.y);
	set_rotation(rotation);
	set_scale_x(scale.x);
	set_scale_y(scale.y);
}

Transform2D SpineBone::get_godot_global_transform() {
	if (get_spine_object() == nullptr)
		return Transform2D();
	if (the_sprite == nullptr)
		return get_godot_transform();
	Transform2D res = the_sprite->get_transform();
	res.translate(get_world_x(), -get_world_y());
	res.rotate(Math::deg2rad(-get_world_rotation_x()));
	res.scale(Vector2(get_world_scale_x(), get_world_scale_y()));
	auto p = the_sprite->get_parent() ? Object::cast_to<CanvasItem>(the_sprite->get_parent()) : nullptr;
	if (p) {
		return p->get_global_transform() * res;
	}
	return res;
}

void SpineBone::set_godot_global_transform(Transform2D transform) {
	if (get_spine_object() == nullptr)
		return;
	if (the_sprite == nullptr)
		set_godot_transform(transform);
	transform = the_sprite->get_global_transform().affine_inverse() * transform;
	Vector2 position = transform.get_origin();
	real_t rotation = transform.get_rotation();
	Vector2 scale = transform.get_scale();
	position.y *= -1;
	auto parent = get_parent();
	if (parent.is_valid()) {
		position = parent->world_to_local(position);
		if (parent->get_world_scale_x() != 0)
			scale.x /= parent->get_world_scale_x();
		else
			print_error("The parent scale.x is zero.");
		if (parent->get_world_scale_y() != 0)
			scale.y /= parent->get_world_scale_y();
		else
			print_error("The parent scale.y is zero.");
	}
	rotation = world_to_local_rotation(Math::rad2deg(-rotation));

	set_x(position.x);
	set_y(position.y);
	set_rotation(rotation);
	set_scale_x(scale.x);
	set_scale_y(scale.y);
}

void SpineBone::set_spine_sprite(SpineSprite *s) {
	the_sprite = s;
}
