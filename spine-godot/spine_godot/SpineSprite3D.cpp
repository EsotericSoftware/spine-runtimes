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

#include "SpineSprite3D.h"

#if VERSION_MAJOR > 3

#include "SpineEvent.h"
#include "SpineRendererObject.h"
#include "SpineSkeletonDataResource.h"
#include "SpineSkin.h"
#include "SpineTrackEntry.h"
#include "SpineSlotNode3D.h"

#ifdef SPINE_GODOT_EXTENSION
#include <godot_cpp/classes/engine.hpp>
#include <godot_cpp/classes/mesh.hpp>
#include <godot_cpp/classes/geometry_instance3d.hpp>
#include <godot_cpp/classes/world3d.hpp>
#else
#include "core/config/engine.h"
#include "scene/3d/visual_instance_3d.h"
#if __has_include("scene/resources/3d/world_3d.h")
#include "scene/resources/3d/world_3d.h"
#else
#include "scene/resources/world_3d.h"
#endif
#endif

#include <spine/Atlas.h>
#include <spine/ClippingAttachment.h>
#include <spine/MeshAttachment.h>
#include <spine/RegionAttachment.h>
#include <spine/SkeletonClipping.h>

#include <cstring>
#include <climits>
#include <algorithm>
#include <mutex>

Ref<Shader> SpineSprite3D::generated_shaders[2][3][2][2];
Ref<Shader> SpineSprite3D::generated_shadow_shaders[3];
static std::mutex generated_shader_mutex;

void SpineSprite3D::clear_statics() {
	std::lock_guard<std::mutex> lock(generated_shader_mutex);
	for (auto &blend : generated_shaders)
		for (auto &cull : blend)
			for (auto &depth : cull)
				for (auto &shader : depth) shader.unref();
	for (auto &shader : generated_shadow_shaders) shader.unref();
}

SpineBatch3D::SpineBatch3D() {
	instance = RS::get_singleton()->instance_create();
	// Godot instances start visible; synchronize the server with cached state.
	RS::get_singleton()->instance_set_visible(instance, false);
#if !defined(SPINE_GODOT_EXTENSION) && VERSION_MINOR >= 7
	RS::get_singleton()->instance_geometry_set_cast_shadows_setting(instance, RSE::SHADOW_CASTING_SETTING_OFF);
#else
	RS::get_singleton()->instance_geometry_set_cast_shadows_setting(instance, RS::SHADOW_CASTING_SETTING_OFF);
#endif
}

void SpineBatch3D::ensure_shadow_instance() {
	if (shadow_instance.is_valid()) return;
	shadow_instance = RS::get_singleton()->instance_create();
	RS::get_singleton()->instance_set_visible(shadow_instance, false);
#if !defined(SPINE_GODOT_EXTENSION) && VERSION_MINOR >= 7
	RS::get_singleton()->instance_geometry_set_cast_shadows_setting(shadow_instance, RSE::SHADOW_CASTING_SETTING_SHADOWS_ONLY);
#else
	RS::get_singleton()->instance_geometry_set_cast_shadows_setting(shadow_instance, RS::SHADOW_CASTING_SETTING_SHADOWS_ONLY);
#endif
}

SpineBatch3D::~SpineBatch3D() {
	clear_mesh();
	if (instance.is_valid()) RS::get_singleton()->free_rid(instance);
	if (shadow_instance.is_valid()) RS::get_singleton()->free_rid(shadow_instance);
}

void SpineBatch3D::clear_mesh() {
	if (mesh.is_valid()) {
		RS::get_singleton()->free_rid(mesh);
		mesh = RID();
	}
	if (instance.is_valid()) RS::get_singleton()->instance_set_base(instance, RID());
	if (shadow_instance.is_valid()) RS::get_singleton()->instance_set_base(shadow_instance, RID());
	vertex_capacity = 0;
	index_capacity = 0;
	uploaded_indices.clear();
	vertex_buffer.clear();
	attribute_buffer.clear();
	index_buffer.clear();
	last_material = RID();
	last_shadow_material = RID();
	last_shadow_mesh = RID();
	uploaded_bounds = AABB();
}

static uint64_t get_custom_array_flags() {
	return (uint64_t(Mesh::ARRAY_CUSTOM_RGBA_FLOAT) << Mesh::ARRAY_FORMAT_CUSTOM0_SHIFT) |
		(uint64_t(Mesh::ARRAY_CUSTOM_RGB_FLOAT) << Mesh::ARRAY_FORMAT_CUSTOM1_SHIFT);
}

static int grow_capacity(int capacity, int count) {
	if (capacity >= count) return capacity;
	// Allocate the first surface exactly. Geometric growth is only needed when
	// an existing batch outgrows its buffers (for example, animated clipping).
	if (capacity > INT_MAX / 2) return count;
	return MAX(count, capacity * 2);
}

static bool update_bytes(uint8_t *target, const void *source, size_t size) {
	if (memcmp(target, source, size) == 0) return false;
	memcpy(target, source, size);
	return true;
}

static bool update_tangent_bytes(uint8_t *target, const float tangent[4]) {
	Vector3 direction(tangent[0], tangent[1], tangent[2]);
	Vector2 encoded = direction.octahedron_tangent_encode(tangent[3]);
	uint16_t packed[2] = {(uint16_t) CLAMP(encoded.x * 65535, 0, 65535), (uint16_t) CLAMP(encoded.y * 65535, 0, 65535)};
	// (1, 1) and (0, 1) decode identically, but the latter collides with
	// Godot's compression sentinel.
	if (packed[0] == 0 && packed[1] == 65535) packed[0] = 65535;
	return update_bytes(target, packed, sizeof(packed));
}

