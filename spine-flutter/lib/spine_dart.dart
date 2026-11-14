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

import 'dart:typed_data';

import 'package:spine_flutter/generated/arrays.dart';
import 'package:universal_ffi/ffi.dart';
import 'package:universal_ffi/ffi_utils.dart';

import 'generated/animation_state.dart';
import 'generated/animation_state_data.dart';
import 'generated/atlas.dart';
import 'generated/attachment.dart';
import 'generated/bone_pose.dart';
import 'generated/bounding_box_attachment.dart';
import 'generated/clipping_attachment.dart';
import 'generated/event.dart';
import 'generated/event_type.dart';
import 'generated/mesh_attachment.dart';
import 'generated/path_attachment.dart';
import 'generated/physics.dart';
import 'generated/point_attachment.dart';
import 'generated/region_attachment.dart';
import 'generated/render_command.dart';
import 'generated/skeleton.dart';
import 'generated/skeleton_data.dart';
import 'generated/skin.dart';
import 'generated/spine_dart_bindings_generated.dart';
import 'generated/texture_region.dart';
import 'generated/track_entry.dart';
import 'spine_bindings.dart';
import 'spine_dart_init.dart' if (dart.library.html) 'spine_dart_init_web.dart';

// Export generated classes
export 'generated/api.dart';
export 'generated/spine_dart_bindings_generated.dart';
export 'spine_bindings.dart';

Future<void> initSpineDart({bool useStaticLinkage = false, bool enableMemoryDebugging = false}) async {
  final ffi = await initSpineDartFFI(useStaticLinkage);
  final bindings = SpineDartBindings(ffi.dylib);
  if (enableMemoryDebugging) bindings.spine_enable_debug_extension(true);

  // Initialize the global bindings for generated code
  SpineBindings.init(bindings);

  return;
}

int majorVersion() => SpineBindings.bindings.spine_major_version();

int minorVersion() => SpineBindings.bindings.spine_minor_version();

void reportLeaks() => SpineBindings.bindings.spine_report_leaks();

/// Load an Atlas from atlas data string
Atlas loadAtlas(String atlasData) {
  final atlasDataNative = atlasData.toNativeUtf8();
  final resultPtr = SpineBindings.bindings.spine_atlas_load(atlasDataNative.cast<Char>());
  malloc.free(atlasDataNative);

  // Check for error
  final errorPtr = SpineBindings.bindings.spine_atlas_result_get_error(resultPtr.cast());
  if (errorPtr != nullptr) {
    final error = errorPtr.cast<Utf8>().toDartString();
    SpineBindings.bindings.spine_atlas_result_dispose(resultPtr.cast());
    throw Exception("Couldn't load atlas: $error");
  }

  // Get atlas
  final atlasPtr = SpineBindings.bindings.spine_atlas_result_get_atlas(resultPtr.cast());
  final atlas = Atlas.fromPointer(atlasPtr);
  SpineBindings.bindings.spine_atlas_result_dispose(resultPtr.cast());
  return atlas;
}

/// Load skeleton data from JSON string
SkeletonData loadSkeletonDataJson(Atlas atlas, String jsonData, {String? path}) {
  final jsonDataNative = jsonData.toNativeUtf8();
  final pathNative = (path ?? '').toNativeUtf8();

  final resultPtr = SpineBindings.bindings
      .spine_skeleton_data_load_json(atlas.nativePtr.cast(), jsonDataNative.cast<Char>(), pathNative.cast<Char>());

  malloc.free(jsonDataNative);
  malloc.free(pathNative);

  // Check for error
  final errorPtr = SpineBindings.bindings.spine_skeleton_data_result_get_error(resultPtr.cast());
  if (errorPtr != nullptr) {
    final error = errorPtr.cast<Utf8>().toDartString();
    SpineBindings.bindings.spine_skeleton_data_result_dispose(resultPtr.cast());
    throw Exception("Couldn't load skeleton data: $error");
  }

  // Get skeleton data
  final skeletonDataPtr = SpineBindings.bindings.spine_skeleton_data_result_get_data(resultPtr.cast());
  final skeletonData = SkeletonData.fromPointer(skeletonDataPtr);
  SpineBindings.bindings.spine_skeleton_data_result_dispose(resultPtr.cast());
  return skeletonData;
}

