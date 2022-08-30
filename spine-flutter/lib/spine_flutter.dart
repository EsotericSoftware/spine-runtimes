import 'dart:convert' as convert;
import 'dart:ffi';
import 'dart:io';
import 'dart:typed_data';
import 'dart:ui';
import 'package:ffi/ffi.dart';
import 'package:flutter/rendering.dart';
import 'package:http/http.dart' as http;

import 'package:flutter/services.dart';
import 'spine_flutter_bindings_generated.dart';
export 'spine_widget.dart';
import 'package:path/path.dart' as path;

int majorVersion() => _bindings.spine_major_version();
int minorVersion() => _bindings.spine_minor_version();
void reportLeaks() => _bindings.spine_report_leaks();

class Color {
  double r;
  double g;
  double b;
  double a;

  Color(this.r, this.g, this.b, this.a);
}

class Bounds {
  double x;
  double y;
  double width;
  double height;

  Bounds(this.x, this.y, this.width, this.height);
}

class Atlas {
  final Pointer<spine_atlas> _atlas;
  final List<Image> atlasPages;
  final List<Paint> atlasPagePaints;
  bool _disposed;

  Atlas(this._atlas, this.atlasPages, this.atlasPagePaints): _disposed = false;

  static Future<Atlas> _load(String atlasFileName, Future<Uint8List> Function(String name) loadFile) async {
    final atlasBytes = await loadFile(atlasFileName);
    final atlasData = convert.utf8.decode(atlasBytes);
    final atlasDataNative = atlasData.toNativeUtf8();
    final atlas = _bindings.spine_atlas_load(atlasDataNative.cast());
    calloc.free(atlasDataNative);
    if (atlas.ref.error.address != nullptr.address) {
      final Pointer<Utf8> error = atlas.ref.error.cast();
      final message = error.toDartString();
      _bindings.spine_atlas_dispose(atlas);
      throw Exception("Couldn't load atlas: $message");
    }

    final atlasDir = path.dirname(atlasFileName);
    List<Image> atlasPages = [];
    List<Paint> atlasPagePaints = [];
    for (int i = 0; i < atlas.ref.numImagePaths; i++) {
      final Pointer<Utf8> atlasPageFile = atlas.ref.imagePaths[i].cast();
      final imagePath = path.join(atlasDir, atlasPageFile.toDartString());
      var imageData = await loadFile(imagePath);
      final Codec codec = await instantiateImageCodec(imageData);
      final FrameInfo frameInfo = await codec.getNextFrame();
      final Image image = frameInfo.image;
      atlasPages.add(image);
      atlasPagePaints.add(Paint()
        ..shader = ImageShader(image, TileMode.clamp, TileMode.clamp, Matrix4.identity().storage, filterQuality: FilterQuality.high)
        ..isAntiAlias = true
      );
    }

    return Atlas(atlas, atlasPages, atlasPagePaints);
  }

  static Future<Atlas> fromAsset(AssetBundle assetBundle, String atlasFileName) async {
    return _load(atlasFileName, (file) async => (await assetBundle.load(file)).buffer.asUint8List());
  }

  static Future<Atlas> fromFile(String atlasFileName) async {
    return _load(atlasFileName, (file) => File(file).readAsBytes());
  }

  static Future<Atlas> fromUrl(String atlasFileName) async {
    return _load(atlasFileName, (file) async {
      return (await http.get(Uri.parse(file))).bodyBytes;
    });
  }

  void dispose() {
    if (_disposed) return;
    _disposed = true;
    _bindings.spine_atlas_dispose(this._atlas);
    for (final image in atlasPages) image.dispose();
  }
}

class SkeletonData {
  final spine_skeleton_data _skeletonData;
  bool _disposed;

  SkeletonData(this._skeletonData): _disposed = false;

  static SkeletonData fromJson(Atlas atlas, String json) {
    final jsonNative = json.toNativeUtf8();
    final result = _bindings.spine_skeleton_data_load_json(atlas._atlas, jsonNative.cast());
    calloc.free(jsonNative);
    if (result.error.address != nullptr.address) {
      final Pointer<Utf8> error = result.error.cast();
      final message = error.toDartString();
      calloc.free(error);
      throw Exception("Couldn't load skeleton data: $message");
    }
    return SkeletonData(result.skeletonData);
  }

  static SkeletonData fromBinary(Atlas atlas, Uint8List binary) {
    final Pointer<Uint8> binaryNative = malloc.allocate(binary.lengthInBytes);
    binaryNative.asTypedList(binary.lengthInBytes).setAll(0, binary);
    final result = _bindings.spine_skeleton_data_load_binary(atlas._atlas, binaryNative.cast(), binary.lengthInBytes);
    malloc.free(binaryNative);
    if (result.error.address != nullptr.address) {
      final Pointer<Utf8> error = result.error.cast();
      final message = error.toDartString();
      calloc.free(error);
      throw Exception("Couldn't load skeleton data: $message");
    }
    return SkeletonData(result.skeletonData);
  }

  void dispose() {
    if (_disposed) return;
    _disposed = true;
    _bindings.spine_skeleton_data_dispose(_skeletonData);
  }
}

enum BlendMode {
  Normal(0),
  Additive(1),
  Multiply(2),
  Screen(3);

  final int value;
  const BlendMode(this.value);
}

