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

#pragma once

#include "SpineCommon.h"
#include "SpinePhysicsConstraintData.h"
#include "SpineBone.h"
#include <spine/PhysicsConstraint.h>

class SpinePhysicsConstraint : public SpineSpriteOwnedObject<spine::PhysicsConstraint> {
	GDCLASS(SpinePhysicsConstraint, SpineObjectWrapper)

protected:
	static void _bind_methods();

public:
	void update(SpineConstant::Physics physics);

	Ref<SpinePhysicsConstraintData> get_data();

	Ref<SpineBone> get_bone();

	void set_bone(Ref<SpineBone> v);

	void set_inertia(float value);
	float get_inertia();

	void set_strength(float value);
	float get_strength();

	void set_damping(float value);
	float get_damping();

	void set_mass_inverse(float value);
	float get_mass_inverse();

	void set_wind(float value);
	float get_wind();

	void set_gravity(float value);
	float get_gravity();

	void set_mix(float value);
	float get_mix();

	void set_reset(bool value);
	bool get_reset();

	void set_ux(float value);
	float get_ux();

	void set_uy(float value);
	float get_uy();

	void set_cx(float value);
	float get_cx();

	void set_cy(float value);
	float get_cy();

	void set_tx(float value);
	float get_tx();

	void set_ty(float value);
	float get_ty();

	void set_x_offset(float value);
	float get_x_offset();

	void set_x_velocity(float value);
	float get_x_velocity();

	void set_y_offset(float value);
	float get_y_offset();

	void set_y_velocity(float value);
	float get_y_velocity();

	void set_rotate_offset(float value);
	float get_rotate_offset();

	void set_rotate_velocity(float value);
	float get_rotate_velocity();

	void set_scale_offset(float value);
	float get_scale_offset();

	void set_scale_velocity(float value);
	float get_scale_velocity();

	void set_active(bool value);
	bool is_active();

	void set_remaining(float value);
	float get_remaining();

	void set_last_Time(float value);
	float get_last_Time();

	void reset();

	void translate(float x, float y);

	void rotate(float x, float y, float degrees);
};
