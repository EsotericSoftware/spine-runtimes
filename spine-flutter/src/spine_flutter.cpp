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

// Atlas

FFI_PLUGIN_EXPORT spine_atlas* spine_atlas_load(const char *atlasData) {
    if (!atlasData) return nullptr;
    int length = (int)strlen(atlasData);
    auto atlas = new (__FILE__, __LINE__) Atlas(atlasData, length, "", (TextureLoader*)nullptr, false);
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

// SkeletonData

FFI_PLUGIN_EXPORT spine_skeleton_data_result spine_skeleton_data_load_json(spine_atlas *atlas, const char *skeletonData) {
    spine_skeleton_data_result result = { nullptr, nullptr };
    Bone::setYDown(true);
    if (!atlas) return result;
    if (!atlas->atlas) return result;
    if (!skeletonData) return result;
    SkeletonJson json((Atlas*)atlas->atlas);
    SkeletonData *data = json.readSkeletonData(skeletonData);
    result.skeletonData = (spine_skeleton_data)data;
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
    result.skeletonData = (spine_skeleton_data)data;
    if (!binary.getError().isEmpty()) {
        result.error = strdup(binary.getError().buffer());
    }
    return result;
}

FFI_PLUGIN_EXPORT spine_bone_data spine_skeleton_data_find_bone(spine_skeleton_data data, const char *name) {
    if (data == nullptr) return nullptr;
    SkeletonData *_data = (SkeletonData*)data;
    return (spine_bone_data)_data->findBone(name);
}

FFI_PLUGIN_EXPORT spine_slot_data spine_skeleton_data_find_slot(spine_skeleton_data data, const char *name) {
    if (data == nullptr) return nullptr;
    SkeletonData *_data = (SkeletonData*)data;
    return (spine_slot_data)_data->findSlot(name);
}

FFI_PLUGIN_EXPORT spine_skin spine_skeleton_data_find_skin(spine_skeleton_data data, const char *name) {
    if (data == nullptr) return nullptr;
    SkeletonData *_data = (SkeletonData*)data;
    return (spine_skin)_data->findSkin(name);
}

FFI_PLUGIN_EXPORT spine_event_data spine_skeleton_data_find_event(spine_skeleton_data data, const char *name) {
    if (data == nullptr) return nullptr;
    SkeletonData *_data = (SkeletonData*)data;
    return (spine_event_data)_data->findEvent(name);
}

FFI_PLUGIN_EXPORT spine_animation spine_skeleton_data_find_animation(spine_skeleton_data data, const char *name) {
    if (data == nullptr) return nullptr;
    SkeletonData *_data = (SkeletonData*)data;
    return (spine_animation)_data->findAnimation(name);
}

FFI_PLUGIN_EXPORT spine_ik_constraint_data spine_skeleton_data_find_ik_constraint(spine_skeleton_data data, const char *name) {
    if (data == nullptr) return nullptr;
    SkeletonData *_data = (SkeletonData*)data;
    return (spine_ik_constraint_data)_data->findIkConstraint(name);
}

FFI_PLUGIN_EXPORT spine_transform_constraint_data spine_skeleton_data_find_transform_constraint(spine_skeleton_data data, const char *name) {
    if (data == nullptr) return nullptr;
    SkeletonData *_data = (SkeletonData*)data;
    return (spine_transform_constraint_data)_data->findTransformConstraint(name);
}

FFI_PLUGIN_EXPORT spine_path_constraint_data spine_skeleton_data_find_path_constraint(spine_skeleton_data data, const char *name) {
    if (data == nullptr) return nullptr;
    SkeletonData *_data = (SkeletonData*)data;
    return (spine_path_constraint_data)_data->findPathConstraint(name);
}

FFI_PLUGIN_EXPORT const char* spine_skeleton_data_get_name(spine_skeleton_data data) {
    if (data == nullptr) return nullptr;
    SkeletonData *_data = (SkeletonData*)data;
    return _data->getName().buffer();
}

FFI_PLUGIN_EXPORT int spine_skeleton_data_get_num_bones(spine_skeleton_data data) {
    if (data == nullptr) return 0;
    SkeletonData *_data = (SkeletonData*)data;
    return (int)_data->getBones().size();
}

FFI_PLUGIN_EXPORT spine_bone_data* spine_skeleton_data_get_bones(spine_skeleton_data data) {
    if (data == nullptr) return nullptr;
    SkeletonData *_data = (SkeletonData*)data;
    return (spine_bone_data*)_data->getBones().buffer();
}

FFI_PLUGIN_EXPORT int spine_skeleton_data_get_num_slots(spine_skeleton_data data) {
    if (data == nullptr) return 0;
    SkeletonData *_data = (SkeletonData*)data;
    return (int)_data->getSlots().size();
}

FFI_PLUGIN_EXPORT spine_slot_data* spine_skeleton_data_get_slots(spine_skeleton_data data) {
    if (data == nullptr) return nullptr;
    SkeletonData *_data = (SkeletonData*)data;
    return (spine_slot_data*)_data->getSlots().buffer();
}

FFI_PLUGIN_EXPORT int spine_skeleton_data_get_num_skins(spine_skeleton_data data) {
    if (data == nullptr) return 0;
    SkeletonData *_data = (SkeletonData*)data;
    return (int)_data->getSkins().size();
}

FFI_PLUGIN_EXPORT spine_skin* spine_skeleton_data_get_skins(spine_skeleton_data data) {
    if (data == nullptr) return nullptr;
    SkeletonData *_data = (SkeletonData*)data;
    return (spine_skin*)_data->getSkins().buffer();
}

FFI_PLUGIN_EXPORT spine_skin spine_skeleton_data_get_default_skin(spine_skeleton_data data) {
    if (data == nullptr) return nullptr;
    SkeletonData *_data = (SkeletonData*)data;
    return (spine_skin)_data->getDefaultSkin();
}

FFI_PLUGIN_EXPORT void spine_skeleton_data_set_default_skin(spine_skeleton_data data, spine_skin skin) {
    if (data == nullptr) return;
    SkeletonData *_data = (SkeletonData*)data;
    _data->setDefaultSkin((Skin*)skin);
}

FFI_PLUGIN_EXPORT int spine_skeleton_data_get_num_events(spine_skeleton_data data) {
    if (data == nullptr) return 0;
    SkeletonData *_data = (SkeletonData*)data;
    return (int)_data->getEvents().size();
}

FFI_PLUGIN_EXPORT spine_event_data* spine_skeleton_data_get_events(spine_skeleton_data data) {
    if (data == nullptr) return nullptr;
    SkeletonData *_data = (SkeletonData*)data;
    return (spine_event_data*)_data->getEvents().buffer();
}

FFI_PLUGIN_EXPORT int spine_skeleton_data_get_num_animations(spine_skeleton_data data) {
    if (data == nullptr) return 0;
    SkeletonData *_data = (SkeletonData*)data;
    return (int)_data->getAnimations().size();
}

FFI_PLUGIN_EXPORT spine_animation* spine_skeleton_data_get_animations(spine_skeleton_data data) {
    if (data == nullptr) return nullptr;
    SkeletonData *_data = (SkeletonData*)data;
    return (spine_animation*)_data->getAnimations().buffer();
}

FFI_PLUGIN_EXPORT int spine_skeleton_data_get_num_ik_constraints(spine_skeleton_data data) {
    if (data == nullptr) return 0;
    SkeletonData *_data = (SkeletonData*)data;
    return (int)_data->getIkConstraints().size();
}

FFI_PLUGIN_EXPORT spine_ik_constraint_data* spine_skeleton_data_get_ik_constraints(spine_skeleton_data data) {
    if (data == nullptr) return nullptr;
    SkeletonData *_data = (SkeletonData*)data;
    return (spine_ik_constraint_data*)_data->getIkConstraints().buffer();
}

FFI_PLUGIN_EXPORT int spine_skeleton_data_get_num_transform_constraints(spine_skeleton_data data) {
    if (data == nullptr) return 0;
    SkeletonData *_data = (SkeletonData*)data;
    return (int)_data->getTransformConstraints().size();
}

FFI_PLUGIN_EXPORT spine_transform_constraint_data* spine_skeleton_data_get_transform_constraints(spine_skeleton_data data) {
    if (data == nullptr) return nullptr;
    SkeletonData *_data = (SkeletonData*)data;
    return (spine_transform_constraint_data*)_data->getTransformConstraints().buffer();
}

FFI_PLUGIN_EXPORT int spine_skeleton_data_get_num_path_constraints(spine_skeleton_data data) {
    if (data == nullptr) return 0;
    SkeletonData *_data = (SkeletonData*)data;
    return (int)_data->getPathConstraints().size();
}

FFI_PLUGIN_EXPORT spine_path_constraint_data* spine_skeleton_data_get_path_constraints(spine_skeleton_data data) {
    if (data == nullptr) return nullptr;
    SkeletonData *_data = (SkeletonData*)data;
    return (spine_path_constraint_data*)_data->getPathConstraints().buffer();
}

FFI_PLUGIN_EXPORT float spine_skeleton_data_get_x(spine_skeleton_data data) {
    if (data == nullptr) return 0;
    SkeletonData *_data = (SkeletonData*)data;
    return _data->getX();
}

FFI_PLUGIN_EXPORT void spine_skeleton_data_set_x(spine_skeleton_data data, float x) {
    if (data == nullptr) return;
    SkeletonData *_data = (SkeletonData*)data;
    _data->setX(x);
}

FFI_PLUGIN_EXPORT float spine_skeleton_data_get_y(spine_skeleton_data data) {
    if (data == nullptr) return 0;
    SkeletonData *_data = (SkeletonData*)data;
    return _data->getY();
}

FFI_PLUGIN_EXPORT void spine_skeleton_data_set_y(spine_skeleton_data data, float y) {
    if (data == nullptr) return;
    SkeletonData *_data = (SkeletonData*)data;
    _data->setY(y);
}

FFI_PLUGIN_EXPORT float spine_skeleton_data_get_width(spine_skeleton_data data) {
    if (data == nullptr) return 0;
    SkeletonData *_data = (SkeletonData*)data;
    return _data->getWidth();
}

FFI_PLUGIN_EXPORT void spine_skeleton_data_set_width(spine_skeleton_data data, float width) {
    if (data == nullptr) return;
    SkeletonData *_data = (SkeletonData*)data;
    _data->setWidth(width);
}

FFI_PLUGIN_EXPORT float spine_skeleton_data_get_height(spine_skeleton_data data) {
    if (data == nullptr) return 0;
    SkeletonData *_data = (SkeletonData*)data;
    return _data->getHeight();
}

FFI_PLUGIN_EXPORT void spine_skeleton_data_set_height(spine_skeleton_data data, float height) {
    if (data == nullptr) return;
    SkeletonData *_data = (SkeletonData*)data;
    _data->setHeight(height);
}

FFI_PLUGIN_EXPORT const char* spine_skeleton_data_get_version(spine_skeleton_data data) {
    if (data == nullptr) return nullptr;
    SkeletonData *_data = (SkeletonData*)data;
    return _data->getVersion().buffer();
}

FFI_PLUGIN_EXPORT const char* spine_skeleton_data_get_hash(spine_skeleton_data data) {
    if (data == nullptr) return nullptr;
    SkeletonData *_data = (SkeletonData*)data;
    return _data->getHash().buffer();
}

FFI_PLUGIN_EXPORT const char* spine_skeleton_data_get_images_path(spine_skeleton_data data) {
    if (data == nullptr) return nullptr;
    SkeletonData *_data = (SkeletonData*)data;
    return _data->getImagesPath().buffer();
}

FFI_PLUGIN_EXPORT const char* spine_skeleton_data_get_audio_path(spine_skeleton_data data) {
    if (data == nullptr) return nullptr;
    SkeletonData *_data = (SkeletonData*)data;
    return _data->getAudioPath().buffer();
}

FFI_PLUGIN_EXPORT float spine_skeleton_data_get_fps(spine_skeleton_data data) {
    if (data == nullptr) return 0;
    SkeletonData *_data = (SkeletonData*)data;
    return _data->getFps();
}

FFI_PLUGIN_EXPORT void spine_skeleton_data_dispose(spine_skeleton_data data) {
    if (!data) return;
    delete (SkeletonData*)data;
}

// RenderCommand
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

// SkeletonDrawable

FFI_PLUGIN_EXPORT spine_skeleton_drawable *spine_skeleton_drawable_create(spine_skeleton_data skeletonData) {
    spine_skeleton_drawable *drawable = SpineExtension::calloc<spine_skeleton_drawable>(1, __FILE__, __LINE__);
    drawable->skeleton = (spine_skeleton)new (__FILE__, __LINE__) Skeleton((SkeletonData*)skeletonData);
    AnimationState *state = new (__FILE__, __LINE__) AnimationState(new AnimationStateData((SkeletonData*)skeletonData));
    drawable->animationState = (spine_animation_state)state;
    state->setManualTrackEntryDisposal(true);
    EventListener *listener =  new EventListener();
    drawable->animationStateEvents = (spine_animation_state_events)listener;
    state->setListener(listener);
    drawable->clipping = new (__FILE__, __LINE__) SkeletonClipping();
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
        int verticesCount;
        Vector<float> *uvs;
        Vector<unsigned short> *indices;
        int indicesCount;
        Color *attachmentColor;
        int pageIndex;

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

// Animation

FFI_PLUGIN_EXPORT const char* spine_animation_get_name(spine_animation animation) {
    if (animation == nullptr) return nullptr;
    Animation *_animation = (Animation*)animation;
    return _animation->getName().buffer();
}

FFI_PLUGIN_EXPORT float spine_animation_get_duration(spine_animation animation) {
    if (animation == nullptr) return 0;
    Animation *_animation = (Animation*)animation;
    return _animation->getDuration();
}

// AnimationState

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
    return (spine_track_entry)_state->setAnimation(trackIndex, animationName, loop);
}

FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_add_animation(spine_animation_state state, int trackIndex, const char* animationName, int loop, float delay) {
    if (state == nullptr) return nullptr;
    AnimationState *_state = (AnimationState*)state;
    return (spine_track_entry)_state->addAnimation(trackIndex, animationName, loop, delay);
}

FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_set_empty_animation(spine_animation_state state, int trackIndex, float mixDuration) {
    if (state == nullptr) return nullptr;
    AnimationState *_state = (AnimationState*)state;
    return (spine_track_entry)_state->setEmptyAnimation(trackIndex, mixDuration);
}

FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_add_empty_animation(spine_animation_state state, int trackIndex, float mixDuration, float delay) {
    if (state == nullptr) return nullptr;
    AnimationState *_state = (AnimationState*)state;
    return (spine_track_entry)_state->addEmptyAnimation(trackIndex, mixDuration, delay);
}

FFI_PLUGIN_EXPORT void spine_animation_state_set_empty_animations(spine_animation_state state, float mixDuration) {
    if (state == nullptr) return;
    AnimationState *_state = (AnimationState*)state;
    _state->setEmptyAnimations(mixDuration);
}

FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_get_current(spine_animation_state state, int trackIndex) {
    if (state == nullptr) return nullptr;
    AnimationState *_state = (AnimationState*)state;
    return (spine_track_entry)_state->getCurrent(trackIndex);
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
    return (int)_events->events.size();
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
    return (spine_event)_events->events[index].event;
}

FFI_PLUGIN_EXPORT void spine_animation_state_events_reset(spine_animation_state_events events) {
    if (events == nullptr) return;
    EventListener *_events = (EventListener*)events;
    _events->events.clear();
}

// TrackEntry

FFI_PLUGIN_EXPORT int spine_track_entry_get_track_index(spine_track_entry entry) {
    if (entry == nullptr) return 0;
    TrackEntry *_entry = (TrackEntry*)entry;
    return _entry->getTrackIndex();
}

FFI_PLUGIN_EXPORT spine_animation spine_track_entry_get_animation(spine_track_entry entry) {
    if (entry == nullptr) return nullptr;
    TrackEntry *_entry = (TrackEntry*)entry;
    return (spine_animation)_entry->getAnimation();
}

FFI_PLUGIN_EXPORT spine_track_entry spine_track_entry_get_previous(spine_track_entry entry) {
    if (entry == nullptr) return nullptr;
    TrackEntry *_entry = (TrackEntry*)entry;
    return (spine_track_entry)_entry->getPrevious();
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
    return (spine_track_entry)_entry->getNext();
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
    return (spine_track_entry)_entry->getMixingFrom();
}

FFI_PLUGIN_EXPORT spine_track_entry spine_track_entry_get_mixing_to(spine_track_entry entry) {
    if (entry == nullptr) return nullptr;
    TrackEntry *_entry = (TrackEntry*)entry;
    return (spine_track_entry)_entry->getMixingTo();
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

// Skeleton

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
    return (spine_bone)_skeleton->findBone(boneName);
}

FFI_PLUGIN_EXPORT spine_slot spine_skeleton_find_slot(spine_skeleton skeleton, const char* slotName) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return (spine_slot)_skeleton->findSlot(slotName);
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
    return (spine_attachment)_skeleton->getAttachment(slotName, attachmentName);
}

