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
	String normal_maps_prefix;

public:
	GodotSpineTextureLoader(Array *t, Array *nt, const String &p) : textures(t), normal_maps(nt), normal_maps_prefix(p) {
		if (textures) textures->clear();
		if (normal_maps) normal_maps->clear();
	}

	String fixPathIssue(const String &path) {
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

		//		print_line(String("Spine is loading texture: ") + String(path.buffer()));
		auto fixed_path = fixPathIssue(String(path.buffer()));
		//        print_line("Fixed path: " + fixed_path);

		// Load texture (e.g. tex.png)
		Ref<Texture> tex = ResourceLoader::load(fixed_path, "", false, &err);
		if (err != OK) {
			print_error(vformat("Can't load texture: \"%s\"", String(path.buffer())));
			page.setRendererObject((void *) memnew(SpineRendererObject{nullptr}));
			return;
		}

		if (textures) textures->append(tex);
		auto p_spine_renderer_object = memnew(SpineRendererObject);
		p_spine_renderer_object->tex = tex;

		// Load normal texture (e.g. n_tex.png)
		String temppath = fixed_path;
		String newpath = vformat("%s/%s_%s", temppath.get_base_dir(), normal_maps_prefix, temppath.get_file());
		//        print_line(vformat("try n tex: %s", newpath));
		if (ResourceLoader::exists(newpath)) {
			Ref<Texture> normal_tex = ResourceLoader::load(newpath);
			if (normal_maps) normal_maps->append(normal_tex);
			p_spine_renderer_object->normal_tex = normal_tex;

			// print_line(String("From atlas resource load: ") + String(" ro ") + String(Variant((long long) p_spine_renderer_object)));
			// print_line(String("From atlas resource load: ") + String(Variant(p_spine_renderer_object->tex)) + String(", ") + String(Variant(p_spine_renderer_object->normal_tex)));
		}

		page.setRendererObject((void *) p_spine_renderer_object);

		page.width = tex->get_width();
		page.height = tex->get_height();
	}

	virtual void unload(void *p) {
		// print_line("I'm out.");
		auto p_spine_renderer_object = (SpineRendererObject *) p;
		Ref<Texture> &tex = p_spine_renderer_object->tex;
		Ref<Texture> &normal_tex = p_spine_renderer_object->normal_tex;

		if (tex.is_valid()) tex.unref();
		if (normal_tex.is_valid()) normal_tex.unref();

		memdelete(p_spine_renderer_object);
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
	ClassDB::bind_method(D_METHOD("get_normal_textures"), &SpineAtlasResource::get_normal_textures);

	ADD_PROPERTY(PropertyInfo(Variant::STRING, "source_path"), "", "get_source_path");
	ADD_PROPERTY(PropertyInfo(Variant::ARRAY, "textures"), "", "get_textures");
	ADD_PROPERTY(PropertyInfo(Variant::ARRAY, "normal_textures"), "", "get_normal_textures");
}


Array SpineAtlasResource::get_textures() {
	return tex_list;
}

Array SpineAtlasResource::get_normal_textures() {
	return ntex_list;
}

String SpineAtlasResource::get_source_path() {
	return source_path;
}

Error SpineAtlasResource::load_from_atlas_file(const String &p_path) {
	//    print_line(vformat("Importing atlas file: %s", p_path));
	source_path = p_path;

	Error err;

	atlas_data = FileAccess::get_file_as_string(p_path, &err);
	if (err != OK) return err;

	if (atlas) delete atlas;
	tex_list.clear();
	ntex_list.clear();
	atlas = new spine::Atlas(atlas_data.utf8(), atlas_data.size(), source_path.get_base_dir().utf8(), new GodotSpineTextureLoader(&tex_list, &ntex_list, normal_texture_prefix));

	//    print_line(vformat("atlas loaded!"));

	if (atlas)
		return OK;

	tex_list.clear();
	ntex_list.clear();

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
	if (err != OK) {
		return err;
	}

	Dictionary content = Dictionary(result);

	source_path = content["source_path"];
	atlas_data = content["atlas_data"];
	normal_texture_prefix = content["normal_texture_prefix"];

	if (atlas) delete atlas;
	tex_list.clear();
	ntex_list.clear();
	atlas = new spine::Atlas(atlas_data.utf8(), atlas_data.size(), source_path.get_base_dir().utf8(), new GodotSpineTextureLoader(&tex_list, &ntex_list, normal_texture_prefix));

	if (atlas)
		return OK;

	tex_list.clear();
	ntex_list.clear();
	return ERR_FILE_UNRECOGNIZED;
}

Error SpineAtlasResource::save_to_file(const String &p_path) {
	Error err;
	FileAccess *file = FileAccess::open(p_path, FileAccess::WRITE, &err);
	if (err != OK) {
		//        print_line(vformat("save file err: %d", err));
		if (file) file->close();
		return err;
	}

	Dictionary content;
	content["source_path"] = source_path;
	content["atlas_data"] = atlas_data;
	content["normal_texture_prefix"] = normal_texture_prefix;

	//    print_line(vformat("storing source_path: %s", source_path));

	file->store_string(JSON::print(content));
	file->close();

	return OK;
}
