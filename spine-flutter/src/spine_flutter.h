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

typedef void* spine_skeleton;
typedef void* spine_skeleton_data;
typedef void* spine_bone;
typedef void* spine_bone_data;
typedef void* spine_slot;
typedef void* spine_slot_data;
typedef void* spine_skin;
typedef void* spine_attachment;
typedef void* spine_ik_constraint;
typedef void* spine_ik_constraint_data;
typedef void* spine_transform_constraint;
typedef void* spine_transform_constraint_data;
typedef void* spine_path_constraint;
typedef void* spine_path_constraint_data;
typedef void* spine_animation_state;
typedef void* spine_animation_state_events;
typedef void* spine_event;
typedef void* spine_event_data;
typedef void* spine_track_entry;
typedef void* spine_animation;

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

typedef struct spine_skeleton_drawable {
    spine_skeleton skeleton;
    spine_animation_state animationState;
    spine_animation_state_events animationStateEvents;
    void *clipping;
    spine_render_command *renderCommand;
} spine_skeleton_drawable;

FFI_PLUGIN_EXPORT int spine_major_version();
FFI_PLUGIN_EXPORT int spine_minor_version();
FFI_PLUGIN_EXPORT void spine_report_leaks();

FFI_PLUGIN_EXPORT spine_atlas* spine_atlas_load(const char *atlasData);
FFI_PLUGIN_EXPORT void spine_atlas_dispose(spine_atlas *atlas);

FFI_PLUGIN_EXPORT spine_skeleton_data_result spine_skeleton_data_load_json(spine_atlas *atlas, const char *skeletonData);
FFI_PLUGIN_EXPORT spine_skeleton_data_result spine_skeleton_data_load_binary(spine_atlas *atlas, const unsigned char *skeletonData, int length);
FFI_PLUGIN_EXPORT void spine_skeleton_data_dispose(spine_skeleton_data skeletonData);

FFI_PLUGIN_EXPORT spine_skeleton_drawable *spine_skeleton_drawable_create(spine_skeleton_data skeletonData);
FFI_PLUGIN_EXPORT spine_render_command *spine_skeleton_drawable_render(spine_skeleton_drawable *drawable);
FFI_PLUGIN_EXPORT void spine_skeleton_drawable_dispose(spine_skeleton_drawable *drawable);

FFI_PLUGIN_EXPORT void spine_animation_state_update(spine_animation_state state, float delta);
FFI_PLUGIN_EXPORT void spine_animation_state_dispose_track_entry(spine_animation_state state, spine_track_entry entry);
FFI_PLUGIN_EXPORT void spine_animation_state_apply(spine_animation_state state, spine_skeleton skeleton);
FFI_PLUGIN_EXPORT void spine_animation_state_clear_tracks(spine_animation_state state);
FFI_PLUGIN_EXPORT void spine_animation_state_clear_track(spine_animation_state state, int trackIndex);
FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_set_animation(spine_animation_state state, int trackIndex, const char* animationName, int loop);
FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_add_animation(spine_animation_state state, int trackIndex, const char* animationName, int loop, float delay);
FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_set_empty_animation(spine_animation_state state, int trackIndex, float mixDuration);
FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_add_empty_animation(spine_animation_state state, int trackIndex, float mixDuration, float delay);
FFI_PLUGIN_EXPORT void spine_animation_state_set_empty_animations(spine_animation_state state, float mixDuration);
FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_get_current(spine_animation_state state, int trackIndex);
FFI_PLUGIN_EXPORT float spine_animation_state_get_time_scale(spine_animation_state state);
FFI_PLUGIN_EXPORT void spine_animation_state_set_time_scale(spine_animation_state state, float timeScale);

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

FFI_PLUGIN_EXPORT void spine_skeleton_update_cache(spine_skeleton skeleton);
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