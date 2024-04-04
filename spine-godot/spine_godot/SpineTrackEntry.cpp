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

#include "SpineTrackEntry.h"
#include "SpineCommon.h"

void SpineTrackEntry::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_track_index"), &SpineTrackEntry::get_track_index);
	ClassDB::bind_method(D_METHOD("get_animation"), &SpineTrackEntry::get_animation);
	ClassDB::bind_method(D_METHOD("get_previous"), &SpineTrackEntry::get_previous);
	ClassDB::bind_method(D_METHOD("get_loop"), &SpineTrackEntry::get_loop);
	ClassDB::bind_method(D_METHOD("set_loop", "v"), &SpineTrackEntry::set_loop);
	ClassDB::bind_method(D_METHOD("get_hold_previous"), &SpineTrackEntry::get_hold_previous);
	ClassDB::bind_method(D_METHOD("set_hold_previous", "v"), &SpineTrackEntry::set_hold_previous);
	ClassDB::bind_method(D_METHOD("get_reverse"), &SpineTrackEntry::get_reverse);
	ClassDB::bind_method(D_METHOD("set_reverse", "v"), &SpineTrackEntry::set_reverse);
	ClassDB::bind_method(D_METHOD("get_shortest_rotation"), &SpineTrackEntry::get_shortest_rotation);
	ClassDB::bind_method(D_METHOD("set_shortest_rotation", "v"), &SpineTrackEntry::set_shortest_rotation);
	ClassDB::bind_method(D_METHOD("get_delay"), &SpineTrackEntry::get_delay);
	ClassDB::bind_method(D_METHOD("set_delay", "v"), &SpineTrackEntry::set_delay);
	ClassDB::bind_method(D_METHOD("get_track_time"), &SpineTrackEntry::get_track_time);
	ClassDB::bind_method(D_METHOD("set_track_time", "v"), &SpineTrackEntry::set_track_time);
	ClassDB::bind_method(D_METHOD("get_track_end"), &SpineTrackEntry::get_track_end);
	ClassDB::bind_method(D_METHOD("set_track_end", "v"), &SpineTrackEntry::set_track_end);
	ClassDB::bind_method(D_METHOD("get_animation_start"), &SpineTrackEntry::get_animation_start);
	ClassDB::bind_method(D_METHOD("set_animation_start", "v"), &SpineTrackEntry::set_animation_start);
	ClassDB::bind_method(D_METHOD("get_animation_end"), &SpineTrackEntry::get_animation_end);
	ClassDB::bind_method(D_METHOD("set_animation_end", "v"), &SpineTrackEntry::set_animation_end);
	ClassDB::bind_method(D_METHOD("get_animation_last"), &SpineTrackEntry::get_animation_last);
	ClassDB::bind_method(D_METHOD("set_animation_last", "v"), &SpineTrackEntry::set_animation_last);
	ClassDB::bind_method(D_METHOD("get_animation_time"), &SpineTrackEntry::get_animation_time);
	ClassDB::bind_method(D_METHOD("get_time_scale"), &SpineTrackEntry::get_time_scale);
	ClassDB::bind_method(D_METHOD("set_time_scale", "v"), &SpineTrackEntry::set_time_scale);
	ClassDB::bind_method(D_METHOD("get_alpha"), &SpineTrackEntry::get_alpha);
	ClassDB::bind_method(D_METHOD("set_alpha", "v"), &SpineTrackEntry::set_alpha);
	ClassDB::bind_method(D_METHOD("get_event_threshold"), &SpineTrackEntry::get_event_threshold);
	ClassDB::bind_method(D_METHOD("set_event_threshold", "v"), &SpineTrackEntry::set_event_threshold);
	ClassDB::bind_method(D_METHOD("get_mix_attachment_threshold"), &SpineTrackEntry::get_mix_attachment_threshold);
	ClassDB::bind_method(D_METHOD("set_mix_attachment_threshold", "v"), &SpineTrackEntry::set_mix_attachment_threshold);
	ClassDB::bind_method(D_METHOD("get_mix_draw_order_threshold"), &SpineTrackEntry::get_mix_draw_order_threshold);
	ClassDB::bind_method(D_METHOD("set_mix_draw_order_threshold", "v"), &SpineTrackEntry::set_mix_draw_order_threshold);
	ClassDB::bind_method(D_METHOD("get_alpha_attachment_threshold"), &SpineTrackEntry::get_alpha_attachment_threshold);
	ClassDB::bind_method(D_METHOD("set_alpha_attachment_threshold", "v"), &SpineTrackEntry::set_alpha_attachment_threshold);
	ClassDB::bind_method(D_METHOD("get_next"), &SpineTrackEntry::get_next);
	ClassDB::bind_method(D_METHOD("is_complete"), &SpineTrackEntry::is_complete);
	ClassDB::bind_method(D_METHOD("get_mix_time"), &SpineTrackEntry::get_mix_time);
	ClassDB::bind_method(D_METHOD("set_mix_time", "v"), &SpineTrackEntry::set_mix_time);
	ClassDB::bind_method(D_METHOD("get_mix_duration"), &SpineTrackEntry::get_mix_duration);
	ClassDB::bind_method(D_METHOD("set_mix_duration", "v"), &SpineTrackEntry::set_mix_duration);
	ClassDB::bind_method(D_METHOD("set_mix_duration_and_delay", "v", "delay"), &SpineTrackEntry::set_mix_duration_and_delay);
	ClassDB::bind_method(D_METHOD("get_mix_blend"), &SpineTrackEntry::get_mix_blend);
	ClassDB::bind_method(D_METHOD("set_mix_blend", "v"), &SpineTrackEntry::set_mix_blend);
	ClassDB::bind_method(D_METHOD("get_mixing_from"), &SpineTrackEntry::get_mixing_from);
	ClassDB::bind_method(D_METHOD("get_mixing_to"), &SpineTrackEntry::get_mixing_to);
	ClassDB::bind_method(D_METHOD("reset_rotation_directions"), &SpineTrackEntry::reset_rotation_directions);
	ClassDB::bind_method(D_METHOD("get_track_complete"), &SpineTrackEntry::get_track_complete);
	ClassDB::bind_method(D_METHOD("was_applied"), &SpineTrackEntry::was_applied);
}