/// Load skeleton data from binary data
SkeletonData loadSkeletonDataBinary(Atlas atlas, Uint8List binaryData, {String? path}) {
  final Pointer<Uint8> binaryNative = malloc.allocate(binaryData.lengthInBytes);
  binaryNative.asTypedList(binaryData.lengthInBytes).setAll(0, binaryData);
  final pathNative = (path ?? '').toNativeUtf8();

  final resultPtr = SpineBindings.bindings.spine_skeleton_data_load_binary(
      atlas.nativePtr.cast(), binaryNative.cast(), binaryData.lengthInBytes, pathNative.cast<Char>());

  malloc.free(binaryNative);
  malloc.free(pathNative);

  // Check for error
  final errorPtr = SpineBindings.bindings.spine_skeleton_data_result_get_error(resultPtr.cast());
  if (errorPtr != nullptr) {
    final error = errorPtr.cast<Utf8>().toDartString();
    SpineBindings.bindings.spine_skeleton_data_result_dispose(resultPtr.cast());
    throw Exception("Couldn't load skeleton data: $error");
  }

  // Get skeleton data
  final skeletonDataPtr = SpineBindings.bindings.spine_skeleton_data_result_get_data(resultPtr.cast());
  final skeletonData = SkeletonData.fromPointer(skeletonDataPtr);
  SpineBindings.bindings.spine_skeleton_data_result_dispose(resultPtr.cast());
  return skeletonData;
}

/// Extension methods for changing the texture region of an attachment.
extension AttachmentExtensions on Attachment {
  /// Replaces the texture region at [sequenceIndex] and recomputes the attachment's sequence data.
  ///
  /// This operation is supported for [RegionAttachment] and [MeshAttachment]. Attachments from skeleton data may be
  /// shared by multiple skeletons, so call [Attachment.copy] before changing a region when the change should only
  /// affect one skeleton. This method does not copy or resize the attachment and does not take ownership of [region].
  /// The region must remain valid for as long as the attachment may be rendered.
  void setRegion(TextureRegion region, {int sequenceIndex = 0}) {
    final attachment = this;
    final ArrayTextureRegion regions;
    if (attachment is RegionAttachment) {
      regions = attachment.sequence.regions;
    } else if (attachment is MeshAttachment) {
      regions = attachment.sequence.regions;
    } else {
      throw UnsupportedError('Only region and mesh attachments have texture regions.');
    }

    if (sequenceIndex < 0 || sequenceIndex >= regions.length) {
      throw RangeError.index(sequenceIndex, regions, 'sequenceIndex');
    }

    final buffer = SpineBindings.bindings.spine_array_texture_region_buffer(regions.nativePtr.cast());
    buffer[sequenceIndex] = region.nativePtr.cast();

    if (attachment is RegionAttachment) {
      attachment.updateSequence();
    } else {
      (attachment as MeshAttachment).updateSequence();
    }
  }
}

/// Represents an entry in a skin
class SkinEntry {
  final int slotIndex;
  final String name;
  final Attachment? attachment;

  SkinEntry._({required this.slotIndex, required this.name, required this.attachment});
}

