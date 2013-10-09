

/* This demonstrates implementing an extension to spine-c. spine/extension.h declares the functions that must be implemented along
 * with internal methods exposed to facilitate extension. */

#include <stdio.h>
#include <spine/spine.h>
#include <spine/extension.h>

/**/

void _spAtlasPage_createTexture (spAtlasPage* self, const char* path) {
	self->rendererObject = 0;
	self->width = 123;
	self->height = 456;
}

void _spAtlasPage_disposeTexture (spAtlasPage* self) {
}

char* _spUtil_readFile (const char* path, int* length) {
	return _readFile(path, length);
}

/**/

int main (void) {
	spAtlas* atlas = spAtlas_readAtlasFile("data/spineboy.atlas");
	printf("First region name: %s, x: %d, y: %d\n", atlas->regions->name, atlas->regions->x, atlas->regions->y);
	printf("First page name: %s, size: %d, %d\n", atlas->pages->name, atlas->pages->width, atlas->pages->height);

	spSkeletonJson* json = spSkeletonJson_create(atlas);
	spSkeletonData *skeletonData = spSkeletonJson_readSkeletonDataFile(json, "data/spineboy.json");
	if (!skeletonData) {
		printf("Error: %s\n", json->error);
		exit(0);
	}
	printf("Default skin name: %s\n", skeletonData->defaultSkin->name);

	spSkeleton* skeleton = spSkeleton_create(skeletonData);

	spAnimation* animation = spSkeletonData_findAnimation(skeletonData, "walk");
	if (!animation) {
		printf("Error: Animation not found: walk\n");
		exit(0);
	}
	printf("Animation timelineCount: %d\n", animation->timelineCount);

	spSkeleton_dispose(skeleton);
	spSkeletonData_dispose(skeletonData);
	spSkeletonJson_dispose(json);
	spAtlas_dispose(atlas);

	return 0;
}
