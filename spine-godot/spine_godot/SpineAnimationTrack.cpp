#include "SpineAnimationTrack.h"
#include "core/engine.h"
#include "scene/animation/animation_player.h"
#include "scene/resources/animation.h"

#ifdef TOOLS_ENABLED
#include "editor/plugins/animation_player_editor_plugin.h"
#endif

void SpineAnimationTrack::_bind_methods() {
	ClassDB::bind_method(D_METHOD("set_track_index", "track_index"), &SpineAnimationTrack::set_track_index);
	ClassDB::bind_method(D_METHOD("get_track_index"), &SpineAnimationTrack::get_track_index);
	ClassDB::bind_method(D_METHOD("set_animation_name", "animation_name"), &SpineAnimationTrack::set_animation_name);
	ClassDB::bind_method(D_METHOD("get_animation_name"), &SpineAnimationTrack::get_animation_name);
	ClassDB::bind_method(D_METHOD("set_loop", "loop"), &SpineAnimationTrack::set_loop);
	ClassDB::bind_method(D_METHOD("get_loop"), &SpineAnimationTrack::get_loop);
	ClassDB::bind_method(D_METHOD("set_animation_time", "time"), &SpineAnimationTrack::set_animation_time);
	ClassDB::bind_method(D_METHOD("get_animation_time"), &SpineAnimationTrack::get_animation_time);
	ClassDB::bind_method(D_METHOD("update_animation_state", "spine_sprite"), &SpineAnimationTrack::update_animation_state);
	
	ADD_PROPERTY(PropertyInfo(Variant::INT, "track_index", PROPERTY_HINT_RANGE, "0,256,0"), "set_track_index", "get_track_index");
	ADD_PROPERTY(PropertyInfo(Variant::STRING, "animation_name"), "set_animation_name", "get_animation_name");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "loop"), "set_loop", "get_loop");
#if VERSION_MAJOR > 3
	ADD_PROPERTY(PropertyInfo(Variant::FLOAT, "animation_time"), "set_animation_time", "get_animation_time");
#else
	ADD_PROPERTY(PropertyInfo(Variant::REAL, "animation_time"), "set_animation_time", "get_animation_time");
#endif
}

SpineAnimationTrack::SpineAnimationTrack(): track_index(-1), loop(false), animation_time(0), sprite(nullptr) {
}

void SpineAnimationTrack::set_track_index(int _track_index) {
	track_index = _track_index;
}

int SpineAnimationTrack::get_track_index() {
	return track_index;
}

void SpineAnimationTrack::_notification(int what) {
	switch(what) {
	case NOTIFICATION_PARENTED: {
			sprite = Object::cast_to<SpineSprite>(get_parent());
			if (sprite) {
				sprite->connect("before_animation_state_update", this, "update_animation_state");
			} else {
				WARN_PRINT("SpineAnimationTrack parent is not a SpineSprite.");
			}
			NOTIFY_PROPERTY_LIST_CHANGED();
			break;
	}
	case NOTIFICATION_READY: {
			setup_animation_player();
			break;
	}
	case NOTIFICATION_UNPARENTED: {
			if (sprite) {
				sprite->disconnect("before_animation_state_update", this, "update_animation_state");
			}
			break;
	}
	default:
		break;
	}
}

void SpineAnimationTrack::setup_animation_player() {
	if (!sprite) return;
	if (!sprite->get_skeleton_data_res().is_valid() || !sprite->get_skeleton_data_res()->is_skeleton_data_loaded()) return;

	// If we don't have a track index yet, find the highest track number used
	// by existing tracks.
	if (track_index < 0) {
		int highest_track_number = -1;
		for (int i = 0; i < sprite->get_child_count(); i++) {
			auto other_track = Object::cast_to<SpineAnimationTrack>(sprite->get_child(i));
			if (other_track) {
				if (other_track->track_index > highest_track_number) 
					highest_track_number = other_track->track_index;
			}
		}
		track_index = highest_track_number + 1;
	}

	// Find the animation player under the track and reset its animation. Create a new one
	// if there isn't one already.
	AnimationPlayer *animation_player = nullptr;
	for (int i = 0; i < get_child_count(); i++) {
		animation_player = Object::cast_to<AnimationPlayer>(get_child(i));
		if (animation_player) {
			break;
		}
	}

	if (!animation_player) {
		animation_player = memnew(AnimationPlayer);
		animation_player->set_name(String("Track ") + String::num_int64(track_index));
		add_child(animation_player);
		animation_player->set_owner(sprite->get_owner());
	} else {
		List<StringName> animation_names;
		animation_player->get_animation_list(&animation_names);
		for (int i = 0; i < animation_name.size(); i++) {
			animation_player->remove_animation(animation_names[i]);
		}
	}
	
	auto skeleton_data = sprite->get_skeleton_data_res()->get_skeleton_data();
	auto &animations = skeleton_data->getAnimations();
	for (int i = 0; i < animations.size(); i++) {
		auto &animation = animations[i];
		Ref<Animation> animation_ref = create_animation(animation, false);
		animation_player->add_animation(animation_ref->get_name(), animation_ref);
		Ref<Animation> animation_looped_ref = create_animation(animation, true);
		animation_player->add_animation(animation_looped_ref->get_name(), animation_looped_ref);
	}
	Ref<Animation> reset_animation_ref;
	INSTANTIATE(reset_animation_ref);	
	reset_animation_ref->set_name("RESET");
	reset_animation_ref->set_loop(true);
	reset_animation_ref->set_length(0.5f);
	reset_animation_ref->add_track(Animation::TYPE_VALUE);
	reset_animation_ref->track_set_path(0, NodePath(".:animation_name"));	
	reset_animation_ref->track_insert_key(0, 0, "");
	animation_player->add_animation(reset_animation_ref->get_name(), reset_animation_ref);
	animation_player->add_animation("-- Empty --", reset_animation_ref);

	this->animation_player = animation_player;
}

