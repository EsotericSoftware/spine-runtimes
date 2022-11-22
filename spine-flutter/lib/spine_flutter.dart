import 'dart:convert' as convert;
import 'dart:io';
import 'dart:typed_data';
import 'dart:ui';

import 'package:flutter/rendering.dart' as rendering;
import 'package:flutter/services.dart';
import 'package:http/http.dart' as http;
import 'package:path/path.dart' as path;

import 'spine_flutter_bindings_generated.dart';
import 'ffi_proxy.dart';
import 'ffi_utf8.dart';

export 'spine_widget.dart';
import 'init.dart' if (dart.library.html) 'init_web.dart';
import 'package:flutter/foundation.dart' show kIsWeb;

late SpineFlutterBindings _bindings;
late Allocator _allocator;

Future<void> initSpineFlutter() async {
  final ffi = await initSpineFlutterFFI();
  _bindings = SpineFlutterBindings(ffi.dylib);
  _allocator = ffi.allocator;
  return;
}

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

class Vec2 {
  double x;
  double y;

  Vec2(this.x, this.y);
}

class Atlas {
  final spine_atlas _atlas;
  final List<Image> atlasPages;
  final List<Paint> atlasPagePaints;
  bool _disposed;

  Atlas._(this._atlas, this.atlasPages, this.atlasPagePaints) : _disposed = false;

  static Future<Atlas> _load(String atlasFileName, Future<Uint8List> Function(String name) loadFile) async {
    final atlasBytes = await loadFile(atlasFileName);
    final atlasData = convert.utf8.decode(atlasBytes);
    final atlasDataNative = atlasData.toNativeUtf8(_allocator);
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
    List<Paint> atlasPagePaints = [];
    final numImagePaths = _bindings.spine_atlas_get_num_image_paths(atlas);
    for (int i = 0; i < numImagePaths; i++) {
      final Pointer<Utf8> atlasPageFile = _bindings.spine_atlas_get_image_path(atlas, i).cast();
      final imagePath = path.join(atlasDir, atlasPageFile.toDartString());
      var imageData = await loadFile(imagePath);
      final Codec codec = await instantiateImageCodec(imageData);
      final FrameInfo frameInfo = await codec.getNextFrame();
      final Image image = frameInfo.image;
      atlasPages.add(image);
      atlasPagePaints.add(Paint()
        ..shader = ImageShader(image, TileMode.clamp, TileMode.clamp, Matrix4.identity().storage,
            filterQuality: FilterQuality.high)
        ..isAntiAlias = true);
    }

    return Atlas._(atlas, atlasPages, atlasPagePaints);
  }

  static Future<Atlas> fromAsset(String atlasFileName, {AssetBundle? bundle}) async {
    bundle ??= rootBundle;
    return _load(atlasFileName, (file) async => (await bundle!.load(file)).buffer.asUint8List());
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
    _bindings.spine_atlas_dispose(_atlas);
    for (final image in atlasPages) {
      image.dispose();
    }
  }
}

class SkeletonData {
  final spine_skeleton_data _data;
  bool _disposed;

  SkeletonData._(this._data) : _disposed = false;

