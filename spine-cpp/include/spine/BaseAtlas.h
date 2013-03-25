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

#ifndef SPINE_BASEATLAS_H_
#define SPINE_BASEATLAS_H_

#include <istream>
#include <string>
#include <vector>

namespace spine {

class BaseAtlasPage;
class BaseAtlasRegion;

class BaseAtlas {
public:
	std::vector<BaseAtlasPage*> pages;
	std::vector<BaseAtlasRegion*> regions;

	virtual BaseAtlasRegion* findRegion (const std::string &name);

protected:
	virtual ~BaseAtlas ();

	void load (std::istream &input);
	void load (const std::string &path);
	void load (const char *begin, const char *end);

private:
	virtual BaseAtlasPage* newAtlasPage (const std::string &name) = 0;
	virtual BaseAtlasRegion* newAtlasRegion (BaseAtlasPage *page) = 0;
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

	BaseAtlasRegion ();
	virtual ~BaseAtlasRegion ();
};

} /* namespace spine */
#endif /* SPINE_BASEATLAS_H_ */