/// Extension method for Skin to get all entries
extension SkinExtensions on Skin {
  /// Get all entries (slot/attachment pairs) in this skin
  List<SkinEntry> getEntries() {
    final entriesPtr = SpineBindings.bindings.spine_skin_get_entries(nativePtr.cast());
    if (entriesPtr == nullptr) return [];

    try {
      final numEntries = SpineBindings.bindings.spine_skin_entries_get_num_entries(entriesPtr.cast());
      final entries = <SkinEntry>[];

      for (int i = 0; i < numEntries; i++) {
        final entryPtr = SpineBindings.bindings.spine_skin_entries_get_entry(entriesPtr.cast(), i);
        if (entryPtr != nullptr) {
          final slotIndex = SpineBindings.bindings.spine_skin_entry_get_slot_index(entryPtr.cast());
          final namePtr = SpineBindings.bindings.spine_skin_entry_get_name(entryPtr.cast());
          final name = namePtr.cast<Utf8>().toDartString();

          final attachmentPtr = SpineBindings.bindings.spine_skin_entry_get_attachment(entryPtr.cast());
          Attachment? attachment;
          if (attachmentPtr.address != 0) {
            // Use RTTI to determine the concrete attachment type
            final rtti = SpineBindings.bindings.spine_attachment_get_rtti(attachmentPtr);
            final className = SpineBindings.bindings.spine_rtti_get_class_name(rtti).cast<Utf8>().toDartString();

            switch (className) {
              case 'spine_region_attachment':
                attachment = RegionAttachment.fromPointer(attachmentPtr.cast());
                break;
              case 'spine_mesh_attachment':
                attachment = MeshAttachment.fromPointer(attachmentPtr.cast());
                break;
              case 'spine_bounding_box_attachment':
                attachment = BoundingBoxAttachment.fromPointer(attachmentPtr.cast());
                break;
              case 'spine_clipping_attachment':
                attachment = ClippingAttachment.fromPointer(attachmentPtr.cast());
                break;
              case 'spine_path_attachment':
                attachment = PathAttachment.fromPointer(attachmentPtr.cast());
                break;
              case 'spine_point_attachment':
                attachment = PointAttachment.fromPointer(attachmentPtr.cast());
                break;
              default:
                // Unknown attachment type, treat as generic Attachment
                attachment = null;
            }
          }

          entries.add(SkinEntry._(
            slotIndex: slotIndex,
            name: name,
            attachment: attachment,
          ));
        }
      }

      return entries;
    } finally {
      SpineBindings.bindings.spine_skin_entries_dispose(entriesPtr.cast());
    }
  }
}

/// Event listener callback for animation state events
typedef AnimationStateListener = void Function(EventType type, TrackEntry entry, Event? event);

/// Manager for animation state event listeners
class AnimationStateEventManager {
  // Use pointer addresses as keys since Dart wrapper objects might be recreated
  final Map<int, AnimationStateListener?> _stateListeners = {};
  final Map<int, Map<int, AnimationStateListener>> _trackEntryListeners = {};

  static final instance = AnimationStateEventManager._();
  AnimationStateEventManager._();

  void setStateListener(AnimationState state, AnimationStateListener? listener) {
    final key = state.nativePtr.address;
    if (listener == null) {
      _stateListeners.remove(key);
    } else {
      _stateListeners[key] = listener;
    }
  }

  AnimationStateListener? getStateListener(AnimationState state) {
    final key = state.nativePtr.address;
    return _stateListeners[key];
  }

  void setTrackEntryListener(TrackEntry entry, AnimationStateListener? listener) {
    // Get the animation state from the track entry itself!
    final state = entry.animationState;
    if (state == null) {
      throw StateError('TrackEntry does not have an associated AnimationState');
    }

    final stateKey = state.nativePtr.address;
    final entryKey = entry.nativePtr.address;
    final listeners = _trackEntryListeners.putIfAbsent(stateKey, () => {});
    if (listener == null) {
      listeners.remove(entryKey);
    } else {
      listeners[entryKey] = listener;
      // print('DEBUG: Registered listener for TrackEntry at address: $entryKey for AnimationState at address: $stateKey');
    }
  }

  AnimationStateListener? getTrackEntryListener(AnimationState state, TrackEntry entry) {
    final stateKey = state.nativePtr.address;
    final entryKey = entry.nativePtr.address;
    final listener = _trackEntryListeners[stateKey]?[entryKey];
    if (listener == null) {
      // print('DEBUG: No listener found for TrackEntry at address: $entryKey in AnimationState at address: $stateKey');
      // print('DEBUG: Available state keys: ${_trackEntryListeners.keys.toList()}');
      // print('DEBUG: Available entry keys for state $stateKey: ${_trackEntryListeners[stateKey]?.keys.toList()}');
    }
    return listener;
  }