void SpineSprite3D::upload_batch(SpineBatch3D *item) {
	int cast_shadows = item->custom_material ? (int) shadow_casting : (int) SHADOW_CASTING_OFF;
	if (cast_shadows != item->last_shadow_casting) {
#if !defined(SPINE_GODOT_EXTENSION) && VERSION_MINOR >= 7
		RSE::ShadowCastingSetting setting = cast_shadows == SHADOW_CASTING_ON ? RSE::SHADOW_CASTING_SETTING_ON :
			(cast_shadows == SHADOW_CASTING_DOUBLE_SIDED ? RSE::SHADOW_CASTING_SETTING_DOUBLE_SIDED :
				(cast_shadows == SHADOW_CASTING_SHADOWS_ONLY ? RSE::SHADOW_CASTING_SETTING_SHADOWS_ONLY : RSE::SHADOW_CASTING_SETTING_OFF));
#else
		RS::ShadowCastingSetting setting = cast_shadows == SHADOW_CASTING_ON ? RS::SHADOW_CASTING_SETTING_ON :
			(cast_shadows == SHADOW_CASTING_DOUBLE_SIDED ? RS::SHADOW_CASTING_SETTING_DOUBLE_SIDED :
				(cast_shadows == SHADOW_CASTING_SHADOWS_ONLY ? RS::SHADOW_CASTING_SETTING_SHADOWS_ONLY : RS::SHADOW_CASTING_SETTING_OFF));
#endif
		RS::get_singleton()->instance_geometry_set_cast_shadows_setting(item->instance, setting);
		item->last_shadow_casting = cast_shadows;
	}
	ERR_FAIL_COND_MSG(item->vertices.size() > INT_MAX || item->indices.size() > INT_MAX, "Batch exceeds Godot's mesh buffer index range.");
	int vertex_count = (int) item->vertices.size();
	int index_count = (int) item->indices.size();
	bool shadow_data_changed = false;
	if (!item->mesh.is_valid() || vertex_count > item->vertex_capacity || index_count > item->index_capacity) {
		int vertex_capacity = grow_capacity(item->vertex_capacity, vertex_count);
		int index_capacity = grow_capacity(item->index_capacity, index_count);
		item->clear_mesh();
		item->vertex_capacity = vertex_capacity;
		item->index_capacity = index_capacity;
		item->mesh = RS::get_singleton()->mesh_create();
		mesh_builds++;
#ifdef TOOLS_ENABLED
		editor_geometry_dirty = true;
#endif
#ifdef SPINE_GODOT_EXTENSION
		PackedVector3Array vertices;
		PackedVector2Array uvs;
		PackedInt32Array indices;
#else
		Vector<Vector3> vertices;
		Vector<Vector2> uvs;
		Vector<int> indices;
#endif
		PackedVector3Array normals;
		PackedFloat32Array tangents;
		PackedFloat32Array light;
		PackedFloat32Array dark;
		vertices.resize(vertex_capacity);
		normals.resize(vertex_capacity);
		tangents.resize(vertex_capacity * 4);
		uvs.resize(vertex_capacity);
		light.resize(vertex_capacity * 4);
		dark.resize(vertex_capacity * 3);
		indices.resize(index_capacity);
		memset(indices.ptrw(), 0, index_capacity * sizeof(int32_t));
		memset(light.ptrw(), 0, vertex_capacity * 4 * sizeof(float));
		memset(dark.ptrw(), 0, vertex_capacity * 3 * sizeof(float));
		for (int i = 0; i < vertex_capacity; i++) {
			normals.set(i, Vector3(0, 0, 1));
			tangents.set(i * 4, 1);
			tangents.set(i * 4 + 1, 0);
			tangents.set(i * 4 + 2, 0);
			tangents.set(i * 4 + 3, 1);
		}
		for (int i = 0; i < vertex_count; i++) {
			const SpineVertex3D &v = item->vertices[i];
			vertices.set(i, Vector3(v.position[0], v.position[1], v.position[2]));
			uvs.set(i, Vector2(v.uv[0], v.uv[1]));
			memcpy(light.ptrw() + i * 4, v.light, sizeof(v.light));
			memcpy(dark.ptrw() + i * 3, v.dark, sizeof(v.dark));
			memcpy(tangents.ptrw() + i * 4, v.tangent, sizeof(v.tangent));
		}
		for (int i = 0; i < index_count; i++) indices.set(i, item->indices[i]);
		Array arrays;
		arrays.resize(Mesh::ARRAY_MAX);
		arrays[Mesh::ARRAY_VERTEX] = vertices;
		arrays[Mesh::ARRAY_NORMAL] = normals;
		arrays[Mesh::ARRAY_TANGENT] = tangents;
		arrays[Mesh::ARRAY_TEX_UV] = uvs;
		arrays[Mesh::ARRAY_CUSTOM0] = light;
		arrays[Mesh::ARRAY_CUSTOM1] = dark;
		arrays[Mesh::ARRAY_INDEX] = indices;
		uint64_t flags = Mesh::ARRAY_FLAG_USE_DYNAMIC_UPDATE | get_custom_array_flags();
#ifdef SPINE_GODOT_EXTENSION
		RS::get_singleton()->mesh_add_surface_from_arrays(item->mesh, RS::PRIMITIVE_TRIANGLES, arrays, Array(), Dictionary(), flags);
		Dictionary surface = RS::get_singleton()->mesh_get_surface(item->mesh, 0);
		RS::ArrayFormat format = (RS::ArrayFormat) static_cast<int64_t>(surface["format"]);
		for (int array : {RS::ARRAY_VERTEX, RS::ARRAY_NORMAL, RS::ARRAY_TANGENT, RS::ARRAY_TEX_UV, RS::ARRAY_CUSTOM0, RS::ARRAY_CUSTOM1})
			item->surface_offsets[array] = RS::get_singleton()->mesh_surface_get_format_offset(format, vertex_capacity, array);
		item->vertex_stride = RS::get_singleton()->mesh_surface_get_format_vertex_stride(format, vertex_capacity);
		item->normal_tangent_stride = RS::get_singleton()->mesh_surface_get_format_normal_tangent_stride(format, vertex_capacity);
		item->attribute_stride = RS::get_singleton()->mesh_surface_get_format_attribute_stride(format, vertex_capacity);
		item->index_stride = RS::get_singleton()->mesh_surface_get_format_index_stride(format, vertex_capacity);
		item->vertex_buffer = surface["vertex_data"];
		item->attribute_buffer = surface["attribute_data"];
		item->index_buffer = surface["index_data"];
#else
#if VERSION_MINOR >= 7
		RenderingServerTypes::SurfaceData surface;
		RS::get_singleton()->mesh_create_surface_data_from_arrays(&surface, RSE::PRIMITIVE_TRIANGLES, arrays, TypedArray<Array>(), Dictionary(),
																  flags);
#else
		RS::SurfaceData surface;
		RS::get_singleton()->mesh_create_surface_data_from_arrays(&surface, RS::PRIMITIVE_TRIANGLES, arrays, TypedArray<Array>(), Dictionary(),
																  flags);
#endif
		RS::get_singleton()->mesh_add_surface(item->mesh, surface);
		uint32_t skin_stride;
		RS::get_singleton()->mesh_surface_make_offsets_from_format(surface.format, vertex_capacity, index_capacity, item->surface_offsets,
																   item->vertex_stride, item->normal_tangent_stride, item->attribute_stride, skin_stride);
		item->index_stride = RS::get_singleton()->mesh_surface_get_format_index_stride(surface.format, vertex_capacity);
		item->vertex_buffer = surface.vertex_data;
		item->attribute_buffer = surface.attribute_data;
		item->index_buffer = surface.index_data;
#endif
		item->uploaded_indices = item->indices;
		RS::get_singleton()->instance_set_base(item->instance, item->mesh);
		if (item->shadow_material.is_valid()) item->ensure_shadow_instance();
	} else {
		bool positions_changed = false;
		bool tangents_changed = false;
		bool vertex_data_changed = false;
		bool attributes_changed = false;
		uint8_t *positions = item->vertex_buffer.ptrw();
		uint8_t *attributes = item->attribute_buffer.ptrw();
		for (int i = 0; i < vertex_count; i++) {
			const SpineVertex3D &v = item->vertices[i];
			bool position_changed = update_bytes(positions + i * item->vertex_stride + item->surface_offsets[Mesh::ARRAY_VERTEX], v.position,
												 sizeof(v.position));
			positions_changed |= position_changed;
			vertex_data_changed |= position_changed;
			if (item->uses_tangents) {
				bool tangent_changed = update_tangent_bytes(positions + item->surface_offsets[Mesh::ARRAY_TANGENT] +
																 i * item->normal_tangent_stride,
															 v.tangent);
				tangents_changed |= tangent_changed;
				vertex_data_changed |= tangent_changed;
			}
			uint8_t *a = attributes + i * item->attribute_stride;
			attributes_changed |= update_bytes(a + item->surface_offsets[Mesh::ARRAY_TEX_UV], v.uv, sizeof(v.uv));
			attributes_changed |= update_bytes(a + item->surface_offsets[Mesh::ARRAY_CUSTOM0], v.light, sizeof(v.light));
			attributes_changed |= update_bytes(a + item->surface_offsets[Mesh::ARRAY_CUSTOM1], v.dark, sizeof(v.dark));
		}
#ifdef TOOLS_ENABLED
		if (positions_changed) editor_geometry_dirty = true;
#endif
		if (vertex_data_changed) {
			RS::get_singleton()->mesh_surface_update_vertex_region(item->mesh, 0, 0, item->vertex_buffer);
			vertex_uploads++;
			if (tangents_changed) tangent_uploads++;
		}
		if (attributes_changed) {
			RS::get_singleton()->mesh_surface_update_attribute_region(item->mesh, 0, 0, item->attribute_buffer);
			attribute_uploads++;
		}
		bool indices_changed = item->indices != item->uploaded_indices;
		if (indices_changed) {
#ifdef TOOLS_ENABLED
			editor_geometry_dirty = true;
#endif
			uint8_t *buffer = item->index_buffer.ptrw();
			int count = MAX(index_count, (int) item->uploaded_indices.size());
			for (int i = 0; i < count; i++) {
				uint32_t index = i < index_count ? item->indices[i] : 0;
				if (item->index_stride == 2) {
					uint16_t small_index = index;
					memcpy(buffer + i * 2, &small_index, 2);
				} else
					memcpy(buffer + i * 4, &index, 4);
			}
			RS::get_singleton()->mesh_surface_update_index_region(item->mesh, 0, 0, item->index_buffer);
			item->uploaded_indices = item->indices;
			index_uploads++;
		}
		shadow_data_changed = vertex_data_changed || attributes_changed || indices_changed;
	}
	AABB bounds = item->bounds;
	if (camera_relative_depth) {
		float depth = MAX(Math::abs(bounds.position.z), Math::abs(bounds.get_end().z));
		bounds.position.z = -depth;
		bounds.size.z = depth * 2;
	}
	if (bounds != item->uploaded_bounds) {
		RS::get_singleton()->mesh_set_custom_aabb(item->mesh, bounds);
		item->uploaded_bounds = bounds;
	}
	RID material = item->material->get_rid();
	if (material != item->last_material) {
		RS::get_singleton()->mesh_surface_set_material(item->mesh, 0, material);
		item->last_material = material;
	}
	RID shadow_material = item->shadow_material.is_valid() ? item->shadow_material->get_rid() : RID();
	if (shadow_material.is_valid()) item->ensure_shadow_instance();
	if (item->shadow_instance.is_valid() && item->last_shadow_mesh != item->mesh) {
		RS::get_singleton()->instance_set_base(item->shadow_instance, item->mesh);
		item->last_shadow_mesh = item->mesh;
	}
	if (item->shadow_instance.is_valid() && shadow_material != item->last_shadow_material) {
		RS::get_singleton()->instance_geometry_set_material_override(item->shadow_instance, shadow_material);
		item->last_shadow_material = shadow_material;
	}
	if (shadow_data_changed && shadow_casting != SHADOW_CASTING_OFF) {
		if (item->custom_material) {
			// Compatibility may retain a positional light's cached shadow after raw
			// mesh-buffer updates. Pulse the existing authored material as an instance
			// override, then restore surface-material ownership without rewriting it.
			RS::get_singleton()->instance_geometry_set_material_override(item->instance, item->material->get_rid());
			RS::get_singleton()->instance_geometry_set_material_override(item->instance, RID());
		} else if (item->shadow_instance.is_valid() && shadow_material.is_valid()) {
			RS::get_singleton()->instance_geometry_set_material_override(item->shadow_instance, RID());
			RS::get_singleton()->instance_geometry_set_material_override(item->shadow_instance, shadow_material);
		}
	}
}

