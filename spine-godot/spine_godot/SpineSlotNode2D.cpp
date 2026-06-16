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

#include "SpineSlotNode2D.h"

#ifdef TOOLS_ENABLED
#ifdef SPINE_GODOT_EXTENSION
// FIXME
#else
#include "editor/editor_node.h"
#endif
#endif
#ifdef SPINE_GODOT_EXTENSION
#include <godot_cpp/classes/viewport.hpp>
#else
#include "scene/main/viewport.h"
#endif

void SpineSlotNode2D::_bind_methods() {
	ClassDB::bind_method(D_METHOD("_on_world_transforms_changed", "spine_sprite"), &SpineSlotNode2D::on_world_transforms_changed);

	ClassDB::bind_method(D_METHOD("set_normal_material", "material"), &SpineSlotNode2D::set_normal_material);
	ClassDB::bind_method(D_METHOD("get_normal_material"), &SpineSlotNode2D::get_normal_material);
	ClassDB::bind_method(D_METHOD("set_additive_material", "material"), &SpineSlotNode2D::set_additive_material);
	ClassDB::bind_method(D_METHOD("get_additive_material"), &SpineSlotNode2D::get_additive_material);
	ClassDB::bind_method(D_METHOD("set_multiply_material", "material"), &SpineSlotNode2D::set_multiply_material);
	ClassDB::bind_method(D_METHOD("get_multiply_material"), &SpineSlotNode2D::get_multiply_material);
	ClassDB::bind_method(D_METHOD("set_screen_material", "material"), &SpineSlotNode2D::set_screen_material);
	ClassDB::bind_method(D_METHOD("get_screen_material"), &SpineSlotNode2D::get_screen_material);

	ADD_GROUP("Materials", "");
	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "normal_material", PROPERTY_HINT_RESOURCE_TYPE, "Material"), "set_normal_material",
				 "get_normal_material");
	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "additive_material", PROPERTY_HINT_RESOURCE_TYPE, "Material"), "set_additive_material",
				 "get_additive_material");
	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "multiply_material", PROPERTY_HINT_RESOURCE_TYPE, "Material"), "set_multiply_material",
				 "get_multiply_material");
	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "screen_material", PROPERTY_HINT_RESOURCE_TYPE, "Material"), "set_screen_material",
				 "get_screen_material");
}

SpineSlotNode2D::SpineSlotNode2D() : slot_index(-1) {
}

void SpineSlotNode2D::_notification(int what) {
	switch (what) {
		case NOTIFICATION_PARENTED: {
			SpineSprite2D *sprite = cast_to<SpineSprite2D>(get_parent());
			if (sprite) {
#if VERSION_MAJOR > 3
				sprite->connect(SNAME("world_transforms_changed"), callable_mp(this, &SpineSlotNode2D::on_world_transforms_changed));
#else
				sprite->connect(SNAME("world_transforms_changed"), this, SNAME("_on_world_transforms_changed"));
#endif
				update_transform(sprite);
#if VERSION_MAJOR == 3
				_change_notify("transform/translation");
				_change_notify("transform/rotation");
				_change_notify("transform/scale");
				_change_notify("translation");
				_change_notify("rotation");
				_change_notify("rotation_deg");
				_change_notify("scale");
#endif
			} else {
				WARN_PRINT("SpineSlotNode2D parent is not a SpineSprite2D.");
			}
			NOTIFY_PROPERTY_LIST_CHANGED();
			break;
		}
		case NOTIFICATION_UNPARENTED: {
			SpineSprite2D *sprite = cast_to<SpineSprite2D>(get_parent());
			if (sprite) {
#if VERSION_MAJOR > 3
				sprite->disconnect(SNAME("world_transforms_changed"), callable_mp(this, &SpineSlotNode2D::on_world_transforms_changed));
#else
				sprite->disconnect(SNAME("world_transforms_changed"), this, SNAME("_on_world_transforms_changed"));
#endif
			}
			break;
		}
		default:
			break;
	}
}

void SpineSlotNode2D::_get_property_list(List<PropertyInfo> *list) const {
#ifdef SPINE_GODOT_EXTENSION
	PackedStringArray slot_names;
#else
	Vector<String> slot_names;
#endif
	SpineSprite2D *sprite = cast_to<SpineSprite2D>(get_parent());
	if (sprite && sprite->get_skeleton_data_res().is_valid())
		sprite->get_skeleton_data_res()->get_slot_names(slot_names);
	else
		slot_names.push_back(slot_name);
	auto element = list->front();
	while (element) {
		auto property_info = element->get();
		if (property_info.name == StringName("SpineSlotNode2D")) break;
		element = element->next();
	}
	PropertyInfo slot_name_property;
	slot_name_property.name = "slot_name";
	slot_name_property.type = Variant::STRING;
	slot_name_property.hint_string = String(",").join(slot_names);
	slot_name_property.hint = PROPERTY_HINT_ENUM;
	slot_name_property.usage = PROPERTY_USAGE_DEFAULT;
	list->insert_after(element, slot_name_property);
}

bool SpineSlotNode2D::_get(const StringName &p_property, Variant &value) const {
	if (p_property == StringName("slot_name")) {
		value = slot_name;
		return true;
	}
	return false;
}

bool SpineSlotNode2D::_set(const StringName &p_property, const Variant &value) {
	if (p_property == StringName("slot_name")) {
		slot_name = value;
		SpineSprite2D *sprite = cast_to<SpineSprite2D>(get_parent());
		update_transform(sprite);
#if VERSION_MAJOR == 3
		_change_notify("transform/translation");
		_change_notify("transform/rotation");
		_change_notify("transform/scale");
		_change_notify("translation");
		_change_notify("rotation");
		_change_notify("rotation_deg");
		_change_notify("scale");
#endif
		return true;
	}
	return false;
}

void SpineSlotNode2D::on_world_transforms_changed(const Variant &_sprite) {
	SpineSprite2D *sprite = cast_to<SpineSprite2D>(_sprite.operator Object *());
	update_transform(sprite);
}

void SpineSlotNode2D::update_transform(SpineSprite2D *sprite) {
	if (!is_visible_in_tree()) return;
	if (!sprite) return;
	if (!sprite->get_skeleton().is_valid() || !sprite->get_skeleton()->get_spine_object()) return;
	auto slot = sprite->get_skeleton()->find_slot(slot_name);
	if (!slot.is_valid()) {
		slot_index = -1;
		return;
	} else {
		slot_index = slot->get_data()->get_index();
	}
	auto bone = slot->get_bone();
	if (!bone.is_valid()) return;
	this->set_global_transform(bone->get_global_transform());
}

void SpineSlotNode2D::set_slot_name(const String &_slot_name) {
	slot_name = _slot_name;
}

String SpineSlotNode2D::get_slot_name() {
	return slot_name;
}

Ref<Material> SpineSlotNode2D::get_normal_material() {
	return normal_material;
}

void SpineSlotNode2D::set_normal_material(Ref<Material> p_material) {
	normal_material = p_material;
}

Ref<Material> SpineSlotNode2D::get_additive_material() {
	return additive_material;
}

void SpineSlotNode2D::set_additive_material(Ref<Material> p_material) {
	additive_material = p_material;
}

Ref<Material> SpineSlotNode2D::get_multiply_material() {
	return multiply_material;
}

void SpineSlotNode2D::set_multiply_material(Ref<Material> p_material) {
	multiply_material = p_material;
}

Ref<Material> SpineSlotNode2D::get_screen_material() {
	return screen_material;
}

void SpineSlotNode2D::set_screen_material(Ref<Material> p_material) {
	screen_material = p_material;
}
