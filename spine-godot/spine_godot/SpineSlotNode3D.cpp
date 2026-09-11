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

#include "SpineSlotNode3D.h"
#if VERSION_MAJOR > 3
#include "SpineSprite3D.h"
#ifdef SPINE_GODOT_EXTENSION
#include <godot_cpp/classes/camera3d.hpp>
#include <godot_cpp/classes/world3d.hpp>
#include <godot_cpp/classes/viewport.hpp>
#include <godot_cpp/classes/geometry_instance3d.hpp>
#include <godot_cpp/classes/mesh_instance3d.hpp>
#include <godot_cpp/classes/scene_tree.hpp>
#else
#include "scene/3d/camera_3d.h"
#include "scene/3d/mesh_instance_3d.h"
#if __has_include("scene/resources/3d/world_3d.h")
#include "scene/resources/3d/world_3d.h"
#else
#include "scene/resources/world_3d.h"
#endif
#include "scene/main/scene_tree.h"
#include "scene/main/viewport.h"
#endif
#include <spine/Slot.h>
#include <spine/Bone.h>
#include <algorithm>

void SpineSlotNode3D::_bind_methods() {
	ClassDB::bind_method(D_METHOD("set_slot_name", "name"), &SpineSlotNode3D::set_slot_name);
	ClassDB::bind_method(D_METHOD("get_slot_name"), &SpineSlotNode3D::get_slot_name);
	ClassDB::bind_method(D_METHOD("get_slot_index"), &SpineSlotNode3D::get_slot_index);
	ClassDB::bind_method(D_METHOD("set_depth_camera", "path"), &SpineSlotNode3D::set_depth_camera);
	ClassDB::bind_method(D_METHOD("get_depth_camera"), &SpineSlotNode3D::get_depth_camera);
	ClassDB::bind_method(D_METHOD("set_normal_material", "material"), &SpineSlotNode3D::set_normal_material);
	ClassDB::bind_method(D_METHOD("get_normal_material"), &SpineSlotNode3D::get_normal_material);
	ClassDB::bind_method(D_METHOD("set_additive_material", "material"), &SpineSlotNode3D::set_additive_material);
	ClassDB::bind_method(D_METHOD("get_additive_material"), &SpineSlotNode3D::get_additive_material);
	ClassDB::bind_method(D_METHOD("get_sorting_warnings"), &SpineSlotNode3D::get_sorting_warnings);
	ADD_PROPERTY(PropertyInfo(Variant::STRING, "slot_name"), "set_slot_name", "get_slot_name");
	ADD_PROPERTY(PropertyInfo(Variant::NODE_PATH, "depth_camera", PROPERTY_HINT_NODE_PATH_VALID_TYPES, "Camera3D"), "set_depth_camera", "get_depth_camera");
	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "normal_material", PROPERTY_HINT_RESOURCE_TYPE, "ShaderMaterial"), "set_normal_material",
				 "get_normal_material");
	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "additive_material", PROPERTY_HINT_RESOURCE_TYPE, "ShaderMaterial"), "set_additive_material",
				 "get_additive_material");
}

SpineSlotNode3D::~SpineSlotNode3D() {
	release_sorting();
}

void SpineSlotNode3D::_notification(int what) {
	switch (what) {
		case NOTIFICATION_ENTER_TREE:
			get_tree()->connect(SNAME("tree_changed"), callable_mp(this, &SpineSlotNode3D::tree_changed));
			geometries_dirty = true;
			update_depth_connection();
			break;
		case NOTIFICATION_EXIT_TREE:
			get_tree()->disconnect(SNAME("tree_changed"), callable_mp(this, &SpineSlotNode3D::tree_changed));
			if (RS::get_singleton()->is_connected(SNAME("frame_pre_draw"), callable_mp(this, &SpineSlotNode3D::update_depth)))
				RS::get_singleton()->disconnect(SNAME("frame_pre_draw"), callable_mp(this, &SpineSlotNode3D::update_depth));
			release_sorting();
			break;
		case NOTIFICATION_UNPARENTED:
			release_sorting();
			if (slot_index >= 0) {
				Vector3 position = get_position();
				position.z = physical_depth;
				set_position(position);
			}
			slot_index = -1;
			break;
		default:
			break;
	}
}

