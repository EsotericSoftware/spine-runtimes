#include "spine_flutter.h"
#include <spine/spine.h>
#include <spine/Version.h>
#include <spine/Debug.h>

using namespace spine;

struct AnimationStateEvent {
    EventType type;
    TrackEntry *entry;
    Event* event;
    AnimationStateEvent( EventType type, TrackEntry *entry, Event* event): type(type), entry(entry), event(event) {};
};

struct EventListener: public AnimationStateListenerObject {
    Vector<AnimationStateEvent> events;

    void callback(AnimationState *state, EventType type, TrackEntry *entry, Event *event) {
        events.add(AnimationStateEvent(type, entry, event));
    }
};

spine::SpineExtension *spine::getDefaultExtension() {
   return new spine::DebugExtension(new spine::DefaultSpineExtension());
}

FFI_PLUGIN_EXPORT int spine_major_version() {
    return SPINE_MAJOR_VERSION;
}

FFI_PLUGIN_EXPORT int spine_minor_version() {
    return SPINE_MINOR_VERSION;
}

FFI_PLUGIN_EXPORT spine_atlas* spine_atlas_load(const char *atlasData) {
    if (!atlasData) return nullptr;
    int length = (int)strlen(atlasData);
    auto atlas = new Atlas(atlasData, length, "", (TextureLoader*)nullptr, false);
    spine_atlas *result = SpineExtension::calloc<spine_atlas>(1, __FILE__, __LINE__);
    result->atlas = atlas;
    result->numImagePaths = (int)atlas->getPages().size();
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

FFI_PLUGIN_EXPORT spine_skeleton_data_result spine_skeleton_data_load_json(spine_atlas *atlas, const char *skeletonData) {
    spine_skeleton_data_result result = { nullptr, nullptr };
    Bone::setYDown(true);
    if (!atlas) return result;
    if (!atlas->atlas) return result;
    if (!skeletonData) return result;
    SkeletonJson json((Atlas*)atlas->atlas);
    SkeletonData *data = json.readSkeletonData(skeletonData);
    result.skeletonData = data;
    if (!json.getError().isEmpty()) {
        result.error = strdup(json.getError().buffer());
    }
    return result;
}

FFI_PLUGIN_EXPORT spine_skeleton_data_result spine_skeleton_data_load_binary(spine_atlas *atlas, const unsigned char *skeletonData, int length) {
    spine_skeleton_data_result result = { nullptr, nullptr };
    Bone::setYDown(true);
    if (!atlas) return result;
    if (!atlas->atlas) return result;
    if (!skeletonData) return result;
    if (length <= 0) return result;
    SkeletonBinary binary((Atlas*)atlas->atlas);
    SkeletonData *data = binary.readSkeletonData(skeletonData, length);
    result.skeletonData = data;
    if (!binary.getError().isEmpty()) {
        result.error = strdup(binary.getError().buffer());
    }
    return result;
}

FFI_PLUGIN_EXPORT void spine_skeleton_data_dispose(spine_skeleton_data skeletonData) {
    if (!skeletonData) return;
    delete (SkeletonData*)skeletonData;
}

spine_render_command *spine_render_command_create(int numVertices, int numIndices, spine_blend_mode blendMode, int pageIndex) {
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

FFI_PLUGIN_EXPORT spine_skeleton_drawable *spine_skeleton_drawable_create(spine_skeleton_data skeletonData) {
    spine_skeleton_drawable *drawable = SpineExtension::calloc<spine_skeleton_drawable>(1, __FILE__, __LINE__);
    drawable->skeleton = new Skeleton((SkeletonData*)skeletonData);
    AnimationState *state = new AnimationState(new AnimationStateData((SkeletonData*)skeletonData));
    drawable->animationState = state;
    state->setManualTrackEntryDisposal(true);
    EventListener *listener =  new EventListener();
    drawable->animationStateEvents = listener;
    state->setListener(listener);
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
    if (drawable->animationStateEvents) delete (Vector<AnimationStateEvent>*)(drawable->animationStateEvents);
    if (drawable->clipping) delete (SkeletonClipping*)drawable->clipping;
    while (drawable->renderCommand) {
        spine_render_command *cmd = drawable->renderCommand;
        drawable->renderCommand = cmd->next;
        spine_render_command_dispose(cmd);
    }
    SpineExtension::free(drawable, __FILE__, __LINE__);
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

FFI_PLUGIN_EXPORT void spine_animation_state_dispose_track_entry(spine_animation_state state, spine_track_entry entry) {
    if (state == nullptr) return;
    if (entry == nullptr) return;
    AnimationState *_state = (AnimationState*)state;
    _state->disposeTrackEntry((TrackEntry*)entry);
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

FFI_PLUGIN_EXPORT void spine_animation_state_clear_track(spine_animation_state state, int trackIndex) {
    if (state == nullptr) return;
    AnimationState *_state = (AnimationState*)state;
    _state->clearTrack(trackIndex);
}

FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_set_animation(spine_animation_state state, int trackIndex, const char* animationName, int loop) {
    if (state == nullptr) return nullptr;
    AnimationState *_state = (AnimationState*)state;
    return _state->setAnimation(trackIndex, animationName, loop);
}

FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_add_animation(spine_animation_state state, int trackIndex, const char* animationName, int loop, float delay) {
    if (state == nullptr) return nullptr;
    AnimationState *_state = (AnimationState*)state;
    return _state->addAnimation(trackIndex, animationName, loop, delay);
}

FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_set_empty_animation(spine_animation_state state, int trackIndex, float mixDuration) {
    if (state == nullptr) return nullptr;
    AnimationState *_state = (AnimationState*)state;
    return _state->setEmptyAnimation(trackIndex, mixDuration);
}

FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_add_empty_animation(spine_animation_state state, int trackIndex, float mixDuration, float delay) {
    if (state == nullptr) return nullptr;
    AnimationState *_state = (AnimationState*)state;
    return _state->addEmptyAnimation(trackIndex, mixDuration, delay);
}

FFI_PLUGIN_EXPORT void spine_animation_state_set_empty_animations(spine_animation_state state, float mixDuration) {
    if (state == nullptr) return;
    AnimationState *_state = (AnimationState*)state;
    _state->setEmptyAnimations(mixDuration);
}

FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_get_current(spine_animation_state state, int trackIndex) {
    if (state == nullptr) return nullptr;
    AnimationState *_state = (AnimationState*)state;
    return _state->getCurrent(trackIndex);
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

FFI_PLUGIN_EXPORT int spine_animation_state_events_get_num_events(spine_animation_state_events events) {
    if (events == nullptr) return 0;
    EventListener *_events = (EventListener*)events;
    return _events->events.size();
}

FFI_PLUGIN_EXPORT spine_event_type spine_animation_state_events_get_event_type(spine_animation_state_events events, int index) {
    if (events == nullptr) return SPINE_EVENT_TYPE_DISPOSE;
    if (index < 0) return SPINE_EVENT_TYPE_DISPOSE;
    EventListener *_events = (EventListener*)events;
    if (index >= _events->events.size()) return SPINE_EVENT_TYPE_DISPOSE;
    return (spine_event_type)_events->events[index].type;
}

FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_events_get_track_entry(spine_animation_state_events events, int index) {
    if (events == nullptr) return nullptr;
    EventListener *_events = (EventListener*)events;
    if (index >= _events->events.size()) return nullptr;
    return (spine_track_entry)_events->events[index].entry;
}

FFI_PLUGIN_EXPORT spine_event spine_animation_state_events_get_event(spine_animation_state_events events, int index) {
    if (events == nullptr) return nullptr;
    EventListener *_events = (EventListener*)events;
    if (index >= _events->events.size()) return nullptr;
    return (spine_track_entry)_events->events[index].event;
}

FFI_PLUGIN_EXPORT void spine_animation_state_events_reset(spine_animation_state_events events) {
    if (events == nullptr) return;
    EventListener *_events = (EventListener*)events;
    _events->events.clear();
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

FFI_PLUGIN_EXPORT void spine_skeleton_update_cache(spine_skeleton skeleton) {
    if (skeleton == nullptr) return;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    _skeleton->updateCache();
}

FFI_PLUGIN_EXPORT void spine_skeleton_update_world_transform(spine_skeleton skeleton) {
    if (skeleton == nullptr) return;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    _skeleton->updateWorldTransform();
}

FFI_PLUGIN_EXPORT void spine_skeleton_update_world_transform_bone(spine_skeleton skeleton, spine_bone parent) {
    if (skeleton == nullptr) return;
    if (parent == nullptr) return;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    Bone *_bone = (Bone*)parent;
    _skeleton->updateWorldTransform(_bone);
}

FFI_PLUGIN_EXPORT void spine_skeleton_set_to_setup_pose(spine_skeleton skeleton) {
    if (skeleton == nullptr) return;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    _skeleton->setToSetupPose();
}

FFI_PLUGIN_EXPORT void spine_skeleton_set_bones_to_setup_pose(spine_skeleton skeleton) {
    if (skeleton == nullptr) return;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    _skeleton->setBonesToSetupPose();
}

FFI_PLUGIN_EXPORT void spine_skeleton_set_slots_to_setup_pose(spine_skeleton skeleton) {
    if (skeleton == nullptr) return;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    _skeleton->setSlotsToSetupPose();
}

FFI_PLUGIN_EXPORT spine_bone spine_skeleton_find_bone(spine_skeleton skeleton, const char* boneName) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return _skeleton->findBone(boneName);
}

FFI_PLUGIN_EXPORT spine_slot spine_skeleton_find_slot(spine_skeleton skeleton, const char* slotName) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return _skeleton->findSlot(slotName);
}

FFI_PLUGIN_EXPORT void spine_skeleton_set_skin_by_name(spine_skeleton skeleton, const char* skinName) {
    if (skeleton == nullptr) return;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    _skeleton->setSkin(skinName);
}

FFI_PLUGIN_EXPORT void spine_skeleton_set_skin(spine_skeleton skeleton, spine_skin skin) {
    if (skeleton == nullptr) return;
    if (skin == nullptr) return;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    _skeleton->setSkin((Skin*)skin);
}

FFI_PLUGIN_EXPORT spine_attachment spine_skeleton_get_attachment_by_name(spine_skeleton skeleton, const char* slotName, const char* attachmentName) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return _skeleton->getAttachment(slotName, attachmentName);
}

FFI_PLUGIN_EXPORT spine_attachment spine_skeleton_get_attachment(spine_skeleton skeleton, int slotIndex, const char* attachmentName) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return _skeleton->getAttachment(slotIndex, attachmentName);
}

FFI_PLUGIN_EXPORT void spine_skeleton_set_attachment(spine_skeleton skeleton, const char* slotName, const char* attachmentName) {
    if (skeleton == nullptr) return;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return _skeleton->setAttachment(slotName, attachmentName);
}

FFI_PLUGIN_EXPORT spine_ik_constraint spine_skeleton_find_ik_constraint(spine_skeleton skeleton, const char* constraintName) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return _skeleton->findIkConstraint(constraintName);
}

FFI_PLUGIN_EXPORT spine_transform_constraint spine_skeleton_find_transform_constraint(spine_skeleton skeleton, const char* constraintName) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return _skeleton->findTransformConstraint(constraintName);
}

