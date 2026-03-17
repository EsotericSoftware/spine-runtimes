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

#include <spine/ColorTimeline.h>

#include <spine/Animation.h>
#include <spine/Bone.h>
#include <spine/Event.h>
#include <spine/Property.h>
#include <spine/Skeleton.h>
#include <spine/Slot.h>
#include <spine/SlotData.h>
#include <spine/SlotPose.h>

using namespace spine;

RTTI_IMPL(RGBATimeline, SlotCurveTimeline)

RGBATimeline::RGBATimeline(size_t frameCount, size_t bezierCount, int slotIndex) : SlotCurveTimeline(frameCount, ENTRIES, bezierCount, slotIndex) {
	PropertyId ids[] = {((PropertyId) Property_Rgb << 32) | slotIndex, ((PropertyId) Property_Alpha << 32) | slotIndex};
	setPropertyIds(ids, 2);
}

RGBATimeline::~RGBATimeline() {
}

void RGBATimeline::setFrame(int frame, float time, float r, float g, float b, float a) {
	frame *= ENTRIES;
	_frames[frame] = time;
	_frames[frame + R] = r;
	_frames[frame + G] = g;
	_frames[frame + B] = b;
	_frames[frame + A] = a;
}

void RGBATimeline::_apply(Slot &slot, SlotPose &pose, float time, float alpha, MixBlend blend) {
	Color &color = pose._color;
	if (time < _frames[0]) {
		Color &setup = slot._data._setup._color;
		switch (blend) {
			case MixBlend_Setup:
				color.set(setup);
				return;
			case MixBlend_First:
				color.add((setup.r - color.r) * alpha, (setup.g - color.g) * alpha, (setup.b - color.b) * alpha, (setup.a - color.a) * alpha);
				return;
			default:
				return;
		}
	}

	float r, g, b, a;
	int i = Animation::search(_frames, time, ENTRIES);
	int curveType = (int) _curves[i / ENTRIES];
	switch (curveType) {
		case LINEAR: {
			float before = _frames[i];
			r = _frames[i + R];
			g = _frames[i + G];
			b = _frames[i + B];
			a = _frames[i + A];
			float t = (time - before) / (_frames[i + ENTRIES] - before);
			r += (_frames[i + ENTRIES + R] - r) * t;
			g += (_frames[i + ENTRIES + G] - g) * t;
			b += (_frames[i + ENTRIES + B] - b) * t;
			a += (_frames[i + ENTRIES + A] - a) * t;
			break;
		}
		case STEPPED: {
			r = _frames[i + R];
			g = _frames[i + G];
			b = _frames[i + B];
			a = _frames[i + A];
			break;
		}
		default: {
			r = getBezierValue(time, i, R, curveType - BEZIER);
			g = getBezierValue(time, i, G, curveType + BEZIER_SIZE - BEZIER);
			b = getBezierValue(time, i, B, curveType + BEZIER_SIZE * 2 - BEZIER);
			a = getBezierValue(time, i, A, curveType + BEZIER_SIZE * 3 - BEZIER);
			break;
		}
	}

	if (alpha == 1)
		color.set(r, g, b, a);
	else {
		if (blend == MixBlend_Setup) color.set(slot._data._setup._color);
		color.add((r - color.r) * alpha, (g - color.g) * alpha, (b - color.b) * alpha, (a - color.a) * alpha);
	}
}

RTTI_IMPL(RGBTimeline, SlotCurveTimeline)

RGBTimeline::RGBTimeline(size_t frameCount, size_t bezierCount, int slotIndex) : SlotCurveTimeline(frameCount, ENTRIES, bezierCount, slotIndex) {
	PropertyId ids[] = {((PropertyId) Property_Rgb << 32) | slotIndex};
	setPropertyIds(ids, 1);
}

RGBTimeline::~RGBTimeline() {
}

void RGBTimeline::setFrame(int frame, float time, float r, float g, float b) {
	frame <<= 2;
	_frames[frame] = time;
	_frames[frame + R] = r;
	_frames[frame + G] = g;
	_frames[frame + B] = b;
}

