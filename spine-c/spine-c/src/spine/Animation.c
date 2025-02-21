/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include <limits.h>
#include <spine/Animation.h>
#include <spine/IkConstraint.h>
#include <spine/extension.h>

_SP_ARRAY_IMPLEMENT_TYPE(spPropertyIdArray, spPropertyId)

_SP_ARRAY_IMPLEMENT_TYPE(spTimelineArray, spTimeline *)

spAnimation *spAnimation_create(const char *name, spTimelineArray *timelines, float duration) {
	int i, n, totalCount = 0;
	spAnimation *self = NEW(spAnimation);
	MALLOC_STR(self->name, name);
	self->timelines = timelines != NULL ? timelines : spTimelineArray_create(1);
	timelines = self->timelines;

	for (i = 0, n = timelines->size; i < n; i++)
		totalCount += timelines->items[i]->propertyIdsCount;
	self->timelineIds = spPropertyIdArray_create(totalCount);

	for (i = 0, n = timelines->size; i < n; i++) {
		spPropertyIdArray_addAllValues(self->timelineIds, timelines->items[i]->propertyIds, 0,
									   timelines->items[i]->propertyIdsCount);
	}
	self->duration = duration;
	return self;
}

void spAnimation_dispose(spAnimation *self) {
	int i;
	for (i = 0; i < self->timelines->size; ++i)
		spTimeline_dispose(self->timelines->items[i]);
	spTimelineArray_dispose(self->timelines);
	spPropertyIdArray_dispose(self->timelineIds);
	FREE(self->name);
	FREE(self);
}

int /*bool*/ spAnimation_hasTimeline(spAnimation *self, spPropertyId *ids, int idsCount) {
	int i, n, ii;
	for (i = 0, n = self->timelineIds->size; i < n; i++) {
		for (ii = 0; ii < idsCount; ii++) {
			if (self->timelineIds->items[i] == ids[ii]) return 1;
		}
	}
	return 0;
}

void spAnimation_apply(const spAnimation *self, spSkeleton *skeleton, float lastTime, float time, int loop, spEvent **events,
					   int *eventsCount, float alpha, spMixBlend blend, spMixDirection direction) {
	int i, n = self->timelines->size;

	if (loop && self->duration) {
		time = FMOD(time, self->duration);
		if (lastTime > 0) lastTime = FMOD(lastTime, self->duration);
	}

	for (i = 0; i < n; ++i)
		spTimeline_apply(self->timelines->items[i], skeleton, lastTime, time, events, eventsCount, alpha, blend,
						 direction);
}

static int search(spFloatArray
						  *values,
				  float time) {
	int i, n;
	float *items = values->items;
	for (
			i = 1, n = values->size;
			i < n;
			i++)
		if (items[i] > time) return i - 1;
	return values->size - 1;
}

static int search2(spFloatArray
						   *values,
				   float time,
				   int step) {
	int i, n;
	float *items = values->items;
	for (
			i = step, n = values->size;
			i < n;
			i += step)
		if (items[i] > time) return i -
									step;
	return values->size -
		   step;
}

/**/

void _spTimeline_init(spTimeline *self,
					  int frameCount,
					  int frameEntries,
					  spPropertyId *propertyIds,
					  int propertyIdsCount,
					  spTimelineType type,
					  void (*dispose)(spTimeline *self),
					  void (*apply)(spTimeline *self, spSkeleton *skeleton, float lastTime, float time,
									spEvent **firedEvents,
									int *eventsCount, float alpha, spMixBlend blend, spMixDirection direction),
					  void (*setBezier)(spTimeline *self, int bezier, int frame, float value, float time1, float value1,
										float cx1, float cy1,
										float cx2, float cy2, float time2, float value2)) {
	int i;
	self->frames = spFloatArray_create(frameCount * frameEntries);
	self->frames->size = frameCount * frameEntries;
	self->frameCount = frameCount;
	self->frameEntries = frameEntries;

	for (i = 0; i < propertyIdsCount; i++)
		self->propertyIds[i] = propertyIds[i];
	self->propertyIdsCount = propertyIdsCount;

	self->type = type;

	self->vtable.dispose = dispose;
	self->vtable.apply = apply;
	self->vtable.setBezier = setBezier;
}

void spTimeline_dispose(spTimeline *self) {
	self->vtable.dispose(self);
	spFloatArray_dispose(self->frames);
	FREE(self);
}

void spTimeline_apply(spTimeline *self, spSkeleton *skeleton, float lastTime, float time, spEvent **firedEvents,
					  int *eventsCount, float alpha, spMixBlend blend, spMixDirection direction) {
	self->vtable.apply(self, skeleton, lastTime, time, firedEvents, eventsCount, alpha, blend, direction);
}

