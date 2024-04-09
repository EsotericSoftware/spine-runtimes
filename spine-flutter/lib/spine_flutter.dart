///
/// Spine Runtimes License Agreement
/// Last updated July 28, 2023. Replaces all prior versions.
///
/// Copyright (c) 2013-2023, Esoteric Software LLC
///
/// Integration of the Spine Runtimes into software or otherwise creating
/// derivative works of the Spine Runtimes is permitted under the terms and
/// conditions of Section 2 of the Spine Editor License Agreement:
/// http://esotericsoftware.com/spine-editor-license
///
/// Otherwise, it is permitted to integrate the Spine Runtimes into software or
/// otherwise create derivative works of the Spine Runtimes (collectively,
/// "Products"), provided that each user of the Products must obtain their own
/// Spine Editor license and redistribution of the Products in any form must
/// include this license and copyright notice.
///
/// THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
/// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
/// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
/// DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
/// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
/// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
/// BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
/// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
/// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
/// SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
///

import 'dart:convert' as convert;
import 'dart:io';
import 'dart:typed_data';
import 'dart:ui';

import 'package:flutter/foundation.dart' show kIsWeb;
import 'package:flutter/material.dart' as material;
import 'package:flutter/rendering.dart' as rendering;
import 'package:flutter/services.dart';
import 'package:http/http.dart' as http;
import 'package:path/path.dart' as path;


import 'ffi_proxy.dart';
import 'init.dart' if (dart.library.html) 'init_web.dart';
import 'spine_flutter_bindings_generated.dart';
import 'raw_image_provider.dart';

export 'spine_widget.dart';

late SpineFlutterBindings _bindings;
late Allocator _allocator;

Future<void> initSpineFlutter({bool enableMemoryDebugging = false}) async {
  final ffi = await initSpineFlutterFFI();
  _bindings = SpineFlutterBindings(ffi.dylib);
  _allocator = ffi.allocator;
  if (enableMemoryDebugging) _bindings.spine_enable_debug_extension(-1);
  return;
}

int majorVersion() => _bindings.spine_major_version();

int minorVersion() => _bindings.spine_minor_version();

void reportLeaks() => _bindings.spine_report_leaks();

/// A color made of red, green, blue, and alpha components,
/// ranging from 0-1.
class Color {
  double r;
  double g;
  double b;
  double a;

  Color(this.r, this.g, this.b, this.a);
}

/// Bounds denoted by the top left corner coordinates [x] and [y]
/// and the [width] and [height].
class Bounds {
  double x;
  double y;
  double width;
  double height;

  Bounds(this.x, this.y, this.width, this.height);
}

/// A two-dimensional vector with [x] and [y] components.
class Vec2 {
  double x;
  double y;

  Vec2(this.x, this.y);
}

/// Atlas data loaded from a `.atlas` file and its corresponding `.png` files. For each atlas image,
/// a corresponding [Image] and [Paint] is constructed, which are used when rendering a skeleton
/// that uses this atlas.
///
/// Use the static methods [fromAsset], [fromFile], and [fromHttp] to load an atlas. Call [dispose]
/// when the atlas is no longer in use to release its resources.
class Atlas {
  static FilterQuality filterQuality = FilterQuality.none;
  final spine_atlas _atlas;
  final List<Image> atlasPages;
  final List<Map<BlendMode, Paint>> atlasPagePaints;
  bool _disposed;

  Atlas._(this._atlas, this.atlasPages, this.atlasPagePaints) : _disposed = false;

  static Future<Atlas> _load(String atlasFileName, Future<Uint8List> Function(String name) loadFile) async {
    final atlasBytes = await loadFile(atlasFileName);
    final atlasData = convert.utf8.decode(atlasBytes);
    final atlasDataNative = atlasData.toNativeUtf8(allocator: _allocator);
    final atlas = _bindings.spine_atlas_load(atlasDataNative.cast());
    _allocator.free(atlasDataNative);
    if (_bindings.spine_atlas_get_error(atlas).address != nullptr.address) {
      final Pointer<Utf8> error = _bindings.spine_atlas_get_error(atlas).cast();
      final message = error.toDartString();
      _bindings.spine_atlas_dispose(atlas);
      throw Exception("Couldn't load atlas: $message");
    }

    final atlasDir = path.dirname(atlasFileName);
    List<Image> atlasPages = [];
    List<Map<BlendMode, Paint>> atlasPagePaints = [];
    final numImagePaths = _bindings.spine_atlas_get_num_image_paths(atlas);
    for (int i = 0; i < numImagePaths; i++) {
      final Pointer<Utf8> atlasPageFile = _bindings.spine_atlas_get_image_path(atlas, i).cast();
      final imagePath = atlasDir + "/" + atlasPageFile.toDartString();
      var imageData = await loadFile(imagePath);
      final Codec codec = await instantiateImageCodec(imageData);
      final FrameInfo frameInfo = await codec.getNextFrame();
      final Image image = frameInfo.image;
      atlasPages.add(image);
      Map<BlendMode, Paint> paints = {};
      for (final blendMode in BlendMode.values) {
        paints[blendMode] = Paint()
          ..shader = ImageShader(image, TileMode.clamp, TileMode.clamp, Matrix4
              .identity()
              .storage, filterQuality: Atlas.filterQuality)
          ..isAntiAlias = true
          ..blendMode = blendMode.canvasBlendMode;
      }
      atlasPagePaints.add(paints);
    }

    return Atlas._(atlas, atlasPages, atlasPagePaints);
  }

  /// Loads an [Atlas] from the file [atlasFileName] in the root bundle or the optionally provided [bundle].
  ///
  /// Throws an [Exception] in case the atlas could not be loaded.
  static Future<Atlas> fromAsset(String atlasFileName, {AssetBundle? bundle}) async {
    bundle ??= rootBundle;
    return _load(atlasFileName, (file) async => (await bundle!.load(file)).buffer.asUint8List());
  }

  /// Loads an [Atlas] from the file [atlasFileName].
  ///
  /// Throws an [Exception] in case the atlas could not be loaded.
  static Future<Atlas> fromFile(String atlasFileName) async {
    return _load(atlasFileName, (file) => File(file).readAsBytes());
  }

  /// Loads an [Atlas] from the URL [atlasURL].
  ///
  /// Throws an [Exception] in case the atlas could not be loaded.
  static Future<Atlas> fromHttp(String atlasURL) async {
    return _load(atlasURL, (file) async {
      return (await http.get(Uri.parse(file))).bodyBytes;
    });
  }

  /// Disposes the (native) resources of this atlas. The atlas can no longer be
  /// used after calling this function. Only the first call to this method will
  /// have an effect. Subsequent calls are ignored.
  void dispose() {
    if (_disposed) return;
    _disposed = true;
    _bindings.spine_atlas_dispose(_atlas);
    for (final image in atlasPages) {
      image.dispose();
    }
    atlasPagePaints.clear();
  }
}

/// Skeleton data loaded from a skeleton `.json` or `.skel` file. Contains bones, slots, constraints,
/// skins, animations, and so on making up a skeleton. Also contains meta data such as the skeletons
/// setup pose bounding box, the Spine editor version it was exported from, and so on.
///
/// Skeleton data is stateless. Stateful [Skeleton] instances can be constructed from a [SkeletonData] instance.
/// A single [SkeletonData] instance can be shared by multiple [Skeleton] instances.
///
/// Use the static methods [fromJson], [fromBinary], [fromAsset], [fromFile], and [fromURL] to load
/// skeleton data. Call [dispose] when the skeleton data is no longer in use to free its resources.
///
/// See [Data objects](http://esotericsoftware.com/spine-runtime-architecture#Data-objects) in the Spine
/// Runtimes Guide.
class SkeletonData {
  final spine_skeleton_data _data;
  bool _disposed;

  SkeletonData._(this._data) : _disposed = false;

  /// Loads a [SkeletonData] from the [json] string, using the provided [atlas] to resolve attachment
  /// images.
  ///
  /// Throws an [Exception] in case the atlas could not be loaded.
  static SkeletonData fromJson(Atlas atlas, String json) {
    final jsonNative = json.toNativeUtf8(allocator: _allocator);
    final result = _bindings.spine_skeleton_data_load_json(atlas._atlas, jsonNative.cast());
    _allocator.free(jsonNative);
    if (_bindings.spine_skeleton_data_result_get_error(result).address != nullptr.address) {
      final Pointer<Utf8> error = _bindings.spine_skeleton_data_result_get_error(result).cast();
      final message = error.toDartString();
      _bindings.spine_skeleton_data_result_dispose(result);
      throw Exception("Couldn't load skeleton data: $message");
    }
    var data = SkeletonData._(_bindings.spine_skeleton_data_result_get_data(result));
    _bindings.spine_skeleton_data_result_dispose(result);
    return data;
  }

  /// Loads a [SkeletonData] from the [binary] skeleton data, using the provided [atlas] to resolve attachment
  /// images.
  ///
  /// Throws an [Exception] in case the skeleton data could not be loaded.
  static SkeletonData fromBinary(Atlas atlas, Uint8List binary) {
    final Pointer<Uint8> binaryNative = _allocator.allocate(binary.lengthInBytes);
    binaryNative.asTypedList(binary.lengthInBytes).setAll(0, binary);
    final result = _bindings.spine_skeleton_data_load_binary(atlas._atlas, binaryNative.cast(), binary.lengthInBytes);
    _allocator.free(binaryNative);
    if (_bindings.spine_skeleton_data_result_get_error(result).address != nullptr.address) {
      final Pointer<Utf8> error = _bindings.spine_skeleton_data_result_get_error(result).cast();
      final message = error.toDartString();
      _bindings.spine_skeleton_data_result_dispose(result);
      throw Exception("Couldn't load skeleton data: $message");
    }
    var data = SkeletonData._(_bindings.spine_skeleton_data_result_get_data(result));
    _bindings.spine_skeleton_data_result_dispose(result);
    return data;
  }

  /// Loads a [SkeletonData] from the file [skeletonFile] in the root bundle or the optionally provided [bundle].
  /// Uses the provided [atlas] to resolve attachment images.
  ///
  /// Throws an [Exception] in case the skeleton data could not be loaded.
  static Future<SkeletonData> fromAsset(Atlas atlas, String skeletonFile, {AssetBundle? bundle}) async {
    bundle ??= rootBundle;
    if (skeletonFile.endsWith(".json")) {
      return fromJson(atlas, await bundle.loadString(skeletonFile));
    } else {
      return fromBinary(atlas, (await bundle.load(skeletonFile)).buffer.asUint8List());
    }
  }

  /// Loads a [SkeletonData] from the file [skeletonFile]. Uses the provided [atlas] to resolve attachment images.
  ///
  /// Throws an [Exception] in case the skeleton data could not be loaded.
  static Future<SkeletonData> fromFile(Atlas atlas, String skeletonFile) async {
    if (skeletonFile.endsWith(".json")) {
      return fromJson(atlas, convert.utf8.decode(await File(skeletonFile).readAsBytes()));
    } else {
      return fromBinary(atlas, await File(skeletonFile).readAsBytes());
    }
  }

  /// Loads a [SkeletonData] from the URL [skeletonURL]. Uses the provided [atlas] to resolve attachment images.
  ///
  /// Throws an [Exception] in case the skeleton data could not be loaded.
  static Future<SkeletonData> fromHttp(Atlas atlas, String skeletonURL) async {
    if (skeletonURL.endsWith(".json")) {
      return fromJson(atlas, convert.utf8.decode((await http.get(Uri.parse(skeletonURL))).bodyBytes));
    } else {
      return fromBinary(atlas, (await http.get(Uri.parse(skeletonURL))).bodyBytes);
    }
  }

  /// The skeleton's bones, sorted parent first. The root bone is always the first bone.
  List<BoneData> getBones() {
    final List<BoneData> bones = [];
    final numBones = _bindings.spine_skeleton_data_get_num_bones(_data);
    final nativeBones = _bindings.spine_skeleton_data_get_bones(_data);
    for (int i = 0; i < numBones; i++) {
      bones.add(BoneData._(nativeBones[i]));
    }
    return bones;
  }

  /// Finds a bone by comparing each bone's name. It is more efficient to cache the results of this method than to call it multiple times.
  BoneData? findBone(String name) {
    final nativeName = name.toNativeUtf8(allocator: _allocator);
    final bone = _bindings.spine_skeleton_data_find_bone(_data, nativeName.cast());
    _allocator.free(nativeName);
    if (bone.address == nullptr.address) return null;
    return BoneData._(bone);
  }

  /// The skeleton's slots.
  List<SlotData> getSlots() {
    final List<SlotData> slots = [];
    final numSlots = _bindings.spine_skeleton_data_get_num_slots(_data);
    final nativeSlots = _bindings.spine_skeleton_data_get_slots(_data);
    for (int i = 0; i < numSlots; i++) {
      slots.add(SlotData._(nativeSlots[i]));
    }
    return slots;
  }

  /// Finds a slot by comparing each slot's name. It is more efficient to cache the results of this method than to call it multiple times.
  SlotData? findSlot(String name) {
    final nativeName = name.toNativeUtf8(allocator: _allocator);
    final slot = _bindings.spine_skeleton_data_find_slot(_data, nativeName.cast());
    _allocator.free(nativeName);
    if (slot.address == nullptr.address) return null;
    return SlotData._(slot);
  }

  /// All skins, including the default skin.
  List<Skin> getSkins() {
    final List<Skin> skins = [];
    final numSkins = _bindings.spine_skeleton_data_get_num_skins(_data);
    final nativeSkins = _bindings.spine_skeleton_data_get_skins(_data);
    for (int i = 0; i < numSkins; i++) {
      skins.add(Skin._(nativeSkins[i]));
    }
    return skins;
  }

  /// The skeleton's default skin. By default this skin contains all attachments that were not in a skin in Spine.
  Skin? getDefaultSkin() {
    final skin = _bindings.spine_skeleton_data_get_default_skin(_data);
    if (skin.address == nullptr.address) return null;
    return Skin._(skin);
  }

  void setDefaultSkin(Skin? skin) {
    if (skin == null) {
      _bindings.spine_skeleton_data_set_default_skin(_data, nullptr);
    } else {
      _bindings.spine_skeleton_data_set_default_skin(_data, skin._skin);
    }
  }

  /// Finds a skin by comparing each skin's name. It is more efficient to cache the results of this method than to call it
  /// multiple times.
  Skin? findSkin(String name) {
    final nativeName = name.toNativeUtf8(allocator: _allocator);
    final skin = _bindings.spine_skeleton_data_find_skin(_data, nativeName.cast());
    _allocator.free(nativeName);
    if (skin.address == nullptr.address) return null;
    return Skin._(skin);
  }

  /// The skeleton's events.
  List<EventData> getEvents() {
    final List<EventData> events = [];
    final numEvents = _bindings.spine_skeleton_data_get_num_events(_data);
    final nativeEvents = _bindings.spine_skeleton_data_get_events(_data);
    for (int i = 0; i < numEvents; i++) {
      events.add(EventData._(nativeEvents[i]));
    }
    return events;
  }

  /// Finds an event by comparing each events's name. It is more efficient to cache the results of this method than to call it
  /// multiple times.
  EventData? findEvent(String name) {
    final nativeName = name.toNativeUtf8(allocator: _allocator);
    final event = _bindings.spine_skeleton_data_find_event(_data, nativeName.cast());
    _allocator.free(nativeName);
    if (event.address == nullptr.address) return null;
    return EventData._(event);
  }

  /// The skeleton's animations.
  List<Animation> getAnimations() {
    final List<Animation> events = [];
    final numAnimation = _bindings.spine_skeleton_data_get_num_animations(_data);
    final nativeAnimations = _bindings.spine_skeleton_data_get_animations(_data);
    for (int i = 0; i < numAnimation; i++) {
      events.add(Animation._(nativeAnimations[i]));
    }
    return events;
  }

  /// Finds an animation by comparing each animation's name. It is more efficient to cache the results of this method than to
  /// call it multiple times.
  Animation? findAnimation(String name) {
    final nativeName = name.toNativeUtf8(allocator: _allocator);
    final animation = _bindings.spine_skeleton_data_find_animation(_data, nativeName.cast());
    _allocator.free(nativeName);
    if (animation.address == nullptr.address) return null;
    return Animation._(animation);
  }

  /// The skeleton's IK constraints.
  List<IkConstraintData> getIkConstraints() {
    final List<IkConstraintData> constraints = [];
    final numConstraints = _bindings.spine_skeleton_data_get_num_ik_constraints(_data);
    final nativeConstraints = _bindings.spine_skeleton_data_get_ik_constraints(_data);
    for (int i = 0; i < numConstraints; i++) {
      constraints.add(IkConstraintData._(nativeConstraints[i]));
    }
    return constraints;
  }

  /// Finds an IK constraint by comparing each IK constraint's name. It is more efficient to cache the results of this method
  /// than to call it multiple times.
  IkConstraintData? findIkConstraint(String name) {
    final nativeName = name.toNativeUtf8(allocator: _allocator);
    final constraint = _bindings.spine_skeleton_data_find_ik_constraint(_data, nativeName.cast());
    _allocator.free(nativeName);
    if (constraint.address == nullptr.address) return null;
    return IkConstraintData._(constraint);
  }

  /// The skeleton's transform constraints.
  List<TransformConstraint> getTransformConstraints() {
    final List<TransformConstraint> constraints = [];
    final numConstraints = _bindings.spine_skeleton_data_get_num_transform_constraints(_data);
    final nativeConstraints = _bindings.spine_skeleton_data_get_transform_constraints(_data);
    for (int i = 0; i < numConstraints; i++) {
      constraints.add(TransformConstraint._(nativeConstraints[i].cast()));
    }
    return constraints;
  }

  /// Finds a transform constraint by comparing each transform constraint's name. It is more efficient to cache the results of
  /// this method than to call it multiple times.
  TransformConstraintData? findTransformConstraint(String name) {
    final nativeName = name.toNativeUtf8(allocator: _allocator);
    final constraint = _bindings.spine_skeleton_data_find_transform_constraint(_data, nativeName.cast());
    _allocator.free(nativeName);
    if (constraint.address == nullptr.address) return null;
    return TransformConstraintData._(constraint);
  }

  /// The skeleton's path constraints.
  List<PathConstraintData> getPathConstraints() {
    final List<PathConstraintData> constraints = [];
    final numConstraints = _bindings.spine_skeleton_data_get_num_path_constraints(_data);
    final nativeConstraints = _bindings.spine_skeleton_data_get_path_constraints(_data);
    for (int i = 0; i < numConstraints; i++) {
      constraints.add(PathConstraintData._(nativeConstraints[i]));
    }
    return constraints;
  }

  /// Finds a path constraint by comparing each path constraint's name. It is more efficient to cache the results of this method
  /// than to call it multiple times.
  PathConstraintData? findPathConstraint(String name) {
    final nativeName = name.toNativeUtf8(allocator: _allocator);
    final constraint = _bindings.spine_skeleton_data_find_path_constraint(_data, nativeName.cast());
    _allocator.free(nativeName);
    if (constraint.address == nullptr.address) return null;
    return PathConstraintData._(constraint);
  }

  /// The skeleton's name, which by default is the name of the skeleton data file when possible, or null when a name hasn't been
  /// set.
  String? getName() {
    Pointer<Utf8> name = _bindings.spine_skeleton_data_get_name(_data).cast();
    if (name.address == nullptr.address) return null;
    return name.toDartString();
  }

  /// The X coordinate of the skeleton's axis aligned bounding box in the setup pose.
  double getX() {
    return _bindings.spine_skeleton_data_get_x(_data);
  }

  void setX(double x) {
    _bindings.spine_skeleton_data_set_x(_data, x);
  }

