#include <spine/Animation.h>
#include <math.h>
#include <spine/util.h>

Animation* Animation_create () {
	Animation* this = calloc(1, sizeof(Animation));
	return this;
}

void Animation_dispose (Animation* this) {
	int i;
	for (i = 0; i < this->timelineCount; ++i)
		Timeline_dispose(this->timelines[i]);
	FREE(this->timelines)
	FREE(this)
}

void Animation_apply (const Animation* this, Skeleton* skeleton, float time, int/*bool*/loop) {
	if (loop && this->duration) time = fmod(time, this->duration);

	int i, n = this->timelineCount;
	for (i = 0; i < n; ++i)
		Timeline_apply(this->timelines[i], skeleton, time, 1);
}

void Animation_mix (const Animation* this, Skeleton* skeleton, float time, int/*bool*/loop, float alpha) {
	if (loop && this->duration) time = fmod(time, this->duration);

	int i, n = this->timelineCount;
	for (i = 0; i < n; ++i)
		Timeline_apply(this->timelines[i], skeleton, time, alpha);
}

/**/

void _Timeline_init (Timeline* timeline) {
}

void _Timeline_deinit (Timeline* timeline) {
}

void Timeline_dispose (Timeline* this) {
	this->_dispose(this);
}

void Timeline_apply (const Timeline *this, Skeleton* skeleton, float time, float alpha) {
	this->_apply(this, skeleton, time, alpha);
}

/**/

static const float CURVE_LINEAR = 0;
static const float CURVE_STEPPED = -1;
static const int CURVE_SEGMENTS = 10;

void _CurveTimeline_init (CurveTimeline* this, int frameCount) {
	_Timeline_init(&this->super);
	this->curves = calloc(1, sizeof(float) * (frameCount - 1) * 6);
}

void _CurveTimeline_deinit (CurveTimeline* this) {
	_Timeline_deinit(&this->super);
	FREE(this->curves)
}

void CurveTimeline_setLinear (CurveTimeline* this, int frameIndex) {
	this->curves[frameIndex * 6] = CURVE_LINEAR;
}

void CurveTimeline_setStepped (CurveTimeline* this, int frameIndex) {
	this->curves[frameIndex * 6] = CURVE_STEPPED;
}

void CurveTimeline_setCurve (CurveTimeline* this, int frameIndex, float cx1, float cy1, float cx2, float cy2) {
	float subdiv_step = 1.0f / CURVE_SEGMENTS;
	float subdiv_step2 = subdiv_step * subdiv_step;
	float subdiv_step3 = subdiv_step2 * subdiv_step;
	float pre1 = 3 * subdiv_step;
	float pre2 = 3 * subdiv_step2;
	float pre4 = 6 * subdiv_step2;
	float pre5 = 6 * subdiv_step3;
	float tmp1x = -cx1 * 2 + cx2;
	float tmp1y = -cy1 * 2 + cy2;
	float tmp2x = (cx1 - cx2) * 3 + 1;
	float tmp2y = (cy1 - cy2) * 3 + 1;
	int i = frameIndex * 6;
	this->curves[i] = cx1 * pre1 + tmp1x * pre2 + tmp2x * subdiv_step3;
	this->curves[i + 1] = cy1 * pre1 + tmp1y * pre2 + tmp2y * subdiv_step3;
	this->curves[i + 2] = tmp1x * pre4 + tmp2x * pre5;
	this->curves[i + 3] = tmp1y * pre4 + tmp2y * pre5;
	this->curves[i + 4] = tmp2x * pre5;
	this->curves[i + 5] = tmp2y * pre5;
}

float CurveTimeline_getCurvePercent (CurveTimeline* this, int frameIndex, float percent) {
	int curveIndex = frameIndex * 6;
	float dfx = this->curves[curveIndex];
	if (dfx == CURVE_LINEAR) return percent;
	if (dfx == CURVE_STEPPED) return 0;
	float dfy = this->curves[curveIndex + 1];
	float ddfx = this->curves[curveIndex + 2];
	float ddfy = this->curves[curveIndex + 3];
	float dddfx = this->curves[curveIndex + 4];
	float dddfy = this->curves[curveIndex + 5];
	float x = dfx, y = dfy;
	int i = CURVE_SEGMENTS - 2;
	while (1) {
		if (x >= percent) {
			float lastX = x - dfx;
			float lastY = y - dfy;
			return lastY + (y - lastY) * (percent - lastX) / (x - lastX);
		}
		if (i == 0) break;
		i--;
		dfx += ddfx;
		dfy += ddfy;
		ddfx += dddfx;
		ddfy += dddfy;
		x += dfx;
		y += dfy;
	}
	return y + (1 - y) * (percent - x) / (1 - x); /* Last point is 1,1. */
}

