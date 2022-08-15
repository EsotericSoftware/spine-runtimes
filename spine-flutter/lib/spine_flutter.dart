
import 'dart:ffi';
import 'dart:io';
import 'package:ffi/ffi.dart';

import 'package:flutter/services.dart';
import 'package:flutter/widgets.dart';
import 'spine_flutter_bindings_generated.dart';
import 'package:path/path.dart' as Path;

int majorVersion() => _bindings.spine_major_version();
int minorVersion() => _bindings.spine_minor_version();

Future<Pointer<spine_atlas>> loadAtlas(AssetBundle assetBundle, String atlasFileName) async {
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
  for (int i = 0; i < atlas.ref.numImagePaths; i++) {
    final Pointer<Utf8> atlasPageFile = atlas.ref.imagePaths[i].cast();
    final imagePath = Path.join(atlasDir, atlasPageFile.toDartString());
    atlasPages.add(await Image(image: AssetImage(imagePath)));
  }

  return atlas;
}

Pointer<spine_skeleton_data> loadSkeletonDataJson(Pointer<spine_atlas> atlas, String json) {
  final jsonNative = json.toNativeUtf8();
  final skeletonData = _bindings.spine_skeleton_data_load_json(atlas, jsonNative.cast());
  if (skeletonData.ref.error.address != nullptr.address) {
    final Pointer<Utf8> error = skeletonData.ref.error.cast();
    final message = error.toDartString();
    _bindings.spine_skeleton_data_dispose(skeletonData);
    throw Exception("Couldn't load skeleton data: " + message);
  }
  return skeletonData;
}

Pointer<spine_skeleton_data> loadSkeletonDataBinary(Pointer<spine_atlas> atlas, ByteData binary) {
  final Pointer<Uint8> binaryNative = malloc.allocate(binary.lengthInBytes);
  binaryNative.asTypedList(binary.lengthInBytes).setAll(0, binary.buffer.asUint8List());
  final skeletonData = _bindings.spine_skeleton_data_load_binary(atlas, binaryNative.cast(), binary.lengthInBytes);
  malloc.free(binaryNative);
  if (skeletonData.ref.error.address != nullptr.address) {
    final Pointer<Utf8> error = skeletonData.ref.error.cast();
    final message = error.toDartString();
    _bindings.spine_skeleton_data_dispose(skeletonData);
    throw Exception("Couldn't load skeleton data: " + message);
  }
  return skeletonData;
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