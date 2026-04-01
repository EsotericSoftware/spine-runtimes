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
import 'rtti.dart';
import 'constraint.dart';
import 'physics.dart';
import 'posed.dart';
import 'posed_active.dart';
import 'skeleton.dart';
import 'slider_data.dart';
import 'slider_pose.dart';

/// Applies an animation based on either the slider's SliderPose::getTime() or a
/// bone's transform property.
///
/// See https://esotericsoftware.com/spine-sliders Sliders in the Spine User
/// Guide. Non-exported base class that inherits from the template
abstract class SliderBase extends PosedActive implements Posed, Constraint {
  final Pointer<spine_slider_base_wrapper> _ptr;

  SliderBase.fromPointer(this._ptr)
      : super.fromPointer(SpineBindings.bindings.spine_slider_base_cast_to_posed_active(_ptr));

  /// Get the native pointer for FFI calls
  @override
  Pointer get nativePtr => _ptr;

  @override
  SliderData get data {
    final result = SpineBindings.bindings.spine_slider_base_get_data(_ptr);
    return SliderData.fromPointer(result);
  }

  /// The unconstrained pose for this object, set by animations and application
  /// code.
  SliderPose get pose {
    final result = SpineBindings.bindings.spine_slider_base_get_pose(_ptr);
    return SliderPose.fromPointer(result);
  }

  /// The pose to use for rendering. If no constraints modify this pose, this is
  /// the same as getPose(). Otherwise it is a copy of getPose() modified by
  /// constraints.
  SliderPose get appliedPose {
    final result = SpineBindings.bindings.spine_slider_base_get_applied_pose(_ptr);
    return SliderPose.fromPointer(result);
  }

  /// Sets the constrained pose to the unconstrained pose, as a starting point
  /// for constraints to be applied.
  @override
  void resetConstrained() {
    SpineBindings.bindings.spine_slider_base_reset_constrained(_ptr);
  }

  /// Sets the applied pose to the constrained pose, in anticipation of the
  /// applied pose being modified by constraints.
  @override
  void constrained() {
    SpineBindings.bindings.spine_slider_base_constrained(_ptr);
  }

  @override
  bool get isPoseEqualToApplied {
    final result = SpineBindings.bindings.spine_slider_base_is_pose_equal_to_applied(_ptr);
    return result;
  }

  @override
  Rtti get rtti {
    final result = SpineBindings.bindings.spine_slider_base_get_rtti(_ptr);
    return Rtti.fromPointer(result);
  }

  @override
  void sort(Skeleton skeleton) {
    SpineBindings.bindings.spine_slider_base_sort(_ptr, skeleton.nativePtr.cast());
  }

  @override
  bool get isSourceActive {
    final result = SpineBindings.bindings.spine_slider_base_is_source_active(_ptr);
    return result;
  }

  /// Inherited from Update
  @override
  void update(Skeleton skeleton, Physics physics) {
    SpineBindings.bindings.spine_slider_base_update(_ptr, skeleton.nativePtr.cast(), physics.value);
  }

  static Rtti rttiStatic() {
    final result = SpineBindings.bindings.spine_slider_base_rtti();
    return Rtti.fromPointer(result);
  }
}
