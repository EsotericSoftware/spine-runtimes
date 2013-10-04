

/* This demonstrates implementing an extension to spine-c. spine/extension.h declares the functions that must be implemented along
 * with internal methods exposed to facilitate extension. */

#include <stdio.h>
#include <spine/spine.h>
#include <spine/extension.h>

/**/

void _AtlasPage_createTexture (AtlasPage* self, const char* path) {
	self->rendererObject = 0;
	self->width = 123;
	self->height = 456;
}

void _AtlasPage_disposeTexture (AtlasPage* self) {
}

char* _Util_readFile (const char* path, int* length) {
	return _readFile(path, length);
}

/**/

int main (void) {
	Atlas* atlas = Atlas_readAtlasFile("data/spineboy.atlas");
	printf("First region name: %s, x: %d, y: %d\n", atlas->regions->name, atlas->regions->x, atlas->regions->y);
	printf("First page name: %s, size: %d, %d\n", atlas->pages->name, atlas->pages->width, atlas->pages->height);

	SkeletonJson* json = SkeletonJson_create(atlas);
	SkeletonData *skeletonData = SkeletonJson_readSkeletonDataFile(json, "data/spineboy.json");
	if (!skeletonData) {
		printf("Error: %s\n", json->error);
		exit(0);
	}
	printf("Default skin name: %s\n", skeletonData->defaultSkin->name);

	Skeleton* skeleton = Skeleton_create(skeletonData);

	Animation* animation = SkeletonData_findAnimation(skeletonData, "walk");
	if (!animation) {
		printf("Error: Animation not found: walk\n");
		exit(0);
	}
	printf("Animation timelineCount: %d\n", animation->timelineCount);

	Skeleton_dispose(skeleton);
	SkeletonData_dispose(skeletonData);
	SkeletonJson_dispose(json);
	Atlas_dispose(atlas);

	return 0;
}