import 'spine_dart.dart';
export 'spine_dart.dart';
import 'raw_image_provider.dart';
export 'spine_widget.dart';
export 'raw_image_provider.dart';

import 'dart:convert' as convert;
import 'dart:io' if (dart.library.html) 'io_stub.dart';
import 'dart:typed_data';
import 'dart:ui';

import "package:universal_ffi/ffi.dart";

import 'package:flutter/foundation.dart' show kIsWeb;
import 'package:flutter/material.dart' as material;
import 'package:flutter/rendering.dart' as rendering;
import 'package:flutter/services.dart';
import 'package:http/http.dart' as http;
import 'package:path/path.dart' as path;

// Backwards compatibility
Future<void> initSpineFlutter({bool useStaticLinkage = false, bool enableMemoryDebugging = false}) async {
  await initSpineDart(useStaticLinkage: useStaticLinkage, enableMemoryDebugging: enableMemoryDebugging);
  return;
}

/// Flutter wrapper for Atlas that manages texture loading and Paint creation
class AtlasFlutter extends Atlas {
  static FilterQuality filterQuality = FilterQuality.low;
  final List<Image> atlasPages;
  final List<Map<BlendMode, Paint>> atlasPagePaints;
  bool _disposed = false;

  AtlasFlutter._(super.ptr, this.atlasPages, this.atlasPagePaints) : super.fromPointer();

  /// Loads an [AtlasFlutter] using a custom file loading function.
  ///
  /// This is the most flexible loading method that allows loading atlas data and images from any source
  /// (memory, custom storage, network with caching, etc.).
  ///
  /// Parameters:
  /// - [atlasFileName]: The path/name of the atlas file. This is passed to [loadFile] to load the atlas data.
  /// - [loadFile]: A function that takes a filename and returns the file data as [Uint8List].
  ///   This function will be called once for the atlas file, and once for each atlas page image.
  ///   The image paths are relative to the atlas file's directory (as specified in the atlas file).
  ///
  /// Example - Loading from memory:
  /// ```dart
  /// final atlasData = Uint8List.fromList([...]); // Your atlas file data
  /// final page1Data = Uint8List.fromList([...]); // Your first page image data
  /// final page2Data = Uint8List.fromList([...]); // Your second page image data
  ///
  /// final atlas = await AtlasFlutter.fromMemory(
  ///   'character.atlas',
  ///   (filename) async {
  ///     if (filename == 'character.atlas') return atlasData;
  ///     if (filename == 'character.png') return page1Data;
  ///     if (filename == 'character2.png') return page2Data;
  ///     throw Exception('Unknown file: $filename');
  ///   },
  /// );
  /// ```
  ///
  /// Note: The [loadFile] function receives the full relative path for images
  /// (e.g., "directory/page.png" if the atlas file specifies that path).
  static Future<AtlasFlutter> fromMemory(String atlasFileName, Future<Uint8List> Function(String name) loadFile) async {
    // Load atlas data
    final atlasBytes = await loadFile(atlasFileName);
    final atlasData = convert.utf8.decode(atlasBytes);
    final atlas = loadAtlas(atlasData);

    // Load images for each atlas page
    final atlasDir = path.dirname(atlasFileName);
    final pages = <Image>[];
    final paints = <Map<BlendMode, Paint>>[];

    // Load images for each atlas page
    for (int i = 0; i < atlas.pages.length; i++) {
      final page = atlas.pages[i];
      if (page == null) continue;

      // Get the texture path from the atlas page
      final texturePath = page.texturePath;
      final imagePath = "$atlasDir/$texturePath";

      final imageData = await loadFile(imagePath);
      final codec = await instantiateImageCodec(imageData);
      final frameInfo = await codec.getNextFrame();
      final image = frameInfo.image;
      pages.add(image);

      // Create paints for each blend mode
      final pagePaints = <BlendMode, Paint>{};
      for (final blendMode in BlendMode.values) {
        pagePaints[blendMode] = Paint()
          ..shader = ImageShader(
            image,
            TileMode.clamp,
            TileMode.clamp,
            Matrix4.identity().storage,
            filterQuality: filterQuality,
          )
          ..isAntiAlias = true
          ..blendMode = blendMode.toFlutterBlendMode();
      }
      paints.add(pagePaints);
    }

    return AtlasFlutter._(atlas.nativePtr.cast(), pages, paints);
  }

