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

#include "SpineAnimationTrack.h"
#if VERSION_MAJOR > 3
#include "core/config/engine.h"
#else
#include "core/engine.h"
#endif
#include "scene/animation/animation_player.h"
#include "scene/resources/animation.h"

#ifdef TOOLS_ENABLED
#include "godot/editor/editor_node.h"
#include "editor/plugins/animation_player_editor_plugin.h"
#include "editor/plugins/animation_tree_editor_plugin.h"
#endif

void SpineAnimationTrack::_bind_methods() {
	ClassDB::bind_method(D_METHOD("set_animation_name", "animation_name"), &SpineAnimationTrack::set_animation_name);
	ClassDB::bind_method(D_METHOD("get_animation_name"), &SpineAnimationTrack::get_animation_name);
	ClassDB::bind_method(D_METHOD("set_loop", "loop"), &SpineAnimationTrack::set_loop);
	ClassDB::bind_method(D_METHOD("get_loop"), &SpineAnimationTrack::get_loop);

	ClassDB::bind_method(D_METHOD("set_track_index", "track_index"), &SpineAnimationTrack::set_track_index);
	ClassDB::bind_method(D_METHOD("get_track_index"), &SpineAnimationTrack::get_track_index);
	ClassDB::bind_method(D_METHOD("set_mix_duration", "mix_duration"), &SpineAnimationTrack::set_mix_duration);
	ClassDB::bind_method(D_METHOD("get_mix_duration"), &SpineAnimationTrack::get_mix_duration);
	ClassDB::bind_method(D_METHOD("set_hold_previous", "hold_previous"), &SpineAnimationTrack::set_hold_previous);
	ClassDB::bind_method(D_METHOD("get_hold_previous"), &SpineAnimationTrack::get_hold_previous);
	ClassDB::bind_method(D_METHOD("set_reverse", "reverse"), &SpineAnimationTrack::set_reverse);
	ClassDB::bind_method(D_METHOD("get_reverse"), &SpineAnimationTrack::get_reverse);
	ClassDB::bind_method(D_METHOD("set_shortest_rotation", "shortest_rotation"), &SpineAnimationTrack::set_shortest_rotation);
	ClassDB::bind_method(D_METHOD("get_shortest_rotation"), &SpineAnimationTrack::get_shortest_rotation);
	ClassDB::bind_method(D_METHOD("set_time_scale", "time_scale"), &SpineAnimationTrack::set_time_scale);
	ClassDB::bind_method(D_METHOD("get_time_scale"), &SpineAnimationTrack::get_time_scale);
	ClassDB::bind_method(D_METHOD("set_alpha", "alpha"), &SpineAnimationTrack::set_alpha);
	ClassDB::bind_method(D_METHOD("get_alpha"), &SpineAnimationTrack::get_alpha);
	ClassDB::bind_method(D_METHOD("set_mix_attachment_threshold", "mix_attachment_threshold"), &SpineAnimationTrack::set_mix_attachment_threshold);
	ClassDB::bind_method(D_METHOD("get_mix_attachment_threshold"), &SpineAnimationTrack::get_mix_attachment_threshold);
	ClassDB::bind_method(D_METHOD("set_mix_draw_order_threshold", "mix_draw_order_threshold"), &SpineAnimationTrack::set_mix_draw_order_threshold);
	ClassDB::bind_method(D_METHOD("get_mix_draw_order_threshold"), &SpineAnimationTrack::get_mix_draw_order_threshold);
	ClassDB::bind_method(D_METHOD("set_mix_blend", "mix_blend"), &SpineAnimationTrack::set_mix_blend);
	ClassDB::bind_method(D_METHOD("get_mix_blend"), &SpineAnimationTrack::get_mix_blend);
	ClassDB::bind_method(D_METHOD("set_blend_tree_mode", "blend_tree_mode_enabled"), &SpineAnimationTrack::set_blend_tree_mode);
	ClassDB::bind_method(D_METHOD("get_blend_tree_mode"), &SpineAnimationTrack::get_blend_tree_mode);
	ClassDB::bind_method(D_METHOD("set_debug", "debug"), &SpineAnimationTrack::set_debug);
	ClassDB::bind_method(D_METHOD("get_debug"), &SpineAnimationTrack::get_debug);

	ClassDB::bind_method(D_METHOD("update_animation_state", "spine_sprite"), &SpineAnimationTrack::update_animation_state);

	ADD_PROPERTY(PropertyInfo(Variant::STRING, "animation_name", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_STORAGE | PROPERTY_USAGE_INTERNAL | PROPERTY_USAGE_NOEDITOR), "set_animation_name", "get_animation_name");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "loop", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_STORAGE | PROPERTY_USAGE_INTERNAL | PROPERTY_USAGE_NOEDITOR), "set_loop", "get_loop");

	ADD_PROPERTY(PropertyInfo(Variant::INT, "track_index", PROPERTY_HINT_RANGE, "0,256,0"), "set_track_index", "get_track_index");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "mix_duration"), "set_mix_duration", "get_mix_duration");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "hold_previous"), "set_hold_previous", "get_hold_previous");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "reverse"), "set_reverse", "get_reverse");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "shortest_rotation"), "set_shortest_rotation", "get_shortest_rotation");
	ADD_PROPERTY(PropertyInfo(Variant::VARIANT_FLOAT, "time_scale"), "set_time_scale", "get_time_scale");
	ADD_PROPERTY(PropertyInfo(Variant::VARIANT_FLOAT, "alpha"), "set_alpha", "get_alpha");
	ADD_PROPERTY(PropertyInfo(Variant::VARIANT_FLOAT, "attachment_threshold"), "set_mix_attachment_threshold", "get_mix_attachment_threshold");
	ADD_PROPERTY(PropertyInfo(Variant::VARIANT_FLOAT, "draw_order_threshold"), "set_mix_draw_order_threshold", "get_mix_draw_order_threshold");
	ADD_PROPERTY(PropertyInfo(Variant::INT, "mix_blend", PROPERTY_HINT_ENUM, "Setup,First,Replace,Add"), "set_mix_blend", "get_mix_blend");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "blend_tree_mode"), "set_blend_tree_mode", "get_blend_tree_mode");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "debug"), "set_debug", "get_debug");
}

