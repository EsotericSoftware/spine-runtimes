#include "SfmlSkeleton.h"
#include <stdlib.h>

void SfmlSkeleton_dispose (Skeleton* skeleton) {
	// SfmlSkeleton* this = (SfmlSkeleton*)skeleton;
}

Skeleton* Skeleton_create (SkeletonData* data) {
	SfmlSkeleton* this = calloc(1, sizeof(SfmlSkeleton));
	Skeleton_init(&this->super, data);
	this->super._dispose = SfmlSkeleton_dispose;
	return &this->super;
}

void Skeleton_draw (Skeleton* skeleton) {
}
