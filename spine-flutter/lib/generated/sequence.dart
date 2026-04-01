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
import 'package:universal_ffi/ffi_utils.dart';
import 'spine_dart_bindings_generated.dart';
import '../spine_bindings.dart';
import 'arrays.dart';
import 'mesh_attachment.dart';
import 'region_attachment.dart';
import 'slot_pose.dart';
import 'texture_region.dart';

/// Holds texture regions, UVs, and vertex offsets for rendering a region or
/// mesh attachment. Regions must be populated and update() called before use.
class Sequence {
  final Pointer<spine_sequence_wrapper> _ptr;

  Sequence.fromPointer(this._ptr);

  /// Get the native pointer for FFI calls
  Pointer get nativePtr => _ptr;

  /// [count] The number of texture regions this sequence will display.
  /// [pathSuffix] If true, getPath(String, int) has a numeric suffix. If false, all regions will use the same path, so count should be 1.
  factory Sequence(int count, bool pathSuffix) {
    final ptr = SpineBindings.bindings.spine_sequence_create(count, pathSuffix);
    return Sequence.fromPointer(ptr);
  }

  /// Copy constructor.
  factory Sequence.from(Sequence other) {
    final ptr = SpineBindings.bindings.spine_sequence_create2(other.nativePtr.cast());
    return Sequence.fromPointer(ptr);
  }

  void dispose() {
    SpineBindings.bindings.spine_sequence_dispose(_ptr);
  }

  /// The list of texture regions this sequence will display.
  ArrayTextureRegion get regions {
    final result = SpineBindings.bindings.spine_sequence_get_regions(_ptr);
    return ArrayTextureRegion.fromPointer(result);
  }

  /// Returns the getRegions() index for SlotPose::getSequenceIndex().
  int resolveIndex(SlotPose pose) {
    final result = SpineBindings.bindings.spine_sequence_resolve_index(_ptr, pose.nativePtr.cast());
    return result;
  }

  /// Returns the texture region from getRegions() for the specified index.
  TextureRegion? getRegion(int index) {
    final result = SpineBindings.bindings.spine_sequence_get_region(_ptr, index);
    return result.address == 0 ? null : TextureRegion.fromPointer(result);
  }

  /// Returns the UVs for the specified index. getRegions() must be populated
  /// and update() called before calling this method.
  ArrayFloat getUVs(int index) {
    final result = SpineBindings.bindings.spine_sequence_get_u_vs(_ptr, index);
    return ArrayFloat.fromPointer(result);
  }

  /// Returns vertex offsets from the center of a RegionAttachment. Invalid to
  /// call for a MeshAttachment.
  ArrayFloat getOffsets(int index) {
    final result = SpineBindings.bindings.spine_sequence_get_offsets(_ptr, index);
    return ArrayFloat.fromPointer(result);
  }

  /// The starting number for the numeric getPath(String, int) suffix.
  int get start {
    final result = SpineBindings.bindings.spine_sequence_get_start(_ptr);
    return result;
  }

  set start(int value) {
    SpineBindings.bindings.spine_sequence_set_start(_ptr, value);
  }

  /// The minimum number of digits in the numeric getPath(String, int) suffix,
  /// for zero padding. 0 for no zero padding.
  int get digits {
    final result = SpineBindings.bindings.spine_sequence_get_digits(_ptr);
    return result;
  }

  set digits(int value) {
    SpineBindings.bindings.spine_sequence_set_digits(_ptr, value);
  }

  /// The index of the region to show for the setup pose.
  int get setupIndex {
    final result = SpineBindings.bindings.spine_sequence_get_setup_index(_ptr);
    return result;
  }

  set setupIndex(int value) {
    SpineBindings.bindings.spine_sequence_set_setup_index(_ptr, value);
  }

  /// Returns true if getPath(String, int) has a numeric suffix.
  bool get hasPathSuffix {
    final result = SpineBindings.bindings.spine_sequence_has_path_suffix(_ptr);
    return result;
  }

  /// Returns the specified base path with an optional numeric suffix for the
  /// specified index.
  String getPath(String basePath, int index) {
    final result = SpineBindings.bindings.spine_sequence_get_path(_ptr, basePath.toNativeUtf8().cast<Char>(), index);
    return result.cast<Utf8>().toDartString();
  }

  /// Returns a unique ID for this sequence.
  int get id {
    final result = SpineBindings.bindings.spine_sequence_get_id(_ptr);
    return result;
  }

  /// Computes UVs and offsets for the specified attachment. Must be called if
  /// the regions or attachment properties are changed.
  void update(RegionAttachment attachment) {
    SpineBindings.bindings.spine_sequence_update_1(_ptr, attachment.nativePtr.cast());
  }

  void update2(MeshAttachment attachment) {
    SpineBindings.bindings.spine_sequence_update_2(_ptr, attachment.nativePtr.cast());
  }
}
