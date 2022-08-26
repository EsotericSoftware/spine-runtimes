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