  /// The Y coordinate of the skeleton's axis aligned bounding box in the setup pose.
  double getY() {
    return _bindings.spine_skeleton_data_get_y(_data);
  }

  void setY(double y) {
    _bindings.spine_skeleton_data_set_x(_data, y);
  }

  /// The width of the skeleton's axis aligned bounding box in the setup pose.
  double getWidth() {
    return _bindings.spine_skeleton_data_get_width(_data);
  }

  void setWidth(double width) {
    _bindings.spine_skeleton_data_set_width(_data, width);
  }

  /// The height of the skeleton's axis aligned bounding box in the setup pose.
  double getHeight() {
    return _bindings.spine_skeleton_data_get_height(_data);
  }

  void setHeight(double height) {
    _bindings.spine_skeleton_data_set_height(_data, height);
  }

  /// The Spine version used to export the skeleton data.
  String? getVersion() {
    Pointer<Utf8> name = _bindings.spine_skeleton_data_get_version(_data).cast();
    if (name.address == nullptr.address) return null;
    return name.toDartString();
  }

  /// The skeleton data hash. This value will change if any of the skeleton data has changed.
  String? getHash() {
    Pointer<Utf8> name = _bindings.spine_skeleton_data_get_hash(_data).cast();
    if (name.address == nullptr.address) return null;
    return name.toDartString();
  }

  /// The path to the images directory as defined in Spine, or null if nonessential data was not exported.
  String? getImagesPath() {
    Pointer<Utf8> name = _bindings.spine_skeleton_data_get_images_path(_data).cast();
    if (name.address == nullptr.address) return null;
    return name.toDartString();
  }

  /// The path to the audio directory as defined in Spine, or null if nonessential data was not exported.
  String? getAudioPath() {
    Pointer<Utf8> name = _bindings.spine_skeleton_data_get_audio_path(_data).cast();
    if (name.address == nullptr.address) return null;
    return name.toDartString();
  }

  /// The dopesheet FPS in Spine, or zero if nonessential data was not exported.
  double getFps() {
    return _bindings.spine_skeleton_data_get_fps(_data);
  }

  /// Disposes the (native) resources of this skeleton data. The skeleton data can no longer be
  /// used after calling this function. Only the first call to this method will
  /// have an effect. Subsequent calls are ignored.
  void dispose() {
    if (_disposed) return;
    _disposed = true;
    _bindings.spine_skeleton_data_dispose(_data);
  }
}

/// Determines how images are blended with existing pixels when drawn. See [Blending](http://esotericsoftware.com/spine-slots#Blending) in
/// the Spine User Guide.
enum BlendMode {
  normal(0, rendering.BlendMode.srcOver),
  additive(1, rendering.BlendMode.plus),
  multiply(2, rendering.BlendMode.multiply),
  screen(3, rendering.BlendMode.screen);

  final int value;
  final rendering.BlendMode canvasBlendMode;

  const BlendMode(this.value, this.canvasBlendMode);
}

/// Determines how a bone inherits world transforms from parent bones. See [Transform inheritance](esotericsoftware.com/spine-bones#Transform-inheritance)
/// in the Spine User Guide.
enum Inherit {
  normal(0),
  onlyTranslation(1),
  noRotationOrReflection(2),
  noScale(3),
  noScaleOrReflection(4);

  final int value;

  const Inherit(this.value);
}

/// Determines how physics and other non-deterministic updates are applied.
enum Physics {
  none(0),
  reset(1),
  update(2),
  pose(3);

  final int value;

  const Physics(this.value);
}

/// Controls how the first bone is positioned along the path.
///
/// See [Position mode](http://esotericsoftware.com/spine-path-constraints#Position-mode) in the Spine User Guide.
enum PositionMode {
  fixed(0),
  percent(1);

  final int value;

  const PositionMode(this.value);
}

/// Controls how bones after the first bone are positioned along the path.
///
/// See [Spacing mode](http://esotericsoftware.com/spine-path-constraints#Spacing-mode) in the Spine User Guide.
enum SpacingMode {
  length(0),
  fixed(1),
  percent(2),
  proportional(3);

  final int value;

  const SpacingMode(this.value);
}

/// Controls how bones are rotated, translated, and scaled to match the path.
///
/// See [Rotate mode](http://esotericsoftware.com/spine-path-constraints#Rotate-mode) in the Spine User Guide.
enum RotateMode {
  tangent(0),
  chain(1),
  chainScale(2);

  final int value;

  const RotateMode(this.value);
}

/// Stores the setup pose for a [Bone].
class BoneData {
  final spine_bone_data _data;

  BoneData._(this._data);

  /// The index of the bone in [Skeleton.getBones].
  int getIndex() {
    return _bindings.spine_bone_data_get_index(_data);
  }

  /// The name of the bone, which is unique across all bones in the skeleton.
  String getName() {
    Pointer<Utf8> name = _bindings.spine_bone_data_get_name(_data).cast();
    return name.toDartString();
  }

  /// The parent bone or `null` if this is the root bone.
  BoneData? getParent() {
    final parent = _bindings.spine_bone_data_get_parent(_data);
    if (parent.address == nullptr.address) return null;
    return BoneData._(parent);
  }

  /// The bone's length.
  double getLength() {
    return _bindings.spine_bone_data_get_length(_data);
  }

  void setLength(double length) {
    _bindings.spine_bone_data_set_length(_data, length);
  }

  /// The local x translation.
  double getX() {
    return _bindings.spine_bone_data_get_x(_data);
  }

  void setX(double x) {
    _bindings.spine_bone_data_set_x(_data, x);
  }

  /// The local y translation.
  double getY() {
    return _bindings.spine_bone_data_get_y(_data);
  }

  void setY(double y) {
    _bindings.spine_bone_data_set_y(_data, y);
  }

  /// The local rotation in degrees.
  double getRotation() {
    return _bindings.spine_bone_data_get_rotation(_data);
  }

  void setRotation(double rotation) {
    _bindings.spine_bone_data_set_rotation(_data, rotation);
  }

  /// The local scaleX.
  double getScaleX() {
    return _bindings.spine_bone_data_get_scale_x(_data);
  }

  void setScaleX(double scaleX) {
    _bindings.spine_bone_data_set_scale_x(_data, scaleX);
  }

  /// The local scaleY.
  double getScaleY() {
    return _bindings.spine_bone_data_get_scale_y(_data);
  }

  void setScaleY(double scaleY) {
    _bindings.spine_bone_data_set_scale_y(_data, scaleY);
  }

  /// The local shearX.
  double getShearX() {
    return _bindings.spine_bone_data_get_shear_x(_data);
  }

  void setShearX(double shearX) {
    _bindings.spine_bone_data_set_shear_x(_data, shearX);
  }

  /// The local shearY.
  double getShearY() {
    return _bindings.spine_bone_data_get_shear_y(_data);
  }

  void setShearY(double shearY) {
    _bindings.spine_bone_data_set_shear_y(_data, shearY);
  }

  /// The [Inherit] for how parent world transforms affect this bone.
  Inherit getInherit() {
    final nativeMode = _bindings.spine_bone_data_get_inherit(_data);
    return Inherit.values[nativeMode];
  }

  void setInherit(Inherit inherit) {
    _bindings.spine_bone_data_set_inherit(_data, inherit.value);
  }

  /// When true, [Skeleton.updateWorldTransform] only updates this bone if the [Skeleton.getSkin] contains this bone.
  ///
  /// See [Skin.getBones].
  bool isSkinRequired() {
    return _bindings.spine_bone_data_is_skin_required(_data) == -1;
  }

  void setIsSkinRequired(bool isSkinRequired) {
    _bindings.spine_bone_data_set_is_skin_required(_data, isSkinRequired ? -1 : 0);
  }

  /// The [Color] of the bone as it was in Spine, or a default color if nonessential data was not exported. Bones are not usually
  /// rendered at runtime.
  Color getColor() {
    final color = _bindings.spine_bone_data_get_color(_data);
    return Color(_bindings.spine_color_get_r(color), _bindings.spine_color_get_g(color), _bindings.spine_color_get_b(color),
        _bindings.spine_color_get_a(color));
  }

  void setColor(double r, double g, double b, double a) {
    _bindings.spine_bone_data_set_color(_data, r, g, b, a);
  }

  @override
  String toString() {
    return getName();
  }
}

/// Stores a bone's current pose.
///
///  A bone has a local transform which is used to compute its world transform. A bone also has an applied transform, which is a
///  local transform that can be applied to compute the world transform. The local transform and applied transform may differ if a
///  constraint or application code modifies the world transform after it was computed from the local transform.
class Bone {
  final spine_bone _bone;

  Bone._(this._bone);

  /// Assume y-axis pointing down for all calculations.
  static void setIsYDown(bool isYDown) {
    _bindings.spine_bone_set_is_y_down(isYDown ? -1 : 0);
  }

  static bool getIsYDown() {
    return _bindings.spine_bone_get_is_y_down() == 1;
  }

  /// Computes the world transform using the parent bone and this bone's local applied transform.
  void update() {
    _bindings.spine_bone_update(_bone);
  }

  /// Computes the world transform using the parent bone and this bone's local transform.
  ///
  /// See [updateWorldTransformWith].
  void updateWorldTransform() {
    _bindings.spine_bone_update_world_transform(_bone);
  }

  /// Computes the world transform using the parent bone and the specified local transform. The applied transform is set to the
  /// specified local transform. Child bones are not updated.
  ///
  /// See [World transform](http://esotericsoftware.com/spine-runtime-skeletons#World-transforms) in the Spine
  /// Runtimes Guide.
  void updateWorldTransformWith(double x, double y, double rotation, double scaleX, double scaleY, double shearX, double shearY) {
    _bindings.spine_bone_update_world_transform_with(_bone, x, y, rotation, scaleX, scaleY, shearX, shearY);
  }

  /// Computes the applied transform values from the world transform.
  ///
  /// If the world transform is modified (by a constraint, [rotateWorld], etc) then this method should be called so
  /// the applied transform matches the world transform. The applied transform may be needed by other code (eg to apply another
  /// constraint).
  ///
  /// Some information is ambiguous in the world transform, such as -1,-1 scale versus 180 rotation. The applied transform after
  /// calling this method is equivalent to the local transform used to compute the world transform, but may not be identical.
  void updateAppliedTransform() {
    _bindings.spine_bone_update_applied_transform(_bone);
  }

  /// Sets this bone's local transform to the setup pose.
  void setToSetupPose() {
    _bindings.spine_bone_set_to_setup_pose(_bone);
  }

  /// Transforms a point from world coordinates to the bone's local coordinates.
  Vec2 worldToLocal(double worldX, double worldY) {
    final local = _bindings.spine_bone_world_to_local(_bone, worldX, worldY);
    final result = Vec2(_bindings.spine_vector_get_x(local), _bindings.spine_vector_get_y(local));
    return result;
  }

  /// Transforms a point from the bone's local coordinates to world coordinates.
  Vec2 localToWorld(double localX, double localY) {
    final world = _bindings.spine_bone_local_to_world(_bone, localX, localY);
    final result = Vec2(_bindings.spine_vector_get_x(world), _bindings.spine_vector_get_y(world));
    return result;
  }

  /// Transforms a world rotation to a local rotation.
  double worldToLocalRotation(double worldRotation) {
    return _bindings.spine_bone_world_to_local_rotation(_bone, worldRotation);
  }

  /// Transforms a local rotation to a world rotation.
  double localToWorldRotation(double localRotation) {
    return _bindings.spine_bone_local_to_world_rotation(_bone, localRotation);
  }

  /// Rotates the world transform the specified amount.
  ///
  /// After changes are made to the world transform, [updateAppliedTransform] should be called and [update] will
  /// need to be called on any child bones, recursively.
  void rotateWorld(double degrees) {
    _bindings.spine_bone_rotate_world(_bone, degrees);
  }

  double getWorldToLocalRotationX() {
    return _bindings.spine_bone_get_world_rotation_x(_bone);
  }

  double getWorldToLocalRotationY() {
    return _bindings.spine_bone_get_world_to_local_rotation_y(_bone);
  }

  /// The bone's setup pose data.
  BoneData getData() {
    return BoneData._(_bindings.spine_bone_get_data(_bone));
  }

  /// The skeleton this bone belongs to.
  Skeleton getSkeleton() {
    return Skeleton._(_bindings.spine_bone_get_skeleton(_bone));
  }

  /// The parent bone, or null if this is the root bone.
  Bone? getParent() {
    final parent = _bindings.spine_bone_get_parent(_bone);
    if (parent.address == nullptr.address) return null;
    return Bone._(parent);
  }

  /// The immediate children of this bone.
  List<Bone> getChildren() {
    List<Bone> children = [];
    final numChildren = _bindings.spine_bone_get_num_children(_bone);
    final nativeChildren = _bindings.spine_bone_get_children(_bone);
    for (int i = 0; i < numChildren; i++) {
      children.add(Bone._(nativeChildren[i]));
    }
    return children;
  }

  /// The local x translation.
  double getX() {
    return _bindings.spine_bone_get_x(_bone);
  }

  void setX(double x) {
    _bindings.spine_bone_set_x(_bone, x);
  }

  /// The local y translation.
  double getY() {
    return _bindings.spine_bone_get_y(_bone);
  }

  void setY(double y) {
    _bindings.spine_bone_set_y(_bone, y);
  }

  /// The local rotation in degrees, counter clockwise.
  double getRotation() {
    return _bindings.spine_bone_get_rotation(_bone);
  }

  void setRotation(double rotation) {
    _bindings.spine_bone_set_rotation(_bone, rotation);
  }

  /// The local scaleX.
  double getScaleX() {
    return _bindings.spine_bone_get_scale_x(_bone);
  }

  void setScaleX(double scaleX) {
    _bindings.spine_bone_set_scale_x(_bone, scaleX);
  }

  /// The local scaleY.
  double getScaleY() {
    return _bindings.spine_bone_get_scale_y(_bone);
  }

  void setScaleY(double scaleY) {
    _bindings.spine_bone_set_scale_y(_bone, scaleY);
  }

  /// The local shearX.
  double getShearX() {
    return _bindings.spine_bone_get_shear_x(_bone);
  }

  void setShearX(double shearX) {
    _bindings.spine_bone_set_shear_x(_bone, shearX);
  }

  /// The local shearY.
  double getShearY() {
    return _bindings.spine_bone_get_shear_y(_bone);
  }

  void setShearY(double shearY) {
    _bindings.spine_bone_set_shear_y(_bone, shearY);
  }

  /// The applied local x translation.
  double getAX() {
    return _bindings.spine_bone_get_a_x(_bone);
  }

  void setAX(double x) {
    _bindings.spine_bone_set_a_x(_bone, x);
  }

  /// The applied local y translation.
  double getAY() {
    return _bindings.spine_bone_get_a_y(_bone);
  }

  void setAY(double y) {
    _bindings.spine_bone_set_a_y(_bone, y);
  }

  /// The applied local rotation in degrees, counter clockwise.
  double getAppliedRotation() {
    return _bindings.spine_bone_get_applied_rotation(_bone);
  }

  void setAppliedRotation(double rotation) {
    _bindings.spine_bone_set_applied_rotation(_bone, rotation);
  }

  /// The applied local scaleX.
  double getAScaleX() {
    return _bindings.spine_bone_get_a_scale_x(_bone);
  }

  void setAScaleX(double scaleX) {
    _bindings.spine_bone_set_a_scale_x(_bone, scaleX);
  }

  /// The applied local scaleY.
  double getAScaleY() {
    return _bindings.spine_bone_get_a_scale_y(_bone);
  }

  void setAScaleY(double scaleY) {
    _bindings.spine_bone_set_a_scale_y(_bone, scaleY);
  }

  /// The applied local shearX.
  double getAShearX() {
    return _bindings.spine_bone_get_a_shear_x(_bone);
  }

  void setAShearX(double shearX) {
    _bindings.spine_bone_set_a_shear_x(_bone, shearX);
  }

  /// The applied local shearY.
  double getAShearY() {
    return _bindings.spine_bone_get_a_shear_y(_bone);
  }

  void setAShearY(double shearY) {
    _bindings.spine_bone_set_a_shear_y(_bone, shearY);
  }

  /// Part of the world transform matrix for the X axis. If changed, [updateAppliedTransform] should be called.
  double getA() {
    return _bindings.spine_bone_get_a(_bone);
  }

  void setA(double a) {
    _bindings.spine_bone_set_a(_bone, a);
  }

  /// Part of the world transform matrix for the Y axis. If changed, [updateAppliedTransform] should be called.
  double getB() {
    return _bindings.spine_bone_get_b(_bone);
  }

  void setB(double b) {
    _bindings.spine_bone_set_b(_bone, b);
  }

  /// Part of the world transform matrix for the X axis. If changed, [updateAppliedTransform] should be called.
  double getC() {
    return _bindings.spine_bone_get_c(_bone);
  }

  void setC(double c) {
    _bindings.spine_bone_set_c(_bone, c);
  }

  /// Part of the world transform matrix for the Y axis. If changed, [updateAppliedTransform] should be called.
  double getD() {
    return _bindings.spine_bone_get_d(_bone);
  }

  void setD(double d) {
    _bindings.spine_bone_set_a(_bone, d);
  }

  /// The world X position. If changed, [updateAppliedTransform] should be called.
  double getWorldX() {
    return _bindings.spine_bone_get_world_x(_bone);
  }

  void setWorldX(double worldX) {
    _bindings.spine_bone_set_world_x(_bone, worldX);
  }

  /// The world Y position. If changed, [updateAppliedTransform] should be called.
  double getWorldY() {
    return _bindings.spine_bone_get_world_y(_bone);
  }

  void setWorldY(double worldY) {
    _bindings.spine_bone_set_world_y(_bone, worldY);
  }

  /// The world rotation for the X axis, calculated using [getA] and [getC].
  double getWorldRotationX() {
    return _bindings.spine_bone_get_world_rotation_x(_bone);
  }

  /// The world rotation for the Y axis, calculated using [getB] and [getD].
  double getWorldRotationY() {
    return _bindings.spine_bone_get_world_rotation_y(_bone);
  }

  /// The magnitude (always positive) of the world scale X, calculated using [getA] and [getC].
  double getWorldScaleX() {
    return _bindings.spine_bone_get_world_scale_x(_bone);
  }

  /// The magnitude (always positive) of the world scale Y, calculated using [getB] and [getD].
  double getWorldScaleY() {
    return _bindings.spine_bone_get_world_scale_y(_bone);
  }

  /// Returns false when the bone has not been computed because [BoneData.getSkinRequired] is true and the
  /// active skin (see [Skeleton.getSkin]) does not contain this bone (see [Skin.getBones]).
  bool isActive() {
    return _bindings.spine_bone_get_is_active(_bone) == -1;
  }

  void setIsActive(bool isActive) {
    _bindings.spine_bone_set_is_active(_bone, isActive ? -1 : 0);
  }
}

/// Stores the setup pose for a [Slot].
class SlotData {
  final spine_slot_data _data;

  SlotData._(this._data);

  /// The index of the slot in [Skeleton.getSlots].
  int getIndex() {
    return _bindings.spine_slot_data_get_index(_data);
  }

  /// The name of the slot, which is unique across all slots in the skeleton.
  String getName() {
    final Pointer<Utf8> value = _bindings.spine_slot_data_get_name(_data).cast();
    return value.toDartString();
  }

  /// The bone this slot belongs to.
  BoneData getBoneData() {
    return BoneData._(_bindings.spine_slot_data_get_bone_data(_data));
  }

