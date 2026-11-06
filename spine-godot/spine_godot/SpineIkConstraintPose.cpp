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

#include "SpineIkConstraintPose.h"
#include "SpineCommon.h"

void SpineIkConstraintPose::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_mix"), &SpineIkConstraintPose::get_mix);
	ClassDB::bind_method(D_METHOD("set_mix", "value"), &SpineIkConstraintPose::set_mix);
	ClassDB::bind_method(D_METHOD("get_softness"), &SpineIkConstraintPose::get_softness);
	ClassDB::bind_method(D_METHOD("set_softness", "value"), &SpineIkConstraintPose::set_softness);
	ClassDB::bind_method(D_METHOD("get_bend_direction"), &SpineIkConstraintPose::get_bend_direction);
	ClassDB::bind_method(D_METHOD("set_bend_direction", "value"), &SpineIkConstraintPose::set_bend_direction);
	ClassDB::bind_method(D_METHOD("get_compress"), &SpineIkConstraintPose::get_compress);
	ClassDB::bind_method(D_METHOD("set_compress", "value"), &SpineIkConstraintPose::set_compress);
	ClassDB::bind_method(D_METHOD("get_stretch"), &SpineIkConstraintPose::get_stretch);
	ClassDB::bind_method(D_METHOD("set_stretch", "value"), &SpineIkConstraintPose::set_stretch);

	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "mix", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_mix", "get_mix");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "softness", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_softness", "get_softness");
	ADD_PROPERTY(PropertyInfo(Variant::INT, "bend_direction", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_bend_direction",
				 "get_bend_direction");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "compress", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_compress", "get_compress");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "stretch", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_stretch", "get_stretch");
}

float SpineIkConstraintPose::get_mix() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMix();
}

void SpineIkConstraintPose::set_mix(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMix(value);
}

float SpineIkConstraintPose::get_softness() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getSoftness();
}

void SpineIkConstraintPose::set_softness(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setSoftness(value);
}

int SpineIkConstraintPose::get_bend_direction() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getBendDirection();
}

void SpineIkConstraintPose::set_bend_direction(int value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setBendDirection(value);
}

bool SpineIkConstraintPose::get_compress() {
	SPINE_CHECK(get_spine_object(), false)
	return get_spine_object()->getCompress();
}

void SpineIkConstraintPose::set_compress(bool value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setCompress(value);
}

bool SpineIkConstraintPose::get_stretch() {
	SPINE_CHECK(get_spine_object(), false)
	return get_spine_object()->getStretch();
}

void SpineIkConstraintPose::set_stretch(bool value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setStretch(value);
}