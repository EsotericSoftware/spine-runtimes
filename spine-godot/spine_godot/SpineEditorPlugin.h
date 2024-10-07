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

#pragma once

#ifdef TOOLS_ENABLED
#include "SpineSprite.h"
#if VERSION_MAJOR > 3
#ifdef SPINE_GODOT_EXTENSION
#include <godot_cpp/classes/editor_import_plugin.hpp>
#else
#include "editor/import/editor_import_plugin.h"
#endif
#endif
#ifdef SPINE_GODOT_EXTENSION
#include <godot_cpp/classes/editor_plugin.hpp>
#include <godot_cpp/classes/editor_inspector_plugin.hpp>
#include <godot_cpp/classes/editor_property.hpp>
#else
#include "editor/editor_node.h"
#include "editor/editor_properties.h"
#include "editor/editor_properties_array_dict.h"
#endif

class SpineAtlasResourceImportPlugin : public EditorImportPlugin {
	GDCLASS(SpineAtlasResourceImportPlugin, EditorImportPlugin)

public:
#ifdef SPINE_GODOT_EXTENSION
	static void _bind_methods() {};

	String _get_importer_name() const override { return "spine.atlas"; }

	String _get_visible_name() const override { return "Spine Runtime Atlas"; }

	PackedStringArray _get_recognized_extensions() const override {
		PackedStringArray extensions;
		extensions.push_back("atlas");
		return extensions;
	}

	String _get_preset_name(int idx) const override { return idx == 0 ? "Default" : "Unknown"; }

	int _get_preset_count() const override { return 1; }

	String _get_save_extension() const override { return "spatlas"; }

	String _get_resource_type() const override { return "SpineAtlasResource"; }
#else
	String get_importer_name() const override { return "spine.atlas"; }

	String get_visible_name() const override { return "Spine Runtime Atlas"; }

	void get_recognized_extensions(List<String> *extensions) const override { extensions->push_back("atlas"); }

	String get_preset_name(int idx) const override { return idx == 0 ? "Default" : "Unknown"; }

	int get_preset_count() const override { return 1; }

	String get_save_extension() const override { return "spatlas"; }

	String get_resource_type() const override { return "SpineAtlasResource"; }
#endif

#if VERSION_MAJOR > 3
#ifdef SPINE_GODOT_EXTENSION
	virtual int _get_import_order() const override { return IMPORT_ORDER_DEFAULT; }

	virtual float _get_priority() const override { return 1; }

	virtual bool _get_option_visibility(const String &p_path, const StringName &p_option_name, const Dictionary &p_options) const override { return true; };

	virtual Error _import(const String &p_source_file, const String &p_save_path, const Dictionary &p_options, const TypedArray<String> &p_platform_variants, const TypedArray<String> &p_gen_files) const override;
#else
	int get_import_order() const override { return IMPORT_ORDER_DEFAULT; }

	float get_priority() const override { return 1; }

	void get_import_options(const String &path, List<ImportOption> *options, int preset) const override;

	virtual bool get_option_visibility(const String &path, const String &option, const HashMap<StringName, Variant> &options) const override { return true; }

	Error import(const String &source_file, const String &save_path, const HashMap<StringName, Variant> &options, List<String> *platform_variants, List<String> *gen_files, Variant *metadata) override;
#endif
#else
	void get_import_options(List<ImportOption> *options, int preset) const override;

	bool get_option_visibility(const String &option, const Map<StringName, Variant> &options) const override { return true; }

	Error import(const String &source_file, const String &save_path, const Map<StringName, Variant> &options, List<String> *platform_variants, List<String> *gen_files, Variant *metadata) override;
#endif
};

class SpineJsonResourceImportPlugin : public EditorImportPlugin {
	GDCLASS(SpineJsonResourceImportPlugin, EditorImportPlugin)

public:
#ifdef SPINE_GODOT_EXTENSION
	static void _bind_methods() {};

	String _get_importer_name() const override { return "spine.json"; }

	String _get_visible_name() const override { return "Spine Skeleton Json"; }

	PackedStringArray _get_recognized_extensions() const override {
		PackedStringArray extensions;
		extensions.push_back("spine-json");
		return extensions;
	}

