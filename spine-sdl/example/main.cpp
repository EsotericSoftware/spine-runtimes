#include <spine-sdl-cpp.h>
#include <SDL.h>

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

	spine::SDLTextureLoader textureLoader(renderer);
	spine::Atlas atlas("/Users/badlogic/workspaces/spine-runtimes/examples/spineboy/export/spineboy.atlas", &textureLoader);
	spine::AtlasAttachmentLoader attachmentLoader(&atlas);
	spine::SkeletonJson json(&attachmentLoader);
	json.setScale(0.5f);
	spine::SkeletonData *skeletonData = json.readSkeletonDataFile("/Users/badlogic/workspaces/spine-runtimes/examples/spineboy/export/spineboy-pro.json");
	spine::SkeletonDrawable drawable(skeletonData);
	drawable.skeleton->setPosition(400, 500);
	drawable.skeleton->setToSetupPose();
	drawable.update(0);
	drawable.animationState->setAnimation(0, "run", true);

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

		drawable.update(deltaTime);
		drawable.draw(renderer);

		SDL_RenderPresent(renderer);
	}

	SDL_DestroyWindow(window);
	SDL_Quit();
	return 0;
}