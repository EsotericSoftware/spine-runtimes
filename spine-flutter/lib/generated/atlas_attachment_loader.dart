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
import 'atlas.dart';
import 'atlas_region.dart';
import 'attachment_loader.dart';
import 'bounding_box_attachment.dart';
import 'clipping_attachment.dart';
import 'mesh_attachment.dart';
import 'path_attachment.dart';
import 'point_attachment.dart';
import 'region_attachment.dart';
import 'sequence.dart';
import 'skin.dart';

/// An AttachmentLoader that configures attachments using texture regions from
/// an Atlas.
///
/// See
/// https://esotericsoftware.com/spine-loading-skeleton-data#JSON-and-binary-data
/// Loading skeleton data in the Spine Runtimes Guide.
class AtlasAttachmentLoader implements AttachmentLoader {
  final Pointer<spine_atlas_attachment_loader_wrapper> _ptr;

  AtlasAttachmentLoader.fromPointer(this._ptr);

  /// Get the native pointer for FFI calls
  @override
  Pointer get nativePtr => _ptr;

  factory AtlasAttachmentLoader(Atlas atlas) {
    final ptr = SpineBindings.bindings.spine_atlas_attachment_loader_create(atlas.nativePtr.cast());
    return AtlasAttachmentLoader.fromPointer(ptr);
  }

  void dispose() {
    SpineBindings.bindings.spine_atlas_attachment_loader_dispose(_ptr);
  }

  @override
  RegionAttachment? newRegionAttachment(Skin skin, String placeholder, String name, String path, Sequence? sequence) {
    final result = SpineBindings.bindings.spine_atlas_attachment_loader_new_region_attachment(
        _ptr,
        skin.nativePtr.cast(),
        placeholder.toNativeUtf8().cast<Char>(),
        name.toNativeUtf8().cast<Char>(),
        path.toNativeUtf8().cast<Char>(),
        sequence?.nativePtr.cast() ?? Pointer.fromAddress(0));
    return result.address == 0 ? null : RegionAttachment.fromPointer(result);
  }

  @override
  MeshAttachment? newMeshAttachment(Skin skin, String placeholder, String name, String path, Sequence? sequence) {
    final result = SpineBindings.bindings.spine_atlas_attachment_loader_new_mesh_attachment(
        _ptr,
        skin.nativePtr.cast(),
        placeholder.toNativeUtf8().cast<Char>(),
        name.toNativeUtf8().cast<Char>(),
        path.toNativeUtf8().cast<Char>(),
        sequence?.nativePtr.cast() ?? Pointer.fromAddress(0));
    return result.address == 0 ? null : MeshAttachment.fromPointer(result);
  }

  @override
  BoundingBoxAttachment? newBoundingBoxAttachment(Skin skin, String placeholder, String name) {
    final result = SpineBindings.bindings.spine_atlas_attachment_loader_new_bounding_box_attachment(
        _ptr, skin.nativePtr.cast(), placeholder.toNativeUtf8().cast<Char>(), name.toNativeUtf8().cast<Char>());
    return result.address == 0 ? null : BoundingBoxAttachment.fromPointer(result);
  }

  @override
  PathAttachment? newPathAttachment(Skin skin, String placeholder, String name) {
    final result = SpineBindings.bindings.spine_atlas_attachment_loader_new_path_attachment(
        _ptr, skin.nativePtr.cast(), placeholder.toNativeUtf8().cast<Char>(), name.toNativeUtf8().cast<Char>());
    return result.address == 0 ? null : PathAttachment.fromPointer(result);
  }

  @override
  PointAttachment? newPointAttachment(Skin skin, String placeholder, String name) {
    final result = SpineBindings.bindings.spine_atlas_attachment_loader_new_point_attachment(
        _ptr, skin.nativePtr.cast(), placeholder.toNativeUtf8().cast<Char>(), name.toNativeUtf8().cast<Char>());
    return result.address == 0 ? null : PointAttachment.fromPointer(result);
  }

  @override
  ClippingAttachment? newClippingAttachment(Skin skin, String placeholder, String name) {
    final result = SpineBindings.bindings.spine_atlas_attachment_loader_new_clipping_attachment(
        _ptr, skin.nativePtr.cast(), placeholder.toNativeUtf8().cast<Char>(), name.toNativeUtf8().cast<Char>());
    return result.address == 0 ? null : ClippingAttachment.fromPointer(result);
  }

  AtlasRegion? findRegion(String name) {
    final result =
        SpineBindings.bindings.spine_atlas_attachment_loader_find_region(_ptr, name.toNativeUtf8().cast<Char>());
    return result.address == 0 ? null : AtlasRegion.fromPointer(result);
  }
}
