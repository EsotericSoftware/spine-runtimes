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

#ifndef SPINE_C_ARRAYS_H
#define SPINE_C_ARRAYS_H

#include "../base.h"
#include "types.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_OPAQUE_TYPE(spine_array_float)
SPINE_OPAQUE_TYPE(spine_array_int)
SPINE_OPAQUE_TYPE(spine_array_unsigned_short)
SPINE_OPAQUE_TYPE(spine_array_property_id)
SPINE_OPAQUE_TYPE(spine_array_animation)
SPINE_OPAQUE_TYPE(spine_array_atlas_page)
SPINE_OPAQUE_TYPE(spine_array_atlas_region)
SPINE_OPAQUE_TYPE(spine_array_attachment)
SPINE_OPAQUE_TYPE(spine_array_bone)
SPINE_OPAQUE_TYPE(spine_array_bone_data)
SPINE_OPAQUE_TYPE(spine_array_bone_pose)
SPINE_OPAQUE_TYPE(spine_array_bounding_box_attachment)
SPINE_OPAQUE_TYPE(spine_array_constraint)
SPINE_OPAQUE_TYPE(spine_array_constraint_data)
SPINE_OPAQUE_TYPE(spine_array_event)
SPINE_OPAQUE_TYPE(spine_array_event_data)
SPINE_OPAQUE_TYPE(spine_array_from_property)
SPINE_OPAQUE_TYPE(spine_array_physics_constraint)
SPINE_OPAQUE_TYPE(spine_array_polygon)
SPINE_OPAQUE_TYPE(spine_array_skin)
SPINE_OPAQUE_TYPE(spine_array_slot)
SPINE_OPAQUE_TYPE(spine_array_slot_data)
SPINE_OPAQUE_TYPE(spine_array_texture_region)
SPINE_OPAQUE_TYPE(spine_array_timeline)
SPINE_OPAQUE_TYPE(spine_array_to_property)
SPINE_OPAQUE_TYPE(spine_array_track_entry)
SPINE_OPAQUE_TYPE(spine_array_update)

SPINE_C_API spine_array_float spine_array_float_create(void);

SPINE_C_API spine_array_float spine_array_float_create_with_capacity(size_t initialCapacity);
SPINE_C_API void spine_array_float_dispose(spine_array_float array);
SPINE_C_API void spine_array_float_clear(spine_array_float array);

SPINE_C_API size_t spine_array_float_get_capacity(spine_array_float array);

SPINE_C_API size_t spine_array_float_size(spine_array_float array);

SPINE_C_API spine_array_float spine_array_float_set_size(spine_array_float array, size_t newSize, float defaultValue);

SPINE_C_API void spine_array_float_ensure_capacity(spine_array_float array, size_t newCapacity);

SPINE_C_API void spine_array_float_add(spine_array_float array, float inValue);

SPINE_C_API void spine_array_float_add_all(spine_array_float array, spine_array_float inValue);

SPINE_C_API void spine_array_float_clear_and_add_all(spine_array_float array, spine_array_float inValue);

SPINE_C_API void spine_array_float_remove_at(spine_array_float array, size_t inIndex);

SPINE_C_API bool spine_array_float_contains(spine_array_float array, float inValue);

SPINE_C_API int spine_array_float_index_of(spine_array_float array, float inValue);

SPINE_C_API /*@null*/ float *spine_array_float_buffer(spine_array_float array);

SPINE_C_API spine_array_int spine_array_int_create(void);

SPINE_C_API spine_array_int spine_array_int_create_with_capacity(size_t initialCapacity);
SPINE_C_API void spine_array_int_dispose(spine_array_int array);
SPINE_C_API void spine_array_int_clear(spine_array_int array);

SPINE_C_API size_t spine_array_int_get_capacity(spine_array_int array);

SPINE_C_API size_t spine_array_int_size(spine_array_int array);

SPINE_C_API spine_array_int spine_array_int_set_size(spine_array_int array, size_t newSize, int defaultValue);

SPINE_C_API void spine_array_int_ensure_capacity(spine_array_int array, size_t newCapacity);

SPINE_C_API void spine_array_int_add(spine_array_int array, int inValue);

SPINE_C_API void spine_array_int_add_all(spine_array_int array, spine_array_int inValue);

SPINE_C_API void spine_array_int_clear_and_add_all(spine_array_int array, spine_array_int inValue);

SPINE_C_API void spine_array_int_remove_at(spine_array_int array, size_t inIndex);

SPINE_C_API bool spine_array_int_contains(spine_array_int array, int inValue);

SPINE_C_API int spine_array_int_index_of(spine_array_int array, int inValue);

SPINE_C_API /*@null*/ int *spine_array_int_buffer(spine_array_int array);

SPINE_C_API spine_array_unsigned_short spine_array_unsigned_short_create(void);

SPINE_C_API spine_array_unsigned_short spine_array_unsigned_short_create_with_capacity(size_t initialCapacity);
SPINE_C_API void spine_array_unsigned_short_dispose(spine_array_unsigned_short array);
SPINE_C_API void spine_array_unsigned_short_clear(spine_array_unsigned_short array);

SPINE_C_API size_t spine_array_unsigned_short_get_capacity(spine_array_unsigned_short array);

SPINE_C_API size_t spine_array_unsigned_short_size(spine_array_unsigned_short array);

SPINE_C_API spine_array_unsigned_short spine_array_unsigned_short_set_size(spine_array_unsigned_short array, size_t newSize,
																		   unsigned short defaultValue);

SPINE_C_API void spine_array_unsigned_short_ensure_capacity(spine_array_unsigned_short array, size_t newCapacity);

