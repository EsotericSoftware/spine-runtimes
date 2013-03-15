#include <stdexcept>
#include <spine/Attachment.h>
#include <spine/Skin.h>
#include <spine/BaseSkeleton.h>
#include <spine/Slot.h>

namespace spine {

Skin::Skin (const std::string &name) :
				name(name) {
}

Skin::~Skin () {
	for (std::map<Key, Attachment*>::iterator iter = attachments.begin(); iter != attachments.end(); iter++)
		delete iter->second;
}

void Skin::addAttachment (int slotIndex, const std::string &name, Attachment *attachment) {
	if (!attachment) throw std::invalid_argument("attachment cannot be null.");
	Key key = {slotIndex, name};
	attachments[key] = attachment;
}

Attachment* Skin::getAttachment (int slotIndex, const std::string &name) {
	Key key = {slotIndex, name};
	if (attachments.find(key) != attachments.end()) return attachments[key];
	return 0;
}

void Skin::attachAll (BaseSkeleton *skeleton, Skin *oldSkin) {
	for (std::map<Key, Attachment*>::iterator iter = attachments.begin(); iter != attachments.end(); iter++) {
		const Key key = iter->first;
		Slot *slot = skeleton->slots[key.slotIndex];
		if (slot->attachment == iter->second) {
			Attachment *attachment = getAttachment(key.slotIndex, key.name);
			if (attachment) slot->setAttachment(attachment);
		}
	}
}

} /* namespace spine */
