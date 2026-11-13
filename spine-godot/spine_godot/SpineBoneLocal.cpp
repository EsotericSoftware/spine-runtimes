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

#include "SpineBoneLocal.h"
#include "SpineCommon.h"

void SpineBoneLocal::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_x"), &SpineBoneLocal::get_x);
	ClassDB::bind_method(D_METHOD("set_x", "v"), &SpineBoneLocal::set_x);
	ClassDB::bind_method(D_METHOD("get_y"), &SpineBoneLocal::get_y);
	ClassDB::bind_method(D_METHOD("set_y", "v"), &SpineBoneLocal::set_y);
	ClassDB::bind_method(D_METHOD("get_rotation"), &SpineBoneLocal::get_rotation);
	ClassDB::bind_method(D_METHOD("set_rotation", "v"), &SpineBoneLocal::set_rotation);
	ClassDB::bind_method(D_METHOD("get_scale_x"), &SpineBoneLocal::get_scale_x);
	ClassDB::bind_method(D_METHOD("set_scale_x", "v"), &SpineBoneLocal::set_scale_x);
	ClassDB::bind_method(D_METHOD("get_scale_y"), &SpineBoneLocal::get_scale_y);
	ClassDB::bind_method(D_METHOD("set_scale_y", "v"), &SpineBoneLocal::set_scale_y);
	ClassDB::bind_method(D_METHOD("get_shear_x"), &SpineBoneLocal::get_shear_x);
	ClassDB::bind_method(D_METHOD("set_shear_x", "v"), &SpineBoneLocal::set_shear_x);
	ClassDB::bind_method(D_METHOD("get_shear_y"), &SpineBoneLocal::get_shear_y);
	ClassDB::bind_method(D_METHOD("set_shear_y", "v"), &SpineBoneLocal::set_shear_y);
	ClassDB::bind_method(D_METHOD("get_inherit"), &SpineBoneLocal::get_inherit);
	ClassDB::bind_method(D_METHOD("set_inherit", "v"), &SpineBoneLocal::set_inherit);

	// Convenience methods
	ClassDB::bind_method(D_METHOD("set_position", "x", "y"), &SpineBoneLocal::set_position);
	ClassDB::bind_method(D_METHOD("set_scale", "scale_x", "scale_y"), &SpineBoneLocal::set_scale);
	ClassDB::bind_method(D_METHOD("set_scale_uniform", "scale"), &SpineBoneLocal::set_scale_uniform);

	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "x", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_x", "get_x");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "y", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_y", "get_y");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "rotation", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_rotation", "get_rotation");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "scale_x", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_scale_x", "get_scale_x");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "scale_y", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_scale_y", "get_scale_y");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "shear_x", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_shear_x", "get_shear_x");
	ADD_PROPERTY(PropertyInfo(VARIANT_FLOAT, "shear_y", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_NONE), "set_shear_y", "get_shear_y");
	ADD_PROPERTY(PropertyInfo(Variant::INT, "inherit", PROPERTY_HINT_ENUM,
							  "Normal,Only Translation,No Rotation Or Reflection,No Scale,No Scale Or Reflection", PROPERTY_USAGE_NONE),
				 "set_inherit", "get_inherit");
}

float SpineBoneLocal::get_x() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getX();
}

void SpineBoneLocal::set_x(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setX(v);
}

float SpineBoneLocal::get_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getY();
}

void SpineBoneLocal::set_y(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setY(v);
}

float SpineBoneLocal::get_rotation() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getRotation();
}

void SpineBoneLocal::set_rotation(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setRotation(v);
}

float SpineBoneLocal::get_scale_x() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getScaleX();
}

void SpineBoneLocal::set_scale_x(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setScaleX(v);
}

float SpineBoneLocal::get_scale_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getScaleY();
}

void SpineBoneLocal::set_scale_y(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setScaleY(v);
}

float SpineBoneLocal::get_shear_x() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getShearX();
}

void SpineBoneLocal::set_shear_x(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setShearX(v);
}

float SpineBoneLocal::get_shear_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getShearY();
}

void SpineBoneLocal::set_shear_y(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setShearY(v);
}

SpineConstant::Inherit SpineBoneLocal::get_inherit() {
	SPINE_CHECK(get_spine_object(), SpineConstant::Inherit_Normal)
	return (SpineConstant::Inherit) get_spine_object()->getInherit();
}

void SpineBoneLocal::set_inherit(SpineConstant::Inherit inherit) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setInherit((spine::Inherit) inherit);
}

void SpineBoneLocal::set_position(float x, float y) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setPosition(x, y);
}

void SpineBoneLocal::set_scale(float scale_x, float scale_y) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setScale(scale_x, scale_y);
}

void SpineBoneLocal::set_scale_uniform(float scale) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setScale(scale);
}