class BoneData {
  final spine_bone_data _data;

  BoneData(this._data);
}

class Bone {
  final spine_bone _bone;

  Bone(this._bone);
}

class SlotData {
  final spine_slot_data _data;

  SlotData(this._data);

  int getIndex() {
    return _bindings.spine_slot_data_get_index(_data);
  }

  String getName() {
    final Pointer<Utf8> value = _bindings.spine_slot_data_get_name(_data).cast();
    return value.toDartString();
  }

  BoneData getBoneData() {
    return BoneData(_bindings.spine_slot_data_get_bone_data(_data));
  }

  Color getColor() {
    final color = _bindings.spine_slot_data_get_color(_data);
    return Color(color.r, color.g, color.b, color.a);
  }

  Color getDarkColor() {
    final color = _bindings.spine_slot_data_get_dark_color(_data);
    return Color(color.r, color.g, color.b, color.a);
  }

  bool hasDarkColor() {
    return _bindings.spine_slot_data_has_dark_color(_data) == -1;
  }

  String getAttachmentName() {
    final Pointer<Utf8> value = _bindings.spine_slot_data_get_attachment_name(_data).cast();
    return value.toDartString();
  }

  BlendMode getBlendMode() {
    return BlendMode.values[_bindings.spine_slot_data_get_blend_mode(_data)];
  }
}

class Slot {
  final spine_slot _slot;

  Slot(this._slot);

  void setToSetupPose() {
    _bindings.spine_slot_set_to_setup_pose(_slot);
  }

  SlotData getData() {
    return SlotData(_bindings.spine_slot_get_data(_slot));
  }

  Bone getBone() {
    return Bone(_bindings.spine_slot_get_bone(_slot));
  }

  Skeleton getSkeleton() {
    return Skeleton(_bindings.spine_slot_get_skeleton(_slot));
  }

  Color getColor() {
    final color = _bindings.spine_slot_get_color(_slot);
    return Color(color.r, color.g, color.b, color.a);
  }

  void setColor(Color color) {
    _bindings.spine_slot_set_color(_slot, color.r, color.g, color.b, color.a);
  }

  Color getDarkColor() {
    final color = _bindings.spine_slot_get_dark_color(_slot);
    return Color(color.r, color.g, color.b, color.a);
  }

  void setDarkColor(Color color) {
    _bindings.spine_slot_set_dark_color(_slot, color.r, color.g, color.b, color.a);
  }

  bool hasDarkColor() {
    return _bindings.spine_slot_has_dark_color(_slot) == -1;
  }

  Attachment? getAttachment() {
    final attachment = _bindings.spine_slot_get_attachment(_slot);
    if (attachment.address == nullptr.address) return null;
    return Attachment(attachment);
  }

  void setAttachment(Attachment? attachment) {
    _bindings.spine_slot_set_attachment(_slot, attachment != null ? attachment._attachment : nullptr);
  }
}

class Attachment {
  final spine_attachment _attachment;

  Attachment(this._attachment);
}

class Skin {
  final spine_skin _skin;

  Skin(this._skin);
}

class IkConstraint {
  final spine_ik_constraint _constraint;

  IkConstraint(this._constraint);
}

class TransformConstraint {
  final spine_transform_constraint _constraint;

  TransformConstraint(this._constraint);
}

class PathConstraint {
  final spine_path_constraint _constraint;

  PathConstraint(this._constraint);
}

class Skeleton {
  final spine_skeleton _skeleton;

  Skeleton(this._skeleton);

  /// Caches information about bones and constraints. Must be called if bones, constraints or weighted path attachments are added
  /// or removed.
  void updateCache() {
    _bindings.spine_skeleton_update_cache(_skeleton);
  }

  /// Updates the world transform for each bone and applies constraints.
  void updateWorldTransform() {
    _bindings.spine_skeleton_update_world_transform(_skeleton);
  }

  void updateWorldTransformBone(Bone parent) {
    _bindings.spine_skeleton_update_world_transform_bone(_skeleton, parent._bone);
  }

  /// Sets the bones, constraints, and slots to their setup pose values.
  void setToSetupPose() {
    _bindings.spine_skeleton_set_to_setup_pose(_skeleton);
  }

  /// Sets the bones and constraints to their setup pose values.
  void setBonesToSetupPose() {
    _bindings.spine_skeleton_set_bones_to_setup_pose(_skeleton);
  }

  void setSlotsToSetupPose() {
    _bindings.spine_skeleton_set_slots_to_setup_pose(_skeleton);
  }

  Bone? findBone(String boneName) {
    final nameNative = boneName.toNativeUtf8();
    final bone = _bindings.spine_skeleton_find_bone(_skeleton, nameNative.cast());
    calloc.free(nameNative);
    if (bone.address == nullptr.address) return null;
    return Bone(bone);
  }

  Slot? findSlot(String slotName) {
    final nameNative = slotName.toNativeUtf8();
    final slot = _bindings.spine_skeleton_find_slot(_skeleton, nameNative.cast());
    calloc.free(nameNative);
    if (slot.address == nullptr.address) return null;
    return Slot(slot);
  }

