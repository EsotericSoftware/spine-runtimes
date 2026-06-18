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
import 'curve_timeline.dart';
import 'mix_from.dart';

/// The base class for a CurveTimeline that sets one property.
abstract class CurveTimeline1 extends CurveTimeline {
  final Pointer<spine_curve_timeline1_wrapper> _ptr;

  CurveTimeline1.fromPointer(this._ptr)
      : super.fromPointer(SpineBindings.bindings.spine_curve_timeline1_cast_to_curve_timeline(_ptr));

  /// Get the native pointer for FFI calls
  @override
  Pointer get nativePtr => _ptr;

  /// Sets the time and value for the specified frame.
  ///
  /// [frame] Between 0 and frameCount, inclusive.
  /// [time] The frame time in seconds.
  void setFrame(int frame, double time, double value) {
    SpineBindings.bindings.spine_curve_timeline1_set_frame(_ptr, frame, time, value);
  }

  /// Returns the interpolated value for the specified time.
  double getCurveValue(double time) {
    final result = SpineBindings.bindings.spine_curve_timeline1_get_curve_value(_ptr, time);
    return result;
  }

  double getRelativeValue(double time, double alpha, MixFrom from, bool add, double current, double setup) {
    final result = SpineBindings.bindings
        .spine_curve_timeline1_get_relative_value(_ptr, time, alpha, from.value, add, current, setup);
    return result;
  }

  double getScaleValue(double time, double alpha, MixFrom from, bool add, bool out, double current, double setup) {
    final result = SpineBindings.bindings
        .spine_curve_timeline1_get_scale_value(_ptr, time, alpha, from.value, add, out, current, setup);
    return result;
  }

  static double beforeFirstKey(MixFrom from, double alpha, double current, double setup) {
    final result = SpineBindings.bindings.spine_curve_timeline1_before_first_key(from.value, alpha, current, setup);
    return result;
  }

  double getAbsoluteValue(double time, double alpha, MixFrom from, bool add, double current, double setup) {
    final result = SpineBindings.bindings
        .spine_curve_timeline1_get_absolute_value_1(_ptr, time, alpha, from.value, add, current, setup);
    return result;
  }

  double getAbsoluteValue2(
      double time, double alpha, MixFrom from, bool add, double current, double setup, double value) {
    final result = SpineBindings.bindings
        .spine_curve_timeline1_get_absolute_value_2(_ptr, time, alpha, from.value, add, current, setup, value);
    return result;
  }
}