FFI_PLUGIN_EXPORT spine_path_constraint spine_skeleton_find_path_constraint(spine_skeleton skeleton, const char* constraintName) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return _skeleton->findPathConstraint(constraintName);
}

FFI_PLUGIN_EXPORT spine_bounds spine_skeleton_get_bounds(spine_skeleton skeleton) {
    spine_bounds bounds = {0, 0, 0, 0};
    if (skeleton == nullptr) return bounds;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    Vector<float> vertices;
    _skeleton->getBounds(bounds.x, bounds.y, bounds.width, bounds.height, vertices);
    return bounds;
}

FFI_PLUGIN_EXPORT spine_bone spine_skeleton_get_root_bone(spine_skeleton skeleton) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return _skeleton->getRootBone();
}

FFI_PLUGIN_EXPORT spine_skeleton_data spine_skeleton_get_data(spine_skeleton skeleton) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return _skeleton->getData();
}

FFI_PLUGIN_EXPORT int spine_skeleton_get_num_bones(spine_skeleton skeleton) {
    if (skeleton == nullptr) return 0;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return _skeleton->getBones().size();
}

FFI_PLUGIN_EXPORT spine_bone* spine_skeleton_get_bones(spine_skeleton skeleton) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return (void**)_skeleton->getBones().buffer();
}

