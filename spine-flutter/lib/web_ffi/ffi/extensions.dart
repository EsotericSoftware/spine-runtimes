import 'dart:typed_data';
import 'types.dart';

import '../modules/memory.dart';
import '../modules/module.dart';
import '../internal/marshaller.dart';

import '../web_ffi_meta.dart';

/// Extension on [Pointer] specialized for the type argument [NativeFunction].
extension NativeFunctionPointer<NF extends Function>
    on Pointer<NativeFunction<NF>> {
  /// Convert to Dart function, automatically marshalling the arguments and return value.
  ///
  /// There are several rules that apply for the return type of `DF`, see
  /// the list of [allowed return types](https://github.com/EPNW/web_ffi/blob/master/return_types.md).
  /// If marshalling failes, a [MarshallingException] is thrown.
  ///
  /// If this is called on a pointer that does not point to a function,
  /// a [ArgumentError](https://api.dart.dev/stable/dart-core/ArgumentError-class.html) is thrown.
  DF asFunction<DF extends Function>() {
    WasmSymbol symbol = symbolByAddress(boundMemory, address);
    if (symbol is FunctionDescription) {
      return marshall<NF, DF>(symbol.function, boundMemory);
    } else {
      throw ArgumentError(
          'No function at address $address was found (but a global symbol)!');
    }
  }
}

extension DynamicLibraryExtension on DynamicLibrary {
  /// Helper that combines lookup and cast to a Dart function.
  ///
  /// This simply calles [DynamicLibrary.lookup] and [NativeFunctionPointer.asFunction]
  /// internally, so see this two methods for additional insights.
  F lookupFunction<T extends Function, F extends Function>(String name) =>
      this.lookup<NativeFunction<T>>(name).asFunction<F>();
}

/// Extension on [Allocator] to provide allocation with [NativeType].
extension AllocatorAlloc on Allocator {
  /// Allocates `sizeOf<T>() * count` bytes of memory using [Allocator.allocate].
  ///
  /// Since this calls [sizeOf<T>] internally, an exception will be thrown if this
  /// method is called with an @[unsized] type or before [Memory.init] was called.
  Pointer<T> call<T extends NativeType>([int count = 1]) =>
      allocate(sizeOf<T>() * count);
}

/// Extension on [Pointer] specialized for the type argument [Float].
extension FloatPointer on Pointer<Float> {
  /// The float at address.
  double get value => this[0];
  void set value(double value) => this[0] = value;

  /// Creates a typed list view backed by memory in the address space.
  ///
  /// The returned view will allow access to the memory range
  /// from address to `address + size * length`.
  Float32List asTypedList(int length) =>
      boundMemory.buffer.asFloat32List(address, length);

  /// The float at `address + size * index`.
  double operator [](int index) =>
      viewSingle(index).getFloat32(0, Memory.endianess);
  void operator []=(int index, double value) =>
      viewSingle(index).setFloat32(0, value, Memory.endianess);
}

/// Extension on [Pointer] specialized for the type argument [Double].
extension DoublePointer on Pointer<Double> {
  /// The double at address.
  double get value => this[0];
  void set value(double value) => this[0] = value;

  /// Creates a typed list view backed by memory in the address space.
  ///
  /// The returned view will allow access to the memory range
  /// from address to `address + size * length`.
  Float64List asTypedList(int length) =>
      boundMemory.buffer.asFloat64List(address, length);

  /// The double at `address + size * index`.
  double operator [](int index) =>
      viewSingle(index).getFloat64(0, Memory.endianess);
  void operator []=(int index, double value) =>
      viewSingle(index).setFloat64(0, value, Memory.endianess);
}

/// Extension on [Pointer] specialized for the type argument [Int8].
extension Int8Pointer on Pointer<Int8> {
  /// The 8-bit integer at `address`.
  int get value => this[0];
  void set value(int value) => this[0] = value;

  /// Creates a typed list view backed by memory in the address space.
  ///
  /// The returned view will allow access to the memory range
  /// from address to `address + size * length`.
  Int8List asTypedList(int length) =>
      boundMemory.buffer.asInt8List(address, length);

  /// The 8-bit integer at `address + size * index`.
  int operator [](int index) => viewSingle(index).getInt8(0);
  void operator []=(int index, int value) =>
      viewSingle(index).setInt8(0, value);
}

/// Extension on [Pointer] specialized for the type argument [Int16].
extension Int16Pointer on Pointer<Int16> {
  /// The 16-bit integer at `address`.
  int get value => this[0];
  void set value(int value) => this[0] = value;

  /// Creates a typed list view backed by memory in the address space.
  ///
  /// The returned view will allow access to the memory range
  /// from address to `address + size * length`.
  Int16List asTypedList(int length) =>
      boundMemory.buffer.asInt16List(address, length);

  /// The 16-bit integer at `address + size * index`.
  int operator [](int index) => viewSingle(index).getInt16(0, Memory.endianess);
  void operator []=(int index, int value) =>
      viewSingle(index).setInt16(0, value, Memory.endianess);
}

/// Extension on [Pointer] specialized for the type argument [Int32].
extension Int32Pointer on Pointer<Int32> {
  /// The 32-bit integer at `address`.
  int get value => this[0];
  void set value(int value) => this[0] = value;

  /// Creates a typed list view backed by memory in the address space.
  ///
  /// The returned view will allow access to the memory range
  /// from address to `address + size * length`.
  Int32List asTypedList(int length) =>
      boundMemory.buffer.asInt32List(address, length);

