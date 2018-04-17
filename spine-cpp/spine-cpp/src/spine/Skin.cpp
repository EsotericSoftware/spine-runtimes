/******************************************************************************
* Spine Runtimes Software License v2.5
*
* Copyright (c) 2013-2016, Esoteric Software
* All rights reserved.
*
* You are granted a perpetual, non-exclusive, non-sublicensable, and
* non-transferable license to use, install, execute, and perform the Spine
* Runtimes software and derivative works solely for personal or internal
* use. Without the written permission of Esoteric Software (see Section 2 of
* the Spine Software License Agreement), you may not (a) modify, translate,
* adapt, or develop new applications using the Spine Runtimes or otherwise
* create derivative works or improvements of the Spine Runtimes or (b) remove,
* delete, alter, or obscure any trademarks or any copyright, trademark, patent,
* or other intellectual property or proprietary rights notices on or in the
* Software, including any copy thereof. Redistributions in binary or source
* form must include this license and terms.
*
* THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
* IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
* MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
* EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
* SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
* PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
* USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
* IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
* ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
* POSSIBILITY OF SUCH DAMAGE.
*****************************************************************************/

#include <spine/Skin.h>

#include <spine/Attachment.h>
#include <spine/Skeleton.h>

#include <spine/Slot.h>

#include <assert.h>

using namespace Spine;

Skin::AttachmentKey::AttachmentKey(int slotIndex, const char* name) :
		_slotIndex(slotIndex),
		_name(name, true) {
}

Skin::AttachmentKey::AttachmentKey(int slotIndex, const String &name) :
		_slotIndex(slotIndex),
		_name(name) {
}

bool Skin::AttachmentKey::operator==(const AttachmentKey &other) const {
	return _slotIndex == other._slotIndex && _name == other._name;
}

std::size_t Skin::HashAttachmentKey::operator()(const Spine::Skin::AttachmentKey &val) const {
	std::size_t h1 = val._slotIndex;
	return h1;
}

Skin::Skin(const String &name) : _name(name) {
	assert(_name.length() > 0);
}

Skin::~Skin() {
	HashMap<AttachmentKey, Attachment *>::Entries entries = _attachments.getEntries();
	while (entries.hasNext()) {
		HashMap<AttachmentKey, Attachment *>::Pair pair = entries.next();
		delete pair.value;
	}
}

void Skin::addAttachment(int slotIndex, const String &name, Attachment *attachment) {
	assert(attachment);
	_attachments.put(AttachmentKey(slotIndex, name), attachment);
}

Attachment *Skin::getAttachment(int slotIndex, const String &name) {
	AttachmentKey key(slotIndex, name.buffer());
	if (_attachments.containsKey(key)) {
		Attachment *attachment = _attachments[key];
		key.getName().unown();
		return attachment;
	} else {
		key.getName().unown();
		return NULL;
	}
}

void Skin::findNamesForSlot(int slotIndex, Vector<String> &names) {
	HashMap<AttachmentKey, Attachment *>::Entries entries = _attachments.getEntries();
	while (entries.hasNext()) {
		HashMap<AttachmentKey, Attachment *>::Pair pair = entries.next();
		if (pair.key._slotIndex == slotIndex) {
			names.add(pair.key._name);
		}
	}
}

void Skin::findAttachmentsForSlot(int slotIndex, Vector<Attachment *> &attachments) {
	HashMap<AttachmentKey, Attachment *>::Entries entries = _attachments.getEntries();
	while (entries.hasNext()) {
		HashMap<AttachmentKey, Attachment *>::Pair pair = entries.next();
		if (pair.key._slotIndex == slotIndex) {
			attachments.add(pair.value);
		}
	}
}

const String &Skin::getName() {
	return _name;
}

HashMap<Skin::AttachmentKey, Attachment *> &Skin::getAttachments() {
	return _attachments;
}

void Skin::attachAll(Skeleton &skeleton, Skin &oldSkin) {
	Vector<Slot *> &slots = skeleton.getSlots();
	HashMap<AttachmentKey, Attachment *>::Entries entries = oldSkin.getAttachments().getEntries();
	while (entries.hasNext()) {
		HashMap<AttachmentKey, Attachment *>::Pair pair = entries.next();
		int slotIndex = pair.key._slotIndex;
		Slot *slot = slots[slotIndex];

		if (slot->getAttachment() == pair.value) {
			Attachment *attachment = NULL;
			if ((attachment = getAttachment(slotIndex, pair.key._name))) {
				slot->setAttachment(attachment);
			}
		}
	}
}