  /// Loads an [AtlasFlutter] from the file [atlasFileName] in the root bundle or the optionally provided [bundle].
  static Future<AtlasFlutter> fromAsset(String atlasFileName, {AssetBundle? bundle}) async {
    bundle ??= rootBundle;
    return fromMemory(atlasFileName, (file) async => (await bundle!.load(file)).buffer.asUint8List());
  }

  /// Loads an [AtlasFlutter] from the file [atlasFileName].
  static Future<AtlasFlutter> fromFile(String atlasFileName) async {
    if (kIsWeb) {
      throw UnsupportedError('File operations are not supported on web. Use fromAsset or fromHttp instead.');
    }
    return fromMemory(atlasFileName, (file) => File(file).readAsBytes());
  }

  /// Loads an [AtlasFlutter] from the URL [atlasURL].
  static Future<AtlasFlutter> fromHttp(String atlasURL) async {
    return fromMemory(atlasURL, (file) async {
      final response = await http.get(Uri.parse(file));
      if (response.statusCode != 200) {
        throw Exception('Failed to load $file: ${response.statusCode}');
      }
      return response.bodyBytes;
    });
  }

  /// Disposes all resources including the native atlas and images
  @override
  void dispose() {
    if (_disposed) return;
    _disposed = true;
    super.dispose();
    for (final image in atlasPages) {
      image.dispose();
    }
    atlasPagePaints.clear();
  }
}

/// Flutter wrapper for SkeletonData that provides convenient loading methods
class SkeletonDataFlutter extends SkeletonData {
  SkeletonDataFlutter._(super.ptr) : super.fromPointer();

  /// Loads a [SkeletonDataFlutter] using a custom file loading function.
  ///
  /// This is the most flexible loading method that allows loading skeleton data from any source
  /// (memory, custom storage, network with caching, etc.).
  ///
  /// Parameters:
  /// - [atlas]: The [AtlasFlutter] to use for resolving attachment images.
  /// - [skeletonFile]: The path/name of the skeleton file. This is passed to [loadFile] to load the skeleton data.
  /// - [loadFile]: A function that takes a filename and returns the file data as [Uint8List].
  ///
  /// Example - Loading from memory:
  /// ```dart
  /// final skeletonData = Uint8List.fromList([...]); // Your skeleton file data
  ///
  /// final skeleton = await SkeletonDataFlutter.fromMemory(
  ///   atlas,
  ///   'character.json',
  ///   (filename) async {
  ///     if (filename == 'character.json') return skeletonData;
  ///     throw Exception('Unknown file: $filename');
  ///   },
  /// );
  /// ```
  ///
  /// Throws an [Exception] in case the skeleton data could not be loaded.
  static Future<SkeletonDataFlutter> fromMemory(
    AtlasFlutter atlas,
    String skeletonFile,
    Future<Uint8List> Function(String name) loadFile,
  ) async {
    final fileData = await loadFile(skeletonFile);
    if (skeletonFile.endsWith(".json")) {
      final jsonData = convert.utf8.decode(fileData);
      final skeletonData = loadSkeletonDataJson(atlas, jsonData, path: skeletonFile);
      return SkeletonDataFlutter._(skeletonData.nativePtr.cast());
    } else {
      final skeletonData = loadSkeletonDataBinary(atlas, fileData, path: skeletonFile);
      return SkeletonDataFlutter._(skeletonData.nativePtr.cast());
    }
  }

  /// Loads a [SkeletonDataFlutter] from the file [skeletonFile] in the root bundle or the optionally provided [bundle].
  /// Uses the provided [atlasFlutter] to resolve attachment images.
  ///
  /// Throws an [Exception] in case the skeleton data could not be loaded.
  static Future<SkeletonDataFlutter> fromAsset(AtlasFlutter atlas, String skeletonFile, {AssetBundle? bundle}) async {
    bundle ??= rootBundle;
    return fromMemory(atlas, skeletonFile, (file) async => (await bundle!.load(file)).buffer.asUint8List());
  }

  /// Loads a [SkeletonDataFlutter] from the file [skeletonFile]. Uses the provided [atlasFlutter] to resolve attachment images.
  ///
  /// Throws an [Exception] in case the skeleton data could not be loaded.
  static Future<SkeletonDataFlutter> fromFile(AtlasFlutter atlasFlutter, String skeletonFile) async {
    if (kIsWeb) {
      throw UnsupportedError('File operations are not supported on web. Use fromAsset or fromHttp instead.');
    }
    return fromMemory(atlasFlutter, skeletonFile, (file) => File(file).readAsBytes());
  }

