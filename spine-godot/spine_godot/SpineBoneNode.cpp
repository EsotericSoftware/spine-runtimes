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

#include "SpineBoneNode.h"

#if VERSION_MAJOR > 3
#include "core/config/engine.h"
#else
#include "core/engine.h"
#endif

void SpineBoneNode::_bind_methods() {
	ClassDB::bind_method(D_METHOD("set_bone_mode"), &SpineBoneNode::set_bone_mode);
	ClassDB::bind_method(D_METHOD("get_bone_mode"), &SpineBoneNode::get_bone_mode);
	ClassDB::bind_method(D_METHOD("set_enabled"), &SpineBoneNode::set_enabled);
	ClassDB::bind_method(D_METHOD("get_enabled"), &SpineBoneNode::get_enabled);
	ClassDB::bind_method(D_METHOD("set_debug_thickness"), &SpineBoneNode::set_debug_thickness);
	ClassDB::bind_method(D_METHOD("get_debug_thickness"), &SpineBoneNode::get_debug_thickness);
	ClassDB::bind_method(D_METHOD("set_debug_color"), &SpineBoneNode::set_debug_color);
	ClassDB::bind_method(D_METHOD("get_debug_color"), &SpineBoneNode::get_debug_color);
	ClassDB::bind_method(D_METHOD("_on_world_transforms_changed", "spine_sprite"), &SpineBoneNode::on_world_transforms_changed);
	ClassDB::bind_method(D_METHOD("find_bone"), &SpineBoneNode::find_bone);
	ClassDB::bind_method(D_METHOD("find_sprite"), &SpineBoneNode::find_parent_sprite);

	ADD_PROPERTY(PropertyInfo(Variant::INT, "bone_mode", PROPERTY_HINT_ENUM, "Follow,Drive"), "set_bone_mode", "get_bone_mode");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "enabled"), "set_enabled", "get_enabled");
	ADD_GROUP("Debug", "");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "thickness"), "set_debug_thickness", "get_debug_thickness");
	ADD_PROPERTY(PropertyInfo(Variant::COLOR, "color"), "set_debug_color", "get_debug_color");
}

