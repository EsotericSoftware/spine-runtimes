/* This demonstrates implementing an extension to spine-c. spine/extension.h declares the functions that must be implemented along
 * with internal methods exposed to facilitate extension. */

#include <stdio.h>
#include <stdlib.h>
#include <spine/spine.h>
#include <spine/extension.h>

/**/

typedef struct {
	AtlasPage super;
	int extraData;
} ExampleAtlasPage;

void _ExampleAtlasPage_free (AtlasPage* page) {
	ExampleAtlasPage* self = SUB_CAST(ExampleAtlasPage, page);
	_AtlasPage_deinit(SUPER(self));

	self->extraData = 0;

	FREE(self);
}

AtlasPage* AtlasPage_new (const char* name) {
	ExampleAtlasPage* self = NEW(ExampleAtlasPage);
	_AtlasPage_init(SUPER(self), name);
	VTABLE(AtlasPage, self) ->free = _ExampleAtlasPage_free;

	self->extraData = 123;

	return SUPER(self);
}

/**/

typedef struct {
	Skeleton super;
	int extraData;
} ExampleSkeleton;

void _ExampleSkeleton_free (Skeleton* skeleton) {
	ExampleSkeleton* self = SUB_CAST(ExampleSkeleton, skeleton);
	_Skeleton_deinit(SUPER(self));

	self->extraData = 0;

	FREE(self);
}

Skeleton* Skeleton_new (SkeletonData* data) {
	ExampleSkeleton* self = NEW(ExampleSkeleton);
	_Skeleton_init(SUPER(self), data);
	VTABLE(Skeleton, self) ->free = _ExampleSkeleton_free;

	self->extraData = 789;

	return SUPER(self);
}

/**/

typedef struct {
	RegionAttachment super;
	int extraData;
} ExampleRegionAttachment;

void _ExampleRegionAttachment_free (Attachment* attachment) {
	ExampleRegionAttachment* self = SUB_CAST(ExampleRegionAttachment, attachment);
	_RegionAttachment_deinit(SUPER(self));

	self->extraData = 0;

	FREE(self);
}

void _ExampleRegionAttachment_draw (Attachment* attachment, Slot* slot) {
	// ExampleRegionAttachment* self = (ExampleRegionAttachment*)attachment;
	// Draw or queue region for drawing.
}

RegionAttachment* RegionAttachment_new (const char* name, AtlasRegion* region) {
	ExampleRegionAttachment* self = NEW(ExampleRegionAttachment);
	_RegionAttachment_init(SUPER(self), name);
	VTABLE(Attachment, self) ->free = _ExampleRegionAttachment_free;
	VTABLE(Attachment, self) ->draw = _ExampleRegionAttachment_draw;

	self->extraData = 456;

	return SUPER(self);
}

/**/

char* _Util_readFile (const char* path, int* length) {
	return _readFile(path, length);
}

/**/

int main (void) {
	Atlas* atlas = Atlas_readAtlasFile("data/spineboy.atlas");
	printf("First region name: %s, x: %d, y: %d\n", atlas->regions->name, atlas->regions->x, atlas->regions->y);
	printf("First page name: %s, extraData: %d\n", atlas->pages->name, ((ExampleAtlasPage*)atlas->pages)->extraData);

	SkeletonJson* json = SkeletonJson_new(atlas);
	SkeletonData *skeletonData = SkeletonJson_readSkeletonDataFile(json, "data/spineboy-skeleton.json");
	if (!skeletonData) printf("Error: %s\n", json->error);
	printf("Attachment extraData: %d\n", ((ExampleRegionAttachment*)skeletonData->defaultSkin->entries->attachment)->extraData);

	Skeleton* skeleton = Skeleton_new(skeletonData);
	printf("Skeleton extraData: %d\n", ((ExampleSkeleton*)skeleton)->extraData);

	Animation* animation = SkeletonJson_readAnimationFile(json, "data/spineboy-walk.json", skeletonData);
	if (!animation) printf("Error: %s\n", json->error);
	printf("Animation timelineCount: %d\n", animation->timelineCount);

	Skeleton_free(skeleton);
	SkeletonData_free(skeletonData);
	SkeletonJson_free(json);
	Atlas_free(atlas);

	return 0;
}
