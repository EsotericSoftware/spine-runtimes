/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ******************************************************************************/

#ifndef SPINE_SKIN_H_
#define SPINE_SKIN_H_

#include <string>
#include <map>

namespace spine {

class BaseSkeleton;
class Attachment;

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
	~Skin ();

	/** The Skin owns the attachment. */
	void addAttachment (int slotIndex, const std::string &name, Attachment *attachment);

	Attachment* getAttachment (int slotIndex, const std::string &name);
};

} /* namespace spine */
#endif /* SPINE_SKIN_H_ */