void SpineSlotNode3D::_validate_property(PropertyInfo &property) const {
	if (property.name != SNAME("slot_name")) return;
	auto sprite = Object::cast_to<SpineSprite3D>(get_parent());
	if (!sprite || sprite->get_skeleton_data_res().is_null()) return;
#ifdef SPINE_GODOT_EXTENSION
	PackedStringArray names;
#else
	Vector<String> names;
#endif
	sprite->get_skeleton_data_res()->get_slot_names(names);
	property.hint = PROPERTY_HINT_ENUM;
	property.hint_string = String(",").join(names);
}

void SpineSlotNode3D::tree_changed() {
	// Unrelated scene changes must not release existing insertion ownership.
	update_geometry_list();
	if (!depth_camera.is_empty()) {
		update_configuration_warnings();
		update_depth();
	}
}

void SpineSlotNode3D::release_sorting() {
	geometry_generation++;
	for (const auto &state : saved_sorting) {
		auto geometry = Object::cast_to<GeometryInstance3D>(ObjectDB::get_instance(state.id));
		if (!geometry) continue;
		geometry->set_sorting_offset(state.offset);
		geometry->set_sorting_use_aabb_center(state.use_aabb_center);
		RS::get_singleton()->instance_set_visible(geometry->get_instance(), geometry->is_visible_in_tree());
	}
	saved_sorting.clear();
	geometries.clear();
	geometries_dirty = true;
}

static void collect_geometry(Node *node, std::vector<ObjectID> &result, bool attached_only = false) {
	for (int i = 0; i < node->get_child_count(); i++) {
		auto child = node->get_child(i);
		// On tree exit Godot clears the viewport before emitting tree_changed,
		// but the departing node is still in its parent's child list.
		if (attached_only && !child->get_viewport()) continue;
		if (Object::cast_to<SpineSprite3D>(child) || Object::cast_to<SpineSlotNode3D>(child)) continue;
		if (Object::cast_to<GeometryInstance3D>(child)) result.push_back(ObjectID(child->get_instance_id()));
		collect_geometry(child, result, attached_only);
	}
}

void SpineSlotNode3D::update_geometry_list() {
	std::vector<ObjectID> current;
	collect_geometry(this, current, is_inside_tree());
	if (current == geometries) return;
	for (auto state = saved_sorting.begin(); state != saved_sorting.end();) {
		if (std::find(current.begin(), current.end(), state->id) != current.end()) {
			++state;
			continue;
		}
		// Restore only departing geometry, before another helper can acquire it.
		auto geometry = Object::cast_to<GeometryInstance3D>(ObjectDB::get_instance(state->id));
		if (geometry) {
			geometry->set_sorting_offset(state->offset);
			geometry->set_sorting_use_aabb_center(state->use_aabb_center);
			RS::get_singleton()->instance_set_visible(geometry->get_instance(), geometry->is_visible_in_tree());
		}
		state = saved_sorting.erase(state);
	}
	geometries.swap(current);
	geometry_generation++;
	geometries_dirty = true;
}

const std::vector<ObjectID> &SpineSlotNode3D::get_geometries() {
	if (geometries_dirty) {
		update_geometry_list();
		for (ObjectID id : geometries) {
			if (std::find_if(saved_sorting.begin(), saved_sorting.end(), [id](const SortingState &state) { return state.id == id; }) != saved_sorting.end()) continue;
			auto geometry = Object::cast_to<GeometryInstance3D>(ObjectDB::get_instance(id));
			if (geometry) saved_sorting.push_back({id, geometry->get_sorting_offset(), geometry->is_sorting_use_aabb_center()});
		}
		geometries_dirty = false;
	}
	for (ObjectID id : geometries) {
		auto geometry = Object::cast_to<GeometryInstance3D>(ObjectDB::get_instance(id));
		if (geometry) RS::get_singleton()->instance_set_visible(geometry->get_instance(), active && geometry->is_visible_in_tree());
	}
	return geometries;
}

void SpineSlotNode3D::sync_slot(spine::Slot *slot, float pixel_size, float depth) {
	active = slot->getBone().isActive();
	if (!active) return;
	auto &pose = slot->getBone().getAppliedPose();
	Transform3D transform;
	transform.basis.set_column(0, Vector3(pose.getA(), -pose.getC(), 0));
	transform.basis.set_column(1, Vector3(pose.getB(), -pose.getD(), 0));
	transform.basis.set_column(2, Vector3(0, 0, 1));
	physical_depth = depth;
	transform.origin = Vector3(pose.getWorldX() * pixel_size, -pose.getWorldY() * pixel_size, depth * get_depth_sign());
	if (get_transform() != transform) set_transform(transform);
}

