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

#ifndef GODOT_SPINENEWANIMATIONSTATE_H
#define GODOT_SPINENEWANIMATIONSTATE_H

#include "SpineNewSkeleton.h"
#include "SpineTrackEntry.h"

class SpineNewAnimationState : public Reference {
	GDCLASS(SpineNewAnimationState, Reference);

protected:
	static void _bind_methods();

private:
	spine::AnimationState *animation_state;
	Ref<SpineNewSkeletonDataResource> skeleton_data_res;

public:
	SpineNewAnimationState();
	~SpineNewAnimationState();

	void set_skeleton_data_res(Ref<SpineNewSkeletonDataResource> skeleton_data_res);
	Ref<SpineNewSkeletonDataResource> get_skeleton_data_res() const;

	inline void set_spine_object(spine::AnimationState *animation_state) { this->animation_state = animation_state; }
	inline spine::AnimationState *get_spine_object() { return animation_state; }

	Ref<SpineTrackEntry> set_animation(const String &anim_name, bool loop, uint64_t track_id);

	Ref<SpineTrackEntry> add_animation(const String &anim_name, float delay, bool loop, uint64_t track_id);

	Ref<SpineTrackEntry> set_empty_animation(uint64_t track_id, float mix_duration);

	Ref<SpineTrackEntry> add_empty_animation(uint64_t track_id, float mix_duration, float delay);
	void set_empty_animations(float mix_duration);

	float get_time_scale();
	void set_time_scale(float time_scale);

	void disable_queue();
	void enable_queue();

	void update(float delta);

	bool apply(Ref<SpineNewSkeleton> skeleton);

	void clear_tracks();

	void clear_track(uint64_t track_id);

	Ref<SpineTrackEntry> get_current(uint64_t track_index);
};

#endif//GODOT_SPINENEWANIMATIONSTATE_H