int SpineTrackEntry::get_track_index() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getTrackIndex();
}

Ref<SpineAnimation> SpineTrackEntry::get_animation() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto animation = get_spine_object()->getAnimation();
	if (!animation) return nullptr;
	Ref<SpineAnimation> animation_ref(memnew(SpineAnimation));
	animation_ref->set_spine_object(*get_spine_owner()->get_skeleton_data_res(), animation);
	return animation_ref;
}

Ref<SpineTrackEntry> SpineTrackEntry::get_previous() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto previous = get_spine_object()->getPrevious();
	if (!previous) return nullptr;
	Ref<SpineTrackEntry> previous_ref(memnew(SpineTrackEntry));
	previous_ref->set_spine_object(get_spine_owner(), previous);
	return previous_ref;
}

bool SpineTrackEntry::get_loop() {
	SPINE_CHECK(get_spine_object(), false)
	return get_spine_object()->getLoop();
}

void SpineTrackEntry::set_loop(bool v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setLoop(v);
}

bool SpineTrackEntry::get_hold_previous() {
	SPINE_CHECK(get_spine_object(), false)
	return get_spine_object()->getHoldPrevious();
}

void SpineTrackEntry::set_hold_previous(bool v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setHoldPrevious(v);
}

bool SpineTrackEntry::get_reverse() {
	SPINE_CHECK(get_spine_object(), false)
	return get_spine_object()->getReverse();
}

void SpineTrackEntry::set_reverse(bool v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setReverse(v);
}

bool SpineTrackEntry::get_shortest_rotation() {
	SPINE_CHECK(get_spine_object(), false)
	return get_spine_object()->getShortestRotation();
}

void SpineTrackEntry::set_shortest_rotation(bool v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setShortestRotation(v);
}

float SpineTrackEntry::get_delay() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getDelay();
}

void SpineTrackEntry::set_delay(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setDelay(v);
}

float SpineTrackEntry::get_track_time() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getTrackTime();
}

void SpineTrackEntry::set_track_time(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setTrackTime(v);
}

