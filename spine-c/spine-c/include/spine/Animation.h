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

#ifndef SPINE_ANIMATION_H_
#define SPINE_ANIMATION_H_

#include <spine/dll.h>
#include <spine/Event.h>
#include <spine/Attachment.h>
#include <spine/VertexAttachment.h>
#include <spine/Sequence.h>
#include <spine/Array.h>
#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

typedef struct spTimeline spTimeline;
struct spSkeleton;
typedef uint64_t spPropertyId;

_SP_ARRAY_DECLARE_TYPE(spPropertyIdArray, spPropertyId)

_SP_ARRAY_DECLARE_TYPE(spTimelineArray, spTimeline*)

typedef struct spAnimation {
	const char *const name;
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

SP_API spAnimation *spAnimation_create(const char *name, spTimelineArray *timelines, float duration);

SP_API void spAnimation_dispose(spAnimation *self);

SP_API int /*bool*/ spAnimation_hasTimeline(spAnimation *self, spPropertyId *ids, int idsCount);

/** Poses the skeleton at the specified time for this animation.
 * @param lastTime The last time the animation was applied.
 * @param events Any triggered events are added. May be null.*/
SP_API void
spAnimation_apply(const spAnimation *self, struct spSkeleton *skeleton, float lastTime, float time, int loop,
				  spEvent **events, int *eventsCount, float alpha, spMixBlend blend, spMixDirection direction);

/**/
typedef enum {
	SP_TIMELINE_ATTACHMENT,
	SP_TIMELINE_ALPHA,
	SP_TIMELINE_PATHCONSTRAINTPOSITION,
	SP_TIMELINE_PATHCONSTRAINTSPACING,
	SP_TIMELINE_ROTATE,
	SP_TIMELINE_SCALEX,
	SP_TIMELINE_SCALEY,
	SP_TIMELINE_SHEARX,
	SP_TIMELINE_SHEARY,
	SP_TIMELINE_TRANSLATEX,
	SP_TIMELINE_TRANSLATEY,
	SP_TIMELINE_SCALE,
	SP_TIMELINE_SHEAR,
	SP_TIMELINE_TRANSLATE,
	SP_TIMELINE_DEFORM,
	SP_TIMELINE_SEQUENCE,
	SP_TIMELINE_IKCONSTRAINT,
	SP_TIMELINE_PATHCONSTRAINTMIX,
	SP_TIMELINE_RGB2,
	SP_TIMELINE_RGBA2,
	SP_TIMELINE_RGBA,
	SP_TIMELINE_RGB,
	SP_TIMELINE_TRANSFORMCONSTRAINT,
	SP_TIMELINE_DRAWORDER,
	SP_TIMELINE_EVENT
} spTimelineType;

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
	SP_PROPERTY_PATHCONSTRAINT_MIX = 1 << 18,
	SP_PROPERTY_SEQUENCE = 1 << 19
} spProperty;

#define SP_MAX_PROPERTY_IDS 3

typedef struct _spTimelineVtable {
	void (*apply)(spTimeline *self, struct spSkeleton *skeleton, float lastTime, float time, spEvent **firedEvents,
				  int *eventsCount, float alpha, spMixBlend blend, spMixDirection direction);

	void (*dispose)(spTimeline *self);

	void
	(*setBezier)(spTimeline *self, int bezier, int frame, float value, float time1, float value1, float cx1, float cy1,
				 float cx2, float cy2, float time2, float value2);
} _spTimelineVtable;

struct spTimeline {
	_spTimelineVtable vtable;
	spPropertyId propertyIds[SP_MAX_PROPERTY_IDS];
	int propertyIdsCount;
	spFloatArray *frames;
	int frameCount;
	int frameEntries;
	spTimelineType type;
};

SP_API void spTimeline_dispose(spTimeline *self);

SP_API void
spTimeline_apply(spTimeline *self, struct spSkeleton *skeleton, float lastTime, float time, spEvent **firedEvents,
				 int *eventsCount, float alpha, spMixBlend blend, spMixDirection direction);

