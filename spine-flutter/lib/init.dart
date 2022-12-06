import 'dart:ffi';
import 'dart:io';

import 'package:ffi/ffi.dart';

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

class SpineFlutterFFI {
  DynamicLibrary dylib;
  Allocator allocator;

  SpineFlutterFFI(this.dylib, this.allocator);
}

Future<SpineFlutterFFI> initSpineFlutterFFI() async {
  return SpineFlutterFFI(_dylib, malloc);
}
