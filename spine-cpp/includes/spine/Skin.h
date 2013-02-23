#ifndef SPINE_SKIN_H_
#define SPINE_SKIN_H_

#include <string>
#include <map>
#include <spine/Attachment.h>

namespace spine {

class BaseSkeleton;

class Skin {
	friend class BaseSkeleton;

private:
	struct Key {
		int slotIndex;
		std::string name;

		friend bool operator< (const Key &key1, const Key &key2) {
			if (key1.slotIndex == key2.slotIndex) return key1.name < key2.name;
			return key1.slotIndex < key2.slotIndex;
		}
	};
	std::map<Key, Attachment*> attachments;

	/** Attach all attachments from this skin if the corresponding attachment from the old skin is currently attached. */
	void attachAll (BaseSkeleton *skeleton, Skin *oldSkin);

public:
	std::string name;

	Skin (const std::string &name);
  ~Skin();

	void addAttachment (int slotIndex, const std::string &name, Attachment *attachment);

	Attachment* getAttachment (int slotIndex, const std::string &name);
};

} /* namespace spine */
#endif /* SPINE_SKIN_H_ */
