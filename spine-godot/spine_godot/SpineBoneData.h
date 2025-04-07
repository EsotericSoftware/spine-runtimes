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

#include "SpineCommon.h"
#include "SpineConstant.h"
#include <spine/BoneData.h>

class SpineSkeletonDataResource;

class SpineBoneData : public SpineSkeletonDataResourceOwnedObject<spine::BoneData> {
	GDCLASS(SpineBoneData, SpineObjectWrapper)

protected:
	static void _bind_methods();

public:
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

	SpineConstant::Inherit get_inherit();

	void set_inherit(SpineConstant::Inherit v);

	bool is_skin_required();

	void set_skin_required(bool v);

	Color get_color();

	void set_color(Color color);

	String get_icon();

	bool is_visible();

	void set_visible(bool v);
};
