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

#include "SpineSlotNode.h"

#ifdef TOOLS_ENABLED
#include "editor/editor_node.h"
#endif
#include "scene/main/viewport.h"

void SpineSlotNode::_bind_methods() {
	ClassDB::bind_method(D_METHOD("_on_world_transforms_changed", "spine_sprite"), &SpineSlotNode::on_world_transforms_changed);

	ClassDB::bind_method(D_METHOD("set_normal_material", "material"), &SpineSlotNode::set_normal_material);
	ClassDB::bind_method(D_METHOD("get_normal_material"), &SpineSlotNode::get_normal_material);
	ClassDB::bind_method(D_METHOD("set_additive_material", "material"), &SpineSlotNode::set_additive_material);
	ClassDB::bind_method(D_METHOD("get_additive_material"), &SpineSlotNode::get_additive_material);
	ClassDB::bind_method(D_METHOD("set_multiply_material", "material"), &SpineSlotNode::set_multiply_material);
	ClassDB::bind_method(D_METHOD("get_multiply_material"), &SpineSlotNode::get_multiply_material);
	ClassDB::bind_method(D_METHOD("set_screen_material", "material"), &SpineSlotNode::set_screen_material);
	ClassDB::bind_method(D_METHOD("get_screen_material"), &SpineSlotNode::get_screen_material);

	ADD_GROUP("Materials", "");
	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "normal_material", PROPERTY_HINT_RESOURCE_TYPE, "Material"), "set_normal_material", "get_normal_material");
	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "additive_material", PROPERTY_HINT_RESOURCE_TYPE, "Material"), "set_additive_material", "get_additive_material");
	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "multiply_material", PROPERTY_HINT_RESOURCE_TYPE, "Material"), "set_multiply_material", "get_multiply_material");
	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "screen_material", PROPERTY_HINT_RESOURCE_TYPE, "Material"), "set_screen_material", "get_screen_material");
}

SpineSlotNode::SpineSlotNode() : slot_index(-1) {
}

void SpineSlotNode::_notification(int what) {
	switch (what) {
		case NOTIFICATION_PARENTED: {
			SpineSprite *sprite = cast_to<SpineSprite>(get_parent());
			if (sprite) {
#if VERSION_MAJOR > 3
				sprite->connect(SNAME("world_transforms_changed"), callable_mp(this, &SpineSlotNode::on_world_transforms_changed));
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
				WARN_PRINT("SpineSlotNode parent is not a SpineSprite.");
			}
			NOTIFY_PROPERTY_LIST_CHANGED();
			break;
		}
		case NOTIFICATION_UNPARENTED: {
			SpineSprite *sprite = cast_to<SpineSprite>(get_parent());
			if (sprite) {
#if VERSION_MAJOR > 3
				sprite->disconnect(SNAME("world_transforms_changed"), callable_mp(this, &SpineSlotNode::on_world_transforms_changed));
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

void SpineSlotNode::_get_property_list(List<PropertyInfo> *list) const {
	Vector<String> slot_names;
	SpineSprite *sprite = cast_to<SpineSprite>(get_parent());
	if (sprite && sprite->get_skeleton_data_res().is_valid()) sprite->get_skeleton_data_res()->get_slot_names(slot_names);
	else
		slot_names.push_back(slot_name);
	auto element = list->front();
	while (element) {
		auto property_info = element->get();
		if (property_info.name == "SpineSlotNode") break;
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

bool SpineSlotNode::_get(const StringName &property, Variant &value) const {
	if (property == "slot_name") {
		value = slot_name;
		return true;
	}
	return false;
}

bool SpineSlotNode::_set(const StringName &property, const Variant &value) {
	if (property == "slot_name") {
		slot_name = value;
		SpineSprite *sprite = cast_to<SpineSprite>(get_parent());
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

void SpineSlotNode::on_world_transforms_changed(const Variant &_sprite) {
	SpineSprite *sprite = cast_to<SpineSprite>(_sprite.operator Object *());
	update_transform(sprite);
}

void SpineSlotNode::update_transform(SpineSprite *sprite) {
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

void SpineSlotNode::set_slot_name(const String &_slot_name) {
	slot_name = _slot_name;
}

String SpineSlotNode::get_slot_name() {
	return slot_name;
}

Ref<Material> SpineSlotNode::get_normal_material() {
	return normal_material;
}

void SpineSlotNode::set_normal_material(Ref<Material> material) {
	normal_material = material;
}

Ref<Material> SpineSlotNode::get_additive_material() {
	return additive_material;
}

void SpineSlotNode::set_additive_material(Ref<Material> material) {
	additive_material = material;
}

Ref<Material> SpineSlotNode::get_multiply_material() {
	return multiply_material;
}

void SpineSlotNode::set_multiply_material(Ref<Material> material) {
	multiply_material = material;
}

Ref<Material> SpineSlotNode::get_screen_material() {
	return screen_material;
}

void SpineSlotNode::set_screen_material(Ref<Material> material) {
	screen_material = material;
}
