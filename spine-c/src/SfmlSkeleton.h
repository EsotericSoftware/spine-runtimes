#ifndef SPINE_SFMLSKELETON_H_
#define SPINE_SFMLSKELETON_H_

#include <spine/Skeleton.h>

#ifdef __cplusplus
namespace spine {extern "C" {
#endif

typedef struct {
	Skeleton super;
	int meow;
} SfmlSkeleton;

Skeleton* Skeleton_create (SkeletonData* data);

void Skeleton_draw ();

#ifdef __cplusplus
}}
#endif

#endif /* SPINE_SFMLSKELETON_H_ */
