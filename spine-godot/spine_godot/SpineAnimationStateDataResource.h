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

#ifndef GODOT_SPINEANIMATIONSTATEDATARESOURCE_H
#define GODOT_SPINEANIMATIONSTATEDATARESOURCE_H

#include "core/variant_parser.h"

#include "SpineSkeletonDataResource.h"

class SpineAnimationStateDataResource : public Resource{
	GDCLASS(SpineAnimationStateDataResource, Resource);

protected:
	static void _bind_methods();

private:
	Ref<SpineSkeletonDataResource> skeleton;

	spine::AnimationStateData *animation_state_data;

	bool animation_state_data_created;

	float default_mix;
public:

	void set_skeleton(const Ref<SpineSkeletonDataResource> &s);
	Ref<SpineSkeletonDataResource> get_skeleton();

	inline spine::AnimationStateData *get_animation_state_data(){
		return animation_state_data;
	}

	void set_default_mix(float m);
	float get_default_mix();

	void set_mix(const String &from, const String &to, float mix_duration);
	float get_mix(const String &from, const String &to);


	void _on_skeleton_data_loaded();
	void _on_skeleton_data_changed();

	bool is_animation_state_data_created();

	SpineAnimationStateDataResource();
	~SpineAnimationStateDataResource();
};

#endif //GODOT_SPINEANIMATIONSTATEDATARESOURCE_H
