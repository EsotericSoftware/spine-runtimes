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

#include "SpineSkeleton.h"
#include "SpineAnimationState.h"
#ifdef SPINE_GODOT_EXTENSION
#include "SpineCommon.h"
#include <godot_cpp/classes/node2d.hpp>
#include <godot_cpp/templates/vector.hpp>
#include <godot_cpp/classes/rendering_server.hpp>
#include <godot_cpp/classes/canvas_item_material.hpp>
#else
#include "scene/2d/node_2d.h"
#endif

class SpineSlotNode;

struct SpineRendererObject;

class SpineSprite;

class Attachment;

class SpineMesh2D : public Node2D {
	GDCLASS(SpineMesh2D, Node2D);

	friend class SpineSprite;

protected:
	void _notification(int what);
	static void _bind_methods();

#ifdef SPINE_GODOT_EXTENSION
	PackedVector2Array vertices;
	PackedVector2Array uvs;
	PackedColorArray colors;
	PackedInt32Array indices;
#else
	Vector<Vector2> vertices;
	Vector<Vector2> uvs;
	Vector<Color> colors;
	Vector<int> indices;
#endif
	SpineRendererObject *renderer_object;

	bool indices_changed;

#if VERSION_MAJOR > 3
	RID mesh;
	uint32_t surface_offsets[RS::ARRAY_MAX];
	int num_vertices;
	int num_indices;
	PackedByteArray vertex_buffer;
	PackedByteArray attribute_buffer;
	uint32_t vertex_stride;
	uint32_t normal_tangent_stride;
	uint32_t attribute_stride;
#else
	RID mesh;
	uint32_t surface_offsets[VS::ARRAY_MAX];
	int num_vertices;
	int num_indices;
	uint32_t mesh_surface_offsets[VS::ARRAY_MAX];
	PoolByteArray mesh_buffer;
	uint32_t mesh_stride[VS::ARRAY_MAX];
	uint32_t mesh_surface_format;
#endif

public:
#if VERSION_MAJOR > 3
	SpineMesh2D() : renderer_object(nullptr), indices_changed(true), num_vertices(0), num_indices(0), vertex_stride(0), normal_tangent_stride(0), attribute_stride(0) {};
	~SpineMesh2D() {
		if (mesh.is_valid()) {
#ifdef SPINE_GODOT_EXTENSION
			RS::get_singleton()->free_rid(mesh);
#else
			RS::get_singleton()->free(mesh);
#endif
		}
	}
#else
	SpineMesh2D() : renderer_object(nullptr), indices_changed(true), num_vertices(0), num_indices(0) {};
	~SpineMesh2D() {
		if (mesh.is_valid()) {
			VS::get_singleton()->free(mesh);
		}
	}
#endif

#ifdef SPINE_GODOT_EXTENSION
	void update_mesh(const PackedVector2Array &vertices,
					 const PackedVector2Array &uvs,
					 const PackedColorArray &colors,
					 const PackedInt32Array &indices,
					 SpineRendererObject *renderer_object);
#else
	void update_mesh(const Vector<Point2> &vertices,
					 const Vector<Point2> &uvs,
					 const Vector<Color> &colors,
					 const Vector<int> &indices,
					 SpineRendererObject *renderer_object);
#endif
};

