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

#ifndef GODOT_SPINESKELETON_H
#define GODOT_SPINESKELETON_H

#include <spine/spine.h>

#include "SpineSkeletonDataResource.h"
#include "SpineBone.h"
#include "SpineSlot.h"
#include "SpineIkConstraint.h"
#include "SpineTransformConstraint.h"
#include "SpinePathConstraint.h"

class SpineSprite;

class SpineSkeleton : public Reference {
	GDCLASS(SpineSkeleton, Reference);

protected:
	static void _bind_methods();

private:
	spine::Skeleton *skeleton;
	SpineSprite *sprite;
	Ref<SpineSkeletonDataResource> skeleton_data_res;

public:
	SpineSkeleton();
	~SpineSkeleton();

	void set_skeleton_data_res(Ref<SpineSkeletonDataResource> data_res);
	Ref<SpineSkeletonDataResource> get_skeleton_data_res() const;

	inline void set_spine_object(spine::Skeleton *s) {
		skeleton = s;
	}
	inline spine::Skeleton *get_spine_object() {
		return skeleton;
	}

	void set_spine_sprite(SpineSprite *sprite);

	void update_world_transform();

	void set_to_setup_pose();

	void set_bones_to_setup_pose();

	void set_slots_to_setup_pose();

	Ref<SpineBone> find_bone(const String &name);

	Ref<SpineSlot> find_slot(const String &name);

	void set_skin_by_name(const String &skin_name);
	void set_skin(Ref<SpineSkin> new_skin);

	Ref<SpineAttachment> get_attachment_by_slot_name(const String &slot_name, const String &attachment_name);
	Ref<SpineAttachment> get_attachment_by_slot_index(int slot_index, const String &attachment_name);

	void set_attachment(const String &slot_name, const String &attachment_name);

	Ref<SpineIkConstraint> find_ik_constraint(const String &constraint_name);
	Ref<SpineTransformConstraint> find_transform_constraint(const String &constraint_name);
	Ref<SpinePathConstraint> find_path_constraint(const String &constraint_name);

	Dictionary get_bounds();

	Ref<SpineBone> get_root_bone();

	Array get_bones();
	Array get_slots();
	Array get_draw_orders();
	Array get_ik_constraints();
	Array get_path_constraints();
	Array get_transform_constraints();

	Ref<SpineSkin> get_skin();

	Color get_color();
	void set_color(Color v);

	void set_position(Vector2 pos);

	float get_x();
	void set_x(float v);

	float get_y();
	void set_y(float v);

	float get_scale_x();
	void set_scale_x(float v);

	float get_scale_y();
	void set_scale_y(float v);
};

#endif//GODOT_SPINESKELETON_H
