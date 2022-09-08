#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>

#if _WIN32
#include <windows.h>
#else
#include <pthread.h>
#include <unistd.h>
#endif

#ifdef __cplusplus
#if _WIN32
#define FFI_PLUGIN_EXPORT extern "C" __declspec(dllexport)
#else
#define FFI_PLUGIN_EXPORT extern "C"
#endif
#else
#if _WIN32
#define FFI_PLUGIN_EXPORT __declspec(dllexport)
#else
#define FFI_PLUGIN_EXPORT
#endif
#endif

#define SPINE_OPAQUE_TYPE(name) \
    typedef struct name##_wrapper {} name##_wrapper; \
    typedef name##_wrapper *name;

SPINE_OPAQUE_TYPE(spine_skeleton)
SPINE_OPAQUE_TYPE(spine_skeleton_data)
SPINE_OPAQUE_TYPE(spine_bone)
SPINE_OPAQUE_TYPE(spine_bone_data)
SPINE_OPAQUE_TYPE(spine_slot)
SPINE_OPAQUE_TYPE(spine_slot_data)
SPINE_OPAQUE_TYPE(spine_skin)
SPINE_OPAQUE_TYPE(spine_attachment)
SPINE_OPAQUE_TYPE(spine_region_attachment)
SPINE_OPAQUE_TYPE(spine_vertex_attachment)
SPINE_OPAQUE_TYPE(spine_mesh_attachment)
SPINE_OPAQUE_TYPE(spine_clipping_attachment)
SPINE_OPAQUE_TYPE(spine_bounding_box_attachment)
SPINE_OPAQUE_TYPE(spine_path_attachment)
SPINE_OPAQUE_TYPE(spine_point_attachment)
SPINE_OPAQUE_TYPE(spine_texture_region)
SPINE_OPAQUE_TYPE(spine_sequence)
SPINE_OPAQUE_TYPE(spine_constraint)
SPINE_OPAQUE_TYPE(spine_constraint_data)
SPINE_OPAQUE_TYPE(spine_ik_constraint)
SPINE_OPAQUE_TYPE(spine_ik_constraint_data)
SPINE_OPAQUE_TYPE(spine_transform_constraint)
SPINE_OPAQUE_TYPE(spine_transform_constraint_data)
SPINE_OPAQUE_TYPE(spine_path_constraint)
SPINE_OPAQUE_TYPE(spine_path_constraint_data)
SPINE_OPAQUE_TYPE(spine_animation_state)
SPINE_OPAQUE_TYPE(spine_animation_state_data)
SPINE_OPAQUE_TYPE(spine_animation_state_events)
SPINE_OPAQUE_TYPE(spine_event)
SPINE_OPAQUE_TYPE(spine_event_data)
SPINE_OPAQUE_TYPE(spine_track_entry)
SPINE_OPAQUE_TYPE(spine_animation)

typedef struct spine_atlas {
    void *atlas;
    char **imagePaths;
    int numImagePaths;
    char *error;
} spine_atlas;

typedef struct spine_skeleton_data_result {
    spine_skeleton_data skeletonData;
    char *error;
} spine_skeleton_data_result;

typedef enum spine_blend_mode {
    SPINE_BLEND_MODE_NORMAL = 0,
    SPINE_BLEND_MODE_ADDITIVE,
    SPINE_BLEND_MODE_MULTIPLY,
    SPINE_BLEND_MODE_SCREEN
} spine_blend_mode;

typedef enum spine_mix_blend {
    SPINE_MIX_BLEND_SETUP = 0,
    SPINE_MIX_BLEND_FIRST,
    SPINE_MIX_BLEND_REPLACE,
    SPINE_MIX_BLEND_ADD
} spine_mix_blend;

typedef enum spine_event_type {
    SPINE_EVENT_TYPE_START = 0,
    SPINE_EVENT_TYPE_INTERRUPT,
    SPINE_EVENT_TYPE_END,
    SPINE_EVENT_TYPE_COMPLETE,
    SPINE_EVENT_TYPE_DISPOSE,
    SPINE_EVENT_TYPE_EVENT
} spine_event_type;

typedef enum spine_attachment_type {
    SPINE_ATTACHMENT_REGION = 0,
    SPINE_ATTACHMENT_MESH,
    SPINE_ATTACHMENT_CLIPPING,
    SPINE_ATTACHMENT_BOUNDING_BOX,
    SPINE_ATTACHMENT_PATH,
    SPINE_ATTACHMENT_POINT,
} spine_attachment_type;

typedef enum spine_constraint_type {
    SPINE_CONSTRAINT_IK,
    SPINE_CONSTRAINT_TRANSFORM,
    SPINE_CONSTRAINT_PATH
} spine_constraint_type;

typedef enum spine_transform_mode {
    SPINE_TRANSFORM_MODE_NORMAL = 0,
    SPINE_TRANSFORM_ONLY_TRANSLATION,
    SPINE_TRANSFORM_NO_ROTATION_OR_REFLECTION,
    SPINE_TRANSFORM_NO_SCALE,
    SPINE_TRANSFORM_NO_SCALE_OR_REFLECTION
} spine_transform_mode;

typedef enum spine_position_mode {
    SPINE_POSITION_MODE_FIXED = 0,
    SPINE_POSITION_MODE_PERCENT
} spine_position_mode;

typedef enum spine_spacing_mode {
    SPINE_SPACING_MODE_LENGTH = 0,
    SPINE_SPACING_MODE_FIXED,
    SPINE_SPACING_MODE_PERCENT,
    SPINE_SPACING_MODE_PROPORTIONAL
} spine_spacing_mode;

typedef enum spine_rotate_mode {
    SPINE_ROTATE_MODE_TANGENT = 0,
    SPINE_ROTATE_MODE_CHAIN,
    SPINE_ROTATE_MODE_CHAIN_SCALE
} spine_rotate_mode;

typedef struct spine_render_command {
    float *positions;
    float *uvs;
    int32_t *colors;
    int numVertices;
    uint16_t *indices;
    int numIndices;
    int atlasPage;
    spine_blend_mode blendMode;
    struct spine_render_command *next;
} spine_render_command;

typedef struct spine_bounds {
    float x, y, width, height;
} spine_bounds;

typedef struct spine_color {
    float r, g, b, a;
} spine_color;

typedef struct spine_vector {
    float x, y;
} spine_vector;

typedef struct spine_skeleton_drawable {
    spine_skeleton skeleton;
    spine_animation_state animationState;
    spine_animation_state_data animationStateData;
    spine_animation_state_events animationStateEvents;
    void *clipping;
    spine_render_command *renderCommand;
} spine_skeleton_drawable;

typedef struct spine_skin_entry {
    int slotIndex;
    const char* name;
    spine_attachment attachment;
} spine_skin_entry;

typedef struct spine_skin_entries {
    int numEntries;
    spine_skin_entry* entries;
} spine_skin_entries;

FFI_PLUGIN_EXPORT int spine_major_version();
FFI_PLUGIN_EXPORT int spine_minor_version();
FFI_PLUGIN_EXPORT void spine_report_leaks();

FFI_PLUGIN_EXPORT spine_atlas* spine_atlas_load(const char *atlasData);
FFI_PLUGIN_EXPORT void spine_atlas_dispose(spine_atlas *atlas);

