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

#include "SpineSliderPose.h"
#include "SpineCommon.h"

void SpineSliderPose::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_time"), &SpineSliderPose::get_time);
	ClassDB::bind_method(D_METHOD("set_time", "value"), &SpineSliderPose::set_time);
	ClassDB::bind_method(D_METHOD("get_mix"), &SpineSliderPose::get_mix);
	ClassDB::bind_method(D_METHOD("set_mix", "value"), &SpineSliderPose::set_mix);

	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "time", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_time", "get_time");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "mix", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_mix", "get_mix");
}

float SpineSliderPose::get_time() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getTime();
}

void SpineSliderPose::set_time(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setTime(value);
}

float SpineSliderPose::get_mix() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMix();
}

void SpineSliderPose::set_mix(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMix(value);
}