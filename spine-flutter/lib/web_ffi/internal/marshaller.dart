import 'package:meta/meta.dart';

import '../ffi/types.dart';
import '../modules/exceptions.dart';
import '../modules/memory.dart';
import 'invoker_generated.dart';
import 'type_utils.dart';

// Called from the invokers
T execute<T>(Function base, List<Object> args, Memory memory) {
  if (T == DartVoidType) {
    Function.apply(base, args.map(_toJsType).toList());
    return null as T;
  } else {
    Object result = Function.apply(base, args.map(_toJsType).toList());
    return _toDartType<T>(result, memory);
  }
}

DF marshall<NF extends Function, DF extends Function>(
    Function base, Memory memory) {
  return _inferFromSignature(DF.toString()).copyWith(base, memory).run as DF;
}

Object _toJsType(Object dartObject) {
  if (dartObject is int || dartObject is double || dartObject is bool) {
    return dartObject;
  } else if (dartObject is Pointer) {
    return dartObject.address;
  } else {
    throw new MarshallingException(
        'Could not convert dart type ${dartObject.runtimeType} to a JavaScript type!');
  }
}

InvokeHelper _inferFromSignature(String signature) {
  String returnType = signature.split('=>').last.trim();
  if (returnType.startsWith(pointerPointerPointerPrefix)) {
    throw new MarshallingException(
        'Nesting pointers is only supported to a deepth of 2!' +
            '\nThis means that you can write Pointer<Pointer<X>> but not Pointer<Pointer<Pointer<X>>>, ...');
  }
  InvokeHelper? h = _knownTypes[returnType];
  if (h != null) {
    return h;
  } else {
    if (returnType.startsWith(pointerNativeFunctionPrefix)) {
      throw new MarshallingException(
          'Using pointers to native functions as return type is only allowed if the type of the native function is dynamic!' +
              '\nThis means that only Pointer<NativeFunction<dynamic>> is allowed!');
    } else {
      throw new MarshallingException(
          'Unknown type $returnType (infered from $signature), all marshallable types: ${listKnownTypes()}');
    }
  }
}

@visibleForTesting
List<String> listKnownTypes() =>
    new List<String>.of(_knownTypes.keys, growable: false);

final Map<String, InvokeHelper> _knownTypes = {
  typeString<int>(): new InvokeHelper<int>(null, null),
  typeString<double>(): new InvokeHelper<double>(null, null),
  typeString<bool>(): new InvokeHelper<bool>(null, null),
  typeString<void>(): new InvokeHelper<void>(null, null)
};

void registerNativeMarshallerType<T extends NativeType>() {
  _knownTypes[typeString<Pointer<T>>()] =
      new InvokeHelper<Pointer<T>>(null, null);
  _knownTypes[typeString<Pointer<Pointer<T>>>()] =
      new InvokeHelper<Pointer<Pointer<T>>>(null, null);
}

void registerNativeMarshallerOpaque<T extends Opaque>() {
  _knownTypes[typeString<Pointer<T>>()] = new OpaqueInvokeHelper<T>(null, null);
  _knownTypes[typeString<Pointer<Pointer<T>>>()] =
      new OpaqueInvokeHelperSquare<T>(null, null);
}

