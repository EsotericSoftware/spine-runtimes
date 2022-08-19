#include "spine_flutter.h"
#include <spine/spine.h>
#include <spine/Version.h>

using namespace spine;

FFI_PLUGIN_EXPORT int32_t spine_major_version() {
    return SPINE_MAJOR_VERSION;
}

FFI_PLUGIN_EXPORT int32_t spine_minor_version() {
    return SPINE_MINOR_VERSION;
}

FFI_PLUGIN_EXPORT spine_atlas* spine_atlas_load(const char *atlasData) {
    if (!atlasData) return nullptr;
    int length = strlen(atlasData);
    auto atlas = new Atlas(atlasData, length, "", (TextureLoader*)nullptr, false);
    spine_atlas *result = SpineExtension::calloc<spine_atlas>(1, __FILE__, __LINE__);
    result->atlas = atlas;
    result->numImagePaths = atlas->getPages().size();
    result->imagePaths = SpineExtension::calloc<char *>(result->numImagePaths, __FILE__, __LINE__);
    for (int i = 0; i < result->numImagePaths; i++) {
        result->imagePaths[i] = strdup(atlas->getPages()[i]->texturePath.buffer());
    }
    return result;
}

FFI_PLUGIN_EXPORT void spine_atlas_dispose(spine_atlas *atlas) {
    if (!atlas) return;
    if (atlas->atlas) delete (Atlas*)atlas->atlas;
    if (atlas->error) free(atlas->error);
    for (int i = 0; i < atlas->numImagePaths; i++) {
        free(atlas->imagePaths[i]);
    }
    SpineExtension::free(atlas, __FILE__, __LINE__);
}

FFI_PLUGIN_EXPORT spine_skeleton_data *spine_skeleton_data_load_json(spine_atlas *atlas, const char *skeletonData) {
    if (!atlas) return nullptr;
    if (!atlas->atlas) return nullptr;
    if (!skeletonData) return nullptr;
    SkeletonJson json((Atlas*)atlas->atlas);
    SkeletonData *data = json.readSkeletonData(skeletonData);
    spine_skeleton_data *result = SpineExtension::calloc<spine_skeleton_data>(1, __FILE__, __LINE__);
    result->skeletonData = data;
    if (!json.getError().isEmpty()) {
        result->error = strdup(json.getError().buffer());
    }
    return result;
}

FFI_PLUGIN_EXPORT spine_skeleton_data* spine_skeleton_data_load_binary(spine_atlas *atlas, const unsigned char *skeletonData, int32_t length) {
    if (!atlas) return nullptr;
    if (!atlas->atlas) return nullptr;
    if (!skeletonData) return nullptr;
    if (length <= 0) return nullptr;
    SkeletonBinary binary((Atlas*)atlas->atlas);
    SkeletonData *data = binary.readSkeletonData(skeletonData, length);
    spine_skeleton_data *result = SpineExtension::calloc<spine_skeleton_data>(1, __FILE__, __LINE__);
    result->skeletonData = data;
    if (!binary.getError().isEmpty()) {
        result->error = strdup(binary.getError().buffer());
    }
    return result;
}

FFI_PLUGIN_EXPORT void spine_skeleton_data_dispose(spine_skeleton_data *skeletonData) {
    if (!skeletonData) return;
    if (skeletonData->skeletonData) delete (SkeletonData*)skeletonData->skeletonData;
    if (skeletonData->error) free(skeletonData->error);
    SpineExtension::free(skeletonData, __FILE__, __LINE__);
}

FFI_PLUGIN_EXPORT spine_skeleton_drawable *spine_skeleton_drawable_create(spine_skeleton_data *skeletonData) {
    spine_skeleton_drawable *drawable = SpineExtension::calloc<spine_skeleton_drawable>(1, __FILE__, __LINE__);
    drawable->skeleton = new Skeleton((SkeletonData*)skeletonData->skeletonData);
    drawable->animationState = new AnimationState(new AnimationStateData((SkeletonData*)skeletonData->skeletonData));
    return drawable;
}

FFI_PLUGIN_EXPORT void spine_skeleton_drawable_update(spine_skeleton_drawable *drawable, float deltaTime) {
    if (!drawable) return;
    if (!drawable->skeleton) return;
    if (!drawable->animationState) return;

    Skeleton *skeleton = (Skeleton*)drawable->skeleton;
    AnimationState *animationState = (AnimationState*)drawable->animationState;
    animationState->update(deltaTime);
    animationState->apply(*skeleton);
    skeleton->updateWorldTransform();
}

spine_render_command *spine_render_command_create(int32_t numVertices, int32_t numIndices, spine_blend_mode blendMode, int pageIndex) {
    spine_render_command *cmd = SpineExtension::alloc<spine_render_command>(1, __FILE__, __LINE__);
    cmd->positions = SpineExtension::alloc<float>(numVertices * 2, __FILE__, __LINE__);
    cmd->uvs = SpineExtension::alloc<float>(numVertices * 2, __FILE__, __LINE__);
    cmd->colors = SpineExtension::alloc<int32_t>(numVertices, __FILE__, __LINE__);
    cmd->numVertices = numVertices;
    cmd->indices = SpineExtension::alloc<uint16_t>(numIndices, __FILE__, __LINE__);
    cmd->numIndices = numIndices;
    cmd->blendMode = blendMode;
    cmd->atlasPage = pageIndex;
    cmd->next = nullptr;
    return cmd;
}