/* @param target After the first and before the last entry. */
static int binarySearch (float *values, int valuesLength, float target, int step) {
	int low = 0;
	int high = valuesLength / step - 2;
	if (high == 0) return step;
	int current = high >> 1;
	while (1) {
		if (values[(current + 1) * step] <= target)
			low = current + 1;
		else
			high = current;
		if (low == high) return (low + 1) * step;
		current = (low + high) >> 1;
	}
	return 0;
}

/*static int linearSearch (float *values, int valuesLength, float target, int step) {
 int i, last = valuesLength - step;
 for (i = 0; i <= last; i += step) {
 if (values[i] <= target) continue;
 return i;
 }
 return -1;
 }*/

/**/

void _BaseTimeline_dispose (Timeline* timeline) {
	struct BaseTimeline* this = (struct BaseTimeline*)timeline;
	_CurveTimeline_deinit(&this->super);
	FREE(this->frames);
	FREE(this);
}

/* Many timelines have structure identical to struct BaseTimeline and extend CurveTimeline. **/
struct BaseTimeline* _BaseTimeline_create (int frameCount, int frameSize) {
	struct BaseTimeline* this = calloc(1, sizeof(struct BaseTimeline));
	_CurveTimeline_init(&this->super, frameCount);
	((Timeline*)this)->_dispose = _BaseTimeline_dispose;

	CAST(int, this->frameCount) = frameCount;
	CAST(float*, this->frames) = calloc(1, sizeof(float) * frameCount * frameSize);

	return this;
}

/**/

static const int ROTATE_LAST_FRAME_TIME = -2;
static const int ROTATE_FRAME_VALUE = 1;

void _RotateTimeline_apply (const Timeline* timeline, Skeleton* skeleton, float time, float alpha) {
	RotateTimeline* this = (RotateTimeline*)timeline;

	if (time < this->frames[0]) return; /* Time is before first frame. */

	Bone *bone = skeleton->bones[this->boneIndex];

	if (time >= this->frames[this->frameCount - 2]) { /* Time is after last frame. */
		float amount = bone->data->rotation + this->frames[this->frameCount - 1] - bone->rotation;
		while (amount > 180)
			amount -= 360;
		while (amount < -180)
			amount += 360;
		bone->rotation += amount * alpha;
		return;
	}

	/* Interpolate between the last frame and the current frame. */
	int frameIndex = binarySearch(this->frames, this->frameCount, time, 2);
	float lastFrameValue = this->frames[frameIndex - 1];
	float frameTime = this->frames[frameIndex];
	float percent = 1 - (time - frameTime) / (this->frames[frameIndex + ROTATE_LAST_FRAME_TIME] - frameTime);
	percent = CurveTimeline_getCurvePercent(&this->super, frameIndex / 2 - 1, percent < 0 ? 0 : (percent > 1 ? 1 : percent));

	float amount = this->frames[frameIndex + ROTATE_FRAME_VALUE] - lastFrameValue;
	while (amount > 180)
		amount -= 360;
	while (amount < -180)
		amount += 360;
	amount = bone->data->rotation + (lastFrameValue + amount * percent) - bone->rotation;
	while (amount > 180)
		amount -= 360;
	while (amount < -180)
		amount += 360;
	bone->rotation += amount * alpha;
}

RotateTimeline* RotateTimeline_create (int frameCount) {
	RotateTimeline* this = _BaseTimeline_create(frameCount, 2);
	((Timeline*)this)->_apply = _RotateTimeline_apply;
	return this;
}

void RotateTimeline_setFrame (RotateTimeline* this, int frameIndex, float time, float angle) {
	frameIndex *= 2;
	this->frames[frameIndex] = time;
	this->frames[frameIndex + 1] = angle;
}

/**/

static const int TRANSLATE_LAST_FRAME_TIME = -3;
static const int TRANSLATE_FRAME_X = 1;
static const int TRANSLATE_FRAME_Y = 2;

