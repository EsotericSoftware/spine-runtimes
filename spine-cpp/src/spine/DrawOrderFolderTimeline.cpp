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
	Array<PropertyId> ids(slots.size());
	for (size_t i = 0; i < slots.size(); ++i) ids.add(((PropertyId) Property_DrawOrderFolder << 32) | (PropertyId) slots[i]);
	setPropertyIds(ids.buffer(), ids.size());

	_slots.addAll(slots);
	_drawOrders.ensureCapacity(frameCount);
	_inFolder.setSize(slotCount, false);
	for (size_t i = 0; i < _slots.size(); ++i) _inFolder[_slots[i]] = true;
	_instant = true;
	for (size_t i = 0; i < frameCount; ++i) {
		Array<int> vec;
		_drawOrders.add(vec);
	}
}

void DrawOrderFolderTimeline::apply(Skeleton &skeleton, float lastTime, float time, Array<Event *> *events, float alpha, MixFrom from, bool add,
									bool out, bool appliedPose) {
	SP_UNUSED(lastTime);
	SP_UNUSED(events);
	SP_UNUSED(alpha);
	SP_UNUSED(add);
	Array<Slot *> &pose = appliedPose ? skeleton._drawOrder.getAppliedPose() : skeleton._drawOrder.getPose();
	Array<Slot *> &setupPose = skeleton._slots;

	if (out || time < _frames[0]) {
		if (from != MixFrom_Current) setup(pose, setupPose);
	} else {
		Array<int> &drawOrder = _drawOrders[Animation::search(_frames, time)];
		if (drawOrder.size() == 0)
			setup(pose, setupPose);
		else
			apply(pose, setupPose, drawOrder);
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

void DrawOrderFolderTimeline::setup(Array<Slot *> &pose, Array<Slot *> &setupPose) {
	for (size_t i = 0, found = 0, done = _slots.size();; ++i) {
		if (_inFolder[pose[i]->getData().getIndex()]) {
			pose[i] = setupPose[_slots[found]];
			if (++found == done) break;
		}
	}
}

void DrawOrderFolderTimeline::apply(Array<Slot *> &pose, Array<Slot *> &setupPose, Array<int> &drawOrderIndices) {
	for (size_t i = 0, found = 0, done = _slots.size();; ++i) {
		if (_inFolder[pose[i]->getData().getIndex()]) {
			pose[i] = setupPose[_slots[drawOrderIndices[found]]];
			if (++found == done) break;
		}
	}
}