FFI_PLUGIN_EXPORT spine_attachment spine_skeleton_get_attachment(spine_skeleton skeleton, int slotIndex, const char* attachmentName) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return (spine_attachment)_skeleton->getAttachment(slotIndex, attachmentName);
}

FFI_PLUGIN_EXPORT void spine_skeleton_set_attachment(spine_skeleton skeleton, const char* slotName, const char* attachmentName) {
    if (skeleton == nullptr) return;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return _skeleton->setAttachment(slotName, attachmentName);
}

FFI_PLUGIN_EXPORT spine_ik_constraint spine_skeleton_find_ik_constraint(spine_skeleton skeleton, const char* constraintName) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return (spine_ik_constraint)_skeleton->findIkConstraint(constraintName);
}

FFI_PLUGIN_EXPORT spine_transform_constraint spine_skeleton_find_transform_constraint(spine_skeleton skeleton, const char* constraintName) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return (spine_transform_constraint)_skeleton->findTransformConstraint(constraintName);
}

FFI_PLUGIN_EXPORT spine_path_constraint spine_skeleton_find_path_constraint(spine_skeleton skeleton, const char* constraintName) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return (spine_path_constraint)_skeleton->findPathConstraint(constraintName);
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
    return (spine_bone)_skeleton->getRootBone();
}

FFI_PLUGIN_EXPORT spine_skeleton_data spine_skeleton_get_data(spine_skeleton skeleton) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return (spine_skeleton_data)_skeleton->getData();
}

FFI_PLUGIN_EXPORT int spine_skeleton_get_num_bones(spine_skeleton skeleton) {
    if (skeleton == nullptr) return 0;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return (int)_skeleton->getBones().size();
}

FFI_PLUGIN_EXPORT spine_bone* spine_skeleton_get_bones(spine_skeleton skeleton) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return (spine_bone*)_skeleton->getBones().buffer();
}

FFI_PLUGIN_EXPORT int spine_skeleton_get_num_slots(spine_skeleton skeleton) {
    if (skeleton == nullptr) return 0;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return (int)_skeleton->getSlots().size();
}

FFI_PLUGIN_EXPORT spine_slot* spine_skeleton_get_slots(spine_skeleton skeleton) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return (spine_slot*)_skeleton->getSlots().buffer();
}

FFI_PLUGIN_EXPORT int spine_skeleton_get_num_draw_order(spine_skeleton skeleton) {
    if (skeleton == nullptr) return 0;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return (int)_skeleton->getDrawOrder().size();
}

FFI_PLUGIN_EXPORT spine_slot* spine_skeleton_get_draw_order(spine_skeleton skeleton) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return (spine_slot*)_skeleton->getDrawOrder().buffer();
}

FFI_PLUGIN_EXPORT int spine_skeleton_get_num_ik_constraints(spine_skeleton skeleton) {
    if (skeleton == nullptr) return 0;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return (int)_skeleton->getIkConstraints().size();
}

FFI_PLUGIN_EXPORT spine_ik_constraint* spine_skeleton_get_ik_constraints(spine_skeleton skeleton) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return (spine_ik_constraint*)_skeleton->getIkConstraints().buffer();
}

FFI_PLUGIN_EXPORT int spine_skeleton_get_num_transform_constraints(spine_skeleton skeleton) {
    if (skeleton == nullptr) return 0;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return (int)_skeleton->getTransformConstraints().size();
}

FFI_PLUGIN_EXPORT spine_transform_constraint* spine_skeleton_get_transform_constraints(spine_skeleton skeleton) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return (spine_transform_constraint*)_skeleton->getTransformConstraints().buffer();
}

FFI_PLUGIN_EXPORT int spine_skeleton_get_num_path_constraints(spine_skeleton skeleton) {
    if (skeleton == nullptr) return 0;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return (int)_skeleton->getPathConstraints().size();
}

FFI_PLUGIN_EXPORT spine_path_constraint* spine_skeleton_get_path_constraints(spine_skeleton skeleton) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return (spine_path_constraint*)_skeleton->getPathConstraints().buffer();
}

