#ifndef SPINE_ANIMATION_H_
#define SPINE_ANIMATION_H_

#include <string>
#include <vector>

namespace spine {

class BaseSkeleton;
class Timeline;

class Animation {
public:
	std::vector<Timeline*> timelines;
	float duration;

	Animation (const std::vector<Timeline*> &timelines, float duration);
  ~Animation();

	void apply (BaseSkeleton *skeleton, float time, bool loop);
};

//

class Timeline {
public:
	virtual ~Timeline () {
	}

	virtual float getDuration () = 0;

	virtual int getKeyframeCount () = 0;

	virtual void apply (BaseSkeleton *skeleton, float time, float alpha) = 0;
};

//

class CurveTimeline: public Timeline {
public:
	float *curves; // dfx, dfy, ddfx, ddfy, dddfx, dddfy, ...

	CurveTimeline (int keyframeCount);
	virtual ~CurveTimeline ();

	void setLinear (int keyframeIndex);

	void setStepped (int keyframeIndex);

	/** Sets the control handle positions for an interpolation bezier curve used to transition from this keyframe to the next.
	 * cx1 and cx2 are from 0 to 1, representing the percent of time between the two keyframes. cy1 and cy2 are the percent of
	 * the difference between the keyframe's values. */
	void setCurve (int keyframeIndex, float cx1, float cy1, float cx2, float cy2);

	float getCurvePercent (int keyframeIndex, float percent);
};

//

class RotateTimeline: public CurveTimeline {
public:
	int framesLength;
	float *frames; // time, value, ...
	int boneIndex;

	RotateTimeline (int keyframeCount);
	virtual ~RotateTimeline ();

	virtual float getDuration ();
	virtual int getKeyframeCount ();
	virtual void apply (BaseSkeleton *skeleton, float time, float alpha);

	void setKeyframe (int keyframeIndex, float time, float value);
};

//

class TranslateTimeline: public CurveTimeline {
public:
	int framesLength;
	float *frames; // time, value, value, ...
	int boneIndex;

	TranslateTimeline (int keyframeCount);
	virtual ~TranslateTimeline ();

	virtual float getDuration ();
	virtual int getKeyframeCount ();
	virtual void apply (BaseSkeleton *skeleton, float time, float alpha);

	void setKeyframe (int keyframeIndex, float time, float x, float y);
};

//

class ScaleTimeline: public TranslateTimeline {
public:
	ScaleTimeline (int keyframeCount);

	virtual void apply (BaseSkeleton *skeleton, float time, float alpha);
};

//

class ColorTimeline: public CurveTimeline {
public:
	int framesLength;
	float* frames; // time, r, g, b, a, ...
	int slotIndex;

	ColorTimeline (int keyframeCount);
	virtual ~ColorTimeline ();

	virtual float getDuration ();
	virtual int getKeyframeCount ();
	virtual void apply (BaseSkeleton *skeleton, float time, float alpha);

	void setKeyframe (int keyframeIndex, float time, float r, float g, float b, float a);
};

//

class AttachmentTimeline: public Timeline {
public:
	int framesLength;
	float *frames; // time, ...
	std::string **attachmentNames;
	int slotIndex;

	AttachmentTimeline (int keyframeCount);
	virtual ~AttachmentTimeline ();

	virtual float getDuration ();
	virtual int getKeyframeCount ();
	virtual void apply (BaseSkeleton *skeleton, float time, float alpha);

	/** @param attachmentName Pass an empty string to clear the image for a slot. */
	void setKeyframe (int keyframeIndex, float time, const std::string &attachmentName);
};

} /* namespace spine */
#endif /* SPINE_ANIMATION_H_ */