SP_API void
spTimeline_setBezier(spTimeline *self, int bezier, int frame, float value, float time1, float value1, float cx1,
					 float cy1, float cx2, float cy2, float time2, float value2);

SP_API float spTimeline_getDuration(const spTimeline *self);

/**/

typedef struct spCurveTimeline {
	spTimeline super;
	spFloatArray *curves; /* type, x, y, ... */
} spCurveTimeline;

SP_API void spCurveTimeline_setLinear(spCurveTimeline *self, int frameIndex);

SP_API void spCurveTimeline_setStepped(spCurveTimeline *self, int frameIndex);

/* Sets the control handle positions for an interpolation bezier curve used to transition from this keyframe to the next.
 * cx1 and cx2 are from 0 to 1, representing the percent of time between the two keyframes. cy1 and cy2 are the percent of
 * the difference between the keyframe's values. */
SP_API void spCurveTimeline_setCurve(spCurveTimeline *self, int frameIndex, float cx1, float cy1, float cx2, float cy2);

SP_API float spCurveTimeline_getCurvePercent(const spCurveTimeline *self, int frameIndex, float percent);

typedef struct spCurveTimeline spCurveTimeline1;

SP_API void spCurveTimeline1_setFrame(spCurveTimeline1 *self, int frame, float time, float value);

SP_API float spCurveTimeline1_getCurveValue(spCurveTimeline1 *self, float time);

typedef struct spCurveTimeline spCurveTimeline2;

SP_API void spCurveTimeline2_setFrame(spCurveTimeline1 *self, int frame, float time, float value1, float value2);

/**/

typedef struct spRotateTimeline {
	spCurveTimeline1 super;
	int boneIndex;
} spRotateTimeline;

SP_API spRotateTimeline *spRotateTimeline_create(int frameCount, int bezierCount, int boneIndex);

SP_API void spRotateTimeline_setFrame(spRotateTimeline *self, int frameIndex, float time, float angle);

/**/

typedef struct spTranslateTimeline {
	spCurveTimeline2 super;
	int boneIndex;
} spTranslateTimeline;

SP_API spTranslateTimeline *spTranslateTimeline_create(int frameCount, int bezierCount, int boneIndex);

SP_API void spTranslateTimeline_setFrame(spTranslateTimeline *self, int frameIndex, float time, float x, float y);

/**/

typedef struct spTranslateXTimeline {
	spCurveTimeline1 super;
	int boneIndex;
} spTranslateXTimeline;

SP_API spTranslateXTimeline *spTranslateXTimeline_create(int frameCount, int bezierCount, int boneIndex);

SP_API void spTranslateXTimeline_setFrame(spTranslateXTimeline *self, int frame, float time, float x);

/**/

typedef struct spTranslateYTimeline {
	spCurveTimeline1 super;
	int boneIndex;
} spTranslateYTimeline;

SP_API spTranslateYTimeline *spTranslateYTimeline_create(int frameCount, int bezierCount, int boneIndex);

SP_API void spTranslateYTimeline_setFrame(spTranslateYTimeline *self, int frame, float time, float y);

/**/

typedef struct spScaleTimeline {
	spCurveTimeline2 super;
	int boneIndex;
} spScaleTimeline;

SP_API spScaleTimeline *spScaleTimeline_create(int frameCount, int bezierCount, int boneIndex);

SP_API void spScaleTimeline_setFrame(spScaleTimeline *self, int frameIndex, float time, float x, float y);

/**/

typedef struct spScaleXTimeline {
	spCurveTimeline1 super;
	int boneIndex;
} spScaleXTimeline;

SP_API spScaleXTimeline *spScaleXTimeline_create(int frameCount, int bezierCount, int boneIndex);

SP_API void spScaleXTimeline_setFrame(spScaleXTimeline *self, int frame, float time, float x);

/**/

typedef struct spScaleYTimeline {
	spCurveTimeline1 super;
	int boneIndex;
} spScaleYTimeline;

