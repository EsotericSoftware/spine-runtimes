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

#include <spine/DrawOrderFolderTimeline.h>

#include <spine/Animation.h>
#include <spine/Event.h>
#include <spine/Property.h>
#include <spine/Skeleton.h>
#include <spine/Slot.h>
#include <spine/SlotData.h>

using namespace spine;

RTTI_IMPL(DrawOrderFolderTimeline, Timeline)

DrawOrderFolderTimeline::DrawOrderFolderTimeline(size_t frameCount, Array<int> &slots, size_t slotCount) : Timeline(frameCount, 1) {
	PropertyId ids[] = {((PropertyId) Property_DrawOrder << 32)};
	setPropertyIds(ids, 1);

	_slots.addAll(slots);
	_drawOrders.ensureCapacity(frameCount);
	_inFolder.setSize(slotCount, false);
	for (size_t i = 0; i < _slots.size(); ++i) _inFolder[_slots[i]] = true;
	for (size_t i = 0; i < frameCount; ++i) {
		Array<int> vec;
		_drawOrders.add(vec);
	}
}

void DrawOrderFolderTimeline::apply(Skeleton &skeleton, float lastTime, float time, Array<Event *> *events, float alpha, MixBlend blend,
									MixDirection direction, bool appliedPose) {
	SP_UNUSED(lastTime);
	SP_UNUSED(events);
	SP_UNUSED(alpha);
	SP_UNUSED(appliedPose);

	if (direction == MixDirection_Out) {
		if (blend == MixBlend_Setup) setup(skeleton);
	} else if (time < _frames[0]) {
		if (blend == MixBlend_Setup || blend == MixBlend_First) setup(skeleton);
	} else {
		Array<int> &drawOrder = _drawOrders[Animation::search(_frames, time)];
		if (drawOrder.size() == 0)
			setup(skeleton);
		else
			apply(skeleton, drawOrder);
	}
}

size_t DrawOrderFolderTimeline::getFrameCount() {
	return _frames.size();
}

Array<int> &DrawOrderFolderTimeline::getSlots() {
	return _slots;
}

Array<Array<int>> &DrawOrderFolderTimeline::getDrawOrders() {
	return _drawOrders;
}

void DrawOrderFolderTimeline::setFrame(size_t frame, float time, Array<int> *drawOrder) {
	_frames[frame] = time;
	_drawOrders[frame].clear();
	if (drawOrder != NULL) _drawOrders[frame].addAll(*drawOrder);
}

void DrawOrderFolderTimeline::setup(Skeleton &skeleton) {
	Array<Slot *> &drawOrder = skeleton._drawOrder;
	Array<Slot *> &allSlots = skeleton._slots;
	for (size_t i = 0, found = 0, done = _slots.size();; ++i) {
		if (_inFolder[drawOrder[i]->getData().getIndex()]) {
			drawOrder[i] = allSlots[_slots[found]];
			if (++found == done) break;
		}
	}
}

void DrawOrderFolderTimeline::apply(Skeleton &skeleton, Array<int> &drawOrderIndices) {
	Array<Slot *> &drawOrder = skeleton._drawOrder;
	Array<Slot *> &allSlots = skeleton._slots;
	for (size_t i = 0, found = 0, done = _slots.size();; ++i) {
		if (_inFolder[drawOrder[i]->getData().getIndex()]) {
			drawOrder[i] = allSlots[_slots[drawOrderIndices[found]]];
			if (++found == done) break;
		}
	}
}
