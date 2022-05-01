/******************************************************************************
* Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

#include "SpineBoneNode.h"

void SpineBoneNode::_bind_methods() {
	ClassDB::bind_method(D_METHOD("set_bone_mode"), &SpineBoneNode::set_bone_mode);
	ClassDB::bind_method(D_METHOD("get_bone_mode"), &SpineBoneNode::get_bone_mode);
	ClassDB::bind_method(D_METHOD("_on_world_transforms_changed", "spine_sprite"), &SpineBoneNode::on_world_transforms_changed);

	ADD_PROPERTY(PropertyInfo(Variant::INT, "bone_mode", PROPERTY_HINT_ENUM, "Follow,Drive"), "set_bone_mode", "get_bone_mode");
}

void SpineBoneNode::_notification(int what) {
	switch(what) {
	case NOTIFICATION_PARENTED: {
			SpineSprite *sprite = cast_to<SpineSprite>(get_parent());
			if (sprite) {
#if VERSION_MAJOR > 3
				sprite->connect("world_transforms_changed", callable_mp(this, &SpineSlotNode::on_world_transforms_changed));
#else
				sprite->connect("world_transforms_changed", this, "_on_world_transforms_changed");
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
				sprite->disconnect("world_transforms_changed", callable_mp(this, &SpineSlotNode::on_world_transforms_changed));
#else
				sprite->disconnect("world_transforms_changed", this, "_on_world_transforms_changed");
#endif
			}       
	}
	default:
		break;
	}
}

void SpineBoneNode::_get_property_list(List<PropertyInfo>* list) const {
	Vector<String> bone_names;
	SpineSprite *sprite = cast_to<SpineSprite>(get_parent());
	if (sprite) sprite->get_skeleton_data_res()->get_bone_names(bone_names);
	else bone_names.push_back(bone_name);
	auto element = list->front();
	while (element) {
		auto property_info = element->get();
		if (property_info.name == "SpineBoneNode") break;
		element = element->next();
	}
	PropertyInfo slot_name_property;
	slot_name_property.name = "bone_name";
	slot_name_property.type = Variant::STRING;
	slot_name_property.hint_string = String(",").join(bone_names);
	slot_name_property.hint = PROPERTY_HINT_ENUM;
	slot_name_property.usage = PROPERTY_USAGE_DEFAULT;
	list->insert_after(element, slot_name_property);
}

bool SpineBoneNode::_get(const StringName& property, Variant& value) const {
	if (property == "bone_name") {
		value = bone_name;
		return true;
	}
	return false;
}

bool SpineBoneNode::_set(const StringName& property, const Variant& value) {
	if (property == "bone_name") {
		bone_name = value;
		SpineSprite *sprite = cast_to<SpineSprite>(get_parent());
		init_transform(sprite);
		return true;
	}
	return false;
}

void SpineBoneNode::on_world_transforms_changed(const Variant& _sprite) {
	SpineSprite* sprite = cast_to<SpineSprite>(_sprite.operator Object*());
	update_transform(sprite);
}

void SpineBoneNode::update_transform(SpineSprite* sprite) {
	if (!is_visible_in_tree()) return;
	if (!sprite) return;
	if (!sprite->get_skeleton().is_valid() || !sprite->get_skeleton()->get_spine_object()) return;
	auto bone = sprite->get_skeleton()->find_bone(bone_name);
	if (!bone.is_valid()) return;

	if (bone_mode == SpineConstant::BoneMode_Drive) {
		bone->set_global_transform(get_global_transform());
	} else {
		this->set_global_transform(bone->get_global_transform());
	}
}

void SpineBoneNode::init_transform(SpineSprite* sprite) {
	if (!sprite) return;
	if (bone_mode == SpineConstant::BoneMode_Drive) return;
	sprite->get_skeleton()->set_to_setup_pose();
	set_global_transform(sprite->get_global_bone_transform(bone_name));
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
}

SpineConstant::BoneMode SpineBoneNode::get_bone_mode() {
	return bone_mode;
}

void SpineBoneNode::set_bone_mode(SpineConstant::BoneMode _bone_mode) {
	bone_mode = _bone_mode;
	SpineSprite *sprite = cast_to<SpineSprite>(get_parent());
	init_transform(sprite);
}
