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

#pragma once

#include "SpineSprite.h"
#include "scene/animation/animation_player.h"
#include "scene/resources/animation.h"

class SpineAnimationTrack : public Node {
	GDCLASS(SpineAnimationTrack, Node)
protected:
	// These are not exposed in the inspector, see SpineAnimationTrackInspectorPlugin.
	// Instead, they are are keyed by the animations created in setup_animation_player
	// and primarily used for animation player editor support like scrubbing.
	String animation_name;
	bool loop;

	// These can be set by the user.
	int track_index;
	float mix_duration;
	bool hold_previous;
	bool reverse;
	bool shortest_rotation;
	float time_scale;
	float alpha;
	float mix_attachment_threshold;
	float mix_draw_order_threshold;
	SpineConstant::MixBlend mix_blend;
	bool blend_tree_mode;
	bool debug;

	SpineSprite *sprite;

	static void _bind_methods();

	void _notification(int what);

	AnimationPlayer *find_animation_player();

	void setup_animation_player();

	static Ref<Animation> create_animation(spine::Animation *animation, bool loop);

	void update_animation_state(const Variant &variant_sprite);

public:
	SpineAnimationTrack();

	void set_animation_name(const String &_animation_name);

	String get_animation_name();

	void set_animation_time(float _animation_time);

	float get_animation_time();

	void set_loop(bool _loop);

	bool get_loop();

	void set_track_index(int _track_index);

	int get_track_index();

	void set_mix_duration(float _mix_duration);

	float get_mix_duration();

	void set_hold_previous(bool _hold_previous);

	bool get_hold_previous();

	void set_reverse(bool _reverse);

	bool get_reverse();

	void set_shortest_rotation(bool _shortest_rotation);

	bool get_shortest_rotation();

	void set_time_scale(float _time_scale);

	float get_time_scale();

	void set_alpha(float _alpha);

	float get_alpha();

	void set_mix_attachment_threshold(float _mix_attachment_threshold);

	float get_mix_attachment_threshold();

	void set_mix_draw_order_threshold(float _mix_draw_order_threshold);

	float get_mix_draw_order_threshold();

	void set_mix_blend(SpineConstant::MixBlend _blend);

	SpineConstant::MixBlend get_mix_blend();

	void set_blend_tree_mode(bool _blend_tree_mode);

	bool get_blend_tree_mode();

	void set_debug(bool _debug);

	bool get_debug();
};
