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

#include "SpinePhysicsConstraint.h"
#include "SpineCommon.h"
#include "SpineSprite.h"

void SpinePhysicsConstraint::_bind_methods() {
	ClassDB::bind_method(D_METHOD("update", "physics"), &SpinePhysicsConstraint::update);
	ClassDB::bind_method(D_METHOD("get_bone"), &SpinePhysicsConstraint::get_bone);
	ClassDB::bind_method(D_METHOD("set_inertia", "value"), &SpinePhysicsConstraint::set_inertia);
	ClassDB::bind_method(D_METHOD("get_inertia"), &SpinePhysicsConstraint::get_inertia);
	ClassDB::bind_method(D_METHOD("set_strength", "value"), &SpinePhysicsConstraint::set_strength);
	ClassDB::bind_method(D_METHOD("get_strength"), &SpinePhysicsConstraint::get_strength);
	ClassDB::bind_method(D_METHOD("set_damping", "value"), &SpinePhysicsConstraint::set_damping);
	ClassDB::bind_method(D_METHOD("get_damping"), &SpinePhysicsConstraint::get_damping);
	ClassDB::bind_method(D_METHOD("set_mass_inverse", "value"), &SpinePhysicsConstraint::set_mass_inverse);
	ClassDB::bind_method(D_METHOD("get_mass_inverse"), &SpinePhysicsConstraint::get_mass_inverse);
	ClassDB::bind_method(D_METHOD("set_wind", "value"), &SpinePhysicsConstraint::set_wind);
	ClassDB::bind_method(D_METHOD("get_wind"), &SpinePhysicsConstraint::get_wind);
	ClassDB::bind_method(D_METHOD("set_gravity", "value"), &SpinePhysicsConstraint::set_gravity);
	ClassDB::bind_method(D_METHOD("get_gravity"), &SpinePhysicsConstraint::get_gravity);
	ClassDB::bind_method(D_METHOD("set_mix", "value"), &SpinePhysicsConstraint::set_mix);
	ClassDB::bind_method(D_METHOD("get_mix"), &SpinePhysicsConstraint::get_mix);
	ClassDB::bind_method(D_METHOD("set_reset", "value"), &SpinePhysicsConstraint::set_reset);
	ClassDB::bind_method(D_METHOD("get_reset"), &SpinePhysicsConstraint::get_reset);
	ClassDB::bind_method(D_METHOD("set_ux", "value"), &SpinePhysicsConstraint::set_ux);
	ClassDB::bind_method(D_METHOD("get_ux"), &SpinePhysicsConstraint::get_ux);
	ClassDB::bind_method(D_METHOD("set_uy", "value"), &SpinePhysicsConstraint::set_uy);
	ClassDB::bind_method(D_METHOD("get_uy"), &SpinePhysicsConstraint::get_uy);
	ClassDB::bind_method(D_METHOD("set_cx", "value"), &SpinePhysicsConstraint::set_cx);
	ClassDB::bind_method(D_METHOD("get_cx"), &SpinePhysicsConstraint::get_cx);
	ClassDB::bind_method(D_METHOD("set_cy", "value"), &SpinePhysicsConstraint::set_cy);
	ClassDB::bind_method(D_METHOD("get_cy"), &SpinePhysicsConstraint::get_cy);
	ClassDB::bind_method(D_METHOD("set_tx", "value"), &SpinePhysicsConstraint::set_tx);
	ClassDB::bind_method(D_METHOD("get_tx"), &SpinePhysicsConstraint::get_tx);
	ClassDB::bind_method(D_METHOD("set_ty", "value"), &SpinePhysicsConstraint::set_ty);
	ClassDB::bind_method(D_METHOD("get_ty"), &SpinePhysicsConstraint::get_ty);
	ClassDB::bind_method(D_METHOD("set_x_offset", "value"), &SpinePhysicsConstraint::set_x_offset);
	ClassDB::bind_method(D_METHOD("get_x_offset"), &SpinePhysicsConstraint::get_x_offset);
	ClassDB::bind_method(D_METHOD("set_x_velocity", "value"), &SpinePhysicsConstraint::set_x_velocity);
	ClassDB::bind_method(D_METHOD("get_x_velocity"), &SpinePhysicsConstraint::get_x_velocity);
	ClassDB::bind_method(D_METHOD("set_y_offset", "value"), &SpinePhysicsConstraint::set_y_offset);
	ClassDB::bind_method(D_METHOD("get_y_offset"), &SpinePhysicsConstraint::get_y_offset);
	ClassDB::bind_method(D_METHOD("set_y_velocity", "value"), &SpinePhysicsConstraint::set_y_velocity);
	ClassDB::bind_method(D_METHOD("get_y_velocity"), &SpinePhysicsConstraint::get_y_velocity);
	ClassDB::bind_method(D_METHOD("set_rotate_offset", "value"), &SpinePhysicsConstraint::set_rotate_offset);
	ClassDB::bind_method(D_METHOD("get_rotate_offset"), &SpinePhysicsConstraint::get_rotate_offset);
	ClassDB::bind_method(D_METHOD("set_rotate_velocity", "value"), &SpinePhysicsConstraint::set_rotate_velocity);
	ClassDB::bind_method(D_METHOD("get_rotate_velocity"), &SpinePhysicsConstraint::get_rotate_velocity);
	ClassDB::bind_method(D_METHOD("set_scale_offset", "value"), &SpinePhysicsConstraint::set_scale_offset);
	ClassDB::bind_method(D_METHOD("get_scale_offset"), &SpinePhysicsConstraint::get_scale_offset);
	ClassDB::bind_method(D_METHOD("set_scale_velocity", "value"), &SpinePhysicsConstraint::set_scale_velocity);
	ClassDB::bind_method(D_METHOD("get_scale_velocity"), &SpinePhysicsConstraint::get_scale_velocity);
	ClassDB::bind_method(D_METHOD("set_active", "value"), &SpinePhysicsConstraint::set_active);
	ClassDB::bind_method(D_METHOD("is_active"), &SpinePhysicsConstraint::is_active);
	ClassDB::bind_method(D_METHOD("set_remaining", "value"), &SpinePhysicsConstraint::set_remaining);
	ClassDB::bind_method(D_METHOD("get_remaining"), &SpinePhysicsConstraint::get_remaining);
	ClassDB::bind_method(D_METHOD("set_last_Time", "value"), &SpinePhysicsConstraint::set_last_Time);
	ClassDB::bind_method(D_METHOD("get_last_Time"), &SpinePhysicsConstraint::get_last_Time);
	ClassDB::bind_method(D_METHOD("reset"), &SpinePhysicsConstraint::reset);
	ClassDB::bind_method(D_METHOD("translate", "x", "y"), &SpinePhysicsConstraint::translate);
	ClassDB::bind_method(D_METHOD("rotate", "x", "y", "degrees"), &SpinePhysicsConstraint::rotate);
}

