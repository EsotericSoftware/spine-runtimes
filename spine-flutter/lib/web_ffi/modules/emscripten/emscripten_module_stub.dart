import 'dart:typed_data';
import '../module.dart';

import '../../web_ffi_meta.dart';

/// Provides access to WebAssembly compiled with [emscripten](https://emscripten.org).
///
/// WebAssembly compiled with emscripten comes with an `<moduleName>.wasm`
/// and an additional `<moduleName>.js` glue JavaScript file. The later is
/// required to be loaded on the page before calling any of this classes
/// functions.
///
/// The WebAssembly must have been compiled with the `-s MODULARIZE=1`
/// and `-s EXPORT_NAME=<moduleName>` flags. Futhermore the `<moduleName.js>`
/// must contain all exported WebAssembly functions that should be usable from
/// dart, so using `-s MAIN_MODULE=1` might be advisable.
///
/// For a detailed walkthrough on how to create and inject these files,
/// see the [example](https://github.com/EPNW/web_ffi/blob/master/example/README.md).
///
/// On platforms where [dart:js](https://api.dart.dev/stable/dart-js/dart-js-library.html)
/// is not available, all methods throw [UnsupportedError]s.
@extra
class EmscriptenModule extends Module {
  /// Connects to the JavaScript glue of the emscripten module.
  ///
  /// This happens in the following way:
  /// First, a JavaScript property named `moduleName` of the global object
  /// is accessed, which should contain a function. Then this function is
  /// called and expected to return a JavaScript emscripten module.
  ///
  /// The JavaScript emscripten module is responsible for retriving the
  /// WebAssembly and compile it accordingly.
  ///
  /// On platforms where [dart:js](https://api.dart.dev/stable/dart-js/dart-js-library.html)
  /// is not available, an [UnsupportedError] is thrown.
  static Future<EmscriptenModule> process(String moduleName) =>
      throw new UnsupportedError(
          'Emscripten operations are only allowed on the web (where dart:js is present)!');

  /// Connects to the JavaScript glue of the emscripten module.
  ///
  /// Works like [process], except that the bytes of the WebAssembly
  /// are passed to the JavaScript emscripten module, so it is
  /// your responsibility to fetch it.
  ///
  /// On platforms where [dart:js](https://api.dart.dev/stable/dart-js/dart-js-library.html)
  /// is not available, an [UnsupportedError] is thrown.
  static Future<EmscriptenModule> compile(
          Uint8List wasmBinary, String moduleName) =>
      throw new UnsupportedError(
          'Emscripten operations are only allowed on the web (where dart:js is present)!');

  EmscriptenModule._();

  @override
  List<WasmSymbol> get exports => throw new UnsupportedError(
      'Emscripten operations are only allowed on the web (where dart:js is present)!');

  @override
  void free(int pointer) => throw new UnsupportedError(
      'Emscripten operations are only allowed on the web (where dart:js is present)!');

  @override
  ByteBuffer get heap => throw new UnsupportedError(
      'Emscripten operations are only allowed on the web (where dart:js is present)!');

  @override
  int malloc(int size) => throw new UnsupportedError(
      'Emscripten operations are only allowed on the web (where dart:js is present)!');
}
