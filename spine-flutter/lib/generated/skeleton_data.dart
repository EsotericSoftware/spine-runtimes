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
import 'animation.dart';
import 'arrays.dart';
import 'bone_data.dart';
import 'event_data.dart';
import 'skin.dart';
import 'slot_data.dart';

/// Stores the setup pose and all of the stateless data for a skeleton.
///
/// See Data objects in the Spine Runtimes Guide.
class SkeletonData {
  final Pointer<spine_skeleton_data_wrapper> _ptr;

  SkeletonData.fromPointer(this._ptr);

  /// Get the native pointer for FFI calls
  Pointer get nativePtr => _ptr;

  factory SkeletonData() {
    final ptr = SpineBindings.bindings.spine_skeleton_data_create();
    return SkeletonData.fromPointer(ptr);
  }

  void dispose() {
    SpineBindings.bindings.spine_skeleton_data_dispose(_ptr);
  }

  /// Finds a bone by comparing each bone's name. It is more efficient to cache
  /// the results of this method than to call it multiple times.
  ///
  /// Returns May be NULL.
  BoneData? findBone(String boneName) {
    final result = SpineBindings.bindings.spine_skeleton_data_find_bone(_ptr, boneName.toNativeUtf8().cast<Char>());
    return result.address == 0 ? null : BoneData.fromPointer(result);
  }

  /// Returns May be NULL.
  SlotData? findSlot(String slotName) {
    final result = SpineBindings.bindings.spine_skeleton_data_find_slot(_ptr, slotName.toNativeUtf8().cast<Char>());
    return result.address == 0 ? null : SlotData.fromPointer(result);
  }

  /// Returns May be NULL.
  Skin? findSkin(String skinName) {
    final result = SpineBindings.bindings.spine_skeleton_data_find_skin(_ptr, skinName.toNativeUtf8().cast<Char>());
    return result.address == 0 ? null : Skin.fromPointer(result);
  }

  /// Returns May be NULL.
  EventData? findEvent(String eventDataName) {
    final result =
        SpineBindings.bindings.spine_skeleton_data_find_event(_ptr, eventDataName.toNativeUtf8().cast<Char>());
    return result.address == 0 ? null : EventData.fromPointer(result);
  }

  /// Returns May be NULL.
  Animation? findAnimation(String animationName) {
    final result =
        SpineBindings.bindings.spine_skeleton_data_find_animation(_ptr, animationName.toNativeUtf8().cast<Char>());
    return result.address == 0 ? null : Animation.fromPointer(result);
  }

  /// Collects animations used by slider constraints.
  ArrayAnimation findSliderAnimations(ArrayAnimation animations) {
    final result = SpineBindings.bindings.spine_skeleton_data_find_slider_animations(_ptr, animations.nativePtr.cast());
    return ArrayAnimation.fromPointer(result);
  }

  /// The skeleton's name, which by default is the name of the skeleton data
  /// file when possible, or null when a name hasn't been set.
  String get name {
    final result = SpineBindings.bindings.spine_skeleton_data_get_name(_ptr);
    return result.cast<Utf8>().toDartString();
  }

  set name(String value) {
    SpineBindings.bindings.spine_skeleton_data_set_name(_ptr, value.toNativeUtf8().cast<Char>());
  }

  /// The skeleton's bones, sorted parent first. The root bone is always the
  /// first bone.
  ArrayBoneData get bones {
    final result = SpineBindings.bindings.spine_skeleton_data_get_bones(_ptr);
    return ArrayBoneData.fromPointer(result);
  }

  /// The skeleton's slots in the setup pose draw order.
  ArraySlotData get slots {
    final result = SpineBindings.bindings.spine_skeleton_data_get_slots(_ptr);
    return ArraySlotData.fromPointer(result);
  }

  /// All skins, including the default skin.
  ArraySkin get skins {
    final result = SpineBindings.bindings.spine_skeleton_data_get_skins(_ptr);
    return ArraySkin.fromPointer(result);
  }

  /// The skeleton's default skin. By default this skin contains all attachments
  /// that were not in a skin in Spine.
  ///
  /// Returns May be NULL.
  Skin? get defaultSkin {
    final result = SpineBindings.bindings.spine_skeleton_data_get_default_skin(_ptr);
    return result.address == 0 ? null : Skin.fromPointer(result);
  }