void SpineSprite3D::_bind_methods() {
	ClassDB::bind_method(D_METHOD("set_skeleton_data_res", "skeleton_data_res"), &SpineSprite3D::set_skeleton_data_res);
	ClassDB::bind_method(D_METHOD("get_skeleton_data_res"), &SpineSprite3D::get_skeleton_data_res);
	ClassDB::bind_method(D_METHOD("get_skeleton"), &SpineSprite3D::get_skeleton);
	ClassDB::bind_method(D_METHOD("get_animation_state"), &SpineSprite3D::get_animation_state);
	ClassDB::bind_method(D_METHOD("on_skeleton_data_changed"), &SpineSprite3D::on_skeleton_data_changed);
	ClassDB::bind_method(D_METHOD("update_skeleton", "delta"), &SpineSprite3D::update_skeleton);
	ClassDB::bind_method(D_METHOD("set_update_mode", "value"), &SpineSprite3D::set_update_mode);
	ClassDB::bind_method(D_METHOD("get_update_mode"), &SpineSprite3D::get_update_mode);
	ClassDB::bind_method(D_METHOD("new_skin", "name"), &SpineSprite3D::new_skin);
	ClassDB::bind_method(D_METHOD("set_time_scale", "value"), &SpineSprite3D::set_time_scale);
	ClassDB::bind_method(D_METHOD("get_time_scale"), &SpineSprite3D::get_time_scale);
	ClassDB::bind_method(D_METHOD("set_pixel_size", "value"), &SpineSprite3D::set_pixel_size);
	ClassDB::bind_method(D_METHOD("get_pixel_size"), &SpineSprite3D::get_pixel_size);
	ClassDB::bind_method(D_METHOD("set_slot_depth_offset", "value"), &SpineSprite3D::set_slot_depth_offset);
	ClassDB::bind_method(D_METHOD("get_slot_depth_offset"), &SpineSprite3D::get_slot_depth_offset);
	ClassDB::bind_method(D_METHOD("set_camera_relative_depth", "value"), &SpineSprite3D::set_camera_relative_depth);
	ClassDB::bind_method(D_METHOD("get_camera_relative_depth"), &SpineSprite3D::get_camera_relative_depth);
	ClassDB::bind_method(D_METHOD("set_depth_write_enabled", "value"), &SpineSprite3D::set_depth_write_enabled);
	ClassDB::bind_method(D_METHOD("get_depth_write_enabled"), &SpineSprite3D::get_depth_write_enabled);
	ClassDB::bind_method(D_METHOD("set_alpha_cutoff", "value"), &SpineSprite3D::set_alpha_cutoff);
	ClassDB::bind_method(D_METHOD("get_alpha_cutoff"), &SpineSprite3D::get_alpha_cutoff);
	ClassDB::bind_method(D_METHOD("get_aabb"), &SpineSprite3D::get_aabb);
	ClassDB::bind_static_method("SpineSprite3D", D_METHOD("get_depth_shader_code"), &SpineSprite3D::get_depth_shader_code);
	ClassDB::bind_method(D_METHOD("set_sorting_step", "value"), &SpineSprite3D::set_sorting_step);
	ClassDB::bind_method(D_METHOD("get_sorting_step"), &SpineSprite3D::get_sorting_step);
	ClassDB::bind_method(D_METHOD("get_render_statistics"), &SpineSprite3D::get_render_statistics);
	ClassDB::bind_method(D_METHOD("set_render_priority", "value"), &SpineSprite3D::set_render_priority);
	ClassDB::bind_method(D_METHOD("get_render_priority"), &SpineSprite3D::get_render_priority);
	ClassDB::bind_method(D_METHOD("set_cull_mode", "value"), &SpineSprite3D::set_cull_mode);
	ClassDB::bind_method(D_METHOD("get_cull_mode"), &SpineSprite3D::get_cull_mode);
	ClassDB::bind_method(D_METHOD("set_lighting_enabled", "value"), &SpineSprite3D::set_lighting_enabled);
	ClassDB::bind_method(D_METHOD("get_lighting_enabled"), &SpineSprite3D::get_lighting_enabled);
	ClassDB::bind_method(D_METHOD("set_normal_map_enabled", "value"), &SpineSprite3D::set_normal_map_enabled);
	ClassDB::bind_method(D_METHOD("get_normal_map_enabled"), &SpineSprite3D::get_normal_map_enabled);
	ClassDB::bind_method(D_METHOD("set_normal_map_flip_y", "value"), &SpineSprite3D::set_normal_map_flip_y);
	ClassDB::bind_method(D_METHOD("get_normal_map_flip_y"), &SpineSprite3D::get_normal_map_flip_y);
	ClassDB::bind_method(D_METHOD("set_normal_scale", "value"), &SpineSprite3D::set_normal_scale);
	ClassDB::bind_method(D_METHOD("get_normal_scale"), &SpineSprite3D::get_normal_scale);
	ClassDB::bind_method(D_METHOD("set_specular", "value"), &SpineSprite3D::set_specular);
	ClassDB::bind_method(D_METHOD("get_specular"), &SpineSprite3D::get_specular);
	ClassDB::bind_method(D_METHOD("set_roughness", "value"), &SpineSprite3D::set_roughness);
	ClassDB::bind_method(D_METHOD("get_roughness"), &SpineSprite3D::get_roughness);
	ClassDB::bind_method(D_METHOD("set_metallic", "value"), &SpineSprite3D::set_metallic);
	ClassDB::bind_method(D_METHOD("get_metallic"), &SpineSprite3D::get_metallic);
	ClassDB::bind_method(D_METHOD("set_shadow_casting", "value"), &SpineSprite3D::set_shadow_casting);
	ClassDB::bind_method(D_METHOD("get_shadow_casting"), &SpineSprite3D::get_shadow_casting);
	ClassDB::bind_method(D_METHOD("set_shadow_alpha_cutoff", "value"), &SpineSprite3D::set_shadow_alpha_cutoff);
	ClassDB::bind_method(D_METHOD("get_shadow_alpha_cutoff"), &SpineSprite3D::get_shadow_alpha_cutoff);
	ClassDB::bind_method(D_METHOD("set_normal_material", "material"), &SpineSprite3D::set_normal_material);
	ClassDB::bind_method(D_METHOD("get_normal_material"), &SpineSprite3D::get_normal_material);
	ClassDB::bind_method(D_METHOD("set_additive_material", "material"), &SpineSprite3D::set_additive_material);
	ClassDB::bind_method(D_METHOD("get_additive_material"), &SpineSprite3D::get_additive_material);

	ADD_SIGNAL(MethodInfo("animation_started", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite3D"),
						  PropertyInfo(Variant::OBJECT, "animation_state", PROPERTY_HINT_TYPE_STRING, "SpineAnimationState"),
						  PropertyInfo(Variant::OBJECT, "track_entry", PROPERTY_HINT_TYPE_STRING, "SpineTrackEntry")));
	ADD_SIGNAL(MethodInfo("animation_interrupted", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite3D"),
						  PropertyInfo(Variant::OBJECT, "animation_state", PROPERTY_HINT_TYPE_STRING, "SpineAnimationState"),
						  PropertyInfo(Variant::OBJECT, "track_entry", PROPERTY_HINT_TYPE_STRING, "SpineTrackEntry")));
	ADD_SIGNAL(MethodInfo("animation_ended", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite3D"),
						  PropertyInfo(Variant::OBJECT, "animation_state", PROPERTY_HINT_TYPE_STRING, "SpineAnimationState"),
						  PropertyInfo(Variant::OBJECT, "track_entry", PROPERTY_HINT_TYPE_STRING, "SpineTrackEntry")));
	ADD_SIGNAL(MethodInfo("animation_completed", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite3D"),
						  PropertyInfo(Variant::OBJECT, "animation_state", PROPERTY_HINT_TYPE_STRING, "SpineAnimationState"),
						  PropertyInfo(Variant::OBJECT, "track_entry", PROPERTY_HINT_TYPE_STRING, "SpineTrackEntry")));
	ADD_SIGNAL(MethodInfo("animation_disposed", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite3D"),
						  PropertyInfo(Variant::OBJECT, "animation_state", PROPERTY_HINT_TYPE_STRING, "SpineAnimationState"),
						  PropertyInfo(Variant::OBJECT, "track_entry", PROPERTY_HINT_TYPE_STRING, "SpineTrackEntry")));
	ADD_SIGNAL(MethodInfo("animation_event", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite3D"),
						  PropertyInfo(Variant::OBJECT, "animation_state", PROPERTY_HINT_TYPE_STRING, "SpineAnimationState"),
						  PropertyInfo(Variant::OBJECT, "track_entry", PROPERTY_HINT_TYPE_STRING, "SpineTrackEntry"),
						  PropertyInfo(Variant::OBJECT, "event", PROPERTY_HINT_TYPE_STRING, "SpineEvent")));
	ADD_SIGNAL(
		MethodInfo("before_animation_state_update", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite3D")));
	ADD_SIGNAL(MethodInfo("before_animation_state_apply", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite3D")));
	ADD_SIGNAL(
		MethodInfo("before_world_transforms_change", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite3D")));
	ADD_SIGNAL(MethodInfo("world_transforms_changed", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite3D")));
	ADD_SIGNAL(MethodInfo("_internal_spine_objects_invalidated"));

	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "skeleton_data_res", PROPERTY_HINT_RESOURCE_TYPE, "SpineSkeletonDataResource"),
				 "set_skeleton_data_res", "get_skeleton_data_res");
	ADD_PROPERTY(PropertyInfo(Variant::INT, "update_mode", PROPERTY_HINT_ENUM, "Process,Physics,Manual"), "set_update_mode", "get_update_mode");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "time_scale"), "set_time_scale", "get_time_scale");
	ADD_GROUP("Rendering", "");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "pixel_size", PROPERTY_HINT_RANGE, "0.000001,10.0,0.0001,or_greater"), "set_pixel_size",
				 "get_pixel_size");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "slot_depth_offset", PROPERTY_HINT_RANGE, "0.0,1.0,0.00001,or_greater"), "set_slot_depth_offset",
				 "get_slot_depth_offset");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "camera_relative_depth"), "set_camera_relative_depth", "get_camera_relative_depth");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "depth_write_enabled"), "set_depth_write_enabled", "get_depth_write_enabled");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "alpha_cutoff", PROPERTY_HINT_RANGE, "0,1,0.001"), "set_alpha_cutoff", "get_alpha_cutoff");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "sorting_step", PROPERTY_HINT_RANGE, "0.001,10,0.01,or_greater"), "set_sorting_step",
				 "get_sorting_step");
	ADD_PROPERTY(PropertyInfo(Variant::INT, "render_priority", PROPERTY_HINT_RANGE, "-128,127,1"), "set_render_priority", "get_render_priority");
	ADD_PROPERTY(PropertyInfo(Variant::INT, "cull_mode", PROPERTY_HINT_ENUM, "Disabled,Back,Front"), "set_cull_mode", "get_cull_mode");
	ADD_GROUP("Lighting and Shadows", "");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "lighting_enabled"), "set_lighting_enabled", "get_lighting_enabled");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "normal_map_enabled"), "set_normal_map_enabled", "get_normal_map_enabled");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "normal_map_flip_y"), "set_normal_map_flip_y", "get_normal_map_flip_y");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "normal_scale", PROPERTY_HINT_RANGE, "0,16,0.01,or_greater"), "set_normal_scale", "get_normal_scale");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "specular", PROPERTY_HINT_RANGE, "0,1,0.01"), "set_specular", "get_specular");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "roughness", PROPERTY_HINT_RANGE, "0,1,0.01"), "set_roughness", "get_roughness");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "metallic", PROPERTY_HINT_RANGE, "0,1,0.01"), "set_metallic", "get_metallic");
	ADD_PROPERTY(PropertyInfo(Variant::INT, "shadow_casting", PROPERTY_HINT_ENUM, "Off,On,Double-Sided,Shadows Only"), "set_shadow_casting",
				 "get_shadow_casting");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "shadow_alpha_cutoff", PROPERTY_HINT_RANGE, "0,1,0.01"), "set_shadow_alpha_cutoff",
				 "get_shadow_alpha_cutoff");
	ADD_GROUP("Materials", "");
	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "normal_material", PROPERTY_HINT_RESOURCE_TYPE, "ShaderMaterial"), "set_normal_material",
				 "get_normal_material");
	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "additive_material", PROPERTY_HINT_RESOURCE_TYPE, "ShaderMaterial"), "set_additive_material",
				 "get_additive_material");
	ADD_GROUP("Preview", "");

	BIND_ENUM_CONSTANT(CULL_DISABLED);
	BIND_ENUM_CONSTANT(CULL_BACK);
	BIND_ENUM_CONSTANT(CULL_FRONT);
	BIND_ENUM_CONSTANT(SHADOW_CASTING_OFF);
	BIND_ENUM_CONSTANT(SHADOW_CASTING_ON);
	BIND_ENUM_CONSTANT(SHADOW_CASTING_DOUBLE_SIDED);
	BIND_ENUM_CONSTANT(SHADOW_CASTING_SHADOWS_ONLY);
}

SpineSprite3D::SpineSprite3D()
	: update_mode(SpineConstant::UpdateMode_Process), time_scale(1.0f), pixel_size(0.01f), slot_depth_offset(0.001f), camera_relative_depth(true),
	  depth_write_enabled(true), alpha_cutoff(0.001f), sorting_step(1.0f),
	  render_priority(0), cull_mode(CULL_DISABLED), lighting_enabled(false), normal_map_enabled(true), normal_map_flip_y(true), normal_scale(1.0f),
	  specular(0.25f), roughness(0.65f), metallic(0.0f), shadow_casting(SHADOW_CASTING_OFF), shadow_alpha_cutoff(0.3f),
	  preview_skin("Default"), preview_animation("-- Empty --"), preview_frame(false), preview_time(0),
	  skeleton_clipper(new spine::SkeletonClipping()), slots_dirty(true), updating_meshes(false), warned_multiply(false), warned_screen(false),
	  active_batches(0), active_vertices(0), active_indices(0), mesh_builds(0), index_uploads(0), vertex_uploads(0), tangent_uploads(0), attribute_uploads(0),
	  material_builds(0) {
	controller = Ref<SpineController>(memnew(SpineController));
	controller->set_listener(this);
	quad_indices.setSize(6, 0);
	quad_indices[0] = 0;
	quad_indices[1] = 1;
	quad_indices[2] = 2;
	quad_indices[3] = 2;
	quad_indices[4] = 3;
	quad_indices[5] = 0;
	scratch_vertices.ensureCapacity(1200);
	set_notify_transform(true);
}