  void removeTrackEntry(AnimationState state, TrackEntry entry) {
    final stateKey = state.nativePtr.address;
    final entryKey = entry.nativePtr.address;
    _trackEntryListeners[stateKey]?.remove(entryKey);
  }

  void clearState(AnimationState state) {
    final key = state.nativePtr.address;
    _stateListeners.remove(key);
    _trackEntryListeners.remove(key);
  }

  /// Debug method to inspect current state of the manager
  void debugPrint() {
    print('\nAnimationStateEventManager contents:');
    print('  State listeners: ${_stateListeners.keys.toList()} (${_stateListeners.length} total)');
    print('  Track entry listeners by state:');
    for (final entry in _trackEntryListeners.entries) {
      print('    State ${entry.key}: ${entry.value.keys.toList()} (${entry.value.length} entries)');
    }
  }
}

/// Extension to manage event listeners on AnimationState
extension AnimationStateListeners on AnimationState {
  /// Set a listener for all animation state events
  void setListener(AnimationStateListener? listener) {
    AnimationStateEventManager.instance.setStateListener(this, listener);
  }

  /// Get the current state listener
  AnimationStateListener? get listener => AnimationStateEventManager.instance.getStateListener(this);
}

/// Extension to add setListener to TrackEntry
extension TrackEntryExtensions on TrackEntry {
  /// Set a listener for events from this track entry
  void setListener(AnimationStateListener? listener) {
    AnimationStateEventManager.instance.setTrackEntryListener(this, listener);
  }
}

/// Represents a bounding box with position and dimensions
class Bounds {
  double x;
  double y;
  double width;
  double height;

  Bounds({
    required this.x,
    required this.y,
    required this.width,
    required this.height,
  });
}

class Vector {
  double x;
  double y;

  Vector({required this.x, required this.y});
}

/// Extension to add bounds property to Skeleton
extension SkeletonExtensions on Skeleton {
  /// Get the axis-aligned bounding box (AABB) containing all world vertices of the skeleton
  Bounds get bounds {
    final output = ArrayFloat();
    SpineBindings.bindings.spine_skeleton_get_bounds(nativePtr.cast(), output.nativePtr.cast());
    final bounds = Bounds(
      x: output[0],
      y: output[1],
      width: output[2],
      height: output[3],
    );
    output.dispose();
    return bounds;
  }

  Vector getPosition() {
    final output = ArrayFloat.withCapacity(2);
    SpineBindings.bindings.spine_skeleton_get_position_v(nativePtr.cast(), output.nativePtr.cast());
    final position = Vector(x: output[0], y: output[1]);
    output.dispose();
    return position;
  }
}

extension BonePoseExtensions on BonePose {
  Vector worldToLocal(double worldX, double worldY) {
    final output = ArrayFloat.withCapacity(2);
    SpineBindings.bindings.spine_bone_pose_world_to_local_v(nativePtr.cast(), worldX, worldY, output.nativePtr.cast());
    final vector = Vector(x: output[0], y: output[1]);
    output.dispose();
    return vector;
  }

  Vector localToWorld(double localX, double localY) {
    final output = ArrayFloat.withCapacity(2);
    SpineBindings.bindings.spine_bone_pose_local_to_world_v(nativePtr.cast(), localX, localY, output.nativePtr.cast());
    final vector = Vector(x: output[0], y: output[1]);
    output.dispose();
    return vector;
  }

  Vector worldToParent(double worldX, double worldY) {
    final output = ArrayFloat.withCapacity(2);
    SpineBindings.bindings.spine_bone_pose_world_to_parent_v(nativePtr.cast(), worldX, worldY, output.nativePtr.cast());
    final vector = Vector(x: output[0], y: output[1]);
    output.dispose();
    return vector;
  }