SpineAnimationTrack::SpineAnimationTrack() : loop(false),
											 track_index(-1),
											 mix_duration(-1),
											 hold_previous(false),
											 reverse(false),
											 shortest_rotation(false),
											 time_scale(1),
											 alpha(1),
											 mix_attachment_threshold(0),
											 mix_draw_order_threshold(0),
											 mix_blend(SpineConstant::MixBlend_Replace),
											 blend_tree_mode(false),
											 debug(false),
											 sprite(nullptr) {
}

void SpineAnimationTrack::_notification(int what) {
	switch (what) {
		case NOTIFICATION_PARENTED: {
			sprite = Object::cast_to<SpineSprite>(get_parent());
			if (sprite)
#if VERSION_MAJOR > 3
				sprite->connect(SNAME("before_animation_state_update"), callable_mp(this, &SpineAnimationTrack::update_animation_state));
#else
				sprite->connect(SNAME("before_animation_state_update"), this, SNAME("update_animation_state"));
#endif
			NOTIFY_PROPERTY_LIST_CHANGED();
			break;
		}
		case NOTIFICATION_READY: {
			setup_animation_player();
			break;
		}
		case NOTIFICATION_UNPARENTED: {
			if (sprite) {
#if VERSION_MAJOR > 3
				sprite->disconnect(SNAME("before_animation_state_update"), callable_mp(this, &SpineAnimationTrack::update_animation_state));
#else
				sprite->disconnect(SNAME("before_animation_state_update"), this, SNAME("update_animation_state"));
#endif
				sprite = nullptr;
			}
			break;
		}
		default:
			break;
	}
}

