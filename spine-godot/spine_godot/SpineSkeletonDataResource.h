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

#ifndef GODOT_SPINESKELETONDATARESOURCE_H
#define GODOT_SPINESKELETONDATARESOURCE_H

#include "core/variant_parser.h"

#include <spine/spine.h>

#include "SpineAtlasResource.h"
#include "SpineSkeletonJsonDataResource.h"
#include "SpineAnimation.h"
#include "SpineBoneData.h"
#include "SpineSlotData.h"
#include "SpineSkin.h"
#include "SpineIkConstraintData.h"
#include "SpineTransformConstraintData.h"
#include "SpinePathConstraintData.h"
#include "SpineEventData.h"

class SpineSkeletonDataResource : public Resource{
	GDCLASS(SpineSkeletonDataResource, Resource);

protected:
	static void _bind_methods();

private:
	Ref<SpineAtlasResource> atlas_res;
	Ref<SpineSkeletonJsonDataResource> skeleton_json_res;
	bool valid;
	bool spine_object;

	spine::SkeletonData *skeleton_data;

	void update_skeleton_data();
public:

	inline void set_spine_object(spine::SkeletonData *s){
		skeleton_data = s;
		if(s)
			spine_object = true;
	}
	inline spine::SkeletonData *get_spine_object(){
		return skeleton_data;
	}

	void load_res(spine::Atlas *a, const String &json_path);

	SpineSkeletonDataResource();
	virtual ~SpineSkeletonDataResource();

    void _get_property_list(List<PropertyInfo> *p_list) const;

	void set_atlas_res(const Ref<SpineAtlasResource> &a);
	Ref<SpineAtlasResource> get_atlas_res();

	void set_skeleton_json_res(const Ref<SpineSkeletonJsonDataResource> &s);
	Ref<SpineSkeletonJsonDataResource> get_skeleton_json_res();

	inline spine::SkeletonData *get_skeleton_data(){return skeleton_data;}

	bool is_skeleton_data_loaded() const;

	void get_animation_names(Vector<String> &l) const;
    void get_skin_names(Vector<String> &l) const;

	// spine api
	Ref<SpineBoneData> find_bone(const String &bone_name);	

	Ref<SpineSlotData> find_slot(const String &slot_name);	

	Ref<SpineSkin> find_skin(const String &skin_name);

	Ref<SpineEventData> find_event(const String &event_data_name);

	Ref<SpineAnimation> find_animation(const String &animation_name);

	Ref<SpineIkConstraintData> find_ik_constraint(const String &constraint_name);
	Ref<SpineTransformConstraintData> find_transform_constraint(const String &constraint_name);
	Ref<SpinePathConstraintData> find_path_constraint(const String &constraint_name);	

	Array get_bones();
	Array get_slots();
	Array get_skins() const;

	Ref<SpineSkin> get_default_skin();
	void set_default_skin(Ref<SpineSkin> v);

	Array get_events();
	Array get_animations();
	Array get_ik_constraints();
	Array get_transform_constraints();
	Array get_path_constraints();

	String get_sk_name();
	void set_sk_name(const String &v);

	float get_x();
	void set_x(float v);

	float get_y();
	void set_y(float v);

	float get_width();
	float get_height();

	String get_version();

	float get_fps();
	void set_fps(float v);
};

#endif //GODOT_SPINESKELETONDATARESOURCE_H
