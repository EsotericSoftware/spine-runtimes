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

#include "SpineTrackEntry.h"

void SpineTrackEntry::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_track_index"), &SpineTrackEntry::get_track_index);

	ClassDB::bind_method(D_METHOD("get_animation"), &SpineTrackEntry::get_animation);

	ClassDB::bind_method(D_METHOD("get_loop"), &SpineTrackEntry::get_loop);
	ClassDB::bind_method(D_METHOD("set_loop", "v"), &SpineTrackEntry::set_loop);

	ClassDB::bind_method(D_METHOD("get_hold_previous"), &SpineTrackEntry::get_hold_previous);
	ClassDB::bind_method(D_METHOD("set_hold_previous", "v"), &SpineTrackEntry::set_hold_previous);

	ClassDB::bind_method(D_METHOD("get_reverse"), &SpineTrackEntry::get_reverse);
	ClassDB::bind_method(D_METHOD("set_reverse", "v"), &SpineTrackEntry::set_reverse);

	ClassDB::bind_method(D_METHOD("get_delay"), &SpineTrackEntry::get_delay);
	ClassDB::bind_method(D_METHOD("set_delay", "v"), &SpineTrackEntry::set_delay);

	ClassDB::bind_method(D_METHOD("get_track_time"), &SpineTrackEntry::get_track_time);
	ClassDB::bind_method(D_METHOD("set_track_time", "v"), &SpineTrackEntry::set_track_time);

	ClassDB::bind_method(D_METHOD("get_track_end"), &SpineTrackEntry::get_track_end);
	ClassDB::bind_method(D_METHOD("set_track_end", "v"), &SpineTrackEntry::set_track_end);

	ClassDB::bind_method(D_METHOD("get_animation_start"), &SpineTrackEntry::get_animation_start);
	ClassDB::bind_method(D_METHOD("set_animation_start", "v"), &SpineTrackEntry::set_animation_start);

	ClassDB::bind_method(D_METHOD("get_animation_last"), &SpineTrackEntry::get_animation_last);
	ClassDB::bind_method(D_METHOD("set_animation_last", "v"), &SpineTrackEntry::set_animation_last);

	ClassDB::bind_method(D_METHOD("get_animation_time"), &SpineTrackEntry::get_animation_time);

	ClassDB::bind_method(D_METHOD("get_time_scale"), &SpineTrackEntry::get_time_scale);
	ClassDB::bind_method(D_METHOD("set_time_scale", "v"), &SpineTrackEntry::set_time_scale);

	ClassDB::bind_method(D_METHOD("get_alpha"), &SpineTrackEntry::get_alpha);
	ClassDB::bind_method(D_METHOD("set_alpha", "v"), &SpineTrackEntry::set_alpha);

	ClassDB::bind_method(D_METHOD("get_event_threshold"), &SpineTrackEntry::get_event_threshold);
	ClassDB::bind_method(D_METHOD("set_event_threshold", "v"), &SpineTrackEntry::set_event_threshold);

	ClassDB::bind_method(D_METHOD("get_attachment_threshold"), &SpineTrackEntry::get_attachment_threshold);
	ClassDB::bind_method(D_METHOD("set_attachment_threshold", "v"), &SpineTrackEntry::set_attachment_threshold);

	ClassDB::bind_method(D_METHOD("get_draw_order_threshold"), &SpineTrackEntry::get_draw_order_threshold);
	ClassDB::bind_method(D_METHOD("set_draw_order_threshold", "v"), &SpineTrackEntry::set_draw_order_threshold);

	ClassDB::bind_method(D_METHOD("get_next"), &SpineTrackEntry::get_next);

	ClassDB::bind_method(D_METHOD("is_complete"), &SpineTrackEntry::is_complete);

	ClassDB::bind_method(D_METHOD("get_mix_time"), &SpineTrackEntry::get_mix_time);
	ClassDB::bind_method(D_METHOD("set_mix_time", "v"), &SpineTrackEntry::set_mix_time);

	ClassDB::bind_method(D_METHOD("get_mix_duration"), &SpineTrackEntry::get_mix_duration);
	ClassDB::bind_method(D_METHOD("set_mix_duration", "v"), &SpineTrackEntry::set_mix_duration);

	ClassDB::bind_method(D_METHOD("get_mix_blend"), &SpineTrackEntry::get_mix_blend);
	ClassDB::bind_method(D_METHOD("set_mix_blend", "v"), &SpineTrackEntry::set_mix_blend);

	ClassDB::bind_method(D_METHOD("get_mixing_from"), &SpineTrackEntry::get_mixing_from);
	ClassDB::bind_method(D_METHOD("get_mixing_to"), &SpineTrackEntry::get_mixing_to);

	ClassDB::bind_method(D_METHOD("reset_rotation_directions"), &SpineTrackEntry::reset_rotation_directions);

	BIND_ENUM_CONSTANT(MIXBLEND_SETUP);
	BIND_ENUM_CONSTANT(MIXBLEND_FIRST);
	BIND_ENUM_CONSTANT(MIXBLEND_REPLACE);
	BIND_ENUM_CONSTANT(MIXBLEND_ADD);
}

