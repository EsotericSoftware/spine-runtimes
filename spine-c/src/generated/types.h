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

#ifndef SPINE_C_TYPES_H
#define SPINE_C_TYPES_H

#ifdef __cplusplus
extern "C" {
#endif

#include "../base.h"

// Forward declarations for all non-enum types
SPINE_OPAQUE_TYPE(spine_alpha_timeline)
SPINE_OPAQUE_TYPE(spine_animation)
SPINE_OPAQUE_TYPE(spine_animation_state)
SPINE_OPAQUE_TYPE(spine_animation_state_data)
SPINE_OPAQUE_TYPE(spine_atlas)
SPINE_OPAQUE_TYPE(spine_atlas_attachment_loader)
SPINE_OPAQUE_TYPE(spine_atlas_page)
SPINE_OPAQUE_TYPE(spine_atlas_region)
SPINE_OPAQUE_TYPE(spine_attachment)
SPINE_OPAQUE_TYPE(spine_attachment_loader)
SPINE_OPAQUE_TYPE(spine_attachment_timeline)
SPINE_OPAQUE_TYPE(spine_bone)
SPINE_OPAQUE_TYPE(spine_bone_data)
SPINE_OPAQUE_TYPE(spine_bone_local)
SPINE_OPAQUE_TYPE(spine_bone_pose)
SPINE_OPAQUE_TYPE(spine_bone_timeline)
SPINE_OPAQUE_TYPE(spine_bone_timeline1)
SPINE_OPAQUE_TYPE(spine_bone_timeline2)
SPINE_OPAQUE_TYPE(spine_bounding_box_attachment)
SPINE_OPAQUE_TYPE(spine_clipping_attachment)
SPINE_OPAQUE_TYPE(spine_color)
SPINE_OPAQUE_TYPE(spine_constraint)
SPINE_OPAQUE_TYPE(spine_constraint_data)
SPINE_OPAQUE_TYPE(spine_constraint_timeline)
SPINE_OPAQUE_TYPE(spine_constraint_timeline1)
SPINE_OPAQUE_TYPE(spine_curve_timeline)
SPINE_OPAQUE_TYPE(spine_curve_timeline1)
SPINE_OPAQUE_TYPE(spine_deform_timeline)
SPINE_OPAQUE_TYPE(spine_draw_order)
SPINE_OPAQUE_TYPE(spine_draw_order_folder_timeline)
SPINE_OPAQUE_TYPE(spine_draw_order_timeline)
SPINE_OPAQUE_TYPE(spine_event)
SPINE_OPAQUE_TYPE(spine_event_data)
SPINE_OPAQUE_TYPE(spine_event_queue_entry)
SPINE_OPAQUE_TYPE(spine_event_timeline)
SPINE_OPAQUE_TYPE(spine_from_property)
SPINE_OPAQUE_TYPE(spine_from_rotate)
SPINE_OPAQUE_TYPE(spine_from_scale_x)
SPINE_OPAQUE_TYPE(spine_from_scale_y)
SPINE_OPAQUE_TYPE(spine_from_shear_y)
SPINE_OPAQUE_TYPE(spine_from_x)
SPINE_OPAQUE_TYPE(spine_from_y)
SPINE_OPAQUE_TYPE(spine_ik_constraint)
SPINE_OPAQUE_TYPE(spine_ik_constraint_base)
SPINE_OPAQUE_TYPE(spine_ik_constraint_data)
SPINE_OPAQUE_TYPE(spine_ik_constraint_pose)
SPINE_OPAQUE_TYPE(spine_ik_constraint_timeline)
SPINE_OPAQUE_TYPE(spine_inherit_timeline)
SPINE_OPAQUE_TYPE(spine_interpolation)
SPINE_OPAQUE_TYPE(spine_linked_mesh)
SPINE_OPAQUE_TYPE(spine_mesh_attachment)
SPINE_OPAQUE_TYPE(spine_path_attachment)
SPINE_OPAQUE_TYPE(spine_path_constraint)
SPINE_OPAQUE_TYPE(spine_path_constraint_base)
SPINE_OPAQUE_TYPE(spine_path_constraint_data)
SPINE_OPAQUE_TYPE(spine_path_constraint_mix_timeline)
SPINE_OPAQUE_TYPE(spine_path_constraint_pose)
SPINE_OPAQUE_TYPE(spine_path_constraint_position_timeline)
SPINE_OPAQUE_TYPE(spine_path_constraint_spacing_timeline)
SPINE_OPAQUE_TYPE(spine_physics_constraint)
SPINE_OPAQUE_TYPE(spine_physics_constraint_base)
SPINE_OPAQUE_TYPE(spine_physics_constraint_damping_timeline)
SPINE_OPAQUE_TYPE(spine_physics_constraint_data)
SPINE_OPAQUE_TYPE(spine_physics_constraint_gravity_timeline)
SPINE_OPAQUE_TYPE(spine_physics_constraint_inertia_timeline)
SPINE_OPAQUE_TYPE(spine_physics_constraint_mass_timeline)
SPINE_OPAQUE_TYPE(spine_physics_constraint_mix_timeline)
SPINE_OPAQUE_TYPE(spine_physics_constraint_pose)
SPINE_OPAQUE_TYPE(spine_physics_constraint_reset_timeline)
SPINE_OPAQUE_TYPE(spine_physics_constraint_strength_timeline)
SPINE_OPAQUE_TYPE(spine_physics_constraint_timeline)
SPINE_OPAQUE_TYPE(spine_physics_constraint_wind_timeline)
SPINE_OPAQUE_TYPE(spine_point_attachment)
SPINE_OPAQUE_TYPE(spine_polygon)
SPINE_OPAQUE_TYPE(spine_posed)
SPINE_OPAQUE_TYPE(spine_posed_active)
SPINE_OPAQUE_TYPE(spine_posed_data)
SPINE_OPAQUE_TYPE(spine_region_attachment)
SPINE_OPAQUE_TYPE(spine_render_command)
SPINE_OPAQUE_TYPE(spine_rgb2_timeline)
SPINE_OPAQUE_TYPE(spine_rgba2_timeline)
SPINE_OPAQUE_TYPE(spine_rgba_timeline)
SPINE_OPAQUE_TYPE(spine_rgb_timeline)
SPINE_OPAQUE_TYPE(spine_rotate_timeline)
SPINE_OPAQUE_TYPE(spine_rtti)
SPINE_OPAQUE_TYPE(spine_scale_timeline)
SPINE_OPAQUE_TYPE(spine_scale_x_timeline)
SPINE_OPAQUE_TYPE(spine_scale_y_timeline)
SPINE_OPAQUE_TYPE(spine_sequence)
SPINE_OPAQUE_TYPE(spine_sequence_timeline)
SPINE_OPAQUE_TYPE(spine_shear_timeline)
SPINE_OPAQUE_TYPE(spine_shear_x_timeline)
SPINE_OPAQUE_TYPE(spine_shear_y_timeline)
SPINE_OPAQUE_TYPE(spine_skeleton)
SPINE_OPAQUE_TYPE(spine_skeleton_binary)
SPINE_OPAQUE_TYPE(spine_skeleton_bounds)
SPINE_OPAQUE_TYPE(spine_skeleton_clipping)
SPINE_OPAQUE_TYPE(spine_skeleton_data)
SPINE_OPAQUE_TYPE(spine_skeleton_json)
SPINE_OPAQUE_TYPE(spine_skeleton_renderer)
SPINE_OPAQUE_TYPE(spine_skin)
SPINE_OPAQUE_TYPE(spine_slider)
SPINE_OPAQUE_TYPE(spine_slider_base)
SPINE_OPAQUE_TYPE(spine_slider_data)
SPINE_OPAQUE_TYPE(spine_slider_mix_timeline)
SPINE_OPAQUE_TYPE(spine_slider_pose)
SPINE_OPAQUE_TYPE(spine_slider_timeline)
SPINE_OPAQUE_TYPE(spine_slot)
SPINE_OPAQUE_TYPE(spine_slot_curve_timeline)
SPINE_OPAQUE_TYPE(spine_slot_data)
SPINE_OPAQUE_TYPE(spine_slot_pose)
SPINE_OPAQUE_TYPE(spine_slot_timeline)
SPINE_OPAQUE_TYPE(spine_texture_region)
SPINE_OPAQUE_TYPE(spine_timeline)
SPINE_OPAQUE_TYPE(spine_to_property)
SPINE_OPAQUE_TYPE(spine_to_rotate)
SPINE_OPAQUE_TYPE(spine_to_scale_x)
SPINE_OPAQUE_TYPE(spine_to_scale_y)
SPINE_OPAQUE_TYPE(spine_to_shear_y)
SPINE_OPAQUE_TYPE(spine_to_x)
SPINE_OPAQUE_TYPE(spine_to_y)
SPINE_OPAQUE_TYPE(spine_track_entry)
SPINE_OPAQUE_TYPE(spine_transform_constraint)
SPINE_OPAQUE_TYPE(spine_transform_constraint_base)
SPINE_OPAQUE_TYPE(spine_transform_constraint_data)
SPINE_OPAQUE_TYPE(spine_transform_constraint_pose)
SPINE_OPAQUE_TYPE(spine_transform_constraint_timeline)
SPINE_OPAQUE_TYPE(spine_translate_timeline)
SPINE_OPAQUE_TYPE(spine_translate_x_timeline)
SPINE_OPAQUE_TYPE(spine_translate_y_timeline)
SPINE_OPAQUE_TYPE(spine_update)
SPINE_OPAQUE_TYPE(spine_vertex_attachment)

// Include all enum types (cannot be forward declared)
#include "attachment_type.h"
#include "blend_mode.h"
#include "event_type.h"
#include "format.h"
#include "inherit.h"
#include "physics.h"
#include "position_mode.h"
#include "property.h"
#include "rotate_mode.h"
#include "scale_y_mode.h"
#include "sequence_mode.h"
#include "spacing_mode.h"
#include "texture_filter.h"
#include "texture_wrap.h"

#ifdef __cplusplus
}
#endif

#endif// SPINE_C_TYPES_H
