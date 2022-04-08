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

#include "SpineNewBone.h"
#include "SpineNewSprite.h"
#include "SpineNewSkeleton.h"

void SpineNewBone::_bind_methods() {
	ClassDB::bind_method(D_METHOD("update_world_transform"), &SpineNewBone::update_world_transform);
	ClassDB::bind_method(D_METHOD("set_to_setup_pose"), &SpineNewBone::set_to_setup_pose);
	ClassDB::bind_method(D_METHOD("world_to_local", "world_position"), &SpineNewBone::world_to_local);
	ClassDB::bind_method(D_METHOD("local_to_world", "local_position"), &SpineNewBone::local_to_world);
	ClassDB::bind_method(D_METHOD("world_to_local_rotation", "world_rotation"), &SpineNewBone::world_to_local_rotation);
	ClassDB::bind_method(D_METHOD("local_to_world_rotation", "local_rotation"), &SpineNewBone::local_to_world_rotation);
	ClassDB::bind_method(D_METHOD("rotate_world"), &SpineNewBone::rotate_world);
	ClassDB::bind_method(D_METHOD("get_world_to_local_rotation_x"), &SpineNewBone::get_world_to_local_rotation_x);
	ClassDB::bind_method(D_METHOD("get_world_to_local_rotation_y"), &SpineNewBone::get_world_to_local_rotation_y);
	ClassDB::bind_method(D_METHOD("get_data"), &SpineNewBone::get_data);
	ClassDB::bind_method(D_METHOD("get_skeleton"), &SpineNewBone::get_skeleton);
	ClassDB::bind_method(D_METHOD("get_parent"), &SpineNewBone::get_parent);
	ClassDB::bind_method(D_METHOD("get_children"), &SpineNewBone::get_children);
	ClassDB::bind_method(D_METHOD("get_x"), &SpineNewBone::get_x);
	ClassDB::bind_method(D_METHOD("set_x", "v"), &SpineNewBone::set_x);
	ClassDB::bind_method(D_METHOD("get_y"), &SpineNewBone::get_y);
	ClassDB::bind_method(D_METHOD("set_y", "v"), &SpineNewBone::set_y);
	ClassDB::bind_method(D_METHOD("get_rotation"), &SpineNewBone::get_rotation);
	ClassDB::bind_method(D_METHOD("set_rotation", "v"), &SpineNewBone::set_rotation);
	ClassDB::bind_method(D_METHOD("get_scale_x"), &SpineNewBone::get_scale_x);
	ClassDB::bind_method(D_METHOD("set_scale_x", "v"), &SpineNewBone::set_scale_x);
	ClassDB::bind_method(D_METHOD("get_scale_y"), &SpineNewBone::get_scale_y);
	ClassDB::bind_method(D_METHOD("set_scale_y", "v"), &SpineNewBone::set_scale_y);
	ClassDB::bind_method(D_METHOD("get_shear_x"), &SpineNewBone::get_shear_x);
	ClassDB::bind_method(D_METHOD("set_shear_x", "v"), &SpineNewBone::set_shear_x);
	ClassDB::bind_method(D_METHOD("get_shear_y"), &SpineNewBone::get_shear_y);
	ClassDB::bind_method(D_METHOD("set_shear_y", "v"), &SpineNewBone::set_shear_y);
	ClassDB::bind_method(D_METHOD("get_applied_rotation"), &SpineNewBone::get_applied_rotation);
	ClassDB::bind_method(D_METHOD("set_applied_rotation", "v"), &SpineNewBone::set_applied_rotation);
	ClassDB::bind_method(D_METHOD("get_a_x"), &SpineNewBone::get_a_x);
	ClassDB::bind_method(D_METHOD("set_a_x", "v"), &SpineNewBone::set_a_x);
	ClassDB::bind_method(D_METHOD("get_a_y"), &SpineNewBone::get_a_y);
	ClassDB::bind_method(D_METHOD("set_a_y", "v"), &SpineNewBone::set_a_y);
	ClassDB::bind_method(D_METHOD("get_a_scale_x"), &SpineNewBone::get_a_scale_x);
	ClassDB::bind_method(D_METHOD("set_a_scale_x", "v"), &SpineNewBone::set_a_scale_x);
	ClassDB::bind_method(D_METHOD("get_a_scale_y"), &SpineNewBone::get_a_scale_y);
	ClassDB::bind_method(D_METHOD("set_a_scale_y", "v"), &SpineNewBone::set_a_scale_y);
	ClassDB::bind_method(D_METHOD("get_a_shear_x"), &SpineNewBone::get_a_shear_x);
	ClassDB::bind_method(D_METHOD("set_a_shear_x", "v"), &SpineNewBone::set_a_shear_x);
	ClassDB::bind_method(D_METHOD("get_a_shear_y"), &SpineNewBone::get_a_shear_y);
	ClassDB::bind_method(D_METHOD("set_a_shear_y", "v"), &SpineNewBone::set_a_shear_y);
	ClassDB::bind_method(D_METHOD("get_a"), &SpineNewBone::get_a);
	ClassDB::bind_method(D_METHOD("set_a", "v"), &SpineNewBone::set_a);
	ClassDB::bind_method(D_METHOD("get_b"), &SpineNewBone::get_b);
	ClassDB::bind_method(D_METHOD("set_b", "v"), &SpineNewBone::set_b);
	ClassDB::bind_method(D_METHOD("get_c"), &SpineNewBone::get_c);
	ClassDB::bind_method(D_METHOD("set_c", "v"), &SpineNewBone::set_c);
	ClassDB::bind_method(D_METHOD("get_d"), &SpineNewBone::get_d);
	ClassDB::bind_method(D_METHOD("set_d", "v"), &SpineNewBone::set_d);
	ClassDB::bind_method(D_METHOD("get_world_x"), &SpineNewBone::get_world_x);
	ClassDB::bind_method(D_METHOD("set_world_x", "v"), &SpineNewBone::set_world_x);
	ClassDB::bind_method(D_METHOD("get_world_y"), &SpineNewBone::get_world_y);
	ClassDB::bind_method(D_METHOD("set_world_y", "v"), &SpineNewBone::set_world_y);
	ClassDB::bind_method(D_METHOD("get_world_rotation_x"), &SpineNewBone::get_world_rotation_x);
	ClassDB::bind_method(D_METHOD("get_world_rotation_y"), &SpineNewBone::get_world_rotation_y);
	ClassDB::bind_method(D_METHOD("get_world_scale_x"), &SpineNewBone::get_world_scale_x);
	ClassDB::bind_method(D_METHOD("get_world_scale_y"), &SpineNewBone::get_world_scale_y);
	ClassDB::bind_method(D_METHOD("is_active"), &SpineNewBone::is_active);
	ClassDB::bind_method(D_METHOD("set_active", "v"), &SpineNewBone::set_active);
	ClassDB::bind_method(D_METHOD("get_godot_transform"), &SpineNewBone::get_godot_transform);
	ClassDB::bind_method(D_METHOD("set_godot_transform", "local_transform"), &SpineNewBone::set_godot_transform);
	ClassDB::bind_method(D_METHOD("get_godot_global_transform"), &SpineNewBone::get_godot_global_transform);
	ClassDB::bind_method(D_METHOD("set_godot_global_transform", "global_transform"), &SpineNewBone::set_godot_global_transform);
	ClassDB::bind_method(D_METHOD("apply_world_transform_2d", "node2d"), &SpineNewBone::apply_world_transform_2d);
}