  /// The [Color] used to tint the slot's attachment. If [hasDarkColor] is true, this is used as the light color for two
  /// color tinting.
  Color getColor() {
    final color = _bindings.spine_slot_data_get_color(_data);
    return Color(_bindings.spine_color_get_r(color), _bindings.spine_color_get_g(color), _bindings.spine_color_get_b(color),
        _bindings.spine_color_get_a(color));
  }

  void setColor(double r, double g, double b, double a) {
    _bindings.spine_slot_data_set_color(_data, r, g, b, a);
  }

  /// The dark color used to tint the slot's attachment for two color tinting, if [hasDarkColor] is true. The dark
  /// color's alpha is not used.
  Color getDarkColor() {
    final color = _bindings.spine_slot_data_get_dark_color(_data);
    return Color(_bindings.spine_color_get_r(color), _bindings.spine_color_get_g(color), _bindings.spine_color_get_b(color),
        _bindings.spine_color_get_a(color));
  }

  void setDarkColor(double r, double g, double b, double a) {
    _bindings.spine_slot_data_set_dark_color(_data, r, g, b, a);
  }

  /// Returns whether this slot has a dark color set for two color tinting.
  bool hasDarkColor() {
    return _bindings.spine_slot_data_has_dark_color(_data) == -1;
  }

  void setHasDarkColor(bool hasDarkColor) {
    _bindings.spine_slot_data_set_has_dark_color(_data, hasDarkColor ? -1 : 0);
  }

  /// The name of the attachment that is visible for this slot in the setup pose, or null if no attachment is visible.
  String getAttachmentName() {
    final Pointer<Utf8> value = _bindings.spine_slot_data_get_attachment_name(_data).cast();
    return value.toDartString();
  }

  void setAttachmentName(String attachmentName) {
    final nativeName = attachmentName.toNativeUtf8(allocator: _allocator);
    _bindings.spine_slot_data_set_attachment_name(_data, nativeName.cast());
    _allocator.free(nativeName);
  }

  /// The [BlendMode] for drawing the slot's attachment.
  BlendMode getBlendMode() {
    return BlendMode.values[_bindings.spine_slot_data_get_blend_mode(_data)];
  }

  void setBlendMode(BlendMode mode) {
    _bindings.spine_slot_data_set_blend_mode(_data, mode.value);
  }

  @override
  String toString() {
    return getName();
  }
}

/// Stores a slot's current pose. Slots organize attachments for [Skeleton.getDrawOrder] purposes and provide a place to store
/// state for an attachment. State cannot be stored in an attachment itself because attachments are stateless and may be shared
/// across multiple skeletons.
class Slot {
  final spine_slot _slot;

  Slot._(this._slot);

  /// Sets this slot to the setup pose.
  void setToSetupPose() {
    _bindings.spine_slot_set_to_setup_pose(_slot);
  }

  /// The slot's setup pose data.
  SlotData getData() {
    return SlotData._(_bindings.spine_slot_get_data(_slot));
  }

  /// The bone this slot belongs to.
  Bone getBone() {
    return Bone._(_bindings.spine_slot_get_bone(_slot));
  }

  /// The skeleton this slot belongs to.
  Skeleton getSkeleton() {
    return Skeleton._(_bindings.spine_slot_get_skeleton(_slot));
  }

  /// The color used to tint the slot's attachment. If [hasDarkColor] is true, this is used as the light color for two
  /// color tinting.
  Color getColor() {
    final color = _bindings.spine_slot_get_color(_slot);
    return Color(_bindings.spine_color_get_r(color), _bindings.spine_color_get_g(color), _bindings.spine_color_get_b(color),
        _bindings.spine_color_get_a(color));
  }

  void setColor(Color color) {
    _bindings.spine_slot_set_color(_slot, color.r, color.g, color.b, color.a);
  }

  /// The dark color used to tint the slot's attachment for two color tinting, if [hasDarkColor] is true. The dark
  /// color's alpha is not used.
  Color getDarkColor() {
    final color = _bindings.spine_slot_get_dark_color(_slot);
    return Color(_bindings.spine_color_get_r(color), _bindings.spine_color_get_g(color), _bindings.spine_color_get_b(color),
        _bindings.spine_color_get_a(color));
  }

  void setDarkColor(Color color) {
    _bindings.spine_slot_set_dark_color(_slot, color.r, color.g, color.b, color.a);
  }

  /// Returns whether this slot has a dark color set for two color tinting.
  bool hasDarkColor() {
    return _bindings.spine_slot_has_dark_color(_slot) == -1;
  }

  /// The current attachment for the slot, or null if the slot has no attachment.
  Attachment? getAttachment() {
    final attachment = _bindings.spine_slot_get_attachment(_slot);
    if (attachment.address == nullptr.address) return null;
    return Attachment._toSubclass(attachment);
  }

  void setAttachment(Attachment? attachment) {
    _bindings.spine_slot_set_attachment(_slot, attachment != null ? attachment._attachment.cast() : nullptr);
  }

  @override
  String toString() {
    return getData().getName();
  }

  /// The index of the texture region to display when the slot's attachment has a [Sequence]. -1 represents the
  /// [Sequence.getSetupIndex].
  int getSequenceIndex() {
    return _bindings.spine_slot_get_sequence_index(_slot);
  }

  void setSequenceIndex(int sequenceIndex) {
    _bindings.spine_slot_set_sequence_index(_slot, sequenceIndex);
  }
}

/// A region within a texture, given in normalized texture coordinates of the top left ([getU], [getV]) and
/// bottom left ([getU2], [getV2]) corner of the region within the texture.
class TextureRegion {
  final spine_texture_region _region;

  TextureRegion._(this._region);

  Pointer<Void> getTexture() {
    return _bindings.spine_texture_region_get_texture(_region);
  }

  void setTexture(Pointer<Void> texture) {
    _bindings.spine_texture_region_set_texture(_region, texture);
  }

  double getU() {
    return _bindings.spine_texture_region_get_u(_region);
  }

  void setU(double u) {
    _bindings.spine_texture_region_set_u(_region, u);
  }

  double getV() {
    return _bindings.spine_texture_region_get_v(_region);
  }

  void setV(double v) {
    _bindings.spine_texture_region_set_v(_region, v);
  }

  double getU2() {
    return _bindings.spine_texture_region_get_u2(_region);
  }

  void setU2(double u2) {
    _bindings.spine_texture_region_set_u2(_region, u2);
  }

  double getV2() {
    return _bindings.spine_texture_region_get_v2(_region);
  }

  void setV2(double v2) {
    _bindings.spine_texture_region_set_v2(_region, v2);
  }

  int getDegrees() {
    return _bindings.spine_texture_region_get_degrees(_region);
  }

  void setDegrees(int degrees) {
    _bindings.spine_texture_region_set_degrees(_region, degrees);
  }

  double getOffsetX() {
    return _bindings.spine_texture_region_get_offset_x(_region);
  }

  void setOffsetX(double offsetX) {
    _bindings.spine_texture_region_set_offset_x(_region, offsetX);
  }

  double getOffsetY() {
    return _bindings.spine_texture_region_get_offset_x(_region);
  }

  void setOffsetY(double offsetX) {
    _bindings.spine_texture_region_set_offset_x(_region, offsetX);
  }

  int getWidth() {
    return _bindings.spine_texture_region_get_width(_region);
  }

  void setWidth(int width) {
    _bindings.spine_texture_region_set_width(_region, width);
  }

  int getHeight() {
    return _bindings.spine_texture_region_get_height(_region);
  }

  void setHeight(int height) {
    _bindings.spine_texture_region_set_height(_region, height);
  }

  int getOriginalWidth() {
    return _bindings.spine_texture_region_get_original_width(_region);
  }

  void setOriginalWidth(int originalWidth) {
    _bindings.spine_texture_region_set_original_width(_region, originalWidth);
  }

  int getOriginalHeight() {
    return _bindings.spine_texture_region_get_original_height(_region);
  }

  void setOriginalHeight(int originalHeight) {
    _bindings.spine_texture_region_set_original_height(_region, originalHeight);
  }
}

/// Stores a sequence of [TextureRegion] instances that will switched through when set on an attachment.
class Sequence {
  final spine_sequence _sequence;

  Sequence._(this._sequence);

  void apply(Slot slot, Attachment attachment) {
    _bindings.spine_sequence_apply(_sequence, slot._slot, attachment._attachment.cast());
  }

  String getPath(String basePath, int index) {
    final nativeBasePath = basePath.toNativeUtf8(allocator: _allocator);
    final Pointer<Utf8> path = _bindings.spine_sequence_get_path(_sequence, nativeBasePath.cast(), index).cast();
    final result = path.toDartString();
    _allocator.free(nativeBasePath);
    _allocator.free(path);
    return result;
  }

  int getId() {
    return _bindings.spine_sequence_get_id(_sequence);
  }

  void setId(int id) {
    _bindings.spine_sequence_set_id(_sequence, id);
  }

  int getStart() {
    return _bindings.spine_sequence_get_start(_sequence);
  }

  void setStart(int start) {
    _bindings.spine_sequence_set_start(_sequence, start);
  }

  int getDigits() {
    return _bindings.spine_sequence_get_digits(_sequence);
  }

  void setDigits(int digits) {
    _bindings.spine_sequence_set_digits(_sequence, digits);
  }

  int getSetupIndex() {
    return _bindings.spine_sequence_get_setup_index(_sequence);
  }

  void setSetupIndex(int setupIndex) {
    _bindings.spine_sequence_set_setup_index(_sequence, setupIndex);
  }

  List<TextureRegion> getRegions() {
    List<TextureRegion> result = [];
    final num = _bindings.spine_sequence_get_num_regions(_sequence);
    final nativeRegions = _bindings.spine_sequence_get_regions(_sequence);
    for (int i = 0; i < num; i++) {
      result.add(TextureRegion._(nativeRegions[i]));
    }
    return result;
  }
}

/// Attachment types.
enum AttachmentType {
  region(0),
  mesh(1),
  clipping(2),
  boundingBox(3),
  path(4),
  point(5);

  final int value;

  const AttachmentType(this.value);
}

/// The base class for all attachments.
abstract class Attachment<T extends Pointer> {
  final T _attachment;

  Attachment._(this._attachment);

  /// The attachment's name.
  String getName() {
    Pointer<Utf8> name = _bindings.spine_attachment_get_name(_attachment.cast()).cast();
    return name.toString();
  }

  /// The attachment's type.
  AttachmentType getType() {
    final type = _bindings.spine_attachment_get_type(_attachment.cast());
    return AttachmentType.values[type];
  }

  static Attachment _toSubclass(spine_attachment attachment) {
    final type = AttachmentType.values[_bindings.spine_attachment_get_type(attachment)];
    switch (type) {
      case AttachmentType.region:
        return RegionAttachment._(attachment.cast());
      case AttachmentType.mesh:
        return MeshAttachment._(attachment.cast());
      case AttachmentType.clipping:
        return ClippingAttachment._(attachment.cast());
      case AttachmentType.boundingBox:
        return BoundingBoxAttachment._(attachment.cast());
      case AttachmentType.path:
        return PathAttachment._(attachment.cast());
      case AttachmentType.point:
        return PointAttachment._(attachment.cast());
    }
  }

  /// Returns a copy of the attachment. Copied attachments need to be disposed manually
  /// when no longer in use via the [dispose] method.
  Attachment copy() {
    return _toSubclass(_bindings.spine_attachment_copy(_attachment.cast()));
  }

  void dispose() {
    _bindings.spine_attachment_dispose(_attachment.cast());
  }
}

/// An attachment that displays a textured quadrilateral.
///
/// See [Region attachments](http://esotericsoftware.com/spine-regions) in the Spine User Guide.
class RegionAttachment extends Attachment<spine_region_attachment> {
  RegionAttachment._(spine_region_attachment attachment) : super._(attachment);

  /// Transforms and returns the attachment's four vertices to world coordinates. If the attachment has a [Sequence], the region may
  /// be changed.
  ///
  /// See [World transforms](http://esotericsoftware.com/spine-runtime-skeletons#World-transforms) in the Spine
  /// Runtimes Guide.
  List<double> computeWorldVertices(Slot slot) {
    Pointer<Float> vertices = _allocator.allocate(4 * 8).cast();
    _bindings.spine_region_attachment_compute_world_vertices(_attachment, slot._slot, vertices);
    final result = vertices.asTypedList(8).toList();
    _allocator.free(vertices);
    return result;
  }

  /// The local x translation.
  double getX() {
    return _bindings.spine_region_attachment_get_x(_attachment);
  }

  void setX(double x) {
    _bindings.spine_region_attachment_set_x(_attachment, x);
  }

  /// The local y translation.
  double getY() {
    return _bindings.spine_region_attachment_get_y(_attachment);
  }

  void setY(double y) {
    _bindings.spine_region_attachment_set_y(_attachment, y);
  }

  /// The local rotation.
  double getRotation() {
    return _bindings.spine_region_attachment_get_rotation(_attachment);
  }

  void setRotation(double rotation) {
    _bindings.spine_region_attachment_set_rotation(_attachment, rotation);
  }

  /// The local scaleX.
  double getScaleX() {
    return _bindings.spine_region_attachment_get_scale_x(_attachment);
  }

  void setScaleX(double scaleX) {
    _bindings.spine_region_attachment_set_scale_x(_attachment, scaleX);
  }

  /// The local scaleY.
  double getScaleY() {
    return _bindings.spine_region_attachment_get_scale_y(_attachment);
  }

  void setScaleY(double scaleY) {
    _bindings.spine_region_attachment_set_scale_x(_attachment, scaleY);
  }

  /// The width of the region attachment in Spine.
  double getWidth() {
    return _bindings.spine_region_attachment_get_width(_attachment);
  }

  void setWidth(double width) {
    _bindings.spine_region_attachment_set_width(_attachment, width);
  }

  /// The height of the region attachment in Spine.
  double getHeight() {
    return _bindings.spine_region_attachment_get_height(_attachment);
  }

  void setHeight(double height) {
    _bindings.spine_region_attachment_set_height(_attachment, height);
  }

  Color getColor() {
    final color = _bindings.spine_region_attachment_get_color(_attachment);
    return Color(_bindings.spine_color_get_r(color), _bindings.spine_color_get_g(color), _bindings.spine_color_get_b(color),
        _bindings.spine_color_get_a(color));
  }

  void setColor(double r, double g, double b, double a) {
    _bindings.spine_region_attachment_set_color(_attachment, r, g, b, a);
  }

  String getPath() {
    Pointer<Utf8> path = _bindings.spine_region_attachment_get_path(_attachment).cast();
    return path.toDartString();
  }

  TextureRegion? getRegion() {
    final region = _bindings.spine_region_attachment_get_region(_attachment);
    if (region.address == nullptr.address) return null;
    return TextureRegion._(region);
  }

  Sequence? getSequence() {
    final sequence = _bindings.spine_region_attachment_get_sequence(_attachment);
    if (sequence.address == nullptr.address) return null;
    return Sequence._(sequence);
  }

  /// For each of the 4 vertices, a pair of `x,y` values that is the local position of the vertex.
  ///
  /// See [updateRegion].
  Float32List getOffset() {
    final num = _bindings.spine_region_attachment_get_num_offset(_attachment);
    final offset = _bindings.spine_region_attachment_get_offset(_attachment);
    return offset.asTypedList(num);
  }

  Float32List getUVs() {
    final num = _bindings.spine_region_attachment_get_num_uvs(_attachment);
    final uvs = _bindings.spine_region_attachment_get_uvs(_attachment);
    return uvs.asTypedList(num);
  }
}

/// Base class for an attachment with vertices that are transformed by one or more bones and can be deformed by a slot's
/// [Slot.getDeform].
class VertexAttachment<T extends Pointer> extends Attachment<T> {
  VertexAttachment._(T attachment) : super._(attachment);

  /// Transforms and returns the attachment's local [getVertices] to world coordinates. If the slot's [Slot.getDeform] is
  /// not empty, it is used to deform the vertices.

  /// See [World transforms](http://esotericsoftware.com/spine-runtime-skeletons#World-transforms) in the Spine
  /// Runtimes Guide.
  List<double> computeWorldVertices(Slot slot) {
    final worldVerticesLength = _bindings.spine_vertex_attachment_get_world_vertices_length(_attachment.cast());
    Pointer<Float> vertices = _allocator.allocate(4 * worldVerticesLength).cast();
    _bindings.spine_vertex_attachment_compute_world_vertices(_attachment.cast(), slot._slot, vertices);
    final result = vertices.asTypedList(worldVerticesLength).toList();
    _allocator.free(vertices);
    return result;
  }

  /// The bones which affect the [getVertices]. The array entries are, for each vertex, the number of bones affecting
  /// the vertex followed by that many bone indices, which is the index of the bone in [Skeleton.getBones]. Will be null
  /// if this attachment has no weights.
  Int32List getBones() {
    final num = _bindings.spine_vertex_attachment_get_num_bones(_attachment.cast());
    final bones = _bindings.spine_vertex_attachment_get_bones(_attachment.cast());
    return bones.asTypedList(num);
  }

  /// The vertex positions in the bone's coordinate system. For a non-weighted attachment, the values are `x,y`
  /// entries for each vertex. For a weighted attachment, the values are `x,y,weight` entries for each bone affecting
  /// each vertex.
  Float32List getVertices() {
    final num = _bindings.spine_vertex_attachment_get_num_vertices(_attachment.cast());
    final vertices = _bindings.spine_vertex_attachment_get_vertices(_attachment.cast());
    return vertices.asTypedList(num);
  }

  /// Timelines for the timeline attachment are also applied to this attachment. May return `null` if not
  /// attachment-specific timelines should be applied.
  Attachment? getTimelineAttachment() {
    final attachment = _bindings.spine_vertex_attachment_get_timeline_attachment(_attachment.cast());
    if (_attachment.address == nullptr.address) return null;
    return Attachment._toSubclass(attachment);
  }

  void setTimelineAttachment(Attachment? attachment) {
    _bindings.spine_vertex_attachment_set_timeline_attachment(
        _attachment.cast(), attachment == null ? nullptr : attachment._attachment.cast());
  }
}

/// An attachment that displays a textured mesh. A mesh has hull vertices and internal vertices within the hull. Holes are not
/// supported. Each vertex has UVs (texture coordinates) and triangles are used to map an image on to the mesh.
///
/// See [Mesh attachments](http://esotericsoftware.com/spine-meshes) in the Spine User Guide.
class MeshAttachment extends VertexAttachment<spine_mesh_attachment> {
  MeshAttachment._(spine_mesh_attachment attachment) : super._(attachment.cast());

  /// Calculates texture coordinates returned by [getUVs] using the coordinates returned by [getRegionUVs] and region. Must be called if
  /// the region, the region's properties, or the [getRegionUVs] are changed.
  void updateRegion() {
    _bindings.spine_mesh_attachment_update_region(_attachment);
  }

  /// The number of entries at the beginning of {@link #vertices} that make up the mesh hull.
  int getHullLength() {
    return _bindings.spine_mesh_attachment_get_hull_length(_attachment);
  }

  void setHullLength(int hullLength) {
    _bindings.spine_mesh_attachment_set_hull_length(_attachment, hullLength);
  }

  /// The UV pair for each vertex, normalized within the texture region.
  Float32List getRegionUVs() {
    final num = _bindings.spine_mesh_attachment_get_num_region_uvs(_attachment);
    final uvs = _bindings.spine_mesh_attachment_get_region_uvs(_attachment);
    return uvs.asTypedList(num);
  }

  /// The UV pair for each vertex, normalized within the entire texture.
  ///
  /// See [updateRegion].
  Float32List getUVs() {
    final num = _bindings.spine_mesh_attachment_get_num_uvs(_attachment);
    final uvs = _bindings.spine_mesh_attachment_get_uvs(_attachment);
    return uvs.asTypedList(num);
  }

