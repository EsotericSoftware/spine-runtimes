#include <spine-cc/Atlas.h>

namespace spine {

AtlasPage::~AtlasPage () {
	CC_SAFE_RELEASE(texture);
}

//

Atlas::Atlas (std::ifstream &file) {
	load(file);
}

Atlas::Atlas (std::istream &input) {
	load(input);
}

Atlas::Atlas (const std::string &text) {
	load(text);
}

Atlas::Atlas (const char *begin, const char *end) {
	load(begin, end);
}

BaseAtlasPage* Atlas::newAtlasPage (const std::string &name) {
	AtlasPage *page = new AtlasPage();
    page->texture = CCTextureCache::sharedTextureCache()->addImage(name.c_str());
	return page;
}

BaseAtlasRegion* Atlas::newAtlasRegion (BaseAtlasPage* page) {
	AtlasRegion *region = new AtlasRegion();
	region->page = reinterpret_cast<AtlasPage*>(page);
	return region;
}

AtlasRegion* Atlas::findRegion (const std::string &name) {
	return reinterpret_cast<AtlasRegion*>(BaseAtlas::findRegion(name));
}

} /* namespace spine */
