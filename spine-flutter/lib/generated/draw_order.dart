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
import 'arrays.dart';

/// Stores the skeleton's draw order, which is the order that each slot's
/// attachment is rendered.
class DrawOrder {
  final Pointer<spine_draw_order_wrapper> _ptr;

  DrawOrder.fromPointer(this._ptr);

  /// Get the native pointer for FFI calls
  Pointer get nativePtr => _ptr;

  factory DrawOrder(ArraySlot setupPose) {
    final ptr = SpineBindings.bindings.spine_draw_order_create(setupPose.nativePtr.cast());
    return DrawOrder.fromPointer(ptr);
  }

  void dispose() {
    SpineBindings.bindings.spine_draw_order_dispose(_ptr);
  }

  /// Sets the unconstrained draw order to the setup pose order.
  void setupPose() {
    SpineBindings.bindings.spine_draw_order_setup_pose(_ptr);
  }

  /// The unconstrained draw order, set by animations and application code.
  ArraySlot get pose {
    final result = SpineBindings.bindings.spine_draw_order_get_pose(_ptr);
    return ArraySlot.fromPointer(result);
  }

  /// The constrained draw order for rendering. If no constraints modify the
  /// draw order, this is the same as getPose(). Otherwise it is a copy of
  /// getPose() modified by constraints.
  ArraySlot get appliedPose {
    final result = SpineBindings.bindings.spine_draw_order_get_applied_pose(_ptr);
    return ArraySlot.fromPointer(result);
  }
}