  /// Triplets of vertex indices which describe the mesh's triangulation.
  Uint16List getTriangles() {
    final num = _bindings.spine_mesh_attachment_get_num_triangles(_attachment);
    final triangles = _bindings.spine_mesh_attachment_get_triangles(_attachment);
    return triangles.asTypedList(num);
  }

  Color getColor() {
    final color = _bindings.spine_mesh_attachment_get_color(_attachment);
    return Color(_bindings.spine_color_get_r(color), _bindings.spine_color_get_g(color), _bindings.spine_color_get_b(color),
        _bindings.spine_color_get_a(color));
  }

  void setColor(double r, double g, double b, double a) {
    _bindings.spine_mesh_attachment_set_color(_attachment, r, g, b, a);
  }

  String getPath() {
    Pointer<Utf8> path = _bindings.spine_mesh_attachment_get_path(_attachment).cast();
    return path.toDartString();
  }

  TextureRegion? getRegion() {
    final region = _bindings.spine_mesh_attachment_get_region(_attachment);
    if (region.address == nullptr.address) return null;
    return TextureRegion._(region);
  }

  Sequence? getSequence() {
    final sequence = _bindings.spine_mesh_attachment_get_sequence(_attachment);
    if (sequence.address == nullptr.address) return null;
    return Sequence._(sequence);
  }

  /// The parent mesh if this is a linked mesh, else null. A linked mesh shares the bones, vertices,
  /// region UVs, triangles, hull length, edges, width, and height with the
  /// parent mesh, but may have a different name or path (and therefore a different texture).
  MeshAttachment? getParentMesh() {
    final parent = _bindings.spine_mesh_attachment_get_parent_mesh(_attachment);
    if (parent.address == nullptr.address) return null;
    return MeshAttachment._(parent);
  }

  void setParentMesh(MeshAttachment? parentMesh) {
    _bindings.spine_mesh_attachment_set_parent_mesh(_attachment, parentMesh == null ? nullptr : parentMesh._attachment);
  }

  /// Vertex index pairs describing edges for controlling triangulation, or be null if nonessential data was not exported. Mesh
  /// triangles will never cross edges. Triangulation is not performed at runtime.
  Uint16List getEdges() {
    final num = _bindings.spine_mesh_attachment_get_num_edges(_attachment);
    final edges = _bindings.spine_mesh_attachment_get_edges(_attachment);
    return edges.asTypedList(num);
  }

  /// The width of the mesh's image, or zero if nonessential data was not exported.
  double getWidth() {
    return _bindings.spine_mesh_attachment_get_width(_attachment);
  }

  void setWidth(double width) {
    _bindings.spine_mesh_attachment_set_width(_attachment, width);
  }

  /// The height of the mesh's image, or zero if nonessential data was not exported.
  double getHeight() {
    return _bindings.spine_mesh_attachment_get_height(_attachment);
  }

  void setHeight(double height) {
    _bindings.spine_mesh_attachment_set_height(_attachment, height);
  }
}

/// An attachment with vertices that make up a polygon used for clipping the rendering of other attachments.
class ClippingAttachment extends VertexAttachment<spine_clipping_attachment> {
  ClippingAttachment._(spine_clipping_attachment attachment) : super._(attachment.cast());

  /// Clipping is performed between the clipping attachment's slot and the end slot. If null clipping is done until the end of
  /// the skeleton's rendering.
  SlotData? getEndSlot() {
    final endSlot = _bindings.spine_clipping_attachment_get_end_slot(_attachment);
    if (endSlot.address == nullptr.address) return null;
    return SlotData._(endSlot);
  }

  void setEndSlot(SlotData? endSlot) {
    _bindings.spine_clipping_attachment_set_end_slot(_attachment, endSlot == null ? nullptr : endSlot._data);
  }

  /// The color of the clipping attachment as it was in Spine, or a default color if nonessential data was not exported. Clipping
  /// attachments are not usually rendered at runtime.
  Color getColor() {
    final color = _bindings.spine_clipping_attachment_get_color(_attachment);
    return Color(_bindings.spine_color_get_r(color), _bindings.spine_color_get_g(color), _bindings.spine_color_get_b(color),
        _bindings.spine_color_get_a(color));
  }

  void setColor(double r, double g, double b, double a) {
    _bindings.spine_clipping_attachment_set_color(_attachment, r, g, b, a);
  }
}

/// An attachment with vertices that make up a polygon. Can be used for hit detection, creating physics bodies, spawning particle
/// effects, and more.
///
/// See [SkeletonBounds] and [Bounding boxes](http://esotericsoftware.com/spine-bounding-boxes) in the Spine User
/// Guide.
class BoundingBoxAttachment extends VertexAttachment<spine_bounding_box_attachment> {
  BoundingBoxAttachment._(spine_bounding_box_attachment attachment) : super._(attachment);

  /// The color of the bounding box as it was in Spine, or a default color if nonessential data was not exported. Bounding boxes
  /// are not usually rendered at runtime.
  Color getColor() {
    final color = _bindings.spine_bounding_box_attachment_get_color(_attachment);
    return Color(_bindings.spine_color_get_r(color), _bindings.spine_color_get_g(color), _bindings.spine_color_get_b(color),
        _bindings.spine_color_get_a(color));
  }

  void setColor(double r, double g, double b, double a) {
    _bindings.spine_bounding_box_attachment_set_color(_attachment, r, g, b, a);
  }
}

/// An attachment whose vertices make up a composite Bezier curve.
///
/// See [PathConstraint] and [Paths](http://esotericsoftware.com/spine-paths) in the Spine User Guide.
class PathAttachment extends VertexAttachment<spine_path_attachment> {
  PathAttachment._(spine_path_attachment attachment) : super._(attachment);

  /// The lengths along the path in the setup pose from the start of the path to the end of each Bezier curve.
  Float32List getLengths() {
    final num = _bindings.spine_path_attachment_get_num_lengths(_attachment);
    final lengths = _bindings.spine_path_attachment_get_lengths(_attachment);
    return lengths.asTypedList(num);
  }

  /// If true, the start and end knots are connected.
  bool isClosed() {
    return _bindings.spine_path_attachment_get_is_closed(_attachment) == -1;
  }

  void setIsClosed(bool isClosed) {
    _bindings.spine_path_attachment_set_is_closed(_attachment, isClosed ? -1 : 0);
  }

  /// If true, additional calculations are performed to make computing positions along the path more accurate and movement along
  /// the path have a constant speed.
  bool isConstantSpeed() {
    return _bindings.spine_path_attachment_get_is_constant_speed(_attachment) == -1;
  }

  void setIsConstantSpeed(bool isClosed) {
    _bindings.spine_path_attachment_set_is_constant_speed(_attachment, isClosed ? -1 : 0);
  }

  /// The color of the path as it was in Spine, or a default color if nonessential data was not exported. Paths are not usually
  /// rendered at runtime.
  Color getColor() {
    final color = _bindings.spine_path_attachment_get_color(_attachment);
    return Color(_bindings.spine_color_get_r(color), _bindings.spine_color_get_g(color), _bindings.spine_color_get_b(color),
        _bindings.spine_color_get_a(color));
  }

  void setColor(double r, double g, double b, double a) {
    _bindings.spine_path_attachment_set_color(_attachment, r, g, b, a);
  }
}

/// An attachment which is a single point and a rotation. This can be used to spawn projectiles, particles, etc. A bone can be
/// used in similar ways, but a PointAttachment is slightly less expensive to compute and can be hidden, shown, and placed in a
/// skin.
///
/// See [Point Attachments](http://esotericsoftware.com/spine-point-attachments) in the Spine User Guide.
class PointAttachment extends Attachment<spine_point_attachment> {
  PointAttachment._(spine_point_attachment attachment) : super._(attachment);

  Vec2 computeWorldPosition(Bone bone) {
    final position = _bindings.spine_point_attachment_compute_world_position(_attachment, bone._bone);
    final result = Vec2(_bindings.spine_vector_get_x(position), _bindings.spine_vector_get_y(position));
    return result;
  }

  double computeWorldRotation(Bone bone) {
    return _bindings.spine_point_attachment_compute_world_rotation(_attachment, bone._bone);
  }

  double getX() {
    return _bindings.spine_point_attachment_get_x(_attachment);
  }

  void setX(double x) {
    _bindings.spine_point_attachment_set_x(_attachment, x);
  }

  double getY() {
    return _bindings.spine_point_attachment_get_y(_attachment);
  }

  void setY(double y) {
    _bindings.spine_point_attachment_set_y(_attachment, y);
  }

  double getRotation() {
    return _bindings.spine_point_attachment_get_rotation(_attachment);
  }

  void setRotation(double rotation) {
    _bindings.spine_point_attachment_set_x(_attachment, rotation);
  }

  /// The color of the point attachment as it was in Spine, or a default clor if nonessential data was not exported. Point
  /// attachments are not usually rendered at runtime.
  Color getColor() {
    final color = _bindings.spine_point_attachment_get_color(_attachment);
    return Color(_bindings.spine_color_get_r(color), _bindings.spine_color_get_g(color), _bindings.spine_color_get_b(color),
        _bindings.spine_color_get_a(color));
  }

  void setColor(double r, double g, double b, double a) {
    _bindings.spine_point_attachment_set_color(_attachment, r, g, b, a);
  }
}

/// An entry storing the attachment to be used for a specific slot within [Skin].
class SkinEntry {
  final int slotIndex;
  final String name;
  final Attachment? attachment;

  SkinEntry(this.slotIndex, this.name, this.attachment);
}

/// Stores attachments by slot index and attachment name.
///
/// Skins constructed manually via the `Skin(String name)` constructor must be manually disposed via the [dipose] method if they
/// are no longer used.
///
/// See [SkeletonData.defaultSkin], [Skeleton.getSkin}, and [Runtime skins](http://esotericsoftware.com/spine-runtime-skins) in the
/// Spine Runtimes Guide.
class Skin {
  late final bool _isCustomSkin;
  late final spine_skin _skin;

  Skin._(this._skin) : _isCustomSkin = false;

  /// Constructs a new empty skin using the given [name]. Skins constructed this way must be manually disposed via the [dispose] method
  /// if they are no longer used.
  Skin(String name) {
    final nativeName = name.toNativeUtf8(allocator: _allocator);
    _skin = _bindings.spine_skin_create(nativeName.cast());
    _allocator.free(nativeName);
    _isCustomSkin = true;
  }

  /// Diposes this skin.
  void dispose() {
    if (!_isCustomSkin) return;
    _bindings.spine_skin_dispose(_skin);
  }

  /// Adds an attachment to the skin for the specified slot index and name.
  void setAttachment(int slotIndex, String name, Attachment? attachment) {
    final nativeName = name.toNativeUtf8(allocator: _allocator);
    _bindings.spine_skin_set_attachment(_skin, slotIndex, nativeName.cast(), attachment == null ? nullptr : attachment._attachment.cast());
    _allocator.free(nativeName);
  }

  /// Returns the attachment for the specified slot index and name, or null.
  Attachment? getAttachment(int slotIndex, String name) {
    final nativeName = name.toNativeUtf8(allocator: _allocator);
    final attachment = _bindings.spine_skin_get_attachment(_skin, slotIndex, nativeName.cast());
    _allocator.free(nativeName);
    if (attachment.address == nullptr.address) return null;
    return Attachment._toSubclass(attachment);
  }

  /// Removes the attachment in the skin for the specified slot index and name, if any.
  void removeAttachment(int slotIndex, String name) {
    final nativeName = name.toNativeUtf8(allocator: _allocator);
    _bindings.spine_skin_remove_attachment(_skin, slotIndex, nativeName.cast());
    _allocator.free(nativeName);
  }

  /// The skin's name, which is unique across all skins in the skeleton.
  String getName() {
    Pointer<Utf8> name = _bindings.spine_skin_get_name(_skin).cast();
    return name.toDartString();
  }

  /// Adds all attachments, bones, and constraints from the specified skin to this skin.
  void addSkin(Skin other) {
    _bindings.spine_skin_add_skin(_skin, other._skin);
  }

  /// Returns all entries in this skin.
  List<SkinEntry> getEntries() {
    List<SkinEntry> result = [];
    final entries = _bindings.spine_skin_get_entries(_skin);
    int numEntries = _bindings.spine_skin_entries_get_num_entries(entries);
    for (int i = 0; i < numEntries; i++) {
      final entry = _bindings.spine_skin_entries_get_entry(entries, i);
      Pointer<Utf8> name = _bindings.spine_skin_entry_get_name(entry).cast();
      result.add(SkinEntry(
          _bindings.spine_skin_entry_get_slot_index(entry),
          name.toDartString(),
          _bindings.spine_skin_entry_get_attachment(entry).address == nullptr.address
              ? null
              : Attachment._toSubclass(_bindings.spine_skin_entry_get_attachment(entry))));
    }
    return result;
  }

  List<BoneData> getBones() {
    List<BoneData> bones = [];
    final numBones = _bindings.spine_skin_get_num_bones(_skin);
    final nativeBones = _bindings.spine_skin_get_bones(_skin);
    for (int i = 0; i < numBones; i++) {
      bones.add(BoneData._(nativeBones[i]));
    }
    return bones;
  }

  List<ConstraintData> getConstraints() {
    List<ConstraintData> constraints = [];
    final numConstraints = _bindings.spine_skin_get_num_constraints(_skin);
    final nativeConstraints = _bindings.spine_skin_get_constraints(_skin);
    for (int i = 0; i < numConstraints; i++) {
      final nativeConstraint = nativeConstraints[i];
      final type = _bindings.spine_constraint_data_get_type(nativeConstraint);
      switch (type) {
        case spine_constraint_type.SPINE_CONSTRAINT_IK:
          constraints.add(IkConstraintData._(nativeConstraint.cast()));
          break;
        case spine_constraint_type.SPINE_CONSTRAINT_TRANSFORM:
          constraints.add(TransformConstraintData._(nativeConstraint.cast()));
          break;
        case spine_constraint_type.SPINE_CONSTRAINT_PATH:
          constraints.add(PathConstraintData._(nativeConstraint.cast()));
          break;
      }
    }
    return constraints;
  }

  /// Adds all bones and constraints and copies of all attachments from the specified skin to this skin. Mesh attachments are not
  /// copied, instead a new linked mesh is created. The attachment copies can be modified without affecting the originals.
  void copy(Skin other) {
    _bindings.spine_skin_copy_skin(_skin, other._skin);
  }
}

/// The base class for all constraint datas.
class ConstraintData<T extends Pointer> {
  final T _data;

  ConstraintData._(this._data);

  /// The constraint's name, which is unique across all constraints in the skeleton of the same type.
  String getName() {
    final Pointer<Utf8> name = _bindings.spine_constraint_data_get_name(_data.cast()).cast();
    return name.toDartString();
  }

  /// The ordinal of this constraint for the order a skeleton's constraints will be applied by
  /// [Skeleton.updateWorldTransform].
  int getOrder() {
    return _bindings.spine_constraint_data_get_order(_data.cast());
  }

  void setOrder(int order) {
    _bindings.spine_constraint_data_set_order(_data.cast(), order);
  }

  /// When true, [Skeleton.updateWorldTransform] only updates this constraint if the skin returned by [Skeleton.getSkin] contains
  /// this constraint.
  ///
  /// See [Skin.getConstraints].
  bool isSkinRequired() {
    return _bindings.spine_constraint_data_get_is_skin_required(_data.cast()) == 1;
  }

  void setIsSkinRequired(bool isSkinRequired) {
    _bindings.spine_constraint_data_set_is_skin_required(_data.cast(), isSkinRequired ? -1 : 0);
  }
}

/// Stores the setup pose for an [IkConstraint].
///
/// See [IK constraints](http://esotericsoftware.com/spine-ik-constraints) in the Spine User Guide.
class IkConstraintData extends ConstraintData<spine_ik_constraint_data> {
  IkConstraintData._(spine_ik_constraint_data data) : super._(data);

  /// The bones that are constrained by this IK constraint.
  List<BoneData> getBones() {
    final List<BoneData> result = [];
    final numBones = _bindings.spine_ik_constraint_data_get_num_bones(_data);
    final nativeBones = _bindings.spine_ik_constraint_data_get_bones(_data);
    for (int i = 0; i < numBones; i++) {
      result.add(BoneData._(nativeBones[i]));
    }
    return result;
  }

  /// The bone that is the IK target.
  BoneData getTarget() {
    return BoneData._(_bindings.spine_ik_constraint_data_get_target(_data));
  }

  void setTarget(BoneData target) {
    _bindings.spine_ik_constraint_data_set_target(_data, target._data);
  }

  /// For two bone IK, controls the bend direction of the IK bones, either 1 or -1.
  int getBendDirection() {
    return _bindings.spine_ik_constraint_data_get_bend_direction(_data);
  }

  void setBendDirection(int bendDirection) {
    _bindings.spine_ik_constraint_data_set_bend_direction(_data, bendDirection);
  }

  /// For one bone IK, when true and the target is too close, the bone is scaled to reach it.
  bool getCompress() {
    return _bindings.spine_ik_constraint_data_get_compress(_data) == -1;
  }

  void setCompress(bool compress) {
    _bindings.spine_ik_constraint_data_set_compress(_data, compress ? -1 : 0);
  }

  /// When true and the target is out of range, the parent bone is scaled to reach it.
  ///
  /// For two bone IK: 1) the child bone's local Y translation is set to 0, 2) stretch is not applied if [getSoftness] is
  /// > 0, and 3) if the parent bone has local nonuniform scale, stretch is not applied.
  bool getStretch() {
    return _bindings.spine_ik_constraint_data_get_stretch(_data) == -1;
  }

  void setStretch(bool stretch) {
    _bindings.spine_ik_constraint_data_set_stretch(_data, stretch ? -1 : 0);
  }

  /// When true and [getCompress] or [getStretch] is used, the bone is scaled on both the X and Y axes.
  bool getUniform() {
    return _bindings.spine_ik_constraint_data_get_uniform(_data) == -1;
  }

  void setUniform(bool uniform) {
    _bindings.spine_ik_constraint_data_set_uniform(_data, uniform ? -1 : 0);
  }

  /// A percentage (0-1) that controls the mix between the constrained and unconstrained rotation.
  ///
  /// For two bone IK: if the parent bone has local nonuniform scale, the child bone's local Y translation is set to 0.
  double getMix() {
    return _bindings.spine_ik_constraint_data_get_mix(_data);
  }

  void setMix(double mix) {
    _bindings.spine_ik_constraint_data_set_mix(_data, mix);
  }

  /// For two bone IK, the target bone's distance from the maximum reach of the bones where rotation begins to slow. The bones
  /// will not straighten completely until the target is this far out of range.
  double getSoftness() {
    return _bindings.spine_ik_constraint_data_get_softness(_data);
  }

  void setSoftness(double softness) {
    _bindings.spine_ik_constraint_data_set_softness(_data, softness);
  }
}

/// Stores the current pose for an IK constraint. An IK constraint adjusts the rotation of 1 or 2 constrained bones so the tip of
/// the last bone is as close to the target bone as possible.
/// <p>
/// See <a href="http://esotericsoftware.com/spine-ik-constraints">IK constraints</a> in the Spine User Guide.
class IkConstraint {
  final spine_ik_constraint _constraint;

  IkConstraint._(this._constraint);

  /// Applies the constraint to the constrained bones.
  void update() {
    _bindings.spine_ik_constraint_update(_constraint);
  }

  int getOrder() {
    return _bindings.spine_ik_constraint_get_order(_constraint);
  }

  /// The IK constraint's setup pose data.
  IkConstraintData getData() {
    return IkConstraintData._(_bindings.spine_ik_constraint_get_data(_constraint));
  }