SPINE_C_API void spine_array_unsigned_short_add(spine_array_unsigned_short array, unsigned short inValue);

SPINE_C_API void spine_array_unsigned_short_add_all(spine_array_unsigned_short array, spine_array_unsigned_short inValue);

SPINE_C_API void spine_array_unsigned_short_clear_and_add_all(spine_array_unsigned_short array, spine_array_unsigned_short inValue);

SPINE_C_API void spine_array_unsigned_short_remove_at(spine_array_unsigned_short array, size_t inIndex);

SPINE_C_API bool spine_array_unsigned_short_contains(spine_array_unsigned_short array, unsigned short inValue);

SPINE_C_API int spine_array_unsigned_short_index_of(spine_array_unsigned_short array, unsigned short inValue);

SPINE_C_API /*@null*/ unsigned short *spine_array_unsigned_short_buffer(spine_array_unsigned_short array);

SPINE_C_API spine_array_property_id spine_array_property_id_create(void);

SPINE_C_API spine_array_property_id spine_array_property_id_create_with_capacity(size_t initialCapacity);
SPINE_C_API void spine_array_property_id_dispose(spine_array_property_id array);
SPINE_C_API void spine_array_property_id_clear(spine_array_property_id array);

SPINE_C_API size_t spine_array_property_id_get_capacity(spine_array_property_id array);

SPINE_C_API size_t spine_array_property_id_size(spine_array_property_id array);

SPINE_C_API spine_array_property_id spine_array_property_id_set_size(spine_array_property_id array, size_t newSize, int64_t defaultValue);

SPINE_C_API void spine_array_property_id_ensure_capacity(spine_array_property_id array, size_t newCapacity);

SPINE_C_API void spine_array_property_id_add(spine_array_property_id array, int64_t inValue);

SPINE_C_API void spine_array_property_id_add_all(spine_array_property_id array, spine_array_property_id inValue);

SPINE_C_API void spine_array_property_id_clear_and_add_all(spine_array_property_id array, spine_array_property_id inValue);

SPINE_C_API void spine_array_property_id_remove_at(spine_array_property_id array, size_t inIndex);

SPINE_C_API bool spine_array_property_id_contains(spine_array_property_id array, int64_t inValue);

SPINE_C_API int spine_array_property_id_index_of(spine_array_property_id array, int64_t inValue);

SPINE_C_API /*@null*/ int64_t *spine_array_property_id_buffer(spine_array_property_id array);

SPINE_C_API spine_array_animation spine_array_animation_create(void);

SPINE_C_API spine_array_animation spine_array_animation_create_with_capacity(size_t initialCapacity);
SPINE_C_API void spine_array_animation_dispose(spine_array_animation array);
SPINE_C_API void spine_array_animation_clear(spine_array_animation array);

SPINE_C_API size_t spine_array_animation_get_capacity(spine_array_animation array);

SPINE_C_API size_t spine_array_animation_size(spine_array_animation array);

SPINE_C_API spine_array_animation spine_array_animation_set_size(spine_array_animation array, size_t newSize, spine_animation defaultValue);

SPINE_C_API void spine_array_animation_ensure_capacity(spine_array_animation array, size_t newCapacity);

SPINE_C_API void spine_array_animation_add(spine_array_animation array, spine_animation inValue);

SPINE_C_API void spine_array_animation_add_all(spine_array_animation array, spine_array_animation inValue);

SPINE_C_API void spine_array_animation_clear_and_add_all(spine_array_animation array, spine_array_animation inValue);

SPINE_C_API void spine_array_animation_remove_at(spine_array_animation array, size_t inIndex);

SPINE_C_API bool spine_array_animation_contains(spine_array_animation array, spine_animation inValue);

SPINE_C_API int spine_array_animation_index_of(spine_array_animation array, spine_animation inValue);

SPINE_C_API /*@null*/ spine_animation *spine_array_animation_buffer(spine_array_animation array);

SPINE_C_API spine_array_atlas_page spine_array_atlas_page_create(void);

SPINE_C_API spine_array_atlas_page spine_array_atlas_page_create_with_capacity(size_t initialCapacity);
SPINE_C_API void spine_array_atlas_page_dispose(spine_array_atlas_page array);
SPINE_C_API void spine_array_atlas_page_clear(spine_array_atlas_page array);

SPINE_C_API size_t spine_array_atlas_page_get_capacity(spine_array_atlas_page array);

SPINE_C_API size_t spine_array_atlas_page_size(spine_array_atlas_page array);

SPINE_C_API spine_array_atlas_page spine_array_atlas_page_set_size(spine_array_atlas_page array, size_t newSize, spine_atlas_page defaultValue);

SPINE_C_API void spine_array_atlas_page_ensure_capacity(spine_array_atlas_page array, size_t newCapacity);

SPINE_C_API void spine_array_atlas_page_add(spine_array_atlas_page array, spine_atlas_page inValue);

SPINE_C_API void spine_array_atlas_page_add_all(spine_array_atlas_page array, spine_array_atlas_page inValue);

SPINE_C_API void spine_array_atlas_page_clear_and_add_all(spine_array_atlas_page array, spine_array_atlas_page inValue);

SPINE_C_API void spine_array_atlas_page_remove_at(spine_array_atlas_page array, size_t inIndex);

SPINE_C_API bool spine_array_atlas_page_contains(spine_array_atlas_page array, spine_atlas_page inValue);

SPINE_C_API int spine_array_atlas_page_index_of(spine_array_atlas_page array, spine_atlas_page inValue);

SPINE_C_API /*@null*/ spine_atlas_page *spine_array_atlas_page_buffer(spine_array_atlas_page array);

SPINE_C_API spine_array_atlas_region spine_array_atlas_region_create(void);

