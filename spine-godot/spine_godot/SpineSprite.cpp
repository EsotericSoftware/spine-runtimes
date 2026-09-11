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

#include "SpineSprite.h"
#include "SpineEvent.h"
#include "SpineTrackEntry.h"
#include "SpineSkeleton.h"
#include "SpineRendererObject.h"
#include "SpineSlotNode.h"

#ifdef SPINE_GODOT_EXTENSION
#include <godot_cpp/core/memory.hpp>
#include <godot_cpp/classes/engine.hpp>
#include <godot_cpp/classes/geometry2d.hpp>
#include <godot_cpp/variant/array.hpp>
#include <godot_cpp/classes/mesh.hpp>
#include <godot_cpp/classes/rendering_server.hpp>
#include <godot_cpp/classes/shader.hpp>
#include <godot_cpp/classes/shader_material.hpp>
#ifdef TOOLS_ENABLED
#include <godot_cpp/classes/editor_interface.hpp>
#endif
#include <godot_cpp/classes/control.hpp>
#include <godot_cpp/classes/viewport.hpp>
#include <godot_cpp/classes/scene_tree.hpp>
#if TOOLS_ENABLED
#include <godot_cpp/classes/editor_plugin.hpp>
#include <godot_cpp/classes/font.hpp>
#endif
#else
#include "core/os/memory.h"

#if VERSION_MAJOR > 3
#include "core/config/engine.h"
#include "core/math/geometry_2d.h"
#include "core/math/transform_2d.h"
#include "core/variant/array.h"
#include "scene/resources/mesh.h"
#include "scene/resources/material.h"
#include "scene/resources/shader.h"
#if (VERSION_MAJOR >= 4 && VERSION_MINOR >= 6)
#include "servers/rendering/rendering_server.h"
#else
#include "servers/rendering_server.h"
#endif
#include "scene/resources/canvas_item_material.h"
#if VERSION_MINOR > 0 && defined(TOOLS_ENABLED)
#include "editor/editor_interface.h"
#endif
#else
#include "core/engine.h"
#endif

#include "scene/gui/control.h"
#include "scene/main/viewport.h"
#if VERSION_MAJOR > 3
#include "scene/main/scene_tree.h"
#endif

#if TOOLS_ENABLED

#if VERSION_MAJOR > 3
#if VERSION_MINOR > 2
#include "editor/plugins/editor_plugin.h"
#else
#include "editor/editor_plugin.h"
#endif
#else
#include "editor/editor_plugin.h"
#endif

#endif
#endif

// Needed due to shared lib initializers in GDExtension.
// See: https://x.com/badlogicgames/status/1843661872404591068
struct SpineSpriteStatics {
private:
	static SpineSpriteStatics *_instance;

public:
	Ref<CanvasItemMaterial> default_materials[4] = {};
#if VERSION_MAJOR > 3 && VERSION_MINOR >= 3
	Ref<ShaderMaterial> tint_black_materials[4] = {};
#endif
	int sprite_count;
	spine::Array<unsigned short> quad_indices;
	spine::Array<float> scratch_vertices;
#ifdef SPINE_GODOT_EXTENSION
	PackedVector2Array scratch_points;
#else
	Vector<Vector2> scratch_points;
#endif

	SpineSpriteStatics() : sprite_count(0) {
		quad_indices.setSize(6, 0);
		quad_indices[0] = 0;
		quad_indices[1] = 1;
		quad_indices[2] = 2;
		quad_indices[3] = 2;
		quad_indices[4] = 3;
		quad_indices[5] = 0;
		scratch_vertices.ensureCapacity(1200);

		Ref<CanvasItemMaterial> material_normal(memnew(CanvasItemMaterial));
		material_normal->set_blend_mode(CanvasItemMaterial::BLEND_MODE_MIX);
		default_materials[spine::BlendMode_Normal] = material_normal;

		Ref<CanvasItemMaterial> material_additive(memnew(CanvasItemMaterial));
		material_additive->set_blend_mode(CanvasItemMaterial::BLEND_MODE_ADD);
		default_materials[spine::BlendMode_Additive] = material_additive;

		Ref<CanvasItemMaterial> material_multiply(memnew(CanvasItemMaterial));
		material_multiply->set_blend_mode(CanvasItemMaterial::BLEND_MODE_MUL);
		default_materials[spine::BlendMode_Multiply] = material_multiply;

		Ref<CanvasItemMaterial> material_screen(memnew(CanvasItemMaterial));
		material_screen->set_blend_mode(CanvasItemMaterial::BLEND_MODE_SUB);
		default_materials[spine::BlendMode_Screen] = material_screen;

#if VERSION_MAJOR > 3 && VERSION_MINOR >= 3
		const char *blend_modes[] = {"blend_mix", "blend_add", "blend_mul", "blend_sub"};
		for (int i = 0; i < 4; i++) {
			Ref<Shader> shader(memnew(Shader));
			shader->set_code(String("shader_type canvas_item;\nrender_mode ") + blend_modes[i] + ";\n" + R"(
				varying vec4 light_color;
				varying vec3 dark_color;
				void vertex() {
					light_color = CUSTOM1 * COLOR;
					dark_color = CUSTOM0.rgb * COLOR.rgb;
				}
				void fragment() {
					vec4 tex = texture(TEXTURE, UV);
					COLOR = vec4(tex.rgb * light_color.rgb + (vec3(1.0) - tex.rgb) * dark_color, tex.a * light_color.a);
				}
			)");
			Ref<ShaderMaterial> material(memnew(ShaderMaterial));
			material->set_shader(shader);
			tint_black_materials[i] = material;
		}
#endif
	}

	static SpineSpriteStatics &instance() {
		if (!_instance) {
			_instance = new SpineSpriteStatics();
		}
		return *_instance;
	}

	static void clear() {
		if (_instance) {
			delete _instance;
		}
		_instance = nullptr;
	}
};

SpineSpriteStatics *SpineSpriteStatics::_instance = nullptr;

static void clear_triangles(SpineMesh2D *mesh_instance) {
#if VERSION_MAJOR > 3
	RenderingServer::get_singleton()->canvas_item_clear(mesh_instance->get_canvas_item());
#else
	VisualServer::get_singleton()->canvas_item_clear(mesh_instance->get_canvas_item());
#endif
}