  /// The bones that will be modified by this IK constraint.
  List<Bone> getBones() {
    List<Bone> result = [];
    final num = _bindings.spine_ik_constraint_get_num_bones(_constraint);
    final nativeBones = _bindings.spine_ik_constraint_get_bones(_constraint);
    for (int i = 0; i < num; i++) {
      result.add(Bone._(nativeBones[i]));
    }
    return result;
  }

  /// The bone that is the IK target.
  Bone getTarget() {
    return Bone._(_bindings.spine_ik_constraint_get_target(_constraint));
  }

  void setTarget(Bone target) {
    _bindings.spine_ik_constraint_set_target(_constraint, target._bone);
  }

  /// For two bone IK, controls the bend direction of the IK bones, either 1 or -1.
  int getBendDirection() {
    return _bindings.spine_ik_constraint_get_bend_direction(_constraint);
  }

  void setBendDirection(int bendDirection) {
    _bindings.spine_ik_constraint_set_bend_direction(_constraint, bendDirection);
  }

  /// For one bone IK, when true and the target is too close, the bone is scaled to reach it.
  bool getCompress() {
    return _bindings.spine_ik_constraint_get_compress(_constraint) == -1;
  }

  void setCompress(bool compress) {
    _bindings.spine_ik_constraint_set_compress(_constraint, compress ? -1 : 0);
  }

  /// When true and the target is out of range, the parent bone is scaled to reach it.
  ///
  /// For two bone IK: 1) the child bone's local Y translation is set to 0, 2) stretch is not applied if [getSoftness] is
  /// > 0, and 3) if the parent bone has local nonuniform scale, stretch is not applied.
  bool getStretch() {
    return _bindings.spine_ik_constraint_get_stretch(_constraint) == -1;
  }

  void setStretch(bool stretch) {
    _bindings.spine_ik_constraint_set_stretch(_constraint, stretch ? -1 : 0);
  }

  /// A percentage (0-1) that controls the mix between the constrained and unconstrained rotation.
  ///
  /// For two bone IK: if the parent bone has local nonuniform scale, the child bone's local Y translation is set to 0.
  double getMix() {
    return _bindings.spine_ik_constraint_get_mix(_constraint);
  }

  void setMix(double mix) {
    _bindings.spine_ik_constraint_set_mix(_constraint, mix);
  }

  /// For two bone IK, the target bone's distance from the maximum reach of the bones where rotation begins to slow. The bones
  /// will not straighten completely until the target is this far out of range.
  double getSoftness() {
    return _bindings.spine_ik_constraint_get_softness(_constraint);
  }

  void setSoftness(double softness) {
    _bindings.spine_ik_constraint_set_softness(_constraint, softness);
  }

  bool isActive() {
    return _bindings.spine_ik_constraint_get_is_active(_constraint) == -1;
  }

  void setIsActive(bool isActive) {
    _bindings.spine_ik_constraint_set_is_active(_constraint, isActive ? -1 : 0);
  }
}

/// Stores the setup pose for a {@link TransformConstraint}.
///
/// See [Transform constraints](http://esotericsoftware.com/spine-transform-constraints) in the Spine User Guide.
class TransformConstraintData extends ConstraintData<spine_transform_constraint_data> {
  TransformConstraintData._(spine_transform_constraint_data data) : super._(data);

  /// The bones that will be modified by this transform constraint.
  List<BoneData> getBones() {
    final List<BoneData> result = [];
    final numBones = _bindings.spine_transform_constraint_data_get_num_bones(_data);
    final nativeBones = _bindings.spine_transform_constraint_data_get_bones(_data);
    for (int i = 0; i < numBones; i++) {
      result.add(BoneData._(nativeBones[i]));
    }
    return result;
  }

  /// The target bone whose world transform will be copied to the constrained bones.
  BoneData getTarget() {
    return BoneData._(_bindings.spine_transform_constraint_data_get_target(_data));
  }

  void setTarget(BoneData target) {
    _bindings.spine_transform_constraint_data_set_target(_data, target._data);
  }

  /// A percentage (0-1) that controls the mix between the constrained and unconstrained rotation.
  double getMixRotate() {
    return _bindings.spine_transform_constraint_data_get_mix_rotate(_data);
  }

  void setMixRotate(double mixRotate) {
    _bindings.spine_transform_constraint_data_set_mix_rotate(_data, mixRotate);
  }

  /// A percentage (0-1) that controls the mix between the constrained and unconstrained translation X.
  double getMixX() {
    return _bindings.spine_transform_constraint_data_get_mix_x(_data);
  }

  void setMixX(double mixX) {
    _bindings.spine_transform_constraint_data_set_mix_x(_data, mixX);
  }

  /// A percentage (0-1) that controls the mix between the constrained and unconstrained translation Y.
  double getMixY() {
    return _bindings.spine_transform_constraint_data_get_mix_y(_data);
  }

  void setMixY(double mixY) {
    _bindings.spine_transform_constraint_data_set_mix_y(_data, mixY);
  }

  /// A percentage (0-1) that controls the mix between the constrained and unconstrained scale X.
  double getMixScaleX() {
    return _bindings.spine_transform_constraint_data_get_mix_scale_x(_data);
  }

  void setMixScaleX(double mixScaleX) {
    _bindings.spine_transform_constraint_data_set_mix_scale_x(_data, mixScaleX);
  }

  /// A percentage (0-1) that controls the mix between the constrained and unconstrained scale Y.
  double getMixScaleY() {
    return _bindings.spine_transform_constraint_data_get_mix_scale_y(_data);
  }

  void setMixScaleY(double mixScaleY) {
    _bindings.spine_transform_constraint_data_set_mix_scale_y(_data, mixScaleY);
  }

  /// A percentage (0-1) that controls the mix between the constrained and unconstrained shear Y.
  double getMixShearY() {
    return _bindings.spine_transform_constraint_data_get_mix_shear_y(_data);
  }

  void setMixShearY(double mixShearY) {
    _bindings.spine_transform_constraint_data_set_mix_shear_y(_data, mixShearY);
  }

  /// An offset added to the constrained bone rotation.
  double getOffsetRotation() {
    return _bindings.spine_transform_constraint_data_get_offset_rotation(_data);
  }

  void setOffsetRotation(double offsetRotation) {
    _bindings.spine_transform_constraint_data_set_offset_rotation(_data, offsetRotation);
  }

  /// An offset added to the constrained bone X translation.
  double getOffsetX() {
    return _bindings.spine_transform_constraint_data_get_offset_x(_data);
  }

  void setOffsetX(double offsetX) {
    _bindings.spine_transform_constraint_data_set_offset_x(_data, offsetX);
  }

  /// An offset added to the constrained bone Y translation.
  double getOffsetY() {
    return _bindings.spine_transform_constraint_data_get_offset_y(_data);
  }

  /// An offset added to the constrained bone scaleX.
  void setOffsetY(double offsetY) {
    _bindings.spine_transform_constraint_data_set_offset_y(_data, offsetY);
  }

  /// An offset added to the constrained bone scaleX.
  double getOffsetScaleX() {
    return _bindings.spine_transform_constraint_data_get_offset_scale_x(_data);
  }

  void setOffsetScaleX(double offsetScaleX) {
    _bindings.spine_transform_constraint_data_set_offset_x(_data, offsetScaleX);
  }

  /// An offset added to the constrained bone scaleY.
  double getOffsetScaleY() {
    return _bindings.spine_transform_constraint_data_get_offset_scale_y(_data);
  }

  void setOffsetScaleY(double offsetScaleY) {
    _bindings.spine_transform_constraint_data_set_offset_scale_y(_data, offsetScaleY);
  }

  /// An offset added to the constrained bone shearY.
  double getOffsetShearY() {
    return _bindings.spine_transform_constraint_data_get_offset_shear_y(_data);
  }

  void setOffsetShearY(double offsetShearY) {
    _bindings.spine_transform_constraint_data_set_offset_shear_y(_data, offsetShearY);
  }

  bool isRelative() {
    return _bindings.spine_transform_constraint_data_get_is_relative(_data) == -1;
  }

  void setIsRelative(bool isRelative) {
    _bindings.spine_transform_constraint_data_set_is_relative(_data, isRelative ? -1 : 0);
  }

  bool isLocal() {
    return _bindings.spine_transform_constraint_data_get_is_local(_data) == -1;
  }

  void setIsLocal(bool isLocal) {
    _bindings.spine_transform_constraint_data_set_is_local(_data, isLocal ? -1 : 0);
  }
}

/// Stores the current pose for a transform constraint. A transform constraint adjusts the world transform of the constrained
/// bones to match that of the target bone.
///
/// See [Transform constraints](http://esotericsoftware.com/spine-transform-constraints) in the Spine User Guide.
class TransformConstraint {
  final spine_transform_constraint _constraint;

  TransformConstraint._(this._constraint);

  /// Applies the constraint to the constrained bones.
  void update() {
    _bindings.spine_transform_constraint_update(_constraint);
  }

  int getOrder() {
    return _bindings.spine_transform_constraint_get_order(_constraint);
  }

  /// The transform constraint's setup pose data.
  TransformConstraintData getData() {
    return TransformConstraintData._(_bindings.spine_transform_constraint_get_data(_constraint));
  }

  /// The bones that will be modified by this transform constraint.
  List<Bone> getBones() {
    List<Bone> result = [];
    final num = _bindings.spine_transform_constraint_get_num_bones(_constraint);
    final nativeBones = _bindings.spine_transform_constraint_get_bones(_constraint);
    for (int i = 0; i < num; i++) {
      result.add(Bone._(nativeBones[i]));
    }
    return result;
  }

  /// The target bone whose world transform will be copied to the constrained bones.
  Bone getTarget() {
    return Bone._(_bindings.spine_transform_constraint_get_target(_constraint));
  }

  void setTarget(Bone target) {
    _bindings.spine_transform_constraint_set_target(_constraint, target._bone);
  }

  /// A percentage (0-1) that controls the mix between the constrained and unconstrained rotation.
  double getMixRotate() {
    return _bindings.spine_transform_constraint_get_mix_rotate(_constraint);
  }

  void setMixRotate(double mixRotate) {
    _bindings.spine_transform_constraint_set_mix_rotate(_constraint, mixRotate);
  }

  /// A percentage (0-1) that controls the mix between the constrained and unconstrained translation X.
  double getMixX() {
    return _bindings.spine_transform_constraint_get_mix_x(_constraint);
  }

  void setMixX(double mixX) {
    _bindings.spine_transform_constraint_set_mix_x(_constraint, mixX);
  }

  /// A percentage (0-1) that controls the mix between the constrained and unconstrained translation Y.
  double getMixY() {
    return _bindings.spine_transform_constraint_get_mix_y(_constraint);
  }

  void setMixY(double mixY) {
    _bindings.spine_transform_constraint_set_mix_y(_constraint, mixY);
  }

  /// A percentage (0-1) that controls the mix between the constrained and unconstrained scale X.
  double getMixScaleX() {
    return _bindings.spine_transform_constraint_get_mix_scale_x(_constraint);
  }

  void setMixScaleX(double mixScaleX) {
    _bindings.spine_transform_constraint_set_mix_scale_x(_constraint, mixScaleX);
  }

  /// A percentage (0-1) that controls the mix between the constrained and unconstrained scale X.
  double getMixScaleY() {
    return _bindings.spine_transform_constraint_get_mix_scale_y(_constraint);
  }

  void setMixScaleY(double mixScaleY) {
    _bindings.spine_transform_constraint_set_mix_scale_y(_constraint, mixScaleY);
  }

  /// A percentage (0-1) that controls the mix between the constrained and unconstrained shear Y.
  double getMixShearY() {
    return _bindings.spine_transform_constraint_get_mix_shear_y(_constraint);
  }

  void setMixShearY(double mixShearY) {
    _bindings.spine_transform_constraint_set_mix_shear_y(_constraint, mixShearY);
  }

  bool isActive() {
    return _bindings.spine_transform_constraint_get_is_active(_constraint) == -1;
  }

  void setIsActive(bool isActive) {
    _bindings.spine_transform_constraint_set_is_active(_constraint, isActive ? -1 : 0);
  }
}

/// Stores the setup pose for a [PathConstraint].
///
/// See [Path constraints](http://esotericsoftware.com/spine-path-constraints) in the Spine User Guide.
class PathConstraintData extends ConstraintData<spine_path_constraint_data> {
  PathConstraintData._(spine_path_constraint_data data) : super._(data);

  /// The bones that will be modified by this path constraint.
  List<BoneData> getBones() {
    final List<BoneData> result = [];
    final numBones = _bindings.spine_path_constraint_data_get_num_bones(_data);
    final nativeBones = _bindings.spine_path_constraint_data_get_bones(_data);
    for (int i = 0; i < numBones; i++) {
      result.add(BoneData._(nativeBones[i]));
    }
    return result;
  }

  /// The slot whose path attachment will be used to constrained the bones.
  SlotData getTarget() {
    return SlotData._(_bindings.spine_path_constraint_data_get_target(_data));
  }

  void setTarget(SlotData target) {
    _bindings.spine_path_constraint_data_set_target(_data, target._data);
  }

  /// The mode for positioning the first bone on the path.
  PositionMode getPositionMode() {
    return PositionMode.values[_bindings.spine_path_constraint_data_get_position_mode(_data)];
  }

  void setPositionMode(PositionMode positionMode) {
    _bindings.spine_path_constraint_data_set_position_mode(_data, positionMode.value);
  }

  /// The mode for positioning the bones after the first bone on the path.
  SpacingMode getSpacingMode() {
    return SpacingMode.values[_bindings.spine_path_constraint_data_get_spacing_mode(_data)];
  }

  void setSpacingMode(SpacingMode spacingMode) {
    _bindings.spine_path_constraint_data_set_spacing_mode(_data, spacingMode.value);
  }

  /// The mode for adjusting the rotation of the bones.
  RotateMode getRotateMode() {
    return RotateMode.values[_bindings.spine_path_constraint_data_get_rotate_mode(_data)];
  }

  void setRotateMode(RotateMode rotateMode) {
    _bindings.spine_path_constraint_data_set_rotate_mode(_data, rotateMode.value);
  }

  /// An offset added to the constrained bone rotation.
  double getOffsetRotation() {
    return _bindings.spine_path_constraint_data_get_offset_rotation(_data);
  }

  void setOffsetRotation(double offsetRotation) {
    _bindings.spine_path_constraint_data_set_offset_rotation(_data, offsetRotation);
  }

  /// The position along the path.
  double getPosition() {
    return _bindings.spine_path_constraint_data_get_position(_data);
  }

  void setPosition(double position) {
    _bindings.spine_path_constraint_data_set_position(_data, position);
  }

  /// The spacing between bones.
  double getSpacing() {
    return _bindings.spine_path_constraint_data_get_spacing(_data);
  }

  void setSpacing(double spacing) {
    _bindings.spine_path_constraint_data_set_spacing(_data, spacing);
  }

  /// A percentage (0-1) that controls the mix between the constrained and unconstrained rotation.
  double getMixRotate() {
    return _bindings.spine_path_constraint_data_get_mix_rotate(_data);
  }

  void setMixRotate(double mixRotate) {
    _bindings.spine_path_constraint_data_set_mix_rotate(_data, mixRotate);
  }

  /// A percentage (0-1) that controls the mix between the constrained and unconstrained translation X.
  double getMixX() {
    return _bindings.spine_path_constraint_data_get_mix_x(_data);
  }

  void setMixX(double mixX) {
    _bindings.spine_path_constraint_data_set_mix_x(_data, mixX);
  }

  /// A percentage (0-1) that controls the mix between the constrained and unconstrained translation Y.
  double getMixY() {
    return _bindings.spine_path_constraint_data_get_mix_x(_data);
  }

  void setMixY(double mixY) {
    _bindings.spine_path_constraint_data_set_mix_y(_data, mixY);
  }
}

/// Stores the current pose for a path constraint. A path constraint adjusts the rotation, translation, and scale of the
/// constrained bones so they follow a [PathAttachment].
///
/// See [Path constraints](http://esotericsoftware.com/spine-path-constraints) in the Spine User Guide.
class PathConstraint {
  final spine_path_constraint _constraint;

  PathConstraint._(this._constraint);

  /// Applies the constraint to the constrained bones.
  void update() {
    _bindings.spine_path_constraint_update(_constraint);
  }

  int getOrder() {
    return _bindings.spine_path_constraint_get_order(_constraint);
  }

  /// The bones that will be modified by this path constraint.
  List<Bone> getBones() {
    List<Bone> result = [];
    final num = _bindings.spine_path_constraint_get_num_bones(_constraint);
    final nativeBones = _bindings.spine_path_constraint_get_bones(_constraint);
    for (int i = 0; i < num; i++) {
      result.add(Bone._(nativeBones[i]));
    }
    return result;
  }

  /// The slot whose path attachment will be used to constrained the bones.
  Slot getTarget() {
    return Slot._(_bindings.spine_path_constraint_get_target(_constraint));
  }

  void setTarget(Slot target) {
    _bindings.spine_path_constraint_set_target(_constraint, target._slot);
  }

  /// The position along the path.
  double getPosition() {
    return _bindings.spine_path_constraint_get_position(_constraint);
  }

  void setPosition(double position) {
    _bindings.spine_path_constraint_set_position(_constraint, position);
  }

  /// The spacing between bones.
  double getSpacing() {
    return _bindings.spine_path_constraint_get_spacing(_constraint);
  }

  void setSpacing(double spacing) {
    _bindings.spine_path_constraint_set_spacing(_constraint, spacing);
  }

  /// A percentage (0-1) that controls the mix between the constrained and unconstrained rotation.
  double getMixRotate() {
    return _bindings.spine_path_constraint_get_mix_rotate(_constraint);
  }

  void setMixRotate(double mixRotate) {
    _bindings.spine_path_constraint_set_mix_rotate(_constraint, mixRotate);
  }

  /// A percentage (0-1) that controls the mix between the constrained and unconstrained translation X.
  double getMixX() {
    return _bindings.spine_path_constraint_get_mix_x(_constraint);
  }

  void setMixX(double mixX) {
    _bindings.spine_path_constraint_set_mix_x(_constraint, mixX);
  }

  /// A percentage (0-1) that controls the mix between the constrained and unconstrained translation Y.
  double getMixY() {
    return _bindings.spine_path_constraint_get_mix_y(_constraint);
  }

  void setMixY(double mixY) {
    _bindings.spine_path_constraint_set_mix_y(_constraint, mixY);
  }

  bool isActive() {
    return _bindings.spine_path_constraint_get_is_active(_constraint) == -1;
  }

  void setIsActive(bool isActive) {
    _bindings.spine_path_constraint_set_is_active(_constraint, isActive ? -1 : 0);
  }
}

/// Stores the current pose for a skeleton.
///
/// See [Instance objects](http://esotericsoftware.com/spine-runtime-architecture#Instance-objects) in the Spine
/// Runtimes Guide.
class Skeleton {
  final spine_skeleton _skeleton;

  Skeleton._(this._skeleton);

  /// Caches information about bones and constraints. Must be called if the [getSkin] is modified or if bones,
  /// constraints, or weighted path attachments are added or removed.
  void updateCache() {
    _bindings.spine_skeleton_update_cache(_skeleton);
  }

  /// Updates the world transform for each bone and applies all constraints.
  ///
  /// See [World transforms](http://esotericsoftware.com/spine-runtime-skeletons#World-transforms) in the Spine
  /// Runtimes Guide.
  void updateWorldTransform(Physics physics) {
    _bindings.spine_skeleton_update_world_transform(_skeleton, physics.value);
  }