void _TranslateTimeline_apply (const Timeline* timeline, Skeleton* skeleton, float time, float alpha) {
	TranslateTimeline* this = (TranslateTimeline*)timeline;

	if (time < this->frames[0]) return; /* Time is before first frame. */

	Bone *bone = skeleton->bones[this->boneIndex];

	if (time >= this->frames[this->frameCount - 3]) { /* Time is after last frame. */
		bone->x += (bone->data->x + this->frames[this->frameCount - 2] - bone->x) * alpha;
		bone->y += (bone->data->y + this->frames[this->frameCount - 1] - bone->y) * alpha;
		return;
	}

	/* Interpolate between the last frame and the current frame. */
	int frameIndex = binarySearch(this->frames, this->frameCount, time, 3);
	float lastFrameX = this->frames[frameIndex - 2];
	float lastFrameY = this->frames[frameIndex - 1];
	float frameTime = this->frames[frameIndex];
	float percent = 1 - (time - frameTime) / (this->frames[frameIndex + TRANSLATE_LAST_FRAME_TIME] - frameTime);
	percent = CurveTimeline_getCurvePercent(&this->super, frameIndex / 3 - 1, percent < 0 ? 0 : (percent > 1 ? 1 : percent));

	bone->x += (bone->data->x + lastFrameX + (this->frames[frameIndex + TRANSLATE_FRAME_X] - lastFrameX) * percent - bone->x)
			* alpha;
	bone->y += (bone->data->y + lastFrameY + (this->frames[frameIndex + TRANSLATE_FRAME_Y] - lastFrameY) * percent - bone->y)
			* alpha;
}

TranslateTimeline* TranslateTimeline_create (int frameCount) {
	TranslateTimeline* this = _BaseTimeline_create(frameCount, 3);
	((Timeline*)this)->_apply = _TranslateTimeline_apply;
	return this;
}

void TranslateTimeline_setFrame (TranslateTimeline* this, int frameIndex, float time, float x, float y) {
	frameIndex *= 3;
	this->frames[frameIndex] = time;
	this->frames[frameIndex + 1] = x;
	this->frames[frameIndex + 2] = y;
}

/**/

void _ScaleTimeline_apply (const Timeline* timeline, Skeleton* skeleton, float time, float alpha) {
	ScaleTimeline* this = (ScaleTimeline*)timeline;

	if (time < this->frames[0]) return; /* Time is before first frame. */

	Bone *bone = skeleton->bones[this->boneIndex];
	if (time >= this->frames[this->frameCount - 3]) { /* Time is after last frame. */
		bone->scaleX += (bone->data->scaleX - 1 + this->frames[this->frameCount - 2] - bone->scaleX) * alpha;
		bone->scaleY += (bone->data->scaleY - 1 + this->frames[this->frameCount - 1] - bone->scaleY) * alpha;
		return;
	}

	/* Interpolate between the last frame and the current frame. */
	int frameIndex = binarySearch(this->frames, this->frameCount, time, 3);
	float lastFrameX = this->frames[frameIndex - 2];
	float lastFrameY = this->frames[frameIndex - 1];
	float frameTime = this->frames[frameIndex];
	float percent = 1 - (time - frameTime) / (this->frames[frameIndex + TRANSLATE_LAST_FRAME_TIME] - frameTime);
	percent = CurveTimeline_getCurvePercent(&this->super, frameIndex / 3 - 1, percent < 0 ? 0 : (percent > 1 ? 1 : percent));

	bone->scaleX += (bone->data->scaleX - 1 + lastFrameX + (this->frames[frameIndex + TRANSLATE_FRAME_X] - lastFrameX) * percent
			- bone->scaleX) * alpha;
	bone->scaleY += (bone->data->scaleY - 1 + lastFrameY + (this->frames[frameIndex + TRANSLATE_FRAME_Y] - lastFrameY) * percent
			- bone->scaleY) * alpha;
}

ScaleTimeline* ScaleTimeline_create (int frameCount) {
	ScaleTimeline* this = _BaseTimeline_create(frameCount, 3);
	((Timeline*)this)->_apply = _ScaleTimeline_apply;
	return this;
}

void ScaleTimeline_setFrame (ScaleTimeline* this, int frameIndex, float time, float x, float y) {
	TranslateTimeline_setFrame(this, frameIndex, time, x, y);
}

/**/

static const int COLOR_LAST_FRAME_TIME = -5;
static const int COLOR_FRAME_R = 1;
static const int COLOR_FRAME_G = 2;
static const int COLOR_FRAME_B = 3;
static const int COLOR_FRAME_A = 4;