  /// Attachments from the new skin are attached if the corresponding attachment from the old skin was attached.
  /// If there was no old skin, each slot's setup mode attachment is attached from the new skin.
  /// After changing the skin, the visible attachments can be reset to those attached in the setup pose by calling
  /// See Skeleton::setSlotsToSetupPose()
  /// Also, often AnimationState::apply(Skeleton&) is called before the next time the
  /// skeleton is rendered to allow any attachment keys in the current animation(s) to hide or show attachments from the new skin.
  /// @param skinName May be NULL.
  void setSkin(String skinName) {
    final nameNative = skinName.toNativeUtf8();
    _bindings.spine_skeleton_set_skin(_skeleton, nameNative.cast());
    calloc.free(nameNative);
  }

  Attachment? getAttachmentByName(String slotName, String attachmentName) {
    final slotNameNative = slotName.toNativeUtf8();
    final attachmentNameNative = attachmentName.toNativeUtf8();
    final attachment = _bindings.spine_skeleton_get_attachment_by_name(_skeleton, slotNameNative.cast(), attachmentNameNative.cast());
    calloc.free(slotNameNative);
    calloc.free(attachmentNameNative);
    if (attachment.address == nullptr.address) return null;
    return Attachment(attachment);
  }

  Attachment? getAttachment(int slotIndex, String attachmentName) {
    final attachmentNameNative = attachmentName.toNativeUtf8();
    final attachment = _bindings.spine_skeleton_get_attachment(_skeleton, slotIndex, attachmentNameNative.cast());
    calloc.free(attachmentNameNative);
    if (attachment.address == nullptr.address) return null;
    return Attachment(attachment);
  }

  void setAttachment(String slotName, String attachmentName) {
    final slotNameNative = slotName.toNativeUtf8();
    final attachmentNameNative = attachmentName.toNativeUtf8();
    _bindings.spine_skeleton_set_attachment(_skeleton, slotNameNative.cast(), attachmentNameNative.cast());
    calloc.free(slotNameNative);
    calloc.free(attachmentNameNative);
  }

  IkConstraint? findIkConstraint(String constraintName) {
    final nameNative = constraintName.toNativeUtf8();
    final constraint = _bindings.spine_skeleton_find_ik_constraint(_skeleton, nameNative.cast());
    calloc.free(nameNative);
    if (constraint.address == nullptr.address) return null;
    return IkConstraint(constraint);
  }

  TransformConstraint? findTransformConstraint(String constraintName) {
    final nameNative = constraintName.toNativeUtf8();
    final constraint = _bindings.spine_skeleton_find_transform_constraint(_skeleton, nameNative.cast());
    calloc.free(nameNative);
    if (constraint.address == nullptr.address) return null;
    return TransformConstraint(constraint);
  }

  PathConstraint? findPathConstraint(String constraintName) {
    final nameNative = constraintName.toNativeUtf8();
    final constraint = _bindings.spine_skeleton_find_path_constraint(_skeleton, nameNative.cast());
    calloc.free(nameNative);
    if (constraint.address == nullptr.address) return null;
    return PathConstraint(constraint);
  }

  /// Returns the axis aligned bounding box (AABB) of the region and mesh attachments for the current pose.
  /// @param outX The horizontal distance between the skeleton origin and the left side of the AABB.
  /// @param outY The vertical distance between the skeleton origin and the bottom side of the AABB.
  /// @param outWidth The width of the AABB
  /// @param outHeight The height of the AABB.
  /// @param outVertexBuffer Reference to hold a Vector of floats. This method will assign it with new floats as needed.
  Bounds getBounds() {
    final nativeBounds = _bindings.spine_skeleton_get_bounds(_skeleton);
    return Bounds(nativeBounds.x, nativeBounds.y, nativeBounds.width, nativeBounds.height);
  }

  Bone? getRootBone() {
    final bone = _bindings.spine_skeleton_get_root_bone(_skeleton);
    if (bone.address == nullptr.address) return null;
    return Bone(bone);
  }

  SkeletonData? getData() {
    final data = _bindings.spine_skeleton_get_data(_skeleton);
    if (data.address == nullptr.address) return null;
    return SkeletonData(data);
  }

  List<Bone> getBones() {
    final List<Bone> bones = [];
    final numBones = _bindings.spine_skeleton_get_num_bones(_skeleton);
    final nativeBones = _bindings.spine_skeleton_get_bones(_skeleton);
    for (int i = 0; i < numBones; i++) {
      bones.add(Bone(nativeBones[i]));
    }
    return bones;
  }

  List<Slot> getSlots() {
    final List<Slot> slots = [];
    final numSlots = _bindings.spine_skeleton_get_num_slots(_skeleton);
    final nativeSlots = _bindings.spine_skeleton_get_slots(_skeleton);
    for (int i = 0; i < numSlots; i++) {
      slots.add(Slot(nativeSlots[i]));
    }
    return slots;
  }

  List<Slot> getDrawOrder() {
    final List<Slot> slots = [];
    final numSlots = _bindings.spine_skeleton_get_num_draw_order(_skeleton);
    final nativeDrawOrder = _bindings.spine_skeleton_get_draw_order(_skeleton);
    for (int i = 0; i < numSlots; i++) {
      slots.add(Slot(nativeDrawOrder[i]));
    }
    return slots;
  }

  List<IkConstraint> getIkConstraints() {
    final List<IkConstraint> constraints = [];
    final numConstraints = _bindings.spine_skeleton_get_num_ik_constraints(_skeleton);
    final nativeConstraints = _bindings.spine_skeleton_get_ik_constraints(_skeleton);
    for (int i = 0; i < numConstraints; i++) {
      constraints.add(IkConstraint(nativeConstraints[i]));
    }
    return constraints;
  }

