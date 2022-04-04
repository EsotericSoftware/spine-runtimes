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

#include "SpineSkin.h"

#include "SpineBoneData.h"
#include "SpineConstraintData.h"

void SpineSkin::_bind_methods() {
	ClassDB::bind_method(D_METHOD("init", "name"), &SpineSkin::init);
	ClassDB::bind_method(D_METHOD("set_attachment", "slot_index", "name", "attachment"), &SpineSkin::set_attachment);
	ClassDB::bind_method(D_METHOD("get_attachment", "slot_index", "name"), &SpineSkin::get_attachment);
	ClassDB::bind_method(D_METHOD("remove_attachment", "slot_index", "name"), &SpineSkin::remove_attachment);
	ClassDB::bind_method(D_METHOD("find_names_for_slot", "slot_index"), &SpineSkin::find_names_for_slot);
	ClassDB::bind_method(D_METHOD("find_attachments_for_slot", "slot_index"), &SpineSkin::find_attachments_for_slot);
	ClassDB::bind_method(D_METHOD("get_skin_name"), &SpineSkin::get_skin_name);
	ClassDB::bind_method(D_METHOD("add_skin", "other"), &SpineSkin::add_skin);
	ClassDB::bind_method(D_METHOD("copy_skin", "other"), &SpineSkin::copy_skin);
	ClassDB::bind_method(D_METHOD("get_attachments"), &SpineSkin::get_attachments);
	ClassDB::bind_method(D_METHOD("get_all_bone_data"), &SpineSkin::get_bones);
	ClassDB::bind_method(D_METHOD("get_all_constraint_data"), &SpineSkin::get_constraint);
}

SpineSkin::SpineSkin() : skin(NULL) {}
SpineSkin::~SpineSkin() {}

#define S_T(x) (spine::String(x.utf8()))
Ref<SpineSkin> SpineSkin::init(const String &name) {
	skin = new spine::Skin(S_T(name));
	return this;
}

void SpineSkin::set_attachment(uint64_t slot_index, const String &name, Ref<SpineAttachment> attachment) {
	if (!attachment.is_valid()) {
		ERR_PRINT("attachment is invalid!");
		return;
	}
	skin->setAttachment(slot_index, S_T(name), attachment->get_spine_object());
}

Ref<SpineAttachment> SpineSkin::get_attachment(uint64_t slot_index, const String &name) {
	auto a = skin->getAttachment(slot_index, S_T(name));
	if (a == NULL) return NULL;
	Ref<SpineAttachment> gd_attachment(memnew(SpineAttachment));
	gd_attachment->set_spine_object(a);
	return gd_attachment;
}

void SpineSkin::remove_attachment(uint64_t slot_index, const String &name) {
	skin->removeAttachment(slot_index, S_T(name));
}

Array SpineSkin::find_names_for_slot(uint64_t slot_index) {
	spine::Vector<spine::String> names;
	skin->findNamesForSlot(slot_index, names);
	Array gd_names;
	gd_names.resize(names.size());
	for (size_t i = 0; i < names.size(); ++i) {
		gd_names[i] = names[i].buffer();
	}
	return gd_names;
}

Array SpineSkin::find_attachments_for_slot(uint64_t slot_index) {
	spine::Vector<spine::Attachment *> as;
	skin->findAttachmentsForSlot(slot_index, as);
	Array gd_as;
	gd_as.resize(as.size());
	for (size_t i = 0; i < as.size(); ++i) {
		if (as[i] == NULL) gd_as[i] = Ref<SpineAttachment>(NULL);
		else {
			Ref<SpineAttachment> gd_a(memnew(SpineAttachment));
			gd_a->set_spine_object(as[i]);
			gd_as[i] = gd_a;
		}
	}
	return gd_as;
}

String SpineSkin::get_skin_name() {
	return skin->getName().buffer();
}

void SpineSkin::add_skin(Ref<SpineSkin> other) {
	if (other.is_valid() && other->get_spine_object()) {
		skin->addSkin(other->get_spine_object());
	} else {
		ERR_PRINT("other is NULL!");
	}
}

void SpineSkin::copy_skin(Ref<SpineSkin> other) {
	if (other.is_valid() && other->get_spine_object()) {
		skin->copySkin(other->get_spine_object());
	} else {
		ERR_PRINT("other is NULL!");
	}
}

Ref<SpineSkinAttachmentMapEntries> SpineSkin::get_attachments() {
	auto *es = new spine::Skin::AttachmentMap::Entries(skin->getAttachments());
	Ref<SpineSkinAttachmentMapEntries> gd_es(memnew(SpineSkinAttachmentMapEntries));
	gd_es->set_spine_object(es);
	return gd_es;
}

Array SpineSkin::get_bones() {
	auto bs = skin->getBones();
	Array gd_bs;
	gd_bs.resize(bs.size());
	for (size_t i = 0; i < bs.size(); ++i) {
		if (bs[i] == NULL) gd_bs[i] = Ref<SpineBoneData>(NULL);
		else {
			Ref<SpineBoneData> gd_b(memnew(SpineBoneData));
			gd_b->set_spine_object(bs[i]);
			gd_bs[i] = gd_b;
		}
	}
	return gd_bs;
}

Array SpineSkin::get_constraint() {
	auto cs = skin->getConstraints();
	Array gd_cs;
	gd_cs.resize(cs.size());
	for (size_t i = 0; i < cs.size(); ++i) {
		if (cs[i] == NULL) gd_cs[i] = Ref<SpineConstraintData>(NULL);
		else {
			Ref<SpineConstraintData> gd_c(memnew(SpineConstraintData));
			gd_c->set_spine_object(cs[i]);
			gd_cs[i] = gd_c;
		}
	}
	return gd_cs;
}