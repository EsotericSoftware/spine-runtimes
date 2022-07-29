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
            skeleton = new (__FILE__, __LINE__) Skeleton(skeletonData);

            ownsAnimationStateData = animationStateData == 0;
            if (ownsAnimationStateData) animationStateData = new (__FILE__, __LINE__) AnimationStateData(skeletonData);
            animationState = new (__FILE__, __LINE__) AnimationState(animationStateData);
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

    class SDLTextureLoader: public spine::TextureLoader {
        void load(AtlasPage &page, const String &path) {
            /*Texture *texture = new Texture();
            if (!texture->loadFromFile(path.buffer())) return;

            if (page.magFilter == TextureFilter_Linear) texture->setSmooth(true);
            if (page.uWrap == TextureWrap_Repeat && page.vWrap == TextureWrap_Repeat) texture->setRepeated(true);

            page.setRendererObject(texture);
            Vector2u size = texture->getSize();
            page.width = size.x;
            page.height = size.y;*/
        }

        void unload(void *texture) {
            // delete (Texture *) texture;
        }
    };

    SpineExtension *getDefaultExtension() {
        return new DefaultSpineExtension();
    }
}

void loadSkeleton(const char *json, const char *skel, const char *atlas) {

}

int main() {
    if (SDL_Init(SDL_INIT_VIDEO)) {
        printf("Error: %s", SDL_GetError());
        return -1;
    }
    SDL_Window *window = SDL_CreateWindow("Spine SDL", SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, 640, 480,
                              SDL_WINDOW_SHOWN);
    if (!window) {
        printf("Error: %s", SDL_GetError());
        return -1;
    }
    SDL_Surface *surface = SDL_GetWindowSurface(window);

    spine::SkeletonDrawable skeletonDrawable(nullptr, nullptr);

    bool exit = false;
    do {
        SDL_Event event;
        while (SDL_PollEvent(&event) != 0) {
            if (event.type == SDL_QUIT)
                exit = true;
        }
    } while (!exit);

    SDL_DestroyWindow(window);
    SDL_Quit();
    return 0;
}