  List<PathConstraint> getPathConstraints() {
    final List<PathConstraint> constraints = [];
    final numConstraints = _bindings.spine_skeleton_get_num_path_constraints(_skeleton);
    final nativeConstraints = _bindings.spine_skeleton_get_path_constraints(_skeleton);
    for (int i = 0; i < numConstraints; i++) {
      constraints.add(PathConstraint(nativeConstraints[i]));
    }
    return constraints;
  }

  List<TransformConstraint> getTransformConstraints() {
    final List<TransformConstraint> constraints = [];
    final numConstraints = _bindings.spine_skeleton_get_num_transform_constraints(_skeleton);
    final nativeConstraints = _bindings.spine_skeleton_get_transform_constraints(_skeleton);
    for (int i = 0; i < numConstraints; i++) {
      constraints.add(TransformConstraint(nativeConstraints[i]));
    }
    return constraints;
  }

  Skin? getSkin() {
    final skin = _bindings.spine_skeleton_get_skin(_skeleton);
    if (skin.address == nullptr.address) return null;
    return Skin(skin);
  }

  Color getColor() {
    final color = _bindings.spine_skeleton_get_color(_skeleton);
    return Color(color.r, color.g, color.b, color.a);
  }

  void setColor(Color color) {
    _bindings.spine_skeleton_set_color(_skeleton, color.r, color.g, color.b, color.a);
  }

  void setPosition(double x, double y) {
    _bindings.spine_skeleton_set_position(_skeleton, x, y);
  }

  double getX() {
    return _bindings.spine_skeleton_get_x(_skeleton);
  }

  void setX(double x) {
    _bindings.spine_skeleton_set_x(_skeleton, x);
  }

  double getY() {
    return _bindings.spine_skeleton_get_x(_skeleton);
  }

  void setY(double y) {
    _bindings.spine_skeleton_set_y(_skeleton, y);
  }

  double getScaleX() {
    return _bindings.spine_skeleton_get_scale_x(_skeleton);
  }

  void setScaleX(double scaleX) {
    _bindings.spine_skeleton_set_scale_x(_skeleton, scaleX);
  }

  double getScaleY() {
    return _bindings.spine_skeleton_get_scale_x(_skeleton);
  }

  void setScaleY(double scaleY) {
    _bindings.spine_skeleton_set_scale_y(_skeleton, scaleY);
  }
}

class Animation {
  final spine_animation _animation;

  Animation(this._animation);

  String getName() {
    final Pointer<Utf8> value = _bindings.spine_animation_get_name(_animation).cast();
    return value.toDartString();
  }

  double getDuration() {
    return _bindings.spine_animation_get_duration(_animation);
  }
}

enum MixBlend {
  Setup(0),
  First(1),
  Replace(2),
  Add(3);

  final int value;
  const MixBlend(this.value);
}

class TrackEntry {
  final spine_track_entry _entry;
  final AnimationState _state;

  TrackEntry(this._entry, this._state);

  /// The index of the track where this entry is either current or queued.
  int getTtrackIndex() {
    return _bindings.spine_track_entry_get_track_index(_entry);
  }

  /// The animation to apply for this track entry.
  Animation getAnimation() {
    return Animation(_bindings.spine_track_entry_get_animation(_entry));
  }

  /// If true, the animation will repeat. If false, it will not, instead its last frame is applied if played beyond its duration.
  bool getLoop() {
    return _bindings.spine_track_entry_get_loop(_entry) == -1;
  }

  void setLoop(bool loop) {
    _bindings.spine_track_entry_set_loop(_entry, loop ? -1 : 0);
  }

  /// If true, when mixing from the previous animation to this animation, the previous animation is applied as normal instead
  /// of being mixed out.
  ///
  /// When mixing between animations that key the same property, if a lower track also keys that property then the value will
  /// briefly dip toward the lower track value during the mix. This happens because the first animation mixes from 100% to 0%
  /// while the second animation mixes from 0% to 100%. Setting holdPrevious to true applies the first animation
  /// at 100% during the mix so the lower track value is overwritten. Such dipping does not occur on the lowest track which
  /// keys the property, only when a higher track also keys the property.
  ///
  /// Snapping will occur if holdPrevious is true and this animation does not key all the same properties as the
  /// previous animation.
  bool getHoldPrevious() {
    return _bindings.spine_track_entry_get_hold_previous(_entry) == -1;
  }

  void setHoldPrevious(bool holdPrevious) {
    _bindings.spine_track_entry_set_hold_previous(_entry, holdPrevious ? -1 : 0);
  }

  bool getReverse() {
    return _bindings.spine_track_entry_get_reverse(_entry) == -1;
  }

  void setReverse(bool reverse) {
    _bindings.spine_track_entry_set_reverse(_entry, reverse ? -1 : 0);
  }

  bool getShortestRotation() {
    return _bindings.spine_track_entry_get_shortest_rotation(_entry) == 1;
  }

  void setShortestRotation(bool shortestRotation) {
    _bindings.spine_track_entry_set_shortest_rotation(_entry, shortestRotation ? -1 : 0);
  }

