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

#include "SpineAtlasResource.h"
#include "SpineRendererObject.h"
#include "core/io/json.h"
#include "scene/resources/texture.h"
#include <spine/TextureLoader.h>

class GodotSpineTextureLoader : public spine::TextureLoader {

	Array *textures;
	Array *normal_maps;
	String normal_map_prefix;

public:
	GodotSpineTextureLoader(Array *_textures, Array *_normal_maps, const String &normal_map_prefix) : textures(_textures), normal_maps(_normal_maps), normal_map_prefix(normal_map_prefix) {
	}

	String fix_path(const String &path) {
		if (path.size() > 5 && path[4] == '/' && path[5] == '/') return path;
		const String prefix = "res:/";
		auto i = path.find(prefix);
		auto sub_str_pos = i + prefix.size() - 1;
		if (sub_str_pos < 0) return path;
		auto res = path.substr(sub_str_pos);

		if (res.size() > 0) {
			if (res[0] != '/') {
				return prefix + "/" + res;
			} else {
				return prefix + res;
			}
		}
		return path;
	}

	virtual void load(spine::AtlasPage &page, const spine::String &path) {
		Error error = OK;
		auto fixed_path = fix_path(String(path.buffer()));

		Ref<Texture> texture = ResourceLoader::load(fixed_path, "", false, &error);
		if (error != OK) {
			ERR_PRINT(vformat("Can't load texture: \"%s\"", String(path.buffer())));
			auto renderer_object = memnew(SpineRendererObject);
			renderer_object->texture = Ref<Texture>(nullptr);
			renderer_object->normal_map = Ref<Texture>(nullptr);
			page.setRendererObject((void *) renderer_object);
			return;
		}

		textures->append(texture);
		auto renderer_object = memnew(SpineRendererObject);
		renderer_object->texture = texture;
		renderer_object->normal_map = Ref<Texture>(nullptr);

		String temp_path = fixed_path;
		String new_path = vformat("%s/%s_%s", temp_path.get_base_dir(), normal_map_prefix, temp_path.get_file());
		if (ResourceLoader::exists(new_path)) {
			Ref<Texture> normal_map = ResourceLoader::load(new_path);
			normal_maps->append(normal_map);
			renderer_object->normal_map = normal_map;
		}

		page.setRendererObject((void *) renderer_object);
		page.width = texture->get_width();
		page.height = texture->get_height();
	}

	virtual void unload(void *data) {
		auto renderer_object = (SpineRendererObject *) data;
		Ref<Texture> &texture = renderer_object->texture;
		if (texture.is_valid()) texture.unref();
		Ref<Texture> &normal_map = renderer_object->normal_map;
		if (normal_map.is_valid()) normal_map.unref();
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
	atlas = new spine::Atlas(atlas_data.utf8(), atlas_data.size(), source_path.get_base_dir().utf8(), texture_loader);
	if (atlas) return OK;

	clear();
	return ERR_FILE_UNRECOGNIZED;
}

Error SpineAtlasResource::load_from_file(const String &path) {
	Error err;
	String json_string = FileAccess::get_file_as_string(path, &err);
	if (err != OK) return err;

	String error_string;
	int error_line;
	JSON json;
	Variant result;
	err = json.parse(json_string, result, error_string, error_line);
	if (err != OK) return err;

	Dictionary content = Dictionary(result);
	source_path = content["source_path"];
	atlas_data = content["atlas_data"];
	normal_map_prefix = content["normal_texture_prefix"];

	clear();
	texture_loader = new GodotSpineTextureLoader(&textures, &normal_maps, normal_map_prefix);
	atlas = new spine::Atlas(atlas_data.utf8(), atlas_data.size(), source_path.get_base_dir().utf8(), texture_loader);
	if (atlas) return OK;

	clear();
	return ERR_FILE_UNRECOGNIZED;
}

Error SpineAtlasResource::save_to_file(const String &path) {
	Error err;
	FileAccess *file = FileAccess::open(path, FileAccess::WRITE, &err);
	if (err != OK) {
		if (file) file->close();
		return err;
	}

	Dictionary content;
	content["source_path"] = source_path;
	content["atlas_data"] = atlas_data;
	content["normal_texture_prefix"] = normal_map_prefix;
	file->store_string(JSON::print(content));
	file->close();
	return OK;
}

RES SpineAtlasResourceFormatLoader::load(const String &path, const String &original_path, Error *error) {
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
	return "SpineAtlasResource";
}

bool SpineAtlasResourceFormatLoader::handles_type(const String &type) const {
	return type == "SpineAtlasResource" || ClassDB::is_parent_class(type, "SpineAtlasResource");
}

Error SpineAtlasResourceFormatSaver::save(const String &path, const RES &resource, uint32_t flags) {
	Ref<SpineAtlasResource> res = resource.get_ref_ptr();
	return res->save_to_file(path);
}

void SpineAtlasResourceFormatSaver::get_recognized_extensions(const RES &resource, List<String> *extensions) const {
	if (Object::cast_to<SpineAtlasResource>(*resource))
		extensions->push_back("spatlas");
}

bool SpineAtlasResourceFormatSaver::recognize(const RES &resource) const {
	return Object::cast_to<SpineAtlasResource>(*resource) != nullptr;
}
