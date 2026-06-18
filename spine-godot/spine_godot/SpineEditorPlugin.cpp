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

#define VERSION_MAJOR 4

#ifdef TOOLS_ENABLED
#include "SpineEditorPlugin.h"
#include "SpineAtlasResource.h"
#include "SpineSkeletonFileResource.h"

#if VERSION_MAJOR > 3
#ifdef SPINE_GODOT_EXTENSION
#include <godot_cpp/classes/button.hpp>
#include <godot_cpp/classes/h_box_container.hpp>
#include <godot_cpp/classes/label.hpp>
#include <godot_cpp/classes/option_button.hpp>
#include <godot_cpp/classes/spin_box.hpp>
#else
#include "editor/editor_undo_redo_manager.h"
#endif
#ifdef SPINE_GODOT_EXTENSION
Error SpineAtlasResourceImportPlugin::_import(const String &source_file, const String &save_path, const Dictionary &options,
											  const TypedArray<String> &platform_variants, const TypedArray<String> &gen_files) const {
#else
#if VERSION_MINOR > 3
Error SpineAtlasResourceImportPlugin::import(ResourceUID::ID p_source_id, const String &source_file, const String &save_path,
											 const HashMap<StringName, Variant> &options, List<String> *r_platform_variants,
											 List<String> *r_gen_files, Variant *r_metadata) {
#else
Error SpineAtlasResourceImportPlugin::import(const String &source_file, const String &save_path, const HashMap<StringName, Variant> &options,
											 List<String> *platform_variants, List<String> *gen_files, Variant *metadata) {
#endif
#endif
#else
Error SpineAtlasResourceImportPlugin::import(const String &source_file, const String &save_path, const Map<StringName, Variant> &options,
											 List<String> *platform_variants, List<String> *gen_files, Variant *metadata) {
#endif
	Ref<SpineAtlasResource> atlas(memnew(SpineAtlasResource));
	atlas->set_normal_texture_prefix(options["normal_map_prefix"]);
	atlas->set_specular_texture_prefix(options["specular_map_prefix"]);
	atlas->load_from_atlas_file_internal(source_file, true);

#if VERSION_MAJOR > 3
#ifdef SPINE_GODOT_EXTENSION
	String file_name = save_path + String(".") + _get_save_extension();
	auto error = ResourceSaver::get_singleton()->save(atlas, file_name);
#else
	String file_name = save_path + String(".") + get_save_extension();
	auto error = ResourceSaver::save(atlas, file_name);
#endif
#else
	String file_name = save_path + String(".") + get_save_extension();
	auto error = ResourceSaver::save(file_name, atlas);
#endif
	return error;
}

#ifdef SPINE_GODOT_EXTENSION
TypedArray<Dictionary> SpineAtlasResourceImportPlugin::_get_import_options(const String &p_path, int32_t p_preset_index) const {
	TypedArray<Dictionary> options;

	Dictionary normal_map_dictionary;
	normal_map_dictionary["name"] = "normal_map_prefix";
	normal_map_dictionary["type"] = Variant::STRING;
	normal_map_dictionary["hint_string"] = "String";
	normal_map_dictionary["default_value"] = String("n");
	options.push_back(normal_map_dictionary);

	Dictionary specular_map_dictionary;
	specular_map_dictionary["name"] = "specular_map_prefix";
	specular_map_dictionary["type"] = Variant::STRING;
	specular_map_dictionary["hint_string"] = "String";
	specular_map_dictionary["default_value"] = String("s");
	options.push_back(specular_map_dictionary);

	return options;
}
#else
#if VERSION_MAJOR > 3
void SpineAtlasResourceImportPlugin::get_import_options(const String &path, List<ImportOption> *options, int preset) const {
#else
void SpineAtlasResourceImportPlugin::get_import_options(List<ImportOption> *options, int preset) const {
#endif
	if (preset == 0) {
		ImportOption normal_map_op;
		normal_map_op.option.name = "normal_map_prefix";
		normal_map_op.option.type = Variant::STRING;
		normal_map_op.option.hint_string = "String";
		normal_map_op.default_value = String("n");
		options->push_back(normal_map_op);

		ImportOption specular_map_op;
		specular_map_op.option.name = "specular_map_prefix";
		specular_map_op.option.type = Variant::STRING;
		specular_map_op.option.hint_string = "String";
		specular_map_op.default_value = String("s");
		options->push_back(specular_map_op);
	}
}
#endif

#if VERSION_MAJOR > 3
#ifdef SPINE_GODOT_EXTENSION
Error SpineJsonResourceImportPlugin::_import(const String &source_file, const String &save_path, const Dictionary &options,
											 const TypedArray<String> &platform_variants, const TypedArray<String> &gen_files) const {
#else
#if VERSION_MINOR > 3
Error SpineJsonResourceImportPlugin::import(ResourceUID::ID p_source_id, const String &source_file, const String &save_path,
											const HashMap<StringName, Variant> &p_options, List<String> *r_platform_variants,
											List<String> *r_gen_files, Variant *r_metadata) {
#else
Error SpineJsonResourceImportPlugin::import(const String &source_file, const String &save_path, const HashMap<StringName, Variant> &options,
											List<String> *platform_variants, List<String> *gen_files, Variant *metadata) {
#endif
#endif
#else
Error SpineJsonResourceImportPlugin::import(const String &source_file, const String &save_path, const Map<StringName, Variant> &options,
											List<String> *platform_variants, List<String> *gen_files, Variant *metadata) {
#endif
	Ref<SpineSkeletonFileResource> skeleton_file_res(memnew(SpineSkeletonFileResource));
	Error error = skeleton_file_res->load_from_file(source_file);
	if (error != OK) return error;

#if VERSION_MAJOR > 3
#ifdef SPINE_GODOT_EXTENSION
	String file_name = save_path + String(".") + _get_save_extension();
	error = ResourceSaver::get_singleton()->save(skeleton_file_res, file_name);
#else
	String file_name = save_path + String(".") + get_save_extension();
	error = ResourceSaver::save(skeleton_file_res, file_name);
#endif
#else
	String file_name = save_path + String(".") + get_save_extension();
	error = ResourceSaver::save(file_name, skeleton_file_res);
#endif
	return error;
}

#if VERSION_MAJOR > 3
#ifdef SPINE_GODOT_EXTENSION
Error SpineBinaryResourceImportPlugin::_import(const String &source_file, const String &save_path, const Dictionary &options,
											   const TypedArray<String> &platform_variants, const TypedArray<String> &gen_files) const {
#else
#if VERSION_MINOR > 3
Error SpineBinaryResourceImportPlugin::import(ResourceUID::ID p_source_id, const String &source_file, const String &save_path,
											  const HashMap<StringName, Variant> &p_options, List<String> *r_platform_variants,
											  List<String> *r_gen_files, Variant *r_metadata) {
#else
Error SpineBinaryResourceImportPlugin::import(const String &source_file, const String &save_path, const HashMap<StringName, Variant> &options,
											  List<String> *platform_variants, List<String> *gen_files, Variant *metadata) {
#endif
#endif
#else
Error SpineBinaryResourceImportPlugin::import(const String &source_file, const String &save_path, const Map<StringName, Variant> &options,
											  List<String> *platform_variants, List<String> *gen_files, Variant *metadata) {
#endif
	Ref<SpineSkeletonFileResource> skeleton_file_res(memnew(SpineSkeletonFileResource));
	Error error = skeleton_file_res->load_from_file(source_file);
	if (error != OK) return error;

#if VERSION_MAJOR > 3
#ifdef SPINE_GODOT_EXTENSION
	String file_name = save_path + String(".") + _get_save_extension();
	error = ResourceSaver::get_singleton()->save(skeleton_file_res, file_name);
#else
	String file_name = save_path + String(".") + get_save_extension();
	error = ResourceSaver::save(skeleton_file_res, file_name);
#endif
#else
	String file_name = save_path + String(".") + get_save_extension();
	error = ResourceSaver::save(file_name, skeleton_file_res);
#endif
	return error;
}

#ifdef SPINE_GODOT_EXTENSION
SpineEditorPlugin::SpineEditorPlugin() {
	atlas_import_plugin = Ref<EditorImportPlugin>(memnew(SpineAtlasResourceImportPlugin));
	json_import_plugin = Ref<EditorImportPlugin>(memnew(SpineJsonResourceImportPlugin));
	binary_import_plugin = Ref<EditorImportPlugin>(memnew(SpineBinaryResourceImportPlugin));
	skeleton_data_inspector_plugin = Ref<EditorInspectorPlugin>(memnew(SpineSkeletonDataResourceInspectorPlugin));

	add_import_plugin(atlas_import_plugin);
	add_import_plugin(json_import_plugin);
	add_import_plugin(binary_import_plugin);
	add_inspector_plugin(skeleton_data_inspector_plugin);
}

void SpineEditorPlugin::_notification(int p_what) {
	if (p_what == NOTIFICATION_PREDELETE) {
		remove_import_plugin(atlas_import_plugin);
		remove_import_plugin(json_import_plugin);
		remove_import_plugin(binary_import_plugin);
		remove_inspector_plugin(skeleton_data_inspector_plugin);
	}
}
#else
SpineEditorPlugin::SpineEditorPlugin(EditorNode *node) {
	add_import_plugin(memnew(SpineAtlasResourceImportPlugin));
	add_import_plugin(memnew(SpineJsonResourceImportPlugin));
	add_import_plugin(memnew(SpineBinaryResourceImportPlugin));
	add_inspector_plugin(memnew(SpineSkeletonDataResourceInspectorPlugin));
	add_inspector_plugin(memnew(SpineSpriteInspectorPlugin));
}
#endif

#ifdef SPINE_GODOT_EXTENSION
bool SpineSkeletonDataResourceInspectorPlugin::_can_handle(Object *object) const {
#else
bool SpineSkeletonDataResourceInspectorPlugin::can_handle(Object *object) {
#endif
	return object && object->is_class("SpineSkeletonDataResource");
}

#if VERSION_MAJOR > 3
#ifdef SPINE_GODOT_EXTENSION
bool SpineSkeletonDataResourceInspectorPlugin::_parse_property(Object *object, Variant::Type type, const String &path, PropertyHint hint,
															   const String &hint_text, const BitField<PropertyUsageFlags> p_usage, bool wide) {
#else
bool SpineSkeletonDataResourceInspectorPlugin::parse_property(Object *object, const Variant::Type type, const String &path, const PropertyHint hint,
															  const String &hint_text, const BitField<PropertyUsageFlags> p_usage, const bool wide) {
#endif
#else
bool SpineSkeletonDataResourceInspectorPlugin::parse_property(Object *object, Variant::Type type, const String &path, PropertyHint hint,
															  const String &hint_text, int usage) {
#endif
	if (path == "animation_mixes") {
		Ref<SpineSkeletonDataResource> skeleton_data = Object::cast_to<SpineSkeletonDataResource>(object);
		if (!skeleton_data.is_valid() || !skeleton_data->is_skeleton_data_loaded()) return true;
		auto mixes_property = memnew(SpineEditorPropertyAnimationMixes);
		mixes_property->setup(skeleton_data);
		add_property_editor(path, mixes_property);
		return true;
	}
	return false;
}

#ifndef SPINE_GODOT_EXTENSION
SpineEditorPropertyAnimationMixes::SpineEditorPropertyAnimationMixes() : skeleton_data(nullptr), container(nullptr), updating(false) {
	INSTANTIATE(array_object);
}

void SpineEditorPropertyAnimationMixes::_bind_methods() {
	ClassDB::bind_method(D_METHOD("add_mix"), &SpineEditorPropertyAnimationMixes::add_mix);
	ClassDB::bind_method(D_METHOD("delete_mix"), &SpineEditorPropertyAnimationMixes::delete_mix);
	ClassDB::bind_method(D_METHOD("update_mix_property"), &SpineEditorPropertyAnimationMixes::update_mix_property);
}

void SpineEditorPropertyAnimationMixes::add_mix() {
	if (!skeleton_data.is_valid() || !skeleton_data->is_skeleton_data_loaded() || updating) return;

	Vector<String> animation_names;
	skeleton_data->get_animation_names(animation_names);
	Ref<SpineAnimationMix> mix = Ref<SpineAnimationMix>(memnew(SpineAnimationMix));
	mix->set_from(animation_names[0]);
	mix->set_to(animation_names[0]);
	mix->set_mix(0);

	Array mixes = skeleton_data->get_animation_mixes().duplicate();
	mixes.push_back(mix);
	emit_changed(get_edited_property(), mixes);
}

void SpineEditorPropertyAnimationMixes::delete_mix(int idx) {
	if (!skeleton_data.is_valid() || !skeleton_data->is_skeleton_data_loaded() || updating) return;

	auto mixes = skeleton_data->get_animation_mixes().duplicate();
#if VERSION_MAJOR > 3
	mixes.remove_at((int) idx);
#else
	mixes.remove((int) idx);
#endif
	emit_changed(get_edited_property(), mixes);
}

void SpineEditorPropertyAnimationMixes::update_mix_property(int index) {
	if (index < 0 || index > mix_properties.size()) return;
	mix_properties[index]->update_property();
}

void SpineEditorPropertyAnimationMixes::update_property() {
	if (updating) return;
	updating = true;

	mix_properties.clear();

	if (container) {
		set_bottom_editor(nullptr);
		memdelete(container);
		container = nullptr;
	}

	if (!skeleton_data.is_valid() || !skeleton_data->is_skeleton_data_loaded()) {
		updating = false;
		return;
	}

	Vector<String> animation_names;
	skeleton_data->get_animation_names(animation_names);

	container = memnew(VBoxContainer);
	add_child(container);
	set_bottom_editor(container);

	Array mixes = skeleton_data->get_animation_mixes();
	array_object->set_array(mixes);
	for (int i = 0; i < mixes.size(); i++) {
		Ref<SpineAnimationMix> mix = mixes[i];
		String property_name = "indices/" + itos(i);

		auto hbox = memnew(HBoxContainer);
		hbox->set_h_size_flags(SIZE_EXPAND_FILL);
		container->add_child(hbox);

		auto mix_property = memnew(SpineEditorPropertyAnimationMix);
		mix_property->set_h_size_flags(SIZE_EXPAND_FILL);
		mix_property->set_name_split_ratio(0);
		hbox->add_child(mix_property);
		mix_property->setup(this, skeleton_data, i);
		mix_property->set_object_and_property(*array_object, property_name);
		mix_property->update_property();
		mix_properties.push_back(mix_property);

		auto delete_button = memnew(Button);
		hbox->add_child(delete_button);
		delete_button->set_text("Remove");
#if VERSION_MAJOR > 3
		delete_button->connect(SNAME("pressed"), callable_mp(this, &SpineEditorPropertyAnimationMixes::delete_mix).bind(i));
#else
		delete_button->connect(SNAME("pressed"), this, SNAME("delete_mix"), varray(i));
#endif
	}

	auto add_mix_button = memnew(Button);
	add_mix_button->set_text("Add mix");
#if VERSION_MAJOR > 3
	add_mix_button->connect(SNAME("pressed"), callable_mp(this, &SpineEditorPropertyAnimationMixes::add_mix));
#else
	add_mix_button->connect(SNAME("pressed"), this, SNAME("add_mix"));
#endif
	container->add_child(add_mix_button);

	updating = false;
}

SpineEditorPropertyAnimationMix::SpineEditorPropertyAnimationMix()
	: mixes_property(nullptr), skeleton_data(nullptr), index(0), container(nullptr), updating(false) {
}

void SpineEditorPropertyAnimationMix::setup(SpineEditorPropertyAnimationMixes *_mixes_property, const Ref<SpineSkeletonDataResource> &_skeleton_data,
											int _index) {
	this->mixes_property = _mixes_property;
	this->skeleton_data = _skeleton_data;
	this->index = _index;
}

void SpineEditorPropertyAnimationMix::_bind_methods() {
	ClassDB::bind_method(D_METHOD("data_changed"), &SpineEditorPropertyAnimationMix::data_changed);
}

void SpineEditorPropertyAnimationMix::data_changed(const String &property, const Variant &value, const String &name, bool changing) {
	auto mix = Object::cast_to<SpineAnimationMix>(get_edited_object()->get(get_edited_property()));

#if VERSION_MAJOR > 3
	auto undo_redo = EditorUndoRedoManager::get_singleton();
#else
	auto undo_redo = EditorNode::get_undo_redo();
#endif
	undo_redo->create_action("Set mix property " + property);
	undo_redo->add_do_property(mix, property, value);
	undo_redo->add_undo_property(mix, property, mix->get(property));
	undo_redo->add_do_method(mixes_property, "update_mix_property", index);
	undo_redo->add_undo_method(mixes_property, "update_mix_property", index);
	// temporarily disable rebuilding the UI, as commit_action() calls update() which calls update_property(). however,
	// data_changed is invoked by the control that changed the property, which would get deleted in update_property().
	updating = true;
	undo_redo->commit_action();
	updating = false;
	emit_changed(property, value, name, changing);
}

void SpineEditorPropertyAnimationMix::update_property() {
	if (updating) return;
	updating = true;

	if (container) {
		memdelete(container);
#if VERSION_MAJOR > 3
		SceneTree::get_singleton()->queue_delete(container);
#else
		container->queue_delete();
#endif
		container = nullptr;
	}

	if (!skeleton_data.is_valid() || !skeleton_data->is_skeleton_data_loaded()) {
		updating = false;
		return;
	}

	auto mix = Object::cast_to<SpineAnimationMix>(get_edited_object()->get(get_edited_property()));
	if (!mix) {
		updating = false;
		return;
	}

	Vector<String> animation_names;
	skeleton_data->get_animation_names(animation_names);

	container = memnew(HBoxContainer);
	container->set_h_size_flags(SIZE_EXPAND_FILL);
	add_child(container);

	auto from_enum = memnew(EditorPropertyTextEnum);
	from_enum->set_h_size_flags(SIZE_EXPAND_FILL);
	from_enum->set_name_split_ratio(0);
	from_enum->set_selectable(false);
	from_enum->setup(animation_names);
	from_enum->set_object_and_property(mix, "from");
	from_enum->update_property();
#if VERSION_MAJOR > 3
	from_enum->connect(SNAME("property_changed"), callable_mp(this, &SpineEditorPropertyAnimationMix::data_changed));
#else
	from_enum->connect(SNAME("property_changed"), this, SNAME("data_changed"));
#endif
	container->add_child(from_enum);

	auto to_enum = memnew(EditorPropertyTextEnum);
	to_enum->set_h_size_flags(SIZE_EXPAND_FILL);
	to_enum->set_name_split_ratio(0);
	to_enum->set_selectable(false);
	to_enum->setup(animation_names);
	to_enum->set_object_and_property(mix, "to");
	to_enum->update_property();
#if VERSION_MAJOR > 3
	to_enum->connect(SNAME("property_changed"), callable_mp(this, &SpineEditorPropertyAnimationMix::data_changed));
#else
	to_enum->connect(SNAME("property_changed"), this, SNAME("data_changed"));
#endif
	container->add_child(to_enum);

	auto mix_float = memnew(EditorPropertyFloat);
	mix_float->set_h_size_flags(SIZE_EXPAND_FILL);
	mix_float->set_name_split_ratio(0);
	mix_float->set_selectable(false);
#if (VERSION_MAJOR >= 4 && VERSION_MINOR >= 6)
	EditorPropertyRangeHint range_hint;
	range_hint.min = 0;
	range_hint.max = 9999999;
	range_hint.step = 0.001;
	range_hint.or_greater = true;
	range_hint.or_less = false;
	range_hint.exp_range = false;
	range_hint.hide_control = false;
	range_hint.radians_as_degrees = false;
	mix_float->setup(range_hint);
#else
	mix_float->setup(0, 9999999, 0.001, true, false, false, false);
#endif
	mix_float->set_object_and_property(mix, "mix");
	mix_float->update_property();
#if VERSION_MAJOR > 3
	mix_float->connect(SNAME("property_changed"), callable_mp(this, &SpineEditorPropertyAnimationMix::data_changed));
#else
	mix_float->connect(SNAME("property_changed"), this, SNAME("data_changed"));
#endif
	container->add_child(mix_float);

	updating = false;
}
#else
SpineEditorPropertyAnimationMixes::SpineEditorPropertyAnimationMixes() : container(nullptr), updating(false) {
}

void SpineEditorPropertyAnimationMixes::_bind_methods() {
	ClassDB::bind_method(D_METHOD("rebuild_ui"), &SpineEditorPropertyAnimationMixes::rebuild_ui);
}

void SpineEditorPropertyAnimationMixes::setup(const Ref<SpineSkeletonDataResource> &_skeleton_data) {
	skeleton_data = _skeleton_data;
	rebuild_ui();
}

void SpineEditorPropertyAnimationMixes::_update_property() {
	if (updating) return;
	rebuild_ui();
}

void SpineEditorPropertyAnimationMixes::rebuild_ui() {
	updating = true;

	if (container) {
		set_bottom_editor(nullptr);
		remove_child(container);
		memdelete(container);
		container = nullptr;
	}

	if (!skeleton_data.is_valid() || !skeleton_data->is_skeleton_data_loaded()) {
		updating = false;
		return;
	}

	PackedStringArray animation_names;
	skeleton_data->get_animation_names(animation_names);

	container = memnew(VBoxContainer);
	add_child(container);
	set_bottom_editor(container);

	Array mixes = skeleton_data->get_animation_mixes();
	for (int i = 0; i < mixes.size(); i++) {
		Ref<SpineAnimationMix> mix = mixes[i];

		auto hbox = memnew(HBoxContainer);
		hbox->set_h_size_flags(SIZE_EXPAND_FILL);
		container->add_child(hbox);

		if (mix.is_null()) {
			auto label = memnew(Label);
			label->set_text("Invalid mix");
			label->set_h_size_flags(SIZE_EXPAND_FILL);
			hbox->add_child(label);
		} else {
			auto from_option = memnew(OptionButton);
			from_option->set_h_size_flags(SIZE_EXPAND_FILL);
			for (int j = 0; j < animation_names.size(); j++) {
				from_option->add_item(animation_names[j]);
				if (animation_names[j] == mix->get_from()) from_option->select(j);
			}
			from_option->connect(SNAME("item_selected"), callable_mp(this, &SpineEditorPropertyAnimationMixes::on_from_changed).bind(i),
								 CONNECT_DEFERRED);
			hbox->add_child(from_option);

			auto to_option = memnew(OptionButton);
			to_option->set_h_size_flags(SIZE_EXPAND_FILL);
			for (int j = 0; j < animation_names.size(); j++) {
				to_option->add_item(animation_names[j]);
				if (animation_names[j] == mix->get_to()) to_option->select(j);
			}
			to_option->connect(SNAME("item_selected"), callable_mp(this, &SpineEditorPropertyAnimationMixes::on_to_changed).bind(i),
							   CONNECT_DEFERRED);
			hbox->add_child(to_option);

			auto spin_box = memnew(SpinBox);
			spin_box->set_h_size_flags(SIZE_EXPAND_FILL);
			spin_box->set_min(0.0);
			spin_box->set_max(9999999.0);
			spin_box->set_step(0.001);
			spin_box->set_value(mix->get_mix());
			spin_box->connect(SNAME("value_changed"), callable_mp(this, &SpineEditorPropertyAnimationMixes::on_mix_value_changed).bind(i),
							  CONNECT_DEFERRED);
			hbox->add_child(spin_box);
		}

		auto delete_button = memnew(Button);
		delete_button->set_text("Remove");
		delete_button->connect(SNAME("pressed"), callable_mp(this, &SpineEditorPropertyAnimationMixes::delete_mix).bind(i), CONNECT_DEFERRED);
		hbox->add_child(delete_button);
	}

	auto add_mix_button = memnew(Button);
	add_mix_button->set_text("Add mix");
	add_mix_button->set_disabled(animation_names.is_empty());
	add_mix_button->connect(SNAME("pressed"), callable_mp(this, &SpineEditorPropertyAnimationMixes::add_mix), CONNECT_DEFERRED);
	container->add_child(add_mix_button);

	updating = false;
}

void SpineEditorPropertyAnimationMixes::add_mix() {
	if (updating || !skeleton_data.is_valid() || !skeleton_data->is_skeleton_data_loaded()) return;

	PackedStringArray animation_names;
	skeleton_data->get_animation_names(animation_names);
	if (animation_names.is_empty()) return;

	Ref<SpineAnimationMix> mix(memnew(SpineAnimationMix));
	mix->set_from(animation_names[0]);
	mix->set_to(animation_names[0]);
	mix->set_mix(0);

	Array mixes = skeleton_data->get_animation_mixes().duplicate();
	mixes.push_back(mix);
	emit_changed(get_edited_property(), mixes);
	call_deferred(SNAME("rebuild_ui"));
}

void SpineEditorPropertyAnimationMixes::delete_mix(int idx) {
	if (updating || !skeleton_data.is_valid() || !skeleton_data->is_skeleton_data_loaded()) return;

	Array mixes = skeleton_data->get_animation_mixes().duplicate();
	if (idx < 0 || idx >= mixes.size()) return;
	mixes.remove_at(idx);
	emit_changed(get_edited_property(), mixes);
	call_deferred(SNAME("rebuild_ui"));
}

void SpineEditorPropertyAnimationMixes::on_from_changed(int option_idx, int mix_idx) {
	if (updating || !skeleton_data.is_valid()) return;

	PackedStringArray animation_names;
	skeleton_data->get_animation_names(animation_names);
	if (option_idx < 0 || option_idx >= animation_names.size()) return;

	Array mixes = skeleton_data->get_animation_mixes().duplicate();
	if (mix_idx < 0 || mix_idx >= mixes.size()) return;
	Ref<SpineAnimationMix> old_mix = mixes[mix_idx];
	if (old_mix.is_null()) return;

	Ref<SpineAnimationMix> mix(memnew(SpineAnimationMix));
	mix->set_from(animation_names[option_idx]);
	mix->set_to(old_mix->get_to());
	mix->set_mix(old_mix->get_mix());
	mixes[mix_idx] = mix;
	emit_changed(get_edited_property(), mixes);
	call_deferred(SNAME("rebuild_ui"));
}

void SpineEditorPropertyAnimationMixes::on_to_changed(int option_idx, int mix_idx) {
	if (updating || !skeleton_data.is_valid()) return;

	PackedStringArray animation_names;
	skeleton_data->get_animation_names(animation_names);
	if (option_idx < 0 || option_idx >= animation_names.size()) return;

	Array mixes = skeleton_data->get_animation_mixes().duplicate();
	if (mix_idx < 0 || mix_idx >= mixes.size()) return;
	Ref<SpineAnimationMix> old_mix = mixes[mix_idx];
	if (old_mix.is_null()) return;

	Ref<SpineAnimationMix> mix(memnew(SpineAnimationMix));
	mix->set_from(old_mix->get_from());
	mix->set_to(animation_names[option_idx]);
	mix->set_mix(old_mix->get_mix());
	mixes[mix_idx] = mix;
	emit_changed(get_edited_property(), mixes);
	call_deferred(SNAME("rebuild_ui"));
}

void SpineEditorPropertyAnimationMixes::on_mix_value_changed(float value, int mix_idx) {
	if (updating || !skeleton_data.is_valid()) return;

	Array mixes = skeleton_data->get_animation_mixes().duplicate();
	if (mix_idx < 0 || mix_idx >= mixes.size()) return;
	Ref<SpineAnimationMix> old_mix = mixes[mix_idx];
	if (old_mix.is_null()) return;

	Ref<SpineAnimationMix> mix(memnew(SpineAnimationMix));
	mix->set_from(old_mix->get_from());
	mix->set_to(old_mix->get_to());
	mix->set_mix(value);
	mixes[mix_idx] = mix;
	emit_changed(get_edited_property(), mixes);
	call_deferred(SNAME("rebuild_ui"));
}
#endif

void SpineSpriteInspectorPlugin::_bind_methods() {
	ClassDB::bind_method(D_METHOD("button_clicked"), &SpineSpriteInspectorPlugin::button_clicked);
}

void SpineSpriteInspectorPlugin::button_clicked(const String &button_name) {
}

#ifdef SPINE_GODOT_EXTENSION
bool SpineSpriteInspectorPlugin::_can_handle(Object *object) const {
#else
bool SpineSpriteInspectorPlugin::can_handle(Object *object) {
#endif
	return Object::cast_to<SpineSprite>(object) != nullptr;
}

#ifdef SPINE_GODOT_EXTENSION
void SpineSpriteInspectorPlugin::_parse_begin(Object *object) {
#else
void SpineSpriteInspectorPlugin::parse_begin(Object *object) {
#endif
	sprite = Object::cast_to<SpineSprite>(object);
	if (!sprite) return;
	if (!sprite->get_skeleton_data_res().is_valid() || !sprite->get_skeleton_data_res()->is_skeleton_data_loaded()) return;
}

#endif