void SpineBoneNode::_notification(int what) {
	switch (what) {
		case NOTIFICATION_PARENTED: {
			SpineSprite *sprite = find_parent_sprite();
			if (sprite) {
#if VERSION_MAJOR > 3
				sprite->connect(SNAME("world_transforms_changed"), callable_mp(this, &SpineBoneNode::on_world_transforms_changed));
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
				WARN_PRINT("SpineBoneNode parent is not a SpineSprite.");
			}
			NOTIFY_PROPERTY_LIST_CHANGED();
			break;
		}
		case NOTIFICATION_UNPARENTED: {
			SpineSprite *sprite = find_parent_sprite();
			if (sprite) {
#if VERSION_MAJOR > 3
				sprite->disconnect(SNAME("world_transforms_changed"), callable_mp(this, &SpineBoneNode::on_world_transforms_changed));
#else
				sprite->disconnect(SNAME("world_transforms_changed"), this, SNAME("_on_world_transforms_changed"));
#endif
			}
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

void SpineBoneNode::_get_property_list(List<PropertyInfo> *list) const {
	Vector<String> bone_names;
	SpineSprite *sprite = find_parent_sprite();
	if (sprite) sprite->get_skeleton_data_res()->get_bone_names(bone_names);
	else
		bone_names.push_back(bone_name);
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

bool SpineBoneNode::_get(const StringName &property, Variant &value) const {
	if (property == "bone_name") {
		value = bone_name;
		return true;
	}
	return false;
}

bool SpineBoneNode::_set(const StringName &property, const Variant &value) {
	if (property == "bone_name") {
		bone_name = value;
		SpineSprite *sprite = find_parent_sprite();
		init_transform(sprite);
		return true;
	}
	return false;
}

void SpineBoneNode::on_world_transforms_changed(const Variant &_sprite) {
	SpineSprite *sprite = cast_to<SpineSprite>(_sprite.operator Object *());
	update_transform(sprite);
#if VERSION_MAJOR > 3
	queue_redraw();
#else
	update();
#endif
}

void SpineBoneNode::update_transform(SpineSprite *sprite) {
	if (!enabled) return;
	Ref<SpineBone> bone = find_bone();
	if (!bone.is_valid()) return;

	Transform2D bone_transform = bone->get_global_transform();
	Transform2D this_transform = get_global_transform();

	if (bone_mode == SpineConstant::BoneMode_Drive) {
		bone->set_global_transform(this_transform);
	} else {
		set_global_transform(bone_transform);
	}

	if (Engine::get_singleton()->is_editor_hint()) {
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
}

void SpineBoneNode::init_transform(SpineSprite *sprite) {
	if (!sprite) return;
	if (bone_mode == SpineConstant::BoneMode_Drive) return;
	sprite->get_skeleton()->set_to_setup_pose();
	sprite->get_skeleton()->update_world_transform(SpineConstant::Physics_Update);
	Transform2D global_transform = sprite->get_global_bone_transform(bone_name);
	set_global_transform(global_transform);
	update_transform(sprite);
}

SpineSprite *SpineBoneNode::find_parent_sprite() const {
	auto parent = get_parent();
	SpineSprite *sprite = nullptr;
	while (parent) {
		sprite = cast_to<SpineSprite>(parent);
		if (sprite) break;
		parent = parent->get_parent();
	}
	return sprite;
}

Ref<SpineBone> SpineBoneNode::find_bone() const {
	if (!is_visible_in_tree()) return nullptr;
	SpineSprite *sprite = find_parent_sprite();
	if (!sprite) return nullptr;
	if (!sprite->get_skeleton().is_valid() || !sprite->get_skeleton()->get_spine_object()) return nullptr;
	auto bone = sprite->get_skeleton()->find_bone(bone_name);
	return bone;
}

void SpineBoneNode::draw() {
	if (!Engine::get_singleton()->is_editor_hint() && !get_tree()->is_debugging_collisions_hint()) return;
	Ref<SpineBone> bone = find_bone();
	if (!bone.is_valid()) return;

	spine::Bone *spine_bone = bone->get_spine_object();
	if (!spine_bone) return;
	float bone_length = spine_bone->getData().getLength();
	if (bone_length == 0) {
		draw_circle(Vector2(0, 0), debug_thickness, debug_color);
	} else {
		Vector<Vector2> points;
		points.push_back(Vector2(-debug_thickness, 0));
		points.push_back(Vector2(0, debug_thickness));
		points.push_back(Vector2(bone_length, 0));
		points.push_back(Vector2(0, -debug_thickness));
		draw_colored_polygon(points, debug_color);
	}
}

SpineConstant::BoneMode SpineBoneNode::get_bone_mode() {
	return bone_mode;
}

void SpineBoneNode::set_bone_mode(SpineConstant::BoneMode _bone_mode) {
	if (bone_mode != _bone_mode) {
		bone_mode = _bone_mode;
		SpineSprite *sprite = find_parent_sprite();
		init_transform(sprite);
	}
}

void SpineBoneNode::set_debug_thickness(float _thickness) {
	debug_thickness = _thickness;
}

float SpineBoneNode::get_debug_thickness() {
	return debug_thickness;
}

void SpineBoneNode::set_debug_color(Color _color) {
	debug_color = _color;
}

Color SpineBoneNode::get_debug_color() {
	return debug_color;
}

void SpineBoneNode::set_enabled(bool _enabled) {
	enabled = _enabled;
	if (!enabled && Engine::get_singleton()->is_editor_hint()) {
		auto sprite = find_parent_sprite();
		if (!sprite) return;
		sprite->get_skeleton()->set_to_setup_pose();
		sprite->get_skeleton()->update_world_transform(SpineConstant::Physics_Update);
	}
}

bool SpineBoneNode::get_enabled() {
	return enabled;
}