AnimationPlayer *SpineAnimationTrack::find_animation_player() {
	AnimationPlayer *animation_player = nullptr;
	for (int i = 0; i < get_child_count(); i++) {
		animation_player = cast_to<AnimationPlayer>(get_child(i));
		if (animation_player) {
			break;
		}
	}
	return animation_player;
}

void SpineAnimationTrack::setup_animation_player() {
	if (!sprite) return;
	if (!sprite->get_skeleton_data_res().is_valid() || !sprite->get_skeleton_data_res()->is_skeleton_data_loaded()) return;
	AnimationPlayer *animation_player = find_animation_player();

	// If we don't have a track index yet, find the highest track number used
	// by existing tracks.
	if (track_index < 0) {
		int highest_track_number = -1;
		for (int i = 0; i < sprite->get_child_count(); i++) {
			auto other_track = cast_to<SpineAnimationTrack>(sprite->get_child(i));
			if (other_track) {
				if (other_track->track_index > highest_track_number)
					highest_track_number = other_track->track_index;
			}
		}
		track_index = highest_track_number + 1;
	}

	// Find the animation player under the track and reset its animation. Create a new one
	// if there isn't one already.
	if (!animation_player) {
		animation_player = memnew(AnimationPlayer);
		animation_player->set_name(String("{0} Track {1}").format(varray(sprite->get_name(), String::num_int64(track_index))));
		add_child(animation_player);
		animation_player->set_owner(sprite->get_owner());
	} else {
#if VERSION_MAJOR > 3
		List<StringName> animation_libraries;
		animation_player->get_animation_library_list(&animation_libraries);
		for (int i = 0; i < animation_libraries.size(); i++) {
			animation_player->remove_animation_library(animation_libraries[i]);
		}
#else
		List<StringName> animation_names;
		animation_player->get_animation_list(&animation_names);
		for (int i = 0; i < animation_names.size(); i++) {
			animation_player->remove_animation(animation_names[i]);
		}
#endif
	}

	auto skeleton_data = sprite->get_skeleton_data_res()->get_skeleton_data();
	auto &animations = skeleton_data->getAnimations();
#if VERSION_MAJOR > 3
	Ref<AnimationLibrary> animation_library;
	animation_library.instantiate();
	animation_player->add_animation_library("", animation_library);
#endif
	for (int i = 0; i < (int) animations.size(); i++) {
		auto &animation = animations[i];
		Ref<Animation> animation_ref = create_animation(animation, false);
		Ref<Animation> animation_looped_ref = create_animation(animation, true);
#if VERSION_MAJOR > 3
		animation_library->add_animation(animation_ref->get_name(), animation_ref);
		animation_library->add_animation(animation_looped_ref->get_name(), animation_looped_ref);
#else
		animation_player->add_animation(animation_ref->get_name(), animation_ref);
		animation_player->add_animation(animation_looped_ref->get_name(), animation_looped_ref);
#endif
	}

	Ref<Animation> reset_animation_ref;
	INSTANTIATE(reset_animation_ref);
	reset_animation_ref->set_name("RESET");
#if VERSION_MAJOR > 3
	// reset_animation_ref->set_loop(true);
#else
	reset_animation_ref->set_loop(true);
#endif
	reset_animation_ref->set_length(0.5f);
	reset_animation_ref->add_track(Animation::TYPE_VALUE);
	reset_animation_ref->track_set_path(0, NodePath(".:animation_name"));
	reset_animation_ref->track_insert_key(0, 0, "");
	reset_animation_ref->add_track(Animation::TYPE_VALUE);
	reset_animation_ref->track_set_path(1, NodePath(".:loop"));
	reset_animation_ref->track_insert_key(1, 0, false);
#if VERSION_MAJOR > 3
	animation_library->add_animation(reset_animation_ref->get_name(), reset_animation_ref);
	animation_library->add_animation("-- Empty --", reset_animation_ref);
#else
	animation_player->add_animation(reset_animation_ref->get_name(), reset_animation_ref);
	animation_player->add_animation("-- Empty --", reset_animation_ref);
#endif
}

