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
#include "SpineBoneData.h"
#include <spine/SliderData.h>

class SpineAnimation;
class SpineSliderPose;

class SpineSliderData : public SpineConstraintData {
	GDCLASS(SpineSliderData, SpineConstraintData)

	spine::SliderData *get_spine_constraint_data() {
		return (spine::SliderData *) get_spine_object();
	}

protected:
	static void _bind_methods();

public:
	Ref<SpineAnimation> get_animation();

	bool get_additive();
	void set_additive(bool value);

	bool get_loop();
	void set_loop(bool value);

	Ref<SpineBoneData> get_bone();
	void set_bone(Ref<SpineBoneData> value);

	float get_scale();
	void set_scale(float value);

	float get_offset();
	void set_offset(float value);

	float get_max();
	void set_max(float value);

	bool get_local();
	void set_local(bool value);

	Ref<SpineSliderPose> get_setup_pose();
};