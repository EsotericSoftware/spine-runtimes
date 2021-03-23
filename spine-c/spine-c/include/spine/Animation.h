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

#ifndef SPINE_ANIMATION_H_
#define SPINE_ANIMATION_H_

#include <spine/dll.h>
#include <spine/Event.h>
#include <spine/Attachment.h>
#include <spine/Array.h>

#ifdef __cplusplus
extern "C" {
#endif

typedef struct spTimeline spTimeline;
struct spSkeleton;
typedef unsigned long long spPropertyId;

_SP_ARRAY_DECLARE_TYPE(spPropertyIdArray, spPropertyId)
_SP_ARRAY_DECLARE_TYPE(spTimelineArray, spTimeline*)

typedef struct spAnimation {
	const char* const name;
	float duration;

	spTimelineArray *timelines;
    spPropertyIdArray *timelineIds;
} spAnimation;

typedef enum {
	SP_MIX_BLEND_SETUP,
	SP_MIX_BLEND_FIRST,
	SP_MIX_BLEND_REPLACE,
	SP_MIX_BLEND_ADD
} spMixBlend;

typedef enum {
	SP_MIX_DIRECTION_IN,
	SP_MIX_DIRECTION_OUT
} spMixDirection;

SP_API spAnimation* spAnimation_create (const char* name, spTimelineArray* timelines);
SP_API void spAnimation_dispose (spAnimation* self);
SP_API int /*bool*/ spAnimation_hasTimeline(spAnimation* self, spPropertyId* ids, int idsCount);

/** Poses the skeleton at the specified time for this animation.
 * @param lastTime The last time the animation was applied.
 * @param events Any triggered events are added. May be null.*/
SP_API void spAnimation_apply (const spAnimation* self, struct spSkeleton* skeleton, float lastTime, float time, int loop,
		spEvent** events, int* eventsCount, float alpha, spMixBlend blend, spMixDirection direction);

/**/

typedef enum {
    SP_PROPERTY_ROTATE = 1 << 0,
    SP_PROPERTY_X = 1 << 1,
    SP_PROPERTY_Y = 1 << 2,
    SP_PROPERTY_SCALEX = 1 << 3,
    SP_PROPERTY_SCALEY = 1 << 4,
    SP_PROPERTY_SHEARX = 1 << 5,
    SP_PROPERTY_SHEARY = 1 << 6,
    SP_PROPERTY_RGB = 1 << 7,
    SP_PROPERTY_ALPHA = 1 << 8,
    SP_PROPERTY_RGB2 = 1 << 9,
    SP_PROPERTY_ATTACHMENT = 1 << 10,
    SP_PROPERTY_DEFORM = 1 << 11,
    SP_PROPERTY_EVENT = 1 << 12,
    SP_PROPERTY_DRAWORDER = 1 << 13,
    SP_PROPERTY_IKCONSTRAINT = 1 << 14,
    SP_PROPERTY_TRANSFORMCONSTRAINT = 1 << 15,
    SP_PROPERTY_PATHCONSTRAINT_POSITION = 1 << 16,
    SP_PROPERTY_PATHCONSTRAINT_SPACING = 1 << 17,
    SP_PROPERTY_PATHCONSTRAINT_MIX = 1 << 18
} spProperty;

#define SP_MAX_PROPERTY_IDS 2

typedef struct _spTimelineVtable {
    void (*apply) (const spTimeline* self, spSkeleton* skeleton, float lastTime, float time, spEvent** firedEvents,
                   int* eventsCount, float alpha, spMixBlend blend, spMixDirection direction);
    void (*dispose) (spTimeline* self);
} _spTimelineVtable;

struct spTimeline {
	_spTimelineVtable vtable;
	spPropertyId propertyIds[SP_MAX_PROPERTY_IDS];
	int propertyIdsCount;
	spFloatArray *frames;
	int frameEntries;
};

SP_API void spTimeline_dispose (spTimeline* self);
SP_API void spTimeline_apply (const spTimeline* self, struct spSkeleton* skeleton, float lastTime, float time, spEvent** firedEvents,
		int* eventsCount, float alpha, spMixBlend blend, spMixDirection direction);
SP_API int spTimeline_getFrameCount (const spTimeline* self);
SP_API float spTimeline_getDuration (const spTimeline* self);

/**/

typedef struct spCurveTimeline {
	spTimeline super;
	spFloatArray* curves; /* type, x, y, ... */
} spCurveTimeline;

SP_API void spCurveTimeline_setLinear (spCurveTimeline* self, int frameIndex);
SP_API void spCurveTimeline_setStepped (spCurveTimeline* self, int frameIndex);

/* Sets the control handle positions for an interpolation bezier curve used to transition from this keyframe to the next.
 * cx1 and cx2 are from 0 to 1, representing the percent of time between the two keyframes. cy1 and cy2 are the percent of
 * the difference between the keyframe's values. */
SP_API void spCurveTimeline_setCurve (spCurveTimeline* self, int frameIndex, float cx1, float cy1, float cx2, float cy2);
SP_API float spCurveTimeline_getCurvePercent (const spCurveTimeline* self, int frameIndex, float percent);

typedef struct spCurveTimeline spCurveTimeline1;

SP_API void spCurveTimeline1_setFrame(spCurveTimeline1* self, int frame, float time, float value);
SP_API float spCurveTimeline1_getCurveValue(spCurveTimeline1* self, float time);

typedef struct spCurveTimeline spCurveTimeline2;

SP_API void spCurveTimeline2_setFrame(spCurveTimeline1* self, int frame, float time, float value1, float value2);

/**/

typedef struct spRotateTimeline {
    spCurveTimeline1 super;
    int boneIndex;
} spRotateTimeline;

SP_API spRotateTimeline* spRotateTimeline_create (int frameCount, int bezierCount, int boneIndex);

SP_API void spRotateTimeline_setFrame (spRotateTimeline* self, int frameIndex, float time, float angle);

/**/

typedef struct spTranslateTimeline {
    spCurveTimeline2 super;
    int boneIndex;
} spTranslateTimeline;

SP_API spTranslateTimeline* spTranslateTimeline_create (int frameCount, int bezierCount, int boneIndex);

SP_API void spTranslateTimeline_setFrame (spTranslateTimeline* self, int frameIndex, float time, float x, float y);

/**/

typedef struct spTranslateXTimeline {
    spCurveTimeline1 super;
    int boneIndex;
} spTranslateXTimeline;

SP_API spTranslateXTimeline* spTranslateXTimeline_create (int frameCount, int bezierCount, int boneIndex);

SP_API void spTranslateXTimeline_setFrame (spTranslateXTimeline* self, int frame, float time, float x);

/**/

typedef struct spTranslateYTimeline {
    spCurveTimeline1 super;
    int boneIndex;
} spTranslateYTimeline;

SP_API spTranslateYTimeline* spTranslateYTimeline_create (int frameCount, int bezierCount, int boneIndex);

SP_API void spTranslateYTimeline_setFrame (spTranslateYTimeline* self, int frame, float time, float y);

/**/

typedef struct spScaleTimeline {
    spCurveTimeline2 super;
    int boneIndex;
} spScaleTimeline;

SP_API spScaleTimeline* spScaleTimeline_create (int frameCount, int bezierCount, int boneIndex);

SP_API void spScaleTimeline_setFrame (spScaleTimeline* self, int frameIndex, float time, float x, float y);

/**/

typedef struct spScaleXTimeline {
    spCurveTimeline1 super;
    int boneIndex;
} spScaleXTimeline;

SP_API spScaleXTimeline* spScaleXTimeline_create (int frameCount, int bezierCount, int boneIndex);

SP_API void spScaleXTimeline_setFrame (spScaleXTimeline* self, int frame, float time, float x);

/**/

typedef struct spScaleYTimeline {
    spCurveTimeline1 super;
    int boneIndex;
} spScaleYTimeline;

SP_API spScaleYTimeline* spScaleYTimeline_create (int frameCount, int bezierCount, int boneIndex);

SP_API void spScaleYTimeline_setFrame (spScaleYTimeline* self, int frame, float time, float y);

/**/

typedef struct spBaseTimeline spShearTimeline;

SP_API spShearTimeline* spShearTimeline_create (int framesCount);

SP_API void spShearTimeline_setFrame (spShearTimeline* self, int frameIndex, float time, float x, float y);

/**/

static const int COLOR_ENTRIES = 5;

typedef struct spColorTimeline {
	spCurveTimeline super;
	int const framesCount;
	float* const frames; /* time, r, g, b, a, ... */
	int slotIndex;
} spColorTimeline;

SP_API spColorTimeline* spColorTimeline_create (int framesCount);

SP_API void spColorTimeline_setFrame (spColorTimeline* self, int frameIndex, float time, float r, float g, float b, float a);

/**/

static const int TWOCOLOR_ENTRIES = 8;

typedef struct spTwoColorTimeline {
	spCurveTimeline super;
	int const framesCount;
	float* const frames; /* time, r, g, b, a, ... */
	int slotIndex;
} spTwoColorTimeline;

SP_API spTwoColorTimeline* spTwoColorTimeline_create (int framesCount);

SP_API void spTwoColorTimeline_setFrame (spTwoColorTimeline* self, int frameIndex, float time, float r, float g, float b, float a, float r2, float g2, float b2);

/**/

typedef struct spAttachmentTimeline {
	spTimeline super;
	int const framesCount;
	float* const frames; /* time, ... */
	int slotIndex;
	const char** const attachmentNames;
} spAttachmentTimeline;

SP_API spAttachmentTimeline* spAttachmentTimeline_create (int framesCount);

/* @param attachmentName May be 0. */
SP_API void spAttachmentTimeline_setFrame (spAttachmentTimeline* self, int frameIndex, float time, const char* attachmentName);

/**/

typedef struct spEventTimeline {
	spTimeline super;
	int const framesCount;
	float* const frames; /* time, ... */
	spEvent** const events;
} spEventTimeline;

SP_API spEventTimeline* spEventTimeline_create (int framesCount);

SP_API void spEventTimeline_setFrame (spEventTimeline* self, int frameIndex, spEvent* event);

/**/

typedef struct spDrawOrderTimeline {
	spTimeline super;
	int const framesCount;
	float* const frames; /* time, ... */
	const int** const drawOrders;
	int const slotsCount;
} spDrawOrderTimeline;

SP_API spDrawOrderTimeline* spDrawOrderTimeline_create (int framesCount, int slotsCount);

SP_API void spDrawOrderTimeline_setFrame (spDrawOrderTimeline* self, int frameIndex, float time, const int* drawOrder);

/**/

typedef struct spDeformTimeline {
	spCurveTimeline super;
	int const framesCount;
	float* const frames; /* time, ... */
	int const frameVerticesCount;
	const float** const frameVertices;
	int slotIndex;
	spAttachment* attachment;
} spDeformTimeline;

SP_API spDeformTimeline* spDeformTimeline_create (int framesCount, int frameVerticesCount);

SP_API void spDeformTimeline_setFrame (spDeformTimeline* self, int frameIndex, float time, float* vertices);

/**/

static const int IKCONSTRAINT_ENTRIES = 6;

typedef struct spIkConstraintTimeline {
	spCurveTimeline super;
	int const framesCount;
	float* const frames; /* time, mix, bendDirection, ... */
	int ikConstraintIndex;
} spIkConstraintTimeline;

SP_API spIkConstraintTimeline* spIkConstraintTimeline_create (int framesCount);

SP_API void spIkConstraintTimeline_setFrame (spIkConstraintTimeline* self, int frameIndex, float time, float mix, float softness, int bendDirection, int /*boolean*/ compress, int /**boolean**/ stretch);

/**/

static const int TRANSFORMCONSTRAINT_ENTRIES = 5;

typedef struct spTransformConstraintTimeline {
	spCurveTimeline super;
	int const framesCount;
	float* const frames; /* time, rotate mix, translate mix, scale mix, shear mix, ... */
	int transformConstraintIndex;
} spTransformConstraintTimeline;

SP_API spTransformConstraintTimeline* spTransformConstraintTimeline_create (int framesCount);

SP_API void spTransformConstraintTimeline_setFrame (spTransformConstraintTimeline* self, int frameIndex, float time, float rotateMix, float translateMix, float scaleMix, float shearMix);

/**/

static const int PATHCONSTRAINTPOSITION_ENTRIES = 2;

typedef struct spPathConstraintPositionTimeline {
	spCurveTimeline super;
	int const framesCount;
	float* const frames; /* time, rotate mix, translate mix, scale mix, shear mix, ... */
	int pathConstraintIndex;
} spPathConstraintPositionTimeline;

SP_API spPathConstraintPositionTimeline* spPathConstraintPositionTimeline_create (int framesCount);

SP_API void spPathConstraintPositionTimeline_setFrame (spPathConstraintPositionTimeline* self, int frameIndex, float time, float value);

/**/

static const int PATHCONSTRAINTSPACING_ENTRIES = 2;

typedef struct spPathConstraintSpacingTimeline {
	spCurveTimeline super;
	int const framesCount;
	float* const frames; /* time, rotate mix, translate mix, scale mix, shear mix, ... */
	int pathConstraintIndex;
} spPathConstraintSpacingTimeline;

SP_API spPathConstraintSpacingTimeline* spPathConstraintSpacingTimeline_create (int framesCount);

SP_API void spPathConstraintSpacingTimeline_setFrame (spPathConstraintSpacingTimeline* self, int frameIndex, float time, float value);

/**/

static const int PATHCONSTRAINTMIX_ENTRIES = 3;

typedef struct spPathConstraintMixTimeline {
	spCurveTimeline super;
	int const framesCount;
	float* const frames; /* time, rotate mix, translate mix, scale mix, shear mix, ... */
	int pathConstraintIndex;
} spPathConstraintMixTimeline;

SP_API spPathConstraintMixTimeline* spPathConstraintMixTimeline_create (int framesCount);

SP_API void spPathConstraintMixTimeline_setFrame (spPathConstraintMixTimeline* self, int frameIndex, float time, float rotateMix, float translateMix);

/**/

#ifdef __cplusplus
}
#endif

#endif /* SPINE_ANIMATION_H_ */
