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

#ifndef SPINE_SFML_H_
#define SPINE_SFML_H_

#include <spine/spine.h>
#include <SFML/Graphics/Vertex.hpp>
#include <SFML/Graphics/VertexArray.hpp>

namespace spine {

typedef struct {
	AtlasPage super;
	sf::Texture* texture;
} SfmlAtlasPage;

/**/

class SkeletonDrawable;

typedef struct {
	Skeleton super;
	SkeletonDrawable* const drawable;
} SfmlSkeleton;

class SkeletonDrawable: public sf::Drawable {
public:
	Skeleton* skeleton;
	AnimationState* state;
	float timeScale;
	sf::VertexArray* vertexArray;
	sf::Texture* texture; // All region attachments must use the same texture.

	SkeletonDrawable (SkeletonData* skeleton, AnimationStateData* stateData = 0);
	~SkeletonDrawable ();

	void update (float deltaTime);

	virtual void draw (sf::RenderTarget& target, sf::RenderStates states) const;
};

/**/

typedef struct {
	RegionAttachment super;
	sf::Vertex vertices[4];
	sf::Texture* texture;
} SfmlRegionAttachment;

} /* namespace spine */
#endif /* SPINE_SFML_H_ */
