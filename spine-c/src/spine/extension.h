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

#ifndef SPINE_EXTENSION_H_
#define SPINE_EXTENSION_H_

#include <spine/Skeleton.h>
#include <spine/RegionAttachment.h>
#include <spine/Animation.h>
#include <spine/Atlas.h>
#include <spine/AttachmentLoader.h>
#include <spine/util.h>

#ifdef __cplusplus
namespace spine {
extern "C" {
#endif

/* Public API that must be implemented: **/

Skeleton* Skeleton_new (SkeletonData* data);

RegionAttachment* RegionAttachment_new (const char* name, AtlasRegion* region);

AtlasPage* AtlasPage_new (const char* name);

/* Internal API available for extension: **/

typedef struct _SkeletonVtable {
	void (*free) (Skeleton* skeleton);
} _SkeletonVtable;

void _Skeleton_init (Skeleton* skeleton, SkeletonData* data);
void _Skeleton_deinit (Skeleton* skeleton);

/**/

typedef struct _AttachmentVtable {
	void (*draw) (Attachment* attachment, struct Slot* slot);
	void (*free) (Attachment* attachment);
} _AttachmentVtable;

void _Attachment_init (Attachment* attachment, const char* name, AttachmentType type);
void _Attachment_deinit (Attachment* attachment);

/**/

void _RegionAttachment_init (RegionAttachment* attachment, const char* name);
void _RegionAttachment_deinit (RegionAttachment* attachment);

/**/

typedef struct _TimelineVtable {
	void (*apply) (const Timeline* timeline, Skeleton* skeleton, float time, float alpha);
	void (*dispose) (Timeline* timeline);
} _TimelineVtable;

void _Timeline_init (Timeline* timeline);
void _Timeline_deinit (Timeline* timeline);

/**/

void _CurveTimeline_init (CurveTimeline* timeline, int frameCount);
void _CurveTimeline_deinit (CurveTimeline* timeline);

/**/

typedef struct _AtlasPageVtable {
	void (*free) (AtlasPage* page);
} _AtlasPageVtable;

void _AtlasPage_init (AtlasPage* page, const char* name);
void _AtlasPage_deinit (AtlasPage* page);

/**/

typedef struct _AttachmentLoaderVtable {
	Attachment* (*newAttachment) (AttachmentLoader* loader, AttachmentType type, const char* name);
	void (*free) (AttachmentLoader* loader);
} _AttachmentLoaderVtable;

void _AttachmentLoader_init (AttachmentLoader* loader);
void _AttachmentLoader_deinit (AttachmentLoader* loader);
void _AttachmentLoader_setError (AttachmentLoader* loader, const char* error1, const char* error2);

#ifdef __cplusplus
}
}
#endif

#endif /* SPINE_EXTENSION_H_ */
