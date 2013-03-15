#include <stdexcept>
#include <spine/SlotData.h>

namespace spine {

SlotData::SlotData (const std::string &name, BoneData *boneData) :
				name(name),
				boneData(boneData),
				r(1),
				g(1),
				b(1),
				a(1),
				attachmentName(0) {
	if (!boneData) throw std::invalid_argument("boneData cannot be null.");
}

SlotData::~SlotData () {
	if (attachmentName) {
		delete attachmentName;
		attachmentName = 0;
	}
}

}
/* namespace spine */
