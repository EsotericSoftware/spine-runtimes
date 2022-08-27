#include "spine_flutter.h"
#include <spine/spine.h>
#include <spine/Version.h>
#include <spine/Debug.h>

using namespace spine;

spine::SpineExtension *spine::getDefaultExtension() {
   return new spine::DebugExtension(new spine::DefaultSpineExtension());
}

FFI_PLUGIN_EXPORT int32_t spine_major_version() {
    return SPINE_MAJOR_VERSION;
}

FFI_PLUGIN_EXPORT int32_t spine_minor_version() {
    return SPINE_MINOR_VERSION;
}

FFI_PLUGIN_EXPORT spine_atlas* spine_atlas_load(const char *atlasData) {
    if (!atlasData) return nullptr;
    int length = (int)strlen(atlasData);
    auto atlas = new Atlas(atlasData, length, "", (TextureLoader*)nullptr, false);
    spine_atlas *result = SpineExtension::calloc<spine_atlas>(1, __FILE__, __LINE__);
    result->atlas = atlas;
    result->numImagePaths = (int32_t)atlas->getPages().size();
    result->imagePaths = SpineExtension::calloc<char *>(result->numImagePaths, __FILE__, __LINE__);
    for (int i = 0; i < result->numImagePaths; i++) {
        result->imagePaths[i] = strdup(atlas->getPages()[i]->texturePath.buffer());
    }
    return result;
}

void spine_report_leaks() {
    ((DebugExtension*)spine::SpineExtension::getInstance())->reportLeaks();
}

FFI_PLUGIN_EXPORT void spine_atlas_dispose(spine_atlas *atlas) {
    if (!atlas) return;
    if (atlas->atlas) delete (Atlas*)atlas->atlas;
    if (atlas->error) free(atlas->error);
    for (int i = 0; i < atlas->numImagePaths; i++) {
        free(atlas->imagePaths[i]);
    }
    SpineExtension::free(atlas->imagePaths, __FILE__, __LINE__);
    SpineExtension::free(atlas, __FILE__, __LINE__);
}

