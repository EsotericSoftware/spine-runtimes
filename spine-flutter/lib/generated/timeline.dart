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
import 'arrays.dart';
import 'mix_from.dart';
import 'skeleton.dart';

/// The base class for all timelines.
///
/// See Applying Animations in the Spine Runtimes Guide.
abstract class Timeline {
  final Pointer<spine_timeline_wrapper> _ptr;

  Timeline.fromPointer(this._ptr);

  /// Get the native pointer for FFI calls
  Pointer get nativePtr => _ptr;

  Rtti get rtti {
    final result = SpineBindings.bindings.spine_timeline_get_rtti(_ptr);
    return Rtti.fromPointer(result);
  }

  /// Applies this timeline to the skeleton.
  ///
  /// See Applying Animations in the Spine Runtimes Guide.
  ///
  /// [skeleton] The skeleton the timeline is applied to. This provides access to the bones, slots, and other skeleton components the timelines may change.
  /// [lastTime] The last time in seconds this timeline was applied. Some timelines trigger only at discrete times, in which case all keys are triggered between lastTime (exclusive) and time (inclusive). Pass -1 the first time a timeline is applied to ensure frame 0 is triggered.
  /// [time] The time in seconds the skeleton is being posed for. Timelines find the frame before and after this time and interpolate between the frame values.
  /// [events] If any events are fired, they are added to this list. Can be NULL to ignore fired events or if no timelines fire events.
  /// [alpha] 0 applies setup or current values (depending on from), 1 uses timeline values, and intermediate values interpolate between them. Adjusting alpha over time can mix a timeline in or out.
  /// [from] If true, alpha transitions between setup and timeline values, setup values are used before the first frame (current values are not used). If false, alpha transitions between current and timeline values, no change is made before the first frame.
  /// [add] If true, for timelines that support it, their values are added to the setup or current values (depending on from).
  /// [out] True when the animation is mixing out, else it is mixing in. Used by timelines that perform instant transitions.
  /// [appliedPose] True to modify getAppliedPose(), else getPose() is modified.
  void apply(Skeleton skeleton, double lastTime, double time, ArrayEvent? events, double alpha, MixFrom from, bool add,
      bool out, bool appliedPose) {
    SpineBindings.bindings.spine_timeline_apply(_ptr, skeleton.nativePtr.cast(), lastTime, time,
        events?.nativePtr.cast() ?? Pointer.fromAddress(0), alpha, from.value, add, out, appliedPose);
  }

  /// True if this timeline supports additive blending.
  bool get additive {
    final result = SpineBindings.bindings.spine_timeline_get_additive(_ptr);
    return result;
  }

  /// True if this timeline sets values instantaneously and does not support
  /// interpolation between frames.
  bool get instant {
    final result = SpineBindings.bindings.spine_timeline_get_instant(_ptr);
    return result;
  }

  int get frameEntries {
    final result = SpineBindings.bindings.spine_timeline_get_frame_entries(_ptr);
    return result;
  }

  int get frameCount {
    final result = SpineBindings.bindings.spine_timeline_get_frame_count(_ptr);
    return result;
  }

  ArrayFloat get frames {
    final result = SpineBindings.bindings.spine_timeline_get_frames(_ptr);
    return ArrayFloat.fromPointer(result);
  }

  double get duration {
    final result = SpineBindings.bindings.spine_timeline_get_duration(_ptr);
    return result;
  }

  ArrayPropertyId get propertyIds {
    final result = SpineBindings.bindings.spine_timeline_get_property_ids(_ptr);
    return ArrayPropertyId.fromPointer(result);
  }

  static Rtti rttiStatic() {
    final result = SpineBindings.bindings.spine_timeline_rtti();
    return Rtti.fromPointer(result);
  }
}
