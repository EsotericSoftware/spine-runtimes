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
import 'attachment.dart';
import 'bone.dart';
import 'bounding_box_attachment.dart';
import 'clipping_attachment.dart';
import 'color.dart';
import 'draw_order.dart';
import 'mesh_attachment.dart';
import 'path_attachment.dart';
import 'physics.dart';
import 'point_attachment.dart';
import 'posed.dart';
import 'region_attachment.dart';
import 'skeleton_data.dart';
import 'skin.dart';
import 'slot.dart';

/// Stores bones and slots to be posed by animations and application code.
/// Multiple skeleton instances can share the same SkeletonData, including
/// animations, attachments, and skins.
///
/// After posing, call updateWorldTransform(Physics) to apply constraints and
/// compute world transforms for rendering.
class Skeleton {
  final Pointer<spine_skeleton_wrapper> _ptr;

  Skeleton.fromPointer(this._ptr);

  /// Get the native pointer for FFI calls
  Pointer get nativePtr => _ptr;

  factory Skeleton(SkeletonData skeletonData) {
    final ptr = SpineBindings.bindings.spine_skeleton_create(skeletonData.nativePtr.cast());
    return Skeleton.fromPointer(ptr);
  }

  void dispose() {
    SpineBindings.bindings.spine_skeleton_dispose(_ptr);
  }

  /// Caches information about bones and constraints. Must be called if the
  /// active skin is modified or if bones, constraints, or weighted path
  /// attachments are added or removed.
  void updateCache() {
    SpineBindings.bindings.spine_skeleton_update_cache(_ptr);
  }

  void printUpdateCache() {
    SpineBindings.bindings.spine_skeleton_print_update_cache(_ptr);
  }

  void constrained(Posed object) {
    SpineBindings.bindings.spine_skeleton_constrained(_ptr, object.nativePtr.cast());
  }

