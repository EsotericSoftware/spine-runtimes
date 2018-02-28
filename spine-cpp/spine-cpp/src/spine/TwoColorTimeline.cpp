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

#include <spine/TwoColorTimeline.h>

#include <spine/Skeleton.h>
#include <spine/Event.h>

#include <spine/Animation.h>
#include <spine/TimelineType.h>
#include <spine/Slot.h>
#include <spine/SlotData.h>

namespace Spine {
RTTI_IMPL(TwoColorTimeline, CurveTimeline);

const int TwoColorTimeline::ENTRIES = 8;
const int TwoColorTimeline::PREV_TIME = -8;
const int TwoColorTimeline::PREV_R = -7;
const int TwoColorTimeline::PREV_G = -6;
const int TwoColorTimeline::PREV_B = -5;
const int TwoColorTimeline::PREV_A = -4;
const int TwoColorTimeline::PREV_R2 = -3;
const int TwoColorTimeline::PREV_G2 = -2;
const int TwoColorTimeline::PREV_B2 = -1;
const int TwoColorTimeline::R = 1;
const int TwoColorTimeline::G = 2;
const int TwoColorTimeline::B = 3;
const int TwoColorTimeline::A = 4;
const int TwoColorTimeline::R2 = 5;
const int TwoColorTimeline::G2 = 6;
const int TwoColorTimeline::B2 = 7;

TwoColorTimeline::TwoColorTimeline(int frameCount) : CurveTimeline(frameCount), _slotIndex(0) {
	_frames.ensureCapacity(frameCount * ENTRIES);
	_frames.setSize(frameCount * ENTRIES, 0);
}

void TwoColorTimeline::apply(Skeleton &skeleton, float lastTime, float time, Vector<Event *> *pEvents, float alpha,
							 MixPose pose, MixDirection direction) {
	Slot *slotP = skeleton._slots[_slotIndex];
	Slot &slot = *slotP;

	if (time < _frames[0]) {
		// Time is before first frame.
		switch (pose) {
			case MixPose_Setup:
				slot.getColor().set(slot.getData().getColor());
				slot.getDarkColor().set(slot.getData().getDarkColor());
				return;
			case MixPose_Current: {
				Color &color = slot.getColor();
				color._r += (color._r - slot._data.getColor()._r) * alpha;
				color._g += (color._g - slot._data.getColor()._g) * alpha;
				color._b += (color._b - slot._data.getColor()._b) * alpha;
				color._a += (color._a - slot._data.getColor()._a) * alpha;

				Color &darkColor = slot.getDarkColor();
				darkColor._r += (darkColor._r - slot._data.getDarkColor()._r) * alpha;
				darkColor._g += (darkColor._g - slot._data.getDarkColor()._g) * alpha;
				darkColor._b += (darkColor._b - slot._data.getDarkColor()._b) * alpha;
				return;
			}
			case MixPose_CurrentLayered:
			default:
				return;
		}
	}

	float r, g, b, a, r2, g2, b2;
	if (time >= _frames[_frames.size() - ENTRIES]) {
		// Time is after last frame.
		int i = static_cast<int>(_frames.size());
		r = _frames[i + PREV_R];
		g = _frames[i + PREV_G];
		b = _frames[i + PREV_B];
		a = _frames[i + PREV_A];
		r2 = _frames[i + PREV_R2];
		g2 = _frames[i + PREV_G2];
		b2 = _frames[i + PREV_B2];
	} else {
		// Interpolate between the previous frame and the current frame.
		int frame = Animation::binarySearch(_frames, time, ENTRIES);
		r = _frames[frame + PREV_R];
		g = _frames[frame + PREV_G];
		b = _frames[frame + PREV_B];
		a = _frames[frame + PREV_A];
		r2 = _frames[frame + PREV_R2];
		g2 = _frames[frame + PREV_G2];
		b2 = _frames[frame + PREV_B2];
		float frameTime = _frames[frame];
		float percent = getCurvePercent(frame / ENTRIES - 1,
										1 - (time - frameTime) / (_frames[frame + PREV_TIME] - frameTime));

		r += (_frames[frame + R] - r) * percent;
		g += (_frames[frame + G] - g) * percent;
		b += (_frames[frame + B] - b) * percent;
		a += (_frames[frame + A] - a) * percent;
		r2 += (_frames[frame + R2] - r2) * percent;
		g2 += (_frames[frame + G2] - g2) * percent;
		b2 += (_frames[frame + B2] - b2) * percent;
	}

	if (alpha == 1) {
		Color &color = slot.getColor();
		color._r = r;
		color._g = g;
		color._b = b;
		color._a = a;

		Color &darkColor = slot.getDarkColor();
		darkColor._r = r2;
		darkColor._g = g2;
		darkColor._b = b2;
	} else {
		float br, bg, bb, ba, br2, bg2, bb2;
		if (pose == MixPose_Setup) {
			br = slot._data.getColor()._r;
			bg = slot._data.getColor()._g;
			bb = slot._data.getColor()._b;
			ba = slot._data.getColor()._a;
			br2 = slot._data.getDarkColor()._r;
			bg2 = slot._data.getDarkColor()._g;
			bb2 = slot._data.getDarkColor()._b;
		} else {
			Color &color = slot.getColor();
			br = color._r;
			bg = color._g;
			bb = color._b;
			ba = color._a;

			Color &darkColor = slot.getDarkColor();
			br2 = darkColor._r;
			bg2 = darkColor._g;
			bb2 = darkColor._b;
		}

		Color &color = slot.getColor();
		color._r = br + ((r - br) * alpha);
		color._g = bg + ((g - bg) * alpha);
		color._b = bb + ((b - bb) * alpha);
		color._a = ba + ((a - ba) * alpha);

		Color &darkColor = slot.getDarkColor();
		darkColor._r = br2 + ((r2 - br2) * alpha);
		darkColor._g = bg2 + ((g2 - bg2) * alpha);
		darkColor._b = bb2 + ((b2 - bb2) * alpha);
	}
}

int TwoColorTimeline::getPropertyId() {
	return ((int) TimelineType_TwoColor << 24) + _slotIndex;
}

void TwoColorTimeline::setFrame(int frameIndex, float time, float r, float g, float b, float a, float r2, float g2,
								float b2) {
	frameIndex *= ENTRIES;
	_frames[frameIndex] = time;
	_frames[frameIndex + R] = r;
	_frames[frameIndex + G] = g;
	_frames[frameIndex + B] = b;
	_frames[frameIndex + A] = a;
	_frames[frameIndex + R2] = r2;
	_frames[frameIndex + G2] = g2;
	_frames[frameIndex + B2] = b2;
}

int TwoColorTimeline::getSlotIndex() {
	return _slotIndex;
}

void TwoColorTimeline::setSlotIndex(int inValue) {
	assert(inValue >= 0);
	_slotIndex = inValue;
}
}
