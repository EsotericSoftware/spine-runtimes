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

#include "SpineBone.h"
#include "SpineConstant.h"
#include "SpineSprite.h"
#include "SpineSkeleton.h"
#include "SpineCommon.h"

void SpineBone::_bind_methods() {
	ClassDB::bind_method(D_METHOD("update_world_transform"), &SpineBone::update_world_transform);
	ClassDB::bind_method(D_METHOD("set_to_setup_pose"), &SpineBone::set_to_setup_pose);
	ClassDB::bind_method(D_METHOD("world_to_local", "world_position"), &SpineBone::world_to_local);
	ClassDB::bind_method(D_METHOD("world_to_parent", "world_position"), &SpineBone::world_to_parent);
	ClassDB::bind_method(D_METHOD("local_to_world", "local_position"), &SpineBone::local_to_world);
	ClassDB::bind_method(D_METHOD("parent_to_world", "local_position"), &SpineBone::parent_to_world);
	ClassDB::bind_method(D_METHOD("world_to_local_rotation", "world_rotation"), &SpineBone::world_to_local_rotation);
	ClassDB::bind_method(D_METHOD("local_to_world_rotation", "local_rotation"), &SpineBone::local_to_world_rotation);
	ClassDB::bind_method(D_METHOD("rotate_world"), &SpineBone::rotate_world);
	ClassDB::bind_method(D_METHOD("get_world_to_local_rotation_x"), &SpineBone::get_world_to_local_rotation_x);
	ClassDB::bind_method(D_METHOD("get_world_to_local_rotation_y"), &SpineBone::get_world_to_local_rotation_y);
	ClassDB::bind_method(D_METHOD("get_data"), &SpineBone::get_data);
	ClassDB::bind_method(D_METHOD("get_parent"), &SpineBone::get_parent);
	ClassDB::bind_method(D_METHOD("get_children"), &SpineBone::get_children);
	ClassDB::bind_method(D_METHOD("get_x"), &SpineBone::get_x);
	ClassDB::bind_method(D_METHOD("set_x", "v"), &SpineBone::set_x);
	ClassDB::bind_method(D_METHOD("get_y"), &SpineBone::get_y);
	ClassDB::bind_method(D_METHOD("set_y", "v"), &SpineBone::set_y);
	ClassDB::bind_method(D_METHOD("get_rotation"), &SpineBone::get_rotation);
	ClassDB::bind_method(D_METHOD("set_rotation", "v"), &SpineBone::set_rotation);
	ClassDB::bind_method(D_METHOD("get_scale_x"), &SpineBone::get_scale_x);
	ClassDB::bind_method(D_METHOD("set_scale_x", "v"), &SpineBone::set_scale_x);
	ClassDB::bind_method(D_METHOD("get_scale_y"), &SpineBone::get_scale_y);
	ClassDB::bind_method(D_METHOD("set_scale_y", "v"), &SpineBone::set_scale_y);
	ClassDB::bind_method(D_METHOD("get_shear_x"), &SpineBone::get_shear_x);
	ClassDB::bind_method(D_METHOD("set_shear_x", "v"), &SpineBone::set_shear_x);
	ClassDB::bind_method(D_METHOD("get_shear_y"), &SpineBone::get_shear_y);
	ClassDB::bind_method(D_METHOD("set_shear_y", "v"), &SpineBone::set_shear_y);
	ClassDB::bind_method(D_METHOD("get_applied_rotation"), &SpineBone::get_applied_rotation);
	ClassDB::bind_method(D_METHOD("set_applied_rotation", "v"), &SpineBone::set_applied_rotation);
	ClassDB::bind_method(D_METHOD("get_a_x"), &SpineBone::get_a_x);
	ClassDB::bind_method(D_METHOD("set_a_x", "v"), &SpineBone::set_a_x);
	ClassDB::bind_method(D_METHOD("get_a_y"), &SpineBone::get_a_y);
	ClassDB::bind_method(D_METHOD("set_a_y", "v"), &SpineBone::set_a_y);
	ClassDB::bind_method(D_METHOD("get_a_scale_x"), &SpineBone::get_a_scale_x);
	ClassDB::bind_method(D_METHOD("set_a_scale_x", "v"), &SpineBone::set_a_scale_x);
	ClassDB::bind_method(D_METHOD("get_a_scale_y"), &SpineBone::get_a_scale_y);
	ClassDB::bind_method(D_METHOD("set_a_scale_y", "v"), &SpineBone::set_a_scale_y);
	ClassDB::bind_method(D_METHOD("get_a_shear_x"), &SpineBone::get_a_shear_x);
	ClassDB::bind_method(D_METHOD("set_a_shear_x", "v"), &SpineBone::set_a_shear_x);
	ClassDB::bind_method(D_METHOD("get_a_shear_y"), &SpineBone::get_a_shear_y);
	ClassDB::bind_method(D_METHOD("set_a_shear_y", "v"), &SpineBone::set_a_shear_y);
	ClassDB::bind_method(D_METHOD("get_a"), &SpineBone::get_a);
	ClassDB::bind_method(D_METHOD("set_a", "v"), &SpineBone::set_a);
	ClassDB::bind_method(D_METHOD("get_b"), &SpineBone::get_b);
	ClassDB::bind_method(D_METHOD("set_b", "v"), &SpineBone::set_b);
	ClassDB::bind_method(D_METHOD("get_c"), &SpineBone::get_c);
	ClassDB::bind_method(D_METHOD("set_c", "v"), &SpineBone::set_c);
	ClassDB::bind_method(D_METHOD("get_d"), &SpineBone::get_d);
	ClassDB::bind_method(D_METHOD("set_d", "v"), &SpineBone::set_d);
	ClassDB::bind_method(D_METHOD("get_world_x"), &SpineBone::get_world_x);
	ClassDB::bind_method(D_METHOD("set_world_x", "v"), &SpineBone::set_world_x);
	ClassDB::bind_method(D_METHOD("get_world_y"), &SpineBone::get_world_y);
	ClassDB::bind_method(D_METHOD("set_world_y", "v"), &SpineBone::set_world_y);
	ClassDB::bind_method(D_METHOD("get_world_rotation_x"), &SpineBone::get_world_rotation_x);
	ClassDB::bind_method(D_METHOD("get_world_rotation_y"), &SpineBone::get_world_rotation_y);
	ClassDB::bind_method(D_METHOD("get_world_scale_x"), &SpineBone::get_world_scale_x);
	ClassDB::bind_method(D_METHOD("get_world_scale_y"), &SpineBone::get_world_scale_y);
	ClassDB::bind_method(D_METHOD("is_active"), &SpineBone::is_active);
	ClassDB::bind_method(D_METHOD("set_active", "v"), &SpineBone::set_active);
	ClassDB::bind_method(D_METHOD("set_inherit", "v"), &SpineBone::set_inherit);
	ClassDB::bind_method(D_METHOD("get_inherit"), &SpineBone::get_inherit);
	ClassDB::bind_method(D_METHOD("get_transform"), &SpineBone::get_transform);
	ClassDB::bind_method(D_METHOD("set_transform", "local_transform"), &SpineBone::set_transform);
	ClassDB::bind_method(D_METHOD("get_global_transform"), &SpineBone::get_global_transform);
	ClassDB::bind_method(D_METHOD("set_global_transform", "global_transform"), &SpineBone::set_global_transform);
}

