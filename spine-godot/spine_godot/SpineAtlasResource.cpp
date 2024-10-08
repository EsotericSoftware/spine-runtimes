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

#include "SpineAtlasResource.h"
#include "SpineRendererObject.h"

#ifdef SPINE_GODOT_EXTENSION
#include <godot_cpp/classes/json.hpp>
#include <godot_cpp/classes/texture.hpp>
#include <godot_cpp/classes/file_access.hpp>
#include <godot_cpp/classes/image.hpp>
#include <godot_cpp/classes/image_texture.hpp>
#else
#include "core/io/json.h"
#include "scene/resources/texture.h"
#if VERSION_MAJOR > 3
#include "core/io/image.h"
#include "scene/resources/image_texture.h"
#else
#include "core/image.h"
#endif
#endif

#ifdef TOOLS_ENABLED
#ifdef SPINE_GODOT_EXTENSION
#include <godot_cpp/classes/editor_file_system.hpp>
#else
#include "editor/editor_file_system.h"
#endif
#endif

#include <spine/TextureLoader.h>

class GodotSpineTextureLoader : public spine::TextureLoader {

	Array *textures;
	Array *normal_maps;
	String normal_map_prefix;
	bool is_importing;

public:
	GodotSpineTextureLoader(Array *_textures, Array *_normal_maps, const String &normal_map_prefix, bool is_importing) : textures(_textures), normal_maps(_normal_maps), normal_map_prefix(normal_map_prefix), is_importing(is_importing) {
	}

	static bool fix_path(String &path) {
		const String prefix = "res:/";
		auto i = path.find(prefix);
		if (i == std::string::npos) {
			return false;
		}

		auto sub_str_pos = i + SSIZE(prefix) - 1;
		auto res = path.substr(sub_str_pos);
		if (!EMPTY(res)) {
			if (res[0] != '/') {
				path = prefix + String("/") + res;
			} else {
				path = prefix + res;
			}
		}
		return true;
	}

#if VERSION_MAJOR > 3
	Ref<Texture2D> get_texture_from_image(const String &path, bool is_resource) {
		Error error = OK;
		if (is_resource) {
#ifdef SPINE_GODOT_EXTENSION
			return ResourceLoader::get_singleton()->load(path, "", ResourceLoader::CACHE_MODE_REUSE);
#else
			return ResourceLoader::load(path, "", ResourceFormatLoader::CACHE_MODE_REUSE, &error);
#endif
		} else {
			Ref<Image> img;
			img.instantiate();
			img = img->load_from_file(path);
			return ImageTexture::create_from_image(img);
		}
	}
#else
	Ref<Texture> get_texture_from_image(const String &path, bool is_resource) {
		Error error = OK;
		if (is_resource) {
			return ResourceLoader::load(path, "", false, &error);
		} else {
			Vector<uint8_t> buf = FileAccess::get_file_as_array(path, &error);
			if (error == OK) {
				Ref<Image> img;
				INSTANTIATE(img);
				img->load(path);

				Ref<ImageTexture> texture;
				INSTANTIATE(texture);
				texture->create_from_image(img);
				return texture;
			}
			return Ref<Texture>();
		}
	}
#endif

	void import_image_resource(const String &path) {
#if VERSION_MAJOR > 4
#ifdef TOOLS_ENABLED
		// Required when importing into editor by e.g. drag & drop. The .png files
		// of the atlas might not have been imported yet.
		// See https://github.com/EsotericSoftware/spine-runtimes/issues/2385
		if (is_importing) {
			HashMap<StringName, Variant> custom_options;
			Dictionary generator_parameters;
			EditorFileSystem::get_singleton()->reimport_append(path, custom_options, "", generator_parameters);
		}
#endif
#endif
	}