FFI_PLUGIN_EXPORT spine_skin spine_skeleton_get_skin(spine_skeleton skeleton) {
    if (skeleton == nullptr) return nullptr;
    Skeleton *_skeleton = (Skeleton*)skeleton;
    return (spine_skin)_skeleton->getSkin();
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

// EventData

FFI_PLUGIN_EXPORT const char* spine_event_data_get_name(spine_event_data event) {
    if (event == nullptr) return nullptr;
    EventData *_event = (EventData*)event;
    return _event->getName().buffer();
}

FFI_PLUGIN_EXPORT int spine_event_data_get_int_value(spine_event_data event) {
    if (event == nullptr) return 0;
    EventData *_event = (EventData*)event;
    return _event->getIntValue();
}

FFI_PLUGIN_EXPORT float spine_event_data_get_float_value(spine_event_data event) {
    if (event == nullptr) return 0;
    EventData *_event = (EventData*)event;
    return _event->getFloatValue();
}

FFI_PLUGIN_EXPORT const char* spine_event_data_get_string_value(spine_event_data event) {
    if (event == nullptr) return nullptr;
    EventData *_event = (EventData*)event;
    return _event->getStringValue().buffer();
}

FFI_PLUGIN_EXPORT const char* spine_event_data_get_audio_path(spine_event_data event) {
    if (event == nullptr) return nullptr;
    EventData *_event = (EventData*)event;
    return _event->getAudioPath().buffer();
}

FFI_PLUGIN_EXPORT float spine_event_data_get_volume(spine_event_data event) {
    if (event == nullptr) return 0;
    EventData *_event = (EventData*)event;
    return _event->getVolume();
}

FFI_PLUGIN_EXPORT float spine_event_data_get_balance(spine_event_data event) {
    if (event == nullptr) return 0;
    EventData *_event = (EventData*)event;
    return _event->getBalance();
}

// Event

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

// SlotData
FFI_PLUGIN_EXPORT int spine_slot_data_get_index(spine_slot_data slot) {
    if (slot == nullptr) return 0;
    SlotData *_slot = (SlotData*)slot;
    return _slot->getIndex();
}

FFI_PLUGIN_EXPORT const char* spine_slot_data_get_name(spine_slot_data slot) {
    if (slot == nullptr) return nullptr;
    SlotData *_slot = (SlotData*)slot;
    return _slot->getName().buffer();
}

FFI_PLUGIN_EXPORT spine_bone_data spine_slot_data_get_bone_data(spine_slot_data slot) {
    if (slot == nullptr) return nullptr;
    SlotData *_slot = (SlotData*)slot;
    return (spine_bone_data)&_slot->getBoneData();
}

FFI_PLUGIN_EXPORT spine_color spine_slot_data_get_color(spine_slot_data slot) {
    spine_color color = { 0, 0, 0, 0 };
    if (slot == nullptr) return color;
    SlotData *_slot = (SlotData*)slot;
    color = { _slot->getColor().r, _slot->getColor().g, _slot->getColor().b, _slot->getColor().a };
    return color;
}

FFI_PLUGIN_EXPORT void spine_slot_data_set_color(spine_slot_data slot, float r, float g, float b, float a) {
    if (slot == nullptr) return;
    SlotData *_slot = (SlotData*)slot;
    _slot->getColor().set(r, g, b, a);
}

FFI_PLUGIN_EXPORT spine_color spine_slot_data_get_dark_color(spine_slot_data slot) {
    spine_color color = { 0, 0, 0, 0 };
    if (slot == nullptr) return color;
    SlotData *_slot = (SlotData*)slot;
    color = { _slot->getDarkColor().r, _slot->getDarkColor().g, _slot->getDarkColor().b, _slot->getDarkColor().a };
    return color;
}

FFI_PLUGIN_EXPORT void spine_slot_data_set_dark_color(spine_slot_data slot, float r, float g, float b, float a) {
    if (slot == nullptr) return;
    SlotData *_slot = (SlotData*)slot;
    _slot->getDarkColor().set(r, g, b, a);
}

FFI_PLUGIN_EXPORT int spine_slot_data_has_dark_color(spine_slot_data slot) {
    if (slot == nullptr) return 0;
    SlotData *_slot = (SlotData*)slot;
    return _slot->hasDarkColor() ? -1 : 0;
}

FFI_PLUGIN_EXPORT void spine_slot_data_set_has_dark_color(spine_slot_data slot, int hasDarkColor) {
    if (slot == nullptr) return;
    SlotData *_slot = (SlotData*)slot;
    _slot->setHasDarkColor(hasDarkColor);
}

FFI_PLUGIN_EXPORT const char* spine_slot_data_get_attachment_name(spine_slot_data slot) {
    if (slot == nullptr) return nullptr;
    SlotData *_slot = (SlotData*)slot;
    return _slot->getAttachmentName().buffer();
}

FFI_PLUGIN_EXPORT void spine_slot_data_set_attachment_name(spine_slot_data slot, const char* attachmentName) {
    if (slot == nullptr) return;
    SlotData *_slot = (SlotData*)slot;
    _slot->setAttachmentName(attachmentName);
}

FFI_PLUGIN_EXPORT spine_blend_mode spine_slot_data_get_blend_mode(spine_slot_data slot) {
    if (slot == nullptr) return SPINE_BLEND_MODE_NORMAL;
    SlotData *_slot = (SlotData*)slot;
    return (spine_blend_mode)_slot->getBlendMode();
}

// Slot
FFI_PLUGIN_EXPORT void spine_slot_set_to_setup_pose(spine_slot slot) {
    if (slot == nullptr) return;
    Slot *_slot = (Slot*)slot;
    _slot->setToSetupPose();
}

FFI_PLUGIN_EXPORT spine_slot_data spine_slot_get_data(spine_slot slot) {
    if (slot == nullptr) return nullptr;
    Slot *_slot = (Slot*)slot;
    return (spine_slot_data)&_slot->getData();
}

FFI_PLUGIN_EXPORT spine_bone spine_slot_get_bone(spine_slot slot) {
    if (slot == nullptr) return nullptr;
    Slot *_slot = (Slot*)slot;
    return (spine_bone)&_slot->getBone();
}

FFI_PLUGIN_EXPORT spine_skeleton spine_slot_get_skeleton(spine_slot slot) {
    if (slot == nullptr) return nullptr;
    Slot *_slot = (Slot*)slot;
    return (spine_skeleton)&_slot->getSkeleton();
}

FFI_PLUGIN_EXPORT spine_color spine_slot_get_color(spine_slot slot) {
    spine_color color = { 0, 0, 0, 0 };
    if (slot == nullptr) return color;
    Slot *_slot = (Slot*)slot;
    color = { _slot->getColor().r, _slot->getColor().g, _slot->getColor().b, _slot->getColor().a };
    return color;
}

FFI_PLUGIN_EXPORT void spine_slot_set_color(spine_slot slot, float r, float g, float b, float a) {
    if (slot == nullptr) return;
    Slot *_slot = (Slot*)slot;
    _slot->getColor().set(r, g, b, a);
}

FFI_PLUGIN_EXPORT spine_color spine_slot_get_dark_color(spine_slot slot) {
    spine_color color = { 0, 0, 0, 0 };
    if (slot == nullptr) return color;
    Slot *_slot = (Slot*)slot;
    color = { _slot->getDarkColor().r, _slot->getDarkColor().g, _slot->getDarkColor().b, _slot->getDarkColor().a };
    return color;
}

FFI_PLUGIN_EXPORT void spine_slot_set_dark_color(spine_slot slot, float r, float g, float b, float a) {
    if (slot == nullptr) return;
    Slot *_slot = (Slot*)slot;
    _slot->getDarkColor().set(r, g, b, a);
}

FFI_PLUGIN_EXPORT int spine_slot_has_dark_color(spine_slot slot) {
    if (slot == nullptr) return 0;
    Slot *_slot = (Slot*)slot;
    return _slot->hasDarkColor() ? -1 : 0;
}

FFI_PLUGIN_EXPORT spine_attachment spine_slot_get_attachment(spine_slot slot) {
    if (slot == nullptr) return nullptr;
    Slot *_slot = (Slot*)slot;
    return (spine_attachment)_slot->getAttachment();
}

FFI_PLUGIN_EXPORT void spine_slot_set_attachment(spine_slot slot, spine_attachment attachment) {
    if (slot == nullptr) return;
    Slot *_slot = (Slot*)slot;
    _slot->setAttachment((Attachment*)attachment);
}

// BoneData
FFI_PLUGIN_EXPORT int spine_bone_data_get_index(spine_bone_data data) {
    if (data == nullptr) return 0;
    BoneData *_data = (BoneData*)data;
    return _data->getIndex();
}

FFI_PLUGIN_EXPORT const char* spine_bone_data_get_name(spine_bone_data data) {
    if (data == nullptr) return nullptr;
    BoneData *_data = (BoneData*)data;
    return _data->getName().buffer();
}

FFI_PLUGIN_EXPORT spine_bone_data spine_bone_data_get_parent(spine_bone_data data) {
    if (data == nullptr) return nullptr;
    BoneData *_data = (BoneData*)data;
    return (spine_bone_data)_data->getParent();
}

FFI_PLUGIN_EXPORT float spine_bone_data_get_length(spine_bone_data data) {
    if (data == nullptr) return 0;
    BoneData *_data = (BoneData*)data;
    return _data->getLength();
}

FFI_PLUGIN_EXPORT void spine_bone_data_set_length(spine_bone_data data, float length) {
    if (data == nullptr) return;
    BoneData *_data = (BoneData*)data;
    _data->setLength(length);
}

FFI_PLUGIN_EXPORT float spine_bone_data_get_x(spine_bone_data data) {
    if (data == nullptr) return 0;
    BoneData *_data = (BoneData*)data;
    return _data->getX();
}

FFI_PLUGIN_EXPORT void spine_bone_data_set_x(spine_bone_data data, float x) {
    if (data == nullptr) return;
    BoneData *_data = (BoneData*)data;
    _data->setX(x);
}

FFI_PLUGIN_EXPORT float spine_bone_data_get_y(spine_bone_data data) {
    if (data == nullptr) return 0;
    BoneData *_data = (BoneData*)data;
    return _data->getY();
}

FFI_PLUGIN_EXPORT void spine_bone_data_set_y(spine_bone_data data, float y) {
    if (data == nullptr) return;
    BoneData *_data = (BoneData*)data;
    _data->setY(y);
}

FFI_PLUGIN_EXPORT float spine_bone_data_get_rotation(spine_bone_data data) {
    if (data == nullptr) return 0;
    BoneData *_data = (BoneData*)data;
    return _data->getRotation();
}

FFI_PLUGIN_EXPORT void spine_bone_data_set_rotation(spine_bone_data data, float rotation) {
    if (data == nullptr) return;
    BoneData *_data = (BoneData*)data;
    _data->setRotation(rotation);
}

FFI_PLUGIN_EXPORT float spine_bone_data_get_scale_x(spine_bone_data data) {
    if (data == nullptr) return 0;
    BoneData *_data = (BoneData*)data;
    return _data->getScaleX();
}

FFI_PLUGIN_EXPORT void spine_bone_data_set_scale_x(spine_bone_data data, float scaleX) {
    if (data == nullptr) return;
    BoneData *_data = (BoneData*)data;
    _data->setScaleX(scaleX);
}

FFI_PLUGIN_EXPORT float spine_bone_data_get_scale_y(spine_bone_data data) {
    if (data == nullptr) return 0;
    BoneData *_data = (BoneData*)data;
    return _data->getScaleY();
}

FFI_PLUGIN_EXPORT void spine_bone_data_set_scale_y(spine_bone_data data, float scaleY) {
    if (data == nullptr) return;
    BoneData *_data = (BoneData*)data;
    _data->setScaleY(scaleY);
}

FFI_PLUGIN_EXPORT float spine_bone_data_get_shear_x(spine_bone_data data) {
    if (data == nullptr) return 0;
    BoneData *_data = (BoneData*)data;
    return _data->getShearX();
}

FFI_PLUGIN_EXPORT void spine_bone_data_set_shear_x(spine_bone_data data, float shearX) {
    if (data == nullptr) return;
    BoneData *_data = (BoneData*)data;
    _data->setShearX(shearX);
}

FFI_PLUGIN_EXPORT float spine_bone_data_get_shear_y(spine_bone_data data) {
    if (data == nullptr) return 0;
    BoneData *_data = (BoneData*)data;
    return _data->getShearY();
}

FFI_PLUGIN_EXPORT void spine_bone_data_set_shear_y(spine_bone_data data, float y) {
    if (data == nullptr) return;
    BoneData *_data = (BoneData*)data;
    _data->setShearY(y);
}

FFI_PLUGIN_EXPORT spine_transform_mode spine_bone_data_get_transform_mode(spine_bone_data data) {
    if (data == nullptr) return SPINE_TRANSFORM_MODE_NORMAL;
    BoneData *_data = (BoneData*)data;
    return (spine_transform_mode)_data->getTransformMode();
}

FFI_PLUGIN_EXPORT void spine_bone_data_set_transform_mode(spine_bone_data data, spine_transform_mode mode) {
    if (data == nullptr) return;
    BoneData *_data = (BoneData*)data;
    _data->setTransformMode((TransformMode)mode);
}

FFI_PLUGIN_EXPORT int spine_bone_data_is_skin_required(spine_bone_data data) {
    if (data == nullptr) return 0;
    BoneData *_data = (BoneData*)data;
    return _data->isSkinRequired() ? -1 : 0;
}

FFI_PLUGIN_EXPORT void spine_bone_data_set_is_skin_required(spine_bone_data data, int isSkinRequired) {
    if (data == nullptr) return;
    BoneData *_data = (BoneData*)data;
    _data->setSkinRequired(isSkinRequired);
}

FFI_PLUGIN_EXPORT spine_color spine_bone_data_get_color(spine_bone_data data) {
    spine_color color = { 0, 0, 0 , 0};
    if (data == nullptr) return color;
    BoneData *_data = (BoneData*)data;
    color = { _data->getColor().r, _data->getColor().g, _data->getColor().b, _data->getColor().a };
    return color;
}

FFI_PLUGIN_EXPORT void spine_bone_data_set_color(spine_bone_data data, float r, float g, float b, float a) {
    if (data == nullptr) return;
    BoneData *_data = (BoneData*)data;
    _data->getColor().set(r, g, b, a);
}

// Bone
FFI_PLUGIN_EXPORT void spine_bone_update(spine_bone bone) {
    if (bone == nullptr) return;
    Bone *_bone = (Bone*)bone;
    _bone->update();
}

FFI_PLUGIN_EXPORT void spine_bone_update_world_transform(spine_bone bone) {
    if (bone == nullptr) return;
    Bone *_bone = (Bone*)bone;
    _bone->updateWorldTransform();
}

FFI_PLUGIN_EXPORT void spine_bone_update_world_transform_with(spine_bone bone, float x, float y, float rotation, float scaleX, float scaleY, float shearX, float shearY) {
    if (bone == nullptr) return;
    Bone *_bone = (Bone*)bone;
    _bone->updateWorldTransform(x, y, rotation, scaleX, scaleY, shearX, shearY);
}

FFI_PLUGIN_EXPORT void spine_bone_set_to_setup_pose(spine_bone bone) {
    if (bone == nullptr) return;
    Bone *_bone = (Bone*)bone;
    _bone->setToSetupPose();
}

FFI_PLUGIN_EXPORT spine_vector spine_bone_world_to_local(spine_bone bone, float worldX, float worldY) {
    spine_vector coords = { 0, 0 };
    if (bone == nullptr) return coords;
    Bone *_bone = (Bone*)bone;
    _bone->worldToLocal(worldX, worldY, coords.x, coords.y);
    return coords;
}

FFI_PLUGIN_EXPORT spine_vector spine_bone_local_to_world(spine_bone bone, float localX, float localY) {
    spine_vector coords = { 0, 0 };
    if (bone == nullptr) return coords;
    Bone *_bone = (Bone*)bone;
    _bone->localToWorld(localX, localY, coords.x, coords.y);
    return coords;
}

FFI_PLUGIN_EXPORT float spine_bone_world_to_local_rotation(spine_bone bone, float worldRotation) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->worldToLocalRotation(worldRotation);
}

FFI_PLUGIN_EXPORT float spine_bone_local_to_world_rotation(spine_bone bone, float localRotation) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->localToWorldRotation(localRotation);
}

FFI_PLUGIN_EXPORT void spine_bone_rotate_world(spine_bone bone, float degrees) {
    if (bone == nullptr) return;
    Bone *_bone = (Bone*)bone;
    _bone->rotateWorld(degrees);
}

FFI_PLUGIN_EXPORT float spine_bone_get_world_to_local_rotation_x(spine_bone bone) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->getWorldToLocalRotationX();
}

FFI_PLUGIN_EXPORT float spine_bone_get_world_to_local_rotation_y(spine_bone bone) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->getWorldToLocalRotationY();
}

FFI_PLUGIN_EXPORT spine_bone_data spine_bone_get_data(spine_bone bone) {
    if (bone == nullptr) return nullptr;
    Bone *_bone = (Bone*)bone;
    return (spine_bone_data)&_bone->getData();
}

FFI_PLUGIN_EXPORT spine_skeleton spine_bone_get_skeleton(spine_bone bone) {
    if (bone == nullptr) return nullptr;
    Bone *_bone = (Bone*)bone;
    return (spine_skeleton)&_bone->getSkeleton();
}

FFI_PLUGIN_EXPORT spine_bone spine_bone_get_parent(spine_bone bone) {
    if (bone == nullptr) return nullptr;
    Bone *_bone = (Bone*)bone;
    return (spine_bone)_bone->getParent();
}

FFI_PLUGIN_EXPORT int spine_bone_get_num_children(spine_bone bone) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return (int)_bone->getChildren().size();
}

FFI_PLUGIN_EXPORT spine_bone* spine_bone_get_children(spine_bone bone) {
    if (bone == nullptr) return nullptr;
    Bone *_bone = (Bone*)bone;
    return (spine_bone*)_bone->getChildren().buffer();
}

FFI_PLUGIN_EXPORT float spine_bone_get_x(spine_bone bone) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->getX();
}

FFI_PLUGIN_EXPORT void spine_bone_set_x(spine_bone bone, float x) {
    if (bone == nullptr) return;
    Bone *_bone = (Bone*)bone;
    _bone->setX(x);
}

FFI_PLUGIN_EXPORT float spine_bone_get_y(spine_bone bone) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->getY();
}

FFI_PLUGIN_EXPORT void spine_bone_set_y(spine_bone bone, float y) {
    if (bone == nullptr) return;
    Bone *_bone = (Bone*)bone;
    _bone->setY(y);
}

FFI_PLUGIN_EXPORT float spine_bone_get_rotation(spine_bone bone) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->getRotation();
}

FFI_PLUGIN_EXPORT void spine_bone_set_rotation(spine_bone bone, float rotation) {
    if (bone == nullptr) return;
    Bone *_bone = (Bone*)bone;
    _bone->setRotation(rotation);
}

FFI_PLUGIN_EXPORT float spine_bone_get_scale_x(spine_bone bone) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->getScaleX();
}

FFI_PLUGIN_EXPORT void spine_bone_set_scale_x(spine_bone bone, float scaleX) {
    if (bone == nullptr) return;
    Bone *_bone = (Bone*)bone;
    _bone->setScaleX(scaleX);
}

FFI_PLUGIN_EXPORT float spine_bone_get_scale_y(spine_bone bone) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->getScaleY();
}

FFI_PLUGIN_EXPORT void spine_bone_set_scale_y(spine_bone bone, float scaleY) {
    if (bone == nullptr) return;
    Bone *_bone = (Bone*)bone;
    _bone->setScaleY(scaleY);
}

FFI_PLUGIN_EXPORT float spine_bone_get_shear_x(spine_bone bone) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->getShearX();
}

FFI_PLUGIN_EXPORT void spine_bone_set_shear_x(spine_bone bone, float shearX) {
    if (bone == nullptr) return;
    Bone *_bone = (Bone*)bone;
    _bone->setShearX(shearX);
}

FFI_PLUGIN_EXPORT float spine_bone_get_shear_y(spine_bone bone) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->getShearY();
}

