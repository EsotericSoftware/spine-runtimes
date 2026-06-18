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

#include <spine/SequenceTimeline.h>
#include <spine/Bone.h>
#include <spine/RegionAttachment.h>
#include <spine/MeshAttachment.h>
#include <spine/VertexAttachment.h>
#include <spine/Event.h>
#include <spine/Skeleton.h>
#include <spine/Attachment.h>
#include <spine/Slot.h>
#include <spine/Animation.h>
#include <spine/MathUtil.h>

using namespace spine;

RTTI_IMPL_MULTI(SequenceTimeline, Timeline, SlotTimeline)

SequenceTimeline::SequenceTimeline(size_t frameCount, int slotIndex, Attachment &attachment)
	: Timeline(frameCount, ENTRIES), SlotTimeline(), _slotIndex(slotIndex), _attachment(&attachment) {
	_instant = true;
	int sequenceId = 0;
	if (attachment.getRTTI().instanceOf(RegionAttachment::rtti)) sequenceId = ((RegionAttachment *) &attachment)->getSequence().getId();
	if (attachment.getRTTI().instanceOf(MeshAttachment::rtti)) sequenceId = ((MeshAttachment *) &attachment)->getSequence().getId();
	PropertyId ids[] = {((PropertyId) Property_Sequence << 32) | ((slotIndex << 16 | sequenceId) & 0xffffffff)};
	setPropertyIds(ids, 1);
}

SequenceTimeline::~SequenceTimeline() {
}

void SequenceTimeline::setFrame(int frame, float time, SequenceMode mode, int index, float delay) {
	Array<float> &frames = this->_frames;
	frame *= ENTRIES;
	frames[frame] = time;
	frames[frame + MODE] = mode | (index << 4);
	frames[frame + DELAY] = delay;
}

int SequenceTimeline::getSlotIndex() {
	return _slotIndex;
}

void SequenceTimeline::setSlotIndex(int inValue) {
	_slotIndex = inValue;
}

void SequenceTimeline::apply(Skeleton &skeleton, float lastTime, float time, Array<Event *> *events, float alpha, MixFrom from, bool add, bool out,
							 bool appliedPose) {
	SP_UNUSED(alpha);
	SP_UNUSED(lastTime);
	SP_UNUSED(events);
	SP_UNUSED(add);

	Array<Slot *> &slots = skeleton.getSlots();
	if (!_attachment->isTimelineActive(slots, getSlotIndex(), appliedPose)) return;
	Array<int> &timelineSlots = _attachment->getTimelineSlots();

	Sequence *sequence = NULL;
	if (_attachment->getRTTI().instanceOf(RegionAttachment::rtti)) sequence = &static_cast<RegionAttachment *>(_attachment)->getSequence();
	if (_attachment->getRTTI().instanceOf(MeshAttachment::rtti)) sequence = &static_cast<MeshAttachment *>(_attachment)->getSequence();
	if (!sequence) return;

	Array<float> &frames = this->_frames;
	if (out || time < frames[0]) {
		if (from != MixFrom_Current) {
			setupPose(*slots[getSlotIndex()], appliedPose);
			for (size_t i = 0; i < timelineSlots.size(); ++i) setupPose(*slots[timelineSlots[i]], appliedPose);
		}
		return;
	}

	int i = Animation::search(frames, time, ENTRIES);
	float before = frames[i];
	int modeAndIndex = (int) frames[i + MODE];
	float delay = frames[i + DELAY];

	applyToSlot(*slots[getSlotIndex()], appliedPose, *sequence, time, before, modeAndIndex, delay);
	for (size_t j = 0; j < timelineSlots.size(); ++j)
		applyToSlot(*slots[timelineSlots[j]], appliedPose, *sequence, time, before, modeAndIndex, delay);
}

void SequenceTimeline::setupPose(Slot &slot, bool appliedPose) {
	if (!slot.getBone().isActive()) return;
	SlotPose &pose = appliedPose ? slot.getAppliedPose() : slot.getPose();
	Attachment *attachment = pose.getAttachment();
	if (attachment == NULL || attachment->getTimelineAttachment() != _attachment) return;
	pose.setSequenceIndex(-1);
}

void SequenceTimeline::applyToSlot(Slot &slot, bool appliedPose, Sequence &sequence, float time, float before, int modeAndIndex, float delay) {
	if (!slot.getBone().isActive()) return;
	SlotPose &pose = appliedPose ? slot.getAppliedPose() : slot.getPose();
	Attachment *attachment = pose.getAttachment();
	if (attachment == NULL || attachment->getTimelineAttachment() != _attachment) return;

	int index = modeAndIndex >> 4, count = (int) sequence.getRegions().size();
	int mode = modeAndIndex & 0xf;
	if (mode != SequenceMode_hold) {
		index += (int) (((time - before) / delay + 0.0001));
		switch (mode) {
			case SequenceMode_once:
				index = MathUtil::min(count - 1, index);
				break;
			case SequenceMode_loop:
				index %= count;
				break;
			case SequenceMode_pingpong: {
				int n = (count << 1) - 2;
				index = n == 0 ? 0 : index % n;
				if (index >= count) index = n - index;
				break;
			}
			case SequenceMode_onceReverse:
				index = MathUtil::max(count - 1 - index, 0);
				break;
			case SequenceMode_loopReverse:
				index = count - 1 - (index % count);
				break;
			case SequenceMode_pingpongReverse: {
				int n = (count << 1) - 2;
				index = n == 0 ? 0 : (index + count - 1) % n;
				if (index >= count) index = n - index;
				break;
			}
		}
	}
	pose.setSequenceIndex(index);
}
