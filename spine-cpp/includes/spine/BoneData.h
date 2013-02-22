#ifndef SPINE_BONEDATA_H_
#define SPINE_BONEDATA_H_

#include <string>

namespace spine {

class BoneData {
public:
	std::string name;
	BoneData* parent;
	float length;
	float x, y;
	float rotation;
	float scaleX, scaleY;
	float flipY;

	BoneData (const std::string &name) :
					name(name),
					parent(0),
					length(0),
					x(0),
					y(0),
					rotation(0),
					scaleX(1),
					scaleY(1),
					flipY(false) {
	}
};

} /* namespace spine */
#endif /* SPINE_BONEDATA_H_ */
