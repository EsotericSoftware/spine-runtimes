#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include <spine/spine.h>
#include "SfmlSkeleton.h"
#include "spine/cJSON.h"

int main (void) {
	//BoneData *boneData = BoneData_create("meow");

	//printf("name: %s\n", boneData->name);
	//printf("length struct: %f\n", boneData->length);

	//SkeletonData* data =
	SkeletonJson_readSkeletonDataFile("data/spineboy-skeleton.json");
	printf("error: %s\n", SkeletonJson_getError());

	SkeletonData *skeletonData = SkeletonData_create();
	Skeleton* skeleton = Skeleton_create(skeletonData);
	//Skeleton_something(skeleton);
	printf("meow? %d\n", ((SfmlSkeleton*)skeleton)->meow);
	Skeleton_dispose(skeleton);
	printf("meow? %d\n", ((SfmlSkeleton*)skeleton)->meow);

	return 0;
}
