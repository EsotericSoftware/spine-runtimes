#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include <spine/spine.h>
#include "SfmlSkeleton.h"
#include "SfmlRegionAttachment.h"
#include "spine/cJSON.h"

Attachment* loadAttachment (AttachmentType type, const char* name) {
	return (Attachment*)SfmlRegionAttachment_create(name);
}

int main (void) {
	Attachment_setAttachmentLoader(loadAttachment);

	SkeletonJson_readSkeletonDataFile("data/spineboy-skeleton.json");
	if (SkeletonJson_getError()) printf("error: %s\n", SkeletonJson_getError());

	SkeletonData *skeletonData = SkeletonData_create();
	Skeleton* skeleton = Skeleton_create(skeletonData);
	printf("meow? %d\n", ((SfmlSkeleton*)skeleton)->meow);
	Skeleton_dispose(skeleton);
	printf("meow? %d\n", ((SfmlSkeleton*)skeleton)->meow);

	Atlas* atlas = Atlas_readAtlasFile("data/spineboy.atlas");
	printf("%s %d", atlas->regions->name, atlas->regions->y);

	return 0;
}
