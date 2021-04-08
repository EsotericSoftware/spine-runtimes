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

#include <spine/Atlas.h>
#include <ctype.h>
#include <spine/extension.h>

spAtlasPage* spAtlasPage_create(spAtlas* atlas, const char* name) {
	spAtlasPage* self = NEW(spAtlasPage);
	CONST_CAST(spAtlas*, self->atlas) = atlas;
	MALLOC_STR(self->name, name);
	return self;
}

void spAtlasPage_dispose(spAtlasPage* self) {
	_spAtlasPage_disposeTexture(self);
	FREE(self->name);
	FREE(self);
}

/**/

spAtlasRegion* spAtlasRegion_create() {
	return NEW(spAtlasRegion);
}

void spAtlasRegion_dispose(spAtlasRegion* self) {
  int i, n;
	FREE(self->name);
	FREE(self->splits);
	FREE(self->pads);
	for (i = 0, n = self->numValues; i < n; i++) {
    FREE(self->names[i]);
  }
	FREE(self->names);
	FREE(self->values);
	FREE(self);
}

/**/

typedef struct SimpleString {
    char *start;
    char* end;
    int length;
} SimpleString;

static SimpleString *ss_trim(SimpleString *self) {
  while (isspace((unsigned char) *self->start) && self->start < self->end)
    self->start++;
  if (self->start == self->end) return self;
  self->end--;
  while (((unsigned char)*self->end == '\r') && self->end >= self->start)
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
  self->line.length = self->end - self->start;
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

static spAtlas* abortAtlas(spAtlas* self) {
	spAtlas_dispose(self);
	return 0;
}

static const char *formatNames[] = {"", "Alpha", "Intensity", "LuminanceAlpha", "RGB565", "RGBA4444", "RGB888",
                                    "RGBA8888"};
static const char *textureFilterNames[] = {"", "Nearest", "Linear", "MipMap", "MipMapNearestNearest",
                                           "MipMapLinearNearest",
                                           "MipMapNearestLinear", "MipMapLinearLinear"};

spAtlas* spAtlas_create(const char* begin, int length, const char* dir, void* rendererObject) {
	spAtlas* self;

	int count;
	const char* end = begin + length;
	int dirLength = (int)strlen(dir);
	int needsSlash = dirLength > 0 && dir[dirLength - 1] != '/' && dir[dirLength - 1] != '\\';

	spAtlasPage *page = 0;
	spAtlasPage *lastPage = 0;
	spAtlasRegion *lastRegion = 0;
	Str str;
	Str tuple[4];

	self = NEW(spAtlas);
	self->rendererObject = rendererObject;

	while (readLine(&begin, end, &str)) {
		if (str.end - str.begin == 0)
			page = 0;
		else if (!page) {
			char* name = mallocString(&str);
			char* path = MALLOC(char, dirLength + needsSlash + strlen(name) + 1);
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

			switch (readTuple(&begin, end, tuple)) {
			case 0:
				return abortAtlas(self);
			case 2: /* size is only optional for an atlas packed with an old TexturePacker. */
				page->width = toInt(tuple);
				page->height = toInt(tuple + 1);
				if (!readTuple(&begin, end, tuple)) return abortAtlas(self);
			}
			page->format = (spAtlasFormat)indexOf(formatNames, 8, tuple);

			if (!readTuple(&begin, end, tuple)) return abortAtlas(self);
			page->minFilter = (spAtlasFilter)indexOf(textureFilterNames, 8, tuple);
			page->magFilter = (spAtlasFilter)indexOf(textureFilterNames, 8, tuple + 1);

			if (!readValue(&begin, end, &str)) return abortAtlas(self);

			page->uWrap = SP_ATLAS_CLAMPTOEDGE;
			page->vWrap = SP_ATLAS_CLAMPTOEDGE;
			if (!equals(&str, "none")) {
				if (str.end - str.begin == 1) {
					if (*str.begin == 'x')
						page->uWrap = SP_ATLAS_REPEAT;
					else if (*str.begin == 'y')
						page->vWrap = SP_ATLAS_REPEAT;
				}
				else if (equals(&str, "xy")) {
					page->uWrap = SP_ATLAS_REPEAT;
					page->vWrap = SP_ATLAS_REPEAT;
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
			region->name = mallocString(&str);

			if (!readValue(&begin, end, &str)) return abortAtlas(self);
			if (equals(&str, "true"))
				region->degrees = 90;
			else if (equals(&str, "false"))
				region->degrees = 0;
			else
				region->degrees = toInt(&str);
			region->rotate = region->degrees == 90;

			if (readTuple(&begin, end, tuple) != 2) return abortAtlas(self);
			region->x = toInt(tuple);
			region->y = toInt(tuple + 1);

			if (readTuple(&begin, end, tuple) != 2) return abortAtlas(self);
			region->width = toInt(tuple);
			region->height = toInt(tuple + 1);

			region->u = region->x / (float)page->width;
			region->v = region->y / (float)page->height;
			if (region->rotate) {
				region->u2 = (region->x + region->height) / (float)page->width;
				region->v2 = (region->y + region->width) / (float)page->height;
			} else {
				region->u2 = (region->x + region->width) / (float)page->width;
				region->v2 = (region->y + region->height) / (float)page->height;
			}

			count = readTuple(&begin, end, tuple);
			if (!count) return abortAtlas(self);
			if (count == 4) { /* split is optional */
				region->splits = MALLOC(int, 4);
				region->splits[0] = toInt(tuple);
				region->splits[1] = toInt(tuple + 1);
				region->splits[2] = toInt(tuple + 2);
				region->splits[3] = toInt(tuple + 3);

				count = readTuple(&begin, end, tuple);
				if (!count) return abortAtlas(self);
				if (count == 4) { /* pad is optional, but only present with splits */
					region->pads = MALLOC(int, 4);
					region->pads[0] = toInt(tuple);
					region->pads[1] = toInt(tuple + 1);
					region->pads[2] = toInt(tuple + 2);
					region->pads[3] = toInt(tuple + 3);

					if (!readTuple(&begin, end, tuple)) return abortAtlas(self);
				}
			}

			region->originalWidth = toInt(tuple);
			region->originalHeight = toInt(tuple + 1);

			readTuple(&begin, end, tuple);
			region->offsetX = toInt(tuple);
			region->offsetY = toInt(tuple + 1);

			if (!readValue(&begin, end, &str)) return abortAtlas(self);
			region->index = toInt(&str);
		}
	}

	return self;
}

spAtlas* spAtlas_createFromFile(const char* path, void* rendererObject) {
	int dirLength;
	char *dir;
	int length;
	const char* data;

	spAtlas* atlas = 0;

	/* Get directory from atlas path. */
	const char* lastForwardSlash = strrchr(path, '/');
	const char* lastBackwardSlash = strrchr(path, '\\');
	const char* lastSlash = lastForwardSlash > lastBackwardSlash ? lastForwardSlash : lastBackwardSlash;
	if (lastSlash == path) lastSlash++; /* Never drop starting slash. */
	dirLength = (int)(lastSlash ? lastSlash - path : 0);
	dir = MALLOC(char, dirLength + 1);
	memcpy(dir, path, dirLength);
	dir[dirLength] = '\0';

	data = _spUtil_readFile(path, &length);
	if (data) atlas = spAtlas_create(data, length, dir, rendererObject);

	FREE(data);
	FREE(dir);
	return atlas;
}

void spAtlas_dispose(spAtlas* self) {
	spAtlasRegion* region, *nextRegion;
	spAtlasPage* page = self->pages;
	while (page) {
		spAtlasPage* nextPage = page->next;
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

spAtlasRegion* spAtlas_findRegion(const spAtlas* self, const char* name) {
	spAtlasRegion* region = self->regions;
	while (region) {
		if (strcmp(region->name, name) == 0) return region;
		region = region->next;
	}
	return 0;
}
