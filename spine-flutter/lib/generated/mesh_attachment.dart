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
import 'color.dart';
import 'sequence.dart';
import 'texture_region.dart';
import 'vertex_attachment.dart';

/// Attachment that displays a texture region using a mesh.
class MeshAttachment extends VertexAttachment {
  final Pointer<spine_mesh_attachment_wrapper> _ptr;

  MeshAttachment.fromPointer(this._ptr)
      : super.fromPointer(SpineBindings.bindings.spine_mesh_attachment_cast_to_vertex_attachment(_ptr));

  /// Get the native pointer for FFI calls
  @override
  Pointer get nativePtr => _ptr;

  factory MeshAttachment(String name, Sequence? sequence) {
    final ptr = SpineBindings.bindings.spine_mesh_attachment_create(
        name.toNativeUtf8().cast<Char>(), sequence?.nativePtr.cast() ?? Pointer.fromAddress(0));
    return MeshAttachment.fromPointer(ptr);
  }

  void dispose() {
    SpineBindings.bindings.spine_mesh_attachment_dispose(_ptr);
  }

  ArrayFloat get regionUVs {
    final result = SpineBindings.bindings.spine_mesh_attachment_get_region_u_vs(_ptr);
    return ArrayFloat.fromPointer(result);
  }

  set regionUVs(ArrayFloat value) {
    SpineBindings.bindings.spine_mesh_attachment_set_region_u_vs(_ptr, value.nativePtr.cast());
  }

  ArrayUnsignedShort get triangles {
    final result = SpineBindings.bindings.spine_mesh_attachment_get_triangles(_ptr);
    return ArrayUnsignedShort.fromPointer(result);
  }

  set triangles(ArrayUnsignedShort value) {
    SpineBindings.bindings.spine_mesh_attachment_set_triangles(_ptr, value.nativePtr.cast());
  }

  int get hullLength {
    final result = SpineBindings.bindings.spine_mesh_attachment_get_hull_length(_ptr);
    return result;
  }

  set hullLength(int value) {
    SpineBindings.bindings.spine_mesh_attachment_set_hull_length(_ptr, value);
  }

  Sequence get sequence {
    final result = SpineBindings.bindings.spine_mesh_attachment_get_sequence(_ptr);
    return Sequence.fromPointer(result);
  }

  void updateSequence() {
    SpineBindings.bindings.spine_mesh_attachment_update_sequence(_ptr);
  }

  String get path {
    final result = SpineBindings.bindings.spine_mesh_attachment_get_path(_ptr);
    return result.cast<Utf8>().toDartString();
  }

  set path(String value) {
    SpineBindings.bindings.spine_mesh_attachment_set_path(_ptr, value.toNativeUtf8().cast<Char>());
  }

  Color get color {
    final result = SpineBindings.bindings.spine_mesh_attachment_get_color(_ptr);
    return Color.fromPointer(result);
  }

  /// The parent mesh if this is a linked mesh, else NULL. A linked mesh shares
  /// the bones, vertices, regionUVs, triangles, hullLength, edges, width, and
  /// height with the parent mesh, but may have a different name or path, and
  /// therefore a different texture region.
  MeshAttachment? get parentMesh {
    final result = SpineBindings.bindings.spine_mesh_attachment_get_parent_mesh(_ptr);
    return result.address == 0 ? null : MeshAttachment.fromPointer(result);
  }

  set parentMesh(MeshAttachment? value) {
    SpineBindings.bindings
        .spine_mesh_attachment_set_parent_mesh(_ptr, value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// Vertex index pairs describing edges for controlling triangulation, or
  /// empty if nonessential data was not exported. Mesh triangles do not cross
  /// edges. Triangulation is not performed at runtime.
  ArrayUnsignedShort get edges {
    final result = SpineBindings.bindings.spine_mesh_attachment_get_edges(_ptr);
    return ArrayUnsignedShort.fromPointer(result);
  }

  set edges(ArrayUnsignedShort value) {
    SpineBindings.bindings.spine_mesh_attachment_set_edges(_ptr, value.nativePtr.cast());
  }

  double get width {
    final result = SpineBindings.bindings.spine_mesh_attachment_get_width(_ptr);
    return result;
  }

  set width(double value) {
    SpineBindings.bindings.spine_mesh_attachment_set_width(_ptr, value);
  }

  double get height {
    final result = SpineBindings.bindings.spine_mesh_attachment_get_height(_ptr);
    return result;
  }

  set height(double value) {
    SpineBindings.bindings.spine_mesh_attachment_set_height(_ptr, value);
  }

  MeshAttachment newLinkedMesh() {
    final result = SpineBindings.bindings.spine_mesh_attachment_new_linked_mesh(_ptr);
    return MeshAttachment.fromPointer(result);
  }

  /// Computes UVs for a mesh attachment.
  ///
  /// [uvs] Output array for the computed UVs, same length as regionUVs.
  static void computeUVs(TextureRegion? region, ArrayFloat regionUVs, ArrayFloat uvs) {
    SpineBindings.bindings.spine_mesh_attachment_compute_u_vs(
        region?.nativePtr.cast() ?? Pointer.fromAddress(0), regionUVs.nativePtr.cast(), uvs.nativePtr.cast());
  }
}
