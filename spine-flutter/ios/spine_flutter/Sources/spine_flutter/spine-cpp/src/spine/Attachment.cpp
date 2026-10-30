/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include <spine/Attachment.h>

#include <spine/Bone.h>
#include <spine/Slot.h>

#include <assert.h>

using namespace spine;

RTTI_IMPL_NOPARENT(Attachment)

Attachment::Attachment(const String &name) : _name(name), _timelineAttachment(this), _refCount(0) {
	assert(_name.length() > 0);
}

Attachment::~Attachment() {
}

const String &Attachment::getName() const {
	return _name;
}

Attachment *Attachment::getTimelineAttachment() {
	return _timelineAttachment;
}

void Attachment::setTimelineAttachment(Attachment *attachment) {
	_timelineAttachment = attachment;
}

Array<int> &Attachment::getTimelineSlots() {
	return _timelineSlots;
}

void Attachment::setTimelineSlots(Array<int> &timelineSlots) {
	_timelineSlots.clearAndAddAll(timelineSlots);
}

bool Attachment::isTimelineActive(Array<Slot *> &slots, int slotIndex, bool appliedPose) {
	Slot *slot = slots[slotIndex];
	if (slot->getBone().isActive()) {
		Attachment *attachment = appliedPose ? slot->getAppliedPose().getAttachment() : slot->getPose().getAttachment();
		if (attachment != NULL && attachment->getTimelineAttachment() == this) return true;
	}
	for (size_t i = 0, n = _timelineSlots.size(); i < n; ++i) {
		slot = slots[_timelineSlots[i]];
		if (!slot->getBone().isActive()) continue;
		Attachment *attachment = appliedPose ? slot->getAppliedPose().getAttachment() : slot->getPose().getAttachment();
		if (attachment != NULL && attachment->getTimelineAttachment() == this) return true;
	}
	return false;
}

int Attachment::getRefCount() {
	return _refCount;
}

void Attachment::reference() {
	_refCount++;
}

void Attachment::dereference() {
	_refCount--;
}