SpineNewBone::SpineNewBone() : bone(nullptr), sprite(nullptr) {}

SpineNewBone::~SpineNewBone() {}

void SpineNewBone::set_spine_sprite(SpineNewSprite* sprite) {
	this->sprite = sprite;
}

void SpineNewBone::update_world_transform() {
	bone->updateWorldTransform();
}

void SpineNewBone::set_to_setup_pose() {
	bone->setToSetupPose();
}

Vector2 SpineNewBone::world_to_local(Vector2 world_position) {
	float x, y;
	bone->worldToLocal(world_position.x, world_position.y, x, y);
	return Vector2(x, y);
}

Vector2 SpineNewBone::local_to_world(Vector2 local_position) {
	float x, y;
	bone->localToWorld(local_position.x, local_position.y, x, y);
	return Vector2(x, y);
}

float SpineNewBone::world_to_local_rotation(float world_rotation) {
	return bone->worldToLocalRotation(world_rotation);
}

float SpineNewBone::local_to_world_rotation(float local_rotation) {
	return bone->localToWorldRotation(local_rotation);
}

void SpineNewBone::rotate_world(float degrees) {
	bone->rotateWorld(degrees);
}

float SpineNewBone::get_world_to_local_rotation_x() {
	return bone->getWorldToLocalRotationX();
}
float SpineNewBone::get_world_to_local_rotation_y() {
	return bone->getWorldToLocalRotationY();
}