	void load(spine::AtlasPage &page, const spine::String &path) override {
		Error error = OK;
		String fixed_path = String(path.buffer());
		bool is_resource = fix_path(fixed_path);

		import_image_resource(fixed_path);

#if SPINE_GODOT_EXTENSION
		// FIXME no error parameter
		Ref<Texture2D> texture = ResourceLoader::get_singleton()->load(fixed_path, "", ResourceLoader::CACHE_MODE_REUSE);
#else
#if VERSION_MAJOR > 3
		Ref<Texture2D> texture = get_texture_from_image(fixed_path, is_resource);
#else
		Ref<Texture> texture = get_texture_from_image(fixed_path, is_resource);
#endif
#endif
		if (error != OK || !texture.is_valid()) {
			ERR_PRINT(vformat("Can't load texture: \"%s\"", String(path.buffer())));
			auto renderer_object = memnew(SpineRendererObject);
			renderer_object->texture = Ref<Texture>(nullptr);
			renderer_object->normal_map = Ref<Texture>(nullptr);
			page.texture = (void *) renderer_object;
			return;
		}

		textures->append(texture);
		auto renderer_object = memnew(SpineRendererObject);
		renderer_object->texture = texture;
		renderer_object->normal_map = Ref<Texture>(nullptr);

		String new_path = vformat("%s/%s_%s", fixed_path.get_base_dir(), normal_map_prefix, fixed_path.get_file());
#if SPINE_GODOT_EXTENSION
		if (ResourceLoader::get_singleton()->exists(new_path)) {
			Ref<Texture> normal_map = ResourceLoader::get_singleton()->load(new_path);
			normal_maps->append(normal_map);
			renderer_object->normal_map = normal_map;
		}
#else
		if (ResourceLoader::exists(new_path)) {
			import_image_resource(new_path);
			Ref<Texture> normal_map = get_texture_from_image(new_path, is_resource);
			normal_maps->append(normal_map);
			renderer_object->normal_map = normal_map;
		}
#endif

#if VERSION_MAJOR > 3
		renderer_object->canvas_texture.instantiate();
		renderer_object->canvas_texture->set_diffuse_texture(renderer_object->texture);
		renderer_object->canvas_texture->set_normal_texture(renderer_object->normal_map);
#endif

		page.texture = (void *) renderer_object;
		page.width = texture->get_width();
		page.height = texture->get_height();
	}

	void unload(void *data) override {
		auto renderer_object = (SpineRendererObject *) data;
		if (renderer_object->texture.is_valid()) renderer_object->texture.unref();
		if (renderer_object->normal_map.is_valid()) renderer_object->normal_map.unref();
#if VERSION_MAJOR > 3
		if (renderer_object->canvas_texture.is_valid()) renderer_object->canvas_texture.unref();
#endif
		memdelete(renderer_object);
	}
};

void SpineAtlasResource::_bind_methods() {
	ClassDB::bind_method(D_METHOD("load_from_atlas_file", "path"), &SpineAtlasResource::load_from_atlas_file);
	ClassDB::bind_method(D_METHOD("get_source_path"), &SpineAtlasResource::get_source_path);
	ClassDB::bind_method(D_METHOD("get_textures"), &SpineAtlasResource::get_textures);
	ClassDB::bind_method(D_METHOD("get_normal_maps"), &SpineAtlasResource::get_normal_maps);

	ADD_PROPERTY(PropertyInfo(Variant::STRING, "source_path"), "", "get_source_path");
	ADD_PROPERTY(PropertyInfo(Variant::ARRAY, "textures"), "", "get_textures");
	ADD_PROPERTY(PropertyInfo(Variant::ARRAY, "normal_maps"), "", "get_normal_maps");

	ADD_SIGNAL(MethodInfo("skeleton_atlas_changed"));
}

SpineAtlasResource::SpineAtlasResource() : atlas(nullptr), texture_loader(nullptr), normal_map_prefix("n") {
}

SpineAtlasResource::~SpineAtlasResource() {
	delete atlas;
	delete texture_loader;
}

void SpineAtlasResource::clear() {
	delete atlas;
	atlas = nullptr;
	delete texture_loader;
	texture_loader = nullptr;
	textures.clear();
	normal_maps.clear();
}

Array SpineAtlasResource::get_textures() {
	return textures;
}

Array SpineAtlasResource::get_normal_maps() {
	return normal_maps;
}