SPINE_C_API spine_array_atlas_region spine_array_atlas_region_create_with_capacity(size_t initialCapacity);
SPINE_C_API void spine_array_atlas_region_dispose(spine_array_atlas_region array);
SPINE_C_API void spine_array_atlas_region_clear(spine_array_atlas_region array);

SPINE_C_API size_t spine_array_atlas_region_get_capacity(spine_array_atlas_region array);

SPINE_C_API size_t spine_array_atlas_region_size(spine_array_atlas_region array);

SPINE_C_API spine_array_atlas_region spine_array_atlas_region_set_size(spine_array_atlas_region array, size_t newSize,
																	   spine_atlas_region defaultValue);

SPINE_C_API void spine_array_atlas_region_ensure_capacity(spine_array_atlas_region array, size_t newCapacity);

SPINE_C_API void spine_array_atlas_region_add(spine_array_atlas_region array, spine_atlas_region inValue);

SPINE_C_API void spine_array_atlas_region_add_all(spine_array_atlas_region array, spine_array_atlas_region inValue);

SPINE_C_API void spine_array_atlas_region_clear_and_add_all(spine_array_atlas_region array, spine_array_atlas_region inValue);

SPINE_C_API void spine_array_atlas_region_remove_at(spine_array_atlas_region array, size_t inIndex);

SPINE_C_API bool spine_array_atlas_region_contains(spine_array_atlas_region array, spine_atlas_region inValue);

SPINE_C_API int spine_array_atlas_region_index_of(spine_array_atlas_region array, spine_atlas_region inValue);

SPINE_C_API /*@null*/ spine_atlas_region *spine_array_atlas_region_buffer(spine_array_atlas_region array);

SPINE_C_API spine_array_attachment spine_array_attachment_create(void);

SPINE_C_API spine_array_attachment spine_array_attachment_create_with_capacity(size_t initialCapacity);
SPINE_C_API void spine_array_attachment_dispose(spine_array_attachment array);
SPINE_C_API void spine_array_attachment_clear(spine_array_attachment array);

SPINE_C_API size_t spine_array_attachment_get_capacity(spine_array_attachment array);

SPINE_C_API size_t spine_array_attachment_size(spine_array_attachment array);

SPINE_C_API spine_array_attachment spine_array_attachment_set_size(spine_array_attachment array, size_t newSize, spine_attachment defaultValue);

SPINE_C_API void spine_array_attachment_ensure_capacity(spine_array_attachment array, size_t newCapacity);

SPINE_C_API void spine_array_attachment_add(spine_array_attachment array, spine_attachment inValue);

SPINE_C_API void spine_array_attachment_add_all(spine_array_attachment array, spine_array_attachment inValue);

SPINE_C_API void spine_array_attachment_clear_and_add_all(spine_array_attachment array, spine_array_attachment inValue);

SPINE_C_API void spine_array_attachment_remove_at(spine_array_attachment array, size_t inIndex);

SPINE_C_API bool spine_array_attachment_contains(spine_array_attachment array, spine_attachment inValue);

SPINE_C_API int spine_array_attachment_index_of(spine_array_attachment array, spine_attachment inValue);

SPINE_C_API /*@null*/ spine_attachment *spine_array_attachment_buffer(spine_array_attachment array);

SPINE_C_API spine_array_bone spine_array_bone_create(void);

SPINE_C_API spine_array_bone spine_array_bone_create_with_capacity(size_t initialCapacity);
SPINE_C_API void spine_array_bone_dispose(spine_array_bone array);
SPINE_C_API void spine_array_bone_clear(spine_array_bone array);

SPINE_C_API size_t spine_array_bone_get_capacity(spine_array_bone array);

SPINE_C_API size_t spine_array_bone_size(spine_array_bone array);

SPINE_C_API spine_array_bone spine_array_bone_set_size(spine_array_bone array, size_t newSize, spine_bone defaultValue);

SPINE_C_API void spine_array_bone_ensure_capacity(spine_array_bone array, size_t newCapacity);

SPINE_C_API void spine_array_bone_add(spine_array_bone array, spine_bone inValue);

SPINE_C_API void spine_array_bone_add_all(spine_array_bone array, spine_array_bone inValue);

SPINE_C_API void spine_array_bone_clear_and_add_all(spine_array_bone array, spine_array_bone inValue);

SPINE_C_API void spine_array_bone_remove_at(spine_array_bone array, size_t inIndex);

SPINE_C_API bool spine_array_bone_contains(spine_array_bone array, spine_bone inValue);

SPINE_C_API int spine_array_bone_index_of(spine_array_bone array, spine_bone inValue);

SPINE_C_API /*@null*/ spine_bone *spine_array_bone_buffer(spine_array_bone array);

SPINE_C_API spine_array_bone_data spine_array_bone_data_create(void);

SPINE_C_API spine_array_bone_data spine_array_bone_data_create_with_capacity(size_t initialCapacity);
SPINE_C_API void spine_array_bone_data_dispose(spine_array_bone_data array);
SPINE_C_API void spine_array_bone_data_clear(spine_array_bone_data array);

SPINE_C_API size_t spine_array_bone_data_get_capacity(spine_array_bone_data array);

SPINE_C_API size_t spine_array_bone_data_size(spine_array_bone_data array);

SPINE_C_API spine_array_bone_data spine_array_bone_data_set_size(spine_array_bone_data array, size_t newSize, spine_bone_data defaultValue);

SPINE_C_API void spine_array_bone_data_ensure_capacity(spine_array_bone_data array, size_t newCapacity);

SPINE_C_API void spine_array_bone_data_add(spine_array_bone_data array, spine_bone_data inValue);