  /// The 32-bit integer at `address + size * index`.
  int operator [](int index) => viewSingle(index).getInt32(0, Memory.endianess);
  void operator []=(int index, int value) =>
      viewSingle(index).setInt32(0, value, Memory.endianess);
}

/// Extension on [Pointer] specialized for the type argument [Int64].
extension Int64Pointer on Pointer<Int64> {
  /// The 64-bit integer at `address`.
  int get value => this[0];
  void set value(int value) => this[0] = value;

  /// Creates a typed list view backed by memory in the address space.
  ///
  /// The returned view will allow access to the memory range
  /// from address to `address + size * length`.
  Int64List asTypedList(int length) =>
      boundMemory.buffer.asInt64List(address, length);

  /// The 64-bit integer at `address + size * index`.
  int operator [](int index) => viewSingle(index).getInt64(0, Memory.endianess);
  void operator []=(int index, int value) =>
      viewSingle(index).setInt64(0, value, Memory.endianess);
}

/// Extension on [Pointer] specialized for the type argument [Uint8].
extension Uint8Pointer on Pointer<Uint8> {
  /// The 8-bit unsigned integer at `address`.
  int get value => this[0];
  void set value(int value) => this[0] = value;

  /// Creates a typed list view backed by memory in the address space.
  ///
  /// The returned view will allow access to the memory range
  /// from address to `address + size * length`.
  Uint8List asTypedList(int length) =>
      boundMemory.buffer.asUint8List(address, length);

  /// The 8-bit unsigned integer at `address + size * index`.
  int operator [](int index) => viewSingle(index).getUint8(0);
  void operator []=(int index, int value) =>
      viewSingle(index).setUint8(0, value);
}

/// Extension on [Pointer] specialized for the type argument [Uint16].
extension Uint16Pointer on Pointer<Uint16> {
  /// The 16-bit unsigned integer at `address`.
  int get value => this[0];
  void set value(int value) => this[0] = value;

  /// Creates a typed list view backed by memory in the address space.
  ///
  /// The returned view will allow access to the memory range
  /// from address to `address + size * length`.
  Uint16List asTypedList(int length) =>
      boundMemory.buffer.asUint16List(address, length);

  /// The 16-bit unsigned integer at `address + size * index`.
  int operator [](int index) =>
      viewSingle(index).getUint16(0, Memory.endianess);
  void operator []=(int index, int value) =>
      viewSingle(index).setUint16(0, value, Memory.endianess);
}

/// Extension on [Pointer] specialized for the type argument [Uint32].
extension Uint32Pointer on Pointer<Uint32> {
  /// The 32-bit unsigned integer at `address`.
  int get value => this[0];
  void set value(int value) => this[0] = value;

  /// Creates a typed list view backed by memory in the address space.
  ///
  /// The returned view will allow access to the memory range
  /// from address to `address + size * length`.
  Uint32List asTypedList(int length) =>
      boundMemory.buffer.asUint32List(address, length);

  /// The 32-bit unsigned integer at `address + size * index`.
  int operator [](int index) =>
      viewSingle(index).getUint32(0, Memory.endianess);
  void operator []=(int index, int value) =>
      viewSingle(index).setUint32(0, value, Memory.endianess);
}

/// Extension on [Pointer] specialized for the type argument [Uint64].
extension Uint64Pointer on Pointer<Uint64> {
  /// The 64-bit unsigned integer at `address`.
  int get value => this[0];
  void set value(int value) => this[0] = value;

  /// Creates a typed list view backed by memory in the address space.
  ///
  /// The returned view will allow access to the memory range
  /// from address to `address + size * length`.
  Uint64List asTypedList(int length) =>
      boundMemory.buffer.asUint64List(address, length);

  /// The 64-bit unsigned integer at `address + size * index`.
  int operator [](int index) =>
      viewSingle(index).getUint64(0, Memory.endianess);
  void operator []=(int index, int value) =>
      viewSingle(index).setUint64(0, value, Memory.endianess);
}

/// Extension on [Pointer] specialized for the type argument [IntPtr].
extension IntPtrPointer on Pointer<IntPtr> {
  /// The 32-bit or 64-bit value at `address`.
  int get value => this[0];
  void set value(int value) => this[0] = value;

  /// Returns `true` if the size of a pointer is 64-bit, `false` otherwise.
  @extra
  bool get is64Bit => size == 8;

  /// The 32-bit or 64-bit integer at `address + size * index`.
  int operator [](int index) => is64Bit
      ? viewSingle(index).getUint64(0, Memory.endianess)
      : viewSingle(index).getUint32(0, Memory.endianess);
  void operator []=(int index, int value) => is64Bit
      ? viewSingle(index).setUint64(0, value, Memory.endianess)
      : viewSingle(index).setUint32(0, value, Memory.endianess);
}

/// Extension on [Pointer] specialized for the type argument [Pointer].
extension PointerPointer<T extends NativeType> on Pointer<Pointer<T>> {
  /// The pointer at `address`.
  Pointer<T> get value => this[0];
  void set value(Pointer<T> value) => this[0] = value;

  /// Returns `true` if the size of a pointer is 64-bit, `false` otherwise.
  @extra
  bool get is64Bit => size == 8;

  /// The pointer at `address + size * index`.
  Pointer<T> operator [](int index) => new Pointer<T>.fromAddress(
      is64Bit
          ? viewSingle(index).getUint64(0, Memory.endianess)
          : viewSingle(index).getUint32(0, Memory.endianess),
      boundMemory);
  void operator []=(int index, Pointer<T> value) => is64Bit
      ? viewSingle(index).setUint64(0, value.address, Memory.endianess)
      : viewSingle(index).setUint32(0, value.address, Memory.endianess);
}