String SpineAtlasResource::get_source_path() {
	return source_path;
}

Error SpineAtlasResource::load_from_atlas_file(const String &path) {
	return load_from_atlas_file_internal(path, false);
}

Error SpineAtlasResource::load_from_atlas_file_internal(const String &path, bool is_importing) {
	source_path = path;
#ifdef SPINE_GODOT_EXTENSION
	atlas_data = FileAccess::get_file_as_string(path);
	if (SSIZE(atlas_data) == 0) return ERR_FILE_UNRECOGNIZED;
#else
	Error err;
	atlas_data = FileAccess::get_file_as_string(path, &err);
	if (err != OK) return err;
#endif

	clear();
	texture_loader = new GodotSpineTextureLoader(&textures, &normal_maps, normal_map_prefix, is_importing);
	auto atlas_utf8 = atlas_data.utf8();
	atlas = new spine::Atlas(atlas_utf8, atlas_utf8.length(), source_path.get_base_dir().utf8(), texture_loader);
	if (atlas) return OK;

	clear();
	return ERR_FILE_UNRECOGNIZED;
}

Error SpineAtlasResource::load_from_file(const String &path) {
	Error error;
#ifdef SPINE_GODOT_EXTENSION
	String json_string = FileAccess::get_file_as_string(path);
	if (SSIZE(json_string) == 0) return ERR_FILE_UNRECOGNIZED;
#else
	String json_string = FileAccess::get_file_as_string(path, &error);
	if (error != OK) return error;
#endif

#if VERSION_MAJOR > 3
	JSON *json = memnew(JSON);
	error = json->parse(json_string);
	if (error != OK) {
		memdelete(json);
		return error;
	}
	Variant result = json->get_data();
	memdelete(json);
#else
	String error_string;
	int error_line;
	Variant result;
	error = JSON::parse(json_string, result, error_string, error_line);
	if (error != OK) return error;
#endif

	Dictionary content = Dictionary(result);
	source_path = content["source_path"];
	atlas_data = content["atlas_data"];
	normal_map_prefix = content["normal_texture_prefix"];

	clear();
	texture_loader = new GodotSpineTextureLoader(&textures, &normal_maps, normal_map_prefix, false);
	auto utf8 = atlas_data.utf8();
	atlas = new spine::Atlas(utf8.ptr(), utf8.size(), source_path.get_base_dir().utf8(), texture_loader);
	if (atlas) return OK;

	clear();
	return ERR_FILE_UNRECOGNIZED;
}

Error SpineAtlasResource::save_to_file(const String &path) {
	Error err;
#if VERSION_MAJOR > 3
#if SPINE_GODOT_EXTENSION
	Ref<FileAccess> file = FileAccess::open(path, FileAccess::WRITE);
	if (file.is_null()) return ERR_FILE_UNRECOGNIZED;
#else
	Ref<FileAccess> file = FileAccess::open(path, FileAccess::WRITE, &err);
	if (err != OK) return err;
#endif
#else
	FileAccess *file = FileAccess::open(path, FileAccess::WRITE, &err);
	if (err != OK) {
		if (file) file->close();
		return err;
	}
#endif

	Dictionary content;
	content["source_path"] = source_path;
	content["atlas_data"] = atlas_data;
	content["normal_texture_prefix"] = normal_map_prefix;
#if VERSION_MAJOR > 3
	JSON *json = memnew(JSON);
	file->store_string(json->stringify(content));
	file->flush();
	memdelete(json);
#else
	file->store_string(JSON::print(content));
	file->close();
#endif
	return OK;
}

#ifndef SPINE_GODOT_EXTENSION
#if VERSION_MAJOR > 3
Error SpineAtlasResource::copy_from(const Ref<Resource> &p_resource) {
	auto error = Resource::copy_from(p_resource);
	if (error != OK) return error;

	const Ref<SpineAtlasResource> &spineAtlas = static_cast<const Ref<SpineAtlasResource> &>(p_resource);
	this->clear();
	this->atlas = spineAtlas->atlas;
	this->texture_loader = spineAtlas->texture_loader;
	spineAtlas->clear_native_data();

	this->source_path = spineAtlas->source_path;
	this->atlas_data = spineAtlas->atlas_data;
	this->normal_map_prefix = spineAtlas->normal_map_prefix;
	this->textures = spineAtlas->textures;
	this->normal_maps = spineAtlas->normal_maps;
	emit_signal(SNAME("skeleton_file_changed"));

	return OK;
}
#endif
#endif

