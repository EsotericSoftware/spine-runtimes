#include <spine/Skin.h>
#include <spine/util.h>

SkinEntry* SkinEntry_create (int slotIndex, const char* name, Attachment* attachment) {
	SkinEntry* entry = calloc(1, sizeof(SkinEntry));
	entry->slotIndex = slotIndex;
	MALLOC_STR(entry->name, name)
	entry->attachment = attachment;
	return entry;
}

void SkinEntry_dispose (SkinEntry* entry) {
	if (entry->next) SkinEntry_dispose((SkinEntry*)entry->next);
	Attachment_dispose(entry->attachment);
	FREE(entry->name)
	FREE(entry)
}

/**/

Skin* Skin_create (const char* name) {
	Skin* this = calloc(1, sizeof(Skin));
	MALLOC_STR(this->name, name)
	return this;
}

void Skin_dispose (Skin* this) {
	SkinEntry_dispose((SkinEntry*)this->entries);
	FREE(this->name)
	FREE(this)
}

void Skin_addAttachment (Skin* this, int slotIndex, const char* name, Attachment* attachment) {
	SkinEntry* newEntry = SkinEntry_create(slotIndex, name, attachment);
	SkinEntry* entry = (SkinEntry*)this->entries;
	if (!entry)
		entry = newEntry;
	else {
		while (entry->next)
			entry = (SkinEntry*)entry->next;
		entry->next = newEntry;
	}
}

Attachment* Skin_getAttachment (const Skin* this, int slotIndex, const char* name) {
	const SkinEntry* entry = this->entries;
	while (entry) {
		if (entry->slotIndex == slotIndex && strcmp(entry->name, name) == 0) return entry->attachment;
		entry = entry->next;
	}
	return 0;
}