float SpineTrackEntry::get_track_end() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getTrackEnd();
}

void SpineTrackEntry::set_track_end(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setTrackEnd(v);
}

float SpineTrackEntry::get_animation_start() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getAnimationStart();
}

void SpineTrackEntry::set_animation_start(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setAnimationStart(v);
}

float SpineTrackEntry::get_animation_end() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getAnimationEnd();
}

void SpineTrackEntry::set_animation_end(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setAnimationEnd(v);
}

float SpineTrackEntry::get_animation_last() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getAnimationLast();
}

void SpineTrackEntry::set_animation_last(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setAnimationLast(v);
}

float SpineTrackEntry::get_animation_time() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getAnimationTime();
}

float SpineTrackEntry::get_time_scale() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getTimeScale();
}

void SpineTrackEntry::set_time_scale(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setTimeScale(v);
}

float SpineTrackEntry::get_alpha() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getAlpha();
}

void SpineTrackEntry::set_alpha(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setAlpha(v);
}

float SpineTrackEntry::get_event_threshold() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getEventThreshold();
}

void SpineTrackEntry::set_event_threshold(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setEventThreshold(v);
}

float SpineTrackEntry::get_mix_attachment_threshold() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMixAttachmentThreshold();
}

void SpineTrackEntry::set_mix_attachment_threshold(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMixAttachmentThreshold(v);
}

float SpineTrackEntry::get_mix_draw_order_threshold() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMixDrawOrderThreshold();
}

void SpineTrackEntry::set_mix_draw_order_threshold(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMixDrawOrderThreshold(v);
}

float SpineTrackEntry::get_alpha_attachment_threshold() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getAlphaAttachmentThreshold();
}

void SpineTrackEntry::set_alpha_attachment_threshold(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setAlphaAttachmentThreshold(v);
}

Ref<SpineTrackEntry> SpineTrackEntry::get_next() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto next = get_spine_object()->getNext();
	if (!next) return nullptr;
	Ref<SpineTrackEntry> next_ref(memnew(SpineTrackEntry));
	next_ref->set_spine_object(get_spine_owner(), next);
	return next_ref;
}

bool SpineTrackEntry::is_complete() {
	SPINE_CHECK(get_spine_object(), false)
	return get_spine_object()->isComplete();
}

float SpineTrackEntry::get_mix_time() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMixTime();
}

void SpineTrackEntry::set_mix_time(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMixTime(v);
}

float SpineTrackEntry::get_mix_duration() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMixDuration();
}

void SpineTrackEntry::set_mix_duration(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMixDuration(v);
}

void SpineTrackEntry::set_mix_duration_and_delay(float v, float delay) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMixDuration(v, delay);
}

SpineConstant::MixBlend SpineTrackEntry::get_mix_blend() {
	SPINE_CHECK(get_spine_object(), SpineConstant::MixBlend_Setup)
	return (SpineConstant::MixBlend) get_spine_object()->getMixBlend();
}

void SpineTrackEntry::set_mix_blend(SpineConstant::MixBlend v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMixBlend((spine::MixBlend) v);
}

Ref<SpineTrackEntry> SpineTrackEntry::get_mixing_from() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto mixing_from = get_spine_object()->getMixingFrom();
	if (!mixing_from) return nullptr;
	Ref<SpineTrackEntry> mixing_from_ref(memnew(SpineTrackEntry));
	mixing_from_ref->set_spine_object(get_spine_owner(), mixing_from);
	return mixing_from_ref;
}

Ref<SpineTrackEntry> SpineTrackEntry::get_mixing_to() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto mixing_to = get_spine_object()->getMixingTo();
	if (!mixing_to) return nullptr;
	Ref<SpineTrackEntry> mixing_to_ref(memnew(SpineTrackEntry));
	mixing_to_ref->set_spine_object(get_spine_owner(), mixing_to);
	return mixing_to_ref;
}

void SpineTrackEntry::reset_rotation_directions() {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->resetRotationDirections();
}

float SpineTrackEntry::get_track_complete() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getTrackComplete();
}

bool SpineTrackEntry::was_applied() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->wasApplied();
}