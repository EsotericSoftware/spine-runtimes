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
        SkeletonClipping clipper;
        Vector<float> worldVertices;
        Vector<unsigned short> quadIndices;

        SkeletonDrawable(SkeletonData *skeletonData, AnimationStateData *animationStateData = nullptr) {
            Bone::setYDown(true);
            skeleton = new(__FILE__, __LINE__) Skeleton(skeletonData);

            ownsAnimationStateData = animationStateData == 0;
            if (ownsAnimationStateData) animationStateData = new(__FILE__, __LINE__) AnimationStateData(skeletonData);
            animationState = new(__FILE__, __LINE__) AnimationState(animationStateData);
            quadIndices.add(0);
            quadIndices.add(1);
            quadIndices.add(2);
            quadIndices.add(2);
            quadIndices.add(3);
            quadIndices.add(0);
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

        void draw(SDL_Renderer *renderer) {
            SDL_Texture *texture;
            spine::Vector<SDL_Vertex> sdlVertices;
            SDL_Vertex sdlVertex;
            Vector<int> sdlIndices;
            for (unsigned i = 0; i < skeleton->getSlots().size(); ++i) {
                Slot &slot = *skeleton->getDrawOrder()[i];
                Attachment *attachment = slot.getAttachment();
                if (!attachment) continue;

                // Early out if the slot color is 0 or the bone is not active
                if (slot.getColor().a == 0 || !slot.getBone().isActive()) {
                    clipper.clipEnd(slot);
                    continue;
                }

                Vector<float> *vertices = &worldVertices;
                int verticesCount = 0;
                Vector<float> *uvs = NULL;
                Vector<unsigned short> *indices;
                int indicesCount = 0;
                Color *attachmentColor;

                if (attachment->getRTTI().isExactly(RegionAttachment::rtti)) {
                    RegionAttachment *regionAttachment = (RegionAttachment *) attachment;
                    attachmentColor = &regionAttachment->getColor();

                    // Early out if the slot color is 0
                    if (attachmentColor->a == 0) {
                        clipper.clipEnd(slot);
                        continue;
                    }

                    worldVertices.setSize(8, 0);
                    regionAttachment->computeWorldVertices(slot, worldVertices, 0, 2);
                    verticesCount = 4;
                    uvs = &regionAttachment->getUVs();
                    indices = &quadIndices;
                    indicesCount = 6;
                    texture = (SDL_Texture *) ((AtlasRegion *) regionAttachment->getRendererObject())->page->getRendererObject();

                } else if (attachment->getRTTI().isExactly(MeshAttachment::rtti)) {
                    MeshAttachment *mesh = (MeshAttachment *) attachment;
                    attachmentColor = &mesh->getColor();

                    // Early out if the slot color is 0
                    if (attachmentColor->a == 0) {
                        clipper.clipEnd(slot);
                        continue;
                    }

                    worldVertices.setSize(mesh->getWorldVerticesLength(), 0);
                    texture = (SDL_Texture *) ((AtlasRegion *) mesh->getRendererObject())->page->getRendererObject();
                    mesh->computeWorldVertices(slot, 0, mesh->getWorldVerticesLength(), worldVertices.buffer(), 0, 2);
                    verticesCount = mesh->getWorldVerticesLength() >> 1;
                    uvs = &mesh->getUVs();
                    indices = &mesh->getTriangles();
                    indicesCount = indices->size();

                } else if (attachment->getRTTI().isExactly(ClippingAttachment::rtti)) {
                    ClippingAttachment *clip = (ClippingAttachment *) slot.getAttachment();
                    clipper.clipStart(slot, clip);
                    continue;
                } else
                    continue;

                Uint8 r = static_cast<Uint8>(skeleton->getColor().r * slot.getColor().r * attachmentColor->r * 255);
                Uint8 g = static_cast<Uint8>(skeleton->getColor().g * slot.getColor().g * attachmentColor->g * 255);
                Uint8 b = static_cast<Uint8>(skeleton->getColor().b * slot.getColor().b * attachmentColor->b * 255);
                Uint8 a = static_cast<Uint8>(skeleton->getColor().a * slot.getColor().a * attachmentColor->a * 255);
                sdlVertex.color.r = r;
                sdlVertex.color.g = g;
                sdlVertex.color.b = b;
                sdlVertex.color.a = a;

                Color light;
                light.r = r / 255.0f;
                light.g = g / 255.0f;
                light.b = b / 255.0f;
                light.a = a / 255.0f;

                if (clipper.isClipping()) {
                    clipper.clipTriangles(worldVertices, *indices, *uvs, 2);
                    vertices = &clipper.getClippedVertices();
                    verticesCount = clipper.getClippedVertices().size() >> 1;
                    uvs = &clipper.getClippedUVs();
                    indices = &clipper.getClippedTriangles();
                    indicesCount = clipper.getClippedTriangles().size();
                }


                sdlVertices.clear();
                for (int ii = 0; ii < verticesCount; ++ii) {
                    sdlVertex.position.x = (*vertices)[ii];
                    sdlVertex.position.y = (*vertices)[ii + 1];
                    sdlVertex.tex_coord.x = (*uvs)[ii];
                    sdlVertex.tex_coord.y = (*uvs)[ii + 1];
                    sdlVertices.add(sdlVertex);
                }
                sdlIndices.clear();
                for (int ii = 0; ii < indices->size(); ii++)
                    sdlIndices.add((*indices)[ii]);

                SDL_RenderGeometry(renderer, texture, sdlVertices.buffer(), sdlVertices.size(), sdlIndices.buffer(), indicesCount);
                clipper.clipEnd(slot);
            }
            clipper.clipEnd();
        }
    };

    class SDLTextureLoader : public spine::TextureLoader {
        SDL_Renderer *renderer;

    public:
        SDLTextureLoader(SDL_Renderer *renderer): renderer(renderer) {
        }

        SDL_Texture *loadTexture(const spine::String &path) {
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
            SDL_Texture *texture = loadTexture(path);
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

    spine::SDLTextureLoader textureLoader(renderer);
    spine::Atlas atlas("/Users/badlogic/workspaces/spine-runtimes/examples/spineboy/export/spineboy.atlas", &textureLoader);
    spine::AtlasAttachmentLoader attachmentLoader(&atlas);
    spine::SkeletonJson json(&attachmentLoader);
    spine::SkeletonData *skeletonData = json.readSkeletonDataFile("/Users/badlogic/workspaces/spine-runtimes/examples/spineboy/export/spineboy-pro.json");
    spine::SkeletonDrawable drawable(skeletonData);
    drawable.skeleton->setPosition(400, 500);
    drawable.update(0);

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
        drawable.draw(renderer);
        SDL_RenderPresent(renderer);
    }

    SDL_DestroyWindow(window);
    SDL_Quit();
    return 0;
}