SpineSprite3D::~SpineSprite3D() {
	remove_render_items();
	controller->set_listener(nullptr);
	controller.unref();
	delete skeleton_clipper;
}

void SpineSprite3D::set_skeleton_data_res(const Ref<SpineSkeletonDataResource> &skeleton_data_res) {
	if (connected_skeleton_data_res.is_valid() &&
		connected_skeleton_data_res->is_connected(SNAME("skeleton_data_changed"), callable_mp(this, &SpineSprite3D::on_skeleton_data_changed))) {
		connected_skeleton_data_res->disconnect(SNAME("skeleton_data_changed"), callable_mp(this, &SpineSprite3D::on_skeleton_data_changed));
	}
	connected_skeleton_data_res = skeleton_data_res;
	if (connected_skeleton_data_res.is_valid()) {
		connected_skeleton_data_res->connect(SNAME("skeleton_data_changed"), callable_mp(this, &SpineSprite3D::on_skeleton_data_changed));
	}
	controller->set_skeleton_data_res(skeleton_data_res);
}

Ref<SpineSkeletonDataResource> SpineSprite3D::get_skeleton_data_res() {
	return controller->get_skeleton_data_res();
}

Ref<SpineSkeleton> SpineSprite3D::get_skeleton() {
	return controller->get_skeleton();
}

Ref<SpineAnimationState> SpineSprite3D::get_animation_state() {
	return controller->get_animation_state();
}

void SpineSprite3D::on_skeleton_data_changed() {
	controller->set_skeleton_data_res(connected_skeleton_data_res);
}

void SpineSprite3D::before_skeleton_data_change() {
	remove_render_items();
	emit_signal(SNAME("_internal_spine_objects_invalidated"));
}

void SpineSprite3D::skeleton_data_changed() {
	Ref<SpineSkeleton> skeleton = controller->get_skeleton();
	if (skeleton.is_valid()) {
		slots_dirty = true;
		if (update_mode == SpineConstant::UpdateMode_Process) {
			_notification(NOTIFICATION_INTERNAL_PROCESS);
		} else if (update_mode == SpineConstant::UpdateMode_Physics) {
			_notification(NOTIFICATION_INTERNAL_PHYSICS_PROCESS);
		}
	}
	NOTIFY_PROPERTY_LIST_CHANGED();
}

bool SpineSprite3D::should_apply_pose() {
	return is_visible_in_tree();
}

void SpineSprite3D::rebuild_slot_nodes() {
	for (auto &nodes : slot_nodes) {
		for (ObjectID id : nodes) {
			auto helper = Object::cast_to<SpineSlotNode3D>(ObjectDB::get_instance(id));
			if (helper) helper->release_sorting();
		}
		nodes.clear();
	}
	auto skeleton = controller->get_skeleton()->get_spine_object();
	slot_nodes.clear();
	for (int i = 0; i < get_child_count(); i++) {
		auto helper = Object::cast_to<SpineSlotNode3D>(get_child(i));
		if (!helper) continue;
		auto slot = skeleton->findSlot(SPINE_STRING(helper->get_slot_name()));
		helper->slot_index = slot ? slot->getData().getIndex() : -1;
		if (slot) {
			if (slot_nodes.empty()) slot_nodes.resize(skeleton->getSlots().size());
			slot_nodes[helper->slot_index].push_back(ObjectID(helper->get_instance_id()));
		}
	}
	slots_dirty = false;
}

void SpineSprite3D::remove_render_items() {
	for (auto &nodes : slot_nodes)
		for (ObjectID id : nodes) {
			auto helper = Object::cast_to<SpineSlotNode3D>(ObjectDB::get_instance(id));
			if (helper) {
				helper->release_sorting();
				helper->slot_index = -1;
			}
		}
	slot_nodes.clear();
	for (auto batch : batches) delete batch;
	batches.clear();
	draw_items.clear();
	materials.clear();
	shadow_materials.clear();
	active_batches = active_vertices = active_indices = 0;
	slots_dirty = true;
	warned_multiply = false;
	warned_screen = false;
#ifdef TOOLS_ENABLED
	editor_geometry_dirty = true;
	update_editor_selection();
#endif
}

#ifdef TOOLS_ENABLED
void SpineSprite3D::update_editor_selection() {
	if (!editor_geometry_dirty || !Engine::get_singleton()->is_editor_hint()) return;
	editor_selection_mesh.unref();
	editor_geometry_dirty = false;
	if (is_inside_tree()) update_gizmos();
}

Ref<TriangleMesh> SpineSprite3D::get_editor_selection_mesh() {
	if (editor_selection_mesh.is_valid() || active_indices == 0) return editor_selection_mesh;
	// Use only the current clipped/deformed geometry, never pooled buffer padding
	// or inserted children. Godot uses these faces for click and box selection.
	PackedVector3Array faces;
	faces.resize(active_indices * (camera_relative_depth ? 2 : 1));
	Vector3 *out = faces.ptrw();
	for (int i = 0; i < active_batches; i++) {
		const auto batch = batches[i];
		for (uint32_t index : batch->indices) {
			const auto &v = batch->vertices[index];
			*out++ = Vector3(v.position[0], v.position[1], v.position[2]);
		}
	}
	// Picking is conservative: either possible side may be visible in an editor view.
	if (camera_relative_depth)
		for (int i = 0; i < active_indices; i++) {
			Vector3 position = faces[i];
			position.z = -position.z;
			*out++ = position;
		}
	editor_selection_mesh = Ref<TriangleMesh>(memnew(TriangleMesh));
	editor_selection_mesh->create_from_faces(faces);
	return editor_selection_mesh;
}
#endif

SpineBatch3D *SpineSprite3D::begin_batch(const Ref<ShaderMaterial> &material, const Ref<ShaderMaterial> &shadow_material, bool custom_material) {
	if (active_batches == (int) batches.size()) batches.push_back(new SpineBatch3D());
	auto batch = batches[active_batches++];
	batch->vertices.clear();
	batch->indices.clear();
	batch->material = material;
	batch->shadow_material = shadow_material;
	batch->custom_material = custom_material;
	batch->uses_tangents = false;
	batch->bounds = AABB();
	draw_items.push_back({batch, ObjectID(), Vector3(), 0, ObjectID(), 0});
	return batch;
}

String SpineSprite3D::get_depth_shader_code() {
	return R"(
// These uniforms are supplied by SpineSprite3D, including custom shader passes.
uniform bool spine_camera_relative_depth = true;
uniform float spine_alpha_cutoff = 0.001;
vec3 spine_apply_camera_depth(vec3 position, vec3 origin, vec3 plane_normal, vec3 camera_position) {
	if (spine_camera_relative_depth && dot(camera_position - origin, plane_normal) < 0.0) position.z = -position.z;
	return position;
}
void spine_apply_alpha_cutoff(float alpha) {
	if (alpha <= 0.0 || alpha < spine_alpha_cutoff) discard;
}
)";
}

Ref<Shader> SpineSprite3D::get_generated_shader(spine::BlendMode blend_mode) {
	// Shader source depends only on blend/culling/depth writes/lighting, not on the character. Sharing
	// these resources avoids compiling the same GLSL program for every sprite.
	std::lock_guard<std::mutex> lock(generated_shader_mutex);
	int blend_index = blend_mode == spine::BlendMode_Additive ? 1 : 0;
	int lighting_index = lighting_enabled ? 1 : 0;
	Ref<Shader> &shared = generated_shaders[blend_index][cull_mode][depth_write_enabled ? 1 : 0][lighting_index];
	if (shared.is_valid()) return shared;
	const char *blend = blend_index == 1 ? "blend_add" : "blend_mix";
	const char *cull = cull_mode == CULL_BACK ? "cull_back" : (cull_mode == CULL_FRONT ? "cull_front" : "cull_disabled");
	const char *depth = depth_write_enabled ? "depth_draw_always" : "depth_draw_never";
	String lighting_mode = lighting_enabled ? String() : String("unshaded, ");
	String lighting_declarations;
	String lighting_vertex;
	String lighting_fragment;
	if (lighting_enabled) {
		lighting_declarations = R"(
uniform sampler2D spine_normal_texture : hint_normal, repeat_disable;
uniform bool spine_has_normal_texture = false;
uniform bool spine_normal_map_enabled = true;
uniform bool spine_normal_map_flip_y = true;
uniform float spine_normal_scale = 1.0;
uniform float spine_specular = 0.25;
uniform float spine_roughness = 0.65;
uniform float spine_metallic = 0.0;
)";
		lighting_fragment = R"(
	if (dot(NORMAL, VIEW) < 0.0) NORMAL = -NORMAL;
	if (spine_normal_map_enabled && spine_has_normal_texture) {
		vec3 spine_mapped_normal = texture(spine_normal_texture, UV).rgb;
		if (spine_normal_map_flip_y) spine_mapped_normal.g = 1.0 - spine_mapped_normal.g;
		NORMAL_MAP = spine_mapped_normal;
		NORMAL_MAP_DEPTH = spine_normal_scale;
	}
	SPECULAR = spine_specular;
	ROUGHNESS = spine_roughness;
	METALLIC = spine_metallic;
)";
	}
	Ref<Shader> shader(memnew(Shader));
	shader->set_code(String("shader_type spatial;\nrender_mode ") + lighting_mode + depth + ", " + blend + ", " + cull + ";\n" +
		get_depth_shader_code() + R"(
// Read encoded atlas colors: PMA must be undone before sRGB decoding.
uniform sampler2D spine_texture : repeat_disable;
uniform bool spine_premultiplied_alpha = false;
varying vec4 spine_light_color;
varying vec3 spine_dark_color;
vec3 spine_srgb_to_linear(vec3 color) {
	return mix(pow((color + vec3(0.055)) / 1.055, vec3(2.4)), color / 12.92, lessThanEqual(color, vec3(0.04045)));
}
)" + lighting_declarations + R"(
void vertex() {
	VERTEX = spine_apply_camera_depth(VERTEX, MODEL_MATRIX[3].xyz, MODEL_NORMAL_MATRIX[2], INV_VIEW_MATRIX[3].xyz);
	spine_light_color = CUSTOM0;
	spine_dark_color = CUSTOM1.rgb;
	if (!OUTPUT_IS_SRGB) {
		spine_light_color.rgb = spine_srgb_to_linear(spine_light_color.rgb);
		spine_dark_color = spine_srgb_to_linear(spine_dark_color);
	}
)" + lighting_vertex + R"(
}
void fragment() {
	vec4 tex = texture(spine_texture, UV);
	if (spine_premultiplied_alpha) tex.rgb = tex.a > 0.000001 ? tex.rgb / tex.a : vec3(0.0);
	if (!OUTPUT_IS_SRGB) tex.rgb = spine_srgb_to_linear(tex.rgb);
	ALBEDO = tex.rgb * spine_light_color.rgb + (vec3(1.0) - tex.rgb) * spine_dark_color;
	ALPHA = tex.a * spine_light_color.a;
	spine_apply_alpha_cutoff(ALPHA);
)" + lighting_fragment + R"(
}
)");
	shared = shader;
	return shader;
}

