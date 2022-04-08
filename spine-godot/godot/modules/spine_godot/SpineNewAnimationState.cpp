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

#include "SpineNewAnimationState.h"

void SpineNewAnimationState::_bind_methods() {
	ClassDB::bind_method(D_METHOD("set_animation", "anim_name", "loop", "track_id"), &SpineNewAnimationState::set_animation, DEFVAL(true), DEFVAL(0));
	ClassDB::bind_method(D_METHOD("update", "delta"), &SpineNewAnimationState::update, DEFVAL(0));
	ClassDB::bind_method(D_METHOD("apply", "skeleton"), &SpineNewAnimationState::apply);
	ClassDB::bind_method(D_METHOD("clear_tracks"), &SpineNewAnimationState::clear_tracks);
	ClassDB::bind_method(D_METHOD("clear_track"), &SpineNewAnimationState::clear_track);
	ClassDB::bind_method(D_METHOD("add_animation", "anim_name", "delay", "loop", "track_id"), &SpineNewAnimationState::add_animation, DEFVAL(0), DEFVAL(true), DEFVAL(0));
	ClassDB::bind_method(D_METHOD("set_empty_animation", "track_id", "mix_duration"), &SpineNewAnimationState::set_empty_animation);
	ClassDB::bind_method(D_METHOD("add_empty_animation", "track_id", "mix_duration", "delay"), &SpineNewAnimationState::add_empty_animation);
	ClassDB::bind_method(D_METHOD("set_empty_animations", "mix_duration"), &SpineNewAnimationState::set_empty_animations);
	ClassDB::bind_method(D_METHOD("get_time_scale"), &SpineNewAnimationState::get_time_scale);
	ClassDB::bind_method(D_METHOD("set_time_scale", "time_scale"), &SpineNewAnimationState::set_time_scale);
	ClassDB::bind_method(D_METHOD("disable_queue"), &SpineNewAnimationState::disable_queue);
	ClassDB::bind_method(D_METHOD("enable_queue"), &SpineNewAnimationState::enable_queue);
	ClassDB::bind_method(D_METHOD("get_current", "track_id"), &SpineNewAnimationState::get_current);
}

SpineNewAnimationState::SpineNewAnimationState() : animation_state(nullptr), skeleton_data_res(nullptr) {
}

SpineNewAnimationState::~SpineNewAnimationState() {
	delete animation_state;
}

void SpineNewAnimationState::set_skeleton_data_res(Ref<SpineNewSkeletonDataResource> data_res) {
	delete animation_state;
	animation_state = nullptr;
	skeleton_data_res = data_res;
	if (!skeleton_data_res.is_valid() || !skeleton_data_res->is_skeleton_data_loaded()) return;
	animation_state = new spine::AnimationState(skeleton_data_res->get_animation_state_data());
}

Ref<SpineNewSkeletonDataResource> SpineNewAnimationState::get_skeleton_data_res() const {
	return skeleton_data_res;
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
Ref<SpineTrackEntry> SpineNewAnimationState::set_animation(const String &anim_name, bool loop, uint64_t track) {
	CHECK_X(nullptr);
	auto skeleton_data = animation_state->getData()->getSkeletonData();
	auto anim = skeleton_data->findAnimation(anim_name.utf8().ptr());
	if (!anim) {
		ERR_PRINT(String("Can not find animation: ") + anim_name);
		return nullptr;
	}
	auto entry = animation_state->setAnimation(track, anim, loop);
	Ref<SpineTrackEntry> gd_entry(memnew(SpineTrackEntry));
	gd_entry->set_spine_object(entry);
	return gd_entry;
}
Ref<SpineTrackEntry> SpineNewAnimationState::add_animation(const String &anim_name, float delay, bool loop, uint64_t track) {
	CHECK_X(nullptr);
	auto skeleton_data = animation_state->getData()->getSkeletonData();
	auto anim = skeleton_data->findAnimation(anim_name.utf8().ptr());
	if (!anim) {
		ERR_PRINT(String("Can not find animation: ") + anim_name);
		return nullptr;
	}
	auto entry = animation_state->addAnimation(track, anim, loop, delay);
	Ref<SpineTrackEntry> gd_entry(memnew(SpineTrackEntry));
	gd_entry->set_spine_object(entry);
	return gd_entry;
}

Ref<SpineTrackEntry> SpineNewAnimationState::set_empty_animation(uint64_t track_id, float mix_duration) {
	CHECK_X(nullptr);
	auto entry = animation_state->setEmptyAnimation(track_id, mix_duration);
	Ref<SpineTrackEntry> gd_entry(memnew(SpineTrackEntry));
	gd_entry->set_spine_object(entry);
	return gd_entry;
}
Ref<SpineTrackEntry> SpineNewAnimationState::add_empty_animation(uint64_t track_id, float mix_duration, float delay) {
	CHECK_X(nullptr);
	auto entry = animation_state->addEmptyAnimation(track_id, mix_duration, delay);
	Ref<SpineTrackEntry> gd_entry(memnew(SpineTrackEntry));
	gd_entry->set_spine_object(entry);
	return gd_entry;
}
void SpineNewAnimationState::set_empty_animations(float mix_duration) {
	CHECK_V;
	animation_state->setEmptyAnimations(mix_duration);
}

void SpineNewAnimationState::update(float delta) {
	CHECK_V;
	animation_state->update(delta);
}
bool SpineNewAnimationState::apply(Ref<SpineNewSkeleton> skeleton) {
	CHECK_X(false);
	return animation_state->apply(*(skeleton->get_spine_object()));
}

void SpineNewAnimationState::clear_tracks() {
	CHECK_V;
	animation_state->clearTracks();
}
void SpineNewAnimationState::clear_track(uint64_t track_id) {
	CHECK_V;
	animation_state->clearTrack(track_id);
}

float SpineNewAnimationState::get_time_scale() {
	CHECK_X(0);
	return animation_state->getTimeScale();
}
void SpineNewAnimationState::set_time_scale(float time_scale) {
	CHECK_V;
	animation_state->setTimeScale(time_scale);
}

void SpineNewAnimationState::disable_queue() {
	CHECK_V;
	animation_state->disableQueue();
}
void SpineNewAnimationState::enable_queue() {
	CHECK_V;
	animation_state->enableQueue();
}

Ref<SpineTrackEntry> SpineNewAnimationState::get_current(uint64_t track_index) {
	CHECK_X(nullptr);
	Ref<SpineTrackEntry> gd_entry(memnew(SpineTrackEntry));
	auto entry = animation_state->getCurrent(track_index);
	if (entry == nullptr) return nullptr;
	gd_entry->set_spine_object(entry);
	return gd_entry;
}

#undef CHECK_V
#undef CHECK_X
