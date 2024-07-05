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

#include "GodotSpineExtension.h"
#ifdef SPINE_GODOT_EXTENSION
#include "SpineCommon.h"
#include <godot_cpp/core/memory.hpp>
#include <godot_cpp/classes/file_access.hpp>
#else
#include "core/os/memory.h"
#include "core/version.h"
#if VERSION_MAJOR > 3
#include "core/io/file_access.h"
#else
#include "core/os/file_access.h"
#endif
#endif
#include <spine/SpineString.h>

spine::SpineExtension *spine::getDefaultExtension() {
	return new GodotSpineExtension();
}

void *GodotSpineExtension::_alloc(size_t size, const char *file, int line) {
	return memalloc(size);
}

void *GodotSpineExtension::_calloc(size_t size, const char *file, int line) {
	auto p = memalloc(size);
	memset(p, 0, size);
	return p;
}

void *GodotSpineExtension::_realloc(void *ptr, size_t size, const char *file, int line) {
	return memrealloc(ptr, size);
}

void GodotSpineExtension::_free(void *mem, const char *file, int line) {
	memfree(mem);
}

char *GodotSpineExtension::_readFile(const spine::String &path, int *length) {
	Error error;
#ifdef SPINE_GODOT_EXTENSION
	// FIXME no error parameter!
	auto res = FileAccess::get_file_as_bytes(String(path.buffer()));
#else
#if VERSION_MAJOR > 3
	auto res = FileAccess::get_file_as_bytes(String(path.buffer()), &error);
#else
	auto res = FileAccess::get_file_as_array(String(path.buffer()), &error);
#endif
	if (error != OK) {
		if (length) *length = 0;
		return NULL;
	}
#endif
	auto r = alloc<char>(res.size(), __FILE__, __LINE__);
	memcpy(r, res.ptr(), res.size());
	if (length) *length = res.size();
	return r;
}
