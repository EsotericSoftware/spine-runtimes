#ifndef SPINE_ATLAS_H_
#define SPINE_ATLAS_H_

#ifdef __cplusplus
namespace spine {
extern "C" {
#endif

typedef enum {
	ATLAS_ALPHA, ATLAS_INTENSITY, ATLAS_LUMINANCEALPHA, ATLAS_RGB565, ATLAS_RGBA4444, ATLAS_RGB888, ATLAS_RGBA8888
} AtlasFormat;

typedef enum {
	ATLAS_NEAREST,
	ATLAS_LINEAR,
	ATLAS_MIPMAP,
	ATLAS_MIPMAPNEARESTNEAREST,
	ATLAS_MIPMAPLINEARNEAREST,
	ATLAS_MIPMAPNEARESTLINEAR,
	ATLAS_MIPMAPLINEARLINEAR
} AtlasFilter;

typedef enum {
	ATLAS_MIRROREDREPEAT, ATLAS_CLAMPTOEDGE, ATLAS_REPEAT
} AtlasWrap;

typedef struct AtlasPage AtlasPage;
struct AtlasPage {
	const char* name;
	AtlasFormat format;
	AtlasFilter minFilter, magFilter;
	AtlasWrap uWrap, vWrap;
	AtlasPage* next;

	void (*_dispose) (AtlasPage* page);
};

AtlasPage* AtlasPage_create (const char* name);
void AtlasPage_dispose (AtlasPage* page);

/**/

typedef struct AtlasRegion AtlasRegion;
struct AtlasRegion {
	const char* name;
	int x, y, width, height;
	float offsetX, offsetY;
	int originalWidth, originalHeight;
	int index;
	int/*bool*/rotate;
	int/*bool*/flip;
	int* splits;
	int* pads;
	AtlasPage* page;
	AtlasRegion* next;
};

AtlasRegion* AtlasRegion_create ();
void AtlasRegion_dispose (AtlasRegion* region);

/**/

typedef struct {
	AtlasPage* pages;
	AtlasRegion* regions;
} Atlas;

Atlas* Atlas_readAtlas (const char* data);
Atlas* Atlas_readAtlasFile (const char* path);
void Atlas_dispose (Atlas* atlas);

AtlasRegion* Atlas_findRegion (const Atlas* atlas, const char* name);

#ifdef __cplusplus
}
}
#endif

#endif /* SPINE_ATLAS_H_ */