FFI_PLUGIN_EXPORT int spine_skeleton_get_num_slots(spine_skeleton skeleton) {
    if (skeleton == nullptr) return 0;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return _skeleton->getSlots().size();
}

FFI_PLUGIN_EXPORT spine_slot* spine_skeleton_get_slots(spine_skeleton skeleton) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return (void**)_skeleton->getSlots().buffer();
}

FFI_PLUGIN_EXPORT int spine_skeleton_get_num_draw_order(spine_skeleton skeleton) {
    if (skeleton == nullptr) return 0;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return _skeleton->getDrawOrder().size();
}

FFI_PLUGIN_EXPORT spine_slot* spine_skeleton_get_draw_order(spine_skeleton skeleton) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return (void**)_skeleton->getDrawOrder().buffer();
}

FFI_PLUGIN_EXPORT int spine_skeleton_get_num_ik_constraints(spine_skeleton skeleton) {
    if (skeleton == nullptr) return 0;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return _skeleton->getIkConstraints().size();
}

FFI_PLUGIN_EXPORT spine_ik_constraint* spine_skeleton_get_ik_constraints(spine_skeleton skeleton) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return (void**)_skeleton->getIkConstraints().buffer();
}

FFI_PLUGIN_EXPORT int spine_skeleton_get_num_transform_constraints(spine_skeleton skeleton) {
    if (skeleton == nullptr) return 0;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return _skeleton->getTransformConstraints().size();
}

