#include "SpineSlotNode.h"

void SpineSlotNode::_bind_methods() {
    ClassDB::bind_method(D_METHOD("set_slot_name"), &SpineSlotNode::set_slot_name);
    ClassDB::bind_method(D_METHOD("get_slot_name"), &SpineSlotNode::get_slot_name);
    ClassDB::bind_method(D_METHOD("_on_world_transforms_changed", "spine_sprite"), &SpineSlotNode::on_world_transforms_changed);
}

SpineSlotNode::SpineSlotNode(): sprite(nullptr) {
}

void SpineSlotNode::_notification(int what) {
    switch(what) {
    case NOTIFICATION_PARENTED: {
        sprite = Object::cast_to<SpineSprite>(get_parent());
        if (sprite) {
            sprite->connect("world_transforms_changed", this, "_on_world_transforms_changed");
        } else {
            WARN_PRINT("SpineSlotProxy parent is not a SpineSprite.");
        }
        NOTIFY_PROPERTY_LIST_CHANGED();
        break;
    }
    case NOTIFICATION_UNPARENTED: {
        if (sprite) {
            sprite->disconnect("world_transforms_changed", this, "_on_world_transforms_changed");
        }       
    }
    default:
        break;
    }
}

void SpineSlotNode::_get_property_list(List<PropertyInfo>* list) const {
    Vector<String> slot_names;
    if (sprite) sprite->get_skeleton_data_res()->get_slot_names(slot_names);
    else slot_names.push_back(slot_name);
    PropertyInfo slot_name_property;
    slot_name_property.name = "slot_name";
    slot_name_property.type = Variant::STRING;
    slot_name_property.hint_string = String(",").join(slot_names);
    slot_name_property.hint = PROPERTY_HINT_ENUM;
    slot_name_property.usage = PROPERTY_USAGE_DEFAULT;
    list->push_back(slot_name_property);
}

bool SpineSlotNode::_get(const StringName& property, Variant& value) const {
    if (property == "slot_name") {
        value = slot_name;
        return true;
    }
    return false;
}

bool SpineSlotNode::_set(const StringName& property, const Variant& value) {
    if (property == "slot_name") {
        slot_name = value;
        return true;
    }
    return false;
}

void SpineSlotNode::on_world_transforms_changed(const Variant& _sprite) {
    SpineSprite* sprite = Object::cast_to<SpineSprite>(_sprite.operator Object*());
    if (!sprite) return;
    auto slot = sprite->get_skeleton()->find_slot(slot_name);
    if (!slot.is_valid()) return;
    auto bone = slot->get_bone();
    if (!bone.is_valid()) return;
    this->set_global_transform(bone->get_global_transform());
}

void SpineSlotNode::set_slot_name(const String& _slot_name) {
    slot_name = _slot_name;
}

String SpineSlotNode::get_slot_name() {
    return slot_name;
}