#ifdef SPINE_GODOT_EXTENSION
static void add_triangles(SpineMesh2D *mesh_instance, const PackedVector2Array &vertices, const PackedVector2Array &uvs,
						  const PackedColorArray &colors, const PackedInt32Array &indices, SpineRendererObject *renderer_object) {
#else
static void add_triangles(SpineMesh2D *mesh_instance, const Vector<Point2> &vertices, const Vector<Point2> &uvs, const Vector<Color> &colors,
						  const Vector<int> &indices, SpineRendererObject *renderer_object) {
#endif
#if VERSION_MAJOR > 3
	mesh_instance->update_mesh(vertices, uvs, colors, indices, renderer_object);
#else
#define USE_MESH 0
#if USE_MESH
	mesh_instance->update_mesh(vertices, uvs, colors, indices, renderer_object);
#else
	auto texture = renderer_object->texture;
	auto normal_map = renderer_object->normal_map;
	auto specular_map = renderer_object->specular_map;
#if VERSION_MAJOR > 3
	VisualServer::get_singleton()->canvas_item_add_triangle_array(mesh_instance->get_canvas_item(), indices, vertices, colors, uvs, Vector<int>(),
																  Vector<float>(), texture.is_null() ? RID() : texture->get_rid(), -1,
																  normal_map.is_null() ? RID() : normal_map->get_rid(),
																  specular_map.is_null() ? RID() : specular_map->get_rid());
#else
	VisualServer::get_singleton()->canvas_item_add_triangle_array(mesh_instance->get_canvas_item(), indices, vertices, colors, uvs, Vector<int>(),
																  Vector<float>(), texture.is_null() ? RID() : texture->get_rid(), -1,
																  normal_map.is_null() ? RID() : normal_map->get_rid());
#endif
#endif
#endif
}

void SpineMesh2D::_notification(int what) {
	switch (what) {
		case NOTIFICATION_READY: {
			set_process_internal(true);
			break;
		}
		case NOTIFICATION_INTERNAL_PROCESS:
#if VERSION_MAJOR > 3
			queue_redraw();
#else
			update();
#endif
			break;
		case NOTIFICATION_DRAW:
			clear_triangles(this);
			if (renderer_object) add_triangles(this, vertices, uvs, colors, indices, renderer_object);
			break;
		default:
			break;
	}
}

void SpineMesh2D::_bind_methods() {
}

#if VERSION_MAJOR > 3 && VERSION_MINOR >= 3
static void tint_to_render_colors(SpineMesh2D *mesh_instance, Color &light, Color &dark) {
	// Unlike ARRAY_COLOR, Godot does not convert custom attributes to linear color space for HDR 2D.
	if (mesh_instance->get_viewport()->is_using_hdr_2d() && RS::get_singleton()->get_rendering_device()) {
		light = light.srgb_to_linear();
		dark = dark.srgb_to_linear();
	}
}

uint64_t SpineMesh2D::add_tint_arrays(Array &arrays) {
	mesh_has_dark_color = has_dark_color;
	if (!has_dark_color) return 0;

	// Float attributes also work around Godot's Compatibility renderer treating RGBA8 custom attributes as a single component.
	PackedFloat32Array dark_colors;
	PackedFloat32Array light_colors;
	dark_colors.resize(vertices.size() * 3);
	light_colors.resize(vertices.size() * 4);
	float *dark = dark_colors.ptrw();
	float *light = light_colors.ptrw();
	Color dark_tint = dark_color;
	Color light_tint = light_color;
	tint_to_render_colors(this, light_tint, dark_tint);
	for (int i = 0; i < vertices.size(); i++) {
		dark[i * 3] = dark_tint.r;
		dark[i * 3 + 1] = dark_tint.g;
		dark[i * 3 + 2] = dark_tint.b;
		light[i * 4] = light_tint.r;
		light[i * 4 + 1] = light_tint.g;
		light[i * 4 + 2] = light_tint.b;
		light[i * 4 + 3] = light_tint.a;
	}
	arrays[Mesh::ARRAY_CUSTOM0] = dark_colors;
	arrays[Mesh::ARRAY_CUSTOM1] = light_colors;
	return (uint64_t(Mesh::ARRAY_CUSTOM_RGB_FLOAT) << Mesh::ARRAY_FORMAT_CUSTOM0_SHIFT) |
		(uint64_t(Mesh::ARRAY_CUSTOM_RGBA_FLOAT) << Mesh::ARRAY_FORMAT_CUSTOM1_SHIFT);
}

void SpineMesh2D::update_tint_attribute_buffer() {
	if (!has_dark_color) return;
	Color dark_tint = dark_color;
	Color light_tint = light_color;
	tint_to_render_colors(this, light_tint, dark_tint);
	float dark[] = {dark_tint.r, dark_tint.g, dark_tint.b};
	float light[] = {light_tint.r, light_tint.g, light_tint.b, light_tint.a};
	uint8_t *buffer = attribute_buffer.ptrw();
	for (int i = 0; i < num_vertices; i++) {
		memcpy(&buffer[i * attribute_stride + surface_offsets[Mesh::ARRAY_CUSTOM0]], dark, sizeof(dark));
		memcpy(&buffer[i * attribute_stride + surface_offsets[Mesh::ARRAY_CUSTOM1]], light, sizeof(light));
	}
}
#endif

#ifdef SPINE_GODOT_EXTENSION
void SpineMesh2D::update_mesh(const PackedVector2Array &vertices, const PackedVector2Array &uvs, const PackedColorArray &colors,
							  const PackedInt32Array &indices, SpineRendererObject *renderer_object) {
	if (!mesh.is_valid() || vertices.size() != num_vertices || indices.size() != num_indices || indices_changed
#if VERSION_MINOR >= 3
		|| has_dark_color != mesh_has_dark_color
#endif
	) {
		if (mesh.is_valid()) {
			RS::get_singleton()->free_rid(mesh);
		}
		mesh = RS::get_singleton()->mesh_create();
		Array arrays;
		arrays.resize(Mesh::ARRAY_MAX);
		arrays[Mesh::ARRAY_VERTEX] = vertices;
		arrays[Mesh::ARRAY_TEX_UV] = uvs;
		arrays[Mesh::ARRAY_COLOR] = colors;
		arrays[Mesh::ARRAY_INDEX] = indices;
		uint64_t flags = RS::ArrayFormat::ARRAY_FLAG_USE_DYNAMIC_UPDATE;
#if VERSION_MINOR >= 3
		flags |= add_tint_arrays(arrays);
#endif
		RS::get_singleton()->mesh_add_surface_from_arrays(mesh, RS::PrimitiveType::PRIMITIVE_TRIANGLES, arrays, Array(), Dictionary(), flags);
		Dictionary surface = RS::get_singleton()->mesh_get_surface(mesh, 0);
		RS::ArrayFormat surface_format = (RS::ArrayFormat) static_cast<int64_t>(surface["format"]);
		surface_offsets[RS::ARRAY_VERTEX] = RS::get_singleton()->mesh_surface_get_format_offset(surface_format, vertices.size(), RS::ARRAY_VERTEX);
		surface_offsets[RS::ARRAY_COLOR] = RS::get_singleton()->mesh_surface_get_format_offset(surface_format, vertices.size(), RS::ARRAY_COLOR);
		surface_offsets[RS::ARRAY_TEX_UV] = RS::get_singleton()->mesh_surface_get_format_offset(surface_format, vertices.size(), RS::ARRAY_TEX_UV);
#if VERSION_MINOR >= 3
		if (has_dark_color) {
			surface_offsets[RS::ARRAY_CUSTOM0] = RS::get_singleton()->mesh_surface_get_format_offset(surface_format, vertices.size(),
																								 RS::ARRAY_CUSTOM0);
			surface_offsets[RS::ARRAY_CUSTOM1] = RS::get_singleton()->mesh_surface_get_format_offset(surface_format, vertices.size(),
																								 RS::ARRAY_CUSTOM1);
		}
#endif
		vertex_stride = RS::get_singleton()->mesh_surface_get_format_vertex_stride(surface_format, vertices.size());
		attribute_stride = RS::get_singleton()->mesh_surface_get_format_attribute_stride(surface_format, vertices.size());
		vertex_buffer = surface["vertex_data"];
		attribute_buffer = surface["attribute_data"];
		num_vertices = vertices.size();
		num_indices = indices.size();
		indices_changed = false;
	} else {
		AABB aabb_new;
		uint8_t color[4] = {uint8_t(CLAMP(colors[0].r * 255.0, 0.0, 255.0)), uint8_t(CLAMP(colors[0].g * 255.0, 0.0, 255.0)),
							uint8_t(CLAMP(colors[0].b * 255.0, 0.0, 255.0)), uint8_t(CLAMP(colors[0].a * 255.0, 0.0, 255.0))};

		uint8_t *vertex_write_buffer = vertex_buffer.ptrw();
		uint8_t *attribute_write_buffer = attribute_buffer.ptrw();
		for (int i = 0; i < vertices.size(); i++) {
			Vector2 vertex(vertices[i]);
			if (i == 0) {
				aabb_new.position = Vector3(vertex.x, vertex.y, 0);
				aabb_new.size = Vector3();
			} else {
				aabb_new.expand_to(Vector3(vertex.x, vertex.y, 0));
			}

			float uv[2] = {(float) uvs[i].x, (float) uvs[i].y};
			memcpy(&vertex_write_buffer[i * vertex_stride + surface_offsets[RS::ARRAY_VERTEX]], &vertex, sizeof(float) * 2);
			memcpy(&attribute_write_buffer[i * attribute_stride + surface_offsets[RS::ARRAY_COLOR]], color, 4);
			memcpy(&attribute_write_buffer[i * attribute_stride + surface_offsets[RS::ARRAY_TEX_UV]], uv, 8);
		}
#if VERSION_MINOR >= 3
		update_tint_attribute_buffer();
#endif
		RS::get_singleton()->mesh_surface_update_vertex_region(mesh, 0, 0, vertex_buffer);
		RS::get_singleton()->mesh_surface_update_attribute_region(mesh, 0, 0, attribute_buffer);
		RS::get_singleton()->mesh_set_custom_aabb(mesh, aabb_new);
	}

	RenderingServer::get_singleton()->canvas_item_add_mesh(this->get_canvas_item(), mesh, Transform2D(), Color(1, 1, 1, 1),
														   renderer_object->canvas_texture->get_rid());
}
#else
void SpineMesh2D::update_mesh(const Vector<Point2> &vertices, const Vector<Point2> &uvs, const Vector<Color> &colors, const Vector<int> &indices,
							  SpineRendererObject *renderer_object) {
#if VERSION_MAJOR > 3
	if (!mesh.is_valid() || vertices.size() != num_vertices || indices.size() != num_indices || indices_changed
#if VERSION_MINOR >= 3
		|| has_dark_color != mesh_has_dark_color
#endif
	) {
		if (mesh.is_valid()) {
#ifdef SPINE_GODOT_EXTENSION
			RS::get_singleton()->free_rid(mesh);
#else
			RS::get_singleton()->free(mesh);
#endif
		}
		mesh = RS::get_singleton()->mesh_create();
		Array arrays;
		arrays.resize(Mesh::ARRAY_MAX);
		arrays[Mesh::ARRAY_VERTEX] = vertices;
		arrays[Mesh::ARRAY_TEX_UV] = uvs;
		arrays[Mesh::ARRAY_COLOR] = colors;
		arrays[Mesh::ARRAY_INDEX] = indices;
#if VERSION_MAJOR >= 4 && VERSION_MINOR >= 7
		RenderingServerTypes::SurfaceData surface;
#else
		RS::SurfaceData surface;
#endif
		uint32_t skin_stride;
		uint64_t flags = Mesh::ArrayFormat::ARRAY_FLAG_USE_DYNAMIC_UPDATE;
#if VERSION_MINOR >= 3
		flags |= add_tint_arrays(arrays);
#endif
#if VERSION_MAJOR >= 4 && VERSION_MINOR >= 7
		RS::get_singleton()->mesh_create_surface_data_from_arrays(&surface, RSE::PRIMITIVE_TRIANGLES, arrays, TypedArray<Array>(), Dictionary(),
																  flags);
#else
		RS::get_singleton()->mesh_create_surface_data_from_arrays(&surface, (RS::PrimitiveType) Mesh::PRIMITIVE_TRIANGLES, arrays,
																  TypedArray<Array>(), Dictionary(), flags);
#endif
		RS::get_singleton()->mesh_add_surface(mesh, surface);
#if VERSION_MINOR > 1
		RS::get_singleton()->mesh_surface_make_offsets_from_format(surface.format, surface.vertex_count, surface.index_count, surface_offsets,
																   vertex_stride, normal_tangent_stride, attribute_stride, skin_stride);
#else
		RS::get_singleton()->mesh_surface_make_offsets_from_format(surface.format, surface.vertex_count, surface.index_count, surface_offsets,
																   vertex_stride, attribute_stride, skin_stride);
#endif
		num_vertices = vertices.size();
		num_indices = indices.size();
		vertex_buffer = surface.vertex_data;
		attribute_buffer = surface.attribute_data;
		indices_changed = false;
	} else {
		AABB aabb_new;
		uint8_t *vertex_write_buffer = vertex_buffer.ptrw();
		uint8_t *attribute_write_buffer = attribute_buffer.ptrw();
		uint8_t color[4] = {uint8_t(CLAMP(colors[0].r * 255.0, 0.0, 255.0)), uint8_t(CLAMP(colors[0].g * 255.0, 0.0, 255.0)),
							uint8_t(CLAMP(colors[0].b * 255.0, 0.0, 255.0)), uint8_t(CLAMP(colors[0].a * 255.0, 0.0, 255.0))};

		for (int i = 0; i < vertices.size(); i++) {
			Vector2 vertex(vertices[i]);
			if (i == 0) {
				aabb_new.position = Vector3(vertex.x, vertex.y, 0);
				aabb_new.size = Vector3();
			} else {
				aabb_new.expand_to(Vector3(vertex.x, vertex.y, 0));
			}

			float uv[2] = {(float) uvs[i].x, (float) uvs[i].y};
#if VERSION_MAJOR >= 4 && VERSION_MINOR >= 7
			memcpy(&vertex_write_buffer[i * vertex_stride + surface_offsets[RSE::ARRAY_VERTEX]], &vertex, sizeof(float) * 2);
			memcpy(&attribute_write_buffer[i * attribute_stride + surface_offsets[RSE::ARRAY_COLOR]], color, 4);
			memcpy(&attribute_write_buffer[i * attribute_stride + surface_offsets[RSE::ARRAY_TEX_UV]], uv, 8);
#else
			memcpy(&vertex_write_buffer[i * vertex_stride + surface_offsets[RS::ARRAY_VERTEX]], &vertex, sizeof(float) * 2);
			memcpy(&attribute_write_buffer[i * attribute_stride + surface_offsets[RS::ARRAY_COLOR]], color, 4);
			memcpy(&attribute_write_buffer[i * attribute_stride + surface_offsets[RS::ARRAY_TEX_UV]], uv, 8);
#endif
		}
#if VERSION_MINOR >= 3
		update_tint_attribute_buffer();
#endif
		RS::get_singleton()->mesh_surface_update_vertex_region(mesh, 0, 0, vertex_buffer);
		RS::get_singleton()->mesh_surface_update_attribute_region(mesh, 0, 0, attribute_buffer);
		RS::get_singleton()->mesh_set_custom_aabb(mesh, aabb_new);
	}

	RenderingServer::get_singleton()->canvas_item_add_mesh(this->get_canvas_item(), mesh, Transform2D(), Color(1, 1, 1, 1),
														   renderer_object->canvas_texture->get_rid());
#else
	if (!mesh.is_valid() || vertices.size() != num_vertices || indices.size() != num_indices || indices_changed) {
		if (mesh.is_valid()) {
			VS::get_singleton()->free(mesh);
		}
		mesh = VS::get_singleton()->mesh_create();
		Array arrays;
		arrays.resize(Mesh::ARRAY_MAX);
		arrays[Mesh::ARRAY_VERTEX] = vertices;
		arrays[Mesh::ARRAY_TEX_UV] = uvs;
		arrays[Mesh::ARRAY_COLOR] = colors;
		arrays[Mesh::ARRAY_INDEX] = indices;
		uint32_t compress_format = (VS::ARRAY_COMPRESS_DEFAULT & ~VS::ARRAY_COMPRESS_TEX_UV);
		VS::get_singleton()->mesh_add_surface_from_arrays(mesh, (VS::PrimitiveType) Mesh::PRIMITIVE_TRIANGLES, arrays, Array(), compress_format);
		int surface_vertex_len = VS::get_singleton()->mesh_surface_get_array_len(mesh, 0);
		int surface_index_len = VS::get_singleton()->mesh_surface_get_array_index_len(mesh, 0);
		mesh_surface_format = VS::get_singleton()->mesh_surface_get_format(mesh, 0);
		mesh_buffer = VS::get_singleton()->mesh_surface_get_array(mesh, 0);
		VS::get_singleton()->mesh_surface_make_offsets_from_format(mesh_surface_format, surface_vertex_len, surface_index_len, mesh_surface_offsets,
																   mesh_stride);
		num_vertices = vertices.size();
		num_indices = indices.size();
		indices_changed = false;
	} else {
		AABB aabb_new;
		PoolVector<uint8_t>::Write write_buffer = mesh_buffer.write();

		uint8_t color[4] = {uint8_t(CLAMP(colors[0].r * 255.0, 0.0, 255.0)), uint8_t(CLAMP(colors[0].g * 255.0, 0.0, 255.0)),
							uint8_t(CLAMP(colors[0].b * 255.0, 0.0, 255.0)), uint8_t(CLAMP(colors[0].a * 255.0, 0.0, 255.0))};

		for (int i = 0; i < vertices.size(); i++) {
			Vector2 vertex(vertices[i]);
			if (i == 0) {
				aabb_new.position = Vector3(vertex.x, vertex.y, 0);
				aabb_new.size = Vector3();
			} else {
				aabb_new.expand_to(Vector3(vertex.x, vertex.y, 0));
			}

			float uv[2] = {(float) uvs[i].x, (float) uvs[i].y};
			memcpy(&write_buffer[i * mesh_stride[VS::ARRAY_VERTEX] + mesh_surface_offsets[VS::ARRAY_VERTEX]], &vertex, sizeof(float) * 2);
			memcpy(&write_buffer[i * mesh_stride[VS::ARRAY_TEX_UV] + mesh_surface_offsets[VS::ARRAY_TEX_UV]], uv, 8);
			memcpy(&write_buffer[i * mesh_stride[VS::ARRAY_COLOR] + mesh_surface_offsets[VS::ARRAY_COLOR]], color, 4);
		}
		write_buffer.release();
		VS::get_singleton()->mesh_surface_update_region(mesh, 0, 0, mesh_buffer);
		VS::get_singleton()->mesh_set_custom_aabb(mesh, aabb_new);
	}
#if VERSION_MAJOR > 3
	VS::get_singleton()->canvas_item_add_mesh(this->get_canvas_item(), mesh, Transform2D(), Color(1, 1, 1, 1),
											  renderer_object->texture.is_null() ? RID() : renderer_object->texture->get_rid(),
											  renderer_object->normal_map.is_null() ? RID() : renderer_object->normal_map->get_rid(),
											  renderer_object->specular_map.is_null() ? RID() : renderer_object->specular_map->get_rid());
#else
	VS::get_singleton()->canvas_item_add_mesh(this->get_canvas_item(), mesh, Transform2D(), Color(1, 1, 1, 1),
											  renderer_object->texture.is_null() ? RID() : renderer_object->texture->get_rid(),
											  renderer_object->normal_map.is_null() ? RID() : renderer_object->normal_map->get_rid());
#endif
#endif
}
#endif

void SpineSprite::clear_statics() {
	SpineSpriteStatics::clear();
}

void SpineSprite::_bind_methods() {
	ClassDB::bind_method(D_METHOD("set_skeleton_data_res", "skeleton_data_res"), &SpineSprite::set_skeleton_data_res);
	ClassDB::bind_method(D_METHOD("get_skeleton_data_res"), &SpineSprite::get_skeleton_data_res);
	ClassDB::bind_method(D_METHOD("get_skeleton"), &SpineSprite::get_skeleton);
	ClassDB::bind_method(D_METHOD("get_animation_state"), &SpineSprite::get_animation_state);
	ClassDB::bind_method(D_METHOD("on_skeleton_data_changed"), &SpineSprite::on_skeleton_data_changed);

	ClassDB::bind_method(D_METHOD("get_global_bone_transform", "bone_name"),
					 static_cast<Transform2D (SpineSprite::*)(const String &)>(&SpineSprite::get_global_bone_transform));
	ClassDB::bind_method(D_METHOD("set_global_bone_transform", "bone_name", "global_transform"),
					 static_cast<void (SpineSprite::*)(const String &, Transform2D)>(&SpineSprite::set_global_bone_transform));

	ClassDB::bind_method(D_METHOD("set_update_mode", "v"), &SpineSprite::set_update_mode);
	ClassDB::bind_method(D_METHOD("get_update_mode"), &SpineSprite::get_update_mode);

	ClassDB::bind_method(D_METHOD("set_normal_material", "material"), &SpineSprite::set_normal_material);
	ClassDB::bind_method(D_METHOD("get_normal_material"), &SpineSprite::get_normal_material);
	ClassDB::bind_method(D_METHOD("set_additive_material", "material"), &SpineSprite::set_additive_material);
	ClassDB::bind_method(D_METHOD("get_additive_material"), &SpineSprite::get_additive_material);
	ClassDB::bind_method(D_METHOD("set_multiply_material", "material"), &SpineSprite::set_multiply_material);
	ClassDB::bind_method(D_METHOD("get_multiply_material"), &SpineSprite::get_multiply_material);
	ClassDB::bind_method(D_METHOD("set_screen_material", "material"), &SpineSprite::set_screen_material);
	ClassDB::bind_method(D_METHOD("get_screen_material"), &SpineSprite::get_screen_material);

	ClassDB::bind_method(D_METHOD("get_time_scale"), &SpineSprite::get_time_scale);
	ClassDB::bind_method(D_METHOD("set_time_scale", "v"), &SpineSprite::set_time_scale);

	ClassDB::bind_method(D_METHOD("set_debug_root", "v"), &SpineSprite::set_debug_root);
	ClassDB::bind_method(D_METHOD("get_debug_root"), &SpineSprite::get_debug_root);
	ClassDB::bind_method(D_METHOD("set_debug_root_color", "v"), &SpineSprite::set_debug_root_color);
	ClassDB::bind_method(D_METHOD("get_debug_root_color"), &SpineSprite::get_debug_root_color);
	ClassDB::bind_method(D_METHOD("set_debug_bones", "v"), &SpineSprite::set_debug_bones);
	ClassDB::bind_method(D_METHOD("get_debug_bones"), &SpineSprite::get_debug_bones);
	ClassDB::bind_method(D_METHOD("set_debug_bones_color", "v"), &SpineSprite::set_debug_bones_color);
	ClassDB::bind_method(D_METHOD("get_debug_bones_color"), &SpineSprite::get_debug_bones_color);
	ClassDB::bind_method(D_METHOD("set_debug_bones_thickness", "v"), &SpineSprite::set_debug_bones_thickness);
	ClassDB::bind_method(D_METHOD("get_debug_bones_thickness"), &SpineSprite::get_debug_bones_thickness);
	ClassDB::bind_method(D_METHOD("set_debug_regions", "v"), &SpineSprite::set_debug_regions);
	ClassDB::bind_method(D_METHOD("get_debug_regions"), &SpineSprite::get_debug_regions);
	ClassDB::bind_method(D_METHOD("set_debug_regions_color", "v"), &SpineSprite::set_debug_regions_color);
	ClassDB::bind_method(D_METHOD("get_debug_regions_color"), &SpineSprite::get_debug_regions_color);
	ClassDB::bind_method(D_METHOD("set_debug_meshes", "v"), &SpineSprite::set_debug_meshes);
	ClassDB::bind_method(D_METHOD("get_debug_meshes"), &SpineSprite::get_debug_meshes);
	ClassDB::bind_method(D_METHOD("set_debug_meshes_color", "v"), &SpineSprite::set_debug_meshes_color);
	ClassDB::bind_method(D_METHOD("get_debug_meshes_color"), &SpineSprite::get_debug_meshes_color);
	ClassDB::bind_method(D_METHOD("set_debug_bounding_boxes", "v"), &SpineSprite::set_debug_bounding_boxes);
	ClassDB::bind_method(D_METHOD("get_debug_bounding_boxes"), &SpineSprite::get_debug_bounding_boxes);
	ClassDB::bind_method(D_METHOD("set_debug_bounding_boxes_color", "v"), &SpineSprite::set_debug_bounding_boxes_color);
	ClassDB::bind_method(D_METHOD("get_debug_bounding_boxes_color"), &SpineSprite::get_debug_bounding_boxes_color);
	ClassDB::bind_method(D_METHOD("set_debug_paths", "v"), &SpineSprite::set_debug_paths);
	ClassDB::bind_method(D_METHOD("get_debug_paths"), &SpineSprite::get_debug_paths);
	ClassDB::bind_method(D_METHOD("set_debug_paths_color", "v"), &SpineSprite::set_debug_paths_color);
	ClassDB::bind_method(D_METHOD("get_debug_paths_color"), &SpineSprite::get_debug_paths_color);
	ClassDB::bind_method(D_METHOD("set_debug_clipping", "v"), &SpineSprite::set_debug_clipping);
	ClassDB::bind_method(D_METHOD("get_debug_clipping"), &SpineSprite::get_debug_clipping);
	ClassDB::bind_method(D_METHOD("set_debug_clipping_color", "v"), &SpineSprite::set_debug_clipping_color);
	ClassDB::bind_method(D_METHOD("get_debug_clipping_color"), &SpineSprite::get_debug_clipping_color);

	ClassDB::bind_method(D_METHOD("update_skeleton", "delta"), &SpineSprite::update_skeleton);
	ClassDB::bind_method(D_METHOD("new_skin", "name"), &SpineSprite::new_skin);

	ADD_SIGNAL(MethodInfo("animation_started", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite"),
						  PropertyInfo(Variant::OBJECT, "animation_state", PROPERTY_HINT_TYPE_STRING, "SpineAnimationState"),
						  PropertyInfo(Variant::OBJECT, "track_entry", PROPERTY_HINT_TYPE_STRING, "SpineTrackEntry")));
	ADD_SIGNAL(MethodInfo("animation_interrupted", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite"),
						  PropertyInfo(Variant::OBJECT, "animation_state", PROPERTY_HINT_TYPE_STRING, "SpineAnimationState"),
						  PropertyInfo(Variant::OBJECT, "track_entry", PROPERTY_HINT_TYPE_STRING, "SpineTrackEntry")));
	ADD_SIGNAL(MethodInfo("animation_ended", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite"),
						  PropertyInfo(Variant::OBJECT, "animation_state", PROPERTY_HINT_TYPE_STRING, "SpineAnimationState"),
						  PropertyInfo(Variant::OBJECT, "track_entry", PROPERTY_HINT_TYPE_STRING, "SpineTrackEntry")));
	ADD_SIGNAL(MethodInfo("animation_completed", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite"),
						  PropertyInfo(Variant::OBJECT, "animation_state", PROPERTY_HINT_TYPE_STRING, "SpineAnimationState"),
						  PropertyInfo(Variant::OBJECT, "track_entry", PROPERTY_HINT_TYPE_STRING, "SpineTrackEntry")));
	ADD_SIGNAL(MethodInfo("animation_disposed", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite"),
						  PropertyInfo(Variant::OBJECT, "animation_state", PROPERTY_HINT_TYPE_STRING, "SpineAnimationState"),
						  PropertyInfo(Variant::OBJECT, "track_entry", PROPERTY_HINT_TYPE_STRING, "SpineTrackEntry")));
	ADD_SIGNAL(MethodInfo("animation_event", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite"),
						  PropertyInfo(Variant::OBJECT, "animation_state", PROPERTY_HINT_TYPE_STRING, "SpineAnimationState"),
						  PropertyInfo(Variant::OBJECT, "track_entry", PROPERTY_HINT_TYPE_STRING, "SpineTrackEntry"),
						  PropertyInfo(Variant::OBJECT, "event", PROPERTY_HINT_TYPE_STRING, "SpineEvent")));
	ADD_SIGNAL(MethodInfo("before_animation_state_update", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite")));
	ADD_SIGNAL(MethodInfo("before_animation_state_apply", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite")));
	ADD_SIGNAL(MethodInfo("before_world_transforms_change", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite")));
	ADD_SIGNAL(MethodInfo("world_transforms_changed", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite")));
	ADD_SIGNAL(MethodInfo("_internal_spine_objects_invalidated"));

	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "skeleton_data_res", PropertyHint::PROPERTY_HINT_RESOURCE_TYPE, "SpineSkeletonDataResource"),
				 "set_skeleton_data_res", "get_skeleton_data_res");
	ADD_PROPERTY(PropertyInfo(Variant::INT, "update_mode", PROPERTY_HINT_ENUM, "Process,Physics,Manual"), "set_update_mode", "get_update_mode");
	ADD_GROUP("Materials", "");
	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "normal_material", PROPERTY_HINT_RESOURCE_TYPE, "Material"), "set_normal_material",
				 "get_normal_material");
	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "additive_material", PROPERTY_HINT_RESOURCE_TYPE, "Material"), "set_additive_material",
				 "get_additive_material");
	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "multiply_material", PROPERTY_HINT_RESOURCE_TYPE, "Material"), "set_multiply_material",
				 "get_multiply_material");
	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "screen_material", PROPERTY_HINT_RESOURCE_TYPE, "Material"), "set_screen_material",
				 "get_screen_material");

	ADD_GROUP("Debug", "");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "root"), "set_debug_root", "get_debug_root");
	ADD_PROPERTY(PropertyInfo(Variant::COLOR, "root_color"), "set_debug_root_color", "get_debug_root_color");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "bones"), "set_debug_bones", "get_debug_bones");
	ADD_PROPERTY(PropertyInfo(Variant::COLOR, "bones_color"), "set_debug_bones_color", "get_debug_bones_color");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "bones_thickness"), "set_debug_bones_thickness", "get_debug_bones_thickness");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "regions"), "set_debug_regions", "get_debug_regions");
	ADD_PROPERTY(PropertyInfo(Variant::COLOR, "regions_color"), "set_debug_regions_color", "get_debug_regions_color");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "meshes"), "set_debug_meshes", "get_debug_meshes");
	ADD_PROPERTY(PropertyInfo(Variant::COLOR, "meshes_color"), "set_debug_meshes_color", "get_debug_meshes_color");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "bounding_boxes"), "set_debug_bounding_boxes", "get_debug_bounding_boxes");
	ADD_PROPERTY(PropertyInfo(Variant::COLOR, "bounding_boxes_color"), "set_debug_bounding_boxes_color", "get_debug_bounding_boxes_color");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "paths"), "set_debug_paths", "get_debug_paths");
	ADD_PROPERTY(PropertyInfo(Variant::COLOR, "paths_color"), "set_debug_paths_color", "get_debug_paths_color");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "clipping"), "set_debug_clipping", "get_debug_clipping");
	ADD_PROPERTY(PropertyInfo(Variant::COLOR, "paths_clipping"), "set_debug_clipping_color", "get_debug_clipping_color");

	ADD_GROUP("Preview", "");
	// Filled in in _get_property_list()
}