FFI_PLUGIN_EXPORT spine_skeleton_data_result spine_skeleton_data_load_json(spine_atlas *atlas, const char *skeletonData);
FFI_PLUGIN_EXPORT spine_skeleton_data_result spine_skeleton_data_load_binary(spine_atlas *atlas, const unsigned char *skeletonData, int length);
FFI_PLUGIN_EXPORT spine_bone_data spine_skeleton_data_find_bone(spine_skeleton_data data, const char *name);
FFI_PLUGIN_EXPORT spine_slot_data spine_skeleton_data_find_slot(spine_skeleton_data data, const char *name);
FFI_PLUGIN_EXPORT spine_skin spine_skeleton_data_find_skin(spine_skeleton_data data, const char *name);
FFI_PLUGIN_EXPORT spine_event_data spine_skeleton_data_find_event(spine_skeleton_data data, const char *name);
FFI_PLUGIN_EXPORT spine_animation spine_skeleton_data_find_animation(spine_skeleton_data data, const char *name);
FFI_PLUGIN_EXPORT spine_ik_constraint_data spine_skeleton_data_find_ik_constraint(spine_skeleton_data data, const char *name);
FFI_PLUGIN_EXPORT spine_transform_constraint_data spine_skeleton_data_find_transform_constraint(spine_skeleton_data data, const char *name);
FFI_PLUGIN_EXPORT spine_path_constraint_data spine_skeleton_data_find_path_constraint(spine_skeleton_data data, const char *name);
FFI_PLUGIN_EXPORT const char* spine_skeleton_data_get_name(spine_skeleton_data data);
// OMITTED setName()
FFI_PLUGIN_EXPORT int spine_skeleton_data_get_num_bones(spine_skeleton_data data);
FFI_PLUGIN_EXPORT spine_bone_data* spine_skeleton_data_get_bones(spine_skeleton_data data);
FFI_PLUGIN_EXPORT int spine_skeleton_data_get_num_slots(spine_skeleton_data data);
FFI_PLUGIN_EXPORT spine_slot_data* spine_skeleton_data_get_slots(spine_skeleton_data data);
FFI_PLUGIN_EXPORT int spine_skeleton_data_get_num_skins(spine_skeleton_data data);
FFI_PLUGIN_EXPORT spine_skin* spine_skeleton_data_get_skins(spine_skeleton_data data);
FFI_PLUGIN_EXPORT spine_skin spine_skeleton_data_get_default_skin(spine_skeleton_data data);
FFI_PLUGIN_EXPORT void spine_skeleton_data_set_default_skin(spine_skeleton_data data, spine_skin skin);
FFI_PLUGIN_EXPORT int spine_skeleton_data_get_num_events(spine_skeleton_data data);
FFI_PLUGIN_EXPORT spine_event_data* spine_skeleton_data_get_events(spine_skeleton_data data);
FFI_PLUGIN_EXPORT int spine_skeleton_data_get_num_animations(spine_skeleton_data data);
FFI_PLUGIN_EXPORT spine_animation* spine_skeleton_data_get_animations(spine_skeleton_data data);
FFI_PLUGIN_EXPORT int spine_skeleton_data_get_num_ik_constraints(spine_skeleton_data data);
FFI_PLUGIN_EXPORT spine_ik_constraint_data* spine_skeleton_data_get_ik_constraints(spine_skeleton_data data);
FFI_PLUGIN_EXPORT int spine_skeleton_data_get_num_transform_constraints(spine_skeleton_data data);
FFI_PLUGIN_EXPORT spine_transform_constraint_data* spine_skeleton_data_get_transform_constraints(spine_skeleton_data data);
FFI_PLUGIN_EXPORT int spine_skeleton_data_get_num_path_constraints(spine_skeleton_data data);
FFI_PLUGIN_EXPORT spine_path_constraint_data* spine_skeleton_data_get_path_constraints(spine_skeleton_data data);
FFI_PLUGIN_EXPORT float spine_skeleton_data_get_x(spine_skeleton_data data);
FFI_PLUGIN_EXPORT void spine_skeleton_data_set_x(spine_skeleton_data data, float x);
FFI_PLUGIN_EXPORT float spine_skeleton_data_get_y(spine_skeleton_data data);
FFI_PLUGIN_EXPORT void spine_skeleton_data_set_y(spine_skeleton_data data, float y);
FFI_PLUGIN_EXPORT float spine_skeleton_data_get_width(spine_skeleton_data data);
FFI_PLUGIN_EXPORT void spine_skeleton_data_set_width(spine_skeleton_data data, float width);
FFI_PLUGIN_EXPORT float spine_skeleton_data_get_height(spine_skeleton_data data);
FFI_PLUGIN_EXPORT void spine_skeleton_data_set_height(spine_skeleton_data data, float height);
FFI_PLUGIN_EXPORT const char* spine_skeleton_data_get_version(spine_skeleton_data data);
// OMITTED setVersion()
FFI_PLUGIN_EXPORT const char* spine_skeleton_data_get_hash(spine_skeleton_data data);
// OMITTED setHash()
FFI_PLUGIN_EXPORT const char* spine_skeleton_data_get_images_path(spine_skeleton_data data);
// OMITTED setImagesPath()
FFI_PLUGIN_EXPORT const char* spine_skeleton_data_get_audio_path(spine_skeleton_data data);
// OMITTED setAudioPath()
FFI_PLUGIN_EXPORT float spine_skeleton_data_get_fps(spine_skeleton_data data);
// OMITTED setFps()
FFI_PLUGIN_EXPORT void spine_skeleton_data_dispose(spine_skeleton_data data);

FFI_PLUGIN_EXPORT spine_skeleton_drawable *spine_skeleton_drawable_create(spine_skeleton_data skeletonData);
FFI_PLUGIN_EXPORT spine_render_command *spine_skeleton_drawable_render(spine_skeleton_drawable *drawable);
FFI_PLUGIN_EXPORT void spine_skeleton_drawable_dispose(spine_skeleton_drawable *drawable);

FFI_PLUGIN_EXPORT const char* spine_animation_get_name(spine_animation animation);
// OMITTED getTimelines()
// OMITTED hasTimeline()
FFI_PLUGIN_EXPORT float spine_animation_get_duration(spine_animation animation);
// OMITTED setDuration()

FFI_PLUGIN_EXPORT spine_skeleton_data spine_animation_state_data_get_skeleton_data(spine_animation_state_data stateData);
FFI_PLUGIN_EXPORT float spine_animation_state_data_get_default_mix(spine_animation_state_data stateData);
FFI_PLUGIN_EXPORT void spine_animation_state_data_set_default_mix(spine_animation_state_data stateData, float defaultMix);
FFI_PLUGIN_EXPORT void spine_animation_state_data_set_mix(spine_animation_state_data stateData, spine_animation from, spine_animation to, float duration);
FFI_PLUGIN_EXPORT float spine_animation_state_data_get_mix(spine_animation_state_data stateData, spine_animation from, spine_animation to);
FFI_PLUGIN_EXPORT void spine_animation_state_data_set_mix_by_name(spine_animation_state_data stateData, const char* fromName, const char* toName, float duration);
FFI_PLUGIN_EXPORT float spine_animation_state_data_get_mix_by_name(spine_animation_state_data stateData, const char* fromName, const char* toName);
FFI_PLUGIN_EXPORT void spine_animation_state_data_clear(spine_animation_state_data stateData);

FFI_PLUGIN_EXPORT void spine_animation_state_update(spine_animation_state state, float delta);
FFI_PLUGIN_EXPORT void spine_animation_state_apply(spine_animation_state state, spine_skeleton skeleton);
FFI_PLUGIN_EXPORT void spine_animation_state_clear_tracks(spine_animation_state state);
FFI_PLUGIN_EXPORT void spine_animation_state_clear_track(spine_animation_state state, int trackIndex);
FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_set_animation_by_name(spine_animation_state state, int trackIndex, const char* animationName, int loop);
FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_set_animation(spine_animation_state state, int trackIndex, spine_animation animation, int loop);
FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_add_animation_by_name(spine_animation_state state, int trackIndex, const char* animationName, int loop, float delay);
FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_add_animation(spine_animation_state state, int trackIndex, spine_animation animation, int loop, float delay);
FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_set_empty_animation(spine_animation_state state, int trackIndex, float mixDuration);
FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_add_empty_animation(spine_animation_state state, int trackIndex, float mixDuration, float delay);
FFI_PLUGIN_EXPORT void spine_animation_state_set_empty_animations(spine_animation_state state, float mixDuration);
FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_get_current(spine_animation_state state, int trackIndex);
FFI_PLUGIN_EXPORT spine_animation_state_data spine_animation_state_get_data(spine_animation_state state);
FFI_PLUGIN_EXPORT float spine_animation_state_get_time_scale(spine_animation_state state);
FFI_PLUGIN_EXPORT void spine_animation_state_set_time_scale(spine_animation_state state, float timeScale);
// OMITTED setListener()
// OMITTED setListener()
// OMITTED disableQueue()
// OMITTED enableQueue()
// OMITTED setManualTrackEntryDisposal()
// OMITTED getManualTrackEntryDisposal()
FFI_PLUGIN_EXPORT void spine_animation_state_dispose_track_entry(spine_animation_state state, spine_track_entry entry);

FFI_PLUGIN_EXPORT int spine_animation_state_events_get_num_events(spine_animation_state_events events);
FFI_PLUGIN_EXPORT spine_event_type spine_animation_state_events_get_event_type(spine_animation_state_events events, int index);
FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_events_get_track_entry(spine_animation_state_events events, int index);
FFI_PLUGIN_EXPORT spine_event spine_animation_state_events_get_event(spine_animation_state_events events, int index);
FFI_PLUGIN_EXPORT void spine_animation_state_events_reset(spine_animation_state_events events);

