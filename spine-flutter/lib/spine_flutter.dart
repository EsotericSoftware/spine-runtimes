import 'dart:ffi';
import 'dart:io';
import 'dart:typed_data';
import 'dart:ui';
import 'package:ffi/ffi.dart';
import 'package:flutter/rendering.dart';

import 'package:flutter/services.dart';
import 'spine_flutter_bindings_generated.dart';
export 'spine_widget.dart';
import 'package:path/path.dart' as Path;

int majorVersion() => _bindings.spine_major_version();
int minorVersion() => _bindings.spine_minor_version();

class SpineAtlas {
  Pointer<spine_atlas> _atlas;
  List<Image> atlasPages;
  List<Paint> atlasPagePaints;

  SpineAtlas(this._atlas, this.atlasPages, this.atlasPagePaints);

  static Future<SpineAtlas> fromAsset(AssetBundle assetBundle, String atlasFileName) async {
    final atlasData = await assetBundle.loadString(atlasFileName);
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
      var imageData = await assetBundle.load(imagePath);
      final Codec codec = await instantiateImageCodec(imageData.buffer.asUint8List());
      final FrameInfo frameInfo = await codec.getNextFrame();
      final Image image = frameInfo.image;
      atlasPages.add(image);
      atlasPagePaints.add(Paint()
        ..shader = ImageShader(image, TileMode.clamp, TileMode.clamp, Matrix4.identity().storage, filterQuality: FilterQuality.high)
        ..isAntiAlias = true
      );
    }

    return SpineAtlas(atlas, atlasPages, atlasPagePaints);
  }
}

class SpineSkeletonData {
  Pointer<spine_skeleton_data> _skeletonData;

  SpineSkeletonData(this._skeletonData);

  static SpineSkeletonData fromJson(SpineAtlas atlas, String json) {
    final jsonNative = json.toNativeUtf8();
    final skeletonData = _bindings.spine_skeleton_data_load_json(atlas._atlas, jsonNative.cast());
    if (skeletonData.ref.error.address != nullptr.address) {
      final Pointer<Utf8> error = skeletonData.ref.error.cast();
      final message = error.toDartString();
      _bindings.spine_skeleton_data_dispose(skeletonData);
      throw Exception("Couldn't load skeleton data: " + message);
    }
    return SpineSkeletonData(skeletonData);
  }

  static SpineSkeletonData fromBinary(SpineAtlas atlas, ByteData binary) {
    final Pointer<Uint8> binaryNative = malloc.allocate(binary.lengthInBytes);
    binaryNative.asTypedList(binary.lengthInBytes).setAll(0, binary.buffer.asUint8List());
    final skeletonData = _bindings.spine_skeleton_data_load_binary(atlas._atlas, binaryNative.cast(), binary.lengthInBytes);
    malloc.free(binaryNative);
    if (skeletonData.ref.error.address != nullptr.address) {
      final Pointer<Utf8> error = skeletonData.ref.error.cast();
      final message = error.toDartString();
      _bindings.spine_skeleton_data_dispose(skeletonData);
      throw Exception("Couldn't load skeleton data: " + message);
    }
    return SpineSkeletonData(skeletonData);
  }
}

class SpineSkeletonDrawable {
  SpineAtlas atlas;
  SpineSkeletonData skeletonData;
  late Pointer<spine_skeleton_drawable> _drawable;

  SpineSkeletonDrawable(this.atlas, this.skeletonData) {
    _drawable = _bindings.spine_skeleton_drawable_create(skeletonData._skeletonData);
  }

  void update(double delta) {
    _bindings.spine_skeleton_drawable_update(_drawable, delta);
  }

  List<SpineRenderCommand> render() {
    Pointer<spine_render_command> nativeCmd = _bindings.spine_skeleton_drawable_render(_drawable);
    List<SpineRenderCommand> commands = [];
    while(nativeCmd.address != nullptr.address) {
      final atlasPage = atlas.atlasPages[nativeCmd.ref.atlasPage];
      commands.add(SpineRenderCommand(nativeCmd, atlasPage.width.toDouble(), atlasPage.height.toDouble()));
      nativeCmd = nativeCmd.ref.next;
    }
    return commands;
  }
}

class SpineRenderCommand {
  late Vertices vertices;
  late int atlasPageIndex;

  SpineRenderCommand(Pointer<spine_render_command> nativeCmd, double pageWidth, double pageHeight) {
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