Ref<SpineBoneData> SpineNewBone::get_data() {
	auto &bd = bone->getData();
	Ref<SpineBoneData> gd_bd(memnew(SpineBoneData));
	gd_bd->set_spine_object(&bd);
	return gd_bd;
}

Ref<SpineNewSkeleton> SpineNewBone::get_skeleton() {
	auto &s = bone->getSkeleton();
	Ref<SpineNewSkeleton> gd_s(memnew(SpineNewSkeleton));
	gd_s->set_spine_object(&s);
	gd_s->set_spine_sprite(sprite);
	return gd_s;
}

Ref<SpineNewBone> SpineNewBone::get_parent() {
	auto b = bone->getParent();
	if (b == NULL) return NULL;
	Ref<SpineNewBone> gd_b(memnew(SpineNewBone));
	gd_b->set_spine_object(b);
	gd_b->set_spine_sprite(sprite);
	return gd_b;
}

Array SpineNewBone::get_children() {
	auto bs = bone->getChildren();
	Array gd_bs;
	gd_bs.resize(bs.size());
	for (size_t i = 0; i < bs.size(); ++i) {
		auto b = bs[i];
		if (b == NULL) gd_bs[i] = Ref<SpineNewBone>(NULL);
		Ref<SpineNewBone> gd_b(memnew(SpineNewBone));
		gd_b->set_spine_object(b);
		gd_b->set_spine_sprite(sprite);
		gd_bs[i] = gd_b;
	}
	return gd_bs;
}

float SpineNewBone::get_x() {
	return bone->getX();
}
void SpineNewBone::set_x(float v) {
	bone->setX(v);
}

float SpineNewBone::get_y() {
	return bone->getY();
}
void SpineNewBone::set_y(float v) {
	bone->setY(v);
}

float SpineNewBone::get_rotation() {
	return bone->getRotation();
}
void SpineNewBone::set_rotation(float v) {
	bone->setRotation(v);
}

float SpineNewBone::get_scale_x() {
	return bone->getScaleX();
}
void SpineNewBone::set_scale_x(float v) {
	bone->setScaleX(v);
}

float SpineNewBone::get_scale_y() {
	return bone->getScaleY();
}
void SpineNewBone::set_scale_y(float v) {
	bone->setScaleY(v);
}

float SpineNewBone::get_shear_x() {
	return bone->getShearX();
}
void SpineNewBone::set_shear_x(float v) {
	bone->setShearX(v);
}

float SpineNewBone::get_shear_y() {
	return bone->getShearY();
}
void SpineNewBone::set_shear_y(float v) {
	bone->setShearY(v);
}

float SpineNewBone::get_applied_rotation() {
	return bone->getAppliedRotation();
}
void SpineNewBone::set_applied_rotation(float v) {
	bone->setAppliedRotation(v);
}

