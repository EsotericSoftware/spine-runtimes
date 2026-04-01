//
// Spine Runtimes License Agreement
// Last updated April 5, 2025. Replaces all prior versions.
//
// Copyright (c) 2013-2025, Esoteric Software LLC
//
// Integration of the Spine Runtimes into software or otherwise creating
// derivative works of the Spine Runtimes is permitted under the terms and
// conditions of Section 2 of the Spine Editor License Agreement:
// http://esotericsoftware.com/spine-editor-license
//
// Otherwise, it is permitted to integrate the Spine Runtimes into software
// or otherwise create derivative works of the Spine Runtimes (collectively,
// "Products"), provided that each user of the Products must obtain their own
// Spine Editor license and redistribution of the Products in any form must
// include this license and copyright notice.
//
// THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
// BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

// AUTO GENERATED FILE, DO NOT EDIT.

import 'package:universal_ffi/ffi.dart';

import 'generated/spine_dart_bindings_generated.dart';

DynamicLibrary? dylibInstance;

class SpineDartFFI {
  final DynamicLibrary dylib;
  final Allocator allocator;

  SpineDartFFI(this.dylib, this.allocator);
}

Future<SpineDartFFI> initSpineDartFFI(bool useStaticLinkage) async {
  if (dylibInstance == null) {
    // Load the wasm module first - this calls initTypes()
    dylibInstance = await DynamicLibrary.open('assets/packages/spine_flutter/lib/assets/libspine_flutter.js');

    // Now register all the opaque types
    registerOpaqueType<spine_alpha_timeline_wrapper>();
    registerOpaqueType<spine_animation_state_data_wrapper>();
    registerOpaqueType<spine_animation_state_events_wrapper>();
    registerOpaqueType<spine_animation_state_wrapper>();
    registerOpaqueType<spine_animation_wrapper>();
    registerOpaqueType<spine_array_animation_wrapper>();
    registerOpaqueType<spine_array_atlas_page_wrapper>();
    registerOpaqueType<spine_array_atlas_region_wrapper>();
    registerOpaqueType<spine_array_attachment_wrapper>();
    registerOpaqueType<spine_array_bone_data_wrapper>();
    registerOpaqueType<spine_array_bone_pose_wrapper>();
    registerOpaqueType<spine_array_bone_wrapper>();
    registerOpaqueType<spine_array_bounding_box_attachment_wrapper>();
    registerOpaqueType<spine_array_constraint_data_wrapper>();
    registerOpaqueType<spine_array_constraint_wrapper>();
    registerOpaqueType<spine_array_event_data_wrapper>();
    registerOpaqueType<spine_array_event_wrapper>();
    registerOpaqueType<spine_array_float_wrapper>();
    registerOpaqueType<spine_array_from_property_wrapper>();
    registerOpaqueType<spine_array_int_wrapper>();
    registerOpaqueType<spine_array_physics_constraint_wrapper>();
    registerOpaqueType<spine_array_polygon_wrapper>();
    registerOpaqueType<spine_array_property_id_wrapper>();
    registerOpaqueType<spine_array_skin_wrapper>();
    registerOpaqueType<spine_array_slot_data_wrapper>();
    registerOpaqueType<spine_array_slot_wrapper>();
    registerOpaqueType<spine_array_texture_region_wrapper>();
    registerOpaqueType<spine_array_timeline_wrapper>();
    registerOpaqueType<spine_array_to_property_wrapper>();
    registerOpaqueType<spine_array_track_entry_wrapper>();
    registerOpaqueType<spine_array_unsigned_short_wrapper>();
    registerOpaqueType<spine_array_update_wrapper>();
    registerOpaqueType<spine_atlas_attachment_loader_wrapper>();
    registerOpaqueType<spine_atlas_page_wrapper>();
    registerOpaqueType<spine_atlas_region_wrapper>();
    registerOpaqueType<spine_atlas_result_wrapper>();
    registerOpaqueType<spine_atlas_wrapper>();
    registerOpaqueType<spine_attachment_loader_wrapper>();
    registerOpaqueType<spine_attachment_timeline_wrapper>();
    registerOpaqueType<spine_attachment_wrapper>();
    registerOpaqueType<spine_bone_data_wrapper>();
    registerOpaqueType<spine_bone_local_wrapper>();
    registerOpaqueType<spine_bone_pose_wrapper>();
    registerOpaqueType<spine_bone_timeline1_wrapper>();
    registerOpaqueType<spine_bone_timeline2_wrapper>();
    registerOpaqueType<spine_bone_timeline_wrapper>();
    registerOpaqueType<spine_bone_wrapper>();
    registerOpaqueType<spine_bounding_box_attachment_wrapper>();
    registerOpaqueType<spine_clipping_attachment_wrapper>();
    registerOpaqueType<spine_color_wrapper>();
    registerOpaqueType<spine_constraint_data_wrapper>();
    registerOpaqueType<spine_constraint_timeline1_wrapper>();
    registerOpaqueType<spine_constraint_timeline_wrapper>();
    registerOpaqueType<spine_constraint_wrapper>();
    registerOpaqueType<spine_curve_timeline1_wrapper>();
    registerOpaqueType<spine_curve_timeline_wrapper>();
    registerOpaqueType<spine_deform_timeline_wrapper>();
    registerOpaqueType<spine_draw_order_folder_timeline_wrapper>();
    registerOpaqueType<spine_draw_order_timeline_wrapper>();
    registerOpaqueType<spine_draw_order_wrapper>();
    registerOpaqueType<spine_event_data_wrapper>();
    registerOpaqueType<spine_event_queue_entry_wrapper>();
    registerOpaqueType<spine_event_timeline_wrapper>();
    registerOpaqueType<spine_event_wrapper>();
    registerOpaqueType<spine_from_property_wrapper>();
    registerOpaqueType<spine_from_rotate_wrapper>();
    registerOpaqueType<spine_from_scale_x_wrapper>();
    registerOpaqueType<spine_from_scale_y_wrapper>();
    registerOpaqueType<spine_from_shear_y_wrapper>();
    registerOpaqueType<spine_from_x_wrapper>();
    registerOpaqueType<spine_from_y_wrapper>();
    registerOpaqueType<spine_ik_constraint_base_wrapper>();
    registerOpaqueType<spine_ik_constraint_data_wrapper>();
    registerOpaqueType<spine_ik_constraint_pose_wrapper>();
    registerOpaqueType<spine_ik_constraint_timeline_wrapper>();
    registerOpaqueType<spine_ik_constraint_wrapper>();
    registerOpaqueType<spine_inherit_timeline_wrapper>();
    registerOpaqueType<spine_linked_mesh_wrapper>();
    registerOpaqueType<spine_mesh_attachment_wrapper>();
    registerOpaqueType<spine_path_attachment_wrapper>();
    registerOpaqueType<spine_path_constraint_base_wrapper>();
    registerOpaqueType<spine_path_constraint_data_wrapper>();
    registerOpaqueType<spine_path_constraint_mix_timeline_wrapper>();
    registerOpaqueType<spine_path_constraint_pose_wrapper>();
    registerOpaqueType<spine_path_constraint_position_timeline_wrapper>();
    registerOpaqueType<spine_path_constraint_spacing_timeline_wrapper>();
    registerOpaqueType<spine_path_constraint_wrapper>();
    registerOpaqueType<spine_physics_constraint_base_wrapper>();
    registerOpaqueType<spine_physics_constraint_damping_timeline_wrapper>();
    registerOpaqueType<spine_physics_constraint_data_wrapper>();
    registerOpaqueType<spine_physics_constraint_gravity_timeline_wrapper>();
    registerOpaqueType<spine_physics_constraint_inertia_timeline_wrapper>();
    registerOpaqueType<spine_physics_constraint_mass_timeline_wrapper>();
    registerOpaqueType<spine_physics_constraint_mix_timeline_wrapper>();
    registerOpaqueType<spine_physics_constraint_pose_wrapper>();
    registerOpaqueType<spine_physics_constraint_reset_timeline_wrapper>();
    registerOpaqueType<spine_physics_constraint_strength_timeline_wrapper>();
    registerOpaqueType<spine_physics_constraint_timeline_wrapper>();
    registerOpaqueType<spine_physics_constraint_wind_timeline_wrapper>();
    registerOpaqueType<spine_physics_constraint_wrapper>();
    registerOpaqueType<spine_point_attachment_wrapper>();
    registerOpaqueType<spine_polygon_wrapper>();
    registerOpaqueType<spine_posed_active_wrapper>();
    registerOpaqueType<spine_posed_data_wrapper>();
    registerOpaqueType<spine_posed_wrapper>();
    registerOpaqueType<spine_region_attachment_wrapper>();
    registerOpaqueType<spine_render_command_wrapper>();
    registerOpaqueType<spine_rgb2_timeline_wrapper>();
    registerOpaqueType<spine_rgb_timeline_wrapper>();
    registerOpaqueType<spine_rgba2_timeline_wrapper>();
    registerOpaqueType<spine_rgba_timeline_wrapper>();
    registerOpaqueType<spine_rotate_timeline_wrapper>();
    registerOpaqueType<spine_rtti_wrapper>();
    registerOpaqueType<spine_scale_timeline_wrapper>();
    registerOpaqueType<spine_scale_x_timeline_wrapper>();
    registerOpaqueType<spine_scale_y_timeline_wrapper>();
    registerOpaqueType<spine_sequence_timeline_wrapper>();
    registerOpaqueType<spine_sequence_wrapper>();
    registerOpaqueType<spine_shear_timeline_wrapper>();
    registerOpaqueType<spine_shear_x_timeline_wrapper>();
    registerOpaqueType<spine_shear_y_timeline_wrapper>();
    registerOpaqueType<spine_skeleton_binary_wrapper>();
    registerOpaqueType<spine_skeleton_bounds_wrapper>();
    registerOpaqueType<spine_skeleton_clipping_wrapper>();
    registerOpaqueType<spine_skeleton_data_result_wrapper>();
    registerOpaqueType<spine_skeleton_data_wrapper>();
    registerOpaqueType<spine_skeleton_drawable_wrapper>();
    registerOpaqueType<spine_skeleton_json_wrapper>();
    registerOpaqueType<spine_skeleton_renderer_wrapper>();
    registerOpaqueType<spine_skeleton_wrapper>();
    registerOpaqueType<spine_skin_entries_wrapper>();
    registerOpaqueType<spine_skin_entry_wrapper>();
    registerOpaqueType<spine_skin_wrapper>();
    registerOpaqueType<spine_slider_base_wrapper>();
    registerOpaqueType<spine_slider_data_wrapper>();
    registerOpaqueType<spine_slider_mix_timeline_wrapper>();
    registerOpaqueType<spine_slider_pose_wrapper>();
    registerOpaqueType<spine_slider_timeline_wrapper>();
    registerOpaqueType<spine_slider_wrapper>();
    registerOpaqueType<spine_slot_curve_timeline_wrapper>();
    registerOpaqueType<spine_slot_data_wrapper>();
    registerOpaqueType<spine_slot_pose_wrapper>();
    registerOpaqueType<spine_slot_timeline_wrapper>();
    registerOpaqueType<spine_slot_wrapper>();
    registerOpaqueType<spine_texture_loader_wrapper>();
    registerOpaqueType<spine_texture_region_wrapper>();
    registerOpaqueType<spine_timeline_wrapper>();
    registerOpaqueType<spine_to_property_wrapper>();
    registerOpaqueType<spine_to_rotate_wrapper>();
    registerOpaqueType<spine_to_scale_x_wrapper>();
    registerOpaqueType<spine_to_scale_y_wrapper>();
    registerOpaqueType<spine_to_shear_y_wrapper>();
    registerOpaqueType<spine_to_x_wrapper>();
    registerOpaqueType<spine_to_y_wrapper>();
    registerOpaqueType<spine_track_entry_wrapper>();
    registerOpaqueType<spine_transform_constraint_base_wrapper>();
    registerOpaqueType<spine_transform_constraint_data_wrapper>();
    registerOpaqueType<spine_transform_constraint_pose_wrapper>();
    registerOpaqueType<spine_transform_constraint_timeline_wrapper>();
    registerOpaqueType<spine_transform_constraint_wrapper>();
    registerOpaqueType<spine_translate_timeline_wrapper>();
    registerOpaqueType<spine_translate_x_timeline_wrapper>();
    registerOpaqueType<spine_translate_y_timeline_wrapper>();
    registerOpaqueType<spine_update_wrapper>();
    registerOpaqueType<spine_vertex_attachment_wrapper>();
  }

  if (dylibInstance != null) {
    return SpineDartFFI(dylibInstance!, dylibInstance!.allocator);
  } else {
    throw Exception("Couldn't load libspine-flutter.js/.wasm");
  }
}
