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

#include <ctype.h>
#include <spine/Atlas.h>
#include <spine/extension.h>

spKeyValueArray *spKeyValueArray_create(int initialCapacity) {
	spKeyValueArray *array = ((spKeyValueArray *) _spCalloc(1, sizeof(spKeyValueArray), "_file_name_", 39));
	array->size = 0;
	array->capacity = initialCapacity;
	array->items = ((spKeyValue *) _spCalloc(initialCapacity, sizeof(spKeyValue), "_file_name_", 39));
	return array;
}

void spKeyValueArray_dispose(spKeyValueArray *self) {
	_spFree((void *) self->items);
	_spFree((void *) self);
}

void spKeyValueArray_clear(spKeyValueArray *self) { self->size = 0; }

spKeyValueArray *spKeyValueArray_setSize(spKeyValueArray *self, int newSize) {
	self->size = newSize;
	if (self->capacity < newSize) {
		self->capacity = ((8) > ((int) (self->size * 1.75f)) ? (8) : ((int) (self->size * 1.75f)));
		self->items = ((spKeyValue *) _spRealloc(self->items, sizeof(spKeyValue) * (self->capacity)));
	}
	return self;
}

void spKeyValueArray_ensureCapacity(spKeyValueArray *self, int newCapacity) {
	if (self->capacity >= newCapacity) return;
	self->capacity = newCapacity;
	self->items = ((spKeyValue *) _spRealloc(self->items, sizeof(spKeyValue) * (self->capacity)));
}

void spKeyValueArray_add(spKeyValueArray *self, spKeyValue value) {
	if (self->size == self->capacity) {
		self->capacity = ((8) > ((int) (self->size * 1.75f)) ? (8) : ((int) (self->size * 1.75f)));
		self->items = ((spKeyValue *) _spRealloc(self->items, sizeof(spKeyValue) * (self->capacity)));
	}
	self->items[self->size++] = value;
}

void spKeyValueArray_addAll(spKeyValueArray *self, spKeyValueArray *other) {
	int i = 0;
	for (; i < other->size; i++) { spKeyValueArray_add(self, other->items[i]); }
}

void spKeyValueArray_addAllValues(spKeyValueArray *self, spKeyValue *values, int offset, int count) {
	int i = offset, n = offset + count;
	for (; i < n; i++) { spKeyValueArray_add(self, values[i]); }
}

int spKeyValueArray_contains(spKeyValueArray *self, spKeyValue value) {
	spKeyValue *items = self->items;
	int i, n;
	for (i = 0, n = self->size; i < n; i++) {
		if (!strcmp(items[i].name, value.name)) return -1;
	}
	return 0;
}

spKeyValue spKeyValueArray_pop(spKeyValueArray *self) {
	spKeyValue item = self->items[--self->size];
	return item;
}

spKeyValue spKeyValueArray_peek(spKeyValueArray *self) { return self->items[self->size - 1]; }

spAtlasPage *spAtlasPage_create(spAtlas *atlas, const char *name) {
	spAtlasPage *self = NEW(spAtlasPage);
	self->atlas = atlas;
	MALLOC_STR(self->name, name);
	self->minFilter = SP_ATLAS_NEAREST;
	self->magFilter = SP_ATLAS_NEAREST;
	self->format = SP_ATLAS_RGBA8888;
	self->uWrap = SP_ATLAS_CLAMPTOEDGE;
	self->vWrap = SP_ATLAS_CLAMPTOEDGE;
	return self;
}

void spAtlasPage_dispose(spAtlasPage *self) {
	_spAtlasPage_disposeTexture(self);
	FREE(self->name);
	FREE(self);
}

/**/

spAtlasRegion *spAtlasRegion_create(void) {
	spAtlasRegion *region = NEW(spAtlasRegion);
	region->keyValues = spKeyValueArray_create(2);
	return region;
}

