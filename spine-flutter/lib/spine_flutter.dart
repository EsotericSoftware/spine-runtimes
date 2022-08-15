
import 'dart:async';
import 'dart:ffi';
import 'dart:io';
import 'dart:isolate';

import 'spine_flutter_bindings_generated.dart';

int spine_major_version() => _bindings.spine_major_version();
int spine_minor_version() => _bindings.spine_minor_version();

const String _libName = 'spine_flutter';

/// The dynamic library in which the symbols for [SpineFlutterBindings] can be found.
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

/// The bindings to the native functions in [_dylib].
final SpineFlutterBindings _bindings = SpineFlutterBindings(_dylib);