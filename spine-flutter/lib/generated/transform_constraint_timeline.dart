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
import 'constraint_timeline.dart';
import 'curve_timeline.dart';

/// Changes a transform constraint's rotate, x, y, scaleX, scaleY, and shearY
/// mix values.
class TransformConstraintTimeline extends CurveTimeline implements ConstraintTimeline {
  final Pointer<spine_transform_constraint_timeline_wrapper> _ptr;

  TransformConstraintTimeline.fromPointer(this._ptr)
      : super.fromPointer(SpineBindings.bindings.spine_transform_constraint_timeline_cast_to_curve_timeline(_ptr));

  /// Get the native pointer for FFI calls
  @override
  Pointer get nativePtr => _ptr;

  factory TransformConstraintTimeline(int frameCount, int bezierCount, int transformConstraintIndex) {
    final ptr = SpineBindings.bindings
        .spine_transform_constraint_timeline_create(frameCount, bezierCount, transformConstraintIndex);
    return TransformConstraintTimeline.fromPointer(ptr);
  }

  void dispose() {
    SpineBindings.bindings.spine_transform_constraint_timeline_dispose(_ptr);
  }

  /// Sets the time, rotate mix, translate mix, scale mix, and shear mix for the
  /// specified frame.
  ///
  /// [frame] Between 0 and frameCount, inclusive.
  /// [time] The frame time in seconds.
  void setFrame(int frame, double time, double mixRotate, double mixX, double mixY, double mixScaleX, double mixScaleY,
      double mixShearY) {
    SpineBindings.bindings.spine_transform_constraint_timeline_set_frame(
        _ptr, frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY);
  }

  @override
  int get constraintIndex {
    final result = SpineBindings.bindings.spine_transform_constraint_timeline_get_constraint_index(_ptr);
    return result;
  }

  @override
  set constraintIndex(int value) {
    SpineBindings.bindings.spine_transform_constraint_timeline_set_constraint_index(_ptr, value);
  }
}