  /// Loads a [SkeletonDataFlutter] from the URL [skeletonURL]. Uses the provided [atlasFlutter] to resolve attachment images.
  ///
  /// Throws an [Exception] in case the skeleton data could not be loaded.
  static Future<SkeletonDataFlutter> fromHttp(AtlasFlutter atlasFlutter, String skeletonURL) async {
    return fromMemory(atlasFlutter, skeletonURL, (file) async {
      final response = await http.get(Uri.parse(file));
      if (response.statusCode != 200) {
        throw Exception('Failed to load $file: ${response.statusCode}');
      }
      return response.bodyBytes;
    });
  }
}

/// Extension to convert Spine BlendMode to Flutter BlendMode
extension BlendModeExtensions on BlendMode {
  rendering.BlendMode toFlutterBlendMode() {
    switch (this) {
      case BlendMode.normal:
        return rendering.BlendMode.srcOver;
      case BlendMode.additive:
        return rendering.BlendMode.plus;
      case BlendMode.multiply:
        return rendering.BlendMode.multiply;
      case BlendMode.screen:
        return rendering.BlendMode.screen;
    }
  }
}

/// Flutter-specific render command that wraps the native RenderCommand and provides
/// Flutter Vertices for efficient rendering.
class RenderCommandFlutter {
  final RenderCommand _nativeCommand;
  late final Vertices vertices;
  late final int atlasPageIndex;
  late final BlendMode blendMode;

  RenderCommandFlutter._(this._nativeCommand, double pageWidth, double pageHeight) {
    // Get atlas page index from texture pointer (which is actually the page index when using spine_atlas_load)
    final texturePtr = _nativeCommand.texture;
    atlasPageIndex = texturePtr?.address ?? 0;

    final numVertices = _nativeCommand.numVertices;
    final numIndices = _nativeCommand.numIndices;

    // Get native data pointers
    final positionsPtr = _nativeCommand.positions;
    final uvsPtr = _nativeCommand.uvs;
    final colorsPtr = _nativeCommand.colors;
    final indicesPtr = _nativeCommand.indices;

    if (positionsPtr == null || uvsPtr == null || colorsPtr == null || indicesPtr == null) {
      throw Exception('Invalid render command data');
    }

    // Convert to typed lists
    final positions = positionsPtr.asTypedList(numVertices * 2);
    final uvs = uvsPtr.asTypedList(numVertices * 2);
    final indices = indicesPtr.asTypedList(numIndices);

    // Scale UVs by texture dimensions
    for (int i = 0; i < numVertices * 2; i += 2) {
      uvs[i] *= pageWidth;
      uvs[i + 1] *= pageHeight;
    }

    // Get blend mode
    blendMode = _nativeCommand.blendMode;

    // Handle colors - convert Uint32 to Int32 view without copying
    final colorsUint32 = colorsPtr.asTypedList(numVertices);
    final colors = Int32List.view(colorsUint32.buffer, colorsUint32.offsetInBytes, colorsUint32.length);

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
      //
      // If the vertex color equals (1, 1, 1, 1), we do not store
      // colors, which will trigger the fast path in Impeller. Otherwise we have to go the slow path, which
      // has to render to an offscreen surface.
      if (colors.isNotEmpty && colors[0] == -1) {
        // Fast path: no vertex colors (all white)
        vertices = Vertices.raw(VertexMode.triangles, positions, textureCoordinates: uvs, indices: indices);
      } else {
        vertices = Vertices.raw(
          VertexMode.triangles,
          positions,
          textureCoordinates: uvs,
          colors: colors,
          indices: indices,
        );
      }
    } else {
      // On web, we need to copy the data
      final positionsCopy = Float32List.fromList(positions);
      final uvsCopy = Float32List.fromList(uvs);
      final colorsCopy = Int32List.fromList(colors);
      final indicesCopy = Uint16List.fromList(indices);
      vertices = Vertices.raw(
        VertexMode.triangles,
        positionsCopy,
        textureCoordinates: uvsCopy,
        colors: colorsCopy,
        indices: indicesCopy,
      );
    }
  }
}

