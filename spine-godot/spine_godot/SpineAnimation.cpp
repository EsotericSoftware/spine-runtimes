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

#include "SpineAnimation.h"
#include "SpineSkeleton.h"
#include "SpineEvent.h"
#include "SpineTimeline.h"
#if VERSION_MAJOR == 3
#include "core/method_bind_ext.gen.inc"
#endif

void SpineAnimation::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_name"), &SpineAnimation::get_name);
	ClassDB::bind_method(D_METHOD("get_duration"), &SpineAnimation::get_duration);
	ClassDB::bind_method(D_METHOD("set_duration", "duration"), &SpineAnimation::set_duration);

	ClassDB::bind_method(D_METHOD("apply", "skeleton", "last_time", "time", "loop", "events", "alpha", "blend", "direction"), &SpineAnimation::apply);
	ClassDB::bind_method(D_METHOD("get_timelines"), &SpineAnimation::get_timelines);
	ClassDB::bind_method(D_METHOD("has_timeline", "ids"), &SpineAnimation::has_timeline);
}

String SpineAnimation::get_name() {
	SPINE_CHECK(get_spine_object(), "")
	return get_spine_object()->getName().buffer();
}

float SpineAnimation::get_duration() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getDuration();
}

void SpineAnimation::set_duration(float duration) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setDuration(duration);
}

void SpineAnimation::apply(Ref<SpineSkeleton> skeleton, float last_time, float time, bool loop,
						   Array events, float alpha, SpineConstant::MixBlend blend,
						   SpineConstant::MixDirection direction) {
	SPINE_CHECK(get_spine_object(), )
	spine::Vector<spine::Event *> spineEvents;
	get_spine_object()->apply(*(skeleton->get_spine_object()), last_time, time, loop, &spineEvents, alpha, (spine::MixBlend) blend, (spine::MixDirection) direction);
	for (int i = 0; i < (int) spineEvents.size(); ++i) {
		auto event_ref = memnew(SpineEvent);
		event_ref->set_spine_object(skeleton->get_spine_owner(), spineEvents[i]);
		events.append(event_ref);
	}
}

Array SpineAnimation::get_timelines() {
	Array result;
	SPINE_CHECK(get_spine_object(), result)
	auto &timelines = get_spine_object()->getTimelines();
	result.resize((int) timelines.size());

	for (int i = 0; i < (int) result.size(); ++i) {
		auto timeline_ref = Ref<SpineTimeline>(memnew(SpineTimeline));
		timeline_ref->set_spine_object(get_spine_owner(), timelines[i]);
#ifdef SPINE_GODOT_EXTENSION
		result[i] = timeline_ref;
#else
		result.set(i, timeline_ref);
#endif
	}
	return result;
}

bool SpineAnimation::has_timeline(Array ids) {
	SPINE_CHECK(get_spine_object(), false)
	spine::Vector<spine::PropertyId> property_ids;
	property_ids.setSize(ids.size(), 0);

	for (int i = 0; i < (int) property_ids.size(); ++i) {
		property_ids[i] = (int64_t) ids[i];
	}
	return get_spine_object()->hasTimeline(property_ids);
}