void SpinePhysicsConstraint::update(SpineConstant::Physics physics) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->update((spine::Physics) physics);
}

Ref<SpinePhysicsConstraintData> SpinePhysicsConstraint::get_data() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto &data = get_spine_object()->getData();
	Ref<SpinePhysicsConstraintData> data_ref(memnew(SpinePhysicsConstraintData));
	data_ref->set_spine_object(*get_spine_owner()->get_skeleton_data_res(), &data);
	return data_ref;
}

Ref<SpineBone> SpinePhysicsConstraint::get_bone() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto target = get_spine_object()->getBone();
	if (!target) return nullptr;
	Ref<SpineBone> target_ref(memnew(SpineBone));
	target_ref->set_spine_object(get_spine_owner(), target);
	return target_ref;
}

void SpinePhysicsConstraint::set_bone(Ref<SpineBone> v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setBone(v.is_valid() && v->get_spine_object() ? v->get_spine_object() : nullptr);
}

void SpinePhysicsConstraint::set_inertia(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setInertia(value);
}

float SpinePhysicsConstraint::get_inertia() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getInertia();
}

void SpinePhysicsConstraint::set_strength(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setStrength(value);
}

float SpinePhysicsConstraint::get_strength() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getStrength();
}

void SpinePhysicsConstraint::set_damping(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setDamping(value);
}

