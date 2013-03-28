#ifndef SPINE_SKELETONJSON_H_
#define SPINE_SKELETONJSON_H_

#include <spine/Attachment.h>
#include <spine/SkeletonData.h>

#ifdef __cplusplus
namespace spine {
extern "C" {
#endif

void SkeletonJson_setScale (float scale);

SkeletonData* SkeletonJson_readSkeletonData (const char* json);
SkeletonData* SkeletonJson_readSkeletonDataFile (const char* path);

/* Animation* readAnimation (char* json, const SkeletonData *skeletonData) const; */

const char* SkeletonJson_getError ();

#ifdef __cplusplus
}
}
#endif

#endif /* SPINE_SKELETONJSON_H_ */
