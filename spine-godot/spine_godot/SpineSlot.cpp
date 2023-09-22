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

#include "SpineSlot.h"
#include "SpineBone.h"
#include "SpineCommon.h"
#include "SpineSprite.h"
#include "SpineSkeletonDataResource.h"

void SpineSlot::_bind_methods() {
	ClassDB::bind_method(D_METHOD("set_to_setup_pose"), &SpineSlot::set_to_setup_pose);
	ClassDB::bind_method(D_METHOD("get_data"), &SpineSlot::get_data);
	ClassDB::bind_method(D_METHOD("get_bone"), &SpineSlot::get_bone);
	ClassDB::bind_method(D_METHOD("get_color"), &SpineSlot::get_color);
	ClassDB::bind_method(D_METHOD("set_color"), &SpineSlot::set_color);
	ClassDB::bind_method(D_METHOD("get_dark_color"), &SpineSlot::get_dark_color);
	ClassDB::bind_method(D_METHOD("set_dark_color", "v"), &SpineSlot::set_dark_color);
	ClassDB::bind_method(D_METHOD("has_dark_color"), &SpineSlot::has_dark_color);
	ClassDB::bind_method(D_METHOD("get_attachment"), &SpineSlot::get_attachment);
	ClassDB::bind_method(D_METHOD("set_attachment", "v"), &SpineSlot::set_attachment);
	ClassDB::bind_method(D_METHOD("get_attachment_state"), &SpineSlot::get_attachment_state);
	ClassDB::bind_method(D_METHOD("set_attachment_state", "v"), &SpineSlot::set_attachment_state);
	ClassDB::bind_method(D_METHOD("get_deform"), &SpineSlot::get_deform);
	ClassDB::bind_method(D_METHOD("set_deform", "v"), &SpineSlot::set_deform);
	ClassDB::bind_method(D_METHOD("get_sequence_index"), &SpineSlot::get_sequence_index);
	ClassDB::bind_method(D_METHOD("set_sequence_index", "v"), &SpineSlot::set_sequence_index);
}

void SpineSlot::set_to_setup_pose() {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setToSetupPose();
}

Ref<SpineSlotData> SpineSlot::get_data() {
	SPINE_CHECK(get_spine_object(), nullptr)
	if (_data.is_valid()) {
		return _data;
	} else {
		auto &slot_data = get_spine_object()->getData();
		Ref<SpineSlotData> slot_data_ref(memnew(SpineSlotData));
		slot_data_ref->set_spine_object(*get_spine_owner()->get_skeleton_data_res(), &slot_data);
		_data = slot_data_ref;
		return slot_data_ref;
	}
}

Ref<SpineBone> SpineSlot::get_bone() {
	SPINE_CHECK(get_spine_object(), nullptr)
	if (_bone.is_valid()) {
		return _bone;
	} else {
		auto &bone = get_spine_object()->getBone();
		Ref<SpineBone> bone_ref(memnew(SpineBone));
		bone_ref->set_spine_object(get_spine_owner(), &bone);
		_bone = bone_ref;
		return bone_ref;
	}
}

Color SpineSlot::get_color() {
	SPINE_CHECK(get_spine_object(), Color(0, 0, 0, 0))
	auto &color = get_spine_object()->getColor();
	return Color(color.r, color.g, color.b, color.a);
}

void SpineSlot::set_color(Color v) {
	SPINE_CHECK(get_spine_object(), )
	auto &color = get_spine_object()->getColor();
	color.set(v.r, v.g, v.b, v.a);
}

Color SpineSlot::get_dark_color() {
	SPINE_CHECK(get_spine_object(), Color(0, 0, 0, 0))
	auto &color = get_spine_object()->getDarkColor();
	return Color(color.r, color.g, color.b, color.a);
}

void SpineSlot::set_dark_color(Color v) {
	SPINE_CHECK(get_spine_object(), )
	auto &color = get_spine_object()->getDarkColor();
	color.set(v.r, v.g, v.b, v.a);
}

bool SpineSlot::has_dark_color() {
	SPINE_CHECK(get_spine_object(), false)
	return get_spine_object()->hasDarkColor();
}

Ref<SpineAttachment> SpineSlot::get_attachment() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto attachment = get_spine_object()->getAttachment();
	if (!attachment) return nullptr;
	Ref<SpineAttachment> attachment_ref(memnew(SpineAttachment));
	attachment_ref->set_spine_object(*get_spine_owner()->get_skeleton_data_res(), attachment);
	return attachment_ref;
}

void SpineSlot::set_attachment(Ref<SpineAttachment> v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setAttachment(v.is_valid() && v->get_spine_object() ? v->get_spine_object() : nullptr);
}

int SpineSlot::get_attachment_state() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getAttachmentState();
}

void SpineSlot::set_attachment_state(int v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setAttachmentState(v);
}

Array SpineSlot::get_deform() {
	Array result;
	SPINE_CHECK(get_spine_object(), result)
	auto &deform = get_spine_object()->getDeform();
	result.resize((int) deform.size());
	for (int i = 0; i < (int) deform.size(); ++i) {
		result[i] = deform[i];
	}
	return result;
}

void SpineSlot::set_deform(Array v) {
	SPINE_CHECK(get_spine_object(), )
	auto &deform = get_spine_object()->getDeform();
	deform.setSize(v.size(), 0);
	for (int i = 0; i < v.size(); ++i) {
		deform[i] = v[i];
	}
}

int SpineSlot::get_sequence_index() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getAttachmentState();
}

void SpineSlot::set_sequence_index(int v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setAttachmentState(v);
}