  /// Seconds to postpone playing the animation. When a track entry is the current track entry, delay postpones incrementing
  /// the track time. When a track entry is queued, delay is the time from the start of the previous animation to when the
  /// track entry will become the current track entry.
  double getDelay() {
    return _bindings.spine_track_entry_get_delay(_entry);
  }

  void setDelay(double delay) {
    _bindings.spine_track_entry_set_delay(_entry, delay);
  }

  /// Current time in seconds this track entry has been the current track entry. The track time determines
  /// TrackEntry.AnimationTime. The track time can be set to start the animation at a time other than 0, without affecting looping.
  double getTrackTime() {
    return _bindings.spine_track_entry_get_track_time(_entry);
  }

  void setTrackTime(double trackTime) {
    _bindings.spine_track_entry_set_track_time(_entry, trackTime);
  }

  /// The track time in seconds when this animation will be removed from the track. Defaults to the animation duration for
  /// non-looping animations and to int.MaxValue for looping animations. If the track end time is reached and no
  /// other animations are queued for playback, and mixing from any previous animations is complete, properties keyed by the animation,
  /// are set to the setup pose and the track is cleared.
  ///
  /// It may be desired to use AnimationState.addEmptyAnimation(int, float, float) to mix the properties back to the
  /// setup pose over time, rather than have it happen instantly.
  double getTrackEnd() {
    return _bindings.spine_track_entry_get_track_end(_entry);
  }

  void setTrackEnd(double trackEnd) {
    _bindings.spine_track_entry_set_track_end(_entry, trackEnd);
  }

  /// Seconds when this animation starts, both initially and after looping. Defaults to 0.
  ///
  /// When changing the animation start time, it often makes sense to set TrackEntry.AnimationLast to the same value to
  /// prevent timeline keys before the start time from triggering.
  double getAnimationStart() {
    return _bindings.spine_track_entry_get_animation_start(_entry);
  }

  void setAnimationStart(double animationStart) {
    _bindings.spine_track_entry_set_animation_start(_entry, animationStart);
  }

  /// Seconds for the last frame of this animation. Non-looping animations won't play past this time. Looping animations will
  /// loop back to TrackEntry.AnimationStart at this time. Defaults to the animation duration.
  double getAnimationEnd() {
    return _bindings.spine_track_entry_get_animation_end(_entry);
  }

  void setAnimationEnd(double animationEnd) {
    _bindings.spine_track_entry_set_animation_end(_entry, animationEnd);
  }

  /// The time in seconds this animation was last applied. Some timelines use this for one-time triggers. Eg, when this
  /// animation is applied, event timelines will fire all events between the animation last time (exclusive) and animation time
  /// (inclusive). Defaults to -1 to ensure triggers on frame 0 happen the first time this animation is applied.
  double getAnimationLast() {
    return _bindings.spine_track_entry_get_animation_last(_entry);
  }

  void setAnimationLast(double animationLast) {
    _bindings.spine_track_entry_set_animation_last(_entry, animationLast);
  }

  /// Uses TrackEntry.TrackTime to compute the animation time between TrackEntry.AnimationStart. and
  /// TrackEntry.AnimationEnd. When the track time is 0, the animation time is equal to the animation start time.
  double getAnimationTime() {
    return _bindings.spine_track_entry_get_animation_time(_entry);
  }

  /// Multiplier for the delta time when the animation state is updated, causing time for this animation to play slower or
  /// faster. Defaults to 1.
  double getTimeScale() {
    return _bindings.spine_animation_state_get_time_scale(_entry);
  }

  void setTimeScale(double timeScale) {
    _bindings.spine_track_entry_set_time_scale(_entry, timeScale);
  }

  /// Values less than 1 mix this animation with the last skeleton pose. Defaults to 1, which overwrites the last skeleton pose with
  /// this animation.
  ///
  /// Typically track 0 is used to completely pose the skeleton, then alpha can be used on higher tracks. It doesn't make sense
  /// to use alpha on track 0 if the skeleton pose is from the last frame render.
  double getAlpha() {
    return _bindings.spine_track_entry_get_alpha(_entry);
  }

  void setAlpha(double alpha) {
    _bindings.spine_track_entry_set_alpha(_entry, alpha);
  }

  /// When the mix percentage (mix time / mix duration) is less than the event threshold, event timelines for the animation
  /// being mixed out will be applied. Defaults to 0, so event timelines are not applied for an animation being mixed out.
  double getEventThreshold() {
    return _bindings.spine_track_entry_get_event_threshold(_entry);
  }

  void setEventThreshold(double eventThreshold) {
    _bindings.spine_track_entry_set_event_threshold(_entry, eventThreshold);
  }

  /// When the mix percentage (mix time / mix duration) is less than the attachment threshold, attachment timelines for the
  /// animation being mixed out will be applied. Defaults to 0, so attachment timelines are not applied for an animation being
  /// mixed out.
  double getAttachmentThreshold() {
    return _bindings.spine_track_entry_get_attachment_threshold(_entry);
  }

  void setAttachmentThreshold(double attachmentThreshold) {
    _bindings.spine_track_entry_set_attachment_threshold(_entry, attachmentThreshold);
  }