SPINE_C_API void spine_array_bone_data_add_all(spine_array_bone_data array, spine_array_bone_data inValue);

SPINE_C_API void spine_array_bone_data_clear_and_add_all(spine_array_bone_data array, spine_array_bone_data inValue);

SPINE_C_API void spine_array_bone_data_remove_at(spine_array_bone_data array, size_t inIndex);

SPINE_C_API bool spine_array_bone_data_contains(spine_array_bone_data array, spine_bone_data inValue);

SPINE_C_API int spine_array_bone_data_index_of(spine_array_bone_data array, spine_bone_data inValue);

SPINE_C_API /*@null*/ spine_bone_data *spine_array_bone_data_buffer(spine_array_bone_data array);

SPINE_C_API spine_array_bone_pose spine_array_bone_pose_create(void);

SPINE_C_API spine_array_bone_pose spine_array_bone_pose_create_with_capacity(size_t initialCapacity);
SPINE_C_API void spine_array_bone_pose_dispose(spine_array_bone_pose array);
SPINE_C_API void spine_array_bone_pose_clear(spine_array_bone_pose array);

SPINE_C_API size_t spine_array_bone_pose_get_capacity(spine_array_bone_pose array);

SPINE_C_API size_t spine_array_bone_pose_size(spine_array_bone_pose array);

SPINE_C_API spine_array_bone_pose spine_array_bone_pose_set_size(spine_array_bone_pose array, size_t newSize, spine_bone_pose defaultValue);

SPINE_C_API void spine_array_bone_pose_ensure_capacity(spine_array_bone_pose array, size_t newCapacity);

SPINE_C_API void spine_array_bone_pose_add(spine_array_bone_pose array, spine_bone_pose inValue);

SPINE_C_API void spine_array_bone_pose_add_all(spine_array_bone_pose array, spine_array_bone_pose inValue);

SPINE_C_API void spine_array_bone_pose_clear_and_add_all(spine_array_bone_pose array, spine_array_bone_pose inValue);

SPINE_C_API void spine_array_bone_pose_remove_at(spine_array_bone_pose array, size_t inIndex);

SPINE_C_API bool spine_array_bone_pose_contains(spine_array_bone_pose array, spine_bone_pose inValue);

SPINE_C_API int spine_array_bone_pose_index_of(spine_array_bone_pose array, spine_bone_pose inValue);

SPINE_C_API /*@null*/ spine_bone_pose *spine_array_bone_pose_buffer(spine_array_bone_pose array);

SPINE_C_API spine_array_bounding_box_attachment spine_array_bounding_box_attachment_create(void);

SPINE_C_API spine_array_bounding_box_attachment spine_array_bounding_box_attachment_create_with_capacity(size_t initialCapacity);
SPINE_C_API void spine_array_bounding_box_attachment_dispose(spine_array_bounding_box_attachment array);
SPINE_C_API void spine_array_bounding_box_attachment_clear(spine_array_bounding_box_attachment array);

SPINE_C_API size_t spine_array_bounding_box_attachment_get_capacity(spine_array_bounding_box_attachment array);

SPINE_C_API size_t spine_array_bounding_box_attachment_size(spine_array_bounding_box_attachment array);

SPINE_C_API spine_array_bounding_box_attachment spine_array_bounding_box_attachment_set_size(spine_array_bounding_box_attachment array,
																							 size_t newSize,
																							 spine_bounding_box_attachment defaultValue);

SPINE_C_API void spine_array_bounding_box_attachment_ensure_capacity(spine_array_bounding_box_attachment array, size_t newCapacity);

SPINE_C_API void spine_array_bounding_box_attachment_add(spine_array_bounding_box_attachment array, spine_bounding_box_attachment inValue);

SPINE_C_API void spine_array_bounding_box_attachment_add_all(spine_array_bounding_box_attachment array, spine_array_bounding_box_attachment inValue);

SPINE_C_API void spine_array_bounding_box_attachment_clear_and_add_all(spine_array_bounding_box_attachment array,
																	   spine_array_bounding_box_attachment inValue);

SPINE_C_API void spine_array_bounding_box_attachment_remove_at(spine_array_bounding_box_attachment array, size_t inIndex);

SPINE_C_API bool spine_array_bounding_box_attachment_contains(spine_array_bounding_box_attachment array, spine_bounding_box_attachment inValue);

SPINE_C_API int spine_array_bounding_box_attachment_index_of(spine_array_bounding_box_attachment array, spine_bounding_box_attachment inValue);

SPINE_C_API /*@null*/ spine_bounding_box_attachment *spine_array_bounding_box_attachment_buffer(spine_array_bounding_box_attachment array);

SPINE_C_API spine_array_constraint spine_array_constraint_create(void);

SPINE_C_API spine_array_constraint spine_array_constraint_create_with_capacity(size_t initialCapacity);
SPINE_C_API void spine_array_constraint_dispose(spine_array_constraint array);
SPINE_C_API void spine_array_constraint_clear(spine_array_constraint array);

SPINE_C_API size_t spine_array_constraint_get_capacity(spine_array_constraint array);

SPINE_C_API size_t spine_array_constraint_size(spine_array_constraint array);

SPINE_C_API spine_array_constraint spine_array_constraint_set_size(spine_array_constraint array, size_t newSize, spine_constraint defaultValue);

SPINE_C_API void spine_array_constraint_ensure_capacity(spine_array_constraint array, size_t newCapacity);

SPINE_C_API void spine_array_constraint_add(spine_array_constraint array, spine_constraint inValue);

SPINE_C_API void spine_array_constraint_add_all(spine_array_constraint array, spine_array_constraint inValue);