FFI_PLUGIN_EXPORT int spine_track_entry_get_track_index(spine_track_entry entry);
FFI_PLUGIN_EXPORT spine_animation spine_track_entry_get_animation(spine_track_entry entry);
FFI_PLUGIN_EXPORT spine_track_entry spine_track_entry_get_previous(spine_track_entry entry);
FFI_PLUGIN_EXPORT int spine_track_entry_get_loop(spine_track_entry entry);
FFI_PLUGIN_EXPORT void spine_track_entry_set_loop(spine_track_entry entry, int loop);
FFI_PLUGIN_EXPORT int spine_track_entry_get_hold_previous(spine_track_entry entry);
FFI_PLUGIN_EXPORT void spine_track_entry_set_hold_previous(spine_track_entry entry, int holdPrevious);
FFI_PLUGIN_EXPORT int spine_track_entry_get_reverse(spine_track_entry entry);
FFI_PLUGIN_EXPORT void spine_track_entry_set_reverse(spine_track_entry entry, int reverse);
FFI_PLUGIN_EXPORT int spine_track_entry_get_shortest_rotation(spine_track_entry entry);
FFI_PLUGIN_EXPORT void spine_track_entry_set_shortest_rotation(spine_track_entry entry, int shortestRotation);
FFI_PLUGIN_EXPORT float spine_track_entry_get_delay(spine_track_entry entry);
FFI_PLUGIN_EXPORT void spine_track_entry_set_delay(spine_track_entry entry, float delay);
FFI_PLUGIN_EXPORT float spine_track_entry_get_track_time(spine_track_entry entry);
FFI_PLUGIN_EXPORT void spine_track_entry_set_track_time(spine_track_entry entry, float trackTime);
FFI_PLUGIN_EXPORT float spine_track_entry_get_track_end(spine_track_entry entry);
FFI_PLUGIN_EXPORT void spine_track_entry_set_track_end(spine_track_entry entry, float trackEnd);
FFI_PLUGIN_EXPORT float spine_track_entry_get_animation_start(spine_track_entry entry);
FFI_PLUGIN_EXPORT void spine_track_entry_set_animation_start(spine_track_entry entry, float animationStart);
FFI_PLUGIN_EXPORT float spine_track_entry_get_animation_end(spine_track_entry entry);
FFI_PLUGIN_EXPORT void spine_track_entry_set_animation_end(spine_track_entry entry, float animationEnd);
FFI_PLUGIN_EXPORT float spine_track_entry_get_animation_last(spine_track_entry entry);
FFI_PLUGIN_EXPORT void spine_track_entry_set_animation_last(spine_track_entry entry, float animationLast);
FFI_PLUGIN_EXPORT float spine_track_entry_get_animation_time(spine_track_entry entry);
FFI_PLUGIN_EXPORT float spine_track_entry_get_time_scale(spine_track_entry entry);
FFI_PLUGIN_EXPORT void spine_track_entry_set_time_scale(spine_track_entry entry, float timeScale);
FFI_PLUGIN_EXPORT float spine_track_entry_get_alpha(spine_track_entry entry);
FFI_PLUGIN_EXPORT void spine_track_entry_set_alpha(spine_track_entry entry, float alpha);
FFI_PLUGIN_EXPORT float spine_track_entry_get_event_threshold(spine_track_entry entry);
FFI_PLUGIN_EXPORT void spine_track_entry_set_event_threshold(spine_track_entry entry, float eventThreshold);
FFI_PLUGIN_EXPORT float spine_track_entry_get_attachment_threshold(spine_track_entry entry);
FFI_PLUGIN_EXPORT void spine_track_entry_set_attachment_threshold(spine_track_entry entry, float attachmentThreshold);
FFI_PLUGIN_EXPORT float spine_track_entry_get_draw_order_threshold(spine_track_entry entry);
FFI_PLUGIN_EXPORT void spine_track_entry_set_draw_order_threshold(spine_track_entry entry, float drawOrderThreshold);
FFI_PLUGIN_EXPORT spine_track_entry spine_track_entry_get_next(spine_track_entry entry);
FFI_PLUGIN_EXPORT int spine_track_entry_is_complete(spine_track_entry entry);
FFI_PLUGIN_EXPORT float spine_track_entry_get_mix_time(spine_track_entry entry);
FFI_PLUGIN_EXPORT void spine_track_entry_set_mix_time(spine_track_entry entry, float mixTime);
FFI_PLUGIN_EXPORT float spine_track_entry_get_mix_duration(spine_track_entry entry);
FFI_PLUGIN_EXPORT void spine_track_entry_set_mix_duration(spine_track_entry entry, float mixDuration);
FFI_PLUGIN_EXPORT spine_mix_blend spine_track_entry_get_mix_blend(spine_track_entry entry);
FFI_PLUGIN_EXPORT void spine_track_entry_set_mix_blend(spine_track_entry entry, spine_mix_blend mixBlend);
FFI_PLUGIN_EXPORT spine_track_entry spine_track_entry_get_mixing_from(spine_track_entry entry);
FFI_PLUGIN_EXPORT spine_track_entry spine_track_entry_get_mixing_to(spine_track_entry entry);
FFI_PLUGIN_EXPORT void spine_track_entry_reset_rotation_directions(spine_track_entry entry);
FFI_PLUGIN_EXPORT float spine_track_entry_get_track_complete(spine_track_entry entry);
// OMITTED setListener()
// OMITTED setListener()

FFI_PLUGIN_EXPORT void spine_skeleton_update_cache(spine_skeleton skeleton);
// OMITTED printUpdateCache()
FFI_PLUGIN_EXPORT void spine_skeleton_update_world_transform(spine_skeleton skeleton);
FFI_PLUGIN_EXPORT void spine_skeleton_update_world_transform_bone(spine_skeleton skeleton, spine_bone parent);
FFI_PLUGIN_EXPORT void spine_skeleton_set_to_setup_pose(spine_skeleton skeleton);
FFI_PLUGIN_EXPORT void spine_skeleton_set_bones_to_setup_pose(spine_skeleton skeleton);
FFI_PLUGIN_EXPORT void spine_skeleton_set_slots_to_setup_pose(spine_skeleton skeleton);
FFI_PLUGIN_EXPORT spine_bone spine_skeleton_find_bone(spine_skeleton skeleton, const char* boneName);
FFI_PLUGIN_EXPORT spine_slot spine_skeleton_find_slot(spine_skeleton skeleton, const char* slotName);
FFI_PLUGIN_EXPORT void spine_skeleton_set_skin_by_name(spine_skeleton skeleton, const char* skinName);
FFI_PLUGIN_EXPORT void spine_skeleton_set_skin(spine_skeleton skeleton, spine_skin skin);
FFI_PLUGIN_EXPORT spine_attachment spine_skeleton_get_attachment_by_name(spine_skeleton skeleton, const char* slotName, const char* attachmentName);
FFI_PLUGIN_EXPORT spine_attachment spine_skeleton_get_attachment(spine_skeleton skeleton, int slotIndex, const char* attachmentName);
FFI_PLUGIN_EXPORT void spine_skeleton_set_attachment(spine_skeleton skeleton, const char* slotName, const char* attachmentName);
FFI_PLUGIN_EXPORT spine_ik_constraint spine_skeleton_find_ik_constraint(spine_skeleton skeleton, const char* constraintName);
FFI_PLUGIN_EXPORT spine_transform_constraint spine_skeleton_find_transform_constraint(spine_skeleton skeleton, const char* constraintName);
FFI_PLUGIN_EXPORT spine_path_constraint spine_skeleton_find_path_constraint(spine_skeleton skeleton, const char* constraintName);
FFI_PLUGIN_EXPORT spine_bounds spine_skeleton_get_bounds(spine_skeleton skeleton);
FFI_PLUGIN_EXPORT spine_bone spine_skeleton_get_root_bone(spine_skeleton skeleton);
FFI_PLUGIN_EXPORT spine_skeleton_data spine_skeleton_get_data(spine_skeleton skeleton);
FFI_PLUGIN_EXPORT int spine_skeleton_get_num_bones(spine_skeleton skeleton);
FFI_PLUGIN_EXPORT spine_bone* spine_skeleton_get_bones(spine_skeleton skeleton);
// OMITTED getUpdateCacheList()
FFI_PLUGIN_EXPORT int spine_skeleton_get_num_slots(spine_skeleton skeleton);
FFI_PLUGIN_EXPORT spine_slot* spine_skeleton_get_slots(spine_skeleton skeleton);
FFI_PLUGIN_EXPORT int spine_skeleton_get_num_draw_order(spine_skeleton skeleton);
FFI_PLUGIN_EXPORT spine_slot* spine_skeleton_get_draw_order(spine_skeleton skeleton);
FFI_PLUGIN_EXPORT int spine_skeleton_get_num_ik_constraints(spine_skeleton skeleton);
FFI_PLUGIN_EXPORT spine_ik_constraint* spine_skeleton_get_ik_constraints(spine_skeleton skeleton);
FFI_PLUGIN_EXPORT int spine_skeleton_get_num_transform_constraints(spine_skeleton skeleton);
FFI_PLUGIN_EXPORT spine_transform_constraint* spine_skeleton_get_transform_constraints(spine_skeleton skeleton);
FFI_PLUGIN_EXPORT int spine_skeleton_get_num_path_constraints(spine_skeleton skeleton);
FFI_PLUGIN_EXPORT spine_path_constraint* spine_skeleton_get_path_constraints(spine_skeleton skeleton);
FFI_PLUGIN_EXPORT spine_skin spine_skeleton_get_skin(spine_skeleton skeleton);
FFI_PLUGIN_EXPORT spine_color spine_skeleton_get_color(spine_skeleton skeleton);
FFI_PLUGIN_EXPORT void spine_skeleton_set_color(spine_skeleton skeleton, float r, float g, float b, float a);
FFI_PLUGIN_EXPORT void spine_skeleton_set_position(spine_skeleton skeleton, float x, float y);
FFI_PLUGIN_EXPORT float spine_skeleton_get_x(spine_skeleton skeleton);
FFI_PLUGIN_EXPORT void spine_skeleton_set_x(spine_skeleton skeleton, float x);
FFI_PLUGIN_EXPORT float spine_skeleton_get_y(spine_skeleton skeleton);
FFI_PLUGIN_EXPORT void spine_skeleton_set_y(spine_skeleton skeleton, float y);
FFI_PLUGIN_EXPORT float spine_skeleton_get_scale_x(spine_skeleton skeleton);
FFI_PLUGIN_EXPORT void spine_skeleton_set_scale_x(spine_skeleton skeleton, float scaleX);
FFI_PLUGIN_EXPORT float spine_skeleton_get_scale_y(spine_skeleton skeleton);
FFI_PLUGIN_EXPORT void spine_skeleton_set_scale_y(spine_skeleton skeleton, float scaleY);