FFI_PLUGIN_EXPORT spine_transform_constraint* spine_skeleton_get_transform_constraints(spine_skeleton skeleton) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return (void**)_skeleton->getTransformConstraints().buffer();
}

FFI_PLUGIN_EXPORT int spine_skeleton_get_num_path_constraints(spine_skeleton skeleton) {
    if (skeleton == nullptr) return 0;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return _skeleton->getPathConstraints().size();
}

FFI_PLUGIN_EXPORT spine_path_constraint* spine_skeleton_get_path_constraints(spine_skeleton skeleton) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return (void**)_skeleton->getPathConstraints().buffer();
}

FFI_PLUGIN_EXPORT spine_skin spine_skeleton_get_skin(spine_skeleton skeleton) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return _skeleton->getSkin();
}

FFI_PLUGIN_EXPORT spine_color spine_skeleton_get_color(spine_skeleton skeleton) {
    spine_color color = {0, 0, 0, 0};
    if (skeleton == nullptr) return color;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    Color &c = _skeleton->getColor();
    color = { c.r, c.g, c.b, c.a };
    return color;
}

FFI_PLUGIN_EXPORT void spine_skeleton_set_color(spine_skeleton skeleton, float r, float g, float b, float a) {
    if (skeleton == nullptr) return;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    _skeleton->getColor().set(r, g, b, a);
}

FFI_PLUGIN_EXPORT void spine_skeleton_set_position(spine_skeleton skeleton, float x, float y) {
    if (skeleton == nullptr) return;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    _skeleton->setPosition(x, y);
}

FFI_PLUGIN_EXPORT float spine_skeleton_get_x(spine_skeleton skeleton) {
    if (skeleton == nullptr) return 0;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return _skeleton->getX();
}

FFI_PLUGIN_EXPORT void spine_skeleton_set_x(spine_skeleton skeleton, float x) {
    if (skeleton == nullptr) return;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    _skeleton->setX(x);
}

FFI_PLUGIN_EXPORT float spine_skeleton_get_y(spine_skeleton skeleton) {
    if (skeleton == nullptr) return 0;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return _skeleton->getY();
}

FFI_PLUGIN_EXPORT void spine_skeleton_set_y(spine_skeleton skeleton, float y) {
    if (skeleton == nullptr) return;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    _skeleton->setY(y);
}

FFI_PLUGIN_EXPORT float spine_skeleton_get_scale_x(spine_skeleton skeleton) {
    if (skeleton == nullptr) return 0;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return _skeleton->getScaleX();
}

FFI_PLUGIN_EXPORT void spine_skeleton_set_scale_x(spine_skeleton skeleton, float scaleX) {
    if (skeleton == nullptr) return;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    _skeleton->setScaleX(scaleX);
}

FFI_PLUGIN_EXPORT float spine_skeleton_get_scale_y(spine_skeleton skeleton) {
    if (skeleton == nullptr) return 0;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return _skeleton->getScaleY();
}

FFI_PLUGIN_EXPORT void spine_skeleton_set_scale_y(spine_skeleton skeleton, float scaleY) {
    if (skeleton == nullptr) return;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    _skeleton->setScaleY(scaleY);
}

FFI_PLUGIN_EXPORT spine_event_data spine_event_get_data(spine_event event) {
    if (event == nullptr) return nullptr;
    Event *_event = (Event*)event;
    return (spine_event_data)&_event->getData();
}

FFI_PLUGIN_EXPORT float spine_event_get_time(spine_event event) {
    if (event == nullptr) return 0;
    Event *_event = (Event*)event;
    return _event->getTime();
}

FFI_PLUGIN_EXPORT int spine_event_get_int_value(spine_event event) {
    if (event == nullptr) return 0;
    Event *_event = (Event*)event;
    return _event->getIntValue();
}

FFI_PLUGIN_EXPORT float spine_event_get_float_value(spine_event event) {
    if (event == nullptr) return 0;
    Event *_event = (Event*)event;
    return _event->getFloatValue();
}

FFI_PLUGIN_EXPORT const char* spine_event_get_string_value(spine_event event) {
    if (event == nullptr) return nullptr;
    Event *_event = (Event*)event;
    return _event->getStringValue().buffer();
}

FFI_PLUGIN_EXPORT float spine_event_get_volume(spine_event event) {
    if (event == nullptr) return 0;
    Event *_event = (Event*)event;
    return _event->getVolume();
}

FFI_PLUGIN_EXPORT float spine_event_get_balance(spine_event event) {
    if (event == nullptr) return 0;
    Event *_event = (Event*)event;
    return _event->getBalance();
}