void SpineBone::update_world_transform() {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->updateWorldTransform();
}

void SpineBone::set_to_setup_pose() {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setToSetupPose();
}

Vector2 SpineBone::world_to_local(Vector2 world_position) {
	SPINE_CHECK(get_spine_object(), Vector2())
	float x, y;
	get_spine_object()->worldToLocal(world_position.x, world_position.y, x, y);
	return Vector2(x, y);
}

Vector2 SpineBone::world_to_parent(Vector2 world_position) {
	SPINE_CHECK(get_spine_object(), Vector2())
	float x, y;
	get_spine_object()->worldToParent(world_position.x, world_position.y, x, y);
	return Vector2(x, y);
}

Vector2 SpineBone::local_to_world(Vector2 local_position) {
	SPINE_CHECK(get_spine_object(), Vector2())
	float x, y;
	get_spine_object()->localToWorld(local_position.x, local_position.y, x, y);
	return Vector2(x, y);
}

Vector2 SpineBone::parent_to_world(Vector2 local_position) {
	SPINE_CHECK(get_spine_object(), Vector2())
	float x, y;
	get_spine_object()->parentToWorld(local_position.x, local_position.y, x, y);
	return Vector2(x, y);
}

float SpineBone::world_to_local_rotation(float world_rotation) {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->worldToLocalRotation(world_rotation);
}

