#include <spine/Skin.h>
#include <spine/util.h>

SkinEntry* _SkinEntry_create (int slotIndex, const char* name, Attachment* attachment) {
	SkinEntry* this = calloc(1, sizeof(SkinEntry));
	this->slotIndex = slotIndex;
	MALLOC_STR(this->name, name)
	this->attachment = attachment;
	return this;
}

void _SkinEntry_dispose (SkinEntry* this) {
	if (this->next) _SkinEntry_dispose((SkinEntry*)this->next);
	Attachment_dispose(this->attachment);
	FREE(this->name)
	FREE(this)
}

/**/

Skin* Skin_create (const char* name) {
	Skin* this = calloc(1, sizeof(Skin));
	MALLOC_STR(this->name, name)
	return this;
}

void Skin_dispose (Skin* this) {
	_SkinEntry_dispose((SkinEntry*)this->entries);
	FREE(this->name)
	FREE(this)
}

void Skin_addAttachment (Skin* this, int slotIndex, const char* name, Attachment* attachment) {
	SkinEntry* newEntry = _SkinEntry_create(slotIndex, name, attachment);
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
