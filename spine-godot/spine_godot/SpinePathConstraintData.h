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

#pragma once

#include "SpineConstraintData.h"
#include "SpineConstant.h"
#include "SpineBoneData.h"
#include "SpineSlotData.h"
#include <spine/PathConstraintData.h>

class SpinePathConstraintData : public SpineConstraintData {
	GDCLASS(SpinePathConstraintData, SpineConstraintData)

	spine::PathConstraintData *get_spine_constraint_data() { return (spine::PathConstraintData *) get_spine_object(); }

protected:
	static void _bind_methods();

public:
	Array get_bones();

	Ref<SpineSlotData> get_target();

	void set_target(Ref<SpineSlotData> v);

	SpineConstant::PositionMode get_position_mode();

	void set_position_mode(SpineConstant::PositionMode v);

	SpineConstant::SpacingMode get_spacing_mode();

	void set_spacing_mode(SpineConstant::SpacingMode v);

	SpineConstant::RotateMode get_rotate_mode();

	void set_rotate_mode(SpineConstant::RotateMode v);

	float get_offset_rotation();

	void set_offset_rotation(float v);

	float get_position();

	void set_position(float v);

	float get_spacing();

	void set_spacing(float v);

	float get_mix_rotate();

	void set_mix_rotate(float v);

	float get_mix_x();

	void set_mix_x(float v);

	float get_mix_y();

	void set_mix_y(float v);
};
