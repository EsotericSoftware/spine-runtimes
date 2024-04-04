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

#include "SpinePhysicsConstraintData.h"
#include "SpineCommon.h"

void SpinePhysicsConstraintData::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_bone"), &SpinePhysicsConstraintData::get_bone);
	ClassDB::bind_method(D_METHOD("get_scale_x"), &SpinePhysicsConstraintData::get_scale_x);
	ClassDB::bind_method(D_METHOD("get_shear_x"), &SpinePhysicsConstraintData::get_shear_x);
	ClassDB::bind_method(D_METHOD("get_limit"), &SpinePhysicsConstraintData::get_limit);
	ClassDB::bind_method(D_METHOD("get_step"), &SpinePhysicsConstraintData::get_step);
	ClassDB::bind_method(D_METHOD("get_inertia"), &SpinePhysicsConstraintData::get_inertia);
	ClassDB::bind_method(D_METHOD("get_strength"), &SpinePhysicsConstraintData::get_strength);
	ClassDB::bind_method(D_METHOD("get_damping"), &SpinePhysicsConstraintData::get_damping);
	ClassDB::bind_method(D_METHOD("get_mass_inverse"), &SpinePhysicsConstraintData::get_mass_inverse);
	ClassDB::bind_method(D_METHOD("get_wind"), &SpinePhysicsConstraintData::get_wind);
	ClassDB::bind_method(D_METHOD("get_gravity"), &SpinePhysicsConstraintData::get_gravity);
	ClassDB::bind_method(D_METHOD("get_mix"), &SpinePhysicsConstraintData::get_mix);
	ClassDB::bind_method(D_METHOD("is_inertia_global"), &SpinePhysicsConstraintData::is_inertia_global);
	ClassDB::bind_method(D_METHOD("is_strength_global"), &SpinePhysicsConstraintData::is_strength_global);
	ClassDB::bind_method(D_METHOD("is_damping_global"), &SpinePhysicsConstraintData::is_damping_global);
	ClassDB::bind_method(D_METHOD("is_mass_global"), &SpinePhysicsConstraintData::is_mass_global);
	ClassDB::bind_method(D_METHOD("is_wind_global"), &SpinePhysicsConstraintData::is_wind_global);
	ClassDB::bind_method(D_METHOD("is_gravity_global"), &SpinePhysicsConstraintData::is_gravity_global);
	ClassDB::bind_method(D_METHOD("is_mix_global"), &SpinePhysicsConstraintData::is_mix_global);
}


Ref<SpineBoneData> SpinePhysicsConstraintData::get_bone() {
	SPINE_CHECK(get_spine_constraint_data(), nullptr)
	auto bone = get_spine_constraint_data()->getBone();
	if (!bone) return nullptr;
	Ref<SpineBoneData> slot_ref(memnew(SpineBoneData));
	slot_ref->set_spine_object(get_spine_owner(), bone);
	return slot_ref;
}

float SpinePhysicsConstraintData::get_scale_x() {
	SPINE_CHECK(get_spine_constraint_data(), 0)
	return get_spine_constraint_data()->getScaleX();
}

float SpinePhysicsConstraintData::get_shear_x() {
	SPINE_CHECK(get_spine_constraint_data(), 0)
	return get_spine_constraint_data()->getShearX();
}

float SpinePhysicsConstraintData::get_limit() {
	SPINE_CHECK(get_spine_constraint_data(), 0)
	return get_spine_constraint_data()->getLimit();
}

float SpinePhysicsConstraintData::get_step() {
	SPINE_CHECK(get_spine_constraint_data(), 0)
	return get_spine_constraint_data()->getStep();
}

float SpinePhysicsConstraintData::get_inertia() {
	SPINE_CHECK(get_spine_constraint_data(), 0)
	return get_spine_constraint_data()->getInertia();
}

float SpinePhysicsConstraintData::get_strength() {
	SPINE_CHECK(get_spine_constraint_data(), 0)
	return get_spine_constraint_data()->getStrength();
}

float SpinePhysicsConstraintData::get_damping() {
	SPINE_CHECK(get_spine_constraint_data(), 0)
	return get_spine_constraint_data()->getDamping();
}

float SpinePhysicsConstraintData::get_mass_inverse() {
	SPINE_CHECK(get_spine_constraint_data(), 0)
	return get_spine_constraint_data()->getMassInverse();
}

float SpinePhysicsConstraintData::get_wind() {
	SPINE_CHECK(get_spine_constraint_data(), 0)
	return get_spine_constraint_data()->getWind();
}

float SpinePhysicsConstraintData::get_gravity() {
	SPINE_CHECK(get_spine_constraint_data(), 0)
	return get_spine_constraint_data()->getGravity();
}

float SpinePhysicsConstraintData::get_mix() {
	SPINE_CHECK(get_spine_constraint_data(), 0)
	return get_spine_constraint_data()->getMix();
}

bool SpinePhysicsConstraintData::is_inertia_global() {
	SPINE_CHECK(get_spine_constraint_data(), false)
	return get_spine_constraint_data()->isInertiaGlobal();
}

bool SpinePhysicsConstraintData::is_strength_global() {
	SPINE_CHECK(get_spine_constraint_data(), false)
	return get_spine_constraint_data()->isStrengthGlobal();
}

bool SpinePhysicsConstraintData::is_damping_global() {
	SPINE_CHECK(get_spine_constraint_data(), false)
	return get_spine_constraint_data()->isDampingGlobal();
}

bool SpinePhysicsConstraintData::is_mass_global() {
	SPINE_CHECK(get_spine_constraint_data(), false)
	return get_spine_constraint_data()->isMassGlobal();
}

bool SpinePhysicsConstraintData::is_wind_global() {
	SPINE_CHECK(get_spine_constraint_data(), false)
	return get_spine_constraint_data()->isWindGlobal();
}

bool SpinePhysicsConstraintData::is_gravity_global() {
	SPINE_CHECK(get_spine_constraint_data(), false)
	return get_spine_constraint_data()->isGravityGlobal();
}

bool SpinePhysicsConstraintData::is_mix_global() {
	SPINE_CHECK(get_spine_constraint_data(), false)
	return get_spine_constraint_data()->isMixGlobal();
}
