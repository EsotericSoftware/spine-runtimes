/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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
#include <spine/ArrayUtils.h>
#include <spine/TextureLoader.h>

#include <ctype.h>

using namespace spine;

RTTI_IMPL(AtlasRegion, TextureRegion)

Atlas::Atlas(const String &path, TextureLoader *textureLoader, bool createTexture) : _textureLoader(textureLoader) {
	int dirLength;
	char *dir;
	int length;
	const char *data;

	/* Get directory from atlas path. */
	const char *lastForwardSlash = strrchr(path.buffer(), '/');
	const char *lastBackwardSlash = strrchr(path.buffer(), '\\');
	const char *lastSlash = lastForwardSlash > lastBackwardSlash ? lastForwardSlash : lastBackwardSlash;
	if (lastSlash == path) lastSlash++; /* Never drop starting slash. */
	dirLength = (int) (lastSlash ? lastSlash - path.buffer() : 0);
	dir = SpineExtension::calloc<char>(dirLength + 1, __FILE__, __LINE__);
	memcpy(dir, path.buffer(), dirLength);
	dir[dirLength] = '\0';

	data = SpineExtension::readFile(path, &length);
	if (data) {
		load(data, length, dir, createTexture);
	}

	SpineExtension::free(data, __FILE__, __LINE__);
	SpineExtension::free(dir, __FILE__, __LINE__);
}

Atlas::Atlas(const char *data, int length, const char *dir, TextureLoader *textureLoader, bool createTexture) : _textureLoader(textureLoader) {
	load(data, length, dir, createTexture);
}

Atlas::~Atlas() {
	if (_textureLoader) {
		for (size_t i = 0, n = _pages.size(); i < n; ++i) {
			_textureLoader->unload(_pages[i]->texture);
		}
	}
	ArrayUtils::deleteElements(_pages);
	ArrayUtils::deleteElements(_regions);
}

void Atlas::flipV() {
	for (size_t i = 0, n = _regions.size(); i < n; ++i) {
		AtlasRegion *regionP = _regions[i];
		AtlasRegion &region = *regionP;
		region._v = 1 - region._v;
		region._v2 = 1 - region._v2;
	}
}

AtlasRegion *Atlas::findRegion(const String &name) {
	for (size_t i = 0, n = _regions.size(); i < n; ++i)
		if (_regions[i]->_name == name) return _regions[i];
	return nullptr;
}

Array<AtlasPage *> &Atlas::getPages() {
	return _pages;
}

Array<AtlasRegion *> &Atlas::getRegions() {
	return _regions;
}

struct SimpleString {
	char *start;
	char *end;
	int length;

	SimpleString trim() {
		while (isspace((unsigned char) *start) && start < end) start++;
		if (start == end) {
			length = (int) (end - start);
			return *this;
		}
		end--;
		while (((unsigned char) *end == '\r') && end >= start) end--;
		end++;
		length = (int) (end - start);
		return *this;
	}

	int indexOf(char needle) {
		char *c = start;
		while (c < end) {
			if (*c == needle) return (int) (c - start);
			c++;
		}
		return -1;
	}

	int indexOf(char needle, int at) {
		char *c = start + at;
		while (c < end) {
			if (*c == needle) return (int) (c - start);
			c++;
		}
		return -1;
	}

	SimpleString substr(int s, int e) {
		e = s + e;
		SimpleString result;
		result.start = start + s;
		result.end = start + e;
		result.length = e - s;
		return result;
	}

	SimpleString substr(int s) {
		SimpleString result;
		result.start = start + s;
		result.end = end;
		result.length = (int) (result.end - result.start);
		return result;
	}

	bool equals(const char *str) {
		int otherLen = (int) strlen(str);
		if (length != otherLen) return false;
		for (int i = 0; i < length; i++) {
			if (start[i] != str[i]) return false;
		}
		return true;
	}

	char *copy() {
		char *string = SpineExtension::calloc<char>(length + 1, __FILE__, __LINE__);
		memcpy(string, start, length);
		string[length] = '\0';
		return string;
	}

	int toInt() {
		return (int) strtol(start, &end, 10);
	}
};

struct AtlasInput {
	const char *start;
	const char *end;
	char *index;
	int length;
	SimpleString line;

	AtlasInput(const char *data, int length) : start(data), end(data + length), index((char *) data), length(length) {
	}

	SimpleString *readLine() {
		if (index >= end) return 0;
		line.start = index;
		while (index < end && *index != '\n') index++;
		line.end = index;
		if (index != end) index++;
		line = line.trim();
		line.length = (int) (end - start);
		return &line;
	}

	static int readEntry(SimpleString entry[5], SimpleString *line) {
		if (line == nullptr) return 0;
		line->trim();
		if (line->length == 0) return 0;

		int colon = line->indexOf(':');
		if (colon == -1) return 0;
		entry[0] = line->substr(0, colon).trim();
		for (int i = 1, lastMatch = colon + 1;; i++) {
			int comma = line->indexOf(',', lastMatch);
			if (comma == -1) {
				entry[i] = line->substr(lastMatch).trim();
				return i;
			}
			entry[i] = line->substr(lastMatch, comma - lastMatch).trim();
			lastMatch = comma + 1;
			if (i == 4) return 4;
		}
	}
};

int indexOf(const char **array, int count, SimpleString *str) {
	for (int i = 0; i < count; i++)
		if (str->equals(array[i])) return i;
	return 0;
}

