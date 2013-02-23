#include <cstring>
#include <iostream>
#include <stdexcept>
#include <math.h>
#include <spine/Animation.h>
#include <spine/Bone.h>
#include <spine/Slot.h>
#include <spine/BaseSkeleton.h>
#include <spine/BoneData.h>

using std::string;
using std::vector;

namespace spine {

Animation::Animation (const vector<Timeline*> &timelines, float duration) :
				timelines(timelines),
				duration(duration) {
}

Animation::~Animation()
{
  for (std::vector<Timeline*>::iterator iter = timelines.begin(); iter != timelines.end(); ++iter)
  {
    delete *iter;
  }
}

void Animation::apply (BaseSkeleton *skeleton, float time, bool loop) {
	if (!skeleton) throw std::invalid_argument("skeleton cannot be null.");

	if (loop && duration) time = fmodf(time, duration);

	for (int i = 0, n = timelines.size(); i < n; i++)
		timelines[i]->apply(skeleton, time, 1);
}

//

static const float LINEAR = 0;
static const float STEPPED = -1;
static const int BEZIER_SEGMENTS = 10;

CurveTimeline::CurveTimeline (int keyframeCount) :
				curves(new float[(keyframeCount - 1) * 6]) {
	memset(curves, 0, sizeof(float) * (keyframeCount - 1) * 6);
}

CurveTimeline::~CurveTimeline () {
	delete[] curves;
}

void CurveTimeline::setLinear (int keyframeIndex) {
	curves[keyframeIndex * 6] = LINEAR;
}

void CurveTimeline::setStepped (int keyframeIndex) {
	curves[keyframeIndex * 6] = STEPPED;
}

void CurveTimeline::setCurve (int keyframeIndex, float cx1, float cy1, float cx2, float cy2) {
	float subdiv_step = 1.0f / BEZIER_SEGMENTS;
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
	int i = keyframeIndex * 6;
	curves[i] = cx1 * pre1 + tmp1x * pre2 + tmp2x * subdiv_step3;
	curves[i + 1] = cy1 * pre1 + tmp1y * pre2 + tmp2y * subdiv_step3;
	curves[i + 2] = tmp1x * pre4 + tmp2x * pre5;
	curves[i + 3] = tmp1y * pre4 + tmp2y * pre5;
	curves[i + 4] = tmp2x * pre5;
	curves[i + 5] = tmp2y * pre5;
}

float CurveTimeline::getCurvePercent (int keyframeIndex, float percent) {
	int curveIndex = keyframeIndex * 6;
	float dfx = curves[curveIndex];
	if (dfx == LINEAR) return percent;
	if (dfx == STEPPED) return 0;
	float dfy = curves[curveIndex + 1];
	float ddfx = curves[curveIndex + 2];
	float ddfy = curves[curveIndex + 3];
	float dddfx = curves[curveIndex + 4];
	float dddfy = curves[curveIndex + 5];
	float x = dfx, y = dfy;
	int i = BEZIER_SEGMENTS - 2;
	while (true) {
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
	return y + (1 - y) * (percent - x) / (1 - x); // Last point is 1,1.
}

//

/** @param target After the first and before the last entry. */
static int binarySearch (float *values, int valuesLength, float target, int step) {
	int low = 0;
	int high = valuesLength / step - 2;
	if (high == 0) return step;
	int current = high >> 1;
	while (true) {
		if (values[(current + 1) * step] <= target)
			low = current + 1;
		else
			high = current;
		if (low == high) return (low + 1) * step;
		current = (low + high) >> 1;
	}
	return 0;
}

/*
 static int linearSearch (float *values, int valuesLength, float target, int step) {
 for (int i = 0, last = valuesLength - step; i <= last; i += step) {
 if (values[i] <= target) continue;
 return i;
 }
 return -1;
 }
 */

static const int ROTATE_LAST_FRAME_TIME = -2;
static const int ROTATE_FRAME_VALUE = 1;

RotateTimeline::RotateTimeline (int keyframeCount) :
				CurveTimeline(keyframeCount),
				framesLength(keyframeCount * 2),
				frames(new float[framesLength]),
				boneIndex(0) {
	memset(frames, 0, sizeof(float) * framesLength);
}

RotateTimeline::~RotateTimeline () {
	delete[] frames;
}

float RotateTimeline::getDuration () {
	return frames[framesLength - 2];
}

int RotateTimeline::getKeyframeCount () {
	return framesLength / 2;
}

void RotateTimeline::setKeyframe (int keyframeIndex, float time, float value) {
	keyframeIndex *= 2;
	frames[keyframeIndex] = time;
	frames[keyframeIndex + 1] = value;
}

void RotateTimeline::apply (BaseSkeleton *skeleton, float time, float alpha) {
	if (time < frames[0]) return; // Time is before first frame.

	Bone *bone = skeleton->bones[boneIndex];

	if (time >= frames[framesLength - 2]) { // Time is after last frame.
		float amount = bone->data->rotation + frames[framesLength - 1] - bone->rotation;
		while (amount > 180)
			amount -= 360;
		while (amount < -180)
			amount += 360;
		bone->rotation += amount * alpha;
		return;
	}

	// Interpolate between the last frame and the current frame.
	int frameIndex = binarySearch(frames, framesLength, time, 2);
	float lastFrameValue = frames[frameIndex - 1];
	float frameTime = frames[frameIndex];
	float percent = 1 - (time - frameTime) / (frames[frameIndex + ROTATE_LAST_FRAME_TIME] - frameTime);
	if (percent < 0)
		percent = 0;
	else if (percent > 1) //
		percent = 1;
	percent = getCurvePercent(frameIndex / 2 - 1, percent);

	float amount = frames[frameIndex + ROTATE_FRAME_VALUE] - lastFrameValue;
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

//

static const int TRANSLATE_LAST_FRAME_TIME = -3;
static const int TRANSLATE_FRAME_X = 1;
static const int TRANSLATE_FRAME_Y = 2;

TranslateTimeline::TranslateTimeline (int keyframeCount) :
				CurveTimeline(keyframeCount),
				framesLength(keyframeCount * 3),
				frames(new float[framesLength]),
				boneIndex(0) {
	memset(frames, 0, sizeof(float) * framesLength);
}

TranslateTimeline::~TranslateTimeline () {
	delete[] frames;
}

float TranslateTimeline::getDuration () {
	return frames[framesLength - 3];
}

int TranslateTimeline::getKeyframeCount () {
	return framesLength / 3;
}

void TranslateTimeline::setKeyframe (int keyframeIndex, float time, float x, float y) {
	keyframeIndex *= 3;
	frames[keyframeIndex] = time;
	frames[keyframeIndex + 1] = x;
	frames[keyframeIndex + 2] = y;
}

void TranslateTimeline::apply (BaseSkeleton *skeleton, float time, float alpha) {
	if (time < frames[0]) return; // Time is before first frame.

	Bone *bone = skeleton->bones[boneIndex];

	if (time >= frames[framesLength - 3]) { // Time is after last frame.
		bone->x += (bone->data->x + frames[framesLength - 2] - bone->x) * alpha;
		bone->y += (bone->data->y + frames[framesLength - 1] - bone->y) * alpha;
		return;
	}

	// Interpolate between the last frame and the current frame.
	int frameIndex = binarySearch(frames, framesLength, time, 3);
	float lastFrameX = frames[frameIndex - 2];
	float lastFrameY = frames[frameIndex - 1];
	float frameTime = frames[frameIndex];
	float percent = 1 - (time - frameTime) / (frames[frameIndex + TRANSLATE_LAST_FRAME_TIME] - frameTime);
	if (percent < 0)
		percent = 0;
	else if (percent > 1) //
		percent = 1;
	percent = getCurvePercent(frameIndex / 3 - 1, percent);

	bone->x += (bone->data->x + lastFrameX + (frames[frameIndex + TRANSLATE_FRAME_X] - lastFrameX) * percent - bone->x) * alpha;
	bone->y += (bone->data->y + lastFrameY + (frames[frameIndex + TRANSLATE_FRAME_Y] - lastFrameY) * percent - bone->y) * alpha;
}

//

ScaleTimeline::ScaleTimeline (int keyframeCount) :
				TranslateTimeline(keyframeCount) {
}

void ScaleTimeline::apply (BaseSkeleton *skeleton, float time, float alpha) {
	if (time < frames[0]) return; // Time is before first frame.

	Bone *bone = skeleton->bones[boneIndex];
	if (time >= frames[framesLength - 3]) { // Time is after last frame.
		bone->scaleX += (bone->data->scaleX - 1 + frames[framesLength - 2] - bone->scaleX) * alpha;
		bone->scaleY += (bone->data->scaleY - 1 + frames[framesLength - 1] - bone->scaleY) * alpha;
		return;
	}

	// Interpolate between the last frame and the current frame.
	int frameIndex = binarySearch(frames, framesLength, time, 3);
	float lastFrameX = frames[frameIndex - 2];
	float lastFrameY = frames[frameIndex - 1];
	float frameTime = frames[frameIndex];
	float percent = 1 - (time - frameTime) / (frames[frameIndex + TRANSLATE_LAST_FRAME_TIME] - frameTime);
	if (percent < 0)
		percent = 0;
	else if (percent > 1) //
		percent = 1;
	percent = getCurvePercent(frameIndex / 3 - 1, percent);

	bone->scaleX += (bone->data->scaleX - 1 + lastFrameX + (frames[frameIndex + TRANSLATE_FRAME_X] - lastFrameX) * percent
			- bone->scaleX) * alpha;
	bone->scaleY += (bone->data->scaleY - 1 + lastFrameY + (frames[frameIndex + TRANSLATE_FRAME_Y] - lastFrameY) * percent
			- bone->scaleY) * alpha;
}

//

static const int COLOR_LAST_FRAME_TIME = -5;
static const int COLOR_FRAME_R = 1;
static const int COLOR_FRAME_G = 2;
static const int COLOR_FRAME_B = 3;
static const int COLOR_FRAME_A = 4;

ColorTimeline::ColorTimeline (int keyframeCount) :
				CurveTimeline(keyframeCount),
				framesLength(keyframeCount * 5),
				frames(new float[framesLength]),
				slotIndex(0) {
	memset(frames, 0, sizeof(float) * framesLength);
}

ColorTimeline::~ColorTimeline () {
	delete[] frames;
}

float ColorTimeline::getDuration () {
	return frames[framesLength - 5];
}

int ColorTimeline::getKeyframeCount () {
	return framesLength / 5;
}

void ColorTimeline::setKeyframe (int keyframeIndex, float time, float r, float g, float b, float a) {
	keyframeIndex *= 5;
	frames[keyframeIndex] = time;
	frames[keyframeIndex + 1] = r;
	frames[keyframeIndex + 2] = g;
	frames[keyframeIndex + 3] = b;
	frames[keyframeIndex + 4] = a;
}

void ColorTimeline::apply (BaseSkeleton *skeleton, float time, float alpha) {
	if (time < frames[0]) return; // Time is before first frame.

	Slot *slot = skeleton->slots[slotIndex];

	if (time >= frames[framesLength - 5]) { // Time is after last frame.
		int i = framesLength - 1;
		slot->r = frames[i - 3];
		slot->g = frames[i - 2];
		slot->b = frames[i - 1];
		slot->a = frames[i];
		return;
	}

	// Interpolate between the last frame and the current frame.
	int frameIndex = binarySearch(frames, framesLength, time, 5);
	float lastFrameR = frames[frameIndex - 4];
	float lastFrameG = frames[frameIndex - 3];
	float lastFrameB = frames[frameIndex - 2];
	float lastFrameA = frames[frameIndex - 1];
	float frameTime = frames[frameIndex];
	float percent = 1 - (time - frameTime) / (frames[frameIndex + COLOR_LAST_FRAME_TIME] - frameTime);
	if (percent < 0)
		percent = 0;
	else if (percent > 1) //
		percent = 1;
	percent = getCurvePercent(frameIndex / 5 - 1, percent);

	float r = lastFrameR + (frames[frameIndex + COLOR_FRAME_R] - lastFrameR) * percent;
	float g = lastFrameG + (frames[frameIndex + COLOR_FRAME_G] - lastFrameG) * percent;
	float b = lastFrameB + (frames[frameIndex + COLOR_FRAME_B] - lastFrameB) * percent;
	float a = lastFrameA + (frames[frameIndex + COLOR_FRAME_A] - lastFrameA) * percent;
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

//

AttachmentTimeline::AttachmentTimeline (int keyframeCount) :
				framesLength(keyframeCount),
				frames(new float[keyframeCount]),
				attachmentNames(new string*[keyframeCount]),
				slotIndex(0) {
	memset(frames, 0, sizeof(float) * keyframeCount);
	memset(attachmentNames, 0, sizeof(string*) * keyframeCount);
}

AttachmentTimeline::~AttachmentTimeline () {
	delete[] frames;

	for (int i = 0; i < framesLength; i++)
		if (attachmentNames[i]) delete attachmentNames[i];
	delete[] attachmentNames;
}

float AttachmentTimeline::getDuration () {
	return frames[framesLength - 1];
}

int AttachmentTimeline::getKeyframeCount () {
	return framesLength;
}

void AttachmentTimeline::setKeyframe (int keyframeIndex, float time, const string &attachmentName) {
	frames[keyframeIndex] = time;
	if (attachmentNames[keyframeIndex]) delete attachmentNames[keyframeIndex];
	attachmentNames[keyframeIndex] = attachmentName.length() == 0 ? 0 : new string(attachmentName);
}

void AttachmentTimeline::apply (BaseSkeleton *skeleton, float time, float alpha) {
	if (time < frames[0]) return; // Time is before first frame.

	int frameIndex;
	if (time >= frames[framesLength - 1]) // Time is after last frame.
		frameIndex = framesLength - 1;
	else
		frameIndex = binarySearch(frames, framesLength, time, 1) - 1;

	string *attachmentName = attachmentNames[frameIndex];
	skeleton->slots[slotIndex]->setAttachment(attachmentName ? skeleton->getAttachment(slotIndex, *attachmentName) : 0);
}

} /* namespace spine */