SPINE_C_API void spine_array_constraint_clear_and_add_all(spine_array_constraint array, spine_array_constraint inValue);

SPINE_C_API void spine_array_constraint_remove_at(spine_array_constraint array, size_t inIndex);

SPINE_C_API bool spine_array_constraint_contains(spine_array_constraint array, spine_constraint inValue);

SPINE_C_API int spine_array_constraint_index_of(spine_array_constraint array, spine_constraint inValue);

SPINE_C_API /*@null*/ spine_constraint *spine_array_constraint_buffer(spine_array_constraint array);

SPINE_C_API spine_array_constraint_data spine_array_constraint_data_create(void);

SPINE_C_API spine_array_constraint_data spine_array_constraint_data_create_with_capacity(size_t initialCapacity);
SPINE_C_API void spine_array_constraint_data_dispose(spine_array_constraint_data array);
SPINE_C_API void spine_array_constraint_data_clear(spine_array_constraint_data array);

SPINE_C_API size_t spine_array_constraint_data_get_capacity(spine_array_constraint_data array);

SPINE_C_API size_t spine_array_constraint_data_size(spine_array_constraint_data array);

SPINE_C_API spine_array_constraint_data spine_array_constraint_data_set_size(spine_array_constraint_data array, size_t newSize,
																			 spine_constraint_data defaultValue);

SPINE_C_API void spine_array_constraint_data_ensure_capacity(spine_array_constraint_data array, size_t newCapacity);

SPINE_C_API void spine_array_constraint_data_add(spine_array_constraint_data array, spine_constraint_data inValue);

SPINE_C_API void spine_array_constraint_data_add_all(spine_array_constraint_data array, spine_array_constraint_data inValue);

SPINE_C_API void spine_array_constraint_data_clear_and_add_all(spine_array_constraint_data array, spine_array_constraint_data inValue);

SPINE_C_API void spine_array_constraint_data_remove_at(spine_array_constraint_data array, size_t inIndex);

SPINE_C_API bool spine_array_constraint_data_contains(spine_array_constraint_data array, spine_constraint_data inValue);

SPINE_C_API int spine_array_constraint_data_index_of(spine_array_constraint_data array, spine_constraint_data inValue);

SPINE_C_API /*@null*/ spine_constraint_data *spine_array_constraint_data_buffer(spine_array_constraint_data array);

SPINE_C_API spine_array_event spine_array_event_create(void);

SPINE_C_API spine_array_event spine_array_event_create_with_capacity(size_t initialCapacity);
SPINE_C_API void spine_array_event_dispose(spine_array_event array);
SPINE_C_API void spine_array_event_clear(spine_array_event array);

SPINE_C_API size_t spine_array_event_get_capacity(spine_array_event array);

SPINE_C_API size_t spine_array_event_size(spine_array_event array);

SPINE_C_API spine_array_event spine_array_event_set_size(spine_array_event array, size_t newSize, spine_event defaultValue);

SPINE_C_API void spine_array_event_ensure_capacity(spine_array_event array, size_t newCapacity);

SPINE_C_API void spine_array_event_add(spine_array_event array, spine_event inValue);

SPINE_C_API void spine_array_event_add_all(spine_array_event array, spine_array_event inValue);

SPINE_C_API void spine_array_event_clear_and_add_all(spine_array_event array, spine_array_event inValue);

SPINE_C_API void spine_array_event_remove_at(spine_array_event array, size_t inIndex);

SPINE_C_API bool spine_array_event_contains(spine_array_event array, spine_event inValue);

SPINE_C_API int spine_array_event_index_of(spine_array_event array, spine_event inValue);

SPINE_C_API /*@null*/ spine_event *spine_array_event_buffer(spine_array_event array);

SPINE_C_API spine_array_event_data spine_array_event_data_create(void);

SPINE_C_API spine_array_event_data spine_array_event_data_create_with_capacity(size_t initialCapacity);
SPINE_C_API void spine_array_event_data_dispose(spine_array_event_data array);
SPINE_C_API void spine_array_event_data_clear(spine_array_event_data array);

SPINE_C_API size_t spine_array_event_data_get_capacity(spine_array_event_data array);

SPINE_C_API size_t spine_array_event_data_size(spine_array_event_data array);

SPINE_C_API spine_array_event_data spine_array_event_data_set_size(spine_array_event_data array, size_t newSize, spine_event_data defaultValue);

SPINE_C_API void spine_array_event_data_ensure_capacity(spine_array_event_data array, size_t newCapacity);

SPINE_C_API void spine_array_event_data_add(spine_array_event_data array, spine_event_data inValue);

SPINE_C_API void spine_array_event_data_add_all(spine_array_event_data array, spine_array_event_data inValue);

SPINE_C_API void spine_array_event_data_clear_and_add_all(spine_array_event_data array, spine_array_event_data inValue);

SPINE_C_API void spine_array_event_data_remove_at(spine_array_event_data array, size_t inIndex);

SPINE_C_API bool spine_array_event_data_contains(spine_array_event_data array, spine_event_data inValue);

SPINE_C_API int spine_array_event_data_index_of(spine_array_event_data array, spine_event_data inValue);

SPINE_C_API /*@null*/ spine_event_data *spine_array_event_data_buffer(spine_array_event_data array);

SPINE_C_API spine_array_from_property spine_array_from_property_create(void);

SPINE_C_API spine_array_from_property spine_array_from_property_create_with_capacity(size_t initialCapacity);
SPINE_C_API void spine_array_from_property_dispose(spine_array_from_property array);
SPINE_C_API void spine_array_from_property_clear(spine_array_from_property array);

SPINE_C_API size_t spine_array_from_property_get_capacity(spine_array_from_property array);

