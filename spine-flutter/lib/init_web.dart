import 'package:flutter/services.dart';
import 'package:inject_js/inject_js.dart' as js;
import 'web_ffi/web_ffi.dart';
import 'web_ffi/web_ffi_modules.dart';
import 'ffi_utf8.dart';
import 'spine_flutter_bindings_generated.dart';

Module? _module;

class SpineFlutterFFI {
  final DynamicLibrary dylib;
  final Allocator allocator;

  SpineFlutterFFI(this.dylib, this.allocator);
}

Future<SpineFlutterFFI> initSpineFlutterFFI() async {
  if (_module == null) {
    Memory.init();

    registerOpaqueType<Utf8>();
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
    Uint8List wasmBinaries = (await rootBundle.load('packages/spine_flutter/lib/assets/libspine_flutter.wasm')).buffer.asUint8List();
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