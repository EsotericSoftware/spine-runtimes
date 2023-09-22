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

#include "SpineAnimationState.h"
#include "SpineTrackEntry.h"

void SpineAnimationState::_bind_methods() {
	ClassDB::bind_method(D_METHOD("update", "delta"), &SpineAnimationState::update, DEFVAL(0));
	ClassDB::bind_method(D_METHOD("apply", "skeleton"), &SpineAnimationState::apply);
	ClassDB::bind_method(D_METHOD("clear_tracks"), &SpineAnimationState::clear_tracks);
	ClassDB::bind_method(D_METHOD("clear_track"), &SpineAnimationState::clear_track);
	ClassDB::bind_method(D_METHOD("get_num_tracks"), &SpineAnimationState::get_num_tracks);
	ClassDB::bind_method(D_METHOD("set_animation", "animation_name", "loop", "track_id"), &SpineAnimationState::set_animation, DEFVAL(true), DEFVAL(0));
	ClassDB::bind_method(D_METHOD("add_animation", "animation_name", "delay", "loop", "track_id"), &SpineAnimationState::add_animation, DEFVAL(0), DEFVAL(true), DEFVAL(0));
	ClassDB::bind_method(D_METHOD("set_empty_animation", "track_id", "mix_duration"), &SpineAnimationState::set_empty_animation);
	ClassDB::bind_method(D_METHOD("add_empty_animation", "track_id", "mix_duration", "delay"), &SpineAnimationState::add_empty_animation);
	ClassDB::bind_method(D_METHOD("set_empty_animations", "mix_duration"), &SpineAnimationState::set_empty_animations);
	ClassDB::bind_method(D_METHOD("get_current", "track_id"), &SpineAnimationState::get_current);
	ClassDB::bind_method(D_METHOD("get_time_scale"), &SpineAnimationState::get_time_scale);
	ClassDB::bind_method(D_METHOD("set_time_scale", "time_scale"), &SpineAnimationState::set_time_scale);
	ClassDB::bind_method(D_METHOD("disable_queue"), &SpineAnimationState::disable_queue);
	ClassDB::bind_method(D_METHOD("enable_queue"), &SpineAnimationState::enable_queue);
}

SpineAnimationState::SpineAnimationState() : animation_state(nullptr), sprite(nullptr) {
}

SpineAnimationState::~SpineAnimationState() {
	delete animation_state;
}

void SpineAnimationState::set_spine_sprite(SpineSprite *_sprite) {
	delete animation_state;
	animation_state = nullptr;
	sprite = _sprite;
	if (!sprite || !sprite->get_skeleton_data_res().is_valid() || !sprite->get_skeleton_data_res()->is_skeleton_data_loaded()) return;
	animation_state = new spine::AnimationState(sprite->get_skeleton_data_res()->get_animation_state_data());
}

void SpineAnimationState::update(float delta) {
	SPINE_CHECK(animation_state, )
	animation_state->update(delta);
}

bool SpineAnimationState::apply(Ref<SpineSkeleton> skeleton) {
	SPINE_CHECK(animation_state, false)
	if (!skeleton->get_spine_object()) return false;
	return animation_state->apply(*(skeleton->get_spine_object()));
}

void SpineAnimationState::clear_tracks() {
	SPINE_CHECK(animation_state, )
	animation_state->clearTracks();
}

void SpineAnimationState::clear_track(int track_id) {
	SPINE_CHECK(animation_state, )
	animation_state->clearTrack(track_id);
}

int SpineAnimationState::get_num_tracks() {
	SPINE_CHECK(animation_state, 0)
	int highest_index = -1;
	for (int i = 0; i < animation_state->getTracks().size(); i++) {
		if (animation_state->getTracks()[i]) highest_index = i;
	}
	return highest_index + 1;
}


Ref<SpineTrackEntry> SpineAnimationState::set_animation(const String &animation_name, bool loop, int track) {
	SPINE_CHECK(animation_state, nullptr)
	auto skeleton_data = animation_state->getData()->getSkeletonData();
	auto animation = skeleton_data->findAnimation(animation_name.utf8().ptr());
	if (!animation) {
		ERR_PRINT(String("Can not find animation: ") + animation_name);
		return nullptr;
	}
	auto track_entry = animation_state->setAnimation(track, animation, loop);
	Ref<SpineTrackEntry> track_entry_ref(memnew(SpineTrackEntry));
	track_entry_ref->set_spine_object(sprite, track_entry);
	return track_entry_ref;
}

Ref<SpineTrackEntry> SpineAnimationState::add_animation(const String &animation_name, float delay, bool loop, int track) {
	SPINE_CHECK(animation_state, nullptr)
	auto skeleton_data = animation_state->getData()->getSkeletonData();
	auto animation = skeleton_data->findAnimation(animation_name.utf8().ptr());
	if (!animation) {
		ERR_PRINT(String("Can not find animation: ") + animation_name);
		return nullptr;
	}
	auto track_entry = animation_state->addAnimation(track, animation, loop, delay);
	Ref<SpineTrackEntry> track_entry_ref(memnew(SpineTrackEntry));
	track_entry_ref->set_spine_object(sprite, track_entry);
	return track_entry_ref;
}

Ref<SpineTrackEntry> SpineAnimationState::set_empty_animation(int track_id, float mix_duration) {
	SPINE_CHECK(animation_state, nullptr)
	auto track_entry = animation_state->setEmptyAnimation(track_id, mix_duration);
	Ref<SpineTrackEntry> track_entry_ref(memnew(SpineTrackEntry));
	track_entry_ref->set_spine_object(sprite, track_entry);
	return track_entry_ref;
}
Ref<SpineTrackEntry> SpineAnimationState::add_empty_animation(int track_id, float mix_duration, float delay) {
	SPINE_CHECK(animation_state, nullptr)
	auto track_entry = animation_state->addEmptyAnimation(track_id, mix_duration, delay);
	Ref<SpineTrackEntry> track_entry_ref(memnew(SpineTrackEntry));
	track_entry_ref->set_spine_object(sprite, track_entry);
	return track_entry_ref;
}
void SpineAnimationState::set_empty_animations(float mix_duration) {
	SPINE_CHECK(animation_state, )
	animation_state->setEmptyAnimations(mix_duration);
}

Ref<SpineTrackEntry> SpineAnimationState::get_current(int track_index) {
	SPINE_CHECK(animation_state, nullptr)
	auto track_entry = animation_state->getCurrent(track_index);
	if (!track_entry) return nullptr;
	Ref<SpineTrackEntry> track_entry_ref(memnew(SpineTrackEntry));
	track_entry_ref->set_spine_object(sprite, track_entry);
	return track_entry_ref;
}

float SpineAnimationState::get_time_scale() {
	SPINE_CHECK(animation_state, 0)
	return animation_state->getTimeScale();
}

void SpineAnimationState::set_time_scale(float time_scale) {
	SPINE_CHECK(animation_state, )
	animation_state->setTimeScale(time_scale);
}

void SpineAnimationState::disable_queue() {
	SPINE_CHECK(animation_state, )
	animation_state->disableQueue();
}

void SpineAnimationState::enable_queue() {
	SPINE_CHECK(animation_state, )
	animation_state->enableQueue();
}
