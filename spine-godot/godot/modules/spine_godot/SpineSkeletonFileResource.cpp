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

void SpineSkeletonFileResource::_bind_methods() {
}

Error SpineSkeletonFileResource::load_from_file(const String &p_path) {
	Error err;

	if (p_path.ends_with("spjson"))
		json = FileAccess::get_file_as_string(p_path, &err);
	else
		binary = FileAccess::get_file_as_array(p_path, &err);
	return err;
}

Error SpineSkeletonFileResource::save_to_file(const String &p_path) {
	Error err;
	FileAccess *file = FileAccess::open(p_path, FileAccess::WRITE, &err);
	if (err != OK) {
		if (file) file->close();
		return err;
	}

	if (!is_binary())
		file->store_string(json);
	else
		file->store_buffer(binary.ptr(), binary.size());
	file->close();

	return OK;
}

RES SpineSkeletonFileResourceFormatLoader::load(const String &p_path, const String &p_original_path, Error *r_error) {
	Ref<SpineSkeletonFileResource> skeleton = memnew(SpineSkeletonFileResource);
	skeleton->load_from_file(p_path);

	if (r_error) {
		*r_error = OK;
	}
	return skeleton;
}

void SpineSkeletonFileResourceFormatLoader::get_recognized_extensions(List<String> *r_extensions) const {
	r_extensions->push_back("spjson");
	r_extensions->push_back("spskel");
}

String SpineSkeletonFileResourceFormatLoader::get_resource_type(const String &p_path) const {
	return "SpineSkeletonFileResource";
}

bool SpineSkeletonFileResourceFormatLoader::handles_type(const String &p_type) const {
	return p_type == "SpineSkeletonFileResource" || ClassDB::is_parent_class(p_type, "SpineSkeletonFileResource");
}
Error SpineSkeletonFileResourceFormatSaver::save(const String &p_path, const RES &p_resource, uint32_t p_flags) {
	Ref<SpineSkeletonFileResource> res = p_resource.get_ref_ptr();
	Error error = res->save_to_file(p_path);
	return error;
}

void SpineSkeletonFileResourceFormatSaver::get_recognized_extensions(const RES &p_resource, List<String> *p_extensions) const {
	if (Object::cast_to<SpineSkeletonFileResource>(*p_resource)) {
		p_extensions->push_back("spjson");
		p_extensions->push_back("spskel");
	}
}

bool SpineSkeletonFileResourceFormatSaver::recognize(const RES &p_resource) const {
	return Object::cast_to<SpineSkeletonFileResource>(*p_resource) != nullptr;
}