#ifdef SPINE_GODOT_EXTENSION
Variant SpineAtlasResourceFormatLoader::_load(const String &path, const String &original_path, bool use_sub_threads, int32_t cache_mode) {
#else
#if VERSION_MAJOR > 3
RES SpineAtlasResourceFormatLoader::load(const String &path, const String &original_path, Error *error, bool use_sub_threads, float *progress, CacheMode cache_mode) {
#else
#if VERSION_MINOR > 5
RES SpineAtlasResourceFormatLoader::load(const String &path, const String &original_path, Error *error, bool p_no_subresource_cache) {
#else
RES SpineAtlasResourceFormatLoader::load(const String &path, const String &original_path, Error *error) {
#endif
#endif
#endif
	Ref<SpineAtlasResource> atlas = memnew(SpineAtlasResource);
	atlas->load_from_file(path);
#ifndef SPINE_GODOT_EXTENSION
	if (error) *error = OK;
#endif
	return atlas;
}


#ifdef SPINE_GODOT_EXTENSION
PackedStringArray SpineAtlasResourceFormatLoader::_get_recognized_extensions() {
	PackedStringArray extensions;
	extensions.push_back("spatlas");
	return extensions;
}
#else
void SpineAtlasResourceFormatLoader::get_recognized_extensions(List<String> *extensions) const {
	const char atlas_ext[] = "spatlas";
	if (!extensions->find(atlas_ext))
		extensions->push_back(atlas_ext);
}
#endif

#ifdef SPINE_GODOT_EXTENSION
String SpineAtlasResourceFormatLoader::_get_resource_type(const String &path) {
#else
String SpineAtlasResourceFormatLoader::get_resource_type(const String &path) const {
#endif
	return path.ends_with("spatlas") || path.ends_with(".atlas") ? "SpineAtlasResource" : "";
}

#ifdef SPINE_GODOT_EXTENSION
bool SpineAtlasResourceFormatLoader::_handles_type(const StringName &type) {
#else
bool SpineAtlasResourceFormatLoader::handles_type(const String &type) const {
#endif
	return type == StringName("SpineAtlasResource") || ClassDB::is_parent_class(type, "SpineAtlasResource");
}

#ifdef SPINE_GODOT_EXTENSION
Error SpineAtlasResourceFormatSaver::_save(const Ref<Resource> &resource, const String &path, uint32_t flags) {
#else
#if VERSION_MAJOR > 3
Error SpineAtlasResourceFormatSaver::save(const RES &resource, const String &path, uint32_t flags) {
#else
Error SpineAtlasResourceFormatSaver::save(const String &path, const RES &resource, uint32_t flags) {
#endif
#endif
	Ref<SpineAtlasResource> res = resource;
	return res->save_to_file(path);
}

#ifdef SPINE_GODOT_EXTENSION
PackedStringArray SpineAtlasResourceFormatSaver::_get_recognized_extensions(const Ref<Resource> &resource) {
	PackedStringArray extensions;
	if (Object::cast_to<SpineAtlasResource>(*resource)) {
		extensions.push_back("spatlas");
	}
	return extensions;
}
#else
void SpineAtlasResourceFormatSaver::get_recognized_extensions(const RES &resource, List<String> *extensions) const {
	if (Object::cast_to<SpineAtlasResource>(*resource))
		extensions->push_back("spatlas");
}
#endif

#ifdef SPINE_GODOT_EXTENSION
bool SpineAtlasResourceFormatSaver::_recognize(const RES &resource) {
#else
bool SpineAtlasResourceFormatSaver::recognize(const RES &resource) const {
#endif
	return Object::cast_to<SpineAtlasResource>(*resource) != nullptr;
}