void RGBTimeline::_apply(Slot &slot, SlotPose &pose, float time, float alpha, MixBlend blend) {
	Color &color = pose._color;
	float r, g, b;
	if (time < _frames[0]) {
		Color &setup = slot._data._setup._color;
		switch (blend) {
			case MixBlend_Setup:
				color.r = setup.r;
				color.g = setup.g;
				color.b = setup.b;
				return;
			case MixBlend_First:
				r = color.r + (setup.r - color.r) * alpha;
				g = color.g + (setup.g - color.g) * alpha;
				b = color.b + (setup.b - color.b) * alpha;
				break;
			default:
				return;
		}
	} else {
		int i = Animation::search(_frames, time, ENTRIES);
		int curveType = (int) _curves[i >> 2];
		switch (curveType) {
			case LINEAR: {
				float before = _frames[i];
				r = _frames[i + R];
				g = _frames[i + G];
				b = _frames[i + B];
				float t = (time - before) / (_frames[i + ENTRIES] - before);
				r += (_frames[i + ENTRIES + R] - r) * t;
				g += (_frames[i + ENTRIES + G] - g) * t;
				b += (_frames[i + ENTRIES + B] - b) * t;
				break;
			}
			case STEPPED: {
				r = _frames[i + R];
				g = _frames[i + G];
				b = _frames[i + B];
				break;
			}
			default: {
				r = getBezierValue(time, i, R, curveType - BEZIER);
				g = getBezierValue(time, i, G, curveType + BEZIER_SIZE - BEZIER);
				b = getBezierValue(time, i, B, curveType + BEZIER_SIZE * 2 - BEZIER);
				break;
			}
		}

		if (alpha != 1) {
			if (blend == MixBlend_Setup) {
				Color &setup = slot._data._setup._color;
				r = setup.r + (r - setup.r) * alpha;
				g = setup.g + (g - setup.g) * alpha;
				b = setup.b + (b - setup.b) * alpha;
			} else {
				r = color.r + (r - color.r) * alpha;
				g = color.g + (g - color.g) * alpha;
				b = color.b + (b - color.b) * alpha;
			}
		}
	}
	color.r = r < 0 ? 0 : (r > 1 ? 1 : r);
	color.g = g < 0 ? 0 : (g > 1 ? 1 : g);
	color.b = b < 0 ? 0 : (b > 1 ? 1 : b);
}

RTTI_IMPL(AlphaTimeline, SlotCurveTimeline)

AlphaTimeline::AlphaTimeline(size_t frameCount, size_t bezierCount, int slotIndex)
	: CurveTimeline1(frameCount, bezierCount), SlotTimeline(), _slotIndex(slotIndex) {
	PropertyId ids[] = {((PropertyId) Property_Alpha << 32) | slotIndex};
	setPropertyIds(ids, 1);
}

AlphaTimeline::~AlphaTimeline() {
}

int AlphaTimeline::getSlotIndex() {
	return _slotIndex;
}

void AlphaTimeline::setSlotIndex(int inValue) {
	_slotIndex = inValue;
}

void AlphaTimeline::apply(Skeleton &skeleton, float lastTime, float time, Array<Event *> *events, float alpha, MixBlend blend, MixDirection direction,
						  bool appliedPose) {
	SP_UNUSED(lastTime);
	SP_UNUSED(events);
	SP_UNUSED(direction);

	Slot *slot = skeleton._slots[_slotIndex];
	if (!slot->_bone._active) return;

	Color &color = (appliedPose ? *slot->_applied : slot->_pose)._color;
	float a;
	if (time < _frames[0]) {
		Color &setup = slot->_data._setup._color;
		switch (blend) {
			case MixBlend_Setup:
				color.a = setup.a;
				return;
			case MixBlend_First:
				a = color.a + (setup.a - color.a) * alpha;
				break;
			default:
				return;
		}
	} else {
		a = getCurveValue(time);
		if (alpha != 1) {
			if (blend == MixBlend_Setup) {
				Color &setup = slot->_data._setup._color;
				a = setup.a + (a - setup.a) * alpha;
			} else
				a = color.a + (a - color.a) * alpha;
		}
	}
	color.a = a < 0 ? 0 : (a > 1 ? 1 : a);
}

RTTI_IMPL(RGBA2Timeline, SlotCurveTimeline)

RGBA2Timeline::RGBA2Timeline(size_t frameCount, size_t bezierCount, int slotIndex) : SlotCurveTimeline(frameCount, ENTRIES, bezierCount, slotIndex) {
	PropertyId ids[] = {((PropertyId) Property_Rgb << 32) | slotIndex, ((PropertyId) Property_Alpha << 32) | slotIndex,
						((PropertyId) Property_Rgb2 << 32) | slotIndex};
	setPropertyIds(ids, 3);
}

RGBA2Timeline::~RGBA2Timeline() {
}

void RGBA2Timeline::setFrame(int frame, float time, float r, float g, float b, float a, float r2, float g2, float b2) {
	frame <<= 3;
	_frames[frame] = time;
	_frames[frame + R] = r;
	_frames[frame + G] = g;
	_frames[frame + B] = b;
	_frames[frame + A] = a;
	_frames[frame + R2] = r2;
	_frames[frame + G2] = g2;
	_frames[frame + B2] = b2;
}

