#ifndef SPINE_EXTENSION_H_
#define SPINE_EXTENSION_H_

#ifdef __cplusplus
namespace spine {
extern "C" {
#endif

#include <spine/Skeleton.h>
#include <spine/RegionAttachment.h>
#include <spine/Animation.h>
#include <spine/Atlas.h>
#include <spine/AttachmentLoader.h>

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
void* _AttachmentLoader_setError (AttachmentLoader* loader, const char* error1, const char* error2);

#ifdef __cplusplus
}
}
#endif

#endif /* SPINE_EXTENSION_H_ */
