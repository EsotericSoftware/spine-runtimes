/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#ifndef SPINE_C_EXTENSIONS_H
#define SPINE_C_EXTENSIONS_H

#include "generated/bone_pose.h"
#ifdef __cplusplus
extern "C" {
#endif

#include "base.h"
#include "generated/atlas.h"
#include "generated/skeleton_data.h"

// Custom types for spine-c-new (not generated)
SPINE_OPAQUE_TYPE(spine_atlas_result)
SPINE_OPAQUE_TYPE(spine_skeleton_data_result)
SPINE_OPAQUE_TYPE(spine_skeleton_drawable)
SPINE_OPAQUE_TYPE(spine_animation_state_events)
SPINE_OPAQUE_TYPE(spine_skin_entry)
SPINE_OPAQUE_TYPE(spine_skin_entries)
SPINE_OPAQUE_TYPE(spine_texture_loader)

// Additional types
typedef void *spine_void;
typedef void (*spine_dispose_renderer_object)(void *);

// Texture loader callbacks
typedef void *(*spine_texture_loader_load_func)(const char *path);
typedef void (*spine_texture_loader_unload_func)(void *texture);

// Version functions
SPINE_C_API int32_t spine_major_version(void);
SPINE_C_API int32_t spine_minor_version(void);
SPINE_C_API void spine_enable_debug_extension(bool enable);
SPINE_C_API void spine_report_leaks(void);

// Atlas functions
SPINE_C_API spine_atlas_result spine_atlas_load(const char *atlasData);
SPINE_C_API spine_atlas_result spine_atlas_load_callback(const char *atlasData, const char *atlasDir, spine_texture_loader_load_func load,
														 spine_texture_loader_unload_func unload);
SPINE_C_API const char *spine_atlas_result_get_error(spine_atlas_result result);
SPINE_C_API spine_atlas spine_atlas_result_get_atlas(spine_atlas_result result);
SPINE_C_API void spine_atlas_result_dispose(spine_atlas_result result);

// Skeleton data functions
SPINE_C_API spine_skeleton_data_result spine_skeleton_data_load_json(spine_atlas atlas, const char *skeletonData, const char *path);
SPINE_C_API spine_skeleton_data_result spine_skeleton_data_load_binary(spine_atlas atlas, const uint8_t *skeletonData, int32_t length,
																	   const char *path);
SPINE_C_API const char *spine_skeleton_data_result_get_error(spine_skeleton_data_result result);
SPINE_C_API spine_skeleton_data spine_skeleton_data_result_get_data(spine_skeleton_data_result result);
SPINE_C_API void spine_skeleton_data_result_dispose(spine_skeleton_data_result result);

// Skeleton drawable functionsp
SPINE_C_API spine_skeleton_drawable spine_skeleton_drawable_create(spine_skeleton_data skeletonData);
SPINE_C_API void spine_skeleton_drawable_update(spine_skeleton_drawable drawable, float delta);
SPINE_C_API spine_render_command spine_skeleton_drawable_render(spine_skeleton_drawable drawable);
SPINE_C_API void spine_skeleton_drawable_dispose(spine_skeleton_drawable drawable);
SPINE_C_API spine_skeleton spine_skeleton_drawable_get_skeleton(spine_skeleton_drawable drawable);
SPINE_C_API spine_animation_state spine_skeleton_drawable_get_animation_state(spine_skeleton_drawable drawable);
SPINE_C_API spine_animation_state_data spine_skeleton_drawable_get_animation_state_data(spine_skeleton_drawable drawable);
SPINE_C_API spine_animation_state_events spine_skeleton_drawable_get_animation_state_events(spine_skeleton_drawable drawable);

// Animation state events functions
SPINE_C_API int32_t spine_animation_state_events_get_num_events(spine_animation_state_events events);
SPINE_C_API int32_t spine_animation_state_events_get_event_type(spine_animation_state_events events, int32_t index);
SPINE_C_API spine_track_entry spine_animation_state_events_get_track_entry(spine_animation_state_events events, int32_t index);
SPINE_C_API spine_event spine_animation_state_events_get_event(spine_animation_state_events events, int32_t index);
SPINE_C_API void spine_animation_state_events_reset(spine_animation_state_events events);

// Skin functions
SPINE_C_API spine_skin_entries spine_skin_get_entries(spine_skin skin);

// Skin entries functions
SPINE_C_API void spine_skin_entries_dispose(spine_skin_entries entries);
SPINE_C_API int32_t spine_skin_entries_get_num_entries(spine_skin_entries entries);
SPINE_C_API spine_skin_entry spine_skin_entries_get_entry(spine_skin_entries entries, int32_t index);

// Skin entry functions
SPINE_C_API int32_t spine_skin_entry_get_slot_index(spine_skin_entry entry);
SPINE_C_API const char *spine_skin_entry_get_name(spine_skin_entry entry);
SPINE_C_API spine_attachment spine_skin_entry_get_attachment(spine_skin_entry entry);

// Skeleton functions
SPINE_C_API void spine_skeleton_get_bounds(spine_skeleton skeleton, spine_array_float output);
SPINE_C_API void spine_skeleton_get_position_v(spine_skeleton skeleton, spine_array_float output);

// BonePose functions
SPINE_C_API void spine_bone_pose_world_to_local_v(spine_bone_pose self, float world_x, float world_y, spine_array_float output);
SPINE_C_API void spine_bone_pose_local_to_world_v(spine_bone_pose self, float local_x, float local_y, spine_array_float output);
SPINE_C_API void spine_bone_pose_world_to_parent_v(spine_bone_pose self, float world_x, float world_y, spine_array_float output);
SPINE_C_API void spine_bone_pose_parent_to_world_v(spine_bone_pose self, float parent_x, float parent_y, spine_array_float output);

// Event listener functions
typedef void (*spine_animation_state_listener)(spine_animation_state state, spine_event_type type, spine_track_entry entry, spine_event event,
											   void *user_data);

SPINE_C_API void spine_animation_state_set_listener(spine_animation_state state, spine_animation_state_listener listener, void *user_data);

SPINE_C_API void spine_track_entry_set_listener(spine_track_entry entry, spine_animation_state_listener listener, void *user_data);

#ifdef __cplusplus
}
#endif

#endif// SPINE_C_EXTENSIONS_H