/// A SkeletonDrawable bundles loading, updating, and rendering an [AtlasFlutter], [Skeleton], and [AnimationState]
/// into a single easy to use class.
///
/// Use the [fromAsset], [fromFile], or [fromHttp] methods to construct a SkeletonDrawable. To have
/// multiple skeleton drawable instances share the same [AtlasFlutter] and [SkeletonDataFlutter], use the constructor.
///
/// You can then directly access the [atlasFlutter], [skeletonDataFlutter], [skeleton], [animationStateData], and [animationState]
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
/// the skeleton drawable was constructed from a shared [AtlasFlutter] and [SkeletonDataFlutter], make sure to dispose the
/// atlas and skeleton data as well, if no skeleton drawable references them anymore.
class SkeletonDrawableFlutter extends SkeletonDrawable {
  final AtlasFlutter atlasFlutter;
  final SkeletonData skeletonData;
  final bool _ownsAtlasAndSkeletonData;
  bool _disposed = false;

  /// Constructs a new skeleton drawable from the given (possibly shared) [AtlasFlutter] and [SkeletonDataFlutter]. If
  /// the atlas and skeleton data are not shared, the drawable can take ownership by passing true for [_ownsAtlasAndSkeletonData].
  /// In that case a call to [dispose] will also dispose the atlas and skeleton data.
  SkeletonDrawableFlutter(this.atlasFlutter, this.skeletonData, this._ownsAtlasAndSkeletonData) : super(skeletonData);

  /// Constructs a new skeleton drawable using a custom file loading function.
  ///
  /// This is the most flexible loading method that allows loading atlas and skeleton data from any source
  /// (memory, custom storage, network with caching, etc.).
  ///
  /// Parameters:
  /// - [atlasFile]: The path/name of the atlas file. This is passed to [loadFile] to load the atlas data.
  /// - [skeletonFile]: The path/name of the skeleton file. This is passed to [loadFile] to load the skeleton data.
  /// - [loadFile]: A function that takes a filename and returns the file data as [Uint8List].
  ///   This function will be called for the atlas file, skeleton file, and each atlas page image.
  ///
  /// Example - Loading from memory:
  /// ```dart
  /// final atlasData = Uint8List.fromList([...]); // Your atlas file data
  /// final skeletonData = Uint8List.fromList([...]); // Your skeleton file data
  /// final imageData = Uint8List.fromList([...]); // Your image file data
  ///
  /// final drawable = await SkeletonDrawableFlutter.fromMemory(
  ///   'character.atlas',
  ///   'character.json',
  ///   (filename) async {
  ///     if (filename == 'character.atlas') return atlasData;
  ///     if (filename == 'character.json') return skeletonData;
  ///     if (filename == 'character.png') return imageData;
  ///     throw Exception('Unknown file: $filename');
  ///   },
  /// );
  /// ```
  ///
  /// Throws an exception in case the data could not be loaded.
  static Future<SkeletonDrawableFlutter> fromMemory(
    String atlasFile,
    String skeletonFile,
    Future<Uint8List> Function(String name) loadFile,
  ) async {
    final atlasFlutter = await AtlasFlutter.fromMemory(atlasFile, loadFile);
    final skeletonDataFlutter = await SkeletonDataFlutter.fromMemory(atlasFlutter, skeletonFile, loadFile);
    return SkeletonDrawableFlutter(atlasFlutter, skeletonDataFlutter, true);
  }

  /// Constructs a new skeleton drawable from the [atlasFile] and [skeletonFile] from the root asset bundle
  /// or the optionally provided [bundle].
  ///
  /// Throws an exception in case the data could not be loaded.
  static Future<SkeletonDrawableFlutter> fromAsset(String atlasFile, String skeletonFile, {AssetBundle? bundle}) async {
    bundle ??= rootBundle;
    final atlasFlutter = await AtlasFlutter.fromAsset(atlasFile, bundle: bundle);
    final skeletonDataFlutter = await SkeletonDataFlutter.fromAsset(atlasFlutter, skeletonFile, bundle: bundle);
    return SkeletonDrawableFlutter(atlasFlutter, skeletonDataFlutter, true);
  }

  /// Constructs a new skeleton drawable from the [atlasFile] and [skeletonFile].
  ///
  /// Throws an exception in case the data could not be loaded.
  static Future<SkeletonDrawableFlutter> fromFile(String atlasFile, String skeletonFile) async {
    if (kIsWeb) {
      throw UnsupportedError('File operations are not supported on web. Use fromAsset or fromHttp instead.');
    }
    final atlasFlutter = await AtlasFlutter.fromFile(atlasFile);
    final skeletonDataFlutter = await SkeletonDataFlutter.fromFile(atlasFlutter, skeletonFile);
    return SkeletonDrawableFlutter(atlasFlutter, skeletonDataFlutter, true);
  }

