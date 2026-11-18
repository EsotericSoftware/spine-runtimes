/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

#include "SpineBonePose.h"
#include "SpineCommon.h"
#include "SpineSkeleton.h"

void SpineBonePose::_bind_methods() {
	// BoneLocal methods
	ClassDB::bind_method(D_METHOD("get_x"), &SpineBonePose::get_x);
	ClassDB::bind_method(D_METHOD("set_x", "v"), &SpineBonePose::set_x);
	ClassDB::bind_method(D_METHOD("get_y"), &SpineBonePose::get_y);
	ClassDB::bind_method(D_METHOD("set_y", "v"), &SpineBonePose::set_y);
	ClassDB::bind_method(D_METHOD("get_rotation"), &SpineBonePose::get_rotation);
	ClassDB::bind_method(D_METHOD("set_rotation", "v"), &SpineBonePose::set_rotation);
	ClassDB::bind_method(D_METHOD("get_scale_x"), &SpineBonePose::get_scale_x);
	ClassDB::bind_method(D_METHOD("set_scale_x", "v"), &SpineBonePose::set_scale_x);
	ClassDB::bind_method(D_METHOD("get_scale_y"), &SpineBonePose::get_scale_y);
	ClassDB::bind_method(D_METHOD("set_scale_y", "v"), &SpineBonePose::set_scale_y);
	ClassDB::bind_method(D_METHOD("get_shear_x"), &SpineBonePose::get_shear_x);
	ClassDB::bind_method(D_METHOD("set_shear_x", "v"), &SpineBonePose::set_shear_x);
	ClassDB::bind_method(D_METHOD("get_shear_y"), &SpineBonePose::get_shear_y);
	ClassDB::bind_method(D_METHOD("set_shear_y", "v"), &SpineBonePose::set_shear_y);
	ClassDB::bind_method(D_METHOD("get_inherit"), &SpineBonePose::get_inherit);
	ClassDB::bind_method(D_METHOD("set_inherit", "v"), &SpineBonePose::set_inherit);

	// BonePose specific methods
	ClassDB::bind_method(D_METHOD("get_a"), &SpineBonePose::get_a);
	ClassDB::bind_method(D_METHOD("set_a", "v"), &SpineBonePose::set_a);
	ClassDB::bind_method(D_METHOD("get_b"), &SpineBonePose::get_b);
	ClassDB::bind_method(D_METHOD("set_b", "v"), &SpineBonePose::set_b);
	ClassDB::bind_method(D_METHOD("get_c"), &SpineBonePose::get_c);
	ClassDB::bind_method(D_METHOD("set_c", "v"), &SpineBonePose::set_c);
	ClassDB::bind_method(D_METHOD("get_d"), &SpineBonePose::get_d);
	ClassDB::bind_method(D_METHOD("set_d", "v"), &SpineBonePose::set_d);
	ClassDB::bind_method(D_METHOD("get_world_x"), &SpineBonePose::get_world_x);
	ClassDB::bind_method(D_METHOD("set_world_x", "v"), &SpineBonePose::set_world_x);
	ClassDB::bind_method(D_METHOD("get_world_y"), &SpineBonePose::get_world_y);
	ClassDB::bind_method(D_METHOD("set_world_y", "v"), &SpineBonePose::set_world_y);
	ClassDB::bind_method(D_METHOD("get_world_rotation_x"), &SpineBonePose::get_world_rotation_x);
	ClassDB::bind_method(D_METHOD("get_world_rotation_y"), &SpineBonePose::get_world_rotation_y);
	ClassDB::bind_method(D_METHOD("get_world_scale_x"), &SpineBonePose::get_world_scale_x);
	ClassDB::bind_method(D_METHOD("get_world_scale_y"), &SpineBonePose::get_world_scale_y);

	// Transformation methods
	ClassDB::bind_method(D_METHOD("world_to_local", "world_position"), &SpineBonePose::world_to_local);
	ClassDB::bind_method(D_METHOD("local_to_world", "local_position"), &SpineBonePose::local_to_world);
	ClassDB::bind_method(D_METHOD("world_to_parent", "world_position"), &SpineBonePose::world_to_parent);
	ClassDB::bind_method(D_METHOD("parent_to_world", "parent_position"), &SpineBonePose::parent_to_world);
	ClassDB::bind_method(D_METHOD("world_to_local_rotation", "world_rotation"), &SpineBonePose::world_to_local_rotation);
	ClassDB::bind_method(D_METHOD("local_to_world_rotation", "local_rotation"), &SpineBonePose::local_to_world_rotation);
	ClassDB::bind_method(D_METHOD("rotate_world", "degrees"), &SpineBonePose::rotate_world);

	// Update methods
	ClassDB::bind_method(D_METHOD("update_world_transform", "skeleton"), &SpineBonePose::update_world_transform);
	ClassDB::bind_method(D_METHOD("update_local_transform", "skeleton"), &SpineBonePose::update_local_transform);

	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "x", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_x", "get_x");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "y", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_y", "get_y");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "rotation", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_rotation", "get_rotation");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "scale_x", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_scale_x", "get_scale_x");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "scale_y", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_scale_y", "get_scale_y");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "shear_x", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_shear_x", "get_shear_x");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "shear_y", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_shear_y", "get_shear_y");
	ADD_PROPERTY(PropertyInfo(Variant::INT, "inherit", PROPERTY_HINT_ENUM,
							  "Normal,Only Translation,No Rotation Or Reflection,No Scale,No Scale Or Reflection", PROPERTY_USAGE_NONE),
				 "set_inherit", "get_inherit");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "a", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_a", "get_a");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "b", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_b", "get_b");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "c", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_c", "get_c");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "d", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_d", "get_d");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "world_x", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_world_x", "get_world_x");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "world_y", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_world_y", "get_world_y");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "world_rotation_x", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "", "get_world_rotation_x");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "world_rotation_y", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "", "get_world_rotation_y");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "world_scale_x", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "", "get_world_scale_x");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "world_scale_y", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "", "get_world_scale_y");
}

