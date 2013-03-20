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

#include <stdexcept>
#include <spine/Slot.h>
#include <spine/SlotData.h>
#include <spine/BaseSkeleton.h>
#include <spine/SkeletonData.h>

namespace spine {

Slot::Slot (SlotData *data, BaseSkeleton *skeleton, Bone *bone) :
				attachmentTime(0),
				data(data),
				skeleton(skeleton),
				bone(bone),
				r(1),
				g(1),
				b(1),
				a(1),
				attachment(0) {
	if (!data) throw std::invalid_argument("data cannot be null.");
	if (!skeleton) throw std::invalid_argument("skeleton cannot be null.");
	if (!bone) throw std::invalid_argument("bone cannot be null.");
	setToBindPose();
}

void Slot::setAttachment (Attachment *attachment) {
	this->attachment = attachment;
	attachmentTime = skeleton->time;
}

void Slot::setAttachmentTime (float time) {
	attachmentTime = skeleton->time - time;
}

float Slot::getAttachmentTime () const {
	return skeleton->time - attachmentTime;
}

void Slot::setToBindPose () {
	for (int i = 0, n = skeleton->data->slots.size(); i < n; i++) {
		if (data == skeleton->data->slots[i]) {
			setToBindPose(i);
			return;
		}
	}
}

void Slot::setToBindPose (int slotIndex) {
	r = data->r;
	g = data->g;
	b = data->b;
	a = data->a;
	setAttachment(data->attachmentName ? skeleton->getAttachment(slotIndex, *data->attachmentName) : 0);
}

} /* namespace spine */
