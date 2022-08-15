#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>

#if _WIN32
#include <windows.h>
#else
#include <pthread.h>
#include <unistd.h>
#endif

#if _WIN32
#define FFI_PLUGIN_EXPORT extern "C" __declspec(dllexport)
#else
#define FFI_PLUGIN_EXPORT extern "C"
#endif

FFI_PLUGIN_EXPORT int32_t spine_major_version();
FFI_PLUGIN_EXPORT int32_t spine_minor_version();