void spAtlasRegion_dispose(spAtlasRegion *self) {
	int i, n;
	FREE(self->name);
	FREE(self->splits);
	FREE(self->pads);
	for (i = 0, n = self->keyValues->size; i < n; i++) {
		FREE(self->keyValues->items[i].name);
	}
	spKeyValueArray_dispose(self->keyValues);
	FREE(self);
}

/**/

typedef struct SimpleString {
	char *start;
	char *end;
	int length;
} SimpleString;

static SimpleString *ss_trim(SimpleString *self) {
	while (isspace((unsigned char) *self->start) && self->start < self->end)
		self->start++;
	if (self->start == self->end) {
		self->length = self->end - self->start;
		return self;
	}
	self->end--;
	while (((unsigned char) *self->end == '\r') && self->end >= self->start)
		self->end--;
	self->end++;
	self->length = self->end - self->start;
	return self;
}

static int ss_indexOf(SimpleString *self, char needle) {
	char *c = self->start;
	while (c < self->end) {
		if (*c == needle) return c - self->start;
		c++;
	}
	return -1;
}

static int ss_indexOf2(SimpleString *self, char needle, int at) {
	char *c = self->start + at;
	while (c < self->end) {
		if (*c == needle) return c - self->start;
		c++;
	}
	return -1;
}

static SimpleString ss_substr(SimpleString *self, int s, int e) {
	SimpleString result;
	e = s + e;
	result.start = self->start + s;
	result.end = self->start + e;
	result.length = e - s;
	return result;
}

static SimpleString ss_substr2(SimpleString *self, int s) {
	SimpleString result;
	result.start = self->start + s;
	result.end = self->end;
	result.length = result.end - result.start;
	return result;
}

static int /*boolean*/ ss_equals(SimpleString *self, const char *str) {
	int i;
	int otherLen = strlen(str);
	if (self->length != otherLen) return 0;
	for (i = 0; i < self->length; i++) {
		if (self->start[i] != str[i]) return 0;
	}
	return -1;
}

static char *ss_copy(SimpleString *self) {
	char *string = CALLOC(char, self->length + 1);
	memcpy(string, self->start, self->length);
	string[self->length] = '\0';
	return string;
}

static int ss_toInt(SimpleString *self) {
	return (int) strtol(self->start, &self->end, 10);
}

typedef struct AtlasInput {
	const char *start;
	const char *end;
	char *index;
	int length;
	SimpleString line;
} AtlasInput;

static SimpleString *ai_readLine(AtlasInput *self) {
	if (self->index >= self->end) return 0;
	self->line.start = self->index;
	while (self->index < self->end && *self->index != '\n')
		self->index++;
	self->line.end = self->index;
	if (self->index != self->end) self->index++;
	self->line = *ss_trim(&self->line);
	self->line.length = self->line.end - self->line.start;
	return &self->line;
}

static int ai_readEntry(SimpleString entry[5], SimpleString *line) {
	int colon, i, lastMatch;
	SimpleString substr;
	if (line == NULL) return 0;
	ss_trim(line);
	if (line->length == 0) return 0;

	colon = ss_indexOf(line, ':');
	if (colon == -1) return 0;
	substr = ss_substr(line, 0, colon);
	entry[0] = *ss_trim(&substr);
	for (i = 1, lastMatch = colon + 1;; i++) {
		int comma = ss_indexOf2(line, ',', lastMatch);
		if (comma == -1) {
			substr = ss_substr2(line, lastMatch);
			entry[i] = *ss_trim(&substr);
			return i;
		}
		substr = ss_substr(line, lastMatch, comma - lastMatch);
		entry[i] = *ss_trim(&substr);
		lastMatch = comma + 1;
		if (i == 4) return 4;
	}
}

static const char *formatNames[] = {"", "Alpha", "Intensity", "LuminanceAlpha", "RGB565", "RGBA4444", "RGB888",
									"RGBA8888"};
static const char *textureFilterNames[] = {"", "Nearest", "Linear", "MipMap", "MipMapNearestNearest",
										   "MipMapLinearNearest",
										   "MipMapNearestLinear", "MipMapLinearLinear"};

