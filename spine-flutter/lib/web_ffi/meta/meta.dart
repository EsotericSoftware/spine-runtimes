class _Extra {
  const _Extra();
}

/// A class, field or method annotated with extra is present in `web_ffi`,
/// but not in `dart:ffi`.
const _Extra extra = const _Extra();

class _NoGeneric {
  const _NoGeneric();
}

/// If a class which is annotead with [noGeneric] is extended or implemented,
/// the derived class MUST NOT impose a type argument!
const _NoGeneric noGeneric = const _NoGeneric();

class _NotConstructible {
  const _NotConstructible();
}

/// A [NativeType] annotated with unsized should not be instantiated.
///
/// However, they are not marked as `abstract` to meet the dart:ffi API.
const _NotConstructible notConstructible = const _NotConstructible();

class _Unsized {
  const _Unsized();
}

/// A [NativeType] annotated with unsized does not have a predefined size.
///
/// Unsized [NativeType]s do not support [sizeOf] because their size is unknown,
/// so calling [sizeOf] with an @[unsized] [NativeType] will throw an exception.
/// Consequently, [Pointer.elementAt] is not available and will also throw an exception.
const _Unsized unsized = const _Unsized();
