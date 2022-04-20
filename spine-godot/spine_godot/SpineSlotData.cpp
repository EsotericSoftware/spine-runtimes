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

#include "SpineSlotData.h"
#include "SpineCommon.h"

void SpineSlotData::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_index"), &SpineSlotData::get_index);
	ClassDB::bind_method(D_METHOD("get_name"), &SpineSlotData::get_name);
	ClassDB::bind_method(D_METHOD("get_bone_data"), &SpineSlotData::get_bone_data);
	ClassDB::bind_method(D_METHOD("get_color"), &SpineSlotData::get_color);
	ClassDB::bind_method(D_METHOD("set_color", "v"), &SpineSlotData::set_color);
	ClassDB::bind_method(D_METHOD("get_dark_color"), &SpineSlotData::get_dark_color);
	ClassDB::bind_method(D_METHOD("set_dark_color", "v"), &SpineSlotData::set_dark_color);
	ClassDB::bind_method(D_METHOD("has_dark_color"), &SpineSlotData::has_dark_color);
	ClassDB::bind_method(D_METHOD("set_has_dark_color", "v"), &SpineSlotData::set_has_dark_color);
	ClassDB::bind_method(D_METHOD("get_attachment_name"), &SpineSlotData::get_attachment_name);
	ClassDB::bind_method(D_METHOD("set_attachment_name", "v"), &SpineSlotData::set_attachment_name);
	ClassDB::bind_method(D_METHOD("get_blend_mode"), &SpineSlotData::get_blend_mode);
	ClassDB::bind_method(D_METHOD("set_blend_mode", "v"), &SpineSlotData::set_blend_mode);

}

SpineSlotData::SpineSlotData() : slot_data(nullptr) {
}

int SpineSlotData::get_index() {
	SPINE_CHECK(slot_data, 0)
	return slot_data->getIndex();
}

String SpineSlotData::get_name() {
	SPINE_CHECK(slot_data, String(""))
	return slot_data->getName().buffer();
}

Ref<SpineBoneData> SpineSlotData::get_bone_data() {
	SPINE_CHECK(slot_data, nullptr)
	auto &bone_data = slot_data->getBoneData();
	Ref<SpineBoneData> bone_data_ref(memnew(SpineBoneData));
	bone_data_ref->set_spine_object(&bone_data);
	return bone_data_ref;
}

Color SpineSlotData::get_color() {
	SPINE_CHECK(slot_data, Color(0, 0, 0, 0))
	auto &color = slot_data->getColor();
	return Color(color.r, color.g, color.b, color.a);
}

void SpineSlotData::set_color(Color v) {
	SPINE_CHECK(slot_data,)
	auto &color = slot_data->getColor();
	color.set(v.r, v.g, v.b, v.a);
}

Color SpineSlotData::get_dark_color() {
	SPINE_CHECK(slot_data, Color(0, 0, 0, 0))
	auto &color = slot_data->getDarkColor();
	return Color(color.r, color.g, color.b, color.a);
}

void SpineSlotData::set_dark_color(Color v) {
	SPINE_CHECK(slot_data,)
	auto &color = slot_data->getDarkColor();
	color.set(v.r, v.g, v.b, v.a);
}

bool SpineSlotData::has_dark_color() {
	SPINE_CHECK(slot_data, false)
	return slot_data->hasDarkColor();
}

void SpineSlotData::set_has_dark_color(bool v) {
	SPINE_CHECK(slot_data,)
	slot_data->setHasDarkColor(v);
}

String SpineSlotData::get_attachment_name() {
	SPINE_CHECK(slot_data, "")
	return slot_data->getAttachmentName().buffer();
}
void SpineSlotData::set_attachment_name(const String &v) {
	SPINE_CHECK(slot_data,)
	slot_data->setAttachmentName(SPINE_STRING(v));
}

SpineConstant::BlendMode SpineSlotData::get_blend_mode() {
	SPINE_CHECK(slot_data, SpineConstant::BLENDMODE_NORMAL)
	return (SpineConstant::BlendMode)slot_data->getBlendMode();
}
void SpineSlotData::set_blend_mode(SpineConstant::BlendMode v) {
	SPINE_CHECK(slot_data,)
	slot_data->setBlendMode((spine::BlendMode) v);
}