  /// When the mix percentage (mix time / mix duration) is less than the draw order threshold, draw order timelines for the
  /// animation being mixed out will be applied. Defaults to 0, so draw order timelines are not applied for an animation being
  /// mixed out.
  double getDrawOrderThreshold() {
    return _bindings.spine_track_entry_get_draw_order_threshold(_entry);
  }

  void setDrawOrderThreshold(double drawOrderThreshold) {
    _bindings.spine_track_entry_set_draw_order_threshold(_entry, drawOrderThreshold);
  }

  /// The animation queued to start after this animation, or null.
  TrackEntry? getNext() {
    final next = _bindings.spine_track_entry_get_next(_entry);
    if (next.address == nullptr.address) return null;
    return TrackEntry(next, this._state);
  }

  /// Returns true if at least one loop has been completed.
  bool isComplete() {
    return _bindings.spine_track_entry_is_complete(_entry) == -1;
  }

  /// Seconds from 0 to the mix duration when mixing from the previous animation to this animation. May be slightly more than
  /// TrackEntry.MixDuration when the mix is complete.
  double getMixTime() {
    return _bindings.spine_track_entry_get_mix_time(_entry);
  }

  void setMixTime(double mixTime) {
    _bindings.spine_track_entry_set_mix_time(_entry, mixTime);
  }

  /// Seconds for mixing from the previous animation to this animation. Defaults to the value provided by
  /// AnimationStateData based on the animation before this animation (if any).
  ///
  /// The mix duration can be set manually rather than use the value from AnimationStateData.GetMix.
  /// In that case, the mixDuration must be set before AnimationState.update(float) is next called.
  ///
  /// When using AnimationState::addAnimation(int, Animation, bool, float) with a delay
  /// less than or equal to 0, note the Delay is set using the mix duration from the AnimationStateData
  double getMixDuration() {
    return _bindings.spine_track_entry_get_mix_duration(_entry);
  }

  void setMixDuration(double mixDuration) {
    _bindings.spine_track_entry_set_mix_duration(_entry, mixDuration);
  }

  MixBlend getMixBlend() {
    return MixBlend.values[_bindings.spine_track_entry_get_mix_blend(_entry)];
  }

  void setMixBlend(MixBlend mixBlend) {
    _bindings.spine_track_entry_set_mix_blend(_entry, mixBlend.value);
  }

  /// The track entry for the previous animation when mixing from the previous animation to this animation, or NULL if no
  /// mixing is currently occuring. When mixing from multiple animations, MixingFrom makes up a double linked list with MixingTo.
  TrackEntry? getMixingFrom() {
    final from = _bindings.spine_track_entry_get_mixing_from(_entry);
    if (from.address == nullptr.address) return null;
    return TrackEntry(from, this._state);
  }

  /// The track entry for the next animation when mixing from this animation, or NULL if no mixing is currently occuring.
  /// When mixing from multiple animations, MixingTo makes up a double linked list with MixingFrom.
  TrackEntry? getMixingTo() {
    final to = _bindings.spine_track_entry_get_mixing_to(_entry);
    if (to.address == nullptr.address) return null;
    return TrackEntry(to, this._state);
  }

  /// Resets the rotation directions for mixing this entry's rotate timelines. This can be useful to avoid bones rotating the
  /// long way around when using alpha and starting animations on other tracks.
  ///
  /// Mixing involves finding a rotation between two others, which has two possible solutions: the short way or the long way around.
  /// The two rotations likely change over time, so which direction is the short or long way also changes.
  /// If the short way was always chosen, bones would flip to the other side when that direction became the long way.
  /// TrackEntry chooses the short way the first time it is applied and remembers that direction.
  void resetRotationDirections() {
    _bindings.spine_track_entry_reset_rotation_directions(_entry);
  }

  double getTrackComplete() {
    return _bindings.spine_track_entry_get_track_complete(_entry);
  }

  void setListener(AnimationStateListener? listener) {
    _state._setTrackEntryListener(_entry, listener);
  }
}

enum EventType {
  Start,
  Interrupt,
  End,
  Complete,
  Dispose,
  Event
}

class EventData {
  final spine_event_data _data;

  EventData(this._data);

  String getName() {
    final Pointer<Utf8> value = _bindings.spine_event_data_get_name(_data).cast();
    return value.toDartString();
  }

  int getIntValue() {
    return _bindings.spine_event_data_get_int_value(_data);
  }

  double getFloatValue() {
    return _bindings.spine_event_data_get_float_value(_data);
  }

  String getStringValue() {
    final Pointer<Utf8> value = _bindings.spine_event_data_get_string_value(_data).cast();
    return value.toDartString();
  }

  String getAudioPath() {
    final Pointer<Utf8> value = _bindings.spine_event_data_get_audio_path(_data).cast();
    return value.toDartString();
  }

  double getVolume() {
    return _bindings.spine_event_data_get_volume(_data);
  }

  double getBalance() {
    return _bindings.spine_event_data_get_balance(_data);
  }
}

class Event {
  final spine_event _event;

  Event(this._event);

  EventData getData() {
    return EventData(_bindings.spine_event_get_data(_event));
  }

  double getTime() {
    return _bindings.spine_event_get_time(_event);
  }

  int getIntValue() {
    return _bindings.spine_event_get_int_value(_event);
  }

  double getFloatValue() {
    return _bindings.spine_event_get_float_value(_event);
  }

  String getStringValue() {
    final Pointer<Utf8> value = _bindings.spine_event_get_string_value(_event).cast();
    return value.toDartString();
  }

