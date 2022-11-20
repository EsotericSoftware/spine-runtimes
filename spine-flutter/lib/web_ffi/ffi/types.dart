import 'dart:typed_data';
import 'package:meta/meta.dart';

import '../modules/module.dart';
import '../modules/memory.dart';
import '../modules/null_memory.dart';

import '../internal/type_utils.dart';

import '../web_ffi_meta.dart';

/// Represents a pointer into the native C memory corresponding to "NULL",
/// e.g. a pointer with address 0.
///
/// You can compare any other pointer with this pointer using == to check
/// if it's also an nullpointer.
///
/// Any other operation than comparing (e.g. calling [Pointer.cast])
/// will result in exceptions.
final Pointer<Never> nullptr = new Pointer<Never>._null();

/// Number of bytes used by native type T.
///
/// MUST NOT be called with types annoteted with @[unsized] or
/// before [Memory.init()] was called or else an exception will be thrown.
int sizeOf<T extends NativeType>() {
  int? size;
  if (isPointerType<T>()) {
    size = sizeMap[IntPtr];
  } else {
    size = sizeMap[T];
  }
  if (size != null) {
    return size;
  } else {
    throw new ArgumentError('The type $T is not known!');
  }
}

bool _isUnsizedType<T extends NativeType>() {
  return isNativeFunctionType<T>() || isVoidType<T>();
}

/// [NativeType]'s subtypes represent a native type in C.
///
/// [NativeType]'s subtypes (except [Pointer]) are not constructible
/// in the Dart code and serve purely as markers in type signatures.
@sealed
@notConstructible
class NativeType {}

/// Represents a native 64 bit double in C.
///
/// Double is not constructible in the Dart code and serves
/// purely as marker in type signatures.
@sealed
@notConstructible
class Double extends NativeType {}

/// Represents a native 32 bit float in C.
///
/// Float is not constructible in the Dart code and serves
/// purely as marker in type signatures.
@sealed
@notConstructible
class Float extends NativeType {}

/// Represents a native signed 8 bit integer in C.
///
/// Int8 is not constructible in the Dart code and serves
/// purely as marker in type signatures.
@sealed
@notConstructible
class Int8 extends NativeType {}

/// Represents a native signed 16 bit integer in C.
///
/// Int16 is not constructible in the Dart code and serves
/// purely as marker in type signatures.
@sealed
@notConstructible
class Int16 extends NativeType {}

/// Represents a native signed 32 bit integer in C.
///
/// Int32 is not constructible in the Dart code and serves
/// purely as marker in type signatures.
@sealed
@notConstructible
class Int32 extends NativeType {}

/// Represents a native signed 64 bit integer in C.
///
/// Int64 is not constructible in the Dart code and serves
/// purely as marker in type signatures.
@sealed
@notConstructible
class Int64 extends NativeType {}

/// Represents a native unsigned 8 bit integer in C.
///
/// Uint8 is not constructible in the Dart code and serves
/// purely as marker in type signatures.
@sealed
@notConstructible
class Uint8 extends NativeType {}

/// Represents a native unsigned 16 bit integer in C.
///
/// Uint16 is not constructible in the Dart code and serves
/// purely as marker in type signatures.
@sealed
@notConstructible
class Uint16 extends NativeType {}

/// Represents a native unsigned 32 bit integer in C.
///
/// Uint32 is not constructible in the Dart code and serves
/// purely as marker in type signatures.
@sealed
@notConstructible
class Uint32 extends NativeType {}

/// Represents a native unsigned 64 bit integer in C.
///
/// Uint64 is not constructible in the Dart code and serves
/// purely as marker in type signatures.
@sealed
@notConstructible
class Uint64 extends NativeType {}

/// Represents a native pointer-sized integer in C.
///
/// IntPtr is not constructible in the Dart code and serves
/// purely as marker in type signatures.
@sealed
@notConstructible
class IntPtr extends NativeType {}

/// Represents a function type in C.
///
/// NativeFunction is not constructible in the Dart code and serves
/// purely as marker in type signatures.
@sealed
@notConstructible
@unsized
class NativeFunction<T extends Function> extends NativeType {}

/// Opaque's subtypes represent opaque types in C.
///
/// Classes that extend Opaque MUST NOT have a type argument!
///
/// Opaque's subtypes are not constructible in the Dart code and serve
/// purely as markers in type signatures.
@noGeneric
@notConstructible
class Opaque extends NativeType {}

/// Represents a void type in C.
///
/// Void is not constructible in the Dart code and serves
/// purely as marker in type signatures.
@sealed
@notConstructible
@unsized
class Void extends NativeType {}

/// Represents a pointer into the native C memory. Cannot be extended.
@sealed
class Pointer<T extends NativeType> extends NativeType {
  //static Pointer<NativeFunction<T>> fromFunction<T extends Function>(Function f,
  //       [Object? exceptionalReturn]) =>
  //   throw new UnimplementedError();