	String _get_preset_name(int idx) const override { return idx == 0 ? "Default" : "Unknown"; }

	int _get_preset_count() const override { return 1; }

	String _get_save_extension() const override { return "spjson"; }

	String _get_resource_type() const override { return "SpineSkeletonFileResource"; }
#else
	String get_importer_name() const override { return "spine.json"; }

	String get_visible_name() const override { return "Spine Skeleton Json"; }

	void get_recognized_extensions(List<String> *extensions) const override { extensions->push_back("spine-json"); }

	String get_preset_name(int idx) const override { return idx == 0 ? "Default" : "Unknown"; }

	int get_preset_count() const override { return 1; }

	String get_save_extension() const override { return "spjson"; }

	String get_resource_type() const override { return "SpineSkeletonFileResource"; }
#endif

#if VERSION_MAJOR > 3
#ifdef SPINE_GODOT_EXTENSION
	int _get_import_order() const override { return IMPORT_ORDER_DEFAULT; }

	float _get_priority() const override { return 1; }

	virtual bool _get_option_visibility(const String &p_path, const StringName &p_option_name, const Dictionary &p_options) const override { return true; };

	virtual Error _import(const String &p_source_file, const String &p_save_path, const Dictionary &p_options, const TypedArray<String> &p_platform_variants, const TypedArray<String> &p_gen_files) const override;
#else
	int get_import_order() const override { return IMPORT_ORDER_DEFAULT; }

	float get_priority() const override { return 1; }

	void get_import_options(const String &path, List<ImportOption> *options, int preset) const override {}

	bool get_option_visibility(const String &path, const String &option, const HashMap<StringName, Variant> &options) const override { return true; }

	Error import(const String &source_file, const String &save_path, const HashMap<StringName, Variant> &options, List<String> *platform_variants, List<String> *gen_files, Variant *metadata) override;
#endif
#else
	void get_import_options(List<ImportOption> *options, int preset) const override {}

	bool get_option_visibility(const String &option, const Map<StringName, Variant> &options) const override { return true; }

	Error import(const String &source_file, const String &save_path, const Map<StringName, Variant> &options, List<String> *platform_variants, List<String> *gen_files, Variant *metadata) override;
#endif
};

class SpineBinaryResourceImportPlugin : public EditorImportPlugin {
	GDCLASS(SpineBinaryResourceImportPlugin, EditorImportPlugin);

public:
#ifdef SPINE_GODOT_EXTENSION
	static void _bind_methods() {};

	String _get_importer_name() const override { return "spine.skel"; }

	String _get_visible_name() const override { return "Spine Skeleton Binary"; }

	PackedStringArray _get_recognized_extensions() const override {
		PackedStringArray extensions;
		extensions.push_back("skel");
		return extensions;
	}

	String _get_preset_name(int idx) const override { return idx == 0 ? "Default" : "Unknown"; }

	int _get_preset_count() const override { return 1; }

	String _get_save_extension() const override { return "spskel"; }

	String _get_resource_type() const override { return "SpineSkeletonFileResource"; }
#else
	String get_importer_name() const override { return "spine.skel"; }

	String get_visible_name() const override { return "Spine Skeleton Binary"; }

	void get_recognized_extensions(List<String> *extensions) const override { extensions->push_back("skel"); }

	String get_preset_name(int idx) const override { return idx == 0 ? "Default" : "Unknown"; }

	int get_preset_count() const override { return 1; }

	String get_save_extension() const override { return "spskel"; }

	String get_resource_type() const override { return "SpineSkeletonFileResource"; }
#endif

#if VERSION_MAJOR > 3
#ifdef SPINE_GODOT_EXTENSION
	int _get_import_order() const override { return IMPORT_ORDER_DEFAULT; }

	float _get_priority() const override { return 1; }

	virtual bool _get_option_visibility(const String &p_path, const StringName &p_option_name, const Dictionary &p_options) const override { return true; };

	virtual Error _import(const String &p_source_file, const String &p_save_path, const Dictionary &p_options, const TypedArray<String> &p_platform_variants, const TypedArray<String> &p_gen_files) const override;
#else
	int get_import_order() const override { return IMPORT_ORDER_DEFAULT; }

	float get_priority() const override { return 1; }

