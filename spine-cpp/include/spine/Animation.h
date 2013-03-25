/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ******************************************************************************/

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
	~Animation ();

	void apply (BaseSkeleton *skeleton, float time, bool loop = false) const;
	void mix (BaseSkeleton *skeleton, float time, bool loop, float alpha) const;
};

//

class Timeline {
public:
	virtual ~Timeline () {
	}

	virtual void apply (BaseSkeleton *skeleton, float time, float alpha = 1) const = 0;
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

	float getCurvePercent (int keyframeIndex, float percent) const;
};

//

class RotateTimeline: public CurveTimeline {
public:
	int framesLength;
	float *frames; // time, value, ...
	int boneIndex;

	RotateTimeline (int keyframeCount);
	virtual ~RotateTimeline ();

	virtual void apply (BaseSkeleton *skeleton, float time, float alpha = 1) const;

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

	virtual void apply (BaseSkeleton *skeleton, float time, float alpha = 1) const;

	void setKeyframe (int keyframeIndex, float time, float x, float y);
};

//

class ScaleTimeline: public TranslateTimeline {
public:
	ScaleTimeline (int keyframeCount);

	virtual void apply (BaseSkeleton *skeleton, float time, float alpha = 1) const;
};

//

class ColorTimeline: public CurveTimeline {
public:
	int framesLength;
	float* frames; // time, r, g, b, a, ...
	int slotIndex;

	ColorTimeline (int keyframeCount);
	virtual ~ColorTimeline ();

	virtual void apply (BaseSkeleton *skeleton, float time, float alpha = 1) const;

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

	virtual void apply (BaseSkeleton *skeleton, float time, float alpha = 1) const;

	/** The AttachmentTimeline owns the attachmentName.
	 * @param attachmentName May be null to clear the image for a slot. */
	void setKeyframe (int keyframeIndex, float time, std::string *attachmentName);
};

} /* namespace spine */
#endif /* SPINE_ANIMATION_H_ */