FFI_PLUGIN_EXPORT const char* spine_event_data_get_name(spine_event_data event);
FFI_PLUGIN_EXPORT int spine_event_data_get_int_value(spine_event_data event);
FFI_PLUGIN_EXPORT void spine_event_data_set_int_value(spine_event_data event, int value);
FFI_PLUGIN_EXPORT float spine_event_data_get_float_value(spine_event_data event);
FFI_PLUGIN_EXPORT void spine_event_data_set_float_value(spine_event_data event, float value);
FFI_PLUGIN_EXPORT const char* spine_event_data_get_string_value(spine_event_data event);
FFI_PLUGIN_EXPORT void spine_event_data_set_string_value(spine_event_data event, const char *value);
FFI_PLUGIN_EXPORT const char* spine_event_data_get_audio_path(spine_event_data event);
// OMITTED setAudioPath()
FFI_PLUGIN_EXPORT float spine_event_data_get_volume(spine_event_data event);
FFI_PLUGIN_EXPORT void spine_event_data_set_volume(spine_event_data event, float volume);
FFI_PLUGIN_EXPORT float spine_event_data_get_balance(spine_event_data event);
FFI_PLUGIN_EXPORT void spine_event_data_set_balance(spine_event_data event, float balance);

FFI_PLUGIN_EXPORT spine_event_data spine_event_get_data(spine_event event);
FFI_PLUGIN_EXPORT float spine_event_get_time(spine_event event);
FFI_PLUGIN_EXPORT int spine_event_get_int_value(spine_event event);
FFI_PLUGIN_EXPORT void spine_event_set_int_value(spine_event event, int value);
FFI_PLUGIN_EXPORT float spine_event_get_float_value(spine_event event);
FFI_PLUGIN_EXPORT void spine_event_set_float_value(spine_event event, float value);
FFI_PLUGIN_EXPORT const char* spine_event_get_string_value(spine_event event);
FFI_PLUGIN_EXPORT void spine_event_set_string_value(spine_event event, const char *value);
FFI_PLUGIN_EXPORT float spine_event_get_volume(spine_event event);
FFI_PLUGIN_EXPORT void spine_event_set_volume(spine_event event, float volume);
FFI_PLUGIN_EXPORT float spine_event_get_balance(spine_event event);
FFI_PLUGIN_EXPORT void spine_event_set_balance(spine_event event, float balance);

FFI_PLUGIN_EXPORT int spine_slot_data_get_index(spine_slot_data slot);
FFI_PLUGIN_EXPORT const char* spine_slot_data_get_name(spine_slot_data slot);
FFI_PLUGIN_EXPORT spine_bone_data spine_slot_data_get_bone_data(spine_slot_data slot);
FFI_PLUGIN_EXPORT spine_color spine_slot_data_get_color(spine_slot_data slot);
FFI_PLUGIN_EXPORT void spine_slot_data_set_color(spine_slot_data slot, float r, float g, float b, float a);
FFI_PLUGIN_EXPORT spine_color spine_slot_data_get_dark_color(spine_slot_data slot);
FFI_PLUGIN_EXPORT void spine_slot_data_set_dark_color(spine_slot_data slot, float r, float g, float b, float a);
FFI_PLUGIN_EXPORT int spine_slot_data_has_dark_color(spine_slot_data slot);
FFI_PLUGIN_EXPORT void spine_slot_data_set_has_dark_color(spine_slot_data slot, int hasDarkColor);
FFI_PLUGIN_EXPORT const char* spine_slot_data_get_attachment_name(spine_slot_data slot);
FFI_PLUGIN_EXPORT void spine_slot_data_set_attachment_name(spine_slot_data slot, const char *attachmentName);
FFI_PLUGIN_EXPORT spine_blend_mode spine_slot_data_get_blend_mode(spine_slot_data slot);
FFI_PLUGIN_EXPORT void spine_slot_data_set_blend_mode(spine_slot_data slot, spine_blend_mode blendMode);

FFI_PLUGIN_EXPORT void spine_slot_set_to_setup_pose(spine_slot slot);
FFI_PLUGIN_EXPORT spine_slot_data spine_slot_get_data(spine_slot slot);
FFI_PLUGIN_EXPORT spine_bone spine_slot_get_bone(spine_slot slot);
FFI_PLUGIN_EXPORT spine_skeleton spine_slot_get_skeleton(spine_slot slot);
FFI_PLUGIN_EXPORT spine_color spine_slot_get_color(spine_slot slot);
FFI_PLUGIN_EXPORT void spine_slot_set_color(spine_slot slot, float r, float g, float b, float a);
FFI_PLUGIN_EXPORT spine_color spine_slot_get_dark_color(spine_slot slot);
FFI_PLUGIN_EXPORT void spine_slot_set_dark_color(spine_slot slot, float r, float g, float b, float a);
FFI_PLUGIN_EXPORT int spine_slot_has_dark_color(spine_slot slot);
FFI_PLUGIN_EXPORT spine_attachment spine_slot_get_attachment(spine_slot slot);
FFI_PLUGIN_EXPORT void spine_slot_set_attachment(spine_slot slot, spine_attachment attachment);
// OMITTED getDeform()
FFI_PLUGIN_EXPORT int spine_slot_get_sequence_index(spine_slot slot);
FFI_PLUGIN_EXPORT void spine_slot_set_sequence_index(spine_slot slot, int sequenceIndex);


FFI_PLUGIN_EXPORT int spine_bone_data_get_index(spine_bone_data data);
FFI_PLUGIN_EXPORT const char* spine_bone_data_get_name(spine_bone_data data);
FFI_PLUGIN_EXPORT spine_bone_data spine_bone_data_get_parent(spine_bone_data data);
FFI_PLUGIN_EXPORT float spine_bone_data_get_length(spine_bone_data data);
FFI_PLUGIN_EXPORT void spine_bone_data_set_length(spine_bone_data data, float length);
FFI_PLUGIN_EXPORT float spine_bone_data_get_x(spine_bone_data data);
FFI_PLUGIN_EXPORT void spine_bone_data_set_x(spine_bone_data data, float x);
FFI_PLUGIN_EXPORT float spine_bone_data_get_y(spine_bone_data data);
FFI_PLUGIN_EXPORT void spine_bone_data_set_y(spine_bone_data data, float y);
FFI_PLUGIN_EXPORT float spine_bone_data_get_rotation(spine_bone_data data);
FFI_PLUGIN_EXPORT void spine_bone_data_set_rotation(spine_bone_data data, float rotation);
FFI_PLUGIN_EXPORT float spine_bone_data_get_scale_x(spine_bone_data data);
FFI_PLUGIN_EXPORT void spine_bone_data_set_scale_x(spine_bone_data data, float scaleX);
FFI_PLUGIN_EXPORT float spine_bone_data_get_scale_y(spine_bone_data data);
FFI_PLUGIN_EXPORT void spine_bone_data_set_scale_y(spine_bone_data data, float scaleY);
FFI_PLUGIN_EXPORT float spine_bone_data_get_shear_x(spine_bone_data data);
FFI_PLUGIN_EXPORT void spine_bone_data_set_shear_x(spine_bone_data data, float shearx);
FFI_PLUGIN_EXPORT float spine_bone_data_get_shear_y(spine_bone_data data);
FFI_PLUGIN_EXPORT void spine_bone_data_set_shear_y(spine_bone_data data, float shearY);
FFI_PLUGIN_EXPORT spine_transform_mode spine_bone_data_get_transform_mode(spine_bone_data data);
FFI_PLUGIN_EXPORT void spine_bone_data_set_transform_mode(spine_bone_data data, spine_transform_mode mode);
FFI_PLUGIN_EXPORT int spine_bone_data_is_skin_required(spine_bone_data data);
FFI_PLUGIN_EXPORT void spine_bone_data_set_is_skin_required(spine_bone_data data, int isSkinRequired);
FFI_PLUGIN_EXPORT spine_color spine_bone_data_get_color(spine_bone_data data);
FFI_PLUGIN_EXPORT void spine_bone_data_set_color(spine_bone_data data, float r, float g, float b, float a);