SpineSprite::SpineSprite()
	: update_mode(SpineConstant::UpdateMode_Process), time_scale(1.0), preview_skin("Default"), preview_animation("-- Empty --"),
	  preview_frame(false), preview_time(0), skeleton_clipper(nullptr) {
	controller = Ref<SpineController>(memnew(SpineController));
	controller->set_listener(this);
	skeleton_clipper = new spine::SkeletonClipping();
	auto statics = SpineSpriteStatics::instance();

	// Default debug settings
	debug_root = false;
	debug_root_color = Color(1, 1, 1, 0.5);
	debug_bones = false;
	debug_bones_color = Color(1, 1, 0, 0.5);
	debug_bones_thickness = 5;
	debug_regions = false;
	debug_regions_color = Color(0, 0, 1, 0.5);
	debug_meshes = false;
	debug_meshes_color = Color(0, 0, 1, 0.5);
	debug_bounding_boxes = false;
	debug_bounding_boxes_color = Color(0, 1, 0, 0.5);
	debug_paths = false;
	debug_paths_color = Color::hex(0xff7f0077);
	debug_clipping = false;
	debug_clipping_color = Color(0.8, 0, 0, 0.8);

	statics.sprite_count++;
}

SpineSprite::~SpineSprite() {
	controller->set_listener(nullptr);
	controller.unref();
	delete skeleton_clipper;
	auto statics = SpineSpriteStatics::instance();
	statics.sprite_count--;
	if (!statics.sprite_count) {
		for (int i = 0; i < 4; i++) statics.default_materials[i].unref();
	}
}

