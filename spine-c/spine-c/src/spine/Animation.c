/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

#include <spine/Animation.h>
#include <spine/IkConstraint.h>
#include <limits.h>
#include <spine/extension.h>

_SP_ARRAY_IMPLEMENT_TYPE(spPropertyIdArray, spPropertyId)
_SP_ARRAY_IMPLEMENT_TYPE(spTimelineArray, spTimeline*)

spAnimation* spAnimation_create (const char* name, spTimelineArray* timelines) {
    int i, n;
	spAnimation* self = NEW(spAnimation);
	MALLOC_STR(self->name, name);
	for (i = 0, n = timelines->size; i < n; i++) {
	    spPropertyIdArray_addAllValues(self->timelineIds, timelines->items[i]->propertyIds, 0, timelines->items[i]->propertyIdsCount);
	}
	return self;
}

void spAnimation_dispose (spAnimation* self) {
	int i;
	for (i = 0; i < self->timelines->size; ++i)
		spTimeline_dispose(self->timelines->items[i]);
	spTimelineArray_dispose(self->timelines);
	spPropertyIdArray_dispose(self->timelineIds);
	FREE(self->name);
	FREE(self);
}

int /*bool*/ spAnimation_hasTimeline(spAnimation* self, spPropertyId* ids, int idsCount) {
    int i, n, ii, nn;
    for (i = 0, n = self->timelineIds->size; i < n; i++) {
        for (ii = 0, nn = idsCount; ii < nn; ii++) {
            if (self->timelineIds->items[i] == ids[ii]) return 1;
        }
    }
    return 0;
}

void spAnimation_apply (const spAnimation* self, spSkeleton* skeleton, float lastTime, float time, int loop, spEvent** events,
	int* eventsCount, float alpha, spMixBlend blend, spMixDirection direction
) {
	int i, n = self->timelines->size;

	if (loop && self->duration) {
		time = FMOD(time, self->duration);
		if (lastTime > 0) lastTime = FMOD(lastTime, self->duration);
	}

	for (i = 0; i < n; ++i)
		spTimeline_apply(self->timelines->items[i], skeleton, lastTime, time, events, eventsCount, alpha, blend, direction);
}

static search (float* values, int valuesCount, float time) {
    int i;
    for (i = 1; i < valuesCount; i++)
        if (values[i] > time) return i - 1;
    return valuesCount - 1;
}

static search2 (float* values, int valuesCount, float time, int step) {
    int i;
    for (i = step; i < valuesCount; i += step)
        if (values[i] > time) return i - step;
    return valuesCount - step;
}

/**/

void _spTimeline_init (spTimeline* self,
    int frameCount,
    int frameEntries,
    spPropertyId* propertyIds,
    int propertyIdsCount,
    void (*dispose) (spTimeline* self),
    void (*apply) (const spTimeline* self, spSkeleton* skeleton, float lastTime, float time, spEvent** firedEvents,
		int* eventsCount, float alpha, spMixBlend blend, spMixDirection direction)
) {
    int i, n;
    self->frames = spFloatArray_create(frameCount * frameEntries);
    self->frameEntries = frameEntries;
	self->vtable.dispose = dispose;
	self->vtable.apply = apply;

	for (i = 0, n = propertyIdsCount; i < n; i++)
	    self->propertyIds[i] = propertyIds[i];
}

void spTimeline_dispose (spTimeline* self) {
    spFloatArray_dispose(self->frames);
	self->vtable.dispose(self);
    FREE(self);
}

void spTimeline_apply (const spTimeline* self, spSkeleton* skeleton, float lastTime, float time, spEvent** firedEvents,
		int* eventsCount, float alpha, spMixBlend blend, spMixDirection direction) {
	self->vtable.apply(self, skeleton, lastTime, time, firedEvents, eventsCount, alpha, blend, direction);
}

int spTimeline_getFrameCount (const spTimeline* self) {
    return self->frames->size / self->frameEntries;
}

float spTimeline_getDuration (const spTimeline* self) {
    return self->frames->items[self->frames->size - self->frameEntries];
}

/**/

#define CURVE_LINEAR 0
#define CURVE_STEPPED 1
#define CURVE_BEZIER 2
#define BEZIER_SIZE 18

void _spCurveTimeline_init (spCurveTimeline* self,
                            int frameCount,
                            int frameEntries,
                            int bezierCount,
                            spPropertyId* propertyIds,
                            int propertyIdsCount,
                            void (*dispose) (spTimeline* self),
                            void (*apply) (const spTimeline* self, spSkeleton* skeleton, float lastTime, float time, spEvent** firedEvents,
                                           int* eventsCount, float alpha, spMixBlend blend, spMixDirection direction)
) {
	_spTimeline_init(SUPER(self), frameCount, frameEntries, propertyIds, propertyIdsCount, dispose, apply);
	self->curves = spFloatArray_create(frameCount + bezierCount * BEZIER_SIZE);
}

void _spCurveTimeline_dispose (spTimeline* self) {
	spFloatArray_dispose(SUB_CAST(spCurveTimeline, self)->curves);
}