float SpinePhysicsConstraint::get_damping() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getDamping();
}

void SpinePhysicsConstraint::set_mass_inverse(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMassInverse(value);
}

float SpinePhysicsConstraint::get_mass_inverse() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMassInverse();
}

void SpinePhysicsConstraint::set_wind(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setWind(value);
}

float SpinePhysicsConstraint::get_wind() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getWind();
}

void SpinePhysicsConstraint::set_gravity(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setGravity(value);
}

float SpinePhysicsConstraint::get_gravity() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getGravity();
}

void SpinePhysicsConstraint::set_mix(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMix(value);
}

float SpinePhysicsConstraint::get_mix() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMix();
}

void SpinePhysicsConstraint::set_reset(bool value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setReset(value);
}

bool SpinePhysicsConstraint::get_reset() {
	SPINE_CHECK(get_spine_object(), false)
	return get_spine_object()->getReset();
}

void SpinePhysicsConstraint::set_ux(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setUx(value);
}

float SpinePhysicsConstraint::get_ux() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getUx();
}

void SpinePhysicsConstraint::set_uy(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setUy(value);
}

float SpinePhysicsConstraint::get_uy() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getUy();
}

void SpinePhysicsConstraint::set_cx(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setCx(value);
}

float SpinePhysicsConstraint::get_cx() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getCx();
}

void SpinePhysicsConstraint::set_cy(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setCy(value);
}

float SpinePhysicsConstraint::get_cy() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getCy();
}

void SpinePhysicsConstraint::set_tx(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setTx(value);
}

float SpinePhysicsConstraint::get_tx() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getTx();
}

void SpinePhysicsConstraint::set_ty(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setTy(value);
}

float SpinePhysicsConstraint::get_ty() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getTy();
}

void SpinePhysicsConstraint::set_x_offset(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setXOffset(value);
}

float SpinePhysicsConstraint::get_x_offset() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getXOffset();
}

void SpinePhysicsConstraint::set_x_velocity(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setXVelocity(value);
}

float SpinePhysicsConstraint::get_x_velocity() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getXVelocity();
}

void SpinePhysicsConstraint::set_y_offset(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setYOffset(value);
}

float SpinePhysicsConstraint::get_y_offset() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getYOffset();
}

void SpinePhysicsConstraint::set_y_velocity(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setYVelocity(value);
}

float SpinePhysicsConstraint::get_y_velocity() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getYVelocity();
}

void SpinePhysicsConstraint::set_rotate_offset(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setRotateOffset(value);
}

float SpinePhysicsConstraint::get_rotate_offset() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getRotateOffset();
}

void SpinePhysicsConstraint::set_rotate_velocity(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setRotateVelocity(value);
}

float SpinePhysicsConstraint::get_rotate_velocity() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getRotateVelocity();
}

void SpinePhysicsConstraint::set_scale_offset(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setScaleOffset(value);
}

float SpinePhysicsConstraint::get_scale_offset() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getScaleOffset();
}

void SpinePhysicsConstraint::set_scale_velocity(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setScaleVelocity(value);
}

float SpinePhysicsConstraint::get_scale_velocity() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getScaleVelocity();
}

void SpinePhysicsConstraint::set_active(bool value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setActive(value);
}

bool SpinePhysicsConstraint::is_active() {
	SPINE_CHECK(get_spine_object(), false)
	return get_spine_object()->isActive();
}

void SpinePhysicsConstraint::set_remaining(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setRemaining(value);
}

float SpinePhysicsConstraint::get_remaining() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getRemaining();
}

void SpinePhysicsConstraint::set_last_Time(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setLastTime(value);
}

float SpinePhysicsConstraint::get_last_Time() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getLastTime();
}

void SpinePhysicsConstraint::reset() {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->reset();
}

void SpinePhysicsConstraint::translate(float x, float y) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->translate(x, y);
}

void SpinePhysicsConstraint::rotate(float x, float y, float degrees) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->rotate(x, y, degrees);
}