void SpineSprite::set_skeleton_data_res(const Ref<SpineSkeletonDataResource> &_skeleton_data) {
	if (connected_skeleton_data_res.is_valid()) {
#if VERSION_MAJOR > 3
		if (connected_skeleton_data_res->is_connected(SNAME("skeleton_data_changed"), callable_mp(this, &SpineSprite::on_skeleton_data_changed)))
			connected_skeleton_data_res->disconnect(SNAME("skeleton_data_changed"), callable_mp(this, &SpineSprite::on_skeleton_data_changed));
#else
		if (connected_skeleton_data_res->is_connected(SNAME("skeleton_data_changed"), this, SNAME("on_skeleton_data_changed")))
			connected_skeleton_data_res->disconnect(SNAME("skeleton_data_changed"), this, SNAME("on_skeleton_data_changed"));
#endif
	}
	connected_skeleton_data_res = _skeleton_data;
	if (connected_skeleton_data_res.is_valid()) {
#if VERSION_MAJOR > 3
		connected_skeleton_data_res->connect(SNAME("skeleton_data_changed"), callable_mp(this, &SpineSprite::on_skeleton_data_changed));
#else
		connected_skeleton_data_res->connect(SNAME("skeleton_data_changed"), this, SNAME("on_skeleton_data_changed"));
#endif
	}
	controller->set_skeleton_data_res(_skeleton_data);
}

Ref<SpineSkeletonDataResource> SpineSprite::get_skeleton_data_res() {
	return controller->get_skeleton_data_res();
}