// BoneLocal properties
float SpineBonePose::get_x() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getX();
}

void SpineBonePose::set_x(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setX(v);
}

float SpineBonePose::get_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getY();
}

void SpineBonePose::set_y(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setY(v);
}

float SpineBonePose::get_rotation() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getRotation();
}

void SpineBonePose::set_rotation(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setRotation(v);
}

float SpineBonePose::get_scale_x() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getScaleX();
}

void SpineBonePose::set_scale_x(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setScaleX(v);
}

float SpineBonePose::get_scale_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getScaleY();
}

void SpineBonePose::set_scale_y(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setScaleY(v);
}

float SpineBonePose::get_shear_x() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getShearX();
}

void SpineBonePose::set_shear_x(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setShearX(v);
}

float SpineBonePose::get_shear_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getShearY();
}

void SpineBonePose::set_shear_y(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setShearY(v);
}

SpineConstant::Inherit SpineBonePose::get_inherit() {
	SPINE_CHECK(get_spine_object(), SpineConstant::Inherit_Normal)
	return (SpineConstant::Inherit) get_spine_object()->getInherit();
}

void SpineBonePose::set_inherit(SpineConstant::Inherit inherit) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setInherit((spine::Inherit) inherit);
}

// BonePose specific properties
float SpineBonePose::get_a() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getA();
}

void SpineBonePose::set_a(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setA(v);
}

float SpineBonePose::get_b() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getB();
}

void SpineBonePose::set_b(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setB(v);
}

float SpineBonePose::get_c() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getC();
}

void SpineBonePose::set_c(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setC(v);
}

float SpineBonePose::get_d() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getD();
}

void SpineBonePose::set_d(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setD(v);
}

float SpineBonePose::get_world_x() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getWorldX();
}

void SpineBonePose::set_world_x(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setWorldX(v);
}

float SpineBonePose::get_world_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getWorldY();
}

void SpineBonePose::set_world_y(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setWorldY(v);
}

float SpineBonePose::get_world_rotation_x() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getWorldRotationX();
}

float SpineBonePose::get_world_rotation_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getWorldRotationY();
}

float SpineBonePose::get_world_scale_x() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getWorldScaleX();
}

float SpineBonePose::get_world_scale_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getWorldScaleY();
}

// Transformation methods
Vector2 SpineBonePose::world_to_local(Vector2 world_position) {
	SPINE_CHECK(get_spine_object(), Vector2())
	float x, y;
	get_spine_object()->worldToLocal(world_position.x, world_position.y, x, y);
	return Vector2(x, y);
}

Vector2 SpineBonePose::local_to_world(Vector2 local_position) {
	SPINE_CHECK(get_spine_object(), Vector2())
	float x, y;
	get_spine_object()->localToWorld(local_position.x, local_position.y, x, y);
	return Vector2(x, y);
}

Vector2 SpineBonePose::world_to_parent(Vector2 world_position) {
	SPINE_CHECK(get_spine_object(), Vector2())
	float x, y;
	get_spine_object()->worldToParent(world_position.x, world_position.y, x, y);
	return Vector2(x, y);
}

Vector2 SpineBonePose::parent_to_world(Vector2 parent_position) {
	SPINE_CHECK(get_spine_object(), Vector2())
	float x, y;
	get_spine_object()->parentToWorld(parent_position.x, parent_position.y, x, y);
	return Vector2(x, y);
}

float SpineBonePose::world_to_local_rotation(float world_rotation) {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->worldToLocalRotation(world_rotation);
}

float SpineBonePose::local_to_world_rotation(float local_rotation) {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->localToWorldRotation(local_rotation);
}

void SpineBonePose::rotate_world(float degrees) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->rotateWorld(degrees);
}

void SpineBonePose::update_world_transform(Ref<SpineSkeleton> skeleton) {
	SPINE_CHECK(get_spine_object(), )
	SPINE_CHECK(skeleton.is_valid() && skeleton->get_spine_object(), )
	get_spine_object()->updateWorldTransform(*skeleton->get_spine_object());
}

void SpineBonePose::update_local_transform(Ref<SpineSkeleton> skeleton) {
	SPINE_CHECK(get_spine_object(), )
	SPINE_CHECK(skeleton.is_valid() && skeleton->get_spine_object(), )
	get_spine_object()->updateLocalTransform(*skeleton->get_spine_object());
}