T _toDartType<T>(Object o, Memory bind) {
  if (T == int) {
    if (o is int) {
      return o as T;
    } else {
      throw new MarshallingException.typeMissmatch(T, o);
    }
  } else if (T == double) {
    if (o is double) {
      return o as T;
    } else {
      throw new MarshallingException.typeMissmatch(T, o);
    }
  } else if (T == bool) {
    if (o is bool) {
      return o as T;
    } else {
      throw new MarshallingException.typeMissmatch(T, o);
    }
  } else {
    if (T == Pointer_Void) {
      if (o is int) {
        return new Pointer<Void>.fromAddress(o, bind) as T;
      } else {
        throw new MarshallingException.noAddress(o);
      }
    } else if (T == Pointer_IntPtr) {
      if (o is int) {
        return new Pointer<IntPtr>.fromAddress(o, bind) as T;
      } else {
        throw new MarshallingException.noAddress(o);
      }
    } else if (T == Pointer_Int8) {
      if (o is int) {
        return new Pointer<Int8>.fromAddress(o, bind) as T;
      } else {
        throw new MarshallingException.noAddress(o);
      }
    } else if (T == Pointer_Int16) {
      if (o is int) {
        return new Pointer<Int16>.fromAddress(o, bind) as T;
      } else {
        throw new MarshallingException.noAddress(o);
      }
    } else if (T == Pointer_Int32) {
      if (o is int) {
        return new Pointer<Int32>.fromAddress(o, bind) as T;
      } else {
        throw new MarshallingException.noAddress(o);
      }
    } else if (T == Pointer_Int64) {
      if (o is int) {
        return new Pointer<Int64>.fromAddress(o, bind) as T;
      } else {
        throw new MarshallingException.noAddress(o);
      }
    } else if (T == Pointer_Double) {
      if (o is int) {
        return new Pointer<Double>.fromAddress(o, bind) as T;
      } else {
        throw new MarshallingException.noAddress(o);
      }
    } else if (T == Pointer_Uint8) {
      if (o is int) {
        return new Pointer<Uint8>.fromAddress(o, bind) as T;
      } else {
        throw new MarshallingException.noAddress(o);
      }
    } else if (T == Pointer_Uint16) {
      if (o is int) {
        return new Pointer<Uint16>.fromAddress(o, bind) as T;
      } else {
        throw new MarshallingException.noAddress(o);
      }
    } else if (T == Pointer_Uint32) {
      if (o is int) {
        return new Pointer<Uint32>.fromAddress(o, bind) as T;
      } else {
        throw new MarshallingException.noAddress(o);
      }
    } else if (T == Pointer_Uint64) {
      if (o is int) {
        return new Pointer<Uint64>.fromAddress(o, bind) as T;
      } else {
        throw new MarshallingException.noAddress(o);
      }
    } else if (T == Pointer_Float) {
      if (o is int) {
        return new Pointer<Float>.fromAddress(o, bind) as T;
      } else {
        throw new MarshallingException.noAddress(o);
      }
    } else if (T == Pointer_Opaque) {
      if (o is int) {
        return new Pointer<Opaque>.fromAddress(o, bind) as T;
      } else {
        throw new MarshallingException.noAddress(o);
      }
    } else if (T == Pointer_NativeFunction_dynamic) {
      if (o is int) {
        return new Pointer<NativeFunction<dynamic>>.fromAddress(o, bind) as T;
      } else {
        throw new MarshallingException.noAddress(o);
      }
    } else {
      if (T == Pointer_Pointer_Void) {
        if (o is int) {
          return new Pointer<Pointer<Void>>.fromAddress(o, bind) as T;
        } else {
          throw new MarshallingException.noAddress(o);
        }
      } else if (T == Pointer_Pointer_IntPtr) {
        if (o is int) {
          return new Pointer<Pointer<IntPtr>>.fromAddress(o, bind) as T;
        } else {
          throw new MarshallingException.noAddress(o);
        }
      } else if (T == Pointer_Pointer_Int8) {
        if (o is int) {
          return new Pointer<Pointer<Int8>>.fromAddress(o, bind) as T;
        } else {
          throw new MarshallingException.noAddress(o);
        }
      } else if (T == Pointer_Pointer_Int16) {
        if (o is int) {
          return new Pointer<Pointer<Int16>>.fromAddress(o, bind) as T;
        } else {
          throw new MarshallingException.noAddress(o);
        }
      } else if (T == Pointer_Pointer_Int32) {
        if (o is int) {
          return new Pointer<Pointer<Int32>>.fromAddress(o, bind) as T;
        } else {
          throw new MarshallingException.noAddress(o);
        }
      } else if (T == Pointer_Pointer_Int64) {
        if (o is int) {
          return new Pointer<Pointer<Int64>>.fromAddress(o, bind) as T;
        } else {
          throw new MarshallingException.noAddress(o);
        }
      } else if (T == Pointer_Pointer_Double) {
        if (o is int) {
          return new Pointer<Pointer<Double>>.fromAddress(o, bind) as T;
        } else {
          throw new MarshallingException.noAddress(o);
        }
      } else if (T == Pointer_Pointer_Uint8) {
        if (o is int) {
          return new Pointer<Pointer<Uint8>>.fromAddress(o, bind) as T;
        } else {
          throw new MarshallingException.noAddress(o);
        }
      } else if (T == Pointer_Pointer_Uint16) {
        if (o is int) {
          return new Pointer<Pointer<Uint16>>.fromAddress(o, bind) as T;
        } else {
          throw new MarshallingException.noAddress(o);
        }
      } else if (T == Pointer_Pointer_Uint32) {
        if (o is int) {
          return new Pointer<Pointer<Uint32>>.fromAddress(o, bind) as T;
        } else {
          throw new MarshallingException.noAddress(o);
        }
      } else if (T == Pointer_Pointer_Uint64) {
        if (o is int) {
          return new Pointer<Pointer<Uint64>>.fromAddress(o, bind) as T;
        } else {
          throw new MarshallingException.noAddress(o);
        }
      } else if (T == Pointer_Pointer_Float) {
        if (o is int) {
          return new Pointer<Pointer<Float>>.fromAddress(o, bind) as T;
        } else {
          throw new MarshallingException.noAddress(o);
        }
      } else if (T == Pointer_Pointer_Opaque) {
        if (o is int) {
          return new Pointer<Pointer<Opaque>>.fromAddress(o, bind) as T;
        } else {
          throw new MarshallingException.noAddress(o);
        }
      } else {
        throw new MarshallingException(
            'Can not back-marshall to type $T (object type is ${o.runtimeType}');
      }
    }
  }
}
