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

#include "SpineSlotRangeProxy.h"
#include "SpineSlotData.h"

#ifdef SPINE_GODOT_EXTENSION
#include <godot_cpp/core/memory.hpp>
#else
#include "core/os/memory.h"
#endif

void SpineSlotRangeProxy::_bind_methods() {
	ClassDB::bind_method(D_METHOD("set_source_sprite", "node_path"), &SpineSlotRangeProxy::set_source_sprite);
	ClassDB::bind_method(D_METHOD("get_source_sprite"), &SpineSlotRangeProxy::get_source_sprite);
	ClassDB::bind_method(D_METHOD("set_follow_source_transform", "enabled"), &SpineSlotRangeProxy::set_follow_source_transform);
	ClassDB::bind_method(D_METHOD("get_follow_source_transform"), &SpineSlotRangeProxy::get_follow_source_transform);
	ClassDB::bind_method(D_METHOD("set_start_slot_name", "slot_name"), &SpineSlotRangeProxy::set_start_slot_name);
	ClassDB::bind_method(D_METHOD("get_start_slot_name"), &SpineSlotRangeProxy::get_start_slot_name);
	ClassDB::bind_method(D_METHOD("set_end_slot_name", "slot_name"), &SpineSlotRangeProxy::set_end_slot_name);
	ClassDB::bind_method(D_METHOD("get_end_slot_name"), &SpineSlotRangeProxy::get_end_slot_name);

	ADD_PROPERTY(PropertyInfo(Variant::NODE_PATH, "source_sprite", PROPERTY_HINT_NODE_PATH_VALID_TYPES, "SpineSprite"), "set_source_sprite",
				 "get_source_sprite");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "follow_source_transform"), "set_follow_source_transform", "get_follow_source_transform");
	// start_slot_name and end_slot_name are added by _get_property_list() because
	// their enum hint depends on the slot names of the resolved source sprite.
}

SpineSlotRangeProxy::SpineSlotRangeProxy()
	: follow_source_transform(true), registered_source(nullptr), registered_start_index(-1), registered_end_index(-1) {
}

void SpineSlotRangeProxy::_notification(int what) {
	switch (what) {
		case NOTIFICATION_ENTER_TREE: {
			set_process_internal(true);
			break;
		}
		case NOTIFICATION_INTERNAL_PROCESS: {
			update_proxy();
			break;
		}
		case NOTIFICATION_EXIT_TREE: {
			unregister_from_source();
			hide_proxy_meshes();
			break;
		}
		default:
			break;
	}
}

void SpineSlotRangeProxy::_get_property_list(List<PropertyInfo> *list) const {
#ifdef SPINE_GODOT_EXTENSION
	PackedStringArray slot_names;
#else
	Vector<String> slot_names;
#endif
	SpineSprite *sprite = resolve_source_sprite();
	if (sprite && sprite->get_skeleton_data_res().is_valid())
		sprite->get_skeleton_data_res()->get_slot_names(slot_names);

	// Leading comma yields an empty first entry: an empty start/end name means
	// "first slot" / "last slot" respectively.
	String hint_string = String(",") + String(",").join(slot_names);

	PropertyInfo start_property;
	start_property.name = "start_slot_name";
	start_property.type = Variant::STRING;
	start_property.hint = PROPERTY_HINT_ENUM;
	start_property.hint_string = hint_string;
	start_property.usage = PROPERTY_USAGE_DEFAULT;
	list->push_back(start_property);

	PropertyInfo end_property;
	end_property.name = "end_slot_name";
	end_property.type = Variant::STRING;
	end_property.hint = PROPERTY_HINT_ENUM;
	end_property.hint_string = hint_string;
	end_property.usage = PROPERTY_USAGE_DEFAULT;
	list->push_back(end_property);
}

bool SpineSlotRangeProxy::_get(const StringName &property, Variant &value) const {
	if (property == StringName("start_slot_name")) {
		value = start_slot_name;
		return true;
	}
	if (property == StringName("end_slot_name")) {
		value = end_slot_name;
		return true;
	}
	return false;
}

bool SpineSlotRangeProxy::_set(const StringName &property, const Variant &value) {
	if (property == StringName("start_slot_name")) {
		start_slot_name = value;
		return true;
	}
	if (property == StringName("end_slot_name")) {
		end_slot_name = value;
		return true;
	}
	return false;
}

SpineSprite *SpineSlotRangeProxy::resolve_source_sprite() const {
	if (source_sprite_path.is_empty()) return nullptr;
	if (!is_inside_tree()) return nullptr;
	Node *node = get_node_or_null(source_sprite_path);
	return Object::cast_to<SpineSprite>(node);
}

bool SpineSlotRangeProxy::resolve_slot_range(SpineSprite *sprite, int &start_index, int &end_index) const {
	if (!sprite) return false;
	int slot_count = sprite->get_draw_order_count();
	if (slot_count <= 0) return false;
	Ref<SpineSkeleton> skeleton = sprite->get_skeleton();
	if (!skeleton.is_valid()) return false;

	if (start_slot_name.is_empty()) {
		start_index = 0;
	} else {
		Ref<SpineSlot> slot = skeleton->find_slot(start_slot_name);
		if (!slot.is_valid()) return false;
		start_index = slot->get_data()->get_index();
	}

	if (end_slot_name.is_empty()) {
		end_index = slot_count - 1;
	} else {
		Ref<SpineSlot> slot = skeleton->find_slot(end_slot_name);
		if (!slot.is_valid()) return false;
		end_index = slot->get_data()->get_index();
	}

	if (start_index < 0 || end_index >= slot_count) return false;
	if (start_index > end_index) {
		WARN_PRINT("SpineSlotRangeProxy: start_slot_name comes after end_slot_name in slot order; the range is empty.");
		return false;
	}
	return true;
}

