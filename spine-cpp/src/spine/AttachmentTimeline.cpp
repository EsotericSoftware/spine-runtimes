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

#include <spine/AttachmentTimeline.h>

#include <spine/Event.h>
#include <spine/Skeleton.h>

#include <spine/Animation.h>
#include <spine/Bone.h>
#include <spine/Property.h>
#include <spine/Slot.h>
#include <spine/SlotData.h>
#include <spine/SlotPose.h>

using namespace spine;

RTTI_IMPL_MULTI(AttachmentTimeline, Timeline, SlotTimeline)

AttachmentTimeline::AttachmentTimeline(size_t frameCount, int slotIndex) : Timeline(frameCount, 1), SlotTimeline(), _slotIndex(slotIndex) {
	PropertyId ids[] = {((PropertyId) Property_Attachment << 32) | slotIndex};
	setPropertyIds(ids, 1);
	_instant = true;

	_attachmentNames.ensureCapacity(frameCount);
	for (size_t i = 0; i < frameCount; ++i) {
		_attachmentNames.add(String());
	}
}

AttachmentTimeline::~AttachmentTimeline() {
}

void AttachmentTimeline::setAttachment(Skeleton &skeleton, SlotPose &pose, String *attachmentName) {
	pose.setAttachment(attachmentName == NULL || attachmentName->isEmpty() ? NULL : skeleton.getAttachment(_slotIndex, *attachmentName));
}

void AttachmentTimeline::apply(Skeleton &skeleton, float lastTime, float time, Array<Event *> *events, float alpha, MixFrom from, bool add, bool out,
							   bool appliedPose) {
	SP_UNUSED(lastTime);
	SP_UNUSED(events);
	SP_UNUSED(alpha);
	SP_UNUSED(add);

	Slot *slot = skeleton._slots[_slotIndex];
	if (!slot->_bone.isActive()) return;
	SlotPose &pose = appliedPose ? *slot->_appliedPose : slot->_pose;

	if (out || time < _frames[0]) {
		if (from != MixFrom_Current) setAttachment(skeleton, pose, &slot->_data._attachmentName);
	} else {
		setAttachment(skeleton, pose, &_attachmentNames[Animation::search(_frames, time)]);
	}
}

void AttachmentTimeline::setFrame(int frame, float time, const String &attachmentName) {
	_frames[frame] = time;
	_attachmentNames[frame] = attachmentName;
}

Array<String> &AttachmentTimeline::getAttachmentNames() {
	return _attachmentNames;
}