void RGBA2Timeline::_apply(Slot &slot, SlotPose &pose, float time, float alpha, MixBlend blend) {
	Color &light = pose._color;
	Color &dark = pose._darkColor;
	float r2, g2, b2;
	if (time < _frames[0]) {
		SlotPose &setup = slot._data._setup;
		Color &setupLight = setup._color;
		Color &setupDark = setup._darkColor;
		switch (blend) {
			case MixBlend_Setup:
				light.set(setupLight);
				dark.r = setupDark.r;
				dark.g = setupDark.g;
				dark.b = setupDark.b;
				/* Fall through. */
			default:
				return;
			case MixBlend_First:
				light.add((setupLight.r - light.r) * alpha, (setupLight.g - light.g) * alpha, (setupLight.b - light.b) * alpha,
						  (setupLight.a - light.a) * alpha);
				r2 = dark.r + (setupDark.r - dark.r) * alpha;
				g2 = dark.g + (setupDark.g - dark.g) * alpha;
				b2 = dark.b + (setupDark.b - dark.b) * alpha;
		}
	} else {
		float r, g, b, a;
		int i = Animation::search(_frames, time, ENTRIES);
		int curveType = (int) _curves[i >> 3];
		switch (curveType) {
			case LINEAR: {
				float before = _frames[i];
				r = _frames[i + R];
				g = _frames[i + G];
				b = _frames[i + B];
				a = _frames[i + A];
				r2 = _frames[i + R2];
				g2 = _frames[i + G2];
				b2 = _frames[i + B2];
				float t = (time - before) / (_frames[i + ENTRIES] - before);
				r += (_frames[i + ENTRIES + R] - r) * t;
				g += (_frames[i + ENTRIES + G] - g) * t;
				b += (_frames[i + ENTRIES + B] - b) * t;
				a += (_frames[i + ENTRIES + A] - a) * t;
				r2 += (_frames[i + ENTRIES + R2] - r2) * t;
				g2 += (_frames[i + ENTRIES + G2] - g2) * t;
				b2 += (_frames[i + ENTRIES + B2] - b2) * t;
				break;
			}
			case STEPPED: {
				r = _frames[i + R];
				g = _frames[i + G];
				b = _frames[i + B];
				a = _frames[i + A];
				r2 = _frames[i + R2];
				g2 = _frames[i + G2];
				b2 = _frames[i + B2];
				break;
			}
			default: {
				r = getBezierValue(time, i, R, curveType - BEZIER);
				g = getBezierValue(time, i, G, curveType + BEZIER_SIZE - BEZIER);
				b = getBezierValue(time, i, B, curveType + BEZIER_SIZE * 2 - BEZIER);
				a = getBezierValue(time, i, A, curveType + BEZIER_SIZE * 3 - BEZIER);
				r2 = getBezierValue(time, i, R2, curveType + BEZIER_SIZE * 4 - BEZIER);
				g2 = getBezierValue(time, i, G2, curveType + BEZIER_SIZE * 5 - BEZIER);
				b2 = getBezierValue(time, i, B2, curveType + BEZIER_SIZE * 6 - BEZIER);
				break;
			}
		}

		if (alpha == 1)
			light.set(r, g, b, a);
		else if (blend == MixBlend_Setup) {
			SlotPose &setup = slot._data._setup;
			Color &setupLight = setup._color;
			light.set(setupLight.r + (r - setupLight.r) * alpha, setupLight.g + (g - setupLight.g) * alpha, setupLight.b + (b - setupLight.b) * alpha,
					  setupLight.a + (a - setupLight.a) * alpha);
			Color &setupDark = setup._darkColor;
			r2 = setupDark.r + (r2 - setupDark.r) * alpha;
			g2 = setupDark.g + (g2 - setupDark.g) * alpha;
			b2 = setupDark.b + (b2 - setupDark.b) * alpha;
		} else {
			light.add((r - light.r) * alpha, (g - light.g) * alpha, (b - light.b) * alpha, (a - light.a) * alpha);
			r2 = dark.r + (r2 - dark.r) * alpha;
			g2 = dark.g + (g2 - dark.g) * alpha;
			b2 = dark.b + (b2 - dark.b) * alpha;
		}
	}

	dark.r = r2 < 0 ? 0 : (r2 > 1 ? 1 : r2);
	dark.g = g2 < 0 ? 0 : (g2 > 1 ? 1 : g2);
	dark.b = b2 < 0 ? 0 : (b2 > 1 ? 1 : b2);
}

RTTI_IMPL(RGB2Timeline, SlotCurveTimeline)

RGB2Timeline::RGB2Timeline(size_t frameCount, size_t bezierCount, int slotIndex) : SlotCurveTimeline(frameCount, ENTRIES, bezierCount, slotIndex) {
	PropertyId ids[] = {((PropertyId) Property_Rgb << 32) | slotIndex, ((PropertyId) Property_Rgb2 << 32) | slotIndex};
	setPropertyIds(ids, 2);
}

RGB2Timeline::~RGB2Timeline() {
}