int indexOf(const char **array, int count, SimpleString *str) {
	int i;
	for (i = 0; i < count; i++)
		if (ss_equals(str, array[i])) return i;
	return 0;
}

spAtlas *spAtlas_create(const char *begin, int length, const char *dir, void *rendererObject) {
	spAtlas *self;
	AtlasInput reader;
	SimpleString *line;
	SimpleString entry[5];
	spAtlasPage *page = NULL;
	spAtlasPage *lastPage = NULL;
	spAtlasRegion *lastRegion = NULL;

	int count;
	int dirLength = (int) strlen(dir);
	int needsSlash = dirLength > 0 && dir[dirLength - 1] != '/' && dir[dirLength - 1] != '\\';

	self = NEW(spAtlas);
	self->rendererObject = rendererObject;

	reader.start = begin;
	reader.end = begin + length;
	reader.index = (char *) begin;
	reader.length = length;

	line = ai_readLine(&reader);
	while (line != NULL && line->length == 0)
		line = ai_readLine(&reader);

	while (-1) {
		if (line == NULL || line->length == 0) break;
		if (ai_readEntry(entry, line) == 0) break;
		line = ai_readLine(&reader);
	}

	while (-1) {
		if (line == NULL) break;
		if (ss_trim(line)->length == 0) {
			page = NULL;
			line = ai_readLine(&reader);
		} else if (page == NULL) {
			char *name = ss_copy(line);
			char *path = CALLOC(char, dirLength + needsSlash + strlen(name) + 1);
			memcpy(path, dir, dirLength);
			if (needsSlash) path[dirLength] = '/';
			strcpy(path + dirLength + needsSlash, name);
			page = spAtlasPage_create(self, name);
			FREE(name);

			if (lastPage)
				lastPage->next = page;
			else
				self->pages = page;
			lastPage = page;

			while (-1) {
				line = ai_readLine(&reader);
				if (ai_readEntry(entry, line) == 0) break;
				if (ss_equals(&entry[0], "size")) {
					page->width = ss_toInt(&entry[1]);
					page->height = ss_toInt(&entry[2]);
				} else if (ss_equals(&entry[0], "format")) {
					page->format = (spAtlasFormat) indexOf(formatNames, 8, &entry[1]);
				} else if (ss_equals(&entry[0], "filter")) {
					page->minFilter = (spAtlasFilter) indexOf(textureFilterNames, 8, &entry[1]);
					page->magFilter = (spAtlasFilter) indexOf(textureFilterNames, 8, &entry[2]);
				} else if (ss_equals(&entry[0], "repeat")) {
					page->uWrap = SP_ATLAS_CLAMPTOEDGE;
					page->vWrap = SP_ATLAS_CLAMPTOEDGE;
					if (ss_indexOf(&entry[1], 'x') != -1) page->uWrap = SP_ATLAS_REPEAT;
					if (ss_indexOf(&entry[1], 'y') != -1) page->vWrap = SP_ATLAS_REPEAT;
				} else if (ss_equals(&entry[0], "pma")) {
					page->pma = ss_equals(&entry[1], "true");
				}
			}

			_spAtlasPage_createTexture(page, path);
			FREE(path);
		} else {
			spAtlasRegion *region = spAtlasRegion_create();
			if (lastRegion)
				lastRegion->next = region;
			else
				self->regions = region;
			lastRegion = region;
			region->page = page;
			region->name = ss_copy(line);
			while (-1) {
				line = ai_readLine(&reader);
				count = ai_readEntry(entry, line);
				if (count == 0) break;
				if (ss_equals(&entry[0], "xy")) {
					region->x = ss_toInt(&entry[1]);
					region->y = ss_toInt(&entry[2]);
				} else if (ss_equals(&entry[0], "size")) {
					region->super.width = ss_toInt(&entry[1]);
					region->super.height = ss_toInt(&entry[2]);
				} else if (ss_equals(&entry[0], "bounds")) {
					region->x = ss_toInt(&entry[1]);
					region->y = ss_toInt(&entry[2]);
					region->super.width = ss_toInt(&entry[3]);
					region->super.height = ss_toInt(&entry[4]);
				} else if (ss_equals(&entry[0], "offset")) {
					region->super.offsetX = ss_toInt(&entry[1]);
					region->super.offsetY = ss_toInt(&entry[2]);
				} else if (ss_equals(&entry[0], "orig")) {
					region->super.originalWidth = ss_toInt(&entry[1]);
					region->super.originalHeight = ss_toInt(&entry[2]);
				} else if (ss_equals(&entry[0], "offsets")) {
					region->super.offsetX = ss_toInt(&entry[1]);
					region->super.offsetY = ss_toInt(&entry[2]);
					region->super.originalWidth = ss_toInt(&entry[3]);
					region->super.originalHeight = ss_toInt(&entry[4]);
				} else if (ss_equals(&entry[0], "rotate")) {
					if (ss_equals(&entry[1], "true")) {
						region->super.degrees = 90;
					} else if (!ss_equals(&entry[1], "false")) {
						region->super.degrees = ss_toInt(&entry[1]);
					}
				} else if (ss_equals(&entry[0], "index")) {
					region->index = ss_toInt(&entry[1]);
				} else {
					int i = 0;
					spKeyValue keyValue;
					keyValue.name = ss_copy(&entry[0]);
					for (i = 0; i < count; i++) {
						keyValue.values[i] = ss_toInt(&entry[i + 1]);
					}
					spKeyValueArray_add(region->keyValues, keyValue);
				}
			}
			if (region->super.originalWidth == 0 && region->super.originalHeight == 0) {
				region->super.originalWidth = region->super.width;
				region->super.originalHeight = region->super.height;
			}

			region->super.u = (float) region->x / page->width;
			region->super.v = (float) region->y / page->height;
			if (region->super.degrees == 90) {
				region->super.u2 = (float) (region->x + region->super.height) / page->width;
				region->super.v2 = (float) (region->y + region->super.width) / page->height;
			} else {
				region->super.u2 = (float) (region->x + region->super.width) / page->width;
				region->super.v2 = (float) (region->y + region->super.height) / page->height;
			}
		}
	}

	return self;
}

