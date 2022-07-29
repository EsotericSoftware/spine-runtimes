#include <SDL.h>
#include <spine-sdl-cpp.h>

int main() {
    SDL_Window *window = nullptr;
    SDL_Surface *surface = nullptr;

    if (SDL_Init(SDL_INIT_VIDEO)) {
        printf("Error: %s", SDL_GetError());
        return -1;
    }
    window = SDL_CreateWindow("Spine SDL", SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, 640, 480, 0);
    if (!window) {
        printf("Error: %s", SDL_GetError());
        return -1;
    }



    SDL_Quit();
    return 0;
}