Ref<Animation> SpineAnimationTrack::create_animation(spine::Animation *animation, bool loop) {
	float duration = animation->getDuration();
	if (duration == 0) duration = 0.5;

	Ref<Animation> animation_ref;
	INSTANTIATE(animation_ref);
	animation_ref->set_name(String(animation->getName().buffer()) + (loop ? "" : "_looped"));
#if VERSION_MAJOR > 3
	// animation_ref->set_loop(!loop);
#else
	animation_ref->set_loop(!loop);
#endif
	animation_ref->set_length(duration);

	animation_ref->add_track(Animation::TYPE_VALUE);
	animation_ref->track_set_path(0, NodePath(".:animation_name"));
	animation_ref->track_insert_key(0, 0, animation->getName().buffer());

	animation_ref->add_track(Animation::TYPE_VALUE);
	animation_ref->track_set_path(1, NodePath(".:loop"));
	animation_ref->track_insert_key(1, 0, !loop);

	return animation_ref;
}

void SpineAnimationTrack::update_animation_state(const Variant &variant_sprite) {
	if (track_index < 0) return;
	sprite = Object::cast_to<SpineSprite>(variant_sprite);
	if (!sprite) return;
	if (!sprite->get_skeleton_data_res().is_valid() || !sprite->get_skeleton_data_res()->is_skeleton_data_loaded()) return;
	if (!sprite->get_skeleton().is_valid() || !sprite->get_animation_state().is_valid()) return;
	spine::AnimationState *animation_state = sprite->get_animation_state()->get_spine_object();
	if (!animation_state) return;
	spine::Skeleton *skeleton = sprite->get_skeleton()->get_spine_object();
	if (!skeleton) return;
	AnimationPlayer *animation_player = find_animation_player();
	if (!animation_player) return;

	if (Engine::get_singleton()->is_editor_hint()) {
#ifdef TOOLS_ENABLED
		if (blend_tree_mode) {
			AnimationTreeEditor *tree_editor = AnimationTreeEditor::get_singleton();
			// When the animation tree dock is no longer visible, bail.
			if (!tree_editor->is_visible_in_tree()) {
				skeleton->setToSetupPose();
				animation_state->clearTracks();
				animation_state->setTimeScale(1);
				return;
			}
			auto current_entry = animation_state->getCurrent(track_index);
			bool should_set_mix = mix_duration >= 0;
			bool should_set_animation = !current_entry || (animation_name != current_entry->getAnimation()->getName().buffer() || current_entry->getLoop() != loop);

			if (should_set_animation) {
				if (!EMPTY(animation_name)) {
					auto entry = animation_state->setAnimation(track_index, SPINE_STRING(animation_name), loop);
					if (should_set_mix) entry->setMixDuration(mix_duration);

					entry->setHoldPrevious(hold_previous);
					entry->setReverse(reverse);
					entry->setShortestRotation(shortest_rotation);
					entry->setTimeScale(time_scale);
					entry->setAlpha(alpha);
					entry->setMixAttachmentThreshold(mix_attachment_threshold);
					entry->setMixDrawOrderThreshold(mix_draw_order_threshold);
					entry->setMixBlend((spine::MixBlend) mix_blend);

					if (debug) print_line(String("Setting animation {0} with mix_duration {1} on track {2} on {3}").format(varray(animation_name, mix_duration, track_index, sprite->get_name())).utf8().ptr());
				} else {
					if (!current_entry || (String("<empty>") != current_entry->getAnimation()->getName().buffer())) {
						auto entry = animation_state->setEmptyAnimation(track_index, should_set_mix ? mix_duration : 0);
						entry->setTrackEnd(FLT_MAX);
						if (debug) print_line(String("Setting empty animation with mix_duration {0} on track {1} on {2}").format(varray(mix_duration, track_index, sprite->get_name())).utf8().ptr());
					}
				}
			}
			return;
		}

		// When the animation dock is no longer visible or we aren't being
		// keyed in the current animation, bail.
#if VERSION_MAJOR > 3
		auto player_editor = AnimationPlayerEditor::get_singleton();
#else
		auto player_editor = AnimationPlayerEditor::singleton;
#endif
		if (!player_editor->is_visible_in_tree()) {
			skeleton->setToSetupPose();
			animation_state->clearTracks();
			animation_state->setTimeScale(1);
			return;
		}

		// Check if the player is actually editing an animation for which there is a track
		// for us.
		Ref<Animation> edited_animation = player_editor->get_track_editor()->get_current_animation();
		if (!edited_animation.is_valid()) {
			skeleton->setToSetupPose();
			animation_state->clearTracks();
			animation_state->setTimeScale(1);
			return;
		}

		int found_track_index = -1;
		auto scene_path = EditorNode::get_singleton()->get_edited_scene()->get_path();
		auto animation_player_path = scene_path.rel_path_to(animation_player->get_path());
		for (int i = 0; i < edited_animation->get_track_count(); i++) {
			auto path = edited_animation->track_get_path(i);
			if (path == animation_player_path) {
				found_track_index = i;
				break;
			}
		}

		// if we are track 0, set the skeleton to the setup pose
		// and the animation state time scale to 0, as we are
		// setting track times manually. Also, kill anything
		// currently in the track.
		if (track_index == 0) {
			skeleton->setToSetupPose();
			animation_state->setTimeScale(0);
		}
		animation_state->clearTrack(track_index);
		if (found_track_index == -1) return;

		// If no animation is set or it's set to "[stop]", we are done.
		if (EMPTY(animation_name) || animation_name == "[stop]") return;

		// If there's no keys on the timeline for this track, we are done.
		if (edited_animation->track_get_key_count(found_track_index) == 0) return;

		// Find the key in the track that matches the editor's playback position
		auto playback_position = player_editor->get_player()->get_current_animation_position();
		int key_index = -1;
		for (int i = 0; i < edited_animation->track_get_key_count(found_track_index); i++) {
			float key_time = edited_animation->track_get_key_time(found_track_index, i);
			if (key_time <= playback_position) {
				key_index = i;
			} else {
				// epsilon compare key and playback time, as playback time is imprecise
				if (fabs(key_time - playback_position) < edited_animation->get_step()) {
					key_index = i;
				}
				break;
			}
		}

		// No key found? bail.
		if (key_index == -1) return;

		// Get the animation from our player for the key
		float key_time = edited_animation->track_get_key_time(found_track_index, key_index);
		String key_value = edited_animation->track_get_key_value(found_track_index, key_index);
		Ref<Animation> keyed_animation = animation_player->get_animation(key_value);
		if (!keyed_animation.is_valid()) return;

		// Calculate the track time and setup the track entry based on the currently keyed
		// properties.
		float track_time = (playback_position - key_time) * time_scale;
		if (track_time < 0) track_time = 0;
		auto entry = animation_state->setAnimation(track_index, SPINE_STRING(animation_name), loop);
		entry->setMixDuration(0);
		entry->setTrackTime(track_time);

		entry->setHoldPrevious(hold_previous);
		entry->setReverse(reverse);
		entry->setShortestRotation(shortest_rotation);
		entry->setAlpha(alpha);
		entry->setMixAttachmentThreshold(mix_attachment_threshold);
		entry->setMixDrawOrderThreshold(mix_draw_order_threshold);
		entry->setMixBlend((spine::MixBlend) mix_blend);
#endif
	} else {
		if (animation_player->is_playing()) {
			auto current_entry = animation_state->getCurrent(track_index);
			bool should_set_mix = mix_duration >= 0;
			bool should_set_animation = !current_entry || (animation_name != current_entry->getAnimation()->getName().buffer() || current_entry->getLoop() != loop);

			if (should_set_animation) {
				if (!EMPTY(animation_name)) {
					auto entry = animation_state->setAnimation(track_index, SPINE_STRING(animation_name), loop);
					if (should_set_mix) entry->setMixDuration(mix_duration);

					entry->setHoldPrevious(hold_previous);
					entry->setReverse(reverse);
					entry->setShortestRotation(shortest_rotation);
					entry->setTimeScale(time_scale);
					entry->setAlpha(alpha);
					entry->setMixAttachmentThreshold(mix_attachment_threshold);
					entry->setMixDrawOrderThreshold(mix_draw_order_threshold);
					entry->setMixBlend((spine::MixBlend) mix_blend);

					if (debug) print_line(String("Setting animation {0} with mix_duration {1} on track {2} on {3}").format(varray(animation_name, mix_duration, track_index, sprite->get_name())).utf8().ptr());
				} else {
					if (!current_entry || (String("<empty>") != current_entry->getAnimation()->getName().buffer())) {
						auto entry = animation_state->setEmptyAnimation(track_index, should_set_mix ? mix_duration : 0);
						entry->setTrackEnd(FLT_MAX);
						if (debug) print_line(String("Setting empty animation with mix_duration {0} on track {1} on {2}").format(varray(mix_duration, track_index, sprite->get_name())).utf8().ptr());
					}
				}
			}
		}
	}
}

