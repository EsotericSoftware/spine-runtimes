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

FFI_PLUGIN_EXPORT int32_t spine_major_version();
FFI_PLUGIN_EXPORT int32_t spine_minor_version();
FFI_PLUGIN_EXPORT void spine_report_leaks();

typedef struct spine_atlas {
    void *atlas;
    char **imagePaths;
    int32_t numImagePaths;
    char *error;
} spine_atlas;

FFI_PLUGIN_EXPORT spine_atlas* spine_atlas_load(const char *atlasData);
FFI_PLUGIN_EXPORT void spine_atlas_dispose(spine_atlas *atlas);

typedef struct spine_skeleton_data {
    void *skeletonData;
    char *error;
} spine_skeleton_data;

FFI_PLUGIN_EXPORT spine_skeleton_data* spine_skeleton_data_load_json(spine_atlas *atlas, const char *skeletonData);
FFI_PLUGIN_EXPORT spine_skeleton_data* spine_skeleton_data_load_binary(spine_atlas *atlas, const unsigned char *skeletonData, int32_t length);
FFI_PLUGIN_EXPORT void spine_skeleton_data_dispose(spine_skeleton_data *skeletonData);

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

typedef struct spine_render_command {
    float *positions;
    float *uvs;
    int32_t *colors;
    int32_t numVertices;
    uint16_t *indices;
    int32_t numIndices;
    int32_t atlasPage;
    spine_blend_mode blendMode;
    struct spine_render_command *next;
} spine_render_command;

typedef void* spine_skeleton;
typedef void* spine_animation_state;
typedef void* spine_track_entry;
typedef void* spine_animation;

typedef struct spine_skeleton_drawable {
    spine_skeleton skeleton;
    spine_animation_state animationState;
    void *clipping;
    spine_render_command *renderCommand;
} spine_skeleton_drawable;

FFI_PLUGIN_EXPORT spine_skeleton_drawable *spine_skeleton_drawable_create(spine_skeleton_data *skeletonData);
FFI_PLUGIN_EXPORT void spine_skeleton_drawable_update(spine_skeleton_drawable *drawable, float deltaTime);
FFI_PLUGIN_EXPORT spine_render_command *spine_skeleton_drawable_render(spine_skeleton_drawable *drawable);
FFI_PLUGIN_EXPORT void spine_skeleton_drawable_dispose(spine_skeleton_drawable *drawable);

FFI_PLUGIN_EXPORT void spine_animation_state_update(spine_animation_state state, float delta);
FFI_PLUGIN_EXPORT void spine_animation_state_apply(spine_animation_state state, spine_skeleton skeleton);
FFI_PLUGIN_EXPORT void spine_animation_state_clear_tracks(spine_animation_state state);
FFI_PLUGIN_EXPORT void spine_animation_state_clear_track(spine_animation_state state, int32_t trackIndex);
FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_set_animation(spine_animation_state state, int32_t trackIndex, const char* animationName, int32_t loop);
FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_add_animation(spine_animation_state state, int32_t trackIndex, const char* animationName, int32_t loop, float delay);
FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_set_empty_animation(spine_animation_state state, int32_t trackIndex, float mixDuration);
FFI_PLUGIN_EXPORT spine_track_entry spine_animation_state_add_empty_animation(spine_animation_state state, int32_t trackIndex, float mixDuration, float delay);
FFI_PLUGIN_EXPORT void spine_animation_state_set_empty_animations(spine_animation_state state, float mixDuration);
FFI_PLUGIN_EXPORT float spine_animation_state_get_time_scale(spine_animation_state state);
FFI_PLUGIN_EXPORT void spine_animation_state_set_time_scale(spine_animation_state state, float timeScale);

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
