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

#include <stdlib.h>

#define SPINE_EXTENSION (SpineExtension::getInstance())

/* All allocation uses these. */
#define MALLOC(TYPE,COUNT) ((TYPE*)SPINE_EXTENSION->spineAlloc(sizeof(TYPE) * (COUNT), __FILE__, __LINE__))
#define CALLOC(TYPE,COUNT) ((TYPE*)SPINE_EXTENSION->spineCalloc(COUNT, sizeof(TYPE), __FILE__, __LINE__))
#define NEW(TYPE) ((TYPE*)SPINE_EXTENSION->spineAlloc(sizeof(TYPE), __FILE__, __LINE__))
#define REALLOC(PTR,TYPE,COUNT) ((TYPE*)SPINE_EXTENSION->spineRealloc(PTR, sizeof(TYPE) * (COUNT), __FILE__, __LINE__))

/* Frees memory. Can be used on const types. */
#define FREE(VALUE) SPINE_EXTENSION->spineFree((void*)VALUE)

/* Call destructor and then frees memory. Can be used on const types. */
#define DESTROY(TYPE,VALUE) VALUE->~TYPE(); SPINE_EXTENSION->spineFree((void*)VALUE)

namespace Spine
{
    class SpineExtension
    {
    public:
        static void setInstance(SpineExtension* inSpineExtension);
        
        static SpineExtension* getInstance();
        
        virtual ~SpineExtension();
        
        /// Implement this function to use your own memory allocator
        virtual void* spineAlloc(size_t size, const char* file, int line) = 0;
        
        virtual void* spineCalloc(size_t num, size_t size, const char* file, int line) = 0;
        
        virtual void* spineRealloc(void* ptr, size_t size, const char* file, int line) = 0;
        
        /// If you provide a spineAllocFunc, you should also provide a spineFreeFunc
        virtual void spineFree(void* mem) = 0;
        
        virtual char* spineReadFile(const char* path, int* length);
        
    protected:
        SpineExtension();
        
    private:
        static SpineExtension* _instance;
    };
    
    class DefaultSpineExtension : public SpineExtension
    {
    public:
        static DefaultSpineExtension* getInstance();
        
        virtual ~DefaultSpineExtension();
        
        virtual void* spineAlloc(size_t size, const char* file, int line);
        
        virtual void* spineCalloc(size_t num, size_t size, const char* file, int line);
        
        virtual void* spineRealloc(void* ptr, size_t size, const char* file, int line);
        
        virtual void spineFree(void* mem);
        
    protected:
        DefaultSpineExtension();
    };
}

#endif /* Spine_Extension_h */
