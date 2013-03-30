#ifndef SPINE_ANIMATION_H_
#define SPINE_ANIMATION_H_

#include <spine/Skeleton.h>

#ifdef __cplusplus
namespace spine {
extern "C" {
#endif

typedef struct Timeline Timeline;

typedef struct {
	int timelineCount;
	Timeline** timelines;

	float duration;
} Animation;

Animation* Animation_create (int timelineCount);
void Animation_dispose (Animation* animation);

void Animation_apply (const Animation* animation, Skeleton* skeleton, float time, int/*bool*/loop);
void Animation_mix (const Animation* animation, Skeleton* skeleton, float time, int/*bool*/loop, float alpha);

/**/

struct Timeline {
	void (*_apply) (const Timeline* timeline, Skeleton* skeleton, float time, float alpha);
	void (*_dispose) (Timeline* timeline);
};

void Timeline_dispose (Timeline* timeline);
void Timeline_apply (const Timeline* timeline, Skeleton* skeleton, float time, float alpha);

/**/

typedef struct {
	Timeline super;
	float* curves; /* dfx, dfy, ddfx, ddfy, dddfx, dddfy, ... */
} CurveTimeline;

void CurveTimeline_setLinear (CurveTimeline* timeline, int frameIndex);
void CurveTimeline_setStepped (CurveTimeline* timeline, int frameIndex);

/* Sets the control handle positions for an interpolation bezier curve used to transition from this keyframe to the next.
 * cx1 and cx2 are from 0 to 1, representing the percent of time between the two keyframes. cy1 and cy2 are the percent of
 * the difference between the keyframe's values. */
void CurveTimeline_setCurve (CurveTimeline* timeline, int frameIndex, float cx1, float cy1, float cx2, float cy2);
float CurveTimeline_getCurvePercent (CurveTimeline* timeline, int frameIndex, float percent);

/**/

typedef struct BaseTimeline {
	CurveTimeline super;
	int const frameCount;
	float* const frames; /* time, angle, ... for rotate. time, x, y, ... for translate and scale. */
	int boneIndex;
} RotateTimeline;

RotateTimeline* RotateTimeline_create (int frameCount);

void RotateTimeline_setFrame (RotateTimeline* timeline, int frameIndex, float time, float angle);

/**/

typedef struct BaseTimeline TranslateTimeline;

TranslateTimeline* TranslateTimeline_create (int frameCount);

void TranslateTimeline_setFrame (TranslateTimeline* timeline, int frameIndex, float time, float x, float y);

/**/

typedef struct BaseTimeline ScaleTimeline;

ScaleTimeline* ScaleTimeline_create (int frameCount);

void ScaleTimeline_setFrame (ScaleTimeline* timeline, int frameIndex, float time, float x, float y);

/**/

typedef struct {
	CurveTimeline super;
	int const frameCount;
	float* const frames; /* time, r, g, b, a, ... */
	int slotIndex;
} ColorTimeline;

ColorTimeline* ColorTimeline_create (int frameCount);

void ColorTimeline_setFrame (ColorTimeline* timeline, int frameIndex, float time, float r, float g, float b, float a);

/**/

typedef struct {
	Timeline super;
	int const frameCount;
	float* const frames; /* time, ... */
	int slotIndex;
	const char** const attachmentNames;
} AttachmentTimeline;

AttachmentTimeline* AttachmentTimeline_create (int frameCount);

/* @param attachmentName May be 0. */
void AttachmentTimeline_setFrame (AttachmentTimeline* timeline, int frameIndex, float time, const char* attachmentName);

#ifdef __cplusplus
}
}
#endif

#endif /* SPINE_ANIMATION_H_ */