  double getVolume() {
    return _bindings.spine_event_get_volume(_event);
  }

  double getBalance() {
    return _bindings.spine_event_get_balance(_event);
  }
}

class AnimationStateEvent {
  final EventType type;
  final TrackEntry entry;
  final Event? event;

  AnimationStateEvent(this.type, this.entry, this.event);
}

typedef AnimationStateListener = void Function(AnimationStateEvent event);

class AnimationState {
  final spine_animation_state _state;
  final spine_animation_state_events _events;
  final Map<spine_track_entry, AnimationStateListener> _trackEntryListeners;
  AnimationStateListener? _stateListener;

  AnimationState(this._state, this._events): _trackEntryListeners = {};

  void _setTrackEntryListener(spine_track_entry entry, AnimationStateListener? listener) {
    if (listener == null) _trackEntryListeners.remove(entry);
    else _trackEntryListeners[entry] = listener;
  }

  /// Increments the track entry times, setting queued animations as current if needed
  /// @param delta delta time
  void update(double delta) {
    _bindings.spine_animation_state_update(_state, delta);

    final numEvents = _bindings.spine_animation_state_events_get_num_events(_events);
    if (numEvents > 0) {
      for (int i = 0; i < numEvents; i++) {
        late final EventType type;
        switch(_bindings.spine_animation_state_events_get_event_type(_events, i)) {
          case 0:
            type = EventType.Start;
            break;
          case 1:
            type = EventType.Interrupt;
            break;
          case 2:
            type = EventType.End;
            break;
          case 3:
            type = EventType.Complete;
            break;
          case 4:
            type = EventType.Dispose;
            break;
          case 5:
            type = EventType.Event;
            break;
        }
        final entry = _bindings.spine_animation_state_events_get_track_entry(_events, i);
        final nativeEvent = _bindings.spine_animation_state_events_get_event(_events, i);
        final event = AnimationStateEvent(type, TrackEntry(entry, this), nativeEvent.address == nullptr.address ? null : Event(nativeEvent));
        if (_trackEntryListeners.containsKey(entry)) {
          _trackEntryListeners[entry]?.call(event);
        }
        if (_stateListener != null) {
          _stateListener?.call(event);
        }
        if (type == EventType.Dispose) {
          _bindings.spine_animation_state_dispose_track_entry(_state, entry);
        }
      }
    }
    _bindings.spine_animation_state_events_reset(_events);
  }

  /// Poses the skeleton using the track entry animations. There are no side effects other than invoking listeners, so the
  /// animation state can be applied to multiple skeletons to pose them identically.
  void apply(Skeleton skeleton) {
    _bindings.spine_animation_state_apply(_state, skeleton._skeleton);
  }

  /// Removes all animations from all tracks, leaving skeletons in their previous pose.
  /// It may be desired to use AnimationState.setEmptyAnimations(float) to mix the skeletons back to the setup pose,
  /// rather than leaving them in their previous pose.
  void clearTracks() {
    _bindings.spine_animation_state_clear_tracks(_state);
  }

  /// Removes all animations from the tracks, leaving skeletons in their previous pose.
  /// It may be desired to use AnimationState.setEmptyAnimations(float) to mix the skeletons back to the setup pose,
  /// rather than leaving them in their previous pose.
  void clearTrack(int trackIndex) {
    _bindings.spine_animation_state_clear_track(_state, trackIndex);
  }

  /// Sets the current animation for a track, discarding any queued animations.
  /// @param loop If true, the animation will repeat.
  /// If false, it will not, instead its last frame is applied if played beyond its duration.
  /// In either case TrackEntry.TrackEnd determines when the track is cleared.
  /// @return
  /// A track entry to allow further customization of animation playback. References to the track entry must not be kept
  /// after AnimationState.Dispose.
  TrackEntry setAnimation(int trackIndex, String animationName, bool loop) {
    final animation = animationName.toNativeUtf8();
    final entry = _bindings.spine_animation_state_set_animation(_state, trackIndex, animation.cast(), loop ? -1 : 0);
    calloc.free(animation);
    if (entry.address == nullptr.address) throw Exception("Couldn't set animation $animationName");
    return TrackEntry(entry, this);
  }

  /// Adds an animation to be played delay seconds after the current or last queued animation
  /// for a track. If the track is empty, it is equivalent to calling setAnimation.
  /// @param delay
  /// Seconds to begin this animation after the start of the previous animation. May be &lt;= 0 to use the animation
  /// duration of the previous track minus any mix duration plus the negative delay.
  ///
  /// @return A track entry to allow further customization of animation playback. References to the track entry must not be kept
  /// after AnimationState.Dispose
  TrackEntry addAnimation(int trackIndex, String animationName, bool loop, double delay) {
    final animation = animationName.toNativeUtf8();
    final entry = _bindings.spine_animation_state_add_animation(_state, trackIndex, animation.cast(), loop ? -1 : 0, delay);
    calloc.free(animation);
    if (entry.address == nullptr.address) throw Exception("Couldn't add animation $animationName");
    return TrackEntry(entry, this);
  }

  /// Sets an empty animation for a track, discarding any queued animations, and mixes to it over the specified mix duration.
  TrackEntry setEmptyAnimation(int trackIndex, double mixDuration) {
    final entry = _bindings.spine_animation_state_set_empty_animation(_state, trackIndex, mixDuration);
    return TrackEntry(entry, this);
  }

