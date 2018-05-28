/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include <stdint.h>

#include <spine/Skin.h>
#include <spine/extension.h>

_Entry* _Entry_create (int slotIndex, const char* name, spAttachment* attachment) {
	_Entry* self = NEW(_Entry);
	self->slotIndex = slotIndex;
	MALLOC_STR(self->name, name);
	self->attachment = attachment;
	return self;
}

void _Entry_dispose (_Entry* self) {
	spAttachment_dispose(self->attachment);
	FREE(self->name);
	FREE(self);
}

/* murmur 3 sample C implementation for little-endian CPUs (copied from wikipedia) */
/* performance could be improved by updating with code optimized for different CPU architectures */
static uint32_t murmur3_32(const uint8_t* key, size_t len, uint32_t seed) {
	uint32_t h = seed;

	if (len > 3) {
		const uint32_t* key_x4 = (const uint32_t*) key;
		size_t i = len >> 2;

		do {
			uint32_t k = *key_x4++;
			k *= 0xcc9e2d51;
			k = (k << 15) | (k >> 17);
			k *= 0x1b873593;
			h ^= k;
			h = (h << 13) | (h >> 19);
			h = (h * 5) + 0xe6546b64;
		} while (--i);

		key = (const uint8_t*) key_x4;
	}

	if (len & 3) {
		size_t i = len & 3;
		uint32_t k = 0;
		key = &key[i - 1];

		do {
			k <<= 8;
			k |= *key--;
		} while (--i);

		k *= 0xcc9e2d51;
		k = (k << 15) | (k >> 17);
		k *= 0x1b873593;
		h ^= k;
	}

	h ^= len;
	h ^= h >> 16;
	h *= 0x85ebca6b;
	h ^= h >> 13;
	h *= 0xc2b2ae35;
	h ^= h >> 16;

	return h;
}

static uint32_t calculateDataHash(int slotIndex, const char* name) {
	return murmur3_32((const uint8_t*)name, strlen (name), slotIndex);
}

static uint32_t calculateEntryHash(_Entry* entry) {
	return calculateDataHash(entry->slotIndex, entry->name);
}

static _SkinHashTableEntry* _SkinHashTableEntry_create (int hashValue, _Entry* entry) {
	_SkinHashTableEntry* self = NEW(_SkinHashTableEntry);
	self->hashValue = hashValue;
	self->entry = entry;
	return self;
}

static void _SkinHashTableEntry_dispose (_SkinHashTableEntry* self) {
	FREE(self);
}

/**/

spSkin* spSkin_create (const char* name) {
	spSkin* self = SUPER(NEW(_spSkin));
	MALLOC_STR(self->name, name);
	return self;
}

void spSkin_dispose (spSkin* self) {
	_Entry* entry = SUB_CAST(_spSkin, self)->entries;

	while (entry) {
		_Entry* nextEntry = entry->next;
		_Entry_dispose(entry);
		entry = nextEntry;
	}

	{
		_SkinHashTableEntry** currentHashtableEntry = SUB_CAST(_spSkin, self)->entriesHashTable;
		int i;

		for (i = 0; i < SKIN_ENTRIES_HASH_TABLE_SIZE; ++i, ++currentHashtableEntry) {
			_SkinHashTableEntry* hashtableEntry = *currentHashtableEntry;

			while (hashtableEntry) {
				_SkinHashTableEntry* nextEntry = hashtableEntry->next;
				_SkinHashTableEntry_dispose(hashtableEntry);
				hashtableEntry = nextEntry;
			}
		}
	}

	FREE(self->name);
	FREE(self);
}

void spSkin_addAttachment (spSkin* self, int slotIndex, const char* name, spAttachment* attachment) {
	_Entry* newEntry = _Entry_create(slotIndex, name, attachment);
	newEntry->next = SUB_CAST(_spSkin, self)->entries;
	SUB_CAST(_spSkin, self)->entries = newEntry;

	{
		uint32_t hashValue = calculateEntryHash(newEntry);
		uint32_t hashTableIndex = hashValue % SKIN_ENTRIES_HASH_TABLE_SIZE;

		_SkinHashTableEntry* newHashEntry = _SkinHashTableEntry_create(hashValue, newEntry);
		newHashEntry->next = SUB_CAST(_spSkin, self)->entriesHashTable[hashTableIndex];
		SUB_CAST(_spSkin, self)->entriesHashTable[hashTableIndex] = newHashEntry;
	}
}

spAttachment* spSkin_getAttachment (const spSkin* self, int slotIndex, const char* name) {
	uint32_t hashValue = calculateDataHash(slotIndex, name);
	uint32_t hashTableIndex = hashValue % SKIN_ENTRIES_HASH_TABLE_SIZE;

	const _SkinHashTableEntry* hashEntry = SUB_CAST(_spSkin, self)->entriesHashTable[hashTableIndex];
	while (hashEntry) {
		if (hashEntry->hashValue == hashValue && hashEntry->entry->slotIndex == slotIndex && strcmp(hashEntry->entry->name, name) == 0) return hashEntry->entry->attachment;
		hashEntry = hashEntry->next;
	}
	return 0;
}

const char* spSkin_getAttachmentName (const spSkin* self, int slotIndex, int attachmentIndex) {
	const _Entry* entry = SUB_CAST(_spSkin, self)->entries;
	int i = 0;
	while (entry) {
		if (entry->slotIndex == slotIndex) {
			if (i == attachmentIndex) return entry->name;
			i++;
		}
		entry = entry->next;
	}
	return 0;
}

void spSkin_attachAll (const spSkin* self, spSkeleton* skeleton, const spSkin* oldSkin) {
	const _Entry *entry = SUB_CAST(_spSkin, oldSkin)->entries;
	while (entry) {
		spSlot *slot = skeleton->slots[entry->slotIndex];
		if (slot->attachment == entry->attachment) {
			spAttachment *attachment = spSkin_getAttachment(self, entry->slotIndex, entry->name);
			if (attachment) spSlot_setAttachment(slot, attachment);
		}
		entry = entry->next;
	}
}
