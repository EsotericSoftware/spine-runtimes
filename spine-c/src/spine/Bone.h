#ifndef SPINE_BONE_H_
#define SPINE_BONE_H_

#include <spine/BoneData.h>

#ifdef __cplusplus
namespace spine {
extern "C" {
#endif

typedef struct Bone Bone;
struct Bone {
	BoneData* const data;
	Bone* const parent;
	float x, y;
	float rotation;
	float scaleX, scaleY;

	float const m00, m01, worldX; /* a b x */
	float const m10, m11, worldY; /* c d y */
	float const worldRotation;
	float const worldScaleX, worldScaleY;
};

void Bone_setYDown (int/*bool*/yDown);

/** @param parent May be zero. */
Bone* Bone_create (BoneData* data, Bone* parent);
void Bone_dispose (Bone* bone);

void Bone_setToBindPose (Bone* bone);

void Bone_updateWorldTransform (Bone* bone, int/*bool*/flipX, int/*bool*/flipY);

#ifdef __cplusplus
}
}
#endif

#endif /* SPINE_BONE_H_ */
