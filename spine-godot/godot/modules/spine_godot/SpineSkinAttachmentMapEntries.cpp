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

#include "SpineSkinAttachmentMapEntries.h"

void SpineSkinAttachmentMapEntry::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_slot_index"), &SpineSkinAttachmentMapEntry::get_slot_index);
	ClassDB::bind_method(D_METHOD("set_slot_index", "v"), &SpineSkinAttachmentMapEntry::set_slot_index);
	ClassDB::bind_method(D_METHOD("get_entry_name"), &SpineSkinAttachmentMapEntry::get_entry_name);
	ClassDB::bind_method(D_METHOD("set_entry_name", "v"), &SpineSkinAttachmentMapEntry::set_entry_name);
	ClassDB::bind_method(D_METHOD("get_attachment"), &SpineSkinAttachmentMapEntry::get_attachment);
	ClassDB::bind_method(D_METHOD("set_attachment", "v"), &SpineSkinAttachmentMapEntry::set_attachment);
}

SpineSkinAttachmentMapEntry::SpineSkinAttachmentMapEntry() : entry(NULL) {}
SpineSkinAttachmentMapEntry::~SpineSkinAttachmentMapEntry() {}

uint64_t SpineSkinAttachmentMapEntry::get_slot_index() {
	return entry->_slotIndex;
}
void SpineSkinAttachmentMapEntry::set_slot_index(uint64_t v) {
	entry->_slotIndex = v;
}

String SpineSkinAttachmentMapEntry::get_entry_name() {
	return entry->_name.buffer();
}
void SpineSkinAttachmentMapEntry::set_entry_name(const String &v) {
	entry->_name = spine::String(v.utf8());
}

Ref<SpineAttachment> SpineSkinAttachmentMapEntry::get_attachment() {
	if (entry->_attachment == NULL) return NULL;
	Ref<SpineAttachment> gd_attachment(memnew(SpineAttachment));
	gd_attachment->set_spine_object(entry->_attachment);
	return gd_attachment;
}
void SpineSkinAttachmentMapEntry::set_attachment(Ref<SpineAttachment> v) {
	if (v.is_valid()) {
		entry->_attachment = v->get_spine_object();
	} else {
		entry->_attachment = NULL;
	}
}

void SpineSkinAttachmentMapEntries::_bind_methods() {
	ClassDB::bind_method(D_METHOD("has_next"), &SpineSkinAttachmentMapEntries::has_next);
	ClassDB::bind_method(D_METHOD("next"), &SpineSkinAttachmentMapEntries::next);
}

SpineSkinAttachmentMapEntries::SpineSkinAttachmentMapEntries() : entries(NULL) {}
SpineSkinAttachmentMapEntries::~SpineSkinAttachmentMapEntries() {
	if (entries) {
		delete entries;
		return;
	}
}

bool SpineSkinAttachmentMapEntries::has_next() {
	return entries->hasNext();
}
Ref<SpineSkinAttachmentMapEntry> SpineSkinAttachmentMapEntries::next() {
	auto &e = entries->next();
	Ref<SpineSkinAttachmentMapEntry> gd_entry(memnew(SpineSkinAttachmentMapEntry));
	gd_entry->set_spine_object(&e);
	return gd_entry;
}