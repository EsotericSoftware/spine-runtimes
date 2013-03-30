#include <spine/Skin.h>
#include <spine/util.h>

SkinEntry* _SkinEntry_create (int slotIndex, const char* name, Attachment* attachment) {
	SkinEntry* self = CALLOC(SkinEntry, 1)
	self->slotIndex = slotIndex;
	MALLOC_STR(self->name, name)
	self->attachment = attachment;
	return self;
}

void _SkinEntry_dispose (SkinEntry* self) {
	if (self->next) _SkinEntry_dispose((SkinEntry*)self->next);
	Attachment_dispose(self->attachment);
	FREE(self->name)
	FREE(self)
}

/**/

Skin* Skin_create (const char* name) {
	Skin* self = CALLOC(Skin, 1)
	MALLOC_STR(self->name, name)
	return self;
}

void Skin_dispose (Skin* self) {
	_SkinEntry_dispose((SkinEntry*)self->entries);
	FREE(self->name)
	FREE(self)
}

void Skin_addAttachment (Skin* self, int slotIndex, const char* name, Attachment* attachment) {
	SkinEntry* newEntry = _SkinEntry_create(slotIndex, name, attachment);
	SkinEntry* entry = (SkinEntry*)self->entries;
	if (!entry)
		CAST(SkinEntry*, self->entries) = newEntry;
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
