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
import 'package:path/path.dart' as Path;

int majorVersion() => _bindings.spine_major_version();
int minorVersion() => _bindings.spine_minor_version();
void reportLeaks() => _bindings.spine_report_leaks();

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
      throw Exception("Couldn't load atlas: " + message);
    }

    final atlasDir = Path.dirname(atlasFileName);
    List<Image> atlasPages = [];
    List<Paint> atlasPagePaints = [];
    for (int i = 0; i < atlas.ref.numImagePaths; i++) {
      final Pointer<Utf8> atlasPageFile = atlas.ref.imagePaths[i].cast();
      final imagePath = Path.join(atlasDir, atlasPageFile.toDartString());
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
  final Pointer<spine_skeleton_data> _skeletonData;
  bool _disposed;

  SkeletonData(this._skeletonData): _disposed = false;

  static SkeletonData fromJson(Atlas atlas, String json) {
    final jsonNative = json.toNativeUtf8();
    final skeletonData = _bindings.spine_skeleton_data_load_json(atlas._atlas, jsonNative.cast());
    if (skeletonData.ref.error.address != nullptr.address) {
      final Pointer<Utf8> error = skeletonData.ref.error.cast();
      final message = error.toDartString();
      _bindings.spine_skeleton_data_dispose(skeletonData);
      throw Exception("Couldn't load skeleton data: " + message);
    }
    return SkeletonData(skeletonData);
  }

  static SkeletonData fromBinary(Atlas atlas, Uint8List binary) {
    final Pointer<Uint8> binaryNative = malloc.allocate(binary.lengthInBytes);
    binaryNative.asTypedList(binary.lengthInBytes).setAll(0, binary);
    final skeletonData = _bindings.spine_skeleton_data_load_binary(atlas._atlas, binaryNative.cast(), binary.lengthInBytes);
    malloc.free(binaryNative);
    if (skeletonData.ref.error.address != nullptr.address) {
      final Pointer<Utf8> error = skeletonData.ref.error.cast();
      final message = error.toDartString();
      _bindings.spine_skeleton_data_dispose(skeletonData);
      throw Exception("Couldn't load skeleton data: " + message);
    }
    return SkeletonData(skeletonData);
  }

  void dispose() {
    if (_disposed) return;
    _disposed = true;
    _bindings.spine_skeleton_data_dispose(this._skeletonData);
  }
}

class Skeleton {
  final spine_skeleton _skeleton;

  Skeleton(this._skeleton);
}

class TrackEntry {
  final spine_track_entry _entry;

  TrackEntry(this._entry);
}

class AnimationState {
  final spine_animation_state _state;

  AnimationState(this._state);

  /// Increments the track entry times, setting queued animations as current if needed
  /// @param delta delta time
  void update(double delta) {
    _bindings.spine_animation_state_update(_state, delta);
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
    return TrackEntry(entry);
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
    return TrackEntry(entry);
  }

  /// Sets an empty animation for a track, discarding any queued animations, and mixes to it over the specified mix duration.
  TrackEntry setEmptyAnimation(int trackIndex, double mixDuration) {
    final entry = _bindings.spine_animation_state_set_empty_animation(_state, trackIndex, mixDuration);
    return TrackEntry(entry);
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
    return TrackEntry(entry);
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
}

class SkeletonDrawable {
  final Atlas atlas;
  final SkeletonData skeletonData;
  late final Pointer<spine_skeleton_drawable> _drawable;
  late final Skeleton skeleton;
  late final AnimationState animationState;
  final bool _ownsData;
  bool _disposed;

  SkeletonDrawable(this.atlas, this.skeletonData, this._ownsData): _disposed = false {
    _drawable = _bindings.spine_skeleton_drawable_create(skeletonData._skeletonData);
    skeleton = Skeleton(_drawable.ref.skeleton);
    animationState = AnimationState(_drawable.ref.animationState);
  }

  void update(double delta) {
    if (_disposed) return;
    _bindings.spine_skeleton_drawable_update(_drawable, delta);
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
    if (_ownsData) {
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
