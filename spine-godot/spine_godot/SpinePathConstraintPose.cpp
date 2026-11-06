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

#include "SpinePathConstraintPose.h"
#include "SpineCommon.h"

void SpinePathConstraintPose::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_position"), &SpinePathConstraintPose::get_position);
	ClassDB::bind_method(D_METHOD("set_position", "value"), &SpinePathConstraintPose::set_position);
	ClassDB::bind_method(D_METHOD("get_spacing"), &SpinePathConstraintPose::get_spacing);
	ClassDB::bind_method(D_METHOD("set_spacing", "value"), &SpinePathConstraintPose::set_spacing);
	ClassDB::bind_method(D_METHOD("get_mix_rotate"), &SpinePathConstraintPose::get_mix_rotate);
	ClassDB::bind_method(D_METHOD("set_mix_rotate", "value"), &SpinePathConstraintPose::set_mix_rotate);
	ClassDB::bind_method(D_METHOD("get_mix_x"), &SpinePathConstraintPose::get_mix_x);
	ClassDB::bind_method(D_METHOD("set_mix_x", "value"), &SpinePathConstraintPose::set_mix_x);
	ClassDB::bind_method(D_METHOD("get_mix_y"), &SpinePathConstraintPose::get_mix_y);
	ClassDB::bind_method(D_METHOD("set_mix_y", "value"), &SpinePathConstraintPose::set_mix_y);

	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "position", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_position", "get_position");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "spacing", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_spacing", "get_spacing");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "mix_rotate", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_mix_rotate", "get_mix_rotate");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "mix_x", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_mix_x", "get_mix_x");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "mix_y", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_mix_y", "get_mix_y");
}

float SpinePathConstraintPose::get_position() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getPosition();
}

void SpinePathConstraintPose::set_position(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setPosition(value);
}

float SpinePathConstraintPose::get_spacing() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getSpacing();
}

void SpinePathConstraintPose::set_spacing(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setSpacing(value);
}

float SpinePathConstraintPose::get_mix_rotate() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMixRotate();
}

void SpinePathConstraintPose::set_mix_rotate(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMixRotate(value);
}

float SpinePathConstraintPose::get_mix_x() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMixX();
}

void SpinePathConstraintPose::set_mix_x(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMixX(value);
}

float SpinePathConstraintPose::get_mix_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMixY();
}

void SpinePathConstraintPose::set_mix_y(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMixY(value);
}