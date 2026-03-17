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
import '../native_array.dart';
import 'alpha_timeline.dart';
import 'animation.dart';
import 'atlas_page.dart';
import 'atlas_region.dart';
import 'attachment.dart';
import 'attachment_timeline.dart';
import 'bone.dart';
import 'bone_data.dart';
import 'bone_pose.dart';
import 'bounding_box_attachment.dart';
import 'clipping_attachment.dart';
import 'constraint.dart';
import 'constraint_data.dart';
import 'deform_timeline.dart';
import 'draw_order_folder_timeline.dart';
import 'draw_order_timeline.dart';
import 'event.dart';
import 'event_data.dart';
import 'event_timeline.dart';
import 'from_property.dart';
import 'from_rotate.dart';
import 'from_scale_x.dart';
import 'from_scale_y.dart';
import 'from_shear_y.dart';
import 'from_x.dart';
import 'from_y.dart';
import 'ik_constraint.dart';
import 'ik_constraint_data.dart';
import 'ik_constraint_timeline.dart';
import 'inherit_timeline.dart';
import 'mesh_attachment.dart';
import 'path_attachment.dart';
import 'path_constraint.dart';
import 'path_constraint_data.dart';
import 'path_constraint_mix_timeline.dart';
import 'path_constraint_position_timeline.dart';
import 'path_constraint_spacing_timeline.dart';
import 'physics_constraint.dart';
import 'physics_constraint_damping_timeline.dart';
import 'physics_constraint_data.dart';
import 'physics_constraint_gravity_timeline.dart';
import 'physics_constraint_inertia_timeline.dart';
import 'physics_constraint_mass_timeline.dart';
import 'physics_constraint_mix_timeline.dart';
import 'physics_constraint_reset_timeline.dart';
import 'physics_constraint_strength_timeline.dart';
import 'physics_constraint_wind_timeline.dart';
import 'point_attachment.dart';
import 'polygon.dart';
import 'region_attachment.dart';
import 'rgb2_timeline.dart';
import 'rgb_timeline.dart';
import 'rgba2_timeline.dart';
import 'rgba_timeline.dart';
import 'rotate_timeline.dart';
import 'scale_timeline.dart';
import 'scale_x_timeline.dart';
import 'scale_y_timeline.dart';
import 'sequence_timeline.dart';
import 'shear_timeline.dart';
import 'shear_x_timeline.dart';
import 'shear_y_timeline.dart';
import 'skin.dart';
import 'slider.dart';
import 'slider_data.dart';
import 'slider_mix_timeline.dart';
import 'slider_timeline.dart';
import 'slot.dart';
import 'slot_data.dart';
import 'texture_region.dart';
import 'timeline.dart';
import 'to_property.dart';
import 'to_rotate.dart';
import 'to_scale_x.dart';
import 'to_scale_y.dart';
import 'to_shear_y.dart';
import 'to_x.dart';
import 'to_y.dart';
import 'track_entry.dart';
import 'transform_constraint.dart';
import 'transform_constraint_data.dart';
import 'transform_constraint_timeline.dart';
import 'translate_timeline.dart';
import 'translate_x_timeline.dart';
import 'translate_y_timeline.dart';
import 'update.dart';

/// ArrayFloat wrapper
class ArrayFloat extends NativeArray<double> {
  final bool _ownsMemory;

  ArrayFloat.fromPointer(Pointer<spine_array_float_wrapper> super.ptr, {bool ownsMemory = false})
      : _ownsMemory = ownsMemory;

  /// Create a new empty array
  factory ArrayFloat() {
    final ptr = SpineBindings.bindings.spine_array_float_create();
    return ArrayFloat.fromPointer(ptr.cast(), ownsMemory: true);
  }

  /// Create a new array with the specified initial capacity
  factory ArrayFloat.withCapacity(int initialCapacity) {
    final ptr = SpineBindings.bindings.spine_array_float_create_with_capacity(initialCapacity);
    return ArrayFloat.fromPointer(ptr.cast(), ownsMemory: true);
  }

  @override
  int get length {
    return SpineBindings.bindings.spine_array_float_size(nativePtr.cast());
  }

  @override
  double operator [](int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final buffer = SpineBindings.bindings.spine_array_float_buffer(nativePtr.cast());
    return buffer.cast<Float>()[index];
  }

  @override
  set length(int newLength) {
    SpineBindings.bindings.spine_array_float_set_size(nativePtr.cast(), newLength, 0.0);
  }

  /// Adds a value to the end of this array.
  @override
  void add(double value) {
    SpineBindings.bindings.spine_array_float_add(nativePtr.cast(), value);
  }

  /// Removes all elements from this array.
  @override
  void clear() {
    SpineBindings.bindings.spine_array_float_clear(nativePtr.cast());
  }

  /// Removes the element at the given index.
  @override
  double removeAt(int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final value = this[index];
    SpineBindings.bindings.spine_array_float_remove_at(nativePtr.cast(), index);
    return value;
  }

  /// Ensures this array has at least the given capacity.
  void ensureCapacity(int capacity) {
    SpineBindings.bindings.spine_array_float_ensure_capacity(nativePtr.cast(), capacity);
  }

  /// Dispose of the native array
  /// Throws an error if the array was not created by this class (i.e., it was obtained from C)
  void dispose() {
    if (!_ownsMemory) {
      throw StateError(
          'Cannot dispose ArrayFloat that was created from C. Only arrays created via factory constructors can be disposed.');
    }
    SpineBindings.bindings.spine_array_float_dispose(nativePtr.cast());
  }
}

/// ArrayInt wrapper
class ArrayInt extends NativeArray<int> {
  final bool _ownsMemory;

  ArrayInt.fromPointer(Pointer<spine_array_int_wrapper> super.ptr, {bool ownsMemory = false})
      : _ownsMemory = ownsMemory;

  /// Create a new empty array
  factory ArrayInt() {
    final ptr = SpineBindings.bindings.spine_array_int_create();
    return ArrayInt.fromPointer(ptr.cast(), ownsMemory: true);
  }

  /// Create a new array with the specified initial capacity
  factory ArrayInt.withCapacity(int initialCapacity) {
    final ptr = SpineBindings.bindings.spine_array_int_create_with_capacity(initialCapacity);
    return ArrayInt.fromPointer(ptr.cast(), ownsMemory: true);
  }

  @override
  int get length {
    return SpineBindings.bindings.spine_array_int_size(nativePtr.cast());
  }

  @override
  int operator [](int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final buffer = SpineBindings.bindings.spine_array_int_buffer(nativePtr.cast());
    return buffer.cast<Int32>()[index];
  }

  @override
  set length(int newLength) {
    SpineBindings.bindings.spine_array_int_set_size(nativePtr.cast(), newLength, 0);
  }

  /// Adds a value to the end of this array.
  @override
  void add(int value) {
    SpineBindings.bindings.spine_array_int_add(nativePtr.cast(), value);
  }

  /// Removes all elements from this array.
  @override
  void clear() {
    SpineBindings.bindings.spine_array_int_clear(nativePtr.cast());
  }

  /// Removes the element at the given index.
  @override
  int removeAt(int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final value = this[index];
    SpineBindings.bindings.spine_array_int_remove_at(nativePtr.cast(), index);
    return value;
  }

  /// Ensures this array has at least the given capacity.
  void ensureCapacity(int capacity) {
    SpineBindings.bindings.spine_array_int_ensure_capacity(nativePtr.cast(), capacity);
  }

  /// Dispose of the native array
  /// Throws an error if the array was not created by this class (i.e., it was obtained from C)
  void dispose() {
    if (!_ownsMemory) {
      throw StateError(
          'Cannot dispose ArrayInt that was created from C. Only arrays created via factory constructors can be disposed.');
    }
    SpineBindings.bindings.spine_array_int_dispose(nativePtr.cast());
  }
}

/// ArrayUnsignedShort wrapper
class ArrayUnsignedShort extends NativeArray<int> {
  final bool _ownsMemory;

  ArrayUnsignedShort.fromPointer(Pointer<spine_array_unsigned_short_wrapper> super.ptr, {bool ownsMemory = false})
      : _ownsMemory = ownsMemory;

  /// Create a new empty array
  factory ArrayUnsignedShort() {
    final ptr = SpineBindings.bindings.spine_array_unsigned_short_create();
    return ArrayUnsignedShort.fromPointer(ptr.cast(), ownsMemory: true);
  }

  /// Create a new array with the specified initial capacity
  factory ArrayUnsignedShort.withCapacity(int initialCapacity) {
    final ptr = SpineBindings.bindings.spine_array_unsigned_short_create_with_capacity(initialCapacity);
    return ArrayUnsignedShort.fromPointer(ptr.cast(), ownsMemory: true);
  }

  @override
  int get length {
    return SpineBindings.bindings.spine_array_unsigned_short_size(nativePtr.cast());
  }

  @override
  int operator [](int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final buffer = SpineBindings.bindings.spine_array_unsigned_short_buffer(nativePtr.cast());
    return buffer.cast<Uint16>()[index];
  }

  @override
  set length(int newLength) {
    SpineBindings.bindings.spine_array_unsigned_short_set_size(nativePtr.cast(), newLength, 0);
  }

  /// Adds a value to the end of this array.
  @override
  void add(int value) {
    SpineBindings.bindings.spine_array_unsigned_short_add(nativePtr.cast(), value);
  }

  /// Removes all elements from this array.
  @override
  void clear() {
    SpineBindings.bindings.spine_array_unsigned_short_clear(nativePtr.cast());
  }

  /// Removes the element at the given index.
  @override
  int removeAt(int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final value = this[index];
    SpineBindings.bindings.spine_array_unsigned_short_remove_at(nativePtr.cast(), index);
    return value;
  }

  /// Ensures this array has at least the given capacity.
  void ensureCapacity(int capacity) {
    SpineBindings.bindings.spine_array_unsigned_short_ensure_capacity(nativePtr.cast(), capacity);
  }

  /// Dispose of the native array
  /// Throws an error if the array was not created by this class (i.e., it was obtained from C)
  void dispose() {
    if (!_ownsMemory) {
      throw StateError(
          'Cannot dispose ArrayUnsignedShort that was created from C. Only arrays created via factory constructors can be disposed.');
    }
    SpineBindings.bindings.spine_array_unsigned_short_dispose(nativePtr.cast());
  }
}

/// ArrayPropertyId wrapper
class ArrayPropertyId extends NativeArray<int> {
  final bool _ownsMemory;

  ArrayPropertyId.fromPointer(Pointer<spine_array_property_id_wrapper> super.ptr, {bool ownsMemory = false})
      : _ownsMemory = ownsMemory;

  /// Create a new empty array
  factory ArrayPropertyId() {
    final ptr = SpineBindings.bindings.spine_array_property_id_create();
    return ArrayPropertyId.fromPointer(ptr.cast(), ownsMemory: true);
  }