float SpineBone::local_to_world_rotation(float local_rotation) {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->localToWorldRotation(local_rotation);
}

void SpineBone::rotate_world(float degrees) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->rotateWorld(degrees);
}

float SpineBone::get_world_to_local_rotation_x() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getWorldToLocalRotationX();
}

float SpineBone::get_world_to_local_rotation_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getWorldToLocalRotationY();
}

Ref<SpineBoneData> SpineBone::get_data() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto &bone_data = get_spine_object()->getData();
	Ref<SpineBoneData> bone_data_ref(memnew(SpineBoneData));
	bone_data_ref->set_spine_object(*get_spine_owner()->get_skeleton_data_res(), &bone_data);
	return bone_data_ref;
}

Ref<SpineBone> SpineBone::get_parent() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto parent = get_spine_object()->getParent();
	if (!parent) return nullptr;
	Ref<SpineBone> parent_ref(memnew(SpineBone));
	parent_ref->set_spine_object(get_spine_owner(), parent);
	return parent_ref;
}

Array SpineBone::get_children() {
	Array result;
	SPINE_CHECK(get_spine_object(), result)
	auto children = get_spine_object()->getChildren();
	result.resize((int) children.size());
	for (int i = 0; i < children.size(); ++i) {
		auto child = children[i];
		Ref<SpineBone> bone_ref(memnew(SpineBone));
		bone_ref->set_spine_object(get_spine_owner(), child);
		result[i] = bone_ref;
	}
	return result;
}

float SpineBone::get_x() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getX();
}

void SpineBone::set_x(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setX(v);
}

float SpineBone::get_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getY();
}

void SpineBone::set_y(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setY(v);
}

float SpineBone::get_rotation() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getRotation();
}

void SpineBone::set_rotation(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setRotation(v);
}

float SpineBone::get_scale_x() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getScaleX();
}

void SpineBone::set_scale_x(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setScaleX(v);
}

float SpineBone::get_scale_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getScaleY();
}

void SpineBone::set_scale_y(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setScaleY(v);
}

float SpineBone::get_shear_x() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getShearX();
}

void SpineBone::set_shear_x(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setShearX(v);
}

float SpineBone::get_shear_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getShearY();
}

void SpineBone::set_shear_y(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setShearY(v);
}

float SpineBone::get_applied_rotation() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getAppliedRotation();
}

void SpineBone::set_applied_rotation(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setAppliedRotation(v);
}

float SpineBone::get_a_x() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getAX();
}

void SpineBone::set_a_x(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setAX(v);
}

float SpineBone::get_a_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getAY();
}

void SpineBone::set_a_y(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setAY(v);
}

float SpineBone::get_a_scale_x() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getAScaleX();
}

void SpineBone::set_a_scale_x(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setAScaleX(v);
}

float SpineBone::get_a_scale_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getAScaleY();
}

void SpineBone::set_a_scale_y(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setAScaleY(v);
}

float SpineBone::get_a_shear_x() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getAShearX();
}

void SpineBone::set_a_shear_x(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setAShearX(v);
}

float SpineBone::get_a_shear_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getAShearY();
}

void SpineBone::set_a_shear_y(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setAShearY(v);
}

float SpineBone::get_a() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getA();
}

void SpineBone::set_a(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setA(v);
}

