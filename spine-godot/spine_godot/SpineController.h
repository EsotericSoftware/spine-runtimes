/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

#pragma once

#include "SpineCommon.h"
#include "SpineSkeleton.h"
#include "SpineAnimationState.h"

class SpineSkeletonDataResource;
class SpineTrackEntry;
class SpineEvent;
class SpineSkin;

class SpineControllerListener {
public:
	virtual ~SpineControllerListener() = default;
	virtual void on_animation_started(Ref<SpineAnimationState> animation_state, Ref<SpineTrackEntry> track_entry) = 0;
	virtual void on_animation_interrupted(Ref<SpineAnimationState> animation_state, Ref<SpineTrackEntry> track_entry) = 0;
	virtual void on_animation_ended(Ref<SpineAnimationState> animation_state, Ref<SpineTrackEntry> track_entry) = 0;
	virtual void on_animation_completed(Ref<SpineAnimationState> animation_state, Ref<SpineTrackEntry> track_entry) = 0;
	virtual void on_animation_disposed(Ref<SpineAnimationState> animation_state, Ref<SpineTrackEntry> track_entry) = 0;
	virtual void on_animation_event(Ref<SpineAnimationState> animation_state, Ref<SpineTrackEntry> track_entry, Ref<SpineEvent> event) = 0;
	virtual void before_skeleton_data_change() = 0;
	virtual void skeleton_data_changed() = 0;
	virtual bool should_apply_pose() = 0;
	virtual void before_animation_state_update() = 0;
	virtual void before_animation_state_apply() = 0;
	virtual void before_world_transforms_change() = 0;
	virtual void world_transforms_changed() = 0;
};

class SpineController : public REFCOUNTED, public spine::AnimationStateListenerObject {
	GDCLASS(SpineController, REFCOUNTED)

	Ref<SpineSkeletonDataResource> skeleton_data_res;
	Ref<SpineSkeleton> skeleton;
	Ref<SpineAnimationState> animation_state;
	SpineControllerListener *listener;
	bool modified_bones;
	int native_operation_depth;
	bool skeleton_data_reset_pending;
	Ref<SpineSkeletonDataResource> pending_skeleton_data_res;

protected:
	static void _bind_methods();

private:
	void rebuild();
	void reset_skeleton_data(const Ref<SpineSkeletonDataResource> &skeleton_data_res);

public:
	SpineController();
	~SpineController() override;

	void set_listener(SpineControllerListener *listener);
	void set_skeleton_data_res(const Ref<SpineSkeletonDataResource> &skeleton_data_res);
	Ref<SpineSkeletonDataResource> get_skeleton_data_res() const;
	Ref<SpineSkeleton> get_skeleton() const;
	Ref<SpineAnimationState> get_animation_state() const;
	Ref<SpineSkin> new_skin(const String &name);

	// Keep the owner's scale live: callbacks may change it between update phases.
	bool update_skeleton(float delta, const float &time_scale);
	void set_modified_bones();
	void begin_native_operation();
	bool end_native_operation();
	bool is_skeleton_data_reset_pending() const;

	void callback(spine::AnimationState *state, spine::EventType type, spine::TrackEntry *entry, spine::Event *event) override;
};