SPINE_C_API size_t spine_array_from_property_size(spine_array_from_property array);

SPINE_C_API spine_array_from_property spine_array_from_property_set_size(spine_array_from_property array, size_t newSize,
																		 spine_from_property defaultValue);

SPINE_C_API void spine_array_from_property_ensure_capacity(spine_array_from_property array, size_t newCapacity);

SPINE_C_API void spine_array_from_property_add(spine_array_from_property array, spine_from_property inValue);

SPINE_C_API void spine_array_from_property_add_all(spine_array_from_property array, spine_array_from_property inValue);

SPINE_C_API void spine_array_from_property_clear_and_add_all(spine_array_from_property array, spine_array_from_property inValue);

SPINE_C_API void spine_array_from_property_remove_at(spine_array_from_property array, size_t inIndex);

SPINE_C_API bool spine_array_from_property_contains(spine_array_from_property array, spine_from_property inValue);

SPINE_C_API int spine_array_from_property_index_of(spine_array_from_property array, spine_from_property inValue);

SPINE_C_API /*@null*/ spine_from_property *spine_array_from_property_buffer(spine_array_from_property array);

SPINE_C_API spine_array_physics_constraint spine_array_physics_constraint_create(void);

SPINE_C_API spine_array_physics_constraint spine_array_physics_constraint_create_with_capacity(size_t initialCapacity);
SPINE_C_API void spine_array_physics_constraint_dispose(spine_array_physics_constraint array);
SPINE_C_API void spine_array_physics_constraint_clear(spine_array_physics_constraint array);

SPINE_C_API size_t spine_array_physics_constraint_get_capacity(spine_array_physics_constraint array);

SPINE_C_API size_t spine_array_physics_constraint_size(spine_array_physics_constraint array);

SPINE_C_API spine_array_physics_constraint spine_array_physics_constraint_set_size(spine_array_physics_constraint array, size_t newSize,
																				   spine_physics_constraint defaultValue);

SPINE_C_API void spine_array_physics_constraint_ensure_capacity(spine_array_physics_constraint array, size_t newCapacity);

SPINE_C_API void spine_array_physics_constraint_add(spine_array_physics_constraint array, spine_physics_constraint inValue);

SPINE_C_API void spine_array_physics_constraint_add_all(spine_array_physics_constraint array, spine_array_physics_constraint inValue);

SPINE_C_API void spine_array_physics_constraint_clear_and_add_all(spine_array_physics_constraint array, spine_array_physics_constraint inValue);

SPINE_C_API void spine_array_physics_constraint_remove_at(spine_array_physics_constraint array, size_t inIndex);

SPINE_C_API bool spine_array_physics_constraint_contains(spine_array_physics_constraint array, spine_physics_constraint inValue);

SPINE_C_API int spine_array_physics_constraint_index_of(spine_array_physics_constraint array, spine_physics_constraint inValue);

SPINE_C_API /*@null*/ spine_physics_constraint *spine_array_physics_constraint_buffer(spine_array_physics_constraint array);

SPINE_C_API spine_array_polygon spine_array_polygon_create(void);

SPINE_C_API spine_array_polygon spine_array_polygon_create_with_capacity(size_t initialCapacity);
SPINE_C_API void spine_array_polygon_dispose(spine_array_polygon array);
SPINE_C_API void spine_array_polygon_clear(spine_array_polygon array);

SPINE_C_API size_t spine_array_polygon_get_capacity(spine_array_polygon array);

SPINE_C_API size_t spine_array_polygon_size(spine_array_polygon array);

SPINE_C_API spine_array_polygon spine_array_polygon_set_size(spine_array_polygon array, size_t newSize, spine_polygon defaultValue);

SPINE_C_API void spine_array_polygon_ensure_capacity(spine_array_polygon array, size_t newCapacity);

SPINE_C_API void spine_array_polygon_add(spine_array_polygon array, spine_polygon inValue);

SPINE_C_API void spine_array_polygon_add_all(spine_array_polygon array, spine_array_polygon inValue);

SPINE_C_API void spine_array_polygon_clear_and_add_all(spine_array_polygon array, spine_array_polygon inValue);

SPINE_C_API void spine_array_polygon_remove_at(spine_array_polygon array, size_t inIndex);

SPINE_C_API bool spine_array_polygon_contains(spine_array_polygon array, spine_polygon inValue);

SPINE_C_API int spine_array_polygon_index_of(spine_array_polygon array, spine_polygon inValue);

SPINE_C_API /*@null*/ spine_polygon *spine_array_polygon_buffer(spine_array_polygon array);

SPINE_C_API spine_array_skin spine_array_skin_create(void);

SPINE_C_API spine_array_skin spine_array_skin_create_with_capacity(size_t initialCapacity);
SPINE_C_API void spine_array_skin_dispose(spine_array_skin array);
SPINE_C_API void spine_array_skin_clear(spine_array_skin array);

SPINE_C_API size_t spine_array_skin_get_capacity(spine_array_skin array);

SPINE_C_API size_t spine_array_skin_size(spine_array_skin array);

SPINE_C_API spine_array_skin spine_array_skin_set_size(spine_array_skin array, size_t newSize, spine_skin defaultValue);

SPINE_C_API void spine_array_skin_ensure_capacity(spine_array_skin array, size_t newCapacity);

SPINE_C_API void spine_array_skin_add(spine_array_skin array, spine_skin inValue);

SPINE_C_API void spine_array_skin_add_all(spine_array_skin array, spine_array_skin inValue);

SPINE_C_API void spine_array_skin_clear_and_add_all(spine_array_skin array, spine_array_skin inValue);

SPINE_C_API void spine_array_skin_remove_at(spine_array_skin array, size_t inIndex);