FFI_PLUGIN_EXPORT void spine_bone_set_shear_y(spine_bone bone, float shearY) {
    if (bone == nullptr) return;
    Bone *_bone = (Bone*)bone;
    _bone->setShearY(shearY);
}

FFI_PLUGIN_EXPORT float spine_bone_get_applied_rotation(spine_bone bone) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->getAppliedRotation();
}

FFI_PLUGIN_EXPORT void spine_bone_set_applied_rotation(spine_bone bone, float rotation) {
    if (bone == nullptr) return;
    Bone *_bone = (Bone*)bone;
    _bone->setAppliedRotation(rotation);
}

FFI_PLUGIN_EXPORT float spine_bone_get_a_x(spine_bone bone) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->getAX();
}

FFI_PLUGIN_EXPORT void spine_bone_set_a_x(spine_bone bone, float x) {
    if (bone == nullptr) return;
    Bone *_bone = (Bone*)bone;
    _bone->setAX(x);
}

FFI_PLUGIN_EXPORT float spine_bone_get_a_y(spine_bone bone) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->getAY();
}

FFI_PLUGIN_EXPORT void spine_bone_set_a_y(spine_bone bone, float y) {
    if (bone == nullptr) return;
    Bone *_bone = (Bone*)bone;
    _bone->setAY(y);
}

FFI_PLUGIN_EXPORT float spine_bone_get_a_scale_x(spine_bone bone) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->getAScaleX();
}

FFI_PLUGIN_EXPORT void spine_bone_set_a_scale_x(spine_bone bone, float scaleX) {
    if (bone == nullptr) return;
    Bone *_bone = (Bone*)bone;
    _bone->setAScaleX(scaleX);
}

FFI_PLUGIN_EXPORT float spine_bone_get_a_scale_y(spine_bone bone) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->getAScaleY();
}

FFI_PLUGIN_EXPORT void spine_bone_set_a_scale_y(spine_bone bone, float scaleY) {
    if (bone == nullptr) return;
    Bone *_bone = (Bone*)bone;
    _bone->setAScaleY(scaleY);
}

FFI_PLUGIN_EXPORT float spine_bone_get_a_shear_x(spine_bone bone) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->getAShearX();
}

FFI_PLUGIN_EXPORT void spine_bone_set_a_shear_x(spine_bone bone, float shearX) {
    if (bone == nullptr) return;
    Bone *_bone = (Bone*)bone;
    _bone->setAShearX(shearX);
}

FFI_PLUGIN_EXPORT float spine_bone_get_a_shear_y(spine_bone bone) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->getAShearY();
}

FFI_PLUGIN_EXPORT void spine_bone_set_shear_a_y(spine_bone bone, float shearY) {
    if (bone == nullptr) return;
    Bone *_bone = (Bone*)bone;
    _bone->setAShearY(shearY);
}

FFI_PLUGIN_EXPORT float spine_bone_get_a(spine_bone bone) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->getA();
}

FFI_PLUGIN_EXPORT void spine_bone_set_a(spine_bone bone, float a) {
    if (bone == nullptr) return;
    Bone *_bone = (Bone*)bone;
    _bone->setA(a);
}

FFI_PLUGIN_EXPORT float spine_bone_get_b(spine_bone bone) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->getB();
}

FFI_PLUGIN_EXPORT void spine_bone_set_b(spine_bone bone, float b) {
    if (bone == nullptr) return;
    Bone *_bone = (Bone*)bone;
    _bone->setB(b);
}

FFI_PLUGIN_EXPORT float spine_bone_get_c(spine_bone bone) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->getC();
}

FFI_PLUGIN_EXPORT void spine_bone_set_c(spine_bone bone, float c) {
    if (bone == nullptr) return;
    Bone *_bone = (Bone*)bone;
    _bone->setC(c);
}

FFI_PLUGIN_EXPORT float spine_bone_get_d(spine_bone bone) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->getD();
}

FFI_PLUGIN_EXPORT void spine_bone_set_d(spine_bone bone, float d) {
    if (bone == nullptr) return;
    Bone *_bone = (Bone*)bone;
    _bone->setD(d);
}

FFI_PLUGIN_EXPORT float spine_bone_get_world_x(spine_bone bone) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->getWorldX();
}

FFI_PLUGIN_EXPORT void spine_bone_set_world_x(spine_bone bone, float worldX) {
    if (bone == nullptr) return;
    Bone *_bone = (Bone*)bone;
    _bone->setWorldX(worldX);
}

FFI_PLUGIN_EXPORT float spine_bone_get_world_y(spine_bone bone) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->getWorldY();
}

FFI_PLUGIN_EXPORT void spine_bone_set_world_y(spine_bone bone, float worldY) {
    if (bone == nullptr) return;
    Bone *_bone = (Bone*)bone;
    _bone->setWorldY(worldY);
}

FFI_PLUGIN_EXPORT float spine_bone_get_world_rotation_x(spine_bone bone) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->getWorldRotationX();
}

FFI_PLUGIN_EXPORT float spine_bone_get_world_rotation_y(spine_bone bone) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->getWorldToLocalRotationY();
}

FFI_PLUGIN_EXPORT float spine_bone_get_world_scale_x(spine_bone bone) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->getWorldScaleX();
}

FFI_PLUGIN_EXPORT float spine_bone_get_world_scale_y(spine_bone bone) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->getWorldScaleY();
}

FFI_PLUGIN_EXPORT int spine_bone_get_is_active(spine_bone bone) {
    if (bone == nullptr) return 0;
    Bone *_bone = (Bone*)bone;
    return _bone->isActive() ? -1 : 0;
}

FFI_PLUGIN_EXPORT void spine_bone_set_is_active(spine_bone bone, int isActive) {
    if (bone == nullptr) return;
    Bone *_bone = (Bone*)bone;
    _bone->setActive(isActive);
}

// Attachment
FFI_PLUGIN_EXPORT const char* spine_attachment_get_name(spine_attachment attachment) {
    if (attachment == nullptr) return nullptr;
    Attachment *_attachment = (Attachment*)attachment;
    return _attachment->getName().buffer();
}

FFI_PLUGIN_EXPORT spine_attachment_type spine_attachment_get_type(spine_attachment attachment) {
    if (attachment == nullptr) return SPINE_ATTACHMENT_REGION;
    Attachment *_attachment = (Attachment*)attachment;
    if (_attachment->getRTTI().isExactly(RegionAttachment::rtti)) {
        return SPINE_ATTACHMENT_REGION;
    } else if (_attachment->getRTTI().isExactly(MeshAttachment::rtti)) {
        return SPINE_ATTACHMENT_MESH;
    } else if (_attachment->getRTTI().isExactly(ClippingAttachment::rtti)) {
        return SPINE_ATTACHMENT_CLIPPING;
    } else if (_attachment->getRTTI().isExactly(BoundingBoxAttachment::rtti)) {
        return SPINE_ATTACHMENT_BOUNDING_BOX;
    } else if (_attachment->getRTTI().isExactly(PathAttachment::rtti)) {
        return SPINE_ATTACHMENT_PATH;
    } else if (_attachment->getRTTI().isExactly(PointAttachment::rtti)) {
        return SPINE_ATTACHMENT_POINT;
    } else {
        return SPINE_ATTACHMENT_REGION;
    }
}

FFI_PLUGIN_EXPORT spine_attachment spine_attachment_copy(spine_attachment attachment) {
    if (attachment == nullptr) return nullptr;
    Attachment *_attachment = (Attachment*)attachment;
    return (spine_attachment)_attachment->copy();
}

FFI_PLUGIN_EXPORT void spine_attachment_dispose(spine_attachment attachment) {
    if (attachment == nullptr) return;
    Attachment *_attachment = (Attachment*)attachment;
    delete _attachment;
}

// PointAttachment
FFI_PLUGIN_EXPORT spine_vector spine_point_attachment_compute_world_position(spine_point_attachment attachment, spine_bone bone) {
    spine_vector result = { 0, 0 };
    if (attachment == nullptr) return result;
    PointAttachment *_attachment = (PointAttachment*)attachment;
    _attachment->computeWorldPosition(*(Bone*)bone, result.x, result.y);
    return result;
}

FFI_PLUGIN_EXPORT float spine_point_attachment_compute_world_rotation(spine_point_attachment attachment, spine_bone bone) {
    if (attachment == nullptr) return 0;
    PointAttachment *_attachment = (PointAttachment*)attachment;
    return _attachment->computeWorldRotation(*(Bone*)bone);
}

FFI_PLUGIN_EXPORT float spine_point_attachment_get_x(spine_point_attachment attachment) {
    if (attachment == nullptr) return 0;
    PointAttachment *_attachment = (PointAttachment*)attachment;
    return _attachment->getX();
}

FFI_PLUGIN_EXPORT void spine_point_attachment_set_x(spine_point_attachment attachment, float x) {
    if (attachment == nullptr) return;
    PointAttachment *_attachment = (PointAttachment*)attachment;
    _attachment->setX(x);
}

FFI_PLUGIN_EXPORT float spine_point_attachment_get_y(spine_point_attachment attachment) {
    if (attachment == nullptr) return 0;
    PointAttachment *_attachment = (PointAttachment*)attachment;
    return _attachment->getY();
}

FFI_PLUGIN_EXPORT void spine_point_attachment_set_y(spine_point_attachment attachment, float y) {
    if (attachment == nullptr) return;
    PointAttachment *_attachment = (PointAttachment*)attachment;
    _attachment->setY(y);
}

FFI_PLUGIN_EXPORT float spine_point_attachment_get_rotation(spine_point_attachment attachment) {
    if (attachment == nullptr) return 0;
    PointAttachment *_attachment = (PointAttachment*)attachment;
    return _attachment->getRotation();
}

FFI_PLUGIN_EXPORT void spine_point_attachment_set_rotation(spine_point_attachment attachment, float rotation) {
    if (attachment == nullptr) return;
    PointAttachment *_attachment = (PointAttachment*)attachment;
    _attachment->setRotation(rotation);
}

FFI_PLUGIN_EXPORT spine_color spine_point_attachment_get_color(spine_point_attachment attachment) {
    spine_color result = { 0, 0, 0, 0 };
    if (attachment == nullptr) return result;
    PointAttachment *_attachment = (PointAttachment*)attachment;
    Color &color = _attachment->getColor();
    result = { color.r, color.g, color.b, color.a };
    return result;
}

FFI_PLUGIN_EXPORT void spine_point_attachment_set_color(spine_point_attachment attachment, float r, float g, float b, float a) {
    if (attachment == nullptr) return;
    PointAttachment *_attachment = (PointAttachment*)attachment;
    _attachment->getColor().set(r, g, b, a);
}

// RegionAttachment
FFI_PLUGIN_EXPORT void spine_region_attachment_update_region(spine_region_attachment attachment) {
    if (attachment == nullptr) return;
    RegionAttachment *_attachment = (RegionAttachment*)attachment;
    _attachment->updateRegion();
}

FFI_PLUGIN_EXPORT void spine_region_attachment_compute_world_vertices(spine_region_attachment attachment, spine_slot slot, float *worldVertices) {
    if (attachment == nullptr) return;
    RegionAttachment *_attachment = (RegionAttachment*)attachment;
    _attachment->computeWorldVertices(*(Slot*)slot, worldVertices, 0);
}

FFI_PLUGIN_EXPORT float spine_region_attachment_get_x(spine_region_attachment attachment) {
    if (attachment == nullptr) return 0;
    RegionAttachment *_attachment = (RegionAttachment*)attachment;
    return _attachment->getX();
}

FFI_PLUGIN_EXPORT void spine_region_attachment_set_x(spine_region_attachment attachment, float x) {
    if (attachment == nullptr) return;
    RegionAttachment *_attachment = (RegionAttachment*)attachment;
    _attachment->setX(x);
}

FFI_PLUGIN_EXPORT float spine_region_attachment_get_y(spine_region_attachment attachment) {
    if (attachment == nullptr) return 0;
    RegionAttachment *_attachment = (RegionAttachment*)attachment;
    return _attachment->getY();
}

FFI_PLUGIN_EXPORT void spine_region_attachment_set_y(spine_region_attachment attachment, float y) {
    if (attachment == nullptr) return;
    RegionAttachment *_attachment = (RegionAttachment*)attachment;
    _attachment->setY(y);
}

FFI_PLUGIN_EXPORT float spine_region_attachment_get_rotation(spine_region_attachment attachment) {
    if (attachment == nullptr) return 0;
    RegionAttachment *_attachment = (RegionAttachment*)attachment;
    return _attachment->getRotation();
}

FFI_PLUGIN_EXPORT void spine_region_attachment_set_rotation(spine_region_attachment attachment, float rotation) {
    if (attachment == nullptr) return;
    RegionAttachment *_attachment = (RegionAttachment*)attachment;
    _attachment->setRotation(rotation);
}

FFI_PLUGIN_EXPORT float spine_region_attachment_get_scale_x(spine_region_attachment attachment) {
    if (attachment == nullptr) return 0;
    RegionAttachment *_attachment = (RegionAttachment*)attachment;
    return _attachment->getScaleX();
}

FFI_PLUGIN_EXPORT void spine_region_attachment_set_scale_x(spine_region_attachment attachment, float scaleX) {
    if (attachment == nullptr) return;
    RegionAttachment *_attachment = (RegionAttachment*)attachment;
    _attachment->setScaleX(scaleX);
}

FFI_PLUGIN_EXPORT float spine_region_attachment_get_scale_y(spine_region_attachment attachment) {
    if (attachment == nullptr) return 0;
    RegionAttachment *_attachment = (RegionAttachment*)attachment;
    return _attachment->getScaleY();
}

FFI_PLUGIN_EXPORT void spine_region_attachment_set_scale_y(spine_region_attachment attachment, float scaleY) {
    if (attachment == nullptr) return;
    RegionAttachment *_attachment = (RegionAttachment*)attachment;
    _attachment->setScaleY(scaleY);
}

