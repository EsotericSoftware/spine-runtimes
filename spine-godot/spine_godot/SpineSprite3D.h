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

#if VERSION_MAJOR > 3

#include "SpineController.h"
#ifdef SPINE_GODOT_EXTENSION
#include <godot_cpp/classes/node3d.hpp>
#include <godot_cpp/classes/mesh.hpp>
#include <godot_cpp/classes/shader.hpp>
#include <godot_cpp/classes/shader_material.hpp>
#include <godot_cpp/classes/rendering_server.hpp>
#include <godot_cpp/core/object_id.hpp>
#else
#include "scene/3d/node_3d.h"
#include "scene/resources/material.h"
#include "scene/resources/mesh.h"
#include "scene/resources/shader.h"
#if VERSION_MINOR >= 6
#include "servers/rendering/rendering_server.h"
#else
#include "servers/rendering_server.h"
#endif
#endif
#include <vector>

#ifdef TOOLS_ENABLED
#ifdef SPINE_GODOT_EXTENSION
#include <godot_cpp/classes/triangle_mesh.hpp>
#else
#include "core/math/triangle_mesh.h"
#endif
#endif

class SpineSlotNode3D;
struct SpineRendererObject;

struct SpineVertex3D {
	float position[3];
	float uv[2];
	float light[4];
	float dark[3];
};

struct SpineBatch3D {
	RID mesh;
	RID instance;
	Ref<ShaderMaterial> material;
	std::vector<SpineVertex3D> vertices;
	std::vector<uint32_t> indices;
	std::vector<uint32_t> uploaded_indices;
	PackedByteArray vertex_buffer;
	PackedByteArray attribute_buffer;
	PackedByteArray index_buffer;
	uint32_t surface_offsets[Mesh::ARRAY_MAX] = {};
	uint32_t vertex_stride = 0;
	uint32_t attribute_stride = 0;
	uint32_t index_stride = 0;
	int vertex_capacity = 0;
	int index_capacity = 0;
	bool active = false;
	float sorting_offset = 0;
	bool sorting_initialized = false;
	bool last_visible = false;
	RID last_scenario;
	RID last_material;
	Transform3D last_transform;
	AABB bounds;
	AABB uploaded_bounds;

	SpineBatch3D();
	~SpineBatch3D();
	void clear_mesh();
};

struct SpineBatchMaterial3D {
	RID texture;
	ObjectID source;
	Ref<ShaderMaterial> material;
	spine::BlendMode blend;
	bool pma;
	int priority;
	int cull;
	bool depth_write;
};

struct SpineDrawItem3D {
	SpineBatch3D *batch;
	ObjectID geometry;
	Vector3 origin;
	float offset;
	ObjectID helper;
	uint64_t generation = 0;
};

class SpineSprite3D : public Node3D, public SpineControllerListener {
	GDCLASS(SpineSprite3D, Node3D)
	friend class SpineSlotNode3D;
	friend class SpineAnimationTrack;
#ifdef TOOLS_ENABLED
	friend class SpineSprite3DGizmoPlugin;
	Ref<TriangleMesh> editor_selection_mesh;
	bool editor_geometry_dirty = true;
	Ref<TriangleMesh> get_editor_selection_mesh();
	void update_editor_selection();
#endif

public:
	enum CullMode {
		CULL_DISABLED,
		CULL_BACK,
		CULL_FRONT
	};

private:
	Ref<SpineController> controller;
	Ref<SpineSkeletonDataResource> connected_skeleton_data_res;
	SpineConstant::UpdateMode update_mode;
	float time_scale;
	float pixel_size;
	float slot_depth_offset;
	bool camera_relative_depth;
	bool depth_write_enabled;
	float alpha_cutoff;
	float sorting_step;
	int render_priority;
	CullMode cull_mode;
	Ref<ShaderMaterial> normal_material;
	Ref<ShaderMaterial> additive_material;
	static Ref<Shader> generated_shaders[2][3][2];
	std::vector<SpineBatchMaterial3D> materials;

	String preview_skin;
	String preview_animation;
	bool preview_frame;
	float preview_time;

