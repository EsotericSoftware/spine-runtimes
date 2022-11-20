import 'dart:typed_data';
import 'package:meta/meta.dart';
import '../web_ffi_meta.dart';

/// Base class to interact with the WebAssembly.
///
/// Currently, only [emscripten](https://emscripten.org) compiled WebAssembly is supported,
/// so the only concrete implementation if this class is [EmscriptenModule].
///
/// To support additional mechanisms/frameworks/compilers, create a subclass of
/// [Module].
@extra
abstract class Module {
  /// Provides access to the malloc function in WebAssembly.
  ///
  /// Allocates `size` bytes of memory and returns the corresponding
  /// address.
  ///
  /// Memory allocated by this should be [free]d afterwards.
  int malloc(int size);

  /// Provides access to the free function in WebAssembly.
  ///
  /// Frees the memory region at `pointer` that was previously
  /// allocated with [malloc].
  void free(int pointer);

  /// Provides access to the [WebAssemblys memory](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/WebAssembly/Memory) buffer.
  ///
  /// The actual [ByteBuffer] object returned by this getter is allowed to change;
  /// It should not be cached in a state variable and is thus annotated with @[doNotStore].
  @doNotStore
  ByteBuffer get heap;

  /// A list containing everything exported by the underlying
  /// [WebAssembly instance](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/WebAssembly/Instance).
  List<WasmSymbol> get exports;
}

/// Describes something exported by the WebAssembly.
@extra
@sealed
abstract class WasmSymbol {
  /// The address of the exported thing.
  final int address;

  /// The name of the exported thing.
  final String name;

  const WasmSymbol({required this.address, required this.name});

  @override
  int get hashCode => toString().hashCode;

  @override
  String toString() => '[address=$address\tname=$name]';
}

/// A global is a symbol exported by the WebAssembly,
/// that is not a function.
@extra
@sealed
class Global extends WasmSymbol {
  const Global({required int address, required String name})
      : super(address: address, name: name);

  @override
  bool operator ==(dynamic other) {
    if (other != null && other is Global) {
      return name == other.name && address == other.address;
    } else {
      return false;
    }
  }
}

/// Describes a function exported from WebAssembly.
@extra
@sealed
class FunctionDescription extends WasmSymbol {
  /// The index of this function in the [WebAssembly table](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/WebAssembly/Table).
  /// This is the same as its address.
  int get tableIndex => address;

  /// The amount of arguments the underyling function has.
  final int argumentCount;

  /// The actual function.
  final Function function;
  const FunctionDescription(
      {required int tableIndex,
      required String name,
      required this.argumentCount,
      required this.function})
      : super(address: tableIndex, name: name);

  @override
  int get hashCode => '$name$argumentCount$tableIndex'.hashCode;

  @override
  bool operator ==(dynamic other) {
    if (other != null && other is FunctionDescription) {
      return argumentCount == other.argumentCount &&
          name == other.name &&
          tableIndex == other.tableIndex;
    } else {
      return false;
    }
  }

  @override
  String toString() =>
      '[tableIndex=$tableIndex\tname=$name\targumentCount=$argumentCount\tfunction=$function]';
}
