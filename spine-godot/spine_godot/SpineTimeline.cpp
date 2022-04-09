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

#include "SpineTimeline.h"
#include "SpineSkeleton.h"
#include "SpineEvent.h"
#include "core/method_bind_ext.gen.inc"

void SpineTimeline::_bind_methods() {
	ClassDB::bind_method(D_METHOD("apply", "skeleton", "last_time", "time", "events", "alpha", "blend", "direction"), &SpineTimeline::apply);
	ClassDB::bind_method(D_METHOD("get_frame_entries"), &SpineTimeline::get_frame_entries);
	ClassDB::bind_method(D_METHOD("get_frame_count"), &SpineTimeline::get_frame_count);
	ClassDB::bind_method(D_METHOD("get_frames"), &SpineTimeline::get_frames);
	ClassDB::bind_method(D_METHOD("get_duration"), &SpineTimeline::get_duration);
	ClassDB::bind_method(D_METHOD("get_property_ids"), &SpineTimeline::get_property_ids);
	ClassDB::bind_method(D_METHOD("get_type"), &SpineTimeline::get_type);
}

SpineTimeline::SpineTimeline() : timeline(NULL) {
}

SpineTimeline::~SpineTimeline() {
}

void SpineTimeline::apply(Ref<SpineSkeleton> skeleton, float lastTime, float time, Array events, float alpha,
						  SpineConstant::MixBlend blend, SpineConstant::MixDirection direction) {
	spine::Vector<spine::Event *> spineEvents;
	spineEvents.setSize(events.size(), nullptr);
	for (size_t i = 0; i < events.size(); ++i) {
		events[i] = ((Ref<SpineEvent>) spineEvents[i])->get_spine_object();
	}
	timeline->apply(*(skeleton->get_spine_object()), lastTime, time, &spineEvents, alpha, (spine::MixBlend) blend, (spine::MixDirection) direction);
}

int64_t SpineTimeline::get_frame_entries() {
	return timeline->getFrameEntries();
}

int64_t SpineTimeline::get_frame_count() {
	return timeline->getFrameCount();
}

Array SpineTimeline::get_frames() {
	auto &frames = timeline->getFrames();
	Array result;
	result.resize(frames.size());

	for (size_t i = 0; i < result.size(); ++i) {
		result[i] = frames[i];
	}

	return result;
}

float SpineTimeline::get_duration() {
	return timeline->getDuration();
}

Array SpineTimeline::get_property_ids() {
	auto &ids = timeline->getPropertyIds();
	Array result;
	result.resize(ids.size());

	for (size_t i = 0; i < result.size(); ++i) {
		result[i] = (int64_t) ids[i];
	}

	return result;
}

String SpineTimeline::get_type() {
	return timeline->getRTTI().getClassName();
}
