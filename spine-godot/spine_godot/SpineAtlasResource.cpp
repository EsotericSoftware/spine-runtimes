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
#else
#include "core/io/json.h"
#include "scene/resources/texture.h"
#endif
#include <spine/TextureLoader.h>

class GodotSpineTextureLoader : public spine::TextureLoader {

	Array *textures;
	Array *normal_maps;
	String normal_map_prefix;

public:
	GodotSpineTextureLoader(Array *_textures, Array *_normal_maps, const String &normal_map_prefix) : textures(_textures), normal_maps(_normal_maps), normal_map_prefix(normal_map_prefix) {
	}

	static String fix_path(const String &path) {
		if (SSIZE(path) > 5 && path[4] == '/' && path[5] == '/') return path;
		const String prefix = "res:/";
		auto i = path.find(prefix);
		auto sub_str_pos = i + SSIZE(prefix) - 1;
		if (sub_str_pos < 0) return path;
		auto res = path.substr(sub_str_pos);

		if (!EMPTY(res)) {
			if (res[0] != '/') {
				return prefix + String("/") + res;
			} else {
				return prefix + res;
			}
		}
		return path;
	}

	void load(spine::AtlasPage &page, const spine::String &path) override {
		Error error = OK;
		auto fixed_path = fix_path(String(path.buffer()));

#if SPINE_GODOT_EXTENSION
		// FIXME no error parameter
		Ref<Texture2D> texture = ResourceLoader::get_singleton()->load(fixed_path, "", ResourceLoader::CACHE_MODE_REUSE);
#else
#if VERSION_MAJOR > 3
		Ref<Texture2D> texture = ResourceLoader::load(fixed_path, "", ResourceFormatLoader::CACHE_MODE_REUSE, &error);
#else
		Ref<Texture> texture = ResourceLoader::load(fixed_path, "", false, &error);
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
			Ref<Texture> normal_map = ResourceLoader::load(new_path);
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
	Error err;
	source_path = path;
	atlas_data = FileAccess::get_file_as_string(path, &err);
	if (err != OK) return err;

	clear();
	texture_loader = new GodotSpineTextureLoader(&textures, &normal_maps, normal_map_prefix);
	auto atlas_utf8 = atlas_data.utf8();
	atlas = new spine::Atlas(atlas_utf8, atlas_utf8.length(), source_path.get_base_dir().utf8(), texture_loader);
	if (atlas) return OK;

	clear();
	return ERR_FILE_UNRECOGNIZED;
}

Error SpineAtlasResource::load_from_file(const String &path) {
	Error error;
	String json_string = FileAccess::get_file_as_string(path, &error);
	if (error != OK) return error;

#if VERSION_MAJOR > 3
	JSON json;
	error = json.parse(json_string);
	if (error != OK) return error;
	Variant result = json.get_data();
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
	texture_loader = new GodotSpineTextureLoader(&textures, &normal_maps, normal_map_prefix);
	auto utf8 = atlas_data.utf8();
	atlas = new spine::Atlas(utf8.ptr(), utf8.size(), source_path.get_base_dir().utf8(), texture_loader);
	if (atlas) return OK;

	clear();
	return ERR_FILE_UNRECOGNIZED;
}

Error SpineAtlasResource::save_to_file(const String &path) {
	Error err;
#if VERSION_MAJOR > 3
	Ref<FileAccess> file = FileAccess::open(path, FileAccess::WRITE, &err);
	if (err != OK) return err;
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
	JSON json;
	file->store_string(json.stringify(content));
	file->flush();
#else
	file->store_string(JSON::print(content));
	file->close();
#endif
	return OK;
}

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

#if VERSION_MAJOR > 3
RES SpineAtlasResourceFormatLoader::load(const String &path, const String &original_path, Error *error, bool use_sub_threads, float *progress, CacheMode cache_mode) {
#else
RES SpineAtlasResourceFormatLoader::load(const String &path, const String &original_path, Error *error) {
#endif
	Ref<SpineAtlasResource> atlas = memnew(SpineAtlasResource);
	atlas->load_from_file(path);
	if (error) *error = OK;
	return atlas;
}

void SpineAtlasResourceFormatLoader::get_recognized_extensions(List<String> *extensions) const {
	const char atlas_ext[] = "spatlas";
	if (!extensions->find(atlas_ext))
		extensions->push_back(atlas_ext);
}

String SpineAtlasResourceFormatLoader::get_resource_type(const String &path) const {
	return path.ends_with("spatlas") || path.ends_with(".atlas") ? "SpineAtlasResource" : "";
}

bool SpineAtlasResourceFormatLoader::handles_type(const String &type) const {
	return type == "SpineAtlasResource" || ClassDB::is_parent_class(type, "SpineAtlasResource");
}

#if VERSION_MAJOR > 3
Error SpineAtlasResourceFormatSaver::save(const RES &resource, const String &path, uint32_t flags) {
#else
Error SpineAtlasResourceFormatSaver::save(const String &path, const RES &resource, uint32_t flags) {
#endif
	Ref<SpineAtlasResource> res = resource;
	return res->save_to_file(path);
}

void SpineAtlasResourceFormatSaver::get_recognized_extensions(const RES &resource, List<String> *extensions) const {
	if (Object::cast_to<SpineAtlasResource>(*resource))
		extensions->push_back("spatlas");
}

bool SpineAtlasResourceFormatSaver::recognize(const RES &resource) const {
	return Object::cast_to<SpineAtlasResource>(*resource) != nullptr;
}