void SpineSprite::on_skeleton_data_changed() {
	controller->set_skeleton_data_res(connected_skeleton_data_res);
}

void SpineSprite::before_skeleton_data_change() {
	remove_meshes();
	emit_signal(SNAME("_internal_spine_objects_invalidated"));
}

void SpineSprite::skeleton_data_changed() {
	Ref<SpineSkeleton> skeleton = controller->get_skeleton();
	if (skeleton.is_valid()) {
		generate_meshes_for_slots(skeleton);

		if (update_mode == SpineConstant::UpdateMode_Process) {
			_notification(NOTIFICATION_INTERNAL_PROCESS);
		} else if (update_mode == SpineConstant::UpdateMode_Physics) {
			_notification(NOTIFICATION_INTERNAL_PHYSICS_PROCESS);
		}
	}
	NOTIFY_PROPERTY_LIST_CHANGED();
}

bool SpineSprite::should_apply_pose() {
	return is_visible_in_tree();
}

void SpineSprite::generate_meshes_for_slots(Ref<SpineSkeleton> skeleton_ref) {
	auto skeleton = skeleton_ref->get_spine_object();
	auto statics = SpineSpriteStatics::instance();
	for (int i = 0, n = (int) skeleton->getSlots().size(); i < n; i++) {
		auto mesh_instance = memnew(SpineMesh2D);
		mesh_instance->set_position(Vector2(0, 0));
		mesh_instance->set_material(statics.default_materials[spine::BlendMode_Normal]);
		// Needed so that debug drawables are rendered in front of attachments
		mesh_instance->set_draw_behind_parent(true);
		add_child(mesh_instance);
		mesh_instances.push_back(mesh_instance);
		slot_nodes.add(spine::Array<SpineSlotNode *>());
	}
}

void SpineSprite::remove_meshes() {
	for (int i = 0; i < mesh_instances.size(); ++i) {
		remove_child(mesh_instances[i]);
		memdelete(mesh_instances[i]);
	}
	mesh_instances.clear();
	slot_nodes.clear();
}

void SpineSprite::sort_slot_nodes() {
	for (int i = 0; i < (int) slot_nodes.size(); i++) {
		slot_nodes[i].setSize(0, nullptr);
	}

	auto &draw_order = controller->get_skeleton()->get_spine_object()->getDrawOrder().getAppliedPose();
	for (int i = 0; i < get_child_count(); i++) {
		auto child = cast_to<Node2D>(get_child(i));
		if (!child) continue;
		// Needed so that debug drawables are rendered in front of attachments and other nodes under the sprite.
		child->set_draw_behind_parent(true);
		auto slot_node = Object::cast_to<SpineSlotNode>(get_child(i));
		if (!slot_node) continue;
		if (slot_node->get_slot_index() == -1 || slot_node->get_slot_index() >= (int) draw_order.size()) {
			continue;
		}
		slot_nodes[slot_node->get_slot_index()].add(slot_node);
	}

	for (int i = 0; i < (int) draw_order.size(); i++) {
		int slot_index = draw_order[i]->getData().getIndex();
		int mesh_index = mesh_instances[i]->get_index();
		spine::Array<SpineSlotNode *> &nodes = slot_nodes[slot_index];
		for (int j = 0; j < (int) nodes.size(); j++) {
			auto node = nodes[j];
			move_child(node, mesh_index + 1);
		}
	}
}

Ref<SpineSkeleton> SpineSprite::get_skeleton() {
	return controller->get_skeleton();
}

Ref<SpineAnimationState> SpineSprite::get_animation_state() {
	return controller->get_animation_state();
}

void SpineSprite::_notification(int what) {
	switch (what) {
		case NOTIFICATION_READY: {
			set_process_internal(update_mode == SpineConstant::UpdateMode_Process);
			set_physics_process_internal(update_mode == SpineConstant::UpdateMode_Physics);
			break;
		}
		case NOTIFICATION_INTERNAL_PROCESS: {
			if (update_mode == SpineConstant::UpdateMode_Process) update_skeleton(get_process_delta_time());
			break;
		}
		case NOTIFICATION_INTERNAL_PHYSICS_PROCESS: {
			if (update_mode == SpineConstant::UpdateMode_Physics) update_skeleton(get_physics_process_delta_time());
			break;
		}
		case NOTIFICATION_DRAW: {
			draw();
			break;
		}
		default:
			break;
	}
}

void SpineSprite::_get_property_list(List<PropertyInfo> *list) const {
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

	PropertyInfo preview_skin_property;
	preview_skin_property.name = "preview_skin";
	preview_skin_property.type = Variant::STRING;
	preview_skin_property.usage = PROPERTY_USAGE_EDITOR | PROPERTY_USAGE_STORAGE;
	preview_skin_property.hint_string = String(",").join(skin_names);
	preview_skin_property.hint = PROPERTY_HINT_ENUM;
	list->push_back(preview_skin_property);

	PropertyInfo preview_anim_property;
	preview_anim_property.name = "preview_animation";
	preview_anim_property.type = Variant::STRING;
	preview_anim_property.usage = PROPERTY_USAGE_EDITOR | PROPERTY_USAGE_STORAGE;
	preview_anim_property.hint_string = String(",").join(animation_names);
	preview_anim_property.hint = PROPERTY_HINT_ENUM;
	list->push_back(preview_anim_property);

	PropertyInfo preview_frame_property;
	preview_frame_property.name = "preview_frame";
	preview_frame_property.type = Variant::BOOL;
	preview_frame_property.usage = PROPERTY_USAGE_EDITOR | PROPERTY_USAGE_STORAGE;
	list->push_back(preview_frame_property);

	PropertyInfo preview_time_property;
	preview_time_property.name = "preview_time";
	preview_time_property.type = VARIANT_FLOAT;
	preview_time_property.usage = PROPERTY_USAGE_EDITOR | PROPERTY_USAGE_STORAGE;
	float animation_duration = 0;
	if (!EMPTY(preview_animation) && preview_animation != "-- Empty --") {
		auto animation = skeleton_data_res->find_animation(preview_animation);
		if (animation.is_valid()) animation_duration = animation->get_duration();
	}
#ifdef SPINE_GODOT_EXTENSION
	preview_time_property.hint_string = String("0.0,") + String::num(animation_duration) + String(",0.01");
#else
	preview_time_property.hint_string = String("0.0,{0},0.01").format(varray(animation_duration));
#endif
	preview_time_property.hint = PROPERTY_HINT_RANGE;
	list->push_back(preview_time_property);
}

bool SpineSprite::_get(const StringName &property, Variant &value) const {
	if (property == StringName("preview_skin")) {
		value = preview_skin;
		return true;
	}

	if (property == StringName("preview_animation")) {
		value = preview_animation;
		return true;
	}

	if (property == StringName("preview_frame")) {
		value = preview_frame;
		return true;
	}

	if (property == StringName("preview_time")) {
		value = preview_time;
		return true;
	}
	return false;
}

static void update_preview_animation(SpineSprite *sprite, const String &skin, const String &animation, bool frame, float time) {
	if (!Engine::get_singleton()->is_editor_hint()) return;
	if (!sprite->get_skeleton().is_valid()) return;

	if (EMPTY(skin) || skin == "Default") {
		sprite->get_skeleton()->set_skin(nullptr);
	} else {
		sprite->get_skeleton()->set_skin_by_name(skin);
	}
	sprite->get_skeleton()->set_to_setup_pose();
	if (EMPTY(animation) || animation == "-- Empty --") {
		sprite->get_animation_state()->set_empty_animation(0, 0);
		return;
	}

	auto track_entry = sprite->get_animation_state()->set_animation(animation, true, 0);
	if (track_entry.is_null()) {
		sprite->get_animation_state()->set_empty_animation(0, 0);
		return;
	}
	track_entry->set_mix_duration(0);
	if (frame) {
		track_entry->set_time_scale(0);
		track_entry->set_track_time(time);
	}
}

bool SpineSprite::_set(const StringName &property, const Variant &value) {
	if (property == StringName("preview_skin")) {
		preview_skin = value;
		update_preview_animation(this, preview_skin, preview_animation, preview_frame, preview_time);
		NOTIFY_PROPERTY_LIST_CHANGED();
		return true;
	}

	if (property == StringName("preview_animation")) {
		preview_animation = value;
		update_preview_animation(this, preview_skin, preview_animation, preview_frame, preview_time);
		NOTIFY_PROPERTY_LIST_CHANGED();
		return true;
	}

	if (property == StringName("preview_frame")) {
		preview_frame = value;
		update_preview_animation(this, preview_skin, preview_animation, preview_frame, preview_time);
		return true;
	}

	if (property == StringName("preview_time")) {
		preview_time = value;
		update_preview_animation(this, preview_skin, preview_animation, preview_frame, preview_time);
		return true;
	}

	return false;
}

void SpineSprite::update_skeleton(float delta) {
	Ref<SpineSkeletonDataResource> skeleton_data_res = controller->get_skeleton_data_res();
	if (!skeleton_data_res.is_valid() || !skeleton_data_res->is_skeleton_data_loaded()) return;
	if (!controller->update_skeleton(delta, time_scale)) return;
	Ref<SpineSkeleton> skeleton = controller->get_skeleton();
	controller->begin_native_operation();
	sort_slot_nodes();
	update_meshes(skeleton);
	if (controller->end_native_operation()) return;
#if VERSION_MAJOR > 3
	queue_redraw();
#else
	update();
#endif
}

