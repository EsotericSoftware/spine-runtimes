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

#include "SpineBone.h"
#include "SpineConstant.h"
#include "SpineSprite.h"
#include "SpineSkeleton.h"
#include "SpineCommon.h"

void SpineBone::_bind_methods() {
	ClassDB::bind_method(D_METHOD("world_to_local", "world_position"), &SpineBone::world_to_local);
	ClassDB::bind_method(D_METHOD("world_to_parent", "world_position"), &SpineBone::world_to_parent);
	ClassDB::bind_method(D_METHOD("local_to_world", "local_position"), &SpineBone::local_to_world);
	ClassDB::bind_method(D_METHOD("parent_to_world", "local_position"), &SpineBone::parent_to_world);
	ClassDB::bind_method(D_METHOD("world_to_local_rotation", "world_rotation"), &SpineBone::world_to_local_rotation);
	ClassDB::bind_method(D_METHOD("local_to_world_rotation", "local_rotation"), &SpineBone::local_to_world_rotation);
	ClassDB::bind_method(D_METHOD("rotate_world"), &SpineBone::rotate_world);
	ClassDB::bind_method(D_METHOD("get_data"), &SpineBone::get_data);
	ClassDB::bind_method(D_METHOD("get_parent"), &SpineBone::get_parent);
	ClassDB::bind_method(D_METHOD("get_children"), &SpineBone::get_children);
	ClassDB::bind_method(D_METHOD("get_pose"), &SpineBone::get_pose);
	ClassDB::bind_method(D_METHOD("get_applied_pose"), &SpineBone::get_applied_pose);
	ClassDB::bind_method(D_METHOD("is_active"), &SpineBone::is_active);
	ClassDB::bind_method(D_METHOD("set_active", "v"), &SpineBone::set_active);
	ClassDB::bind_method(D_METHOD("update", "skeleton", "physics"), &SpineBone::update);
	ClassDB::bind_method(D_METHOD("get_transform"), &SpineBone::get_transform);
	ClassDB::bind_method(D_METHOD("set_transform", "local_transform"), &SpineBone::set_transform);
	ClassDB::bind_method(D_METHOD("get_global_transform"), &SpineBone::get_global_transform);
	ClassDB::bind_method(D_METHOD("set_global_transform", "global_transform"), &SpineBone::set_global_transform);
}

Vector2 SpineBone::world_to_local(Vector2 world_position) {
	SPINE_CHECK(get_spine_object(), Vector2())
	float x, y;
	get_spine_object()->getAppliedPose().worldToLocal(world_position.x, world_position.y, x, y);
	return Vector2(x, y);
}

Vector2 SpineBone::world_to_parent(Vector2 world_position) {
	SPINE_CHECK(get_spine_object(), Vector2())
	float x, y;
	get_spine_object()->getAppliedPose().worldToParent(world_position.x, world_position.y, x, y);
	return Vector2(x, y);
}

Vector2 SpineBone::local_to_world(Vector2 local_position) {
	SPINE_CHECK(get_spine_object(), Vector2())
	float x, y;
	get_spine_object()->getAppliedPose().localToWorld(local_position.x, local_position.y, x, y);
	return Vector2(x, y);
}

Vector2 SpineBone::parent_to_world(Vector2 local_position) {
	SPINE_CHECK(get_spine_object(), Vector2())
	float x, y;
	get_spine_object()->getAppliedPose().parentToWorld(local_position.x, local_position.y, x, y);
	return Vector2(x, y);
}

float SpineBone::world_to_local_rotation(float world_rotation) {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getAppliedPose().worldToLocalRotation(world_rotation);
}

float SpineBone::local_to_world_rotation(float local_rotation) {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getAppliedPose().localToWorldRotation(local_rotation);
}

void SpineBone::rotate_world(float degrees) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->getAppliedPose().rotateWorld(degrees);
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
	auto &children = get_spine_object()->getChildren();
	result.resize((int) children.size());
	for (int i = 0; i < children.size(); ++i) {
		auto child = children[i];
		Ref<SpineBone> bone_ref(memnew(SpineBone));
		bone_ref->set_spine_object(get_spine_owner(), child);
		result[i] = bone_ref;
	}
	return result;
}

