/// Occures if it's not possible to convert dart types to JavaScript types.
///
/// This usually happens if a not allowed type is uses as a [NativeType]'s
/// type argument, or a not allowed return value of a [NativeFunction] is
/// used.
class MarshallingException implements Exception {
  final dynamic message;
  const MarshallingException([this.message]);

  MarshallingException.noAddress(Object o)
      : this('Expected a address (int) but found ${o.runtimeType}');

  MarshallingException.typeMissmatch(Type t, Object o)
      : this('Expected a type of $t but object has type ${o.runtimeType}');

  @override
  String toString() => new Exception(message).toString();
}
