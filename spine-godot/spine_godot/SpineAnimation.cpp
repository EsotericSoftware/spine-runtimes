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

#include "SpineAnimation.h"

#include "SpineSkeleton.h"
#include "SpineEvent.h"
#include "SpineTimeline.h"

// enable more than 5 arguments of a method bind function
#include "core/method_bind_ext.gen.inc"

void SpineAnimation::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_anim_name"), &SpineAnimation::get_anim_name);
	ClassDB::bind_method(D_METHOD("get_duration"), &SpineAnimation::get_duration);
	ClassDB::bind_method(D_METHOD("set_duration", "v"), &SpineAnimation::set_duration);

	ClassDB::bind_method(D_METHOD("apply", "skeleton", "last_time", "time", "loop", "events", "alpha", "blend", "direction"), &SpineAnimation::apply);
	ClassDB::bind_method(D_METHOD("get_timelines"), &SpineAnimation::get_timelines);
	ClassDB::bind_method(D_METHOD("has_timeline", "ids"), &SpineAnimation::has_timeline);
}

SpineAnimation::SpineAnimation() : animation(NULL) {}
SpineAnimation::~SpineAnimation() {}

String SpineAnimation::get_anim_name() {
	return animation->getName().buffer();
}

float SpineAnimation::get_duration() {
	return animation->getDuration();
}
void SpineAnimation::set_duration(float v) {
	animation->setDuration(v);
}

void SpineAnimation::apply(Ref<SpineSkeleton> skeleton, float lastTime, float time, bool loop,
						   Array pEvents, float alpha, SpineConstant::MixBlend blend,
						   SpineConstant::MixDirection direction) {
	spine::Vector<spine::Event *> events;
	events.setSize(pEvents.size(), nullptr);
	for (size_t i = 0; i < events.size(); ++i) {
		events[i] = ((Ref<SpineEvent>) (pEvents[i]))->get_spine_object();
	}
	animation->apply(*(skeleton->get_spine_object()), lastTime, time, loop, &events, alpha, (spine::MixBlend) blend, (spine::MixDirection) direction);
}

Array SpineAnimation::get_timelines() {
	auto &timelines = animation->getTimelines();
	Array res;
	res.resize(timelines.size());

	for (size_t i = 0; i < res.size(); ++i) {
		auto a = Ref<SpineTimeline>(memnew(SpineTimeline));
		a->set_spine_object(timelines[i]);
		res.set(i, a);
	}

	return res;
}

bool SpineAnimation::has_timeline(Array ids) {
	spine::Vector<spine::PropertyId> list;
	list.setSize(ids.size(), 0);

	for (size_t i = 0; i < list.size(); ++i) {
		list[i] = (int64_t) ids[i];
	}
	return animation->hasTimeline(list);
}
