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

#include "SpineBoneData.h"
#include "SpineCommon.h"

void SpineBoneData::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_index"), &SpineBoneData::get_index);
	ClassDB::bind_method(D_METHOD("get_bone_name"), &SpineBoneData::get_bone_name);
	ClassDB::bind_method(D_METHOD("get_parent"), &SpineBoneData::get_parent);
	ClassDB::bind_method(D_METHOD("get_length"), &SpineBoneData::get_length);
	ClassDB::bind_method(D_METHOD("set_length", "v"), &SpineBoneData::set_length);
	ClassDB::bind_method(D_METHOD("get_x"), &SpineBoneData::get_x);
	ClassDB::bind_method(D_METHOD("set_x", "v"), &SpineBoneData::set_x);
	ClassDB::bind_method(D_METHOD("get_y"), &SpineBoneData::get_y);
	ClassDB::bind_method(D_METHOD("set_y", "v"), &SpineBoneData::set_y);
	ClassDB::bind_method(D_METHOD("get_rotation"), &SpineBoneData::get_rotation);
	ClassDB::bind_method(D_METHOD("set_rotation", "v"), &SpineBoneData::set_rotation);
	ClassDB::bind_method(D_METHOD("get_scale_x"), &SpineBoneData::get_scale_x);
	ClassDB::bind_method(D_METHOD("set_scale_x", "v"), &SpineBoneData::set_scale_x);
	ClassDB::bind_method(D_METHOD("get_scale_y"), &SpineBoneData::get_scale_y);
	ClassDB::bind_method(D_METHOD("set_scale_y", "v"), &SpineBoneData::set_scale_y);
	ClassDB::bind_method(D_METHOD("get_shear_x"), &SpineBoneData::get_shear_x);
	ClassDB::bind_method(D_METHOD("set_shear_x", "v"), &SpineBoneData::set_shear_x);
	ClassDB::bind_method(D_METHOD("get_shear_y"), &SpineBoneData::get_shear_y);
	ClassDB::bind_method(D_METHOD("set_shear_y", "v"), &SpineBoneData::set_shear_y);
	ClassDB::bind_method(D_METHOD("get_inherit"), &SpineBoneData::get_inherit);
	ClassDB::bind_method(D_METHOD("set_inherit", "v"), &SpineBoneData::set_inherit);
	ClassDB::bind_method(D_METHOD("is_skin_required"), &SpineBoneData::is_skin_required);
	ClassDB::bind_method(D_METHOD("set_skin_required", "v"), &SpineBoneData::set_skin_required);
	ClassDB::bind_method(D_METHOD("get_color"), &SpineBoneData::get_color);
	ClassDB::bind_method(D_METHOD("set_color", "v"), &SpineBoneData::set_color);
	ClassDB::bind_method(D_METHOD("get_icon"), &SpineBoneData::get_icon);
	ClassDB::bind_method(D_METHOD("set_visible", "v"), &SpineBoneData::set_visible);
	ClassDB::bind_method(D_METHOD("is_visible"), &SpineBoneData::is_visible);
}

int SpineBoneData::get_index() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getIndex();
}

String SpineBoneData::get_bone_name() {
	SPINE_CHECK(get_spine_object(), "")
	return get_spine_object()->getName().buffer();
}

Ref<SpineBoneData> SpineBoneData::get_parent() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto parent = get_spine_object()->getParent();
	if (!parent) return nullptr;
	Ref<SpineBoneData> parent_ref(memnew(SpineBoneData));
	parent_ref->set_spine_object(get_spine_owner(), parent);
	return parent_ref;
}

float SpineBoneData::get_length() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getLength();
}

void SpineBoneData::set_length(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setLength(v);
}

float SpineBoneData::get_x() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getX();
}

void SpineBoneData::set_x(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setX(v);
}

float SpineBoneData::get_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getY();
}

void SpineBoneData::set_y(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setY(v);
}

float SpineBoneData::get_rotation() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getRotation();
}

void SpineBoneData::set_rotation(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setRotation(v);
}

float SpineBoneData::get_scale_x() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getScaleX();
}

void SpineBoneData::set_scale_x(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setScaleX(v);
}

float SpineBoneData::get_scale_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getScaleY();
}

void SpineBoneData::set_scale_y(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setScaleY(v);
}

float SpineBoneData::get_shear_x() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getShearX();
}

void SpineBoneData::set_shear_x(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setShearX(v);
}

float SpineBoneData::get_shear_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getShearY();
}

void SpineBoneData::set_shear_y(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setShearY(v);
}

SpineConstant::Inherit SpineBoneData::get_inherit() {
	SPINE_CHECK(get_spine_object(), SpineConstant::Inherit::Inherit_Normal)
	return (SpineConstant::Inherit) get_spine_object()->getInherit();
}

void SpineBoneData::set_inherit(SpineConstant::Inherit v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setInherit((spine::Inherit) v);
}

bool SpineBoneData::is_skin_required() {
	SPINE_CHECK(get_spine_object(), false)
	return get_spine_object()->isSkinRequired();
}

void SpineBoneData::set_skin_required(bool v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setSkinRequired(v);
}

Color SpineBoneData::get_color() {
	SPINE_CHECK(get_spine_object(), Color())
	auto color = get_spine_object()->getColor();
	return Color(color.r, color.g, color.b, color.a);
}

void SpineBoneData::set_color(Color color) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->getColor().set(color.r, color.g, color.b, color.a);
}

String SpineBoneData::get_icon() {
	SPINE_CHECK(get_spine_object(), "")
	return get_spine_object()->getIcon().buffer();
}

bool SpineBoneData::is_visible() {
	SPINE_CHECK(get_spine_object(), true)
	return get_spine_object()->isVisible();
}

void SpineBoneData::set_visible(bool v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setVisible(v);
}