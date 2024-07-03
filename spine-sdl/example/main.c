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

#include <spine-sdl-c.h>
#include <SDL.h>
#undef main

int main() {
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

	spAtlas *atlas = spAtlas_createFromFile("data/spineboy-pma.atlas", renderer);
	spSkeletonJson *json = spSkeletonJson_create(atlas);
	json->scale = 0.5f;
	spSkeletonData *skeletonData = spSkeletonJson_readSkeletonDataFile(json, "data/spineboy-pro.json");
	spAnimationStateData *animationStateData = spAnimationStateData_create(skeletonData);
	animationStateData->defaultMix = 0.2f;
	spSkeletonDrawable *drawable = spSkeletonDrawable_create(skeletonData, animationStateData);
	drawable->usePremultipliedAlpha = -1;
	drawable->skeleton->x = 400;
	drawable->skeleton->y = 500;
	spSkeleton_setToSetupPose(drawable->skeleton);
	spSkeletonDrawable_update(drawable, 0, SP_PHYSICS_UPDATE);
	spAnimationState_setAnimationByName(drawable->animationState, 0, "portal", 0);
	spAnimationState_addAnimationByName(drawable->animationState, 0, "run", -1, 0);

	int quit = 0;
	uint64_t lastFrameTime = SDL_GetPerformanceCounter();
	while (!quit) {
		SDL_Event event;
		while (SDL_PollEvent(&event) != 0) {
			if (event.type == SDL_QUIT) {
				quit = -1;
				break;
			}
		}

		SDL_SetRenderDrawColor(renderer, 94, 93, 96, 255);
		SDL_RenderClear(renderer);

		uint64_t now = SDL_GetPerformanceCounter();
		double deltaTime = (now - lastFrameTime) / (double) SDL_GetPerformanceFrequency();
		lastFrameTime = now;

		spSkeletonDrawable_update(drawable, deltaTime, SP_PHYSICS_UPDATE);
		spSkeletonDrawable_draw(drawable, renderer);

		SDL_RenderPresent(renderer);
	}

	SDL_DestroyWindow(window);
	SDL_Quit();
	return 0;
}