void RGB2Timeline::setFrame(int frame, float time, float r, float g, float b, float r2, float g2, float b2) {
	frame *= ENTRIES;
	_frames[frame] = time;
	_frames[frame + R] = r;
	_frames[frame + G] = g;
	_frames[frame + B] = b;
	_frames[frame + R2] = r2;
	_frames[frame + G2] = g2;
	_frames[frame + B2] = b2;
}

void RGB2Timeline::_apply(Slot &slot, SlotPose &pose, float time, float alpha, MixBlend blend) {
	Color &light = pose._color;
	Color &dark = pose._darkColor;
	float r, g, b, r2, g2, b2;
	if (time < _frames[0]) {
		SlotPose &setup = slot._data._setup;
		Color &setupLight = setup._color;
		Color &setupDark = setup._darkColor;
		switch (blend) {
			case MixBlend_Setup:
				light.r = setupLight.r;
				light.g = setupLight.g;
				light.b = setupLight.b;
				dark.r = setupDark.r;
				dark.g = setupDark.g;
				dark.b = setupDark.b;
				/* Fall through. */
			default:
				return;
			case MixBlend_First:
				r = light.r + (setupLight.r - light.r) * alpha;
				g = light.g + (setupLight.g - light.g) * alpha;
				b = light.b + (setupLight.b - light.b) * alpha;
				r2 = dark.r + (setupDark.r - dark.r) * alpha;
				g2 = dark.g + (setupDark.g - dark.g) * alpha;
				b2 = dark.b + (setupDark.b - dark.b) * alpha;
		}
	} else {
		int i = Animation::search(_frames, time, ENTRIES);
		int curveType = (int) _curves[i / ENTRIES];
		switch (curveType) {
			case LINEAR: {
				float before = _frames[i];
				r = _frames[i + R];
				g = _frames[i + G];
				b = _frames[i + B];
				r2 = _frames[i + R2];
				g2 = _frames[i + G2];
				b2 = _frames[i + B2];
				float t = (time - before) / (_frames[i + ENTRIES] - before);
				r += (_frames[i + ENTRIES + R] - r) * t;
				g += (_frames[i + ENTRIES + G] - g) * t;
				b += (_frames[i + ENTRIES + B] - b) * t;
				r2 += (_frames[i + ENTRIES + R2] - r2) * t;
				g2 += (_frames[i + ENTRIES + G2] - g2) * t;
				b2 += (_frames[i + ENTRIES + B2] - b2) * t;
				break;
			}
			case STEPPED: {
				r = _frames[i + R];
				g = _frames[i + G];
				b = _frames[i + B];
				r2 = _frames[i + R2];
				g2 = _frames[i + G2];
				b2 = _frames[i + B2];
				break;
			}
			default: {
				r = getBezierValue(time, i, R, curveType - BEZIER);
				g = getBezierValue(time, i, G, curveType + BEZIER_SIZE - BEZIER);
				b = getBezierValue(time, i, B, curveType + BEZIER_SIZE * 2 - BEZIER);
				r2 = getBezierValue(time, i, R2, curveType + BEZIER_SIZE * 3 - BEZIER);
				g2 = getBezierValue(time, i, G2, curveType + BEZIER_SIZE * 4 - BEZIER);
				b2 = getBezierValue(time, i, B2, curveType + BEZIER_SIZE * 5 - BEZIER);
				break;
			}
		}

		if (alpha != 1) {
			if (blend == MixBlend_Setup) {
				SlotPose &setup = slot._data._setup;
				Color &setupLight = setup._color;
				r = setupLight.r + (r - setupLight.r) * alpha;
				g = setupLight.g + (g - setupLight.g) * alpha;
				b = setupLight.b + (b - setupLight.b) * alpha;
				Color &setupDark = setup._darkColor;
				r2 = setupDark.r + (r2 - setupDark.r) * alpha;
				g2 = setupDark.g + (g2 - setupDark.g) * alpha;
				b2 = setupDark.b + (b2 - setupDark.b) * alpha;
			} else {
				r = light.r + (r - light.r) * alpha;
				g = light.g + (g - light.g) * alpha;
				b = light.b + (b - light.b) * alpha;
				r2 = dark.r + (r2 - dark.r) * alpha;
				g2 = dark.g + (g2 - dark.g) * alpha;
				b2 = dark.b + (b2 - dark.b) * alpha;
			}
		}
	}

	light.r = r < 0 ? 0 : (r > 1 ? 1 : r);
	light.g = g < 0 ? 0 : (g > 1 ? 1 : g);
	light.b = b < 0 ? 0 : (b > 1 ? 1 : b);
	dark.r = r2 < 0 ? 0 : (r2 > 1 ? 1 : r2);
	dark.g = g2 < 0 ? 0 : (g2 > 1 ? 1 : g2);
	dark.b = b2 < 0 ? 0 : (b2 > 1 ? 1 : b2);
}