class SpineSprite : public Node2D,
					public spine::AnimationStateListenerObject {
	GDCLASS(SpineSprite, Node2D)

	friend class SpineBone;

protected:
	Ref<SpineSkeletonDataResource> skeleton_data_res;
	Ref<SpineSkeleton> skeleton;
	Ref<SpineAnimationState> animation_state;
	SpineConstant::UpdateMode update_mode;

	String preview_skin;
	String preview_animation;
	bool preview_frame;
	float preview_time;

	bool debug_root;
	Color debug_root_color;
	bool debug_bones;
	Color debug_bones_color;
	float debug_bones_thickness;
	bool debug_regions;
	Color debug_regions_color;
	bool debug_meshes;
	Color debug_meshes_color;
	bool debug_bounding_boxes;
	Color debug_bounding_boxes_color;
	bool debug_paths;
	Color debug_paths_color;
	bool debug_clipping;
	Color debug_clipping_color;

	spine::Vector<spine::Vector<SpineSlotNode *>> slot_nodes;
	Vector<SpineMesh2D *> mesh_instances;
	Ref<Material> normal_material;
	Ref<Material> additive_material;
	Ref<Material> multiply_material;
	Ref<Material> screen_material;
	spine::SkeletonClipping *skeleton_clipper;
	bool modified_bones;

	static void _bind_methods();
	void _notification(int what);
	void _get_property_list(List<PropertyInfo> *list) const;
	bool _get(const StringName &property, Variant &value) const;
	bool _set(const StringName &property, const Variant &value);

	void generate_meshes_for_slots(Ref<SpineSkeleton> skeleton_ref);
	void remove_meshes();
	void sort_slot_nodes();
	void update_meshes(Ref<SpineSkeleton> skeleton_ref);
	void set_modified_bones() { modified_bones = true; }
	void draw();
	void draw_bone(spine::Bone *bone, const Color &color);

	void callback(spine::AnimationState *state, spine::EventType type, spine::TrackEntry *entry, spine::Event *event) override;

public:
	SpineSprite();
	~SpineSprite();

	void set_skeleton_data_res(const Ref<SpineSkeletonDataResource> &_spine_skeleton_data_resource);

	Ref<SpineSkeletonDataResource> get_skeleton_data_res();

	Ref<SpineSkeleton> get_skeleton();

	Ref<SpineAnimationState> get_animation_state();

	void on_skeleton_data_changed();

	void update_skeleton(float delta);

	Transform2D get_global_bone_transform(const String &bone_name);

	void set_global_bone_transform(const String &bone_name, Transform2D transform);

	SpineConstant::UpdateMode get_update_mode();

	void set_update_mode(SpineConstant::UpdateMode v);

	Ref<SpineSkin> new_skin(const String &name);

	Ref<Material> get_normal_material();

	void set_normal_material(Ref<Material> material);

	Ref<Material> get_additive_material();

	void set_additive_material(Ref<Material> material);

	Ref<Material> get_multiply_material();

	void set_multiply_material(Ref<Material> material);

	Ref<Material> get_screen_material();

	void set_screen_material(Ref<Material> material);

	bool get_debug_root() { return debug_root; }

	void set_debug_root(bool root) { debug_root = root; }

	Color get_debug_root_color() { return debug_root_color; }

	void set_debug_root_color(const Color &color) { debug_root_color = color; }

	bool get_debug_bones() { return debug_bones; }

	void set_debug_bones(bool bones) { debug_bones = bones; }

	Color get_debug_bones_color() { return debug_bones_color; }

	void set_debug_bones_color(const Color &color) { debug_bones_color = color; }

	float get_debug_bones_thickness() { return debug_bones_thickness; }

	void set_debug_bones_thickness(float thickness) { debug_bones_thickness = thickness; }

	bool get_debug_regions() { return debug_regions; }

	void set_debug_regions(bool regions) { debug_regions = regions; }

	Color get_debug_regions_color() { return debug_regions_color; }

	void set_debug_regions_color(const Color &color) { debug_regions_color = color; }

	bool get_debug_meshes() { return debug_meshes; }

	void set_debug_meshes(bool meshes) { debug_meshes = meshes; }

	Color get_debug_meshes_color() { return debug_meshes_color; }

	void set_debug_meshes_color(const Color &color) { debug_meshes_color = color; }

	bool get_debug_paths() { return debug_paths; }

	void set_debug_paths(bool paths) { debug_paths = paths; }

	Color get_debug_paths_color() { return debug_paths_color; }

	void set_debug_paths_color(const Color &color) { debug_paths_color = color; }

	bool get_debug_bounding_boxes() { return debug_bounding_boxes; }

	void set_debug_bounding_boxes(bool paths) { debug_bounding_boxes = paths; }

	Color get_debug_bounding_boxes_color() { return debug_bounding_boxes_color; }

	void set_debug_bounding_boxes_color(const Color &color) { debug_bounding_boxes_color = color; }

	bool get_debug_clipping() { return debug_clipping; }

	void set_debug_clipping(bool clipping) { debug_clipping = clipping; }

	Color get_debug_clipping_color() { return debug_clipping_color; }

	void set_debug_clipping_color(const Color &color) { debug_clipping_color = color; }

#ifndef SPINE_GODOT_EXTENSION
// FIXME
#ifdef TOOLS_ENABLED
	virtual Rect2 _edit_get_rect() const;
	virtual bool _edit_use_rect() const;
#endif
#endif

	static void clear_statics();
};
