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
#include "core/io/json.h"

#include <spine/Atlas.h>

class GodotSpineTextureLoader : public spine::TextureLoader {
private:
	Array *textures, *normal_maps;
	String normal_map_prefix;

public:
	GodotSpineTextureLoader(Array *t, Array *nt, const String &p) : textures(t), normal_maps(nt), normal_map_prefix(p) {
		if (textures) textures->clear();
		if (normal_maps) normal_maps->clear();
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
		Error err = OK;
		auto fixed_path = fix_path(String(path.buffer()));

		Ref<Texture> texture = ResourceLoader::load(fixed_path, "", false, &err);
		if (err != OK) {
			print_error(vformat("Can't load texture: \"%s\"", String(path.buffer())));
			page.setRendererObject((void *) memnew(SpineRendererObject{nullptr}));
			return;
		}

		if (textures) textures->append(texture);
		auto spine_renderer_object = memnew(SpineRendererObject);
		spine_renderer_object->texture = texture;

		String temp_path = fixed_path;
		String new_path = vformat("%s/%s_%s", temp_path.get_base_dir(), normal_map_prefix, temp_path.get_file());
		if (ResourceLoader::exists(new_path)) {
			Ref<Texture> normal_map = ResourceLoader::load(new_path);
			if (normal_maps) normal_maps->append(normal_map);
			spine_renderer_object->normal_map = normal_map;
		}

		page.setRendererObject((void *) spine_renderer_object);

		page.width = texture->get_width();
		page.height = texture->get_height();
	}

	virtual void unload(void *p) {
		auto spine_renderer_object = (SpineRendererObject *) p;
		Ref<Texture> &texture = spine_renderer_object->texture;
		if (texture.is_valid()) texture.unref();
		Ref<Texture> &normal_map = spine_renderer_object->normal_map;
		if (normal_map.is_valid()) normal_map.unref();
		memdelete(spine_renderer_object);
	}
};

SpineAtlasResource::SpineAtlasResource() : atlas(nullptr), normal_texture_prefix("n") {}

SpineAtlasResource::~SpineAtlasResource() {
	if (atlas) delete atlas;
}

void SpineAtlasResource::_bind_methods() {
	ClassDB::bind_method(D_METHOD("load_from_atlas_file", "path"), &SpineAtlasResource::load_from_atlas_file);

	ClassDB::bind_method(D_METHOD("get_source_path"), &SpineAtlasResource::get_source_path);

	ClassDB::bind_method(D_METHOD("get_textures"), &SpineAtlasResource::get_textures);
	ClassDB::bind_method(D_METHOD("get_normal_maps"), &SpineAtlasResource::get_normal_maps);

	ADD_PROPERTY(PropertyInfo(Variant::STRING, "source_path"), "", "get_source_path");
	ADD_PROPERTY(PropertyInfo(Variant::ARRAY, "textures"), "", "get_textures");
	ADD_PROPERTY(PropertyInfo(Variant::ARRAY, "normal_maps"), "", "get_normal_maps");
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

Error SpineAtlasResource::load_from_atlas_file(const String &p_path) {
	source_path = p_path;
	Error err;

	atlas_data = FileAccess::get_file_as_string(p_path, &err);
	if (err != OK) return err;

	if (atlas) delete atlas;
	textures.clear();
	normal_maps.clear();
	atlas = new spine::Atlas(atlas_data.utf8(), atlas_data.size(), source_path.get_base_dir().utf8(), new GodotSpineTextureLoader(&textures, &normal_maps, normal_texture_prefix));
	if (atlas) return OK;

	textures.clear();
	normal_maps.clear();
	return ERR_FILE_UNRECOGNIZED;
}

Error SpineAtlasResource::load_from_file(const String &p_path) {
	Error err;
	String json_string = FileAccess::get_file_as_string(p_path, &err);
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
	normal_texture_prefix = content["normal_texture_prefix"];

	if (atlas) delete atlas;
	textures.clear();
	normal_maps.clear();
	atlas = new spine::Atlas(atlas_data.utf8(), atlas_data.size(), source_path.get_base_dir().utf8(), new GodotSpineTextureLoader(&textures, &normal_maps, normal_texture_prefix));
	if (atlas) return OK;

	textures.clear();
	normal_maps.clear();
	return ERR_FILE_UNRECOGNIZED;
}

Error SpineAtlasResource::save_to_file(const String &p_path) {
	Error err;
	FileAccess *file = FileAccess::open(p_path, FileAccess::WRITE, &err);
	if (err != OK) {
		if (file) file->close();
		return err;
	}

	Dictionary content;
	content["source_path"] = source_path;
	content["atlas_data"] = atlas_data;
	content["normal_texture_prefix"] = normal_texture_prefix;

	file->store_string(JSON::print(content));
	file->close();

	return OK;
}

RES SpineAtlasResourceFormatLoader::load(const String &p_path, const String &p_original_path, Error *r_error) {
	Ref<SpineAtlasResource> atlas = memnew(SpineAtlasResource);
	atlas->load_from_file(p_path);

	if (r_error) {
		*r_error = OK;
	}
	return atlas;
}

void SpineAtlasResourceFormatLoader::get_recognized_extensions(List<String> *r_extensions) const {
	const char atlas_ext[] = "spatlas";
	if (!r_extensions->find(atlas_ext)) {
		r_extensions->push_back(atlas_ext);
	}
}

String SpineAtlasResourceFormatLoader::get_resource_type(const String &p_path) const {
	return "SpineAtlasResource";
}

bool SpineAtlasResourceFormatLoader::handles_type(const String &p_type) const {
	return p_type == "SpineAtlasResource" || ClassDB::is_parent_class(p_type, "SpineAtlasResource");
}

Error SpineAtlasResourceFormatSaver::save(const String &p_path, const RES &p_resource, uint32_t p_flags) {
	Ref<SpineAtlasResource> res = p_resource.get_ref_ptr();
	Error error = res->save_to_file(p_path);
	return error;
}

void SpineAtlasResourceFormatSaver::get_recognized_extensions(const RES &p_resource, List<String> *p_extensions) const {
	if (Object::cast_to<SpineAtlasResource>(*p_resource)) {
		p_extensions->push_back("spatlas");
	}
}

bool SpineAtlasResourceFormatSaver::recognize(const RES &p_resource) const {
	return Object::cast_to<SpineAtlasResource>(*p_resource) != nullptr;
}
