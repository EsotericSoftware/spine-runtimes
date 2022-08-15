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