FFI_PLUGIN_EXPORT void spine_bone_update(spine_bone bone);
FFI_PLUGIN_EXPORT void spine_bone_update_world_transform(spine_bone bone);
FFI_PLUGIN_EXPORT void spine_bone_update_world_transform_with(spine_bone bone, float x, float y, float rotation, float scaleX, float scaleY, float shearX, float shearY);
FFI_PLUGIN_EXPORT void spine_bone_set_to_setup_pose(spine_bone bone);
FFI_PLUGIN_EXPORT spine_vector spine_bone_world_to_local(spine_bone bone, float worldX, float worldY);
FFI_PLUGIN_EXPORT spine_vector spine_bone_local_to_world(spine_bone bone, float localX, float localY);
FFI_PLUGIN_EXPORT float spine_bone_world_to_local_rotation(spine_bone bone, float worldRotation);
FFI_PLUGIN_EXPORT float spine_bone_local_to_world_rotation(spine_bone bone, float localRotation);
FFI_PLUGIN_EXPORT void spine_bone_rotate_world(spine_bone bone, float degrees);
FFI_PLUGIN_EXPORT float spine_bone_get_world_to_local_rotation_x(spine_bone bone);
FFI_PLUGIN_EXPORT float spine_bone_get_world_to_local_rotation_y(spine_bone bone);
FFI_PLUGIN_EXPORT spine_bone_data spine_bone_get_data(spine_bone bone);
FFI_PLUGIN_EXPORT spine_skeleton spine_bone_get_skeleton(spine_bone bone);
FFI_PLUGIN_EXPORT spine_bone spine_bone_get_parent(spine_bone bone);
FFI_PLUGIN_EXPORT int spine_bone_get_num_children(spine_bone bone);
FFI_PLUGIN_EXPORT spine_bone* spine_bone_get_children(spine_bone bone);
FFI_PLUGIN_EXPORT float spine_bone_get_x(spine_bone bone);
FFI_PLUGIN_EXPORT void spine_bone_set_x(spine_bone bone, float x);
FFI_PLUGIN_EXPORT float spine_bone_get_y(spine_bone bone);
FFI_PLUGIN_EXPORT void spine_bone_set_y(spine_bone bone, float y);
FFI_PLUGIN_EXPORT float spine_bone_get_rotation(spine_bone bone);
FFI_PLUGIN_EXPORT void spine_bone_set_rotation(spine_bone bone, float rotation);
FFI_PLUGIN_EXPORT float spine_bone_get_scale_x(spine_bone bone);
FFI_PLUGIN_EXPORT void spine_bone_set_scale_x(spine_bone bone, float scaleX);
FFI_PLUGIN_EXPORT float spine_bone_get_scale_y(spine_bone bone);
FFI_PLUGIN_EXPORT void spine_bone_set_scale_y(spine_bone bone, float scaleY);
FFI_PLUGIN_EXPORT float spine_bone_get_shear_x(spine_bone bone);
FFI_PLUGIN_EXPORT void spine_bone_set_shear_x(spine_bone bone, float shearX);
FFI_PLUGIN_EXPORT float spine_bone_get_shear_y(spine_bone bone);
FFI_PLUGIN_EXPORT void spine_bone_set_shear_y(spine_bone bone, float shearY);
FFI_PLUGIN_EXPORT float spine_bone_get_applied_rotation(spine_bone bone);
FFI_PLUGIN_EXPORT void spine_bone_set_applied_rotation(spine_bone bone, float rotation);
FFI_PLUGIN_EXPORT float spine_bone_get_a_x(spine_bone bone);
FFI_PLUGIN_EXPORT void spine_bone_set_a_x(spine_bone bone, float x);
FFI_PLUGIN_EXPORT float spine_bone_get_a_y(spine_bone bone);
FFI_PLUGIN_EXPORT void spine_bone_set_a_y(spine_bone bone, float y);
FFI_PLUGIN_EXPORT float spine_bone_get_a_scale_x(spine_bone bone);
FFI_PLUGIN_EXPORT void spine_bone_set_a_scale_x(spine_bone bone, float scaleX);
FFI_PLUGIN_EXPORT float spine_bone_get_a_scale_y(spine_bone bone);
FFI_PLUGIN_EXPORT void spine_bone_set_a_scale_y(spine_bone bone, float scaleY);
FFI_PLUGIN_EXPORT float spine_bone_get_a_shear_x(spine_bone bone);
FFI_PLUGIN_EXPORT void spine_bone_set_a_shear_x(spine_bone bone, float shearX);
FFI_PLUGIN_EXPORT float spine_bone_get_a_shear_y(spine_bone bone);
FFI_PLUGIN_EXPORT void spine_bone_set_a_shear_y(spine_bone bone, float shearY);
FFI_PLUGIN_EXPORT float spine_bone_get_a(spine_bone bone);
FFI_PLUGIN_EXPORT void spine_bone_set_a(spine_bone bone, float a);
FFI_PLUGIN_EXPORT float spine_bone_get_b(spine_bone bone);
FFI_PLUGIN_EXPORT void spine_bone_set_b(spine_bone bone, float b);
FFI_PLUGIN_EXPORT float spine_bone_get_c(spine_bone bone);
FFI_PLUGIN_EXPORT void spine_bone_set_c(spine_bone bone, float c);
FFI_PLUGIN_EXPORT float spine_bone_get_d(spine_bone bone);
FFI_PLUGIN_EXPORT void spine_bone_set_d(spine_bone bone, float d);
FFI_PLUGIN_EXPORT float spine_bone_get_world_x(spine_bone bone);
FFI_PLUGIN_EXPORT void spine_bone_set_world_x(spine_bone bone, float worldX);
FFI_PLUGIN_EXPORT float spine_bone_get_world_y(spine_bone bone);
FFI_PLUGIN_EXPORT void spine_bone_set_world_y(spine_bone bone, float worldY);
FFI_PLUGIN_EXPORT float spine_bone_get_world_rotation_x(spine_bone bone);
FFI_PLUGIN_EXPORT float spine_bone_get_world_rotation_y(spine_bone bone);
FFI_PLUGIN_EXPORT float spine_bone_get_world_scale_x(spine_bone bone);
FFI_PLUGIN_EXPORT float spine_bone_get_world_scale_y(spine_bone bone);
FFI_PLUGIN_EXPORT int spine_bone_get_is_active(spine_bone bone);
FFI_PLUGIN_EXPORT void spine_bone_set_is_active(spine_bone bone, int isActive);

FFI_PLUGIN_EXPORT const char* spine_attachment_get_name(spine_attachment attachment);
FFI_PLUGIN_EXPORT spine_attachment_type spine_attachment_get_type(spine_attachment attachment);
FFI_PLUGIN_EXPORT spine_attachment spine_attachment_copy(spine_attachment attachment);
FFI_PLUGIN_EXPORT void spine_attachment_dispose(spine_attachment attachment);

FFI_PLUGIN_EXPORT spine_vector spine_point_attachment_compute_world_position(spine_point_attachment attachment, spine_bone bone);
FFI_PLUGIN_EXPORT float spine_point_attachment_compute_world_rotation(spine_point_attachment attachment, spine_bone bone);
FFI_PLUGIN_EXPORT float spine_point_attachment_get_x(spine_point_attachment attachment);
FFI_PLUGIN_EXPORT void spine_point_attachment_set_x(spine_point_attachment attachment, float x);
FFI_PLUGIN_EXPORT float spine_point_attachment_get_y(spine_point_attachment attachment);
FFI_PLUGIN_EXPORT void spine_point_attachment_set_y(spine_point_attachment attachment, float y);
FFI_PLUGIN_EXPORT float spine_point_attachment_get_rotation(spine_point_attachment attachment);
FFI_PLUGIN_EXPORT void spine_point_attachment_set_rotation(spine_point_attachment attachment, float rotation);
FFI_PLUGIN_EXPORT spine_color spine_point_attachment_get_color(spine_point_attachment attachment);
FFI_PLUGIN_EXPORT void spine_point_attachment_set_color(spine_point_attachment attachment, float r, float g, float b, float a);