FFI_PLUGIN_EXPORT spine_skeleton_data *spine_skeleton_data_load_json(spine_atlas *atlas, const char *skeletonData) {
    Bone::setYDown(true);
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
    Bone::setYDown(true);
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

spine_render_command *spine_render_command_create(int32_t numVertices, int32_t numIndices, spine_blend_mode blendMode, int pageIndex) {
    spine_render_command *cmd = SpineExtension::alloc<spine_render_command>(1, __FILE__, __LINE__);
    cmd->positions = SpineExtension::alloc<float>(numVertices << 1, __FILE__, __LINE__);
    cmd->uvs = SpineExtension::alloc<float>(numVertices << 1, __FILE__, __LINE__);
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

FFI_PLUGIN_EXPORT spine_skeleton_drawable *spine_skeleton_drawable_create(spine_skeleton_data *skeletonData) {
    spine_skeleton_drawable *drawable = SpineExtension::calloc<spine_skeleton_drawable>(1, __FILE__, __LINE__);
    drawable->skeleton = new Skeleton((SkeletonData*)skeletonData->skeletonData);
    drawable->animationState = new AnimationState(new AnimationStateData((SkeletonData*)skeletonData->skeletonData));
    drawable->clipping = new SkeletonClipping();
    return drawable;
}

FFI_PLUGIN_EXPORT void spine_skeleton_drawable_dispose(spine_skeleton_drawable *drawable) {
    if (!drawable) return;
    if (drawable->skeleton) delete (Skeleton*)drawable->skeleton;
    if (drawable->animationState) {
        AnimationState *state = (AnimationState*)drawable->animationState;
        delete state->getData();
        delete (AnimationState*)state;
    }
    if (drawable->clipping) delete (SkeletonClipping*)drawable->clipping;
    while (drawable->renderCommand) {
        spine_render_command *cmd = drawable->renderCommand;
        drawable->renderCommand = cmd->next;
        spine_render_command_dispose(cmd);
    }
    SpineExtension::free(drawable, __FILE__, __LINE__);
}

FFI_PLUGIN_EXPORT void spine_skeleton_drawable_update(spine_skeleton_drawable *drawable, float deltaTime) {
    if (!drawable) return;
    if (!drawable->skeleton) return;
    if (!drawable->animationState) return;
    if (!drawable->clipping) return;

    Skeleton *skeleton = (Skeleton*)drawable->skeleton;
    AnimationState *animationState = (AnimationState*)drawable->animationState;
    animationState->update(deltaTime);
    animationState->apply(*skeleton);
    skeleton->updateWorldTransform();
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
    SkeletonClipping &clipper = *(SkeletonClipping*)drawable->clipping;
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
            verticesCount = (int)(mesh->getWorldVerticesLength() >> 1);
            uvs = &mesh->getUVs();
            indices = &mesh->getTriangles();
            indicesCount = (int)indices->size();

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
            verticesCount = (int)(clipper.getClippedVertices().size() >> 1);
            uvs = &clipper.getClippedUVs();
            indices = &clipper.getClippedTriangles();
            indicesCount = (int)(clipper.getClippedTriangles().size());
        }

        spine_render_command *cmd = spine_render_command_create(verticesCount, indicesCount, (spine_blend_mode)slot.getData().getBlendMode(), pageIndex);

        memcpy(cmd->positions, vertices->buffer(), (verticesCount << 1) * sizeof(float));
        memcpy(cmd->uvs, uvs->buffer(), (verticesCount << 1) * sizeof(float));
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

FFI_PLUGIN_EXPORT void spine_animation_state_update(spine_animation_state state, float delta) {
    if (state == nullptr) return;
    AnimationState *_state = (AnimationState*)state;
    _state->update(delta);
}

FFI_PLUGIN_EXPORT void spine_animation_state_apply(spine_animation_state state, spine_skeleton skeleton) {
    if (state == nullptr) return;
    AnimationState *_state = (AnimationState*)state;
    _state->apply(*(Skeleton*)skeleton);
}

FFI_PLUGIN_EXPORT void spine_animation_state_clear_tracks(spine_animation_state state) {
    if (state == nullptr) return;
    AnimationState *_state = (AnimationState*)state;
    _state->clearTracks();
}

FFI_PLUGIN_EXPORT void spine_animation_state_clear_track(spine_animation_state state, int32_t trackIndex) {
    if (state == nullptr) return;
    AnimationState *_state = (AnimationState*)state;
    _state->clearTrack(trackIndex);
}

FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_set_animation(spine_animation_state state, int32_t trackIndex, const char* animationName, int32_t loop) {
    if (state == nullptr) return nullptr;
    AnimationState *_state = (AnimationState*)state;
    return _state->setAnimation(trackIndex, animationName, loop);
}

FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_add_animation(spine_animation_state state, int32_t trackIndex, const char* animationName, int32_t loop, float delay) {
    if (state == nullptr) return nullptr;
    AnimationState *_state = (AnimationState*)state;
    return _state->addAnimation(trackIndex, animationName, loop, delay);
}

FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_set_empty_animation(spine_animation_state state, int32_t trackIndex, float mixDuration) {
    if (state == nullptr) return nullptr;
    AnimationState *_state = (AnimationState*)state;
    return _state->setEmptyAnimation(trackIndex, mixDuration);
}

FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_add_empty_animation(spine_animation_state state, int32_t trackIndex, float mixDuration, float delay) {
    if (state == nullptr) return nullptr;
    AnimationState *_state = (AnimationState*)state;
    return _state->addEmptyAnimation(trackIndex, mixDuration, delay);
}

FFI_PLUGIN_EXPORT void spine_animation_state_set_empty_animations(spine_animation_state state, float mixDuration) {
    if (state == nullptr) return;
    AnimationState *_state = (AnimationState*)state;
    _state->setEmptyAnimations(mixDuration);
}

FFI_PLUGIN_EXPORT float spine_animation_state_get_time_scale(spine_animation_state state) {
    if (state == nullptr) return 0;
    AnimationState *_state = (AnimationState*)state;
    return _state->getTimeScale();
}

FFI_PLUGIN_EXPORT void spine_animation_state_set_time_scale(spine_animation_state state, float timeScale) {
    if (state == nullptr) return;
    AnimationState *_state = (AnimationState*)state;
    _state->setTimeScale(timeScale);
}

FFI_PLUGIN_EXPORT int spine_track_entry_get_track_index(spine_track_entry entry) {
    if (entry == nullptr) return 0;
    TrackEntry *_entry = (TrackEntry*)entry;
    return _entry->getTrackIndex();
}

FFI_PLUGIN_EXPORT spine_animation spine_track_entry_get_animation(spine_track_entry entry) {
    if (entry == nullptr) return nullptr;
    TrackEntry *_entry = (TrackEntry*)entry;
    return _entry->getAnimation();
}

FFI_PLUGIN_EXPORT spine_track_entry spine_track_entry_get_previous(spine_track_entry entry) {
    if (entry == nullptr) return nullptr;
    TrackEntry *_entry = (TrackEntry*)entry;
    return _entry->getPrevious();
}

FFI_PLUGIN_EXPORT int spine_track_entry_get_loop(spine_track_entry entry) {
    if (entry == nullptr) return 0;
    TrackEntry *_entry = (TrackEntry*)entry;
    return _entry->getLoop() ? -1 : 0;
}

FFI_PLUGIN_EXPORT void spine_track_entry_set_loop(spine_track_entry entry, int loop) {
    if (entry == nullptr) return;
    TrackEntry *_entry = (TrackEntry*)entry;
    _entry->setLoop(loop);
}

FFI_PLUGIN_EXPORT int spine_track_entry_get_hold_previous(spine_track_entry entry) {
    if (entry == nullptr) return 0;
    TrackEntry *_entry = (TrackEntry*)entry;
    return _entry->getHoldPrevious() ? -1 : 0;
}

FFI_PLUGIN_EXPORT void spine_track_entry_set_hold_previous(spine_track_entry entry, int holdPrevious) {
    if (entry == nullptr) return;
    TrackEntry *_entry = (TrackEntry*)entry;
    _entry->setHoldPrevious(holdPrevious);
}

FFI_PLUGIN_EXPORT int spine_track_entry_get_reverse(spine_track_entry entry) {
    if (entry == nullptr) return 0;
    TrackEntry *_entry = (TrackEntry*)entry;
    return _entry->getReverse() ? -1 : 0;
}

FFI_PLUGIN_EXPORT void spine_track_entry_set_reverse(spine_track_entry entry, int reverse) {
    if (entry == nullptr) return;
    TrackEntry *_entry = (TrackEntry*)entry;
    _entry->setReverse(reverse);
}

FFI_PLUGIN_EXPORT int spine_track_entry_get_shortest_rotation(spine_track_entry entry) {
    if (entry == nullptr) return 0;
    TrackEntry *_entry = (TrackEntry*)entry;
    return _entry->getShortestRotation() ? -1 : 0;
}

FFI_PLUGIN_EXPORT void spine_track_entry_set_shortest_rotation(spine_track_entry entry, int shortestRotation) {
    if (entry == nullptr) return;
    TrackEntry *_entry = (TrackEntry*)entry;
    _entry->setShortestRotation(shortestRotation);
}

FFI_PLUGIN_EXPORT float spine_track_entry_get_delay(spine_track_entry entry) {
    if (entry == nullptr) return 0;
    TrackEntry *_entry = (TrackEntry*)entry;
    return _entry->getDelay();
}

FFI_PLUGIN_EXPORT void spine_track_entry_set_delay(spine_track_entry entry, float delay) {
    if (entry == nullptr) return;
    TrackEntry *_entry = (TrackEntry*)entry;
    _entry->setDelay(delay);
}

FFI_PLUGIN_EXPORT float spine_track_entry_get_track_time(spine_track_entry entry) {
    if (entry == nullptr) return 0;
    TrackEntry *_entry = (TrackEntry*)entry;
    return _entry->getTrackTime();
}

FFI_PLUGIN_EXPORT void spine_track_entry_set_track_time(spine_track_entry entry, float trackTime) {
    if (entry == nullptr) return;
    TrackEntry *_entry = (TrackEntry*)entry;
    _entry->setTrackTime(trackTime);
}

FFI_PLUGIN_EXPORT float spine_track_entry_get_track_end(spine_track_entry entry) {
    if (entry == nullptr) return 0;
    TrackEntry *_entry = (TrackEntry*)entry;
    return _entry->getTrackEnd();
}

FFI_PLUGIN_EXPORT void spine_track_entry_set_track_end(spine_track_entry entry, float trackEnd) {
    if (entry == nullptr) return;
    TrackEntry *_entry = (TrackEntry*)entry;
    _entry->setTrackEnd(trackEnd);
}

FFI_PLUGIN_EXPORT float spine_track_entry_get_animation_start(spine_track_entry entry) {
    if (entry == nullptr) return 0;
    TrackEntry *_entry = (TrackEntry*)entry;
    return _entry->getAnimationStart();
}

FFI_PLUGIN_EXPORT void spine_track_entry_set_animation_start(spine_track_entry entry, float animationStart) {
    if (entry == nullptr) return;
    TrackEntry *_entry = (TrackEntry*)entry;
    _entry->setAnimationStart(animationStart);
}

FFI_PLUGIN_EXPORT float spine_track_entry_get_animation_end(spine_track_entry entry) {
    if (entry == nullptr) return 0;
    TrackEntry *_entry = (TrackEntry*)entry;
    return _entry->getAnimationEnd();
}

FFI_PLUGIN_EXPORT void spine_track_entry_set_animation_end(spine_track_entry entry, float animationEnd) {
    if (entry == nullptr) return;
    TrackEntry *_entry = (TrackEntry*)entry;
    _entry->setAnimationEnd(animationEnd);
}

FFI_PLUGIN_EXPORT float spine_track_entry_get_animation_last(spine_track_entry entry) {
    if (entry == nullptr) return 0;
    TrackEntry *_entry = (TrackEntry*)entry;
    return _entry->getAnimationLast();
}

FFI_PLUGIN_EXPORT void spine_track_entry_set_animation_last(spine_track_entry entry, float animationLast) {
    if (entry == nullptr) return;
    TrackEntry *_entry = (TrackEntry*)entry;
    _entry->setAnimationLast(animationLast);
}

FFI_PLUGIN_EXPORT float spine_track_entry_get_animation_time(spine_track_entry entry) {
    if (entry == nullptr) return 0;
    TrackEntry *_entry = (TrackEntry*)entry;
    return _entry->getAnimationTime();
}

FFI_PLUGIN_EXPORT float spine_track_entry_get_time_scale(spine_track_entry entry) {
    if (entry == nullptr) return 0;
    TrackEntry *_entry = (TrackEntry*)entry;
    return _entry->getTimeScale();
}

FFI_PLUGIN_EXPORT void spine_track_entry_set_time_scale(spine_track_entry entry, float timeScale) {
    if (entry == nullptr) return;
    TrackEntry *_entry = (TrackEntry*)entry;
    _entry->setTimeScale(timeScale);
}

FFI_PLUGIN_EXPORT float spine_track_entry_get_alpha(spine_track_entry entry) {
    if (entry == nullptr) return 0;
    TrackEntry *_entry = (TrackEntry*)entry;
    return _entry->getAlpha();
}

FFI_PLUGIN_EXPORT void spine_track_entry_set_alpha(spine_track_entry entry, float alpha) {
    if (entry == nullptr) return;
    TrackEntry *_entry = (TrackEntry*)entry;
    _entry->setAlpha(alpha);
}

FFI_PLUGIN_EXPORT float spine_track_entry_get_event_threshold(spine_track_entry entry) {
    if (entry == nullptr) return 0;
    TrackEntry *_entry = (TrackEntry*)entry;
    return _entry->getEventThreshold();
}

FFI_PLUGIN_EXPORT void spine_track_entry_set_event_threshold(spine_track_entry entry, float eventThreshold) {
    if (entry == nullptr) return;
    TrackEntry *_entry = (TrackEntry*)entry;
    _entry->setEventThreshold(eventThreshold);
}

FFI_PLUGIN_EXPORT float spine_track_entry_get_attachment_threshold(spine_track_entry entry) {
    if (entry == nullptr) return 0;
    TrackEntry *_entry = (TrackEntry*)entry;
    return _entry->getAttachmentThreshold();
}

FFI_PLUGIN_EXPORT void spine_track_entry_set_attachment_threshold(spine_track_entry entry, float attachmentThreshold) {
    if (entry == nullptr) return;
    TrackEntry *_entry = (TrackEntry*)entry;
    _entry->setAttachmentThreshold(attachmentThreshold);
}

FFI_PLUGIN_EXPORT float spine_track_entry_get_draw_order_threshold(spine_track_entry entry) {
    if (entry == nullptr) return 0;
    TrackEntry *_entry = (TrackEntry*)entry;
    return _entry->getDrawOrderThreshold();
}

FFI_PLUGIN_EXPORT void spine_track_entry_set_draw_order_threshold(spine_track_entry entry, float drawOrderThreshold) {
    if (entry == nullptr) return;
    TrackEntry *_entry = (TrackEntry*)entry;
    _entry->setDrawOrderThreshold(drawOrderThreshold);
}

FFI_PLUGIN_EXPORT spine_track_entry spine_track_entry_get_next(spine_track_entry entry) {
    if (entry == nullptr) return nullptr;
    TrackEntry *_entry = (TrackEntry*)entry;
    return _entry->getNext();
}

FFI_PLUGIN_EXPORT int spine_track_entry_is_complete(spine_track_entry entry) {
    if (entry == nullptr) return 0;
    TrackEntry *_entry = (TrackEntry*)entry;
    return _entry->isComplete() ? -1 : 0;
}

FFI_PLUGIN_EXPORT float spine_track_entry_get_mix_time(spine_track_entry entry) {
    if (entry == nullptr) return 0;
    TrackEntry *_entry = (TrackEntry*)entry;
    return _entry->getMixTime();
}

FFI_PLUGIN_EXPORT void spine_track_entry_set_mix_time(spine_track_entry entry, float mixTime) {
    if (entry == nullptr) return;
    TrackEntry *_entry = (TrackEntry*)entry;
    _entry->setMixTime(mixTime);
}

FFI_PLUGIN_EXPORT float spine_track_entry_get_mix_duration(spine_track_entry entry) {
    if (entry == nullptr) return 0;
    TrackEntry *_entry = (TrackEntry*)entry;
    return _entry->getMixDuration();
}

FFI_PLUGIN_EXPORT void spine_track_entry_set_mix_duration(spine_track_entry entry, float mixDuration) {
    if (entry == nullptr) return;
    TrackEntry *_entry = (TrackEntry*)entry;
    _entry->setMixDuration(mixDuration);
}

FFI_PLUGIN_EXPORT spine_mix_blend spine_track_entry_get_mix_blend(spine_track_entry entry) {
    if (entry == nullptr) return SPINE_MIX_BLEND_SETUP;
    TrackEntry *_entry = (TrackEntry*)entry;
    return (spine_mix_blend)_entry->getMixBlend();
}

FFI_PLUGIN_EXPORT void spine_track_entry_set_mix_blend(spine_track_entry entry, spine_mix_blend mixBlend) {
    if (entry == nullptr) return;
    TrackEntry *_entry = (TrackEntry*)entry;
    _entry->setMixBlend((MixBlend)mixBlend);
}

FFI_PLUGIN_EXPORT spine_track_entry spine_track_entry_get_mixing_from(spine_track_entry entry) {
    if (entry == nullptr) return nullptr;
    TrackEntry *_entry = (TrackEntry*)entry;
    return _entry->getMixingFrom();
}

FFI_PLUGIN_EXPORT spine_track_entry spine_track_entry_get_mixing_to(spine_track_entry entry) {
    if (entry == nullptr) return nullptr;
    TrackEntry *_entry = (TrackEntry*)entry;
    return _entry->getMixingTo();
}

FFI_PLUGIN_EXPORT void spine_track_entry_reset_rotation_directions(spine_track_entry entry) {
    if (entry == nullptr) return;
    TrackEntry *_entry = (TrackEntry*)entry;
    _entry->resetRotationDirections();
}

FFI_PLUGIN_EXPORT float spine_track_entry_get_track_complete(spine_track_entry entry) {
    if (entry == nullptr) return 0;
    TrackEntry *_entry = (TrackEntry*)entry;
    return _entry->getTrackComplete();
}