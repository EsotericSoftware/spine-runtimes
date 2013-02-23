#ifndef SPINE_BASEATLASDATA_H_
#define SPINE_BASEATLASDATA_H_

#include <istream>
#include <string>
#include <vector>
#include <map>

namespace spine {

class BaseAtlasPage;
class BaseAtlasRegion;

class BaseAtlas {
public:
	std::vector<BaseAtlasPage*> pages;
	std::vector<BaseAtlasRegion*> regions;

	virtual ~BaseAtlas ();

	void load (std::ifstream &file);
	void load (std::istream &input);
	void load (const std::string &text);
	void load (const char *begin, const char *end);

	virtual BaseAtlasRegion* findRegion (const std::string &name);

private:
	virtual BaseAtlasPage* newAtlasPage (std::string name) = 0;
	virtual BaseAtlasRegion* newAtlasRegion (BaseAtlasPage*) = 0;
};

//

enum Format {
	alpha, intensity, luminanceAlpha, rgb565, rgba4444, rgb888, rgba8888
};

enum TextureFilter {
	nearest, linear, mipMap, mipMapNearestNearest, mipMapLinearNearest, mipMapNearestLinear, mipMapLinearLinear
};

enum TextureWrap {
	mirroredRepeat, clampToEdge, repeat
};

//

class BaseAtlasPage {
public:
	std::string name;
	Format format;
	TextureFilter minFilter, magFilter;
	TextureWrap uWrap, vWrap;

	virtual ~BaseAtlasPage () {
	}
};

//

class BaseAtlasRegion {
public:
	std::string name;
	int x, y, width, height;
	float offsetX, offsetY;
	int originalWidth, originalHeight;
	int index;
	bool rotate;
	bool flip;
	int *splits;
	int *pads;

  BaseAtlasRegion() : splits(0), pads(0) {}
	virtual ~BaseAtlasRegion ();
};

} /* namespace spine */
#endif /* SPINE_BASEATLASDATA_H_ */