FFI_PLUGIN_EXPORT float spine_region_attachment_get_width(spine_region_attachment attachment) {
    if (attachment == nullptr) return 0;
    RegionAttachment *_attachment = (RegionAttachment*)attachment;
    return _attachment->getWidth();
}

FFI_PLUGIN_EXPORT void spine_region_attachment_set_width(spine_region_attachment attachment, float width) {
    if (attachment == nullptr) return;
    RegionAttachment *_attachment = (RegionAttachment*)attachment;
    _attachment->setWidth(width);
}

FFI_PLUGIN_EXPORT float spine_region_attachment_get_height(spine_region_attachment attachment) {
    if (attachment == nullptr) return 0;
    RegionAttachment *_attachment = (RegionAttachment*)attachment;
    return _attachment->getHeight();
}

FFI_PLUGIN_EXPORT void spine_region_attachment_set_height(spine_region_attachment attachment, float height) {
    if (attachment == nullptr) return;
    RegionAttachment *_attachment = (RegionAttachment*)attachment;
    _attachment->setHeight(height);
}

FFI_PLUGIN_EXPORT spine_color spine_region_attachment_get_color(spine_region_attachment attachment) {
    spine_color result = { 0, 0, 0, 0 };
    if (attachment == nullptr) return result;
    RegionAttachment *_attachment = (RegionAttachment*)attachment;
    Color &color = _attachment->getColor();
    result = { color.r, color.g, color.b, color.a };
    return result;
}

FFI_PLUGIN_EXPORT void spine_region_attachment_set_color(spine_region_attachment attachment, float r, float g, float b, float a) {
    if (attachment == nullptr) return;
    RegionAttachment *_attachment = (RegionAttachment*)attachment;
    _attachment->getColor().set(r, g, b, a);
}

FFI_PLUGIN_EXPORT const char *spine_region_attachment_get_path(spine_region_attachment attachment) {
    if (attachment == nullptr) return nullptr;
    RegionAttachment *_attachment = (RegionAttachment*)attachment;
    return _attachment->getPath().buffer();
}

FFI_PLUGIN_EXPORT spine_texture_region spine_region_attachment_get_region(spine_region_attachment attachment) {
    if (attachment == nullptr) return nullptr;
    RegionAttachment *_attachment = (RegionAttachment*)attachment;
    return (spine_texture_region)_attachment->getRegion();
}

FFI_PLUGIN_EXPORT spine_sequence spine_region_attachment_get_sequence(spine_region_attachment attachment) {
    if (attachment == nullptr) return nullptr;
    RegionAttachment *_attachment = (RegionAttachment*)attachment;
    return (spine_sequence)_attachment->getSequence();
}

FFI_PLUGIN_EXPORT int spine_region_attachment_get_num_offset(spine_region_attachment attachment) {
    if (attachment == nullptr) return 0;
    RegionAttachment *_attachment = (RegionAttachment*)attachment;
    return (int)_attachment->getOffset().size();
}

FFI_PLUGIN_EXPORT float *spine_region_attachment_get_offset(spine_region_attachment attachment) {
    if (attachment == nullptr) return nullptr;
    RegionAttachment *_attachment = (RegionAttachment*)attachment;
    return _attachment->getOffset().buffer();
}

FFI_PLUGIN_EXPORT int spine_region_attachment_get_num_uvs(spine_region_attachment attachment) {
    if (attachment == nullptr) return 0;
    RegionAttachment *_attachment = (RegionAttachment*)attachment;
    return (int)_attachment->getUVs().size();
}

FFI_PLUGIN_EXPORT float *spine_region_attachment_get_uvs(spine_region_attachment attachment) {
    if (attachment == nullptr) return nullptr;
    RegionAttachment *_attachment = (RegionAttachment*)attachment;
    return _attachment->getUVs().buffer();
}

// VertexAttachment
FFI_PLUGIN_EXPORT int spine_vertex_attachment_get_world_vertices_length(spine_vertex_attachment attachment) {
    if (attachment == nullptr) return 0;
    VertexAttachment *_attachment = (VertexAttachment*)attachment;
    return (int)_attachment->getWorldVerticesLength();
}

FFI_PLUGIN_EXPORT void spine_vertex_attachment_compute_world_vertices(spine_vertex_attachment attachment, spine_slot slot, float *worldVertices) {
    if (attachment == nullptr) return;
    VertexAttachment *_attachment = (VertexAttachment*)attachment;
    _attachment->computeWorldVertices(*(Slot*)slot, worldVertices);
}

FFI_PLUGIN_EXPORT int spine_vertex_attachment_get_num_bones(spine_vertex_attachment attachment) {
    if (attachment == nullptr) return 0;
    VertexAttachment *_attachment = (VertexAttachment*)attachment;
    return (int)_attachment->getBones().size();
}

FFI_PLUGIN_EXPORT int *spine_region_attachment_get_bones(spine_region_attachment attachment) {
    if (attachment == nullptr) return nullptr;
    VertexAttachment *_attachment = (VertexAttachment*)attachment;
    return _attachment->getBones().buffer();
}

// VertexAttachment
FFI_PLUGIN_EXPORT int spine_vertex_attachment_get_num_vertices(spine_vertex_attachment attachment) {
    if (attachment == nullptr) return 0;
    VertexAttachment *_attachment = (VertexAttachment*)attachment;
    return (int)_attachment->getVertices().size();
}

FFI_PLUGIN_EXPORT float *spine_region_attachment_get_vertices(spine_region_attachment attachment) {
    if (attachment == nullptr) return nullptr;
    VertexAttachment *_attachment = (VertexAttachment*)attachment;
    return _attachment->getVertices().buffer();
}

FFI_PLUGIN_EXPORT spine_attachment spine_vertex_attachment_get_timeline_attachment(spine_vertex_attachment attachment) {
    if (attachment == nullptr) return nullptr;
    VertexAttachment *_attachment = (VertexAttachment*)attachment;
    return (spine_attachment)_attachment->getTimelineAttachment();
}

FFI_PLUGIN_EXPORT void spine_vertex_attachment_set_timeline_attachment(spine_vertex_attachment attachment, spine_attachment timelineAttachment) {
    if (attachment == nullptr) return;
    VertexAttachment *_attachment = (VertexAttachment*)attachment;
    _attachment->setTimelineAttachment((Attachment*)timelineAttachment);
}

// MeshAttachment
FFI_PLUGIN_EXPORT void spine_mesh_attachment_update_region(spine_mesh_attachment attachment) {
    if (attachment == nullptr) return;
    MeshAttachment *_attachment = (MeshAttachment*)attachment;
    _attachment->updateRegion();
}

FFI_PLUGIN_EXPORT int spine_mesh_attachment_get_hull_length(spine_mesh_attachment attachment) {
    if (attachment == nullptr) return 0;
    MeshAttachment *_attachment = (MeshAttachment*)attachment;
    return _attachment->getHullLength();
}

FFI_PLUGIN_EXPORT void spine_mesh_attachment_set_hull_length(spine_mesh_attachment attachment, int hullLength) {
    if (attachment == nullptr) return;
    MeshAttachment *_attachment = (MeshAttachment*)attachment;
    _attachment->setHullLength(hullLength);
}

FFI_PLUGIN_EXPORT int spine_mesh_attachment_get_num_region_uvs(spine_mesh_attachment attachment) {
    if (attachment == nullptr) return 0;
    MeshAttachment *_attachment = (MeshAttachment*)attachment;
    return _attachment->getRegionUVs().size();
}

FFI_PLUGIN_EXPORT float *spine_mesh_attachment_get_region_uvs(spine_mesh_attachment attachment) {
    if (attachment == nullptr) return nullptr;
    MeshAttachment *_attachment = (MeshAttachment*)attachment;
    return _attachment->getRegionUVs().buffer();
}

FFI_PLUGIN_EXPORT int spine_mesh_attachment_get_num_uvs(spine_mesh_attachment attachment) {
    if (attachment == nullptr) return 0;
    MeshAttachment *_attachment = (MeshAttachment*)attachment;
    return _attachment->getUVs().size();
}

FFI_PLUGIN_EXPORT float *spine_mesh_attachment_get_uvs(spine_mesh_attachment attachment) {
    if (attachment == nullptr) return 0;
    MeshAttachment *_attachment = (MeshAttachment*)attachment;
    return _attachment->getUVs().buffer();
}

FFI_PLUGIN_EXPORT int spine_mesh_attachment_get_num_triangles(spine_mesh_attachment attachment) {
    if (attachment == nullptr) return 0;
    MeshAttachment *_attachment = (MeshAttachment*)attachment;
    return _attachment->getTriangles().size();
}

FFI_PLUGIN_EXPORT unsigned short *spine_mesh_attachment_get_triangles(spine_mesh_attachment attachment) {
    if (attachment == nullptr) return nullptr;
    MeshAttachment *_attachment = (MeshAttachment*)attachment;
    return _attachment->getTriangles().buffer();
}

FFI_PLUGIN_EXPORT spine_color spine_mesh_attachment_get_color(spine_mesh_attachment attachment) {
    spine_color result = { 0, 0, 0, 0 };
    if (attachment == nullptr) return result;
    MeshAttachment *_attachment = (MeshAttachment*)attachment;
    Color &color = _attachment->getColor();
    result = { color.r, color.g, color.b, color.a };
    return result;
}

FFI_PLUGIN_EXPORT void spine_mesh_attachment_set_color(spine_mesh_attachment attachment, float r, float g, float b, float a) {
    if (attachment == nullptr) return;
    MeshAttachment *_attachment = (MeshAttachment*)attachment;
    _attachment->getColor().set(r, g, b, a);
}

FFI_PLUGIN_EXPORT const char *spine_mesh_attachment_get_path(spine_mesh_attachment attachment) {
    if (attachment == nullptr) return nullptr;
    MeshAttachment *_attachment = (MeshAttachment*)attachment;
    return _attachment->getPath().buffer();
}

FFI_PLUGIN_EXPORT spine_texture_region spine_mesh_attachment_get_region(spine_mesh_attachment attachment) {
    if (attachment == nullptr) return nullptr;
    MeshAttachment *_attachment = (MeshAttachment*)attachment;
    return (spine_texture_region)_attachment->getRegion();
}

FFI_PLUGIN_EXPORT spine_sequence spine_mesh_attachment_get_sequence(spine_mesh_attachment attachment) {
    if (attachment == nullptr) return nullptr;
    MeshAttachment *_attachment = (MeshAttachment*)attachment;
    return (spine_sequence)_attachment->getSequence();
}

FFI_PLUGIN_EXPORT spine_mesh_attachment spine_mesh_attachment_get_parent_mesh(spine_mesh_attachment attachment) {
    if (attachment == nullptr) return nullptr;
    MeshAttachment *_attachment = (MeshAttachment*)attachment;
    return (spine_mesh_attachment)_attachment->getParentMesh();
}

FFI_PLUGIN_EXPORT void spine_mesh_attachment_set_parent_mesh(spine_mesh_attachment attachment, spine_mesh_attachment parentMesh) {
    if (attachment == nullptr) return;
    MeshAttachment *_attachment = (MeshAttachment*)attachment;
    _attachment->setParentMesh((MeshAttachment*)parentMesh);
}

FFI_PLUGIN_EXPORT int spine_mesh_attachment_get_num_edges(spine_mesh_attachment attachment) {
    if (attachment == nullptr) return 0;
    MeshAttachment *_attachment = (MeshAttachment*)attachment;
    return _attachment->getEdges().size();
}

FFI_PLUGIN_EXPORT unsigned short *spine_mesh_attachment_get_edges(spine_mesh_attachment attachment) {
    if (attachment == nullptr) return nullptr;
    MeshAttachment *_attachment = (MeshAttachment*)attachment;
    return _attachment->getEdges().buffer();
}

FFI_PLUGIN_EXPORT float spine_mesh_attachment_get_width(spine_mesh_attachment attachment) {
    if (attachment == nullptr) return 0;
    MeshAttachment *_attachment = (MeshAttachment*)attachment;
    return _attachment->getWidth();
}

FFI_PLUGIN_EXPORT void spine_mesh_attachment_set_width(spine_mesh_attachment attachment, float width) {
    if (attachment == nullptr) return;
    MeshAttachment *_attachment = (MeshAttachment*)attachment;
    _attachment->setWidth(width);
}

FFI_PLUGIN_EXPORT float spine_mesh_attachment_get_height(spine_mesh_attachment attachment) {
    if (attachment == nullptr) return 0;
    MeshAttachment *_attachment = (MeshAttachment*)attachment;
    return _attachment->getHeight();
}

FFI_PLUGIN_EXPORT void spine_mesh_attachment_set_height(spine_mesh_attachment attachment, float height) {
    if (attachment == nullptr) return;
    MeshAttachment *_attachment = (MeshAttachment*)attachment;
    _attachment->setHeight(height);
}

// ClippingAttachment
FFI_PLUGIN_EXPORT spine_slot_data spine_clipping_attachment_get_end_slot(spine_clipping_attachment attachment) {
    if (attachment == nullptr) return nullptr;
    ClippingAttachment *_attachment = (ClippingAttachment*)attachment;
    return (spine_slot_data)_attachment->getEndSlot();
}

FFI_PLUGIN_EXPORT void spine_clipping_attachment_set_end_slot(spine_clipping_attachment attachment, spine_slot_data endSlot) {
    if (attachment == nullptr) return;
    ClippingAttachment *_attachment = (ClippingAttachment*)attachment;
    _attachment->setEndSlot((SlotData*)endSlot);
}

FFI_PLUGIN_EXPORT spine_color spine_clipping_attachment_get_color(spine_clipping_attachment attachment) {
    spine_color result = { 0, 0, 0, 0 };
    if (attachment == nullptr) return result;
    ClippingAttachment *_attachment = (ClippingAttachment*)attachment;
    Color &color = _attachment->getColor();
    result = { color.r, color.g, color.b, color.a };
    return result;
}