Ref<SpineBonePose> SpineBone::get_pose() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto &pose = get_spine_object()->getPose();
	Ref<SpineBonePose> pose_ref(memnew(SpineBonePose));
	pose_ref->set_spine_object(get_spine_owner(), &pose);
	return pose_ref;
}

Ref<SpineBonePose> SpineBone::get_applied_pose() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto &applied_pose = get_spine_object()->getAppliedPose();
	Ref<SpineBonePose> pose_ref(memnew(SpineBonePose));
	pose_ref->set_spine_object(get_spine_owner(), &applied_pose);
	return pose_ref;
}

bool SpineBone::is_active() {
	SPINE_CHECK(get_spine_object(), false)
	return get_spine_object()->isActive();
}
void SpineBone::set_active(bool v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setActive(v);
}

Transform2D SpineBone::get_transform() {
	SPINE_CHECK(get_spine_object(), Transform2D())
	auto &pose = get_spine_object()->getPose();
	float rotation_x = spine::MathUtil::Deg_Rad * (pose.getRotation() + pose.getShearX());
	float rotation_y = spine::MathUtil::Deg_Rad * (pose.getRotation() + 90 + pose.getShearY());
	Transform2D transform;
	transform[0] = Vector2(spine::MathUtil::cos(rotation_x) * pose.getScaleX(), spine::MathUtil::sin(rotation_x) * pose.getScaleX());
	transform[1] = Vector2(spine::MathUtil::cos(rotation_y) * pose.getScaleY(), spine::MathUtil::sin(rotation_y) * pose.getScaleY());
	transform[2] = Vector2(pose.getX(), pose.getY());
	return transform;
}

void SpineBone::set_transform(Transform2D transform) {
	SPINE_CHECK(get_spine_object(), )
	Vector2 position = transform.get_origin();
	float rotation = spine::MathUtil::Rad_Deg * transform.get_rotation();
	Vector2 scale = transform.get_scale();

	auto &pose = get_spine_object()->getPose();
	pose.setX(position.x);
	pose.setY(position.y);
	pose.setRotation(rotation);
	pose.setScaleX(scale.x);
	pose.setScaleY(scale.y);

	get_spine_owner()->set_modified_bones();
}

Transform2D SpineBone::get_global_transform() {
	SPINE_CHECK(get_spine_object(), Transform2D())
	if (!get_spine_owner()) return get_transform();
	if (!get_spine_owner()->is_visible_in_tree()) return get_transform();
	auto &applied_pose = get_spine_object()->getAppliedPose();
	Transform2D local;
	local[0] = Vector2(applied_pose.getA(), applied_pose.getC());
	local[1] = Vector2(applied_pose.getB(), applied_pose.getD());
	local[2] = Vector2(applied_pose.getWorldX(), applied_pose.getWorldY());
	return get_spine_owner()->get_global_transform() * local;
}

void SpineBone::set_global_transform(Transform2D transform) {
	SPINE_CHECK(get_spine_object(), )
	if (!get_spine_owner()) {
		set_transform(transform);
		return;
	}
	if (!get_spine_owner()->is_visible_in_tree()) return;

	auto bone = get_spine_object();
	Transform2D inverse_sprite_transform = get_spine_owner()->get_global_transform().affine_inverse();
	Transform2D local = inverse_sprite_transform * transform;
	auto &applied_pose = bone->getAppliedPose();
	applied_pose.setA(local[0].x);
	applied_pose.setC(local[0].y);
	applied_pose.setB(local[1].x);
	applied_pose.setD(local[1].y);
	applied_pose.setWorldX(local.get_origin().x);
	applied_pose.setWorldY(local.get_origin().y);
	applied_pose.updateLocalTransform(*get_spine_owner()->get_skeleton()->get_spine_object());
	bone->getPose().set(applied_pose);

	get_spine_owner()->set_modified_bones();
}

void SpineBone::update(Ref<SpineSkeleton> skeleton, SpineConstant::Physics physics) {
	SPINE_CHECK(get_spine_object(), )
	SPINE_CHECK(skeleton.is_valid() && skeleton->get_spine_object(), )
	get_spine_object()->update(*skeleton->get_spine_object(), (spine::Physics) physics);
}