  set defaultSkin(Skin? value) {
    SpineBindings.bindings
        .spine_skeleton_data_set_default_skin(_ptr, value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// The skeleton's events.
  ArrayEventData get events {
    final result = SpineBindings.bindings.spine_skeleton_data_get_events(_ptr);
    return ArrayEventData.fromPointer(result);
  }

  /// The skeleton's animations.
  ArrayAnimation get animations {
    final result = SpineBindings.bindings.spine_skeleton_data_get_animations(_ptr);
    return ArrayAnimation.fromPointer(result);
  }

  /// The skeleton's constraints.
  ArrayConstraintData get constraints {
    final result = SpineBindings.bindings.spine_skeleton_data_get_constraints(_ptr);
    return ArrayConstraintData.fromPointer(result);
  }

  /// The X coordinate of the skeleton's axis aligned bounding box in the setup
  /// pose.
  double get x {
    final result = SpineBindings.bindings.spine_skeleton_data_get_x(_ptr);
    return result;
  }

  set x(double value) {
    SpineBindings.bindings.spine_skeleton_data_set_x(_ptr, value);
  }

  /// The Y coordinate of the skeleton's axis aligned bounding box in the setup
  /// pose.
  double get y {
    final result = SpineBindings.bindings.spine_skeleton_data_get_y(_ptr);
    return result;
  }

  set y(double value) {
    SpineBindings.bindings.spine_skeleton_data_set_y(_ptr, value);
  }

  /// The width of the skeleton's axis aligned bounding box in the setup pose.
  double get width {
    final result = SpineBindings.bindings.spine_skeleton_data_get_width(_ptr);
    return result;
  }

  set width(double value) {
    SpineBindings.bindings.spine_skeleton_data_set_width(_ptr, value);
  }

  /// The height of the skeleton's axis aligned bounding box in the setup pose.
  double get height {
    final result = SpineBindings.bindings.spine_skeleton_data_get_height(_ptr);
    return result;
  }

  set height(double value) {
    SpineBindings.bindings.spine_skeleton_data_set_height(_ptr, value);
  }

  /// Baseline scale factor for applying physics and other effects based on
  /// distance to non-scalable properties, such as angle or scale. Default is
  /// 100.
  double get referenceScale {
    final result = SpineBindings.bindings.spine_skeleton_data_get_reference_scale(_ptr);
    return result;
  }

  set referenceScale(double value) {
    SpineBindings.bindings.spine_skeleton_data_set_reference_scale(_ptr, value);
  }

  /// The Spine version used to export this data, or NULL.
  String get version {
    final result = SpineBindings.bindings.spine_skeleton_data_get_version(_ptr);
    return result.cast<Utf8>().toDartString();
  }

  set version(String value) {
    SpineBindings.bindings.spine_skeleton_data_set_version(_ptr, value.toNativeUtf8().cast<Char>());
  }

  /// The skeleton data hash. This value will change if any of the skeleton data
  /// has changed.
  String get hash {
    final result = SpineBindings.bindings.spine_skeleton_data_get_hash(_ptr);
    return result.cast<Utf8>().toDartString();
  }

  set hash(String value) {
    SpineBindings.bindings.spine_skeleton_data_set_hash(_ptr, value.toNativeUtf8().cast<Char>());
  }

  /// The path to the images directory as defined in Spine, or null if
  /// nonessential data was not exported.
  String get imagesPath {
    final result = SpineBindings.bindings.spine_skeleton_data_get_images_path(_ptr);
    return result.cast<Utf8>().toDartString();
  }

  set imagesPath(String value) {
    SpineBindings.bindings.spine_skeleton_data_set_images_path(_ptr, value.toNativeUtf8().cast<Char>());
  }

  /// The path to the audio directory as defined in Spine, or null if
  /// nonessential data was not exported.
  String get audioPath {
    final result = SpineBindings.bindings.spine_skeleton_data_get_audio_path(_ptr);
    return result.cast<Utf8>().toDartString();
  }

  set audioPath(String value) {
    SpineBindings.bindings.spine_skeleton_data_set_audio_path(_ptr, value.toNativeUtf8().cast<Char>());
  }

  /// The dopesheet FPS in Spine. Available only when nonessential data was
  /// exported.
  double get fps {
    final result = SpineBindings.bindings.spine_skeleton_data_get_fps(_ptr);
    return result;
  }

  set fps(double value) {
    SpineBindings.bindings.spine_skeleton_data_set_fps(_ptr, value);
  }
}