FFI_PLUGIN_EXPORT void spine_region_attachment_update_region(spine_region_attachment attachment);
FFI_PLUGIN_EXPORT void spine_region_attachment_compute_world_vertices(spine_region_attachment attachment, spine_slot slot, float *worldVertices);
FFI_PLUGIN_EXPORT float spine_region_attachment_get_x(spine_region_attachment attachment);
FFI_PLUGIN_EXPORT void spine_region_attachment_set_x(spine_region_attachment attachment, float x);
FFI_PLUGIN_EXPORT float spine_region_attachment_get_y(spine_region_attachment attachment);
FFI_PLUGIN_EXPORT void spine_region_attachment_set_y(spine_region_attachment attachment, float y);
FFI_PLUGIN_EXPORT float spine_region_attachment_get_rotation(spine_region_attachment attachment);
FFI_PLUGIN_EXPORT void spine_region_attachment_set_rotation(spine_region_attachment attachment, float rotation);
FFI_PLUGIN_EXPORT float spine_region_attachment_get_scale_x(spine_region_attachment attachment);
FFI_PLUGIN_EXPORT void spine_region_attachment_set_scale_x(spine_region_attachment attachment, float scaleX);
FFI_PLUGIN_EXPORT float spine_region_attachment_get_scale_y(spine_region_attachment attachment);
FFI_PLUGIN_EXPORT void spine_region_attachment_set_scale_y(spine_region_attachment attachment, float scaleY);
FFI_PLUGIN_EXPORT float spine_region_attachment_get_width(spine_region_attachment attachment);
FFI_PLUGIN_EXPORT void spine_region_attachment_set_width(spine_region_attachment attachment, float width);
FFI_PLUGIN_EXPORT float spine_region_attachment_get_height(spine_region_attachment attachment);
FFI_PLUGIN_EXPORT void spine_region_attachment_set_height(spine_region_attachment attachment, float height);
FFI_PLUGIN_EXPORT spine_color spine_region_attachment_get_color(spine_region_attachment attachment);
FFI_PLUGIN_EXPORT void spine_region_attachment_set_color(spine_region_attachment attachment, float r, float g, float b, float a);
FFI_PLUGIN_EXPORT const char *spine_region_attachment_get_path(spine_region_attachment attachment);
FFI_PLUGIN_EXPORT spine_texture_region spine_region_attachment_get_region(spine_region_attachment attachment);
FFI_PLUGIN_EXPORT spine_sequence spine_region_attachment_get_sequence(spine_region_attachment attachment);
FFI_PLUGIN_EXPORT int spine_region_attachment_get_num_offset(spine_region_attachment attachment);
FFI_PLUGIN_EXPORT float *spine_region_attachment_get_offset(spine_region_attachment attachment);
FFI_PLUGIN_EXPORT int spine_region_attachment_get_num_uvs(spine_region_attachment attachment);
FFI_PLUGIN_EXPORT float *spine_region_attachment_get_uvs(spine_region_attachment attachment);

FFI_PLUGIN_EXPORT int spine_vertex_attachment_get_world_vertices_length(spine_vertex_attachment attachment);
FFI_PLUGIN_EXPORT void spine_vertex_attachment_compute_world_vertices(spine_vertex_attachment attachment, spine_slot slot, float *worldVertices);
FFI_PLUGIN_EXPORT int spine_vertex_attachment_get_num_bones(spine_vertex_attachment attachment);
FFI_PLUGIN_EXPORT int *spine_region_attachment_get_bones(spine_region_attachment attachment);
FFI_PLUGIN_EXPORT int spine_vertex_attachment_get_num_vertices(spine_vertex_attachment attachment);
FFI_PLUGIN_EXPORT float *spine_region_attachment_get_vertices(spine_region_attachment attachment);
FFI_PLUGIN_EXPORT spine_attachment spine_vertex_attachment_get_timeline_attachment(spine_vertex_attachment timelineAttachment);
FFI_PLUGIN_EXPORT void spine_vertex_attachment_set_timeline_attachment(spine_vertex_attachment attachment, spine_attachment timelineAttachment);

FFI_PLUGIN_EXPORT void spine_mesh_attachment_update_region(spine_mesh_attachment attachment);
FFI_PLUGIN_EXPORT int spine_mesh_attachment_get_hull_length(spine_mesh_attachment attachment);
FFI_PLUGIN_EXPORT void spine_mesh_attachment_set_hull_length(spine_mesh_attachment attachment, int hullLength);
FFI_PLUGIN_EXPORT int spine_mesh_attachment_get_num_region_uvs(spine_mesh_attachment attachment);
FFI_PLUGIN_EXPORT float *spine_mesh_attachment_get_region_uvs(spine_mesh_attachment attachment);
FFI_PLUGIN_EXPORT int spine_mesh_attachment_get_num_uvs(spine_mesh_attachment attachment);
FFI_PLUGIN_EXPORT float *spine_mesh_attachment_get_uvs(spine_mesh_attachment attachment);
FFI_PLUGIN_EXPORT int spine_mesh_attachment_get_num_triangles(spine_mesh_attachment attachment);
FFI_PLUGIN_EXPORT unsigned short *spine_mesh_attachment_get_triangles(spine_mesh_attachment attachment);
FFI_PLUGIN_EXPORT spine_color spine_mesh_attachment_get_color(spine_mesh_attachment attachment);
FFI_PLUGIN_EXPORT void spine_mesh_attachment_set_color(spine_mesh_attachment attachment, float r, float g, float b, float a);
FFI_PLUGIN_EXPORT const char *spine_mesh_attachment_get_path(spine_mesh_attachment attachment);
FFI_PLUGIN_EXPORT spine_texture_region spine_mesh_attachment_get_region(spine_mesh_attachment attachment);
FFI_PLUGIN_EXPORT spine_sequence spine_mesh_attachment_get_sequence(spine_mesh_attachment attachment);
FFI_PLUGIN_EXPORT spine_mesh_attachment spine_mesh_attachment_get_parent_mesh(spine_mesh_attachment attachment);
FFI_PLUGIN_EXPORT void spine_mesh_attachment_set_parent_mesh(spine_mesh_attachment attachment, spine_mesh_attachment parentMesh);
FFI_PLUGIN_EXPORT int spine_mesh_attachment_get_num_edges(spine_mesh_attachment attachment);
FFI_PLUGIN_EXPORT unsigned short *spine_mesh_attachment_get_edges(spine_mesh_attachment attachment);
FFI_PLUGIN_EXPORT float spine_mesh_attachment_get_width(spine_mesh_attachment attachment);
FFI_PLUGIN_EXPORT void spine_mesh_attachment_set_width(spine_mesh_attachment attachment, float width);
FFI_PLUGIN_EXPORT float spine_mesh_attachment_get_height(spine_mesh_attachment attachment);
FFI_PLUGIN_EXPORT void spine_mesh_attachment_set_height(spine_mesh_attachment attachment, float height);

FFI_PLUGIN_EXPORT spine_slot_data spine_clipping_attachment_get_end_slot(spine_clipping_attachment attachment);
FFI_PLUGIN_EXPORT void spine_clipping_attachment_set_end_slot(spine_clipping_attachment attachment, spine_slot_data endSlot);
FFI_PLUGIN_EXPORT spine_color spine_clipping_attachment_get_color(spine_clipping_attachment attachment);
FFI_PLUGIN_EXPORT void spine_clipping_attachment_set_color(spine_clipping_attachment attachment, float r, float g, float b, float a);

FFI_PLUGIN_EXPORT spine_color spine_bounding_box_attachment_get_color(spine_bounding_box_attachment attachment);
FFI_PLUGIN_EXPORT void spine_bounding_box_attachment_set_color(spine_bounding_box_attachment attachment, float r, float g, float b, float a);

FFI_PLUGIN_EXPORT int spine_path_attachment_get_num_lengths(spine_path_attachment attachment);
FFI_PLUGIN_EXPORT float *spine_path_attachment_get_lengths(spine_path_attachment attachment);
FFI_PLUGIN_EXPORT int spine_path_attachment_get_is_closed(spine_path_attachment attachment);
FFI_PLUGIN_EXPORT void spine_path_attachment_set_is_closed(spine_path_attachment attachment, int isClosed);
FFI_PLUGIN_EXPORT int spine_path_attachment_get_is_constant_speed(spine_path_attachment attachment);
FFI_PLUGIN_EXPORT void spine_path_attachment_set_is_constant_speed(spine_path_attachment attachment, int isConstantSpeed);
FFI_PLUGIN_EXPORT spine_color spine_path_attachment_get_color(spine_path_attachment attachment);
FFI_PLUGIN_EXPORT void spine_path_attachment_set_color(spine_path_attachment attachment, float r, float g, float b, float a);