	void get_import_options(const String &path, List<ImportOption> *options, int preset) const override {}

	bool get_option_visibility(const String &path, const String &option, const HashMap<StringName, Variant> &options) const override { return true; }

	Error import(const String &source_file, const String &save_path, const HashMap<StringName, Variant> &options, List<String> *platform_variants, List<String> *gen_files, Variant *metadata) override;
#endif
#else
	void get_import_options(List<ImportOption> *options, int preset) const override {}

	bool get_option_visibility(const String &option, const Map<StringName, Variant> &options) const override { return true; }

	Error import(const String &source_file, const String &save_path, const Map<StringName, Variant> &options, List<String> *platform_variants, List<String> *gen_files, Variant *metadata) override;
#endif
};

class SpineEditorPlugin : public EditorPlugin {
	GDCLASS(SpineEditorPlugin, EditorPlugin)

	static void _bind_methods() {}

public:
#ifdef SPINE_GODOT_EXTENSION
	explicit SpineEditorPlugin();

	String _get_plugin_name() const override { return "SpineEditorPlugin"; }
#else
	explicit SpineEditorPlugin(EditorNode *node);

	String get_name() const override { return "SpineEditorPlugin"; }
#endif
};

class SpineSkeletonDataResourceInspectorPlugin : public EditorInspectorPlugin {
	GDCLASS(SpineSkeletonDataResourceInspectorPlugin, EditorInspectorPlugin)

public:
#ifdef SPINE_GODOT_EXTENSION
	static void _bind_methods() {}

	bool _can_handle(Object *p_object) const override;
	bool _parse_property(Object *object, Variant::Type type, const String &path, PropertyHint hint, const String &hint_text, const BitField<PropertyUsageFlags> p_usage, bool wide) override;
#else
	bool can_handle(Object *object) override;
#if VERSION_MAJOR > 3
	bool parse_property(Object *object, Variant::Type type, const String &path, PropertyHint hint, const String &hint_text, const BitField<PropertyUsageFlags> p_usage, bool wide) override;
#else
	bool parse_property(Object *object, Variant::Type type, const String &path, PropertyHint hint, const String &hint_text, int usage) override;
#endif
#endif
};

#ifndef SPINE_GODOT_EXTENSION
class SpineEditorPropertyAnimationMix;

class SpineEditorPropertyAnimationMixes : public EditorProperty {
	GDCLASS(SpineEditorPropertyAnimationMixes, EditorProperty)

	Ref<EditorPropertyArrayObject> array_object;
	Ref<SpineSkeletonDataResource> skeleton_data;
	VBoxContainer *container;
	Vector<SpineEditorPropertyAnimationMix *> mix_properties;
	bool updating;

	static void _bind_methods();
	void add_mix();
	void delete_mix(int idx);
	void update_mix_property(int index);

public:
	SpineEditorPropertyAnimationMixes();
	void setup(const Ref<SpineSkeletonDataResource> &_skeleton_data) { this->skeleton_data = _skeleton_data; };
	void update_property() override;
};

class SpineEditorPropertyAnimationMix : public EditorProperty {
	GDCLASS(SpineEditorPropertyAnimationMix, EditorProperty)

	SpineEditorPropertyAnimationMixes *mixes_property;
	Ref<SpineSkeletonDataResource> skeleton_data;
	int index;
	Container *container;
	bool updating;

	static void _bind_methods();
	void data_changed(const String &property, const Variant &value, const String &name, bool changing);

public:
	SpineEditorPropertyAnimationMix();
	void setup(SpineEditorPropertyAnimationMixes *mixes_property, const Ref<SpineSkeletonDataResource> &skeleton_data, int index);
	void update_property() override;
};
#endif

class SpineSpriteInspectorPlugin : public EditorInspectorPlugin {
	GDCLASS(SpineSpriteInspectorPlugin, EditorInspectorPlugin)

	SpineSprite *sprite;

	static void _bind_methods();
	void button_clicked(const String &button_name);

public:
#ifdef SPINE_GODOT_EXTENSION
	virtual bool _can_handle(Object *object) const override;
	virtual void _parse_begin(Object *object) override;
#else
	bool can_handle(Object *object) override;
	void parse_begin(Object *object) override;
#endif
};

#endif