  Vector parentToWorld(double parentX, double parentY) {
    final output = ArrayFloat.withCapacity(2);
    SpineBindings.bindings
        .spine_bone_pose_parent_to_world_v(nativePtr.cast(), parentX, parentY, output.nativePtr.cast());
    final vector = Vector(x: output[0], y: output[1]);
    output.dispose();
    return vector;
  }
}

/// Convenient drawable that combines skeleton, animation state, and rendering
class SkeletonDrawable {
  final Pointer<spine_skeleton_drawable_wrapper> _drawable;

  late final Skeleton skeleton;
  late final AnimationState animationState;
  late final AnimationStateData animationStateData;

  SkeletonDrawable(SkeletonData skeletonData)
      : _drawable = SpineBindings.bindings.spine_skeleton_drawable_create(skeletonData.nativePtr.cast()) {
    if (_drawable == nullptr) {
      throw Exception("Failed to create skeleton drawable");
    }

    // Get references to the skeleton and animation state
    final skeletonPtr = SpineBindings.bindings.spine_skeleton_drawable_get_skeleton(_drawable.cast());
    skeleton = Skeleton.fromPointer(skeletonPtr);

    final animationStatePtr = SpineBindings.bindings.spine_skeleton_drawable_get_animation_state(_drawable.cast());
    animationState = AnimationState.fromPointer(animationStatePtr);

    final animationStateDataPtr =
        SpineBindings.bindings.spine_skeleton_drawable_get_animation_state_data(_drawable.cast());
    animationStateData = AnimationStateData.fromPointer(animationStateDataPtr);
  }

  /// Update the animation state and process events
  void update(double delta) {
    // Update animation state
    animationState.update(delta);

    // Process events
    final eventsPtr = SpineBindings.bindings.spine_skeleton_drawable_get_animation_state_events(_drawable.cast());
    if (eventsPtr != nullptr) {
      final numEvents = SpineBindings.bindings.spine_animation_state_events_get_num_events(eventsPtr.cast());

      for (int i = 0; i < numEvents; i++) {
        // Get event type
        final eventTypeValue = SpineBindings.bindings.spine_animation_state_events_get_event_type(eventsPtr.cast(), i);
        final type = EventType.fromValue(eventTypeValue);

        // Get track entry
        final trackEntryPtr = SpineBindings.bindings.spine_animation_state_events_get_track_entry(eventsPtr.cast(), i);
        final trackEntry = TrackEntry.fromPointer(trackEntryPtr);

        // Get event (may be null)
        final eventPtr = SpineBindings.bindings.spine_animation_state_events_get_event(eventsPtr.cast(), i);
        final event = eventPtr.address == 0 ? null : Event.fromPointer(eventPtr);

        // Call track entry listener if registered
        final trackListener = AnimationStateEventManager.instance.getTrackEntryListener(animationState, trackEntry);
        trackListener?.call(type, trackEntry, event);

        // Call global state listener
        animationState.listener?.call(type, trackEntry, event);

        // Remove listener if track entry is being disposed
        if (type == EventType.dispose) {
          AnimationStateEventManager.instance.removeTrackEntry(animationState, trackEntry);
        }
      }

      // Reset events for next frame
      SpineBindings.bindings.spine_animation_state_events_reset(eventsPtr.cast());
    }

    // Apply animation state to skeleton
    animationState.apply(skeleton);

    // Update skeleton physics and world transforms
    skeleton.update(delta);
    skeleton.updateWorldTransform(Physics.update);
  }

  /// Render the skeleton and get render commands
  RenderCommand? render() {
    final renderCommand = SpineBindings.bindings.spine_skeleton_drawable_render(_drawable.cast());
    return renderCommand.address == 0 ? null : RenderCommand.fromPointer(renderCommand);
  }

  void dispose() {
    AnimationStateEventManager.instance.clearState(animationState);
    SpineBindings.bindings.spine_skeleton_drawable_dispose(_drawable.cast());
  }
}