FFI_PLUGIN_EXPORT void spine_skin_set_attachment(spine_skin skin, int slotIndex, const char* name, spine_attachment attachment);
FFI_PLUGIN_EXPORT spine_attachment spine_skin_get_attachment(spine_skin skin, int slotIndex, const char* name);
FFI_PLUGIN_EXPORT void spine_skin_remove_attachment(spine_skin skin, int slotIndex, const char* name);
FFI_PLUGIN_EXPORT const char* spine_skin_get_name(spine_skin skin);
FFI_PLUGIN_EXPORT void spine_skin_add_skin(spine_skin skin, spine_skin other);
FFI_PLUGIN_EXPORT spine_skin_entries *spine_skin_get_entries(spine_skin skin);
FFI_PLUGIN_EXPORT void spine_skin_entries_dispose(spine_skin_entries *entries);
FFI_PLUGIN_EXPORT int spine_skin_get_num_bones(spine_skin skin);
FFI_PLUGIN_EXPORT spine_bone_data* spine_skin_get_bones(spine_skin skin);
FFI_PLUGIN_EXPORT int spine_skin_get_num_constraints(spine_skin skin);
FFI_PLUGIN_EXPORT spine_constraint_data* spine_skin_get_constraints(spine_skin skin);
FFI_PLUGIN_EXPORT spine_skin spine_skin_create(const char* name);
FFI_PLUGIN_EXPORT void spine_skin_dispose(spine_skin skin);

FFI_PLUGIN_EXPORT spine_constraint_type spine_constraint_data_get_type(spine_constraint_data data);

FFI_PLUGIN_EXPORT int spine_ik_constraint_data_get_num_bones(spine_ik_constraint_data data);
FFI_PLUGIN_EXPORT spine_bone_data* spine_ik_constraint_data_get_bones(spine_ik_constraint_data data);
FFI_PLUGIN_EXPORT spine_bone_data spine_ik_constraint_data_get_target(spine_ik_constraint_data data);
FFI_PLUGIN_EXPORT void spine_ik_constraint_data_set_target(spine_ik_constraint_data data, spine_bone_data target);
FFI_PLUGIN_EXPORT int spine_ik_constraint_data_get_bend_direction(spine_ik_constraint_data data);
FFI_PLUGIN_EXPORT void spine_ik_constraint_data_set_bend_direction(spine_ik_constraint_data data, int bendDirection);
FFI_PLUGIN_EXPORT int spine_ik_constraint_data_get_compress(spine_ik_constraint_data data);
FFI_PLUGIN_EXPORT void spine_ik_constraint_data_set_compress(spine_ik_constraint_data data, int compress);
FFI_PLUGIN_EXPORT int spine_ik_constraint_data_get_stretch(spine_ik_constraint_data data);
FFI_PLUGIN_EXPORT void spine_ik_constraint_data_set_stretch(spine_ik_constraint_data data, int stretch);
FFI_PLUGIN_EXPORT int spine_ik_constraint_data_get_uniform(spine_ik_constraint_data data);
FFI_PLUGIN_EXPORT void spine_ik_constraint_data_set_uniform(spine_ik_constraint_data data, int uniform);
FFI_PLUGIN_EXPORT float spine_ik_constraint_data_get_mix(spine_ik_constraint_data data);
FFI_PLUGIN_EXPORT void spine_ik_constraint_data_set_mix(spine_ik_constraint_data data, float mix);
FFI_PLUGIN_EXPORT float spine_ik_constraint_data_get_softness(spine_ik_constraint_data data);
FFI_PLUGIN_EXPORT void spine_ik_constraint_data_set_softness(spine_ik_constraint_data data, float softness);

FFI_PLUGIN_EXPORT void spine_ik_constraint_update(spine_ik_constraint constraint);
FFI_PLUGIN_EXPORT int spine_ik_constraint_get_order(spine_ik_constraint constraint);
FFI_PLUGIN_EXPORT spine_ik_constraint_data spine_ik_constraint_get_data(spine_ik_constraint constraint);
FFI_PLUGIN_EXPORT int spine_ik_constraint_get_num_bones(spine_ik_constraint constraint);
FFI_PLUGIN_EXPORT spine_bone* spine_ik_constraint_get_bones(spine_ik_constraint constraint);
FFI_PLUGIN_EXPORT spine_bone spine_ik_constraint_get_target(spine_ik_constraint constraint);
FFI_PLUGIN_EXPORT void spine_ik_constraint_set_target(spine_ik_constraint constraint, spine_bone target);
FFI_PLUGIN_EXPORT int spine_ik_constraint_get_bend_direction(spine_ik_constraint constraint);
FFI_PLUGIN_EXPORT void spine_ik_constraint_set_bend_direction(spine_ik_constraint constraint, int bendDirection);
FFI_PLUGIN_EXPORT int spine_ik_constraint_get_compress(spine_ik_constraint constraint);
FFI_PLUGIN_EXPORT void spine_ik_constraint_set_compress(spine_ik_constraint constraint, int compress);
FFI_PLUGIN_EXPORT int spine_ik_constraint_get_stretch(spine_ik_constraint constraint);
FFI_PLUGIN_EXPORT void spine_ik_constraint_set_stretch(spine_ik_constraint constraint, int stretch);
FFI_PLUGIN_EXPORT float spine_ik_constraint_get_mix(spine_ik_constraint constraint);
FFI_PLUGIN_EXPORT void spine_ik_constraint_set_mix(spine_ik_constraint constraint, float mix);
FFI_PLUGIN_EXPORT float spine_ik_constraint_get_softness(spine_ik_constraint constraint);
FFI_PLUGIN_EXPORT void spine_ik_constraint_set_softness(spine_ik_constraint constraint, float softness);
FFI_PLUGIN_EXPORT int spine_ik_constraint_get_is_active(spine_ik_constraint constraint);
FFI_PLUGIN_EXPORT void spine_ik_constraint_set_is_active(spine_ik_constraint constraint, int isActive);

FFI_PLUGIN_EXPORT int spine_transform_constraint_data_get_num_bones(spine_transform_constraint_data data);
FFI_PLUGIN_EXPORT spine_bone_data* spine_transform_constraint_data_get_bones(spine_transform_constraint_data data);
FFI_PLUGIN_EXPORT spine_bone_data spine_transform_constraint_data_get_target(spine_transform_constraint_data data);
FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_target(spine_transform_constraint_data data, spine_bone_data target);
FFI_PLUGIN_EXPORT float spine_transform_constraint_data_get_mix_rotate(spine_transform_constraint_data data);
FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_mix_rotate(spine_transform_constraint_data data, float mixRotate);
FFI_PLUGIN_EXPORT float spine_transform_constraint_data_get_mix_x(spine_transform_constraint_data data);
FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_mix_x(spine_transform_constraint_data data, float mixX);
FFI_PLUGIN_EXPORT float spine_transform_constraint_data_get_mix_y(spine_transform_constraint_data data);
FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_mix_y(spine_transform_constraint_data data, float mixY);
FFI_PLUGIN_EXPORT float spine_transform_constraint_data_get_mix_scale_x(spine_transform_constraint_data data);
FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_mix_scale_x(spine_transform_constraint_data data, float mixScaleX);
FFI_PLUGIN_EXPORT float spine_transform_constraint_data_get_mix_scale_y(spine_transform_constraint_data data);
FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_mix_scale_y(spine_transform_constraint_data data, float mixScaleY);
FFI_PLUGIN_EXPORT float spine_transform_constraint_data_get_mix_shear_y(spine_transform_constraint_data data);
FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_mix_shear_y(spine_transform_constraint_data data, float mixShearY);
FFI_PLUGIN_EXPORT float spine_transform_constraint_data_get_offset_rotation(spine_transform_constraint_data data);
FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_offset_rotation(spine_transform_constraint_data data, float offsetRotation);
FFI_PLUGIN_EXPORT float spine_transform_constraint_data_get_offset_x(spine_transform_constraint_data data);
FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_offset_x(spine_transform_constraint_data data, float offsetX);
FFI_PLUGIN_EXPORT float spine_transform_constraint_data_get_offset_y(spine_transform_constraint_data data);
FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_offset_y(spine_transform_constraint_data data, float offsetY);
FFI_PLUGIN_EXPORT float spine_transform_constraint_data_get_offset_scale_x(spine_transform_constraint_data data);
FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_offset_scale_x(spine_transform_constraint_data data, float offsetScaleX);
FFI_PLUGIN_EXPORT float spine_transform_constraint_data_get_offset_scale_y(spine_transform_constraint_data data);
FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_offset_scale_y(spine_transform_constraint_data data, float offsetScaleY);
FFI_PLUGIN_EXPORT float spine_transform_constraint_data_get_offset_shear_y(spine_transform_constraint_data data);
FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_offset_shear_y(spine_transform_constraint_data data, float offsetShearY);
FFI_PLUGIN_EXPORT int spine_transform_constraint_data_get_is_relative(spine_transform_constraint_data data);
FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_is_relative(spine_transform_constraint_data data, int isRelative);
FFI_PLUGIN_EXPORT int spine_transform_constraint_data_get_is_local(spine_transform_constraint_data data);
FFI_PLUGIN_EXPORT void spine_transform_constraint_data_set_is_local(spine_transform_constraint_data data, int isLocal);

