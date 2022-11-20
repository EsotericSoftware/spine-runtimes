import 'dart:typed_data';
import 'package:meta/meta.dart';

import 'module.dart';
import '../ffi/types.dart';
import '../internal/marshaller.dart';
import '../web_ffi_meta.dart';

final Map<Type, int> sizeMap = {};

/// Must be called with each type that extends Opaque before
/// attemtping to use that type.
@extra
void registerOpaqueType<T extends Opaque>() {
  sizeMap[T] = sizeOf<Opaque>();
  registerNativeMarshallerOpaque<T>();
}

void _registerType<T extends NativeType>(int size) {
  sizeMap[T] = size;
  registerNativeMarshallerType<T>();
}

/// Represents the native heap.
@extra
class Memory implements Allocator {
  /// The endianess of data stored.
  ///
  /// The WebAssembly speficiation defines little endianess, so this is a constant.
  static const Endian endianess = Endian.little;

  /// Must be called before working with `web_ffi` to initalize all type sizes.
  ///
  /// The optional parameter [pointerSizeBytes] can be used to adjust the size
  /// of pointers. It defaults to `4` since WebAssembly usually uses 32 bit pointers.
  /// If you want to use wasm64, set [pointerSizeBytes] to `8` to denote 64 bit pointers.
  static void init([int pointerSizeBytes = 4]) {
    _registerType<Float>(4);
    _registerType<Double>(8);
    _registerType<Int8>(1);
    _registerType<Uint8>(1);
    _registerType<Int16>(2);
    _registerType<Uint16>(2);
    _registerType<Int32>(4);
    _registerType<Uint32>(4);
    _registerType<Int64>(8);
    _registerType<Uint64>(8);
    _registerType<IntPtr>(pointerSizeBytes);
    _registerType<Opaque>(pointerSizeBytes);
    registerNativeMarshallerType<Void>();
    registerNativeMarshallerType<NativeFunction<dynamic>>();
  }

  /// The default [Memory] object to use.
  ///
  /// This field is null until it is either manually set to a [Memory] object,
  /// or automatically set by [DynamicLibrary.fromModule].
  ///
  /// This is most notably used when creating a pointer using [Pointer.fromAddress]
  /// with no explicite memory to bind to given.
  static Memory? global;

  /// Can be used to directly access the memory of this object.
  ///
  /// The value of this field should not be stored in a state variable,
  /// since the returned buffer may change over time.
  @doNotStore
  ByteBuffer get buffer => _module.heap;

  final Module _module;
  final Map<String, WasmSymbol> _symbolsByName;
  final Map<int, WasmSymbol> _symbolsByAddress;

  Memory._(this._module)
      : _symbolsByAddress = new Map<int, WasmSymbol>.fromEntries(_module.exports
            .map<MapEntry<int, WasmSymbol>>((WasmSymbol symbol) =>
                new MapEntry<int, WasmSymbol>(symbol.address, symbol))),
        _symbolsByName = new Map<String, WasmSymbol>.fromEntries(_module.exports
            .map<MapEntry<String, WasmSymbol>>((WasmSymbol symbol) =>
                new MapEntry<String, WasmSymbol>(symbol.name, symbol)));

  @override
  Pointer<T> allocate<T extends NativeType>(int byteCount, {int? alignment}) {
    return new Pointer<T>.fromAddress(_module.malloc(byteCount), this);
  }

  @override
  void free(Pointer<NativeType> pointer) {
    _module.free(pointer.address);
  }
}

Memory createMemory(Module module) => new Memory._(module);

WasmSymbol symbolByAddress(Memory m, int address) {
  WasmSymbol? s = m._symbolsByAddress[address];
  if (s != null) {
    return s;
  } else {
    throw new ArgumentError('Could not find symbol at $address!');
  }
}

WasmSymbol symbolByName(Memory m, String name) {
  WasmSymbol? s = m._symbolsByName[name];
  if (s != null) {
    return s;
  } else {
    throw new ArgumentError('Could not find symbol $name!');
  }
}

/// Used on [DynamicLibrary] creation to control if the therby newly created
/// [Memory] object should be registered as [Memory.global].
@extra
enum MemoryRegisterMode { yes, no, onlyIfGlobalNotSet }
