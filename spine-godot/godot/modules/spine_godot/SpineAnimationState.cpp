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

#include "SpineAnimationState.h"

void SpineAnimationState::_bind_methods() {
	ClassDB::bind_method(D_METHOD("set_animation", "anim_name", "loop", "track_id"), &SpineAnimationState::set_animation, DEFVAL(true), DEFVAL(0));
	ClassDB::bind_method(D_METHOD("update", "delta"), &SpineAnimationState::update, DEFVAL(0));
	ClassDB::bind_method(D_METHOD("apply", "skeleton"), &SpineAnimationState::apply);
	ClassDB::bind_method(D_METHOD("clear_tracks"), &SpineAnimationState::clear_tracks);
	ClassDB::bind_method(D_METHOD("clear_track"), &SpineAnimationState::clear_track);
	ClassDB::bind_method(D_METHOD("add_animation", "anim_name", "delay", "loop", "track_id"), &SpineAnimationState::add_animation, DEFVAL(0), DEFVAL(true), DEFVAL(0));
	ClassDB::bind_method(D_METHOD("set_empty_animation", "track_id", "mix_duration"), &SpineAnimationState::set_empty_animation);
	ClassDB::bind_method(D_METHOD("add_empty_animation", "track_id", "mix_duration", "delay"), &SpineAnimationState::add_empty_animation);
	ClassDB::bind_method(D_METHOD("set_empty_animations", "mix_duration"), &SpineAnimationState::set_empty_animations);
	ClassDB::bind_method(D_METHOD("get_time_scale"), &SpineAnimationState::get_time_scale);
	ClassDB::bind_method(D_METHOD("set_time_scale", "time_scale"), &SpineAnimationState::set_time_scale);
	ClassDB::bind_method(D_METHOD("disable_queue"), &SpineAnimationState::disable_queue);
	ClassDB::bind_method(D_METHOD("enable_queue"), &SpineAnimationState::enable_queue);
	ClassDB::bind_method(D_METHOD("get_current", "track_id"), &SpineAnimationState::get_current);
}

SpineAnimationState::SpineAnimationState() : animation_state(NULL) {
}

SpineAnimationState::~SpineAnimationState() {
	if (animation_state) {
		delete animation_state;
		animation_state = NULL;
	}
}

void SpineAnimationState::create_animation_state(spine::AnimationStateData *animation_state_data) {
	if (animation_state) {
		delete animation_state;
		animation_state = NULL;
	}
	animation_state = new spine::AnimationState(animation_state_data);
}

#define CHECK_V                                              \
	if (!animation_state) {                                  \
		ERR_PRINT("The animation state is not loaded yet!"); \
		return;                                              \
	}
#define CHECK_X(x)                                           \
	if (!animation_state) {                                  \
		ERR_PRINT("The animation state is not loaded yet!"); \
		return x;                                            \
	}
#define S_T(x) (spine::String(x.utf8()))
Ref<SpineTrackEntry> SpineAnimationState::set_animation(const String &anim_name, bool loop, uint64_t track) {
	CHECK_X(NULL);
	auto skeleton_data = animation_state->getData()->getSkeletonData();
	auto anim = skeleton_data->findAnimation(anim_name.utf8().ptr());
	if (!anim) {
		ERR_PRINT(String("Can not find animation: ") + anim_name);
		return NULL;
	}
	auto entry = animation_state->setAnimation(track, anim, loop);
	Ref<SpineTrackEntry> gd_entry(memnew(SpineTrackEntry));
	gd_entry->set_spine_object(entry);
	return gd_entry;
}
Ref<SpineTrackEntry> SpineAnimationState::add_animation(const String &anim_name, float delay, bool loop, uint64_t track) {
	CHECK_X(NULL);
	auto skeleton_data = animation_state->getData()->getSkeletonData();
	auto anim = skeleton_data->findAnimation(anim_name.utf8().ptr());
	if (!anim) {
		ERR_PRINT(String("Can not find animation: ") + anim_name);
		return NULL;
	}
	auto entry = animation_state->addAnimation(track, anim, loop, delay);
	Ref<SpineTrackEntry> gd_entry(memnew(SpineTrackEntry));
	gd_entry->set_spine_object(entry);
	return gd_entry;
}

Ref<SpineTrackEntry> SpineAnimationState::set_empty_animation(uint64_t track_id, float mix_duration) {
	CHECK_X(NULL);
	auto entry = animation_state->setEmptyAnimation(track_id, mix_duration);
	Ref<SpineTrackEntry> gd_entry(memnew(SpineTrackEntry));
	gd_entry->set_spine_object(entry);
	return gd_entry;
}
Ref<SpineTrackEntry> SpineAnimationState::add_empty_animation(uint64_t track_id, float mix_duration, float delay) {
	CHECK_X(NULL);
	auto entry = animation_state->addEmptyAnimation(track_id, mix_duration, delay);
	Ref<SpineTrackEntry> gd_entry(memnew(SpineTrackEntry));
	gd_entry->set_spine_object(entry);
	return gd_entry;
}
void SpineAnimationState::set_empty_animations(float mix_duration) {
	CHECK_V;
	animation_state->setEmptyAnimations(mix_duration);
}

void SpineAnimationState::update(float delta) {
	CHECK_V;
	animation_state->update(delta);
}
bool SpineAnimationState::apply(Ref<SpineSkeleton> skeleton) {
	CHECK_X(false);
	return animation_state->apply(*(skeleton->get_spine_object()));
}


void SpineAnimationState::clear_tracks() {
	CHECK_V;
	animation_state->clearTracks();
}
void SpineAnimationState::clear_track(uint64_t track_id) {
	CHECK_V;
	animation_state->clearTrack(track_id);
}

float SpineAnimationState::get_time_scale() {
	CHECK_X(0);
	return animation_state->getTimeScale();
}
void SpineAnimationState::set_time_scale(float time_scale) {
	CHECK_V;
	animation_state->setTimeScale(time_scale);
}

void SpineAnimationState::disable_queue() {
	CHECK_V;
	animation_state->disableQueue();
}
void SpineAnimationState::enable_queue() {
	CHECK_V;
	animation_state->enableQueue();
}

Ref<SpineTrackEntry> SpineAnimationState::get_current(uint64_t track_index) {
	CHECK_X(NULL);
	Ref<SpineTrackEntry> gd_entry(memnew(SpineTrackEntry));
	auto entry = animation_state->getCurrent(track_index);
	if (entry == NULL) return NULL;
	gd_entry->set_spine_object(entry);
	return gd_entry;
}

#undef CHECK_V
#undef CHECK_X
