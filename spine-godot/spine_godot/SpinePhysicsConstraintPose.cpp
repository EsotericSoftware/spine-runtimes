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

#include "SpinePhysicsConstraintPose.h"
#include "SpineCommon.h"

void SpinePhysicsConstraintPose::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_inertia"), &SpinePhysicsConstraintPose::get_inertia);
	ClassDB::bind_method(D_METHOD("set_inertia", "value"), &SpinePhysicsConstraintPose::set_inertia);
	ClassDB::bind_method(D_METHOD("get_strength"), &SpinePhysicsConstraintPose::get_strength);
	ClassDB::bind_method(D_METHOD("set_strength", "value"), &SpinePhysicsConstraintPose::set_strength);
	ClassDB::bind_method(D_METHOD("get_damping"), &SpinePhysicsConstraintPose::get_damping);
	ClassDB::bind_method(D_METHOD("set_damping", "value"), &SpinePhysicsConstraintPose::set_damping);
	ClassDB::bind_method(D_METHOD("get_mass_inverse"), &SpinePhysicsConstraintPose::get_mass_inverse);
	ClassDB::bind_method(D_METHOD("set_mass_inverse", "value"), &SpinePhysicsConstraintPose::set_mass_inverse);
	ClassDB::bind_method(D_METHOD("get_wind"), &SpinePhysicsConstraintPose::get_wind);
	ClassDB::bind_method(D_METHOD("set_wind", "value"), &SpinePhysicsConstraintPose::set_wind);
	ClassDB::bind_method(D_METHOD("get_gravity"), &SpinePhysicsConstraintPose::get_gravity);
	ClassDB::bind_method(D_METHOD("set_gravity", "value"), &SpinePhysicsConstraintPose::set_gravity);
	ClassDB::bind_method(D_METHOD("get_mix"), &SpinePhysicsConstraintPose::get_mix);
	ClassDB::bind_method(D_METHOD("set_mix", "value"), &SpinePhysicsConstraintPose::set_mix);

	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "inertia", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_inertia", "get_inertia");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "strength", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_strength", "get_strength");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "damping", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_damping", "get_damping");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "mass_inverse", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_mass_inverse", "get_mass_inverse");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "wind", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_wind", "get_wind");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "gravity", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_gravity", "get_gravity");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "mix", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_mix", "get_mix");
}

float SpinePhysicsConstraintPose::get_inertia() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getInertia();
}

void SpinePhysicsConstraintPose::set_inertia(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setInertia(value);
}

float SpinePhysicsConstraintPose::get_strength() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getStrength();
}

void SpinePhysicsConstraintPose::set_strength(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setStrength(value);
}

float SpinePhysicsConstraintPose::get_damping() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getDamping();
}

void SpinePhysicsConstraintPose::set_damping(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setDamping(value);
}

float SpinePhysicsConstraintPose::get_mass_inverse() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMassInverse();
}

void SpinePhysicsConstraintPose::set_mass_inverse(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMassInverse(value);
}

float SpinePhysicsConstraintPose::get_wind() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getWind();
}

void SpinePhysicsConstraintPose::set_wind(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setWind(value);
}

float SpinePhysicsConstraintPose::get_gravity() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getGravity();
}

void SpinePhysicsConstraintPose::set_gravity(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setGravity(value);
}

float SpinePhysicsConstraintPose::get_mix() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMix();
}

void SpinePhysicsConstraintPose::set_mix(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMix(value);
}