float SpineNewBone::get_a_x() {
	return bone->getAX();
}
void SpineNewBone::set_a_x(float v) {
	bone->setAX(v);
}

float SpineNewBone::get_a_y() {
	return bone->getAY();
}
void SpineNewBone::set_a_y(float v) {
	bone->setAY(v);
}

float SpineNewBone::get_a_scale_x() {
	return bone->getAScaleX();
}
void SpineNewBone::set_a_scale_x(float v) {
	bone->setAScaleX(v);
}

float SpineNewBone::get_a_scale_y() {
	return bone->getAScaleY();
}
void SpineNewBone::set_a_scale_y(float v) {
	bone->setAScaleY(v);
}

float SpineNewBone::get_a_shear_x() {
	return bone->getAShearX();
}
void SpineNewBone::set_a_shear_x(float v) {
	bone->setAShearX(v);
}

float SpineNewBone::get_a_shear_y() {
	return bone->getAShearY();
}
void SpineNewBone::set_a_shear_y(float v) {
	bone->setAShearY(v);
}

float SpineNewBone::get_a() {
	return bone->getA();
}
void SpineNewBone::set_a(float v) {
	bone->setA(v);
}

float SpineNewBone::get_b() {
	return bone->getB();
}
void SpineNewBone::set_b(float v) {
	bone->setB(v);
}

float SpineNewBone::get_c() {
	return bone->getC();
}
void SpineNewBone::set_c(float v) {
	bone->setC(v);
}

float SpineNewBone::get_d() {
	return bone->getD();
}
void SpineNewBone::set_d(float v) {
	bone->setD(v);
}

float SpineNewBone::get_world_x() {
	return bone->getWorldX();
}
void SpineNewBone::set_world_x(float v) {
	bone->setWorldX(v);
}

float SpineNewBone::get_world_y() {
	return bone->getWorldY();
}
void SpineNewBone::set_world_y(float v) {
	bone->setWorldY(v);
}

float SpineNewBone::get_world_rotation_x() {
	return bone->getWorldRotationX();
}
float SpineNewBone::get_world_rotation_y() {
	return bone->getWorldRotationY();
}

float SpineNewBone::get_world_scale_x() {
	return bone->getWorldScaleX();
}
float SpineNewBone::get_world_scale_y() {
	return bone->getWorldScaleY();
}

bool SpineNewBone::is_active() {
	return bone->isActive();
}
void SpineNewBone::set_active(bool v) {
	bone->setActive(v);
}

// External feature functions
void SpineNewBone::apply_world_transform_2d(Variant o) {
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

Transform2D SpineNewBone::get_godot_transform() {
	if (get_spine_object() == nullptr)
		return Transform2D();
	Transform2D trans;
	trans.translate(get_x(), -get_y());
	// It seems that spine uses degree for rotation
	trans.rotate(Math::deg2rad(-get_rotation()));
	trans.scale(Size2(get_scale_x(), get_scale_y()));
	return trans;
}

void SpineNewBone::set_godot_transform(Transform2D trans) {
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

Transform2D SpineNewBone::get_godot_global_transform() {
	if (get_spine_object() == nullptr)
		return Transform2D();
	if (sprite == nullptr)
		return get_godot_transform();
	Transform2D res = sprite->get_transform();
	res.translate(get_world_x(), -get_world_y());
	res.rotate(Math::deg2rad(-get_world_rotation_x()));
	res.scale(Vector2(get_world_scale_x(), get_world_scale_y()));
	auto p = sprite->get_parent() ? Object::cast_to<CanvasItem>(sprite->get_parent()) : nullptr;
	if (p) {
		return p->get_global_transform() * res;
	}
	return res;
}

void SpineNewBone::set_godot_global_transform(Transform2D transform) {
	if (get_spine_object() == nullptr)
		return;
	if (sprite == nullptr)
		set_godot_transform(transform);
	transform = sprite->get_global_transform().affine_inverse() * transform;
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
