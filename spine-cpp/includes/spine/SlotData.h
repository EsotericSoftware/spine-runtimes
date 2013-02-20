#ifndef SPINE_SLOTDATA_H_
#define SPINE_SLOTDATA_H_

#include <string>
#include <stdexcept>

namespace spine {

class BoneData;

class SlotData {
public:
	std::string name;
	BoneData *boneData;
	float r, g, b, a;
	std::string *attachmentName;

	SlotData (const std::string &name, BoneData *boneData) :
					name(name),
					boneData(boneData),
					r(1),
					g(1),
					b(1),
					a(1),
					attachmentName(0) {
		if (!boneData) throw std::invalid_argument("boneData cannot be null.");
	}

	~SlotData () {
		if (attachmentName) {
			delete attachmentName;
			attachmentName = 0;
		}
	}
};

} /* namespace spine */
#endif /* SPINE_SLOTDATA_H_ */
