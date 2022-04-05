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

#ifndef GODOT_SPINERUNTIMEEDITORPLUGIN_H
#define GODOT_SPINERUNTIMEEDITORPLUGIN_H

#ifdef TOOLS_ENABLED
#include "editor/editor_node.h"

class SpineAtlasResourceImportPlugin : public EditorImportPlugin {
	GDCLASS(SpineAtlasResourceImportPlugin, EditorImportPlugin);

public:
	String get_importer_name() const override { return "spine.atlas"; }
	String get_visible_name() const override { return "Spine Runtime Atlas"; }
	void get_recognized_extensions(List<String> *p_extensions) const override { p_extensions->push_back("atlas"); }
	String get_preset_name(int p_idx) const override {
		if (p_idx == 0) return "Default";
		else
			return "Unknown";
	}
	int get_preset_count() const override { return 1; }
	String get_save_extension() const override { return "spatlas"; }
	String get_resource_type() const override { return "SpineAtlasResource"; }
	void get_import_options(List<ImportOption> *r_options, int p_preset) const override;
	bool get_option_visibility(const String &p_option, const Map<StringName, Variant> &p_options) const override { return true; }
	Error import(const String &p_source_file, const String &p_save_path, const Map<StringName, Variant> &p_options, List<String> *r_platform_variants, List<String> *r_gen_files, Variant *r_metadata) override;
};

class SpineJsonResourceImportPlugin : public EditorImportPlugin {
	GDCLASS(SpineJsonResourceImportPlugin, EditorImportPlugin);

public:
	String get_importer_name() const override { return "spine.json"; }
	String get_visible_name() const override { return "Spine Runtime Json"; }
	void get_recognized_extensions(List<String> *p_extensions) const override { p_extensions->push_back("json"); }
	String get_preset_name(int p_idx) const override {
		if (p_idx == 0) return "Default";
		else
			return "Unknown";
	}
	int get_preset_count() const override { return 1; }
	String get_save_extension() const override { return "spjson"; }
	String get_resource_type() const override { return "SpineSkeletonFileResource"; }
	void get_import_options(List<ImportOption> *r_options, int p_preset) const override {}
	bool get_option_visibility(const String &p_option, const Map<StringName, Variant> &p_options) const override { return true; }
	Error import(const String &p_source_file, const String &p_save_path, const Map<StringName, Variant> &p_options, List<String> *r_platform_variants, List<String> *r_gen_files, Variant *r_metadata) override;
};

class SpineBinaryResourceImportPlugin : public EditorImportPlugin {
GDCLASS(SpineBinaryResourceImportPlugin, EditorImportPlugin);

public:
	String get_importer_name() const override { return "spine.skel"; }
	String get_visible_name() const override { return "Spine Runtime Binary"; }
	void get_recognized_extensions(List<String> *p_extensions) const override { p_extensions->push_back("skel"); }
	String get_preset_name(int p_idx) const override {
		if (p_idx == 0) return "Default";
		else
			return "Unknown";
	}
	int get_preset_count() const override { return 1; }
	String get_save_extension() const override { return "spskel"; }
	String get_resource_type() const override { return "SpineSkeletonFileResource"; }
	void get_import_options(List<ImportOption> *r_options, int p_preset) const override {}
	bool get_option_visibility(const String &p_option, const Map<StringName, Variant> &p_options) const override { return true; }
	Error import(const String &p_source_file, const String &p_save_path, const Map<StringName, Variant> &p_options, List<String> *r_platform_variants, List<String> *r_gen_files, Variant *r_metadata) override;
};

class SpineRuntimeEditorPlugin : public EditorPlugin {
	GDCLASS(SpineRuntimeEditorPlugin, EditorPlugin);

public:
	SpineRuntimeEditorPlugin(EditorNode *p_node);
	~SpineRuntimeEditorPlugin();

	String get_name() const override { return "SpineRuntimeEditorPlugin"; }
	bool has_main_screen() const { return false; }
	bool handles(Object *p_object) const override;
};
#endif

#endif//GODOT_SPINERUNTIMEEDITORPLUGIN_H
