#include <cstdio>
#include <iostream>
#include <algorithm>
#include <cctype>
#include <stdexcept>
#include <spine/AtlasData.h>

using std::string;
using std::runtime_error;

namespace spine {

static inline string& trim (string &s) {
	s.erase(s.begin(), std::find_if(s.begin(), s.end(), std::not1(std::ptr_fun<int, int>(std::isspace))));
	s.erase(std::find_if(s.rbegin(), s.rend(), std::not1(std::ptr_fun<int, int>(std::isspace))).base(), s.end());
	return s;
}

static inline void readLine (const char *&current, const char *end, string &value) {
	const char *begin = current;
	while (current != end) {
		char c = *current++;
		if (c == '\n') break;
	}
	value.clear();
	value.append(begin, current - 1);
}

static inline string& readValue (const char *&begin, const char *end, string &value) {
	readLine(begin, end, value);
	int colon = value.find(':');
	if (colon == -1) throw runtime_error("Invalid line: " + value);
	value = value.substr(colon + 1);
	return trim(value);
}

/** Returns the number of tuple values read (2 or 4). */
static inline int readTuple (const char *&begin, const char *end, string &value, string tuple[4]) {
	readLine(begin, end, value);
	int colon = value.find(':');
	if (colon == -1) throw runtime_error("Invalid line: " + value);
	int i = 0, lastMatch = colon + 1;
	for (i = 0; i < 3; i++) {
		int comma = value.find(',', lastMatch);
		if (comma == -1) {
			if (i == 0) throw runtime_error("Invalid line: " + value);
			break;
		}
		tuple[i] = value.substr(lastMatch, comma - lastMatch);
		trim(tuple[i]);
		lastMatch = comma + 1;
	}
	tuple[i] = value.substr(lastMatch);
	trim(tuple[i]);
	return i + 1;
}

static inline int indexOf (const string *array, int count, const string &value) {
	for (int i = count - 1; i >= 0; i--)
		if (array[i] == value) return i;
	throw runtime_error("Invalid value: " + value);
}

static string formatNames[] = {"Alpha", "Intensity", "LuminanceAlpha", "RGB565", "RGBA4444", "RGB888", "RGBA8888"};
static string textureFilterNames[] = {"Nearest", "Linear", "MipMap", "MipMapNearestNearest", "MipMapLinearNearest",
		"MipMapNearestLinear", "MipMapLinearLinear"};

AtlasData::AtlasData (std::istream &file) {
	string text;
	std::getline(file, text, (char)EOF);
	const char *begin = text.c_str();
	const char *end = begin + text.length();
	init(begin, end);
}

AtlasData::AtlasData (const string &text) {
	const char *begin = text.c_str();
	const char *end = begin + text.length();
	init(begin, end);
}

AtlasData::AtlasData (const char *begin, const char *end) {
	init(begin, end);
}

void AtlasData::init (const char *current, const char *end) {
	string value;
	string tuple[4];
	AtlasPage *page;
	while (current != end) {
		readLine(current, end, value);
		trim(value);
		if (value.length() == 0) {
			page = 0;
		} else if (!page) {
			page = new AtlasPage();
			pages.push_back(page);
			page->name = value;
			page->format = static_cast<Format>(indexOf(formatNames, 7, readValue(current, end, value)));

			readTuple(current, end, value, tuple);
			page->minFilter = static_cast<TextureFilter>(indexOf(textureFilterNames, 7, tuple[0]));
			page->magFilter = static_cast<TextureFilter>(indexOf(textureFilterNames, 7, tuple[1]));

			readValue(current, end, value);
			if (value == "x") {
				page->uWrap = repeat;
				page->vWrap = clampToEdge;
			} else if (value == "y") {
				page->uWrap = clampToEdge;
				page->vWrap = repeat;
			} else if (value == "xy") {
				page->uWrap = repeat;
				page->vWrap = repeat;
			}
		} else {
			AtlasRegion *region = new AtlasRegion();
			regions.push_back(region);
			region->name = value;
			region->page = page;

			region->rotate = readValue(current, end, value) == "true";

			readTuple(current, end, value, tuple);
			region->x = atoi(tuple[0].c_str());
			region->y = atoi(tuple[1].c_str());

			readTuple(current, end, value, tuple);
			region->width = atoi(tuple[0].c_str());
			region->height = atoi(tuple[1].c_str());

			if (readTuple(current, end, value, tuple) == 4) { // split is optional
				region->splits = new int[4];
				region->splits[0] = atoi(tuple[0].c_str());
				region->splits[1] = atoi(tuple[1].c_str());
				region->splits[2] = atoi(tuple[2].c_str());
				region->splits[3] = atoi(tuple[3].c_str());

				if (readTuple(current, end, value, tuple) == 4) { // pad is optional, but only present with splits
					region->pads = new int[4];
					region->pads[0] = atoi(tuple[0].c_str());
					region->pads[1] = atoi(tuple[1].c_str());
					region->pads[2] = atoi(tuple[2].c_str());
					region->pads[3] = atoi(tuple[3].c_str());

					readTuple(current, end, value, tuple);
				}
			}

			region->originalWidth = atoi(tuple[0].c_str());
			region->originalHeight = atoi(tuple[1].c_str());

			readTuple(current, end, value, tuple);
			region->offsetX = atoi(tuple[0].c_str());
			region->offsetY = atoi(tuple[1].c_str());

			region->index = atoi(readValue(current, end, value).c_str());
		}
	}
}

AtlasData::~AtlasData () {
	for (int i = 0, n = pages.size(); i < n; i++)
		delete pages[i];
	for (int i = 0, n = regions.size(); i < n; i++)
		delete regions[i];
}

} /* namespace spine */