  /// Create a new array with the specified initial capacity
  factory ArrayPropertyId.withCapacity(int initialCapacity) {
    final ptr = SpineBindings.bindings.spine_array_property_id_create_with_capacity(initialCapacity);
    return ArrayPropertyId.fromPointer(ptr.cast(), ownsMemory: true);
  }

  @override
  int get length {
    return SpineBindings.bindings.spine_array_property_id_size(nativePtr.cast());
  }

  @override
  int operator [](int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final buffer = SpineBindings.bindings.spine_array_property_id_buffer(nativePtr.cast());
    // NOTE: This will not compile due to C API bug - buffer() returns int instead of Pointer
    return buffer.cast<Int64>()[index];
  }

  @override
  set length(int newLength) {
    SpineBindings.bindings.spine_array_property_id_set_size(nativePtr.cast(), newLength, 0);
  }

  /// Adds a value to the end of this array.
  @override
  void add(int value) {
    SpineBindings.bindings.spine_array_property_id_add(nativePtr.cast(), value);
  }

  /// Removes all elements from this array.
  @override
  void clear() {
    SpineBindings.bindings.spine_array_property_id_clear(nativePtr.cast());
  }

  /// Removes the element at the given index.
  @override
  int removeAt(int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final value = this[index];
    SpineBindings.bindings.spine_array_property_id_remove_at(nativePtr.cast(), index);
    return value;
  }

  /// Ensures this array has at least the given capacity.
  void ensureCapacity(int capacity) {
    SpineBindings.bindings.spine_array_property_id_ensure_capacity(nativePtr.cast(), capacity);
  }

  /// Dispose of the native array
  /// Throws an error if the array was not created by this class (i.e., it was obtained from C)
  void dispose() {
    if (!_ownsMemory) {
      throw StateError(
          'Cannot dispose ArrayPropertyId that was created from C. Only arrays created via factory constructors can be disposed.');
    }
    SpineBindings.bindings.spine_array_property_id_dispose(nativePtr.cast());
  }
}

/// ArrayAnimation wrapper
class ArrayAnimation extends NativeArray<Animation?> {
  final bool _ownsMemory;

  ArrayAnimation.fromPointer(Pointer<spine_array_animation_wrapper> super.ptr, {bool ownsMemory = false})
      : _ownsMemory = ownsMemory;

  /// Create a new empty array
  factory ArrayAnimation() {
    final ptr = SpineBindings.bindings.spine_array_animation_create();
    return ArrayAnimation.fromPointer(ptr.cast(), ownsMemory: true);
  }

  /// Create a new array with the specified initial capacity
  factory ArrayAnimation.withCapacity(int initialCapacity) {
    final ptr = SpineBindings.bindings.spine_array_animation_create_with_capacity(initialCapacity);
    return ArrayAnimation.fromPointer(ptr.cast(), ownsMemory: true);
  }

  @override
  int get length {
    return SpineBindings.bindings.spine_array_animation_size(nativePtr.cast());
  }

  @override
  Animation? operator [](int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final buffer = SpineBindings.bindings.spine_array_animation_buffer(nativePtr.cast());
    return buffer[index].address == 0 ? null : Animation.fromPointer(buffer[index]);
  }

  @override
  set length(int newLength) {
    SpineBindings.bindings.spine_array_animation_set_size(nativePtr.cast(), newLength, Pointer.fromAddress(0));
  }

