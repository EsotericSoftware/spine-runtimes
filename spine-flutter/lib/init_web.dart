///
/// Spine Runtimes License Agreement
/// Last updated April 5, 2025. Replaces all prior versions.
///
/// Copyright (c) 2013-2025, Esoteric Software LLC
///
/// Integration of the Spine Runtimes into software or otherwise creating
/// derivative works of the Spine Runtimes is permitted under the terms and
/// conditions of Section 2 of the Spine Editor License Agreement:
/// http://esotericsoftware.com/spine-editor-license
///
/// Otherwise, it is permitted to integrate the Spine Runtimes into software
/// or otherwise create derivative works of the Spine Runtimes (collectively,
/// "Products"), provided that each user of the Products must obtain their own
/// Spine Editor license and redistribution of the Products in any form must
/// include this license and copyright notice.
///
/// THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
/// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
/// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
/// DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
/// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
/// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
/// BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
/// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
/// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
/// THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
///

// ignore_for_file: type_argument_not_matching_bounds
import 'package:flutter/services.dart';
import 'package:inject_js/inject_js.dart' as js;
import 'package:web_ffi_fork/web_ffi.dart';
import 'package:web_ffi_fork/web_ffi_modules.dart';

import 'spine_flutter_bindings_generated.dart';

Module? _module;

class SpineFlutterFFI {
  final DynamicLibrary dylib;
  final Allocator allocator;

  SpineFlutterFFI(this.dylib, this.allocator);
}

Future<SpineFlutterFFI> initSpineFlutterFFI(bool useStaticLinkage) async {
  if (_module == null) {
    Memory.init();

    registerOpaqueType<spine_skeleton_wrapper>();
    registerOpaqueType<spine_skeleton_data_wrapper>();
    registerOpaqueType<spine_bone_wrapper>();
    registerOpaqueType<spine_bone_data_wrapper>();
    registerOpaqueType<spine_slot_wrapper>();
    registerOpaqueType<spine_slot_data_wrapper>();
    registerOpaqueType<spine_skin_wrapper>();
    registerOpaqueType<spine_attachment_wrapper>();
    registerOpaqueType<spine_region_attachment_wrapper>();
    registerOpaqueType<spine_vertex_attachment_wrapper>();
    registerOpaqueType<spine_mesh_attachment_wrapper>();
    registerOpaqueType<spine_clipping_attachment_wrapper>();
    registerOpaqueType<spine_bounding_box_attachment_wrapper>();
    registerOpaqueType<spine_path_attachment_wrapper>();
    registerOpaqueType<spine_point_attachment_wrapper>();
    registerOpaqueType<spine_texture_region_wrapper>();
    registerOpaqueType<spine_sequence_wrapper>();
    registerOpaqueType<spine_constraint_wrapper>();
    registerOpaqueType<spine_constraint_data_wrapper>();
    registerOpaqueType<spine_ik_constraint_wrapper>();
    registerOpaqueType<spine_ik_constraint_data_wrapper>();
    registerOpaqueType<spine_transform_constraint_wrapper>();
    registerOpaqueType<spine_transform_constraint_data_wrapper>();
    registerOpaqueType<spine_path_constraint_wrapper>();
    registerOpaqueType<spine_path_constraint_data_wrapper>();
    registerOpaqueType<spine_animation_state_wrapper>();
    registerOpaqueType<spine_animation_state_data_wrapper>();
    registerOpaqueType<spine_animation_state_events_wrapper>();
    registerOpaqueType<spine_event_wrapper>();
    registerOpaqueType<spine_event_data_wrapper>();
    registerOpaqueType<spine_track_entry_wrapper>();
    registerOpaqueType<spine_animation_wrapper>();
    registerOpaqueType<spine_atlas_wrapper>();
    registerOpaqueType<spine_skeleton_data_result_wrapper>();
    registerOpaqueType<spine_render_command_wrapper>();
    registerOpaqueType<spine_bounds_wrapper>();
    registerOpaqueType<spine_color_wrapper>();
    registerOpaqueType<spine_vector_wrapper>();
    registerOpaqueType<spine_skeleton_drawable_wrapper>();
    registerOpaqueType<spine_skin_entry_wrapper>();
    registerOpaqueType<spine_skin_entries_wrapper>();

    await js.importLibrary('assets/packages/spine_flutter/lib/assets/libspine_flutter.js');
    Uint8List wasmBinaries =
        (await rootBundle.load('packages/spine_flutter/lib/assets/libspine_flutter.wasm')).buffer.asUint8List();
    _module = await EmscriptenModule.compile(wasmBinaries, 'libspine_flutter');
  }
  Module? m = _module;
  if (m != null) {
    final dylib = DynamicLibrary.fromModule(m);
    return SpineFlutterFFI(dylib, dylib.boundMemory);
  } else {
    throw Exception("Couldn't load libspine-flutter.js/.wasm");
  }
}