Ref<Animation> SpineAnimationTrack::create_animation(spine::Animation *animation, bool loop) {
	float duration = animation->getDuration();
	if (duration == 0) duration = 0.5;
	
	Ref<Animation> animation_ref;
	INSTANTIATE(animation_ref);	
	animation_ref->set_name(String(animation->getName().buffer()) + (loop ? "" : "_looped"));
	animation_ref->set_loop(!loop);
	animation_ref->set_length(duration);

	animation_ref->add_track(Animation::TYPE_VALUE);
	animation_ref->track_set_path(0, NodePath(".:animation_name"));	
	animation_ref->track_insert_key(0, 0, animation->getName().buffer());
	
	animation_ref->add_track(Animation::TYPE_VALUE);
	animation_ref->track_set_path(1, NodePath(".:animation_time"));
	animation_ref->track_insert_key(1, 0, 0);
	animation_ref->track_insert_key(1, duration, duration);
	
	animation_ref->add_track(Animation::TYPE_VALUE);
	animation_ref->track_set_path(2, NodePath(".:loop"));
	animation_ref->track_insert_key(2, 0, !loop);

	return animation_ref;
}

void SpineAnimationTrack::update_animation_state(const Variant &variant_sprite) {
	sprite = Object::cast_to<SpineSprite>(variant_sprite);
	if (!sprite) return;
	if (!sprite->get_skeleton_data_res().is_valid() || !sprite->get_skeleton_data_res()->is_skeleton_data_loaded()) return;
	if (track_index < 0) return;

	spine::AnimationState *animation_state = sprite->get_animation_state()->get_spine_object();
	if (!animation_state) return;
	spine::Skeleton *skeleton = sprite->get_skeleton()->get_spine_object();
	if (!skeleton) return;

	if (Engine::get_singleton()->is_editor_hint()) {
#ifdef TOOLS_ENABLED
		// When the animation dock is no longer visible, reset the skeleton.
		if (!AnimationPlayerEditor::singleton->is_visible_in_tree()) {
			skeleton->setToSetupPose();
			animation_state->clearTracks();
			animation_state->setTimeScale(1);
			return;
		}
#endif
		
		if (track_index == 0) skeleton->setToSetupPose();
		animation_state->setTimeScale(0);
		animation_state->clearTrack(track_index);
		if (!EMPTY(animation_name)) {
			auto entry = animation_state->setAnimation(track_index, SPINE_STRING(animation_name), loop);
			entry->setMixDuration(0);
			entry->setTrackTime(animation_time);
		}
	} else {
		auto current_entry = animation_state->getCurrent(track_index);
		if (current_entry) {
			if (animation_name != current_entry->getAnimation()->getName().buffer() || current_entry->getLoop() != loop) {
				if (!EMPTY(animation_name))
					animation_state->setAnimation(track_index, SPINE_STRING(animation_name), loop);
				else
					animation_state->setEmptyAnimation(track_index, 0);
			}
		} else {
			if (!EMPTY(animation_name))
				animation_state->setAnimation(track_index, SPINE_STRING(animation_name), loop);
			else
				animation_state->setEmptyAnimation(track_index, 0);
		}
	}
}

void SpineAnimationTrack::set_animation_name(const String& _animation_name) {
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

void SpineAnimationTrack::set_animation_time(float _animation_time) {
	animation_time = _animation_time;
}

float SpineAnimationTrack::get_animation_time() {
	return animation_time;
}
