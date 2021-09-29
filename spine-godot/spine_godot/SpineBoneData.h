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

#ifndef GODOT_SPINEBONEDATA_H
#define GODOT_SPINEBONEDATA_H

#include "core/variant_parser.h"

#include <spine/spine.h>

class SpineBoneData : public Reference {
	GDCLASS(SpineBoneData, Reference);

protected:
	static void _bind_methods();

private:
	spine::BoneData *bone_data;

public:
	SpineBoneData();
	~SpineBoneData();

	inline void set_spine_object(spine::BoneData *b) {
		bone_data = b;
	}
	inline spine::BoneData *get_spine_object() {
		return bone_data;
	}

	enum TransformMode {
		TRANSFORMMODE_NORMAL = 0,
		TRANSFORMMODE_ONLYTRANSLATION,
		TRANSFORMMODE_NOROTATIONORREFLECTION,
		TRANSFORMMODE_NOSCALE,
		TRANSFORMMODE_NOSCALEORREFLECTION
	};

	int get_index();

	String get_bone_name();

	Ref<SpineBoneData> get_parent();

	float get_length();
	void set_length(float v);

	float get_x();
	void set_x(float v);

	float get_y();
	void set_y(float v);

	float get_rotation();
	void set_rotation(float v);

	float get_scale_x();
	void set_scale_x(float v);

	float get_scale_y();
	void set_scale_y(float v);

	float get_shear_x();
	void set_shear_x(float v);

	float get_shear_y();
	void set_shear_y(float v);

	TransformMode get_transform_mode();
	void set_transform_mode(TransformMode v);

	bool is_skin_required();
	void set_skin_required(bool v);
};

VARIANT_ENUM_CAST(SpineBoneData::TransformMode);
#endif//GODOT_SPINEBONEDATA_H
