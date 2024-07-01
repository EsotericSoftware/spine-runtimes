/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#ifndef SPINE_SDL
#define SPINE_SDL

#include <spine/spine.h>
#include <SDL.h>

namespace spine {
	class SkeletonDrawable {
	public:
		SkeletonDrawable(SkeletonData *skeletonData, AnimationStateData *animationStateData = nullptr);

		~SkeletonDrawable();

		void update(float delta, Physics physics);

		void draw(SDL_Renderer *renderer);

		Skeleton *skeleton;
		AnimationState *animationState;
		bool usePremultipliedAlpha;

	private:
		bool ownsAnimationStateData;
		Vector<SDL_Vertex> sdlVertices;
		Vector<int> sdlIndices;
	};

	class SDLTextureLoader : public spine::TextureLoader {
		SDL_Renderer *renderer;

	public:
		SDLTextureLoader(SDL_Renderer *renderer) : renderer(renderer) {
		}

		void load(AtlasPage &page, const String &path);

		void unload(void *texture);
	};
}// namespace spine

#endif
