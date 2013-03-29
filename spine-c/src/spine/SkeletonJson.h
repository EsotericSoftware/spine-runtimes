#ifndef SPINE_SKELETONJSON_H_
#define SPINE_SKELETONJSON_H_

#include <spine/Attachment.h>
#include <spine/AttachmentLoader.h>
#include <spine/SkeletonData.h>
#include <spine/Atlas.h>

#ifdef __cplusplus
namespace spine {
extern "C" {
#endif

typedef struct {
	float scale;
	AttachmentLoader* attachmentLoader;
	const char* const error;
} SkeletonJson;

SkeletonJson* SkeletonJson_createWithLoader (AttachmentLoader* attachmentLoader);
SkeletonJson* SkeletonJson_create (Atlas* atlas);
void SkeletonJson_dispose (SkeletonJson* skeletonJson);

SkeletonData* SkeletonJson_readSkeletonData (SkeletonJson* skeletonJson, const char* json);
SkeletonData* SkeletonJson_readSkeletonDataFile (SkeletonJson* skeletonJson, const char* path);

/* Animation* SkeletonJson_readAnimation (SkeletonJson* skeletonJson, char* json, const SkeletonData *skeletonData); */

#ifdef __cplusplus
}
}
#endif

#endif /* SPINE_SKELETONJSON_H_ */