float SpineBone::get_b() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getB();
}

void SpineBone::set_b(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setB(v);
}

float SpineBone::get_c() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getC();
}

void SpineBone::set_c(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setC(v);
}

float SpineBone::get_d() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getD();
}

void SpineBone::set_d(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setD(v);
}

float SpineBone::get_world_x() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getWorldX();
}

void SpineBone::set_world_x(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setWorldX(v);
}

float SpineBone::get_world_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getWorldY();
}

void SpineBone::set_world_y(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setWorldY(v);
}

float SpineBone::get_world_rotation_x() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getWorldRotationX();
}

float SpineBone::get_world_rotation_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getWorldRotationY();
}


float SpineBone::get_world_scale_x() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getWorldScaleX();
}

float SpineBone::get_world_scale_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getWorldScaleY();
}

bool SpineBone::is_active() {
	SPINE_CHECK(get_spine_object(), false)
	return get_spine_object()->isActive();
}
void SpineBone::set_active(bool v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setActive(v);
}

SpineConstant::Inherit SpineBone::get_inherit() {
	SPINE_CHECK(get_spine_object(), SpineConstant::Inherit_Normal);
	return (SpineConstant::Inherit) get_spine_object()->getInherit();
}

void SpineBone::set_inherit(SpineConstant::Inherit inherit) {
	SPINE_CHECK(get_spine_object(), );
	get_spine_object()->setInherit((spine::Inherit) inherit);
}

Transform2D SpineBone::get_transform() {
	SPINE_CHECK(get_spine_object(), Transform2D())
	Transform2D transform;
	transform.rotate(spine::MathUtil::Deg_Rad * get_rotation());
	transform.scale(Size2(get_scale_x(), get_scale_y()));
	transform.set_origin(Vector2(get_x(), get_y()));
	return transform;
}

void SpineBone::set_transform(Transform2D transform) {
	SPINE_CHECK(get_spine_object(), )
	Vector2 position = transform.get_origin();
	float rotation = spine::MathUtil::Rad_Deg * transform.get_rotation();
	Vector2 scale = transform.get_scale();

	set_x(position.x);
	set_y(position.y);
	set_rotation(rotation);
	set_scale_x(scale.x);
	set_scale_y(scale.y);

	get_spine_owner()->set_modified_bones();
}

Transform2D SpineBone::get_global_transform() {
	SPINE_CHECK(get_spine_object(), Transform2D())
	if (!get_spine_owner()) return get_transform();
	if (!get_spine_owner()->is_visible_in_tree()) return get_transform();
	Transform2D local;
	local.rotate(spine::MathUtil::Deg_Rad * get_world_rotation_x());
	local.scale(Vector2(get_world_scale_x(), get_world_scale_y()));
	local.set_origin(Vector2(get_world_x(), get_world_y()));
	return get_spine_owner()->get_global_transform() * local;
}

void SpineBone::set_global_transform(Transform2D transform) {
	SPINE_CHECK(get_spine_object(), )
	if (!get_spine_owner()) set_transform(transform);
	if (!get_spine_owner()->is_visible_in_tree()) return;

	auto bone = get_spine_object();

	Transform2D inverse_sprite_transform = get_spine_owner()->get_global_transform().affine_inverse();
	transform = inverse_sprite_transform * transform;
	Vector2 position = transform.get_origin();
	float rotation = spine::MathUtil::Rad_Deg * transform.get_rotation();
	Vector2 scale = transform.get_scale();
	Vector2 local_position = position;
	float local_rotation = bone->worldToLocalRotation(rotation) - 180;
	Vector2 local_scale = scale;
	spine::Bone *parent = bone->getParent();
	if (parent) {
		parent->worldToLocal(local_position.x, local_position.y, local_position.x, local_position.y);
	}
	bone->setX(local_position.x);
	bone->setY(local_position.y);
	bone->setRotation(local_rotation);
	bone->setScaleX(local_scale.x);
	bone->setScaleY(local_scale.y);

	get_spine_owner()->set_modified_bones();
}
