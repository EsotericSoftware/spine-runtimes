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

#ifndef SPINE_C_H
#define SPINE_C_H

// Base definitions
#include "../src/base.h"

// All type declarations and enum includes
#include "../src/generated/types.h"

// Extension types & functions
#include "../src/extensions.h"

// Cast functions for type conversions
#include "../src/generated/casts.h"

// Generated class types
#include "../src/generated/alpha_timeline.h"
#include "../src/generated/animation.h"
#include "../src/generated/animation_state.h"
#include "../src/generated/animation_state_data.h"
#include "../src/generated/atlas.h"
#include "../src/generated/atlas_attachment_loader.h"
#include "../src/generated/atlas_page.h"
#include "../src/generated/atlas_region.h"
#include "../src/generated/attachment.h"
#include "../src/generated/attachment_loader.h"
#include "../src/generated/attachment_timeline.h"
#include "../src/generated/bone.h"
#include "../src/generated/bone_data.h"
#include "../src/generated/bone_local.h"
#include "../src/generated/bone_pose.h"
#include "../src/generated/bone_timeline.h"
#include "../src/generated/bone_timeline1.h"
#include "../src/generated/bone_timeline2.h"
#include "../src/generated/bounding_box_attachment.h"
#include "../src/generated/clipping_attachment.h"
#include "../src/generated/color.h"
#include "../src/generated/constraint.h"
#include "../src/generated/constraint_data.h"
#include "../src/generated/constraint_timeline.h"
#include "../src/generated/constraint_timeline1.h"
#include "../src/generated/curve_timeline.h"
#include "../src/generated/curve_timeline1.h"
#include "../src/generated/deform_timeline.h"
#include "../src/generated/draw_order_folder_timeline.h"
#include "../src/generated/draw_order_timeline.h"
#include "../src/generated/event.h"
#include "../src/generated/event_data.h"
#include "../src/generated/event_queue_entry.h"
#include "../src/generated/event_timeline.h"
#include "../src/generated/from_property.h"
#include "../src/generated/from_rotate.h"
#include "../src/generated/from_scale_x.h"
#include "../src/generated/from_scale_y.h"
#include "../src/generated/from_shear_y.h"
#include "../src/generated/from_x.h"
#include "../src/generated/from_y.h"
#include "../src/generated/ik_constraint.h"
#include "../src/generated/ik_constraint_base.h"
#include "../src/generated/ik_constraint_data.h"
#include "../src/generated/ik_constraint_pose.h"
#include "../src/generated/ik_constraint_timeline.h"
#include "../src/generated/inherit_timeline.h"
#include "../src/generated/linked_mesh.h"
#include "../src/generated/mesh_attachment.h"
#include "../src/generated/path_attachment.h"
#include "../src/generated/path_constraint.h"
#include "../src/generated/path_constraint_base.h"
#include "../src/generated/path_constraint_data.h"
#include "../src/generated/path_constraint_mix_timeline.h"
#include "../src/generated/path_constraint_pose.h"
#include "../src/generated/path_constraint_position_timeline.h"
#include "../src/generated/path_constraint_spacing_timeline.h"
#include "../src/generated/physics_constraint.h"
#include "../src/generated/physics_constraint_base.h"
#include "../src/generated/physics_constraint_damping_timeline.h"
#include "../src/generated/physics_constraint_data.h"
#include "../src/generated/physics_constraint_gravity_timeline.h"
#include "../src/generated/physics_constraint_inertia_timeline.h"
#include "../src/generated/physics_constraint_mass_timeline.h"
#include "../src/generated/physics_constraint_mix_timeline.h"
#include "../src/generated/physics_constraint_pose.h"
#include "../src/generated/physics_constraint_reset_timeline.h"
#include "../src/generated/physics_constraint_strength_timeline.h"
#include "../src/generated/physics_constraint_timeline.h"
#include "../src/generated/physics_constraint_wind_timeline.h"
#include "../src/generated/point_attachment.h"
#include "../src/generated/polygon.h"
#include "../src/generated/posed.h"
#include "../src/generated/posed_active.h"
#include "../src/generated/posed_data.h"
#include "../src/generated/region_attachment.h"
#include "../src/generated/render_command.h"
#include "../src/generated/rgb2_timeline.h"
#include "../src/generated/rgba2_timeline.h"
#include "../src/generated/rgba_timeline.h"
#include "../src/generated/rgb_timeline.h"
#include "../src/generated/rotate_timeline.h"
#include "../src/generated/rtti.h"
#include "../src/generated/scale_timeline.h"
#include "../src/generated/scale_x_timeline.h"
#include "../src/generated/scale_y_timeline.h"
#include "../src/generated/sequence.h"
#include "../src/generated/sequence_timeline.h"
#include "../src/generated/shear_timeline.h"
#include "../src/generated/shear_x_timeline.h"
#include "../src/generated/shear_y_timeline.h"
#include "../src/generated/skeleton.h"
#include "../src/generated/skeleton_binary.h"
#include "../src/generated/skeleton_bounds.h"
#include "../src/generated/skeleton_clipping.h"
#include "../src/generated/skeleton_data.h"
#include "../src/generated/skeleton_json.h"
#include "../src/generated/skeleton_renderer.h"
#include "../src/generated/skin.h"
#include "../src/generated/slider.h"
#include "../src/generated/slider_base.h"
#include "../src/generated/slider_data.h"
#include "../src/generated/slider_mix_timeline.h"
#include "../src/generated/slider_pose.h"
#include "../src/generated/slider_timeline.h"
#include "../src/generated/slot.h"
#include "../src/generated/slot_curve_timeline.h"
#include "../src/generated/slot_data.h"
#include "../src/generated/slot_pose.h"
#include "../src/generated/slot_timeline.h"
#include "../src/generated/texture_region.h"
#include "../src/generated/timeline.h"
#include "../src/generated/to_property.h"
#include "../src/generated/to_rotate.h"
#include "../src/generated/to_scale_x.h"
#include "../src/generated/to_scale_y.h"
#include "../src/generated/to_shear_y.h"
#include "../src/generated/to_x.h"
#include "../src/generated/to_y.h"
#include "../src/generated/track_entry.h"
#include "../src/generated/transform_constraint.h"
#include "../src/generated/transform_constraint_base.h"
#include "../src/generated/transform_constraint_data.h"
#include "../src/generated/transform_constraint_pose.h"
#include "../src/generated/transform_constraint_timeline.h"
#include "../src/generated/translate_timeline.h"
#include "../src/generated/translate_x_timeline.h"
#include "../src/generated/translate_y_timeline.h"
#include "../src/generated/update.h"
#include "../src/generated/vertex_attachment.h"

#endif// SPINE_C_H
