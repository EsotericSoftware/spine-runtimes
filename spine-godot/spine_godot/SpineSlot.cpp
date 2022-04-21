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

#include "SpineSlot.h"
#include "SpineBone.h"
#include "SpineSkeleton.h"
#include "SpineCommon.h"

void SpineSlot::_bind_methods() {
	ClassDB::bind_method(D_METHOD("set_to_setup_pose"), &SpineSlot::set_to_setup_pose);
	ClassDB::bind_method(D_METHOD("get_data"), &SpineSlot::get_data);
	ClassDB::bind_method(D_METHOD("get_bone"), &SpineSlot::get_bone);
	ClassDB::bind_method(D_METHOD("get_skeleton"), &SpineSlot::get_skeleton);
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

SpineSlot::SpineSlot() : slot(nullptr) {
}

void SpineSlot::set_to_setup_pose() {
	SPINE_CHECK(slot,)
	slot->setToSetupPose();
}

Ref<SpineSlotData> SpineSlot::get_data() {
	SPINE_CHECK(slot, nullptr)
	auto &slot_data = slot->getData();
	Ref<SpineSlotData> slot_data_ref(memnew(SpineSlotData));
	slot_data_ref->set_spine_object(&slot_data);
	return slot_data_ref;
}

Ref<SpineBone> SpineSlot::get_bone() {
	SPINE_CHECK(slot, nullptr)
	auto &bone = slot->getBone();
	Ref<SpineBone> bone_ref(memnew(SpineBone));
	bone_ref->set_spine_object(&bone);
	return bone_ref;
}

Ref<SpineSkeleton> SpineSlot::get_skeleton() {
	SPINE_CHECK(slot, nullptr)
	auto &skeleton = slot->getSkeleton();
	Ref<SpineSkeleton> skeleton_ref(memnew(SpineSkeleton));
	skeleton_ref->set_spine_object(&skeleton);
	return skeleton_ref;
}

Color SpineSlot::get_color() {
	SPINE_CHECK(slot, Color(0, 0, 0, 0))
	auto &color = slot->getColor();
	return Color(color.r, color.g, color.b, color.a);
}

void SpineSlot::set_color(Color v) {
	SPINE_CHECK(slot,)
	auto &color = slot->getColor();
	color.set(v.r, v.g, v.b, v.a);
}

Color SpineSlot::get_dark_color() {
	SPINE_CHECK(slot, Color(0, 0, 0, 0))
	auto &color = slot->getDarkColor();
	return Color(color.r, color.g, color.b, color.a);
}

void SpineSlot::set_dark_color(Color v) {
	SPINE_CHECK(slot,)
	auto &color = slot->getDarkColor();
	color.set(v.r, v.g, v.b, v.a);
}

bool SpineSlot::has_dark_color() {
	SPINE_CHECK(slot, false)
	return slot->hasDarkColor();
}

Ref<SpineAttachment> SpineSlot::get_attachment() {
	SPINE_CHECK(slot, nullptr)
	auto attachment = slot->getAttachment();
	if (!attachment) return nullptr;
	Ref<SpineAttachment> attachment_ref(memnew(SpineAttachment));
	attachment_ref->set_spine_object(attachment);
	return attachment_ref;
}

void SpineSlot::set_attachment(Ref<SpineAttachment> v) {
	SPINE_CHECK(slot,)
	slot->setAttachment(v.is_valid() ? v->get_spine_object() : nullptr);
}

int SpineSlot::get_attachment_state() {
	SPINE_CHECK(slot, 0)
	return slot->getAttachmentState();
}

void SpineSlot::set_attachment_state(int v) {
	SPINE_CHECK(slot,)
	slot->setAttachmentState(v);
}

Array SpineSlot::get_deform() {
	Array result;
	SPINE_CHECK(slot, result)
	auto &deform = slot->getDeform();
	result.resize((int)deform.size());
	for (int i = 0; i < deform.size(); ++i) {
		result[i] = deform[i];
	}
	return result;
}

void SpineSlot::set_deform(Array v) {
	SPINE_CHECK(slot,)
	auto &deform = slot->getDeform();
	deform.setSize(v.size(), 0);
	for (int i = 0; i < v.size(); ++i) {
		deform[i] = v[i];
	}
}

int SpineSlot::get_sequence_index() {
	SPINE_CHECK(slot, 0)
	return slot->getAttachmentState();
}

void SpineSlot::set_sequence_index(int v) {
	SPINE_CHECK(slot,)
	slot->setAttachmentState(v);
}
