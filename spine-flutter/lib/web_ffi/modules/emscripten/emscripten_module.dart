@JS()
library emscripten_module;

import 'dart:typed_data';
import 'package:js/js.dart';
import 'package:js/js_util.dart';
import '../module.dart';
import '../../web_ffi_meta.dart';

@JS('globalThis')
external Object get _globalThis;

@JS('Object.entries')
external List? _entries(Object? o);

@JS()
@anonymous
class _EmscriptenModuleJs {
  external Uint8List? get wasmBinary;
  external Uint8List? get HEAPU8;
  external Object? get asm;

  // Must have an unnamed factory constructor with named arguments.
  external factory _EmscriptenModuleJs({Uint8List wasmBinary});
}

const String _github = r'https://github.com/EPNW/web_ffi';
String _adu(WasmSymbol? original, WasmSymbol? tried) =>
    'CRITICAL EXCEPTION! Address double use! This should never happen, please report this issue on github immediately at $_github' +
    '\r\nOriginal: $original' +
    '\r\nTried: $tried';

typedef int _Malloc(int size);
typedef void _Free(int address);

FunctionDescription _fromWasmFunction(String name, Function func) {
  String? s = getProperty(func, 'name');
  if (s != null) {
    int? index = int.tryParse(s);
    if (index != null) {
      int? length = getProperty(func, 'length');
      if (length != null) {
        return new FunctionDescription(
            tableIndex: index,
            name: name,
            function: func,
            argumentCount: length);
      } else {
        throw new ArgumentError('$name does not seem to be a function symbol!');
      }
    } else {
      throw new ArgumentError('$name does not seem to be a function symbol!');
    }
  } else {
    throw new ArgumentError('$name does not seem to be a function symbol!');
  }
}

/// Documentation is in `emscripten_module_stub.dart`!
@extra
class EmscriptenModule extends Module {
  static Function _moduleFunction(String moduleName) {
    Function? moduleFunction = getProperty(_globalThis, moduleName);
    if (moduleFunction != null) {
      return moduleFunction;
    } else {
      throw StateError('Could not find a emscripten module named $moduleName');
    }
  }

  /// Documentation is in `emscripten_module_stub.dart`!
  static Future<EmscriptenModule> process(String moduleName) async {
    Function moduleFunction = _moduleFunction(moduleName);
    _EmscriptenModuleJs module = new _EmscriptenModuleJs();
    Object? o = moduleFunction(module);
    if (o != null) {
      await promiseToFuture(o);
      return new EmscriptenModule._fromJs(module);
    } else {
      throw new StateError('Could not instantiate an emscripten module!');
    }
  }

  /// Documentation is in `emscripten_module_stub.dart`!
  static Future<EmscriptenModule> compile(
      Uint8List wasmBinary, String moduleName) async {
    Function moduleFunction = _moduleFunction(moduleName);
    _EmscriptenModuleJs module =
        new _EmscriptenModuleJs(wasmBinary: wasmBinary);
    Object? o = moduleFunction(module);
    if (o != null) {
      await promiseToFuture(o);
      return new EmscriptenModule._fromJs(module);
    } else {
      throw new StateError('Could not instantiate an emscripten module!');
    }
  }

  final _EmscriptenModuleJs _emscriptenModuleJs;
  final List<WasmSymbol> _exports;
  final _Malloc _malloc;
  final _Free _free;

  @override
  List<WasmSymbol> get exports => _exports;

  EmscriptenModule._(
      this._emscriptenModuleJs, this._exports, this._malloc, this._free);

  factory EmscriptenModule._fromJs(_EmscriptenModuleJs module) {
    Object? asm = module.asm;
    if (asm != null) {
      Map<int, WasmSymbol> knownAddresses = {};
      _Malloc? malloc;
      _Free? free;
      List<WasmSymbol> exports = [];
      List? entries = _entries(asm);
      if (entries != null) {
        for (dynamic entry in entries) {
          if (entry is List) {
            Object value = entry.last;
            if (value is int) {
              Global g =
                  new Global(address: value, name: entry.first as String);
              if (knownAddresses.containsKey(value) &&
                  knownAddresses[value] is! Global) {
                throw new StateError(_adu(knownAddresses[value], g));
              }
              knownAddresses[value] = g;
              exports.add(g);
            } else if (value is Function) {
              FunctionDescription description =
                  _fromWasmFunction(entry.first as String, value);
              // It might happen that there are two different c functions that do nothing else than calling the same underlying c function
              // In this case, a compiler might substitute both functions with the underlying c function
              // So we got two functions with different names at the same table index
              // So it is actually ok if there are two things at the same address, as long as they are both functions
              if (knownAddresses.containsKey(description.tableIndex) &&
                  knownAddresses[description.tableIndex]
                      is! FunctionDescription) {
                throw new StateError(
                    _adu(knownAddresses[description.tableIndex], description));
              }
              knownAddresses[description.tableIndex] = description;
              exports.add(description);
              if (description.name == 'malloc') {
                malloc = description.function as _Malloc;
              } else if (description.name == 'free') {
                free = description.function as _Free;
              }
            }
          } else {
            throw new StateError(
                'Unexpected entry in entries(Module[\'asm\'])!');
          }
        }
        if (malloc != null) {
          if (free != null) {
            return new EmscriptenModule._(module, exports, malloc, free);
          } else {
            throw new StateError('Module does not export the free function!');
          }
        } else {
          throw new StateError('Module does not export the malloc function!');
        }
      } else {
        throw new StateError(
            'JavaScript error: Could not access entries of Module[\'asm\']!');
      }
    } else {
      throw new StateError(
          'Could not access Module[\'asm\'], are your sure your module was compiled using emscripten?');
    }
  }

  @override
  void free(int pointer) => _free(pointer);

  @override
  ByteBuffer get heap => _getHeap();
  ByteBuffer _getHeap() {
    Uint8List? h = _emscriptenModuleJs.HEAPU8;
    if (h != null) {
      return h.buffer;
    } else {
      throw StateError('Unexpected memory error!');
    }
  }

  @override
  int malloc(int size) => _malloc(size);
}