void Atlas::load(const char *begin, int length, const char *dir, bool createTexture) {
	static const char *formatNames[] = {"", "Alpha", "Intensity", "LuminanceAlpha", "RGB565", "RGBA4444", "RGB888", "RGBA8888"};
	static const char *textureFilterNames[] =
		{"", "Nearest", "Linear", "MipMap", "MipMapNearestNearest", "MipMapLinearNearest", "MipMapNearestLinear", "MipMapLinearLinear"};

	int dirLength = (int) strlen(dir);
	int needsSlash = dirLength > 0 && dir[dirLength - 1] != '/' && dir[dirLength - 1] != '\\';
	AtlasInput reader(begin, length);
	SimpleString entry[5];
	AtlasPage *page = nullptr;

	SimpleString *line = reader.readLine();
	while (line != nullptr && line->length == 0) line = reader.readLine();

	while (true) {
		if (line == nullptr || line->length == 0) break;
		if (reader.readEntry(entry, line) == 0) break;
		line = reader.readLine();
	}

	while (true) {
		if (line == nullptr) break;
		if (line->trim().length == 0) {
			page = nullptr;
			line = reader.readLine();
		} else if (page == nullptr) {
			char *name = line->copy();
			char *path = SpineExtension::calloc<char>(dirLength + needsSlash + strlen(name) + 1, __FILE__, __LINE__);
			memcpy(path, dir, dirLength);
			if (needsSlash) path[dirLength] = '/';
			strcpy(path + dirLength + needsSlash, name);
			page = new (__FILE__, __LINE__) AtlasPage(String(name, true));

			while (true) {
				line = reader.readLine();
				if (reader.readEntry(entry, line) == 0) break;
				if (entry[0].equals("size")) {
					page->width = entry[1].toInt();
					page->height = entry[2].toInt();
				} else if (entry[0].equals("format")) {
					page->format = (Format) indexOf(formatNames, 8, &entry[1]);
				} else if (entry[0].equals("filter")) {
#ifdef SPINE_UE4
					page->minFilter = (SpineTextureFilter) indexOf(textureFilterNames, 8, &entry[1]);
					page->magFilter = (SpineTextureFilter) indexOf(textureFilterNames, 8, &entry[2]);
#else
					page->minFilter = (TextureFilter) indexOf(textureFilterNames, 8, &entry[1]);
					page->magFilter = (TextureFilter) indexOf(textureFilterNames, 8, &entry[2]);
#endif
				} else if (entry[0].equals("repeat")) {
					page->uWrap = TextureWrap_ClampToEdge;
					page->vWrap = TextureWrap_ClampToEdge;
					if (entry[1].indexOf('x') != -1) page->uWrap = TextureWrap_Repeat;
					if (entry[1].indexOf('y') != -1) page->vWrap = TextureWrap_Repeat;
				} else if (entry[0].equals("pma")) {
					page->pma = entry[1].equals("true");
				}
			}

			page->index = (int) _pages.size();
			if (createTexture && _textureLoader) _textureLoader->load(*page, String(path));
			page->texturePath = String(path, true);
			_pages.add(page);
		} else {
			AtlasRegion *region = new (__FILE__, __LINE__) AtlasRegion();
			region->_page = page;
			region->_rendererObject = page->texture;
			region->_name = String(line->copy(), true);
			while (true) {
				line = reader.readLine();
				int count = reader.readEntry(entry, line);
				if (count == 0) break;
				if (entry[0].equals("xy")) {
					region->_x = entry[1].toInt();
					region->_y = entry[2].toInt();
				} else if (entry[0].equals("size")) {
					region->_packedWidth = entry[1].toInt();
					region->_packedHeight = entry[2].toInt();
				} else if (entry[0].equals("bounds")) {
					region->_x = entry[1].toInt();
					region->_y = entry[2].toInt();
					region->_packedWidth = entry[3].toInt();
					region->_packedHeight = entry[4].toInt();
				} else if (entry[0].equals("offset")) {
					region->_offsetX = entry[1].toInt();
					region->_offsetY = entry[2].toInt();
				} else if (entry[0].equals("orig")) {
					region->_originalWidth = entry[1].toInt();
					region->_originalHeight = entry[2].toInt();
				} else if (entry[0].equals("offsets")) {
					region->_offsetX = entry[1].toInt();
					region->_offsetY = entry[2].toInt();
					region->_originalWidth = entry[3].toInt();
					region->_originalHeight = entry[4].toInt();
				} else if (entry[0].equals("rotate")) {
					if (entry[1].equals("true")) {
						region->_degrees = 90;
					} else if (!entry[1].equals("false")) {
						region->_degrees = entry[1].toInt();
					}
					region->_rotate = region->_degrees == 90;
				} else if (entry[0].equals("index")) {
					region->_index = entry[1].toInt();
				} else {
					region->_names.add(String(entry[0].copy()));
					for (int i = 0; i < count; i++) {
						region->_values.add(entry[i + 1].toInt());
					}
				}
			}
			if (region->_originalWidth == 0 && region->_originalHeight == 0) {
				region->_originalWidth = region->_packedWidth;
				region->_originalHeight = region->_packedHeight;
			}

			region->_u = (float) region->_x / page->width;
			region->_v = (float) region->_y / page->height;
			if (region->_degrees == 90) {
				region->_u2 = (float) (region->_x + region->_packedHeight) / page->width;
				region->_v2 = (float) (region->_y + region->_packedWidth) / page->height;
			} else {
				region->_u2 = (float) (region->_x + region->_packedWidth) / page->width;
				region->_v2 = (float) (region->_y + region->_packedHeight) / page->height;
			}
			// Calculate regionWidth/Height from UV coordinates
			region->_regionWidth = abs((int) ((region->_u2 - region->_u) * page->width));
			region->_regionHeight = abs((int) ((region->_v2 - region->_v) * page->height));

			if (region->_degrees == 90) {
				int temp = region->_packedWidth;
				region->_packedWidth = region->_packedHeight;
				region->_packedHeight = temp;
			}

			_regions.add(region);
		}
	}
}
