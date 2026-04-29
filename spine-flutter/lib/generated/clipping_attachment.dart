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
import 'color.dart';
import 'slot_data.dart';
import 'vertex_attachment.dart';

/// ClippingAttachment wrapper
class ClippingAttachment extends VertexAttachment {
  final Pointer<spine_clipping_attachment_wrapper> _ptr;

  ClippingAttachment.fromPointer(this._ptr)
      : super.fromPointer(SpineBindings.bindings.spine_clipping_attachment_cast_to_vertex_attachment(_ptr));

  /// Get the native pointer for FFI calls
  @override
  Pointer get nativePtr => _ptr;

  factory ClippingAttachment(String name) {
    final ptr = SpineBindings.bindings.spine_clipping_attachment_create(name.toNativeUtf8().cast<Char>());
    return ClippingAttachment.fromPointer(ptr);
  }

  void dispose() {
    SpineBindings.bindings.spine_clipping_attachment_dispose(_ptr);
  }

  /// Clipping is performed between the clipping attachment's slot and the end
  /// slot. If NULL, clipping is done until the end of the skeleton's rendering.
  SlotData? get endSlot {
    final result = SpineBindings.bindings.spine_clipping_attachment_get_end_slot(_ptr);
    return result.address == 0 ? null : SlotData.fromPointer(result);
  }

  set endSlot(SlotData? value) {
    SpineBindings.bindings
        .spine_clipping_attachment_set_end_slot(_ptr, value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// When true the clipping polygon is treated as convex for more efficient
  /// clipping. If the polygon deforms to concave then the convex hull is used.
  /// When false the clipping polygon can be concave and if so has an additional
  /// CPU cost. Inverse clipping always uses convex.
  bool get convex {
    final result = SpineBindings.bindings.spine_clipping_attachment_get_convex(_ptr);
    return result;
  }

  set convex(bool value) {
    SpineBindings.bindings.spine_clipping_attachment_set_convex(_ptr, value);
  }

  /// When false, everything inside the clipping polygon is visible. When true,
  /// everything outside the clipping polygon is visible and clipping is convex.
  bool get inverse {
    final result = SpineBindings.bindings.spine_clipping_attachment_get_inverse(_ptr);
    return result;
  }

  set inverse(bool value) {
    SpineBindings.bindings.spine_clipping_attachment_set_inverse(_ptr, value);
  }

  Color get color {
    final result = SpineBindings.bindings.spine_clipping_attachment_get_color(_ptr);
    return Color.fromPointer(result);
  }
}
