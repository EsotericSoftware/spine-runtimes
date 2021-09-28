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

#include "GodotSpineExtension.h"

#include "core/variant_parser.h"
#include <spine/SpineString.h>

#include <iostream>

spine::SpineExtension *spine::getDefaultExtension() {
    return new GodotSpineExtension();
}

GodotSpineExtension::GodotSpineExtension(){}
GodotSpineExtension::~GodotSpineExtension(){}

void *GodotSpineExtension::_alloc(size_t size, const char *file, int line){
//	std::cout<<"_alloc "<<file<<" "<<line<<std::endl;
    return memalloc(size);
}

void *GodotSpineExtension::_calloc(size_t size, const char *file, int line){
//	std::cout<<"_calloc "<<file<<" "<<line<<std::endl;
    auto p = memalloc(size);
    memset(p, 0, size);
    return p;
}

void *GodotSpineExtension::_realloc(void *ptr, size_t size, const char *file, int line){
//	std::cout<<"_realloc "<<file<<" "<<line<<std::endl;
    return memrealloc(ptr, size);
}

void GodotSpineExtension::_free(void *mem, const char *file, int line){
//	std::cout<<"_free "<<file<<" "<<line<<std::endl;
    memfree(mem);
}

char *GodotSpineExtension::_readFile(const spine::String &path, int *length){
    Error error;
    auto res = FileAccess::get_file_as_array(String(path.buffer()), &error);
//    std::cout<<"Spine is loading something: "<<path.buffer()<<std::endl;
    if (error != OK){
        if(length) *length = 0;
        return NULL;
    }

    if(length) *length = res.size();
    auto r = alloc<char>(res.size(), __FILE__, __LINE__);
    for(size_t i=0;i<res.size();++i)
        r[i] = res[i];
    return r;
}