void SpineAnimationTrack::set_animation_name(const String &_animation_name) {
	animation_name = _animation_name;
}

String SpineAnimationTrack::get_animation_name() {
	return animation_name;
}

void SpineAnimationTrack::set_loop(bool _loop) {
	loop = _loop;
}

bool SpineAnimationTrack::get_loop() {
	return loop;
}

void SpineAnimationTrack::set_track_index(int _track_index) {
	track_index = _track_index;
}

int SpineAnimationTrack::get_track_index() {
	return track_index;
}

void SpineAnimationTrack::set_mix_duration(float _mix_duration) {
	mix_duration = _mix_duration;
}

float SpineAnimationTrack::get_mix_duration() {
	return mix_duration;
}

void SpineAnimationTrack::set_hold_previous(bool _hold_previous) {
	hold_previous = _hold_previous;
}

bool SpineAnimationTrack::get_hold_previous() {
	return hold_previous;
}

void SpineAnimationTrack::set_reverse(bool _reverse) {
	reverse = _reverse;
}

bool SpineAnimationTrack::get_reverse() {
	return reverse;
}

void SpineAnimationTrack::set_shortest_rotation(bool _shortest_rotation) {
	shortest_rotation = _shortest_rotation;
}

bool SpineAnimationTrack::get_shortest_rotation() {
	return shortest_rotation;
}

