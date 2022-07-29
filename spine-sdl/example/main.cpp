#include <SDL.h>
#include <spine-sdl-cpp.h>
#include <spine/spine.h>

#define STB_IMAGE_IMPLEMENTATION

#include <stb_image.h>

namespace spine {
    struct SkeletonDrawable {
        Skeleton *skeleton;
        bool ownsAnimationStateData;
        AnimationState *animationState;

        SkeletonDrawable(SkeletonData *skeletonData, AnimationStateData *animationStateData = nullptr) {
            Bone::setYDown(true);
            skeleton = new(__FILE__, __LINE__) Skeleton(skeletonData);

            ownsAnimationStateData = animationStateData == 0;
            if (ownsAnimationStateData) animationStateData = new(__FILE__, __LINE__) AnimationStateData(skeletonData);
            animationState = new(__FILE__, __LINE__) AnimationState(animationStateData);
        }

        ~SkeletonDrawable() {
            if (ownsAnimationStateData) delete animationState->getData();
            delete animationState;
            delete skeleton;
        }

        void update(float delta) {
            animationState->update(delta);
            animationState->apply(*skeleton);
            skeleton->updateWorldTransform();
        }

        void draw() {

        }
    };

    class SDLTextureLoader : public spine::TextureLoader {
        SDL_Renderer *renderer;

        SDLTextureLoader(SDL_Renderer *renderer): renderer(renderer) {
        }

        SDL_Texture *loadTexture(SDL_Renderer *renderer, const spine::String &path) {
            int width, height, components;
            stbi_uc *imageData = stbi_load(path.buffer(), &width, &height, &components, 4);
            if (!imageData) return nullptr;
            SDL_Texture *texture = SDL_CreateTexture(renderer, SDL_PIXELFORMAT_ABGR8888, SDL_TEXTUREACCESS_STATIC, width, height);
            if (!texture) {
                stbi_image_free(imageData);
                return nullptr;
            }
            if (SDL_UpdateTexture(texture, nullptr, imageData, width * 4)) {
                stbi_image_free(imageData);
                return nullptr;
            }
            stbi_image_free(imageData);
            return texture;
        }

        void load(AtlasPage &page, const String &path) {
            SDL_Texture *texture = loadTexture(renderer, path);
            if (!texture) return;
            page.setRendererObject(texture);
            SDL_QueryTexture(texture, nullptr, nullptr, &page.width, &page.height);

            /*if (page.magFilter == TextureFilter_Linear) texture->setSmooth(true);
            if (page.uWrap == TextureWrap_Repeat && page.vWrap == TextureWrap_Repeat) texture->setRepeated(true);*/
        }

        void unload(void *texture) {
            SDL_DestroyTexture((SDL_Texture*)texture);
        }
    };

    SpineExtension *getDefaultExtension() {
        return new DefaultSpineExtension();
    }
}

spine::SkeletonDrawable *loadSkeleton(const char *json, const char *skel, const char *atlas) {
    return nullptr;
}

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
    SDL_Renderer *renderer = SDL_CreateRenderer(window, -1, SDL_RENDERER_ACCELERATED);
    if (!renderer) {
        printf("Error: %s", SDL_GetError());
        return -1;
    }
    // spine::SkeletonDrawable skeletonDrawable(nullptr, nullptr);

    bool quit = false;
    while (!quit) {
        SDL_Event event;
        while (SDL_PollEvent(&event) != 0) {
            if (event.type == SDL_QUIT) {
                quit = true;
                break;
            }
        }

        SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
        SDL_RenderClear(renderer);
        SDL_RenderPresent(renderer);
    }

    SDL_DestroyWindow(window);
    SDL_Quit();
    return 0;
}