Ref<Shader> SpineSprite3D::get_generated_shadow_shader(int shadow_cull) {
	std::lock_guard<std::mutex> lock(generated_shader_mutex);
	Ref<Shader> &shared = generated_shadow_shaders[shadow_cull];
	if (shared.is_valid()) return shared;
	const char *cull = shadow_cull == CULL_BACK ? "cull_back" : (shadow_cull == CULL_FRONT ? "cull_front" : "cull_disabled");
	Ref<Shader> shader(memnew(Shader));
	shader->set_code(String("shader_type spatial;\nrender_mode unshaded, depth_draw_opaque, ") + cull + R"(;
uniform sampler2D spine_texture : repeat_disable;
uniform float spine_shadow_alpha_cutoff = 0.3;
varying float spine_tint_alpha;
void vertex() {
	spine_tint_alpha = CUSTOM0.a;
}
void fragment() {
	ALBEDO = vec3(1.0);
	ALPHA = texture(spine_texture, UV).a * spine_tint_alpha;
	ALPHA_SCISSOR_THRESHOLD = max(spine_shadow_alpha_cutoff, 0.000001);
}
)");
	shared = shader;
	return shader;
}

Ref<ShaderMaterial> SpineSprite3D::get_material(SpineRendererObject *renderer_object, bool premultiplied_alpha, spine::BlendMode blend_mode,
												const Ref<ShaderMaterial> &override_material) {
	if (blend_mode == spine::BlendMode_Multiply) {
		if (!warned_multiply) {
			WARN_PRINT("SpineSprite3D does not yet support multiply blending; affected slots use normal blending.");
			warned_multiply = true;
		}
		blend_mode = spine::BlendMode_Normal;
	} else if (blend_mode == spine::BlendMode_Screen) {
		if (!warned_screen) {
			WARN_PRINT("SpineSprite3D does not yet support screen blending; affected slots use normal blending.");
			warned_screen = true;
		}
		blend_mode = spine::BlendMode_Normal;
	}

	RID texture_rid = renderer_object && renderer_object->texture.is_valid() ? renderer_object->texture->get_rid() : RID();
	RID normal_texture_rid = renderer_object && renderer_object->normal_map.is_valid() ? renderer_object->normal_map->get_rid() : RID();
	Ref<ShaderMaterial> source = override_material;
	if (source.is_null()) source = blend_mode == spine::BlendMode_Additive ? additive_material : normal_material;
	ObjectID source_id = source.is_valid() ? ObjectID(source->get_instance_id()) : ObjectID();
	bool generated_lighting = source.is_null() && lighting_enabled;
	int cull = source.is_valid() ? -1 : (int) cull_mode;
	bool depth_write = source.is_null() && depth_write_enabled;
	for (const auto &cached : materials) {
		if (cached.texture == texture_rid && cached.normal_texture == normal_texture_rid && cached.source == source_id && cached.blend == blend_mode &&
			cached.pma == premultiplied_alpha && cached.lighting == generated_lighting && cached.priority == render_priority && cached.cull == cull &&
			cached.depth_write == depth_write)
			return cached.material;
	}
	Ref<ShaderMaterial> material;
	if (source.is_valid()) {
		Ref<Resource> duplicate = source->duplicate();
		material = duplicate;
		Ref<Material> tail = material;
		Ref<Material> source_pass = source->get_next_pass();
		std::vector<Material *> visited = {source.ptr()};
		while (source_pass.is_valid()) {
			bool cycle = false;
			for (auto seen : visited)
				if (seen == source_pass.ptr()) cycle = true;
			if (cycle) {
				ERR_PRINT("Cyclic next_pass chain in a SpineSprite3D material.");
				tail->set_next_pass(Ref<Material>());
				break;
			}
			visited.push_back(source_pass.ptr());
			Ref<Material> pass = source_pass->duplicate();
			pass->set_render_priority(render_priority);
			Ref<ShaderMaterial> shader_pass = pass;
			if (shader_pass.is_valid()) {
				shader_pass->set_shader_parameter(SNAME("spine_texture"), renderer_object ? Variant(renderer_object->texture) : Variant());
				shader_pass->set_shader_parameter(SNAME("spine_normal_texture"), renderer_object ? Variant(renderer_object->normal_map) : Variant());
				shader_pass->set_shader_parameter(SNAME("spine_has_normal_texture"), renderer_object && renderer_object->normal_map.is_valid());
				shader_pass->set_shader_parameter(SNAME("spine_premultiplied_alpha"), premultiplied_alpha);
			}
			tail->set_next_pass(pass);
			tail = pass;
			source_pass = source_pass->get_next_pass();
			material_builds++;
		}
	} else {
		material = Ref<ShaderMaterial>(memnew(ShaderMaterial));
		material->set_shader(get_generated_shader(blend_mode));
	}
	material->set_render_priority(render_priority);
	if (renderer_object && renderer_object->texture.is_valid())
		material->set_shader_parameter(SNAME("spine_texture"), renderer_object->texture);
	else
		material->set_shader_parameter(SNAME("spine_texture"), Variant());
	material->set_shader_parameter(SNAME("spine_normal_texture"), renderer_object ? Variant(renderer_object->normal_map) : Variant());
	material->set_shader_parameter(SNAME("spine_has_normal_texture"), renderer_object && renderer_object->normal_map.is_valid());
	material->set_shader_parameter(SNAME("spine_premultiplied_alpha"), premultiplied_alpha);

	apply_runtime_parameters(material);
	materials.push_back({texture_rid, normal_texture_rid, source_id, material, blend_mode, premultiplied_alpha, generated_lighting, render_priority, cull,
						 depth_write});
	material_builds++;
	return material;
}

Ref<ShaderMaterial> SpineSprite3D::get_shadow_material(SpineRendererObject *renderer_object) {
	if (shadow_casting == SHADOW_CASTING_OFF || !renderer_object || !renderer_object->texture.is_valid()) return Ref<ShaderMaterial>();
	RID texture = renderer_object->texture->get_rid();
	int shadow_cull = shadow_casting == SHADOW_CASTING_DOUBLE_SIDED ? CULL_DISABLED : (int) cull_mode;
	for (const auto &cached : shadow_materials)
		if (cached.texture == texture && cached.cull == shadow_cull) return cached.material;
	Ref<ShaderMaterial> material(memnew(ShaderMaterial));
	material->set_shader(get_generated_shadow_shader(shadow_cull));
	material->set_shader_parameter(SNAME("spine_texture"), renderer_object->texture);
	material->set_shader_parameter(SNAME("spine_shadow_alpha_cutoff"), shadow_alpha_cutoff);
	shadow_materials.push_back({texture, material, shadow_cull});
	material_builds++;
	return material;
}

static void build_spine_vertex_tangents(SpineBatch3D *batch, uint32_t base_vertex, int vertex_count, spine::Array<unsigned short> &indices,
										std::vector<Vector3> &tangent_accum, std::vector<Vector3> &bitangent_accum) {
	tangent_accum.assign(vertex_count, Vector3());
	bitangent_accum.assign(vertex_count, Vector3());
	for (size_t i = 0; i + 2 < indices.size(); i += 3) {
		int i0 = indices[i], i1 = indices[i + 1], i2 = indices[i + 2];
		if (i0 < 0 || i1 < 0 || i2 < 0 || i0 >= vertex_count || i1 >= vertex_count || i2 >= vertex_count) continue;
		const SpineVertex3D &v0 = batch->vertices[base_vertex + i0];
		const SpineVertex3D &v1 = batch->vertices[base_vertex + i1];
		const SpineVertex3D &v2 = batch->vertices[base_vertex + i2];
		Vector3 p0(v0.position[0], v0.position[1], v0.position[2]);
		Vector3 p1(v1.position[0], v1.position[1], v1.position[2]);
		Vector3 p2(v2.position[0], v2.position[1], v2.position[2]);
		Vector2 uv0(v0.uv[0], v0.uv[1]);
		Vector2 uv1(v1.uv[0], v1.uv[1]);
		Vector2 uv2(v2.uv[0], v2.uv[1]);
		Vector3 e1 = p1 - p0, e2 = p2 - p0;
		Vector2 duv1 = uv1 - uv0, duv2 = uv2 - uv0;
		float denominator = duv1.x * duv2.y - duv2.x * duv1.y;
		if (Math::abs(denominator) <= 0.0000001f) continue;
		float inverse = 1.0f / denominator;
		Vector3 tangent = (e1 * duv2.y - e2 * duv1.y) * inverse;
		Vector3 bitangent = (e2 * duv1.x - e1 * duv2.x) * inverse;
		for (int index : {i0, i1, i2}) {
			tangent_accum[index] += tangent;
			bitangent_accum[index] += bitangent;
		}
	}
	const Vector3 normal(0, 0, 1);
	for (int i = 0; i < vertex_count; i++) {
		Vector3 tangent = tangent_accum[i];
		if (tangent.length_squared() <= CMP_EPSILON2)
			tangent = Vector3(1, 0, 0);
		else
			tangent = (tangent - normal * normal.dot(tangent)).normalized();
		Vector3 bitangent = bitangent_accum[i];
		float sign = bitangent.length_squared() <= CMP_EPSILON2 || normal.cross(tangent).dot(bitangent) >= 0 ? 1.0f : -1.0f;
		SpineVertex3D &vertex = batch->vertices[base_vertex + i];
		vertex.tangent[0] = tangent.x;
		vertex.tangent[1] = tangent.y;
		vertex.tangent[2] = tangent.z;
		vertex.tangent[3] = sign;
	}
}

