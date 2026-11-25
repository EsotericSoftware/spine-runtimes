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

#include "SpineSlotPose.h"
#include "SpineCommon.h"
#include "SpineController.h"
#include "SpineSprite.h"

void SpineSlotPose::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_color"), &SpineSlotPose::get_color);
	ClassDB::bind_method(D_METHOD("set_color", "v"), &SpineSlotPose::set_color);
	ClassDB::bind_method(D_METHOD("get_dark_color"), &SpineSlotPose::get_dark_color);
	ClassDB::bind_method(D_METHOD("set_dark_color", "v"), &SpineSlotPose::set_dark_color);
	ClassDB::bind_method(D_METHOD("has_dark_color"), &SpineSlotPose::has_dark_color);
	ClassDB::bind_method(D_METHOD("set_has_dark_color", "v"), &SpineSlotPose::set_has_dark_color);
	ClassDB::bind_method(D_METHOD("get_attachment"), &SpineSlotPose::get_attachment);
	ClassDB::bind_method(D_METHOD("set_attachment", "v"), &SpineSlotPose::set_attachment);
	ClassDB::bind_method(D_METHOD("get_sequence_index"), &SpineSlotPose::get_sequence_index);
	ClassDB::bind_method(D_METHOD("set_sequence_index", "v"), &SpineSlotPose::set_sequence_index);
	ClassDB::bind_method(D_METHOD("get_deform"), &SpineSlotPose::get_deform);
	ClassDB::bind_method(D_METHOD("set_deform", "v"), &SpineSlotPose::set_deform);
}

Color SpineSlotPose::get_color() {
	SPINE_CHECK(get_spine_object(), Color(0, 0, 0, 0))
	auto &color = get_spine_object()->getColor();
	return Color(color.r, color.g, color.b, color.a);
}

void SpineSlotPose::set_color(Color v) {
	SPINE_CHECK(get_spine_object(), )
	auto &color = get_spine_object()->getColor();
	color.set(v.r, v.g, v.b, v.a);
}

Color SpineSlotPose::get_dark_color() {
	SPINE_CHECK(get_spine_object(), Color(0, 0, 0, 0))
	auto &color = get_spine_object()->getDarkColor();
	return Color(color.r, color.g, color.b, color.a);
}

void SpineSlotPose::set_dark_color(Color v) {
	SPINE_CHECK(get_spine_object(), )
	auto &color = get_spine_object()->getDarkColor();
	color.set(v.r, v.g, v.b, v.a);
}

bool SpineSlotPose::has_dark_color() {
	SPINE_CHECK(get_spine_object(), false)
	return get_spine_object()->hasDarkColor();
}

void SpineSlotPose::set_has_dark_color(bool v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setHasDarkColor(v);
}

Ref<SpineAttachment> SpineSlotPose::get_attachment() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto attachment = get_spine_object()->getAttachment();
	if (!attachment) return nullptr;
	Ref<SpineAttachment> attachment_ref(memnew(SpineAttachment));
	attachment_ref->set_spine_object(*get_spine_controller()->get_skeleton_data_res(), attachment);
	return attachment_ref;
}

void SpineSlotPose::set_attachment(Ref<SpineAttachment> v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setAttachment(v.is_valid() && v->get_spine_object() ? v->get_spine_object() : nullptr);
}

int SpineSlotPose::get_sequence_index() {
	SPINE_CHECK(get_spine_object(), -1)
	return get_spine_object()->getSequenceIndex();
}

void SpineSlotPose::set_sequence_index(int v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setSequenceIndex(v);
}

Array SpineSlotPose::get_deform() {
	Array result;
	SPINE_CHECK(get_spine_object(), result)
	auto &deform = get_spine_object()->getDeform();
	result.resize((int) deform.size());
	for (int i = 0; i < (int) deform.size(); ++i) {
		result[i] = deform[i];
	}
	return result;
}

void SpineSlotPose::set_deform(const Array &v) {
	SPINE_CHECK(get_spine_object(), )
	auto &deform = get_spine_object()->getDeform();
	deform.setSize(v.size(), 0);
	for (int i = 0; i < v.size(); ++i) {
		deform[i] = v[i];
	}
}