  void sortBone(Bone? bone) {
    SpineBindings.bindings.spine_skeleton_sort_bone(_ptr, bone?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  static void sortReset(ArrayBone bones) {
    SpineBindings.bindings.spine_skeleton_sort_reset(bones.nativePtr.cast());
  }

  /// Updates the world transform for each bone and applies all constraints.
  ///
  /// See [World
  /// transforms](http://esotericsoftware.com/spine-runtime-skeletons#World-transforms)
  /// in the Spine Runtimes Guide.
  void updateWorldTransform(Physics physics) {
    SpineBindings.bindings.spine_skeleton_update_world_transform(_ptr, physics.value);
  }

  /// Sets the bones, constraints, and slots to their setup pose values.
  void setupPose() {
    SpineBindings.bindings.spine_skeleton_setup_pose(_ptr);
  }

  /// Sets the bones and constraints to their setup pose values.
  void setupPoseBones() {
    SpineBindings.bindings.spine_skeleton_setup_pose_bones(_ptr);
  }

  void setupPoseSlots() {
    SpineBindings.bindings.spine_skeleton_setup_pose_slots(_ptr);
  }

  SkeletonData get data {
    final result = SpineBindings.bindings.spine_skeleton_get_data(_ptr);
    return SkeletonData.fromPointer(result);
  }

  ArrayBone get bones {
    final result = SpineBindings.bindings.spine_skeleton_get_bones(_ptr);
    return ArrayBone.fromPointer(result);
  }

  ArrayUpdate get updateCacheList {
    final result = SpineBindings.bindings.spine_skeleton_get_update_cache(_ptr);
    return ArrayUpdate.fromPointer(result);
  }

  Bone? get rootBone {
    final result = SpineBindings.bindings.spine_skeleton_get_root_bone(_ptr);
    return result.address == 0 ? null : Bone.fromPointer(result);
  }

  /// Returns May be NULL.
  Bone? findBone(String boneName) {
    final result = SpineBindings.bindings.spine_skeleton_find_bone(_ptr, boneName.toNativeUtf8().cast<Char>());
    return result.address == 0 ? null : Bone.fromPointer(result);
  }

  /// The skeleton's slots. To add a slot, also add it to DrawOrder::getPose().
  ArraySlot get slots {
    final result = SpineBindings.bindings.spine_skeleton_get_slots(_ptr);
    return ArraySlot.fromPointer(result);
  }

  /// Returns May be NULL.
  Slot? findSlot(String slotName) {
    final result = SpineBindings.bindings.spine_skeleton_find_slot(_ptr, slotName.toNativeUtf8().cast<Char>());
    return result.address == 0 ? null : Slot.fromPointer(result);
  }

  /// The skeleton's draw order. Use DrawOrder::getAppliedPose() for rendering
  /// and DrawOrder::getPose() for changing the draw order.
  DrawOrder get drawOrder {
    final result = SpineBindings.bindings.spine_skeleton_get_draw_order(_ptr);
    return DrawOrder.fromPointer(result);
  }

  Skin? get skin {
    final result = SpineBindings.bindings.spine_skeleton_get_skin(_ptr);
    return result.address == 0 ? null : Skin.fromPointer(result);
  }

  /// A convenience method to set an attachment by finding the slot with
  /// findSlot(String), finding the attachment with getAttachment(int, String),
  /// then setting the slot's SlotPose::getAttachment().
  ///
  /// [placeholderName] May be empty.
  void setAttachment(String slotName, String placeholderName) {
    SpineBindings.bindings.spine_skeleton_set_attachment(
        _ptr, slotName.toNativeUtf8().cast<Char>(), placeholderName.toNativeUtf8().cast<Char>());
  }

  ArrayConstraint get constraints {
    final result = SpineBindings.bindings.spine_skeleton_get_constraints(_ptr);
    return ArrayConstraint.fromPointer(result);
  }

  /// The skeleton's physics constraints.
  ArrayPhysicsConstraint get physicsConstraints {
    final result = SpineBindings.bindings.spine_skeleton_get_physics_constraints(_ptr);
    return ArrayPhysicsConstraint.fromPointer(result);
  }

  Color get color {
    final result = SpineBindings.bindings.spine_skeleton_get_color(_ptr);
    return Color.fromPointer(result);
  }

  double get scaleX {
    final result = SpineBindings.bindings.spine_skeleton_get_scale_x(_ptr);
    return result;
  }

  set scaleX(double value) {
    SpineBindings.bindings.spine_skeleton_set_scale_x(_ptr, value);
  }

  double get scaleY {
    final result = SpineBindings.bindings.spine_skeleton_get_scale_y(_ptr);
    return result;
  }

  set scaleY(double value) {
    SpineBindings.bindings.spine_skeleton_set_scale_y(_ptr, value);
  }

  void setScale(double scaleX, double scaleY) {
    SpineBindings.bindings.spine_skeleton_set_scale(_ptr, scaleX, scaleY);
  }

  double get x {
    final result = SpineBindings.bindings.spine_skeleton_get_x(_ptr);
    return result;
  }

  set x(double value) {
    SpineBindings.bindings.spine_skeleton_set_x(_ptr, value);
  }

  double get y {
    final result = SpineBindings.bindings.spine_skeleton_get_y(_ptr);
    return result;
  }

  set y(double value) {
    SpineBindings.bindings.spine_skeleton_set_y(_ptr, value);
  }

  void setPosition(double x, double y) {
    SpineBindings.bindings.spine_skeleton_set_position(_ptr, x, y);
  }

  /// The x component of a vector that defines the direction
  /// PhysicsConstraintPose::getWind() is applied.
  double get windX {
    final result = SpineBindings.bindings.spine_skeleton_get_wind_x(_ptr);
    return result;
  }

  set windX(double value) {
    SpineBindings.bindings.spine_skeleton_set_wind_x(_ptr, value);
  }

  /// The y component of a vector that defines the direction
  /// PhysicsConstraintPose::getWind() is applied.
  double get windY {
    final result = SpineBindings.bindings.spine_skeleton_get_wind_y(_ptr);
    return result;
  }

  set windY(double value) {
    SpineBindings.bindings.spine_skeleton_set_wind_y(_ptr, value);
  }

  /// The x component of a vector that defines the direction
  /// PhysicsConstraintPose::getGravity() is applied.
  double get gravityX {
    final result = SpineBindings.bindings.spine_skeleton_get_gravity_x(_ptr);
    return result;
  }

  set gravityX(double value) {
    SpineBindings.bindings.spine_skeleton_set_gravity_x(_ptr, value);
  }

  /// The y component of a vector that defines the direction
  /// PhysicsConstraintPose::getGravity() is applied.
  double get gravityY {
    final result = SpineBindings.bindings.spine_skeleton_get_gravity_y(_ptr);
    return result;
  }

  set gravityY(double value) {
    SpineBindings.bindings.spine_skeleton_set_gravity_y(_ptr, value);
  }

  /// Rotates the physics constraint so next {
  void physicsTranslate(double x, double y) {
    SpineBindings.bindings.spine_skeleton_physics_translate(_ptr, x, y);
  }

  /// Calls {
  void physicsRotate(double x, double y, double degrees) {
    SpineBindings.bindings.spine_skeleton_physics_rotate(_ptr, x, y, degrees);
  }

  /// Returns the skeleton's time, used for time-based manipulations, such as
  /// PhysicsConstraint.
  ///
  /// See update().
  double get time {
    final result = SpineBindings.bindings.spine_skeleton_get_time(_ptr);
    return result;
  }

  set time(double value) {
    SpineBindings.bindings.spine_skeleton_set_time(_ptr, value);
  }

  void update(double delta) {
    SpineBindings.bindings.spine_skeleton_update(_ptr, delta);
  }

  /// Sets a skin by name (see setSkin).
  void setSkin(String skinName) {
    SpineBindings.bindings.spine_skeleton_set_skin_1(_ptr, skinName.toNativeUtf8().cast<Char>());
  }

  /// Sets the skin used to look up attachments before looking in
  /// SkeletonData::getDefaultSkin(). If the skin is changed, updateCache() is
  /// called.
  ///
  /// Attachments from the new skin are attached if the corresponding attachment
  /// from the old skin was attached. If there was no old skin, each slot's
  /// setup pose placeholder attachment is attached from the new skin.
  ///
  /// After changing the skin, the visible attachments can be reset to those
  /// attached in the setup pose by calling setupPoseSlots(). Also,
  /// AnimationState::apply(Skeleton & ) is often called before the next time
  /// the skeleton is rendered so attachment keys in the current animation(s)
  /// can hide or show attachments from the new skin.
  ///
  /// [newSkin] May be NULL.
  void setSkin2(Skin? newSkin) {
    SpineBindings.bindings.spine_skeleton_set_skin_2(_ptr, newSkin?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// Finds an attachment by looking in getSkin() and
  /// SkeletonData::getDefaultSkin() using the slot name and skin placeholder
  /// name. First the skin is checked and if the attachment was not found, the
  /// default skin is checked.
  ///
  /// Returns May be NULL.
  Attachment? getAttachment(String slotName, String placeholderName) {
    final result = SpineBindings.bindings.spine_skeleton_get_attachment_1(
        _ptr, slotName.toNativeUtf8().cast<Char>(), placeholderName.toNativeUtf8().cast<Char>());
    if (result.address == 0) return null;
    final rtti = SpineBindings.bindings.spine_attachment_get_rtti(result);
    final className = SpineBindings.bindings.spine_rtti_get_class_name(rtti).cast<Utf8>().toDartString();
    switch (className) {
      case 'BoundingBoxAttachment':
        final castedPtr = SpineBindings.bindings.spine_attachment_cast_to_bounding_box_attachment(result);
        return BoundingBoxAttachment.fromPointer(castedPtr);
      case 'ClippingAttachment':
        final castedPtr = SpineBindings.bindings.spine_attachment_cast_to_clipping_attachment(result);
        return ClippingAttachment.fromPointer(castedPtr);
      case 'MeshAttachment':
        final castedPtr = SpineBindings.bindings.spine_attachment_cast_to_mesh_attachment(result);
        return MeshAttachment.fromPointer(castedPtr);
      case 'PathAttachment':
        final castedPtr = SpineBindings.bindings.spine_attachment_cast_to_path_attachment(result);
        return PathAttachment.fromPointer(castedPtr);
      case 'PointAttachment':
        final castedPtr = SpineBindings.bindings.spine_attachment_cast_to_point_attachment(result);
        return PointAttachment.fromPointer(castedPtr);
      case 'RegionAttachment':
        final castedPtr = SpineBindings.bindings.spine_attachment_cast_to_region_attachment(result);
        return RegionAttachment.fromPointer(castedPtr);
      default:
        throw UnsupportedError('Unknown concrete type: $className for abstract class Attachment');
    }
  }

  /// Finds an attachment by looking in getSkin() and
  /// SkeletonData::getDefaultSkin() using the slot index and skin placeholder
  /// name. First the skin is checked and if the attachment was not found, the
  /// default skin is checked.
  ///
  /// Returns May be NULL.
  Attachment? getAttachment2(int slotIndex, String placeholderName) {
    final result = SpineBindings.bindings
        .spine_skeleton_get_attachment_2(_ptr, slotIndex, placeholderName.toNativeUtf8().cast<Char>());
    if (result.address == 0) return null;
    final rtti = SpineBindings.bindings.spine_attachment_get_rtti(result);
    final className = SpineBindings.bindings.spine_rtti_get_class_name(rtti).cast<Utf8>().toDartString();
    switch (className) {
      case 'BoundingBoxAttachment':
        final castedPtr = SpineBindings.bindings.spine_attachment_cast_to_bounding_box_attachment(result);
        return BoundingBoxAttachment.fromPointer(castedPtr);
      case 'ClippingAttachment':
        final castedPtr = SpineBindings.bindings.spine_attachment_cast_to_clipping_attachment(result);
        return ClippingAttachment.fromPointer(castedPtr);
      case 'MeshAttachment':
        final castedPtr = SpineBindings.bindings.spine_attachment_cast_to_mesh_attachment(result);
        return MeshAttachment.fromPointer(castedPtr);
      case 'PathAttachment':
        final castedPtr = SpineBindings.bindings.spine_attachment_cast_to_path_attachment(result);
        return PathAttachment.fromPointer(castedPtr);
      case 'PointAttachment':
        final castedPtr = SpineBindings.bindings.spine_attachment_cast_to_point_attachment(result);
        return PointAttachment.fromPointer(castedPtr);
      case 'RegionAttachment':
        final castedPtr = SpineBindings.bindings.spine_attachment_cast_to_region_attachment(result);
        return RegionAttachment.fromPointer(castedPtr);
      default:
        throw UnsupportedError('Unknown concrete type: $className for abstract class Attachment');
    }
  }

  set setColor(Color value) {
    SpineBindings.bindings.spine_skeleton_set_color_1(_ptr, value.nativePtr.cast());
  }

  void setColor2(double r, double g, double b, double a) {
    SpineBindings.bindings.spine_skeleton_set_color_2(_ptr, r, g, b, a);
  }
}
