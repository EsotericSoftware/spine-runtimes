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

#include "SpineSprite.h"
#include "SpineEvent.h"
#include "SpineTrackEntry.h"
#include "SpineSkeleton.h"
#include "SpineRendererObject.h"
#include "SpineSlotNode.h"

#if VERSION_MAJOR > 3
#include "core/config/engine.h"
#include "core/math/geometry_2d.h"
#else
#include "core/engine.h"
#endif

#include "scene/gui/control.h"
#include "scene/main/viewport.h"

#if TOOLS_ENABLED
#include "editor/editor_plugin.h"
#endif

Ref<CanvasItemMaterial> SpineSprite::default_materials[4] = {};
static int sprite_count = 0;
static spine::Vector<unsigned short> quad_indices;
static spine::Vector<float> scratch_vertices;
static Vector<Vector2> scratch_points;
static Vector<Vector2> scratch_uvs;
static Vector<Color> scratch_colors;
static Vector<int> scratch_indices;

void SpineSprite::_bind_methods() {
	ClassDB::bind_method(D_METHOD("set_skeleton_data_res", "skeleton_data_res"), &SpineSprite::set_skeleton_data_res);
	ClassDB::bind_method(D_METHOD("get_skeleton_data_res"), &SpineSprite::get_skeleton_data_res);
	ClassDB::bind_method(D_METHOD("get_skeleton"), &SpineSprite::get_skeleton);
	ClassDB::bind_method(D_METHOD("get_animation_state"), &SpineSprite::get_animation_state);
	ClassDB::bind_method(D_METHOD("on_skeleton_data_changed"), &SpineSprite::on_skeleton_data_changed);

	ClassDB::bind_method(D_METHOD("get_global_bone_transform", "bone_name"), &SpineSprite::get_global_bone_transform);
	ClassDB::bind_method(D_METHOD("set_global_bone_transform", "bone_name", "global_transform"), &SpineSprite::set_global_bone_transform);

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

	ADD_SIGNAL(MethodInfo("animation_started", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite"), PropertyInfo(Variant::OBJECT, "animation_state", PROPERTY_HINT_TYPE_STRING, "SpineAnimationState"), PropertyInfo(Variant::OBJECT, "track_entry", PROPERTY_HINT_TYPE_STRING, "SpineTrackEntry")));
	ADD_SIGNAL(MethodInfo("animation_interrupted", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite"), PropertyInfo(Variant::OBJECT, "animation_state", PROPERTY_HINT_TYPE_STRING, "SpineAnimationState"), PropertyInfo(Variant::OBJECT, "track_entry", PROPERTY_HINT_TYPE_STRING, "SpineTrackEntry")));
	ADD_SIGNAL(MethodInfo("animation_ended", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite"), PropertyInfo(Variant::OBJECT, "animation_state", PROPERTY_HINT_TYPE_STRING, "SpineAnimationState"), PropertyInfo(Variant::OBJECT, "track_entry", PROPERTY_HINT_TYPE_STRING, "SpineTrackEntry")));
	ADD_SIGNAL(MethodInfo("animation_completed", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite"), PropertyInfo(Variant::OBJECT, "animation_state", PROPERTY_HINT_TYPE_STRING, "SpineAnimationState"), PropertyInfo(Variant::OBJECT, "track_entry", PROPERTY_HINT_TYPE_STRING, "SpineTrackEntry")));
	ADD_SIGNAL(MethodInfo("animation_disposed", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite"), PropertyInfo(Variant::OBJECT, "animation_state", PROPERTY_HINT_TYPE_STRING, "SpineAnimationState"), PropertyInfo(Variant::OBJECT, "track_entry", PROPERTY_HINT_TYPE_STRING, "SpineTrackEntry")));
	ADD_SIGNAL(MethodInfo("animation_event", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite"), PropertyInfo(Variant::OBJECT, "animation_state", PROPERTY_HINT_TYPE_STRING, "SpineAnimationState"), PropertyInfo(Variant::OBJECT, "track_entry", PROPERTY_HINT_TYPE_STRING, "SpineTrackEntry"), PropertyInfo(Variant::OBJECT, "event", PROPERTY_HINT_TYPE_STRING, "SpineEvent")));
	ADD_SIGNAL(MethodInfo("before_animation_state_update", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite")));
	ADD_SIGNAL(MethodInfo("before_animation_state_apply", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite")));
	ADD_SIGNAL(MethodInfo("before_world_transforms_change", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite")));
	ADD_SIGNAL(MethodInfo("world_transforms_changed", PropertyInfo(Variant::OBJECT, "spine_sprite", PROPERTY_HINT_TYPE_STRING, "SpineSprite")));
	ADD_SIGNAL(MethodInfo("_internal_spine_objects_invalidated"));

	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "skeleton_data_res", PropertyHint::PROPERTY_HINT_RESOURCE_TYPE, "SpineSkeletonDataResource"), "set_skeleton_data_res", "get_skeleton_data_res");
	ADD_PROPERTY(PropertyInfo(Variant::INT, "update_mode", PROPERTY_HINT_ENUM, "Process,Physics,Manual"), "set_update_mode", "get_update_mode");
	ADD_GROUP("Materials", "");
	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "normal_material", PROPERTY_HINT_RESOURCE_TYPE, "Material"), "set_normal_material", "get_normal_material");
	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "additive_material", PROPERTY_HINT_RESOURCE_TYPE, "Material"), "set_additive_material", "get_additive_material");
	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "multiply_material", PROPERTY_HINT_RESOURCE_TYPE, "Material"), "set_multiply_material", "get_multiply_material");
	ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "screen_material", PROPERTY_HINT_RESOURCE_TYPE, "Material"), "set_screen_material", "get_screen_material");

	ADD_GROUP("Debug", "");
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

SpineSprite::SpineSprite() : update_mode(SpineConstant::UpdateMode_Process), preview_skin("Default"), preview_animation("-- Empty --"), preview_frame(false), preview_time(0), skeleton_clipper(nullptr), modified_bones(false) {
	skeleton_clipper = new spine::SkeletonClipping();

	// One material per blend mode, shared across all sprites.
	if (!default_materials[0].is_valid()) {
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
	}

	// Setup static scratch buffers
	if (quad_indices.size() == 0) {
		quad_indices.setSize(6, 0);
		quad_indices[0] = 0;
		quad_indices[1] = 1;
		quad_indices[2] = 2;
		quad_indices[3] = 2;
		quad_indices[4] = 3;
		quad_indices[5] = 0;
		scratch_vertices.ensureCapacity(1200);
	}

	// Default debug settings
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

	sprite_count++;
}

SpineSprite::~SpineSprite() {
	delete skeleton_clipper;
	sprite_count--;
	if (!sprite_count) {
		for (int i = 0; i < 4; i++)
			default_materials[i].unref();
	}
}

void SpineSprite::set_skeleton_data_res(const Ref<SpineSkeletonDataResource> &_skeleton_data) {
	skeleton_data_res = _skeleton_data;
	on_skeleton_data_changed();
}
Ref<SpineSkeletonDataResource> SpineSprite::get_skeleton_data_res() {
	return skeleton_data_res;
}

void SpineSprite::on_skeleton_data_changed() {
	remove_meshes();
	skeleton.unref();
	animation_state.unref();
	emit_signal("_internal_spine_objects_invalidated");

	if (skeleton_data_res.is_valid()) {
#if VERSION_MAJOR > 3
		if (!skeleton_data_res->is_connected("skeleton_data_changed", callable_mp(this, &SpineSprite::on_skeleton_data_changed)))
			skeleton_data_res->connect("skeleton_data_changed", callable_mp(this, &SpineSprite::on_skeleton_data_changed));
#else
		if (!skeleton_data_res->is_connected("skeleton_data_changed", this, "on_skeleton_data_changed"))
			skeleton_data_res->connect("skeleton_data_changed", this, "on_skeleton_data_changed");
#endif
	}

	if (skeleton_data_res.is_valid() && skeleton_data_res->is_skeleton_data_loaded()) {
		skeleton = Ref<SpineSkeleton>(memnew(SpineSkeleton));
		skeleton->set_spine_sprite(this);

		animation_state = Ref<SpineAnimationState>(memnew(SpineAnimationState));
		animation_state->set_spine_sprite(this);
		animation_state->get_spine_object()->setListener(this);

		animation_state->update(0);
		animation_state->apply(skeleton);
		skeleton->update_world_transform();
		generate_meshes_for_slots(skeleton);

		if (update_mode == SpineConstant::UpdateMode_Process) {
			_notification(NOTIFICATION_INTERNAL_PROCESS);
		} else if (update_mode == SpineConstant::UpdateMode_Physics) {
			_notification(NOTIFICATION_INTERNAL_PHYSICS_PROCESS);
		}
	}

	NOTIFY_PROPERTY_LIST_CHANGED();
}

void SpineSprite::generate_meshes_for_slots(Ref<SpineSkeleton> skeleton_ref) {
	auto skeleton = skeleton_ref->get_spine_object();
	for (int i = 0, n = (int) skeleton->getSlots().size(); i < n; i++) {
		auto mesh_instance = memnew(MeshInstance2D);
		mesh_instance->set_position(Vector2(0, 0));
		mesh_instance->set_material(default_materials[spine::BlendMode_Normal]);
		// Needed so that debug drawables are rendered in front of attachments
		mesh_instance->set_draw_behind_parent(true);
		add_child(mesh_instance);
		mesh_instances.push_back(mesh_instance);
		slot_nodes.add(spine::Vector<SpineSlotNode *>());
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

	auto draw_order = skeleton->get_spine_object()->getDrawOrder();
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
		spine::Vector<SpineSlotNode *> &nodes = slot_nodes[slot_index];
		for (int j = 0; j < (int) nodes.size(); j++) {
			auto node = nodes[j];
			move_child(node, mesh_index + 1);
		}
	}
}

Ref<SpineSkeleton> SpineSprite::get_skeleton() {
	return skeleton;
}

Ref<SpineAnimationState> SpineSprite::get_animation_state() {
	return animation_state;
}

void SpineSprite::_notification(int what) {
	switch (what) {
		case NOTIFICATION_READY: {
			set_process_internal(update_mode == SpineConstant::UpdateMode_Process);
			set_physics_process_internal(update_mode == SpineConstant::UpdateMode_Physics);
			break;
		}
		case NOTIFICATION_INTERNAL_PROCESS: {
			if (update_mode == SpineConstant::UpdateMode_Process)
				update_skeleton(get_process_delta_time());
			break;
		}
		case NOTIFICATION_INTERNAL_PHYSICS_PROCESS: {
			if (update_mode == SpineConstant::UpdateMode_Physics)
				update_skeleton(get_physics_process_delta_time());
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
	if (!skeleton_data_res.is_valid() || !skeleton_data_res->is_skeleton_data_loaded()) return;
	Vector<String> animation_names;
	Vector<String> skin_names;
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
	preview_time_property.hint_string = String("0.0,{0},0.01").format(varray(animation_duration));
	preview_time_property.hint = PROPERTY_HINT_RANGE;
	list->push_back(preview_time_property);
}

bool SpineSprite::_get(const StringName &property, Variant &value) const {
	if (property == "preview_skin") {
		value = preview_skin;
		return true;
	}

	if (property == "preview_animation") {
		value = preview_animation;
		return true;
	}

	if (property == "preview_frame") {
		value = preview_frame;
		return true;
	}

	if (property == "preview_time") {
		value = preview_time;
		return true;
	}
	return false;
}

static void update_preview_animation(SpineSprite *sprite, const String &skin, const String &animation, bool frame, float time) {
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
	track_entry->set_mix_duration(0);
	if (frame) {
		track_entry->set_time_scale(0);
		track_entry->set_track_time(time);
	}
}

bool SpineSprite::_set(const StringName &property, const Variant &value) {
	if (property == "preview_skin") {
		preview_skin = value;
		update_preview_animation(this, preview_skin, preview_animation, preview_frame, preview_time);
		NOTIFY_PROPERTY_LIST_CHANGED();
		return true;
	}

	if (property == "preview_animation") {
		preview_animation = value;
		update_preview_animation(this, preview_skin, preview_animation, preview_frame, preview_time);
		NOTIFY_PROPERTY_LIST_CHANGED();
		return true;
	}

	if (property == "preview_frame") {
		preview_frame = value;
		update_preview_animation(this, preview_skin, preview_animation, preview_frame, preview_time);
		return true;
	}

	if (property == "preview_time") {
		preview_time = value;
		update_preview_animation(this, preview_skin, preview_animation, preview_frame, preview_time);
		return true;
	}

	return false;
}

void SpineSprite::update_skeleton(float delta) {
	if (!skeleton_data_res.is_valid() ||
		!skeleton_data_res->is_skeleton_data_loaded() ||
		!skeleton.is_valid() ||
		!skeleton->get_spine_object() ||
		!animation_state.is_valid() ||
		!animation_state->get_spine_object())
		return;

	emit_signal("before_animation_state_update", this);
	animation_state->update(delta);
	if (!is_visible_in_tree()) return;
	emit_signal("before_animation_state_apply", this);
	animation_state->apply(skeleton);
	emit_signal("before_world_transforms_change", this);
	skeleton->update_world_transform();
	modified_bones = false;
	emit_signal("world_transforms_changed", this);
	if (modified_bones) skeleton->update_world_transform();
	sort_slot_nodes();
	update_meshes(skeleton);
#if VERSION_MAJOR > 3
	queue_redraw();
#else
	update();
#endif
}

static void clear_mesh_instance(MeshInstance2D *mesh_instance) {
#if VERSION_MAJOR > 3
	RenderingServer::get_singleton()->canvas_item_clear(mesh_instance->get_canvas_item());
#else
	VisualServer::get_singleton()->canvas_item_clear(mesh_instance->get_canvas_item());
#endif
}

static void add_triangles(MeshInstance2D *mesh_instance,
						  const Vector<Point2> &vertices,
						  const Vector<Point2> &uvs,
						  const Vector<Color> &colors,
						  const Vector<int> &indices,
						  SpineRendererObject *renderer_object) {
#if VERSION_MAJOR > 3
	RenderingServer::get_singleton()->canvas_item_add_triangle_array(mesh_instance->get_canvas_item(),
																	 indices,
																	 vertices,
																	 colors,
																	 uvs,
																	 Vector<int>(),
																	 Vector<float>(),
																	 renderer_object->canvas_texture.is_valid() ? renderer_object->canvas_texture->get_rid() : RID(),
																	 -1);
#else
	auto texture = renderer_object->texture;
	auto normal_map = renderer_object->normal_map;
	VisualServer::get_singleton()->canvas_item_add_triangle_array(mesh_instance->get_canvas_item(),
																  indices,
																  vertices,
																  colors,
																  uvs,
																  Vector<int>(),
																  Vector<float>(),
																  texture.is_null() ? RID() : texture->get_rid(),
																  -1,
																  normal_map.is_null() ? RID() : normal_map->get_rid());
#endif
}

void SpineSprite::update_meshes(Ref<SpineSkeleton> skeleton_ref) {
	spine::Skeleton *skeleton = skeleton_ref->get_spine_object();
	for (int i = 0, n = (int) skeleton->getSlots().size(); i < n; ++i) {
		spine::Slot *slot = skeleton->getDrawOrder()[i];
		spine::Attachment *attachment = slot->getAttachment();
		MeshInstance2D *mesh_instance = mesh_instances[i];
		clear_mesh_instance(mesh_instance);
		if (!attachment) {
			skeleton_clipper->clipEnd(*slot);
			continue;
		}

		spine::Color skeleton_color = skeleton->getColor();
		spine::Color slot_color = slot->getColor();
		spine::Color tint(skeleton_color.r * slot_color.r, skeleton_color.g * slot_color.g, skeleton_color.b * slot_color.b, skeleton_color.a * slot_color.a);
		SpineRendererObject *renderer_object;
		spine::Vector<float> *vertices = &scratch_vertices;
		spine::Vector<float> *uvs;
		spine::Vector<unsigned short> *indices;

		if (attachment->getRTTI().isExactly(spine::RegionAttachment::rtti)) {
			auto *region = (spine::RegionAttachment *) attachment;

			vertices->setSize(8, 0);
			region->computeWorldVertices(*slot, *vertices, 0);
			renderer_object = (SpineRendererObject *) ((spine::AtlasRegion *) region->getRendererObject())->page->getRendererObject();
			uvs = &region->getUVs();
			indices = &quad_indices;

			auto attachment_color = region->getColor();
			tint.r *= attachment_color.r;
			tint.g *= attachment_color.g;
			tint.b *= attachment_color.b;
			tint.a *= attachment_color.a;
		} else if (attachment->getRTTI().isExactly(spine::MeshAttachment::rtti)) {
			auto *mesh = (spine::MeshAttachment *) attachment;

			vertices->setSize(mesh->getWorldVerticesLength(), 0);
			mesh->computeWorldVertices(*slot, *vertices);
			renderer_object = (SpineRendererObject *) ((spine::AtlasRegion *) mesh->getRendererObject())->page->getRendererObject();
			uvs = &mesh->getUVs();
			indices = &mesh->getTriangles();

			auto attachment_color = mesh->getColor();
			tint.r *= attachment_color.r;
			tint.g *= attachment_color.g;
			tint.b *= attachment_color.b;
			tint.a *= attachment_color.a;
		} else if (attachment->getRTTI().isExactly(spine::ClippingAttachment::rtti)) {
			auto clip = (spine::ClippingAttachment *) attachment;
			skeleton_clipper->clipStart(*slot, clip);
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
			// Set the mesh
			size_t num_vertices = vertices->size() / 2;
			scratch_points.resize((int) num_vertices);
			memcpy(scratch_points.ptrw(), vertices->buffer(), num_vertices * 2 * sizeof(float));
			scratch_uvs.resize((int) num_vertices);
			memcpy(scratch_uvs.ptrw(), uvs->buffer(), num_vertices * 2 * sizeof(float));
			scratch_colors.resize((int) num_vertices);
			for (int j = 0; j < (int) num_vertices; j++) {
				scratch_colors.set(j, Color(tint.r, tint.g, tint.b, tint.a));
			}
			scratch_indices.resize((int) indices->size());
			for (int j = 0; j < (int) indices->size(); ++j) {
				scratch_indices.set(j, indices->buffer()[j]);
			}

			add_triangles(mesh_instance, scratch_points, scratch_uvs, scratch_colors, scratch_indices, renderer_object);

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

			// Set the custom material, or the default material
			if (custom_material.is_valid()) mesh_instance->set_material(custom_material);
			else
				mesh_instance->set_material(default_materials[slot->getData().getBlendMode()]);
		}
		skeleton_clipper->clipEnd(*slot);
	}
	skeleton_clipper->clipEnd();
}

void SpineSprite::draw() {
	if (!Engine::get_singleton()->is_editor_hint() && !get_tree()->is_debugging_collisions_hint()) return;
	if (!animation_state.is_valid() && !skeleton.is_valid()) return;

	auto mouse_position = get_local_mouse_position();
	spine::Slot *hovered_slot = nullptr;

	if (debug_regions) {
		draw_set_transform(Vector2(0, 0), 0, Vector2(1, 1));
		auto &draw_order = skeleton->get_spine_object()->getDrawOrder();
		for (int i = 0; i < (int) draw_order.size(); i++) {
			auto *slot = draw_order[i];
			if (!slot->getBone().isActive()) continue;
			auto *attachment = slot->getAttachment();
			if (!attachment) continue;
			if (!attachment->getRTTI().isExactly(spine::RegionAttachment::rtti)) continue;
			auto *region = (spine::RegionAttachment *) attachment;
			auto *vertices = &scratch_vertices;
			vertices->setSize(8, 0);
			region->computeWorldVertices(*slot, *vertices, 0);
			scratch_points.resize(0);
			for (int i = 0, j = 0; i < 4; i++, j += 2) {
				float x = vertices->buffer()[j];
				float y = vertices->buffer()[j + 1];
				scratch_points.push_back(Vector2(x, y));
			}
			scratch_points.push_back(Vector2(vertices->buffer()[0], vertices->buffer()[1]));

			Color color = debug_regions_color;
			if (GEOMETRY2D::is_point_in_polygon(mouse_position, scratch_points)) {
				hovered_slot = slot;
				color = Color(1, 1, 1, 1);
			}
			scratch_points.push_back(Vector2(vertices->buffer()[0], vertices->buffer()[1]));
			draw_polyline(scratch_points, color, 2);
		}
	}

	if (debug_meshes) {
		draw_set_transform(Vector2(0, 0), 0, Vector2(1, 1));
		auto &draw_order = skeleton->get_spine_object()->getDrawOrder();
		for (int i = 0; i < (int) draw_order.size(); i++) {
			auto *slot = draw_order[i];
			if (!slot->getBone().isActive()) continue;
			auto *attachment = slot->getAttachment();
			if (!attachment) continue;
			if (!attachment->getRTTI().isExactly(spine::MeshAttachment::rtti)) continue;
			auto *mesh = (spine::MeshAttachment *) attachment;
			auto *vertices = &scratch_vertices;
			vertices->setSize(mesh->getWorldVerticesLength(), 0);
			mesh->computeWorldVertices(*slot, *vertices);
			scratch_points.resize(0);
			for (int i = 0, j = 0; i < mesh->getHullLength(); i++, j += 2) {
				float x = vertices->buffer()[j];
				float y = vertices->buffer()[j + 1];
				scratch_points.push_back(Vector2(x, y));
			}

			Color color = debug_meshes_color;
			if (GEOMETRY2D::is_point_in_polygon(mouse_position, scratch_points)) {
				hovered_slot = slot;
				color = Color(1, 1, 1, 1);
			}
			scratch_points.push_back(Vector2(vertices->buffer()[0], vertices->buffer()[1]));
			draw_polyline(scratch_points, color, 2);
		}
	}

	if (debug_bounding_boxes) {
		draw_set_transform(Vector2(0, 0), 0, Vector2(1, 1));
		auto &draw_order = skeleton->get_spine_object()->getDrawOrder();
		for (int i = 0; i < (int) draw_order.size(); i++) {
			auto *slot = draw_order[i];
			if (!slot->getBone().isActive()) continue;
			auto *attachment = slot->getAttachment();
			if (!attachment) continue;
			if (!attachment->getRTTI().isExactly(spine::BoundingBoxAttachment::rtti)) continue;
			auto *bounding_box = (spine::BoundingBoxAttachment *) attachment;
			auto *vertices = &scratch_vertices;
			vertices->setSize(bounding_box->getWorldVerticesLength(), 0);
			bounding_box->computeWorldVertices(*slot, *vertices);
			size_t num_vertices = vertices->size() / 2;
			scratch_points.resize((int) num_vertices);
			memcpy(scratch_points.ptrw(), vertices->buffer(), num_vertices * 2 * sizeof(float));
			scratch_points.push_back(Vector2(vertices->buffer()[0], vertices->buffer()[1]));
			draw_polyline(scratch_points, debug_bounding_boxes_color, 2);
		}
	}

	if (debug_clipping) {
		draw_set_transform(Vector2(0, 0), 0, Vector2(1, 1));
		auto &draw_order = skeleton->get_spine_object()->getDrawOrder();
		for (int i = 0; i < (int) draw_order.size(); i++) {
			auto *slot = draw_order[i];
			if (!slot->getBone().isActive()) continue;
			auto *attachment = slot->getAttachment();
			if (!attachment) continue;
			if (!attachment->getRTTI().isExactly(spine::ClippingAttachment::rtti)) continue;
			auto *clipping = (spine::ClippingAttachment *) attachment;
			auto *vertices = &scratch_vertices;
			vertices->setSize(clipping->getWorldVerticesLength(), 0);
			clipping->computeWorldVertices(*slot, *vertices);
			size_t num_vertices = vertices->size() / 2;
			scratch_points.resize((int) num_vertices);
			memcpy(scratch_points.ptrw(), vertices->buffer(), num_vertices * 2 * sizeof(float));
			scratch_points.push_back(Vector2(vertices->buffer()[0], vertices->buffer()[1]));
			draw_polyline(scratch_points, debug_clipping_color, 2);
		}
	}


	spine::Bone *hovered_bone = nullptr;
	if (debug_bones) {
		auto &bones = skeleton->get_spine_object()->getBones();
		for (int i = 0; i < (int) bones.size(); i++) {
			auto *bone = bones[i];
			if (!bone->isActive()) continue;
			draw_bone(bone, debug_bones_color);

			float bone_length = bone->getData().getLength();
			if (bone_length == 0) bone_length = debug_bones_thickness * 2;

			scratch_points.resize(5);
			scratch_points.set(0, Vector2(-debug_bones_thickness, 0));
			scratch_points.set(1, Vector2(0, debug_bones_thickness));
			scratch_points.set(2, Vector2(bone_length, 0));
			scratch_points.set(3, Vector2(0, -debug_bones_thickness));
			scratch_points.set(4, Vector2(-debug_bones_thickness, 0));
			Transform2D bone_transform(spine::MathUtil::Deg_Rad * bone->getWorldRotationX(), Vector2(bone->getWorldX(), bone->getWorldY()));
			bone_transform.scale_basis(Vector2(bone->getWorldScaleX(), bone->getWorldScaleY()));
			auto mouse_local_position = bone_transform.affine_inverse().xform(mouse_position);
			if (GEOMETRY2D::is_point_in_polygon(mouse_local_position, scratch_points)) {
				hovered_bone = bone;
			}
		}
	}

#if TOOLS_ENABLED
	Ref<Font> default_font;
	auto control = memnew(Control);
#if VERSION_MAJOR > 3
	default_font = control->get_theme_default_font();
#else
	default_font = control->get_font("font", "Label");
#endif
	memfree(control);

	float editor_scale = EditorInterface::get_singleton()->get_editor_scale();
	float inverse_zoom = 1 / get_viewport()->get_global_canvas_transform().get_scale().x * editor_scale;
	Vector<String> hover_text_lines;
	if (hovered_slot) {
		hover_text_lines.push_back(String("Slot: ") + hovered_slot->getData().getName().buffer());
	}

	if (hovered_bone) {
		float thickness = debug_bones_thickness;
		debug_bones_thickness *= 1.1;
		draw_bone(hovered_bone, Color(debug_bones_color.r, debug_bones_color.g, debug_bones_color.b, 1));
		debug_bones_thickness = thickness;
		hover_text_lines.push_back(String("Bone: ") + hovered_bone->getData().getName().buffer());
	}

	auto global_scale = get_global_scale();
	draw_set_transform(mouse_position + Vector2(20, 0), -get_global_rotation(), Vector2(inverse_zoom * (1 / global_scale.x), inverse_zoom * (1 / global_scale.y)));

#if VERSION_MAJOR > 3
	float line_height = default_font->get_height(Font::DEFAULT_FONT_SIZE) + default_font->get_descent(Font::DEFAULT_FONT_SIZE);
#else
	float line_height = default_font->get_height() + default_font->get_descent();
#endif
	float rect_width = 0;
	for (int i = 0; i < hover_text_lines.size(); i++) {
		rect_width = MAX(rect_width, default_font->get_string_size(hover_text_lines[i]).x);
	}

#if VERSION_MAJOR > 3
	Rect2 background_rect(0, -default_font->get_height(Font::DEFAULT_FONT_SIZE) - 5, rect_width + 20, line_height * hover_text_lines.size() + 10);
#else
	Rect2 background_rect(0, -default_font->get_height() - 5, rect_width + 20, line_height * hover_text_lines.size() + 10);
#endif
	if (hover_text_lines.size() > 0) draw_rect(background_rect, Color(0, 0, 0, 0.8));
	for (int i = 0; i < hover_text_lines.size(); i++) {
#if VERSION_MAJOR > 3
		draw_string(default_font, Vector2(10, 0 + i * default_font->get_height(Font::DEFAULT_FONT_SIZE)), hover_text_lines[i], HORIZONTAL_ALIGNMENT_LEFT, -1, Font::DEFAULT_FONT_SIZE, Color(1, 1, 1, 1));
#else
		draw_string(default_font, Vector2(10, 0 + i * default_font->get_height()), hover_text_lines[i], Color(1, 1, 1, 1));
#endif
	}
#endif
}

void SpineSprite::draw_bone(spine::Bone *bone, const Color &color) {
	draw_set_transform(Vector2(bone->getWorldX(), bone->getWorldY()), spine::MathUtil::Deg_Rad * bone->getWorldRotationX(), Vector2(bone->getWorldScaleX(), bone->getWorldScaleY()));
	float bone_length = bone->getData().getLength();
	if (bone_length == 0) bone_length = debug_bones_thickness * 2;
	Vector<Vector2> points;
	points.push_back(Vector2(-debug_bones_thickness, 0));
	points.push_back(Vector2(0, debug_bones_thickness));
	points.push_back(Vector2(bone_length, 0));
	points.push_back(Vector2(0, -debug_bones_thickness));
	draw_colored_polygon(points, color);
}

void SpineSprite::callback(spine::AnimationState *state, spine::EventType type, spine::TrackEntry *entry, spine::Event *event) {
	Ref<SpineTrackEntry> entry_ref = Ref<SpineTrackEntry>(memnew(SpineTrackEntry));
	entry_ref->set_spine_object(this, entry);

	Ref<SpineEvent> event_ref(nullptr);
	if (event) {
		event_ref = Ref<SpineEvent>(memnew(SpineEvent));
		event_ref->set_spine_object(this, event);
	}

	switch (type) {
		case spine::EventType_Start:
			emit_signal("animation_started", this, animation_state, entry_ref);
			break;
		case spine::EventType_Interrupt:
			emit_signal("animation_interrupted", this, animation_state, entry_ref);
			break;
		case spine::EventType_End:
			emit_signal("animation_ended", this, animation_state, entry_ref);
			break;
		case spine::EventType_Complete:
			emit_signal("animation_completed", this, animation_state, entry_ref);
			break;
		case spine::EventType_Dispose:
			emit_signal("animation_disposed", this, animation_state, entry_ref);
			break;
		case spine::EventType_Event:
			emit_signal("animation_event", this, animation_state, entry_ref, event_ref);
			break;
	}
}

Transform2D SpineSprite::get_global_bone_transform(const String &bone_name) {
	if (!animation_state.is_valid() && !skeleton.is_valid()) return get_global_transform();
	auto bone = skeleton->find_bone(bone_name);
	if (!bone.is_valid()) {
		print_error(vformat("Bone: '%s' not found.", bone_name));
		return get_global_transform();
	}
	return bone->get_global_transform();
}

void SpineSprite::set_global_bone_transform(const String &bone_name, Transform2D transform) {
	if (!animation_state.is_valid() && !skeleton.is_valid()) return;
	auto bone = skeleton->find_bone(bone_name);
	if (!bone.is_valid()) return;
	bone->set_global_transform(transform);
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
	Ref<SpineSkin> skin = memnew(SpineSkin);
	skin->init(name, this);
	return skin;
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

#ifdef TOOLS_ENABLED
Rect2 SpineSprite::_edit_get_rect() const {
	if (skeleton_data_res.is_valid() && skeleton_data_res->is_skeleton_data_loaded()) {
		auto data = skeleton_data_res->get_skeleton_data();
		return Rect2(data->getX(), -data->getY() - data->getHeight(), data->getWidth(), data->getHeight());
	}

	return Node2D::_edit_get_rect();
}

bool SpineSprite::_edit_use_rect() const {
	return skeleton_data_res.is_valid() && skeleton_data_res->is_skeleton_data_loaded();
}
#endif