void spTimeline_setBezier(spTimeline *self, int bezier, int frame, float value, float time1, float value1, float cx1,
						  float cy1, float cx2, float cy2, float time2, float value2) {
	if (self->vtable.setBezier)
		self->vtable.setBezier(self, bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
}

float spTimeline_getDuration(const spTimeline *self) {
	return self->frames->items[self->frames->size - self->frameEntries];
}

/**/

#define CURVE_LINEAR 0
#define CURVE_STEPPED 1
#define CURVE_BEZIER 2
#define BEZIER_SIZE 18

void _spCurveTimeline_init(spCurveTimeline *self,
						   int frameCount,
						   int frameEntries,
						   int bezierCount,
						   spPropertyId *propertyIds,
						   int propertyIdsCount,
						   spTimelineType type,
						   void (*dispose)(spTimeline *self),
						   void (*apply)(spTimeline *self, spSkeleton *skeleton, float lastTime, float time,
										 spEvent **firedEvents,
										 int *eventsCount, float alpha, spMixBlend blend, spMixDirection direction),
						   void (*setBezier)(spTimeline *self, int bezier, int frame, float value, float time1,
											 float value1, float cx1, float cy1,
											 float cx2, float cy2, float time2, float value2)) {
	_spTimeline_init(SUPER(self), frameCount, frameEntries, propertyIds, propertyIdsCount, type, dispose, apply,
					 setBezier);
	self->curves = spFloatArray_create(frameCount + bezierCount * BEZIER_SIZE);
	self->curves->size = frameCount + bezierCount * BEZIER_SIZE;
	self->curves->items[frameCount - 1] = CURVE_STEPPED;
}

void _spCurveTimeline_dispose(spTimeline *self) {
	spFloatArray_dispose(SUB_CAST(spCurveTimeline, self)->curves);
}

void _spCurveTimeline_setBezier(spTimeline *timeline, int bezier, int frame, float value, float time1, float value1,
								float cx1, float cy1, float cx2, float cy2, float time2, float value2) {
	spCurveTimeline *self = SUB_CAST(spCurveTimeline, timeline);
	float tmpx, tmpy, dddx, dddy, ddx, ddy, dx, dy, x, y;
	int i = self->super.frameCount + bezier * BEZIER_SIZE, n;
	float *curves = self->curves->items;
	if (value == 0) curves[frame] = CURVE_BEZIER + i;
	tmpx = (time1 - cx1 * 2 + cx2) * 0.03;
	tmpy = (value1 - cy1 * 2 + cy2) * 0.03;
	dddx = ((cx1 - cx2) * 3 - time1 + time2) * 0.006;
	dddy = ((cy1 - cy2) * 3 - value1 + value2) * 0.006;
	ddx = tmpx * 2 + dddx;
	ddy = tmpy * 2 + dddy;
	dx = (cx1 - time1) * 0.3 + tmpx + dddx * 0.16666667;
	dy = (cy1 - value1) * 0.3 + tmpy + dddy * 0.16666667;
	x = time1 + dx, y = value1 + dy;
	for (n = i + BEZIER_SIZE; i < n; i += 2) {
		curves[i] = x;
		curves[i + 1] = y;
		dx += ddx;
		dy += ddy;
		ddx += dddx;
		ddy += dddy;
		x += dx;
		y += dy;
	}
}

float _spCurveTimeline_getBezierValue(spCurveTimeline *self, float time, int frameIndex, int valueOffset, int i) {
	float *curves = self->curves->items;
	float *frames = SUPER(self)->frames->items;
	float x, y;
	int n;
	if (curves[i] > time) {
		x = frames[frameIndex];
		y = frames[frameIndex + valueOffset];
		return y + (time - x) / (curves[i] - x) * (curves[i + 1] - y);
	}
	n = i + BEZIER_SIZE;
	for (i += 2; i < n; i += 2) {
		if (curves[i] >= time) {
			x = curves[i - 2];
			y = curves[i - 1];
			return y + (time - x) / (curves[i] - x) * (curves[i + 1] - y);
		}
	}
	frameIndex += self->super.frameEntries;
	x = curves[n - 2];
	y = curves[n - 1];
	return y + (time - x) / (frames[frameIndex] - x) * (frames[frameIndex + valueOffset] - y);
}

void spCurveTimeline_setLinear(spCurveTimeline *self, int frame) {
	self->curves->items[frame] = CURVE_LINEAR;
}

void spCurveTimeline_setStepped(spCurveTimeline *self, int frame) {
	self->curves->items[frame] = CURVE_STEPPED;
}

#define CURVE1_ENTRIES 2
#define CURVE1_VALUE 1

void spCurveTimeline1_setFrame(spCurveTimeline1 *self, int frame, float time, float value) {
	float *frames = self->super.frames->items;
	frame <<= 1;
	frames[frame] = time;
	frames[frame + CURVE1_VALUE] = value;
}

float spCurveTimeline1_getCurveValue(spCurveTimeline1 *self, float time) {
	float *frames = self->super.frames->items;
	float *curves = self->curves->items;
	int i = self->super.frames->size - 2;
	int ii, curveType;
	for (ii = 2; ii <= i; ii += 2) {
		if (frames[ii] > time) {
			i = ii - 2;
			break;
		}
	}

	curveType = (int) curves[i >> 1];
	switch (curveType) {
		case CURVE_LINEAR: {
			float before = frames[i], value = frames[i + CURVE1_VALUE];
			return value + (time - before) / (frames[i + CURVE1_ENTRIES] - before) *
								   (frames[i + CURVE1_ENTRIES + CURVE1_VALUE] - value);
		}
		case CURVE_STEPPED:
			return frames[i + CURVE1_VALUE];
	}
	return _spCurveTimeline_getBezierValue(self, time, i, CURVE1_VALUE, curveType - CURVE_BEZIER);
}

float spCurveTimeline1_getRelativeValue(spCurveTimeline1 *self, float time, float alpha, spMixBlend blend, float current, float setup) {
	float *frames = self->super.frames->items;
	if (time < frames[0]) {
		switch (blend) {
			case SP_MIX_BLEND_SETUP:
				return setup;
			case SP_MIX_BLEND_FIRST:
				return current + (setup - current) * alpha;
			default:
				return current;
		}
	}
	float value = spCurveTimeline1_getCurveValue(self, time);
	switch (blend) {
		case SP_MIX_BLEND_SETUP:
			return setup + value * alpha;
		case SP_MIX_BLEND_FIRST:
		case SP_MIX_BLEND_REPLACE:
			value += setup - current;
			break;
		case SP_MIX_BLEND_ADD:
			break;
	}
	return current + value * alpha;
}

float spCurveTimeline1_getAbsoluteValue(spCurveTimeline1 *self, float time, float alpha, spMixBlend blend, float current, float setup) {
	float *frames = self->super.frames->items;
	if (time < frames[0]) {
		switch (blend) {
			case SP_MIX_BLEND_SETUP:
				return setup;
			case SP_MIX_BLEND_FIRST:
				return current + (setup - current) * alpha;
			default:
				return current;
		}
	}
	float value = spCurveTimeline1_getCurveValue(self, time);
	if (blend == SP_MIX_BLEND_SETUP) return setup + (value - setup) * alpha;
	return current + (value - current) * alpha;
}

float spCurveTimeline1_getAbsoluteValue2(spCurveTimeline1 *self, float time, float alpha, spMixBlend blend, float current, float setup, float value) {
	float *frames = self->super.frames->items;
	if (time < frames[0]) {
		switch (blend) {
			case SP_MIX_BLEND_SETUP:
				return setup;
			case SP_MIX_BLEND_FIRST:
				return current + (setup - current) * alpha;
			default:
				return current;
		}
	}
	if (blend == SP_MIX_BLEND_SETUP) return setup + (value - setup) * alpha;
	return current + (value - current) * alpha;
}

float spCurveTimeline1_getScaleValue(spCurveTimeline1 *self, float time, float alpha, spMixBlend blend, spMixDirection direction, float current, float setup) {
	float *frames = self->super.frames->items;
	if (time < frames[0]) {
		switch (blend) {
			case SP_MIX_BLEND_SETUP:
				return setup;
			case SP_MIX_BLEND_FIRST:
				return current + (setup - current) * alpha;
			default:
				return current;
		}
	}
	float value = spCurveTimeline1_getCurveValue(self, time) * setup;
	if (alpha == 1) {
		if (blend == SP_MIX_BLEND_ADD) return current + value - setup;
		return value;
	}
	// Mixing out uses sign of setup or current pose, else use sign of key.
	if (direction == SP_MIX_DIRECTION_OUT) {
		switch (blend) {
			case SP_MIX_BLEND_SETUP:
				return setup + (ABS(value) * SIGNUM(setup) - setup) * alpha;
			case SP_MIX_BLEND_FIRST:
			case SP_MIX_BLEND_REPLACE:
				return current + (ABS(value) * SIGNUM(current) - current) * alpha;
			default:
				break;
		}
	} else {
		float s;
		switch (blend) {
			case SP_MIX_BLEND_SETUP:
				s = ABS(setup) * SIGNUM(value);
				return s + (value - s) * alpha;
			case SP_MIX_BLEND_FIRST:
			case SP_MIX_BLEND_REPLACE:
				s = ABS(current) * SIGNUM(value);
				return s + (value - s) * alpha;
			default:
				break;
		}
	}
	return current + (value - setup) * alpha;
}

#define CURVE2_ENTRIES 3
#define CURVE2_VALUE1 1
#define CURVE2_VALUE2 2

SP_API void spCurveTimeline2_setFrame(spCurveTimeline1 *self, int frame, float time, float value1, float value2) {
	float *frames = self->super.frames->items;
	frame *= CURVE2_ENTRIES;
	frames[frame] = time;
	frames[frame + CURVE2_VALUE1] = value1;
	frames[frame + CURVE2_VALUE2] = value2;
}

/**/

void _spRotateTimeline_apply(spTimeline *timeline, spSkeleton *skeleton, float lastTime, float time, spEvent **firedEvents,
							 int *eventsCount, float alpha, spMixBlend blend, spMixDirection direction) {
	spRotateTimeline *self = SUB_CAST(spRotateTimeline, timeline);
	spBone *bone = skeleton->bones[self->boneIndex];
	if (bone->active) bone->rotation = spCurveTimeline1_getRelativeValue(SUPER(self), time, alpha, blend, bone->rotation, bone->data->rotation);

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(direction);
}

spRotateTimeline *spRotateTimeline_create(int frameCount, int bezierCount, int boneIndex) {
	spRotateTimeline *timeline = NEW(spRotateTimeline);
	spPropertyId ids[1];
	ids[0] = ((spPropertyId) SP_PROPERTY_ROTATE << 32) | boneIndex;
	_spCurveTimeline_init(SUPER(timeline), frameCount, CURVE1_ENTRIES, bezierCount, ids, 1, SP_TIMELINE_ROTATE,
						  _spCurveTimeline_dispose, _spRotateTimeline_apply, _spCurveTimeline_setBezier);
	timeline->boneIndex = boneIndex;
	return timeline;
}

void spRotateTimeline_setFrame(spRotateTimeline *self, int frame, float time, float degrees) {
	spCurveTimeline1_setFrame(SUPER(self), frame, time, degrees);
}

/**/

void _spTranslateTimeline_apply(spTimeline *timeline, spSkeleton *skeleton, float lastTime, float time,
								spEvent **firedEvents, int *eventsCount, float alpha, spMixBlend blend,
								spMixDirection direction) {
	spBone *bone;
	float x, y, t;
	int i, curveType;

	spTranslateTimeline *self = SUB_CAST(spTranslateTimeline, timeline);
	float *frames = self->super.super.frames->items;
	float *curves = self->super.curves->items;

	bone = skeleton->bones[self->boneIndex];
	if (!bone->active) return;

	if (time < frames[0]) {
		switch (blend) {
			case SP_MIX_BLEND_SETUP:
				bone->x = bone->data->x;
				bone->y = bone->data->y;
				return;
			case SP_MIX_BLEND_FIRST:
				bone->x += (bone->data->x - bone->x) * alpha;
				bone->y += (bone->data->y - bone->y) * alpha;
			default: {
			}
		}
		return;
	}

	i = search2(self->super.super.frames, time, CURVE2_ENTRIES);
	curveType = (int) curves[i / CURVE2_ENTRIES];
	switch (curveType) {
		case CURVE_LINEAR: {
			float before = frames[i];
			x = frames[i + CURVE2_VALUE1];
			y = frames[i + CURVE2_VALUE2];
			t = (time - before) / (frames[i + CURVE2_ENTRIES] - before);
			x += (frames[i + CURVE2_ENTRIES + CURVE2_VALUE1] - x) * t;
			y += (frames[i + CURVE2_ENTRIES + CURVE2_VALUE2] - y) * t;
			break;
		}
		case CURVE_STEPPED: {
			x = frames[i + CURVE2_VALUE1];
			y = frames[i + CURVE2_VALUE2];
			break;
		}
		default: {
			x = _spCurveTimeline_getBezierValue(SUPER(self), time, i, CURVE2_VALUE1, curveType - CURVE_BEZIER);
			y = _spCurveTimeline_getBezierValue(SUPER(self), time, i, CURVE2_VALUE2,
												curveType + BEZIER_SIZE - CURVE_BEZIER);
		}
	}

	switch (blend) {
		case SP_MIX_BLEND_SETUP:
			bone->x = bone->data->x + x * alpha;
			bone->y = bone->data->y + y * alpha;
			break;
		case SP_MIX_BLEND_FIRST:
		case SP_MIX_BLEND_REPLACE:
			bone->x += (bone->data->x + x - bone->x) * alpha;
			bone->y += (bone->data->y + y - bone->y) * alpha;
			break;
		case SP_MIX_BLEND_ADD:
			bone->x += x * alpha;
			bone->y += y * alpha;
	}

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(direction);
}

spTranslateTimeline *spTranslateTimeline_create(int frameCount, int bezierCount, int boneIndex) {
	spTranslateTimeline *timeline = NEW(spTranslateTimeline);
	spPropertyId ids[2];
	ids[0] = ((spPropertyId) SP_PROPERTY_X << 32) | boneIndex;
	ids[1] = ((spPropertyId) SP_PROPERTY_Y << 32) | boneIndex;
	_spCurveTimeline_init(SUPER(timeline), frameCount, CURVE2_ENTRIES, bezierCount, ids, 2, SP_TIMELINE_TRANSLATE,
						  _spCurveTimeline_dispose, _spTranslateTimeline_apply, _spCurveTimeline_setBezier);
	timeline->boneIndex = boneIndex;
	return timeline;
}

void spTranslateTimeline_setFrame(spTranslateTimeline *self, int frame, float time, float x, float y) {
	spCurveTimeline2_setFrame(SUPER(self), frame, time, x, y);
}

/**/

void _spTranslateXTimeline_apply(spTimeline *timeline, spSkeleton *skeleton, float lastTime, float time,
								 spEvent **firedEvents, int *eventsCount, float alpha, spMixBlend blend,
								 spMixDirection direction) {
	spBone *bone;
	float x;

	spTranslateXTimeline *self = SUB_CAST(spTranslateXTimeline, timeline);
	float *frames = self->super.super.frames->items;

	bone = skeleton->bones[self->boneIndex];
	if (!bone->active) return;

	if (time < frames[0]) {
		switch (blend) {
			case SP_MIX_BLEND_SETUP:
				bone->x = bone->data->x;
				return;
			case SP_MIX_BLEND_FIRST:
				bone->x += (bone->data->x - bone->x) * alpha;
			default: {
			}
		}
		return;
	}

	x = spCurveTimeline1_getCurveValue(SUPER(self), time);
	switch (blend) {
		case SP_MIX_BLEND_SETUP:
			bone->x = bone->data->x + x * alpha;
			break;
		case SP_MIX_BLEND_FIRST:
		case SP_MIX_BLEND_REPLACE:
			bone->x += (bone->data->x + x - bone->x) * alpha;
			break;
		case SP_MIX_BLEND_ADD:
			bone->x += x * alpha;
	}

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(direction);
}

spTranslateXTimeline *spTranslateXTimeline_create(int frameCount, int bezierCount, int boneIndex) {
	spTranslateXTimeline *timeline = NEW(spTranslateXTimeline);
	spPropertyId ids[1];
	ids[0] = ((spPropertyId) SP_PROPERTY_X << 32) | boneIndex;
	_spCurveTimeline_init(SUPER(timeline), frameCount, CURVE1_ENTRIES, bezierCount, ids, 1, SP_TIMELINE_TRANSLATEX,
						  _spCurveTimeline_dispose, _spTranslateXTimeline_apply, _spCurveTimeline_setBezier);
	timeline->boneIndex = boneIndex;
	return timeline;
}

void spTranslateXTimeline_setFrame(spTranslateXTimeline *self, int frame, float time, float x) {
	spCurveTimeline1_setFrame(SUPER(self), frame, time, x);
}

/**/

void _spTranslateYTimeline_apply(spTimeline *timeline, spSkeleton *skeleton, float lastTime, float time,
								 spEvent **firedEvents, int *eventsCount, float alpha, spMixBlend blend,
								 spMixDirection direction) {
	spBone *bone;
	float y;

	spTranslateYTimeline *self = SUB_CAST(spTranslateYTimeline, timeline);
	float *frames = self->super.super.frames->items;

	bone = skeleton->bones[self->boneIndex];
	if (!bone->active) return;

	if (time < frames[0]) {
		switch (blend) {
			case SP_MIX_BLEND_SETUP:
				bone->y = bone->data->y;
				return;
			case SP_MIX_BLEND_FIRST:
				bone->y += (bone->data->y - bone->y) * alpha;
			default: {
			}
		}
		return;
	}

	y = spCurveTimeline1_getCurveValue(SUPER(self), time);
	switch (blend) {
		case SP_MIX_BLEND_SETUP:
			bone->y = bone->data->y + y * alpha;
			break;
		case SP_MIX_BLEND_FIRST:
		case SP_MIX_BLEND_REPLACE:
			bone->y += (bone->data->y + y - bone->y) * alpha;
			break;
		case SP_MIX_BLEND_ADD:
			bone->y += y * alpha;
	}

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(direction);
}

spTranslateYTimeline *spTranslateYTimeline_create(int frameCount, int bezierCount, int boneIndex) {
	spTranslateYTimeline *timeline = NEW(spTranslateYTimeline);
	spPropertyId ids[1];
	ids[0] = ((spPropertyId) SP_PROPERTY_Y << 32) | boneIndex;
	_spCurveTimeline_init(SUPER(timeline), frameCount, CURVE1_ENTRIES, bezierCount, ids, 1, SP_TIMELINE_TRANSLATEY,
						  _spCurveTimeline_dispose, _spTranslateYTimeline_apply, _spCurveTimeline_setBezier);
	timeline->boneIndex = boneIndex;
	return timeline;
}

void spTranslateYTimeline_setFrame(spTranslateYTimeline *self, int frame, float time, float y) {
	spCurveTimeline1_setFrame(SUPER(self), frame, time, y);
}

/**/

void _spScaleTimeline_apply(spTimeline *timeline, spSkeleton *skeleton, float lastTime, float time, spEvent **firedEvents,
							int *eventsCount, float alpha, spMixBlend blend, spMixDirection direction) {
	spBone *bone;
	int i, curveType;
	float x, y, t;

	spScaleTimeline *self = SUB_CAST(spScaleTimeline, timeline);
	float *frames = self->super.super.frames->items;
	float *curves = self->super.curves->items;

	bone = skeleton->bones[self->boneIndex];
	if (!bone->active) return;
	if (time < frames[0]) {
		switch (blend) {
			case SP_MIX_BLEND_SETUP:
				bone->scaleX = bone->data->scaleX;
				bone->scaleY = bone->data->scaleY;
				return;
			case SP_MIX_BLEND_FIRST:
				bone->scaleX += (bone->data->scaleX - bone->scaleX) * alpha;
				bone->scaleY += (bone->data->scaleY - bone->scaleY) * alpha;
			default: {
			}
		}
		return;
	}

	i = search2(self->super.super.frames, time, CURVE2_ENTRIES);
	curveType = (int) curves[i / CURVE2_ENTRIES];
	switch (curveType) {
		case CURVE_LINEAR: {
			float before = frames[i];
			x = frames[i + CURVE2_VALUE1];
			y = frames[i + CURVE2_VALUE2];
			t = (time - before) / (frames[i + CURVE2_ENTRIES] - before);
			x += (frames[i + CURVE2_ENTRIES + CURVE2_VALUE1] - x) * t;
			y += (frames[i + CURVE2_ENTRIES + CURVE2_VALUE2] - y) * t;
			break;
		}
		case CURVE_STEPPED: {
			x = frames[i + CURVE2_VALUE1];
			y = frames[i + CURVE2_VALUE2];
			break;
		}
		default: {
			x = _spCurveTimeline_getBezierValue(SUPER(self), time, i, CURVE2_VALUE1, curveType - CURVE_BEZIER);
			y = _spCurveTimeline_getBezierValue(SUPER(self), time, i, CURVE2_VALUE2,
												curveType + BEZIER_SIZE - CURVE_BEZIER);
		}
	}
	x *= bone->data->scaleX;
	y *= bone->data->scaleY;

	if (alpha == 1) {
		if (blend == SP_MIX_BLEND_ADD) {
			bone->scaleX += x - bone->data->scaleX;
			bone->scaleY += y - bone->data->scaleY;
		} else {
			bone->scaleX = x;
			bone->scaleY = y;
		}
	} else {
		float bx, by;
		if (direction == SP_MIX_DIRECTION_OUT) {
			switch (blend) {
				case SP_MIX_BLEND_SETUP:
					bx = bone->data->scaleX;
					by = bone->data->scaleY;
					bone->scaleX = bx + (ABS(x) * SIGNUM(bx) - bx) * alpha;
					bone->scaleY = by + (ABS(y) * SIGNUM(by) - by) * alpha;
					break;
				case SP_MIX_BLEND_FIRST:
				case SP_MIX_BLEND_REPLACE:
					bx = bone->scaleX;
					by = bone->scaleY;
					bone->scaleX = bx + (ABS(x) * SIGNUM(bx) - bx) * alpha;
					bone->scaleY = by + (ABS(y) * SIGNUM(by) - by) * alpha;
					break;
				case SP_MIX_BLEND_ADD:
					bone->scaleX += (x - bone->data->scaleX) * alpha;
					bone->scaleY += (y - bone->data->scaleY) * alpha;
			}
		} else {
			switch (blend) {
				case SP_MIX_BLEND_SETUP:
					bx = ABS(bone->data->scaleX) * SIGNUM(x);
					by = ABS(bone->data->scaleY) * SIGNUM(y);
					bone->scaleX = bx + (x - bx) * alpha;
					bone->scaleY = by + (y - by) * alpha;
					break;
				case SP_MIX_BLEND_FIRST:
				case SP_MIX_BLEND_REPLACE:
					bx = ABS(bone->scaleX) * SIGNUM(x);
					by = ABS(bone->scaleY) * SIGNUM(y);
					bone->scaleX = bx + (x - bx) * alpha;
					bone->scaleY = by + (y - by) * alpha;
					break;
				case SP_MIX_BLEND_ADD:
					bone->scaleX += (x - bone->data->scaleX) * alpha;
					bone->scaleY += (y - bone->data->scaleY) * alpha;
			}
		}
	}

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
}

spScaleTimeline *spScaleTimeline_create(int frameCount, int bezierCount, int boneIndex) {
	spScaleTimeline *timeline = NEW(spScaleTimeline);
	spPropertyId ids[2];
	ids[0] = ((spPropertyId) SP_PROPERTY_SCALEX << 32) | boneIndex;
	ids[1] = ((spPropertyId) SP_PROPERTY_SCALEY << 32) | boneIndex;
	_spCurveTimeline_init(SUPER(timeline), frameCount, CURVE2_ENTRIES, bezierCount, ids, 2, SP_TIMELINE_SCALE,
						  _spCurveTimeline_dispose, _spScaleTimeline_apply, _spCurveTimeline_setBezier);
	timeline->boneIndex = boneIndex;
	return timeline;
}

void spScaleTimeline_setFrame(spScaleTimeline *self, int frame, float time, float x, float y) {
	spCurveTimeline2_setFrame(SUPER(self), frame, time, x, y);
}

/**/

void _spScaleXTimeline_apply(spTimeline *timeline, spSkeleton *skeleton, float lastTime, float time,
							 spEvent **firedEvents, int *eventsCount, float alpha, spMixBlend blend,
							 spMixDirection direction) {
	spScaleXTimeline *self = SUB_CAST(spScaleXTimeline, timeline);
	spBone *bone = skeleton->bones[self->boneIndex];

	if (bone->active) bone->scaleX = spCurveTimeline1_getScaleValue(SUPER(self), time, alpha, blend, direction, bone->scaleX, bone->data->scaleX);

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
}

spScaleXTimeline *spScaleXTimeline_create(int frameCount, int bezierCount, int boneIndex) {
	spScaleXTimeline *timeline = NEW(spScaleXTimeline);
	spPropertyId ids[1];
	ids[0] = ((spPropertyId) SP_PROPERTY_SCALEX << 32) | boneIndex;
	_spCurveTimeline_init(SUPER(timeline), frameCount, CURVE1_ENTRIES, bezierCount, ids, 1, SP_TIMELINE_SCALEX,
						  _spCurveTimeline_dispose, _spScaleXTimeline_apply, _spCurveTimeline_setBezier);
	timeline->boneIndex = boneIndex;
	return timeline;
}

void spScaleXTimeline_setFrame(spScaleXTimeline *self, int frame, float time, float y) {
	spCurveTimeline1_setFrame(SUPER(self), frame, time, y);
}

/**/

void _spScaleYTimeline_apply(spTimeline *timeline, spSkeleton *skeleton, float lastTime, float time,
							 spEvent **firedEvents, int *eventsCount, float alpha, spMixBlend blend,
							 spMixDirection direction) {
	spScaleYTimeline *self = SUB_CAST(spScaleYTimeline, timeline);
	spBone *bone = skeleton->bones[self->boneIndex];

	if (bone->active) bone->scaleY = spCurveTimeline1_getScaleValue(SUPER(self), time, alpha, blend, direction, bone->scaleX, bone->data->scaleY);

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
}

spScaleYTimeline *spScaleYTimeline_create(int frameCount, int bezierCount, int boneIndex) {
	spScaleYTimeline *timeline = NEW(spScaleYTimeline);
	spPropertyId ids[1];
	ids[0] = ((spPropertyId) SP_PROPERTY_SCALEY << 32) | boneIndex;
	_spCurveTimeline_init(SUPER(timeline), frameCount, CURVE1_ENTRIES, bezierCount, ids, 1, SP_TIMELINE_SCALEY,
						  _spCurveTimeline_dispose, _spScaleYTimeline_apply, _spCurveTimeline_setBezier);
	timeline->boneIndex = boneIndex;
	return timeline;
}

void spScaleYTimeline_setFrame(spScaleYTimeline *self, int frame, float time, float y) {
	spCurveTimeline1_setFrame(SUPER(self), frame, time, y);
}

/**/

void _spShearTimeline_apply(spTimeline *timeline, spSkeleton *skeleton, float lastTime, float time, spEvent **firedEvents,
							int *eventsCount, float alpha, spMixBlend blend, spMixDirection direction) {
	spBone *bone;
	float x, y, t;
	int i, curveType;

	spShearTimeline *self = SUB_CAST(spShearTimeline, timeline);
	float *frames = SUPER(self)->super.frames->items;
	float *curves = SUPER(self)->curves->items;

	bone = skeleton->bones[self->boneIndex];
	if (!bone->active) return;
	if (time < frames[0]) {
		switch (blend) {
			case SP_MIX_BLEND_SETUP:
				bone->shearX = bone->data->shearX;
				bone->shearY = bone->data->shearY;
				return;
			case SP_MIX_BLEND_FIRST:
				bone->shearX += (bone->data->shearX - bone->shearX) * alpha;
				bone->shearY += (bone->data->shearY - bone->shearY) * alpha;
			default: {
			}
		}
		return;
	}

	i = search2(self->super.super.frames, time, CURVE2_ENTRIES);
	curveType = (int) curves[i / CURVE2_ENTRIES];
	switch (curveType) {
		case CURVE_LINEAR: {
			float before = frames[i];
			x = frames[i + CURVE2_VALUE1];
			y = frames[i + CURVE2_VALUE2];
			t = (time - before) / (frames[i + CURVE2_ENTRIES] - before);
			x += (frames[i + CURVE2_ENTRIES + CURVE2_VALUE1] - x) * t;
			y += (frames[i + CURVE2_ENTRIES + CURVE2_VALUE2] - y) * t;
			break;
		}
		case CURVE_STEPPED: {
			x = frames[i + CURVE2_VALUE1];
			y = frames[i + CURVE2_VALUE2];
			break;
		}
		default: {
			x = _spCurveTimeline_getBezierValue(SUPER(self), time, i, CURVE2_VALUE1, curveType - CURVE_BEZIER);
			y = _spCurveTimeline_getBezierValue(SUPER(self), time, i, CURVE2_VALUE2,
												curveType + BEZIER_SIZE - CURVE_BEZIER);
		}
	}

	switch (blend) {
		case SP_MIX_BLEND_SETUP:
			bone->shearX = bone->data->shearX + x * alpha;
			bone->shearY = bone->data->shearY + y * alpha;
			break;
		case SP_MIX_BLEND_FIRST:
		case SP_MIX_BLEND_REPLACE:
			bone->shearX += (bone->data->shearX + x - bone->shearX) * alpha;
			bone->shearY += (bone->data->shearY + y - bone->shearY) * alpha;
			break;
		case SP_MIX_BLEND_ADD:
			bone->shearX += x * alpha;
			bone->shearY += y * alpha;
	}

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(direction);
}

spShearTimeline *spShearTimeline_create(int frameCount, int bezierCount, int boneIndex) {
	spShearTimeline *timeline = NEW(spShearTimeline);
	spPropertyId ids[2];
	ids[0] = ((spPropertyId) SP_PROPERTY_SHEARX << 32) | boneIndex;
	ids[1] = ((spPropertyId) SP_PROPERTY_SHEARY << 32) | boneIndex;
	_spCurveTimeline_init(SUPER(timeline), frameCount, CURVE2_ENTRIES, bezierCount, ids, 2, SP_TIMELINE_SHEAR,
						  _spCurveTimeline_dispose, _spShearTimeline_apply, _spCurveTimeline_setBezier);
	timeline->boneIndex = boneIndex;
	return timeline;
}

void spShearTimeline_setFrame(spShearTimeline *self, int frame, float time, float x, float y) {
	spCurveTimeline2_setFrame(SUPER(self), frame, time, x, y);
}

/**/

void _spShearXTimeline_apply(spTimeline *timeline, spSkeleton *skeleton, float lastTime, float time,
							 spEvent **firedEvents, int *eventsCount, float alpha, spMixBlend blend,
							 spMixDirection direction) {
	spShearXTimeline *self = SUB_CAST(spShearXTimeline, timeline);
	spBone *bone = skeleton->bones[self->boneIndex];

	if (bone->active) bone->shearX = spCurveTimeline1_getRelativeValue(SUPER(self), time, alpha, blend, bone->shearX, bone->data->shearX);

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(direction);
}

spShearXTimeline *spShearXTimeline_create(int frameCount, int bezierCount, int boneIndex) {
	spShearXTimeline *timeline = NEW(spShearXTimeline);
	spPropertyId ids[1];
	ids[0] = ((spPropertyId) SP_PROPERTY_SHEARX << 32) | boneIndex;
	_spCurveTimeline_init(SUPER(timeline), frameCount, CURVE1_ENTRIES, bezierCount, ids, 1, SP_TIMELINE_SHEARX,
						  _spCurveTimeline_dispose, _spShearXTimeline_apply, _spCurveTimeline_setBezier);
	timeline->boneIndex = boneIndex;
	return timeline;
}

void spShearXTimeline_setFrame(spShearXTimeline *self, int frame, float time, float x) {
	spCurveTimeline1_setFrame(SUPER(self), frame, time, x);
}

/**/

void _spShearYTimeline_apply(spTimeline *timeline, spSkeleton *skeleton, float lastTime, float time,
							 spEvent **firedEvents, int *eventsCount, float alpha, spMixBlend blend,
							 spMixDirection direction) {
	spShearYTimeline *self = SUB_CAST(spShearYTimeline, timeline);
	spBone *bone = skeleton->bones[self->boneIndex];

	if (bone->active) bone->shearY = spCurveTimeline1_getRelativeValue(SUPER(self), time, alpha, blend, bone->shearY, bone->data->shearY);

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(direction);
}

spShearYTimeline *spShearYTimeline_create(int frameCount, int bezierCount, int boneIndex) {
	spShearYTimeline *timeline = NEW(spShearYTimeline);
	spPropertyId ids[1];
	ids[0] = ((spPropertyId) SP_PROPERTY_SHEARY << 32) | boneIndex;
	_spCurveTimeline_init(SUPER(timeline), frameCount, CURVE1_ENTRIES, bezierCount, ids, 1, SP_TIMELINE_SHEARY,
						  _spCurveTimeline_dispose, _spShearYTimeline_apply, _spCurveTimeline_setBezier);
	timeline->boneIndex = boneIndex;
	return timeline;
}

void spShearYTimeline_setFrame(spShearYTimeline *self, int frame, float time, float y) {
	spCurveTimeline1_setFrame(SUPER(self), frame, time, y);
}

/**/

static const int RGBA_ENTRIES = 5, COLOR_R = 1, COLOR_G = 2, COLOR_B = 3, COLOR_A = 4;

void _spRGBATimeline_apply(spTimeline *timeline, spSkeleton *skeleton, float lastTime, float time, spEvent **firedEvents,
						   int *eventsCount, float alpha, spMixBlend blend, spMixDirection direction) {
	spSlot *slot;
	int i, curveType;
	float r, g, b, a, t;
	spColor *color;
	spColor *setup;
	spRGBATimeline *self = (spRGBATimeline *) timeline;
	float *frames = self->super.super.frames->items;
	float *curves = self->super.curves->items;

	slot = skeleton->slots[self->slotIndex];
	if (!slot->bone->active) return;

	if (time < frames[0]) {
		color = &slot->color;
		setup = &slot->data->color;
		switch (blend) {
			case SP_MIX_BLEND_SETUP:
				spColor_setFromColor(color, setup);
				return;
			case SP_MIX_BLEND_FIRST:
				spColor_addFloats(color, (setup->r - color->r) * alpha, (setup->g - color->g) * alpha,
								  (setup->b - color->b) * alpha,
								  (setup->a - color->a) * alpha);
			default: {
			}
		}
		return;
	}

	i = search2(self->super.super.frames, time, RGBA_ENTRIES);
	curveType = (int) curves[i / RGBA_ENTRIES];
	switch (curveType) {
		case CURVE_LINEAR: {
			float before = frames[i];
			r = frames[i + COLOR_R];
			g = frames[i + COLOR_G];
			b = frames[i + COLOR_B];
			a = frames[i + COLOR_A];
			t = (time - before) / (frames[i + RGBA_ENTRIES] - before);
			r += (frames[i + RGBA_ENTRIES + COLOR_R] - r) * t;
			g += (frames[i + RGBA_ENTRIES + COLOR_G] - g) * t;
			b += (frames[i + RGBA_ENTRIES + COLOR_B] - b) * t;
			a += (frames[i + RGBA_ENTRIES + COLOR_A] - a) * t;
			break;
		}
		case CURVE_STEPPED: {
			r = frames[i + COLOR_R];
			g = frames[i + COLOR_G];
			b = frames[i + COLOR_B];
			a = frames[i + COLOR_A];
			break;
		}
		default: {
			r = _spCurveTimeline_getBezierValue(SUPER(self), time, i, COLOR_R, curveType - CURVE_BEZIER);
			g = _spCurveTimeline_getBezierValue(SUPER(self), time, i, COLOR_G,
												curveType + BEZIER_SIZE - CURVE_BEZIER);
			b = _spCurveTimeline_getBezierValue(SUPER(self), time, i, COLOR_B,
												curveType + BEZIER_SIZE * 2 - CURVE_BEZIER);
			a = _spCurveTimeline_getBezierValue(SUPER(self), time, i, COLOR_A,
												curveType + BEZIER_SIZE * 3 - CURVE_BEZIER);
		}
	}
	color = &slot->color;
	if (alpha == 1)
		spColor_setFromFloats(color, r, g, b, a);
	else {
		if (blend == SP_MIX_BLEND_SETUP) spColor_setFromColor(color, &slot->data->color);
		spColor_addFloats(color, (r - color->r) * alpha, (g - color->g) * alpha, (b - color->b) * alpha,
						  (a - color->a) * alpha);
	}

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(direction);
}

spRGBATimeline *spRGBATimeline_create(int framesCount, int bezierCount, int slotIndex) {
	spRGBATimeline *timeline = NEW(spRGBATimeline);
	spPropertyId ids[2];
	ids[0] = ((spPropertyId) SP_PROPERTY_RGB << 32) | slotIndex;
	ids[1] = ((spPropertyId) SP_PROPERTY_ALPHA << 32) | slotIndex;
	_spCurveTimeline_init(SUPER(timeline), framesCount, RGBA_ENTRIES, bezierCount, ids, 2, SP_TIMELINE_RGBA,
						  _spCurveTimeline_dispose, _spRGBATimeline_apply, _spCurveTimeline_setBezier);
	timeline->slotIndex = slotIndex;
	return timeline;
}

void spRGBATimeline_setFrame(spRGBATimeline *self, int frame, float time, float r, float g, float b, float a) {
	float *frames = self->super.super.frames->items;
	frame *= RGBA_ENTRIES;
	frames[frame] = time;
	frames[frame + COLOR_R] = r;
	frames[frame + COLOR_G] = g;
	frames[frame + COLOR_B] = b;
	frames[frame + COLOR_A] = a;
}

/**/

#define RGB_ENTRIES 4

void _spRGBTimeline_apply(spTimeline *timeline, spSkeleton *skeleton, float lastTime, float time, spEvent **firedEvents,
						  int *eventsCount, float alpha, spMixBlend blend, spMixDirection direction) {
	spSlot *slot;
	int i, curveType;
	float r, g, b, t;
	spColor *color;
	spColor *setup;
	spRGBTimeline *self = (spRGBTimeline *) timeline;
	float *frames = self->super.super.frames->items;
	float *curves = self->super.curves->items;

	slot = skeleton->slots[self->slotIndex];
	if (!slot->bone->active) return;

	if (time < frames[0]) {
		color = &slot->color;
		setup = &slot->data->color;
		switch (blend) {
			case SP_MIX_BLEND_SETUP:
				spColor_setFromColor(color, setup);
				return;
			case SP_MIX_BLEND_FIRST:
				spColor_addFloats(color, (setup->r - color->r) * alpha, (setup->g - color->g) * alpha,
								  (setup->b - color->b) * alpha,
								  (setup->a - color->a) * alpha);
			default: {
			}
		}
		return;
	}

	i = search2(self->super.super.frames, time, RGB_ENTRIES);
	curveType = (int) curves[i / RGB_ENTRIES];
	switch (curveType) {
		case CURVE_LINEAR: {
			float before = frames[i];
			r = frames[i + COLOR_R];
			g = frames[i + COLOR_G];
			b = frames[i + COLOR_B];
			t = (time - before) / (frames[i + RGB_ENTRIES] - before);
			r += (frames[i + RGB_ENTRIES + COLOR_R] - r) * t;
			g += (frames[i + RGB_ENTRIES + COLOR_G] - g) * t;
			b += (frames[i + RGB_ENTRIES + COLOR_B] - b) * t;
			break;
		}
		case CURVE_STEPPED: {
			r = frames[i + COLOR_R];
			g = frames[i + COLOR_G];
			b = frames[i + COLOR_B];
			break;
		}
		default: {
			r = _spCurveTimeline_getBezierValue(SUPER(self), time, i, COLOR_R, curveType - CURVE_BEZIER);
			g = _spCurveTimeline_getBezierValue(SUPER(self), time, i, COLOR_G,
												curveType + BEZIER_SIZE - CURVE_BEZIER);
			b = _spCurveTimeline_getBezierValue(SUPER(self), time, i, COLOR_B,
												curveType + BEZIER_SIZE * 2 - CURVE_BEZIER);
		}
	}
	color = &slot->color;
	if (alpha == 1) {
		color->r = r;
		color->g = g;
		color->b = b;
	} else {
		if (blend == SP_MIX_BLEND_SETUP) {
			color->r = slot->data->color.r;
			color->g = slot->data->color.g;
			color->b = slot->data->color.b;
		}
		color->r += (r - color->r) * alpha;
		color->g += (g - color->g) * alpha;
		color->b += (b - color->b) * alpha;
	}

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(direction);
}

spRGBTimeline *spRGBTimeline_create(int framesCount, int bezierCount, int slotIndex) {
	spRGBTimeline *timeline = NEW(spRGBTimeline);
	spPropertyId ids[1];
	ids[0] = ((spPropertyId) SP_PROPERTY_RGB << 32) | slotIndex;
	_spCurveTimeline_init(SUPER(timeline), framesCount, RGB_ENTRIES, bezierCount, ids, 1, SP_TIMELINE_RGB,
						  _spCurveTimeline_dispose, _spRGBTimeline_apply, _spCurveTimeline_setBezier);
	timeline->slotIndex = slotIndex;
	return timeline;
}

void spRGBTimeline_setFrame(spRGBTimeline *self, int frame, float time, float r, float g, float b) {
	float *frames = self->super.super.frames->items;
	frame *= RGB_ENTRIES;
	frames[frame] = time;
	frames[frame + COLOR_R] = r;
	frames[frame + COLOR_G] = g;
	frames[frame + COLOR_B] = b;
}

/**/

void _spAlphaTimeline_apply(spTimeline *timeline, spSkeleton *skeleton, float lastTime, float time,
							spEvent **firedEvents, int *eventsCount, float alpha, spMixBlend blend,
							spMixDirection direction) {
	spSlot *slot;
	float a;
	spColor *color;
	spColor *setup;
	spAlphaTimeline *self = (spAlphaTimeline *) timeline;
	float *frames = self->super.super.frames->items;

	slot = skeleton->slots[self->slotIndex];
	if (!slot->bone->active) return;

	if (time < frames[0]) { /* Time is before first frame-> */
		color = &slot->color;
		setup = &slot->data->color;
		switch (blend) {
			case SP_MIX_BLEND_SETUP:
				color->a = setup->a;
				return;
			case SP_MIX_BLEND_FIRST:
				color->a += (setup->a - color->a) * alpha;
			default: {
			}
		}
		return;
	}

	a = spCurveTimeline1_getCurveValue(SUPER(self), time);
	if (alpha == 1)
		slot->color.a = a;
	else {
		if (blend == SP_MIX_BLEND_SETUP) slot->color.a = slot->data->color.a;
		slot->color.a += (a - slot->color.a) * alpha;
	}

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(direction);
}

spAlphaTimeline *spAlphaTimeline_create(int frameCount, int bezierCount, int slotIndex) {
	spAlphaTimeline *timeline = NEW(spAlphaTimeline);
	spPropertyId ids[1];
	ids[0] = ((spPropertyId) SP_PROPERTY_ALPHA << 32) | slotIndex;
	_spCurveTimeline_init(SUPER(timeline), frameCount, CURVE1_ENTRIES, bezierCount, ids, 1, SP_TIMELINE_ALPHA,
						  _spCurveTimeline_dispose, _spAlphaTimeline_apply, _spCurveTimeline_setBezier);
	timeline->slotIndex = slotIndex;
	return timeline;
}

void spAlphaTimeline_setFrame(spAlphaTimeline *self, int frame, float time, float alpha) {
	spCurveTimeline1_setFrame(SUPER(self), frame, time, alpha);
}

/**/

static const int RGBA2_ENTRIES = 8, COLOR_R2 = 5, COLOR_G2 = 6, COLOR_B2 = 7;

void _spRGBA2Timeline_apply(spTimeline *timeline, spSkeleton *skeleton, float lastTime, float time, spEvent **firedEvents,
							int *eventsCount, float alpha, spMixBlend blend, spMixDirection direction) {
	spSlot *slot;
	int i, curveType;
	float r, g, b, a, r2, g2, b2, t;
	spColor *light, *setupLight;
	spColor *dark, *setupDark;
	spRGBA2Timeline *self = (spRGBA2Timeline *) timeline;
	float *frames = self->super.super.frames->items;
	float *curves = self->super.curves->items;

	slot = skeleton->slots[self->slotIndex];
	if (!slot->bone->active) return;

	if (time < frames[0]) {
		light = &slot->color;
		dark = slot->darkColor;
		setupLight = &slot->data->color;
		setupDark = slot->data->darkColor;
		switch (blend) {
			case SP_MIX_BLEND_SETUP:
				spColor_setFromColor(light, setupLight);
				spColor_setFromFloats3(dark, setupDark->r, setupDark->g, setupDark->b);
				return;
			case SP_MIX_BLEND_FIRST:
				spColor_addFloats(light, (setupLight->r - light->r) * alpha, (setupLight->g - light->g) * alpha,
								  (setupLight->b - light->b) * alpha,
								  (setupLight->a - light->a) * alpha);
				dark->r += (setupDark->r - dark->r) * alpha;
				dark->g += (setupDark->g - dark->g) * alpha;
				dark->b += (setupDark->b - dark->b) * alpha;
			default: {
			}
		}
		return;
	}

	r = 0, g = 0, b = 0, a = 0, r2 = 0, g2 = 0, b2 = 0;
	i = search2(self->super.super.frames, time, RGBA2_ENTRIES);
	curveType = (int) curves[i / RGBA2_ENTRIES];
	switch (curveType) {
		case CURVE_LINEAR: {
			float before = frames[i];
			r = frames[i + COLOR_R];
			g = frames[i + COLOR_G];
			b = frames[i + COLOR_B];
			a = frames[i + COLOR_A];
			r2 = frames[i + COLOR_R2];
			g2 = frames[i + COLOR_G2];
			b2 = frames[i + COLOR_B2];
			t = (time - before) / (frames[i + RGBA2_ENTRIES] - before);
			r += (frames[i + RGBA2_ENTRIES + COLOR_R] - r) * t;
			g += (frames[i + RGBA2_ENTRIES + COLOR_G] - g) * t;
			b += (frames[i + RGBA2_ENTRIES + COLOR_B] - b) * t;
			a += (frames[i + RGBA2_ENTRIES + COLOR_A] - a) * t;
			r2 += (frames[i + RGBA2_ENTRIES + COLOR_R2] - r2) * t;
			g2 += (frames[i + RGBA2_ENTRIES + COLOR_G2] - g2) * t;
			b2 += (frames[i + RGBA2_ENTRIES + COLOR_B2] - b2) * t;
			break;
		}
		case CURVE_STEPPED: {
			r = frames[i + COLOR_R];
			g = frames[i + COLOR_G];
			b = frames[i + COLOR_B];
			a = frames[i + COLOR_A];
			r2 = frames[i + COLOR_R2];
			g2 = frames[i + COLOR_G2];
			b2 = frames[i + COLOR_B2];
			break;
		}
		default: {
			r = _spCurveTimeline_getBezierValue(SUPER(self), time, i, COLOR_R, curveType - CURVE_BEZIER);
			g = _spCurveTimeline_getBezierValue(SUPER(self), time, i, COLOR_G,
												curveType + BEZIER_SIZE - CURVE_BEZIER);
			b = _spCurveTimeline_getBezierValue(SUPER(self), time, i, COLOR_B,
												curveType + BEZIER_SIZE * 2 - CURVE_BEZIER);
			a = _spCurveTimeline_getBezierValue(SUPER(self), time, i, COLOR_A,
												curveType + BEZIER_SIZE * 3 - CURVE_BEZIER);
			r2 = _spCurveTimeline_getBezierValue(SUPER(self), time, i, COLOR_R2,
												 curveType + BEZIER_SIZE * 4 - CURVE_BEZIER);
			g2 = _spCurveTimeline_getBezierValue(SUPER(self), time, i, COLOR_G2,
												 curveType + BEZIER_SIZE * 5 - CURVE_BEZIER);
			b2 = _spCurveTimeline_getBezierValue(SUPER(self), time, i, COLOR_B2,
												 curveType + BEZIER_SIZE * 6 - CURVE_BEZIER);
		}
	}

	light = &slot->color, dark = slot->darkColor;
	if (alpha == 1) {
		spColor_setFromFloats(light, r, g, b, a);
		spColor_setFromFloats3(dark, r2, g2, b2);
	} else {
		if (blend == SP_MIX_BLEND_SETUP) {
			spColor_setFromColor(light, &slot->data->color);
			spColor_setFromColor(dark, slot->data->darkColor);
		}
		spColor_addFloats(light, (r - light->r) * alpha, (g - light->g) * alpha, (b - light->b) * alpha,
						  (a - light->a) * alpha);
		dark->r += (r2 - dark->r) * alpha;
		dark->g += (g2 - dark->g) * alpha;
		dark->b += (b2 - dark->b) * alpha;
	}

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(direction);
}

spRGBA2Timeline *spRGBA2Timeline_create(int framesCount, int bezierCount, int slotIndex) {
	spRGBA2Timeline *timeline = NEW(spRGBA2Timeline);
	spPropertyId ids[3];
	ids[0] = ((spPropertyId) SP_PROPERTY_RGB << 32) | slotIndex;
	ids[1] = ((spPropertyId) SP_PROPERTY_ALPHA << 32) | slotIndex;
	ids[2] = ((spPropertyId) SP_PROPERTY_RGB2 << 32) | slotIndex;
	_spCurveTimeline_init(SUPER(timeline), framesCount, RGBA2_ENTRIES, bezierCount, ids, 3, SP_TIMELINE_RGBA2,
						  _spCurveTimeline_dispose, _spRGBA2Timeline_apply, _spCurveTimeline_setBezier);
	timeline->slotIndex = slotIndex;
	return timeline;
}

void spRGBA2Timeline_setFrame(spRGBA2Timeline *self, int frame, float time, float r, float g, float b, float a, float r2,
							  float g2, float b2) {
	float *frames = self->super.super.frames->items;
	frame *= RGBA2_ENTRIES;
	frames[frame] = time;
	frames[frame + COLOR_R] = r;
	frames[frame + COLOR_G] = g;
	frames[frame + COLOR_B] = b;
	frames[frame + COLOR_A] = a;
	frames[frame + COLOR_R2] = r2;
	frames[frame + COLOR_G2] = g2;
	frames[frame + COLOR_B2] = b2;
}

/**/

static const int RGB2_ENTRIES = 7, COLOR2_R2 = 5, COLOR2_G2 = 6, COLOR2_B2 = 7;

void _spRGB2Timeline_apply(spTimeline *timeline, spSkeleton *skeleton, float lastTime, float time, spEvent **firedEvents,
						   int *eventsCount, float alpha, spMixBlend blend, spMixDirection direction) {
	spSlot *slot;
	int i, curveType;
	float r, g, b, r2, g2, b2, t;
	spColor *light, *setupLight;
	spColor *dark, *setupDark;
	spRGB2Timeline *self = (spRGB2Timeline *) timeline;
	float *frames = self->super.super.frames->items;
	float *curves = self->super.curves->items;

	slot = skeleton->slots[self->slotIndex];
	if (!slot->bone->active) return;

	if (time < frames[0]) {
		light = &slot->color;
		dark = slot->darkColor;
		setupLight = &slot->data->color;
		setupDark = slot->data->darkColor;
		switch (blend) {
			case SP_MIX_BLEND_SETUP:
				spColor_setFromColor3(light, setupLight);
				spColor_setFromColor3(dark, setupDark);
				return;
			case SP_MIX_BLEND_FIRST:
				spColor_addFloats3(light, (setupLight->r - light->r) * alpha, (setupLight->g - light->g) * alpha,
								   (setupLight->b - light->b) * alpha);
				dark->r += (setupDark->r - dark->r) * alpha;
				dark->g += (setupDark->g - dark->g) * alpha;
				dark->b += (setupDark->b - dark->b) * alpha;
			default: {
			}
		}
		return;
	}

	r = 0, g = 0, b = 0, r2 = 0, g2 = 0, b2 = 0;
	i = search2(self->super.super.frames, time, RGB2_ENTRIES);
	curveType = (int) curves[i / RGB2_ENTRIES];
	switch (curveType) {
		case CURVE_LINEAR: {
			float before = frames[i];
			r = frames[i + COLOR_R];
			g = frames[i + COLOR_G];
			b = frames[i + COLOR_B];
			r2 = frames[i + COLOR2_R2];
			g2 = frames[i + COLOR2_G2];
			b2 = frames[i + COLOR2_B2];
			t = (time - before) / (frames[i + RGB2_ENTRIES] - before);
			r += (frames[i + RGB2_ENTRIES + COLOR_R] - r) * t;
			g += (frames[i + RGB2_ENTRIES + COLOR_G] - g) * t;
			b += (frames[i + RGB2_ENTRIES + COLOR_B] - b) * t;
			r2 += (frames[i + RGB2_ENTRIES + COLOR2_R2] - r2) * t;
			g2 += (frames[i + RGB2_ENTRIES + COLOR2_G2] - g2) * t;
			b2 += (frames[i + RGB2_ENTRIES + COLOR2_B2] - b2) * t;
			break;
		}
		case CURVE_STEPPED: {
			r = frames[i + COLOR_R];
			g = frames[i + COLOR_G];
			b = frames[i + COLOR_B];
			r2 = frames[i + COLOR2_R2];
			g2 = frames[i + COLOR2_G2];
			b2 = frames[i + COLOR2_B2];
			break;
		}
		default: {
			r = _spCurveTimeline_getBezierValue(SUPER(self), time, i, COLOR_R, curveType - CURVE_BEZIER);
			g = _spCurveTimeline_getBezierValue(SUPER(self), time, i, COLOR_G,
												curveType + BEZIER_SIZE - CURVE_BEZIER);
			b = _spCurveTimeline_getBezierValue(SUPER(self), time, i, COLOR_B,
												curveType + BEZIER_SIZE * 2 - CURVE_BEZIER);
			r2 = _spCurveTimeline_getBezierValue(SUPER(self), time, i, COLOR2_R2,
												 curveType + BEZIER_SIZE * 3 - CURVE_BEZIER);
			g2 = _spCurveTimeline_getBezierValue(SUPER(self), time, i, COLOR2_G2,
												 curveType + BEZIER_SIZE * 4 - CURVE_BEZIER);
			b2 = _spCurveTimeline_getBezierValue(SUPER(self), time, i, COLOR2_B2,
												 curveType + BEZIER_SIZE * 5 - CURVE_BEZIER);
		}
	}

	light = &slot->color, dark = slot->darkColor;
	if (alpha == 1) {
		spColor_setFromFloats3(light, r, g, b);
		spColor_setFromFloats3(dark, r2, g2, b2);
	} else {
		if (blend == SP_MIX_BLEND_SETUP) {
			spColor_setFromColor3(light, &slot->data->color);

			spColor_setFromColor3(dark, slot->data->darkColor);
		}
		spColor_addFloats3(light, (r - light->r) * alpha, (g - light->g) * alpha, (b - light->b) * alpha);
		dark->r += (r2 - dark->r) * alpha;
		dark->g += (g2 - dark->g) * alpha;
		dark->b += (b2 - dark->b) * alpha;
	}

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(direction);
}

spRGB2Timeline *spRGB2Timeline_create(int framesCount, int bezierCount, int slotIndex) {
	spRGB2Timeline *timeline = NEW(spRGB2Timeline);
	spPropertyId ids[2];
	ids[0] = ((spPropertyId) SP_PROPERTY_RGB << 32) | slotIndex;
	ids[1] = ((spPropertyId) SP_PROPERTY_RGB2 << 32) | slotIndex;
	_spCurveTimeline_init(SUPER(timeline), framesCount, RGB2_ENTRIES, bezierCount, ids, 2, SP_TIMELINE_RGB2,
						  _spCurveTimeline_dispose, _spRGB2Timeline_apply, _spCurveTimeline_setBezier);
	timeline->slotIndex = slotIndex;
	return timeline;
}

void spRGB2Timeline_setFrame(spRGB2Timeline *self, int frame, float time, float r, float g, float b, float r2, float g2,
							 float b2) {
	float *frames = self->super.super.frames->items;
	frame *= RGB2_ENTRIES;
	frames[frame] = time;
	frames[frame + COLOR_R] = r;
	frames[frame + COLOR_G] = g;
	frames[frame + COLOR_B] = b;
	frames[frame + COLOR2_R2] = r2;
	frames[frame + COLOR2_G2] = g2;
	frames[frame + COLOR2_B2] = b2;
}

/**/

static void
_spSetAttachment(spAttachmentTimeline *timeline, spSkeleton *skeleton, spSlot *slot, const char *attachmentName) {
	spSlot_setAttachment(slot, attachmentName == NULL ? NULL : spSkeleton_getAttachmentForSlotIndex(skeleton, timeline->slotIndex, attachmentName));
}

void _spAttachmentTimeline_apply(spTimeline *timeline, spSkeleton *skeleton, float lastTime, float time,
								 spEvent **firedEvents, int *eventsCount, float alpha, spMixBlend blend,
								 spMixDirection direction) {
	const char *attachmentName;
	spAttachmentTimeline *self = (spAttachmentTimeline *) timeline;
	float *frames = self->super.frames->items;
	spSlot *slot = skeleton->slots[self->slotIndex];
	if (!slot->bone->active) return;

	if (direction == SP_MIX_DIRECTION_OUT) {
		if (blend == SP_MIX_BLEND_SETUP) {
			_spSetAttachment(self, skeleton, slot, slot->data->attachmentName);
		}
		return;
	}

	if (time < frames[0]) {
		if (blend == SP_MIX_BLEND_SETUP || blend == SP_MIX_BLEND_FIRST) {
			_spSetAttachment(self, skeleton, slot, slot->data->attachmentName);
		}
		return;
	}

	if (time < frames[0]) {
		if (blend == SP_MIX_BLEND_SETUP || blend == SP_MIX_BLEND_FIRST)
			_spSetAttachment(self, skeleton, slot, slot->data->attachmentName);
		return;
	}

	attachmentName = self->attachmentNames[search(self->super.frames, time)];
	_spSetAttachment(self, skeleton, slot, attachmentName);

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(alpha);
}

void _spAttachmentTimeline_dispose(spTimeline *timeline) {
	spAttachmentTimeline *self = SUB_CAST(spAttachmentTimeline, timeline);
	int i;
	for (i = 0; i < self->super.frames->size; ++i)
		FREE(self->attachmentNames[i]);
	FREE(self->attachmentNames);
}

spAttachmentTimeline *spAttachmentTimeline_create(int framesCount, int slotIndex) {
	spAttachmentTimeline *self = NEW(spAttachmentTimeline);
	spPropertyId ids[1];
	ids[0] = ((spPropertyId) SP_PROPERTY_ATTACHMENT << 32) | slotIndex;
	_spTimeline_init(SUPER(self), framesCount, 1, ids, 1, SP_TIMELINE_ATTACHMENT, _spAttachmentTimeline_dispose,
					 _spAttachmentTimeline_apply, 0);
	self->attachmentNames = CALLOC(char *, framesCount);
	self->slotIndex = slotIndex;
	return self;
}

void spAttachmentTimeline_setFrame(spAttachmentTimeline *self, int frame, float time, const char *attachmentName) {
	self->super.frames->items[frame] = time;

	FREE(self->attachmentNames[frame]);
	if (attachmentName)
		MALLOC_STR(self->attachmentNames[frame], attachmentName);
	else
		self->attachmentNames[frame] = 0;
}

/**/

void _spDeformTimeline_setBezier(spTimeline *timeline, int bezier, int frame, float value, float time1, float value1,
								 float cx1, float cy1,
								 float cx2, float cy2, float time2, float value2) {
	spDeformTimeline *self = SUB_CAST(spDeformTimeline, timeline);
	int n, i = self->super.super.frameCount + bezier * BEZIER_SIZE;
	float *curves = self->super.curves->items;
	float tmpx = (time1 - cx1 * 2 + cx2) * 0.03, tmpy = cy2 * 0.03 - cy1 * 0.06;
	float dddx = ((cx1 - cx2) * 3 - time1 + time2) * 0.006, dddy = (cy1 - cy2 + 0.33333333) * 0.018;
	float ddx = tmpx * 2 + dddx, ddy = tmpy * 2 + dddy;
	float dx = (cx1 - time1) * 0.3 + tmpx + dddx * 0.16666667, dy = cy1 * 0.3 + tmpy + dddy * 0.16666667;
	float x = time1 + dx, y = dy;
	if (value == 0) curves[frame] = CURVE_BEZIER + i;
	for (n = i + BEZIER_SIZE; i < n; i += 2) {
		curves[i] = x;
		curves[i + 1] = y;
		dx += ddx;
		dy += ddy;
		ddx += dddx;
		ddy += dddy;
		x += dx;
		y += dy;
	}

	UNUSED(value1);
	UNUSED(value2);
}

float _spDeformTimeline_getCurvePercent(spDeformTimeline *self, float time, int frame) {
	float *curves = self->super.curves->items;
	float *frames = self->super.super.frames->items;
	int n, i = (int) curves[frame];
	int frameEntries = self->super.super.frameEntries;
	float x, y;
	switch (i) {
		case CURVE_LINEAR: {
			x = frames[frame];
			return (time - x) / (frames[frame + frameEntries] - x);
		}
		case CURVE_STEPPED: {
			return 0;
		}
		default: {
		}
	}
	i -= CURVE_BEZIER;
	if (curves[i] > time) {
		x = frames[frame];
		return curves[i + 1] * (time - x) / (curves[i] - x);
	}
	n = i + BEZIER_SIZE;
	for (i += 2; i < n; i += 2) {
		if (curves[i] >= time) {
			x = curves[i - 2], y = curves[i - 1];
			return y + (time - x) / (curves[i] - x) * (curves[i + 1] - y);
		}
	}
	x = curves[n - 2], y = curves[n - 1];
	return y + (1 - y) * (time - x) / (frames[frame + frameEntries] - x);
}

void _spDeformTimeline_apply(
		spTimeline *timeline, spSkeleton *skeleton, float lastTime, float time, spEvent **firedEvents,
		int *eventsCount, float alpha, spMixBlend blend, spMixDirection direction) {
	int frame, i, vertexCount;
	float percent;
	const float *prevVertices;
	const float *nextVertices;
	float *frames;
	int framesCount;
	float **frameVertices;
	float *deformArray;
	spDeformTimeline *self = (spDeformTimeline *) timeline;

	spSlot *slot = skeleton->slots[self->slotIndex];
	if (!slot->bone->active) return;

	if (!slot->attachment) return;
	switch (slot->attachment->type) {
		case SP_ATTACHMENT_BOUNDING_BOX:
		case SP_ATTACHMENT_CLIPPING:
		case SP_ATTACHMENT_MESH:
		case SP_ATTACHMENT_PATH: {
			spVertexAttachment *vertexAttachment = SUB_CAST(spVertexAttachment, slot->attachment);
			if (vertexAttachment->timelineAttachment != self->attachment) return;
			break;
		}
		default:
			return;
	}

	frames = self->super.super.frames->items;
	framesCount = self->super.super.frames->size;
	vertexCount = self->frameVerticesCount;
	if (slot->deformCount < vertexCount) {
		if (slot->deformCapacity < vertexCount) {
			FREE(slot->deform);
			slot->deform = MALLOC(float, vertexCount);
			slot->deformCapacity = vertexCount;
		}
	}
	if (slot->deformCount == 0) blend = SP_MIX_BLEND_SETUP;

	frameVertices = self->frameVertices;
	deformArray = slot->deform;

	if (time < frames[0]) { /* Time is before first frame. */
		spVertexAttachment *vertexAttachment = SUB_CAST(spVertexAttachment, slot->attachment);
		switch (blend) {
			case SP_MIX_BLEND_SETUP:
				slot->deformCount = 0;
				return;
			case SP_MIX_BLEND_FIRST:
				if (alpha == 1) {
					slot->deformCount = 0;
					return;
				}
				slot->deformCount = vertexCount;
				if (!vertexAttachment->bones) {
					float *setupVertices = vertexAttachment->vertices;
					for (i = 0; i < vertexCount; i++) {
						deformArray[i] += (setupVertices[i] - deformArray[i]) * alpha;
					}
				} else {
					alpha = 1 - alpha;
					for (i = 0; i < vertexCount; i++) {
						deformArray[i] *= alpha;
					}
				}
			case SP_MIX_BLEND_REPLACE:
			case SP_MIX_BLEND_ADD:; /* to appease compiler */
		}
		return;
	}

	slot->deformCount = vertexCount;
	if (time >= frames[framesCount - 1]) { /* Time is after last frame. */
		const float *lastVertices = self->frameVertices[framesCount - 1];
		if (alpha == 1) {
			if (blend == SP_MIX_BLEND_ADD) {
				spVertexAttachment *vertexAttachment = SUB_CAST(spVertexAttachment, slot->attachment);
				if (!vertexAttachment->bones) {
					/* Unweighted vertex positions, with alpha. */
					float *setupVertices = vertexAttachment->vertices;
					for (i = 0; i < vertexCount; i++) {
						deformArray[i] += lastVertices[i] - setupVertices[i];
					}
				} else {
					/* Weighted deform offsets, with alpha. */
					for (i = 0; i < vertexCount; i++)
						deformArray[i] += lastVertices[i];
				}
			} else {
				/* Vertex positions or deform offsets, no alpha. */
				memcpy(deformArray, lastVertices, vertexCount * sizeof(float));
			}
		} else {
			spVertexAttachment *vertexAttachment;
			switch (blend) {
				case SP_MIX_BLEND_SETUP:
					vertexAttachment = SUB_CAST(spVertexAttachment, slot->attachment);
					if (!vertexAttachment->bones) {
						/* Unweighted vertex positions, with alpha. */
						float *setupVertices = vertexAttachment->vertices;
						for (i = 0; i < vertexCount; i++) {
							float setup = setupVertices[i];
							deformArray[i] = setup + (lastVertices[i] - setup) * alpha;
						}
					} else {
						/* Weighted deform offsets, with alpha. */
						for (i = 0; i < vertexCount; i++)
							deformArray[i] = lastVertices[i] * alpha;
					}
					break;
				case SP_MIX_BLEND_FIRST:
				case SP_MIX_BLEND_REPLACE:
					/* Vertex positions or deform offsets, with alpha. */
					for (i = 0; i < vertexCount; i++)
						deformArray[i] += (lastVertices[i] - deformArray[i]) * alpha;
					break;
				case SP_MIX_BLEND_ADD:
					vertexAttachment = SUB_CAST(spVertexAttachment, slot->attachment);
					if (!vertexAttachment->bones) {
						/* Unweighted vertex positions, with alpha. */
						float *setupVertices = vertexAttachment->vertices;
						for (i = 0; i < vertexCount; i++) {
							deformArray[i] += (lastVertices[i] - setupVertices[i]) * alpha;
						}
					} else {
						for (i = 0; i < vertexCount; i++)
							deformArray[i] += lastVertices[i] * alpha;
					}
			}
		}
		return;
	}

	/* Interpolate between the previous frame and the current frame. */
	frame = search(self->super.super.frames, time);
	percent = _spDeformTimeline_getCurvePercent(self, time, frame);
	prevVertices = frameVertices[frame];
	nextVertices = frameVertices[frame + 1];

	if (alpha == 1) {
		if (blend == SP_MIX_BLEND_ADD) {
			spVertexAttachment *vertexAttachment = SUB_CAST(spVertexAttachment, slot->attachment);
			if (!vertexAttachment->bones) {
				float *setupVertices = vertexAttachment->vertices;
				for (i = 0; i < vertexCount; i++) {
					float prev = prevVertices[i];
					deformArray[i] += prev + (nextVertices[i] - prev) * percent - setupVertices[i];
				}
			} else {
				for (i = 0; i < vertexCount; i++) {
					float prev = prevVertices[i];
					deformArray[i] += prev + (nextVertices[i] - prev) * percent;
				}
			}
		} else {
			for (i = 0; i < vertexCount; i++) {
				float prev = prevVertices[i];
				deformArray[i] = prev + (nextVertices[i] - prev) * percent;
			}
		}
	} else {
		spVertexAttachment *vertexAttachment;
		switch (blend) {
			case SP_MIX_BLEND_SETUP:
				vertexAttachment = SUB_CAST(spVertexAttachment, slot->attachment);
				if (!vertexAttachment->bones) {
					float *setupVertices = vertexAttachment->vertices;
					for (i = 0; i < vertexCount; i++) {
						float prev = prevVertices[i], setup = setupVertices[i];
						deformArray[i] = setup + (prev + (nextVertices[i] - prev) * percent - setup) * alpha;
					}
				} else {
					for (i = 0; i < vertexCount; i++) {
						float prev = prevVertices[i];
						deformArray[i] = (prev + (nextVertices[i] - prev) * percent) * alpha;
					}
				}
				break;
			case SP_MIX_BLEND_FIRST:
			case SP_MIX_BLEND_REPLACE:
				for (i = 0; i < vertexCount; i++) {
					float prev = prevVertices[i];
					deformArray[i] += (prev + (nextVertices[i] - prev) * percent - deformArray[i]) * alpha;
				}
				break;
			case SP_MIX_BLEND_ADD:
				vertexAttachment = SUB_CAST(spVertexAttachment, slot->attachment);
				if (!vertexAttachment->bones) {
					float *setupVertices = vertexAttachment->vertices;
					for (i = 0; i < vertexCount; i++) {
						float prev = prevVertices[i];
						deformArray[i] += (prev + (nextVertices[i] - prev) * percent - setupVertices[i]) * alpha;
					}
				} else {
					for (i = 0; i < vertexCount; i++) {
						float prev = prevVertices[i];
						deformArray[i] += (prev + (nextVertices[i] - prev) * percent) * alpha;
					}
				}
		}
	}

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(direction);
}

void _spDeformTimeline_dispose(spTimeline *timeline) {
	spDeformTimeline *self = SUB_CAST(spDeformTimeline, timeline);
	int i;
	for (i = 0; i < self->super.super.frames->size; ++i)
		FREE(self->frameVertices[i]);
	FREE(self->frameVertices);
	_spCurveTimeline_dispose(timeline);
}

spDeformTimeline *spDeformTimeline_create(int framesCount, int frameVerticesCount, int bezierCount, int slotIndex,
										  spVertexAttachment *attachment) {
	spDeformTimeline *self = NEW(spDeformTimeline);
	spPropertyId ids[1];
	ids[0] = ((spPropertyId) SP_PROPERTY_DEFORM << 32) | ((slotIndex << 16 | attachment->id) & 0xffffffff);
	_spCurveTimeline_init(SUPER(self), framesCount, 1, bezierCount, ids, 1, SP_TIMELINE_DEFORM,
						  _spDeformTimeline_dispose, _spDeformTimeline_apply, _spDeformTimeline_setBezier);
	self->frameVertices = CALLOC(float *, framesCount);
	self->frameVerticesCount = frameVerticesCount;
	self->slotIndex = slotIndex;
	self->attachment = SUPER(attachment);
	return self;
}

void spDeformTimeline_setFrame(spDeformTimeline *self, int frame, float time, float *vertices) {
	self->super.super.frames->items[frame] = time;

	FREE(self->frameVertices[frame]);
	if (!vertices)
		self->frameVertices[frame] = 0;
	else {
		self->frameVertices[frame] = MALLOC(float, self->frameVerticesCount);
		memcpy(self->frameVertices[frame], vertices, self->frameVerticesCount * sizeof(float));
	}
}

/**/

static const int SEQUENCE_ENTRIES = 3, MODE = 1, DELAY = 2;

void _spSequenceTimeline_apply(spTimeline *timeline, spSkeleton *skeleton, float lastTime, float time, spEvent **firedEvents,
							   int *eventsCount, float alpha, spMixBlend blend, spMixDirection direction) {
	spSequenceTimeline *self = (spSequenceTimeline *) timeline;
	spSlot *slot = skeleton->slots[self->slotIndex];
	spAttachment *slotAttachment;
	float *frames;
	int i, modeAndIndex, count, index, mode;
	float before, delay;
	spSequence *sequence = NULL;

	if (!slot->bone->active) return;

	slotAttachment = slot->attachment;
	if (slotAttachment != self->attachment) {
		if (slotAttachment == NULL) return;
		switch (slotAttachment->type) {
			case SP_ATTACHMENT_BOUNDING_BOX:
			case SP_ATTACHMENT_CLIPPING:
			case SP_ATTACHMENT_MESH:
			case SP_ATTACHMENT_PATH: {
				spVertexAttachment *vertexAttachment = SUB_CAST(spVertexAttachment, slot->attachment);
				if (vertexAttachment->timelineAttachment != self->attachment) return;
				break;
			}
			default:
				return;
		}
	}

	if (self->attachment->type == SP_ATTACHMENT_REGION) sequence = ((spRegionAttachment *) self->attachment)->sequence;
	if (self->attachment->type == SP_ATTACHMENT_MESH) sequence = ((spMeshAttachment *) self->attachment)->sequence;
	if (!sequence) return;

	if (direction == SP_MIX_DIRECTION_OUT) {
		if (blend == SP_MIX_BLEND_SETUP) slot->sequenceIndex = -1;
		return;
	}

	frames = self->super.frames->items;
	if (time < frames[0]) { /* Time is before first frame. */
		if (blend == SP_MIX_BLEND_SETUP || blend == SP_MIX_BLEND_FIRST) slot->sequenceIndex = -1;
		return;
	}

	i = search2(self->super.frames, time, SEQUENCE_ENTRIES);
	before = frames[i];
	modeAndIndex = (int) frames[i + MODE];
	delay = frames[i + DELAY];

	index = modeAndIndex >> 4;
	count = sequence->regions->size;
	mode = modeAndIndex & 0xf;
	if (mode != SP_SEQUENCE_MODE_HOLD) {
		index += (int) (((time - before) / delay + 0.0001));
		switch (mode) {
			case SP_SEQUENCE_MODE_ONCE:
				index = MIN(count - 1, index);
				break;
			case SP_SEQUENCE_MODE_LOOP:
				index %= count;
				break;
			case SP_SEQUENCE_MODE_PINGPONG: {
				int n = (count << 1) - 2;
				index = n == 0 ? 0 : index % n;
				if (index >= count) index = n - index;
				break;
			}
			case SP_SEQUENCE_MODE_ONCEREVERSE:
				index = MAX(count - 1 - index, 0);
				break;
			case SP_SEQUENCE_MODE_LOOPREVERSE:
				index = count - 1 - (index % count);
				break;
			case SP_SEQUENCE_MODE_PINGPONGREVERSE: {
				int n = (count << 1) - 2;
				index = n == 0 ? 0 : (index + count - 1) % n;
				if (index >= count) index = n - index;
			}
		}
	}
	slot->sequenceIndex = index;

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(alpha);
	UNUSED(direction);
}

void _spSequenceTimeline_dispose(spTimeline *timeline) {
	/* NO-OP */
	UNUSED(timeline);
}

spSequenceTimeline *spSequenceTimeline_create(int framesCount, int slotIndex, spAttachment *attachment) {
	int sequenceId = 0;
	spSequenceTimeline *self = NEW(spSequenceTimeline);
	spPropertyId ids[1];
	if (attachment->type == SP_ATTACHMENT_REGION) sequenceId = ((spRegionAttachment *) attachment)->sequence->id;
	if (attachment->type == SP_ATTACHMENT_MESH) sequenceId = ((spMeshAttachment *) attachment)->sequence->id;
	ids[0] = (spPropertyId) SP_PROPERTY_SEQUENCE << 32 | ((slotIndex << 16 | sequenceId) & 0xffffffff);
	_spTimeline_init(SUPER(self), framesCount, SEQUENCE_ENTRIES, ids, 1, SP_TIMELINE_SEQUENCE, _spSequenceTimeline_dispose,
					 _spSequenceTimeline_apply, 0);
	self->slotIndex = slotIndex;
	self->attachment = attachment;
	return self;
}

void spSequenceTimeline_setFrame(spSequenceTimeline *self, int frame, float time, int mode, int index, float delay) {
	float *frames = self->super.frames->items;
	frame *= SEQUENCE_ENTRIES;
	frames[frame] = time;
	frames[frame + MODE] = mode | (index << 4);
	frames[frame + DELAY] = delay;
}

/**/

/** Fires events for frames > lastTime and <= time. */
void _spEventTimeline_apply(spTimeline *timeline, spSkeleton *skeleton, float lastTime, float time, spEvent **firedEvents,
							int *eventsCount, float alpha, spMixBlend blend, spMixDirection direction) {
	spEventTimeline *self = (spEventTimeline *) timeline;
	float *frames = self->super.frames->items;
	int framesCount = self->super.frames->size;
	int i;
	if (!firedEvents) return;

	if (lastTime > time) { /* Fire events after last time for looped animations. */
		_spEventTimeline_apply(timeline, skeleton, lastTime, (float) INT_MAX, firedEvents, eventsCount, alpha, blend,
							   direction);
		lastTime = -1;
	} else if (lastTime >= frames[framesCount - 1]) {
		/* Last time is after last i. */
		return;
	}

	if (time < frames[0]) return; /* Time is before first i. */

	if (lastTime < frames[0])
		i = 0;
	else {
		float frameTime;
		i = search(self->super.frames, lastTime) + 1;
		frameTime = frames[i];
		while (i > 0) { /* Fire multiple events with the same i. */
			if (frames[i - 1] != frameTime) break;
			i--;
		}
	}
	for (; i < framesCount && time >= frames[i]; ++i) {
		firedEvents[*eventsCount] = self->events[i];
		(*eventsCount)++;
	}
	UNUSED(direction);
}

void _spEventTimeline_dispose(spTimeline *timeline) {
	spEventTimeline *self = SUB_CAST(spEventTimeline, timeline);
	int i;

	for (i = 0; i < self->super.frames->size; ++i)
		spEvent_dispose(self->events[i]);
	FREE(self->events);
}

spEventTimeline *spEventTimeline_create(int framesCount) {
	spEventTimeline *self = NEW(spEventTimeline);
	spPropertyId ids[1];
	ids[0] = (spPropertyId) SP_PROPERTY_EVENT << 32;
	_spTimeline_init(SUPER(self), framesCount, 1, ids, 1, SP_TIMELINE_EVENT, _spEventTimeline_dispose,
					 _spEventTimeline_apply, 0);
	self->events = CALLOC(spEvent *, framesCount);
	return self;
}

void spEventTimeline_setFrame(spEventTimeline *self, int frame, spEvent *event) {
	self->super.frames->items[frame] = event->time;

	FREE(self->events[frame]);
	self->events[frame] = event;
}

/**/

void _spDrawOrderTimeline_apply(spTimeline *timeline, spSkeleton *skeleton, float lastTime, float time,
								spEvent **firedEvents, int *eventsCount, float alpha, spMixBlend blend,
								spMixDirection direction) {
	int i;
	const int *drawOrderToSetupIndex;
	spDrawOrderTimeline *self = (spDrawOrderTimeline *) timeline;
	float *frames = self->super.frames->items;

	if (direction == SP_MIX_DIRECTION_OUT) {
		if (blend == SP_MIX_BLEND_SETUP)
			memcpy(skeleton->drawOrder, skeleton->slots, self->slotsCount * sizeof(spSlot *));
		return;
	}

	if (time < frames[0]) {
		if (blend == SP_MIX_BLEND_SETUP || blend == SP_MIX_BLEND_FIRST)
			memcpy(skeleton->drawOrder, skeleton->slots, self->slotsCount * sizeof(spSlot *));
		return;
	}

	drawOrderToSetupIndex = self->drawOrders[search(self->super.frames, time)];
	if (!drawOrderToSetupIndex)
		memcpy(skeleton->drawOrder, skeleton->slots, self->slotsCount * sizeof(spSlot *));
	else {
		for (i = 0; i < self->slotsCount; ++i)
			skeleton->drawOrder[i] = skeleton->slots[drawOrderToSetupIndex[i]];
	}

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(alpha);
}

void _spDrawOrderTimeline_dispose(spTimeline *timeline) {
	spDrawOrderTimeline *self = SUB_CAST(spDrawOrderTimeline, timeline);
	int i;

	for (i = 0; i < self->super.frames->size; ++i)
		FREE(self->drawOrders[i]);
	FREE(self->drawOrders);
}

spDrawOrderTimeline *spDrawOrderTimeline_create(int framesCount, int slotsCount) {
	spDrawOrderTimeline *self = NEW(spDrawOrderTimeline);
	spPropertyId ids[1];
	ids[0] = (spPropertyId) SP_PROPERTY_DRAWORDER << 32;
	_spTimeline_init(SUPER(self), framesCount, 1, ids, 1, SP_TIMELINE_DRAWORDER, _spDrawOrderTimeline_dispose,
					 _spDrawOrderTimeline_apply, 0);

	self->drawOrders = CALLOC(int *, framesCount);
	self->slotsCount = slotsCount;

	return self;
}

void spDrawOrderTimeline_setFrame(spDrawOrderTimeline *self, int frame, float time, const int *drawOrder) {
	self->super.frames->items[frame] = time;

	FREE(self->drawOrders[frame]);
	if (!drawOrder)
		self->drawOrders[frame] = 0;
	else {
		self->drawOrders[frame] = MALLOC(int, self->slotsCount);
		memcpy(self->drawOrders[frame], drawOrder, self->slotsCount * sizeof(int));
	}
}

/**/
void _spInheritTimeline_apply(spTimeline *timeline, spSkeleton *skeleton, float lastTime, float time,
							  spEvent **firedEvents, int *eventsCount, float alpha, spMixBlend blend,
							  spMixDirection direction) {
	spInheritTimeline *self = (spInheritTimeline *) timeline;
	spBone *bone = skeleton->bones[self->boneIndex];
	float *frames = self->super.frames->items;
	if (!bone->active) return;

	if (direction == SP_MIX_DIRECTION_OUT) {
		if (blend == SP_MIX_BLEND_SETUP) bone->inherit = bone->data->inherit;
		return;
	}

	if (time < frames[0]) {
		if (blend == SP_MIX_BLEND_SETUP || blend == SP_MIX_BLEND_FIRST) bone->inherit = bone->data->inherit;
		return;
	}
	int idx = search2(self->super.frames, time, 2) + 1;
	bone->inherit = (spInherit) frames[idx];

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(alpha);
	UNUSED(direction);
}

void _spInheritTimeline_dispose(spTimeline *timeline) {
	// no-op, spTimeline_dispose disposes frames.
	UNUSED(timeline);
}

spInheritTimeline *spInheritTimeline_create(int framesCount, int boneIndex) {
	spInheritTimeline *self = NEW(spInheritTimeline);
	spPropertyId ids[1];
	ids[0] = (spPropertyId) SP_PROPERTY_INHERIT << 32;
	_spTimeline_init(SUPER(self), framesCount, 2, ids, 1, SP_TIMELINE_INHERIT, _spInheritTimeline_dispose,
					 _spInheritTimeline_apply, 0);

	self->boneIndex = boneIndex;

	return self;
}

void spInheritTimeline_setFrame(spInheritTimeline *self, int frame, float time, spInherit inherit) {
	frame *= 2;
	self->super.frames->items[frame] = time;
	self->super.frames->items[frame + 1] = inherit;
}


/**/

static const int IKCONSTRAINT_ENTRIES = 6;
static const int IKCONSTRAINT_MIX = 1, IKCONSTRAINT_SOFTNESS = 2, IKCONSTRAINT_BEND_DIRECTION = 3, IKCONSTRAINT_COMPRESS = 4, IKCONSTRAINT_STRETCH = 5;

void _spIkConstraintTimeline_apply(spTimeline *timeline, spSkeleton *skeleton, float lastTime, float time,
								   spEvent **firedEvents, int *eventsCount, float alpha, spMixBlend blend,
								   spMixDirection direction) {
	int i, curveType;
	float mix, softness, t;
	spIkConstraint *constraint;
	spIkConstraintTimeline *self = (spIkConstraintTimeline *) timeline;
	float *frames = self->super.super.frames->items;
	float *curves = self->super.curves->items;

	constraint = skeleton->ikConstraints[self->ikConstraintIndex];
	if (!constraint->active) return;

	if (time < frames[0]) {
		switch (blend) {
			case SP_MIX_BLEND_SETUP:
				constraint->mix = constraint->data->mix;
				constraint->softness = constraint->data->softness;
				constraint->bendDirection = constraint->data->bendDirection;
				constraint->compress = constraint->data->compress;
				constraint->stretch = constraint->data->stretch;
				return;
			case SP_MIX_BLEND_FIRST:
				constraint->mix += (constraint->data->mix - constraint->mix) * alpha;
				constraint->softness += (constraint->data->softness - constraint->softness) * alpha;
				constraint->bendDirection = constraint->data->bendDirection;
				constraint->compress = constraint->data->compress;
				constraint->stretch = constraint->data->stretch;
				return;
			default:
				return;
		}
	}

	i = search2(self->super.super.frames, time, IKCONSTRAINT_ENTRIES);
	curveType = (int) curves[i / IKCONSTRAINT_ENTRIES];
	switch (curveType) {
		case CURVE_LINEAR: {
			float before = frames[i];
			mix = frames[i + IKCONSTRAINT_MIX];
			softness = frames[i + IKCONSTRAINT_SOFTNESS];
			t = (time - before) / (frames[i + IKCONSTRAINT_ENTRIES] - before);
			mix += (frames[i + IKCONSTRAINT_ENTRIES + IKCONSTRAINT_MIX] - mix) * t;
			softness += (frames[i + IKCONSTRAINT_ENTRIES + IKCONSTRAINT_SOFTNESS] - softness) * t;
			break;
		}
		case CURVE_STEPPED: {
			mix = frames[i + IKCONSTRAINT_MIX];
			softness = frames[i + IKCONSTRAINT_SOFTNESS];
			break;
		}
		default: {
			mix = _spCurveTimeline_getBezierValue(SUPER(self), time, i, IKCONSTRAINT_MIX, curveType - CURVE_BEZIER);
			softness = _spCurveTimeline_getBezierValue(SUPER(self), time, i, IKCONSTRAINT_SOFTNESS,
													   curveType + BEZIER_SIZE - CURVE_BEZIER);
		}
	}

	if (blend == SP_MIX_BLEND_SETUP) {
		constraint->mix = constraint->data->mix + (mix - constraint->data->mix) * alpha;
		constraint->softness = constraint->data->softness + (softness - constraint->data->softness) * alpha;

		if (direction == SP_MIX_DIRECTION_OUT) {
			constraint->bendDirection = constraint->data->bendDirection;
			constraint->compress = constraint->data->compress;
			constraint->stretch = constraint->data->stretch;
		} else {
			constraint->bendDirection = frames[i + IKCONSTRAINT_BEND_DIRECTION];
			constraint->compress = frames[i + IKCONSTRAINT_COMPRESS] != 0;
			constraint->stretch = frames[i + IKCONSTRAINT_STRETCH] != 0;
		}
	} else {
		constraint->mix += (mix - constraint->mix) * alpha;
		constraint->softness += (softness - constraint->softness) * alpha;
		if (direction == SP_MIX_DIRECTION_IN) {
			constraint->bendDirection = frames[i + IKCONSTRAINT_BEND_DIRECTION];
			constraint->compress = frames[i + IKCONSTRAINT_COMPRESS] != 0;
			constraint->stretch = frames[i + IKCONSTRAINT_STRETCH] != 0;
		}
	}

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
}

spIkConstraintTimeline *spIkConstraintTimeline_create(int framesCount, int bezierCount, int ikConstraintIndex) {
	spIkConstraintTimeline *timeline = NEW(spIkConstraintTimeline);
	spPropertyId ids[1];
	ids[0] = ((spPropertyId) SP_PROPERTY_IKCONSTRAINT << 32) | ikConstraintIndex;
	_spCurveTimeline_init(SUPER(timeline), framesCount, IKCONSTRAINT_ENTRIES, bezierCount, ids, 1,
						  SP_TIMELINE_IKCONSTRAINT, _spCurveTimeline_dispose, _spIkConstraintTimeline_apply,
						  _spCurveTimeline_setBezier);
	timeline->ikConstraintIndex = ikConstraintIndex;
	return timeline;
}

void spIkConstraintTimeline_setFrame(spIkConstraintTimeline *self, int frame, float time, float mix, float softness,
									 int bendDirection, int /*boolean*/ compress, int /*boolean*/ stretch) {
	float *frames = self->super.super.frames->items;
	frame *= IKCONSTRAINT_ENTRIES;
	frames[frame] = time;
	frames[frame + IKCONSTRAINT_MIX] = mix;
	frames[frame + IKCONSTRAINT_SOFTNESS] = softness;
	frames[frame + IKCONSTRAINT_BEND_DIRECTION] = (float) bendDirection;
	frames[frame + IKCONSTRAINT_COMPRESS] = compress ? 1 : 0;
	frames[frame + IKCONSTRAINT_STRETCH] = stretch ? 1 : 0;
}

/**/
static const int TRANSFORMCONSTRAINT_ENTRIES = 7;
static const int TRANSFORMCONSTRAINT_ROTATE = 1;
static const int TRANSFORMCONSTRAINT_X = 2;
static const int TRANSFORMCONSTRAINT_Y = 3;
static const int TRANSFORMCONSTRAINT_SCALEX = 4;
static const int TRANSFORMCONSTRAINT_SCALEY = 5;
static const int TRANSFORMCONSTRAINT_SHEARY = 6;

void _spTransformConstraintTimeline_apply(spTimeline *timeline, spSkeleton *skeleton, float lastTime, float time,
										  spEvent **firedEvents, int *eventsCount, float alpha, spMixBlend blend,
										  spMixDirection direction) {
	int i, curveType;
	float rotate, x, y, scaleX, scaleY, shearY, t;
	spTransformConstraint *constraint;
	spTransformConstraintTimeline *self = (spTransformConstraintTimeline *) timeline;
	float *frames;
	float *curves;
	spTransformConstraintData *data;

	constraint = skeleton->transformConstraints[self->transformConstraintIndex];
	if (!constraint->active) return;

	frames = self->super.super.frames->items;
	curves = self->super.curves->items;

	data = constraint->data;
	if (time < frames[0]) {
		switch (blend) {
			case SP_MIX_BLEND_SETUP:
				constraint->mixRotate = data->mixRotate;
				constraint->mixX = data->mixX;
				constraint->mixY = data->mixY;
				constraint->mixScaleX = data->mixScaleX;
				constraint->mixScaleY = data->mixScaleY;
				constraint->mixShearY = data->mixShearY;
				return;
			case SP_MIX_BLEND_FIRST:
				constraint->mixRotate += (data->mixRotate - constraint->mixRotate) * alpha;
				constraint->mixX += (data->mixX - constraint->mixX) * alpha;
				constraint->mixY += (data->mixY - constraint->mixY) * alpha;
				constraint->mixScaleX += (data->mixScaleX - constraint->mixScaleX) * alpha;
				constraint->mixScaleY += (data->mixScaleY - constraint->mixScaleY) * alpha;
				constraint->mixShearY += (data->mixShearY - constraint->mixShearY) * alpha;
				return;
			default:
				return;
		}
	}

	i = search2(self->super.super.frames, time, TRANSFORMCONSTRAINT_ENTRIES);
	curveType = (int) curves[i / TRANSFORMCONSTRAINT_ENTRIES];
	switch (curveType) {
		case CURVE_LINEAR: {
			float before = frames[i];
			rotate = frames[i + TRANSFORMCONSTRAINT_ROTATE];
			x = frames[i + TRANSFORMCONSTRAINT_X];
			y = frames[i + TRANSFORMCONSTRAINT_Y];
			scaleX = frames[i + TRANSFORMCONSTRAINT_SCALEX];
			scaleY = frames[i + TRANSFORMCONSTRAINT_SCALEY];
			shearY = frames[i + TRANSFORMCONSTRAINT_SHEARY];
			t = (time - before) / (frames[i + TRANSFORMCONSTRAINT_ENTRIES] - before);
			rotate += (frames[i + TRANSFORMCONSTRAINT_ENTRIES + TRANSFORMCONSTRAINT_ROTATE] - rotate) * t;
			x += (frames[i + TRANSFORMCONSTRAINT_ENTRIES + TRANSFORMCONSTRAINT_X] - x) * t;
			y += (frames[i + TRANSFORMCONSTRAINT_ENTRIES + TRANSFORMCONSTRAINT_Y] - y) * t;
			scaleX += (frames[i + TRANSFORMCONSTRAINT_ENTRIES + TRANSFORMCONSTRAINT_SCALEX] - scaleX) * t;
			scaleY += (frames[i + TRANSFORMCONSTRAINT_ENTRIES + TRANSFORMCONSTRAINT_SCALEY] - scaleY) * t;
			shearY += (frames[i + TRANSFORMCONSTRAINT_ENTRIES + TRANSFORMCONSTRAINT_SHEARY] - shearY) * t;
			break;
		}
		case CURVE_STEPPED: {
			rotate = frames[i + TRANSFORMCONSTRAINT_ROTATE];
			x = frames[i + TRANSFORMCONSTRAINT_X];
			y = frames[i + TRANSFORMCONSTRAINT_Y];
			scaleX = frames[i + TRANSFORMCONSTRAINT_SCALEX];
			scaleY = frames[i + TRANSFORMCONSTRAINT_SCALEY];
			shearY = frames[i + TRANSFORMCONSTRAINT_SHEARY];
			break;
		}
		default: {
			rotate = _spCurveTimeline_getBezierValue(SUPER(self), time, i, TRANSFORMCONSTRAINT_ROTATE,
													 curveType - CURVE_BEZIER);
			x = _spCurveTimeline_getBezierValue(SUPER(self), time, i, TRANSFORMCONSTRAINT_X,
												curveType + BEZIER_SIZE - CURVE_BEZIER);
			y = _spCurveTimeline_getBezierValue(SUPER(self), time, i, TRANSFORMCONSTRAINT_Y,
												curveType + BEZIER_SIZE * 2 - CURVE_BEZIER);
			scaleX = _spCurveTimeline_getBezierValue(SUPER(self), time, i, TRANSFORMCONSTRAINT_SCALEX,
													 curveType + BEZIER_SIZE * 3 - CURVE_BEZIER);
			scaleY = _spCurveTimeline_getBezierValue(SUPER(self), time, i, TRANSFORMCONSTRAINT_SCALEY,
													 curveType + BEZIER_SIZE * 4 - CURVE_BEZIER);
			shearY = _spCurveTimeline_getBezierValue(SUPER(self), time, i, TRANSFORMCONSTRAINT_SHEARY,
													 curveType + BEZIER_SIZE * 5 - CURVE_BEZIER);
		}
	}

	if (blend == SP_MIX_BLEND_SETUP) {
		constraint->mixRotate = data->mixRotate + (rotate - data->mixRotate) * alpha;
		constraint->mixX = data->mixX + (x - data->mixX) * alpha;
		constraint->mixY = data->mixY + (y - data->mixY) * alpha;
		constraint->mixScaleX = data->mixScaleX + (scaleX - data->mixScaleX) * alpha;
		constraint->mixScaleY = data->mixScaleY + (scaleY - data->mixScaleY) * alpha;
		constraint->mixShearY = data->mixShearY + (shearY - data->mixShearY) * alpha;
	} else {
		constraint->mixRotate += (rotate - constraint->mixRotate) * alpha;
		constraint->mixX += (x - constraint->mixX) * alpha;
		constraint->mixY += (y - constraint->mixY) * alpha;
		constraint->mixScaleX += (scaleX - constraint->mixScaleX) * alpha;
		constraint->mixScaleY += (scaleY - constraint->mixScaleY) * alpha;
		constraint->mixShearY += (shearY - constraint->mixShearY) * alpha;
	}

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(direction);
}

spTransformConstraintTimeline *
spTransformConstraintTimeline_create(int framesCount, int bezierCount, int transformConstraintIndex) {
	spTransformConstraintTimeline *timeline = NEW(spTransformConstraintTimeline);
	spPropertyId ids[1];
	ids[0] = ((spPropertyId) SP_PROPERTY_TRANSFORMCONSTRAINT << 32) | transformConstraintIndex;
	_spCurveTimeline_init(SUPER(timeline), framesCount, TRANSFORMCONSTRAINT_ENTRIES, bezierCount, ids, 1,
						  SP_TIMELINE_TRANSFORMCONSTRAINT, _spCurveTimeline_dispose,
						  _spTransformConstraintTimeline_apply, _spCurveTimeline_setBezier);
	timeline->transformConstraintIndex = transformConstraintIndex;
	return timeline;
}

void spTransformConstraintTimeline_setFrame(spTransformConstraintTimeline *self, int frame, float time, float mixRotate,
											float mixX, float mixY, float mixScaleX, float mixScaleY, float mixShearY) {
	float *frames = self->super.super.frames->items;
	frame *= TRANSFORMCONSTRAINT_ENTRIES;
	frames[frame] = time;
	frames[frame + TRANSFORMCONSTRAINT_ROTATE] = mixRotate;
	frames[frame + TRANSFORMCONSTRAINT_X] = mixX;
	frames[frame + TRANSFORMCONSTRAINT_Y] = mixY;
	frames[frame + TRANSFORMCONSTRAINT_SCALEX] = mixScaleX;
	frames[frame + TRANSFORMCONSTRAINT_SCALEY] = mixScaleY;
	frames[frame + TRANSFORMCONSTRAINT_SHEARY] = mixShearY;
}

/**/
static const int PATHCONSTRAINTPOSITION_ENTRIES = 2;
static const int PATHCONSTRAINTPOSITION_VALUE = 1;

void _spPathConstraintPositionTimeline_apply(spTimeline *timeline, spSkeleton *skeleton, float lastTime, float time,
											 spEvent **firedEvents, int *eventsCount, float alpha, spMixBlend blend,
											 spMixDirection direction) {
	spPathConstraintPositionTimeline *self = (spPathConstraintPositionTimeline *) timeline;
	spPathConstraint *constraint = skeleton->pathConstraints[self->pathConstraintIndex];
	if (constraint->active) constraint->position = spCurveTimeline1_getAbsoluteValue(SUPER(self), time, alpha, blend, constraint->position, constraint->data->position);

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(direction);
}

spPathConstraintPositionTimeline *
spPathConstraintPositionTimeline_create(int framesCount, int bezierCount, int pathConstraintIndex) {
	spPathConstraintPositionTimeline *timeline = NEW(spPathConstraintPositionTimeline);
	spPropertyId ids[1];
	ids[0] = ((spPropertyId) SP_PROPERTY_PATHCONSTRAINT_POSITION << 32) | pathConstraintIndex;
	_spCurveTimeline_init(SUPER(timeline), framesCount, PATHCONSTRAINTPOSITION_ENTRIES, bezierCount, ids, 1,
						  SP_TIMELINE_PATHCONSTRAINTPOSITION, _spCurveTimeline_dispose,
						  _spPathConstraintPositionTimeline_apply, _spCurveTimeline_setBezier);
	timeline->pathConstraintIndex = pathConstraintIndex;
	return timeline;
}

void spPathConstraintPositionTimeline_setFrame(spPathConstraintPositionTimeline *self, int frame, float time, float value) {
	float *frames = self->super.super.frames->items;
	frame *= PATHCONSTRAINTPOSITION_ENTRIES;
	frames[frame] = time;
	frames[frame + PATHCONSTRAINTPOSITION_VALUE] = value;
}

/**/
static const int PATHCONSTRAINTSPACING_ENTRIES = 2;
static const int PATHCONSTRAINTSPACING_VALUE = 1;

void _spPathConstraintSpacingTimeline_apply(spTimeline *timeline, spSkeleton *skeleton, float lastTime, float time,
											spEvent **firedEvents, int *eventsCount, float alpha, spMixBlend blend,
											spMixDirection direction) {
	spPathConstraintSpacingTimeline *self = (spPathConstraintSpacingTimeline *) timeline;
	spPathConstraint *constraint = skeleton->pathConstraints[self->pathConstraintIndex];
	if (constraint->active) constraint->spacing = spCurveTimeline1_getAbsoluteValue(SUPER(self), time, alpha, blend, constraint->spacing, constraint->data->spacing);

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(direction);
}

spPathConstraintSpacingTimeline *
spPathConstraintSpacingTimeline_create(int framesCount, int bezierCount, int pathConstraintIndex) {
	spPathConstraintSpacingTimeline *timeline = NEW(spPathConstraintSpacingTimeline);
	spPropertyId ids[1];
	ids[0] = ((spPropertyId) SP_PROPERTY_PATHCONSTRAINT_SPACING << 32) | pathConstraintIndex;
	_spCurveTimeline_init(SUPER(timeline), framesCount, PATHCONSTRAINTSPACING_ENTRIES, bezierCount, ids, 1,
						  SP_TIMELINE_PATHCONSTRAINTSPACING, _spCurveTimeline_dispose,
						  _spPathConstraintSpacingTimeline_apply, _spCurveTimeline_setBezier);
	timeline->pathConstraintIndex = pathConstraintIndex;
	return timeline;
}

void spPathConstraintSpacingTimeline_setFrame(spPathConstraintSpacingTimeline *self, int frame, float time, float value) {
	float *frames = self->super.super.frames->items;
	frame *= PATHCONSTRAINTSPACING_ENTRIES;
	frames[frame] = time;
	frames[frame + PATHCONSTRAINTSPACING_VALUE] = value;
}

/**/

static const int PATHCONSTRAINTMIX_ENTRIES = 4;
static const int PATHCONSTRAINTMIX_ROTATE = 1;
static const int PATHCONSTRAINTMIX_X = 2;
static const int PATHCONSTRAINTMIX_Y = 3;

void _spPathConstraintMixTimeline_apply(spTimeline *timeline, spSkeleton *skeleton, float lastTime, float time,
										spEvent **firedEvents, int *eventsCount, float alpha, spMixBlend blend,
										spMixDirection direction) {
	int i, curveType;
	float rotate, x, y, t;
	spPathConstraint *constraint;
	spPathConstraintMixTimeline *self = (spPathConstraintMixTimeline *) timeline;
	float *frames;
	float *curves;

	constraint = skeleton->pathConstraints[self->pathConstraintIndex];
	if (!constraint->active) return;

	frames = self->super.super.frames->items;
	curves = self->super.curves->items;

	if (time < frames[0]) {
		switch (blend) {
			case SP_MIX_BLEND_SETUP:
				constraint->mixRotate = constraint->data->mixRotate;
				constraint->mixX = constraint->data->mixX;
				constraint->mixY = constraint->data->mixY;
				return;
			case SP_MIX_BLEND_FIRST:
				constraint->mixRotate += (constraint->data->mixRotate - constraint->mixRotate) * alpha;
				constraint->mixX += (constraint->data->mixX - constraint->mixX) * alpha;
				constraint->mixY += (constraint->data->mixY - constraint->mixY) * alpha;
			default: {
			}
		}
		return;
	}

	i = search2(self->super.super.frames, time, PATHCONSTRAINTMIX_ENTRIES);
	curveType = (int) curves[i >> 2];
	switch (curveType) {
		case CURVE_LINEAR: {
			float before = frames[i];
			rotate = frames[i + PATHCONSTRAINTMIX_ROTATE];
			x = frames[i + PATHCONSTRAINTMIX_X];
			y = frames[i + PATHCONSTRAINTMIX_Y];
			t = (time - before) / (frames[i + PATHCONSTRAINTMIX_ENTRIES] - before);
			rotate += (frames[i + PATHCONSTRAINTMIX_ENTRIES + PATHCONSTRAINTMIX_ROTATE] - rotate) * t;
			x += (frames[i + PATHCONSTRAINTMIX_ENTRIES + PATHCONSTRAINTMIX_X] - x) * t;
			y += (frames[i + PATHCONSTRAINTMIX_ENTRIES + PATHCONSTRAINTMIX_Y] - y) * t;
			break;
		}
		case CURVE_STEPPED: {
			rotate = frames[i + PATHCONSTRAINTMIX_ROTATE];
			x = frames[i + PATHCONSTRAINTMIX_X];
			y = frames[i + PATHCONSTRAINTMIX_Y];
			break;
		}
		default: {
			rotate = _spCurveTimeline_getBezierValue(SUPER(self), time, i, PATHCONSTRAINTMIX_ROTATE,
													 curveType - CURVE_BEZIER);
			x = _spCurveTimeline_getBezierValue(SUPER(self), time, i, PATHCONSTRAINTMIX_X,
												curveType + BEZIER_SIZE - CURVE_BEZIER);
			y = _spCurveTimeline_getBezierValue(SUPER(self), time, i, PATHCONSTRAINTMIX_Y,
												curveType + BEZIER_SIZE * 2 - CURVE_BEZIER);
		}
	}

	if (blend == SP_MIX_BLEND_SETUP) {
		spPathConstraintData *data = constraint->data;
		constraint->mixRotate = data->mixRotate + (rotate - data->mixRotate) * alpha;
		constraint->mixX = data->mixX + (x - data->mixX) * alpha;
		constraint->mixY = data->mixY + (y - data->mixY) * alpha;
	} else {
		constraint->mixRotate += (rotate - constraint->mixRotate) * alpha;
		constraint->mixX += (x - constraint->mixX) * alpha;
		constraint->mixY += (y - constraint->mixY) * alpha;
	}

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(direction);
}

spPathConstraintMixTimeline *
spPathConstraintMixTimeline_create(int framesCount, int bezierCount, int pathConstraintIndex) {
	spPathConstraintMixTimeline *timeline = NEW(spPathConstraintMixTimeline);
	spPropertyId ids[1];
	ids[0] = ((spPropertyId) SP_PROPERTY_PATHCONSTRAINT_MIX << 32) | pathConstraintIndex;
	_spCurveTimeline_init(SUPER(timeline), framesCount, PATHCONSTRAINTMIX_ENTRIES, bezierCount, ids, 1,
						  SP_TIMELINE_PATHCONSTRAINTMIX, _spCurveTimeline_dispose, _spPathConstraintMixTimeline_apply,
						  _spCurveTimeline_setBezier);
	timeline->pathConstraintIndex = pathConstraintIndex;
	return timeline;
}

void spPathConstraintMixTimeline_setFrame(spPathConstraintMixTimeline *self, int frame, float time, float mixRotate,
										  float mixX, float mixY) {
	float *frames = self->super.super.frames->items;
	frame *= PATHCONSTRAINTMIX_ENTRIES;
	frames[frame] = time;
	frames[frame + PATHCONSTRAINTMIX_ROTATE] = mixRotate;
	frames[frame + PATHCONSTRAINTMIX_X] = mixX;
	frames[frame + PATHCONSTRAINTMIX_Y] = mixY;
}

/**/

int /*bool*/ _spPhysicsConstraintTimeline_global(spPhysicsConstraintData *data, spTimelineType type) {
	switch (type) {
		case SP_TIMELINE_PHYSICSCONSTRAINT_INERTIA:
			return data->inertiaGlobal;
		case SP_TIMELINE_PHYSICSCONSTRAINT_STRENGTH:
			return data->strengthGlobal;
		case SP_TIMELINE_PHYSICSCONSTRAINT_DAMPING:
			return data->dampingGlobal;
		case SP_TIMELINE_PHYSICSCONSTRAINT_MASS:
			return data->massGlobal;
		case SP_TIMELINE_PHYSICSCONSTRAINT_WIND:
			return data->windGlobal;
		case SP_TIMELINE_PHYSICSCONSTRAINT_GRAVITY:
			return data->gravityGlobal;
		case SP_TIMELINE_PHYSICSCONSTRAINT_MIX:
			return data->mixGlobal;
		default:
			// should never happen
			return 0;
	}
}

void _spPhysicsConstraintTimeline_set(spPhysicsConstraint *constraint, spTimelineType type, float value) {
	switch (type) {
		case SP_TIMELINE_PHYSICSCONSTRAINT_INERTIA:
			constraint->inertia = value;
			break;
		case SP_TIMELINE_PHYSICSCONSTRAINT_STRENGTH:
			constraint->strength = value;
			break;
		case SP_TIMELINE_PHYSICSCONSTRAINT_DAMPING:
			constraint->damping = value;
			break;
		case SP_TIMELINE_PHYSICSCONSTRAINT_MASS:
			constraint->massInverse = value;
			break;
		case SP_TIMELINE_PHYSICSCONSTRAINT_WIND:
			constraint->wind = value;
			break;
		case SP_TIMELINE_PHYSICSCONSTRAINT_GRAVITY:
			constraint->gravity = value;
			break;
		case SP_TIMELINE_PHYSICSCONSTRAINT_MIX:
			constraint->mix = value;
			break;
		default:
			// should never happen
			break;
	}
}

float _spPhysicsConstraintTimeline_get(spPhysicsConstraint *constraint, spTimelineType type) {
	switch (type) {
		case SP_TIMELINE_PHYSICSCONSTRAINT_INERTIA:
			return constraint->inertia;
		case SP_TIMELINE_PHYSICSCONSTRAINT_STRENGTH:
			return constraint->strength;
		case SP_TIMELINE_PHYSICSCONSTRAINT_DAMPING:
			return constraint->damping;
		case SP_TIMELINE_PHYSICSCONSTRAINT_MASS:
			return constraint->massInverse;
		case SP_TIMELINE_PHYSICSCONSTRAINT_WIND:
			return constraint->wind;
		case SP_TIMELINE_PHYSICSCONSTRAINT_GRAVITY:
			return constraint->gravity;
		case SP_TIMELINE_PHYSICSCONSTRAINT_MIX:
			return constraint->mix;
		default:
			// should never happen
			return 0;
	}
}

float _spPhysicsConstraintTimeline_setup(spPhysicsConstraint *constraint, spTimelineType type) {
	switch (type) {
		case SP_TIMELINE_PHYSICSCONSTRAINT_INERTIA:
			return constraint->data->inertia;
		case SP_TIMELINE_PHYSICSCONSTRAINT_STRENGTH:
			return constraint->data->strength;
		case SP_TIMELINE_PHYSICSCONSTRAINT_DAMPING:
			return constraint->data->damping;
		case SP_TIMELINE_PHYSICSCONSTRAINT_MASS:
			return constraint->data->massInverse;
		case SP_TIMELINE_PHYSICSCONSTRAINT_WIND:
			return constraint->data->wind;
		case SP_TIMELINE_PHYSICSCONSTRAINT_GRAVITY:
			return constraint->data->gravity;
		case SP_TIMELINE_PHYSICSCONSTRAINT_MIX:
			return constraint->data->mix;
		default:
			// should never happen
			return 0;
	}
}

void _spPhysicsConstraintTimeline_apply(spTimeline *timeline, spSkeleton *skeleton, float lastTime, float time,
										spEvent **firedEvents, int *eventsCount, float alpha, spMixBlend blend,
										spMixDirection direction) {
	spPhysicsConstraintTimeline *self = SUB_CAST(spPhysicsConstraintTimeline, timeline);
	spTimelineType type = self->super.super.type;
	float *frames = self->super.super.frames->items;
	if (self->physicsConstraintIndex == -1) {
		float value = time >= frames[0] ? spCurveTimeline1_getCurveValue(SUPER(self), time) : 0;

		spPhysicsConstraint **physicsConstraints = skeleton->physicsConstraints;
		for (int i = 0; i < skeleton->physicsConstraintsCount; i++) {
			spPhysicsConstraint *constraint = physicsConstraints[i];
			if (constraint->active && _spPhysicsConstraintTimeline_global(constraint->data, type))
				_spPhysicsConstraintTimeline_set(constraint, type, spCurveTimeline1_getAbsoluteValue2(SUPER(self), time, alpha, blend, _spPhysicsConstraintTimeline_get(constraint, type), _spPhysicsConstraintTimeline_setup(constraint, type), value));
		}
	} else {
		spPhysicsConstraint *constraint = skeleton->physicsConstraints[self->physicsConstraintIndex];
		if (constraint->active) _spPhysicsConstraintTimeline_set(constraint, type, spCurveTimeline1_getAbsoluteValue(SUPER(self), time, alpha, blend, _spPhysicsConstraintTimeline_get(constraint, type), _spPhysicsConstraintTimeline_setup(constraint, type)));
	}
	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(direction);
}

spPhysicsConstraintTimeline *
spPhysicsConstraintTimeline_create(int frameCount, int bezierCount, int physicsConstraintIndex, spTimelineType type) {
	spPhysicsConstraintTimeline *timeline = NEW(spPhysicsConstraintTimeline);
	spPropertyId ids[1];
	spPropertyId id;
	switch (type) {
		case SP_TIMELINE_PHYSICSCONSTRAINT_INERTIA:
			id = SP_PROPERTY_PHYSICSCONSTRAINT_INERTIA;
			break;
		case SP_TIMELINE_PHYSICSCONSTRAINT_STRENGTH:
			id = SP_PROPERTY_PHYSICSCONSTRAINT_STRENGTH;
			break;
		case SP_TIMELINE_PHYSICSCONSTRAINT_DAMPING:
			id = SP_PROPERTY_PHYSICSCONSTRAINT_DAMPING;
			break;
		case SP_TIMELINE_PHYSICSCONSTRAINT_MASS:
			id = SP_PROPERTY_PHYSICSCONSTRAINT_MASS;
			break;
		case SP_TIMELINE_PHYSICSCONSTRAINT_WIND:
			id = SP_PROPERTY_PHYSICSCONSTRAINT_WIND;
			break;
		case SP_TIMELINE_PHYSICSCONSTRAINT_GRAVITY:
			id = SP_PROPERTY_PHYSICSCONSTRAINT_GRAVITY;
			break;
		case SP_TIMELINE_PHYSICSCONSTRAINT_MIX:
			id = SP_PROPERTY_PHYSICSCONSTRAINT_MIX;
			break;
		default:
			// should never happen
			id = SP_PROPERTY_PHYSICSCONSTRAINT_INERTIA;
	}
	ids[0] = ((spPropertyId) id << 32) | physicsConstraintIndex;
	_spCurveTimeline_init(SUPER(timeline), frameCount, CURVE1_ENTRIES, bezierCount, ids, 1, type,
						  _spCurveTimeline_dispose, _spPhysicsConstraintTimeline_apply, _spCurveTimeline_setBezier);
	timeline->physicsConstraintIndex = physicsConstraintIndex;
	return timeline;
}

void spPhysicsConstraintTimeline_setFrame(spPhysicsConstraintTimeline *self, int frame, float time, float value) {
	spCurveTimeline1_setFrame(SUPER(self), frame, time, value);
}

/**/
void _spPhysicsConstraintResetTimeline_apply(spTimeline *timeline, spSkeleton *skeleton, float lastTime, float time,
											 spEvent **firedEvents, int *eventsCount, float alpha, spMixBlend blend,
											 spMixDirection direction) {
	spPhysicsConstraintResetTimeline *self = (spPhysicsConstraintResetTimeline *) timeline;
	spPhysicsConstraint *constraint = NULL;
	if (self->physicsConstraintIndex != -1) {
		constraint = skeleton->physicsConstraints[self->physicsConstraintIndex];
		if (!constraint->active) return;
	}

	float *frames = SUPER(self)->frames->items;
	if (lastTime > time) {// Apply after lastTime for looped animations.
		_spPhysicsConstraintResetTimeline_apply(SUPER(self), skeleton, lastTime, INT_MAX, NULL, 0, alpha, blend, direction);
		lastTime = -1;
	} else if (lastTime >= frames[SUPER(self)->frameCount - 1])// Last time is after last frame.
		return;
	if (time < frames[0]) return;

	if (lastTime < frames[0] || time >= frames[search(self->super.frames, lastTime) + 1]) {
		if (constraint != NULL)
			spPhysicsConstraint_reset(constraint);
		else {
			spPhysicsConstraint **physicsConstraints = skeleton->physicsConstraints;
			for (int i = 0; i < skeleton->physicsConstraintsCount; i++) {
				constraint = physicsConstraints[i];
				if (constraint->active) spPhysicsConstraint_reset(constraint);
			}
		}
	}

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(alpha);
	UNUSED(direction);
}

void _spPhysicsConstraintResetTimeline_dispose(spTimeline *timeline) {
	// no-op, spTimeline_dispose disposes frames.
	UNUSED(timeline);
}

spPhysicsConstraintResetTimeline *spPhysicsConstraintResetTimeline_create(int framesCount, int physicsConstraintIndex) {
	spPhysicsConstraintResetTimeline *self = NEW(spPhysicsConstraintResetTimeline);
	spPropertyId ids[1];
	ids[0] = (spPropertyId) SP_PROPERTY_PHYSICSCONSTRAINT_RESET << 32;
	_spTimeline_init(SUPER(self), framesCount, 1, ids, 1, SP_TIMELINE_PHYSICSCONSTRAINT_RESET, _spPhysicsConstraintResetTimeline_dispose,
					 _spPhysicsConstraintResetTimeline_apply, 0);

	self->physicsConstraintIndex = physicsConstraintIndex;

	return self;
}

void spPhysicsConstraintResetTimeline_setFrame(spPhysicsConstraintResetTimeline *self, int frame, float time) {
	self->super.frames->items[frame] = time;
}