void _spCurveTimeline_setBezier (spCurveTimeline* self, int bezier, int frame, float value, float time1, float value1, float cx1, float cy1, float cx2, float cy2, float time2, float value2) {
    float tmpx, tmpy, dddx, dddy,ddx, ddy, dx, dy, x, y;
    int i = spTimeline_getFrameCount(SUPER(self)) + bezier * BEZIER_SIZE, n;
    float* curves = self->curves->items;
    if (value == 0) curves[frame] = CURVE_BEZIER + i;
    tmpx = (time1 - cx1 * 2 + cx2) * 0.03; tmpy = (value1 - cy1 * 2 + cy2) * 0.03;
    dddx = ((cx1 - cx2) * 3 - time1 + time2) * 0.006; dddy = ((cy1 - cy2) * 3 - value1 + value2) * 0.006;
    ddx = tmpx * 2 + dddx; ddy = tmpy * 2 + dddy;
    dx = (cx1 - time1) * 0.3 + tmpx + dddx * 0.16666667; dy = (cy1 - value1) * 0.3 + tmpy + dddy * 0.16666667;
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

float _spCurveTimeline_getBezierValue(spCurveTimeline* self, float time, int frame, int valueOffset, int i) {
    float* curves = self->curves->items;
    float* frames = SUPER(self)->frames->items;
    float x, y;
    int n;
    if (curves[i] > time) {
        x = frames[frame]; y = frames[frame + valueOffset];
        return y + (time - x) / (curves[i] - x) * (curves[i + 1] - y);
    }
    n = i + BEZIER_SIZE;
    for (i += 2; i < n; i += 2) {
        if (curves[i] >= time) {
            x = curves[i - 2]; y = curves[i - 1];
            return y + (time - x) / (curves[i] - x) * (curves[i + 1] - y);
        }
    }
    frame += self->super.frameEntries;
    x = curves[n - 2]; y = curves[n - 1];
    return y + (time - x) / (frames[frame] - x) * (frames[frame + valueOffset] - y);
}

void spCurveTimeline_setLinear (spCurveTimeline* self, int frameIndex) {
	self->curves->items[frameIndex * BEZIER_SIZE] = CURVE_LINEAR;
}

void spCurveTimeline_setStepped (spCurveTimeline* self, int frameIndex) {
	self->curves->items[frameIndex * BEZIER_SIZE] = CURVE_STEPPED;
}

#define CURVE1_ENTRIES 2
#define CURVE1_VALUE 1

void spCurveTimeline1_setFrame(spCurveTimeline1* self, int frame, float time, float value) {
    float *frames = self->super.frames->items;
    frame <<= 1;
    frames[frame] = time;
    frames[frame + CURVE1_VALUE] = value;
}

float spCurveTimeline1_getCurveValue(spCurveTimeline1* self, float time) {
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

    curveType = (int)curves[i >> 1];
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

#define CURVE2_ENTRIES 3
#define CURVE2_VALUE1 1
#define CURVE2_VALUE2 2

SP_API void spCurveTimeline2_setFrame(spCurveTimeline1* self, int frame, float time, float value1, float value2) {
    float *frames = self->super.frames->items;
    frame *= CURVE2_ENTRIES;
    frames[frame] = time;
    frames[frame + CURVE2_VALUE1] = value1;
    frames[frame + CURVE2_VALUE2] = value2;
}

/**/

void _spRotateTimeline_apply (const spTimeline* timeline, spSkeleton* skeleton, float lastTime, float time, spEvent** firedEvents,
	int* eventsCount, float alpha, spMixBlend blend, spMixDirection direction
) {
	spBone *bone;
    float r;
	spRotateTimeline* self = SUB_CAST(spRotateTimeline, timeline);
	float *frames = self->super.super.frames->items;

	bone = skeleton->bones[self->boneIndex];
    if (!bone->active) return;

    if (time < frames[0]) {
        switch (blend) {
            case SP_MIX_BLEND_SETUP:
                bone->rotation = bone->data->rotation;
                return;
            case SP_MIX_BLEND_FIRST:
                bone->rotation += (bone->data->rotation - bone->rotation) * alpha;
            default: {}
        }
        return;
    }

    r = spCurveTimeline1_getCurveValue(SUPER(self), time);
    switch (blend) {
        case SP_MIX_BLEND_SETUP:
            bone->rotation = bone->data->rotation + r * alpha;
            break;
        case SP_MIX_BLEND_FIRST:
        case SP_MIX_BLEND_REPLACE:
            r += bone->data->rotation - bone->rotation;
        case SP_MIX_BLEND_ADD:
            bone->rotation += r * alpha;
    }

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(direction);
}

spRotateTimeline* spRotateTimeline_create (int frameCount, int bezierCount, int boneIndex) {
    spRotateTimeline* timeline = NEW(spRotateTimeline);
    spPropertyId ids[1];
    ids[0] = ((spPropertyId)SP_PROPERTY_ROTATE << 32) | boneIndex;
    _spCurveTimeline_init(SUPER(timeline), frameCount, CURVE1_ENTRIES, bezierCount, ids, 1, _spCurveTimeline_dispose, _spRotateTimeline_apply);
    timeline->boneIndex = boneIndex;
    return timeline;
}

void spRotateTimeline_setFrame (spRotateTimeline* self, int frameIndex, float time, float degrees) {
    spCurveTimeline1_setFrame(SUPER(self), frameIndex, time, degrees);
}

/**/

void _spTranslateTimeline_apply (const spTimeline* timeline, spSkeleton* skeleton, float lastTime, float time,
	spEvent** firedEvents, int* eventsCount, float alpha, spMixBlend blend, spMixDirection direction
) {
	spBone *bone;
	float x, y, t;
	int i, curveType;

	spTranslateTimeline* self = SUB_CAST(spTranslateTimeline, timeline);
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
            default: {}
        }
        return;
    }

    i = search(frames, time, CURVE2_ENTRIES);
    curveType = (int)curves[i / CURVE2_ENTRIES];
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
            y = _spCurveTimeline_getBezierValue(SUPER(self), time, i, CURVE2_VALUE2, curveType + BEZIER_SIZE - CURVE_BEZIER);
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

spTranslateTimeline* spTranslateTimeline_create (int frameCount, int bezierCount, int boneIndex) {
    spTranslateTimeline* timeline = NEW(spTranslateTimeline);
    spPropertyId ids[2];
    ids[0] = ((spPropertyId)SP_PROPERTY_X << 32) | boneIndex;
    ids[1] = ((spPropertyId)SP_PROPERTY_Y << 32) | boneIndex;
    _spCurveTimeline_init(SUPER(timeline), frameCount, CURVE2_ENTRIES, bezierCount, ids, 2, _spCurveTimeline_dispose, _spTranslateTimeline_apply);
    timeline->boneIndex = boneIndex;
    return timeline;
}

void spTranslateTimeline_setFrame (spTranslateTimeline* self, int frameIndex, float time, float x, float y) {
	spCurveTimeline2_setFrame(SUPER(self), frameIndex, time, x, y);
}

/**/

void _spTranslateXTimeline_apply (const spTimeline* timeline, spSkeleton* skeleton, float lastTime, float time,
                                  spEvent** firedEvents, int* eventsCount, float alpha, spMixBlend blend, spMixDirection direction
) {
    spBone *bone;
    float x, t;
    int i, curveType;

    spTranslateXTimeline* self = SUB_CAST(spTranslateXTimeline, timeline);
    float *frames = self->super.super.frames->items;
    float *curves = self->super.curves->items;

    bone = skeleton->bones[self->boneIndex];
    if (!bone->active) return;

    if (time < frames[0]) {
        switch (blend) {
            case SP_MIX_BLEND_SETUP:
                bone->x = bone->data->x;
                return;
            case SP_MIX_BLEND_FIRST:
                bone->x += (bone->data->x - bone->x) * alpha;
            default: {}
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

spTranslateXTimeline* spTranslateXTimeline_create (int frameCount, int bezierCount, int boneIndex) {
    spTranslateXTimeline* timeline = NEW(spTranslateXTimeline);
    spPropertyId ids[1];
    ids[0] = ((spPropertyId)SP_PROPERTY_X << 32) | boneIndex;
    _spCurveTimeline_init(SUPER(timeline), frameCount, CURVE1_ENTRIES, bezierCount, ids, 1, _spCurveTimeline_dispose, _spTranslateXTimeline_apply);
    timeline->boneIndex = boneIndex;
    return timeline;
}

void spTranslateXTimeline_setFrame (spTranslateXTimeline* self, int frame, float time, float x) {
    spCurveTimeline1_setFrame(SUPER(self), frame, time, x);
}

/**/

void _spTranslateYTimeline_apply (const spTimeline* timeline, spSkeleton* skeleton, float lastTime, float time,
                                  spEvent** firedEvents, int* eventsCount, float alpha, spMixBlend blend, spMixDirection direction
) {
    spBone *bone;
    int frame;
    float y, t;
    int i, curveType;

    spTranslateXTimeline* self = SUB_CAST(spTranslateXTimeline, timeline);
    float *frames = self->super.super.frames->items;
    float *curves = self->super.curves->items;

    bone = skeleton->bones[self->boneIndex];
    if (!bone->active) return;

    if (time < frames[0]) {
        switch (blend) {
            case SP_MIX_BLEND_SETUP:
                bone->y = bone->data->y;
                return;
            case SP_MIX_BLEND_FIRST:
                bone->y += (bone->data->y - bone->y) * alpha;
            default: {}
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
}

spTranslateYTimeline* spTranslateYTimeline_create (int frameCount, int bezierCount, int boneIndex) {
    spTranslateYTimeline* timeline = NEW(spTranslateYTimeline);
    spPropertyId ids[1];
    ids[0] = ((spPropertyId)SP_PROPERTY_Y << 32) | boneIndex;
    _spCurveTimeline_init(SUPER(timeline), frameCount, CURVE1_ENTRIES, bezierCount, ids, 1, _spCurveTimeline_dispose, _spTranslateYTimeline_apply);
    timeline->boneIndex = boneIndex;
    return timeline;
}

void spTranslateYTimeline_setFrame (spTranslateYTimeline* self, int frame, float time, float y) {
    spCurveTimeline1_setFrame(SUPER(self), frame, time, y);
}

/**/

void _spScaleTimeline_apply (const spTimeline* timeline, spSkeleton* skeleton, float lastTime, float time, spEvent** firedEvents,
	int* eventsCount, float alpha, spMixBlend blend, spMixDirection direction
) {
	spBone *bone;
	int i, curveType;
	float x, y, t;

	spScaleTimeline* self = SUB_CAST(spScaleTimeline, timeline);
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
            default: {}
        }
        return;
    }
    
    i = search(frames, time, CURVE2_ENTRIES);
    curveType = (int)curves[i / CURVE2_ENTRIES];
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
            y = _spCurveTimeline_getBezierValue(SUPER(self), time, i, CURVE2_VALUE2, curveType + BEZIER_SIZE - CURVE_BEZIER);
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
                    bx = bone->scaleX;
                    by = bone->scaleY;
                    bone->scaleX = bx + (ABS(x) * SIGNUM(bx) - bone->data->scaleX) * alpha;
                    bone->scaleY = by + (ABS(y) * SIGNUM(by) - bone->data->scaleY) * alpha;
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
                    bx = SIGNUM(x);
                    by = SIGNUM(y);
                    bone->scaleX = ABS(bone->scaleX) * bx + (x - ABS(bone->data->scaleX) * bx) * alpha;
                    bone->scaleY = ABS(bone->scaleY) * by + (y - ABS(bone->data->scaleY) * by) * alpha;
            }
        }
    }

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
}

spScaleTimeline* spScaleTimeline_create (int frameCount, int bezierCount, int boneIndex) {
    spScaleTimeline* timeline = NEW(spScaleTimeline);
    spPropertyId ids[2];
    ids[0] = ((spPropertyId)SP_PROPERTY_SCALEX << 32) | boneIndex;
    ids[1] = ((spPropertyId)SP_PROPERTY_SCALEY << 32) | boneIndex;
    _spCurveTimeline_init(SUPER(timeline), frameCount, CURVE2_ENTRIES, bezierCount, ids, 2, _spCurveTimeline_dispose, _spScaleTimeline_apply);
    timeline->boneIndex = boneIndex;
    return timeline;
}

void spScaleTimeline_setFrame (spScaleTimeline* self, int frameIndex, float time, float x, float y) {
	spCurveTimeline2_setFrame(SUPER(self), frameIndex, time, x, y);
}

/**/

void _spScaleXTimeline_apply (const spTimeline* timeline, spSkeleton* skeleton, float lastTime, float time,
                              spEvent** firedEvents, int* eventsCount, float alpha, spMixBlend blend, spMixDirection direction
) {
    spBone *bone;
    int frame;
    float x, t;
    int i, curveType;

    spTranslateXTimeline* self = SUB_CAST(spTranslateXTimeline, timeline);
    float *frames = self->super.super.frames->items;
    float *curves = self->super.curves->items;

    bone = skeleton->bones[self->boneIndex];
    if (!bone->active) return;

    if (time < frames[0]) {
        switch (blend) {
            case SP_MIX_BLEND_SETUP:
                bone->scaleX = bone->data->scaleX;
                return;
            case SP_MIX_BLEND_FIRST:
                bone->scaleX += (bone->data->scaleX - bone->scaleX) * alpha;
            default: {}
        }
        return;
    }

    x = spCurveTimeline1_getCurveValue(SUPER(self), time) * bone->data->scaleX;
    if (alpha == 1) {
        if (blend == SP_MIX_BLEND_ADD)
            bone->scaleX += x - bone->data->scaleX;
        else
            bone->scaleX = x;
    } else {
        /* Mixing out uses sign of setup or current pose, else use sign of key. */
        float bx;
        if (direction == SP_MIX_DIRECTION_OUT) {
            switch (blend) {
                case SP_MIX_BLEND_SETUP:
                    bx = bone->data->scaleX;
                    bone->scaleX = bx + (ABS(x) * SIGNUM(bx) - bx) * alpha;
                    break;
                case SP_MIX_BLEND_FIRST:
                case SP_MIX_BLEND_REPLACE:
                    bx = bone->scaleX;
                    bone->scaleX = bx + (ABS(x) * SIGNUM(bx) - bx) * alpha;
                    break;
                case SP_MIX_BLEND_ADD:
                    bx = bone->scaleX;
                    bone->scaleX = bx + (ABS(x) * SIGNUM(bx) - bone->data->scaleX) * alpha;
            }
        } else {
            switch (blend) {
                case SP_MIX_BLEND_SETUP:
                    bx = ABS(bone->data->scaleX) * SIGNUM(x);
                    bone->scaleX = bx + (x - bx) * alpha;
                    break;
                case SP_MIX_BLEND_FIRST:
                case SP_MIX_BLEND_REPLACE:
                    bx = ABS(bone->scaleX) * SIGNUM(x);
                    bone->scaleX = bx + (x - bx) * alpha;
                    break;
                case SP_MIX_BLEND_ADD:
                    bx = SIGNUM(x);
                    bone->scaleX = ABS(bone->scaleX) * bx + (x - ABS(bone->data->scaleX) * bx) * alpha;
            }
        }
    }

    UNUSED(lastTime);
    UNUSED(firedEvents);
    UNUSED(eventsCount);
}

spScaleXTimeline* spScaleXTimeline_create (int frameCount, int bezierCount, int boneIndex) {
    spScaleXTimeline* timeline = NEW(spScaleXTimeline);
    spPropertyId ids[1];
    ids[0] = ((spPropertyId)SP_PROPERTY_SCALEX << 32) | boneIndex;
    _spCurveTimeline_init(SUPER(timeline), frameCount, CURVE1_ENTRIES, bezierCount, ids, 1, _spCurveTimeline_dispose, _spScaleXTimeline_apply);
    timeline->boneIndex = boneIndex;
    return timeline;
}

void spScaleXTimeline_setFrame (spScaleXTimeline* self, int frame, float time, float y) {
    spCurveTimeline1_setFrame(SUPER(self), frame, time, y);
}

/**/

void _spScaleYTimeline_apply (const spTimeline* timeline, spSkeleton* skeleton, float lastTime, float time,
                                  spEvent** firedEvents, int* eventsCount, float alpha, spMixBlend blend, spMixDirection direction
) {
    spBone *bone;
    int frame;
    float y, t;
    int i, curveType;

    spTranslateXTimeline* self = SUB_CAST(spTranslateXTimeline, timeline);
    float *frames = self->super.super.frames->items;
    float *curves = self->super.curves->items;

    bone = skeleton->bones[self->boneIndex];
    if (!bone->active) return;

    if (time < frames[0]) {
        switch (blend) {
            case SP_MIX_BLEND_SETUP:
                bone->scaleY = bone->data->scaleY;
                return;
            case SP_MIX_BLEND_FIRST:
                bone->scaleY += (bone->data->scaleY - bone->scaleY) * alpha;
            default: {}
        }
        return;
    }

    y = spCurveTimeline1_getCurveValue(SUPER(self), time) * bone->data->scaleY;
    if (alpha == 1) {
        if (blend == SP_MIX_BLEND_ADD)
            bone->scaleY += y - bone->data->scaleY;
        else
            bone->scaleY = y;
    } else {
        /* Mixing out uses sign of setup or current pose, else use sign of key. */
        float by = 0;
        if (direction == SP_MIX_DIRECTION_OUT) {
            switch (blend) {
                case SP_MIX_BLEND_SETUP:
                    by = bone->data->scaleY;
                    bone->scaleY = by + (ABS(y) * SIGNUM(by) - by) * alpha;
                    break;
                case SP_MIX_BLEND_FIRST:
                case SP_MIX_BLEND_REPLACE:
                    by = bone->scaleY;
                    bone->scaleY = by + (ABS(y) * SIGNUM(by) - by) * alpha;
                    break;
                case SP_MIX_BLEND_ADD:
                    by = bone->scaleY;
                    bone->scaleY = by + (ABS(y) * SIGNUM(by) - bone->data->scaleY) * alpha;
            }
        } else {
            switch (blend) {
                case SP_MIX_BLEND_SETUP:
                    by = ABS(bone->data->scaleY) * SIGNUM(y);
                    bone->scaleY = by + (y - by) * alpha;
                    break;
                case SP_MIX_BLEND_FIRST:
                case SP_MIX_BLEND_REPLACE:
                    by = ABS(bone->scaleY) * SIGNUM(y);
                    bone->scaleY = by + (y - by) * alpha;
                    break;
                case SP_MIX_BLEND_ADD:
                    by = SIGNUM(y);
                    bone->scaleY = ABS(bone->scaleY) * by + (y - ABS(bone->data->scaleY) * by) * alpha;
            }
        }
    }

    UNUSED(lastTime);
    UNUSED(firedEvents);
    UNUSED(eventsCount);
}

spScaleYTimeline* spScaleYTimeline_create (int frameCount, int bezierCount, int boneIndex) {
    spScaleYTimeline* timeline = NEW(spScaleYTimeline);
    spPropertyId ids[1];
    ids[0] = ((spPropertyId)SP_PROPERTY_SCALEY << 32) | boneIndex;
    _spCurveTimeline_init(SUPER(timeline), frameCount, CURVE1_ENTRIES, bezierCount, ids, 1, _spCurveTimeline_dispose, _spScaleYTimeline_apply);
    timeline->boneIndex = boneIndex;
    return timeline;
}

void spScaleYTimeline_setFrame (spScaleYTimeline* self, int frame, float time, float y) {
    spCurveTimeline1_setFrame(SUPER(self), frame, time, y);
}

/**/

void _spShearTimeline_apply (const spTimeline* timeline, spSkeleton* skeleton, float lastTime, float time, spEvent** firedEvents,
	int* eventsCount, float alpha, spMixBlend blend, spMixDirection direction
) {
	spBone *bone;
	int frame;
	float frameTime, percent, x, y;
	float *frames;
	int framesCount;

	spShearTimeline* self = SUB_CAST(spShearTimeline, timeline);

	bone = skeleton->bones[self->boneIndex];
	if (!bone->active) return;
	frames = self->frames;
	framesCount = self->framesCount;
	if (time < self->frames[0]) {
		switch (blend) {
		case SP_MIX_BLEND_SETUP:
			bone->shearX = bone->data->shearX;
			bone->shearY = bone->data->shearY;
			return;
		case SP_MIX_BLEND_FIRST:
			bone->shearX += (bone->data->shearX - bone->shearX) * alpha;
			bone->shearY += (bone->data->shearY - bone->shearY) * alpha;
		case SP_MIX_BLEND_REPLACE:
		case SP_MIX_BLEND_ADD:
			; /* to appease compiler */
		}
		return;
	}

	if (time >= frames[framesCount - TRANSLATE_ENTRIES]) { /* Time is after last frame. */
		x = frames[framesCount + TRANSLATE_PREV_X];
		y = frames[framesCount + TRANSLATE_PREV_Y];
	} else {
		/* Interpolate between the previous frame and the current frame. */
		frame = binarySearch(frames, framesCount, time, TRANSLATE_ENTRIES);
		x = frames[frame + TRANSLATE_PREV_X];
		y = frames[frame + TRANSLATE_PREV_Y];
		frameTime = frames[frame];
		percent = spCurveTimeline_getCurvePercent(SUPER(self), frame / TRANSLATE_ENTRIES - 1,
			1 - (time - frameTime) / (frames[frame + TRANSLATE_PREV_TIME] - frameTime));

		x = x + (frames[frame + TRANSLATE_X] - x) * percent;
		y = y + (frames[frame + TRANSLATE_Y] - y) * percent;
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

int _spShearTimeline_getPropertyId (const spTimeline* timeline) {
	return (SP_TIMELINE_SHEAR << 24) + SUB_CAST(spShearTimeline, timeline)->boneIndex;
}

spShearTimeline* spShearTimeline_create (int framesCount) {
	return (spShearTimeline*)_spBaseTimeline_create(framesCount, SP_TIMELINE_SHEAR, 3, _spShearTimeline_apply, _spShearTimeline_getPropertyId);
}

void spShearTimeline_setFrame (spShearTimeline* self, int frameIndex, float time, float x, float y) {
	spTranslateTimeline_setFrame(self, frameIndex, time, x, y);
}

/**/

static const int COLOR_PREV_TIME = -5, COLOR_PREV_R = -4, COLOR_PREV_G = -3, COLOR_PREV_B = -2, COLOR_PREV_A = -1;
static const int COLOR_R = 1, COLOR_G = 2, COLOR_B = 3, COLOR_A = 4;

void _spColorTimeline_apply (const spTimeline* timeline, spSkeleton* skeleton, float lastTime, float time, spEvent** firedEvents,
	int* eventsCount, float alpha, spMixBlend blend, spMixDirection direction
) {
	spSlot *slot;
	int frame;
	float percent, frameTime;
	float r, g, b, a;
	spColor* color;
	spColor* setup;
	spColorTimeline* self = (spColorTimeline*)timeline;
	slot = skeleton->slots[self->slotIndex];
	if (!slot->bone->active) return;

	if (time < self->frames[0]) {
		switch (blend) {
		case SP_MIX_BLEND_SETUP:
			spColor_setFromColor(&slot->color, &slot->data->color);
			return;
		case SP_MIX_BLEND_FIRST:
			color = &slot->color;
			setup = &slot->data->color;
			spColor_addFloats(color, (setup->r - color->r) * alpha, (setup->g - color->g) * alpha, (setup->b - color->b) * alpha,
				(setup->a - color->a) * alpha);
		case SP_MIX_BLEND_REPLACE:
		case SP_MIX_BLEND_ADD:
			; /* to appease compiler */
		}
		return;
	}

	if (time >= self->frames[self->framesCount - 5]) { /* Time is after last frame */
		int i = self->framesCount;
		r = self->frames[i + COLOR_PREV_R];
		g = self->frames[i + COLOR_PREV_G];
		b = self->frames[i + COLOR_PREV_B];
		a = self->frames[i + COLOR_PREV_A];
	} else {
		/* Interpolate between the previous frame and the current frame. */
		frame = binarySearch(self->frames, self->framesCount, time, COLOR_ENTRIES);

		r = self->frames[frame + COLOR_PREV_R];
		g = self->frames[frame + COLOR_PREV_G];
		b = self->frames[frame + COLOR_PREV_B];
		a = self->frames[frame + COLOR_PREV_A];

		frameTime = self->frames[frame];
		percent = spCurveTimeline_getCurvePercent(SUPER(self), frame / COLOR_ENTRIES - 1,
			1 - (time - frameTime) / (self->frames[frame + COLOR_PREV_TIME] - frameTime));

		r += (self->frames[frame + COLOR_R] - r) * percent;
		g += (self->frames[frame + COLOR_G] - g) * percent;
		b += (self->frames[frame + COLOR_B] - b) * percent;
		a += (self->frames[frame + COLOR_A] - a) * percent;
	}
	if (alpha == 1) {
		spColor_setFromFloats(&slot->color, r, g, b, a);
	} else {
		if (blend == SP_MIX_BLEND_SETUP) spColor_setFromColor(&slot->color, &slot->data->color);
		spColor_addFloats(&slot->color, (r - slot->color.r) * alpha, (g - slot->color.g) * alpha, (b - slot->color.b) * alpha, (a - slot->color.a) * alpha);
	}

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(direction);
}

int _spColorTimeline_getPropertyId (const spTimeline* timeline) {
	return (SP_TIMELINE_COLOR << 24) + SUB_CAST(spColorTimeline, timeline)->slotIndex;
}

spColorTimeline* spColorTimeline_create (int framesCount) {
	return (spColorTimeline*)_spBaseTimeline_create(framesCount, SP_TIMELINE_COLOR, 5, _spColorTimeline_apply, _spColorTimeline_getPropertyId);
}

void spColorTimeline_setFrame (spColorTimeline* self, int frameIndex, float time, float r, float g, float b, float a) {
	frameIndex *= COLOR_ENTRIES;
	self->frames[frameIndex] = time;
	self->frames[frameIndex + COLOR_R] = r;
	self->frames[frameIndex + COLOR_G] = g;
	self->frames[frameIndex + COLOR_B] = b;
	self->frames[frameIndex + COLOR_A] = a;
}

/**/

static const int TWOCOLOR_PREV_TIME = -8, TWOCOLOR_PREV_R = -7, TWOCOLOR_PREV_G = -6, TWOCOLOR_PREV_B = -5, TWOCOLOR_PREV_A = -4;
static const int TWOCOLOR_PREV_R2 = -3, TWOCOLOR_PREV_G2 = -2, TWOCOLOR_PREV_B2 = -1;
static const int TWOCOLOR_R = 1, TWOCOLOR_G = 2, TWOCOLOR_B = 3, TWOCOLOR_A = 4, TWOCOLOR_R2 = 5, TWOCOLOR_G2 = 6, TWOCOLOR_B2 = 7;

void _spTwoColorTimeline_apply (const spTimeline* timeline, spSkeleton* skeleton, float lastTime, float time, spEvent** firedEvents,
	int* eventsCount, float alpha, spMixBlend blend, spMixDirection direction
) {
	spSlot *slot;
	int frame;
	float percent, frameTime;
	float r, g, b, a, r2, g2, b2;
	spColor* light;
	spColor* dark;
	spColor* setupLight;
	spColor* setupDark;
	spColorTimeline* self = (spColorTimeline*)timeline;
	slot = skeleton->slots[self->slotIndex];
	if (!slot->bone->active) return;

	if (time < self->frames[0]) {
		switch (blend) {
		case SP_MIX_BLEND_SETUP:
			spColor_setFromColor(&slot->color, &slot->data->color);
			spColor_setFromColor(slot->darkColor, slot->data->darkColor);
			return;
		case SP_MIX_BLEND_FIRST:
			light = &slot->color;
			dark = slot->darkColor;
			setupLight = &slot->data->color;
			setupDark = slot->data->darkColor;
			spColor_addFloats(light, (setupLight->r - light->r) * alpha, (setupLight->g - light->g) * alpha, (setupLight->b - light->b) * alpha,
				(setupLight->a - light->a) * alpha);
			spColor_addFloats(dark, (setupDark->r - dark->r) * alpha, (setupDark->g - dark->g) * alpha, (setupDark->b - dark->b) * alpha, 0);
		case SP_MIX_BLEND_REPLACE:
		case SP_MIX_BLEND_ADD:
			; /* to appease compiler */
		}
		return;
	}

	if (time >= self->frames[self->framesCount - TWOCOLOR_ENTRIES]) { /* Time is after last frame */
		int i = self->framesCount;
		r = self->frames[i + TWOCOLOR_PREV_R];
		g = self->frames[i + TWOCOLOR_PREV_G];
		b = self->frames[i + TWOCOLOR_PREV_B];
		a = self->frames[i + TWOCOLOR_PREV_A];
		r2 = self->frames[i + TWOCOLOR_PREV_R2];
		g2 = self->frames[i + TWOCOLOR_PREV_G2];
		b2 = self->frames[i + TWOCOLOR_PREV_B2];
	} else {
		/* Interpolate between the previous frame and the current frame. */
		frame = binarySearch(self->frames, self->framesCount, time, TWOCOLOR_ENTRIES);

		r = self->frames[frame + TWOCOLOR_PREV_R];
		g = self->frames[frame + TWOCOLOR_PREV_G];
		b = self->frames[frame + TWOCOLOR_PREV_B];
		a = self->frames[frame + TWOCOLOR_PREV_A];
		r2 = self->frames[frame + TWOCOLOR_PREV_R2];
		g2 = self->frames[frame + TWOCOLOR_PREV_G2];
		b2 = self->frames[frame + TWOCOLOR_PREV_B2];

		frameTime = self->frames[frame];
		percent = spCurveTimeline_getCurvePercent(SUPER(self), frame / TWOCOLOR_ENTRIES - 1,
			1 - (time - frameTime) / (self->frames[frame + TWOCOLOR_PREV_TIME] - frameTime));

		r += (self->frames[frame + TWOCOLOR_R] - r) * percent;
		g += (self->frames[frame + TWOCOLOR_G] - g) * percent;
		b += (self->frames[frame + TWOCOLOR_B] - b) * percent;
		a += (self->frames[frame + TWOCOLOR_A] - a) * percent;
		r2 += (self->frames[frame + TWOCOLOR_R2] - r2) * percent;
		g2 += (self->frames[frame + TWOCOLOR_G2] - g2) * percent;
		b2 += (self->frames[frame + TWOCOLOR_B2] - b2) * percent;
	}
	if (alpha == 1) {
		spColor_setFromFloats(&slot->color, r, g, b, a);
		spColor_setFromFloats(slot->darkColor, r2, g2, b2, 1);
	} else {
		light = &slot->color;
		dark = slot->darkColor;
		if (blend == SP_MIX_BLEND_SETUP) {
			spColor_setFromColor(light, &slot->data->color);
			spColor_setFromColor(dark, slot->data->darkColor);
		}
		spColor_addFloats(light, (r - light->r) * alpha, (g - light->g) * alpha, (b - light->b) * alpha, (a - light->a) * alpha);
		spColor_addFloats(dark, (r2 - dark->r) * alpha, (g2 - dark->g) * alpha, (b2 - dark->b) * alpha, 0);
	}

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(direction);
}

int _spTwoColorTimeline_getPropertyId (const spTimeline* timeline) {
	return (SP_TIMELINE_TWOCOLOR << 24) + SUB_CAST(spTwoColorTimeline, timeline)->slotIndex;
}

spTwoColorTimeline* spTwoColorTimeline_create (int framesCount) {
	return (spTwoColorTimeline*)_spBaseTimeline_create(framesCount, SP_TIMELINE_TWOCOLOR, TWOCOLOR_ENTRIES, _spTwoColorTimeline_apply, _spTwoColorTimeline_getPropertyId);
}

void spTwoColorTimeline_setFrame (spTwoColorTimeline* self, int frameIndex, float time, float r, float g, float b, float a, float r2, float g2, float b2) {
	frameIndex *= TWOCOLOR_ENTRIES;
	self->frames[frameIndex] = time;
	self->frames[frameIndex + TWOCOLOR_R] = r;
	self->frames[frameIndex + TWOCOLOR_G] = g;
	self->frames[frameIndex + TWOCOLOR_B] = b;
	self->frames[frameIndex + TWOCOLOR_A] = a;
	self->frames[frameIndex + TWOCOLOR_R2] = r2;
	self->frames[frameIndex + TWOCOLOR_G2] = g2;
	self->frames[frameIndex + TWOCOLOR_B2] = b2;
}

/**/

static void _spSetAttachment(spAttachmentTimeline* timeline, spSkeleton* skeleton, spSlot* slot, const char* attachmentName) {
    slot->attachment = attachmentName == NULL ? NULL : spSkeleton_getAttachmentForSlotIndex(skeleton, timeline->slotIndex, attachmentName);
}

void _spAttachmentTimeline_apply (const spTimeline* timeline, spSkeleton* skeleton, float lastTime, float time,
		spEvent** firedEvents, int* eventsCount, float alpha, spMixBlend blend, spMixDirection direction) {
	const char* attachmentName;
	spAttachmentTimeline* self = (spAttachmentTimeline*)timeline;
	int frameIndex;
	spSlot* slot = skeleton->slots[self->slotIndex];
	if (!slot->bone->active) return;

	if (direction == SP_MIX_DIRECTION_OUT) {
	    if (blend == SP_MIX_BLEND_SETUP)
	        _spSetAttachment(self, skeleton, slot, slot->data->attachmentName);
		return;
	}

	if (time < self->frames[0]) {
		if (blend == SP_MIX_BLEND_SETUP || blend == SP_MIX_BLEND_FIRST) {
			_spSetAttachment(self, skeleton, slot, slot->data->attachmentName);
		}
		return;
	}

	if (time >= self->frames[self->framesCount - 1])
		frameIndex = self->framesCount - 1;
	else
		frameIndex = binarySearch1(self->frames, self->framesCount, time) - 1;

	attachmentName = self->attachmentNames[frameIndex];
	spSlot_setAttachment(skeleton->slots[self->slotIndex],
		attachmentName ? spSkeleton_getAttachmentForSlotIndex(skeleton, self->slotIndex, attachmentName) : 0);

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(alpha);
}

int _spAttachmentTimeline_getPropertyId (const spTimeline* timeline) {
	return (SP_TIMELINE_ATTACHMENT << 24) + SUB_CAST(spAttachmentTimeline, timeline)->slotIndex;
}

void _spAttachmentTimeline_dispose (spTimeline* timeline) {
	spAttachmentTimeline* self = SUB_CAST(spAttachmentTimeline, timeline);
	int i;

	_spTimeline_deinit(timeline);

	for (i = 0; i < self->framesCount; ++i)
		FREE(self->attachmentNames[i]);
	FREE(self->attachmentNames);
	FREE(self->frames);
	FREE(self);
}

spAttachmentTimeline* spAttachmentTimeline_create (int framesCount) {
	spAttachmentTimeline* self = NEW(spAttachmentTimeline);
	_spTimeline_init(SUPER(self), SP_TIMELINE_ATTACHMENT, _spAttachmentTimeline_dispose, _spAttachmentTimeline_apply, _spAttachmentTimeline_getPropertyId);

	CONST_CAST(int, self->framesCount) = framesCount;
	CONST_CAST(float*, self->frames) = CALLOC(float, framesCount);
	CONST_CAST(char**, self->attachmentNames) = CALLOC(char*, framesCount);

	return self;
}

void spAttachmentTimeline_setFrame (spAttachmentTimeline* self, int frameIndex, float time, const char* attachmentName) {
	self->frames[frameIndex] = time;

	FREE(self->attachmentNames[frameIndex]);
	if (attachmentName)
		MALLOC_STR(self->attachmentNames[frameIndex], attachmentName);
	else
		self->attachmentNames[frameIndex] = 0;
}

/**/

void _spDeformTimeline_apply (const spTimeline* timeline, spSkeleton* skeleton, float lastTime, float time, spEvent** firedEvents,
	int* eventsCount, float alpha, spMixBlend blend, spMixDirection direction
) {
	int frame, i, vertexCount;
	float percent, frameTime;
	const float* prevVertices;
	const float* nextVertices;
	float* frames;
	int framesCount;
	const float** frameVertices;
	float* deformArray;
	spDeformTimeline* self = (spDeformTimeline*)timeline;

	spSlot *slot = skeleton->slots[self->slotIndex];
	if (!slot->bone->active) return;

	if (!slot->attachment) return;
	switch (slot->attachment->type) {
		case SP_ATTACHMENT_BOUNDING_BOX:
		case SP_ATTACHMENT_CLIPPING:
		case SP_ATTACHMENT_MESH:
		case SP_ATTACHMENT_PATH: {
			spVertexAttachment* vertexAttachment = SUB_CAST(spVertexAttachment, slot->attachment);
			if (vertexAttachment->deformAttachment != SUB_CAST(spVertexAttachment, self->attachment)) return;
			break;
		}
		default:
			return;
	}

	frames = self->frames;
	framesCount = self->framesCount;
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
		spVertexAttachment* vertexAttachment = SUB_CAST(spVertexAttachment, slot->attachment);
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
					float* setupVertices = vertexAttachment->vertices;
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
			case SP_MIX_BLEND_ADD:
				; /* to appease compiler */
		}
		return;
	}

	slot->deformCount = vertexCount;
	if (time >= frames[framesCount - 1]) { /* Time is after last frame. */
		const float* lastVertices = self->frameVertices[framesCount - 1];
		if (alpha == 1) {
			if (blend == SP_MIX_BLEND_ADD) {
				spVertexAttachment* vertexAttachment = SUB_CAST(spVertexAttachment, slot->attachment);
				if (!vertexAttachment->bones) {
					/* Unweighted vertex positions, with alpha. */
					float* setupVertices = vertexAttachment->vertices;
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
			spVertexAttachment* vertexAttachment;
			switch (blend) {
				case SP_MIX_BLEND_SETUP:
					vertexAttachment = SUB_CAST(spVertexAttachment, slot->attachment);
					if (!vertexAttachment->bones) {
						/* Unweighted vertex positions, with alpha. */
						float* setupVertices = vertexAttachment->vertices;
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
				case SP_MIX_BLEND_ADD:
					vertexAttachment = SUB_CAST(spVertexAttachment, slot->attachment);
					if (!vertexAttachment->bones) {
						/* Unweighted vertex positions, with alpha. */
						float* setupVertices = vertexAttachment->vertices;
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
	frame = binarySearch(frames, framesCount, time, 1);
	prevVertices = frameVertices[frame - 1];
	nextVertices = frameVertices[frame];
	frameTime = frames[frame];
	percent = spCurveTimeline_getCurvePercent(SUPER(self), frame - 1, 1 - (time - frameTime) / (frames[frame - 1] - frameTime));

	if (alpha == 1) {
		if (blend == SP_MIX_BLEND_ADD) {
			spVertexAttachment* vertexAttachment = SUB_CAST(spVertexAttachment, slot->attachment);
			if (!vertexAttachment->bones) {
				float* setupVertices = vertexAttachment->vertices;
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
		spVertexAttachment* vertexAttachment;
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

int _spDeformTimeline_getPropertyId (const spTimeline* timeline) {
	return (SP_TIMELINE_DEFORM << 27) + SUB_CAST(spVertexAttachment, SUB_CAST(spDeformTimeline, timeline)->attachment)->id + SUB_CAST(spDeformTimeline, timeline)->slotIndex;
}

void _spDeformTimeline_dispose (spTimeline* timeline) {
	spDeformTimeline* self = SUB_CAST(spDeformTimeline, timeline);
	int i;

	_spCurveTimeline_deinit(SUPER(self));

	for (i = 0; i < self->framesCount; ++i)
		FREE(self->frameVertices[i]);
	FREE(self->frameVertices);
	FREE(self->frames);
	FREE(self);
}

spDeformTimeline* spDeformTimeline_create (int framesCount, int frameVerticesCount) {
	spDeformTimeline* self = NEW(spDeformTimeline);
	_spCurveTimeline_init(SUPER(self), SP_TIMELINE_DEFORM, framesCount, _spDeformTimeline_dispose, _spDeformTimeline_apply, _spDeformTimeline_getPropertyId);
	CONST_CAST(int, self->framesCount) = framesCount;
	CONST_CAST(float*, self->frames) = CALLOC(float, self->framesCount);
	CONST_CAST(float**, self->frameVertices) = CALLOC(float*, framesCount);
	CONST_CAST(int, self->frameVerticesCount) = frameVerticesCount;
	return self;
}

void spDeformTimeline_setFrame (spDeformTimeline* self, int frameIndex, float time, float* vertices) {
	self->frames[frameIndex] = time;

	FREE(self->frameVertices[frameIndex]);
	if (!vertices)
		self->frameVertices[frameIndex] = 0;
	else {
		self->frameVertices[frameIndex] = MALLOC(float, self->frameVerticesCount);
		memcpy(CONST_CAST(float*, self->frameVertices[frameIndex]), vertices, self->frameVerticesCount * sizeof(float));
	}
}


/**/

/** Fires events for frames > lastTime and <= time. */
void _spEventTimeline_apply (const spTimeline* timeline, spSkeleton* skeleton, float lastTime, float time, spEvent** firedEvents,
	int* eventsCount, float alpha, spMixBlend blend, spMixDirection direction
) {
	spEventTimeline* self = (spEventTimeline*)timeline;
	int frame;
	if (!firedEvents) return;

	if (lastTime > time) { /* Fire events after last time for looped animations. */
		_spEventTimeline_apply(timeline, skeleton, lastTime, (float)INT_MAX, firedEvents, eventsCount, alpha, blend, direction);
		lastTime = -1;
	} else if (lastTime >= self->frames[self->framesCount - 1]) /* Last time is after last frame. */
	return;
	if (time < self->frames[0]) return; /* Time is before first frame. */

	if (lastTime < self->frames[0])
		frame = 0;
	else {
		float frameTime;
		frame = binarySearch1(self->frames, self->framesCount, lastTime);
		frameTime = self->frames[frame];
		while (frame > 0) { /* Fire multiple events with the same frame. */
			if (self->frames[frame - 1] != frameTime) break;
			frame--;
		}
	}
	for (; frame < self->framesCount && time >= self->frames[frame]; ++frame) {
		firedEvents[*eventsCount] = self->events[frame];
		(*eventsCount)++;
	}
	UNUSED(direction);
}

int _spEventTimeline_getPropertyId (const spTimeline* timeline) {
	return SP_TIMELINE_EVENT << 24;
	UNUSED(timeline);
}

void _spEventTimeline_dispose (spTimeline* timeline) {
	spEventTimeline* self = SUB_CAST(spEventTimeline, timeline);
	int i;

	_spTimeline_deinit(timeline);

	for (i = 0; i < self->framesCount; ++i)
		spEvent_dispose(self->events[i]);
	FREE(self->events);
	FREE(self->frames);
	FREE(self);
}

spEventTimeline* spEventTimeline_create (int framesCount) {
	spEventTimeline* self = NEW(spEventTimeline);
	_spTimeline_init(SUPER(self), SP_TIMELINE_EVENT, _spEventTimeline_dispose, _spEventTimeline_apply, _spEventTimeline_getPropertyId);

	CONST_CAST(int, self->framesCount) = framesCount;
	CONST_CAST(float*, self->frames) = CALLOC(float, framesCount);
	CONST_CAST(spEvent**, self->events) = CALLOC(spEvent*, framesCount);

	return self;
}

void spEventTimeline_setFrame (spEventTimeline* self, int frameIndex, spEvent* event) {
	self->frames[frameIndex] = event->time;

	FREE(self->events[frameIndex]);
	self->events[frameIndex] = event;
}

/**/

void _spDrawOrderTimeline_apply (const spTimeline* timeline, spSkeleton* skeleton, float lastTime, float time,
	spEvent** firedEvents, int* eventsCount, float alpha, spMixBlend blend, spMixDirection direction
) {
	int i;
	int frame;
	const int* drawOrderToSetupIndex;
	spDrawOrderTimeline* self = (spDrawOrderTimeline*)timeline;

	if (direction == SP_MIX_DIRECTION_OUT ) {
		if (blend == SP_MIX_BLEND_SETUP) memcpy(skeleton->drawOrder, skeleton->slots, self->slotsCount * sizeof(spSlot*));
		return;
	}

	if (time < self->frames[0]) {
		if (blend == SP_MIX_BLEND_SETUP || blend == SP_MIX_BLEND_FIRST) memcpy(skeleton->drawOrder, skeleton->slots, self->slotsCount * sizeof(spSlot*));
		return;
	}

	if (time >= self->frames[self->framesCount - 1]) /* Time is after last frame. */
		frame = self->framesCount - 1;
	else
		frame = binarySearch1(self->frames, self->framesCount, time) - 1;

	drawOrderToSetupIndex = self->drawOrders[frame];
	if (!drawOrderToSetupIndex)
		memcpy(skeleton->drawOrder, skeleton->slots, self->slotsCount * sizeof(spSlot*));
	else {
		for (i = 0; i < self->slotsCount; ++i)
			skeleton->drawOrder[i] = skeleton->slots[drawOrderToSetupIndex[i]];
	}

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(alpha);
}

int _spDrawOrderTimeline_getPropertyId (const spTimeline* timeline) {
	return SP_TIMELINE_DRAWORDER << 24;
	UNUSED(timeline);
}

void _spDrawOrderTimeline_dispose (spTimeline* timeline) {
	spDrawOrderTimeline* self = SUB_CAST(spDrawOrderTimeline, timeline);
	int i;

	_spTimeline_deinit(timeline);

	for (i = 0; i < self->framesCount; ++i)
		FREE(self->drawOrders[i]);
	FREE(self->drawOrders);
	FREE(self->frames);
	FREE(self);
}

spDrawOrderTimeline* spDrawOrderTimeline_create (int framesCount, int slotsCount) {
	spDrawOrderTimeline* self = NEW(spDrawOrderTimeline);
	_spTimeline_init(SUPER(self), SP_TIMELINE_DRAWORDER, _spDrawOrderTimeline_dispose, _spDrawOrderTimeline_apply, _spDrawOrderTimeline_getPropertyId);

	CONST_CAST(int, self->framesCount) = framesCount;
	CONST_CAST(float*, self->frames) = CALLOC(float, framesCount);
	CONST_CAST(int**, self->drawOrders) = CALLOC(int*, framesCount);
	CONST_CAST(int, self->slotsCount) = slotsCount;

	return self;
}

void spDrawOrderTimeline_setFrame (spDrawOrderTimeline* self, int frameIndex, float time, const int* drawOrder) {
	self->frames[frameIndex] = time;

	FREE(self->drawOrders[frameIndex]);
	if (!drawOrder)
		self->drawOrders[frameIndex] = 0;
	else {
		self->drawOrders[frameIndex] = MALLOC(int, self->slotsCount);
		memcpy(CONST_CAST(int*, self->drawOrders[frameIndex]), drawOrder, self->slotsCount * sizeof(int));
	}
}

/**/

static const int IKCONSTRAINT_PREV_TIME = -6, IKCONSTRAINT_PREV_MIX = -5, IKCONSTRAINT_PREV_SOFTNESS = -4, IKCONSTRAINT_PREV_BEND_DIRECTION = -3, IKCONSTRAINT_PREV_COMPRESS = -2, IKCONSTRAINT_PREV_STRETCH = -1;
static const int IKCONSTRAINT_MIX = 1, IKCONSTRAINT_SOFTNESS = 2, IKCONSTRAINT_BEND_DIRECTION = 3, IKCONSTRAINT_COMPRESS = 4, IKCONSTRAINT_STRETCH = 5;

void _spIkConstraintTimeline_apply (const spTimeline* timeline, spSkeleton* skeleton, float lastTime, float time,
	spEvent** firedEvents, int* eventsCount, float alpha, spMixBlend blend, spMixDirection direction
) {
	int frame;
	float frameTime, percent, mix, softness;
	float *frames;
	int framesCount;
	spIkConstraint* constraint;
	spIkConstraintTimeline* self = (spIkConstraintTimeline*)timeline;

	constraint = skeleton->ikConstraints[self->ikConstraintIndex];
	if (!constraint->active) return;

	if (time < self->frames[0]) {
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
			case SP_MIX_BLEND_REPLACE:
			case SP_MIX_BLEND_ADD:
				; /* to appease compiler */
		}
		return;
	}

	frames = self->frames;
	framesCount = self->framesCount;
	if (time >= frames[framesCount - IKCONSTRAINT_ENTRIES]) { /* Time is after last frame. */
		if (blend == SP_MIX_BLEND_SETUP) {
			constraint->mix = constraint->data->mix + (frames[framesCount + IKCONSTRAINT_PREV_MIX] - constraint->data->mix) * alpha;
			constraint->softness = constraint->data->softness
				+ (frames[framesCount + IKCONSTRAINT_PREV_SOFTNESS] - constraint->data->softness) * alpha;
			if (direction == SP_MIX_DIRECTION_OUT) {
				constraint->bendDirection = constraint->data->bendDirection;
				constraint->compress = constraint->data->compress;
				constraint->stretch = constraint->data->stretch;
			} else {
				constraint->bendDirection = (int)frames[framesCount + IKCONSTRAINT_PREV_BEND_DIRECTION];
				constraint->compress = frames[framesCount + IKCONSTRAINT_PREV_COMPRESS] ? 1 : 0;
				constraint->stretch = frames[framesCount + IKCONSTRAINT_PREV_STRETCH] ? 1 : 0;
			}
		} else {
			constraint->mix += (frames[framesCount + IKCONSTRAINT_PREV_MIX] - constraint->mix) * alpha;
			constraint->softness += (frames[framesCount + IKCONSTRAINT_PREV_SOFTNESS] - constraint->softness) * alpha;
			if (direction == SP_MIX_DIRECTION_IN) {
				constraint->bendDirection = (int)frames[framesCount + IKCONSTRAINT_PREV_BEND_DIRECTION];
				constraint->compress = frames[framesCount + IKCONSTRAINT_PREV_COMPRESS] ? 1 : 0;
				constraint->stretch = frames[framesCount + IKCONSTRAINT_PREV_STRETCH] ? 1 : 0;
			}
		}
		return;
	}

	/* Interpolate between the previous frame and the current frame. */
	frame = binarySearch(self->frames, self->framesCount, time, IKCONSTRAINT_ENTRIES);
	mix = self->frames[frame + IKCONSTRAINT_PREV_MIX];
	softness = frames[frame + IKCONSTRAINT_PREV_SOFTNESS];
	frameTime = self->frames[frame];
	percent = spCurveTimeline_getCurvePercent(SUPER(self), frame / IKCONSTRAINT_ENTRIES - 1, 1 - (time - frameTime) / (self->frames[frame + IKCONSTRAINT_PREV_TIME] - frameTime));

	if (blend == SP_MIX_BLEND_SETUP) {
		constraint->mix = constraint->data->mix + (mix + (frames[frame + IKCONSTRAINT_MIX] - mix) * percent - constraint->data->mix) * alpha;
		constraint->softness = constraint->data->softness
			+ (softness + (frames[frame + IKCONSTRAINT_SOFTNESS] - softness) * percent - constraint->data->softness) * alpha;
		if (direction == SP_MIX_DIRECTION_OUT) {
			constraint->bendDirection = constraint->data->bendDirection;
			constraint->compress = constraint->data->compress;
			constraint->stretch = constraint->data->stretch;
		} else {
			constraint->bendDirection = (int)frames[frame + IKCONSTRAINT_PREV_BEND_DIRECTION];
			constraint->compress = frames[frame + IKCONSTRAINT_PREV_COMPRESS] ? 1 : 0;
			constraint->stretch = frames[frame + IKCONSTRAINT_PREV_STRETCH] ? 1 : 0;
		}
	} else {
		constraint->mix += (mix + (frames[frame + IKCONSTRAINT_MIX] - mix) * percent - constraint->mix) * alpha;
		constraint->softness += (softness + (frames[frame + IKCONSTRAINT_SOFTNESS] - softness) * percent - constraint->softness) * alpha;
		if (direction == SP_MIX_DIRECTION_IN) {
			constraint->bendDirection = (int)frames[frame + IKCONSTRAINT_PREV_BEND_DIRECTION];
			constraint->compress = frames[frame + IKCONSTRAINT_PREV_COMPRESS] ? 1 : 0;
			constraint->stretch = frames[frame + IKCONSTRAINT_PREV_STRETCH] ? 1 : 0;
		}
	}

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
}

int _spIkConstraintTimeline_getPropertyId (const spTimeline* timeline) {
	return (SP_TIMELINE_IKCONSTRAINT << 24) + SUB_CAST(spIkConstraintTimeline, timeline)->ikConstraintIndex;
}

spIkConstraintTimeline* spIkConstraintTimeline_create (int framesCount) {
	return (spIkConstraintTimeline*)_spBaseTimeline_create(framesCount, SP_TIMELINE_IKCONSTRAINT, IKCONSTRAINT_ENTRIES, _spIkConstraintTimeline_apply, _spIkConstraintTimeline_getPropertyId);
}

void spIkConstraintTimeline_setFrame (spIkConstraintTimeline* self, int frameIndex, float time, float mix, float softness,
	int bendDirection, int /*boolean*/ compress, int /*boolean*/ stretch
) {
	frameIndex *= IKCONSTRAINT_ENTRIES;
	self->frames[frameIndex] = time;
	self->frames[frameIndex + IKCONSTRAINT_MIX] = mix;
	self->frames[frameIndex + IKCONSTRAINT_SOFTNESS] = softness;
	self->frames[frameIndex + IKCONSTRAINT_BEND_DIRECTION] = (float)bendDirection;
	self->frames[frameIndex + IKCONSTRAINT_COMPRESS] = compress ? 1 : 0;
	self->frames[frameIndex + IKCONSTRAINT_STRETCH] = stretch ? 1 : 0;
}

/**/
static const int TRANSFORMCONSTRAINT_PREV_TIME = -5;
static const int TRANSFORMCONSTRAINT_PREV_ROTATE = -4;
static const int TRANSFORMCONSTRAINT_PREV_TRANSLATE = -3;
static const int TRANSFORMCONSTRAINT_PREV_SCALE = -2;
static const int TRANSFORMCONSTRAINT_PREV_SHEAR = -1;
static const int TRANSFORMCONSTRAINT_ROTATE = 1;
static const int TRANSFORMCONSTRAINT_TRANSLATE = 2;
static const int TRANSFORMCONSTRAINT_SCALE = 3;
static const int TRANSFORMCONSTRAINT_SHEAR = 4;

void _spTransformConstraintTimeline_apply (const spTimeline* timeline, spSkeleton* skeleton, float lastTime, float time,
	spEvent** firedEvents, int* eventsCount, float alpha, spMixBlend blend, spMixDirection direction
) {
	int frame;
	float frameTime, percent, rotate, translate, scale, shear;
	spTransformConstraint* constraint;
	spTransformConstraintTimeline* self = (spTransformConstraintTimeline*)timeline;
	float *frames;
	int framesCount;

	constraint = skeleton->transformConstraints[self->transformConstraintIndex];
	if (!constraint->active) return;

	if (time < self->frames[0]) {
		spTransformConstraintData* data = constraint->data;
		switch (blend) {
			case SP_MIX_BLEND_SETUP:
				constraint->rotateMix = data->rotateMix;
				constraint->translateMix = data->translateMix;
				constraint->scaleMix = data->scaleMix;
				constraint->shearMix = data->shearMix;
				return;
			case SP_MIX_BLEND_FIRST:
				constraint->rotateMix += (data->rotateMix - constraint->rotateMix) * alpha;
				constraint->translateMix += (data->translateMix - constraint->translateMix) * alpha;
				constraint->scaleMix += (data->scaleMix - constraint->scaleMix) * alpha;
				constraint->shearMix += (data->shearMix - constraint->shearMix) * alpha;
			case SP_MIX_BLEND_REPLACE:
			case SP_MIX_BLEND_ADD:
				; /* to appease compiler */
		}
		return;
		return;
	}

	frames = self->frames;
	framesCount = self->framesCount;
	if (time >= frames[framesCount - TRANSFORMCONSTRAINT_ENTRIES]) { /* Time is after last frame. */
		int i = framesCount;
		rotate = frames[i + TRANSFORMCONSTRAINT_PREV_ROTATE];
		translate = frames[i + TRANSFORMCONSTRAINT_PREV_TRANSLATE];
		scale = frames[i + TRANSFORMCONSTRAINT_PREV_SCALE];
		shear = frames[i + TRANSFORMCONSTRAINT_PREV_SHEAR];
	} else {
		/* Interpolate between the previous frame and the current frame. */
		frame = binarySearch(frames, framesCount, time, TRANSFORMCONSTRAINT_ENTRIES);
		rotate = frames[frame + TRANSFORMCONSTRAINT_PREV_ROTATE];
		translate = frames[frame + TRANSFORMCONSTRAINT_PREV_TRANSLATE];
		scale = frames[frame + TRANSFORMCONSTRAINT_PREV_SCALE];
		shear = frames[frame + TRANSFORMCONSTRAINT_PREV_SHEAR];
		frameTime = frames[frame];
		percent = spCurveTimeline_getCurvePercent(SUPER(self), frame / TRANSFORMCONSTRAINT_ENTRIES - 1,
										1 - (time - frameTime) / (frames[frame + TRANSFORMCONSTRAINT_PREV_TIME] - frameTime));

		rotate += (frames[frame + TRANSFORMCONSTRAINT_ROTATE] - rotate) * percent;
		translate += (frames[frame + TRANSFORMCONSTRAINT_TRANSLATE] - translate) * percent;
		scale += (frames[frame + TRANSFORMCONSTRAINT_SCALE] - scale) * percent;
		shear += (frames[frame + TRANSFORMCONSTRAINT_SHEAR] - shear) * percent;
	}
	if (blend == SP_MIX_BLEND_SETUP) {
		spTransformConstraintData* data = constraint->data;
		constraint->rotateMix = data->rotateMix + (rotate - data->rotateMix) * alpha;
		constraint->translateMix = data->translateMix + (translate - data->translateMix) * alpha;
		constraint->scaleMix = data->scaleMix + (scale - data->scaleMix) * alpha;
		constraint->shearMix = data->shearMix + (shear - data->shearMix) * alpha;
	} else {
		constraint->rotateMix += (rotate - constraint->rotateMix) * alpha;
		constraint->translateMix += (translate - constraint->translateMix) * alpha;
		constraint->scaleMix += (scale - constraint->scaleMix) * alpha;
		constraint->shearMix += (shear - constraint->shearMix) * alpha;
	}

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(direction);
}

int _spTransformConstraintTimeline_getPropertyId (const spTimeline* timeline) {
	return (SP_TIMELINE_TRANSFORMCONSTRAINT << 24) + SUB_CAST(spTransformConstraintTimeline, timeline)->transformConstraintIndex;
}

spTransformConstraintTimeline* spTransformConstraintTimeline_create (int framesCount) {
	return (spTransformConstraintTimeline*)_spBaseTimeline_create(framesCount, SP_TIMELINE_TRANSFORMCONSTRAINT,
		TRANSFORMCONSTRAINT_ENTRIES, _spTransformConstraintTimeline_apply, _spTransformConstraintTimeline_getPropertyId);
}

void spTransformConstraintTimeline_setFrame (spTransformConstraintTimeline* self, int frameIndex, float time, float rotateMix,
	float translateMix, float scaleMix, float shearMix
) {
	frameIndex *= TRANSFORMCONSTRAINT_ENTRIES;
	self->frames[frameIndex] = time;
	self->frames[frameIndex + TRANSFORMCONSTRAINT_ROTATE] = rotateMix;
	self->frames[frameIndex + TRANSFORMCONSTRAINT_TRANSLATE] = translateMix;
	self->frames[frameIndex + TRANSFORMCONSTRAINT_SCALE] = scaleMix;
	self->frames[frameIndex + TRANSFORMCONSTRAINT_SHEAR] = shearMix;
}

/**/

static const int PATHCONSTRAINTPOSITION_PREV_TIME = -2;
static const int PATHCONSTRAINTPOSITION_PREV_VALUE = -1;
static const int PATHCONSTRAINTPOSITION_VALUE = 1;

void _spPathConstraintPositionTimeline_apply(const spTimeline* timeline, spSkeleton* skeleton, float lastTime, float time,
	spEvent** firedEvents, int* eventsCount, float alpha, spMixBlend blend, spMixDirection direction
) {
	int frame;
	float frameTime, percent, position;
	spPathConstraint* constraint;
	spPathConstraintPositionTimeline* self = (spPathConstraintPositionTimeline*)timeline;
	float* frames;
	int framesCount;

	constraint = skeleton->pathConstraints[self->pathConstraintIndex];
	if (!constraint->active) return;

	if (time < self->frames[0]) {
		switch (blend) {
			case SP_MIX_BLEND_SETUP:
				constraint->position = constraint->data->position;
				return;
			case SP_MIX_BLEND_FIRST:
				constraint->position += (constraint->data->position - constraint->position) * alpha;
			case SP_MIX_BLEND_REPLACE:
			case SP_MIX_BLEND_ADD:
				; /* to appease compiler */
		}
		return;
	}

	frames = self->frames;
	framesCount = self->framesCount;
	if (time >= frames[framesCount - PATHCONSTRAINTPOSITION_ENTRIES]) /* Time is after last frame. */
		position = frames[framesCount + PATHCONSTRAINTPOSITION_PREV_VALUE];
	else {
		/* Interpolate between the previous frame and the current frame. */
		frame = binarySearch(frames, framesCount, time, PATHCONSTRAINTPOSITION_ENTRIES);
		position = frames[frame + PATHCONSTRAINTPOSITION_PREV_VALUE];
		frameTime = frames[frame];
		percent = spCurveTimeline_getCurvePercent(SUPER(self), frame / PATHCONSTRAINTPOSITION_ENTRIES - 1,
										1 - (time - frameTime) / (frames[frame + PATHCONSTRAINTPOSITION_PREV_TIME] - frameTime));

		position += (frames[frame + PATHCONSTRAINTPOSITION_VALUE] - position) * percent;
	}
	if (blend == SP_MIX_BLEND_SETUP)
		constraint->position = constraint->data->position + (position - constraint->data->position) * alpha;
	else
		constraint->position += (position - constraint->position) * alpha;

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(direction);
}

int _spPathConstraintPositionTimeline_getPropertyId (const spTimeline* timeline) {
	return (SP_TIMELINE_PATHCONSTRAINTPOSITION << 24) + SUB_CAST(spPathConstraintPositionTimeline, timeline)->pathConstraintIndex;
}

spPathConstraintPositionTimeline* spPathConstraintPositionTimeline_create (int framesCount) {
	return (spPathConstraintPositionTimeline*)_spBaseTimeline_create(framesCount, SP_TIMELINE_PATHCONSTRAINTPOSITION,
		PATHCONSTRAINTPOSITION_ENTRIES, _spPathConstraintPositionTimeline_apply, _spPathConstraintPositionTimeline_getPropertyId);
}

void spPathConstraintPositionTimeline_setFrame (spPathConstraintPositionTimeline* self, int frameIndex, float time, float value) {
	frameIndex *= PATHCONSTRAINTPOSITION_ENTRIES;
	self->frames[frameIndex] = time;
	self->frames[frameIndex + PATHCONSTRAINTPOSITION_VALUE] = value;
}

/**/
static const int PATHCONSTRAINTSPACING_PREV_TIME = -2;
static const int PATHCONSTRAINTSPACING_PREV_VALUE = -1;
static const int PATHCONSTRAINTSPACING_VALUE = 1;

void _spPathConstraintSpacingTimeline_apply(const spTimeline* timeline, spSkeleton* skeleton, float lastTime, float time,
	spEvent** firedEvents, int* eventsCount, float alpha, spMixBlend blend, spMixDirection direction
) {
	int frame;
	float frameTime, percent, spacing;
	spPathConstraint* constraint;
	spPathConstraintSpacingTimeline* self = (spPathConstraintSpacingTimeline*)timeline;
	float* frames;
	int framesCount;

	constraint = skeleton->pathConstraints[self->pathConstraintIndex];
	if (!constraint->active) return;

	if (time < self->frames[0]) {
		switch (blend) {
			case SP_MIX_BLEND_SETUP:
				constraint->spacing = constraint->data->spacing;
				return;
			case SP_MIX_BLEND_FIRST:
				constraint->spacing += (constraint->data->spacing - constraint->spacing) * alpha;
			case SP_MIX_BLEND_REPLACE:
			case SP_MIX_BLEND_ADD:
				; /* to appease compiler */
		}
		return;
	}

	frames = self->frames;
	framesCount = self->framesCount;
	if (time >= frames[framesCount - PATHCONSTRAINTSPACING_ENTRIES]) /* Time is after last frame. */
		spacing = frames[framesCount + PATHCONSTRAINTSPACING_PREV_VALUE];
	else {
		/* Interpolate between the previous frame and the current frame. */
		frame = binarySearch(frames, framesCount, time, PATHCONSTRAINTSPACING_ENTRIES);
		spacing = frames[frame + PATHCONSTRAINTSPACING_PREV_VALUE];
		frameTime = frames[frame];
		percent = spCurveTimeline_getCurvePercent(SUPER(self), frame / PATHCONSTRAINTSPACING_ENTRIES - 1,
										1 - (time - frameTime) / (frames[frame + PATHCONSTRAINTSPACING_PREV_TIME] - frameTime));

		spacing += (frames[frame + PATHCONSTRAINTSPACING_VALUE] - spacing) * percent;
	}

	if (blend == SP_MIX_BLEND_SETUP)
		constraint->spacing = constraint->data->spacing + (spacing - constraint->data->spacing) * alpha;
	else
		constraint->spacing += (spacing - constraint->spacing) * alpha;

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(direction);
}

int _spPathConstraintSpacingTimeline_getPropertyId (const spTimeline* timeline) {
	return (SP_TIMELINE_PATHCONSTRAINTSPACING << 24) + SUB_CAST(spPathConstraintSpacingTimeline, timeline)->pathConstraintIndex;
}

spPathConstraintSpacingTimeline* spPathConstraintSpacingTimeline_create (int framesCount) {
	return (spPathConstraintSpacingTimeline*)_spBaseTimeline_create(framesCount, SP_TIMELINE_PATHCONSTRAINTSPACING,
		PATHCONSTRAINTSPACING_ENTRIES, _spPathConstraintSpacingTimeline_apply, _spPathConstraintSpacingTimeline_getPropertyId);
}

void spPathConstraintSpacingTimeline_setFrame (spPathConstraintSpacingTimeline* self, int frameIndex, float time, float value) {
	frameIndex *= PATHCONSTRAINTSPACING_ENTRIES;
	self->frames[frameIndex] = time;
	self->frames[frameIndex + PATHCONSTRAINTSPACING_VALUE] = value;
}

/**/

static const int PATHCONSTRAINTMIX_PREV_TIME = -3;
static const int PATHCONSTRAINTMIX_PREV_ROTATE = -2;
static const int PATHCONSTRAINTMIX_PREV_TRANSLATE = -1;
static const int PATHCONSTRAINTMIX_ROTATE = 1;
static const int PATHCONSTRAINTMIX_TRANSLATE = 2;

void _spPathConstraintMixTimeline_apply(const spTimeline* timeline, spSkeleton* skeleton, float lastTime, float time,
	spEvent** firedEvents, int* eventsCount, float alpha, spMixBlend blend, spMixDirection direction
) {
	int frame;
	float frameTime, percent, rotate, translate;
	spPathConstraint* constraint;
	spPathConstraintMixTimeline* self = (spPathConstraintMixTimeline*)timeline;
	float* frames;
	int framesCount;

	constraint = skeleton->pathConstraints[self->pathConstraintIndex];
	if (!constraint->active) return;

	if (time < self->frames[0]) {
		switch (blend) {
			case SP_MIX_BLEND_SETUP:
				constraint->rotateMix = constraint->data->rotateMix;
				constraint->translateMix = constraint->data->translateMix;
				return;
			case SP_MIX_BLEND_FIRST:
				constraint->rotateMix += (constraint->data->rotateMix - constraint->rotateMix) * alpha;
				constraint->translateMix += (constraint->data->translateMix - constraint->translateMix) * alpha;
			case SP_MIX_BLEND_REPLACE:
			case SP_MIX_BLEND_ADD:
				; /* to appease compiler */
		}
		return;
	}

	frames = self->frames;
	framesCount = self->framesCount;
	if (time >= frames[framesCount - PATHCONSTRAINTMIX_ENTRIES]) { /* Time is after last frame. */
		rotate = frames[framesCount + PATHCONSTRAINTMIX_PREV_ROTATE];
		translate = frames[framesCount + PATHCONSTRAINTMIX_PREV_TRANSLATE];
	} else {
		/* Interpolate between the previous frame and the current frame. */
		frame = binarySearch(frames, framesCount, time, PATHCONSTRAINTMIX_ENTRIES);
		rotate = frames[frame + PATHCONSTRAINTMIX_PREV_ROTATE];
		translate = frames[frame + PATHCONSTRAINTMIX_PREV_TRANSLATE];
		frameTime = frames[frame];
		percent = spCurveTimeline_getCurvePercent(SUPER(self), frame / PATHCONSTRAINTMIX_ENTRIES - 1,
										1 - (time - frameTime) / (frames[frame + PATHCONSTRAINTMIX_PREV_TIME] - frameTime));

		rotate += (frames[frame + PATHCONSTRAINTMIX_ROTATE] - rotate) * percent;
		translate += (frames[frame + PATHCONSTRAINTMIX_TRANSLATE] - translate) * percent;
	}

	if (blend == SP_MIX_BLEND_SETUP) {
		constraint->rotateMix = constraint->data->rotateMix + (rotate - constraint->data->rotateMix) * alpha;
		constraint->translateMix = constraint->data->translateMix + (translate - constraint->data->translateMix) * alpha;
	} else {
		constraint->rotateMix += (rotate - constraint->rotateMix) * alpha;
		constraint->translateMix += (translate - constraint->translateMix) * alpha;
	}

	UNUSED(lastTime);
	UNUSED(firedEvents);
	UNUSED(eventsCount);
	UNUSED(direction);
}

int _spPathConstraintMixTimeline_getPropertyId (const spTimeline* timeline) {
	return (SP_TIMELINE_PATHCONSTRAINTMIX << 24) + SUB_CAST(spPathConstraintMixTimeline, timeline)->pathConstraintIndex;
}

spPathConstraintMixTimeline* spPathConstraintMixTimeline_create (int framesCount) {
	return (spPathConstraintMixTimeline*)_spBaseTimeline_create(framesCount, SP_TIMELINE_PATHCONSTRAINTMIX,
		PATHCONSTRAINTMIX_ENTRIES, _spPathConstraintMixTimeline_apply, _spPathConstraintMixTimeline_getPropertyId);
}

void spPathConstraintMixTimeline_setFrame (spPathConstraintMixTimeline* self, int frameIndex, float time, float rotateMix, float translateMix) {
	frameIndex *= PATHCONSTRAINTMIX_ENTRIES;
	self->frames[frameIndex] = time;
	self->frames[frameIndex + PATHCONSTRAINTMIX_ROTATE] = rotateMix;
	self->frames[frameIndex + PATHCONSTRAINTMIX_TRANSLATE] = translateMix;
}