void _ColorTimeline_apply (const Timeline* timeline, Skeleton* skeleton, float time, float alpha) {
	ColorTimeline* this = (ColorTimeline*)timeline;

	if (time < this->frames[0]) return; /* Time is before first frame. */

	Slot *slot = skeleton->slots[this->slotIndex];

	if (time >= this->frames[this->frameCount - 5]) { /* Time is after last frame. */
		int i = this->frameCount - 1;
		slot->r = this->frames[i - 3];
		slot->g = this->frames[i - 2];
		slot->b = this->frames[i - 1];
		slot->a = this->frames[i];
		return;
	}

	/* Interpolate between the last frame and the current frame. */
	int frameIndex = binarySearch(this->frames, this->frameCount, time, 5);
	float lastFrameR = this->frames[frameIndex - 4];
	float lastFrameG = this->frames[frameIndex - 3];
	float lastFrameB = this->frames[frameIndex - 2];
	float lastFrameA = this->frames[frameIndex - 1];
	float frameTime = this->frames[frameIndex];
	float percent = 1 - (time - frameTime) / (this->frames[frameIndex + COLOR_LAST_FRAME_TIME] - frameTime);
	percent = CurveTimeline_getCurvePercent(&this->super, frameIndex / 5 - 1, percent < 0 ? 0 : (percent > 1 ? 1 : percent));

	float r = lastFrameR + (this->frames[frameIndex + COLOR_FRAME_R] - lastFrameR) * percent;
	float g = lastFrameG + (this->frames[frameIndex + COLOR_FRAME_G] - lastFrameG) * percent;
	float b = lastFrameB + (this->frames[frameIndex + COLOR_FRAME_B] - lastFrameB) * percent;
	float a = lastFrameA + (this->frames[frameIndex + COLOR_FRAME_A] - lastFrameA) * percent;
	if (alpha < 1) {
		slot->r += (r - slot->r) * alpha;
		slot->g += (g - slot->g) * alpha;
		slot->b += (b - slot->b) * alpha;
		slot->a += (a - slot->a) * alpha;
	} else {
		slot->r = r;
		slot->g = g;
		slot->b = b;
		slot->a = a;
	}
}

ColorTimeline* ColorTimeline_create (int frameCount) {
	ColorTimeline* this = (ColorTimeline*)_BaseTimeline_create(frameCount, 5);
	((Timeline*)this)->_apply = _ColorTimeline_apply;
	return this;
}

void ColorTimeline_setFrame (ColorTimeline* this, int frameIndex, float time, float r, float g, float b, float a) {
	frameIndex *= 5;
	this->frames[frameIndex] = time;
	this->frames[frameIndex + 1] = r;
	this->frames[frameIndex + 2] = g;
	this->frames[frameIndex + 3] = b;
	this->frames[frameIndex + 4] = a;
}

/**/

void _AttachmentTimeline_apply (const Timeline* timeline, Skeleton* skeleton, float time, float alpha) {
	AttachmentTimeline* this = (AttachmentTimeline*)timeline;

	if (time < this->frames[0]) return; /* Time is before first frame. */

	int frameIndex;
	if (time >= this->frames[this->frameCount - 1]) /* Time is after last frame. */
		frameIndex = this->frameCount - 1;
	else
		frameIndex = binarySearch(this->frames, this->frameCount, time, 1) - 1;

	const char* attachmentName = this->attachmentNames[frameIndex];
	Slot_setAttachment(skeleton->slots[this->slotIndex],
			attachmentName ? Skeleton_getAttachmentForSlotIndex(skeleton, this->slotIndex, attachmentName) : 0);
}

void _AttachmentTimeline_dispose (Timeline* timeline) {
	_Timeline_deinit(timeline);
	AttachmentTimeline* this = (AttachmentTimeline*)timeline;

	int i;
	for (i = 0; i < this->frameCount; ++i)
		FREE(this->attachmentNames[i])
	FREE(this->attachmentNames)

	FREE(this)
}

AttachmentTimeline* AttachmentTimeline_create (int frameCount) {
	AttachmentTimeline* this = calloc(1, sizeof(AttachmentTimeline));
	_Timeline_init(&this->super);
	((Timeline*)this)->_dispose = _AttachmentTimeline_dispose;
	((Timeline*)this)->_apply = _AttachmentTimeline_apply;
	CAST(char*, this->attachmentNames) = calloc(1, sizeof(char*) * frameCount);

	CAST(int, this->frameCount) = frameCount;
	CAST(float*, this->frames) = calloc(1, sizeof(float) * frameCount);

	return this;
}

void AttachmentTimeline_setFrame (AttachmentTimeline* this, int frameIndex, float time, const char* attachmentName) {
	this->frames[frameIndex] = time;
	FREE(this->attachmentNames[frameIndex])
	if (attachmentName)
		MALLOC_STR(this->attachmentNames[frameIndex], attachmentName)
	else
		this->attachmentNames[frameIndex] = 0;
}
