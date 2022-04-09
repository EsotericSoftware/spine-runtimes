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

#ifndef GODOT_SPINETIMELINE_H
#define GODOT_SPINETIMELINE_H

#include "spine/Timeline.h"
#include "SpineConstant.h"
#include "core/reference.h"

class SpineSkeleton;
class SpineEvent;

class SpineTimeline : public Reference {
	GDCLASS(SpineTimeline, Reference);

protected:
	static void _bind_methods();

private:
	spine::Timeline *timeline;

public:
	SpineTimeline();
	~SpineTimeline();

	inline void set_spine_object(spine::Timeline *timeline) { this->timeline = timeline; }
	inline spine::Timeline *get_spine_object() { return timeline; }

	void apply(Ref<SpineSkeleton> skeleton, float lastTime, float time, Array events, float alpha, SpineConstant::MixBlend blend, SpineConstant::MixDirection direction);

	int64_t get_frame_entries();

	int64_t get_frame_count();

	Array get_frames();

	float get_duration();

	Array get_property_ids();

	String get_type();
};

#endif//GODOT_SPINETIMELINE_H
