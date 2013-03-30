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

#ifdef __cplusplus
namespace spine {
extern "C" {
#endif

/* Methods that must be implemented: **/

Skeleton* Skeleton_create (SkeletonData* data);

RegionAttachment* RegionAttachment_create (const char* name, AtlasRegion* region);

AtlasPage* AtlasPage_create (const char* name);

/* Internal methods needed for extension: **/

void _Skeleton_init (Skeleton* skeleton, SkeletonData* data);
void _Skeleton_deinit (Skeleton* skeleton);

void _Attachment_init (Attachment* attachment, const char* name, AttachmentType type);
void _Attachment_deinit (Attachment* attachment);

void _RegionAttachment_init (RegionAttachment* attachment, const char* name);
void _RegionAttachment_deinit (RegionAttachment* attachment);

void _Timeline_init (Timeline* timeline);
void _Timeline_deinit (Timeline* timeline);

void _CurveTimeline_init (CurveTimeline* timeline, int frameCount);
void _CurveTimeline_deinit (CurveTimeline* timeline);

void _AtlasPage_init (AtlasPage* page, const char* name);
void _AtlasPage_deinit (AtlasPage* page);

void _AttachmentLoader_init (AttachmentLoader* loader);
void _AttachmentLoader_deinit (AttachmentLoader* loader);
void _AttachmentLoader_setError (AttachmentLoader* loader, const char* error1, const char* error2);

#ifdef __cplusplus
}
}
#endif

#endif /* SPINE_EXTENSION_H_ */