SPINE_C_API bool spine_array_skin_contains(spine_array_skin array, spine_skin inValue);

SPINE_C_API int spine_array_skin_index_of(spine_array_skin array, spine_skin inValue);

SPINE_C_API /*@null*/ spine_skin *spine_array_skin_buffer(spine_array_skin array);

SPINE_C_API spine_array_slot spine_array_slot_create(void);

SPINE_C_API spine_array_slot spine_array_slot_create_with_capacity(size_t initialCapacity);
SPINE_C_API void spine_array_slot_dispose(spine_array_slot array);
SPINE_C_API void spine_array_slot_clear(spine_array_slot array);

SPINE_C_API size_t spine_array_slot_get_capacity(spine_array_slot array);

SPINE_C_API size_t spine_array_slot_size(spine_array_slot array);

SPINE_C_API spine_array_slot spine_array_slot_set_size(spine_array_slot array, size_t newSize, spine_slot defaultValue);

SPINE_C_API void spine_array_slot_ensure_capacity(spine_array_slot array, size_t newCapacity);

SPINE_C_API void spine_array_slot_add(spine_array_slot array, spine_slot inValue);

SPINE_C_API void spine_array_slot_add_all(spine_array_slot array, spine_array_slot inValue);

SPINE_C_API void spine_array_slot_clear_and_add_all(spine_array_slot array, spine_array_slot inValue);

SPINE_C_API void spine_array_slot_remove_at(spine_array_slot array, size_t inIndex);

SPINE_C_API bool spine_array_slot_contains(spine_array_slot array, spine_slot inValue);

SPINE_C_API int spine_array_slot_index_of(spine_array_slot array, spine_slot inValue);

SPINE_C_API /*@null*/ spine_slot *spine_array_slot_buffer(spine_array_slot array);

SPINE_C_API spine_array_slot_data spine_array_slot_data_create(void);

SPINE_C_API spine_array_slot_data spine_array_slot_data_create_with_capacity(size_t initialCapacity);
SPINE_C_API void spine_array_slot_data_dispose(spine_array_slot_data array);
SPINE_C_API void spine_array_slot_data_clear(spine_array_slot_data array);

SPINE_C_API size_t spine_array_slot_data_get_capacity(spine_array_slot_data array);

SPINE_C_API size_t spine_array_slot_data_size(spine_array_slot_data array);

SPINE_C_API spine_array_slot_data spine_array_slot_data_set_size(spine_array_slot_data array, size_t newSize, spine_slot_data defaultValue);

SPINE_C_API void spine_array_slot_data_ensure_capacity(spine_array_slot_data array, size_t newCapacity);

SPINE_C_API void spine_array_slot_data_add(spine_array_slot_data array, spine_slot_data inValue);

SPINE_C_API void spine_array_slot_data_add_all(spine_array_slot_data array, spine_array_slot_data inValue);

SPINE_C_API void spine_array_slot_data_clear_and_add_all(spine_array_slot_data array, spine_array_slot_data inValue);

SPINE_C_API void spine_array_slot_data_remove_at(spine_array_slot_data array, size_t inIndex);

SPINE_C_API bool spine_array_slot_data_contains(spine_array_slot_data array, spine_slot_data inValue);

SPINE_C_API int spine_array_slot_data_index_of(spine_array_slot_data array, spine_slot_data inValue);

SPINE_C_API /*@null*/ spine_slot_data *spine_array_slot_data_buffer(spine_array_slot_data array);

SPINE_C_API spine_array_texture_region spine_array_texture_region_create(void);

SPINE_C_API spine_array_texture_region spine_array_texture_region_create_with_capacity(size_t initialCapacity);
SPINE_C_API void spine_array_texture_region_dispose(spine_array_texture_region array);
SPINE_C_API void spine_array_texture_region_clear(spine_array_texture_region array);

SPINE_C_API size_t spine_array_texture_region_get_capacity(spine_array_texture_region array);

SPINE_C_API size_t spine_array_texture_region_size(spine_array_texture_region array);

SPINE_C_API spine_array_texture_region spine_array_texture_region_set_size(spine_array_texture_region array, size_t newSize,
																		   spine_texture_region defaultValue);

SPINE_C_API void spine_array_texture_region_ensure_capacity(spine_array_texture_region array, size_t newCapacity);

SPINE_C_API void spine_array_texture_region_add(spine_array_texture_region array, spine_texture_region inValue);

SPINE_C_API void spine_array_texture_region_add_all(spine_array_texture_region array, spine_array_texture_region inValue);

SPINE_C_API void spine_array_texture_region_clear_and_add_all(spine_array_texture_region array, spine_array_texture_region inValue);

SPINE_C_API void spine_array_texture_region_remove_at(spine_array_texture_region array, size_t inIndex);

SPINE_C_API bool spine_array_texture_region_contains(spine_array_texture_region array, spine_texture_region inValue);

SPINE_C_API int spine_array_texture_region_index_of(spine_array_texture_region array, spine_texture_region inValue);

SPINE_C_API /*@null*/ spine_texture_region *spine_array_texture_region_buffer(spine_array_texture_region array);

SPINE_C_API spine_array_timeline spine_array_timeline_create(void);

SPINE_C_API spine_array_timeline spine_array_timeline_create_with_capacity(size_t initialCapacity);
SPINE_C_API void spine_array_timeline_dispose(spine_array_timeline array);
SPINE_C_API void spine_array_timeline_clear(spine_array_timeline array);

SPINE_C_API size_t spine_array_timeline_get_capacity(spine_array_timeline array);

SPINE_C_API size_t spine_array_timeline_size(spine_array_timeline array);