void SpineSlotNode3D::update_depth_connection() {
	if (!is_inside_tree()) return;
	bool connected = RS::get_singleton()->is_connected(SNAME("frame_pre_draw"), callable_mp(this, &SpineSlotNode3D::update_depth));
	if (!depth_camera.is_empty() && !connected)
		RS::get_singleton()->connect(SNAME("frame_pre_draw"), callable_mp(this, &SpineSlotNode3D::update_depth));
	else if (depth_camera.is_empty() && connected)
		RS::get_singleton()->disconnect(SNAME("frame_pre_draw"), callable_mp(this, &SpineSlotNode3D::update_depth));
}

float SpineSlotNode3D::get_depth_sign() const {
	auto sprite = Object::cast_to<SpineSprite3D>(get_parent());
	if (!is_inside_tree() || depth_camera.is_empty() || !sprite || !sprite->get_camera_relative_depth()) return 1;
	auto camera = Object::cast_to<Camera3D>(get_node_or_null(depth_camera));
	// tree_changed can run between a Node3D's world exit and tree exit. Resolve
	// the world through its viewport, rather than calling get_world_3d then.
	if (!camera || !camera->is_inside_tree() || !camera->get_viewport() || !sprite->get_viewport() ||
		camera->get_viewport()->find_world_3d() != sprite->get_viewport()->find_world_3d()) return 1;
	return sprite->to_local(camera->get_global_transform().origin).z < 0 ? -1 : 1;
}

void SpineSlotNode3D::update_depth() {
	if (!is_inside_tree() || slot_index < 0) return;
	Vector3 position = get_position();
	float depth = physical_depth * get_depth_sign();
	if (position.z == depth) return;
	position.z = depth;
	auto sprite = Object::cast_to<SpineSprite3D>(get_parent());
	ObjectID sprite_id = sprite ? ObjectID(sprite->get_instance_id()) : ObjectID();
	set_position(position);
	// Transform notifications can replace resources or detach nodes.
	sprite = Object::cast_to<SpineSprite3D>(ObjectDB::get_instance(sprite_id));
	if (sprite && !sprite->updating_meshes) sprite->update_sorting();
}

void SpineSlotNode3D::set_depth_camera(const NodePath &path) {
	depth_camera = path;
	update_depth_connection();
	update_configuration_warnings();
	update_depth();
}

NodePath SpineSlotNode3D::get_depth_camera() const {
	return depth_camera;
}

void SpineSlotNode3D::set_slot_name(const String &name) {
	slot_name = name;
	slot_index = -1;
	release_sorting();
	auto sprite = Object::cast_to<SpineSprite3D>(get_parent());
	if (sprite) sprite->slots_dirty = true;
	update_configuration_warnings();
}
String SpineSlotNode3D::get_slot_name() const {
	return slot_name;
}
int SpineSlotNode3D::get_slot_index() const {
	return slot_index;
}
void SpineSlotNode3D::set_normal_material(const Ref<ShaderMaterial> &material) {
	normal_material = material;
	auto sprite = Object::cast_to<SpineSprite3D>(get_parent());
	if (sprite) sprite->refresh_material_template(material);
}
Ref<ShaderMaterial> SpineSlotNode3D::get_normal_material() const {
	return normal_material;
}
void SpineSlotNode3D::set_additive_material(const Ref<ShaderMaterial> &material) {
	additive_material = material;
	auto sprite = Object::cast_to<SpineSprite3D>(get_parent());
	if (sprite) sprite->refresh_material_template(material);
}
Ref<ShaderMaterial> SpineSlotNode3D::get_additive_material() const {
	return additive_material;
}

static bool priority_matches(Ref<Material> material, int priority) {
	std::vector<Material *> visited;
	while (material.is_valid()) {
		if (material->get_render_priority() != priority) return false;
		for (auto previous : visited)
			if (previous == material.ptr()) return false;
		visited.push_back(material.ptr());
		material = material->get_next_pass();
	}
	return true;
}