void spine_render_command_dispose(spine_render_command *cmd) {
    if (!cmd) return;
    if (cmd->positions) SpineExtension::free(cmd->positions, __FILE__, __LINE__);
    if (cmd->uvs) SpineExtension::free(cmd->uvs, __FILE__, __LINE__);
    if (cmd->colors) SpineExtension::free(cmd->colors, __FILE__, __LINE__);
    if (cmd->indices) SpineExtension::free(cmd->indices, __FILE__, __LINE__);
    SpineExtension::free(cmd, __FILE__, __LINE__);
}

FFI_PLUGIN_EXPORT spine_render_command *spine_skeleton_drawable_render(spine_skeleton_drawable *drawable) {
    if (!drawable) return nullptr;
    if (!drawable->skeleton) return nullptr;

    while (drawable->renderCommand) {
        spine_render_command *cmd = drawable->renderCommand;
        drawable->renderCommand = cmd->next;
        spine_render_command_dispose(cmd);
    }

    Vector<unsigned short> quadIndices;
    quadIndices.add(0);
    quadIndices.add(1);
    quadIndices.add(2);
    quadIndices.add(2);
    quadIndices.add(3);
    quadIndices.add(0);
    Vector<float> worldVertices;
    SkeletonClipping clipper;
    Skeleton *skeleton = (Skeleton*)drawable->skeleton;
    spine_render_command *lastCommand = nullptr;

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
        int pageIndex = -1;

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
            pageIndex = ((AtlasRegion *) regionAttachment->getRendererObject())->page->index;

        } else if (attachment->getRTTI().isExactly(MeshAttachment::rtti)) {
            MeshAttachment *mesh = (MeshAttachment *) attachment;
            attachmentColor = &mesh->getColor();

            // Early out if the slot color is 0
            if (attachmentColor->a == 0) {
                clipper.clipEnd(slot);
                continue;
            }

            worldVertices.setSize(mesh->getWorldVerticesLength(), 0);
            pageIndex = ((AtlasRegion *) mesh->getRendererObject())->page->index;
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

        uint8_t r = static_cast<uint8_t>(skeleton->getColor().r * slot.getColor().r * attachmentColor->r * 255);
        uint8_t g = static_cast<uint8_t>(skeleton->getColor().g * slot.getColor().g * attachmentColor->g * 255);
        uint8_t b = static_cast<uint8_t>(skeleton->getColor().b * slot.getColor().b * attachmentColor->b * 255);
        uint8_t a = static_cast<uint8_t>(skeleton->getColor().a * slot.getColor().a * attachmentColor->a * 255);
        uint32_t color = (a << 24) | (r << 16) | (g << 8) | b;

        if (clipper.isClipping()) {
            clipper.clipTriangles(worldVertices, *indices, *uvs, 2);
            vertices = &clipper.getClippedVertices();
            verticesCount = clipper.getClippedVertices().size() >> 1;
            uvs = &clipper.getClippedUVs();
            indices = &clipper.getClippedTriangles();
            indicesCount = clipper.getClippedTriangles().size();
        }

        spine_render_command *cmd = spine_render_command_create(verticesCount, indicesCount, (spine_blend_mode)slot.getData().getBlendMode(), pageIndex);

        memcpy(cmd->positions, vertices->buffer(), (verticesCount << 2) * sizeof(float));
        memcpy(cmd->uvs, uvs->buffer(), (verticesCount << 2) * sizeof(float));
        for (int ii = 0; ii < verticesCount; ii++) cmd->colors[ii] = color;
        memcpy(cmd->indices, indices->buffer(), indices->size() * sizeof(uint16_t));

        if (!lastCommand) {
            drawable->renderCommand = lastCommand = cmd;
        } else {
            lastCommand->next = cmd;
            lastCommand = cmd;
        }

        clipper.clipEnd(slot);
    }
    clipper.clipEnd();

    return drawable->renderCommand;
}

FFI_PLUGIN_EXPORT void spine_skeleton_drawable_dispose(spine_skeleton_drawable *drawable) {
    if (!drawable) return;
    if (drawable->skeleton) delete (Skeleton*)drawable->skeleton;
    if (drawable->animationState) delete (AnimationState*)drawable->animationState;
    while (drawable->renderCommand) {
        spine_render_command *cmd = drawable->renderCommand;
        drawable->renderCommand = cmd->next;
        spine_render_command_dispose(cmd);
    }
    SpineExtension::free(drawable, __FILE__, __LINE__);
}

spine::SpineExtension *spine::getDefaultExtension() {
   return new spine::DefaultSpineExtension();
}
