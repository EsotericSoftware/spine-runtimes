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

#include <spine/DrawOrderTimeline.h>

#include <spine/Event.h>
#include <spine/Skeleton.h>

#include <spine/Animation.h>
#include <spine/Property.h>
#include <spine/Slot.h>
#include <spine/SlotData.h>

using namespace spine;

RTTI_IMPL(DrawOrderTimeline, Timeline)

PropertyId DrawOrderTimeline::getPropertyId() {
	return ((PropertyId) Property_DrawOrder << 32);
}

DrawOrderTimeline::DrawOrderTimeline(size_t frameCount) : Timeline(frameCount, 1) {
	PropertyId ids[] = {getPropertyId()};
	setPropertyIds(ids, 1);
	_instant = true;

	_drawOrders.ensureCapacity(frameCount);
	for (size_t i = 0; i < frameCount; ++i) {
		Array<int> vec;
		_drawOrders.add(vec);
	}
}

void DrawOrderTimeline::apply(Skeleton &skeleton, float lastTime, float time, Array<Event *> *events, float alpha, MixFrom from, bool add, bool out,
							  bool appliedPose) {
	SP_UNUSED(appliedPose);
	SP_UNUSED(lastTime);
	SP_UNUSED(events);
	SP_UNUSED(alpha);
	SP_UNUSED(add);

	Array<Slot *> &pose = appliedPose ? skeleton._drawOrder.getAppliedPose() : skeleton._drawOrder.getPose();
	Array<Slot *> &setup = skeleton._slots;
	if (out || time < _frames[0]) {
		if (from != MixFrom_Current) {
			pose.setSize(setup.size(), NULL);
			for (size_t i = 0, n = setup.size(); i < n; ++i) pose[i] = setup[i];
		}
		return;
	}

	Array<int> &drawOrderToSetupIndex = _drawOrders[Animation::search(_frames, time)];
	if (drawOrderToSetupIndex.size() == 0) {
		pose.setSize(setup.size(), NULL);
		for (size_t i = 0, n = setup.size(); i < n; ++i) pose[i] = setup[i];
	} else {
		pose.setSize(drawOrderToSetupIndex.size(), NULL);
		for (size_t i = 0, n = drawOrderToSetupIndex.size(); i < n; ++i) pose[i] = setup[drawOrderToSetupIndex[i]];
	}
}

void DrawOrderTimeline::setFrame(size_t frame, float time, Array<int> *drawOrder) {
	_frames[frame] = time;
	_drawOrders[frame].clear();
	if (drawOrder != NULL) {
		_drawOrders[frame].addAll(*drawOrder);
	}
}

size_t DrawOrderTimeline::getFrameCount() {
	return _frames.size();
}

Array<Array<int>> &DrawOrderTimeline::getDrawOrders() {
	return _drawOrders;
}
