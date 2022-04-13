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

#ifdef TOOLS_ENABLED
#include "SpineEditorPlugin.h"

#include "SpineAtlasResource.h"
#include "SpineSkeletonFileResource.h"

Error SpineAtlasResourceImportPlugin::import(const String &source_file, const String &save_path, const Map<StringName, Variant> &options, List<String> *platform_variants, List<String> *gen_files, Variant *metadata) {
	Ref<SpineAtlasResource> atlas(memnew(SpineAtlasResource));
	atlas->set_normal_texture_prefix(options["normal_map_prefix"]);
	atlas->load_from_atlas_file(source_file);

	String file_name = vformat("%s.%s", save_path, get_save_extension());
	auto error = ResourceSaver::save(file_name, atlas);
	return error;
}

void SpineAtlasResourceImportPlugin::get_import_options(List<ImportOption> *options, int preset) const {
	if (preset == 0) {
		ImportOption op;
		op.option.name = "normal_map_prefix";
		op.option.type = Variant::STRING;
		op.option.hint_string = "String";
		op.default_value = String("n");
		options->push_back(op);
	}
}

Error SpineJsonResourceImportPlugin::import(const String &source_file, const String &save_path, const Map<StringName, Variant> &options, List<String> *platform_variants, List<String> *gen_files, Variant *metadata) {
	Ref<SpineSkeletonFileResource> skeleton_file_res(memnew(SpineSkeletonFileResource));
	skeleton_file_res->load_from_file(source_file);

	String file_name = vformat("%s.%s", save_path, get_save_extension());
	auto error = ResourceSaver::save(file_name, skeleton_file_res);
	return error;
}

Error SpineBinaryResourceImportPlugin::import(const String &source_file, const String &save_path, const Map<StringName, Variant> &options, List<String> *platform_variants, List<String> *gen_files, Variant *metadata) {
	Ref<SpineSkeletonFileResource> skeleton_file_res(memnew(SpineSkeletonFileResource));
	skeleton_file_res->load_from_file(source_file);

	String file_name = vformat("%s.%s", save_path, get_save_extension());
	auto error = ResourceSaver::save(file_name, skeleton_file_res);
	return error;
}

SpineEditorPlugin::SpineEditorPlugin(EditorNode *node) {
	add_import_plugin(memnew(SpineAtlasResourceImportPlugin));
	add_import_plugin(memnew(SpineJsonResourceImportPlugin));
	add_import_plugin(memnew(SpineBinaryResourceImportPlugin));
	add_inspector_plugin(memnew(SpineAnimationMixesInspectorPlugin));
}

SpineEditorPlugin::~SpineEditorPlugin() {
}

bool SpineEditorPlugin::handles(Object *object) const {
	return object->is_class("SpineSprite") || object->is_class("SpineSkeletonDataResource");
}

bool SpineAnimationMixesInspectorPlugin::can_handle(Object *object) {
	return object->is_class("SpineSkeletonDataResource");
}

bool SpineAnimationMixesInspectorPlugin::parse_property(Object *object, Variant::Type type, const String &path,
														PropertyHint hint, const String &hint_text, int usage) {
	if (path == "animation_mixes" && object) {
		auto mixes_property = memnew(SpineEditorPropertyAnimationMixes);
		mixes_property->setup(Object::cast_to<SpineSkeletonDataResource>(object));
		add_property_editor(path, mixes_property);
		return true;
	}
	return false;
}

SpineEditorPropertyAnimationMixes::SpineEditorPropertyAnimationMixes(): skeleton_data(nullptr), container(nullptr), updating(false) {
}

void SpineEditorPropertyAnimationMixes::_bind_methods() {
	ClassDB::bind_method(D_METHOD("add_mix"), &SpineEditorPropertyAnimationMixes::add_mix);
	ClassDB::bind_method(D_METHOD("delete_mix"), &SpineEditorPropertyAnimationMixes::delete_mix);
	ClassDB::bind_method(D_METHOD("property_changed"), &SpineEditorPropertyAnimationMixes::property_changed);
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
	skeleton_data->set_animation_mixes(mixes);
	emit_changed(get_edited_property(), mixes);
}

void SpineEditorPropertyAnimationMixes::delete_mix(int64_t idx) {
	if (!skeleton_data.is_valid() || !skeleton_data->is_skeleton_data_loaded() || updating) return;

	auto mixes = skeleton_data->get_animation_mixes().duplicate();
	mixes.remove(idx);
	skeleton_data->set_animation_mixes(mixes);
	emit_changed(get_edited_property(), mixes);
}

void SpineEditorPropertyAnimationMixes::property_changed(const String &property, Variant value, const String &name, bool changing, Ref<SpineAnimationMix> mix) {
	if (property == "from") mix->set_from(value);
	if (property == "to") mix->set_to(value);
	if (property == "mix") mix->set_mix(value);
	emit_changed(get_edited_property(), skeleton_data->get_animation_mixes().duplicate());
}

void SpineEditorPropertyAnimationMixes::update_property() {
	if (updating) return;
	updating = true;
	
	if (container) {
		set_bottom_editor(nullptr);
		memdelete(container);
		container->queue_delete();
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
	for (int i = 0; i < mixes.size(); i++) {
		Ref<SpineAnimationMix> mix = mixes[i];
		auto hbox = memnew(HBoxContainer);
		hbox->set_h_size_flags(SIZE_FILL);

		auto from_enum = memnew(EditorPropertyTextEnum);
		from_enum->set_h_size_flags(SIZE_EXPAND_FILL);
		from_enum->setup(animation_names);
		from_enum->set_object_and_property(*mix, "from");
		from_enum->update_property();
		from_enum->connect("property_changed", this, "property_changed", varray(mix));
		hbox->add_child(from_enum);

		auto to_enum = memnew(EditorPropertyTextEnum);
		to_enum->set_h_size_flags(SIZE_EXPAND_FILL);
		to_enum->setup(animation_names);
		to_enum->set_object_and_property(*mix, "to");
		to_enum->update_property();
		to_enum->connect("property_changed", this, "property_changed", varray(mix));
		hbox->add_child(to_enum);

		auto mix_float = memnew(EditorPropertyFloat);
		mix_float->set_h_size_flags(SIZE_EXPAND_FILL);
		mix_float->setup(0, 9999999, 0.001, true, false, false, false);
		mix_float->set_object_and_property(*mix, "mix");
		mix_float->update_property();
		mix_float->connect("property_changed", this, "property_changed", varray(mix));
		hbox->add_child(mix_float);

		auto delete_button = memnew(Button);
		delete_button->set_text("Delete");
		delete_button->connect("pressed", this, "delete_mix", varray(i));
		hbox->add_child(delete_button);
		
		container->add_child(hbox);
	}

	auto add_mix_button = memnew(Button);
	add_mix_button->set_text("Add mix");
	add_mix_button->set_h_size_flags(SIZE_EXPAND_FILL);
	add_mix_button->connect("pressed", this, "add_mix");
	container->add_child(add_mix_button);

	updating = false;
}


#endif
