#include <spine/Atlas.h>
#include <ctype.h>
#include <spine/util.h>

AtlasPage* AtlasPage_create () {
	AtlasPage* this = calloc(1, sizeof(AtlasPage));
	return this;
}

void AtlasPage_dispose (AtlasPage* this) {
	if (this->next) AtlasPage_dispose(this->next);
	FREE(this->name);
	FREE(this);
}

/**/

AtlasRegion* AtlasRegion_create () {
	AtlasRegion* this = calloc(1, sizeof(AtlasRegion));
	return this;
}

void AtlasRegion_dispose (AtlasRegion* this) {
	if (this->next) AtlasRegion_dispose(this->next);
	FREE(this->name);
	FREE(this->splits);
	FREE(this->pads);
	FREE(this);
}

/**/

typedef struct {
	const char* begin;
	const char* end;
} Str;

static void trim (Str* str) {
	while (isspace(*str->begin) && str->begin < str->end)
		(str->begin)++;
	if (str->begin == str->end) return;
	str->end--;
	while (isspace(*str->end) && str->end >= str->begin)
		str->end--;
	str->end++;
}

/** Tokenize string without modification. Returns 0 on failure. */
static int readLine (const char* data, Str* str) {
	static const char* nextStart;
	if (data) {
		nextStart = data;
		return 1;
	}
	if (*nextStart == '\0') return 0;
	str->begin = nextStart;

	/* Find next delimiter. */
	do {
		nextStart++;
	} while (*nextStart != '\0' && *nextStart != '\n');

	str->end = nextStart;
	trim(str);

	if (*nextStart != '\0') nextStart++;
	return 1;
}

/** Moves str->begin past the first occurence of c. Returns 0 on failure. */
static int beginPast (Str* str, char c) {
	const char* begin = str->begin;
	while (1) {
		char lastSkippedChar = *begin;
		if (begin == str->end) return 0;
		begin++;
		if (lastSkippedChar == c) break;
	}
	str->begin = begin;
	return 1;
}

/* Returns 0 on failure. */
static int readValue (Str* str) {
	readLine(0, str);
	if (!beginPast(str, ':')) return 0;
	trim(str);
	return 1;
}

/** Returns the number of tuple values read (2, 4, or 0 for failure). */
static int readTuple (Str tuple[]) {
	Str str;
	readLine(0, &str);
	if (!beginPast(&str, ':')) return 0;
	int i = 0;
	for (i = 0; i < 3; ++i) {
		tuple[i].begin = str.begin;
		if (!beginPast(&str, ',')) {
			if (i == 0) return 0;
			break;
		}
		tuple[i].end = str.begin - 2;
		trim(&tuple[i]);
	}
	tuple[i].begin = str.begin;
	tuple[i].end = str.end;
	trim(&tuple[i]);
	return i + 1;
}

static char* mallocString (Str* str) {
	int length = str->end - str->begin;
	char* string = malloc(length + 1);
	memcpy(string, str->begin, length);
	string[length] = '\0';
	return string;
}

static int indexOf (const char** array, int count, Str* str) {
	int length = str->end - str->begin;
	int i;
	for (i = count - 1; i >= 0; i--)
		if (strncmp(array[i], str->begin, length) == 0) return i;
	return -1;
}

static int equals (Str* str, char* other) {
	return strncmp(other, str->begin, str->end - str->begin) == 0;
}

static int toInt (Str* str) {
	return strtol(str->begin, (char**)&str->end, 10);
}

static const char* formatNames[] = {"Alpha", "Intensity", "LuminanceAlpha", "RGB565", "RGBA4444", "RGB888", "RGBA8888"};
static const char* textureFilterNames[] = {"Nearest", "Linear", "MipMap", "MipMapNearestNearest", "MipMapLinearNearest",
		"MipMapNearestLinear", "MipMapLinearLinear"};

Atlas* Atlas_readAtlas (const char* data) {
	Atlas* this = calloc(1, sizeof(Atlas));

	AtlasPage *page = 0;
	AtlasPage *lastPage = 0;
	AtlasRegion *lastRegion = 0;
	Str str;
	Str tuple[4];
	readLine(data, 0);
	while (readLine(0, &str)) {
		if (str.end - str.begin == 0) {
			page = 0;
		} else if (!page) {
			page = AtlasPage_create();
			if (lastPage)
				lastPage->next = page;
			else
				this->pages = page;
			lastPage = page;

			page->name = mallocString(&str);

			if (!readValue(&str)) return 0;
			page->format = (AtlasFormat)indexOf(formatNames, 7, &str);

			if (!readTuple(tuple)) return 0;
			page->minFilter = (AtlasFilter)indexOf(textureFilterNames, 7, tuple);
			page->magFilter = (AtlasFilter)indexOf(textureFilterNames, 7, tuple + 1);

			if (!readValue(&str)) return 0;
			if (!equals(&str, "none")) {
				page->uWrap = *str.begin == 'x' ? ATLAS_REPEAT : (*str.begin == 'y' ? ATLAS_CLAMPTOEDGE : ATLAS_REPEAT);
				page->vWrap = *str.begin == 'x' ? ATLAS_CLAMPTOEDGE : (*str.begin == 'y' ? ATLAS_REPEAT : ATLAS_REPEAT);
			}
		} else {
			AtlasRegion *region = AtlasRegion_create();
			if (lastRegion)
				lastRegion->next = region;
			else
				this->regions = region;
			lastRegion = region;

			region->page = page;
			region->name = mallocString(&str);

			if (!readValue(&str)) return 0;
			region->rotate = equals(&str, "true");

			if (readTuple(tuple) != 2) return 0;
			region->x = toInt(tuple);
			region->y = toInt(tuple + 1);

			if (readTuple(tuple) != 2) return 0;
			region->width = toInt(tuple);
			region->height = toInt(tuple + 1);

			int count;
			if (!(count = readTuple(tuple))) return 0;
			if (count == 4) { /* split is optional */
				region->splits = malloc(sizeof(int) * 4);
				region->splits[0] = toInt(tuple);
				region->splits[1] = toInt(tuple + 1);
				region->splits[2] = toInt(tuple + 2);
				region->splits[3] = toInt(tuple + 3);

				if (!(count = readTuple(tuple))) return 0;
				if (count == 4) { /* pad is optional, but only present with splits */
					region->pads = malloc(sizeof(int) * 4);
					region->pads[0] = toInt(tuple);
					region->pads[1] = toInt(tuple + 1);
					region->pads[2] = toInt(tuple + 2);
					region->pads[3] = toInt(tuple + 3);

					if (!readTuple(tuple)) return 0;
				}
			}

			region->originalWidth = toInt(tuple);
			region->originalHeight = toInt(tuple + 1);

			readTuple(tuple);
			region->offsetX = (float)toInt(tuple);
			region->offsetY = (float)toInt(tuple + 1);

			if (!readValue(&str)) return 0;
			region->index = toInt(&str);
		}
	}

	return this;
}

Atlas* Atlas_readAtlasFile (const char* path) {
	const char* data = readFile(path);
	if (!data) return 0;
	Atlas* atlas = Atlas_readAtlas(data);
	FREE(data)
	return atlas;
}

void Atlas_dispose (Atlas* this) {
	if (this->pages) AtlasPage_dispose(this->pages);
	if (this->regions) AtlasRegion_dispose(this->regions);
	FREE(this)
}

AtlasRegion* Atlas_findRegion (const Atlas* this, const char* name) {
	AtlasRegion* region = this->regions;
	while (region) {
		if (strcmp(region->name, name) == 0) return region;
		region = region->next;
	}
	return 0;
}