  static SkeletonData fromJson(Atlas atlas, String json) {
    final jsonNative = json.toNativeUtf8(_allocator);
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

  static Future<SkeletonData> fromAsset(Atlas atlas, String skeletonFile, {AssetBundle? bundle}) async {
    bundle ??= rootBundle;
    if (skeletonFile.endsWith(".json")) {
      return fromJson(atlas, await bundle.loadString(skeletonFile));
    } else {
      return fromBinary(atlas, (await bundle.load(skeletonFile)).buffer.asUint8List());
    }
  }

  static Future<SkeletonData> fromFile(Atlas atlas, String skeletonFile) async {
    if (skeletonFile.endsWith(".json")) {
      return fromJson(atlas, convert.utf8.decode(await File(skeletonFile).readAsBytes()));
    } else {
      return fromBinary(atlas, await File(skeletonFile).readAsBytes());
    }
  }

  static Future<SkeletonData> fromHttp(Atlas atlas, String skeletonFile) async {
    if (skeletonFile.endsWith(".json")) {
      return fromJson(atlas, convert.utf8.decode((await http.get(Uri.parse(skeletonFile))).bodyBytes));
    } else {
      return fromBinary(atlas, (await http.get(Uri.parse(skeletonFile))).bodyBytes);
    }
  }

  /// Finds a bone by comparing each bone's name. It is more efficient to cache the results of this method than to call it multiple times.
  BoneData? findBone(String name) {
    final nativeName = name.toNativeUtf8(_allocator);
    final bone = _bindings.spine_skeleton_data_find_bone(_data, nativeName.cast());
    _allocator.free(nativeName);
    if (bone.address == nullptr.address) return null;
    return BoneData._(bone);
  }

  /// Finds a slot by comparing each slot's name. It is more efficient to cache the results of this method than to call it multiple times.
  SlotData? findSlot(String name) {
    final nativeName = name.toNativeUtf8(_allocator);
    final slot = _bindings.spine_skeleton_data_find_slot(_data, nativeName.cast());
    _allocator.free(nativeName);
    if (slot.address == nullptr.address) return null;
    return SlotData._(slot);
  }

  /// Finds a skin by comparing each skin's name. It is more efficient to cache the results of this method than to call it
  /// multiple times.
  Skin? findSkin(String name) {
    final nativeName = name.toNativeUtf8(_allocator);
    final skin = _bindings.spine_skeleton_data_find_skin(_data, nativeName.cast());
    _allocator.free(nativeName);
    if (skin.address == nullptr.address) return null;
    return Skin._(skin);
  }

  /// Finds an event by comparing each events's name. It is more efficient to cache the results of this method than to call it
  /// multiple times.
  EventData? findEvent(String name) {
    final nativeName = name.toNativeUtf8(_allocator);
    final event = _bindings.spine_skeleton_data_find_event(_data, nativeName.cast());
    _allocator.free(nativeName);
    if (event.address == nullptr.address) return null;
    return EventData._(event);
  }

  /// Finds an animation by comparing each animation's name. It is more efficient to cache the results of this method than to
  /// call it multiple times.
  Animation? findAnimation(String name) {
    final nativeName = name.toNativeUtf8(_allocator);
    final animation = _bindings.spine_skeleton_data_find_animation(_data, nativeName.cast());
    _allocator.free(nativeName);
    if (animation.address == nullptr.address) return null;
    return Animation._(animation);
  }

  /// Finds an IK constraint by comparing each IK constraint's name. It is more efficient to cache the results of this method
  /// than to call it multiple times.
  IkConstraintData? findIkConstraint(String name) {
    final nativeName = name.toNativeUtf8(_allocator);
    final constraint = _bindings.spine_skeleton_data_find_ik_constraint(_data, nativeName.cast());
    _allocator.free(nativeName);
    if (constraint.address == nullptr.address) return null;
    return IkConstraintData._(constraint);
  }

  /// Finds a transform constraint by comparing each transform constraint's name. It is more efficient to cache the results of
  /// this method than to call it multiple times.
  TransformConstraintData? findTransformConstraint(String name) {
    final nativeName = name.toNativeUtf8(_allocator);
    final constraint = _bindings.spine_skeleton_data_find_transform_constraint(_data, nativeName.cast());
    _allocator.free(nativeName);
    if (constraint.address == nullptr.address) return null;
    return TransformConstraintData._(constraint);
  }

  /// Finds a path constraint by comparing each path constraint's name. It is more efficient to cache the results of this method
  /// than to call it multiple times.
  PathConstraintData? findPathConstraint(String name) {
    final nativeName = name.toNativeUtf8(_allocator);
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

  void dispose() {
    if (_disposed) return;
    _disposed = true;
    _bindings.spine_skeleton_data_dispose(_data);
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

enum TransformMode {
  Normal(0),
  OnlyTranslation(1),
  NoRotationOrReflection(2),
  NoScale(3),
  NoScaleOrReflection(4);

  final int value;

  const TransformMode(this.value);
}

enum PositionMode {
  Fixed(0),
  Percent(1);

  final int value;

  const PositionMode(this.value);
}

enum SpacingMode {
  Length(0),
  Fixed(1),
  Percent(2),
  Proportional(3);

  final int value;

  const SpacingMode(this.value);
}

enum RotateMode {
  Tangent(0),
  Chain(1),
  ChainScale(2);

  final int value;

  const RotateMode(this.value);
}

class BoneData {
  final spine_bone_data _data;

  BoneData._(this._data);

  int getIndex() {
    return _bindings.spine_bone_data_get_index(_data);
  }

  String getName() {
    Pointer<Utf8> name = _bindings.spine_bone_data_get_name(_data).cast();
    return name.toDartString();
  }

  BoneData? getParent() {
    final parent = _bindings.spine_bone_data_get_parent(_data);
    if (parent.address == nullptr.address) return null;
    return BoneData._(parent);
  }

  double getLength() {
    return _bindings.spine_bone_data_get_length(_data);
  }

  void setLength(double length) {
    _bindings.spine_bone_data_set_length(_data, length);
  }

  double getX() {
    return _bindings.spine_bone_data_get_x(_data);
  }

  void setX(double x) {
    _bindings.spine_bone_data_set_x(_data, x);
  }

  double getY() {
    return _bindings.spine_bone_data_get_y(_data);
  }

  void setY(double y) {
    _bindings.spine_bone_data_set_y(_data, y);
  }

  double getRotation() {
    return _bindings.spine_bone_data_get_rotation(_data);
  }

  void setRotation(double rotation) {
    _bindings.spine_bone_data_set_rotation(_data, rotation);
  }

  double getScaleX() {
    return _bindings.spine_bone_data_get_scale_x(_data);
  }

  void setScaleX(double scaleX) {
    _bindings.spine_bone_data_set_scale_x(_data, scaleX);
  }

  double getScaleY() {
    return _bindings.spine_bone_data_get_scale_y(_data);
  }

  void setScaleY(double scaleY) {
    _bindings.spine_bone_data_set_scale_y(_data, scaleY);
  }

  double getShearX() {
    return _bindings.spine_bone_data_get_shear_x(_data);
  }

  void setShearX(double shearX) {
    _bindings.spine_bone_data_set_shear_x(_data, shearX);
  }

  double getShearY() {
    return _bindings.spine_bone_data_get_shear_y(_data);
  }

  void setShearY(double shearY) {
    _bindings.spine_bone_data_set_shear_y(_data, shearY);
  }

  TransformMode getTransformMode() {
    final nativeMode = _bindings.spine_bone_data_get_transform_mode(_data);
    return TransformMode.values[nativeMode];
  }

  void setTransformMode(TransformMode mode) {
    _bindings.spine_bone_data_set_transform_mode(_data, mode.value);
  }

  bool isSkinRequired() {
    return _bindings.spine_bone_data_is_skin_required(_data) == -1;
  }

  void setIsSkinRequired(bool isSkinRequired) {
    _bindings.spine_bone_data_set_is_skin_required(_data, isSkinRequired ? -1 : 0);
  }

  Color getColor() {
    final color = _bindings.spine_bone_data_get_color(_data);
    return Color(
        _bindings.spine_color_get_r(color),
        _bindings.spine_color_get_g(color),
        _bindings.spine_color_get_b(color),
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

class Bone {
  final spine_bone _bone;

  Bone._(this._bone);

  static void setIsYDown(bool isYDown) {
    _bindings.spine_bone_set_is_y_down(isYDown ? -1 : 0);
  }

  static bool getIsYDown() {
    return _bindings.spine_bone_get_is_y_down() == 1;
  }

  void update() {
    _bindings.spine_bone_update(_bone);
  }

  void updateWorldTransform() {
    _bindings.spine_bone_update_world_transform(_bone);
  }

  void updateWorldTransformWith(
      double x, double y, double rotation, double scaleX, double scaleY, double shearX, double shearY) {
    _bindings.spine_bone_update_world_transform_with(_bone, x, y, rotation, scaleX, scaleY, shearX, shearY);
  }

  void setToSetupPose() {
    _bindings.spine_bone_set_to_setup_pose(_bone);
  }

  Vec2 worldToLocal(double worldX, double worldY) {
    final local = _bindings.spine_bone_world_to_local(_bone, worldX, worldY);
    final result = Vec2(_bindings.spine_vector_get_x(local), _bindings.spine_vector_get_y(local));
    _allocator.free(local);
    return result;
  }

  Vec2 localToWorld(double localX, double localY) {
    final world = _bindings.spine_bone_local_to_world(_bone, localX, localY);
    final result = Vec2(_bindings.spine_vector_get_x(world), _bindings.spine_vector_get_y(world));
    _allocator.free(world);
    return result;
  }

  double worldToLocalRotation(double worldRotation) {
    return _bindings.spine_bone_world_to_local_rotation(_bone, worldRotation);
  }

  double localToWorldRotation(double localRotation) {
    return _bindings.spine_bone_local_to_world_rotation(_bone, localRotation);
  }

  void rotateWorld(double degrees) {
    _bindings.spine_bone_rotate_world(_bone, degrees);
  }

  double getWorldToLocalRotationX() {
    return _bindings.spine_bone_get_world_rotation_x(_bone);
  }

  double getWorldToLocalRotationY() {
    return _bindings.spine_bone_get_world_to_local_rotation_y(_bone);
  }

  BoneData getData() {
    return BoneData._(_bindings.spine_bone_get_data(_bone));
  }

  Skeleton getSkeleton() {
    return Skeleton._(_bindings.spine_bone_get_skeleton(_bone));
  }

  Bone? getParent() {
    final parent = _bindings.spine_bone_get_parent(_bone);
    if (parent.address == nullptr.address) return null;
    return Bone._(parent);
  }

  List<Bone> getChildren() {
    List<Bone> children = [];
    final numChildren = _bindings.spine_bone_get_num_children(_bone);
    final nativeChildren = _bindings.spine_bone_get_children(_bone);
    for (int i = 0; i < numChildren; i++) {
      children.add(Bone._(nativeChildren[i]));
    }
    return children;
  }

  double getX() {
    return _bindings.spine_bone_get_x(_bone);
  }

  void setX(double x) {
    _bindings.spine_bone_set_x(_bone, x);
  }

  double getY() {
    return _bindings.spine_bone_get_y(_bone);
  }

  void setY(double y) {
    _bindings.spine_bone_set_y(_bone, y);
  }

  double getRotation() {
    return _bindings.spine_bone_get_rotation(_bone);
  }

  void setRotation(double rotation) {
    _bindings.spine_bone_set_rotation(_bone, rotation);
  }

  double getScaleX() {
    return _bindings.spine_bone_get_scale_x(_bone);
  }

  void setScaleX(double scaleX) {
    _bindings.spine_bone_set_scale_x(_bone, scaleX);
  }

  double getScaleY() {
    return _bindings.spine_bone_get_scale_y(_bone);
  }

  void setScaleY(double scaleY) {
    _bindings.spine_bone_set_scale_y(_bone, scaleY);
  }

  double getShearX() {
    return _bindings.spine_bone_get_shear_x(_bone);
  }

  void setShearX(double shearX) {
    _bindings.spine_bone_set_shear_x(_bone, shearX);
  }

  double getShearY() {
    return _bindings.spine_bone_get_shear_y(_bone);
  }

  void setShearY(double shearY) {
    _bindings.spine_bone_set_shear_y(_bone, shearY);
  }

  double getAX() {
    return _bindings.spine_bone_get_a_x(_bone);
  }

  void setAX(double x) {
    _bindings.spine_bone_set_a_x(_bone, x);
  }

  double getAY() {
    return _bindings.spine_bone_get_a_y(_bone);
  }

  void setAY(double y) {
    _bindings.spine_bone_set_a_y(_bone, y);
  }

  double getAppliedRotation() {
    return _bindings.spine_bone_get_applied_rotation(_bone);
  }

  void setAppliedRotation(double rotation) {
    _bindings.spine_bone_set_applied_rotation(_bone, rotation);
  }

  double getAScaleX() {
    return _bindings.spine_bone_get_a_scale_x(_bone);
  }

  void setAScaleX(double scaleX) {
    _bindings.spine_bone_set_a_scale_x(_bone, scaleX);
  }

  double getAScaleY() {
    return _bindings.spine_bone_get_a_scale_y(_bone);
  }

  void setAScaleY(double scaleY) {
    _bindings.spine_bone_set_a_scale_y(_bone, scaleY);
  }

  double getAShearX() {
    return _bindings.spine_bone_get_a_shear_x(_bone);
  }

  void setAShearX(double shearX) {
    _bindings.spine_bone_set_a_shear_x(_bone, shearX);
  }

  double getAShearY() {
    return _bindings.spine_bone_get_a_shear_y(_bone);
  }

  void setAShearY(double shearY) {
    _bindings.spine_bone_set_a_shear_y(_bone, shearY);
  }

  double getA() {
    return _bindings.spine_bone_get_a(_bone);
  }

  void setA(double a) {
    _bindings.spine_bone_set_a(_bone, a);
  }

  double getB() {
    return _bindings.spine_bone_get_b(_bone);
  }

  void setB(double b) {
    _bindings.spine_bone_set_b(_bone, b);
  }

  double getC() {
    return _bindings.spine_bone_get_c(_bone);
  }

  void setC(double c) {
    _bindings.spine_bone_set_c(_bone, c);
  }

  double getD() {
    return _bindings.spine_bone_get_d(_bone);
  }

  void setD(double d) {
    _bindings.spine_bone_set_a(_bone, d);
  }

  double getWorldX() {
    return _bindings.spine_bone_get_world_x(_bone);
  }

  void setWorldX(double worldX) {
    _bindings.spine_bone_set_world_x(_bone, worldX);
  }

  double getWorldY() {
    return _bindings.spine_bone_get_world_y(_bone);
  }

  void setWorldY(double worldY) {
    _bindings.spine_bone_set_world_y(_bone, worldY);
  }

  double getWorldRotationX() {
    return _bindings.spine_bone_get_world_rotation_x(_bone);
  }

  double getWorldRotationY() {
    return _bindings.spine_bone_get_world_rotation_y(_bone);
  }

  double getWorldScaleX() {
    return _bindings.spine_bone_get_world_scale_x(_bone);
  }

  double getWorldScaleY() {
    return _bindings.spine_bone_get_world_scale_y(_bone);
  }

  bool isActive() {
    return _bindings.spine_bone_get_is_active(_bone) == -1;
  }

  void setIsActive(bool isActive) {
    _bindings.spine_bone_set_is_active(_bone, isActive ? -1 : 0);
  }
}

class SlotData {
  final spine_slot_data _data;

  SlotData._(this._data);

  int getIndex() {
    return _bindings.spine_slot_data_get_index(_data);
  }

  String getName() {
    final Pointer<Utf8> value = _bindings.spine_slot_data_get_name(_data).cast();
    return value.toDartString();
  }

  BoneData getBoneData() {
    return BoneData._(_bindings.spine_slot_data_get_bone_data(_data));
  }

  Color getColor() {
    final color = _bindings.spine_slot_data_get_color(_data);
    return Color(
        _bindings.spine_color_get_r(color),
        _bindings.spine_color_get_g(color),
        _bindings.spine_color_get_b(color),
        _bindings.spine_color_get_a(color));
  }

  void setColor(double r, double g, double b, double a) {
    _bindings.spine_slot_data_set_color(_data, r, g, b, a);
  }

  Color getDarkColor() {
    final color = _bindings.spine_slot_data_get_dark_color(_data);
    return Color(
        _bindings.spine_color_get_r(color),
        _bindings.spine_color_get_g(color),
        _bindings.spine_color_get_b(color),
        _bindings.spine_color_get_a(color));
  }

  void setDarkColor(double r, double g, double b, double a) {
    _bindings.spine_slot_data_set_dark_color(_data, r, g, b, a);
  }

  bool hasDarkColor() {
    return _bindings.spine_slot_data_has_dark_color(_data) == -1;
  }

  void setHasDarkColor(bool hasDarkColor) {
    _bindings.spine_slot_data_set_has_dark_color(_data, hasDarkColor ? -1 : 0);
  }

  String getAttachmentName() {
    final Pointer<Utf8> value = _bindings.spine_slot_data_get_attachment_name(_data).cast();
    return value.toDartString();
  }

  void setAttachmentName(String attachmentName) {
    final nativeName = attachmentName.toNativeUtf8(_allocator);
    _bindings.spine_slot_data_set_attachment_name(_data, nativeName.cast());
    _allocator.free(nativeName);
  }

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

class Slot {
  final spine_slot _slot;

  Slot._(this._slot);

  void setToSetupPose() {
    _bindings.spine_slot_set_to_setup_pose(_slot);
  }

  SlotData getData() {
    return SlotData._(_bindings.spine_slot_get_data(_slot));
  }

  Bone getBone() {
    return Bone._(_bindings.spine_slot_get_bone(_slot));
  }

  Skeleton getSkeleton() {
    return Skeleton._(_bindings.spine_slot_get_skeleton(_slot));
  }

  Color getColor() {
    final color = _bindings.spine_slot_get_color(_slot);
    return Color(
        _bindings.spine_color_get_r(color),
        _bindings.spine_color_get_g(color),
        _bindings.spine_color_get_b(color),
        _bindings.spine_color_get_a(color));
  }

  void setColor(Color color) {
    _bindings.spine_slot_set_color(_slot, color.r, color.g, color.b, color.a);
  }

  Color getDarkColor() {
    final color = _bindings.spine_slot_get_dark_color(_slot);
    return Color(
        _bindings.spine_color_get_r(color),
        _bindings.spine_color_get_g(color),
        _bindings.spine_color_get_b(color),
        _bindings.spine_color_get_a(color));
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
    return Attachment._toSubclass(attachment);
  }

  void setAttachment(Attachment? attachment) {
    _bindings.spine_slot_set_attachment(_slot, attachment != null ? attachment._attachment.cast() : nullptr);
  }

  @override
  String toString() {
    return getData().getName();
  }

  int getSequenceIndex() {
    return _bindings.spine_slot_get_sequence_index(_slot);
  }

  void setSequenceIndex(int sequenceIndex) {
    _bindings.spine_slot_set_sequence_index(_slot, sequenceIndex);
  }
}

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

class Sequence {
  final spine_sequence _sequence;

  Sequence._(this._sequence);

  void apply(Slot slot, Attachment attachment) {
    _bindings.spine_sequence_apply(_sequence, slot._slot, attachment._attachment.cast());
  }

  String getPath(String basePath, int index) {
    final nativeBasePath = basePath.toNativeUtf8(_allocator);
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

enum AttachmentType {
  Region(0),
  Mesh(1),
  Clipping(2),
  BoundingBox(3),
  Path(4),
  Point(5);

  final int value;

  const AttachmentType(this.value);
}

abstract class Attachment<T extends Pointer> {
  final T _attachment;

  Attachment._(this._attachment);

  String getName() {
    Pointer<Utf8> name = _bindings.spine_attachment_get_name(_attachment.cast()).cast();
    return name.toString();
  }

  AttachmentType getType() {
    final type = _bindings.spine_attachment_get_type(_attachment.cast());
    return AttachmentType.values[type];
  }

  static Attachment _toSubclass(spine_attachment attachment) {
    final type = AttachmentType.values[_bindings.spine_attachment_get_type(attachment)];
    switch (type) {
      case AttachmentType.Region:
        return RegionAttachment._(attachment.cast());
      case AttachmentType.Mesh:
        return MeshAttachment._(attachment.cast());
      case AttachmentType.Clipping:
        return ClippingAttachment._(attachment.cast());
      case AttachmentType.BoundingBox:
        return BoundingBoxAttachment._(attachment.cast());
      case AttachmentType.Path:
        return PathAttachment._(attachment.cast());
      case AttachmentType.Point:
        return PointAttachment._(attachment.cast());
    }
  }

  Attachment copy() {
    return _toSubclass(_bindings.spine_attachment_copy(_attachment.cast()));
  }

  void dispose() {
    _bindings.spine_attachment_dispose(_attachment.cast());
  }
}

class RegionAttachment extends Attachment<spine_region_attachment> {
  RegionAttachment._(spine_region_attachment attachment) : super._(attachment);

  List<double> computeWorldVertices(Slot slot) {
    Pointer<Float> vertices = _allocator.allocate(4 * 8).cast();
    _bindings.spine_region_attachment_compute_world_vertices(_attachment, slot._slot, vertices);
    final result = vertices.asTypedList(8).toList();
    _allocator.free(vertices);
    return result;
  }

  double getX() {
    return _bindings.spine_region_attachment_get_x(_attachment);
  }

  void setX(double x) {
    _bindings.spine_region_attachment_set_x(_attachment, x);
  }

  double getY() {
    return _bindings.spine_region_attachment_get_y(_attachment);
  }

  void setY(double y) {
    _bindings.spine_region_attachment_set_y(_attachment, y);
  }

  double getRotation() {
    return _bindings.spine_region_attachment_get_rotation(_attachment);
  }

  void setRotation(double rotation) {
    _bindings.spine_region_attachment_set_rotation(_attachment, rotation);
  }

  double getScaleX() {
    return _bindings.spine_region_attachment_get_scale_x(_attachment);
  }

  void setScaleX(double scaleX) {
    _bindings.spine_region_attachment_set_scale_x(_attachment, scaleX);
  }

  double getScaleY() {
    return _bindings.spine_region_attachment_get_scale_y(_attachment);
  }

  void setScaleY(double scaleY) {
    _bindings.spine_region_attachment_set_scale_x(_attachment, scaleY);
  }

  double getWidth() {
    return _bindings.spine_region_attachment_get_width(_attachment);
  }

  void setWidth(double width) {
    _bindings.spine_region_attachment_set_width(_attachment, width);
  }

  double getHeight() {
    return _bindings.spine_region_attachment_get_height(_attachment);
  }

  void setHeight(double height) {
    _bindings.spine_region_attachment_set_height(_attachment, height);
  }

  Color getColor() {
    final color = _bindings.spine_region_attachment_get_color(_attachment);
    return Color(
        _bindings.spine_color_get_r(color),
        _bindings.spine_color_get_g(color),
        _bindings.spine_color_get_b(color),
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

  Float32List getOffset() {
    final num = _bindings.spine_region_attachment_get_num_offset(_attachment);
    final offset = _bindings.spine_region_attachment_get_offset(_attachment);
    return offset.asTypedList(num);
  }

  Float32List getUVs() {
    final num = _bindings.spine_region_attachment_get_num_uvs(_attachment);
    final offset = _bindings.spine_region_attachment_get_uvs(_attachment);
    return offset.asTypedList(num);
  }
}

class VertexAttachment<T extends Pointer> extends Attachment<T> {
  VertexAttachment._(T attachment) : super._(attachment);

  List<double> computeWorldVertices(Slot slot) {
    final worldVerticesLength = _bindings.spine_vertex_attachment_get_world_vertices_length(_attachment.cast());
    Pointer<Float> vertices = _allocator.allocate(4 * worldVerticesLength).cast();
    _bindings.spine_vertex_attachment_compute_world_vertices(_attachment.cast(), slot._slot, vertices);
    final result = vertices.asTypedList(worldVerticesLength).toList();
    _allocator.free(vertices);
    return result;
  }

  Int32List getBones() {
    final num = _bindings.spine_vertex_attachment_get_num_bones(_attachment.cast());
    final bones = _bindings.spine_vertex_attachment_get_bones(_attachment.cast());
    return bones.asTypedList(num);
  }

  Float32List getVertices() {
    final num = _bindings.spine_vertex_attachment_get_num_vertices(_attachment.cast());
    final vertices = _bindings.spine_vertex_attachment_get_vertices(_attachment.cast());
    return vertices.asTypedList(num);
  }

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

class MeshAttachment extends VertexAttachment<spine_mesh_attachment> {
  MeshAttachment._(spine_mesh_attachment attachment) : super._(attachment.cast());

  void updateRegion() {
    _bindings.spine_mesh_attachment_update_region(_attachment);
  }

  int getHullLength() {
    return _bindings.spine_mesh_attachment_get_hull_length(_attachment);
  }

  void setHullLength(int hullLength) {
    _bindings.spine_mesh_attachment_set_hull_length(_attachment, hullLength);
  }

  Float32List getRegionUVs() {
    final num = _bindings.spine_mesh_attachment_get_num_region_uvs(_attachment);
    final uvs = _bindings.spine_mesh_attachment_get_region_uvs(_attachment);
    return uvs.asTypedList(num);
  }

  Float32List getUVs() {
    final num = _bindings.spine_mesh_attachment_get_num_uvs(_attachment);
    final uvs = _bindings.spine_mesh_attachment_get_uvs(_attachment);
    return uvs.asTypedList(num);
  }

  Uint16List getTriangles() {
    final num = _bindings.spine_mesh_attachment_get_num_triangles(_attachment);
    final triangles = _bindings.spine_mesh_attachment_get_triangles(_attachment);
    return triangles.asTypedList(num);
  }

  Color getColor() {
    final color = _bindings.spine_mesh_attachment_get_color(_attachment);
    return Color(
        _bindings.spine_color_get_r(color),
        _bindings.spine_color_get_g(color),
        _bindings.spine_color_get_b(color),
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

  MeshAttachment? getParentMesh() {
    final parent = _bindings.spine_mesh_attachment_get_parent_mesh(_attachment);
    if (parent.address == nullptr.address) return null;
    return MeshAttachment._(parent);
  }

  void setParentMesh(MeshAttachment? parentMesh) {
    _bindings.spine_mesh_attachment_set_parent_mesh(_attachment, parentMesh == null ? nullptr : parentMesh._attachment);
  }

  Uint16List getEdges() {
    final num = _bindings.spine_mesh_attachment_get_num_edges(_attachment);
    final edges = _bindings.spine_mesh_attachment_get_edges(_attachment);
    return edges.asTypedList(num);
  }

  double getWidth() {
    return _bindings.spine_mesh_attachment_get_width(_attachment);
  }

  void setWidth(double width) {
    _bindings.spine_mesh_attachment_set_width(_attachment, width);
  }

  double getHeight() {
    return _bindings.spine_mesh_attachment_get_height(_attachment);
  }

  void setHeight(double height) {
    _bindings.spine_mesh_attachment_set_height(_attachment, height);
  }
}

class ClippingAttachment extends VertexAttachment<spine_clipping_attachment> {
  ClippingAttachment._(spine_clipping_attachment attachment) : super._(attachment.cast());

  SlotData? getEndSlot() {
    final endSlot = _bindings.spine_clipping_attachment_get_end_slot(_attachment);
    if (endSlot.address == nullptr.address) return null;
    return SlotData._(endSlot);
  }

  void setEndSlot(SlotData? endSlot) {
    _bindings.spine_clipping_attachment_set_end_slot(_attachment, endSlot == null ? nullptr : endSlot._data);
  }

  Color getColor() {
    final color = _bindings.spine_clipping_attachment_get_color(_attachment);
    return Color(
        _bindings.spine_color_get_r(color),
        _bindings.spine_color_get_g(color),
        _bindings.spine_color_get_b(color),
        _bindings.spine_color_get_a(color));
  }

  void setColor(double r, double g, double b, double a) {
    _bindings.spine_clipping_attachment_set_color(_attachment, r, g, b, a);
  }
}

class BoundingBoxAttachment extends VertexAttachment<spine_bounding_box_attachment> {
  BoundingBoxAttachment._(spine_bounding_box_attachment attachment) : super._(attachment);

  Color getColor() {
    final color = _bindings.spine_bounding_box_attachment_get_color(_attachment);
    return Color(
        _bindings.spine_color_get_r(color),
        _bindings.spine_color_get_g(color),
        _bindings.spine_color_get_b(color),
        _bindings.spine_color_get_a(color));
  }

  void setColor(double r, double g, double b, double a) {
    _bindings.spine_bounding_box_attachment_set_color(_attachment, r, g, b, a);
  }
}

class PathAttachment extends VertexAttachment<spine_path_attachment> {
  PathAttachment._(spine_path_attachment attachment) : super._(attachment);

  Float32List getLengths() {
    final num = _bindings.spine_path_attachment_get_num_lengths(_attachment);
    final lengths = _bindings.spine_path_attachment_get_lengths(_attachment);
    return lengths.asTypedList(num);
  }

  bool isClosed() {
    return _bindings.spine_path_attachment_get_is_closed(_attachment) == -1;
  }

  void setIsClosed(bool isClosed) {
    _bindings.spine_path_attachment_set_is_closed(_attachment, isClosed ? -1 : 0);
  }

  bool isConstantSpeed() {
    return _bindings.spine_path_attachment_get_is_constant_speed(_attachment) == -1;
  }

  void setIsConstantSpeed(bool isClosed) {
    _bindings.spine_path_attachment_set_is_constant_speed(_attachment, isClosed ? -1 : 0);
  }

  Color getColor() {
    final color = _bindings.spine_path_attachment_get_color(_attachment);
    return Color(
        _bindings.spine_color_get_r(color),
        _bindings.spine_color_get_g(color),
        _bindings.spine_color_get_b(color),
        _bindings.spine_color_get_a(color));
  }

  void setColor(double r, double g, double b, double a) {
    _bindings.spine_path_attachment_set_color(_attachment, r, g, b, a);
  }
}

class PointAttachment extends Attachment<spine_point_attachment> {
  PointAttachment._(spine_point_attachment attachment) : super._(attachment);

  Vec2 computeWorldPosition(Bone bone) {
    final position = _bindings.spine_point_attachment_compute_world_position(_attachment, bone._bone);
    final result = Vec2(_bindings.spine_vector_get_x(position), _bindings.spine_vector_get_y(position));
    _allocator.free(position);
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

  Color getColor() {
    final color = _bindings.spine_point_attachment_get_color(_attachment);
    return Color(
        _bindings.spine_color_get_r(color),
        _bindings.spine_color_get_g(color),
        _bindings.spine_color_get_b(color),
        _bindings.spine_color_get_a(color));
  }

  void setColor(double r, double g, double b, double a) {
    _bindings.spine_point_attachment_set_color(_attachment, r, g, b, a);
  }
}

class SkinEntry {
  final int slotIndex;
  final String name;
  final Attachment? attachment;

  SkinEntry(this.slotIndex, this.name, this.attachment);
}

class Skin {
  late final bool _isCustomSkin;
  late final spine_skin _skin;

  Skin._(this._skin) : _isCustomSkin = false;

  Skin.new(String name) {
    final nativeName = name.toNativeUtf8(_allocator);
    _skin = _bindings.spine_skin_create(nativeName.cast());
    _allocator.free(nativeName);
    _isCustomSkin = true;
  }

  void dispose() {
    if (!_isCustomSkin) return;
    _bindings.spine_skin_dispose(_skin);
  }

  void setAttachment(int slotIndex, String name, Attachment? attachment) {
    final nativeName = name.toNativeUtf8(_allocator);
    _bindings.spine_skin_set_attachment(
        _skin, slotIndex, nativeName.cast(), attachment == null ? nullptr : attachment._attachment.cast());
    _allocator.free(nativeName);
  }

  Attachment? getAttachment(int slotIndex, String name) {
    final nativeName = name.toNativeUtf8(_allocator);
    final attachment = _bindings.spine_skin_get_attachment(_skin, slotIndex, nativeName.cast());
    _allocator.free(nativeName);
    if (attachment.address == nullptr.address) return null;
    return Attachment._toSubclass(attachment);
  }

  void removeAttachment(int slotIndex, String name) {
    final nativeName = name.toNativeUtf8(_allocator);
    _bindings.spine_skin_remove_attachment(_skin, slotIndex, nativeName.cast());
    _allocator.free(nativeName);
  }

  String getName() {
    Pointer<Utf8> name = _bindings.spine_skin_get_name(_skin).cast();
    return name.toDartString();
  }

  void addSkin(Skin other) {
    _bindings.spine_skin_add_skin(_skin, other._skin);
  }

  List<SkinEntry> getEntries() {
    List<SkinEntry> result = [];
    final entries = _bindings.spine_skin_get_entries(_skin);
    int numEntries = _bindings.spine_skin_entries_get_num_entries(entries);
    for (int i = 0; i < numEntries; i++) {
      final entry = _bindings.spine_skin_entries_get_entry(entries, i);
      Pointer<Utf8> name = _bindings.spine_skin_entry_get_name(entry).cast();
      result.add(SkinEntry(_bindings.spine_skin_entry_get_slot_index(entry), name.toDartString(),
          _bindings.spine_skin_entry_get_attachment(entry).address == nullptr.address ? null : Attachment._toSubclass(_bindings.spine_skin_entry_get_attachment(entry))));
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

  void copy(Skin other) {
    _bindings.spine_skin_copy_skin(_skin, other._skin);
  }
}

class ConstraintData<T extends Pointer> {
  final T _data;

  ConstraintData._(this._data);

  String getName() {
    final Pointer<Utf8> name = _bindings.spine_constraint_data_get_name(_data.cast()).cast();
    return name.toDartString();
  }

  int getOrder() {
    return _bindings.spine_constraint_data_get_order(_data.cast());
  }

  void setOrder(int order) {
    _bindings.spine_constraint_data_set_order(_data.cast(), order);
  }

  bool isSkinRequired() {
    return _bindings.spine_constraint_data_get_is_skin_required(_data.cast()) == 1;
  }

  void setIsSkinRequired(bool isSkinRequired) {
    _bindings.spine_constraint_data_set_is_skin_required(_data.cast(), isSkinRequired ? -1 : 0);
  }
}

class IkConstraintData extends ConstraintData<spine_ik_constraint_data> {
  IkConstraintData._(spine_ik_constraint_data data) : super._(data);

  List<BoneData> getBones() {
    final List<BoneData> result = [];
    final numBones = _bindings.spine_ik_constraint_data_get_num_bones(_data);
    final nativeBones = _bindings.spine_ik_constraint_data_get_bones(_data);
    for (int i = 0; i < numBones; i++) {
      result.add(BoneData._(nativeBones[i]));
    }
    return result;
  }

  BoneData getTarget() {
    return BoneData._(_bindings.spine_ik_constraint_data_get_target(_data));
  }

  void setTarget(BoneData target) {
    _bindings.spine_ik_constraint_data_set_target(_data, target._data);
  }

  int getBendDirection() {
    return _bindings.spine_ik_constraint_data_get_bend_direction(_data);
  }

  void setBendDirection(int bendDirection) {
    _bindings.spine_ik_constraint_data_set_bend_direction(_data, bendDirection);
  }

  bool getCompress() {
    return _bindings.spine_ik_constraint_data_get_compress(_data) == -1;
  }

  void setCompress(bool compress) {
    _bindings.spine_ik_constraint_data_set_compress(_data, compress ? -1 : 0);
  }

  bool getStretch() {
    return _bindings.spine_ik_constraint_data_get_stretch(_data) == -1;
  }

  void setStretch(bool stretch) {
    _bindings.spine_ik_constraint_data_set_stretch(_data, stretch ? -1 : 0);
  }

  bool getUniform() {
    return _bindings.spine_ik_constraint_data_get_uniform(_data) == -1;
  }

  void setUniform(bool uniform) {
    _bindings.spine_ik_constraint_data_set_uniform(_data, uniform ? -1 : 0);
  }

  double getMix() {
    return _bindings.spine_ik_constraint_data_get_mix(_data);
  }

  void setMix(double mix) {
    _bindings.spine_ik_constraint_data_set_mix(_data, mix);
  }

  double getSoftness() {
    return _bindings.spine_ik_constraint_data_get_softness(_data);
  }

  void setSoftness(double softness) {
    _bindings.spine_ik_constraint_data_set_softness(_data, softness);
  }
}

class IkConstraint {
  final spine_ik_constraint _constraint;

  IkConstraint._(this._constraint);

  void update() {
    _bindings.spine_ik_constraint_update(_constraint);
  }

  int getOrder() {
    return _bindings.spine_ik_constraint_get_order(_constraint);
  }

  IkConstraintData getData() {
    return IkConstraintData._(_bindings.spine_ik_constraint_get_data(_constraint));
  }

  List<Bone> getBones() {
    List<Bone> result = [];
    final num = _bindings.spine_ik_constraint_get_num_bones(_constraint);
    final nativeBones = _bindings.spine_ik_constraint_get_bones(_constraint);
    for (int i = 0; i < num; i++) {
      result.add(Bone._(nativeBones[i]));
    }
    return result;
  }

  Bone getTarget() {
    return Bone._(_bindings.spine_ik_constraint_get_target(_constraint));
  }

  void setTarget(Bone target) {
    _bindings.spine_ik_constraint_set_target(_constraint, target._bone);
  }

  int getBendDirection() {
    return _bindings.spine_ik_constraint_get_bend_direction(_constraint);
  }

  void setBendDirection(int bendDirection) {
    _bindings.spine_ik_constraint_set_bend_direction(_constraint, bendDirection);
  }

  bool getCompress() {
    return _bindings.spine_ik_constraint_get_compress(_constraint) == -1;
  }

  void setCompress(bool compress) {
    _bindings.spine_ik_constraint_set_compress(_constraint, compress ? -1 : 0);
  }

  bool getStretch() {
    return _bindings.spine_ik_constraint_get_stretch(_constraint) == -1;
  }

  void setStretch(bool stretch) {
    _bindings.spine_ik_constraint_set_stretch(_constraint, stretch ? -1 : 0);
  }

  double getMix() {
    return _bindings.spine_ik_constraint_get_mix(_constraint);
  }

  void setMix(double mix) {
    _bindings.spine_ik_constraint_set_mix(_constraint, mix);
  }

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

class TransformConstraintData extends ConstraintData<spine_transform_constraint_data> {
  TransformConstraintData._(spine_transform_constraint_data data) : super._(data);

  List<BoneData> getBones() {
    final List<BoneData> result = [];
    final numBones = _bindings.spine_transform_constraint_data_get_num_bones(_data);
    final nativeBones = _bindings.spine_transform_constraint_data_get_bones(_data);
    for (int i = 0; i < numBones; i++) {
      result.add(BoneData._(nativeBones[i]));
    }
    return result;
  }

  BoneData getTarget() {
    return BoneData._(_bindings.spine_transform_constraint_data_get_target(_data));
  }

  void setTarget(BoneData target) {
    _bindings.spine_transform_constraint_data_set_target(_data, target._data);
  }

  double getMixRotate() {
    return _bindings.spine_transform_constraint_data_get_mix_rotate(_data);
  }

  void setMixRotate(double mixRotate) {
    _bindings.spine_transform_constraint_data_set_mix_rotate(_data, mixRotate);
  }

  double getMixX() {
    return _bindings.spine_transform_constraint_data_get_mix_x(_data);
  }

  void setMixX(double mixX) {
    _bindings.spine_transform_constraint_data_set_mix_x(_data, mixX);
  }

  double getMixY() {
    return _bindings.spine_transform_constraint_data_get_mix_y(_data);
  }

  void setMixY(double mixY) {
    _bindings.spine_transform_constraint_data_set_mix_y(_data, mixY);
  }

  double getMixScaleX() {
    return _bindings.spine_transform_constraint_data_get_mix_scale_x(_data);
  }

  void setMixScaleX(double mixScaleX) {
    _bindings.spine_transform_constraint_data_set_mix_scale_x(_data, mixScaleX);
  }

  double getMixScaleY() {
    return _bindings.spine_transform_constraint_data_get_mix_scale_y(_data);
  }

  void setMixScaleY(double mixScaleY) {
    _bindings.spine_transform_constraint_data_set_mix_scale_y(_data, mixScaleY);
  }

  double getMixShearY() {
    return _bindings.spine_transform_constraint_data_get_mix_shear_y(_data);
  }

  void setMixShearY(double mixShearY) {
    _bindings.spine_transform_constraint_data_set_mix_shear_y(_data, mixShearY);
  }

  double getOffsetRotation() {
    return _bindings.spine_transform_constraint_data_get_offset_rotation(_data);
  }

  void setOffsetRotation(double offsetRotation) {
    _bindings.spine_transform_constraint_data_set_offset_rotation(_data, offsetRotation);
  }

  double getOffsetX() {
    return _bindings.spine_transform_constraint_data_get_offset_x(_data);
  }

  void setOffsetX(double offsetX) {
    _bindings.spine_transform_constraint_data_set_offset_x(_data, offsetX);
  }

  double getOffsetY() {
    return _bindings.spine_transform_constraint_data_get_offset_y(_data);
  }

  void setOffsetY(double offsetY) {
    _bindings.spine_transform_constraint_data_set_offset_y(_data, offsetY);
  }

  double getOffsetScaleX() {
    return _bindings.spine_transform_constraint_data_get_offset_scale_x(_data);
  }

  void setOffsetScaleX(double offsetScaleX) {
    _bindings.spine_transform_constraint_data_set_offset_x(_data, offsetScaleX);
  }

  double getOffsetScaleY() {
    return _bindings.spine_transform_constraint_data_get_offset_scale_y(_data);
  }

  void setOffsetScaleY(double offsetScaleY) {
    _bindings.spine_transform_constraint_data_set_offset_scale_y(_data, offsetScaleY);
  }

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

class TransformConstraint {
  final spine_transform_constraint _constraint;

  TransformConstraint._(this._constraint);

  void update() {
    _bindings.spine_transform_constraint_update(_constraint);
  }

  int getOrder() {
    return _bindings.spine_transform_constraint_get_order(_constraint);
  }

  TransformConstraintData getData() {
    return TransformConstraintData._(_bindings.spine_transform_constraint_get_data(_constraint));
  }

  List<Bone> getBones() {
    List<Bone> result = [];
    final num = _bindings.spine_transform_constraint_get_num_bones(_constraint);
    final nativeBones = _bindings.spine_transform_constraint_get_bones(_constraint);
    for (int i = 0; i < num; i++) {
      result.add(Bone._(nativeBones[i]));
    }
    return result;
  }

  Bone getTarget() {
    return Bone._(_bindings.spine_transform_constraint_get_target(_constraint));
  }

  void setTarget(Bone target) {
    _bindings.spine_transform_constraint_set_target(_constraint, target._bone);
  }

  double getMixRotate() {
    return _bindings.spine_transform_constraint_get_mix_rotate(_constraint);
  }

  void setMixRotate(double mixRotate) {
    _bindings.spine_transform_constraint_set_mix_rotate(_constraint, mixRotate);
  }

  double getMixX() {
    return _bindings.spine_transform_constraint_get_mix_x(_constraint);
  }

  void setMixX(double mixX) {
    _bindings.spine_transform_constraint_set_mix_x(_constraint, mixX);
  }

  double getMixY() {
    return _bindings.spine_transform_constraint_get_mix_y(_constraint);
  }

  void setMixY(double mixY) {
    _bindings.spine_transform_constraint_set_mix_y(_constraint, mixY);
  }

  double getMixScaleX() {
    return _bindings.spine_transform_constraint_get_mix_scale_x(_constraint);
  }

  void setMixScaleX(double mixScaleX) {
    _bindings.spine_transform_constraint_set_mix_scale_x(_constraint, mixScaleX);
  }

  double getMixScaleY() {
    return _bindings.spine_transform_constraint_get_mix_scale_y(_constraint);
  }

  void setMixScaleY(double mixScaleY) {
    _bindings.spine_transform_constraint_set_mix_scale_y(_constraint, mixScaleY);
  }

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

class PathConstraintData extends ConstraintData<spine_path_constraint_data> {
  PathConstraintData._(spine_path_constraint_data data) : super._(data);

  List<BoneData> getBones() {
    final List<BoneData> result = [];
    final numBones = _bindings.spine_path_constraint_data_get_num_bones(_data);
    final nativeBones = _bindings.spine_path_constraint_data_get_bones(_data);
    for (int i = 0; i < numBones; i++) {
      result.add(BoneData._(nativeBones[i]));
    }
    return result;
  }

  SlotData getTarget() {
    return SlotData._(_bindings.spine_path_constraint_data_get_target(_data));
  }

  void setTarget(SlotData target) {
    _bindings.spine_path_constraint_data_set_target(_data, target._data);
  }

  PositionMode getPositionMode() {
    return PositionMode.values[_bindings.spine_path_constraint_data_get_position_mode(_data)];
  }

  void setPositionMode(PositionMode positionMode) {
    _bindings.spine_path_constraint_data_set_position_mode(_data, positionMode.value);
  }

  SpacingMode getSpacingMode() {
    return SpacingMode.values[_bindings.spine_path_constraint_data_get_spacing_mode(_data)];
  }

  void setSpacingMode(SpacingMode spacingMode) {
    _bindings.spine_path_constraint_data_set_spacing_mode(_data, spacingMode.value);
  }

  RotateMode getRotateMode() {
    return RotateMode.values[_bindings.spine_path_constraint_data_get_rotate_mode(_data)];
  }

  void setRotateMode(RotateMode rotateMode) {
    _bindings.spine_path_constraint_data_set_rotate_mode(_data, rotateMode.value);
  }

  double getOffsetRotation() {
    return _bindings.spine_path_constraint_data_get_offset_rotation(_data);
  }

  void setOffsetRotation(double offsetRotation) {
    _bindings.spine_path_constraint_data_set_offset_rotation(_data, offsetRotation);
  }

  double getPosition() {
    return _bindings.spine_path_constraint_data_get_position(_data);
  }

  void setPosition(double position) {
    _bindings.spine_path_constraint_data_set_position(_data, position);
  }

  double getSpacing() {
    return _bindings.spine_path_constraint_data_get_spacing(_data);
  }

  void setSpacing(double spacing) {
    _bindings.spine_path_constraint_data_set_spacing(_data, spacing);
  }

  double getMixRotate() {
    return _bindings.spine_path_constraint_data_get_mix_rotate(_data);
  }

  void setMixRotate(double mixRotate) {
    _bindings.spine_path_constraint_data_set_mix_rotate(_data, mixRotate);
  }

  double getMixX() {
    return _bindings.spine_path_constraint_data_get_mix_x(_data);
  }

  void setMixX(double mixX) {
    _bindings.spine_path_constraint_data_set_mix_x(_data, mixX);
  }

  double getMixY() {
    return _bindings.spine_path_constraint_data_get_mix_x(_data);
  }

  void setMixY(double mixY) {
    _bindings.spine_path_constraint_data_set_mix_y(_data, mixY);
  }
}

class PathConstraint {
  final spine_path_constraint _constraint;

  PathConstraint._(this._constraint);

  void update() {
    _bindings.spine_path_constraint_update(_constraint);
  }

  int getOrder() {
    return _bindings.spine_path_constraint_get_order(_constraint);
  }

  List<Bone> getBones() {
    List<Bone> result = [];
    final num = _bindings.spine_path_constraint_get_num_bones(_constraint);
    final nativeBones = _bindings.spine_path_constraint_get_bones(_constraint);
    for (int i = 0; i < num; i++) {
      result.add(Bone._(nativeBones[i]));
    }
    return result;
  }

  Slot getTarget() {
    return Slot._(_bindings.spine_path_constraint_get_target(_constraint));
  }

  void setTarget(Slot target) {
    _bindings.spine_path_constraint_set_target(_constraint, target._slot);
  }

  double getPosition() {
    return _bindings.spine_path_constraint_get_position(_constraint);
  }

  void setPosition(double position) {
    _bindings.spine_path_constraint_set_position(_constraint, position);
  }

  double getSpacing() {
    return _bindings.spine_path_constraint_get_spacing(_constraint);
  }

  void setSpacing(double spacing) {
    _bindings.spine_path_constraint_set_spacing(_constraint, spacing);
  }

  double getMixRotate() {
    return _bindings.spine_path_constraint_get_mix_rotate(_constraint);
  }

  void setMixRotate(double mixRotate) {
    _bindings.spine_path_constraint_set_mix_rotate(_constraint, mixRotate);
  }

  double getMixX() {
    return _bindings.spine_path_constraint_get_mix_x(_constraint);
  }

  void setMixX(double mixX) {
    _bindings.spine_path_constraint_set_mix_x(_constraint, mixX);
  }

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

class Skeleton {
  final spine_skeleton _skeleton;

  Skeleton._(this._skeleton);

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
    final nameNative = boneName.toNativeUtf8(_allocator);
    final bone = _bindings.spine_skeleton_find_bone(_skeleton, nameNative.cast());
    _allocator.free(nameNative);
    if (bone.address == nullptr.address) return null;
    return Bone._(bone);
  }

  Slot? findSlot(String slotName) {
    final nameNative = slotName.toNativeUtf8(_allocator);
    final slot = _bindings.spine_skeleton_find_slot(_skeleton, nameNative.cast());
    _allocator.free(nameNative);
    if (slot.address == nullptr.address) return null;
    return Slot._(slot);
  }

  /// Attachments from the new skin are attached if the corresponding attachment from the old skin was attached.
  /// If there was no old skin, each slot's setup mode attachment is attached from the new skin.
  /// After changing the skin, the visible attachments can be reset to those attached in the setup pose by calling
  /// See Skeleton::setSlotsToSetupPose()
  /// Also, often AnimationState::apply(Skeleton&) is called before the next time the
  /// skeleton is rendered to allow any attachment keys in the current animation(s) to hide or show attachments from the new skin.
  /// @param skinName May be NULL.
  void setSkinByName(String skinName) {
    final nameNative = skinName.toNativeUtf8(_allocator);
    _bindings.spine_skeleton_set_skin_by_name(_skeleton, nameNative.cast());
    _allocator.free(nameNative);
  }

  void setSkin(Skin skin) {
    _bindings.spine_skeleton_set_skin(_skeleton, skin._skin);
  }

  Attachment? getAttachmentByName(String slotName, String attachmentName) {
    final slotNameNative = slotName.toNativeUtf8(_allocator);
    final attachmentNameNative = attachmentName.toNativeUtf8(_allocator);
    final attachment =
        _bindings.spine_skeleton_get_attachment_by_name(_skeleton, slotNameNative.cast(), attachmentNameNative.cast());
    _allocator.free(slotNameNative);
    _allocator.free(attachmentNameNative);
    if (attachment.address == nullptr.address) return null;
    return Attachment._toSubclass(attachment);
  }

  Attachment? getAttachment(int slotIndex, String attachmentName) {
    final attachmentNameNative = attachmentName.toNativeUtf8(_allocator);
    final attachment = _bindings.spine_skeleton_get_attachment(_skeleton, slotIndex, attachmentNameNative.cast());
    _allocator.free(attachmentNameNative);
    if (attachment.address == nullptr.address) return null;
    return Attachment._toSubclass(attachment);
  }

  void setAttachment(String slotName, String attachmentName) {
    final slotNameNative = slotName.toNativeUtf8(_allocator);
    final attachmentNameNative = attachmentName.toNativeUtf8(_allocator);
    _bindings.spine_skeleton_set_attachment(_skeleton, slotNameNative.cast(), attachmentNameNative.cast());
    _allocator.free(slotNameNative);
    _allocator.free(attachmentNameNative);
  }

  IkConstraint? findIkConstraint(String constraintName) {
    final nameNative = constraintName.toNativeUtf8(_allocator);
    final constraint = _bindings.spine_skeleton_find_ik_constraint(_skeleton, nameNative.cast());
    _allocator.free(nameNative);
    if (constraint.address == nullptr.address) return null;
    return IkConstraint._(constraint);
  }

  TransformConstraint? findTransformConstraint(String constraintName) {
    final nameNative = constraintName.toNativeUtf8(_allocator);
    final constraint = _bindings.spine_skeleton_find_transform_constraint(_skeleton, nameNative.cast());
    _allocator.free(nameNative);
    if (constraint.address == nullptr.address) return null;
    return TransformConstraint._(constraint);
  }

  PathConstraint? findPathConstraint(String constraintName) {
    final nameNative = constraintName.toNativeUtf8(_allocator);
    final constraint = _bindings.spine_skeleton_find_path_constraint(_skeleton, nameNative.cast());
    _allocator.free(nameNative);
    if (constraint.address == nullptr.address) return null;
    return PathConstraint._(constraint);
  }

  /// Returns the axis aligned bounding box (AABB) of the region and mesh attachments for the current pose.
  /// @param outX The horizontal distance between the skeleton origin and the left side of the AABB.
  /// @param outY The vertical distance between the skeleton origin and the bottom side of the AABB.
  /// @param outWidth The width of the AABB
  /// @param outHeight The height of the AABB.
  /// @param outVertexBuffer Reference to hold a Vector of floats. This method will assign it with new floats as needed.
  Bounds getBounds() {
    final nativeBounds = _bindings.spine_skeleton_get_bounds(_skeleton);
    final bounds = Bounds(
        _bindings.spine_bounds_get_x(nativeBounds),
        _bindings.spine_bounds_get_y(nativeBounds),
        _bindings.spine_bounds_get_width(nativeBounds),
        _bindings.spine_bounds_get_height(nativeBounds));
    _allocator.free(nativeBounds);
    return bounds;
  }

  Bone? getRootBone() {
    final bone = _bindings.spine_skeleton_get_root_bone(_skeleton);
    if (bone.address == nullptr.address) return null;
    return Bone._(bone);
  }

  SkeletonData? getData() {
    final data = _bindings.spine_skeleton_get_data(_skeleton);
    if (data.address == nullptr.address) return null;
    return SkeletonData._(data);
  }

  List<Bone> getBones() {
    final List<Bone> bones = [];
    final numBones = _bindings.spine_skeleton_get_num_bones(_skeleton);
    final nativeBones = _bindings.spine_skeleton_get_bones(_skeleton);
    for (int i = 0; i < numBones; i++) {
      bones.add(Bone._(nativeBones[i]));
    }
    return bones;
  }

  List<Slot> getSlots() {
    final List<Slot> slots = [];
    final numSlots = _bindings.spine_skeleton_get_num_slots(_skeleton);
    final nativeSlots = _bindings.spine_skeleton_get_slots(_skeleton);
    for (int i = 0; i < numSlots; i++) {
      slots.add(Slot._(nativeSlots[i]));
    }
    return slots;
  }

  List<Slot> getDrawOrder() {
    final List<Slot> slots = [];
    final numSlots = _bindings.spine_skeleton_get_num_draw_order(_skeleton);
    final nativeDrawOrder = _bindings.spine_skeleton_get_draw_order(_skeleton);
    for (int i = 0; i < numSlots; i++) {
      slots.add(Slot._(nativeDrawOrder[i]));
    }
    return slots;
  }

  List<IkConstraint> getIkConstraints() {
    final List<IkConstraint> constraints = [];
    final numConstraints = _bindings.spine_skeleton_get_num_ik_constraints(_skeleton);
    final nativeConstraints = _bindings.spine_skeleton_get_ik_constraints(_skeleton);
    for (int i = 0; i < numConstraints; i++) {
      constraints.add(IkConstraint._(nativeConstraints[i]));
    }
    return constraints;
  }

  List<PathConstraint> getPathConstraints() {
    final List<PathConstraint> constraints = [];
    final numConstraints = _bindings.spine_skeleton_get_num_path_constraints(_skeleton);
    final nativeConstraints = _bindings.spine_skeleton_get_path_constraints(_skeleton);
    for (int i = 0; i < numConstraints; i++) {
      constraints.add(PathConstraint._(nativeConstraints[i]));
    }
    return constraints;
  }

  List<TransformConstraint> getTransformConstraints() {
    final List<TransformConstraint> constraints = [];
    final numConstraints = _bindings.spine_skeleton_get_num_transform_constraints(_skeleton);
    final nativeConstraints = _bindings.spine_skeleton_get_transform_constraints(_skeleton);
    for (int i = 0; i < numConstraints; i++) {
      constraints.add(TransformConstraint._(nativeConstraints[i]));
    }
    return constraints;
  }

  Skin? getSkin() {
    final skin = _bindings.spine_skeleton_get_skin(_skeleton);
    if (skin.address == nullptr.address) return null;
    return Skin._(skin);
  }

  Color getColor() {
    final color = _bindings.spine_skeleton_get_color(_skeleton);
    return Color(
        _bindings.spine_color_get_r(color),
        _bindings.spine_color_get_g(color),
        _bindings.spine_color_get_b(color),
        _bindings.spine_color_get_a(color));
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

// FIXME expose timelines and apply?
class Animation {
  final spine_animation _animation;

  Animation._(this._animation);

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

  TrackEntry._(this._entry, this._state);

  /// The index of the track where this entry is either current or queued.
  int getTtrackIndex() {
    return _bindings.spine_track_entry_get_track_index(_entry);
  }

  /// The animation to apply for this track entry.
  Animation getAnimation() {
    return Animation._(_bindings.spine_track_entry_get_animation(_entry));
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
    return _bindings.spine_track_entry_get_time_scale(_entry);
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
    return TrackEntry._(next, _state);
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
    return TrackEntry._(from, _state);
  }

  /// The track entry for the next animation when mixing from this animation, or NULL if no mixing is currently occuring.
  /// When mixing from multiple animations, MixingTo makes up a double linked list with MixingFrom.
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

  double getTrackComplete() {
    return _bindings.spine_track_entry_get_track_complete(_entry);
  }

  void setListener(AnimationStateListener? listener) {
    _state._setTrackEntryListener(_entry, listener);
  }
}

enum EventType { Start, Interrupt, End, Complete, Dispose, Event }

class EventData {
  final spine_event_data _data;

  EventData._(this._data);

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
    final nativeString = value.toNativeUtf8(_allocator);
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

class Event {
  final spine_event _event;

  Event._(this._event);

  EventData getData() {
    return EventData._(_bindings.spine_event_get_data(_event));
  }

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
    final nativeString = value.toNativeUtf8(_allocator);
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

typedef AnimationStateListener = void Function(EventType type, TrackEntry entry, Event? event);

class AnimationStateData {
  spine_animation_state_data _data;

  AnimationStateData._(this._data);

  SkeletonData getSkeletonData() {
    return SkeletonData._(_bindings.spine_animation_state_data_get_skeleton_data(_data));
  }

  double getDefaultMix() {
    return _bindings.spine_animation_state_data_get_default_mix(_data);
  }

  void setDefaultMix(double defaultMix) {
    _bindings.spine_animation_state_data_set_default_mix(_data, defaultMix);
  }

  void setMixByName(String fromName, String toName, double duration) {
    final fromNative = fromName.toNativeUtf8(_allocator);
    final toNative = toName.toNativeUtf8(_allocator);
    _bindings.spine_animation_state_data_set_mix_by_name(_data, fromNative.cast(), toNative.cast(), duration);
    _allocator.free(fromNative);
    _allocator.free(toNative);
  }

  double getMixByName(String fromName, String toName) {
    final fromNative = fromName.toNativeUtf8(_allocator);
    final toNative = toName.toNativeUtf8(_allocator);
    final duration = _bindings.spine_animation_state_data_get_mix_by_name(_data, fromNative.cast(), toNative.cast());
    _allocator.free(fromNative);
    _allocator.free(toNative);
    return duration;
  }

  void setMix(Animation from, Animation to, double duration) {
    _bindings.spine_animation_state_data_set_mix(_data, from._animation, to._animation, duration);
  }

  double getMix(Animation from, Animation to) {
    return _bindings.spine_animation_state_data_get_mix(_data, from._animation, to._animation);
  }

  void clear() {
    _bindings.spine_animation_state_data_clear(_data);
  }
}

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

  /// Increments the track entry times, setting queued animations as current if needed
  /// @param delta delta time
  void update(double delta) {
    _bindings.spine_animation_state_update(_state, delta);

    final numEvents = _bindings.spine_animation_state_events_get_num_events(_events);
    if (numEvents > 0) {
      for (int i = 0; i < numEvents; i++) {
        late final EventType type;
        switch (_bindings.spine_animation_state_events_get_event_type(_events, i)) {
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
        final nativeEntry = _bindings.spine_animation_state_events_get_track_entry(_events, i);
        final entry = TrackEntry._(nativeEntry, this);
        final nativeEvent = _bindings.spine_animation_state_events_get_event(_events, i);
        final event = nativeEvent.address == nullptr.address ? null : Event._(nativeEvent);
        if (_trackEntryListeners.containsKey(nativeEntry)) {
          _trackEntryListeners[entry]?.call(type, entry, event);
        }
        if (_stateListener != null) {
          _stateListener?.call(type, entry, event);
        }
        if (type == EventType.Dispose) {
          _bindings.spine_animation_state_dispose_track_entry(_state, nativeEntry);
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
  TrackEntry setAnimationByName(int trackIndex, String animationName, bool loop) {
    final animation = animationName.toNativeUtf8(_allocator);
    final entry =
        _bindings.spine_animation_state_set_animation_by_name(_state, trackIndex, animation.cast(), loop ? -1 : 0);
    _allocator.free(animation);
    if (entry.address == nullptr.address) throw Exception("Couldn't set animation $animationName");
    return TrackEntry._(entry, this);
  }

  TrackEntry setAnimation(int trackIndex, Animation animation, bool loop) {
    final entry =
        _bindings.spine_animation_state_set_animation(_state, trackIndex, animation._animation, loop ? -1 : 0);
    if (entry.address == nullptr.address) throw Exception("Couldn't set animation ${animation.getName()}");
    return TrackEntry._(entry, this);
  }

  /// Adds an animation to be played delay seconds after the current or last queued animation
  /// for a track. If the track is empty, it is equivalent to calling setAnimation.
  /// @param delay
  /// Seconds to begin this animation after the start of the previous animation. May be &lt;= 0 to use the animation
  /// duration of the previous track minus any mix duration plus the negative delay.
  ///
  /// @return A track entry to allow further customization of animation playback. References to the track entry must not be kept
  /// after AnimationState.Dispose
  TrackEntry addAnimationByName(int trackIndex, String animationName, bool loop, double delay) {
    final animation = animationName.toNativeUtf8(_allocator);
    final entry = _bindings.spine_animation_state_add_animation_by_name(
        _state, trackIndex, animation.cast(), loop ? -1 : 0, delay);
    _allocator.free(animation);
    if (entry.address == nullptr.address) throw Exception("Couldn't add animation $animationName");
    return TrackEntry._(entry, this);
  }

  TrackEntry addAnimation(int trackIndex, Animation animation, bool loop, double delay) {
    final entry =
        _bindings.spine_animation_state_add_animation(_state, trackIndex, animation._animation, loop ? -1 : 0, delay);
    if (entry.address == nullptr.address) throw Exception("Couldn't add animation ${animation.getName()}");
    return TrackEntry._(entry, this);
  }

  /// Sets an empty animation for a track, discarding any queued animations, and mixes to it over the specified mix duration.
  TrackEntry setEmptyAnimation(int trackIndex, double mixDuration) {
    final entry = _bindings.spine_animation_state_set_empty_animation(_state, trackIndex, mixDuration);
    return TrackEntry._(entry, this);
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
    return TrackEntry._(entry, this);
  }

  TrackEntry? getCurrent(int trackIndex) {
    final entry = _bindings.spine_animation_state_get_current(_state, trackIndex);
    if (entry.address == nullptr.address) return null;
    return TrackEntry._(entry, this);
  }

  int getNumTracks() {
    return _bindings.spine_animation_state_get_num_tracks(_state);
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

  AnimationStateData getData() {
    return AnimationStateData._(_bindings.spine_animation_state_get_data(_state));
  }

  void setListener(AnimationStateListener? listener) {
    _stateListener = listener;
  }
}

// FIXME add callbacks for update, apply and updateWorldTransform. Pass through SpineWidgetController
class SkeletonDrawable {
  final Atlas atlas;
  final SkeletonData skeletonData;
  late final spine_skeleton_drawable _drawable;
  late final Skeleton skeleton;
  late final AnimationStateData animationStateData;
  late final AnimationState animationState;
  final bool _ownsAtlasAndSkeletonData;
  bool _disposed;

  SkeletonDrawable(this.atlas, this.skeletonData, this._ownsAtlasAndSkeletonData) : _disposed = false {
    _drawable = _bindings.spine_skeleton_drawable_create(skeletonData._data);
    skeleton = Skeleton._(_bindings.spine_skeleton_drawable_get_skeleton(_drawable));
    animationStateData = AnimationStateData._(_bindings.spine_skeleton_drawable_get_animation_state_data(_drawable));
    animationState = AnimationState._(_bindings.spine_skeleton_drawable_get_animation_state(_drawable), _bindings.spine_skeleton_drawable_get_animation_state_events(_drawable));
    skeleton.updateWorldTransform();
  }

  static Future<SkeletonDrawable> fromAsset(String atlasFile, String skeletonFile, {AssetBundle? bundle}) async {
    bundle ??= rootBundle;
    var atlas = await Atlas.fromAsset(atlasFile, bundle: bundle);
    var skeletonData = await SkeletonData.fromAsset(atlas, skeletonFile, bundle: bundle);
    return SkeletonDrawable(atlas, skeletonData, true);
  }

  static Future<SkeletonDrawable> fromFile(String atlasFile, String skeletonFile) async {
    var atlas = await Atlas.fromFile(atlasFile);
    var skeletonData = await SkeletonData.fromFile(atlas, skeletonFile);
    return SkeletonDrawable(atlas, skeletonData, true);
  }

  static Future<SkeletonDrawable> fromHttp(String atlasFile, String skeletonFile) async {
    var atlas = await Atlas.fromUrl(atlasFile);
    var skeletonData = await SkeletonData.fromHttp(atlas, skeletonFile);
    return SkeletonDrawable(atlas, skeletonData, true);
  }

  void update(double delta) {
    if (_disposed) return;
    animationState.update(delta);
    animationState.apply(skeleton);
    skeleton.updateWorldTransform();
  }

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

  void renderToCanvas(Canvas canvas) {
    var commands = render();
    for (final cmd in commands) {
      canvas.drawVertices(
          cmd.vertices, rendering.BlendMode.modulate, atlas.atlasPagePaints[cmd.atlasPageIndex]);
    }
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

  RenderCommand._(spine_render_command nativeCmd, double pageWidth, double pageHeight) {
    atlasPageIndex = _bindings.spine_render_command_get_atlas_page(nativeCmd);
    int numVertices = _bindings.spine_render_command_get_num_vertices(nativeCmd);
    int numIndices = _bindings.spine_render_command_get_num_indices(nativeCmd);
    final positions = _bindings.spine_render_command_get_positions(nativeCmd).asTypedList(numVertices * 2);
    final uvs = _bindings.spine_render_command_get_uvs(nativeCmd).asTypedList(numVertices * 2);
    for (int i = 0; i < numVertices * 2; i += 2) {
      uvs[i] *= pageWidth;
      uvs[i + 1] *= pageHeight;
    }
    final colors = _bindings.spine_render_command_get_colors(nativeCmd).asTypedList(numVertices);
    final indices = _bindings.spine_render_command_get_indices(nativeCmd).asTypedList(numIndices);

    if (!kIsWeb) {
      // We pass the native data as views directly to Vertices.raw. According to the sources, the data
      // is copied, so it doesn't matter that we free up the underlying memory on the next
      // render call. See the implementation of Vertices.raw() here:
      // https://github.com/flutter/engine/blob/5c60785b802ad2c8b8899608d949342d5c624952/lib/ui/painting/vertices.cc#L21
      vertices = Vertices.raw(VertexMode.triangles, positions,
          textureCoordinates: uvs,
          colors: _bindings.spine_render_command_get_colors(nativeCmd).asTypedList(numVertices),
          indices: _bindings.spine_render_command_get_indices(nativeCmd).asTypedList(numIndices));
    } else {
      // On the web, rendering is done through CanvasKit, which requires copies of the native data.
      final positionsCopy = Float32List.fromList(positions);
      final uvsCopy = Float32List.fromList(uvs);
      final colorsCopy = Int32List.fromList(colors);
      final indicesCopy = Uint16List.fromList(indices);
      vertices = Vertices.raw(VertexMode.triangles, positionsCopy,
          textureCoordinates: uvsCopy,
          colors: colorsCopy,
          indices: indicesCopy);
    }
  }
}