#ifndef SPINE_BONEDATA_H_
#define SPINE_BONEDATA_H_

#ifdef __cplusplus
namespace spine {
extern "C" {
#endif

typedef struct BoneData BoneData;
struct BoneData {
	const char* const name;
	BoneData* const parent;
	float length;
	float x, y;
	float rotation;
	float scaleX, scaleY;
};

BoneData* BoneData_create (const char* name, BoneData* parent);
void BoneData_dispose (BoneData* boneData);

#ifdef __cplusplus
}
}
#endif

#endif /* SPINE_BONEDATA_H_ */