SP_API spScaleYTimeline *spScaleYTimeline_create(int frameCount, int bezierCount, int boneIndex);

SP_API void spScaleYTimeline_setFrame(spScaleYTimeline *self, int frame, float time, float y);

/**/

typedef struct spShearTimeline {
	spCurveTimeline2 super;
	int boneIndex;
} spShearTimeline;

SP_API spShearTimeline *spShearTimeline_create(int frameCount, int bezierCount, int boneIndex);

SP_API void spShearTimeline_setFrame(spShearTimeline *self, int frameIndex, float time, float x, float y);

/**/

typedef struct spShearXTimeline {
	spCurveTimeline1 super;
	int boneIndex;
} spShearXTimeline;

SP_API spShearXTimeline *spShearXTimeline_create(int frameCount, int bezierCount, int boneIndex);

SP_API void spShearXTimeline_setFrame(spShearXTimeline *self, int frame, float time, float x);

/**/

typedef struct spShearYTimeline {
	spCurveTimeline1 super;
	int boneIndex;
} spShearYTimeline;

SP_API spShearYTimeline *spShearYTimeline_create(int frameCount, int bezierCount, int boneIndex);

SP_API void spShearYTimeline_setFrame(spShearYTimeline *self, int frame, float time, float x);

/**/

typedef struct spRGBATimeline {
	spCurveTimeline2 super;
	int slotIndex;
} spRGBATimeline;

SP_API spRGBATimeline *spRGBATimeline_create(int framesCount, int bezierCount, int slotIndex);

SP_API void
spRGBATimeline_setFrame(spRGBATimeline *self, int frameIndex, float time, float r, float g, float b, float a);

/**/

typedef struct spRGBTimeline {
	spCurveTimeline2 super;
	int slotIndex;
} spRGBTimeline;

SP_API spRGBTimeline *spRGBTimeline_create(int framesCount, int bezierCount, int slotIndex);

SP_API void spRGBTimeline_setFrame(spRGBTimeline *self, int frameIndex, float time, float r, float g, float b);

/**/

typedef struct spAlphaTimeline {
	spCurveTimeline1 super;
	int slotIndex;
} spAlphaTimeline;

SP_API spAlphaTimeline *spAlphaTimeline_create(int frameCount, int bezierCount, int slotIndex);

SP_API void spAlphaTimeline_setFrame(spAlphaTimeline *self, int frame, float time, float x);

/**/

typedef struct spRGBA2Timeline {
	spCurveTimeline super;
	int slotIndex;
} spRGBA2Timeline;

SP_API spRGBA2Timeline *spRGBA2Timeline_create(int framesCount, int bezierCount, int slotIndex);

SP_API void
spRGBA2Timeline_setFrame(spRGBA2Timeline *self, int frameIndex, float time, float r, float g, float b, float a,
						 float r2, float g2, float b2);

/**/

typedef struct spRGB2Timeline {
	spCurveTimeline super;
	int slotIndex;
} spRGB2Timeline;

SP_API spRGB2Timeline *spRGB2Timeline_create(int framesCount, int bezierCount, int slotIndex);

SP_API void
spRGB2Timeline_setFrame(spRGB2Timeline *self, int frameIndex, float time, float r, float g, float b, float r2, float g2,
						float b2);

/**/

typedef struct spAttachmentTimeline {
	spTimeline super;
	int slotIndex;
	const char **const attachmentNames;
} spAttachmentTimeline;

SP_API spAttachmentTimeline *spAttachmentTimeline_create(int framesCount, int SlotIndex);

/* @param attachmentName May be 0. */
SP_API void
spAttachmentTimeline_setFrame(spAttachmentTimeline *self, int frameIndex, float time, const char *attachmentName);

/**/

typedef struct spDeformTimeline {
	spCurveTimeline super;
	int const frameVerticesCount;
	const float **const frameVertices;
	int slotIndex;
	spAttachment *attachment;
} spDeformTimeline;

