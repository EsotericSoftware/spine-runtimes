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

#include "SpineTransformConstraintPose.h"
#include "SpineCommon.h"

void SpineTransformConstraintPose::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_mix_rotate"), &SpineTransformConstraintPose::get_mix_rotate);
	ClassDB::bind_method(D_METHOD("set_mix_rotate", "value"), &SpineTransformConstraintPose::set_mix_rotate);
	ClassDB::bind_method(D_METHOD("get_mix_x"), &SpineTransformConstraintPose::get_mix_x);
	ClassDB::bind_method(D_METHOD("set_mix_x", "value"), &SpineTransformConstraintPose::set_mix_x);
	ClassDB::bind_method(D_METHOD("get_mix_y"), &SpineTransformConstraintPose::get_mix_y);
	ClassDB::bind_method(D_METHOD("set_mix_y", "value"), &SpineTransformConstraintPose::set_mix_y);
	ClassDB::bind_method(D_METHOD("get_mix_scale_x"), &SpineTransformConstraintPose::get_mix_scale_x);
	ClassDB::bind_method(D_METHOD("set_mix_scale_x", "value"), &SpineTransformConstraintPose::set_mix_scale_x);
	ClassDB::bind_method(D_METHOD("get_mix_scale_y"), &SpineTransformConstraintPose::get_mix_scale_y);
	ClassDB::bind_method(D_METHOD("set_mix_scale_y", "value"), &SpineTransformConstraintPose::set_mix_scale_y);
	ClassDB::bind_method(D_METHOD("get_mix_shear_y"), &SpineTransformConstraintPose::get_mix_shear_y);
	ClassDB::bind_method(D_METHOD("set_mix_shear_y", "value"), &SpineTransformConstraintPose::set_mix_shear_y);

	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "mix_rotate", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_mix_rotate", "get_mix_rotate");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "mix_x", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_mix_x", "get_mix_x");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "mix_y", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_mix_y", "get_mix_y");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "mix_scale_x", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_mix_scale_x", "get_mix_scale_x");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "mix_scale_y", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_mix_scale_y", "get_mix_scale_y");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "mix_shear_y", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_mix_shear_y", "get_mix_shear_y");
}

float SpineTransformConstraintPose::get_mix_rotate() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMixRotate();
}

void SpineTransformConstraintPose::set_mix_rotate(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMixRotate(value);
}

float SpineTransformConstraintPose::get_mix_x() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMixX();
}

void SpineTransformConstraintPose::set_mix_x(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMixX(value);
}

float SpineTransformConstraintPose::get_mix_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMixY();
}

void SpineTransformConstraintPose::set_mix_y(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMixY(value);
}

float SpineTransformConstraintPose::get_mix_scale_x() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMixScaleX();
}

void SpineTransformConstraintPose::set_mix_scale_x(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMixScaleX(value);
}

float SpineTransformConstraintPose::get_mix_scale_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMixScaleY();
}

void SpineTransformConstraintPose::set_mix_scale_y(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMixScaleY(value);
}

float SpineTransformConstraintPose::get_mix_shear_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMixShearY();
}

void SpineTransformConstraintPose::set_mix_shear_y(float value) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMixShearY(value);
}