	std::vector<SpineBatch3D *> batches;
	std::vector<SpineDrawItem3D> draw_items;
	std::vector<std::vector<ObjectID>> slot_nodes;
	RID render_scenario;
	spine::SkeletonClipping *skeleton_clipper;
	spine::Array<unsigned short> quad_indices;
	spine::Array<float> scratch_vertices;
	bool slots_dirty;
	bool updating_meshes;
	bool warned_multiply;
	bool warned_screen;
	int active_batches;
	int active_vertices;
	int active_indices;
	uint64_t mesh_builds;
	uint64_t index_uploads;
	uint64_t vertex_uploads;
	uint64_t attribute_uploads;
	uint64_t material_builds;

	static void _bind_methods();
	void _notification(int what);
	void _get_property_list(List<PropertyInfo> *list) const;
	bool _get(const StringName &property, Variant &value) const;
	bool _set(const StringName &property, const Variant &value);

	void remove_render_items();
	void rebuild_slot_nodes();
	void update_render_items(Ref<SpineSkeleton> skeleton);
	void refresh_render_items();
	void update_render_item_world_state();
	void update_sorting();
	void invalidate_materials();
	void apply_depth_parameters(const Ref<Material> &material);
	void refresh_depth_parameters();
	void refresh_material_template(const Ref<ShaderMaterial> &source);
	SpineBatch3D *begin_batch(const Ref<ShaderMaterial> &material);
	void upload_batch(SpineBatch3D *batch);
	Ref<ShaderMaterial> get_material(SpineRendererObject *renderer_object, bool pma, spine::BlendMode blend,
									 const Ref<ShaderMaterial> &override_material);
	Ref<Shader> get_generated_shader(spine::BlendMode blend_mode);

	void before_skeleton_data_change() override;
	void skeleton_data_changed() override;
	bool should_apply_pose() override;
	void on_animation_started(Ref<SpineAnimationState> animation_state, Ref<SpineTrackEntry> track_entry) override;
	void on_animation_interrupted(Ref<SpineAnimationState> animation_state, Ref<SpineTrackEntry> track_entry) override;
	void on_animation_ended(Ref<SpineAnimationState> animation_state, Ref<SpineTrackEntry> track_entry) override;
	void on_animation_completed(Ref<SpineAnimationState> animation_state, Ref<SpineTrackEntry> track_entry) override;
	void on_animation_disposed(Ref<SpineAnimationState> animation_state, Ref<SpineTrackEntry> track_entry) override;
	void on_animation_event(Ref<SpineAnimationState> animation_state, Ref<SpineTrackEntry> track_entry, Ref<SpineEvent> event) override;
	void before_animation_state_update() override;
	void before_animation_state_apply() override;
	void before_world_transforms_change() override;
	void world_transforms_changed() override;

public:
	SpineSprite3D();
	~SpineSprite3D() override;
	static void clear_statics();
	static String get_depth_shader_code();
	AABB get_aabb() const;
	void set_skeleton_data_res(const Ref<SpineSkeletonDataResource> &skeleton_data_res);
	Ref<SpineSkeletonDataResource> get_skeleton_data_res();
	Ref<SpineSkeleton> get_skeleton();
	Ref<SpineAnimationState> get_animation_state();
	void on_skeleton_data_changed();
	void update_skeleton(float delta);
	SpineConstant::UpdateMode get_update_mode();
	void set_update_mode(SpineConstant::UpdateMode value);
	Ref<SpineSkin> new_skin(const String &name);
	void set_time_scale(float value);
	float get_time_scale();
	void set_pixel_size(float value);
	float get_pixel_size();
	void set_slot_depth_offset(float value);
	float get_slot_depth_offset();
	void set_camera_relative_depth(bool value);
	bool get_camera_relative_depth() const;
	void set_depth_write_enabled(bool value);
	bool get_depth_write_enabled() const;
	void set_alpha_cutoff(float value);
	float get_alpha_cutoff() const;
	void set_sorting_step(float value);
	float get_sorting_step();
	void set_render_priority(int value);
	int get_render_priority();
	void set_cull_mode(CullMode value);
	CullMode get_cull_mode();
	void set_normal_material(const Ref<ShaderMaterial> &material);
	Ref<ShaderMaterial> get_normal_material();
	void set_additive_material(const Ref<ShaderMaterial> &material);
	Ref<ShaderMaterial> get_additive_material();
	Dictionary get_render_statistics() const;
};

VARIANT_ENUM_CAST(SpineSprite3D::CullMode)
#endif
