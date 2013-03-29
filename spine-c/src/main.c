/* This demonstrates implementing an extension to spine-c. spine/extension.h declares the functions that must be implemented along
 * with a number of internal methods exposed to facilitate extension. */

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
	ExampleAtlasPage* this = (ExampleAtlasPage*)page;
	_AtlasPage_deinit(&this->super);
	this->extraData = 0;
	FREE(this)
}

AtlasPage* AtlasPage_create (const char* name) {
	ExampleAtlasPage* this = calloc(1, sizeof(ExampleAtlasPage));
	_AtlasPage_init(&this->super, name);
	this->extraData = 123;
	this->super._dispose = _ExampleAtlasPage_dispose;
	return &this->super;
}

/**/

typedef struct {
	RegionAttachment super;
	int extraData;
} ExampleRegionAttachment;

void _ExampleRegionAttachment_dispose (Attachment* attachment) {
	ExampleRegionAttachment* this = (ExampleRegionAttachment*)attachment;
	_RegionAttachment_deinit(&this->super);
	this->extraData = 0;
	FREE(this)
}

RegionAttachment* RegionAttachment_create (const char* name, AtlasRegion* region) {
	ExampleRegionAttachment* this = calloc(1, sizeof(ExampleRegionAttachment));
	_RegionAttachment_init(&this->super, name);
	this->extraData = 456;
	this->super.super._dispose = _ExampleRegionAttachment_dispose;
	return &this->super;
}

/**/

typedef struct {
	Skeleton super;
	int extraData;
} ExampleSkeleton;

void _ExampleSkeleton_dispose (Skeleton* skeleton) {
	ExampleSkeleton* this = (ExampleSkeleton*)skeleton;
	_Skeleton_deinit(&this->super);
	this->extraData = 0;
	FREE(this)
}

Skeleton* Skeleton_create (SkeletonData* data) {
	ExampleSkeleton* this = calloc(1, sizeof(ExampleSkeleton));
	_Skeleton_init(&this->super, data);
	this->extraData = 789;
	this->super._dispose = _ExampleSkeleton_dispose;
	return &this->super;
}

/**/

int main (void) {
	Atlas* atlas = Atlas_readAtlasFile("data/spineboy.atlas");
	printf("First page name: %s, extraData: %d\n", atlas->pages->name, ((ExampleAtlasPage*)atlas->pages)->extraData);
	printf("First region name: %s, x: %d, y: %d\n", atlas->regions->name, atlas->regions->x, atlas->regions->y);

	SkeletonJson* json = SkeletonJson_create(atlas);
	SkeletonData *skeletonData = SkeletonJson_readSkeletonDataFile(json, "data/spineboy-skeleton.json");
	if (!skeletonData) printf("error: %s\n", json->error);
	printf("Attachment extraData: %d\n", ((ExampleRegionAttachment*)skeletonData->defaultSkin->entries->attachment)->extraData);

	Skeleton* skeleton = Skeleton_create(skeletonData);
	printf("Skeleton extraData: %d\n", ((ExampleSkeleton*)skeleton)->extraData);

	Skeleton_dispose(skeleton);
	SkeletonData_dispose(skeletonData);
	SkeletonJson_dispose(json);
	Atlas_dispose(atlas);

	return 0;
}