void SpineAnimationTrack::set_time_scale(float _time_scale) {
	time_scale = _time_scale;
}

float SpineAnimationTrack::get_time_scale() {
	return time_scale;
}

void SpineAnimationTrack::set_alpha(float _alpha) {
	alpha = _alpha;
}

float SpineAnimationTrack::get_alpha() {
	return alpha;
}

void SpineAnimationTrack::set_mix_attachment_threshold(float _mix_attachment_threshold) {
	mix_attachment_threshold = _mix_attachment_threshold;
}

float SpineAnimationTrack::get_mix_attachment_threshold() {
	return mix_attachment_threshold;
}

void SpineAnimationTrack::set_mix_draw_order_threshold(float _mix_draw_order_threshold) {
	mix_draw_order_threshold = _mix_draw_order_threshold;
}

float SpineAnimationTrack::get_mix_draw_order_threshold() {
	return mix_draw_order_threshold;
}

void SpineAnimationTrack::set_mix_blend(SpineConstant::MixBlend _blend) {
	mix_blend = _blend;
}

SpineConstant::MixBlend SpineAnimationTrack::get_mix_blend() {
	return mix_blend;
}

void SpineAnimationTrack::set_blend_tree_mode(bool _blend_tree_mode) {
	blend_tree_mode = _blend_tree_mode;
}

bool SpineAnimationTrack::get_blend_tree_mode() {
	return blend_tree_mode;
}

void SpineAnimationTrack::set_debug(bool _debug) {
	debug = _debug;
}

bool SpineAnimationTrack::get_debug() {
	return debug;
}
