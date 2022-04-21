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

#include "SpineSkeletonFileResource.h"
#if VERSION_MAJOR > 3
#include "core/io/file_access.h"
#else
#include "core/os/file_access.h"
#endif

void SpineSkeletonFileResource::_bind_methods() {
}

Error SpineSkeletonFileResource::load_from_file(const String &path) {
	Error error;
	if (path.ends_with("spjson"))
		json = FileAccess::get_file_as_string(path, &error);
	else
		binary = FileAccess::get_file_as_array(path, &error);
	return error;
}

Error SpineSkeletonFileResource::save_to_file(const String &path) {
	Error error;
#if VERSION_MAJOR > 3
	Ref<FileAccess> file = FileAccess::open(path, FileAccess::WRITE, &error);
	if (error != OK) return error;
#else
	FileAccess *file = FileAccess::open(path, FileAccess::WRITE, &error);
	if (error != OK) {
		if (file) file->close();
		return error;
	}
#endif
	if (!is_binary())
		file->store_string(json);
	else
		file->store_buffer(binary.ptr(), binary.size());
#if VERSION_MAJOR > 3
	file->flush();
#else
	file->close();
#endif
	return OK;
}

#if VERSION_MAJOR > 3
RES SpineSkeletonFileResourceFormatLoader::load(const String &path, const String &original_path, Error *error, bool use_sub_threads, float *progress, CacheMode cache_mode) {
#else
RES SpineSkeletonFileResourceFormatLoader::load(const String &path, const String &original_path, Error *error) {
#endif
	Ref<SpineSkeletonFileResource> skeleton_file = memnew(SpineSkeletonFileResource);
	skeleton_file->load_from_file(path);
	if (error) *error = OK;
	return skeleton_file;
}

void SpineSkeletonFileResourceFormatLoader::get_recognized_extensions(List<String> *extensions) const {
	extensions->push_back("spjson");
	extensions->push_back("spskel");
}

String SpineSkeletonFileResourceFormatLoader::get_resource_type(const String &path) const {
	return "SpineSkeletonFileResource";
}

bool SpineSkeletonFileResourceFormatLoader::handles_type(const String &type) const {
	return type == "SpineSkeletonFileResource" || ClassDB::is_parent_class(type, "SpineSkeletonFileResource");
}

Error SpineSkeletonFileResourceFormatSaver::save(const String &path, const RES &resource, uint32_t flags) {
	Ref<SpineSkeletonFileResource> res = resource;
	Error error = res->save_to_file(path);
	return error;
}

void SpineSkeletonFileResourceFormatSaver::get_recognized_extensions(const RES &resource, List<String> *p_extensions) const {
	if (Object::cast_to<SpineSkeletonFileResource>(*resource)) {
		p_extensions->push_back("spjson");
		p_extensions->push_back("spskel");
	}
}

bool SpineSkeletonFileResourceFormatSaver::recognize(const RES &p_resource) const {
	return Object::cast_to<SpineSkeletonFileResource>(*p_resource) != nullptr;
}
