/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#ifndef Spine_Extension_h
#define Spine_Extension_h

/* All allocation uses these. */
#define MALLOC(TYPE,COUNT) ((TYPE*)spineAlloc(sizeof(TYPE) * (COUNT), __FILE__, __LINE__))
#define NEW(TYPE) ((TYPE*)spineAlloc(sizeof(TYPE), __FILE__, __LINE__))
#define REALLOC(PTR,TYPE,COUNT) ((TYPE*)spineRealloc(PTR, sizeof(TYPE) * (COUNT), __FILE__, __LINE__))

/* Frees memory. Can be used on const types. */
#define FREE(VALUE) spineFree((void*)VALUE)

/* Call destructor and then frees memory. Can be used on const types. */
#define DESTROY(TYPE,VALUE) VALUE->~TYPE(); spineFree((void*)VALUE)

#include <stdlib.h>

namespace Spine
{
    /// Implement this function to use your own memory allocator.
    void* spineAlloc(size_t size, const char* file, int line);
    
    void* spineRealloc(void* ptr, size_t size, const char* file, int line);
    
    /// If you implement spineAlloc, you should also implement this function.
    void spineFree(void* mem);
    
    char* spineReadFile(const char* path, int* length);
}

#endif /* Spine_Extension_h */