FFI_PLUGIN_EXPORT void spine_clipping_attachment_set_color(spine_clipping_attachment attachment, float r, float g, float b, float a) {
    if (attachment == nullptr) return;
    ClippingAttachment *_attachment = (ClippingAttachment*)attachment;
    _attachment->getColor().set(r, g, b, a);
}

// BoundingBoxAttachment
FFI_PLUGIN_EXPORT spine_color spine_bounding_box_attachment_get_color(spine_bounding_box_attachment attachment) {
    spine_color result = { 0, 0, 0, 0 };
    if (attachment == nullptr) return result;
    BoundingBoxAttachment *_attachment = (BoundingBoxAttachment*)attachment;
    Color &color = _attachment->getColor();
    result = { color.r, color.g, color.b, color.a };
    return result;
}

FFI_PLUGIN_EXPORT void spine_bounding_box_attachment_set_color(spine_bounding_box_attachment attachment, float r, float g, float b, float a) {
    if (attachment == nullptr) return;
    BoundingBoxAttachment *_attachment = (BoundingBoxAttachment*)attachment;
    _attachment->getColor().set(r, g, b, a);
}

// PathAttachment
FFI_PLUGIN_EXPORT int spine_path_attachment_get_num_lengths(spine_path_attachment attachment) {
    if (attachment == nullptr) return 0;
    PathAttachment *_attachment = (PathAttachment*)attachment;
    return _attachment->getLengths().size();
}

FFI_PLUGIN_EXPORT float *spine_path_attachment_get_lengths(spine_path_attachment attachment) {
    if (attachment == nullptr) return nullptr;
    PathAttachment *_attachment = (PathAttachment*)attachment;
    return _attachment->getLengths().buffer();
}

FFI_PLUGIN_EXPORT int spine_path_attachment_get_is_closed(spine_path_attachment attachment) {
    if (attachment == nullptr) return 0;
    PathAttachment *_attachment = (PathAttachment*)attachment;
    return _attachment->isClosed() ? -1 : 0;
}

FFI_PLUGIN_EXPORT void spine_path_attachment_set_is_closed(spine_path_attachment attachment, int isClosed) {
    if (attachment == nullptr) return;
    PathAttachment *_attachment = (PathAttachment*)attachment;
    _attachment->setClosed(isClosed);
}

FFI_PLUGIN_EXPORT int spine_path_attachment_get_is_constant_speed(spine_path_attachment attachment) {
    if (attachment == nullptr) return 0;
    PathAttachment *_attachment = (PathAttachment*)attachment;
    return _attachment->isConstantSpeed() ? -1 : 0;
}

FFI_PLUGIN_EXPORT void spine_path_attachment_set_is_constant_speed(spine_path_attachment attachment, int isConstantSpeed) {
    if (attachment == nullptr) return;
    PathAttachment *_attachment = (PathAttachment*)attachment;
    _attachment->setConstantSpeed(isConstantSpeed);
}

FFI_PLUGIN_EXPORT spine_color spine_path_attachment_get_color(spine_path_attachment attachment) {
    spine_color result = { 0, 0, 0, 0 };
    if (attachment == nullptr) return result;
    PathAttachment *_attachment = (PathAttachment*)attachment;
    Color &color = _attachment->getColor();
    result = { color.r, color.g, color.b, color.a };
    return result;
}

FFI_PLUGIN_EXPORT void spine_path_attachment_set_color(spine_path_attachment attachment, float r, float g, float b, float a) {
    if (attachment == nullptr) return;
    PathAttachment *_attachment = (PathAttachment*)attachment;
    _attachment->getColor().set(r, g, b, a);
}

// Skin
FFI_PLUGIN_EXPORT void spine_skin_set_attachment(spine_skin skin, int slotIndex, const char* name, spine_attachment attachment) {
    if (skin == nullptr) return;
    Skin *_skin = (Skin*)skin;
    _skin->setAttachment(slotIndex, name, (Attachment*)attachment);
}

FFI_PLUGIN_EXPORT spine_attachment spine_skin_get_attachment(spine_skin skin, int slotIndex, const char* name) {
    if (skin == nullptr) return nullptr;
    Skin *_skin = (Skin*)skin;
    return (spine_attachment)_skin->getAttachment(slotIndex, name);
}

FFI_PLUGIN_EXPORT void spine_skin_remove_attachment(spine_skin skin, int slotIndex, const char* name) {
    if (skin == nullptr) return;
    Skin *_skin = (Skin*)skin;
    _skin->removeAttachment(slotIndex, name);
}

FFI_PLUGIN_EXPORT const char* spine_skin_get_name(spine_skin skin) {
    if (skin == nullptr) return nullptr;
    Skin *_skin = (Skin*)skin;
    return _skin->getName().buffer();
}

FFI_PLUGIN_EXPORT void spine_skin_add_skin(spine_skin skin, spine_skin other) {
    if (skin == nullptr) return;
    if (other == nullptr) return;
    Skin *_skin = (Skin*)skin;
    _skin->addSkin((Skin*)other);
}

FFI_PLUGIN_EXPORT spine_skin_entries *spine_skin_get_entries(spine_skin skin) {
    if (skin == nullptr) return nullptr;
    Skin *_skin = (Skin*)skin;
    spine_skin_entries *entries = SpineExtension::getInstance()->calloc<spine_skin_entries>(1, __FILE__, __LINE__);
    {
        Skin::AttachmentMap::Entries mapEntries = _skin->getAttachments();
        while (mapEntries.hasNext()) entries->numEntries++;
    }
    {
        entries->entries = SpineExtension::getInstance()->calloc<spine_skin_entry>(entries->numEntries, __FILE__, __LINE__);
        Skin::AttachmentMap::Entries mapEntries = _skin->getAttachments();
        int i = 0;
        while (mapEntries.hasNext()) {
            Skin::AttachmentMap::Entry entry = mapEntries.next();
            entries->entries[i++] = { (int)entry._slotIndex, entry._name.buffer(), (spine_attachment)entry._attachment };
        }
    }
    return entries;
}

FFI_PLUGIN_EXPORT void spine_skin_entries_dispose(spine_skin_entries *entries) {
    if (entries == nullptr) return;
    SpineExtension::getInstance()->free(entries->entries, __FILE__, __LINE__);
    SpineExtension::getInstance()->free(entries, __FILE__, __LINE__);
}

FFI_PLUGIN_EXPORT int spine_skin_get_num_bones(spine_skin skin) {
    if (skin == nullptr) return 0;
    Skin *_skin = (Skin*)skin;
    return (int)_skin->getBones().size();
}

FFI_PLUGIN_EXPORT spine_bone_data* spine_skin_get_bones(spine_skin skin) {
    if (skin == nullptr) return nullptr;
    Skin *_skin = (Skin*)skin;
    return (spine_bone_data*)_skin->getBones().buffer();
}

FFI_PLUGIN_EXPORT int spine_skin_get_num_constraints(spine_skin skin) {
    if (skin == nullptr) return 0;
    Skin *_skin = (Skin*)skin;
    return (int)_skin->getConstraints().size();
}

FFI_PLUGIN_EXPORT spine_constraint_data* spine_skin_get_constraints(spine_skin skin) {
    if (skin == nullptr) return nullptr;
    Skin *_skin = (Skin*)skin;
    return (spine_constraint_data*)_skin->getConstraints().buffer();
}

FFI_PLUGIN_EXPORT spine_skin spine_skin_create(const char* name) {
    if (name == nullptr) return nullptr;
    return (spine_skin)new (__FILE__, __LINE__) Skin(name);
}

FFI_PLUGIN_EXPORT void spine_skin_dispose(spine_skin skin) {
    if (skin == nullptr) return;
    Skin *_skin = (Skin*)skin;
    delete _skin;
}

FFI_PLUGIN_EXPORT spine_constraint_type spine_constraint_data_get_type(spine_constraint_data data) {
    if (data == nullptr) return SPINE_CONSTRAINT_IK;
    ConstraintData *_data = (ConstraintData*)data;
    if (_data->getRTTI().isExactly(IkConstraintData::rtti)) {
        return SPINE_CONSTRAINT_IK;
    } else if (_data->getRTTI().isExactly(TransformConstraintData::rtti)) {
        return SPINE_CONSTRAINT_TRANSFORM;
    } else if (_data->getRTTI().isExactly(PathConstraintData::rtti)) {
        return SPINE_CONSTRAINT_PATH;
    } else {
        return SPINE_CONSTRAINT_IK;
    }
}

// IkConstraintData
FFI_PLUGIN_EXPORT int spine_ik_constraint_data_get_num_bones(spine_ik_constraint_data data) {
    if (data == nullptr) return 0;
    IkConstraintData *_data = (IkConstraintData*)data;
    return (int)_data->getBones().size();
}

FFI_PLUGIN_EXPORT spine_bone_data* spine_ik_constraint_data_get_bones(spine_ik_constraint_data data) {
    if (data == nullptr) return nullptr;
    IkConstraintData *_data = (IkConstraintData*)data;
    return (spine_bone_data*)_data->getBones().buffer();
}

FFI_PLUGIN_EXPORT spine_bone_data spine_ik_constraint_data_get_target(spine_ik_constraint_data data) {
    if (data == nullptr) return nullptr;
    IkConstraintData *_data = (IkConstraintData*)data;
    return (spine_bone_data)_data->getTarget();
}

FFI_PLUGIN_EXPORT void spine_ik_constraint_data_set_target(spine_ik_constraint_data data, spine_bone_data target) {
    if (data == nullptr) return;
    IkConstraintData *_data = (IkConstraintData*)data;
    _data->setTarget((BoneData*)target);
}

FFI_PLUGIN_EXPORT int spine_ik_constraint_data_get_bend_direction(spine_ik_constraint_data data) {
    if (data == nullptr) return 1;
    IkConstraintData *_data = (IkConstraintData*)data;
    return _data->getBendDirection();
}

FFI_PLUGIN_EXPORT void spine_ik_constraint_data_set_bend_direction(spine_ik_constraint_data data, int bendDirection) {
    if (data == nullptr) return;
    IkConstraintData *_data = (IkConstraintData*)data;
    _data->setBendDirection(bendDirection);
}

FFI_PLUGIN_EXPORT int spine_ik_constraint_data_get_compress(spine_ik_constraint_data data) {
    if (data == nullptr) return 0;
    IkConstraintData *_data = (IkConstraintData*)data;
    return _data->getCompress() ? -1 : 0;
}

FFI_PLUGIN_EXPORT void spine_ik_constraint_data_set_compress(spine_ik_constraint_data data, int compress) {
    if (data == nullptr) return;
    IkConstraintData *_data = (IkConstraintData*)data;
    _data->setCompress(compress);
}

FFI_PLUGIN_EXPORT int spine_ik_constraint_data_get_stretch(spine_ik_constraint_data data) {
    if (data == nullptr) return 0;
    IkConstraintData *_data = (IkConstraintData*)data;
    return _data->getStretch() ? -1 : 0;
}

FFI_PLUGIN_EXPORT void spine_ik_constraint_data_set_stretch(spine_ik_constraint_data data, int stretch) {
    if (data == nullptr) return;
    IkConstraintData *_data = (IkConstraintData*)data;
    _data->setStretch(stretch);
}

FFI_PLUGIN_EXPORT int spine_ik_constraint_data_get_uniform(spine_ik_constraint_data data) {
    if (data == nullptr) return 0;
    IkConstraintData *_data = (IkConstraintData*)data;
    return _data->getUniform() ? -1 : 0;
}

FFI_PLUGIN_EXPORT float spine_ik_constraint_data_get_mix(spine_ik_constraint_data data) {
    if (data == nullptr) return 0;
    IkConstraintData *_data = (IkConstraintData*)data;
    return _data->getMix();
}

FFI_PLUGIN_EXPORT void spine_ik_constraint_data_set_mix(spine_ik_constraint_data data, float mix) {
    if (data == nullptr) return;
    IkConstraintData *_data = (IkConstraintData*)data;
    _data->setMix(mix);
}

FFI_PLUGIN_EXPORT float spine_ik_constraint_data_get_softness(spine_ik_constraint_data data) {
    if (data == nullptr) return 0;
    IkConstraintData *_data = (IkConstraintData*)data;
    return _data->getSoftness();
}

FFI_PLUGIN_EXPORT void spine_ik_constraint_data_set_softness(spine_ik_constraint_data data, float softness) {
    if (data == nullptr) return;
    IkConstraintData *_data = (IkConstraintData*)data;
    _data->setSoftness(softness);
}

// IKConstraint
FFI_PLUGIN_EXPORT void spine_ik_constraint_update(spine_ik_constraint constraint) {
    if (constraint == nullptr) return;
    IkConstraint *_constraint = (IkConstraint*)constraint;
    _constraint->update();
}

FFI_PLUGIN_EXPORT int spine_ik_constraint_get_order(spine_ik_constraint constraint) {
    if (constraint == nullptr) return 0;
    IkConstraint *_constraint = (IkConstraint*)constraint;
    return _constraint->getOrder();
}

FFI_PLUGIN_EXPORT spine_ik_constraint_data spine_ik_constraint_get_data(spine_ik_constraint constraint) {
    if (constraint == nullptr) return nullptr;
    IkConstraint *_constraint = (IkConstraint*)constraint;
    return (spine_ik_constraint_data)&_constraint->getData();
}

FFI_PLUGIN_EXPORT int spine_ik_constraint_get_num_bones(spine_ik_constraint constraint) {
    if (constraint == nullptr) return 0;
    IkConstraint *_constraint = (IkConstraint*)constraint;
    return (int)_constraint->getBones().size();
}