SP_API spDeformTimeline *
spDeformTimeline_create(int framesCount, int frameVerticesCount, int bezierCount, int slotIndex,
						spVertexAttachment *attachment);

SP_API void spDeformTimeline_setFrame(spDeformTimeline *self, int frameIndex, float time, float *vertices);

/**/

typedef struct spSequenceTimeline {
	spTimeline super;
	int slotIndex;
	spAttachment *attachment;
} spSequenceTimeline;

SP_API spSequenceTimeline *spSequenceTimeline_create(int framesCount, int slotIndex, spAttachment *attachment);

SP_API void spSequenceTimeline_setFrame(spSequenceTimeline *self, int frameIndex, float time, int mode, int index, float delay);

/**/

/**/

typedef struct spEventTimeline {
	spTimeline super;
	spEvent **const events;
} spEventTimeline;

SP_API spEventTimeline *spEventTimeline_create(int framesCount);

SP_API void spEventTimeline_setFrame(spEventTimeline *self, int frameIndex, spEvent *event);

/**/

typedef struct spDrawOrderTimeline {
	spTimeline super;
	const int **const drawOrders;
	int const slotsCount;
} spDrawOrderTimeline;

SP_API spDrawOrderTimeline *spDrawOrderTimeline_create(int framesCount, int slotsCount);

SP_API void spDrawOrderTimeline_setFrame(spDrawOrderTimeline *self, int frameIndex, float time, const int *drawOrder);

/**/

typedef struct spIkConstraintTimeline {
	spCurveTimeline super;
	int ikConstraintIndex;
} spIkConstraintTimeline;

SP_API spIkConstraintTimeline *
spIkConstraintTimeline_create(int framesCount, int bezierCount, int transformConstraintIndex);

SP_API void
spIkConstraintTimeline_setFrame(spIkConstraintTimeline *self, int frameIndex, float time, float mix, float softness,
								int bendDirection, int /*boolean*/ compress, int /**boolean**/ stretch);

/**/

typedef struct spTransformConstraintTimeline {
	spCurveTimeline super;
	int transformConstraintIndex;
} spTransformConstraintTimeline;

SP_API spTransformConstraintTimeline *
spTransformConstraintTimeline_create(int framesCount, int bezierCount, int transformConstraintIndex);

SP_API void
spTransformConstraintTimeline_setFrame(spTransformConstraintTimeline *self, int frameIndex, float time, float mixRotate,
									   float mixX, float mixY, float mixScaleX, float mixScaleY, float mixShearY);

/**/

typedef struct spPathConstraintPositionTimeline {
	spCurveTimeline super;
	int pathConstraintIndex;
} spPathConstraintPositionTimeline;

SP_API spPathConstraintPositionTimeline *
spPathConstraintPositionTimeline_create(int framesCount, int bezierCount, int pathConstraintIndex);

SP_API void
spPathConstraintPositionTimeline_setFrame(spPathConstraintPositionTimeline *self, int frameIndex, float time,
										  float value);

/**/

typedef struct spPathConstraintSpacingTimeline {
	spCurveTimeline super;
	int pathConstraintIndex;
} spPathConstraintSpacingTimeline;

SP_API spPathConstraintSpacingTimeline *
spPathConstraintSpacingTimeline_create(int framesCount, int bezierCount, int pathConstraintIndex);

SP_API void spPathConstraintSpacingTimeline_setFrame(spPathConstraintSpacingTimeline *self, int frameIndex, float time,
													 float value);

/**/

typedef struct spPathConstraintMixTimeline {
	spCurveTimeline super;
	int pathConstraintIndex;
} spPathConstraintMixTimeline;

SP_API spPathConstraintMixTimeline *
spPathConstraintMixTimeline_create(int framesCount, int bezierCount, int pathConstraintIndex);

SP_API void
spPathConstraintMixTimeline_setFrame(spPathConstraintMixTimeline *self, int frameIndex, float time, float mixRotate,
									 float mixX, float mixY);

/**/

#ifdef __cplusplus
}
#endif

#endif /* SPINE_ANIMATION_H_ */
