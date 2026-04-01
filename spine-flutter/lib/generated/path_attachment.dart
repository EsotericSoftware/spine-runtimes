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
import 'vertex_attachment.dart';

/// PathAttachment wrapper
class PathAttachment extends VertexAttachment {
  final Pointer<spine_path_attachment_wrapper> _ptr;

  PathAttachment.fromPointer(this._ptr)
      : super.fromPointer(SpineBindings.bindings.spine_path_attachment_cast_to_vertex_attachment(_ptr));

  /// Get the native pointer for FFI calls
  @override
  Pointer get nativePtr => _ptr;

  factory PathAttachment(String name) {
    final ptr = SpineBindings.bindings.spine_path_attachment_create(name.toNativeUtf8().cast<Char>());
    return PathAttachment.fromPointer(ptr);
  }

  void dispose() {
    SpineBindings.bindings.spine_path_attachment_dispose(_ptr);
  }

  /// The length in the setup pose from the start of the path to the end of each
  /// curve.
  ArrayFloat get lengths {
    final result = SpineBindings.bindings.spine_path_attachment_get_lengths(_ptr);
    return ArrayFloat.fromPointer(result);
  }

  set lengths(ArrayFloat value) {
    SpineBindings.bindings.spine_path_attachment_set_lengths(_ptr, value.nativePtr.cast());
  }

  bool get closed {
    final result = SpineBindings.bindings.spine_path_attachment_get_closed(_ptr);
    return result;
  }

  set closed(bool value) {
    SpineBindings.bindings.spine_path_attachment_set_closed(_ptr, value);
  }

  /// If true, additional calculations are performed to make computing positions
  /// along the path more accurate so movement along the path has a constant
  /// speed.
  bool get constantSpeed {
    final result = SpineBindings.bindings.spine_path_attachment_get_constant_speed(_ptr);
    return result;
  }

  set constantSpeed(bool value) {
    SpineBindings.bindings.spine_path_attachment_set_constant_speed(_ptr, value);
  }

  Color get color {
    final result = SpineBindings.bindings.spine_path_attachment_get_color(_ptr);
    return Color.fromPointer(result);
  }
}
