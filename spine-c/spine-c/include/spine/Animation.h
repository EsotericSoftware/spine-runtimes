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

#ifdef __cplusplus
extern "C" {
#endif

typedef struct spTimeline spTimeline;
struct spSkeleton;

typedef struct spAnimation {
	const char* const name;
	float duration;

	int timelinesCount;
	spTimeline** timelines;
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

SP_API spAnimation* spAnimation_create (const char* name, int timelinesCount);
SP_API void spAnimation_dispose (spAnimation* self);

/** Poses the skeleton at the specified time for this animation.
 * @param lastTime The last time the animation was applied.
 * @param events Any triggered events are added. May be null.*/
SP_API void spAnimation_apply (const spAnimation* self, struct spSkeleton* skeleton, float lastTime, float time, int loop,
		spEvent** events, int* eventsCount, float alpha, spMixBlend blend, spMixDirection direction);

/**/

typedef enum {
	SP_TIMELINE_ROTATE,
	SP_TIMELINE_TRANSLATE,
	SP_TIMELINE_SCALE,
	SP_TIMELINE_SHEAR,
	SP_TIMELINE_ATTACHMENT,
	SP_TIMELINE_COLOR,
	SP_TIMELINE_DEFORM,
	SP_TIMELINE_EVENT,
	SP_TIMELINE_DRAWORDER,
	SP_TIMELINE_IKCONSTRAINT,
	SP_TIMELINE_TRANSFORMCONSTRAINT,
	SP_TIMELINE_PATHCONSTRAINTPOSITION,
	SP_TIMELINE_PATHCONSTRAINTSPACING,
	SP_TIMELINE_PATHCONSTRAINTMIX,
	SP_TIMELINE_TWOCOLOR
} spTimelineType;

struct spTimeline {
	const spTimelineType type;
	const void* const vtable;
};

SP_API void spTimeline_dispose (spTimeline* self);
SP_API void spTimeline_apply (const spTimeline* self, struct spSkeleton* skeleton, float lastTime, float time, spEvent** firedEvents,
		int* eventsCount, float alpha, spMixBlend blend, spMixDirection direction);
SP_API int spTimeline_getPropertyId (const spTimeline* self);

/**/

typedef struct spCurveTimeline {
	spTimeline super;
	float* curves; /* type, x, y, ... */
} spCurveTimeline;

SP_API void spCurveTimeline_setLinear (spCurveTimeline* self, int frameIndex);
SP_API void spCurveTimeline_setStepped (spCurveTimeline* self, int frameIndex);

/* Sets the control handle positions for an interpolation bezier curve used to transition from this keyframe to the next.
 * cx1 and cx2 are from 0 to 1, representing the percent of time between the two keyframes. cy1 and cy2 are the percent of
 * the difference between the keyframe's values. */
SP_API void spCurveTimeline_setCurve (spCurveTimeline* self, int frameIndex, float cx1, float cy1, float cx2, float cy2);
SP_API float spCurveTimeline_getCurvePercent (const spCurveTimeline* self, int frameIndex, float percent);

/**/

typedef struct spBaseTimeline {
	spCurveTimeline super;
	int const framesCount;
	float* const frames; /* time, angle, ... for rotate. time, x, y, ... for translate and scale. */
	int boneIndex;
} spBaseTimeline;

/**/

static const int ROTATE_PREV_TIME = -2, ROTATE_PREV_ROTATION = -1;
static const int ROTATE_ROTATION = 1;
static const int ROTATE_ENTRIES = 2;

typedef struct spBaseTimeline spRotateTimeline;

SP_API spRotateTimeline* spRotateTimeline_create (int framesCount);

SP_API void spRotateTimeline_setFrame (spRotateTimeline* self, int frameIndex, float time, float angle);

/**/

static const int TRANSLATE_ENTRIES = 3;

typedef struct spBaseTimeline spTranslateTimeline;

SP_API spTranslateTimeline* spTranslateTimeline_create (int framesCount);

SP_API void spTranslateTimeline_setFrame (spTranslateTimeline* self, int frameIndex, float time, float x, float y);

/**/

typedef struct spBaseTimeline spScaleTimeline;

SP_API spScaleTimeline* spScaleTimeline_create (int framesCount);

SP_API void spScaleTimeline_setFrame (spScaleTimeline* self, int frameIndex, float time, float x, float y);

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
