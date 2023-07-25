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

#pragma once

#include "SpineCommon.h"
#include "core/io/resource_loader.h"
#include "core/io/resource_saver.h"
#include "core/io/image_loader.h"
#include <spine/Atlas.h>

class GodotSpineTextureLoader;

class SpineAtlasResource : public Resource {
	GDCLASS(SpineAtlasResource, Resource)

	void clear();

protected:
	static void _bind_methods();

	spine::Atlas *atlas;
	GodotSpineTextureLoader *texture_loader;

	String source_path;
	String atlas_data;
	String normal_map_prefix;

	Array textures;
	Array normal_maps;

public:
	SpineAtlasResource();
	~SpineAtlasResource() override;

	spine::Atlas *get_spine_atlas() { return atlas; }

	void set_normal_texture_prefix(const String &prefix) { normal_map_prefix = prefix; }

	Error load_from_atlas_file(const String &path);// .atlas

	Error load_from_file(const String &path);// .spatlas

	Error save_to_file(const String &path);// .spatlas

	String get_source_path();

	Array get_textures();

	Array get_normal_maps();
};

class SpineAtlasResourceFormatLoader : public ResourceFormatLoader {
	GDCLASS(SpineAtlasResourceFormatLoader, ResourceFormatLoader)

public:
#if VERSION_MAJOR > 3
	RES load(const String &path, const String &original_path, Error *error, bool use_sub_threads, float *progress, CacheMode cache_mode) override;
#else
	RES load(const String &path, const String &original_path, Error *error) override;
#endif

	void get_recognized_extensions(List<String> *extensions) const override;

	bool handles_type(const String &type) const override;

	String get_resource_type(const String &path) const override;
};

class SpineAtlasResourceFormatSaver : public ResourceFormatSaver {
	GDCLASS(SpineAtlasResourceFormatSaver, ResourceFormatSaver)

public:
#if VERSION_MAJOR > 3
	Error save(const RES &resource, const String &path, uint32_t flags) override;
#else
	Error save(const String &path, const RES &resource, uint32_t flags) override;
#endif

	void get_recognized_extensions(const RES &resource, List<String> *extensions) const override;

	bool recognize(const RES &resource) const override;
};
