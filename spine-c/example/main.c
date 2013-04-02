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

void _ExampleAtlasPage_dispose (AtlasPage* page) {
	ExampleAtlasPage* self = SUB_CAST(ExampleAtlasPage, page);
	_AtlasPage_deinit(SUPER(self));

	self->extraData = 0;

	FREE(self);
}

AtlasPage* AtlasPage_create (const char* name) {
	ExampleAtlasPage* self = NEW(ExampleAtlasPage);
	_AtlasPage_init(SUPER(self), name);
	VTABLE(AtlasPage, self) ->dispose = _ExampleAtlasPage_dispose;

	self->extraData = 123;

	return SUPER(self);
}

/**/

typedef struct {
	Skeleton super;
	int extraData;
} ExampleSkeleton;

void _ExampleSkeleton_dispose (Skeleton* skeleton) {
	ExampleSkeleton* self = SUB_CAST(ExampleSkeleton, skeleton);
	_Skeleton_deinit(SUPER(self));

	self->extraData = 0;

	FREE(self);
}

Skeleton* Skeleton_create (SkeletonData* data) {
	ExampleSkeleton* self = NEW(ExampleSkeleton);
	_Skeleton_init(SUPER(self), data);
	VTABLE(Skeleton, self) ->dispose = _ExampleSkeleton_dispose;

	self->extraData = 789;

	return SUPER(self);
}

/**/

typedef struct {
	RegionAttachment super;
	int extraData;
} ExampleRegionAttachment;

void _ExampleRegionAttachment_dispose (Attachment* attachment) {
	ExampleRegionAttachment* self = SUB_CAST(ExampleRegionAttachment, attachment);
	_RegionAttachment_deinit(SUPER(self));

	self->extraData = 0;

	FREE(self);
}

void _ExampleRegionAttachment_draw (Attachment* attachment, Slot* slot) {
	// ExampleRegionAttachment* self = (ExampleRegionAttachment*)attachment;
	// Draw or queue region for drawing.
}

RegionAttachment* RegionAttachment_create (const char* name, AtlasRegion* region) {
	ExampleRegionAttachment* self = NEW(ExampleRegionAttachment);
	_RegionAttachment_init(SUPER(self), name);
	VTABLE(Attachment, self) ->dispose = _ExampleRegionAttachment_dispose;
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

	SkeletonJson* json = SkeletonJson_create(atlas);
	SkeletonData *skeletonData = SkeletonJson_readSkeletonDataFile(json, "data/spineboy-skeleton.json");
	if (!skeletonData) printf("Error: %s\n", json->error);
	printf("Default skin name: %s\n", skeletonData->defaultSkin->name);

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