void SpineSprite::update_meshes(Ref<SpineSkeleton> skeleton_ref) {
	auto statics = SpineSpriteStatics::instance();
	spine::Skeleton *skeleton = skeleton_ref->get_spine_object();
	for (int i = 0, n = (int) skeleton->getSlots().size(); i < n; ++i) {
		spine::Slot *slot = skeleton->getDrawOrder().getAppliedPose()[i];
		spine::Attachment *attachment = slot->getAppliedPose().getAttachment();
		SpineMesh2D *mesh_instance = mesh_instances[i];
		mesh_instance->renderer_object = nullptr;

		if (!attachment) {
			skeleton_clipper->clipEnd(*slot);
			continue;
		}
		if (!slot->getBone().isActive()) {
			skeleton_clipper->clipEnd(*slot);
			continue;
		}

		spine::Color skeleton_color = skeleton->getColor();
		spine::Color slot_color = slot->getAppliedPose().getColor();
#if VERSION_MAJOR > 3 && VERSION_MINOR >= 3
		mesh_instance->has_dark_color = slot->getAppliedPose().hasDarkColor();
		spine::Color slot_dark_color = slot->getAppliedPose().getDarkColor();
		Color dark_tint(skeleton_color.r * slot_dark_color.r, skeleton_color.g * slot_dark_color.g, skeleton_color.b * slot_dark_color.b);
#endif
		spine::Color tint(skeleton_color.r * slot_color.r, skeleton_color.g * slot_color.g, skeleton_color.b * slot_color.b,
						  skeleton_color.a * slot_color.a);
		SpineRendererObject *renderer_object;
		spine::Array<float> *vertices = &statics.scratch_vertices;
		spine::Array<float> *uvs;
		spine::Array<unsigned short> *indices;

		if (attachment->getRTTI().isExactly(spine::RegionAttachment::rtti)) {
			auto region = (spine::RegionAttachment *) attachment;
			auto &sequence = region->getSequence();
			int sequenceIndex = sequence.resolveIndex(slot->getAppliedPose());

			vertices->setSize(8, 0);
			region->computeWorldVertices(*slot, sequence.getOffsets(sequenceIndex).buffer(), vertices->buffer(), 0);
			renderer_object = (SpineRendererObject *) ((spine::AtlasRegion *) sequence.getRegion(sequenceIndex))->getPage()->texture;
			uvs = &sequence.getUVs(sequenceIndex);
			indices = &statics.quad_indices;

			auto &attachment_color = region->getColor();
			tint.r *= attachment_color.r;
			tint.g *= attachment_color.g;
			tint.b *= attachment_color.b;
			tint.a *= attachment_color.a;
#if VERSION_MAJOR > 3 && VERSION_MINOR >= 3
			dark_tint.r *= attachment_color.r;
			dark_tint.g *= attachment_color.g;
			dark_tint.b *= attachment_color.b;
#endif
		} else if (attachment->getRTTI().isExactly(spine::MeshAttachment::rtti)) {
			auto mesh = (spine::MeshAttachment *) attachment;
			auto &sequence = mesh->getSequence();
			int sequenceIndex = sequence.resolveIndex(slot->getAppliedPose());

			vertices->setSize(mesh->getWorldVerticesLength(), 0);
			mesh->computeWorldVertices(*skeleton, *slot, 0, mesh->getWorldVerticesLength(), vertices->buffer(), 0, 2);

			renderer_object = (SpineRendererObject *) ((spine::AtlasRegion *) sequence.getRegion(sequenceIndex))->getPage()->texture;
			uvs = &sequence.getUVs(sequenceIndex);
			indices = &mesh->getTriangles();

			auto &attachment_color = mesh->getColor();
			tint.r *= attachment_color.r;
			tint.g *= attachment_color.g;
			tint.b *= attachment_color.b;
			tint.a *= attachment_color.a;
#if VERSION_MAJOR > 3 && VERSION_MINOR >= 3
			dark_tint.r *= attachment_color.r;
			dark_tint.g *= attachment_color.g;
			dark_tint.b *= attachment_color.b;
#endif
		} else if (attachment->getRTTI().isExactly(spine::ClippingAttachment::rtti)) {
			auto clip = (spine::ClippingAttachment *) attachment;
			skeleton_clipper->clipStart(*skeleton, *slot, clip);
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
			mesh_instance->set_light_mask(get_light_mask());
			size_t num_vertices = vertices->size() / 2;
			mesh_instance->vertices.resize((int) num_vertices);
			memcpy(mesh_instance->vertices.ptrw(), vertices->buffer(), num_vertices * 2 * sizeof(float));
			mesh_instance->uvs.resize((int) num_vertices);
			memcpy(mesh_instance->uvs.ptrw(), uvs->buffer(), num_vertices * 2 * sizeof(float));
			auto indices_changed = false;
			if (mesh_instance->indices.size() == indices->size()) {
				auto old_indices = mesh_instance->indices.ptr();
				auto new_indices = indices->buffer();
				for (int j = 0; j < (int) indices->size(); j++) {
					if (old_indices[j] != new_indices[j]) {
						indices_changed = true;
						break;
					}
				}
			} else {
				indices_changed = true;
			}

			if (indices_changed) {
				mesh_instance->indices.resize((int) indices->size());
				for (int j = 0; j < (int) indices->size(); ++j) {
					mesh_instance->indices.set(j, indices->buffer()[j]);
				}
				mesh_instance->indices_changed = true;
			}

			mesh_instance->renderer_object = renderer_object;

			spine::BlendMode blend_mode = slot->getData().getBlendMode();
			Ref<Material> custom_material;

			// See if we have a slot node for this slot with a custom material
			auto &nodes = slot_nodes[slot->getData().getIndex()];
			if (nodes.size() > 0) {
				auto slot_node = nodes[0];
				if (slot_node) {
					switch (blend_mode) {
						case spine::BlendMode_Normal:
							custom_material = slot_node->get_normal_material();
							break;
						case spine::BlendMode_Additive:
							custom_material = slot_node->get_additive_material();
							break;
						case spine::BlendMode_Multiply:
							custom_material = slot_node->get_multiply_material();
							break;
						case spine::BlendMode_Screen:
							custom_material = slot_node->get_screen_material();
							break;
					}
				}
			}

			// Else, check if we have a material on the sprite itself
			if (!custom_material.is_valid()) {
				switch (blend_mode) {
					case spine::BlendMode_Normal:
						custom_material = normal_material;
						break;
					case spine::BlendMode_Additive:
						custom_material = additive_material;
						break;
					case spine::BlendMode_Multiply:
						custom_material = multiply_material;
						break;
					case spine::BlendMode_Screen:
						custom_material = screen_material;
						break;
				}
			}

			Color vertex_color(tint.r, tint.g, tint.b, tint.a);
#if VERSION_MAJOR > 3 && VERSION_MINOR >= 3
			mesh_instance->light_color = vertex_color;
			mesh_instance->dark_color = dark_tint;
			if (mesh_instance->has_dark_color && !custom_material.is_valid()) {
				// Godot modulates COLOR before vertex(). Keep Spine's colors separate so modulation also affects
				// dark tint when a light color channel is zero. Custom materials retain their existing COLOR input.
				vertex_color = Color(1, 1, 1, 1);
				custom_material = statics.tint_black_materials[blend_mode];
			}
#endif
			if (custom_material.is_valid())
				mesh_instance->set_material(custom_material);
			else
				mesh_instance->set_material(statics.default_materials[blend_mode]);

			mesh_instance->colors.resize((int) num_vertices);
			for (int j = 0; j < (int) num_vertices; j++) {
				mesh_instance->colors.set(j, vertex_color);
			}
		}
		skeleton_clipper->clipEnd(*slot);
	}
	skeleton_clipper->clipEnd();
}

