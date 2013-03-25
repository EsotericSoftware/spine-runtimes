/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ******************************************************************************/

#include <spine-cocos2dx/Atlas.h>
#include <stdexcept>

USING_NS_CC;

namespace spine {

AtlasPage::~AtlasPage () {
	CC_SAFE_RELEASE_NULL(texture);
	CC_SAFE_RELEASE_NULL(atlas);
}

//

Atlas::Atlas (const std::string &path) {
	unsigned long size;
	char *data = reinterpret_cast<char*>(CCFileUtils::sharedFileUtils()->getFileData(
			CCFileUtils::sharedFileUtils()->fullPathForFilename(path.c_str()).c_str(), "r", &size));
	if (!data) throw std::runtime_error("Error reading atlas file: " + path);
	load(data, data + size);
}

Atlas::Atlas (std::istream &input) {
	load(input);
}

Atlas::Atlas (const char *begin, const char *end) {
	load(begin, end);
}

BaseAtlasPage* Atlas::newAtlasPage (const std::string &name) {
	AtlasPage *page = new AtlasPage();
	page->texture = CCTextureCache::sharedTextureCache()->addImage(name.c_str());
	page->atlas = CCTextureAtlas::createWithTexture(page->texture, 4);
	page->atlas->retain();
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
