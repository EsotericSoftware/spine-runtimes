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

#include "SpineBoneData.h"
#include "common.h"

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
	ClassDB::bind_method(D_METHOD("get_transform_mode"), &SpineBoneData::get_transform_mode);
	ClassDB::bind_method(D_METHOD("set_transform_mode", "v"), &SpineBoneData::set_transform_mode);
	ClassDB::bind_method(D_METHOD("is_skin_required"), &SpineBoneData::is_skin_required);
	ClassDB::bind_method(D_METHOD("set_skin_required", "v"), &SpineBoneData::set_skin_required);
	ClassDB::bind_method(D_METHOD("get_color"), &SpineBoneData::get_color);
	ClassDB::bind_method(D_METHOD("set_color", "v"), &SpineBoneData::set_color);
}

SpineBoneData::SpineBoneData() : bone_data(nullptr) {
}

SpineBoneData::~SpineBoneData() {
}

int SpineBoneData::get_index() {
	SPINE_CHECK(bone_data, 0)
	return bone_data->getIndex();
}

String SpineBoneData::get_bone_name() {
	SPINE_CHECK(bone_data, "")
	return bone_data->getName().buffer();
}

Ref<SpineBoneData> SpineBoneData::get_parent() {
	SPINE_CHECK(bone_data, nullptr)
	auto p = bone_data->getParent();
	if (p == nullptr) return nullptr;
	Ref<SpineBoneData> gd_bone_data(memnew(SpineBoneData));
	gd_bone_data->set_spine_object(p);
	return gd_bone_data;
}

float SpineBoneData::get_length() {
	SPINE_CHECK(bone_data, 0)
	return bone_data->getLength();
}

void SpineBoneData::set_length(float v) {
	SPINE_CHECK(bone_data,)
	bone_data->setLength(v);
}

float SpineBoneData::get_x() {
	SPINE_CHECK(bone_data, 0)
	return bone_data->getX();
}

void SpineBoneData::set_x(float v) {
	SPINE_CHECK(bone_data,)
	bone_data->setX(v);
}

float SpineBoneData::get_y() {
	SPINE_CHECK(bone_data, 0)
	return bone_data->getY();
}

void SpineBoneData::set_y(float v) {
	SPINE_CHECK(bone_data,)
	bone_data->setY(v);
}

float SpineBoneData::get_rotation() {
	SPINE_CHECK(bone_data, 0)
	return bone_data->getRotation();
}

void SpineBoneData::set_rotation(float v) {
	SPINE_CHECK(bone_data,)
	bone_data->setRotation(v);
}

float SpineBoneData::get_scale_x() {
	SPINE_CHECK(bone_data, 0)
	return bone_data->getScaleX();
}

void SpineBoneData::set_scale_x(float v) {
	SPINE_CHECK(bone_data,)
	bone_data->setScaleX(v);
}

float SpineBoneData::get_scale_y() {
	SPINE_CHECK(bone_data, 0)
	return bone_data->getScaleY();
}

void SpineBoneData::set_scale_y(float v) {
	SPINE_CHECK(bone_data,)
	bone_data->setScaleY(v);
}

float SpineBoneData::get_shear_x() {
	SPINE_CHECK(bone_data, 0)
	return bone_data->getShearX();
}

void SpineBoneData::set_shear_x(float v) {
	SPINE_CHECK(bone_data,)
	bone_data->setShearX(v);
}

float SpineBoneData::get_shear_y() {
	SPINE_CHECK(bone_data, 0)
	return bone_data->getShearY();
}

void SpineBoneData::set_shear_y(float v) {
	SPINE_CHECK(bone_data,)
	bone_data->setShearY(v);
}

SpineConstant::TransformMode SpineBoneData::get_transform_mode() {
	SPINE_CHECK(bone_data, SpineConstant::TransformMode::TransformMode_Normal)
	return (SpineConstant::TransformMode) bone_data->getTransformMode();
}

void SpineBoneData::set_transform_mode(SpineConstant::TransformMode v) {
	SPINE_CHECK(bone_data,)
	bone_data->setTransformMode((spine::TransformMode) v);
}

bool SpineBoneData::is_skin_required() {
	SPINE_CHECK(bone_data, false)
	return bone_data->isSkinRequired();
}

void SpineBoneData::set_skin_required(bool v) {
	SPINE_CHECK(bone_data,)
	bone_data->setSkinRequired(v);
}

Color SpineBoneData::get_color() {
	SPINE_CHECK(bone_data, Color())
	auto color = bone_data->getColor();
	return Color(color.r, color.g, color.b, color.a);
}

void SpineBoneData::set_color(Color color) {
	SPINE_CHECK(bone_data,)
	bone_data->getColor().set(color.r, color.g, color.b, color.a);
}