FFI_PLUGIN_EXPORT void spine_transform_constraint_update(spine_transform_constraint constraint);
FFI_PLUGIN_EXPORT int spine_transform_constraint_get_order(spine_transform_constraint constraint);
FFI_PLUGIN_EXPORT spine_transform_constraint_data spine_transform_constraint_get_data(spine_transform_constraint constraint);
FFI_PLUGIN_EXPORT int spine_transform_constraint_get_num_bones(spine_transform_constraint constraint);
FFI_PLUGIN_EXPORT spine_bone* spine_transform_constraint_get_bones(spine_transform_constraint constraint);
FFI_PLUGIN_EXPORT spine_bone spine_transform_constraint_get_target(spine_transform_constraint constraint);
FFI_PLUGIN_EXPORT void spine_transform_constraint_set_target(spine_transform_constraint constraint, spine_bone target);
FFI_PLUGIN_EXPORT float spine_transform_constraint_get_mix_rotate(spine_transform_constraint constraint);
FFI_PLUGIN_EXPORT void spine_transform_constraint_set_mix_rotate(spine_transform_constraint constraint, float mixRotate);
FFI_PLUGIN_EXPORT float spine_transform_constraint_get_mix_x(spine_transform_constraint constraint);
FFI_PLUGIN_EXPORT void spine_transform_constraint_set_mix_x(spine_transform_constraint constraint, float mixX);
FFI_PLUGIN_EXPORT float spine_transform_constraint_get_mix_y(spine_transform_constraint constraint);
FFI_PLUGIN_EXPORT void spine_transform_constraint_set_mix_y(spine_transform_constraint constraint, float mixY);
FFI_PLUGIN_EXPORT float spine_transform_constraint_get_mix_scale_x(spine_transform_constraint constraint);
FFI_PLUGIN_EXPORT void spine_transform_constraint_set_mix_scale_x(spine_transform_constraint constraint, float mixScaleX);
FFI_PLUGIN_EXPORT float spine_transform_constraint_get_mix_scale_y(spine_transform_constraint constraint);
FFI_PLUGIN_EXPORT void spine_transform_constraint_set_mix_scale_y(spine_transform_constraint constraint, float mixScaleY);
FFI_PLUGIN_EXPORT float spine_transform_constraint_get_mix_shear_y(spine_transform_constraint constraint);
FFI_PLUGIN_EXPORT void spine_transform_constraint_set_mix_shear_y(spine_transform_constraint constraint, float mixShearY);
FFI_PLUGIN_EXPORT float spine_transform_constraint_get_is_active(spine_transform_constraint constraint);
FFI_PLUGIN_EXPORT void spine_transform_constraint_set_is_active(spine_transform_constraint constraint, int isActive);

FFI_PLUGIN_EXPORT int spine_path_constraint_data_get_num_bones(spine_path_constraint_data data);
FFI_PLUGIN_EXPORT spine_bone_data* spine_path_constraint_data_get_bones(spine_path_constraint_data data);
FFI_PLUGIN_EXPORT spine_slot_data spine_path_constraint_data_get_target(spine_path_constraint_data data);
FFI_PLUGIN_EXPORT void spine_path_constraint_data_set_target(spine_path_constraint_data data, spine_slot_data target);
FFI_PLUGIN_EXPORT spine_position_mode spine_path_constraint_data_get_position_mode(spine_path_constraint_data data);
FFI_PLUGIN_EXPORT void spine_path_constraint_data_set_position_mode(spine_path_constraint_data data, spine_position_mode positionMode);
FFI_PLUGIN_EXPORT spine_spacing_mode spine_path_constraint_data_get_spacing_mode(spine_path_constraint_data data);
FFI_PLUGIN_EXPORT void spine_path_constraint_data_set_spacing_mode(spine_path_constraint_data data, spine_spacing_mode spacingMode);
FFI_PLUGIN_EXPORT spine_rotate_mode spine_path_constraint_data_get_rotate_mode(spine_path_constraint_data data);
FFI_PLUGIN_EXPORT void spine_path_constraint_data_set_rotate_mode(spine_path_constraint_data data, spine_rotate_mode rotateMode);
FFI_PLUGIN_EXPORT float spine_path_constraint_data_get_offset_rotation(spine_path_constraint_data data);
FFI_PLUGIN_EXPORT void spine_path_constraint_data_set_offset_rotation(spine_path_constraint_data data, float offsetRotation);
FFI_PLUGIN_EXPORT float spine_path_constraint_data_get_position(spine_path_constraint_data data);
FFI_PLUGIN_EXPORT void spine_path_constraint_data_set_position(spine_path_constraint_data data, float position);
FFI_PLUGIN_EXPORT float spine_path_constraint_data_get_spacing(spine_path_constraint_data data);
FFI_PLUGIN_EXPORT void spine_path_constraint_data_set_spacing(spine_path_constraint_data data, float spacing);
FFI_PLUGIN_EXPORT float spine_path_constraint_data_get_mix_rotate(spine_path_constraint_data data);
FFI_PLUGIN_EXPORT void spine_path_constraint_data_set_mix_rotate(spine_path_constraint_data data, float mixRotate);
FFI_PLUGIN_EXPORT float spine_path_constraint_data_get_mix_x(spine_path_constraint_data data);
FFI_PLUGIN_EXPORT void spine_path_constraint_data_set_mix_x(spine_path_constraint_data data, float mixX);
FFI_PLUGIN_EXPORT float spine_path_constraint_data_get_mix_y(spine_path_constraint_data data);
FFI_PLUGIN_EXPORT void spine_path_constraint_data_set_mix_y(spine_path_constraint_data data, float mixY);

FFI_PLUGIN_EXPORT void spine_path_constraint_update(spine_path_constraint constraint);
FFI_PLUGIN_EXPORT int spine_path_constraint_get_order(spine_path_constraint constraint);
FFI_PLUGIN_EXPORT spine_path_constraint_data spine_path_constraint_get_data(spine_path_constraint constraint);
FFI_PLUGIN_EXPORT int spine_path_constraint_get_num_bones(spine_path_constraint constraint);
FFI_PLUGIN_EXPORT spine_bone* spine_path_constraint_get_bones(spine_path_constraint constraint);
FFI_PLUGIN_EXPORT spine_slot spine_path_constraint_get_target(spine_path_constraint constraint);
FFI_PLUGIN_EXPORT void spine_path_constraint_set_target(spine_path_constraint constraint, spine_slot target);
FFI_PLUGIN_EXPORT float spine_path_constraint_get_position(spine_path_constraint constraint);
FFI_PLUGIN_EXPORT void spine_path_constraint_set_position(spine_path_constraint constraint, float position);
FFI_PLUGIN_EXPORT float spine_path_constraint_get_spacing(spine_path_constraint constraint);
FFI_PLUGIN_EXPORT void spine_path_constraint_set_spacing(spine_path_constraint constraint, float spacing);
FFI_PLUGIN_EXPORT float spine_path_constraint_get_mix_rotate(spine_path_constraint constraint);
FFI_PLUGIN_EXPORT void spine_path_constraint_set_mix_rotate(spine_path_constraint constraint, float mixRotate);
FFI_PLUGIN_EXPORT float spine_path_constraint_get_mix_x(spine_path_constraint constraint);
FFI_PLUGIN_EXPORT void spine_path_constraint_set_mix_x(spine_path_constraint constraint, float mixX);
FFI_PLUGIN_EXPORT float spine_path_constraint_get_mix_y(spine_path_constraint constraint);
FFI_PLUGIN_EXPORT void spine_path_constraint_set_mix_y(spine_path_constraint constraint, float mixY);
FFI_PLUGIN_EXPORT int spine_path_constraint_get_is_active(spine_path_constraint constraint);
FFI_PLUGIN_EXPORT void spine_path_constraint_set_is_active(spine_path_constraint constraint, int isActive);