void SpineSprite3D::update_render_items(Ref<SpineSkeleton> skeleton_ref) {
	if (updating_meshes) return;
	updating_meshes = true;
	if (slots_dirty) rebuild_slot_nodes();
	active_batches = active_vertices = active_indices = 0;
	draw_items.clear();
	SpineBatch3D *current = nullptr;
	spine::Skeleton *skeleton = skeleton_ref->get_spine_object();
	int slot_count = (int) skeleton->getSlots().size();
	bool has_helpers = !slot_nodes.empty();
	for (int i = 0; i <= slot_count; i++) {
		// Insert after the preceding slot, including empty and clipping slots.
		if (i > 0 && has_helpers) {
			auto previous = skeleton->getDrawOrder().getAppliedPose()[i - 1];
			for (ObjectID id : slot_nodes[previous->getData().getIndex()]) {
				auto helper = Object::cast_to<SpineSlotNode3D>(ObjectDB::get_instance(id));
				if (!helper) continue;
				helper->sync_slot(previous, pixel_size, (i - 1) * slot_depth_offset);
				if (controller->is_skeleton_data_reset_pending()) break;
				helper = Object::cast_to<SpineSlotNode3D>(ObjectDB::get_instance(id));
				if (!helper) continue;
				for (ObjectID geometry : helper->get_geometries()) {
					current = nullptr;
					draw_items.push_back({nullptr, geometry, Vector3(), 0, id, helper->geometry_generation});
				}
			}
		}
		if (i == slot_count || controller->is_skeleton_data_reset_pending()) break;
		spine::Slot *slot = skeleton->getDrawOrder().getAppliedPose()[i];
		spine::Attachment *attachment = slot->getAppliedPose().getAttachment();
		if (!attachment || !slot->getBone().isActive()) {
			skeleton_clipper->clipEnd(*slot);
			continue;
		}

		spine::Color skeleton_color = skeleton->getColor();
		spine::Color slot_color = slot->getAppliedPose().getColor();
		spine::Color slot_dark_color = slot->getAppliedPose().getDarkColor();
		Color light_tint(skeleton_color.r * slot_color.r, skeleton_color.g * slot_color.g, skeleton_color.b * slot_color.b,
						 skeleton_color.a * slot_color.a);
		Color dark_tint;
		if (slot->getAppliedPose().hasDarkColor()) {
			dark_tint = Color(skeleton_color.r * slot_dark_color.r, skeleton_color.g * slot_dark_color.g, skeleton_color.b * slot_dark_color.b, 1);
		}

		SpineRendererObject *renderer_object = nullptr;
		bool premultiplied_alpha = false;
		spine::Array<float> *vertices = &scratch_vertices;
		spine::Array<float> *uvs = nullptr;
		spine::Array<unsigned short> *indices = nullptr;

		if (attachment->getRTTI().isExactly(spine::RegionAttachment::rtti)) {
			auto region = (spine::RegionAttachment *) attachment;
			auto &sequence = region->getSequence();
			int sequence_index = sequence.resolveIndex(slot->getAppliedPose());
			vertices->setSize(8, 0);
			region->computeWorldVertices(*slot, sequence.getOffsets(sequence_index).buffer(), vertices->buffer(), 0);
			auto atlas_region = (spine::AtlasRegion *) sequence.getRegion(sequence_index);
			renderer_object = (SpineRendererObject *) atlas_region->getPage()->texture;
			premultiplied_alpha = atlas_region->getPage()->pma;
			uvs = &sequence.getUVs(sequence_index);
			indices = &quad_indices;
			auto &attachment_color = region->getColor();
			light_tint.r *= attachment_color.r;
			light_tint.g *= attachment_color.g;
			light_tint.b *= attachment_color.b;
			light_tint.a *= attachment_color.a;
			dark_tint.r *= attachment_color.r;
			dark_tint.g *= attachment_color.g;
			dark_tint.b *= attachment_color.b;
		} else if (attachment->getRTTI().isExactly(spine::MeshAttachment::rtti)) {
			auto mesh = (spine::MeshAttachment *) attachment;
			auto &sequence = mesh->getSequence();
			int sequence_index = sequence.resolveIndex(slot->getAppliedPose());
			vertices->setSize(mesh->getWorldVerticesLength(), 0);
			mesh->computeWorldVertices(*skeleton, *slot, 0, mesh->getWorldVerticesLength(), vertices->buffer(), 0, 2);
			auto atlas_region = (spine::AtlasRegion *) sequence.getRegion(sequence_index);
			renderer_object = (SpineRendererObject *) atlas_region->getPage()->texture;
			premultiplied_alpha = atlas_region->getPage()->pma;
			uvs = &sequence.getUVs(sequence_index);
			indices = &mesh->getTriangles();
			auto &attachment_color = mesh->getColor();
			light_tint.r *= attachment_color.r;
			light_tint.g *= attachment_color.g;
			light_tint.b *= attachment_color.b;
			light_tint.a *= attachment_color.a;
			dark_tint.r *= attachment_color.r;
			dark_tint.g *= attachment_color.g;
			dark_tint.b *= attachment_color.b;
		} else if (attachment->getRTTI().isExactly(spine::ClippingAttachment::rtti)) {
			skeleton_clipper->clipStart(*skeleton, *slot, (spine::ClippingAttachment *) attachment);
			continue;
		} else {
			skeleton_clipper->clipEnd(*slot);
			continue;
		}

		if (skeleton_clipper->isClipping()) {
			skeleton_clipper->clipTriangles(*vertices, *indices, *uvs, 2);
			if (skeleton_clipper->getClippedTriangles().size() == 0) {
				skeleton_clipper->clipEnd(*slot);
				continue;
			}
			vertices = &skeleton_clipper->getClippedVertices();
			uvs = &skeleton_clipper->getClippedUVs();
			indices = &skeleton_clipper->getClippedTriangles();
		}

		if (indices->size() > 0) {
			spine::BlendMode blend = slot->getData().getBlendMode();
			Ref<ShaderMaterial> override_material;
			if (has_helpers) {
				auto &helpers = slot_nodes[slot->getData().getIndex()];
				if (!helpers.empty()) {
					auto helper = Object::cast_to<SpineSlotNode3D>(ObjectDB::get_instance(helpers[0]));
					if (helper)
						override_material = blend == spine::BlendMode_Additive ? helper->get_additive_material() : helper->get_normal_material();
				}
			}
			Ref<ShaderMaterial> material = get_material(renderer_object, premultiplied_alpha, blend, override_material);
			bool custom_material = override_material.is_valid();
			if (!custom_material)
				custom_material = blend == spine::BlendMode_Additive ? additive_material.is_valid() : normal_material.is_valid();
			Ref<ShaderMaterial> shadow_material = custom_material ? Ref<ShaderMaterial>() : get_shadow_material(renderer_object);
			if (!current || current->material != material || current->shadow_material != shadow_material || current->custom_material != custom_material)
				current = begin_batch(material, shadow_material, custom_material);
			uint32_t base_vertex = (uint32_t) current->vertices.size();
			int vertex_count = (int) vertices->size() / 2;
			current->vertices.resize(base_vertex + vertex_count);
			float z = i * slot_depth_offset;
			for (int j = 0; j < vertex_count; j++) {
				auto &v = current->vertices[base_vertex + j];
				v = {{(*vertices)[j * 2] * pixel_size, -(*vertices)[j * 2 + 1] * pixel_size, z},
					 {(*uvs)[j * 2], (*uvs)[j * 2 + 1]},
					 {light_tint.r, light_tint.g, light_tint.b, light_tint.a},
					 {dark_tint.r, dark_tint.g, dark_tint.b},
					 {1, 0, 0, 1}};
				Vector3 position(v.position[0], v.position[1], v.position[2]);
				if (base_vertex + j == 0)
					current->bounds.position = position;
				else
					current->bounds.expand_to(position);
			}
			if (custom_material || (lighting_enabled && renderer_object && renderer_object->normal_map.is_valid())) {
				current->uses_tangents = true;
				build_spine_vertex_tangents(current, base_vertex, vertex_count, *indices, tangent_accum, bitangent_accum);
			}
			for (size_t j = 0; j < indices->size(); j++) current->indices.push_back(base_vertex + (*indices)[j]);
			active_vertices += vertex_count;
			active_indices += (int) indices->size();
		}
		skeleton_clipper->clipEnd(*slot);
	}
	skeleton_clipper->clipEnd();
	if (!controller->is_skeleton_data_reset_pending()) {
		for (int i = 0; i < (int) batches.size(); i++) {
#ifdef TOOLS_ENABLED
			if (batches[i]->active != (i < active_batches)) editor_geometry_dirty = true;
#endif
			batches[i]->active = i < active_batches;
			if (batches[i]->active) upload_batch(batches[i]);
		}
		update_render_item_world_state();
#ifdef TOOLS_ENABLED
		update_editor_selection();
#endif
	}
	updating_meshes = false;
}

void SpineSprite3D::refresh_render_items() {
	Ref<SpineSkeleton> skeleton = controller->get_skeleton();
	if (!skeleton.is_valid() || !skeleton->get_spine_object()) return;
	controller->begin_native_operation();
	update_render_items(skeleton);
	controller->end_native_operation();
}

void SpineSprite3D::update_render_item_world_state() {
	Transform3D transform;
	if (is_inside_tree()) transform = get_global_transform();
	bool node_visible = is_visible_in_tree();
	for (auto batch : batches) {
		if (batch->last_scenario != render_scenario) {
			RS::get_singleton()->instance_set_scenario(batch->instance, render_scenario);
			batch->last_scenario = render_scenario;
		}
		if (batch->last_transform != transform) {
			RS::get_singleton()->instance_set_transform(batch->instance, transform);
			batch->last_transform = transform;
		}
		if (batch->shadow_instance.is_valid() && batch->last_shadow_scenario != render_scenario) {
			RS::get_singleton()->instance_set_scenario(batch->shadow_instance, render_scenario);
			batch->last_shadow_scenario = render_scenario;
		}
		if (batch->shadow_instance.is_valid() && (!batch->shadow_transform_initialized || batch->last_shadow_transform != transform)) {
			RS::get_singleton()->instance_set_transform(batch->shadow_instance, transform);
			batch->last_shadow_transform = transform;
			batch->shadow_transform_initialized = true;
		}
		bool visible = node_visible && batch->active && (batch->custom_material || shadow_casting != SHADOW_CASTING_SHADOWS_ONLY);
		if (visible != batch->last_visible) {
			RS::get_singleton()->instance_set_visible(batch->instance, visible);
			batch->last_visible = visible;
		}
		bool shadow_visible = node_visible && batch->active && shadow_casting != SHADOW_CASTING_OFF && batch->shadow_material.is_valid();
		if (batch->shadow_instance.is_valid() && shadow_visible != batch->last_shadow_visible) {
			RS::get_singleton()->instance_set_visible(batch->shadow_instance, shadow_visible);
			batch->last_shadow_visible = shadow_visible;
		}
	}
	update_sorting();
}