void SpineSlotRangeProxy::register_with_source(SpineSprite *sprite, int start_index, int end_index) {
	if (!sprite) return;
	sprite->_register_proxy(this, start_index, end_index);
	registered_source = sprite;
	registered_start_index = start_index;
	registered_end_index = end_index;
	Callable callable = callable_mp(this, &SpineSlotRangeProxy::_on_source_tree_exiting);
	if (!sprite->is_connected(SNAME("tree_exiting"), callable))
		sprite->connect(SNAME("tree_exiting"), callable, CONNECT_ONE_SHOT);
}

void SpineSlotRangeProxy::unregister_from_source() {
	if (registered_source) {
		Callable callable = callable_mp(this, &SpineSlotRangeProxy::_on_source_tree_exiting);
		if (registered_source->is_connected(SNAME("tree_exiting"), callable))
			registered_source->disconnect(SNAME("tree_exiting"), callable);
		registered_source->_unregister_proxy(this);
		registered_source = nullptr;
	}
	registered_start_index = -1;
	registered_end_index = -1;
}

void SpineSlotRangeProxy::_on_source_tree_exiting() {
	// The source is leaving the tree; it is still valid here. The one-shot
	// connection has already been removed, so it must not be disconnected again.
	if (registered_source) registered_source->_unregister_proxy(this);
	registered_source = nullptr;
	registered_start_index = -1;
	registered_end_index = -1;
}

void SpineSlotRangeProxy::clear_proxy_meshes() {
	for (int i = 0; i < proxy_meshes.size(); i++) {
		remove_child(proxy_meshes[i]);
		memdelete(proxy_meshes[i]);
	}
	proxy_meshes.clear();
}

void SpineSlotRangeProxy::rebuild_proxy_meshes(int count) {
	clear_proxy_meshes();
	for (int i = 0; i < count; i++) {
		SpineMesh2D *mesh_instance = memnew(SpineMesh2D);
		mesh_instance->set_position(Vector2(0, 0));
		add_child(mesh_instance);
		proxy_meshes.push_back(mesh_instance);
	}
}

void SpineSlotRangeProxy::hide_proxy_meshes() {
	for (int i = 0; i < proxy_meshes.size(); i++) {
		proxy_meshes[i]->renderer_object = nullptr;
		proxy_meshes[i]->set_visible(false);
	}
}

void SpineSlotRangeProxy::update_proxy() {
	SpineSprite *sprite = resolve_source_sprite();
	if (!sprite) {
		unregister_from_source();
		hide_proxy_meshes();
		return;
	}

	int start_index = 0, end_index = 0;
	if (!resolve_slot_range(sprite, start_index, end_index)) {
		unregister_from_source();
		hide_proxy_meshes();
		return;
	}

	if (registered_source != sprite || registered_start_index != start_index || registered_end_index != end_index) {
		unregister_from_source();
		register_with_source(sprite, start_index, end_index);
		rebuild_proxy_meshes(end_index - start_index + 1);
	}

	if (follow_source_transform) set_global_transform(sprite->get_global_transform());

	// Mirror the geometry the source computed for this frame. The source must
	// process before this proxy for the data to be current; see the header.
	Vector<SpineMesh2D *> source_meshes;
	sprite->collect_slot_range_meshes(start_index, end_index, source_meshes);

	for (int i = 0; i < proxy_meshes.size(); i++) {
		SpineMesh2D *destination = proxy_meshes[i];
		if (i < source_meshes.size()) {
			SpineMesh2D *source = source_meshes[i];
			destination->vertices = source->vertices;
			destination->uvs = source->uvs;
			destination->colors = source->colors;
			destination->indices = source->indices;
			destination->renderer_object = source->renderer_object;
			// The mesh mirrors a different source slot whenever draw order changes,
			// so the index buffer must be treated as changed every frame.
			destination->indices_changed = true;
			destination->set_material(source->get_material());
			destination->set_light_mask(source->get_light_mask());
			destination->set_visible(true);
		} else {
			destination->renderer_object = nullptr;
			destination->set_visible(false);
		}
	}
}

void SpineSlotRangeProxy::set_source_sprite(const NodePath &path) {
	if (source_sprite_path == path) return;
	unregister_from_source();
	source_sprite_path = path;
}

NodePath SpineSlotRangeProxy::get_source_sprite() const {
	return source_sprite_path;
}

void SpineSlotRangeProxy::set_follow_source_transform(bool enabled) {
	follow_source_transform = enabled;
}

bool SpineSlotRangeProxy::get_follow_source_transform() const {
	return follow_source_transform;
}

void SpineSlotRangeProxy::set_start_slot_name(const String &name) {
	start_slot_name = name;
}

String SpineSlotRangeProxy::get_start_slot_name() const {
	return start_slot_name;
}

void SpineSlotRangeProxy::set_end_slot_name(const String &name) {
	end_slot_name = name;
}

String SpineSlotRangeProxy::get_end_slot_name() const {
	return end_slot_name;
}
