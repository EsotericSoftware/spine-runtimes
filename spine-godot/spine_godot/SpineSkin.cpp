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

#include "SpineSkin.h"
#include "SpineBoneData.h"
#include "SpineConstraintData.h"
#include "SpineCommon.h"
#include "SpineSprite.h"

void SpineSkin::_bind_methods() {
	ClassDB::bind_method(D_METHOD("set_attachment", "slot_index", "name", "attachment"), &SpineSkin::set_attachment);
	ClassDB::bind_method(D_METHOD("get_attachment", "slot_index", "name"), &SpineSkin::get_attachment);
	ClassDB::bind_method(D_METHOD("remove_attachment", "slot_index", "name"), &SpineSkin::remove_attachment);
	ClassDB::bind_method(D_METHOD("find_names_for_slot", "slot_index"), &SpineSkin::find_names_for_slot);
	ClassDB::bind_method(D_METHOD("find_attachments_for_slot", "slot_index"), &SpineSkin::find_attachments_for_slot);
	ClassDB::bind_method(D_METHOD("get_name"), &SpineSkin::get_name);
	ClassDB::bind_method(D_METHOD("add_skin", "other"), &SpineSkin::add_skin);
	ClassDB::bind_method(D_METHOD("copy_skin", "other"), &SpineSkin::copy_skin);
	ClassDB::bind_method(D_METHOD("get_attachments"), &SpineSkin::get_attachments);
	ClassDB::bind_method(D_METHOD("get_bones"), &SpineSkin::get_bones);
	ClassDB::bind_method(D_METHOD("get_constraints"), &SpineSkin::get_constraints);
}

SpineSkin::SpineSkin() : owns_skin(false) {
}

SpineSkin::~SpineSkin() {
	if (owns_skin) delete get_spine_object();
}

Ref<SpineSkin> SpineSkin::init(const String &name, SpineSprite *sprite) {
	if (get_spine_object()) {
		ERR_PRINT("Can not initialize an already initialized skin.");
		return this;
	}
	if (!sprite) {
		ERR_PRINT("Must provide a valid SpineSprite.");
		return this;
	}
	if (!sprite->get_skeleton_data_res().is_valid() || !sprite->get_skeleton_data_res()->is_skeleton_data_loaded()) {
		ERR_PRINT("SpineSkeletonDataResource on SpineSprite must be valid and loaded.");
		return this;
	}
	owns_skin = true;
	set_spine_object(*sprite->get_skeleton_data_res(), new spine::Skin(SPINE_STRING(name)));
	return this;
}

void SpineSkin::set_attachment(int slot_index, const String &name, Ref<SpineAttachment> attachment) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setAttachment(slot_index, SPINE_STRING(name), attachment.is_valid() && attachment->get_spine_owner() ? attachment->get_spine_object() : nullptr);
}

Ref<SpineAttachment> SpineSkin::get_attachment(int slot_index, const String &name) {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto attachment = get_spine_object()->getAttachment(slot_index, SPINE_STRING(name));
	if (attachment) return nullptr;
	Ref<SpineAttachment> attachment_ref(memnew(SpineAttachment));
	attachment_ref->set_spine_object(get_spine_owner(), attachment);
	return attachment_ref;
}

void SpineSkin::remove_attachment(int slot_index, const String &name) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->removeAttachment(slot_index, SPINE_STRING(name));
}

Array SpineSkin::find_names_for_slot(int slot_index) {
	Array result;
	SPINE_CHECK(get_spine_object(), result)
	spine::Vector<spine::String> names;
	get_spine_object()->findNamesForSlot(slot_index, names);
	result.resize((int) names.size());
	for (int i = 0; i < names.size(); ++i) {
		result[i] = names[i].buffer();
	}
	return result;
}

Array SpineSkin::find_attachments_for_slot(int slot_index) {
	Array result;
	SPINE_CHECK(get_spine_object(), result)
	spine::Vector<spine::Attachment *> attachments;
	get_spine_object()->findAttachmentsForSlot(slot_index, attachments);
	result.resize((int) attachments.size());
	for (int i = 0; i < attachments.size(); ++i) {
		if (!attachments[i]) {
			result[i] = Ref<SpineAttachment>(nullptr);
		} else {
			Ref<SpineAttachment> attachment_ref(memnew(SpineAttachment));
			attachment_ref->set_spine_object(get_spine_owner(), attachments[i]);
			result[i] = attachment_ref;
		}
	}
	return result;
}

String SpineSkin::get_name() {
	SPINE_CHECK(get_spine_object(), "")
	return get_spine_object()->getName().buffer();
}

void SpineSkin::add_skin(Ref<SpineSkin> other) {
	SPINE_CHECK(get_spine_object(), )
	if (!other.is_valid() || !other->get_spine_object()) {
		ERR_PRINT("other is not a valid SpineSkin.");
		return;
	}
	get_spine_object()->addSkin(other->get_spine_object());
}

void SpineSkin::copy_skin(Ref<SpineSkin> other) {
	SPINE_CHECK(get_spine_object(), )
	if (!other.is_valid() || !other->get_spine_object()) {
		ERR_PRINT("other is not a valid SpineSkin.");
		return;
	}
	get_spine_object()->copySkin(other->get_spine_object());
}

Array SpineSkin::get_attachments() {
	Array result;
	SPINE_CHECK(get_spine_object(), result)
	auto entries = get_spine_object()->getAttachments();
	while (entries.hasNext()) {
		spine::Skin::AttachmentMap::Entry &entry = entries.next();
		Ref<SpineSkinEntry> entry_ref = memnew(SpineSkinEntry);
		Ref<SpineAttachment> attachment_ref = nullptr;
		if (entry._attachment) {
			attachment_ref = Ref<SpineAttachment>(memnew(SpineAttachment));
			attachment_ref->set_spine_object(get_spine_owner(), entry._attachment);
		}
		entry_ref->init(entry._slotIndex, entry._name.buffer(), attachment_ref);
		result.push_back(entry_ref);
	}
	return result;
}

Array SpineSkin::get_bones() {
	Array result;
	SPINE_CHECK(get_spine_object(), result)
	auto bones = get_spine_object()->getBones();
	result.resize((int) bones.size());
	for (int i = 0; i < bones.size(); ++i) {
		Ref<SpineBoneData> bone_ref(memnew(SpineBoneData));
		bone_ref->set_spine_object(get_spine_owner(), bones[i]);
		result[i] = bone_ref;
	}
	return result;
}

Array SpineSkin::get_constraints() {
	Array result;
	SPINE_CHECK(get_spine_object(), result)
	auto constraints = get_spine_object()->getConstraints();
	result.resize((int) constraints.size());
	for (int i = 0; i < constraints.size(); ++i) {
		Ref<SpineConstraintData> constraint_ref(memnew(SpineConstraintData));
		constraint_ref->set_spine_object(get_spine_owner(), constraints[i]);
		result[i] = constraint_ref;
	}
	return result;
}

void SpineSkinEntry::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_slot_index"), &SpineSkinEntry::get_slot_index);
	ClassDB::bind_method(D_METHOD("get_name"), &SpineSkinEntry::get_name);
	ClassDB::bind_method(D_METHOD("get_attachment"), &SpineSkinEntry::get_attachment);
}

SpineSkinEntry::SpineSkinEntry() : slot_index(0) {
}

int SpineSkinEntry::get_slot_index() {
	return slot_index;
}

const String &SpineSkinEntry::get_name() {
	return name;
}

Ref<SpineAttachment> SpineSkinEntry::get_attachment() {
	return attachment;
}