  /// Adds a value to the end of this array.
  @override
  void add(Animation? value) {
    SpineBindings.bindings
        .spine_array_animation_add(nativePtr.cast(), value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// Removes all elements from this array.
  @override
  void clear() {
    SpineBindings.bindings.spine_array_animation_clear(nativePtr.cast());
  }

  /// Removes the element at the given index.
  @override
  Animation? removeAt(int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final value = this[index];
    SpineBindings.bindings.spine_array_animation_remove_at(nativePtr.cast(), index);
    return value;
  }

  /// Ensures this array has at least the given capacity.
  void ensureCapacity(int capacity) {
    SpineBindings.bindings.spine_array_animation_ensure_capacity(nativePtr.cast(), capacity);
  }

  /// Dispose of the native array
  /// Throws an error if the array was not created by this class (i.e., it was obtained from C)
  void dispose() {
    if (!_ownsMemory) {
      throw StateError(
          'Cannot dispose ArrayAnimation that was created from C. Only arrays created via factory constructors can be disposed.');
    }
    SpineBindings.bindings.spine_array_animation_dispose(nativePtr.cast());
  }
}

/// ArrayAtlasPage wrapper
class ArrayAtlasPage extends NativeArray<AtlasPage?> {
  final bool _ownsMemory;

  ArrayAtlasPage.fromPointer(Pointer<spine_array_atlas_page_wrapper> super.ptr, {bool ownsMemory = false})
      : _ownsMemory = ownsMemory;

  /// Create a new empty array
  factory ArrayAtlasPage() {
    final ptr = SpineBindings.bindings.spine_array_atlas_page_create();
    return ArrayAtlasPage.fromPointer(ptr.cast(), ownsMemory: true);
  }

  /// Create a new array with the specified initial capacity
  factory ArrayAtlasPage.withCapacity(int initialCapacity) {
    final ptr = SpineBindings.bindings.spine_array_atlas_page_create_with_capacity(initialCapacity);
    return ArrayAtlasPage.fromPointer(ptr.cast(), ownsMemory: true);
  }

  @override
  int get length {
    return SpineBindings.bindings.spine_array_atlas_page_size(nativePtr.cast());
  }

  @override
  AtlasPage? operator [](int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final buffer = SpineBindings.bindings.spine_array_atlas_page_buffer(nativePtr.cast());
    return buffer[index].address == 0 ? null : AtlasPage.fromPointer(buffer[index]);
  }

  @override
  set length(int newLength) {
    SpineBindings.bindings.spine_array_atlas_page_set_size(nativePtr.cast(), newLength, Pointer.fromAddress(0));
  }

  /// Adds a value to the end of this array.
  @override
  void add(AtlasPage? value) {
    SpineBindings.bindings
        .spine_array_atlas_page_add(nativePtr.cast(), value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// Removes all elements from this array.
  @override
  void clear() {
    SpineBindings.bindings.spine_array_atlas_page_clear(nativePtr.cast());
  }

  /// Removes the element at the given index.
  @override
  AtlasPage? removeAt(int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final value = this[index];
    SpineBindings.bindings.spine_array_atlas_page_remove_at(nativePtr.cast(), index);
    return value;
  }

  /// Ensures this array has at least the given capacity.
  void ensureCapacity(int capacity) {
    SpineBindings.bindings.spine_array_atlas_page_ensure_capacity(nativePtr.cast(), capacity);
  }

  /// Dispose of the native array
  /// Throws an error if the array was not created by this class (i.e., it was obtained from C)
  void dispose() {
    if (!_ownsMemory) {
      throw StateError(
          'Cannot dispose ArrayAtlasPage that was created from C. Only arrays created via factory constructors can be disposed.');
    }
    SpineBindings.bindings.spine_array_atlas_page_dispose(nativePtr.cast());
  }
}

/// ArrayAtlasRegion wrapper
class ArrayAtlasRegion extends NativeArray<AtlasRegion?> {
  final bool _ownsMemory;

  ArrayAtlasRegion.fromPointer(Pointer<spine_array_atlas_region_wrapper> super.ptr, {bool ownsMemory = false})
      : _ownsMemory = ownsMemory;

  /// Create a new empty array
  factory ArrayAtlasRegion() {
    final ptr = SpineBindings.bindings.spine_array_atlas_region_create();
    return ArrayAtlasRegion.fromPointer(ptr.cast(), ownsMemory: true);
  }

  /// Create a new array with the specified initial capacity
  factory ArrayAtlasRegion.withCapacity(int initialCapacity) {
    final ptr = SpineBindings.bindings.spine_array_atlas_region_create_with_capacity(initialCapacity);
    return ArrayAtlasRegion.fromPointer(ptr.cast(), ownsMemory: true);
  }

  @override
  int get length {
    return SpineBindings.bindings.spine_array_atlas_region_size(nativePtr.cast());
  }

  @override
  AtlasRegion? operator [](int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final buffer = SpineBindings.bindings.spine_array_atlas_region_buffer(nativePtr.cast());
    return buffer[index].address == 0 ? null : AtlasRegion.fromPointer(buffer[index]);
  }

  @override
  set length(int newLength) {
    SpineBindings.bindings.spine_array_atlas_region_set_size(nativePtr.cast(), newLength, Pointer.fromAddress(0));
  }

  /// Adds a value to the end of this array.
  @override
  void add(AtlasRegion? value) {
    SpineBindings.bindings
        .spine_array_atlas_region_add(nativePtr.cast(), value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// Removes all elements from this array.
  @override
  void clear() {
    SpineBindings.bindings.spine_array_atlas_region_clear(nativePtr.cast());
  }

  /// Removes the element at the given index.
  @override
  AtlasRegion? removeAt(int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final value = this[index];
    SpineBindings.bindings.spine_array_atlas_region_remove_at(nativePtr.cast(), index);
    return value;
  }

  /// Ensures this array has at least the given capacity.
  void ensureCapacity(int capacity) {
    SpineBindings.bindings.spine_array_atlas_region_ensure_capacity(nativePtr.cast(), capacity);
  }

  /// Dispose of the native array
  /// Throws an error if the array was not created by this class (i.e., it was obtained from C)
  void dispose() {
    if (!_ownsMemory) {
      throw StateError(
          'Cannot dispose ArrayAtlasRegion that was created from C. Only arrays created via factory constructors can be disposed.');
    }
    SpineBindings.bindings.spine_array_atlas_region_dispose(nativePtr.cast());
  }
}

/// ArrayAttachment wrapper
class ArrayAttachment extends NativeArray<Attachment?> {
  final bool _ownsMemory;

  ArrayAttachment.fromPointer(Pointer<spine_array_attachment_wrapper> super.ptr, {bool ownsMemory = false})
      : _ownsMemory = ownsMemory;

  /// Create a new empty array
  factory ArrayAttachment() {
    final ptr = SpineBindings.bindings.spine_array_attachment_create();
    return ArrayAttachment.fromPointer(ptr.cast(), ownsMemory: true);
  }

  /// Create a new array with the specified initial capacity
  factory ArrayAttachment.withCapacity(int initialCapacity) {
    final ptr = SpineBindings.bindings.spine_array_attachment_create_with_capacity(initialCapacity);
    return ArrayAttachment.fromPointer(ptr.cast(), ownsMemory: true);
  }

  @override
  int get length {
    return SpineBindings.bindings.spine_array_attachment_size(nativePtr.cast());
  }

  @override
  Attachment? operator [](int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final buffer = SpineBindings.bindings.spine_array_attachment_buffer(nativePtr.cast());
    if (buffer[index].address == 0) return null;
    final rtti = SpineBindings.bindings.spine_attachment_get_rtti(buffer[index]);
    final className = SpineBindings.bindings.spine_rtti_get_class_name(rtti).cast<Utf8>().toDartString();
    switch (className) {
      case 'BoundingBoxAttachment':
        final castedPtr = SpineBindings.bindings.spine_attachment_cast_to_bounding_box_attachment(buffer[index]);
        return BoundingBoxAttachment.fromPointer(castedPtr);
      case 'ClippingAttachment':
        final castedPtr = SpineBindings.bindings.spine_attachment_cast_to_clipping_attachment(buffer[index]);
        return ClippingAttachment.fromPointer(castedPtr);
      case 'MeshAttachment':
        final castedPtr = SpineBindings.bindings.spine_attachment_cast_to_mesh_attachment(buffer[index]);
        return MeshAttachment.fromPointer(castedPtr);
      case 'PathAttachment':
        final castedPtr = SpineBindings.bindings.spine_attachment_cast_to_path_attachment(buffer[index]);
        return PathAttachment.fromPointer(castedPtr);
      case 'PointAttachment':
        final castedPtr = SpineBindings.bindings.spine_attachment_cast_to_point_attachment(buffer[index]);
        return PointAttachment.fromPointer(castedPtr);
      case 'RegionAttachment':
        final castedPtr = SpineBindings.bindings.spine_attachment_cast_to_region_attachment(buffer[index]);
        return RegionAttachment.fromPointer(castedPtr);
      default:
        throw UnsupportedError('Unknown concrete type: $className for abstract class Attachment');
    }
  }

  @override
  set length(int newLength) {
    SpineBindings.bindings.spine_array_attachment_set_size(nativePtr.cast(), newLength, Pointer.fromAddress(0));
  }

  /// Adds a value to the end of this array.
  @override
  void add(Attachment? value) {
    SpineBindings.bindings
        .spine_array_attachment_add(nativePtr.cast(), value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// Removes all elements from this array.
  @override
  void clear() {
    SpineBindings.bindings.spine_array_attachment_clear(nativePtr.cast());
  }

  /// Removes the element at the given index.
  @override
  Attachment? removeAt(int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final value = this[index];
    SpineBindings.bindings.spine_array_attachment_remove_at(nativePtr.cast(), index);
    return value;
  }

  /// Ensures this array has at least the given capacity.
  void ensureCapacity(int capacity) {
    SpineBindings.bindings.spine_array_attachment_ensure_capacity(nativePtr.cast(), capacity);
  }

  /// Dispose of the native array
  /// Throws an error if the array was not created by this class (i.e., it was obtained from C)
  void dispose() {
    if (!_ownsMemory) {
      throw StateError(
          'Cannot dispose ArrayAttachment that was created from C. Only arrays created via factory constructors can be disposed.');
    }
    SpineBindings.bindings.spine_array_attachment_dispose(nativePtr.cast());
  }
}

/// ArrayBone wrapper
class ArrayBone extends NativeArray<Bone?> {
  final bool _ownsMemory;

  ArrayBone.fromPointer(Pointer<spine_array_bone_wrapper> super.ptr, {bool ownsMemory = false})
      : _ownsMemory = ownsMemory;

  /// Create a new empty array
  factory ArrayBone() {
    final ptr = SpineBindings.bindings.spine_array_bone_create();
    return ArrayBone.fromPointer(ptr.cast(), ownsMemory: true);
  }

  /// Create a new array with the specified initial capacity
  factory ArrayBone.withCapacity(int initialCapacity) {
    final ptr = SpineBindings.bindings.spine_array_bone_create_with_capacity(initialCapacity);
    return ArrayBone.fromPointer(ptr.cast(), ownsMemory: true);
  }

  @override
  int get length {
    return SpineBindings.bindings.spine_array_bone_size(nativePtr.cast());
  }

  @override
  Bone? operator [](int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final buffer = SpineBindings.bindings.spine_array_bone_buffer(nativePtr.cast());
    return buffer[index].address == 0 ? null : Bone.fromPointer(buffer[index]);
  }

  @override
  set length(int newLength) {
    SpineBindings.bindings.spine_array_bone_set_size(nativePtr.cast(), newLength, Pointer.fromAddress(0));
  }

  /// Adds a value to the end of this array.
  @override
  void add(Bone? value) {
    SpineBindings.bindings.spine_array_bone_add(nativePtr.cast(), value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// Removes all elements from this array.
  @override
  void clear() {
    SpineBindings.bindings.spine_array_bone_clear(nativePtr.cast());
  }

  /// Removes the element at the given index.
  @override
  Bone? removeAt(int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final value = this[index];
    SpineBindings.bindings.spine_array_bone_remove_at(nativePtr.cast(), index);
    return value;
  }

  /// Ensures this array has at least the given capacity.
  void ensureCapacity(int capacity) {
    SpineBindings.bindings.spine_array_bone_ensure_capacity(nativePtr.cast(), capacity);
  }

  /// Dispose of the native array
  /// Throws an error if the array was not created by this class (i.e., it was obtained from C)
  void dispose() {
    if (!_ownsMemory) {
      throw StateError(
          'Cannot dispose ArrayBone that was created from C. Only arrays created via factory constructors can be disposed.');
    }
    SpineBindings.bindings.spine_array_bone_dispose(nativePtr.cast());
  }
}

/// ArrayBoneData wrapper
class ArrayBoneData extends NativeArray<BoneData?> {
  final bool _ownsMemory;

  ArrayBoneData.fromPointer(Pointer<spine_array_bone_data_wrapper> super.ptr, {bool ownsMemory = false})
      : _ownsMemory = ownsMemory;

  /// Create a new empty array
  factory ArrayBoneData() {
    final ptr = SpineBindings.bindings.spine_array_bone_data_create();
    return ArrayBoneData.fromPointer(ptr.cast(), ownsMemory: true);
  }

  /// Create a new array with the specified initial capacity
  factory ArrayBoneData.withCapacity(int initialCapacity) {
    final ptr = SpineBindings.bindings.spine_array_bone_data_create_with_capacity(initialCapacity);
    return ArrayBoneData.fromPointer(ptr.cast(), ownsMemory: true);
  }

  @override
  int get length {
    return SpineBindings.bindings.spine_array_bone_data_size(nativePtr.cast());
  }

  @override
  BoneData? operator [](int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final buffer = SpineBindings.bindings.spine_array_bone_data_buffer(nativePtr.cast());
    return buffer[index].address == 0 ? null : BoneData.fromPointer(buffer[index]);
  }

  @override
  set length(int newLength) {
    SpineBindings.bindings.spine_array_bone_data_set_size(nativePtr.cast(), newLength, Pointer.fromAddress(0));
  }

  /// Adds a value to the end of this array.
  @override
  void add(BoneData? value) {
    SpineBindings.bindings
        .spine_array_bone_data_add(nativePtr.cast(), value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// Removes all elements from this array.
  @override
  void clear() {
    SpineBindings.bindings.spine_array_bone_data_clear(nativePtr.cast());
  }

  /// Removes the element at the given index.
  @override
  BoneData? removeAt(int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final value = this[index];
    SpineBindings.bindings.spine_array_bone_data_remove_at(nativePtr.cast(), index);
    return value;
  }

  /// Ensures this array has at least the given capacity.
  void ensureCapacity(int capacity) {
    SpineBindings.bindings.spine_array_bone_data_ensure_capacity(nativePtr.cast(), capacity);
  }

  /// Dispose of the native array
  /// Throws an error if the array was not created by this class (i.e., it was obtained from C)
  void dispose() {
    if (!_ownsMemory) {
      throw StateError(
          'Cannot dispose ArrayBoneData that was created from C. Only arrays created via factory constructors can be disposed.');
    }
    SpineBindings.bindings.spine_array_bone_data_dispose(nativePtr.cast());
  }
}

/// ArrayBonePose wrapper
class ArrayBonePose extends NativeArray<BonePose?> {
  final bool _ownsMemory;

  ArrayBonePose.fromPointer(Pointer<spine_array_bone_pose_wrapper> super.ptr, {bool ownsMemory = false})
      : _ownsMemory = ownsMemory;

  /// Create a new empty array
  factory ArrayBonePose() {
    final ptr = SpineBindings.bindings.spine_array_bone_pose_create();
    return ArrayBonePose.fromPointer(ptr.cast(), ownsMemory: true);
  }

  /// Create a new array with the specified initial capacity
  factory ArrayBonePose.withCapacity(int initialCapacity) {
    final ptr = SpineBindings.bindings.spine_array_bone_pose_create_with_capacity(initialCapacity);
    return ArrayBonePose.fromPointer(ptr.cast(), ownsMemory: true);
  }

  @override
  int get length {
    return SpineBindings.bindings.spine_array_bone_pose_size(nativePtr.cast());
  }

  @override
  BonePose? operator [](int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final buffer = SpineBindings.bindings.spine_array_bone_pose_buffer(nativePtr.cast());
    return buffer[index].address == 0 ? null : BonePose.fromPointer(buffer[index]);
  }

  @override
  set length(int newLength) {
    SpineBindings.bindings.spine_array_bone_pose_set_size(nativePtr.cast(), newLength, Pointer.fromAddress(0));
  }

  /// Adds a value to the end of this array.
  @override
  void add(BonePose? value) {
    SpineBindings.bindings
        .spine_array_bone_pose_add(nativePtr.cast(), value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// Removes all elements from this array.
  @override
  void clear() {
    SpineBindings.bindings.spine_array_bone_pose_clear(nativePtr.cast());
  }

  /// Removes the element at the given index.
  @override
  BonePose? removeAt(int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final value = this[index];
    SpineBindings.bindings.spine_array_bone_pose_remove_at(nativePtr.cast(), index);
    return value;
  }

  /// Ensures this array has at least the given capacity.
  void ensureCapacity(int capacity) {
    SpineBindings.bindings.spine_array_bone_pose_ensure_capacity(nativePtr.cast(), capacity);
  }

  /// Dispose of the native array
  /// Throws an error if the array was not created by this class (i.e., it was obtained from C)
  void dispose() {
    if (!_ownsMemory) {
      throw StateError(
          'Cannot dispose ArrayBonePose that was created from C. Only arrays created via factory constructors can be disposed.');
    }
    SpineBindings.bindings.spine_array_bone_pose_dispose(nativePtr.cast());
  }
}

/// ArrayBoundingBoxAttachment wrapper
class ArrayBoundingBoxAttachment extends NativeArray<BoundingBoxAttachment?> {
  final bool _ownsMemory;

  ArrayBoundingBoxAttachment.fromPointer(Pointer<spine_array_bounding_box_attachment_wrapper> super.ptr,
      {bool ownsMemory = false})
      : _ownsMemory = ownsMemory;

  /// Create a new empty array
  factory ArrayBoundingBoxAttachment() {
    final ptr = SpineBindings.bindings.spine_array_bounding_box_attachment_create();
    return ArrayBoundingBoxAttachment.fromPointer(ptr.cast(), ownsMemory: true);
  }

  /// Create a new array with the specified initial capacity
  factory ArrayBoundingBoxAttachment.withCapacity(int initialCapacity) {
    final ptr = SpineBindings.bindings.spine_array_bounding_box_attachment_create_with_capacity(initialCapacity);
    return ArrayBoundingBoxAttachment.fromPointer(ptr.cast(), ownsMemory: true);
  }

  @override
  int get length {
    return SpineBindings.bindings.spine_array_bounding_box_attachment_size(nativePtr.cast());
  }

  @override
  BoundingBoxAttachment? operator [](int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final buffer = SpineBindings.bindings.spine_array_bounding_box_attachment_buffer(nativePtr.cast());
    return buffer[index].address == 0 ? null : BoundingBoxAttachment.fromPointer(buffer[index]);
  }

  @override
  set length(int newLength) {
    SpineBindings.bindings
        .spine_array_bounding_box_attachment_set_size(nativePtr.cast(), newLength, Pointer.fromAddress(0));
  }

  /// Adds a value to the end of this array.
  @override
  void add(BoundingBoxAttachment? value) {
    SpineBindings.bindings
        .spine_array_bounding_box_attachment_add(nativePtr.cast(), value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// Removes all elements from this array.
  @override
  void clear() {
    SpineBindings.bindings.spine_array_bounding_box_attachment_clear(nativePtr.cast());
  }

  /// Removes the element at the given index.
  @override
  BoundingBoxAttachment? removeAt(int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final value = this[index];
    SpineBindings.bindings.spine_array_bounding_box_attachment_remove_at(nativePtr.cast(), index);
    return value;
  }

  /// Ensures this array has at least the given capacity.
  void ensureCapacity(int capacity) {
    SpineBindings.bindings.spine_array_bounding_box_attachment_ensure_capacity(nativePtr.cast(), capacity);
  }

  /// Dispose of the native array
  /// Throws an error if the array was not created by this class (i.e., it was obtained from C)
  void dispose() {
    if (!_ownsMemory) {
      throw StateError(
          'Cannot dispose ArrayBoundingBoxAttachment that was created from C. Only arrays created via factory constructors can be disposed.');
    }
    SpineBindings.bindings.spine_array_bounding_box_attachment_dispose(nativePtr.cast());
  }
}

/// ArrayConstraint wrapper
class ArrayConstraint extends NativeArray<Constraint?> {
  final bool _ownsMemory;

  ArrayConstraint.fromPointer(Pointer<spine_array_constraint_wrapper> super.ptr, {bool ownsMemory = false})
      : _ownsMemory = ownsMemory;

  /// Create a new empty array
  factory ArrayConstraint() {
    final ptr = SpineBindings.bindings.spine_array_constraint_create();
    return ArrayConstraint.fromPointer(ptr.cast(), ownsMemory: true);
  }

  /// Create a new array with the specified initial capacity
  factory ArrayConstraint.withCapacity(int initialCapacity) {
    final ptr = SpineBindings.bindings.spine_array_constraint_create_with_capacity(initialCapacity);
    return ArrayConstraint.fromPointer(ptr.cast(), ownsMemory: true);
  }

  @override
  int get length {
    return SpineBindings.bindings.spine_array_constraint_size(nativePtr.cast());
  }

  @override
  Constraint? operator [](int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final buffer = SpineBindings.bindings.spine_array_constraint_buffer(nativePtr.cast());
    if (buffer[index].address == 0) return null;
    final rtti = SpineBindings.bindings.spine_constraint_get_rtti(buffer[index]);
    final className = SpineBindings.bindings.spine_rtti_get_class_name(rtti).cast<Utf8>().toDartString();
    switch (className) {
      case 'IkConstraint':
        final castedPtr = SpineBindings.bindings.spine_constraint_cast_to_ik_constraint(buffer[index]);
        return IkConstraint.fromPointer(castedPtr);
      case 'PathConstraint':
        final castedPtr = SpineBindings.bindings.spine_constraint_cast_to_path_constraint(buffer[index]);
        return PathConstraint.fromPointer(castedPtr);
      case 'PhysicsConstraint':
        final castedPtr = SpineBindings.bindings.spine_constraint_cast_to_physics_constraint(buffer[index]);
        return PhysicsConstraint.fromPointer(castedPtr);
      case 'Slider':
        final castedPtr = SpineBindings.bindings.spine_constraint_cast_to_slider(buffer[index]);
        return Slider.fromPointer(castedPtr);
      case 'TransformConstraint':
        final castedPtr = SpineBindings.bindings.spine_constraint_cast_to_transform_constraint(buffer[index]);
        return TransformConstraint.fromPointer(castedPtr);
      default:
        throw UnsupportedError('Unknown concrete type: $className for abstract class Constraint');
    }
  }

  @override
  set length(int newLength) {
    SpineBindings.bindings.spine_array_constraint_set_size(nativePtr.cast(), newLength, Pointer.fromAddress(0));
  }

  /// Adds a value to the end of this array.
  @override
  void add(Constraint? value) {
    SpineBindings.bindings
        .spine_array_constraint_add(nativePtr.cast(), value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// Removes all elements from this array.
  @override
  void clear() {
    SpineBindings.bindings.spine_array_constraint_clear(nativePtr.cast());
  }

  /// Removes the element at the given index.
  @override
  Constraint? removeAt(int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final value = this[index];
    SpineBindings.bindings.spine_array_constraint_remove_at(nativePtr.cast(), index);
    return value;
  }

  /// Ensures this array has at least the given capacity.
  void ensureCapacity(int capacity) {
    SpineBindings.bindings.spine_array_constraint_ensure_capacity(nativePtr.cast(), capacity);
  }

  /// Dispose of the native array
  /// Throws an error if the array was not created by this class (i.e., it was obtained from C)
  void dispose() {
    if (!_ownsMemory) {
      throw StateError(
          'Cannot dispose ArrayConstraint that was created from C. Only arrays created via factory constructors can be disposed.');
    }
    SpineBindings.bindings.spine_array_constraint_dispose(nativePtr.cast());
  }
}

/// ArrayConstraintData wrapper
class ArrayConstraintData extends NativeArray<ConstraintData?> {
  final bool _ownsMemory;

  ArrayConstraintData.fromPointer(Pointer<spine_array_constraint_data_wrapper> super.ptr, {bool ownsMemory = false})
      : _ownsMemory = ownsMemory;

  /// Create a new empty array
  factory ArrayConstraintData() {
    final ptr = SpineBindings.bindings.spine_array_constraint_data_create();
    return ArrayConstraintData.fromPointer(ptr.cast(), ownsMemory: true);
  }

  /// Create a new array with the specified initial capacity
  factory ArrayConstraintData.withCapacity(int initialCapacity) {
    final ptr = SpineBindings.bindings.spine_array_constraint_data_create_with_capacity(initialCapacity);
    return ArrayConstraintData.fromPointer(ptr.cast(), ownsMemory: true);
  }

  @override
  int get length {
    return SpineBindings.bindings.spine_array_constraint_data_size(nativePtr.cast());
  }

  @override
  ConstraintData? operator [](int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final buffer = SpineBindings.bindings.spine_array_constraint_data_buffer(nativePtr.cast());
    if (buffer[index].address == 0) return null;
    final rtti = SpineBindings.bindings.spine_constraint_data_get_rtti(buffer[index]);
    final className = SpineBindings.bindings.spine_rtti_get_class_name(rtti).cast<Utf8>().toDartString();
    switch (className) {
      case 'IkConstraintData':
        final castedPtr = SpineBindings.bindings.spine_constraint_data_cast_to_ik_constraint_data(buffer[index]);
        return IkConstraintData.fromPointer(castedPtr);
      case 'PathConstraintData':
        final castedPtr = SpineBindings.bindings.spine_constraint_data_cast_to_path_constraint_data(buffer[index]);
        return PathConstraintData.fromPointer(castedPtr);
      case 'PhysicsConstraintData':
        final castedPtr = SpineBindings.bindings.spine_constraint_data_cast_to_physics_constraint_data(buffer[index]);
        return PhysicsConstraintData.fromPointer(castedPtr);
      case 'SliderData':
        final castedPtr = SpineBindings.bindings.spine_constraint_data_cast_to_slider_data(buffer[index]);
        return SliderData.fromPointer(castedPtr);
      case 'TransformConstraintData':
        final castedPtr = SpineBindings.bindings.spine_constraint_data_cast_to_transform_constraint_data(buffer[index]);
        return TransformConstraintData.fromPointer(castedPtr);
      default:
        throw UnsupportedError('Unknown concrete type: $className for abstract class ConstraintData');
    }
  }

  @override
  set length(int newLength) {
    SpineBindings.bindings.spine_array_constraint_data_set_size(nativePtr.cast(), newLength, Pointer.fromAddress(0));
  }

  /// Adds a value to the end of this array.
  @override
  void add(ConstraintData? value) {
    SpineBindings.bindings
        .spine_array_constraint_data_add(nativePtr.cast(), value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// Removes all elements from this array.
  @override
  void clear() {
    SpineBindings.bindings.spine_array_constraint_data_clear(nativePtr.cast());
  }

  /// Removes the element at the given index.
  @override
  ConstraintData? removeAt(int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final value = this[index];
    SpineBindings.bindings.spine_array_constraint_data_remove_at(nativePtr.cast(), index);
    return value;
  }

  /// Ensures this array has at least the given capacity.
  void ensureCapacity(int capacity) {
    SpineBindings.bindings.spine_array_constraint_data_ensure_capacity(nativePtr.cast(), capacity);
  }

  /// Dispose of the native array
  /// Throws an error if the array was not created by this class (i.e., it was obtained from C)
  void dispose() {
    if (!_ownsMemory) {
      throw StateError(
          'Cannot dispose ArrayConstraintData that was created from C. Only arrays created via factory constructors can be disposed.');
    }
    SpineBindings.bindings.spine_array_constraint_data_dispose(nativePtr.cast());
  }
}

/// ArrayEvent wrapper
class ArrayEvent extends NativeArray<Event?> {
  final bool _ownsMemory;

  ArrayEvent.fromPointer(Pointer<spine_array_event_wrapper> super.ptr, {bool ownsMemory = false})
      : _ownsMemory = ownsMemory;

  /// Create a new empty array
  factory ArrayEvent() {
    final ptr = SpineBindings.bindings.spine_array_event_create();
    return ArrayEvent.fromPointer(ptr.cast(), ownsMemory: true);
  }

  /// Create a new array with the specified initial capacity
  factory ArrayEvent.withCapacity(int initialCapacity) {
    final ptr = SpineBindings.bindings.spine_array_event_create_with_capacity(initialCapacity);
    return ArrayEvent.fromPointer(ptr.cast(), ownsMemory: true);
  }

  @override
  int get length {
    return SpineBindings.bindings.spine_array_event_size(nativePtr.cast());
  }

  @override
  Event? operator [](int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final buffer = SpineBindings.bindings.spine_array_event_buffer(nativePtr.cast());
    return buffer[index].address == 0 ? null : Event.fromPointer(buffer[index]);
  }

  @override
  set length(int newLength) {
    SpineBindings.bindings.spine_array_event_set_size(nativePtr.cast(), newLength, Pointer.fromAddress(0));
  }

  /// Adds a value to the end of this array.
  @override
  void add(Event? value) {
    SpineBindings.bindings.spine_array_event_add(nativePtr.cast(), value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// Removes all elements from this array.
  @override
  void clear() {
    SpineBindings.bindings.spine_array_event_clear(nativePtr.cast());
  }

  /// Removes the element at the given index.
  @override
  Event? removeAt(int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final value = this[index];
    SpineBindings.bindings.spine_array_event_remove_at(nativePtr.cast(), index);
    return value;
  }

  /// Ensures this array has at least the given capacity.
  void ensureCapacity(int capacity) {
    SpineBindings.bindings.spine_array_event_ensure_capacity(nativePtr.cast(), capacity);
  }

  /// Dispose of the native array
  /// Throws an error if the array was not created by this class (i.e., it was obtained from C)
  void dispose() {
    if (!_ownsMemory) {
      throw StateError(
          'Cannot dispose ArrayEvent that was created from C. Only arrays created via factory constructors can be disposed.');
    }
    SpineBindings.bindings.spine_array_event_dispose(nativePtr.cast());
  }
}

/// ArrayEventData wrapper
class ArrayEventData extends NativeArray<EventData?> {
  final bool _ownsMemory;

  ArrayEventData.fromPointer(Pointer<spine_array_event_data_wrapper> super.ptr, {bool ownsMemory = false})
      : _ownsMemory = ownsMemory;

  /// Create a new empty array
  factory ArrayEventData() {
    final ptr = SpineBindings.bindings.spine_array_event_data_create();
    return ArrayEventData.fromPointer(ptr.cast(), ownsMemory: true);
  }

  /// Create a new array with the specified initial capacity
  factory ArrayEventData.withCapacity(int initialCapacity) {
    final ptr = SpineBindings.bindings.spine_array_event_data_create_with_capacity(initialCapacity);
    return ArrayEventData.fromPointer(ptr.cast(), ownsMemory: true);
  }

  @override
  int get length {
    return SpineBindings.bindings.spine_array_event_data_size(nativePtr.cast());
  }

  @override
  EventData? operator [](int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final buffer = SpineBindings.bindings.spine_array_event_data_buffer(nativePtr.cast());
    return buffer[index].address == 0 ? null : EventData.fromPointer(buffer[index]);
  }

  @override
  set length(int newLength) {
    SpineBindings.bindings.spine_array_event_data_set_size(nativePtr.cast(), newLength, Pointer.fromAddress(0));
  }

  /// Adds a value to the end of this array.
  @override
  void add(EventData? value) {
    SpineBindings.bindings
        .spine_array_event_data_add(nativePtr.cast(), value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// Removes all elements from this array.
  @override
  void clear() {
    SpineBindings.bindings.spine_array_event_data_clear(nativePtr.cast());
  }

  /// Removes the element at the given index.
  @override
  EventData? removeAt(int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final value = this[index];
    SpineBindings.bindings.spine_array_event_data_remove_at(nativePtr.cast(), index);
    return value;
  }

  /// Ensures this array has at least the given capacity.
  void ensureCapacity(int capacity) {
    SpineBindings.bindings.spine_array_event_data_ensure_capacity(nativePtr.cast(), capacity);
  }

  /// Dispose of the native array
  /// Throws an error if the array was not created by this class (i.e., it was obtained from C)
  void dispose() {
    if (!_ownsMemory) {
      throw StateError(
          'Cannot dispose ArrayEventData that was created from C. Only arrays created via factory constructors can be disposed.');
    }
    SpineBindings.bindings.spine_array_event_data_dispose(nativePtr.cast());
  }
}

/// ArrayFromProperty wrapper
class ArrayFromProperty extends NativeArray<FromProperty?> {
  final bool _ownsMemory;

  ArrayFromProperty.fromPointer(Pointer<spine_array_from_property_wrapper> super.ptr, {bool ownsMemory = false})
      : _ownsMemory = ownsMemory;

  /// Create a new empty array
  factory ArrayFromProperty() {
    final ptr = SpineBindings.bindings.spine_array_from_property_create();
    return ArrayFromProperty.fromPointer(ptr.cast(), ownsMemory: true);
  }

  /// Create a new array with the specified initial capacity
  factory ArrayFromProperty.withCapacity(int initialCapacity) {
    final ptr = SpineBindings.bindings.spine_array_from_property_create_with_capacity(initialCapacity);
    return ArrayFromProperty.fromPointer(ptr.cast(), ownsMemory: true);
  }

  @override
  int get length {
    return SpineBindings.bindings.spine_array_from_property_size(nativePtr.cast());
  }

  @override
  FromProperty? operator [](int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final buffer = SpineBindings.bindings.spine_array_from_property_buffer(nativePtr.cast());
    if (buffer[index].address == 0) return null;
    final rtti = SpineBindings.bindings.spine_from_property_get_rtti(buffer[index]);
    final className = SpineBindings.bindings.spine_rtti_get_class_name(rtti).cast<Utf8>().toDartString();
    switch (className) {
      case 'FromRotate':
        final castedPtr = SpineBindings.bindings.spine_from_property_cast_to_from_rotate(buffer[index]);
        return FromRotate.fromPointer(castedPtr);
      case 'FromScaleX':
        final castedPtr = SpineBindings.bindings.spine_from_property_cast_to_from_scale_x(buffer[index]);
        return FromScaleX.fromPointer(castedPtr);
      case 'FromScaleY':
        final castedPtr = SpineBindings.bindings.spine_from_property_cast_to_from_scale_y(buffer[index]);
        return FromScaleY.fromPointer(castedPtr);
      case 'FromShearY':
        final castedPtr = SpineBindings.bindings.spine_from_property_cast_to_from_shear_y(buffer[index]);
        return FromShearY.fromPointer(castedPtr);
      case 'FromX':
        final castedPtr = SpineBindings.bindings.spine_from_property_cast_to_from_x(buffer[index]);
        return FromX.fromPointer(castedPtr);
      case 'FromY':
        final castedPtr = SpineBindings.bindings.spine_from_property_cast_to_from_y(buffer[index]);
        return FromY.fromPointer(castedPtr);
      default:
        throw UnsupportedError('Unknown concrete type: $className for abstract class FromProperty');
    }
  }

  @override
  set length(int newLength) {
    SpineBindings.bindings.spine_array_from_property_set_size(nativePtr.cast(), newLength, Pointer.fromAddress(0));
  }

  /// Adds a value to the end of this array.
  @override
  void add(FromProperty? value) {
    SpineBindings.bindings
        .spine_array_from_property_add(nativePtr.cast(), value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// Removes all elements from this array.
  @override
  void clear() {
    SpineBindings.bindings.spine_array_from_property_clear(nativePtr.cast());
  }

  /// Removes the element at the given index.
  @override
  FromProperty? removeAt(int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final value = this[index];
    SpineBindings.bindings.spine_array_from_property_remove_at(nativePtr.cast(), index);
    return value;
  }

  /// Ensures this array has at least the given capacity.
  void ensureCapacity(int capacity) {
    SpineBindings.bindings.spine_array_from_property_ensure_capacity(nativePtr.cast(), capacity);
  }

  /// Dispose of the native array
  /// Throws an error if the array was not created by this class (i.e., it was obtained from C)
  void dispose() {
    if (!_ownsMemory) {
      throw StateError(
          'Cannot dispose ArrayFromProperty that was created from C. Only arrays created via factory constructors can be disposed.');
    }
    SpineBindings.bindings.spine_array_from_property_dispose(nativePtr.cast());
  }
}

/// ArrayPhysicsConstraint wrapper
class ArrayPhysicsConstraint extends NativeArray<PhysicsConstraint?> {
  final bool _ownsMemory;

  ArrayPhysicsConstraint.fromPointer(Pointer<spine_array_physics_constraint_wrapper> super.ptr,
      {bool ownsMemory = false})
      : _ownsMemory = ownsMemory;

  /// Create a new empty array
  factory ArrayPhysicsConstraint() {
    final ptr = SpineBindings.bindings.spine_array_physics_constraint_create();
    return ArrayPhysicsConstraint.fromPointer(ptr.cast(), ownsMemory: true);
  }

  /// Create a new array with the specified initial capacity
  factory ArrayPhysicsConstraint.withCapacity(int initialCapacity) {
    final ptr = SpineBindings.bindings.spine_array_physics_constraint_create_with_capacity(initialCapacity);
    return ArrayPhysicsConstraint.fromPointer(ptr.cast(), ownsMemory: true);
  }

  @override
  int get length {
    return SpineBindings.bindings.spine_array_physics_constraint_size(nativePtr.cast());
  }

  @override
  PhysicsConstraint? operator [](int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final buffer = SpineBindings.bindings.spine_array_physics_constraint_buffer(nativePtr.cast());
    return buffer[index].address == 0 ? null : PhysicsConstraint.fromPointer(buffer[index]);
  }

  @override
  set length(int newLength) {
    SpineBindings.bindings.spine_array_physics_constraint_set_size(nativePtr.cast(), newLength, Pointer.fromAddress(0));
  }

  /// Adds a value to the end of this array.
  @override
  void add(PhysicsConstraint? value) {
    SpineBindings.bindings
        .spine_array_physics_constraint_add(nativePtr.cast(), value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// Removes all elements from this array.
  @override
  void clear() {
    SpineBindings.bindings.spine_array_physics_constraint_clear(nativePtr.cast());
  }

  /// Removes the element at the given index.
  @override
  PhysicsConstraint? removeAt(int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final value = this[index];
    SpineBindings.bindings.spine_array_physics_constraint_remove_at(nativePtr.cast(), index);
    return value;
  }

  /// Ensures this array has at least the given capacity.
  void ensureCapacity(int capacity) {
    SpineBindings.bindings.spine_array_physics_constraint_ensure_capacity(nativePtr.cast(), capacity);
  }

  /// Dispose of the native array
  /// Throws an error if the array was not created by this class (i.e., it was obtained from C)
  void dispose() {
    if (!_ownsMemory) {
      throw StateError(
          'Cannot dispose ArrayPhysicsConstraint that was created from C. Only arrays created via factory constructors can be disposed.');
    }
    SpineBindings.bindings.spine_array_physics_constraint_dispose(nativePtr.cast());
  }
}

/// ArrayPolygon wrapper
class ArrayPolygon extends NativeArray<Polygon?> {
  final bool _ownsMemory;

  ArrayPolygon.fromPointer(Pointer<spine_array_polygon_wrapper> super.ptr, {bool ownsMemory = false})
      : _ownsMemory = ownsMemory;

  /// Create a new empty array
  factory ArrayPolygon() {
    final ptr = SpineBindings.bindings.spine_array_polygon_create();
    return ArrayPolygon.fromPointer(ptr.cast(), ownsMemory: true);
  }

  /// Create a new array with the specified initial capacity
  factory ArrayPolygon.withCapacity(int initialCapacity) {
    final ptr = SpineBindings.bindings.spine_array_polygon_create_with_capacity(initialCapacity);
    return ArrayPolygon.fromPointer(ptr.cast(), ownsMemory: true);
  }

  @override
  int get length {
    return SpineBindings.bindings.spine_array_polygon_size(nativePtr.cast());
  }

  @override
  Polygon? operator [](int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final buffer = SpineBindings.bindings.spine_array_polygon_buffer(nativePtr.cast());
    return buffer[index].address == 0 ? null : Polygon.fromPointer(buffer[index]);
  }

  @override
  set length(int newLength) {
    SpineBindings.bindings.spine_array_polygon_set_size(nativePtr.cast(), newLength, Pointer.fromAddress(0));
  }

  /// Adds a value to the end of this array.
  @override
  void add(Polygon? value) {
    SpineBindings.bindings.spine_array_polygon_add(nativePtr.cast(), value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// Removes all elements from this array.
  @override
  void clear() {
    SpineBindings.bindings.spine_array_polygon_clear(nativePtr.cast());
  }

  /// Removes the element at the given index.
  @override
  Polygon? removeAt(int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final value = this[index];
    SpineBindings.bindings.spine_array_polygon_remove_at(nativePtr.cast(), index);
    return value;
  }

  /// Ensures this array has at least the given capacity.
  void ensureCapacity(int capacity) {
    SpineBindings.bindings.spine_array_polygon_ensure_capacity(nativePtr.cast(), capacity);
  }

  /// Dispose of the native array
  /// Throws an error if the array was not created by this class (i.e., it was obtained from C)
  void dispose() {
    if (!_ownsMemory) {
      throw StateError(
          'Cannot dispose ArrayPolygon that was created from C. Only arrays created via factory constructors can be disposed.');
    }
    SpineBindings.bindings.spine_array_polygon_dispose(nativePtr.cast());
  }
}

/// ArraySkin wrapper
class ArraySkin extends NativeArray<Skin?> {
  final bool _ownsMemory;

  ArraySkin.fromPointer(Pointer<spine_array_skin_wrapper> super.ptr, {bool ownsMemory = false})
      : _ownsMemory = ownsMemory;

  /// Create a new empty array
  factory ArraySkin() {
    final ptr = SpineBindings.bindings.spine_array_skin_create();
    return ArraySkin.fromPointer(ptr.cast(), ownsMemory: true);
  }

  /// Create a new array with the specified initial capacity
  factory ArraySkin.withCapacity(int initialCapacity) {
    final ptr = SpineBindings.bindings.spine_array_skin_create_with_capacity(initialCapacity);
    return ArraySkin.fromPointer(ptr.cast(), ownsMemory: true);
  }

  @override
  int get length {
    return SpineBindings.bindings.spine_array_skin_size(nativePtr.cast());
  }

  @override
  Skin? operator [](int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final buffer = SpineBindings.bindings.spine_array_skin_buffer(nativePtr.cast());
    return buffer[index].address == 0 ? null : Skin.fromPointer(buffer[index]);
  }

  @override
  set length(int newLength) {
    SpineBindings.bindings.spine_array_skin_set_size(nativePtr.cast(), newLength, Pointer.fromAddress(0));
  }

  /// Adds a value to the end of this array.
  @override
  void add(Skin? value) {
    SpineBindings.bindings.spine_array_skin_add(nativePtr.cast(), value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// Removes all elements from this array.
  @override
  void clear() {
    SpineBindings.bindings.spine_array_skin_clear(nativePtr.cast());
  }

  /// Removes the element at the given index.
  @override
  Skin? removeAt(int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final value = this[index];
    SpineBindings.bindings.spine_array_skin_remove_at(nativePtr.cast(), index);
    return value;
  }

  /// Ensures this array has at least the given capacity.
  void ensureCapacity(int capacity) {
    SpineBindings.bindings.spine_array_skin_ensure_capacity(nativePtr.cast(), capacity);
  }

  /// Dispose of the native array
  /// Throws an error if the array was not created by this class (i.e., it was obtained from C)
  void dispose() {
    if (!_ownsMemory) {
      throw StateError(
          'Cannot dispose ArraySkin that was created from C. Only arrays created via factory constructors can be disposed.');
    }
    SpineBindings.bindings.spine_array_skin_dispose(nativePtr.cast());
  }
}

/// ArraySlot wrapper
class ArraySlot extends NativeArray<Slot?> {
  final bool _ownsMemory;

  ArraySlot.fromPointer(Pointer<spine_array_slot_wrapper> super.ptr, {bool ownsMemory = false})
      : _ownsMemory = ownsMemory;

  /// Create a new empty array
  factory ArraySlot() {
    final ptr = SpineBindings.bindings.spine_array_slot_create();
    return ArraySlot.fromPointer(ptr.cast(), ownsMemory: true);
  }

  /// Create a new array with the specified initial capacity
  factory ArraySlot.withCapacity(int initialCapacity) {
    final ptr = SpineBindings.bindings.spine_array_slot_create_with_capacity(initialCapacity);
    return ArraySlot.fromPointer(ptr.cast(), ownsMemory: true);
  }

  @override
  int get length {
    return SpineBindings.bindings.spine_array_slot_size(nativePtr.cast());
  }

  @override
  Slot? operator [](int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final buffer = SpineBindings.bindings.spine_array_slot_buffer(nativePtr.cast());
    return buffer[index].address == 0 ? null : Slot.fromPointer(buffer[index]);
  }

  @override
  set length(int newLength) {
    SpineBindings.bindings.spine_array_slot_set_size(nativePtr.cast(), newLength, Pointer.fromAddress(0));
  }

  /// Adds a value to the end of this array.
  @override
  void add(Slot? value) {
    SpineBindings.bindings.spine_array_slot_add(nativePtr.cast(), value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// Removes all elements from this array.
  @override
  void clear() {
    SpineBindings.bindings.spine_array_slot_clear(nativePtr.cast());
  }

  /// Removes the element at the given index.
  @override
  Slot? removeAt(int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final value = this[index];
    SpineBindings.bindings.spine_array_slot_remove_at(nativePtr.cast(), index);
    return value;
  }

  /// Ensures this array has at least the given capacity.
  void ensureCapacity(int capacity) {
    SpineBindings.bindings.spine_array_slot_ensure_capacity(nativePtr.cast(), capacity);
  }

  /// Dispose of the native array
  /// Throws an error if the array was not created by this class (i.e., it was obtained from C)
  void dispose() {
    if (!_ownsMemory) {
      throw StateError(
          'Cannot dispose ArraySlot that was created from C. Only arrays created via factory constructors can be disposed.');
    }
    SpineBindings.bindings.spine_array_slot_dispose(nativePtr.cast());
  }
}

/// ArraySlotData wrapper
class ArraySlotData extends NativeArray<SlotData?> {
  final bool _ownsMemory;

  ArraySlotData.fromPointer(Pointer<spine_array_slot_data_wrapper> super.ptr, {bool ownsMemory = false})
      : _ownsMemory = ownsMemory;

  /// Create a new empty array
  factory ArraySlotData() {
    final ptr = SpineBindings.bindings.spine_array_slot_data_create();
    return ArraySlotData.fromPointer(ptr.cast(), ownsMemory: true);
  }

  /// Create a new array with the specified initial capacity
  factory ArraySlotData.withCapacity(int initialCapacity) {
    final ptr = SpineBindings.bindings.spine_array_slot_data_create_with_capacity(initialCapacity);
    return ArraySlotData.fromPointer(ptr.cast(), ownsMemory: true);
  }

  @override
  int get length {
    return SpineBindings.bindings.spine_array_slot_data_size(nativePtr.cast());
  }

  @override
  SlotData? operator [](int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final buffer = SpineBindings.bindings.spine_array_slot_data_buffer(nativePtr.cast());
    return buffer[index].address == 0 ? null : SlotData.fromPointer(buffer[index]);
  }

  @override
  set length(int newLength) {
    SpineBindings.bindings.spine_array_slot_data_set_size(nativePtr.cast(), newLength, Pointer.fromAddress(0));
  }

  /// Adds a value to the end of this array.
  @override
  void add(SlotData? value) {
    SpineBindings.bindings
        .spine_array_slot_data_add(nativePtr.cast(), value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// Removes all elements from this array.
  @override
  void clear() {
    SpineBindings.bindings.spine_array_slot_data_clear(nativePtr.cast());
  }

  /// Removes the element at the given index.
  @override
  SlotData? removeAt(int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final value = this[index];
    SpineBindings.bindings.spine_array_slot_data_remove_at(nativePtr.cast(), index);
    return value;
  }

  /// Ensures this array has at least the given capacity.
  void ensureCapacity(int capacity) {
    SpineBindings.bindings.spine_array_slot_data_ensure_capacity(nativePtr.cast(), capacity);
  }

  /// Dispose of the native array
  /// Throws an error if the array was not created by this class (i.e., it was obtained from C)
  void dispose() {
    if (!_ownsMemory) {
      throw StateError(
          'Cannot dispose ArraySlotData that was created from C. Only arrays created via factory constructors can be disposed.');
    }
    SpineBindings.bindings.spine_array_slot_data_dispose(nativePtr.cast());
  }
}

/// ArrayTextureRegion wrapper
class ArrayTextureRegion extends NativeArray<TextureRegion?> {
  final bool _ownsMemory;

  ArrayTextureRegion.fromPointer(Pointer<spine_array_texture_region_wrapper> super.ptr, {bool ownsMemory = false})
      : _ownsMemory = ownsMemory;

  /// Create a new empty array
  factory ArrayTextureRegion() {
    final ptr = SpineBindings.bindings.spine_array_texture_region_create();
    return ArrayTextureRegion.fromPointer(ptr.cast(), ownsMemory: true);
  }

  /// Create a new array with the specified initial capacity
  factory ArrayTextureRegion.withCapacity(int initialCapacity) {
    final ptr = SpineBindings.bindings.spine_array_texture_region_create_with_capacity(initialCapacity);
    return ArrayTextureRegion.fromPointer(ptr.cast(), ownsMemory: true);
  }

  @override
  int get length {
    return SpineBindings.bindings.spine_array_texture_region_size(nativePtr.cast());
  }

  @override
  TextureRegion? operator [](int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final buffer = SpineBindings.bindings.spine_array_texture_region_buffer(nativePtr.cast());
    return buffer[index].address == 0 ? null : TextureRegion.fromPointer(buffer[index]);
  }

  @override
  set length(int newLength) {
    SpineBindings.bindings.spine_array_texture_region_set_size(nativePtr.cast(), newLength, Pointer.fromAddress(0));
  }

  /// Adds a value to the end of this array.
  @override
  void add(TextureRegion? value) {
    SpineBindings.bindings
        .spine_array_texture_region_add(nativePtr.cast(), value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// Removes all elements from this array.
  @override
  void clear() {
    SpineBindings.bindings.spine_array_texture_region_clear(nativePtr.cast());
  }

  /// Removes the element at the given index.
  @override
  TextureRegion? removeAt(int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final value = this[index];
    SpineBindings.bindings.spine_array_texture_region_remove_at(nativePtr.cast(), index);
    return value;
  }

  /// Ensures this array has at least the given capacity.
  void ensureCapacity(int capacity) {
    SpineBindings.bindings.spine_array_texture_region_ensure_capacity(nativePtr.cast(), capacity);
  }

  /// Dispose of the native array
  /// Throws an error if the array was not created by this class (i.e., it was obtained from C)
  void dispose() {
    if (!_ownsMemory) {
      throw StateError(
          'Cannot dispose ArrayTextureRegion that was created from C. Only arrays created via factory constructors can be disposed.');
    }
    SpineBindings.bindings.spine_array_texture_region_dispose(nativePtr.cast());
  }
}

/// ArrayTimeline wrapper
class ArrayTimeline extends NativeArray<Timeline?> {
  final bool _ownsMemory;

  ArrayTimeline.fromPointer(Pointer<spine_array_timeline_wrapper> super.ptr, {bool ownsMemory = false})
      : _ownsMemory = ownsMemory;

  /// Create a new empty array
  factory ArrayTimeline() {
    final ptr = SpineBindings.bindings.spine_array_timeline_create();
    return ArrayTimeline.fromPointer(ptr.cast(), ownsMemory: true);
  }

  /// Create a new array with the specified initial capacity
  factory ArrayTimeline.withCapacity(int initialCapacity) {
    final ptr = SpineBindings.bindings.spine_array_timeline_create_with_capacity(initialCapacity);
    return ArrayTimeline.fromPointer(ptr.cast(), ownsMemory: true);
  }

  @override
  int get length {
    return SpineBindings.bindings.spine_array_timeline_size(nativePtr.cast());
  }

  @override
  Timeline? operator [](int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final buffer = SpineBindings.bindings.spine_array_timeline_buffer(nativePtr.cast());
    if (buffer[index].address == 0) return null;
    final rtti = SpineBindings.bindings.spine_timeline_get_rtti(buffer[index]);
    final className = SpineBindings.bindings.spine_rtti_get_class_name(rtti).cast<Utf8>().toDartString();
    switch (className) {
      case 'AlphaTimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_alpha_timeline(buffer[index]);
        return AlphaTimeline.fromPointer(castedPtr);
      case 'AttachmentTimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_attachment_timeline(buffer[index]);
        return AttachmentTimeline.fromPointer(castedPtr);
      case 'DeformTimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_deform_timeline(buffer[index]);
        return DeformTimeline.fromPointer(castedPtr);
      case 'DrawOrderFolderTimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_draw_order_folder_timeline(buffer[index]);
        return DrawOrderFolderTimeline.fromPointer(castedPtr);
      case 'DrawOrderTimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_draw_order_timeline(buffer[index]);
        return DrawOrderTimeline.fromPointer(castedPtr);
      case 'EventTimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_event_timeline(buffer[index]);
        return EventTimeline.fromPointer(castedPtr);
      case 'IkConstraintTimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_ik_constraint_timeline(buffer[index]);
        return IkConstraintTimeline.fromPointer(castedPtr);
      case 'InheritTimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_inherit_timeline(buffer[index]);
        return InheritTimeline.fromPointer(castedPtr);
      case 'PathConstraintMixTimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_path_constraint_mix_timeline(buffer[index]);
        return PathConstraintMixTimeline.fromPointer(castedPtr);
      case 'PathConstraintPositionTimeline':
        final castedPtr =
            SpineBindings.bindings.spine_timeline_cast_to_path_constraint_position_timeline(buffer[index]);
        return PathConstraintPositionTimeline.fromPointer(castedPtr);
      case 'PathConstraintSpacingTimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_path_constraint_spacing_timeline(buffer[index]);
        return PathConstraintSpacingTimeline.fromPointer(castedPtr);
      case 'PhysicsConstraintDampingTimeline':
        final castedPtr =
            SpineBindings.bindings.spine_timeline_cast_to_physics_constraint_damping_timeline(buffer[index]);
        return PhysicsConstraintDampingTimeline.fromPointer(castedPtr);
      case 'PhysicsConstraintGravityTimeline':
        final castedPtr =
            SpineBindings.bindings.spine_timeline_cast_to_physics_constraint_gravity_timeline(buffer[index]);
        return PhysicsConstraintGravityTimeline.fromPointer(castedPtr);
      case 'PhysicsConstraintInertiaTimeline':
        final castedPtr =
            SpineBindings.bindings.spine_timeline_cast_to_physics_constraint_inertia_timeline(buffer[index]);
        return PhysicsConstraintInertiaTimeline.fromPointer(castedPtr);
      case 'PhysicsConstraintMassTimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_physics_constraint_mass_timeline(buffer[index]);
        return PhysicsConstraintMassTimeline.fromPointer(castedPtr);
      case 'PhysicsConstraintMixTimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_physics_constraint_mix_timeline(buffer[index]);
        return PhysicsConstraintMixTimeline.fromPointer(castedPtr);
      case 'PhysicsConstraintResetTimeline':
        final castedPtr =
            SpineBindings.bindings.spine_timeline_cast_to_physics_constraint_reset_timeline(buffer[index]);
        return PhysicsConstraintResetTimeline.fromPointer(castedPtr);
      case 'PhysicsConstraintStrengthTimeline':
        final castedPtr =
            SpineBindings.bindings.spine_timeline_cast_to_physics_constraint_strength_timeline(buffer[index]);
        return PhysicsConstraintStrengthTimeline.fromPointer(castedPtr);
      case 'PhysicsConstraintWindTimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_physics_constraint_wind_timeline(buffer[index]);
        return PhysicsConstraintWindTimeline.fromPointer(castedPtr);
      case 'RGB2Timeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_rgb2_timeline(buffer[index]);
        return Rgb2Timeline.fromPointer(castedPtr);
      case 'RGBA2Timeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_rgba2_timeline(buffer[index]);
        return Rgba2Timeline.fromPointer(castedPtr);
      case 'RGBATimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_rgba_timeline(buffer[index]);
        return RgbaTimeline.fromPointer(castedPtr);
      case 'RGBTimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_rgb_timeline(buffer[index]);
        return RgbTimeline.fromPointer(castedPtr);
      case 'RotateTimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_rotate_timeline(buffer[index]);
        return RotateTimeline.fromPointer(castedPtr);
      case 'ScaleTimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_scale_timeline(buffer[index]);
        return ScaleTimeline.fromPointer(castedPtr);
      case 'ScaleXTimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_scale_x_timeline(buffer[index]);
        return ScaleXTimeline.fromPointer(castedPtr);
      case 'ScaleYTimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_scale_y_timeline(buffer[index]);
        return ScaleYTimeline.fromPointer(castedPtr);
      case 'SequenceTimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_sequence_timeline(buffer[index]);
        return SequenceTimeline.fromPointer(castedPtr);
      case 'ShearTimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_shear_timeline(buffer[index]);
        return ShearTimeline.fromPointer(castedPtr);
      case 'ShearXTimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_shear_x_timeline(buffer[index]);
        return ShearXTimeline.fromPointer(castedPtr);
      case 'ShearYTimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_shear_y_timeline(buffer[index]);
        return ShearYTimeline.fromPointer(castedPtr);
      case 'SliderMixTimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_slider_mix_timeline(buffer[index]);
        return SliderMixTimeline.fromPointer(castedPtr);
      case 'SliderTimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_slider_timeline(buffer[index]);
        return SliderTimeline.fromPointer(castedPtr);
      case 'TransformConstraintTimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_transform_constraint_timeline(buffer[index]);
        return TransformConstraintTimeline.fromPointer(castedPtr);
      case 'TranslateTimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_translate_timeline(buffer[index]);
        return TranslateTimeline.fromPointer(castedPtr);
      case 'TranslateXTimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_translate_x_timeline(buffer[index]);
        return TranslateXTimeline.fromPointer(castedPtr);
      case 'TranslateYTimeline':
        final castedPtr = SpineBindings.bindings.spine_timeline_cast_to_translate_y_timeline(buffer[index]);
        return TranslateYTimeline.fromPointer(castedPtr);
      default:
        throw UnsupportedError('Unknown concrete type: $className for abstract class Timeline');
    }
  }

  @override
  set length(int newLength) {
    SpineBindings.bindings.spine_array_timeline_set_size(nativePtr.cast(), newLength, Pointer.fromAddress(0));
  }

  /// Adds a value to the end of this array.
  @override
  void add(Timeline? value) {
    SpineBindings.bindings
        .spine_array_timeline_add(nativePtr.cast(), value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// Removes all elements from this array.
  @override
  void clear() {
    SpineBindings.bindings.spine_array_timeline_clear(nativePtr.cast());
  }

  /// Removes the element at the given index.
  @override
  Timeline? removeAt(int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final value = this[index];
    SpineBindings.bindings.spine_array_timeline_remove_at(nativePtr.cast(), index);
    return value;
  }

  /// Ensures this array has at least the given capacity.
  void ensureCapacity(int capacity) {
    SpineBindings.bindings.spine_array_timeline_ensure_capacity(nativePtr.cast(), capacity);
  }

  /// Dispose of the native array
  /// Throws an error if the array was not created by this class (i.e., it was obtained from C)
  void dispose() {
    if (!_ownsMemory) {
      throw StateError(
          'Cannot dispose ArrayTimeline that was created from C. Only arrays created via factory constructors can be disposed.');
    }
    SpineBindings.bindings.spine_array_timeline_dispose(nativePtr.cast());
  }
}

/// ArrayToProperty wrapper
class ArrayToProperty extends NativeArray<ToProperty?> {
  final bool _ownsMemory;

  ArrayToProperty.fromPointer(Pointer<spine_array_to_property_wrapper> super.ptr, {bool ownsMemory = false})
      : _ownsMemory = ownsMemory;

  /// Create a new empty array
  factory ArrayToProperty() {
    final ptr = SpineBindings.bindings.spine_array_to_property_create();
    return ArrayToProperty.fromPointer(ptr.cast(), ownsMemory: true);
  }

  /// Create a new array with the specified initial capacity
  factory ArrayToProperty.withCapacity(int initialCapacity) {
    final ptr = SpineBindings.bindings.spine_array_to_property_create_with_capacity(initialCapacity);
    return ArrayToProperty.fromPointer(ptr.cast(), ownsMemory: true);
  }

  @override
  int get length {
    return SpineBindings.bindings.spine_array_to_property_size(nativePtr.cast());
  }

  @override
  ToProperty? operator [](int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final buffer = SpineBindings.bindings.spine_array_to_property_buffer(nativePtr.cast());
    if (buffer[index].address == 0) return null;
    final rtti = SpineBindings.bindings.spine_to_property_get_rtti(buffer[index]);
    final className = SpineBindings.bindings.spine_rtti_get_class_name(rtti).cast<Utf8>().toDartString();
    switch (className) {
      case 'ToRotate':
        final castedPtr = SpineBindings.bindings.spine_to_property_cast_to_to_rotate(buffer[index]);
        return ToRotate.fromPointer(castedPtr);
      case 'ToScaleX':
        final castedPtr = SpineBindings.bindings.spine_to_property_cast_to_to_scale_x(buffer[index]);
        return ToScaleX.fromPointer(castedPtr);
      case 'ToScaleY':
        final castedPtr = SpineBindings.bindings.spine_to_property_cast_to_to_scale_y(buffer[index]);
        return ToScaleY.fromPointer(castedPtr);
      case 'ToShearY':
        final castedPtr = SpineBindings.bindings.spine_to_property_cast_to_to_shear_y(buffer[index]);
        return ToShearY.fromPointer(castedPtr);
      case 'ToX':
        final castedPtr = SpineBindings.bindings.spine_to_property_cast_to_to_x(buffer[index]);
        return ToX.fromPointer(castedPtr);
      case 'ToY':
        final castedPtr = SpineBindings.bindings.spine_to_property_cast_to_to_y(buffer[index]);
        return ToY.fromPointer(castedPtr);
      default:
        throw UnsupportedError('Unknown concrete type: $className for abstract class ToProperty');
    }
  }

  @override
  set length(int newLength) {
    SpineBindings.bindings.spine_array_to_property_set_size(nativePtr.cast(), newLength, Pointer.fromAddress(0));
  }

  /// Adds a value to the end of this array.
  @override
  void add(ToProperty? value) {
    SpineBindings.bindings
        .spine_array_to_property_add(nativePtr.cast(), value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// Removes all elements from this array.
  @override
  void clear() {
    SpineBindings.bindings.spine_array_to_property_clear(nativePtr.cast());
  }

  /// Removes the element at the given index.
  @override
  ToProperty? removeAt(int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final value = this[index];
    SpineBindings.bindings.spine_array_to_property_remove_at(nativePtr.cast(), index);
    return value;
  }

  /// Ensures this array has at least the given capacity.
  void ensureCapacity(int capacity) {
    SpineBindings.bindings.spine_array_to_property_ensure_capacity(nativePtr.cast(), capacity);
  }

  /// Dispose of the native array
  /// Throws an error if the array was not created by this class (i.e., it was obtained from C)
  void dispose() {
    if (!_ownsMemory) {
      throw StateError(
          'Cannot dispose ArrayToProperty that was created from C. Only arrays created via factory constructors can be disposed.');
    }
    SpineBindings.bindings.spine_array_to_property_dispose(nativePtr.cast());
  }
}

/// ArrayTrackEntry wrapper
class ArrayTrackEntry extends NativeArray<TrackEntry?> {
  final bool _ownsMemory;

  ArrayTrackEntry.fromPointer(Pointer<spine_array_track_entry_wrapper> super.ptr, {bool ownsMemory = false})
      : _ownsMemory = ownsMemory;

  /// Create a new empty array
  factory ArrayTrackEntry() {
    final ptr = SpineBindings.bindings.spine_array_track_entry_create();
    return ArrayTrackEntry.fromPointer(ptr.cast(), ownsMemory: true);
  }

  /// Create a new array with the specified initial capacity
  factory ArrayTrackEntry.withCapacity(int initialCapacity) {
    final ptr = SpineBindings.bindings.spine_array_track_entry_create_with_capacity(initialCapacity);
    return ArrayTrackEntry.fromPointer(ptr.cast(), ownsMemory: true);
  }

  @override
  int get length {
    return SpineBindings.bindings.spine_array_track_entry_size(nativePtr.cast());
  }

  @override
  TrackEntry? operator [](int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final buffer = SpineBindings.bindings.spine_array_track_entry_buffer(nativePtr.cast());
    return buffer[index].address == 0 ? null : TrackEntry.fromPointer(buffer[index]);
  }

  @override
  set length(int newLength) {
    SpineBindings.bindings.spine_array_track_entry_set_size(nativePtr.cast(), newLength, Pointer.fromAddress(0));
  }

  /// Adds a value to the end of this array.
  @override
  void add(TrackEntry? value) {
    SpineBindings.bindings
        .spine_array_track_entry_add(nativePtr.cast(), value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// Removes all elements from this array.
  @override
  void clear() {
    SpineBindings.bindings.spine_array_track_entry_clear(nativePtr.cast());
  }

  /// Removes the element at the given index.
  @override
  TrackEntry? removeAt(int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final value = this[index];
    SpineBindings.bindings.spine_array_track_entry_remove_at(nativePtr.cast(), index);
    return value;
  }

  /// Ensures this array has at least the given capacity.
  void ensureCapacity(int capacity) {
    SpineBindings.bindings.spine_array_track_entry_ensure_capacity(nativePtr.cast(), capacity);
  }

  /// Dispose of the native array
  /// Throws an error if the array was not created by this class (i.e., it was obtained from C)
  void dispose() {
    if (!_ownsMemory) {
      throw StateError(
          'Cannot dispose ArrayTrackEntry that was created from C. Only arrays created via factory constructors can be disposed.');
    }
    SpineBindings.bindings.spine_array_track_entry_dispose(nativePtr.cast());
  }
}

/// ArrayUpdate wrapper
class ArrayUpdate extends NativeArray<Update?> {
  final bool _ownsMemory;

  ArrayUpdate.fromPointer(Pointer<spine_array_update_wrapper> super.ptr, {bool ownsMemory = false})
      : _ownsMemory = ownsMemory;

  /// Create a new empty array
  factory ArrayUpdate() {
    final ptr = SpineBindings.bindings.spine_array_update_create();
    return ArrayUpdate.fromPointer(ptr.cast(), ownsMemory: true);
  }

  /// Create a new array with the specified initial capacity
  factory ArrayUpdate.withCapacity(int initialCapacity) {
    final ptr = SpineBindings.bindings.spine_array_update_create_with_capacity(initialCapacity);
    return ArrayUpdate.fromPointer(ptr.cast(), ownsMemory: true);
  }

  @override
  int get length {
    return SpineBindings.bindings.spine_array_update_size(nativePtr.cast());
  }

  @override
  Update? operator [](int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final buffer = SpineBindings.bindings.spine_array_update_buffer(nativePtr.cast());
    if (buffer[index].address == 0) return null;
    final rtti = SpineBindings.bindings.spine_update_get_rtti(buffer[index]);
    final className = SpineBindings.bindings.spine_rtti_get_class_name(rtti).cast<Utf8>().toDartString();
    switch (className) {
      case 'Bone':
        final castedPtr = SpineBindings.bindings.spine_update_cast_to_bone(buffer[index]);
        return Bone.fromPointer(castedPtr);
      case 'BonePose':
        final castedPtr = SpineBindings.bindings.spine_update_cast_to_bone_pose(buffer[index]);
        return BonePose.fromPointer(castedPtr);
      case 'IkConstraint':
        final castedPtr = SpineBindings.bindings.spine_update_cast_to_ik_constraint(buffer[index]);
        return IkConstraint.fromPointer(castedPtr);
      case 'PathConstraint':
        final castedPtr = SpineBindings.bindings.spine_update_cast_to_path_constraint(buffer[index]);
        return PathConstraint.fromPointer(castedPtr);
      case 'PhysicsConstraint':
        final castedPtr = SpineBindings.bindings.spine_update_cast_to_physics_constraint(buffer[index]);
        return PhysicsConstraint.fromPointer(castedPtr);
      case 'Slider':
        final castedPtr = SpineBindings.bindings.spine_update_cast_to_slider(buffer[index]);
        return Slider.fromPointer(castedPtr);
      case 'TransformConstraint':
        final castedPtr = SpineBindings.bindings.spine_update_cast_to_transform_constraint(buffer[index]);
        return TransformConstraint.fromPointer(castedPtr);
      default:
        throw UnsupportedError('Unknown concrete type: $className for abstract class Update');
    }
  }

  @override
  set length(int newLength) {
    SpineBindings.bindings.spine_array_update_set_size(nativePtr.cast(), newLength, Pointer.fromAddress(0));
  }

  /// Adds a value to the end of this array.
  @override
  void add(Update? value) {
    SpineBindings.bindings.spine_array_update_add(nativePtr.cast(), value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// Removes all elements from this array.
  @override
  void clear() {
    SpineBindings.bindings.spine_array_update_clear(nativePtr.cast());
  }

  /// Removes the element at the given index.
  @override
  Update? removeAt(int index) {
    if (index < 0 || index >= length) {
      throw RangeError.index(index, this, 'index');
    }
    final value = this[index];
    SpineBindings.bindings.spine_array_update_remove_at(nativePtr.cast(), index);
    return value;
  }

  /// Ensures this array has at least the given capacity.
  void ensureCapacity(int capacity) {
    SpineBindings.bindings.spine_array_update_ensure_capacity(nativePtr.cast(), capacity);
  }

  /// Dispose of the native array
  /// Throws an error if the array was not created by this class (i.e., it was obtained from C)
  void dispose() {
    if (!_ownsMemory) {
      throw StateError(
          'Cannot dispose ArrayUpdate that was created from C. Only arrays created via factory constructors can be disposed.');
    }
    SpineBindings.bindings.spine_array_update_dispose(nativePtr.cast());
  }
}
