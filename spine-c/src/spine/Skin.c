/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ******************************************************************************/

#include <spine/Skin.h>
#include <spine/extension.h>

SkinEntry* _SkinEntry_new (int slotIndex, const char* name, Attachment* attachment) {
	SkinEntry* self = NEW(SkinEntry);
	self->slotIndex = slotIndex;
	MALLOC_STR(self->name, name);
	self->attachment = attachment;
	return self;
}

void _SkinEntry_free (SkinEntry* self) {
	Attachment_free(self->attachment);
	FREE(self->name);
	FREE(self);
}

/**/

Skin* Skin_new (const char* name) {
	Skin* self = NEW(Skin);
	MALLOC_STR(self->name, name);
	return self;
}

void Skin_free (Skin* self) {
	SkinEntry* entry = CONST_CAST(SkinEntry*, self->entries);
	while (entry) {
		SkinEntry* nextEtry = CONST_CAST(SkinEntry*, entry->next);
		_SkinEntry_free(entry);
		entry = nextEtry;
	}

	FREE(self->name);
	FREE(self);
}

void Skin_addAttachment (Skin* self, int slotIndex, const char* name, Attachment* attachment) {
	SkinEntry* newEntry = _SkinEntry_new(slotIndex, name, attachment);
	SkinEntry* entry = CONST_CAST(SkinEntry*, self->entries);
	if (!entry)
		CONST_CAST(SkinEntry*, self->entries) = newEntry;
	else {
		while (entry->next)
			entry = (SkinEntry*)entry->next;
		entry->next = newEntry;
	}
}

Attachment* Skin_getAttachment (const Skin* self, int slotIndex, const char* name) {
	const SkinEntry* entry = self->entries;
	while (entry) {
		if (entry->slotIndex == slotIndex && strcmp(entry->name, name) == 0) return entry->attachment;
		entry = entry->next;
	}
	return 0;
}
