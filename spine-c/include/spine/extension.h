/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ******************************************************************************/

/*
 Implementation notes:

 - An OOP style is used where each "class" is made up of a struct and a number of functions prefixed with the struct name.

 - struct fields that are const are readonly. Either they are set in a constructor and can never be changed, or they can only be
 changed by calling a function.

 - Inheritance is done using a struct field named "super" as the first field, allowing the struct to be cast to its "super class".

 - Classes intended for inheritance provide init/deinit functions which subclasses must call in their create/dispose functions.

 - Polymorphism is done by a base class providing a "vtable" pointer to a struct containing function pointers. The public API
 delegates to the appropriate vtable function. Subclasses may change the vtable pointers.

 - Subclasses do not provide a dispose function, instead the base class' dispose function should be used, which will delegate to
 a dispose function in its vtable.

 - Classes not designed for inheritance cannot be extended. They may use an internal subclass to hide private data and don't
 expose a vtable.

 - The public API hides implementation details such as vtable structs and init/deinit functions. An internal API is exposed in
 extension.h to allow classes to be extended. Internal structs and functions begin with underscore (_).

 - OOP in C tends to lose type safety. Macros are provided in extension.h to give context for why a cast is being done.
 */

#ifndef SPINE_EXTENSION_H_
#define SPINE_EXTENSION_H_

/* All allocation uses these. */
#define MALLOC(TYPE,COUNT) ((TYPE*)malloc(sizeof(TYPE) * COUNT))
#define CALLOC(TYPE,COUNT) ((TYPE*)calloc(1, sizeof(TYPE) * COUNT))
#define NEW(TYPE) CALLOC(TYPE,1)

/* Gets the direct super class. Type safe. */
#define SUPER(VALUE) (&VALUE->super)

/* Cast to a super class. Not type safe, use with care. Prefer SUPER() where possible. */
#define SUPER_CAST(TYPE,VALUE) ((TYPE*)VALUE)

/* Cast to a sub class. Not type safe, use with care. */
#define SUB_CAST(TYPE,VALUE) ((TYPE*)VALUE)

/* Casts away const. Can be used as an lvalue. Not type safe, use with care. */
#define CONST_CAST(TYPE,VALUE) (*(TYPE*)&VALUE)

/* Gets the vtable for the specified type. Not type safe, use with care. */
#define VTABLE(TYPE,VALUE) ((_##TYPE##Vtable*)((TYPE*)VALUE)->vtable)

/* Frees memory. Can be used on const. */
#define FREE(VALUE) free((void*)VALUE)

/* Allocates a new char[], assigns it to TO, and copies FROM to it. Can be used on const. */
#define MALLOC_STR(TO,FROM) strcpy(CONST_CAST(char*, TO) = (char*)malloc(strlen(FROM) + 1), FROM)

#include <stdlib.h>
#include <string.h>
#include <spine/Skeleton.h>
#include <spine/RegionAttachment.h>
#include <spine/Animation.h>
#include <spine/Atlas.h>
#include <spine/AttachmentLoader.h>

#ifdef __cplusplus
namespace spine {
extern "C" {
#endif

/*
 * Functions that must be implemented:
 */

RegionAttachment* RegionAttachment_create (const char* name, AtlasRegion* region);

AtlasPage* AtlasPage_create (const char* name, const char* path);

char* _Util_readFile (const char* path, int* length);

/*
 * Internal API available for extension:
 */

char* _readFile (const char* path, int* length);

typedef struct _SkeletonVtable {
	void (*dispose) (Skeleton* skeleton);
} _SkeletonVtable;

void _Skeleton_init (Skeleton* self, SkeletonData* data);
void _Skeleton_deinit (Skeleton* self);

/**/

typedef struct _AttachmentVtable {
	void (*draw) (Attachment* self, struct Slot* slot);
	void (*dispose) (Attachment* self);
} _AttachmentVtable;

void _Attachment_init (Attachment* self, const char* name, AttachmentType type);
void _Attachment_deinit (Attachment* self);

/**/

void _RegionAttachment_init (RegionAttachment* self, const char* name);
void _RegionAttachment_deinit (RegionAttachment* self);

/**/

typedef struct _TimelineVtable {
	void (*apply) (const Timeline* self, Skeleton* skeleton, float time, float alpha);
	void (*dispose) (Timeline* self);
} _TimelineVtable;

void _Timeline_init (Timeline* self);
void _Timeline_deinit (Timeline* self);

/**/

void _CurveTimeline_init (CurveTimeline* self, int frameCount);
void _CurveTimeline_deinit (CurveTimeline* self);

/**/

typedef struct _AtlasPageVtable {
	void (*dispose) (AtlasPage* self);
} _AtlasPageVtable;

void _AtlasPage_init (AtlasPage* self, const char* name);
void _AtlasPage_deinit (AtlasPage* self);

/**/

typedef struct _AttachmentLoaderVtable {
	Attachment* (*newAttachment) (AttachmentLoader* self, AttachmentType type, const char* name);
	void (*dispose) (AttachmentLoader* self);
} _AttachmentLoaderVtable;

void _AttachmentLoader_init (AttachmentLoader* self);
void _AttachmentLoader_deinit (AttachmentLoader* self);
void _AttachmentLoader_setError (AttachmentLoader* self, const char* error1, const char* error2);

#ifdef __cplusplus
}
}
#endif

#endif /* SPINE_EXTENSION_H_ */
