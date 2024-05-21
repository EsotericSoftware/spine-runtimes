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

#include <spine-sdl-cpp.h>
#include <SDL.h>
#undef main

int main(int argc, char **argv) {
	if (SDL_Init(SDL_INIT_VIDEO)) {
		printf("Error: %s", SDL_GetError());
		return -1;
	}
	SDL_Window *window = SDL_CreateWindow("Spine SDL", SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, 800, 600, 0);
	if (!window) {
		printf("Error: %s", SDL_GetError());
		return -1;
	}
	SDL_Renderer *renderer = SDL_CreateRenderer(window, -1, SDL_RENDERER_ACCELERATED | SDL_RENDERER_PRESENTVSYNC);
	if (!renderer) {
		printf("Error: %s", SDL_GetError());
		return -1;
	}

	spine::SDLTextureLoader textureLoader(renderer);
	spine::Atlas atlas("data/spineboy-pma.atlas", &textureLoader);
	spine::AtlasAttachmentLoader attachmentLoader(&atlas);
	spine::SkeletonJson json(&attachmentLoader);
	json.setScale(0.5f);
	spine::SkeletonData *skeletonData = json.readSkeletonDataFile("data/spineboy-pro.json");
	spine::SkeletonDrawable drawable(skeletonData);
    drawable.usePremultipliedAlpha = true;
	drawable.animationState->getData()->setDefaultMix(0.2f);
	drawable.skeleton->setPosition(400, 500);
	drawable.skeleton->setToSetupPose();
	drawable.animationState->setAnimation(0, "portal", true);
	drawable.animationState->addAnimation(0, "run", true, 0);
    drawable.update(0, spine::Physics_Update);

	bool quit = false;
	uint64_t lastFrameTime = SDL_GetPerformanceCounter();
	while (!quit) {
		SDL_Event event;
		while (SDL_PollEvent(&event) != 0) {
			if (event.type == SDL_QUIT) {
				quit = true;
				break;
			}
		}

		SDL_SetRenderDrawColor(renderer, 94, 93, 96, 255);
		SDL_RenderClear(renderer);

		uint64_t now = SDL_GetPerformanceCounter();
		double deltaTime = (now - lastFrameTime) / (double) SDL_GetPerformanceFrequency();
		lastFrameTime = now;

		drawable.update(deltaTime, spine::Physics_Update);
		drawable.draw(renderer);

		SDL_RenderPresent(renderer);
	}

	SDL_DestroyWindow(window);
	SDL_Quit();
	return 0;
}
