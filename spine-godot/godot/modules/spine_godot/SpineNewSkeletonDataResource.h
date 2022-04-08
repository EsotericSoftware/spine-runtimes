#ifndef GODOT_SPINENEWSKELETONDATARESOURCE_H
#define GODOT_SPINENEWSKELETONDATARESOURCE_H

#include "SpineAtlasResource.h"
#include "SpineSkeletonFileResource.h"
#include "SpineAnimation.h"
#include "SpineBoneData.h"
#include "SpineSlotData.h"
#include "SpineSkin.h"
#include "SpineIkConstraintData.h"
#include "SpineTransformConstraintData.h"
#include "SpinePathConstraintData.h"
#include "SpineEventData.h"

class SpineNewSkeletonDataResource : public Resource {
	GDCLASS(SpineNewSkeletonDataResource, Resource);

protected:
	static void _bind_methods();

private:
	Ref<SpineAtlasResource> atlas_res;
	Ref<SpineSkeletonFileResource> skeleton_file_res;
	bool valid;
	bool spine_object;

	spine::SkeletonData *skeleton_data;

	void update_skeleton_data();

public:
	SpineNewSkeletonDataResource();
	virtual ~SpineNewSkeletonDataResource();

	inline void set_spine_object(spine::SkeletonData *s) {
		skeleton_data = s;
		if (s)
			spine_object = true;
	}
	inline spine::SkeletonData *get_spine_object() {
		return skeleton_data;
	}

	void load_res(spine::Atlas *atlas, const String &json, const Vector<uint8_t> &binary);

	void _get_property_list(List<PropertyInfo> *p_list) const;

	void set_atlas_res(const Ref<SpineAtlasResource> &a);
	Ref<SpineAtlasResource> get_atlas_res();

	void set_skeleton_file_res(const Ref<SpineSkeletonFileResource> &s);
	Ref<SpineSkeletonFileResource> get_skeleton_file_res();

	inline spine::SkeletonData *get_skeleton_data() { return skeleton_data; }

	bool is_skeleton_data_loaded() const;

	void get_animation_names(Vector<String> &l) const;
	void get_skin_names(Vector<String> &l) const;

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

	String get_skeleton_name();
	void set_skeleton_name(const String &v);

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


#endif //GODOT_SPINENEWSKELETONDATARESOURCE_H