static bool mesh_priorities_match(const Ref<Mesh> &mesh, int priority) {
	if (mesh.is_null()) return true;
	for (int i = 0; i < mesh->get_surface_count(); i++) {
		Ref<Material> material = mesh->surface_get_material(i);
		if (material.is_null() && priority != 0) return false;
		if (!priority_matches(material, priority)) return false;
	}
	return true;
}

PackedStringArray SpineSlotNode3D::get_sorting_warnings() const {
	PackedStringArray warnings;
	auto sprite = Object::cast_to<SpineSprite3D>(get_parent());
	if (!sprite) {
		warnings.push_back("SpineSlotNode3D must be a direct child of SpineSprite3D.");
		return warnings;
	}
	if (sprite->get_skeleton().is_valid() && !sprite->get_skeleton()->find_slot(slot_name).is_valid())
		warnings.push_back("The selected Spine slot does not exist.");
	if (!depth_camera.is_empty() && is_inside_tree()) {
		auto camera = Object::cast_to<Camera3D>(get_node_or_null(depth_camera));
		if (!camera || !camera->is_inside_tree() || !camera->get_viewport() || !sprite->get_viewport() ||
			camera->get_viewport()->find_world_3d() != sprite->get_viewport()->find_world_3d())
			warnings.push_back("depth_camera must reference a Camera3D in the same World3D; using fixed physical depth instead.");
		warnings.push_back("depth_camera moves this node and all descendants for ONE camera, including physics objects. It cannot follow opposite views simultaneously.");
	}
	std::vector<ObjectID> descendants;
	collect_geometry((Node *) this, descendants);
	for (ObjectID id : descendants) {
		auto geometry = Object::cast_to<GeometryInstance3D>(ObjectDB::get_instance(id));
		bool valid = priority_matches(geometry->get_material_override(), sprite->get_render_priority()) &&
			priority_matches(geometry->get_material_overlay(), sprite->get_render_priority());
		auto mesh = Object::cast_to<MeshInstance3D>(geometry);
		if (mesh)
			for (int i = 0; i < mesh->get_surface_override_material_count(); i++) {
				Ref<Material> material = mesh->get_active_material(i);
				if (material.is_null() && sprite->get_render_priority() != 0) valid = false;
				if (!priority_matches(material, sprite->get_render_priority())) valid = false;
			}
		bool inspected = mesh || geometry->get_material_override().is_valid();
		if (!inspected && geometry->has_method(SNAME("get_draw_pass_mesh"))) {
			int passes = geometry->call(SNAME("get_draw_passes"));
			for (int i = 0; i < passes; i++)
				valid &= mesh_priorities_match(geometry->call(SNAME("get_draw_pass_mesh"), i), sprite->get_render_priority());
			inspected = true;
		} else if (!inspected && geometry->has_method(SNAME("get_multimesh"))) {
			Variant value = geometry->call(SNAME("get_multimesh"));
			Object *multimesh = value;
			if (multimesh) valid &= mesh_priorities_match(multimesh->call(SNAME("get_mesh")), sprite->get_render_priority());
			inspected = true;
		} else if (!inspected && geometry->has_method(SNAME("get_mesh"))) {
			valid &= mesh_priorities_match(geometry->call(SNAME("get_mesh")), sprite->get_render_priority());
			inspected = true;
		}
		if (geometry->has_method(SNAME("get_render_priority"))) {
			inspected = true;
			if ((int) geometry->call(SNAME("get_render_priority")) != sprite->get_render_priority()) valid = false;
		}
		if (!inspected)
			warnings.push_back(
				vformat("%s: cannot inspect this renderable's passes; verify every transparent material uses character render_priority.",
						geometry->get_name()));
		if (geometry->has_method(SNAME("get_outline_render_priority")) &&
			(int) geometry->call(SNAME("get_outline_render_priority")) != sprite->get_render_priority())
			valid = false;
		if (!valid)
			warnings.push_back(vformat("%s: every inserted transparent surface/pass must use character render_priority %d.", geometry->get_name(),
									   sprite->get_render_priority()));
	}
	return warnings;
}

#ifdef SPINE_GODOT_EXTENSION
PackedStringArray SpineSlotNode3D::_get_configuration_warnings() const {
#else
PackedStringArray SpineSlotNode3D::get_configuration_warnings() const {
#endif
	return get_sorting_warnings();
}
#endif
