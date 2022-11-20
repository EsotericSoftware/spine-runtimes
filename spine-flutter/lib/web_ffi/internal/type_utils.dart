import '../ffi/types.dart';

/// Hacky workadround, see https://github.com/dart-lang/language/issues/123
Type _extractType<T>() => T;
String typeString<T>() => _extractType<T>().toString();

// Variable names begin with a capital letter on purpose (opposing dart conventions) to hilight that
// they are treated like types (which are written with a captial letter in dart).
final Type Pointer_IntPtr = _extractType<Pointer<IntPtr>>();
final Type Pointer_Void = _extractType<Pointer<Void>>();
final Type Pointer_Int8 = _extractType<Pointer<Int8>>();
final Type Pointer_Int16 = _extractType<Pointer<Int16>>();
final Type Pointer_Int32 = _extractType<Pointer<Int32>>();
final Type Pointer_Int64 = _extractType<Pointer<Int64>>();
final Type Pointer_Double = _extractType<Pointer<Double>>();
final Type Pointer_Uint8 = _extractType<Pointer<Uint8>>();
final Type Pointer_Uint16 = _extractType<Pointer<Uint16>>();
final Type Pointer_Uint32 = _extractType<Pointer<Uint32>>();
final Type Pointer_Uint64 = _extractType<Pointer<Uint64>>();
final Type Pointer_Float = _extractType<Pointer<Float>>();
final Type Pointer_Opaque = _extractType<Pointer<Opaque>>();
final Type Pointer_Pointer_IntPtr = _extractType<Pointer<Pointer<IntPtr>>>();
final Type Pointer_Pointer_Void = _extractType<Pointer<Pointer<Void>>>();
final Type Pointer_Pointer_Int8 = _extractType<Pointer<Pointer<Int8>>>();
final Type Pointer_Pointer_Int16 = _extractType<Pointer<Pointer<Int16>>>();
final Type Pointer_Pointer_Int32 = _extractType<Pointer<Pointer<Int32>>>();
final Type Pointer_Pointer_Int64 = _extractType<Pointer<Pointer<Int64>>>();
final Type Pointer_Pointer_Double = _extractType<Pointer<Pointer<Double>>>();
final Type Pointer_Pointer_Uint8 = _extractType<Pointer<Pointer<Uint8>>>();
final Type Pointer_Pointer_Uint16 = _extractType<Pointer<Pointer<Uint16>>>();
final Type Pointer_Pointer_Uint32 = _extractType<Pointer<Pointer<Uint32>>>();
final Type Pointer_Pointer_Uint64 = _extractType<Pointer<Pointer<Uint64>>>();
final Type Pointer_Pointer_Float = _extractType<Pointer<Pointer<Float>>>();
final Type Pointer_Pointer_Opaque = _extractType<Pointer<Pointer<Opaque>>>();
final Type Pointer_NativeFunction_dynamic =
    _extractType<Pointer<NativeFunction<dynamic>>>();
final Type DartVoidType = _extractType<void>();
final Type FfiVoidType = _extractType<Void>();

final String _dynamicTypeString = typeString<dynamic>();

final String pointerPointerPointerPrefix =
    typeString<Pointer<Pointer<Pointer<dynamic>>>>()
        .split(_dynamicTypeString)
        .first;

final String pointerNativeFunctionPrefix =
    typeString<Pointer<NativeFunction<dynamic>>>()
        .split(_dynamicTypeString)
        .first;

final String _nativeFunctionPrefix =
    typeString<NativeFunction<dynamic>>().split(_dynamicTypeString).first;
bool isNativeFunctionType<T extends NativeType>() =>
    typeString<T>().startsWith(_nativeFunctionPrefix);

final String _pointerPrefix =
    typeString<Pointer<dynamic>>().split(_dynamicTypeString).first;
bool isPointerType<T extends NativeType>() =>
    typeString<T>().startsWith(_pointerPrefix);

bool isVoidType<T extends NativeType>() => _extractType<T>() == FfiVoidType;