FFI_PLUGIN_EXPORT spine_bone* spine_ik_constraint_get_bones(spine_ik_constraint constraint) {
    if (constraint == nullptr) return nullptr;
    IkConstraint *_constraint = (IkConstraint*)constraint;
    return (spine_bone*)_constraint->getBones().buffer();
}

FFI_PLUGIN_EXPORT spine_bone spine_ik_constraint_get_target(spine_ik_constraint constraint) {
    if (constraint == nullptr) return nullptr;
    IkConstraint *_constraint = (IkConstraint*)constraint;
    return (spine_bone)_constraint->getTarget();
}

FFI_PLUGIN_EXPORT void spine_ik_constraint_set_target(spine_ik_constraint constraint, spine_bone target) {
    if (constraint == nullptr) return;
    IkConstraint *_constraint = (IkConstraint*)constraint;
    _constraint->setTarget((Bone*)target);
}

FFI_PLUGIN_EXPORT int spine_ik_constraint_get_bend_direction(spine_ik_constraint constraint) {
    if (constraint == nullptr) return 1;
    IkConstraint *_constraint = (IkConstraint*)constraint;
    return _constraint->getBendDirection();
}

FFI_PLUGIN_EXPORT void spine_ik_constraint_set_bend_direction(spine_ik_constraint constraint, int bendDirection) {
    if (constraint == nullptr) return;
    IkConstraint *_constraint = (IkConstraint*)constraint;
    _constraint->setBendDirection(bendDirection);
}

FFI_PLUGIN_EXPORT int spine_ik_constraint_get_compress(spine_ik_constraint constraint) {
    if (constraint == nullptr) return 0;
    IkConstraint *_constraint = (IkConstraint*)constraint;
    return _constraint->getCompress() ? -1 : 0;
}

FFI_PLUGIN_EXPORT void spine_ik_constraint_set_compress(spine_ik_constraint constraint, int compress) {
    if (constraint == nullptr) return;
    IkConstraint *_constraint = (IkConstraint*)constraint;
    _constraint->setCompress(compress);
}

FFI_PLUGIN_EXPORT int spine_ik_constraint_get_stretch(spine_ik_constraint constraint) {
    if (constraint == nullptr) return 0;
    IkConstraint *_constraint = (IkConstraint*)constraint;
    return _constraint->getStretch() ? -1 : 0;
}

FFI_PLUGIN_EXPORT void spine_ik_constraint_set_stretch(spine_ik_constraint constraint, int stretch) {
    if (constraint == nullptr) return;
    IkConstraint *_constraint = (IkConstraint*)constraint;
    _constraint->setStretch(stretch);
}

FFI_PLUGIN_EXPORT float spine_ik_constraint_get_mix(spine_ik_constraint constraint) {
    if (constraint == nullptr) return 0;
    IkConstraint *_constraint = (IkConstraint*)constraint;
    return _constraint->getMix();
}

FFI_PLUGIN_EXPORT void spine_ik_constraint_set_mix(spine_ik_constraint constraint, float mix) {
    if (constraint == nullptr) return;
    IkConstraint *_constraint = (IkConstraint*)constraint;
    _constraint->setMix(mix);
}

FFI_PLUGIN_EXPORT float spine_ik_constraint_get_softness(spine_ik_constraint constraint) {
    if (constraint == nullptr) return 0;
    IkConstraint *_constraint = (IkConstraint*)constraint;
    return _constraint->getSoftness();
}

FFI_PLUGIN_EXPORT void spine_ik_constraint_set_softness(spine_ik_constraint constraint, float softness) {
    if (constraint == nullptr) return;
    IkConstraint *_constraint = (IkConstraint*)constraint;
    _constraint->setSoftness(softness);
}

FFI_PLUGIN_EXPORT int spine_ik_constraint_get_is_active(spine_ik_constraint constraint) {
    if (constraint == nullptr) return 0;
    IkConstraint *_constraint = (IkConstraint*)constraint;
    return _constraint->isActive() ? -1 : 0;
}

FFI_PLUGIN_EXPORT void spine_ik_constraint_set_is_active(spine_ik_constraint constraint, int isActive) {
    if (constraint == nullptr) return;
    IkConstraint *_constraint = (IkConstraint*)constraint;
    _constraint->setActive(isActive);
}

// TransformConstraintData
FFI_PLUGIN_EXPORT int spine_transform_constraint_data_get_num_bones(spine_transform_constraint_data data) {
    if (data == nullptr) return 0;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    return (int)_data->getBones().size();
}

FFI_PLUGIN_EXPORT spine_bone_data* spine_transform_constraint_data_get_bones(spine_transform_constraint_data data) {
    if (data == nullptr) return nullptr;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    return (spine_bone_data*)_data->getBones().buffer();
}

FFI_PLUGIN_EXPORT spine_bone_data spine_transform_constraint_data_get_target(spine_transform_constraint_data data) {
    if (data == nullptr) return nullptr;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    return (spine_bone_data)_data->getTarget();
}

FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_target(spine_transform_constraint_data data, spine_bone_data target) {
    if (data == nullptr) return;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    _data->setTarget((BoneData*)target);
}

FFI_PLUGIN_EXPORT float spine_transform_constraint_data_get_mix_rotate(spine_transform_constraint_data data) {
    if (data == nullptr) return 0;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    return _data->getMixRotate();
}

FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_mix_rotate(spine_transform_constraint_data data, float mixRotate) {
    if (data == nullptr) return;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    _data->setMixRotate(mixRotate);
}

FFI_PLUGIN_EXPORT float spine_transform_constraint_data_get_mix_x(spine_transform_constraint_data data) {
    if (data == nullptr) return 0;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    return _data->getMixX();
}

FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_mix_x(spine_transform_constraint_data data, float mixX) {
    if (data == nullptr) return;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    _data->setMixX(mixX);
}

FFI_PLUGIN_EXPORT float spine_transform_constraint_data_get_mix_y(spine_transform_constraint_data data) {
    if (data == nullptr) return 0;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    return _data->getMixY();
}

FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_mix_y(spine_transform_constraint_data data, float mixY) {
    if (data == nullptr) return;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    _data->setMixY(mixY);
}

FFI_PLUGIN_EXPORT float spine_transform_constraint_data_get_mix_scale_x(spine_transform_constraint_data data) {
    if (data == nullptr) return 0;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    return _data->getMixScaleX();
}

FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_mix_scale_x(spine_transform_constraint_data data, float mixScaleX) {
    if (data == nullptr) return;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    _data->setMixScaleX(mixScaleX);
}

FFI_PLUGIN_EXPORT float spine_transform_constraint_data_get_mix_scale_y(spine_transform_constraint_data data) {
    if (data == nullptr) return 0;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    return _data->getMixScaleY();
}

FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_mix_scale_y(spine_transform_constraint_data data, float mixScaleY) {
    if (data == nullptr) return;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    _data->setMixScaleY(mixScaleY);
}

FFI_PLUGIN_EXPORT float spine_transform_constraint_data_get_mix_shear_y(spine_transform_constraint_data data) {
    if (data == nullptr) return 0;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    return _data->getMixShearY();
}

FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_mix_shear_y(spine_transform_constraint_data data, float mixShearY) {
    if (data == nullptr) return;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    _data->setMixShearY(mixShearY);
}

FFI_PLUGIN_EXPORT float spine_transform_constraint_data_get_offset_rotation(spine_transform_constraint_data data) {
    if (data == nullptr) return 0;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    return _data->getOffsetRotation();
}

FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_offset_rotation(spine_transform_constraint_data data, float offsetRotation) {
    if (data == nullptr) return;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    _data->setOffsetRotation(offsetRotation);
}

FFI_PLUGIN_EXPORT float spine_transform_constraint_data_get_offset_x(spine_transform_constraint_data data) {
    if (data == nullptr) return 0;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    return _data->getOffsetX();
}

FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_offset_x(spine_transform_constraint_data data, float offsetX) {
    if (data == nullptr) return;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    _data->setOffsetX(offsetX);
}

FFI_PLUGIN_EXPORT float spine_transform_constraint_data_get_offset_y(spine_transform_constraint_data data) {
    if (data == nullptr) return 0;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    return _data->getOffsetY();
}

FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_offset_y(spine_transform_constraint_data data, float offsetY) {
    if (data == nullptr) return;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    _data->setOffsetY(offsetY);
}

FFI_PLUGIN_EXPORT float spine_transform_constraint_data_get_offset_scale_x(spine_transform_constraint_data data) {
    if (data == nullptr) return 0;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    return _data->getOffsetScaleX();
}

FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_offset_scale_x(spine_transform_constraint_data data, float offsetScaleX) {
    if (data == nullptr) return;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    _data->setOffsetScaleX(offsetScaleX);
}

FFI_PLUGIN_EXPORT float spine_transform_constraint_data_get_offset_scale_y(spine_transform_constraint_data data) {
    if (data == nullptr) return 0;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    return _data->getOffsetScaleY();
}

FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_offset_scale_y(spine_transform_constraint_data data, float offsetScaleY) {
    if (data == nullptr) return;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    _data->setOffsetScaleY(offsetScaleY);
}

FFI_PLUGIN_EXPORT float spine_transform_constraint_data_get_offset_shear_y(spine_transform_constraint_data data) {
    if (data == nullptr) return 0;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    return _data->getOffsetShearY();
}

FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_offset_shear_y(spine_transform_constraint_data data, float offsetShearY) {
    if (data == nullptr) return;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    _data->setOffsetShearY(offsetShearY);
}

FFI_PLUGIN_EXPORT int spine_transform_constraint_data_get_is_relative(spine_transform_constraint_data data) {
    if (data == nullptr) return 0;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    return _data->isRelative() ? -1 : 0;
}

FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_is_relative(spine_transform_constraint_data data, int isRelative) {
    if (data == nullptr) return;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    _data->setRelative(isRelative);
}

FFI_PLUGIN_EXPORT int spine_transform_constraint_data_get_is_local(spine_transform_constraint_data data) {
    if (data == nullptr) return 0;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    return _data->isLocal() ? -1 : 0;
}

FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_is_local(spine_transform_constraint_data data, int isLocal) {
    if (data == nullptr) return;
    TransformConstraintData *_data = (TransformConstraintData*)data;
    _data->setLocal(isLocal);
}

// TransformConstraint
FFI_PLUGIN_EXPORT void spine_transform_constraint_update(spine_transform_constraint constraint) {
    if (constraint == nullptr) return;
    TransformConstraint *_constraint = (TransformConstraint*)constraint;
    _constraint->update();
}

FFI_PLUGIN_EXPORT int spine_transform_constraint_get_order(spine_transform_constraint constraint) {
    if (constraint == nullptr) return 0;
    TransformConstraint *_constraint = (TransformConstraint*)constraint;
    return _constraint->getOrder();
}

FFI_PLUGIN_EXPORT spine_transform_constraint_data spine_transform_constraint_get_data(spine_transform_constraint constraint) {
    if (constraint == nullptr) return nullptr;
    TransformConstraint *_constraint = (TransformConstraint*)constraint;
    return (spine_transform_constraint_data)&_constraint->getData();
}

FFI_PLUGIN_EXPORT int spine_transform_constraint_get_num_bones(spine_transform_constraint constraint) {
    if (constraint == nullptr) return 0;
    TransformConstraint *_constraint = (TransformConstraint*)constraint;
    return (int)_constraint->getBones().size();
}

FFI_PLUGIN_EXPORT spine_bone* spine_transform_constraint_get_bones(spine_transform_constraint constraint) {
    if (constraint == nullptr) return nullptr;
    TransformConstraint *_constraint = (TransformConstraint*)constraint;
    return (spine_bone*)_constraint->getBones().buffer();
}

FFI_PLUGIN_EXPORT spine_bone spine_transform_constraint_get_target(spine_transform_constraint constraint) {
    if (constraint == nullptr) return nullptr;
    TransformConstraint *_constraint = (TransformConstraint*)constraint;
    return (spine_bone)_constraint->getTarget();
}

FFI_PLUGIN_EXPORT void spine_transform_constraint_set_target(spine_transform_constraint constraint, spine_bone target) {
    if (constraint == nullptr) return;
    TransformConstraint *_constraint = (TransformConstraint*)constraint;
    _constraint->setTarget((Bone*)target);
}

FFI_PLUGIN_EXPORT float spine_transform_constraint_get_mix_rotate(spine_transform_constraint constraint) {
    if (constraint == nullptr) return 0;
    TransformConstraint *_constraint = (TransformConstraint*)constraint;
    return _constraint->getMixRotate();
}

FFI_PLUGIN_EXPORT void spine_transform_constraint_set_mix_rotate(spine_transform_constraint constraint, float mixRotate) {
    if (constraint == nullptr) return;
    TransformConstraint *_constraint = (TransformConstraint*)constraint;
    _constraint->setMixRotate(mixRotate);
}

FFI_PLUGIN_EXPORT float spine_transform_constraint_get_mix_x(spine_transform_constraint constraint) {
    if (constraint == nullptr) return 0;
    TransformConstraint *_constraint = (TransformConstraint*)constraint;
    return _constraint->getMixX();
}

FFI_PLUGIN_EXPORT void spine_transform_constraint_set_mix_x(spine_transform_constraint constraint, float mixX) {
    if (constraint == nullptr) return;
    TransformConstraint *_constraint = (TransformConstraint*)constraint;
    _constraint->setMixX(mixX);
}

FFI_PLUGIN_EXPORT float spine_transform_constraint_get_mix_y(spine_transform_constraint constraint) {
    if (constraint == nullptr) return 0;
    TransformConstraint *_constraint = (TransformConstraint*)constraint;
    return _constraint->getMixY();
}

FFI_PLUGIN_EXPORT void spine_transform_constraint_set_mix_y(spine_transform_constraint constraint, float mixY) {
    if (constraint == nullptr) return;
    TransformConstraint *_constraint = (TransformConstraint*)constraint;
    _constraint->setMixY(mixY);
}