spAtlas *spAtlas_createFromFile(const char *path, void *rendererObject) {
	int dirLength;
	char *dir;
	int length;
	const char *data;

	spAtlas *atlas = 0;

	/* Get directory from atlas path. */
	const char *lastForwardSlash = strrchr(path, '/');
	const char *lastBackwardSlash = strrchr(path, '\\');
	const char *lastSlash = lastForwardSlash > lastBackwardSlash ? lastForwardSlash : lastBackwardSlash;
	if (lastSlash == path) lastSlash++; /* Never drop starting slash. */
	dirLength = (int) (lastSlash ? lastSlash - path : 0);
	dir = MALLOC(char, dirLength + 1);
	memcpy(dir, path, dirLength);
	dir[dirLength] = '\0';

	data = _spUtil_readFile(path, &length);
	if (data) atlas = spAtlas_create(data, length, dir, rendererObject);

	FREE(data);
	FREE(dir);
	return atlas;
}

void spAtlas_dispose(spAtlas *self) {
	spAtlasRegion *region, *nextRegion;
	spAtlasPage *page = self->pages;
	while (page) {
		spAtlasPage *nextPage = page->next;
		spAtlasPage_dispose(page);
		page = nextPage;
	}

	region = self->regions;
	while (region) {
		nextRegion = region->next;
		spAtlasRegion_dispose(region);
		region = nextRegion;
	}

	FREE(self);
}

spAtlasRegion *spAtlas_findRegion(const spAtlas *self, const char *name) {
	spAtlasRegion *region = self->regions;
	while (region) {
		if (strcmp(region->name, name) == 0) return region;
		region = region->next;
	}
	return 0;
}
