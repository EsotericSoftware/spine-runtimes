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

#ifndef GODOT_SPINEANIMATIONTRACK_H
#define GODOT_SPINEANIMATIONTRACK_H

#include "SpineCommon.h"
#include "SpineSprite.h"
#include "scene/animation/animation_player.h"
#include "scene/resources/animation.h"

class SpineAnimationTrack : public Node {
	GDCLASS(SpineAnimationTrack, Node)
protected:
	int track_index;
	String animation_name;
	String last_animation_name;
	bool loop;
	float animation_time;
	SpineSprite *sprite;
	AnimationPlayer *animation_player;

	static void _bind_methods();
	void _notification(int what);
	void setup_animation_player();
	Ref<Animation> create_animation(spine::Animation *animation, bool loop);
	void _on_before_world_transforms_change(const Variant& _sprite);
	void update_animation_state(const Variant &variant_sprite);
public:
	SpineAnimationTrack();
	
	void set_track_index(int _track_index);
	int get_track_index();

	void set_animation_name(const String& _animation_name);
	String get_animation_name();

	void set_loop(bool _loop);
	bool get_loop();

	void set_animation_time (float _animation_time);
	float get_animation_time();
};

#endif