  /// Temporarily sets the root bone as a child of the specified bone, then updates the world transform for each bone and applies
  /// all constraints.
  ///
  /// See [World transforms](http://esotericsoftware.com/spine-runtime-skeletons#World-transforms) in the Spine
  /// Runtimes Guide.
  void updateWorldTransformBone(Physics physics, Bone parent) {
    _bindings.spine_skeleton_update_world_transform_bone(_skeleton, physics.value, parent._bone);
  }

  /// Sets the bones, constraints, slots, and draw order to their setup pose values.
  void setToSetupPose() {
    _bindings.spine_skeleton_set_to_setup_pose(_skeleton);
  }

  /// Sets the bones and constraints to their setup pose values.
  void setBonesToSetupPose() {
    _bindings.spine_skeleton_set_bones_to_setup_pose(_skeleton);
  }

  /// Sets the slots and draw order to their setup pose values.
  void setSlotsToSetupPose() {
    _bindings.spine_skeleton_set_slots_to_setup_pose(_skeleton);
  }

  /// Finds a bone by comparing each bone's name. It is more efficient to cache the results of this method than to call it
  /// repeatedly.
  Bone? findBone(String boneName) {
    final nameNative = boneName.toNativeUtf8(allocator: _allocator);
    final bone = _bindings.spine_skeleton_find_bone(_skeleton, nameNative.cast());
    _allocator.free(nameNative);
    if (bone.address == nullptr.address) return null;
    return Bone._(bone);
  }

  /// Finds a slot by comparing each slot's name. It is more efficient to cache the results of this method than to call it
  /// repeatedly.
  Slot? findSlot(String slotName) {
    final nameNative = slotName.toNativeUtf8(allocator: _allocator);
    final slot = _bindings.spine_skeleton_find_slot(_skeleton, nameNative.cast());
    _allocator.free(nameNative);
    if (slot.address == nullptr.address) return null;
    return Slot._(slot);
  }

  /// Sets a skin by name.
  ///
  /// See [setSkin].
  void setSkinByName(String skinName) {
    final nameNative = skinName.toNativeUtf8(allocator: _allocator);
    _bindings.spine_skeleton_set_skin_by_name(_skeleton, nameNative.cast());
    _allocator.free(nameNative);
  }

  /// Sets the skin used to look up attachments before looking in the default skin (see [SkeletonData.getDefaultSkin]). If the
  /// skin is changed, [updateCache] is called.
  ///
  /// Attachments from the new skin are attached if the corresponding attachment from the old skin was attached. If there was no
  /// old skin, each slot's setup mode attachment is attached from the new skin.
  ///
  /// After changing the skin, the visible attachments can be reset to those attached in the setup pose by calling
  /// [setSlotsToSetupPose]. Also, often [AnimationState.apply] is called before the next time the
  /// skeleton is rendered to allow any attachment keys in the current animation(s) to hide or show attachments from the new
  /// skin.
  void setSkin(Skin skin) {
    _bindings.spine_skeleton_set_skin(_skeleton, skin._skin);
  }

  /// Finds an attachment by looking in the currently set skin (see [getSkin]) and default skin (see [SkeletonData.getDefaultSkin]) using
  /// the slot name and attachment name.
  ///
  /// See [getAttachment].
  Attachment? getAttachmentByName(String slotName, String attachmentName) {
    final slotNameNative = slotName.toNativeUtf8(allocator: _allocator);
    final attachmentNameNative = attachmentName.toNativeUtf8(allocator: _allocator);
    final attachment = _bindings.spine_skeleton_get_attachment_by_name(_skeleton, slotNameNative.cast(), attachmentNameNative.cast());
    _allocator.free(slotNameNative);
    _allocator.free(attachmentNameNative);
    if (attachment.address == nullptr.address) return null;
    return Attachment._toSubclass(attachment);
  }

  /// Finds an attachment by looking in the currently set skin (see [getSkin]) and default skin (see [SkeletonData.getDefaultSkin]) using the
  /// slot index and attachment name. First the skin is checked and if the attachment was not found, the default skin is checked.
  ///
  /// See [Runtime skins](http://esotericsoftware.com/spine-runtime-skins) in the Spine Runtimes Guide.
  Attachment? getAttachment(int slotIndex, String attachmentName) {
    final attachmentNameNative = attachmentName.toNativeUtf8(allocator: _allocator);
    final attachment = _bindings.spine_skeleton_get_attachment(_skeleton, slotIndex, attachmentNameNative.cast());
    _allocator.free(attachmentNameNative);
    if (attachment.address == nullptr.address) return null;
    return Attachment._toSubclass(attachment);
  }

  /// A convenience method to set an attachment by finding the slot with [findSlot], finding the attachment with
  /// [getAttachment], then setting the slot's attachment. The [attachmentName] may be an empty string to clear the slot's attachment.
  void setAttachment(String slotName, String attachmentName) {
    final slotNameNative = slotName.toNativeUtf8(allocator: _allocator);
    final attachmentNameNative = attachmentName.toNativeUtf8(allocator: _allocator);
    _bindings.spine_skeleton_set_attachment(_skeleton, slotNameNative.cast(), attachmentNameNative.cast());
    _allocator.free(slotNameNative);
    _allocator.free(attachmentNameNative);
  }

  /// Finds an IK constraint by comparing each IK constraint's name. It is more efficient to cache the results of this method
  /// than to call it repeatedly.
  IkConstraint? findIkConstraint(String constraintName) {
    final nameNative = constraintName.toNativeUtf8(allocator: _allocator);
    final constraint = _bindings.spine_skeleton_find_ik_constraint(_skeleton, nameNative.cast());
    _allocator.free(nameNative);
    if (constraint.address == nullptr.address) return null;
    return IkConstraint._(constraint);
  }

  /// Finds a transform constraint by comparing each transform constraint's name. It is more efficient to cache the results of
  /// this method than to call it repeatedly.
  TransformConstraint? findTransformConstraint(String constraintName) {
    final nameNative = constraintName.toNativeUtf8(allocator: _allocator);
    final constraint = _bindings.spine_skeleton_find_transform_constraint(_skeleton, nameNative.cast());
    _allocator.free(nameNative);
    if (constraint.address == nullptr.address) return null;
    return TransformConstraint._(constraint);
  }

  /// Finds a path constraint by comparing each path constraint's name. It is more efficient to cache the results of this method
  /// than to call it repeatedly.
  PathConstraint? findPathConstraint(String constraintName) {
    final nameNative = constraintName.toNativeUtf8(allocator: _allocator);
    final constraint = _bindings.spine_skeleton_find_path_constraint(_skeleton, nameNative.cast());
    _allocator.free(nameNative);
    if (constraint.address == nullptr.address) return null;
    return PathConstraint._(constraint);
  }

  /// Returns the axis aligned bounding box (AABB) of the region and mesh attachments for the current pose.
  Bounds getBounds() {
    final nativeBounds = _bindings.spine_skeleton_get_bounds(_skeleton);
    final bounds = Bounds(_bindings.spine_bounds_get_x(nativeBounds), _bindings.spine_bounds_get_y(nativeBounds),
        _bindings.spine_bounds_get_width(nativeBounds), _bindings.spine_bounds_get_height(nativeBounds));
    return bounds;
  }

  /// Returns the root bone, or null if the skeleton has no bones.
  Bone? getRootBone() {
    final bone = _bindings.spine_skeleton_get_root_bone(_skeleton);
    if (bone.address == nullptr.address) return null;
    return Bone._(bone);
  }

  /// The skeleton's setup pose data.
  SkeletonData? getData() {
    final data = _bindings.spine_skeleton_get_data(_skeleton);
    if (data.address == nullptr.address) return null;
    return SkeletonData._(data);
  }

  /// The skeleton's bones, sorted parent first. The root bone is always the first bone.
  List<Bone> getBones() {
    final List<Bone> bones = [];
    final numBones = _bindings.spine_skeleton_get_num_bones(_skeleton);
    final nativeBones = _bindings.spine_skeleton_get_bones(_skeleton);
    for (int i = 0; i < numBones; i++) {
      bones.add(Bone._(nativeBones[i]));
    }
    return bones;
  }

  /// The skeleton's slots.
  List<Slot> getSlots() {
    final List<Slot> slots = [];
    final numSlots = _bindings.spine_skeleton_get_num_slots(_skeleton);
    final nativeSlots = _bindings.spine_skeleton_get_slots(_skeleton);
    for (int i = 0; i < numSlots; i++) {
      slots.add(Slot._(nativeSlots[i]));
    }
    return slots;
  }

  /// The skeleton's slots in the order they should be drawn. The returned array may be modified to change the draw order.
  List<Slot> getDrawOrder() {
    final List<Slot> slots = [];
    final numSlots = _bindings.spine_skeleton_get_num_draw_order(_skeleton);
    final nativeDrawOrder = _bindings.spine_skeleton_get_draw_order(_skeleton);
    for (int i = 0; i < numSlots; i++) {
      slots.add(Slot._(nativeDrawOrder[i]));
    }
    return slots;
  }

  /// The skeleton's IK constraints.
  List<IkConstraint> getIkConstraints() {
    final List<IkConstraint> constraints = [];
    final numConstraints = _bindings.spine_skeleton_get_num_ik_constraints(_skeleton);
    final nativeConstraints = _bindings.spine_skeleton_get_ik_constraints(_skeleton);
    for (int i = 0; i < numConstraints; i++) {
      constraints.add(IkConstraint._(nativeConstraints[i]));
    }
    return constraints;
  }

  /// The skeleton's path constraints.
  List<PathConstraint> getPathConstraints() {
    final List<PathConstraint> constraints = [];
    final numConstraints = _bindings.spine_skeleton_get_num_path_constraints(_skeleton);
    final nativeConstraints = _bindings.spine_skeleton_get_path_constraints(_skeleton);
    for (int i = 0; i < numConstraints; i++) {
      constraints.add(PathConstraint._(nativeConstraints[i]));
    }
    return constraints;
  }

  /// The skeleton's transform constraints.
  List<TransformConstraint> getTransformConstraints() {
    final List<TransformConstraint> constraints = [];
    final numConstraints = _bindings.spine_skeleton_get_num_transform_constraints(_skeleton);
    final nativeConstraints = _bindings.spine_skeleton_get_transform_constraints(_skeleton);
    for (int i = 0; i < numConstraints; i++) {
      constraints.add(TransformConstraint._(nativeConstraints[i]));
    }
    return constraints;
  }

  /// The skeleton's current skin.
  Skin? getSkin() {
    final skin = _bindings.spine_skeleton_get_skin(_skeleton);
    if (skin.address == nullptr.address) return null;
    return Skin._(skin);
  }

  /// The color to tint all the skeleton's attachments.
  Color getColor() {
    final color = _bindings.spine_skeleton_get_color(_skeleton);
    return Color(_bindings.spine_color_get_r(color), _bindings.spine_color_get_g(color), _bindings.spine_color_get_b(color),
        _bindings.spine_color_get_a(color));
  }

  void setColor(Color color) {
    _bindings.spine_skeleton_set_color(_skeleton, color.r, color.g, color.b, color.a);
  }

  /// Sets the skeleton X and Y position, which is added to the root bone worldX and worldY position.
  ///
  /// Bones that do not inherit translation are still affected by this property.
  void setPosition(double x, double y) {
    _bindings.spine_skeleton_set_position(_skeleton, x, y);
  }

  Vec2 getPosition() {
    final x = _bindings.spine_skeleton_get_x(_skeleton);
    final y = _bindings.spine_skeleton_get_y(_skeleton);
    return Vec2(x, y);
  }

  /// Sets the skeleton X position, which is added to the root bone worldX position.
  ///
  /// Bones that do not inherit translation are still affected by this property.
  double getX() {
    return _bindings.spine_skeleton_get_x(_skeleton);
  }

  void setX(double x) {
    _bindings.spine_skeleton_set_x(_skeleton, x);
  }

  /// Sets the skeleton Y position, which is added to the root bone worldY position.
  /// <p>
  /// Bones that do not inherit translation are still affected by this property.
  double getY() {
    return _bindings.spine_skeleton_get_y(_skeleton);
  }

  void setY(double y) {
    _bindings.spine_skeleton_set_y(_skeleton, y);
  }

  /// Scales the entire skeleton on the X axis.
  ///
  /// Bones that do not inherit scale are still affected by this property.
  double getScaleX() {
    return _bindings.spine_skeleton_get_scale_x(_skeleton);
  }

  void setScaleX(double scaleX) {
    _bindings.spine_skeleton_set_scale_x(_skeleton, scaleX);
  }

  /// Scales the entire skeleton on the Y axis.
  ///
  /// Bones that do not inherit scale are still affected by this property.
  double getScaleY() {
    return _bindings.spine_skeleton_get_scale_y(_skeleton);
  }

  void setScaleY(double scaleY) {
    _bindings.spine_skeleton_set_scale_y(_skeleton, scaleY);
  }

  double getTime() {
    return _bindings.spine_skeleton_get_time(_skeleton);
  }

  void setTime(double time) {
    return _bindings.spine_skeleton_set_time(_skeleton, time);
  }

  void update(double delta) {
    _bindings.spine_skeleton_update(_skeleton, delta);
  }
}

/// Stores a list of timelines to animate a skeleton's pose over time.
class Animation {
  final spine_animation _animation;

  Animation._(this._animation);

  /// The animation's name, which is unique across all animations in the skeleton.
  String getName() {
    final Pointer<Utf8> value = _bindings.spine_animation_get_name(_animation).cast();
    return value.toDartString();
  }

  /// The duration of the animation in seconds, which is usually the highest time of all frames in the timeline. The duration is
  /// used to know when it has completed and when it should loop back to the start.
  double getDuration() {
    return _bindings.spine_animation_get_duration(_animation);
  }
}

/// Controls how timeline values are mixed with setup pose values or current pose values when a timeline is applied with
/// <code>alpha</code> < 1.
enum MixBlend {
  /// Transitions from the setup value to the timeline value (the current value is not used). Before the first frame, the
  /// setup value is set.
  setup(0),

  /// Transitions from the current value to the timeline value. Before the first frame, transitions from the current value to
  /// the setup value. Timelines which perform instant transitions, such as {@link DrawOrderTimeline} or
  /// {@link AttachmentTimeline}, use the setup value before the first frame.
  /// <p>
  /// <code>first</code> is intended for the first animations applied, not for animations layered on top of those.
  first(1),

  /// Transitions from the current value to the timeline value. No change is made before the first frame (the current value is
  /// kept until the first frame).
  /// <p>
  /// <code>replace</code> is intended for animations layered on top of others, not for the first animations applied.
  replace(2),

  /// Transitions from the current value to the current value plus the timeline value. No change is made before the first
  /// frame (the current value is kept until the first frame).
  /// <p>
  /// <code>add</code> is intended for animations layered on top of others, not for the first animations applied. Properties
  /// set by additive animations must be set manually or by another animation before applying the additive animations, else the
  /// property values will increase each time the additive animations are applied.
  add(3);

  final int value;

  const MixBlend(this.value);
}

/// Stores settings and other state for the playback of an animation on an [AnimationState] track.
///
/// References to a track entry must not be kept after the dispose [EventType] is reported to [AnimationStateListener].
class TrackEntry {
  final spine_track_entry _entry;
  final AnimationState _state;

  TrackEntry._(this._entry, this._state);

  /// The index of the track where this track entry is either current or queued.
  ///
  /// See [AnimationState.getCurrent].
  int getTtrackIndex() {
    return _bindings.spine_track_entry_get_track_index(_entry);
  }

  /// The animation to apply for this track entry.
  Animation getAnimation() {
    return Animation._(_bindings.spine_track_entry_get_animation(_entry));
  }

  /// If true, the animation will repeat. If false it will not, instead its last frame is applied if played beyond its
  /// duration.
  bool getLoop() {
    return _bindings.spine_track_entry_get_loop(_entry) == -1;
  }

  void setLoop(bool loop) {
    _bindings.spine_track_entry_set_loop(_entry, loop ? -1 : 0);
  }

  /// Seconds to postpone playing the animation. When this track entry is the current track entry, <code>delay</code>
  /// postpones incrementing the [getTrackTime]. When this track entry is queued, <code>delay</code> is the time from
  /// the start of the previous animation to when this track entry will become the current track entry (ie when the previous
  /// track entry [getTrackTime] >= this track entry's <code>delay</code>).
  ///
  /// [getTimeScale] affects the delay.
  ///
  /// When using [AnimationState.addAnimation] with a <code>delay</code> <= 0, the delay
  /// is set using the mix duration from the [AnimationStateData]. If [getMixDuration] is set afterward, the delay
  /// may need to be adjusted.
  bool getHoldPrevious() {
    return _bindings.spine_track_entry_get_hold_previous(_entry) == -1;
  }

  void setHoldPrevious(bool holdPrevious) {
    _bindings.spine_track_entry_set_hold_previous(_entry, holdPrevious ? -1 : 0);
  }

  /// If true, the animation will be applied in reverse. Events are not fired when an animation is applied in reverse.
  bool getReverse() {
    return _bindings.spine_track_entry_get_reverse(_entry) == -1;
  }

  void setReverse(bool reverse) {
    _bindings.spine_track_entry_set_reverse(_entry, reverse ? -1 : 0);
  }

  /// If true, mixing rotation between tracks always uses the shortest rotation direction. If the rotation is animated, the
  /// shortest rotation direction may change during the mix.
  ///
  /// If false, the shortest rotation direction is remembered when the mix starts and the same direction is used for the rest
  /// of the mix. Defaults to false.
  bool getShortestRotation() {
    return _bindings.spine_track_entry_get_shortest_rotation(_entry) == 1;
  }

  void setShortestRotation(bool shortestRotation) {
    _bindings.spine_track_entry_set_shortest_rotation(_entry, shortestRotation ? -1 : 0);
  }

  /// Seconds to postpone playing the animation. When this track entry is the current track entry, <code>delay</code>
  /// postpones incrementing the [getTrackTime]. When this track entry is queued, <code>delay</code> is the time from
  /// the start of the previous animation to when this track entry will become the current track entry (ie when the previous
  /// track entry [getTrackTime] >= this track entry's <code>delay</code>).
  ///
  /// [getTimeScale] affects the delay.
  ///
  /// When using [AnimationState.addAnimation] with a <code>delay</code> <= 0, the delay
  /// is set using the mix duration from the [AnimationStateData]. If [getMixDuration] is set afterward, the delay
  /// may need to be adjusted.
  double getDelay() {
    return _bindings.spine_track_entry_get_delay(_entry);
  }

  void setDelay(double delay) {
    _bindings.spine_track_entry_set_delay(_entry, delay);
  }

  /// Current time in seconds this track entry has been the current track entry. The track time determines
  /// [getAnimationTime]. The track time can be set to start the animation at a time other than 0, without affecting
  /// looping.
  double getTrackTime() {
    return _bindings.spine_track_entry_get_track_time(_entry);
  }

  void setTrackTime(double trackTime) {
    _bindings.spine_track_entry_set_track_time(_entry, trackTime);
  }

  /// The track time in seconds when this animation will be removed from the track. Defaults to the highest possible float
  /// value, meaning the animation will be applied until a new animation is set or the track is cleared. If the track end time
  /// is reached, no other animations are queued for playback, and mixing from any previous animations is complete, then the
  /// properties keyed by the animation are set to the setup pose and the track is cleared.
  ///
  /// It may be desired to use [AnimationState.addEmptyAnimation] rather than have the animation
  /// abruptly cease being applied.
  double getTrackEnd() {
    return _bindings.spine_track_entry_get_track_end(_entry);
  }

  void setTrackEnd(double trackEnd) {
    _bindings.spine_track_entry_set_track_end(_entry, trackEnd);
  }

