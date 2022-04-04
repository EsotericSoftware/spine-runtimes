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

#ifndef GODOT_SPINEANIMATIONSTATE_H
#define GODOT_SPINEANIMATIONSTATE_H

#include "core/variant_parser.h"

#include "SpineAnimationStateDataResource.h"
#include "SpineSkeleton.h"
#include "SpineTrackEntry.h"

class SpineAnimationState : public Reference {
	GDCLASS(SpineAnimationState, Reference);

protected:
	static void _bind_methods();

private:
	spine::AnimationState *animation_state;

	Ref<SpineAnimationStateDataResource> anim_state_data_res;

public:
	void load_animation_state(Ref<SpineAnimationStateDataResource> ad);

	inline void set_animation_state(spine::AnimationState *a) {
		animation_state = a;
	}
	inline spine::AnimationState *get_animation_state() {
		return animation_state;
	}

	void reload_animation_state();

	Ref<SpineTrackEntry> set_animation(const String &anim_name, bool loop, uint64_t track_id);
	inline void set_animation_by_ref(Ref<SpineAnimation> anim, bool loop, uint64_t track_id) {
		if (anim.is_valid()) {
			animation_state->setAnimation(track_id, anim->get_spine_object(), loop);
		}
	}
	Ref<SpineTrackEntry> add_animation(const String &anim_name, float delay, bool loop, uint64_t track_id);

	Ref<SpineTrackEntry> set_empty_animation(uint64_t track_id, float mix_duration);
	Ref<SpineTrackEntry> add_empty_animation(uint64_t track_id, float mix_duration, float delay);
	void set_empty_animations(float mix_duration);

	Ref<SpineAnimationStateDataResource> get_data();

	float get_time_scale();
	void set_time_scale(float v);

	void disable_queue();
	void enable_queue();

	void update(float delta);
	bool apply(Ref<SpineSkeleton> skeleton);

	void clear_tracks();
	void clear_track(uint64_t track_id);

	Ref<SpineTrackEntry> get_current(uint64_t track_index);

	SpineAnimationState();
	~SpineAnimationState();
};

#endif//GODOT_SPINEANIMATIONSTATE_H