  /// Constructs a new skeleton drawable from the [atlasUrl] and [skeletonUrl].
  ///
  /// Throws an exception in case the data could not be loaded.
  static Future<SkeletonDrawableFlutter> fromHttp(String atlasUrl, String skeletonUrl) async {
    final atlasFlutter = await AtlasFlutter.fromHttp(atlasUrl);
    final skeletonDataFlutter = await SkeletonDataFlutter.fromHttp(atlasFlutter, skeletonUrl);
    return SkeletonDrawableFlutter(atlasFlutter, skeletonDataFlutter, true);
  }

  /// Renders to current skeleton pose to a list of [RenderCommandFlutter] instances. The render commands
  /// can be rendered via [Canvas.drawVertices].
  List<RenderCommandFlutter> renderFlutter() {
    if (_disposed) return [];

    var commands = <RenderCommandFlutter>[];
    var nativeCmd = render();

    while (nativeCmd != null) {
      // Get page dimensions from atlas
      final pageIndex = nativeCmd.texture?.address ?? 0;
      final pages = atlasFlutter.pages;
      final page = pages[pageIndex];
      if (page != null) {
        commands.add(RenderCommandFlutter._(nativeCmd, page.width.toDouble(), page.height.toDouble()));
      } else {
        commands.add(RenderCommandFlutter._(nativeCmd, 1.0, 1.0));
      }
      nativeCmd = nativeCmd.next;
    }

    return commands;
  }

  /// Renders the skeleton drawable's current pose to the given [canvas]. Does not perform any
  /// scaling or fitting.
  List<RenderCommandFlutter> renderToCanvas(Canvas canvas) {
    var commands = renderFlutter();

    for (final cmd in commands) {
      // Get the paint for this atlas page and blend mode
      Paint? paint;
      if (cmd.atlasPageIndex < atlasFlutter.atlasPagePaints.length) {
        paint = atlasFlutter.atlasPagePaints[cmd.atlasPageIndex][cmd.blendMode];
      }

      // Fallback to a simple paint if textures aren't loaded
      paint ??= Paint()
        ..color = material.Colors.white
        ..style = PaintingStyle.fill;

      canvas.drawVertices(
        cmd.vertices,
        rendering.BlendMode.modulate,
        paint,
      );
    }
    return commands;
  }

  /// Renders the skeleton drawable's current pose to a [PictureRecorder] with the given [width] and [height].
  /// Uses [bgColor], a 32-bit ARGB color value, to paint the background.
  /// Scales and centers the skeleton to fit the within the bounds of [width] and [height].
  PictureRecorder renderToPictureRecorder(double width, double height, int bgColor) {
    var bounds = skeleton.bounds;
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
    var rawImageData = (await (await recorder.endRecording().toImage(
                  width.toInt(),
                  height.toInt(),
                ))
            .toByteData(format: ImageByteFormat.rawRgba))!
        .buffer
        .asUint8List();
    return RawImageData(rawImageData, width.toInt(), height.toInt());
  }

  /// Set a listener for all animation state events
  void setListener(AnimationStateListener? listener) {
    animationState.setListener(listener);
  }

  /// Disposes the skeleton drawable's resources. If the skeleton drawable owns the atlas
  /// and skeleton data, they are disposed as well. Must be called when the skeleton drawable
  /// is no longer in use.
  @override
  void dispose() {
    if (_disposed) return;
    _disposed = true;
    super.dispose();

    if (_ownsAtlasAndSkeletonData) {
      atlasFlutter.dispose();
      skeletonData.dispose();
    }
  }
}

/// Renders debug information for a [SkeletonDrawableFlutter], like bone locations, to a [Canvas].
/// See [DebugRenderer.render].
class DebugRenderer {
  const DebugRenderer();

  void render(SkeletonDrawableFlutter drawable, Canvas canvas, List<RenderCommandFlutter> commands) {
    final bonePaint = Paint()
      ..color = material.Colors.blue
      ..style = PaintingStyle.fill;
    for (final bone in drawable.skeleton.bones) {
      if (bone == null) continue;
      canvas.drawRect(
        Rect.fromCenter(center: Offset(bone.appliedPose.worldX, bone.appliedPose.worldY), width: 5, height: 5),
        bonePaint,
      );
    }
  }
}