SpineTrackEntry::SpineTrackEntry() : track_entry(NULL) {}
SpineTrackEntry::~SpineTrackEntry() {}

int SpineTrackEntry::get_track_index() {
	return track_entry->getTrackIndex();
}

Ref<SpineAnimation> SpineTrackEntry::get_animation() {
	Ref<SpineAnimation> gd_anim(memnew(SpineAnimation));
	auto anim = track_entry->getAnimation();
	if (anim == NULL) return NULL;
	gd_anim->set_spine_object(anim);
	return gd_anim;
}

bool SpineTrackEntry::get_loop() {
	return track_entry->getLoop();
}
void SpineTrackEntry::set_loop(bool v) {
	track_entry->setLoop(v);
}

bool SpineTrackEntry::get_hold_previous() {
	return track_entry->getHoldPrevious();
}
void SpineTrackEntry::set_hold_previous(bool v) {
	track_entry->setHoldPrevious(v);
}

float SpineTrackEntry::get_delay() {
	return track_entry->getDelay();
}
void SpineTrackEntry::set_delay(float v) {
	track_entry->setDelay(v);
}

float SpineTrackEntry::get_track_time() {
	return track_entry->getTrackTime();
}
void SpineTrackEntry::set_track_time(float v) {
	track_entry->setTrackTime(v);
}

float SpineTrackEntry::get_track_end() {
	return track_entry->getTrackEnd();
}
void SpineTrackEntry::set_track_end(float v) {
	track_entry->setTrackEnd(v);
}

float SpineTrackEntry::get_animation_start() {
	return track_entry->getAnimationStart();
}
void SpineTrackEntry::set_animation_start(float v) {
	track_entry->setAnimationStart(v);
}

float SpineTrackEntry::get_animation_last() {
	return track_entry->getAnimationLast();
}
void SpineTrackEntry::set_animation_last(float v) {
	track_entry->setAnimationLast(v);
}

float SpineTrackEntry::get_animation_time() {
	return track_entry->getAnimationTime();
}

float SpineTrackEntry::get_time_scale() {
	return track_entry->getTimeScale();
}
void SpineTrackEntry::set_time_scale(float v) {
	track_entry->setTimeScale(v);
}

float SpineTrackEntry::get_alpha() {
	return track_entry->getAlpha();
}
void SpineTrackEntry::set_alpha(float v) {
	track_entry->setAlpha(v);
}

float SpineTrackEntry::get_event_threshold() {
	return track_entry->getEventThreshold();
}
void SpineTrackEntry::set_event_threshold(float v) {
	track_entry->setEventThreshold(v);
}

float SpineTrackEntry::get_attachment_threshold() {
	return track_entry->getAttachmentThreshold();
}
void SpineTrackEntry::set_attachment_threshold(float v) {
	track_entry->setAttachmentThreshold(v);
}

float SpineTrackEntry::get_draw_order_threshold() {
	return track_entry->getDrawOrderThreshold();
}
void SpineTrackEntry::set_draw_order_threshold(float v) {
	track_entry->setDrawOrderThreshold(v);
}

Ref<SpineTrackEntry> SpineTrackEntry::get_next() {
	Ref<SpineTrackEntry> gd_entry(memnew(SpineTrackEntry));
	auto entry = track_entry->getNext();
	if (entry == NULL) return NULL;
	gd_entry->set_spine_object(entry);
	return gd_entry;
}

bool SpineTrackEntry::is_complete() {
	return track_entry->isComplete();
}

float SpineTrackEntry::get_mix_time() {
	return track_entry->getMixTime();
}
void SpineTrackEntry::set_mix_time(float v) {
	track_entry->setMixTime(v);
}

float SpineTrackEntry::get_mix_duration() {
	return track_entry->getMixDuration();
}
void SpineTrackEntry::set_mix_duration(float v) {
	track_entry->setMixDuration(v);
}

SpineTrackEntry::MixBlend SpineTrackEntry::get_mix_blend() {
	int mb = track_entry->getMixBlend();
	return (MixBlend) mb;
}
void SpineTrackEntry::set_mix_blend(SpineTrackEntry::MixBlend v) {
	int mb = (int) v;
	track_entry->setMixBlend((spine::MixBlend) mb);
}

Ref<SpineTrackEntry> SpineTrackEntry::get_mixing_from() {
	Ref<SpineTrackEntry> gd_entry(memnew(SpineTrackEntry));
	auto entry = track_entry->getMixingFrom();
	if (entry == NULL) return NULL;
	gd_entry->set_spine_object(entry);
	return gd_entry;
}
Ref<SpineTrackEntry> SpineTrackEntry::get_mixing_to() {
	Ref<SpineTrackEntry> gd_entry(memnew(SpineTrackEntry));
	auto entry = track_entry->getMixingTo();
	if (entry == NULL) return NULL;
	gd_entry->set_spine_object(entry);
	return gd_entry;
}

void SpineTrackEntry::reset_rotation_directions() {
	track_entry->resetRotationDirections();
}

bool SpineTrackEntry::get_reverse() {
	return track_entry->getReverse();
}

void SpineTrackEntry::set_reverse(bool v) {
	track_entry->setReverse(v);
}
