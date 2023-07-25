#pragma once

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

class SpineAnimationMix : public Resource {
	GDCLASS(SpineAnimationMix, Resource)

protected:
	static void _bind_methods();

	String from;
	String to;
	float mix;

public:
	SpineAnimationMix();

	void set_from(const String &from);

	String get_from();

	void set_to(const String &to);

	String get_to();

	void set_mix(float mix);

	float get_mix();
};

class SpineSkeletonDataResource : public Resource {
	GDCLASS(SpineSkeletonDataResource, Resource)

protected:
	static void _bind_methods();

private:
	Ref<SpineAtlasResource> atlas_res;
	Ref<SpineSkeletonFileResource> skeleton_file_res;
	float default_mix;
	Array animation_mixes;

	spine::SkeletonData *skeleton_data;
	spine::AnimationStateData *animation_state_data;

	void update_skeleton_data();

	void load_resources(spine::Atlas *atlas, const String &json, const Vector<uint8_t> &binary);

public:
	SpineSkeletonDataResource();
	virtual ~SpineSkeletonDataResource();

	bool is_skeleton_data_loaded() const;

	void set_atlas_res(const Ref<SpineAtlasResource> &atlas);
	Ref<SpineAtlasResource> get_atlas_res();

	void set_skeleton_file_res(const Ref<SpineSkeletonFileResource> &skeleton_file);
	Ref<SpineSkeletonFileResource> get_skeleton_file_res();

	spine::SkeletonData *get_skeleton_data() const { return skeleton_data; }

	spine::AnimationStateData *get_animation_state_data() const { return animation_state_data; }

	void get_animation_names(Vector<String> &animation_names) const;

	void get_skin_names(Vector<String> &l) const;

	void get_slot_names(Vector<String> &slot_names);

	void get_bone_names(Vector<String> &bone_names);

	void set_default_mix(float default_mix);

	float get_default_mix();

	void set_animation_mixes(Array animation_mixes);

	Array get_animation_mixes();

	// Used by SpineEditorPropertyAnimationMix(es) to update the underlying AnimationState
	void update_mixes();

	// Spine API
	Ref<SpineBoneData> find_bone(const String &bone_name) const;

	Ref<SpineSlotData> find_slot(const String &slot_name) const;

	Ref<SpineSkin> find_skin(const String &skin_name) const;

	Ref<SpineEventData> find_event(const String &event_data_name) const;

	Ref<SpineAnimation> find_animation(const String &animation_name) const;

	Ref<SpineIkConstraintData> find_ik_constraint(const String &constraint_name) const;

	Ref<SpineTransformConstraintData> find_transform_constraint(const String &constraint_name) const;

	Ref<SpinePathConstraintData> find_path_constraint(const String &constraint_name) const;

	String get_skeleton_name() const;

	Array get_bones() const;

	Array get_slots() const;

	Array get_skins() const;

	Ref<SpineSkin> get_default_skin() const;

	void set_default_skin(Ref<SpineSkin> skin);

	Array get_events() const;

	Array get_animations() const;

	Array get_ik_constraints() const;

	Array get_transform_constraints() const;

	Array get_path_constraints() const;

	float get_x() const;

	float get_y() const;

	float get_width() const;

	float get_height() const;

	String get_version() const;

	String get_hash() const;

	String get_images_path() const;

	String get_audio_path() const;

	float get_fps() const;
};