  /// Access to the raw pointer value.
  final int address;

  /// The [Memory] object this pointer is bound to.
  ///
  /// The `Memory` object backs this pointer, if the value of
  /// this pointer is accessed.
  @extra
  final Memory boundMemory;

  /// How much memory in bytes the type this pointer points to occupies,
  /// or `null` for @[unsized] types.
  @extra
  final int? size;

  factory Pointer._null() {
    return new Pointer._(0, new NullMemory(), null);
  }

  /// Constructs a pointer from an address.
  ///
  /// The optional parameter `bindTo` can be ommited, if and only if
  /// [Memory.global] is set, which is then used as `Memory` to bind to.
  factory Pointer.fromAddress(int ptr, [Memory? bindTo]) {
    bindTo = Memory.global;
    Memory m;
    if (bindTo != null) {
      m = bindTo;
    } else {
      throw new StateError(
          'No global memory set and no explcity memory to bind to given!');
    }
    return new Pointer._(ptr, m, _isUnsizedType<T>() ? null : sizeOf<T>());
  }

  Pointer._(this.address, this.boundMemory, this.size);

  /// Casts this pointer to an other type.
  Pointer<U> cast<U extends NativeType>() => new Pointer<U>._(
      address, boundMemory, _isUnsizedType<U>() ? null : sizeOf<U>());

  /// Pointer arithmetic (takes element size into account).
  ///
  /// Throws an [UnsupportedError] if called on a pointer with an @[unsized]
  /// type argument.
  Pointer<T> elementAt(int index) {
    int? s = size;
    if (s != null) {
      return new Pointer<T>._(address + index * s, boundMemory, s);
    } else {
      throw new UnsupportedError(
          'elementAt is not supported for unsized types!');
    }
  }

  /// The hash code for a Pointer only depends on its address.
  @override
  int get hashCode => address;

  /// Two pointers are equal if their address is the same, independently
  /// of their type argument and of the memory they are bound to.
  @override
  bool operator ==(Object other) =>
      (other is Pointer && other.address == address);

  /// Returns a view of a single element at [index] (takes element
  /// size into account).
  ///
  /// Any modifications to the data will also alter the [Memory] object.
  ///
  /// Throws an [UnsupportedError] if called on a pointer with an @[unsized]
  /// type argument.
  @extra
  ByteData viewSingle(int index) {
    int? s = size;
    if (s != null) {
      return boundMemory.buffer.asByteData(address + index * s, s);
    } else {
      throw new UnsupportedError(
          'viewSingle is not supported for unsized types!');
    }
  }
}

/// Represents a dynamically loaded C library.
class DynamicLibrary {
  @extra
  final Memory boundMemory;

  /// Creates a new instance based on the given module.
  ///
  /// While for each [DynamicLibrary] a new [Memory] object is
  /// created, the [Memory] objects share the backing memory if
  /// they are created based on the same module.
  ///
  /// The [registerMode] parameter can be used to control if the
  /// newly created [Memory] object should be registered as
  /// [Memory.global].
  @extra
  factory DynamicLibrary.fromModule(Module module,
      [MemoryRegisterMode registerMode =
          MemoryRegisterMode.onlyIfGlobalNotSet]) {
    Memory memory = createMemory(module);
    switch (registerMode) {
      case MemoryRegisterMode.yes:
        Memory.global = memory;
        break;
      case MemoryRegisterMode.no:
        break;
      case MemoryRegisterMode.onlyIfGlobalNotSet:
        if (Memory.global == null) {
          Memory.global = memory;
        }
        break;
    }
    return new DynamicLibrary._(memory);
  }

  DynamicLibrary._(this.boundMemory);

  /// Looks up a symbol in the DynamicLibrary and returns its address in memory.
  ///
  /// Throws an [ArgumentError] if it fails to lookup the symbol.
  ///
  /// While this method checks if the underyling wasm symbol is a actually
  /// a function when you lookup a [NativeFunction]`<T>`, it does not check if
  /// the return type and parameters of `T` match the wasm function.
  Pointer<T> lookup<T extends NativeType>(String name) {
    WasmSymbol symbol = symbolByName(boundMemory, name);
    if (isNativeFunctionType<T>()) {
      if (symbol is FunctionDescription) {
        return new Pointer<T>.fromAddress(symbol.tableIndex, boundMemory);
      } else {
        throw new ArgumentError(
            'Tried to look up $name as a function, but it seems it is NOT a function!');
      }
    } else {
      return new Pointer<T>.fromAddress(symbol.address, boundMemory);
    }
  }
}

/// Manages memory on the native heap.
abstract class Allocator {
  /// Allocates byteCount bytes of memory on the native heap.
  ///
  /// The parameter `alignment` is ignored.
  Pointer<T> allocate<T extends NativeType>(int byteCount, {int? alignment});

  /// Releases memory allocated on the native heap.
  void free(Pointer<NativeType> pointer);
}