  /// Adds an empty animation to be played after the current or last queued animation for a track, and mixes to it over the
  /// specified mix duration.
  /// @return
  /// A track entry to allow further customization of animation playback. References to the track entry must not be kept after AnimationState.Dispose.
  ///
  /// @param trackIndex Track number.
  /// @param mixDuration Mix duration.
  /// @param delay Seconds to begin this animation after the start of the previous animation. May be &lt;= 0 to use the animation
  /// duration of the previous track minus any mix duration plus the negative delay.
  TrackEntry addEmptyAnimation(int trackIndex, double mixDuration, double delay) {
    final entry = _bindings.spine_animation_state_add_empty_animation(_state, trackIndex, mixDuration, delay);
    return TrackEntry(entry, this);
  }

  TrackEntry? getCurrent(int trackIndex) {
    final entry = _bindings.spine_animation_state_get_current(_state, trackIndex);
    if (entry.address == nullptr.address) return null;
    return TrackEntry(entry, this);
  }

  /// Sets an empty animation for every track, discarding any queued animations, and mixes to it over the specified mix duration.
  void setEmptyAnimations(double mixDuration) {
    _bindings.spine_animation_state_set_empty_animations(_state, mixDuration);
  }

  double getTimeScale() {
    return _bindings.spine_animation_state_get_time_scale(_state);
  }

  void setTimeScale(double timeScale) {
    _bindings.spine_animation_state_set_time_scale(_state, timeScale);
  }

  void setListener(AnimationStateListener? listener) {
    _stateListener = listener;
  }
}

class SkeletonDrawable {
  final Atlas atlas;
  final SkeletonData skeletonData;
  late final Pointer<spine_skeleton_drawable> _drawable;
  late final Skeleton skeleton;
  late final AnimationState animationState;
  final bool _ownsAtlasAndSkeletonData;
  bool _disposed;

  SkeletonDrawable(this.atlas, this.skeletonData, this._ownsAtlasAndSkeletonData): _disposed = false {
    _drawable = _bindings.spine_skeleton_drawable_create(skeletonData._skeletonData);
    skeleton = Skeleton(_drawable.ref.skeleton);
    animationState = AnimationState(_drawable.ref.animationState, _drawable.ref.animationStateEvents);
  }

  void update(double delta) {
    if (_disposed) return;
    animationState.update(delta);
    animationState.apply(skeleton);
    skeleton.updateWorldTransform();
  }

  List<RenderCommand> render() {
    if (_disposed) return [];
    Pointer<spine_render_command> nativeCmd = _bindings.spine_skeleton_drawable_render(_drawable);
    List<RenderCommand> commands = [];
    while(nativeCmd.address != nullptr.address) {
      final atlasPage = atlas.atlasPages[nativeCmd.ref.atlasPage];
      commands.add(RenderCommand(nativeCmd, atlasPage.width.toDouble(), atlasPage.height.toDouble()));
      nativeCmd = nativeCmd.ref.next;
    }
    return commands;
  }

  void dispose() {
    if (_disposed) return;
    _disposed = true;
    if (_ownsAtlasAndSkeletonData) {
      atlas.dispose();
      skeletonData.dispose();
    }
    _bindings.spine_skeleton_drawable_dispose(_drawable);
  }
}

class RenderCommand {
  late final Vertices vertices;
  late final int atlasPageIndex;

  RenderCommand(Pointer<spine_render_command> nativeCmd, double pageWidth, double pageHeight) {
    atlasPageIndex = nativeCmd.ref.atlasPage;
    int numVertices = nativeCmd.ref.numVertices;
    int numIndices = nativeCmd.ref.numIndices;
    final uvs = nativeCmd.ref.uvs.asTypedList(numVertices * 2);
    for (int i = 0; i < numVertices * 2; i += 2) {
      uvs[i] *= pageWidth;
      uvs[i+1] *= pageHeight;
    }
    // We pass the native data as views directly to Vertices.raw. According to the sources, the data
    // is copied, so it doesn't matter that we free up the underlying memory on the next
    // render call. See the implementation of Vertices.raw() here:
    // https://github.com/flutter/engine/blob/5c60785b802ad2c8b8899608d949342d5c624952/lib/ui/painting/vertices.cc#L21
    vertices = Vertices.raw(VertexMode.triangles,
        nativeCmd.ref.positions.asTypedList(numVertices * 2),
        textureCoordinates: uvs,
        colors: nativeCmd.ref.colors.asTypedList(numVertices),
        indices: nativeCmd.ref.indices.asTypedList(numIndices)
    );
  }
}

const String _libName = 'spine_flutter';

final DynamicLibrary _dylib = () {
  if (Platform.isMacOS || Platform.isIOS) {
    return DynamicLibrary.open('$_libName.framework/$_libName');
  }
  if (Platform.isAndroid || Platform.isLinux) {
    return DynamicLibrary.open('lib$_libName.so');
  }
  if (Platform.isWindows) {
    return DynamicLibrary.open('$_libName.dll');
  }
  throw UnsupportedError('Unknown platform: ${Platform.operatingSystem}');
}();

final SpineFlutterBindings _bindings = SpineFlutterBindings(_dylib);
