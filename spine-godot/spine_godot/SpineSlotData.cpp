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

void SpineSlotData::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_index"), &SpineSlotData::get_index);
	ClassDB::bind_method(D_METHOD("get_slot_name"), &SpineSlotData::get_slot_name);
	ClassDB::bind_method(D_METHOD("get_bone_data"), &SpineSlotData::get_bone_data);
	ClassDB::bind_method(D_METHOD("get_color"), &SpineSlotData::get_color);
	ClassDB::bind_method(D_METHOD("get_dark_color"), &SpineSlotData::get_dark_color);
	ClassDB::bind_method(D_METHOD("has_dark_color"), &SpineSlotData::has_dark_color);
	ClassDB::bind_method(D_METHOD("set_has_dark_color", "v"), &SpineSlotData::set_has_dark_color);
	ClassDB::bind_method(D_METHOD("get_attachment_name"), &SpineSlotData::get_attachment_name);
	ClassDB::bind_method(D_METHOD("set_attachment_name", "v"), &SpineSlotData::set_attachment_name);
	ClassDB::bind_method(D_METHOD("get_blend_mode"), &SpineSlotData::get_blend_mode);
	ClassDB::bind_method(D_METHOD("set_blend_mode", "v"), &SpineSlotData::set_blend_mode);

	ClassDB::bind_method(D_METHOD("set_color", "v"), &SpineSlotData::set_color);
	ClassDB::bind_method(D_METHOD("set_dark_color", "v"), &SpineSlotData::set_dark_color);

	BIND_ENUM_CONSTANT(BLENDMODE_NORMAL);
	BIND_ENUM_CONSTANT(BLENDMODE_ADDITIVE);
	BIND_ENUM_CONSTANT(BLENDMODE_MULTIPLY);
	BIND_ENUM_CONSTANT(BLENDMODE_SCREEN);
}

SpineSlotData::SpineSlotData() : slot_data(NULL) {}
SpineSlotData::~SpineSlotData() {}

#define S_T(x) (spine::String(x.utf8()))
int SpineSlotData::get_index() {
	return slot_data->getIndex();
}

String SpineSlotData::get_slot_name() {
	return slot_data->getName().buffer();
}

Ref<SpineBoneData> SpineSlotData::get_bone_data() {
	auto &bd = slot_data->getBoneData();
	Ref<SpineBoneData> gd_bone_data(memnew(SpineBoneData));
	gd_bone_data->set_spine_object(&bd);
	return gd_bone_data;
}

Color SpineSlotData::get_color() {
	auto &c = slot_data->getColor();
	return Color(c.r, c.g, c.b, c.a);
}
void SpineSlotData::set_color(Color v) {
	auto &c = slot_data->getColor();
	c.set(v.r, v.g, v.b, v.a);
}

Color SpineSlotData::get_dark_color() {
	auto &c = slot_data->getDarkColor();
	return Color(c.r, c.g, c.b, c.a);
}
void SpineSlotData::set_dark_color(Color v) {
	auto &c = slot_data->getDarkColor();
	c.set(v.r, v.g, v.b, v.a);
}

bool SpineSlotData::has_dark_color() {
	return slot_data->hasDarkColor();
}
void SpineSlotData::set_has_dark_color(bool v) {
	slot_data->setHasDarkColor(v);
}

String SpineSlotData::get_attachment_name() {
	return slot_data->getAttachmentName().buffer();
}
void SpineSlotData::set_attachment_name(const String &v) {
	slot_data->setAttachmentName(S_T(v));
}

SpineSlotData::BlendMode SpineSlotData::get_blend_mode() {
	auto bm = (int) slot_data->getBlendMode();
	return (BlendMode) bm;
}
void SpineSlotData::set_blend_mode(BlendMode v) {
	auto bm = (int) v;
	slot_data->setBlendMode((spine::BlendMode) bm);
}

#undef S_T