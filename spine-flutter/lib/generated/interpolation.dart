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
import 'spine_dart_bindings_generated.dart';
import '../spine_bindings.dart';

/// Takes a linear value in the range 0-1 and outputs a usually non-linear,
/// interpolated value.
class Interpolation {
  final Pointer<spine_interpolation_wrapper> _ptr;

  Interpolation.fromPointer(this._ptr);

  /// Get the native pointer for FFI calls
  Pointer get nativePtr => _ptr;

  static Interpolation linear() {
    final result = SpineBindings.bindings.spine_interpolation_linear();
    return Interpolation.fromPointer(result);
  }

  /// Aka "smoothstep".
  static Interpolation smooth() {
    final result = SpineBindings.bindings.spine_interpolation_smooth();
    return Interpolation.fromPointer(result);
  }

  /// Slow, then fast.
  static Interpolation slowFast() {
    final result = SpineBindings.bindings.spine_interpolation_slow_fast();
    return Interpolation.fromPointer(result);
  }

  /// Fast, then slow.
  static Interpolation fastSlow() {
    final result = SpineBindings.bindings.spine_interpolation_fast_slow();
    return Interpolation.fromPointer(result);
  }

  static Interpolation circle() {
    final result = SpineBindings.bindings.spine_interpolation_circle();
    return Interpolation.fromPointer(result);
  }

  /// [a] Alpha value between 0 and 1.
  double apply(double a) {
    final result = SpineBindings.bindings.spine_interpolation_apply_1(_ptr, a);
    return result;
  }

  double apply2(double start, double end, double a) {
    final result = SpineBindings.bindings.spine_interpolation_apply_2(_ptr, start, end, a);
    return result;
  }
}
