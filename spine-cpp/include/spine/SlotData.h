#ifndef SPINE_SLOTDATA_H_
#define SPINE_SLOTDATA_H_

#include <string>

namespace spine {

class BoneData;

class SlotData {
public:
	std::string name;
	BoneData *boneData;
	float r, g, b, a;
	std::string *attachmentName;

	SlotData (const std::string &name, BoneData *boneData);
	~SlotData ();
};

} /* namespace spine */
#endif /* SPINE_SLOTDATA_H_ */
