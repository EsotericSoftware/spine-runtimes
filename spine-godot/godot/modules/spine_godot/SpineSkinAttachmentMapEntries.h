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

#ifndef GODOT_SPINESKINATTACHMENTMAPENTRIES_H
#define GODOT_SPINESKINATTACHMENTMAPENTRIES_H

#include "core/variant_parser.h"

#include <spine/spine.h>

#include "SpineAttachment.h"

class SpineSkinAttachmentMapEntry : public Reference {
	GDCLASS(SpineSkinAttachmentMapEntry, Reference);

protected:
	static void _bind_methods();

private:
	spine::Skin::AttachmentMap::Entry *entry;

public:
	SpineSkinAttachmentMapEntry();
	~SpineSkinAttachmentMapEntry();

	inline void set_spine_object(spine::Skin::AttachmentMap::Entry *e) {
		entry = e;
	}
	inline spine::Skin::AttachmentMap::Entry *get_spine_object() {
		return entry;
	}

	uint64_t get_slot_index();
	void set_slot_index(uint64_t v);

	String get_entry_name();
	void set_entry_name(const String &v);

	Ref<SpineAttachment> get_attachment();
	void set_attachment(Ref<SpineAttachment> v);
};

class SpineSkinAttachmentMapEntries : public Reference {
	GDCLASS(SpineSkinAttachmentMapEntries, Reference);

protected:
	static void _bind_methods();

private:
	spine::Skin::AttachmentMap::Entries *entries;

public:
	SpineSkinAttachmentMapEntries();
	~SpineSkinAttachmentMapEntries();

	inline void set_spine_object(spine::Skin::AttachmentMap::Entries *e) {
		entries = e;
	}
	inline spine::Skin::AttachmentMap::Entries *get_spine_object() {
		return entries;
	}

	bool has_next();
	Ref<SpineSkinAttachmentMapEntry> next();
};

#endif//GODOT_SPINESKINATTACHMENTMAPENTRIES_H