SPINE_C_API spine_array_timeline spine_array_timeline_set_size(spine_array_timeline array, size_t newSize, spine_timeline defaultValue);

SPINE_C_API void spine_array_timeline_ensure_capacity(spine_array_timeline array, size_t newCapacity);

SPINE_C_API void spine_array_timeline_add(spine_array_timeline array, spine_timeline inValue);

SPINE_C_API void spine_array_timeline_add_all(spine_array_timeline array, spine_array_timeline inValue);

SPINE_C_API void spine_array_timeline_clear_and_add_all(spine_array_timeline array, spine_array_timeline inValue);

SPINE_C_API void spine_array_timeline_remove_at(spine_array_timeline array, size_t inIndex);

SPINE_C_API bool spine_array_timeline_contains(spine_array_timeline array, spine_timeline inValue);

SPINE_C_API int spine_array_timeline_index_of(spine_array_timeline array, spine_timeline inValue);

SPINE_C_API /*@null*/ spine_timeline *spine_array_timeline_buffer(spine_array_timeline array);

SPINE_C_API spine_array_to_property spine_array_to_property_create(void);

SPINE_C_API spine_array_to_property spine_array_to_property_create_with_capacity(size_t initialCapacity);
SPINE_C_API void spine_array_to_property_dispose(spine_array_to_property array);
SPINE_C_API void spine_array_to_property_clear(spine_array_to_property array);

SPINE_C_API size_t spine_array_to_property_get_capacity(spine_array_to_property array);

SPINE_C_API size_t spine_array_to_property_size(spine_array_to_property array);

SPINE_C_API spine_array_to_property spine_array_to_property_set_size(spine_array_to_property array, size_t newSize, spine_to_property defaultValue);

SPINE_C_API void spine_array_to_property_ensure_capacity(spine_array_to_property array, size_t newCapacity);

SPINE_C_API void spine_array_to_property_add(spine_array_to_property array, spine_to_property inValue);

SPINE_C_API void spine_array_to_property_add_all(spine_array_to_property array, spine_array_to_property inValue);

SPINE_C_API void spine_array_to_property_clear_and_add_all(spine_array_to_property array, spine_array_to_property inValue);

SPINE_C_API void spine_array_to_property_remove_at(spine_array_to_property array, size_t inIndex);

SPINE_C_API bool spine_array_to_property_contains(spine_array_to_property array, spine_to_property inValue);

SPINE_C_API int spine_array_to_property_index_of(spine_array_to_property array, spine_to_property inValue);

SPINE_C_API /*@null*/ spine_to_property *spine_array_to_property_buffer(spine_array_to_property array);

SPINE_C_API spine_array_track_entry spine_array_track_entry_create(void);

SPINE_C_API spine_array_track_entry spine_array_track_entry_create_with_capacity(size_t initialCapacity);
SPINE_C_API void spine_array_track_entry_dispose(spine_array_track_entry array);
SPINE_C_API void spine_array_track_entry_clear(spine_array_track_entry array);

SPINE_C_API size_t spine_array_track_entry_get_capacity(spine_array_track_entry array);

SPINE_C_API size_t spine_array_track_entry_size(spine_array_track_entry array);

SPINE_C_API spine_array_track_entry spine_array_track_entry_set_size(spine_array_track_entry array, size_t newSize, spine_track_entry defaultValue);

SPINE_C_API void spine_array_track_entry_ensure_capacity(spine_array_track_entry array, size_t newCapacity);

SPINE_C_API void spine_array_track_entry_add(spine_array_track_entry array, spine_track_entry inValue);

SPINE_C_API void spine_array_track_entry_add_all(spine_array_track_entry array, spine_array_track_entry inValue);

SPINE_C_API void spine_array_track_entry_clear_and_add_all(spine_array_track_entry array, spine_array_track_entry inValue);

SPINE_C_API void spine_array_track_entry_remove_at(spine_array_track_entry array, size_t inIndex);

SPINE_C_API bool spine_array_track_entry_contains(spine_array_track_entry array, spine_track_entry inValue);

SPINE_C_API int spine_array_track_entry_index_of(spine_array_track_entry array, spine_track_entry inValue);

SPINE_C_API /*@null*/ spine_track_entry *spine_array_track_entry_buffer(spine_array_track_entry array);

SPINE_C_API spine_array_update spine_array_update_create(void);

SPINE_C_API spine_array_update spine_array_update_create_with_capacity(size_t initialCapacity);
SPINE_C_API void spine_array_update_dispose(spine_array_update array);
SPINE_C_API void spine_array_update_clear(spine_array_update array);

SPINE_C_API size_t spine_array_update_get_capacity(spine_array_update array);

SPINE_C_API size_t spine_array_update_size(spine_array_update array);

SPINE_C_API spine_array_update spine_array_update_set_size(spine_array_update array, size_t newSize, spine_update defaultValue);

SPINE_C_API void spine_array_update_ensure_capacity(spine_array_update array, size_t newCapacity);

SPINE_C_API void spine_array_update_add(spine_array_update array, spine_update inValue);

SPINE_C_API void spine_array_update_add_all(spine_array_update array, spine_array_update inValue);

SPINE_C_API void spine_array_update_clear_and_add_all(spine_array_update array, spine_array_update inValue);

SPINE_C_API void spine_array_update_remove_at(spine_array_update array, size_t inIndex);

SPINE_C_API bool spine_array_update_contains(spine_array_update array, spine_update inValue);

SPINE_C_API int spine_array_update_index_of(spine_array_update array, spine_update inValue);

SPINE_C_API /*@null*/ spine_update *spine_array_update_buffer(spine_array_update array);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_C_ARRAYS_H */
