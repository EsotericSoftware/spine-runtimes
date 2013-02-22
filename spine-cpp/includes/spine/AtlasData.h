#ifndef SPINE_ATLASDATA_H_
#define SPINE_ATLASDATA_H_

#include <istream>
#include <string>
#include <vector>

namespace spine {

class AtlasPage;
class AtlasRegion;

class AtlasData {
public:
	std::vector<AtlasPage*> pages;
	std::vector<AtlasRegion*> regions;

	AtlasData (const char *begin, const char *end);
	AtlasData (const std::string &json);
	AtlasData (std::istream &file);
	~AtlasData ();

private:
	void init (const char *begin, const char *end);
};

enum Format {
	alpha, intensity, luminanceAlpha, rgb565, rgba4444, rgb888, rgba8888
};

enum TextureFilter {
	nearest, linear, mipMap, mipMapNearestNearest, mipMapLinearNearest, mipMapNearestLinear, mipMapLinearLinear
};

enum TextureWrap {
	mirroredRepeat, clampToEdge, repeat
};

class AtlasPage {
public:
	std::string name;
	Format format;
	TextureFilter minFilter, magFilter;
	TextureWrap uWrap, vWrap;
};

class AtlasRegion {
public:
	std::string name;
	AtlasPage *page;
	int x, y, width, height;
	float offsetX, offsetY;
	int originalWidth, originalHeight;
	int index;
	bool rotate;
	bool flip;
	int *splits;
	int *pads;
};

} /* namespace spine */
#endif /* SPINE_ATLASDATA_H_ */