void SpineSprite3D::update_sorting() {
	if (!is_inside_tree() || draw_items.empty()) return;
	Vector3 origin = get_global_transform().origin;
	Vector3 previous = origin;
	double offset = 0;
	bool first = true;
	for (auto &item : draw_items) {
		item.origin = origin;
		if (!item.batch) {
			auto helper = Object::cast_to<SpineSlotNode3D>(ObjectDB::get_instance(item.helper));
			if (helper && helper->get_parent() == this && helper->geometry_generation == item.generation) {
				auto geometry = Object::cast_to<GeometryInstance3D>(ObjectDB::get_instance(item.geometry));
				if (geometry && geometry->is_inside_tree()) item.origin = geometry->get_global_transform().origin;
			}
		}
		// Both radial camera distance and orthographic depth differences are bounded
		// by this distance, so inserted child origins cannot reverse draw order.
		if (!first) offset += previous.distance_to(item.origin) + sorting_step;
		first = false;
		item.offset = (float) offset;
		previous = item.origin;
	}
	float center = (float) (offset * 0.5);
	for (auto &item : draw_items) {
		float value = item.offset - center;
		if (item.batch) {
			if (!item.batch->sorting_initialized || item.batch->sorting_offset != value) {
				item.batch->sorting_offset = value;
				item.batch->sorting_initialized = true;
				RS::get_singleton()->instance_set_pivot_data(item.batch->instance, value, false);
			}
		} else {
			auto helper = Object::cast_to<SpineSlotNode3D>(ObjectDB::get_instance(item.helper));
			if (!helper || helper->get_parent() != this || helper->geometry_generation != item.generation) continue;
			auto geometry = Object::cast_to<GeometryInstance3D>(ObjectDB::get_instance(item.geometry));
			if (!geometry || !geometry->is_inside_tree()) continue;
			if (geometry->get_sorting_offset() != value) geometry->set_sorting_offset(value);
			if (geometry->is_sorting_use_aabb_center()) geometry->set_sorting_use_aabb_center(false);
		}
	}
}

Dictionary SpineSprite3D::get_render_statistics() const {
	Dictionary result;
	result["batches"] = active_batches;
	result["vertices"] = active_vertices;
	result["indices"] = active_indices;
	result["batch_pool"] = (int) batches.size();
	result["materials"] = (int) materials.size();
	result["shadow_materials"] = (int) shadow_materials.size();
	int shadow_instances = 0;
	int shadow_instance_pool = 0;
	for (auto batch : batches) {
		if (batch->shadow_instance.is_valid()) shadow_instance_pool++;
		if (batch->active && batch->shadow_instance.is_valid() && batch->shadow_material.is_valid() && shadow_casting != SHADOW_CASTING_OFF)
			shadow_instances++;
	}
	result["shadow_instances"] = shadow_instances;
	result["shadow_instance_pool"] = shadow_instance_pool;
	result["mesh_builds"] = (int64_t) mesh_builds;
	result["index_uploads"] = (int64_t) index_uploads;
	result["vertex_uploads"] = (int64_t) vertex_uploads;
	result["tangent_uploads"] = (int64_t) tangent_uploads;
	result["attribute_uploads"] = (int64_t) attribute_uploads;
	result["material_builds"] = (int64_t) material_builds;
	int shared_shaders = 0;
	{
		std::lock_guard<std::mutex> lock(generated_shader_mutex);
		for (const auto &blend : generated_shaders)
			for (const auto &cull : blend)
				for (const auto &depth : cull)
					for (const auto &shader : depth)
						if (shader.is_valid()) shared_shaders++;
		for (const auto &shader : generated_shadow_shaders)
			if (shader.is_valid()) shared_shaders++;
	}
	result["shared_shaders"] = shared_shaders;
	int64_t vertex_capacity = 0, index_capacity = 0;
	for (auto batch : batches) {
		vertex_capacity += batch->vertex_capacity;
		index_capacity += batch->index_capacity;
	}
	result["vertex_capacity"] = vertex_capacity;
	result["index_capacity"] = index_capacity;
	return result;
}

AABB SpineSprite3D::get_aabb() const {
	AABB bounds;
	bool first = true;
	for (auto batch : batches) {
		if (!batch->active) continue;
		bounds = first ? batch->uploaded_bounds : bounds.merge(batch->uploaded_bounds);
		first = false;
	}
	return bounds;
}

void SpineSprite3D::apply_runtime_parameters(const Ref<Material> &material) {
	Ref<Material> pass = material;
	while (pass.is_valid()) {
		Ref<ShaderMaterial> shader_pass = pass;
		if (shader_pass.is_valid()) {
			shader_pass->set_shader_parameter(SNAME("spine_camera_relative_depth"), camera_relative_depth);
			shader_pass->set_shader_parameter(SNAME("spine_alpha_cutoff"), alpha_cutoff);
			shader_pass->set_shader_parameter(SNAME("spine_normal_map_enabled"), normal_map_enabled);
			shader_pass->set_shader_parameter(SNAME("spine_normal_map_flip_y"), normal_map_flip_y);
			shader_pass->set_shader_parameter(SNAME("spine_normal_scale"), normal_scale);
			shader_pass->set_shader_parameter(SNAME("spine_specular"), specular);
			shader_pass->set_shader_parameter(SNAME("spine_roughness"), roughness);
			shader_pass->set_shader_parameter(SNAME("spine_metallic"), metallic);
		}
		pass = pass->get_next_pass();
	}
}

void SpineSprite3D::refresh_runtime_parameters() {
	for (const auto &entry : materials) apply_runtime_parameters(entry.material);
}

void SpineSprite3D::refresh_shadow_parameters() {
	for (const auto &entry : shadow_materials) entry.material->set_shader_parameter(SNAME("spine_shadow_alpha_cutoff"), shadow_alpha_cutoff);
	// Static shadow maps are not always invalidated by a shader uniform update.
	// Reapplying the instance override dirties shadow rendering without allocating
	// another material for every animated cutoff value.
	for (auto batch : batches) {
		if (!batch->shadow_instance.is_valid() || !batch->shadow_material.is_valid()) continue;
		RS::get_singleton()->instance_geometry_set_material_override(batch->shadow_instance, RID());
		RS::get_singleton()->instance_geometry_set_material_override(batch->shadow_instance, batch->shadow_material->get_rid());
	}
}

void SpineSprite3D::invalidate_materials() {
	// Render-state changes select another cached key; they do not discard the pool.
	refresh_render_items();
}

void SpineSprite3D::refresh_material_template(const Ref<ShaderMaterial> &source) {
	materials.erase(std::remove_if(materials.begin(), materials.end(),
								   [](const SpineBatchMaterial3D &entry) {
									   return entry.source.is_valid() && !ObjectDB::get_instance(entry.source);
								   }),
					materials.end());
	if (source.is_valid()) {
		ObjectID id(source->get_instance_id());
		Ref<Shader> shader = source->get_shader();
		for (auto &entry : materials) {
			if (entry.source != id) continue;
			// A changed pass graph must be cloned again, but ordinary animated shader
			// uniforms update existing material objects instead of duplicating them.
			if (source->get_next_pass().is_valid() || entry.material->get_next_pass().is_valid()) {
				entry.material.unref();
				continue;
			}
			Variant texture = entry.material->get_shader_parameter(SNAME("spine_texture"));
			Variant normal_texture = entry.material->get_shader_parameter(SNAME("spine_normal_texture"));
			Variant has_normal_texture = entry.material->get_shader_parameter(SNAME("spine_has_normal_texture"));
			if (entry.material->get_shader() != shader) entry.material->set_shader(shader);
			if (shader.is_valid()) {
#ifdef SPINE_GODOT_EXTENSION
				Array uniforms = shader->get_shader_uniform_list();
				for (int i = 0; i < uniforms.size(); i++) {
					Dictionary info = uniforms[i];
					StringName name = info["name"];
					entry.material->set_shader_parameter(name, source->get_shader_parameter(name));
				}
#else
				List<PropertyInfo> uniforms;
				shader->get_shader_uniform_list(&uniforms);
				for (const auto &info : uniforms) entry.material->set_shader_parameter(info.name, source->get_shader_parameter(info.name));
#endif
			}
			entry.material->set_shader_parameter(SNAME("spine_texture"), texture);
			entry.material->set_shader_parameter(SNAME("spine_normal_texture"), normal_texture);
			entry.material->set_shader_parameter(SNAME("spine_has_normal_texture"), has_normal_texture);
			entry.material->set_shader_parameter(SNAME("spine_premultiplied_alpha"), entry.pma);
			apply_runtime_parameters(entry.material);
		}
	}
	materials.erase(std::remove_if(materials.begin(), materials.end(), [](const SpineBatchMaterial3D &entry) { return entry.material.is_null(); }),
					materials.end());
	refresh_render_items();
}

void SpineSprite3D::update_skeleton(float delta) {
	Ref<SpineSkeletonDataResource> skeleton_data_res = controller->get_skeleton_data_res();
	if (!skeleton_data_res.is_valid() || !skeleton_data_res->is_skeleton_data_loaded()) return;
	if (!controller->update_skeleton(delta, time_scale)) return;
	Ref<SpineSkeleton> skeleton = controller->get_skeleton();
	controller->begin_native_operation();
	update_render_items(skeleton);
	controller->end_native_operation();
}

void SpineSprite3D::_notification(int what) {
	switch (what) {
		case NOTIFICATION_CHILD_ORDER_CHANGED:
			slots_dirty = true;
			break;
		case NOTIFICATION_READY:
			set_process_internal(update_mode == SpineConstant::UpdateMode_Process);
			set_physics_process_internal(update_mode == SpineConstant::UpdateMode_Physics);
			break;
		case NOTIFICATION_INTERNAL_PROCESS:
			if (update_mode == SpineConstant::UpdateMode_Process) update_skeleton(get_process_delta_time());
			break;
		case NOTIFICATION_INTERNAL_PHYSICS_PROCESS:
			if (update_mode == SpineConstant::UpdateMode_Physics) update_skeleton(get_physics_process_delta_time());
			break;
		case NOTIFICATION_ENTER_WORLD:
			render_scenario = get_world_3d()->get_scenario();
			update_render_item_world_state();
			break;
		case NOTIFICATION_EXIT_WORLD:
			render_scenario = RID();
			update_render_item_world_state();
			break;
		case NOTIFICATION_TRANSFORM_CHANGED:
		case NOTIFICATION_VISIBILITY_CHANGED:
			update_render_item_world_state();
			break;
		default:
			break;
	}
}

void SpineSprite3D::_get_property_list(List<PropertyInfo> *list) const {
	Ref<SpineSkeletonDataResource> skeleton_data_res = controller->get_skeleton_data_res();
	if (!skeleton_data_res.is_valid() || !skeleton_data_res->is_skeleton_data_loaded()) return;
#ifdef SPINE_GODOT_EXTENSION
	PackedStringArray animation_names;
	PackedStringArray skin_names;
#else
	Vector<String> animation_names;
	Vector<String> skin_names;
#endif
	skeleton_data_res->get_animation_names(animation_names);
	skeleton_data_res->get_skin_names(skin_names);
	animation_names.insert(0, "-- Empty --");

	PropertyInfo skin_property(Variant::STRING, "preview_skin", PROPERTY_HINT_ENUM, String(",").join(skin_names),
							   PROPERTY_USAGE_EDITOR | PROPERTY_USAGE_STORAGE);
	list->push_back(skin_property);
	PropertyInfo animation_property(Variant::STRING, "preview_animation", PROPERTY_HINT_ENUM, String(",").join(animation_names),
									PROPERTY_USAGE_EDITOR | PROPERTY_USAGE_STORAGE);
	list->push_back(animation_property);
	list->push_back(PropertyInfo(Variant::BOOL, "preview_frame", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_EDITOR | PROPERTY_USAGE_STORAGE));
	float duration = 0;
	if (!EMPTY(preview_animation) && preview_animation != "-- Empty --") {
		Ref<SpineAnimation> animation = skeleton_data_res->find_animation(preview_animation);
		if (animation.is_valid()) duration = animation->get_duration();
	}
	list->push_back(PropertyInfo(VARIANT_FLOAT, "preview_time", PROPERTY_HINT_RANGE, String("0.0,") + String::num(duration) + String(",0.01"),
								 PROPERTY_USAGE_EDITOR | PROPERTY_USAGE_STORAGE));
}

