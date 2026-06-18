/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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
#include <spine/Event.h>
#include <spine/Skeleton.h>
#include <spine/Timeline.h>
#include <spine/BoneTimeline.h>
#include <spine/RotateTimeline.h>
#include <spine/TranslateTimeline.h>

#include <spine/ArrayUtils.h>

#include <stdint.h>

using namespace spine;

Animation::Animation(const String &name) : _timelines(), _timelineIds(), _bones(), _duration(0), _name(name), _color(1, 1, 1, 1) {
}

bool Animation::hasTimeline(Array<PropertyId> &ids) {
	for (size_t i = 0; i < ids.size(); i++) {
		if (_timelineIds.containsKey(ids[i])) return true;
	}
	return false;
}

Animation::~Animation() {
	ArrayUtils::deleteElements(_timelines);
}

void Animation::apply(Skeleton &skeleton, float lastTime, float time, bool loop, Array<Event *> *events, float alpha, MixFrom from, bool add,
					  bool out, bool appliedPose) {
	if (loop && _duration != 0) {
		time = MathUtil::fmod(time, _duration);
		if (lastTime > 0) {
			lastTime = MathUtil::fmod(lastTime, _duration);
		}
	}

	for (size_t i = 0, n = _timelines.size(); i < n; ++i) {
		_timelines[i]->apply(skeleton, lastTime, time, events, alpha, from, add, out, appliedPose);
	}
}

const String &Animation::getName() {
	return _name;
}

const Array<int> &Animation::getBones() {
	return _bones;
}

Color &Animation::getColor() {
	return _color;
}

Array<Timeline *> &Animation::getTimelines() {
	return _timelines;
}

float Animation::getDuration() {
	return _duration;
}

void Animation::setDuration(float inValue) {
	_duration = inValue;
}

int Animation::search(Array<float> &frames, float target) {
	size_t n = (int) frames.size();
	for (size_t i = 1; i < n; i++) {
		if (frames[i] > target) return (int) (i - 1);
	}
	return (int) (n - 1);
}

int Animation::search(Array<float> &frames, float target, int step) {
	size_t n = frames.size();
	for (size_t i = step; i < n; i += step)
		if (frames[i] > target) return (int) (i - step);
	return (int) (n - step);
}

void Animation::setTimelines(Array<Timeline *> &timelines, Array<int> &bones) {
	_timelines = timelines;
	_bones = bones;

	size_t n = timelines.size();
	_timelineIds.clear();
	for (size_t i = 0; i < n; i++) {
		Timeline *timeline = timelines[i];
		Array<PropertyId> &propertyIds = timeline->getPropertyIds();
		for (size_t ii = 0; ii < propertyIds.size(); ii++) {
			_timelineIds.put(propertyIds[ii], true);
		}
	}
}
