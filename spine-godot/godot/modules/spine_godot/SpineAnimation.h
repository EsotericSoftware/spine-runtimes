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

#ifndef GODOT_SPINEANIMATION_H
#define GODOT_SPINEANIMATION_H

#include "SpineConstant.h"

#include <spine/spine.h>

class SpineEvent;
class SpineSkeleton;
class SpineTimeline;

class SpineAnimation : public Reference {
	GDCLASS(SpineAnimation, Reference);

private:
	spine::Animation *animation;

protected:
	static void _bind_methods();

public:
	SpineAnimation();
	~SpineAnimation();

	inline void set_spine_object(spine::Animation *animation) {
		this->animation = animation;
	}
	inline spine::Animation *get_spine_object() {
		return animation;
	}

	// Vector<Ref<SpineEvent>> pEvents
	void apply(Ref<SpineSkeleton> skeleton, float lastTime, float time, bool loop, Array pEvents, float alpha, SpineConstant::MixBlend blend, SpineConstant::MixDirection direction);

	Array get_timelines();       // Vector<Ref<SpineTimeline>>

	bool has_timeline(Array ids);// Vector<SpineConstant::PropertyId>

	String get_name();

	float get_duration();

	void set_duration(float duration);
};

#endif//GODOT_SPINEANIMATION_H