  /// Seconds when this animation starts, both initially and after looping. Defaults to 0.
  ///
  /// When changing the <code>animationStart</code> time, it often makes sense to set [getAnimationLast] to the same
  /// value to prevent timeline keys before the start time from triggering.
  double getAnimationStart() {
    return _bindings.spine_track_entry_get_animation_start(_entry);
  }

  void setAnimationStart(double animationStart) {
    _bindings.spine_track_entry_set_animation_start(_entry, animationStart);
  }

  /// Seconds for the last frame of this animation. Non-looping animations won't play past this time. Looping animations will
  /// loop back to [getAnimationStart] at this time. Defaults to the animation [Animation.getDuration].
  double getAnimationEnd() {
    return _bindings.spine_track_entry_get_animation_end(_entry);
  }

  void setAnimationEnd(double animationEnd) {
    _bindings.spine_track_entry_set_animation_end(_entry, animationEnd);
  }

  /// The time in seconds this animation was last applied. Some timelines use this for one-time triggers. Eg, when this
  /// animation is applied, event timelines will fire all events between the <code>animationLast</code> time (exclusive) and
  /// <code>animationTime</code> (inclusive). Defaults to -1 to ensure triggers on frame 0 happen the first time this animation
  /// is applied.
  double getAnimationLast() {
    return _bindings.spine_track_entry_get_animation_last(_entry);
  }

  void setAnimationLast(double animationLast) {
    _bindings.spine_track_entry_set_animation_last(_entry, animationLast);
  }

  /// Uses [getTrackTime] to compute the <code>animationTime</code>. When the <code>trackTime</code> is 0, the
  /// <code>animationTime</code> is equal to the <code>animationStart</code> time.
  /// <p>
  /// The <code>animationTime</code> is between [getAnimationStart] and [getAnimationEnd], except if this
  /// track entry is non-looping and [getAnimationEnd] is >= to the animation [Animation.getDuration], then
  /// <code>animationTime</code> continues to increase past [getAnimationEnd].
  double getAnimationTime() {
    return _bindings.spine_track_entry_get_animation_time(_entry);
  }

  /// Multiplier for the delta time when this track entry is updated, causing time for this animation to pass slower or
  /// faster. Defaults to 1.
  ///
  /// Values < 0 are not supported. To play an animation in reverse, use [getReverse].
  ///
  /// [getMixTime] is not affected by track entry time scale, so [getMixDuration] may need to be adjusted to
  /// match the animation speed.
  ///
  /// When using [AnimationState.addAnimation] with a <code>delay</code> <= 0, the
  /// [getDelay] is set using the mix duration from the [AnimationStateData], assuming time scale to be 1. If
  /// the time scale is not 1, the delay may need to be adjusted.
  ///
  /// See [AnimationState.getTimeScale] for affecting all animations.
  double getTimeScale() {
    return _bindings.spine_track_entry_get_time_scale(_entry);
  }

  void setTimeScale(double timeScale) {
    _bindings.spine_track_entry_set_time_scale(_entry, timeScale);
  }

  /// Values < 1 mix this animation with the skeleton's current pose (usually the pose resulting from lower tracks). Defaults
  /// to 1, which overwrites the skeleton's current pose with this animation.
  ///
  /// Typically track 0 is used to completely pose the skeleton, then alpha is used on higher tracks. It doesn't make sense to
  /// use alpha on track 0 if the skeleton pose is from the last frame render.
  Future<double> getAlpha() async {
    return _bindings.spine_track_entry_get_alpha(_entry);
  }

  void setAlpha(double alpha) {
    _bindings.spine_track_entry_set_alpha(_entry, alpha);
  }

  /// When the mix percentage ([getMixTime] / [getMixDuration]) is less than the
  /// <code>eventThreshold</code>, event timelines are applied while this animation is being mixed out. Defaults to 0, so event
  /// timelines are not applied while this animation is being mixed out.
  double getEventThreshold() {
    return _bindings.spine_track_entry_get_event_threshold(_entry);
  }

  void setEventThreshold(double eventThreshold) {
    _bindings.spine_track_entry_set_event_threshold(_entry, eventThreshold);
  }

  /// Values less than 1 mix this animation with the last skeleton pose. Defaults to 1, which overwrites the last skeleton pose with
  /// this animation.
  ///
  /// Typically track 0 is used to completely pose the skeleton, then alpha can be used on higher tracks. It doesn't make sense
  /// to use alpha on track 0 if the skeleton pose is from the last frame render.
  double getAlphaAttachmentThreshold() {
    return _bindings.spine_track_entry_get_alpha_attachment_threshold(_entry);
  }

  void setAlphaAttachmentThreshold(double attachmentThreshold) {
    _bindings.spine_track_entry_set_alpha_attachment_threshold(_entry, attachmentThreshold);
  }

  /// When the mix percentage ([getMixTime] / [getMixDuration]) is less than the
  /// <code>attachmentThreshold</code>, attachment timelines are applied while this animation is being mixed out. Defaults to
  /// 0, so attachment timelines are not applied while this animation is being mixed out.
  double getMixAttachmentThreshold() {
    return _bindings.spine_track_entry_get_mix_attachment_threshold(_entry);
  }

  void setMixAttachmentThreshold(double attachmentThreshold) {
    _bindings.spine_track_entry_set_mix_attachment_threshold(_entry, attachmentThreshold);
  }

  /// When the mix percentage ([getMixTime] / [getMixDuration]) is less than the
  /// <code>drawOrderThreshold</code>, draw order timelines are applied while this animation is being mixed out. Defaults to 0,
  /// so draw order timelines are not applied while this animation is being mixed out.
  double getMixDrawOrderThreshold() {
    return _bindings.spine_track_entry_get_mix_draw_order_threshold(_entry);
  }

  void setMixDrawOrderThreshold(double drawOrderThreshold) {
    _bindings.spine_track_entry_set_mix_draw_order_threshold(_entry, drawOrderThreshold);
  }

  /// The animation queued to start after this animation, or null if there is none. <code>next</code> makes up a doubly linked
  /// list.
  ///
  /// See [AnimationState.clearNext] to truncate the list.
  TrackEntry? getNext() {
    final next = _bindings.spine_track_entry_get_next(_entry);
    if (next.address == nullptr.address) return null;
    return TrackEntry._(next, _state);
  }

  /// Returns true if at least one loop has been completed.
  bool isComplete() {
    return _bindings.spine_track_entry_is_complete(_entry) == -1;
  }

  /// Seconds from 0 to the [getMixDuration] when mixing from the previous animation to this animation. May be
  /// slightly more than <code>mixDuration</code> when the mix is complete.
  double getMixTime() {
    return _bindings.spine_track_entry_get_mix_time(_entry);
  }

  void setMixTime(double mixTime) {
    _bindings.spine_track_entry_set_mix_time(_entry, mixTime);
  }

  /// Seconds for mixing from the previous animation to this animation. Defaults to the value provided by
  /// [AnimationStateData.getMix] based on the animation before this animation (if any).
  ///
  /// A mix duration of 0 still mixes out over one frame to provide the track entry being mixed out a chance to revert the
  /// properties it was animating. A mix duration of 0 can be set at any time to end the mix on the next
  /// [AnimationState.update].
  ///
  /// The <code>mixDuration</code> can be set manually rather than use the value from
  /// [AnimationStateData.getMix]. In that case, the <code>mixDuration</code> can be set for a new
  /// track entry only before [AnimationState.update] is first called.
  /// <p>
  /// When using [AnimationState.addAnimation] with a <code>delay</code> <= 0, the
  /// [getDelay] is set using the mix duration from the [AnimationStateData]. If <code>mixDuration</code> is set
  /// afterward, the delay may need to be adjusted. For example:
  /// <code>entry.delay = entry.previous.getTrackComplete() - entry.mixDuration;</code>
  double getMixDuration() {
    return _bindings.spine_track_entry_get_mix_duration(_entry);
  }

  void setMixDuration(double mixDuration) {
    _bindings.spine_track_entry_set_mix_duration(_entry, mixDuration);
  }

  /// Controls how properties keyed in the animation are mixed with lower tracks. Defaults to [MixBlend.replace].
  ///
  /// Track entries on track 0 ignore this setting and always use {@link MixBlend#first}.
  ///
  /// The <code>mixBlend</code> can be set for a new track entry only before [AnimationState.apply] is first
  /// called.
  MixBlend getMixBlend() {
    return MixBlend.values[_bindings.spine_track_entry_get_mix_blend(_entry)];
  }

  void setMixBlend(MixBlend mixBlend) {
    _bindings.spine_track_entry_set_mix_blend(_entry, mixBlend.value);
  }

  /// The track entry for the previous animation when mixing from the previous animation to this animation, or null if no
  /// mixing is currently occurring. When mixing from multiple animations, <code>mixingFrom</code> makes up a linked list.
  TrackEntry? getMixingFrom() {
    final from = _bindings.spine_track_entry_get_mixing_from(_entry);
    if (from.address == nullptr.address) return null;
    return TrackEntry._(from, _state);
  }

  /// The track entry for the next animation when mixing from this animation to the next animation, or null if no mixing is
  /// currently occurring. When mixing to multiple animations, <code>mixingTo</code> makes up a linked list.
  TrackEntry? getMixingTo() {
    final to = _bindings.spine_track_entry_get_mixing_to(_entry);
    if (to.address == nullptr.address) return null;
    return TrackEntry._(to, _state);
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

  /// If this track entry is non-looping, the track time in seconds when [getAnimationEnd] is reached, or the current
  /// [getTrackTime] if it has already been reached. If this track entry is looping, the track time when this
  /// animation will reach its next [getAnimationEnd] (the next loop completion).
  double getTrackComplete() {
    return _bindings.spine_track_entry_get_track_complete(_entry);
  }

  /// The listener for events generated by this track entry, or null.
  ///
  /// A track entry returned from [AnimationState.setAnimation] is already the current animation
  /// for the track, so the track entry listener will not be called for [EventType.start].
  void setListener(AnimationStateListener? listener) {
    _state._setTrackEntryListener(_entry, listener);
  }
}

/// The event type passed to [AnimationStateListener]
enum EventType {
  /// Emitted when [TrackEntry] has been set as the current entry. [EventType.end] will occur when this entry will no
  /// longer be applied.
  start,

  /// Emitted when another entry has replaced the current entry. This entry may continue being applied for
  /// mixing.
  interrupt,

  /// Emitted when this entry will never be applied again. This only occurs if this entry has previously been set as the
  /// current entry ([EventType.start] was emitted).
  end,

  /// Emitted every time the current entry's animation completes a loop. This may occur during mixing (after
  /// [EventType.interrupted] is emitted).
  ///
  /// If [TrackEntry.getMixingTo] of the entry reported by the event is not null, the entry is mixing out (it is not the current entry).
  ///
  /// Because this event is triggered at the end of [AnimationState.apply], any animations set in response to
  /// the event won't be applied until the next time the [AnimationState] is applied.
  complete,

  /// Emitted when this entry will be disposed. This may occur without the entry ever being set as the current entry.
  ///
  /// References to the entry should not be kept after <code>dispose</code> is called, as it may be destroyed or reused.
  dispose,

  /// Invoked when the current entry's animation triggers an event. This may occur during mixing (after
  /// [EventType.interrupt] is emitted), see [TrackEntry.getEventThreshold].
  ///
  /// Because this event is triggered at the end of [AnimationState.apply], any animations set in response to
  /// the event won't be applied until the next time the [AnimationState] is applied.
  event
}

/// Stores the setup pose values for an [Event].
///
/// See <a href="http://esotericsoftware.com/spine-events">Events</a> in the Spine User Guide.
class EventData {
  final spine_event_data _data;

  EventData._(this._data);

  /// The name of the event, which is unique across all events in the skeleton.
  String getName() {
    final Pointer<Utf8> value = _bindings.spine_event_data_get_name(_data).cast();
    return value.toDartString();
  }

  int getIntValue() {
    return _bindings.spine_event_data_get_int_value(_data);
  }

  void setIntValue(int value) {
    _bindings.spine_event_data_set_int_value(_data, value);
  }

  double getFloatValue() {
    return _bindings.spine_event_data_get_float_value(_data);
  }

  void setFloatValue(double value) {
    _bindings.spine_event_data_set_float_value(_data, value);
  }

  String getStringValue() {
    final Pointer<Utf8> value = _bindings.spine_event_data_get_string_value(_data).cast();
    return value.toDartString();
  }

  void setStringValue(String value) {
    final nativeString = value.toNativeUtf8(allocator: _allocator);
    _bindings.spine_event_data_set_string_value(_data, nativeString.cast());
    _allocator.free(nativeString);
  }

  String getAudioPath() {
    final Pointer<Utf8> value = _bindings.spine_event_data_get_audio_path(_data).cast();
    return value.toDartString();
  }

  double getVolume() {
    return _bindings.spine_event_data_get_volume(_data);
  }

  void setVolume(double volume) {
    _bindings.spine_event_data_set_volume(_data, volume);
  }

  double getBalance() {
    return _bindings.spine_event_data_get_balance(_data);
  }

  void setBalance(double value) {
    _bindings.spine_event_data_set_balance(_data, value);
  }
}

/// Stores the current pose values for an {@link Event}.
///
/// See [AnimationStateListener], [EventType.event], and
/// <a href="http://esotericsoftware.com/spine-events">Events</a> in the Spine User Guide.
class Event {
  final spine_event _event;

  Event._(this._event);

  /// The events's setup pose data.
  EventData getData() {
    return EventData._(_bindings.spine_event_get_data(_event));
  }

  /// The animation time this event was keyed.
  double getTime() {
    return _bindings.spine_event_get_time(_event);
  }

  int getIntValue() {
    return _bindings.spine_event_get_int_value(_event);
  }

  void setIntValue(int value) {
    _bindings.spine_event_set_int_value(_event, value);
  }

  double getFloatValue() {
    return _bindings.spine_event_get_float_value(_event);
  }

  void setFloatValue(double value) {
    _bindings.spine_event_set_float_value(_event, value);
  }

  String getStringValue() {
    final Pointer<Utf8> value = _bindings.spine_event_get_string_value(_event).cast();
    return value.toDartString();
  }

  void setStringValue(String value) {
    final nativeString = value.toNativeUtf8(allocator: _allocator);
    _bindings.spine_event_set_string_value(_event, nativeString.cast());
    _allocator.free(nativeString);
  }

  double getVolume() {
    return _bindings.spine_event_get_volume(_event);
  }

  void setVolume(double volume) {
    _bindings.spine_event_set_volume(_event, volume);
  }

  double getBalance() {
    return _bindings.spine_event_get_balance(_event);
  }

  void setBalance(double balance) {
    _bindings.spine_event_set_balance(_event, balance);
  }
}

/// The callback to implement for receiving [TrackEntry] events. It is always safe to call [AnimationState] methods when receiving
/// events.
///
/// TrackEntry events are collected during [AnimationState.update] and [AnimationState.apply] and
/// fired only after those methods are finished.
///
/// See [TrackEntry.setListener] and [AnimationState.setListener].
typedef AnimationStateListener = void Function(EventType type, TrackEntry entry, Event? event);

/// Stores mix (crossfade) durations to be applied when {@link AnimationState} animations are changed.
class AnimationStateData {
  final spine_animation_state_data _data;

  AnimationStateData._(this._data);

  /// The SkeletonData to look up animations when they are specified by name.
  SkeletonData getSkeletonData() {
    return SkeletonData._(_bindings.spine_animation_state_data_get_skeleton_data(_data));
  }

  double getDefaultMix() {
    return _bindings.spine_animation_state_data_get_default_mix(_data);
  }

  void setDefaultMix(double defaultMix) {
    _bindings.spine_animation_state_data_set_default_mix(_data, defaultMix);
  }

  /// Sets a mix duration by animation name.
  ///
  /// See [setMix].
  void setMixByName(String fromName, String toName, double duration) {
    final fromNative = fromName.toNativeUtf8(allocator: _allocator);
    final toNative = toName.toNativeUtf8(allocator: _allocator);
    _bindings.spine_animation_state_data_set_mix_by_name(_data, fromNative.cast(), toNative.cast(), duration);
    _allocator.free(fromNative);
    _allocator.free(toNative);
  }

  /// Returns the mix duration to use when changing from the specified animation to the other, or the [getDefaultMix] if
  /// no mix duration has been set.
  double getMixByName(String fromName, String toName) {
    final fromNative = fromName.toNativeUtf8(allocator: _allocator);
    final toNative = toName.toNativeUtf8(allocator: _allocator);
    final duration = _bindings.spine_animation_state_data_get_mix_by_name(_data, fromNative.cast(), toNative.cast());
    _allocator.free(fromNative);
    _allocator.free(toNative);
    return duration;
  }

  /// Sets the mix duration when changing from the specified animation to the other.
  ///
  /// See [TrackEntry.mixDuration].
  Future<void> setMix(Animation from, Animation to, double duration) async {
    _bindings.spine_animation_state_data_set_mix(_data, from._animation, to._animation, duration);
  }

  /// Returns the mix duration to use when changing from the specified animation to the other, or the [getDefaultMix] if
  /// no mix duration has been set.
  double getMix(Animation from, Animation to) {
    return _bindings.spine_animation_state_data_get_mix(_data, from._animation, to._animation);
  }

  /// Removes all mix durations.
  void clear() {
    _bindings.spine_animation_state_data_clear(_data);
  }
}

/// Applies animations over time, queues animations for later playback, mixes (crossfading) between animations, and applies
/// multiple animations on top of each other (layering).
///
/// See <a href='http://esotericsoftware.com/spine-applying-animations/'>Applying Animations</a> in the Spine Runtimes Guide.
class AnimationState {
  final spine_animation_state _state;
  final spine_animation_state_events _events;
  final Map<spine_track_entry, AnimationStateListener> _trackEntryListeners;
  AnimationStateListener? _stateListener;

  AnimationState._(this._state, this._events) : _trackEntryListeners = {};

  void _setTrackEntryListener(spine_track_entry entry, AnimationStateListener? listener) {
    if (listener == null) {
      _trackEntryListeners.remove(entry);
    } else {
      _trackEntryListeners[entry] = listener;
    }
  }

  /// Increments each track entry [TrackEntry.getTrackTime], setting queued animations as current if needed.
  void update(double delta) {
    _bindings.spine_animation_state_update(_state, delta);

    final numEvents = _bindings.spine_animation_state_events_get_num_events(_events);
    if (numEvents > 0) {
      for (int i = 0; i < numEvents; i++) {
        late final EventType type;
        switch (_bindings.spine_animation_state_events_get_event_type(_events, i)) {
          case 0:
            type = EventType.start;
            break;
          case 1:
            type = EventType.interrupt;
            break;
          case 2:
            type = EventType.end;
            break;
          case 3:
            type = EventType.complete;
            break;
          case 4:
            type = EventType.dispose;
            break;
          case 5:
            type = EventType.event;
            break;
        }
        final nativeEntry = _bindings.spine_animation_state_events_get_track_entry(_events, i);
        final entry = TrackEntry._(nativeEntry, this);
        final nativeEvent = _bindings.spine_animation_state_events_get_event(_events, i);
        final event = nativeEvent.address == nullptr.address ? null : Event._(nativeEvent);
        if (_trackEntryListeners.containsKey(nativeEntry)) {
          _trackEntryListeners[nativeEntry]?.call(type, entry, event);
        }
        if (_stateListener != null) {
          _stateListener?.call(type, entry, event);
        }
        if (type == EventType.dispose) {
          _bindings.spine_animation_state_dispose_track_entry(_state, nativeEntry);
        }
      }
    }
    _bindings.spine_animation_state_events_reset(_events);
  }

