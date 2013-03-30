/* This demonstrates implementing an extension to spine-c. spine/extension.h declares the functions that must be implemented along
 * with internal methods exposed to facilitate extension. */

#include <stdio.h>
#include <stdlib.h>
#include <spine/spine.h>
#include <spine/extension.h>
#include <spine/util.h>

/**/

typedef struct {
	AtlasPage super;
	int extraData;
} ExampleAtlasPage;

void _ExampleAtlasPage_dispose (AtlasPage* page) {
	ExampleAtlasPage* self = (ExampleAtlasPage*)page;
	_AtlasPage_deinit(&self->super);

	self->extraData = 0;

	FREE(self)
}

AtlasPage* AtlasPage_create (const char* name) {
	ExampleAtlasPage* self = CALLOC(ExampleAtlasPage, 1)
	_AtlasPage_init(&self->super, name);
	self->super._dispose = _ExampleAtlasPage_dispose;

	self->extraData = 123;

	return &self->super;
}

/**/

typedef struct {
	RegionAttachment super;
	int extraData;
} ExampleRegionAttachment;

void _ExampleRegionAttachment_dispose (Attachment* attachment) {
	ExampleRegionAttachment* self = (ExampleRegionAttachment*)attachment;
	_RegionAttachment_deinit(&self->super);

	self->extraData = 0;

	FREE(self)
}

RegionAttachment* RegionAttachment_create (const char* name, AtlasRegion* region) {
	ExampleRegionAttachment* self = CALLOC(ExampleRegionAttachment, 1)
	_RegionAttachment_init(&self->super, name);
	self->super.super._dispose = _ExampleRegionAttachment_dispose;

	self->extraData = 456;

	return &self->super;
}

/**/

typedef struct {
	Skeleton super;
	int extraData;
} ExampleSkeleton;

void _ExampleSkeleton_dispose (Skeleton* skeleton) {
	ExampleSkeleton* self = (ExampleSkeleton*)skeleton;
	_Skeleton_deinit(&self->super);

	self->extraData = 0;

	FREE(self)
}

Skeleton* Skeleton_create (SkeletonData* data) {
	ExampleSkeleton* self = CALLOC(ExampleSkeleton, 1)
	_Skeleton_init(&self->super, data);
	self->super._dispose = _ExampleSkeleton_dispose;

	self->extraData = 789;

	return &self->super;
}

/**/

int main (void) {
	Atlas* atlas = Atlas_readAtlasFile("data/spineboy.atlas");
	printf("First region name: %s, x: %d, y: %d\n", atlas->regions->name, atlas->regions->x, atlas->regions->y);
	printf("First page name: %s, extraData: %d\n", atlas->pages->name, ((ExampleAtlasPage*)atlas->pages)->extraData);

	SkeletonJson* json = SkeletonJson_create(atlas);
	SkeletonData *skeletonData = SkeletonJson_readSkeletonDataFile(json, "data/spineboy-skeleton.json");
	if (!skeletonData) printf("Error: %s\n", json->error);
	printf("Attachment extraData: %d\n", ((ExampleRegionAttachment*)skeletonData->defaultSkin->entries->attachment)->extraData);

	Skeleton* skeleton = Skeleton_create(skeletonData);
	printf("Skeleton extraData: %d\n", ((ExampleSkeleton*)skeleton)->extraData);

	Animation* animation = SkeletonJson_readAnimationFile(json, "data/spineboy-walk.json", skeletonData);
	if (!animation) printf("Error: %s\n", json->error);
	printf("Animation timelineCount: %d\n", animation->timelineCount);

	Skeleton_dispose(skeleton);
	SkeletonData_dispose(skeletonData);
	SkeletonJson_dispose(json);
	Atlas_dispose(atlas);

	return 0;
}
