#ifndef SPINE_BONE_H_
#define SPINE_BONE_H_

namespace spine {

class BoneData;

class Bone {
public:
	BoneData *data;
	Bone *parent;
	float x, y;
	float rotation;
	float scaleX, scaleY;

	float m00, m01, worldX; // a b x
	float m10, m11, worldY; // c d y
	float worldRotation;
	float worldScaleX, worldScaleY;

	Bone (BoneData *data);

	void setToBindPose ();

	void updateWorldTransform (bool flipX, bool flipY);
};

} /* namespace spine */
#endif /* SPINE_BONE_H_ */