  /// Poses the skeleton using the track entry animations. The animation state is not changed, so can be applied to multiple
  /// skeletons to pose them identically.
  ///
  /// Returns true if any animations were applied.
  void apply(Skeleton skeleton) {
    _bindings.spine_animation_state_apply(_state, skeleton._skeleton);
  }

  /// Removes all animations from all tracks, leaving skeletons in their current pose.
  ///
  /// It may be desired to use [setEmptyAnimations] to mix the skeletons back to the setup pose,
  /// rather than leaving them in their current pose.
  void clearTracks() {
    _bindings.spine_animation_state_clear_tracks(_state);
  }

  /// Removes all animations from the track, leaving skeletons in their current pose.
  ///
  /// It may be desired to use [setEmptyAnimations] to mix the skeletons back to the setup pose,
  /// rather than leaving them in their current pose.
  void clearTrack(int trackIndex) {
    _bindings.spine_animation_state_clear_track(_state, trackIndex);
  }

  /// Sets an animation by name.
  ///
  /// See [setAnimation].
  TrackEntry setAnimationByName(int trackIndex, String animationName, bool loop) {
    final animation = animationName.toNativeUtf8(allocator: _allocator);
    final entry = _bindings.spine_animation_state_set_animation_by_name(_state, trackIndex, animation.cast(), loop ? -1 : 0);
    _allocator.free(animation);
    if (entry.address == nullptr.address) throw Exception("Couldn't set animation $animationName");
    return TrackEntry._(entry, this);
  }

  /// Sets the current [animation] for a track at [trackIndex], discarding any queued animations. If the formerly current track entry was never
  /// applied to a skeleton, it is replaced (not mixed from).
  ///
  /// If [loop] is true, the animation will repeat. If false it will not, instead its last frame is applied if played beyond its
  /// duration. In either case [TrackEntry.getTrackEnd] determines when the track is cleared.
  ///
  /// Returns a track entry to allow further customization of animation playback. References to the track entry must not be kept
  /// after the [EventType.dispose] event occurs.
  TrackEntry setAnimation(int trackIndex, Animation animation, bool loop) {
    final entry = _bindings.spine_animation_state_set_animation(_state, trackIndex, animation._animation, loop ? -1 : 0);
    if (entry.address == nullptr.address) throw Exception("Couldn't set animation ${animation.getName()}");
    return TrackEntry._(entry, this);
  }

  /// Queues an animation by name.
  ///
  /// See [addAnimation].
  TrackEntry addAnimationByName(int trackIndex, String animationName, bool loop, double delay) {
    final animation = animationName.toNativeUtf8(allocator: _allocator);
    final entry = _bindings.spine_animation_state_add_animation_by_name(_state, trackIndex, animation.cast(), loop ? -1 : 0, delay);
    _allocator.free(animation);
    if (entry.address == nullptr.address) throw Exception("Couldn't add animation $animationName");
    return TrackEntry._(entry, this);
  }

  /// Adds an [animation] to be played after the current or last queued animation for a track at [trackIndex]. If the track is empty, it is
  /// equivalent to calling [setAnimation].
  ///
  /// If [delay] > 0, sets [TrackEntry.getDelay]. If [delay] <= 0, the delay set is the duration of the previous track entry
  /// minus any mix duration (from the [AnimationStateData]) plus the specified <code>delay</code> (ie the mix
  /// ends at (<code>delay</code> = 0) or before (<code>delay</code> < 0) the previous track entry duration). If the
  /// previous entry is looping, its next loop completion is used instead of its duration.
  ///
  /// Returns a track entry to allow further customization of animation playback. References to the track entry must not be kept
  /// after the [EventType.dispose] event occurs.
  TrackEntry addAnimation(int trackIndex, Animation animation, bool loop, double delay) {
    final entry = _bindings.spine_animation_state_add_animation(_state, trackIndex, animation._animation, loop ? -1 : 0, delay);
    if (entry.address == nullptr.address) throw Exception("Couldn't add animation ${animation.getName()}");
    return TrackEntry._(entry, this);
  }

  /// Sets an empty animation for a track at [trackIndex], discarding any queued animations, and sets the track entry's
  /// [TrackEntry.getMixDuration] to [mixDuration]. An empty animation has no timelines and serves as a placeholder for mixing in or out.
  ///
  /// Mixing out is done by setting an empty animation with a mix duration using either [setEmptyAnimation],
  /// [setEmptyAnimations], or [addEmptyAnimation]. Mixing to an empty animation causes
  /// the previous animation to be applied less and less over the mix duration. Properties keyed in the previous animation
  /// transition to the value from lower tracks or to the setup pose value if no lower tracks key the property. A mix duration of
  /// 0 still mixes out over one frame.
  ///
  /// Mixing in is done by first setting an empty animation, then adding an animation using
  /// [addAnimation] with the desired delay (an empty animation has a duration of 0) and on
  /// the returned track entry, set the [TrackEntry.setMixDuration]. Mixing from an empty animation causes the new
  /// animation to be applied more and more over the mix duration. Properties keyed in the new animation transition from the value
  /// from lower tracks or from the setup pose value if no lower tracks key the property to the value keyed in the new
  /// animation.
  TrackEntry setEmptyAnimation(int trackIndex, double mixDuration) {
    final entry = _bindings.spine_animation_state_set_empty_animation(_state, trackIndex, mixDuration);
    return TrackEntry._(entry, this);
  }

  /// Adds an empty animation to be played after the current or last queued animation for a track, and sets the track entry's
  /// [TrackEntry.getMixDuration]. If the track is empty, it is equivalent to calling
  /// [setEmptyAnimation].
  ///
  /// See [setEmptyAnimation].
  ///
  /// If [delay] > 0, sets [TrackEntry.getDelay]. If <= 0, the delay set is the duration of the previous track entry
  /// minus any mix duration plus the specified <code>delay</code> (ie the mix ends at (<code>delay</code> = 0) or
  /// before (<code>delay</code> < 0) the previous track entry duration). If the previous entry is looping, its next
  /// loop completion is used instead of its duration.
  ///
  /// Returns a track entry to allow further customization of animation playback. References to the track entry must not be kept
  /// after the [EventType.dispose] event occurs.
  TrackEntry addEmptyAnimation(int trackIndex, double mixDuration, double delay) {
    final entry = _bindings.spine_animation_state_add_empty_animation(_state, trackIndex, mixDuration, delay);
    return TrackEntry._(entry, this);
  }

  /// Returns the track entry for the animation currently playing on the track, or null if no animation is currently playing.
  TrackEntry? getCurrent(int trackIndex) {
    final entry = _bindings.spine_animation_state_get_current(_state, trackIndex);
    if (entry.address == nullptr.address) return null;
    return TrackEntry._(entry, this);
  }

  /// Returns the number of tracks that have animations queued.
  int getNumTracks() {
    return _bindings.spine_animation_state_get_num_tracks(_state);
  }

  /// Sets an empty animation for every track, discarding any queued animations, and mixes to it over the specified mix
  /// duration.
  void setEmptyAnimations(double mixDuration) {
    _bindings.spine_animation_state_set_empty_animations(_state, mixDuration);
  }

  /// Multiplier for the delta time when the animation state is updated, causing time for all animations and mixes to play slower
  /// or faster. Defaults to 1.
  ///
  /// See [TrackEntry.getTimeScale] for affecting a single animation.
  double getTimeScale() {
    return _bindings.spine_animation_state_get_time_scale(_state);
  }

  void setTimeScale(double timeScale) {
    _bindings.spine_animation_state_set_time_scale(_state, timeScale);
  }

  /// The [AnimationStateData] to look up mix durations.
  AnimationStateData getData() {
    return AnimationStateData._(_bindings.spine_animation_state_get_data(_state));
  }

  /// The listener for events generated for all tracks managed by the AnimationState, or null.
  ///
  /// A track entry returned from [setAnimation] is already the current animation
  /// for the track, so the track entry listener will not be called for [EventType.start].
  void setListener(AnimationStateListener? listener) {
    _stateListener = listener;
  }
}

/// A SkeletonDrawable bundles loading, updating, and rendering an [Atlas], [Skeleton], and [AnimationState]
/// into a single easy to use class.
///
/// Use the [fromAsset], [fromFile], or [fromHttp] methods to construct a SkeletonDrawable. To have
/// multiple skeleton drawable instances share the same [Atlas] and [SkeletonData], use the constructor.
///
/// You can then directly access the [atlas], [skeletonData], [skeleton], [animationStateData], and [animationState]
/// to query and animate the skeleton. Use the [AnimationState] to queue animations on one or more tracks
/// via [AnimationState.setAnimation] or [AnimationState.addAnimation].
///
/// To update the [AnimationState] and apply it to the [Skeleton] call the [update] function, providing it
/// a delta time in seconds to advance the animations.
///
/// To render the current pose of the [Skeleton], use the rendering methods [render], [renderToCanvas], [renderToPictureRecorder],
/// [renderToPng], or [renderToRawImageData], depending on your needs.
///
/// When the skeleton drawable is no longer needed, call the [dispose] method to release its resources. If
/// the skeleton drawable was constructed from a shared [Atlas] and [SkeletonData], make sure to dispose the
/// atlas and skeleton data as well, if no skeleton drawable references them anymore.
class SkeletonDrawable {
  final Atlas atlas;
  final SkeletonData skeletonData;
  late final spine_skeleton_drawable _drawable;
  late final Skeleton skeleton;
  late final AnimationStateData animationStateData;
  late final AnimationState animationState;
  final bool _ownsAtlasAndSkeletonData;
  bool _disposed;

  /// Constructs a new skeleton drawable from the given (possibly shared) [Atlas] and [SkeletonData]. If
  /// the atlas and skeleton data are not shared, the drawable can take ownership by passing true for [_ownsAtlasAndSkeletonData].
  /// In that case a call to [dispose] will also dispose the atlas and skeleton data.
  SkeletonDrawable(this.atlas, this.skeletonData, this._ownsAtlasAndSkeletonData) : _disposed = false {
    _drawable = _bindings.spine_skeleton_drawable_create(skeletonData._data);
    skeleton = Skeleton._(_bindings.spine_skeleton_drawable_get_skeleton(_drawable));
    animationStateData = AnimationStateData._(_bindings.spine_skeleton_drawable_get_animation_state_data(_drawable));
    animationState = AnimationState._(_bindings.spine_skeleton_drawable_get_animation_state(_drawable),
        _bindings.spine_skeleton_drawable_get_animation_state_events(_drawable));
    skeleton.updateWorldTransform(Physics.none);
  }

  /// Constructs a new skeleton drawable from the [atlasFile] and [skeletonFile] from the root asset bundle
  /// or the optionally provided [bundle].
  ///
  /// Throws an exception in case the data could not be loaded.
  static Future<SkeletonDrawable> fromAsset(String atlasFile, String skeletonFile, {AssetBundle? bundle}) async {
    bundle ??= rootBundle;
    var atlas = await Atlas.fromAsset(atlasFile, bundle: bundle);
    var skeletonData = await SkeletonData.fromAsset(atlas, skeletonFile, bundle: bundle);
    return SkeletonDrawable(atlas, skeletonData, true);
  }

  /// Constructs a new skeleton drawable from the [atlasFile] and [skeletonFile].
  ///
  /// Throws an exception in case the data could not be loaded.
  static Future<SkeletonDrawable> fromFile(String atlasFile, String skeletonFile) async {
    var atlas = await Atlas.fromFile(atlasFile);
    var skeletonData = await SkeletonData.fromFile(atlas, skeletonFile);
    return SkeletonDrawable(atlas, skeletonData, true);
  }

  /// Constructs a new skeleton drawable from the [atlasUrl] and [skeletonUrl].
  ///
  /// Throws an exception in case the data could not be loaded.
  static Future<SkeletonDrawable> fromHttp(String atlasUrl, String skeletonUrl) async {
    var atlas = await Atlas.fromHttp(atlasUrl);
    var skeletonData = await SkeletonData.fromHttp(atlas, skeletonUrl);
    return SkeletonDrawable(atlas, skeletonData, true);
  }

  /// Updates the [AnimationState] using the [delta] time given in seconds, applies the
  /// animation state to the [Skeleton] and updates the world transforms of the skeleton
  /// to calculate its current pose.
  void update(double delta) {
    if (_disposed) return;
    animationState.update(delta);
    animationState.apply(skeleton);
    skeleton.update(delta);
    skeleton.updateWorldTransform(Physics.update);
  }

  /// Renders to current skeleton pose to a list of [RenderCommand] instances. The render commands
  /// can be rendered via [Canvas.drawVertices].
  List<RenderCommand> render() {
    if (_disposed) return [];
    spine_render_command nativeCmd = _bindings.spine_skeleton_drawable_render(_drawable);
    List<RenderCommand> commands = [];
    while (nativeCmd.address != nullptr.address) {
      final atlasPage = atlas.atlasPages[_bindings.spine_render_command_get_atlas_page(nativeCmd)];
      commands.add(RenderCommand._(nativeCmd, atlasPage.width.toDouble(), atlasPage.height.toDouble()));
      nativeCmd = _bindings.spine_render_command_get_next(nativeCmd);
    }
    return commands;
  }

  /// Renders the skeleton drawable's current pose to the given [canvas]. Does not perform any
  /// scaling or fitting.
  List<RenderCommand> renderToCanvas(Canvas canvas) {
    var commands = render();
    for (final cmd in commands) {
      canvas.drawVertices(cmd.vertices, rendering.BlendMode.modulate, atlas.atlasPagePaints[cmd.atlasPageIndex][cmd.blendMode]!);
    }
    return commands;
  }

  /// Renders the skeleton drawable's current pose to a [PictureRecorder] with the given [width] and [height].
  /// Uses [bgColor], a 32-bit ARGB color value, to paint the background.
  /// Scales and centers the skeleton to fit the within the bounds of [width] and [height].
  PictureRecorder renderToPictureRecorder(double width, double height, int bgColor) {
    var bounds = skeleton.getBounds();
    var scale = 1 / (bounds.width > bounds.height ? bounds.width / width : bounds.height / height);

    var recorder = PictureRecorder();
    var canvas = Canvas(recorder);
    var paint = Paint()
      ..color = material.Color(bgColor)
      ..style = PaintingStyle.fill;
    canvas.drawRect(Rect.fromLTWH(0, 0, width, height), paint);
    canvas.translate(width / 2, height / 2);
    canvas.scale(scale, scale);
    canvas.translate(-(bounds.x + bounds.width / 2), -(bounds.y + bounds.height / 2));
    canvas.drawRect(const Rect.fromLTRB(-5, -5, 5, -5), paint..color = material.Colors.red);
    renderToCanvas(canvas);
    return recorder;
  }

  /// Renders the skeleton drawable's current pose to a PNG encoded in a [Uint8List], with the given [width] and [height].
  /// Uses [bgColor], a 32-bit ARGB color value, to paint the background.
  /// Scales and centers the skeleton to fit the within the bounds of [width] and [height].
  Future<Uint8List> renderToPng(double width, double height, int bgColor) async {
    final recorder = renderToPictureRecorder(width, height, bgColor);
    final image = await recorder.endRecording().toImage(width.toInt(), height.toInt());
    return (await image.toByteData(format: ImageByteFormat.png))!.buffer.asUint8List();
  }

  /// Renders the skeleton drawable's current pose to a [RawImageData], with the given [width] and [height].
  /// Uses [bgColor], a 32-bit ARGB color value, to paint the background.
  /// Scales and centers the skeleton to fit the within the bounds of [width] and [height].
  Future<RawImageData> renderToRawImageData(double width, double height, int bgColor) async {
    final recorder = renderToPictureRecorder(width, height, bgColor);
    var rawImageData =
        (await (await recorder.endRecording().toImage(width.toInt(), height.toInt())).toByteData(format: ImageByteFormat.rawRgba))!
            .buffer
            .asUint8List();
    return RawImageData(rawImageData, width.toInt(), height.toInt());
  }

  /// Disposes the skeleton drawable's resources. If the skeleton drawable owns the atlas
  /// and skeleton data, they are disposed as well. Must be called when the skeleton drawable
  /// is no longer in use.
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

/// Stores the vertices, indices, and atlas page index to be used for rendering one or more attachments
/// of a [Skeleton] to a [Canvas]. See the implementation of [SkeletonDrawable.renderToCanvas] on how to use this data to render it to a
/// [Canvas].
class RenderCommand {
  late final Vertices vertices;
  late final int atlasPageIndex;
  late final Float32List positions;
  late final Float32List uvs;
  late final Int32List colors;
  late final Uint16List indices;
  late final BlendMode blendMode;

  RenderCommand._(spine_render_command nativeCmd, double pageWidth, double pageHeight) {
    atlasPageIndex = _bindings.spine_render_command_get_atlas_page(nativeCmd);
    int numVertices = _bindings.spine_render_command_get_num_vertices(nativeCmd);
    int numIndices = _bindings.spine_render_command_get_num_indices(nativeCmd);
    positions = _bindings.spine_render_command_get_positions(nativeCmd).asTypedList(numVertices * 2);
    uvs = _bindings.spine_render_command_get_uvs(nativeCmd).asTypedList(numVertices * 2);
    for (int i = 0; i < numVertices * 2; i += 2) {
      uvs[i] *= pageWidth;
      uvs[i + 1] *= pageHeight;
    }
    colors = _bindings.spine_render_command_get_colors(nativeCmd).asTypedList(numVertices);
    indices = _bindings.spine_render_command_get_indices(nativeCmd).asTypedList(numIndices);
    blendMode = BlendMode.values[_bindings.spine_render_command_get_blend_mode(nativeCmd)];

    if (!kIsWeb) {
      // We pass the native data as views directly to Vertices.raw. According to the sources, the data
      // is copied, so it doesn't matter that we free up the underlying memory on the next
      // render call. See the implementation of Vertices.raw() here:
      // https://github.com/flutter/engine/blob/5c60785b802ad2c8b8899608d949342d5c624952/lib/ui/painting/vertices.cc#L21
      //
      // Impeller is currently using a slow path when using vertex colors.
      // See https://github.com/flutter/flutter/issues/127486
      //
      // We thus batch all meshes not only by atlas page and blend mode, but also vertex color.
      // See spine_flutter.cpp, batch_commands().
      //
      // If the vertex color equals (1, 1, 1, 1), we do not store
      // colors, which will trigger the fast path in Impeller. Otherwise we have to go the slow path, which
      // has to render to an offscreen surface.
      if (colors.isNotEmpty && colors[0] == -1) {
        vertices = Vertices.raw(VertexMode.triangles, positions, textureCoordinates: uvs, indices: indices);
      } else {
        vertices = Vertices.raw(VertexMode.triangles, positions, textureCoordinates: uvs, colors: colors, indices: indices);
      }
    } else {
      // On the web, rendering is done through CanvasKit, which requires copies of the native data.
      final positionsCopy = Float32List.fromList(positions);
      final uvsCopy = Float32List.fromList(uvs);
      final colorsCopy = Int32List.fromList(colors);
      final indicesCopy = Uint16List.fromList(indices);
      vertices = Vertices.raw(VertexMode.triangles, positionsCopy, textureCoordinates: uvsCopy, colors: colorsCopy, indices: indicesCopy);
    }
  }
}

/// Renders debug information for a [SkeletonDrawable], like bone locations, to a [Canvas].
/// See [DebugRenderer.render].
class DebugRenderer {
  const DebugRenderer();

  void render(SkeletonDrawable drawable, Canvas canvas, List<RenderCommand> commands) {
    final bonePaint = Paint()
      ..color = material.Colors.blue
      ..style = PaintingStyle.fill;
    for (final bone in drawable.skeleton.getBones()) {
      canvas.drawRect(Rect.fromCenter(center: Offset(bone.getWorldX(), bone.getWorldY()), width: 5, height: 5), bonePaint);
    }
  }
}
