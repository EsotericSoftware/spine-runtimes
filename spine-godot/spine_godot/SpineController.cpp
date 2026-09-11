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

#include "SpineController.h"
#include "SpineEvent.h"
#include "SpineSkeletonDataResource.h"
#include "SpineSkin.h"
#include "SpineTrackEntry.h"

void SpineController::_bind_methods() {
	ADD_SIGNAL(MethodInfo("_internal_spine_objects_invalidated"));
}

SpineController::SpineController()
	: listener(nullptr), modified_bones(false), native_operation_depth(0), skeleton_data_reset_pending(false) {
}

SpineController::~SpineController() {
	listener = nullptr;
	emit_signal(SNAME("_internal_spine_objects_invalidated"));
	if (animation_state.is_valid()) animation_state->clear_spine_controller();
	if (skeleton.is_valid()) skeleton->clear_spine_controller();
	animation_state.unref();
	skeleton.unref();
}

void SpineController::set_listener(SpineControllerListener *_listener) {
	listener = _listener;
}

void SpineController::set_skeleton_data_res(const Ref<SpineSkeletonDataResource> &_skeleton_data_res) {
	if (native_operation_depth > 0) {
		pending_skeleton_data_res = _skeleton_data_res;
		skeleton_data_reset_pending = true;
		return;
	}
	reset_skeleton_data(_skeleton_data_res);
}

void SpineController::reset_skeleton_data(const Ref<SpineSkeletonDataResource> &_skeleton_data_res) {
	native_operation_depth++;
	if (listener) listener->before_skeleton_data_change();
	emit_signal(SNAME("_internal_spine_objects_invalidated"));
	if (animation_state.is_valid()) animation_state->clear_spine_controller();
	if (skeleton.is_valid()) skeleton->clear_spine_controller();
	animation_state.unref();
	skeleton.unref();
	modified_bones = false;
	skeleton_data_res = _skeleton_data_res;
	rebuild();
	if (listener) listener->skeleton_data_changed();
	native_operation_depth--;
	if (native_operation_depth == 0 && skeleton_data_reset_pending) {
		Ref<SpineSkeletonDataResource> new_skeleton_data_res = pending_skeleton_data_res;
		pending_skeleton_data_res.unref();
		skeleton_data_reset_pending = false;
		reset_skeleton_data(new_skeleton_data_res);
	}
}

void SpineController::rebuild() {
	if (!skeleton_data_res.is_valid() || !skeleton_data_res->is_skeleton_data_loaded()) return;

	skeleton = Ref<SpineSkeleton>(memnew(SpineSkeleton));
	skeleton->set_spine_controller(this);
	animation_state = Ref<SpineAnimationState>(memnew(SpineAnimationState));
	animation_state->set_spine_controller(this);
	animation_state->get_spine_object()->setListener(this);

	animation_state->update(0);
	animation_state->apply(skeleton);
	skeleton->update_world_transform(SpineConstant::Physics_Update);
}

Ref<SpineSkeletonDataResource> SpineController::get_skeleton_data_res() const {
	return skeleton_data_res;
}

Ref<SpineSkeleton> SpineController::get_skeleton() const {
	return skeleton;
}

Ref<SpineAnimationState> SpineController::get_animation_state() const {
	return animation_state;
}

Ref<SpineSkin> SpineController::new_skin(const String &name) {
	Ref<SpineSkin> skin = memnew(SpineSkin);
	skin->init(name, this);
	return skin;
}

bool SpineController::update_skeleton(float delta, const float &time_scale) {
	if (!skeleton.is_valid() || !skeleton->get_spine_object() || !animation_state.is_valid() || !animation_state->get_spine_object()) return false;

	begin_native_operation();
	if (listener) listener->before_animation_state_update();
	if (is_skeleton_data_reset_pending()) {
		end_native_operation();
		return false;
	}
	animation_state->update(delta * time_scale);
	if (is_skeleton_data_reset_pending()) {
		end_native_operation();
		return false;
	}
	if (listener && !listener->should_apply_pose()) {
		end_native_operation();
		return false;
	}
	if (listener) listener->before_animation_state_apply();
	if (is_skeleton_data_reset_pending()) {
		end_native_operation();
		return false;
	}
	animation_state->apply(skeleton);
	if (is_skeleton_data_reset_pending()) {
		end_native_operation();
		return false;
	}
	if (listener) listener->before_world_transforms_change();
	if (is_skeleton_data_reset_pending()) {
		end_native_operation();
		return false;
	}
	skeleton->update(delta * time_scale);
	skeleton->update_world_transform(SpineConstant::Physics_Update);
	modified_bones = false;
	if (listener) listener->world_transforms_changed();
	if (is_skeleton_data_reset_pending()) {
		end_native_operation();
		return false;
	}
	if (modified_bones) skeleton->update_world_transform(SpineConstant::Physics_Update);
	return !end_native_operation();
}

void SpineController::set_modified_bones() {
	modified_bones = true;
}

void SpineController::begin_native_operation() {
	native_operation_depth++;
}

bool SpineController::end_native_operation() {
	if (native_operation_depth == 0) return false;
	native_operation_depth--;
	if (native_operation_depth > 0 || !skeleton_data_reset_pending) return false;
	Ref<SpineSkeletonDataResource> new_skeleton_data_res = pending_skeleton_data_res;
	pending_skeleton_data_res.unref();
	skeleton_data_reset_pending = false;
	reset_skeleton_data(new_skeleton_data_res);
	return true;
}

bool SpineController::is_skeleton_data_reset_pending() const {
	return skeleton_data_reset_pending;
}

void SpineController::callback(spine::AnimationState *state, spine::EventType type, spine::TrackEntry *entry, spine::Event *event) {
	if (!listener) return;
	Ref<SpineTrackEntry> entry_ref = Ref<SpineTrackEntry>(memnew(SpineTrackEntry));
	entry_ref->set_spine_object(this, entry);
	Ref<SpineEvent> event_ref(nullptr);
	if (event) {
		event_ref = Ref<SpineEvent>(memnew(SpineEvent));
		event_ref->set_spine_object(this, event);
	}

	switch (type) {
		case spine::EventType_Start:
			listener->on_animation_started(animation_state, entry_ref);
			break;
		case spine::EventType_Interrupt:
			listener->on_animation_interrupted(animation_state, entry_ref);
			break;
		case spine::EventType_End:
			listener->on_animation_ended(animation_state, entry_ref);
			break;
		case spine::EventType_Complete:
			listener->on_animation_completed(animation_state, entry_ref);
			break;
		case spine::EventType_Dispose:
			listener->on_animation_disposed(animation_state, entry_ref);
			break;
		case spine::EventType_Event:
			listener->on_animation_event(animation_state, entry_ref, event_ref);
			break;
	}
}