bool SpineSprite3D::_get(const StringName &property, Variant &value) const {
	if (property == SNAME("preview_skin"))
		value = preview_skin;
	else if (property == SNAME("preview_animation"))
		value = preview_animation;
	else if (property == SNAME("preview_frame"))
		value = preview_frame;
	else if (property == SNAME("preview_time"))
		value = preview_time;
	else
		return false;
	return true;
}

static void update_preview_animation_3d(SpineSprite3D *sprite, const String &skin, const String &animation, bool frame, float time) {
	if (!Engine::get_singleton()->is_editor_hint() || !sprite->get_skeleton().is_valid()) return;
	if (EMPTY(skin) || skin == "Default")
		sprite->get_skeleton()->set_skin(nullptr);
	else
		sprite->get_skeleton()->set_skin_by_name(skin);
	sprite->get_skeleton()->set_to_setup_pose();
	if (EMPTY(animation) || animation == "-- Empty --") {
		sprite->get_animation_state()->set_empty_animation(0, 0);
		return;
	}
	Ref<SpineTrackEntry> entry = sprite->get_animation_state()->set_animation(animation, true, 0);
	if (entry.is_null()) {
		sprite->get_animation_state()->set_empty_animation(0, 0);
		return;
	}
	entry->set_mix_duration(0);
	if (frame) {
		entry->set_time_scale(0);
		entry->set_track_time(time);
	}
}

bool SpineSprite3D::_set(const StringName &property, const Variant &value) {
	if (property == SNAME("preview_skin"))
		preview_skin = value;
	else if (property == SNAME("preview_animation"))
		preview_animation = value;
	else if (property == SNAME("preview_frame"))
		preview_frame = value;
	else if (property == SNAME("preview_time"))
		preview_time = value;
	else
		return false;
	update_preview_animation_3d(this, preview_skin, preview_animation, preview_frame, preview_time);
	if (property == SNAME("preview_skin") || property == SNAME("preview_animation")) NOTIFY_PROPERTY_LIST_CHANGED();
	return true;
}

void SpineSprite3D::on_animation_started(Ref<SpineAnimationState> state, Ref<SpineTrackEntry> entry) {
	emit_signal(SNAME("animation_started"), this, state, entry);
}
void SpineSprite3D::on_animation_interrupted(Ref<SpineAnimationState> state, Ref<SpineTrackEntry> entry) {
	emit_signal(SNAME("animation_interrupted"), this, state, entry);
}
void SpineSprite3D::on_animation_ended(Ref<SpineAnimationState> state, Ref<SpineTrackEntry> entry) {
	emit_signal(SNAME("animation_ended"), this, state, entry);
}
void SpineSprite3D::on_animation_completed(Ref<SpineAnimationState> state, Ref<SpineTrackEntry> entry) {
	emit_signal(SNAME("animation_completed"), this, state, entry);
}
void SpineSprite3D::on_animation_disposed(Ref<SpineAnimationState> state, Ref<SpineTrackEntry> entry) {
	emit_signal(SNAME("animation_disposed"), this, state, entry);
}
void SpineSprite3D::on_animation_event(Ref<SpineAnimationState> state, Ref<SpineTrackEntry> entry, Ref<SpineEvent> event) {
	emit_signal(SNAME("animation_event"), this, state, entry, event);
}
void SpineSprite3D::before_animation_state_update() {
	emit_signal(SNAME("before_animation_state_update"), this);
}
void SpineSprite3D::before_animation_state_apply() {
	emit_signal(SNAME("before_animation_state_apply"), this);
}
void SpineSprite3D::before_world_transforms_change() {
	emit_signal(SNAME("before_world_transforms_change"), this);
}
void SpineSprite3D::world_transforms_changed() {
	emit_signal(SNAME("world_transforms_changed"), this);
}

SpineConstant::UpdateMode SpineSprite3D::get_update_mode() {
	return update_mode;
}
void SpineSprite3D::set_update_mode(SpineConstant::UpdateMode value) {
	update_mode = value;
	set_process_internal(update_mode == SpineConstant::UpdateMode_Process);
	set_physics_process_internal(update_mode == SpineConstant::UpdateMode_Physics);
}
Ref<SpineSkin> SpineSprite3D::new_skin(const String &name) {
	return controller->new_skin(name);
}
void SpineSprite3D::set_time_scale(float value) {
	time_scale = value;
}
float SpineSprite3D::get_time_scale() {
	return time_scale;
}
void SpineSprite3D::set_pixel_size(float value) {
	ERR_FAIL_COND_MSG(!Math::is_finite(value) || value <= 0, "pixel_size must be finite and greater than zero.");
	pixel_size = value;
	refresh_render_items();
}
float SpineSprite3D::get_pixel_size() {
	return pixel_size;
}
void SpineSprite3D::set_slot_depth_offset(float value) {
	ERR_FAIL_COND_MSG(!Math::is_finite(value) || value < 0, "slot_depth_offset must be finite and non-negative.");
	slot_depth_offset = value;
	refresh_render_items();
}
float SpineSprite3D::get_slot_depth_offset() {
	return slot_depth_offset;
}
void SpineSprite3D::set_camera_relative_depth(bool value) {
	if (camera_relative_depth == value) return;
	camera_relative_depth = value;
	refresh_runtime_parameters();
#ifdef TOOLS_ENABLED
	editor_geometry_dirty = true;
#endif
	refresh_render_items();
}
bool SpineSprite3D::get_camera_relative_depth() const {
	return camera_relative_depth;
}
void SpineSprite3D::set_depth_write_enabled(bool value) {
	if (depth_write_enabled == value) return;
	depth_write_enabled = value;
	invalidate_materials();
}
bool SpineSprite3D::get_depth_write_enabled() const {
	return depth_write_enabled;
}
void SpineSprite3D::set_alpha_cutoff(float value) {
	ERR_FAIL_COND_MSG(!Math::is_finite(value) || value < 0 || value > 1, "alpha_cutoff must be finite and between zero and one.");
	if (alpha_cutoff == value) return;
	alpha_cutoff = value;
	refresh_runtime_parameters();
}
float SpineSprite3D::get_alpha_cutoff() const {
	return alpha_cutoff;
}
void SpineSprite3D::set_sorting_step(float value) {
	ERR_FAIL_COND_MSG(!Math::is_finite(value) || value <= 0, "sorting_step must be finite and greater than zero.");
	sorting_step = value;
	update_sorting();
}
float SpineSprite3D::get_sorting_step() {
	return sorting_step;
}
void SpineSprite3D::set_render_priority(int value) {
	render_priority = CLAMP(value, -128, 127);
	invalidate_materials();
}
int SpineSprite3D::get_render_priority() {
	return render_priority;
}
void SpineSprite3D::set_cull_mode(CullMode value) {
	ERR_FAIL_INDEX((int) value, 3);
	cull_mode = value;
	invalidate_materials();
}
SpineSprite3D::CullMode SpineSprite3D::get_cull_mode() {
	return cull_mode;
}
void SpineSprite3D::set_lighting_enabled(bool value) {
	if (lighting_enabled == value) return;
	lighting_enabled = value;
	invalidate_materials();
}
bool SpineSprite3D::get_lighting_enabled() const {
	return lighting_enabled;
}
void SpineSprite3D::set_normal_map_enabled(bool value) {
	if (normal_map_enabled == value) return;
	normal_map_enabled = value;
	refresh_runtime_parameters();
}
bool SpineSprite3D::get_normal_map_enabled() const {
	return normal_map_enabled;
}
void SpineSprite3D::set_normal_map_flip_y(bool value) {
	if (normal_map_flip_y == value) return;
	normal_map_flip_y = value;
	refresh_runtime_parameters();
}
bool SpineSprite3D::get_normal_map_flip_y() const {
	return normal_map_flip_y;
}
void SpineSprite3D::set_normal_scale(float value) {
	ERR_FAIL_COND_MSG(!Math::is_finite(value) || value < 0, "normal_scale must be finite and non-negative.");
	if (normal_scale == value) return;
	normal_scale = value;
	refresh_runtime_parameters();
}
float SpineSprite3D::get_normal_scale() const {
	return normal_scale;
}
void SpineSprite3D::set_specular(float value) {
	ERR_FAIL_COND_MSG(!Math::is_finite(value) || value < 0 || value > 1, "specular must be finite and between zero and one.");
	if (specular == value) return;
	specular = value;
	refresh_runtime_parameters();
}
float SpineSprite3D::get_specular() const {
	return specular;
}
void SpineSprite3D::set_roughness(float value) {
	ERR_FAIL_COND_MSG(!Math::is_finite(value) || value < 0 || value > 1, "roughness must be finite and between zero and one.");
	if (roughness == value) return;
	roughness = value;
	refresh_runtime_parameters();
}
float SpineSprite3D::get_roughness() const {
	return roughness;
}
void SpineSprite3D::set_metallic(float value) {
	ERR_FAIL_COND_MSG(!Math::is_finite(value) || value < 0 || value > 1, "metallic must be finite and between zero and one.");
	if (metallic == value) return;
	metallic = value;
	refresh_runtime_parameters();
}
float SpineSprite3D::get_metallic() const {
	return metallic;
}
void SpineSprite3D::set_shadow_casting(ShadowCasting value) {
	ERR_FAIL_INDEX((int) value, 4);
	if (shadow_casting == value) return;
	shadow_casting = value;
	invalidate_materials();
}
SpineSprite3D::ShadowCasting SpineSprite3D::get_shadow_casting() const {
	return shadow_casting;
}
void SpineSprite3D::set_shadow_alpha_cutoff(float value) {
	ERR_FAIL_COND_MSG(!Math::is_finite(value) || value < 0 || value > 1, "shadow_alpha_cutoff must be finite and between zero and one.");
	if (shadow_alpha_cutoff == value) return;
	shadow_alpha_cutoff = value;
	refresh_shadow_parameters();
}
float SpineSprite3D::get_shadow_alpha_cutoff() const {
	return shadow_alpha_cutoff;
}
void SpineSprite3D::set_normal_material(const Ref<ShaderMaterial> &material) {
	normal_material = material;
	refresh_material_template(material);
}
Ref<ShaderMaterial> SpineSprite3D::get_normal_material() {
	return normal_material;
}
void SpineSprite3D::set_additive_material(const Ref<ShaderMaterial> &material) {
	additive_material = material;
	refresh_material_template(material);
}
Ref<ShaderMaterial> SpineSprite3D::get_additive_material() {
	return additive_material;
}

#endif