FFI_PLUGIN_EXPORT float spine_transform_constraint_get_mix_scale_x(spine_transform_constraint constraint) {
    if (constraint == nullptr) return 0;
    TransformConstraint *_constraint = (TransformConstraint*)constraint;
    return _constraint->getMixScaleX();
}

FFI_PLUGIN_EXPORT void spine_transform_constraint_set_mix_scale_x(spine_transform_constraint constraint, float mixScaleX) {
    if (constraint == nullptr) return;
    TransformConstraint *_constraint = (TransformConstraint*)constraint;
    _constraint->setMixScaleX(mixScaleX);
}

FFI_PLUGIN_EXPORT float spine_transform_constraint_get_mix_scale_y(spine_transform_constraint constraint) {
    if (constraint == nullptr) return 0;
    TransformConstraint *_constraint = (TransformConstraint*)constraint;
    return _constraint->getMixScaleY();
}

FFI_PLUGIN_EXPORT void spine_transform_constraint_set_mix_scale_y(spine_transform_constraint constraint, float mixScaleY) {
    if (constraint == nullptr) return;
    TransformConstraint *_constraint = (TransformConstraint*)constraint;
    _constraint->setMixScaleY(mixScaleY);
}

FFI_PLUGIN_EXPORT float spine_transform_constraint_get_mix_shear_y(spine_transform_constraint constraint) {
    if (constraint == nullptr) return 0;
    TransformConstraint *_constraint = (TransformConstraint*)constraint;
    return _constraint->getMixShearY();
}

FFI_PLUGIN_EXPORT void spine_transform_constraint_set_mix_shear_y(spine_transform_constraint constraint, float mixShearY) {
    if (constraint == nullptr) return;
    TransformConstraint *_constraint = (TransformConstraint*)constraint;
    _constraint->setMixShearY(mixShearY);
}

FFI_PLUGIN_EXPORT float spine_transform_constraint_get_is_active(spine_transform_constraint constraint) {
    if (constraint == nullptr) return 0;
    TransformConstraint *_constraint = (TransformConstraint*)constraint;
    return _constraint->isActive() ? -1 : 0;
}

FFI_PLUGIN_EXPORT void spine_transform_constraint_set_is_active(spine_transform_constraint constraint, int isActive) {
    if (constraint == nullptr) return;
    TransformConstraint *_constraint = (TransformConstraint*)constraint;
    _constraint->setActive(isActive);
}

// PathConstraintData
FFI_PLUGIN_EXPORT int spine_path_constraint_data_get_num_bones(spine_path_constraint_data data) {
    if (data == nullptr) return 0;
    PathConstraintData *_data = (PathConstraintData*)data;
    return (int)_data->getBones().size();
}

FFI_PLUGIN_EXPORT spine_bone_data* spine_path_constraint_data_get_bones(spine_path_constraint_data data) {
    if (data == nullptr) return nullptr;
    PathConstraintData *_data = (PathConstraintData*)data;
    return (spine_bone_data*)_data->getBones().buffer();
}

FFI_PLUGIN_EXPORT spine_slot_data spine_path_constraint_data_get_target(spine_path_constraint_data data) {
    if (data == nullptr) return nullptr;
    PathConstraintData *_data = (PathConstraintData*)data;
    return (spine_slot_data)_data->getTarget();
}

FFI_PLUGIN_EXPORT void spine_path_constraint_data_set_target(spine_path_constraint_data data, spine_slot_data target) {
    if (data == nullptr) return;
    PathConstraintData *_data = (PathConstraintData*)data;
    _data->setTarget((SlotData*)target);
}

FFI_PLUGIN_EXPORT spine_position_mode spine_path_constraint_data_get_position_mode(spine_path_constraint_data data) {
    if (data == nullptr) return SPINE_POSITION_MODE_FIXED;
    PathConstraintData *_data = (PathConstraintData*)data;
    return (spine_position_mode)_data->getPositionMode();
}

FFI_PLUGIN_EXPORT void spine_path_constraint_data_set_position_mode(spine_path_constraint_data data, spine_position_mode positionMode) {
    if (data == nullptr) return;
    PathConstraintData *_data = (PathConstraintData*)data;
    _data->setPositionMode((PositionMode)positionMode);
}

FFI_PLUGIN_EXPORT spine_spacing_mode spine_path_constraint_data_get_spacing_mode(spine_path_constraint_data data) {
    if (data == nullptr) return SPINE_SPACING_MODE_LENGTH;
    PathConstraintData *_data = (PathConstraintData*)data;
    return (spine_spacing_mode)_data->getSpacingMode();
}

FFI_PLUGIN_EXPORT void spine_path_constraint_data_set_spacing_mode(spine_path_constraint_data data, spine_spacing_mode spacingMode) {
    if (data == nullptr) return;
    PathConstraintData *_data = (PathConstraintData*)data;
    _data->setSpacingMode((SpacingMode)spacingMode);
}

FFI_PLUGIN_EXPORT spine_rotate_mode spine_path_constraint_data_get_rotate_mode(spine_path_constraint_data data) {
    if (data == nullptr) return SPINE_ROTATE_MODE_TANGENT;
    PathConstraintData *_data = (PathConstraintData*)data;
    return (spine_rotate_mode)_data->getRotateMode();
}

FFI_PLUGIN_EXPORT void spine_path_constraint_data_set_rotate_mode(spine_path_constraint_data data, spine_rotate_mode rotateMode) {
    if (data == nullptr) return;
    PathConstraintData *_data = (PathConstraintData*)data;
    _data->setRotateMode((RotateMode)rotateMode);
}

FFI_PLUGIN_EXPORT float spine_path_constraint_data_get_offset_rotation(spine_path_constraint_data data) {
    if (data == nullptr) return 0;
    PathConstraintData *_data = (PathConstraintData*)data;
    return _data->getOffsetRotation();
}

FFI_PLUGIN_EXPORT void spine_path_constraint_data_set_offset_rotation(spine_path_constraint_data data, float offsetRotation) {
    if (data == nullptr) return;
    PathConstraintData *_data = (PathConstraintData*)data;
    _data->setOffsetRotation(offsetRotation);
}

FFI_PLUGIN_EXPORT float spine_path_constraint_data_get_position(spine_path_constraint_data data) {
    if (data == nullptr) return 0;
    PathConstraintData *_data = (PathConstraintData*)data;
    return _data->getPosition();
}

FFI_PLUGIN_EXPORT void spine_path_constraint_data_set_position(spine_path_constraint_data data, float position) {
    if (data == nullptr) return;
    PathConstraintData *_data = (PathConstraintData*)data;
    _data->setPosition(position);
}

FFI_PLUGIN_EXPORT float spine_path_constraint_data_get_spacing(spine_path_constraint_data data) {
    if (data == nullptr) return 0;
    PathConstraintData *_data = (PathConstraintData*)data;
    return _data->getSpacing();
}

FFI_PLUGIN_EXPORT void spine_path_constraint_data_set_spacing(spine_path_constraint_data data, float spacing) {
    if (data == nullptr) return;
    PathConstraintData *_data = (PathConstraintData*)data;
    _data->setSpacing(spacing);
}

FFI_PLUGIN_EXPORT float spine_path_constraint_data_get_mix_rotate(spine_path_constraint_data data) {
    if (data == nullptr) return 0;
    PathConstraintData *_data = (PathConstraintData*)data;
    return _data->getMixRotate();
}

FFI_PLUGIN_EXPORT void spine_path_constraint_data_set_mix_rotate(spine_path_constraint_data data, float mixRotate) {
    if (data == nullptr) return;
    PathConstraintData *_data = (PathConstraintData*)data;
    _data->setMixRotate(mixRotate);
}

FFI_PLUGIN_EXPORT float spine_path_constraint_data_get_mix_x(spine_path_constraint_data data) {
    if (data == nullptr) return 0;
    PathConstraintData *_data = (PathConstraintData*)data;
    return _data->getMixX();
}

FFI_PLUGIN_EXPORT void spine_path_constraint_data_set_mix_x(spine_path_constraint_data data, float mixX) {
    if (data == nullptr) return;
    PathConstraintData *_data = (PathConstraintData*)data;
    _data->setMixX(mixX);
}

FFI_PLUGIN_EXPORT float spine_path_constraint_data_get_mix_y(spine_path_constraint_data data) {
    if (data == nullptr) return 0;
    PathConstraintData *_data = (PathConstraintData*)data;
    return _data->getMixY();
}

FFI_PLUGIN_EXPORT void spine_path_constraint_data_set_mix_y(spine_path_constraint_data data, float mixY) {
    if (data == nullptr) return;
    PathConstraintData *_data = (PathConstraintData*)data;
    _data->setMixY(mixY);
}

// PathConstraint
FFI_PLUGIN_EXPORT void spine_path_constraint_update(spine_path_constraint constraint) {
    if (constraint == nullptr) return;
    PathConstraint *_constraint = (PathConstraint*)constraint;
    _constraint->update();
}

FFI_PLUGIN_EXPORT int spine_path_constraint_get_order(spine_path_constraint constraint) {
    if (constraint == nullptr) return 0;
    PathConstraint *_constraint = (PathConstraint*)constraint;
    return _constraint->getOrder();
}

FFI_PLUGIN_EXPORT spine_path_constraint_data spine_path_constraint_get_data(spine_path_constraint constraint) {
    if (constraint == nullptr) return nullptr;
    PathConstraint *_constraint = (PathConstraint*)constraint;
    return (spine_path_constraint_data)&_constraint->getData();
}

FFI_PLUGIN_EXPORT int spine_path_constraint_get_num_bones(spine_path_constraint constraint) {
    if (constraint == nullptr) return 0;
    PathConstraint *_constraint = (PathConstraint*)constraint;
    return (int)_constraint->getBones().size();
}

FFI_PLUGIN_EXPORT spine_bone* spine_path_constraint_get_bones(spine_path_constraint constraint) {
    if (constraint == nullptr) return nullptr;
    PathConstraint *_constraint = (PathConstraint*)constraint;
    return (spine_bone*)_constraint->getBones().buffer();
}

FFI_PLUGIN_EXPORT spine_slot spine_path_constraint_get_target(spine_path_constraint constraint) {
    if (constraint == nullptr) return nullptr;
    PathConstraint *_constraint = (PathConstraint*)constraint;
    return (spine_slot)_constraint->getTarget();
}

FFI_PLUGIN_EXPORT void spine_path_constraint_set_target(spine_path_constraint constraint, spine_slot target) {
    if (constraint == nullptr) return;
    PathConstraint *_constraint = (PathConstraint*)constraint;
    _constraint->setTarget((Slot*)target);
}

FFI_PLUGIN_EXPORT float spine_path_constraint_get_position(spine_path_constraint constraint) {
    if (constraint == nullptr) return 0;
    PathConstraint *_constraint = (PathConstraint*)constraint;
    return _constraint->getPosition();
}

FFI_PLUGIN_EXPORT void spine_path_constraint_set_position(spine_path_constraint constraint, float position) {
    if (constraint == nullptr) return;
    PathConstraint *_constraint = (PathConstraint*)constraint;
    _constraint->setPosition(position);
}

FFI_PLUGIN_EXPORT float spine_path_constraint_get_spacing(spine_path_constraint constraint) {
    if (constraint == nullptr) return 0;
    PathConstraint *_constraint = (PathConstraint*)constraint;
    return _constraint->getSpacing();
}

FFI_PLUGIN_EXPORT void spine_path_constraint_set_spacing(spine_path_constraint constraint, float spacing) {
    if (constraint == nullptr) return;
    PathConstraint *_constraint = (PathConstraint*)constraint;
    _constraint->setSpacing(spacing);
}

FFI_PLUGIN_EXPORT float spine_path_constraint_get_mix_rotate(spine_path_constraint constraint) {
    if (constraint == nullptr) return 0;
    PathConstraint *_constraint = (PathConstraint*)constraint;
    return _constraint->getMixRotate();
}

FFI_PLUGIN_EXPORT void spine_path_constraint_set_mix_rotate(spine_path_constraint constraint, float mixRotate) {
    if (constraint == nullptr) return;
    PathConstraint *_constraint = (PathConstraint*)constraint;
    _constraint->setMixRotate(mixRotate);
}

FFI_PLUGIN_EXPORT float spine_path_constraint_get_mix_x(spine_path_constraint constraint) {
    if (constraint == nullptr) return 0;
    PathConstraint *_constraint = (PathConstraint*)constraint;
    return _constraint->getMixX();
}

FFI_PLUGIN_EXPORT void spine_path_constraint_set_mix_x(spine_path_constraint constraint, float mixX) {
    if (constraint == nullptr) return;
    PathConstraint *_constraint = (PathConstraint*)constraint;
    _constraint->setMixX(mixX);
}

FFI_PLUGIN_EXPORT float spine_path_constraint_get_mix_y(spine_path_constraint constraint) {
    if (constraint == nullptr) return 0;
    PathConstraint *_constraint = (PathConstraint*)constraint;
    return _constraint->getMixY();
}

FFI_PLUGIN_EXPORT void spine_path_constraint_set_mix_y(spine_path_constraint constraint, float mixY) {
    if (constraint == nullptr) return;
    PathConstraint *_constraint = (PathConstraint*)constraint;
    _constraint->setMixY(mixY);
}

FFI_PLUGIN_EXPORT int spine_path_constraint_get_is_active(spine_path_constraint constraint) {
    if (constraint == nullptr) return 0;
    PathConstraint *_constraint = (PathConstraint*)constraint;
    return _constraint->isActive() ? -1 : 0;
}

FFI_PLUGIN_EXPORT void spine_path_constraint_set_is_active(spine_path_constraint constraint, int isActive) {
    if (constraint == nullptr) return;
    PathConstraint *_constraint = (PathConstraint*)constraint;
    _constraint->setActive(isActive);
}