#ifdef SPINE_GODOT_EXTENSION
void createLinesFromMesh(PackedVector2Array &scratch_points, spine::Array<unsigned short> &triangles, spine::Array<float> *vertices) {
#else
void createLinesFromMesh(Vector<Vector2> &scratch_points, spine::Array<unsigned short> &triangles, spine::Array<float> *vertices) {
#endif
	scratch_points.resize(0);
	for (int i = 0; i < triangles.size(); i += 3) {
		int i1 = triangles[i];
		int i2 = triangles[i + 1];
		int i3 = triangles[i + 2];
		Vector2 v1(vertices->buffer()[i1 * 2], vertices->buffer()[i1 * 2 + 1]);
		Vector2 v2(vertices->buffer()[i2 * 2], vertices->buffer()[i2 * 2 + 1]);
		Vector2 v3(vertices->buffer()[i3 * 2], vertices->buffer()[i3 * 2 + 1]);
		scratch_points.push_back(v1);
		scratch_points.push_back(v2);
		scratch_points.push_back(v2);
		scratch_points.push_back(v3);
		scratch_points.push_back(v3);
		scratch_points.push_back(v1);
	}
}

void SpineSprite::draw() {
	Ref<SpineSkeleton> skeleton = controller->get_skeleton();
	if (!skeleton.is_valid()) return;
	if (!Engine::get_singleton()->is_editor_hint() && !get_tree()->is_debugging_collisions_hint()) return;

	auto &statics = SpineSpriteStatics::instance();

#if VERSION_MAJOR > 3
	RS::get_singleton()->canvas_item_clear(this->get_canvas_item());
#else
	VisualServer::get_singleton()->canvas_item_clear(this->get_canvas_item());
#endif

	auto mouse_position = get_local_mouse_position();
	spine::Slot *hovered_slot = nullptr;

	if (debug_regions) {
		draw_set_transform(Vector2(0, 0), 0, Vector2(1, 1));
		auto &draw_order = skeleton->get_spine_object()->getDrawOrder().getAppliedPose();
		for (int i = 0; i < (int) draw_order.size(); i++) {
			auto slot = draw_order[i];
			if (!slot->getBone().isActive()) continue;
			auto attachment = slot->getAppliedPose().getAttachment();
			if (!attachment) continue;
			if (!attachment->getRTTI().isExactly(spine::RegionAttachment::rtti)) continue;
			auto region = (spine::RegionAttachment *) attachment;
			auto &sequence = region->getSequence();
			int sequenceIndex = sequence.resolveIndex(slot->getAppliedPose());
			auto vertices = &statics.scratch_vertices;
			vertices->setSize(8, 0);
			region->computeWorldVertices(*slot, sequence.getOffsets(sequenceIndex).buffer(), vertices->buffer(), 0);

			// Render triangles.
			createLinesFromMesh(statics.scratch_points, statics.quad_indices, vertices);
			draw_polyline(statics.scratch_points, debug_regions_color);

			// Render hull.
			statics.scratch_points.resize(0);
			for (int i = 0, j = 0; i < 4; i++, j += 2) {
				float x = vertices->buffer()[j];
				float y = vertices->buffer()[j + 1];
				statics.scratch_points.push_back(Vector2(x, y));
			}
			statics.scratch_points.push_back(Vector2(vertices->buffer()[0], vertices->buffer()[1]));

			Color color = debug_regions_color;
#ifdef SPINE_GODOT_EXTENSION
			if (GEOMETRY2D::get_singleton()->is_point_in_polygon(mouse_position, statics.scratch_points)) {
#else
			if (GEOMETRY2D::is_point_in_polygon(mouse_position, statics.scratch_points)) {
#endif
				hovered_slot = slot;
				color = Color(1, 1, 1, 1);
			}
			statics.scratch_points.push_back(Vector2(vertices->buffer()[0], vertices->buffer()[1]));
			draw_polyline(statics.scratch_points, color, 2);
		}
	}

	if (debug_meshes) {
		draw_set_transform(Vector2(0, 0), 0, Vector2(1, 1));
		auto &draw_order = skeleton->get_spine_object()->getDrawOrder().getAppliedPose();
		for (int i = 0; i < (int) draw_order.size(); i++) {
			auto slot = draw_order[i];
			if (!slot->getBone().isActive()) continue;
			auto attachment = slot->getAppliedPose().getAttachment();
			if (!attachment) continue;
			if (!attachment->getRTTI().isExactly(spine::MeshAttachment::rtti)) continue;
			auto mesh = (spine::MeshAttachment *) attachment;
			auto vertices = &statics.scratch_vertices;
			vertices->setSize(mesh->getWorldVerticesLength(), 0);
			mesh->computeWorldVertices(*skeleton->get_spine_object(), *slot, 0, mesh->getWorldVerticesLength(), vertices->buffer(), 0, 2);

			// Render triangles.
			createLinesFromMesh(statics.scratch_points, mesh->getTriangles(), vertices);
			draw_polyline(statics.scratch_points, debug_meshes_color);

			// Render hull
			statics.scratch_points.resize(0);
			for (int i = 0, j = 0; i < mesh->getHullLength(); i++, j += 2) {
				float x = vertices->buffer()[j];
				float y = vertices->buffer()[j + 1];
				statics.scratch_points.push_back(Vector2(x, y));
			}

			Color color = debug_meshes_color;
#ifdef SPINE_GODOT_EXTENSION
			if (GEOMETRY2D::get_singleton()->is_point_in_polygon(mouse_position, statics.scratch_points)) {
#else
			if (GEOMETRY2D::is_point_in_polygon(mouse_position, statics.scratch_points)) {
#endif
				hovered_slot = slot;
				color = Color(1, 1, 1, 1);
			}
			statics.scratch_points.push_back(Vector2(vertices->buffer()[0], vertices->buffer()[1]));
			draw_polyline(statics.scratch_points, color, 2);
		}
	}

	if (debug_bounding_boxes) {
		draw_set_transform(Vector2(0, 0), 0, Vector2(1, 1));
		auto &draw_order = skeleton->get_spine_object()->getDrawOrder().getAppliedPose();
		for (int i = 0; i < (int) draw_order.size(); i++) {
			auto slot = draw_order[i];
			if (!slot->getBone().isActive()) continue;
			auto attachment = slot->getAppliedPose().getAttachment();
			if (!attachment) continue;
			if (!attachment->getRTTI().isExactly(spine::BoundingBoxAttachment::rtti)) continue;
			auto bounding_box = (spine::BoundingBoxAttachment *) attachment;
			auto vertices = &statics.scratch_vertices;
			vertices->setSize(bounding_box->getWorldVerticesLength(), 0);
			bounding_box->computeWorldVertices(*skeleton->get_spine_object(), *slot, 0, bounding_box->getWorldVerticesLength(), vertices->buffer(), 0,
											   2);
			size_t num_vertices = vertices->size() / 2;
			statics.scratch_points.resize((int) num_vertices);
			memcpy(statics.scratch_points.ptrw(), vertices->buffer(), num_vertices * 2 * sizeof(float));
			statics.scratch_points.push_back(Vector2(vertices->buffer()[0], vertices->buffer()[1]));
			draw_polyline(statics.scratch_points, debug_bounding_boxes_color, 2);
		}
	}

	if (debug_clipping) {
		draw_set_transform(Vector2(0, 0), 0, Vector2(1, 1));
		auto &draw_order = skeleton->get_spine_object()->getDrawOrder().getAppliedPose();
		for (int i = 0; i < (int) draw_order.size(); i++) {
			auto slot = draw_order[i];
			if (!slot->getBone().isActive()) continue;
			auto attachment = slot->getAppliedPose().getAttachment();
			if (!attachment) continue;
			if (!attachment->getRTTI().isExactly(spine::ClippingAttachment::rtti)) continue;
			auto clipping = (spine::ClippingAttachment *) attachment;
			auto vertices = &statics.scratch_vertices;
			vertices->setSize(clipping->getWorldVerticesLength(), 0);
			clipping->computeWorldVertices(*skeleton->get_spine_object(), *slot, 0, clipping->getWorldVerticesLength(), vertices->buffer(), 0, 2);
			size_t num_vertices = vertices->size() / 2;
			statics.scratch_points.resize((int) num_vertices);
			memcpy(statics.scratch_points.ptrw(), vertices->buffer(), num_vertices * 2 * sizeof(float));
			statics.scratch_points.push_back(Vector2(vertices->buffer()[0], vertices->buffer()[1]));
			draw_polyline(statics.scratch_points, debug_clipping_color, 2);
		}
	}


	spine::Bone *hovered_bone = nullptr;
	if (debug_root) {
		auto bone = skeleton->get_spine_object()->getRootBone();
		draw_bone(bone, debug_root_color);

		float bone_length = bone->getData().getLength();
		if (bone_length == 0) bone_length = debug_bones_thickness * 2;

		statics.scratch_points.resize(5);
		statics.scratch_points.set(0, Vector2(-debug_bones_thickness, 0));
		statics.scratch_points.set(1, Vector2(0, debug_bones_thickness));
		statics.scratch_points.set(2, Vector2(bone_length, 0));
		statics.scratch_points.set(3, Vector2(0, -debug_bones_thickness));
		statics.scratch_points.set(4, Vector2(-debug_bones_thickness, 0));
		Transform2D bone_transform(spine::MathUtil::Deg_Rad * bone->getAppliedPose().getWorldRotationX(),
								   Vector2(bone->getAppliedPose().getWorldX(), bone->getAppliedPose().getWorldY()));
		bone_transform.scale_basis(Vector2(bone->getAppliedPose().getWorldScaleX(), bone->getAppliedPose().getWorldScaleY()));
		auto mouse_local_position = bone_transform.affine_inverse().xform(mouse_position);
#ifdef SPINE_GODOT_EXTENSION
		if (GEOMETRY2D::get_singleton()->is_point_in_polygon(mouse_local_position, statics.scratch_points)) {
#else
		if (GEOMETRY2D::is_point_in_polygon(mouse_local_position, statics.scratch_points)) {
#endif
			hovered_bone = bone;
		}
	}

	if (debug_bones) {
		auto &bones = skeleton->get_spine_object()->getBones();
		for (int i = 0; i < (int) bones.size(); i++) {
			auto bone = bones[i];
			if (!bone->isActive()) continue;
			draw_bone(bone, debug_bones_color);

			float bone_length = bone->getData().getLength();
			if (bone_length == 0) bone_length = debug_bones_thickness * 2;

			statics.scratch_points.resize(5);
			statics.scratch_points.set(0, Vector2(-debug_bones_thickness, 0));
			statics.scratch_points.set(1, Vector2(0, debug_bones_thickness));
			statics.scratch_points.set(2, Vector2(bone_length, 0));
			statics.scratch_points.set(3, Vector2(0, -debug_bones_thickness));
			statics.scratch_points.set(4, Vector2(-debug_bones_thickness, 0));
			Transform2D bone_transform(spine::MathUtil::Deg_Rad * bone->getAppliedPose().getWorldRotationX(),
									   Vector2(bone->getAppliedPose().getWorldX(), bone->getAppliedPose().getWorldY()));
			bone_transform.scale_basis(Vector2(bone->getAppliedPose().getWorldScaleX(), bone->getAppliedPose().getWorldScaleY()));
			auto mouse_local_position = bone_transform.affine_inverse().xform(mouse_position);
#ifdef SPINE_GODOT_EXTENSION
			if (GEOMETRY2D::get_singleton()->is_point_in_polygon(mouse_local_position, statics.scratch_points)) {
#else
			if (GEOMETRY2D::is_point_in_polygon(mouse_local_position, statics.scratch_points)) {
#endif
				hovered_bone = bone;
			}
		}
	}

#if TOOLS_ENABLED
	float editor_scale = 1.0;
	if (Engine::get_singleton()->is_editor_hint()) editor_scale = EditorInterface::get_singleton()->get_editor_scale();

	float inverse_zoom = 1 / get_viewport()->get_global_canvas_transform().get_scale().x * editor_scale;
	Vector<String> hover_text_lines;
	if (hovered_slot) {
		String name;
#if (VERSION_MAJOR >= 4 && VERSION_MINOR >= 5)
		name = String::utf8(hovered_slot->getData().getName().buffer());
#else
		name.parse_utf8(hovered_slot->getData().getName().buffer());
#endif
		hover_text_lines.push_back(String("Slot: ") + name);
	}

	if (hovered_bone) {
		float thickness = debug_bones_thickness;
		debug_bones_thickness *= 1.1;
		draw_bone(hovered_bone, Color(debug_bones_color.r, debug_bones_color.g, debug_bones_color.b, 1));
		debug_bones_thickness = thickness;
		String name;
#if (VERSION_MAJOR >= 4 && VERSION_MINOR >= 5)
		name = String::utf8(hovered_bone->getData().getName().buffer());
#else
		name.parse_utf8(hovered_bone->getData().getName().buffer());
#endif
		hover_text_lines.push_back(String("Bone: ") + name);
	}

	auto global_scale = get_global_scale();
	draw_set_transform(mouse_position + Vector2(20, 0), -get_global_rotation(),
					   Vector2(inverse_zoom * (1 / global_scale.x), inverse_zoom * (1 / global_scale.y)));

	if (!debug_font.is_valid()) {
		auto control = memnew(Control);
#if VERSION_MAJOR > 3
		debug_font = control->get_theme_default_font();
#else
		debug_font = control->get_font(SNAME("font"), SNAME("Label"));
#endif
		memdelete(control);
	}
	const Ref<Font> &default_font = debug_font;

#if VERSION_MAJOR > 3
#ifdef SPINE_GODOT_EXTENSION
	// FIXME possibly wrong
	float line_height = default_font->get_height() + default_font->get_descent();
#else
	float line_height = default_font->get_height(Font::DEFAULT_FONT_SIZE) + default_font->get_descent(Font::DEFAULT_FONT_SIZE);
#endif
#else
	float line_height = default_font->get_height() + default_font->get_descent();
#endif
	float rect_width = 0;
	for (int i = 0; i < hover_text_lines.size(); i++) {
		rect_width = MAX(rect_width, default_font->get_string_size(hover_text_lines[i]).x);
	}

#if VERSION_MAJOR > 3
#ifdef SPINE_GODOT_EXTENSION
	Rect2 background_rect(0, -default_font->get_height() - 5, rect_width + 20, line_height * hover_text_lines.size() + 10);
#else
	Rect2 background_rect(0, -default_font->get_height(Font::DEFAULT_FONT_SIZE) - 5, rect_width + 20, line_height * hover_text_lines.size() + 10);
#endif
#else
	Rect2 background_rect(0, -default_font->get_height() - 5, rect_width + 20, line_height * hover_text_lines.size() + 10);
#endif
	if (hover_text_lines.size() > 0) draw_rect(background_rect, Color(0, 0, 0, 0.8));
	for (int i = 0; i < hover_text_lines.size(); i++) {
#if VERSION_MAJOR > 3
#ifdef SPINE_GODOT_EXTENSION
		draw_string(default_font, Vector2(10, 0 + i * default_font->get_height()), hover_text_lines[i], HORIZONTAL_ALIGNMENT_LEFT, -1, 16,
					Color(1, 1, 1, 1));
#else
		draw_string(default_font, Vector2(10, 0 + i * default_font->get_height(Font::DEFAULT_FONT_SIZE)), hover_text_lines[i],
					HORIZONTAL_ALIGNMENT_LEFT, -1, Font::DEFAULT_FONT_SIZE, Color(1, 1, 1, 1));
#endif
#else
		draw_string(default_font, Vector2(10, 0 + i * default_font->get_height()), hover_text_lines[i], Color(1, 1, 1, 1));
#endif
	}
#endif
}

void SpineSprite::draw_bone(spine::Bone *bone, const Color &color) {
	draw_set_transform(Vector2(bone->getAppliedPose().getWorldX(), bone->getAppliedPose().getWorldY()),
					   spine::MathUtil::Deg_Rad * bone->getAppliedPose().getWorldRotationX(),
					   Vector2(bone->getAppliedPose().getWorldScaleX(), bone->getAppliedPose().getWorldScaleY()));
	float bone_length = bone->getData().getLength();
	if (bone_length == 0) bone_length = debug_bones_thickness * 2;
#ifdef SPINE_GODOT_EXTENSION
	PackedVector2Array points;
#else
	Vector<Vector2> points;
#endif
	points.push_back(Vector2(-debug_bones_thickness, 0));
	points.push_back(Vector2(0, debug_bones_thickness));
	points.push_back(Vector2(bone_length, 0));
	points.push_back(Vector2(0, -debug_bones_thickness));
	draw_colored_polygon(points, color);
}

void SpineSprite::on_animation_started(Ref<SpineAnimationState> animation_state, Ref<SpineTrackEntry> track_entry) {
	emit_signal(SNAME("animation_started"), this, animation_state, track_entry);
}

void SpineSprite::on_animation_interrupted(Ref<SpineAnimationState> animation_state, Ref<SpineTrackEntry> track_entry) {
	emit_signal(SNAME("animation_interrupted"), this, animation_state, track_entry);
}

void SpineSprite::on_animation_ended(Ref<SpineAnimationState> animation_state, Ref<SpineTrackEntry> track_entry) {
	emit_signal(SNAME("animation_ended"), this, animation_state, track_entry);
}

void SpineSprite::on_animation_completed(Ref<SpineAnimationState> animation_state, Ref<SpineTrackEntry> track_entry) {
	emit_signal(SNAME("animation_completed"), this, animation_state, track_entry);
}

void SpineSprite::on_animation_disposed(Ref<SpineAnimationState> animation_state, Ref<SpineTrackEntry> track_entry) {
	emit_signal(SNAME("animation_disposed"), this, animation_state, track_entry);
}

void SpineSprite::on_animation_event(Ref<SpineAnimationState> animation_state, Ref<SpineTrackEntry> track_entry, Ref<SpineEvent> event) {
	emit_signal(SNAME("animation_event"), this, animation_state, track_entry, event);
}

void SpineSprite::before_animation_state_update() {
	emit_signal(SNAME("before_animation_state_update"), this);
}

void SpineSprite::before_animation_state_apply() {
	emit_signal(SNAME("before_animation_state_apply"), this);
}

void SpineSprite::before_world_transforms_change() {
	emit_signal(SNAME("before_world_transforms_change"), this);
}

void SpineSprite::world_transforms_changed() {
	emit_signal(SNAME("world_transforms_changed"), this);
}

Transform2D SpineSprite::get_global_bone_transform(const String &bone_name) {
	Ref<SpineSkeleton> skeleton = controller->get_skeleton();
	if (!skeleton.is_valid()) return get_global_transform();
	auto bone = skeleton->find_bone(bone_name);
	if (!bone.is_valid()) return get_global_transform();
	return get_global_bone_transform(bone->get_spine_object());
}

Transform2D SpineSprite::get_global_bone_transform(spine::Bone *bone) {
	if (!bone) return get_global_transform();
	if (!is_visible_in_tree()) {
		float rotation_x = spine::MathUtil::Deg_Rad * (bone->getPose().getRotation() + bone->getPose().getShearX());
		float rotation_y = spine::MathUtil::Deg_Rad * (bone->getPose().getRotation() + 90 + bone->getPose().getShearY());
		Transform2D transform;
		transform[0] = Vector2(spine::MathUtil::cos(rotation_x) * bone->getPose().getScaleX(),
							   spine::MathUtil::sin(rotation_x) * bone->getPose().getScaleX());
		transform[1] = Vector2(spine::MathUtil::cos(rotation_y) * bone->getPose().getScaleY(),
							   spine::MathUtil::sin(rotation_y) * bone->getPose().getScaleY());
		transform[2] = Vector2(bone->getPose().getX(), bone->getPose().getY());
		return transform;
	}

	auto &applied_pose = bone->getAppliedPose();
	Transform2D local;
	local[0] = Vector2(applied_pose.getA(), applied_pose.getC());
	local[1] = Vector2(applied_pose.getB(), applied_pose.getD());
	// Bone::setYDown(true) is a runtime coordinate conversion for Godot. Don't expose it as an authored Node2D Y scale.
	if (spine::Bone::isYDown()) local[1] = Vector2(-local[1].x, -local[1].y);
	local[2] = Vector2(applied_pose.getWorldX(), applied_pose.getWorldY());
	return get_global_transform() * local;
}

void SpineSprite::set_global_bone_transform(const String &bone_name, Transform2D transform) {
	Ref<SpineSkeleton> skeleton = controller->get_skeleton();
	if (!skeleton.is_valid()) return;
	auto bone = skeleton->find_bone(bone_name);
	if (!bone.is_valid()) return;
	set_global_bone_transform(bone->get_spine_object(), transform);
}

void SpineSprite::set_global_bone_transform(spine::Bone *bone, Transform2D transform) {
	Ref<SpineSkeleton> skeleton = controller->get_skeleton();
	if (!skeleton.is_valid() || !bone || !is_visible_in_tree()) return;
	Transform2D local = get_global_transform().affine_inverse() * transform;
	if (spine::Bone::isYDown()) local[1] = Vector2(-local[1].x, -local[1].y);
	auto &applied_pose = bone->getAppliedPose();
	applied_pose.setA(local[0].x);
	applied_pose.setC(local[0].y);
	applied_pose.setB(local[1].x);
	applied_pose.setD(local[1].y);
	applied_pose.setWorldX(local.get_origin().x);
	applied_pose.setWorldY(local.get_origin().y);
	applied_pose.updateLocalTransform(*skeleton->get_spine_object());
	bone->getPose().set(applied_pose);
	controller->set_modified_bones();
}

SpineConstant::UpdateMode SpineSprite::get_update_mode() {
	return update_mode;
}

void SpineSprite::set_update_mode(SpineConstant::UpdateMode v) {
	update_mode = v;
	set_process_internal(update_mode == SpineConstant::UpdateMode_Process);
	set_physics_process_internal(update_mode == SpineConstant::UpdateMode_Physics);
}

Ref<SpineSkin> SpineSprite::new_skin(const String &name) {
	return controller->new_skin(name);
}

Ref<Material> SpineSprite::get_normal_material() {
	return normal_material;
}

void SpineSprite::set_normal_material(Ref<Material> material) {
	normal_material = material;
}

Ref<Material> SpineSprite::get_additive_material() {
	return additive_material;
}

void SpineSprite::set_additive_material(Ref<Material> material) {
	additive_material = material;
}

Ref<Material> SpineSprite::get_multiply_material() {
	return multiply_material;
}

void SpineSprite::set_multiply_material(Ref<Material> material) {
	multiply_material = material;
}

Ref<Material> SpineSprite::get_screen_material() {
	return screen_material;
}

void SpineSprite::set_screen_material(Ref<Material> material) {
	screen_material = material;
}

void SpineSprite::set_time_scale(float time_scale) {
	this->time_scale = time_scale;
}

float SpineSprite::get_time_scale() {
	return time_scale;
}

#ifndef SPINE_GODOT_EXTENSION
// FIXME
#ifdef TOOLS_ENABLED
Rect2 SpineSprite::_edit_get_rect() const {
	Ref<SpineSkeletonDataResource> skeleton_data_res = controller->get_skeleton_data_res();
	if (skeleton_data_res.is_valid() && skeleton_data_res->is_skeleton_data_loaded()) {
		auto data = skeleton_data_res->get_skeleton_data();
		return Rect2(data->getX(), -data->getY() - data->getHeight(), data->getWidth(), data->getHeight());
	}
	return Node2D::_edit_get_rect();
}

bool SpineSprite::_edit_use_rect() const {
	Ref<SpineSkeletonDataResource> skeleton_data_res = controller->get_skeleton_data_res();
	return skeleton_data_res.is_valid() && skeleton_data_res->is_skeleton_data_loaded();
}
#endif
#endif
