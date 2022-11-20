import '../ffi/types.dart';
import '../modules/memory.dart';
import 'marshaller.dart' show execute;

/// https://stackoverflow.com/questions/26122009/is-there-a-maximum-number-of-parameters-for-functions-in-c-with-the-gcc-resp-m
/// the C Standard 5.2.4.1 says:
/// 4095 characters in a logical source line
/// 127 parameters in one function definition
/// 127 arguments in one function call

class OpaqueInvokeHelper<T extends Opaque> extends InvokeHelper<Pointer<T>> {
  OpaqueInvokeHelper(Function? base, Memory? memory) : super(base, memory);

  @override
  InvokeHelper<Pointer<T>> copyWith(Function base, Memory memory) {
    return new OpaqueInvokeHelper<T>(base, memory);
  }

  @override
  Pointer<T> run(
          [dynamic arg0,
          dynamic arg1,
          dynamic arg2,
          dynamic arg3,
          dynamic arg4,
          dynamic arg5,
          dynamic arg6,
          dynamic arg7,
          dynamic arg8,
          dynamic arg9,
          dynamic arg10,
          dynamic arg11,
          dynamic arg12,
          dynamic arg13,
          dynamic arg14,
          dynamic arg15,
          dynamic arg16,
          dynamic arg17,
          dynamic arg18,
          dynamic arg19,
          dynamic arg20,
          dynamic arg21,
          dynamic arg22,
          dynamic arg23,
          dynamic arg24,
          dynamic arg25,
          dynamic arg26,
          dynamic arg27,
          dynamic arg28,
          dynamic arg29,
          dynamic arg30,
          dynamic arg31,
          dynamic arg32,
          dynamic arg33,
          dynamic arg34,
          dynamic arg35,
          dynamic arg36,
          dynamic arg37,
          dynamic arg38,
          dynamic arg39,
          dynamic arg40,
          dynamic arg41,
          dynamic arg42,
          dynamic arg43,
          dynamic arg44,
          dynamic arg45,
          dynamic arg46,
          dynamic arg47,
          dynamic arg48,
          dynamic arg49,
          dynamic arg50,
          dynamic arg51,
          dynamic arg52,
          dynamic arg53,
          dynamic arg54,
          dynamic arg55,
          dynamic arg56,
          dynamic arg57,
          dynamic arg58,
          dynamic arg59,
          dynamic arg60,
          dynamic arg61,
          dynamic arg62,
          dynamic arg63,
          dynamic arg64,
          dynamic arg65,
          dynamic arg66,
          dynamic arg67,
          dynamic arg68,
          dynamic arg69,
          dynamic arg70,
          dynamic arg71,
          dynamic arg72,
          dynamic arg73,
          dynamic arg74,
          dynamic arg75,
          dynamic arg76,
          dynamic arg77,
          dynamic arg78,
          dynamic arg79,
          dynamic arg80,
          dynamic arg81,
          dynamic arg82,
          dynamic arg83,
          dynamic arg84,
          dynamic arg85,
          dynamic arg86,
          dynamic arg87,
          dynamic arg88,
          dynamic arg89,
          dynamic arg90,
          dynamic arg91,
          dynamic arg92,
          dynamic arg93,
          dynamic arg94,
          dynamic arg95,
          dynamic arg96,
          dynamic arg97,
          dynamic arg98,
          dynamic arg99,
          dynamic arg100,
          dynamic arg101,
          dynamic arg102,
          dynamic arg103,
          dynamic arg104,
          dynamic arg105,
          dynamic arg106,
          dynamic arg107,
          dynamic arg108,
          dynamic arg109,
          dynamic arg110,
          dynamic arg111,
          dynamic arg112,
          dynamic arg113,
          dynamic arg114,
          dynamic arg115,
          dynamic arg116,
          dynamic arg117,
          dynamic arg118,
          dynamic arg119,
          dynamic arg120,
          dynamic arg121,
          dynamic arg122,
          dynamic arg123,
          dynamic arg124,
          dynamic arg125,
          dynamic arg126]) =>
      new InvokeHelper<Pointer<Opaque>>(_base, _memory)
          .run(
              arg0,
              arg1,
              arg2,
              arg3,
              arg4,
              arg5,
              arg6,
              arg7,
              arg8,
              arg9,
              arg10,
              arg11,
              arg12,
              arg13,
              arg14,
              arg15,
              arg16,
              arg17,
              arg18,
              arg19,
              arg20,
              arg21,
              arg22,
              arg23,
              arg24,
              arg25,
              arg26,
              arg27,
              arg28,
              arg29,
              arg30,
              arg31,
              arg32,
              arg33,
              arg34,
              arg35,
              arg36,
              arg37,
              arg38,
              arg39,
              arg40,
              arg41,
              arg42,
              arg43,
              arg44,
              arg45,
              arg46,
              arg47,
              arg48,
              arg49,
              arg50,
              arg51,
              arg52,
              arg53,
              arg54,
              arg55,
              arg56,
              arg57,
              arg58,
              arg59,
              arg60,
              arg61,
              arg62,
              arg63,
              arg64,
              arg65,
              arg66,
              arg67,
              arg68,
              arg69,
              arg70,
              arg71,
              arg72,
              arg73,
              arg74,
              arg75,
              arg76,
              arg77,
              arg78,
              arg79,
              arg80,
              arg81,
              arg82,
              arg83,
              arg84,
              arg85,
              arg86,
              arg87,
              arg88,
              arg89,
              arg90,
              arg91,
              arg92,
              arg93,
              arg94,
              arg95,
              arg96,
              arg97,
              arg98,
              arg99,
              arg100,
              arg101,
              arg102,
              arg103,
              arg104,
              arg105,
              arg106,
              arg107,
              arg108,
              arg109,
              arg110,
              arg111,
              arg112,
              arg113,
              arg114,
              arg115,
              arg116,
              arg117,
              arg118,
              arg119,
              arg120,
              arg121,
              arg122,
              arg123,
              arg124,
              arg125,
              arg126)
          .cast<T>();
}

class OpaqueInvokeHelperSquare<T extends Opaque>
    extends InvokeHelper<Pointer<Pointer<T>>> {
  OpaqueInvokeHelperSquare(Function? base, Memory? memory)
      : super(base, memory);

  @override
  InvokeHelper<Pointer<Pointer<T>>> copyWith(Function base, Memory memory) {
    return new OpaqueInvokeHelperSquare<T>(base, memory);
  }

  @override
  Pointer<Pointer<T>> run(
          [dynamic arg0,
          dynamic arg1,
          dynamic arg2,
          dynamic arg3,
          dynamic arg4,
          dynamic arg5,
          dynamic arg6,
          dynamic arg7,
          dynamic arg8,
          dynamic arg9,
          dynamic arg10,
          dynamic arg11,
          dynamic arg12,
          dynamic arg13,
          dynamic arg14,
          dynamic arg15,
          dynamic arg16,
          dynamic arg17,
          dynamic arg18,
          dynamic arg19,
          dynamic arg20,
          dynamic arg21,
          dynamic arg22,
          dynamic arg23,
          dynamic arg24,
          dynamic arg25,
          dynamic arg26,
          dynamic arg27,
          dynamic arg28,
          dynamic arg29,
          dynamic arg30,
          dynamic arg31,
          dynamic arg32,
          dynamic arg33,
          dynamic arg34,
          dynamic arg35,
          dynamic arg36,
          dynamic arg37,
          dynamic arg38,
          dynamic arg39,
          dynamic arg40,
          dynamic arg41,
          dynamic arg42,
          dynamic arg43,
          dynamic arg44,
          dynamic arg45,
          dynamic arg46,
          dynamic arg47,
          dynamic arg48,
          dynamic arg49,
          dynamic arg50,
          dynamic arg51,
          dynamic arg52,
          dynamic arg53,
          dynamic arg54,
          dynamic arg55,
          dynamic arg56,
          dynamic arg57,
          dynamic arg58,
          dynamic arg59,
          dynamic arg60,
          dynamic arg61,
          dynamic arg62,
          dynamic arg63,
          dynamic arg64,
          dynamic arg65,
          dynamic arg66,
          dynamic arg67,
          dynamic arg68,
          dynamic arg69,
          dynamic arg70,
          dynamic arg71,
          dynamic arg72,
          dynamic arg73,
          dynamic arg74,
          dynamic arg75,
          dynamic arg76,
          dynamic arg77,
          dynamic arg78,
          dynamic arg79,
          dynamic arg80,
          dynamic arg81,
          dynamic arg82,
          dynamic arg83,
          dynamic arg84,
          dynamic arg85,
          dynamic arg86,
          dynamic arg87,
          dynamic arg88,
          dynamic arg89,
          dynamic arg90,
          dynamic arg91,
          dynamic arg92,
          dynamic arg93,
          dynamic arg94,
          dynamic arg95,
          dynamic arg96,
          dynamic arg97,
          dynamic arg98,
          dynamic arg99,
          dynamic arg100,
          dynamic arg101,
          dynamic arg102,
          dynamic arg103,
          dynamic arg104,
          dynamic arg105,
          dynamic arg106,
          dynamic arg107,
          dynamic arg108,
          dynamic arg109,
          dynamic arg110,
          dynamic arg111,
          dynamic arg112,
          dynamic arg113,
          dynamic arg114,
          dynamic arg115,
          dynamic arg116,
          dynamic arg117,
          dynamic arg118,
          dynamic arg119,
          dynamic arg120,
          dynamic arg121,
          dynamic arg122,
          dynamic arg123,
          dynamic arg124,
          dynamic arg125,
          dynamic arg126]) =>
      new InvokeHelper<Pointer<Pointer<Opaque>>>(_base, _memory)
          .run(
              arg0,
              arg1,
              arg2,
              arg3,
              arg4,
              arg5,
              arg6,
              arg7,
              arg8,
              arg9,
              arg10,
              arg11,
              arg12,
              arg13,
              arg14,
              arg15,
              arg16,
              arg17,
              arg18,
              arg19,
              arg20,
              arg21,
              arg22,
              arg23,
              arg24,
              arg25,
              arg26,
              arg27,
              arg28,
              arg29,
              arg30,
              arg31,
              arg32,
              arg33,
              arg34,
              arg35,
              arg36,
              arg37,
              arg38,
              arg39,
              arg40,
              arg41,
              arg42,
              arg43,
              arg44,
              arg45,
              arg46,
              arg47,
              arg48,
              arg49,
              arg50,
              arg51,
              arg52,
              arg53,
              arg54,
              arg55,
              arg56,
              arg57,
              arg58,
              arg59,
              arg60,
              arg61,
              arg62,
              arg63,
              arg64,
              arg65,
              arg66,
              arg67,
              arg68,
              arg69,
              arg70,
              arg71,
              arg72,
              arg73,
              arg74,
              arg75,
              arg76,
              arg77,
              arg78,
              arg79,
              arg80,
              arg81,
              arg82,
              arg83,
              arg84,
              arg85,
              arg86,
              arg87,
              arg88,
              arg89,
              arg90,
              arg91,
              arg92,
              arg93,
              arg94,
              arg95,
              arg96,
              arg97,
              arg98,
              arg99,
              arg100,
              arg101,
              arg102,
              arg103,
              arg104,
              arg105,
              arg106,
              arg107,
              arg108,
              arg109,
              arg110,
              arg111,
              arg112,
              arg113,
              arg114,
              arg115,
              arg116,
              arg117,
              arg118,
              arg119,
              arg120,
              arg121,
              arg122,
              arg123,
              arg124,
              arg125,
              arg126)
          .cast<Pointer<T>>();
}

class InvokeHelper<T> {
  final Memory? _memory;
  final Function? _base;

  const InvokeHelper(this._base, this._memory);

  InvokeHelper<T> copyWith(Function base, Memory memory) {
    return new InvokeHelper(base, memory);
  }

  T run(
      [dynamic arg0,
      dynamic arg1,
      dynamic arg2,
      dynamic arg3,
      dynamic arg4,
      dynamic arg5,
      dynamic arg6,
      dynamic arg7,
      dynamic arg8,
      dynamic arg9,
      dynamic arg10,
      dynamic arg11,
      dynamic arg12,
      dynamic arg13,
      dynamic arg14,
      dynamic arg15,
      dynamic arg16,
      dynamic arg17,
      dynamic arg18,
      dynamic arg19,
      dynamic arg20,
      dynamic arg21,
      dynamic arg22,
      dynamic arg23,
      dynamic arg24,
      dynamic arg25,
      dynamic arg26,
      dynamic arg27,
      dynamic arg28,
      dynamic arg29,
      dynamic arg30,
      dynamic arg31,
      dynamic arg32,
      dynamic arg33,
      dynamic arg34,
      dynamic arg35,
      dynamic arg36,
      dynamic arg37,
      dynamic arg38,
      dynamic arg39,
      dynamic arg40,
      dynamic arg41,
      dynamic arg42,
      dynamic arg43,
      dynamic arg44,
      dynamic arg45,
      dynamic arg46,
      dynamic arg47,
      dynamic arg48,
      dynamic arg49,
      dynamic arg50,
      dynamic arg51,
      dynamic arg52,
      dynamic arg53,
      dynamic arg54,
      dynamic arg55,
      dynamic arg56,
      dynamic arg57,
      dynamic arg58,
      dynamic arg59,
      dynamic arg60,
      dynamic arg61,
      dynamic arg62,
      dynamic arg63,
      dynamic arg64,
      dynamic arg65,
      dynamic arg66,
      dynamic arg67,
      dynamic arg68,
      dynamic arg69,
      dynamic arg70,
      dynamic arg71,
      dynamic arg72,
      dynamic arg73,
      dynamic arg74,
      dynamic arg75,
      dynamic arg76,
      dynamic arg77,
      dynamic arg78,
      dynamic arg79,
      dynamic arg80,
      dynamic arg81,
      dynamic arg82,
      dynamic arg83,
      dynamic arg84,
      dynamic arg85,
      dynamic arg86,
      dynamic arg87,
      dynamic arg88,
      dynamic arg89,
      dynamic arg90,
      dynamic arg91,
      dynamic arg92,
      dynamic arg93,
      dynamic arg94,
      dynamic arg95,
      dynamic arg96,
      dynamic arg97,
      dynamic arg98,
      dynamic arg99,
      dynamic arg100,
      dynamic arg101,
      dynamic arg102,
      dynamic arg103,
      dynamic arg104,
      dynamic arg105,
      dynamic arg106,
      dynamic arg107,
      dynamic arg108,
      dynamic arg109,
      dynamic arg110,
      dynamic arg111,
      dynamic arg112,
      dynamic arg113,
      dynamic arg114,
      dynamic arg115,
      dynamic arg116,
      dynamic arg117,
      dynamic arg118,
      dynamic arg119,
      dynamic arg120,
      dynamic arg121,
      dynamic arg122,
      dynamic arg123,
      dynamic arg124,
      dynamic arg125,
      dynamic arg126]) {
    if (_base == null || _memory == null) {
      throw StateError('Call copyWith first!');
    }
    Function base = _base!;
    Memory memory = _memory!;
    List<Object> args = [];
    if (arg0 != null) {
      args.add(arg0);
      if (arg1 != null) {
        args.add(arg1);
        if (arg2 != null) {
          args.add(arg2);
          if (arg3 != null) {
            args.add(arg3);
            if (arg4 != null) {
              args.add(arg4);
              if (arg5 != null) {
                args.add(arg5);
                if (arg6 != null) {
                  args.add(arg6);
                  if (arg7 != null) {
                    args.add(arg7);
                    if (arg8 != null) {
                      args.add(arg8);
                      if (arg9 != null) {
                        args.add(arg9);
                        if (arg10 != null) {
                          args.add(arg10);
                          if (arg11 != null) {
                            args.add(arg11);
                            if (arg12 != null) {
                              args.add(arg12);
                              if (arg13 != null) {
                                args.add(arg13);
                                if (arg14 != null) {
                                  args.add(arg14);
                                  if (arg15 != null) {
                                    args.add(arg15);
                                    if (arg16 != null) {
                                      args.add(arg16);
                                      if (arg17 != null) {
                                        args.add(arg17);
                                        if (arg18 != null) {
                                          args.add(arg18);
                                          if (arg19 != null) {
                                            args.add(arg19);
                                            if (arg20 != null) {
                                              args.add(arg20);
                                              if (arg21 != null) {
                                                args.add(arg21);
                                                if (arg22 != null) {
                                                  args.add(arg22);
                                                  if (arg23 != null) {
                                                    args.add(arg23);
                                                    if (arg24 != null) {
                                                      args.add(arg24);
                                                      if (arg25 != null) {
                                                        args.add(arg25);
                                                        if (arg26 != null) {
                                                          args.add(arg26);
                                                          if (arg27 != null) {
                                                            args.add(arg27);
                                                            if (arg28 != null) {
                                                              args.add(arg28);
                                                              if (arg29 !=
                                                                  null) {
                                                                args.add(arg29);
                                                                if (arg30 !=
                                                                    null) {
                                                                  args.add(
                                                                      arg30);
                                                                  if (arg31 !=
                                                                      null) {
                                                                    args.add(
                                                                        arg31);
                                                                    if (arg32 !=
                                                                        null) {
                                                                      args.add(
                                                                          arg32);
                                                                      if (arg33 !=
                                                                          null) {
                                                                        args.add(
                                                                            arg33);
                                                                        if (arg34 !=
                                                                            null) {
                                                                          args.add(
                                                                              arg34);
                                                                          if (arg35 !=
                                                                              null) {
                                                                            args.add(arg35);
                                                                            if (arg36 !=
                                                                                null) {
                                                                              args.add(arg36);
                                                                              if (arg37 != null) {
                                                                                args.add(arg37);
                                                                                if (arg38 != null) {
                                                                                  args.add(arg38);
                                                                                  if (arg39 != null) {
                                                                                    args.add(arg39);
                                                                                    if (arg40 != null) {
                                                                                      args.add(arg40);
                                                                                      if (arg41 != null) {
                                                                                        args.add(arg41);
                                                                                        if (arg42 != null) {
                                                                                          args.add(arg42);
                                                                                          if (arg43 != null) {
                                                                                            args.add(arg43);
                                                                                            if (arg44 != null) {
                                                                                              args.add(arg44);
                                                                                              if (arg45 != null) {
                                                                                                args.add(arg45);
                                                                                                if (arg46 != null) {
                                                                                                  args.add(arg46);
                                                                                                  if (arg47 != null) {
                                                                                                    args.add(arg47);
                                                                                                    if (arg48 != null) {
                                                                                                      args.add(arg48);
                                                                                                      if (arg49 != null) {
                                                                                                        args.add(arg49);
                                                                                                        if (arg50 != null) {
                                                                                                          args.add(arg50);
                                                                                                          if (arg51 != null) {
                                                                                                            args.add(arg51);
                                                                                                            if (arg52 != null) {
                                                                                                              args.add(arg52);
                                                                                                              if (arg53 != null) {
                                                                                                                args.add(arg53);
                                                                                                                if (arg54 != null) {
                                                                                                                  args.add(arg54);
                                                                                                                  if (arg55 != null) {
                                                                                                                    args.add(arg55);
                                                                                                                    if (arg56 != null) {
                                                                                                                      args.add(arg56);
                                                                                                                      if (arg57 != null) {
                                                                                                                        args.add(arg57);
                                                                                                                        if (arg58 != null) {
                                                                                                                          args.add(arg58);
                                                                                                                          if (arg59 != null) {
                                                                                                                            args.add(arg59);
                                                                                                                            if (arg60 != null) {
                                                                                                                              args.add(arg60);
                                                                                                                              if (arg61 != null) {
                                                                                                                                args.add(arg61);
                                                                                                                                if (arg62 != null) {
                                                                                                                                  args.add(arg62);
                                                                                                                                  if (arg63 != null) {
                                                                                                                                    args.add(arg63);
                                                                                                                                    if (arg64 != null) {
                                                                                                                                      args.add(arg64);
                                                                                                                                      if (arg65 != null) {
                                                                                                                                        args.add(arg65);
                                                                                                                                        if (arg66 != null) {
                                                                                                                                          args.add(arg66);
                                                                                                                                          if (arg67 != null) {
                                                                                                                                            args.add(arg67);
                                                                                                                                            if (arg68 != null) {
                                                                                                                                              args.add(arg68);
                                                                                                                                              if (arg69 != null) {
                                                                                                                                                args.add(arg69);
                                                                                                                                                if (arg70 != null) {
                                                                                                                                                  args.add(arg70);
                                                                                                                                                  if (arg71 != null) {
                                                                                                                                                    args.add(arg71);
                                                                                                                                                    if (arg72 != null) {
                                                                                                                                                      args.add(arg72);
                                                                                                                                                      if (arg73 != null) {
                                                                                                                                                        args.add(arg73);
                                                                                                                                                        if (arg74 != null) {
                                                                                                                                                          args.add(arg74);
                                                                                                                                                          if (arg75 != null) {
                                                                                                                                                            args.add(arg75);
                                                                                                                                                            if (arg76 != null) {
                                                                                                                                                              args.add(arg76);
                                                                                                                                                              if (arg77 != null) {
                                                                                                                                                                args.add(arg77);
                                                                                                                                                                if (arg78 != null) {
                                                                                                                                                                  args.add(arg78);
                                                                                                                                                                  if (arg79 != null) {
                                                                                                                                                                    args.add(arg79);
                                                                                                                                                                    if (arg80 != null) {
                                                                                                                                                                      args.add(arg80);
                                                                                                                                                                      if (arg81 != null) {
                                                                                                                                                                        args.add(arg81);
                                                                                                                                                                        if (arg82 != null) {
                                                                                                                                                                          args.add(arg82);
                                                                                                                                                                          if (arg83 != null) {
                                                                                                                                                                            args.add(arg83);
                                                                                                                                                                            if (arg84 != null) {
                                                                                                                                                                              args.add(arg84);
                                                                                                                                                                              if (arg85 != null) {
                                                                                                                                                                                args.add(arg85);
                                                                                                                                                                                if (arg86 != null) {
                                                                                                                                                                                  args.add(arg86);
                                                                                                                                                                                  if (arg87 != null) {
                                                                                                                                                                                    args.add(arg87);
                                                                                                                                                                                    if (arg88 != null) {
                                                                                                                                                                                      args.add(arg88);
                                                                                                                                                                                      if (arg89 != null) {
                                                                                                                                                                                        args.add(arg89);
                                                                                                                                                                                        if (arg90 != null) {
                                                                                                                                                                                          args.add(arg90);
                                                                                                                                                                                          if (arg91 != null) {
                                                                                                                                                                                            args.add(arg91);
                                                                                                                                                                                            if (arg92 != null) {
                                                                                                                                                                                              args.add(arg92);
                                                                                                                                                                                              if (arg93 != null) {
                                                                                                                                                                                                args.add(arg93);
                                                                                                                                                                                                if (arg94 != null) {
                                                                                                                                                                                                  args.add(arg94);
                                                                                                                                                                                                  if (arg95 != null) {
                                                                                                                                                                                                    args.add(arg95);
                                                                                                                                                                                                    if (arg96 != null) {
                                                                                                                                                                                                      args.add(arg96);
                                                                                                                                                                                                      if (arg97 != null) {
                                                                                                                                                                                                        args.add(arg97);
                                                                                                                                                                                                        if (arg98 != null) {
                                                                                                                                                                                                          args.add(arg98);
                                                                                                                                                                                                          if (arg99 != null) {
                                                                                                                                                                                                            args.add(arg99);
                                                                                                                                                                                                            if (arg100 != null) {
                                                                                                                                                                                                              args.add(arg100);
                                                                                                                                                                                                              if (arg101 != null) {
                                                                                                                                                                                                                args.add(arg101);
                                                                                                                                                                                                                if (arg102 != null) {
                                                                                                                                                                                                                  args.add(arg102);
                                                                                                                                                                                                                  if (arg103 != null) {
                                                                                                                                                                                                                    args.add(arg103);
                                                                                                                                                                                                                    if (arg104 != null) {
                                                                                                                                                                                                                      args.add(arg104);
                                                                                                                                                                                                                      if (arg105 != null) {
                                                                                                                                                                                                                        args.add(arg105);
                                                                                                                                                                                                                        if (arg106 != null) {
                                                                                                                                                                                                                          args.add(arg106);
                                                                                                                                                                                                                          if (arg107 != null) {
                                                                                                                                                                                                                            args.add(arg107);
                                                                                                                                                                                                                            if (arg108 != null) {
                                                                                                                                                                                                                              args.add(arg108);
                                                                                                                                                                                                                              if (arg109 != null) {
                                                                                                                                                                                                                                args.add(arg109);
                                                                                                                                                                                                                                if (arg110 != null) {
                                                                                                                                                                                                                                  args.add(arg110);
                                                                                                                                                                                                                                  if (arg111 != null) {
                                                                                                                                                                                                                                    args.add(arg111);
                                                                                                                                                                                                                                    if (arg112 != null) {
                                                                                                                                                                                                                                      args.add(arg112);
                                                                                                                                                                                                                                      if (arg113 != null) {
                                                                                                                                                                                                                                        args.add(arg113);
                                                                                                                                                                                                                                        if (arg114 != null) {
                                                                                                                                                                                                                                          args.add(arg114);
                                                                                                                                                                                                                                          if (arg115 != null) {
                                                                                                                                                                                                                                            args.add(arg115);
                                                                                                                                                                                                                                            if (arg116 != null) {
                                                                                                                                                                                                                                              args.add(arg116);
                                                                                                                                                                                                                                              if (arg117 != null) {
                                                                                                                                                                                                                                                args.add(arg117);
                                                                                                                                                                                                                                                if (arg118 != null) {
                                                                                                                                                                                                                                                  args.add(arg118);
                                                                                                                                                                                                                                                  if (arg119 != null) {
                                                                                                                                                                                                                                                    args.add(arg119);
                                                                                                                                                                                                                                                    if (arg120 != null) {
                                                                                                                                                                                                                                                      args.add(arg120);
                                                                                                                                                                                                                                                      if (arg121 != null) {
                                                                                                                                                                                                                                                        args.add(arg121);
                                                                                                                                                                                                                                                        if (arg122 != null) {
                                                                                                                                                                                                                                                          args.add(arg122);
                                                                                                                                                                                                                                                          if (arg123 != null) {
                                                                                                                                                                                                                                                            args.add(arg123);
                                                                                                                                                                                                                                                            if (arg124 != null) {
                                                                                                                                                                                                                                                              args.add(arg124);
                                                                                                                                                                                                                                                              if (arg125 != null) {
                                                                                                                                                                                                                                                                args.add(arg125);
                                                                                                                                                                                                                                                                if (arg126 != null) {
                                                                                                                                                                                                                                                                  args.add(arg126);
                                                                                                                                                                                                                                                                }
                                                                                                                                                                                                                                                              }
                                                                                                                                                                                                                                                            }
                                                                                                                                                                                                                                                          }
                                                                                                                                                                                                                                                        }
                                                                                                                                                                                                                                                      }
                                                                                                                                                                                                                                                    }
                                                                                                                                                                                                                                                  }
                                                                                                                                                                                                                                                }
                                                                                                                                                                                                                                              }
                                                                                                                                                                                                                                            }
                                                                                                                                                                                                                                          }
                                                                                                                                                                                                                                        }
                                                                                                                                                                                                                                      }
                                                                                                                                                                                                                                    }
                                                                                                                                                                                                                                  }
                                                                                                                                                                                                                                }
                                                                                                                                                                                                                              }
                                                                                                                                                                                                                            }
                                                                                                                                                                                                                          }
                                                                                                                                                                                                                        }
                                                                                                                                                                                                                      }
                                                                                                                                                                                                                    }
                                                                                                                                                                                                                  }
                                                                                                                                                                                                                }
                                                                                                                                                                                                              }
                                                                                                                                                                                                            }
                                                                                                                                                                                                          }
                                                                                                                                                                                                        }
                                                                                                                                                                                                      }
                                                                                                                                                                                                    }
                                                                                                                                                                                                  }
                                                                                                                                                                                                }
                                                                                                                                                                                              }
                                                                                                                                                                                            }
                                                                                                                                                                                          }
                                                                                                                                                                                        }
                                                                                                                                                                                      }
                                                                                                                                                                                    }
                                                                                                                                                                                  }
                                                                                                                                                                                }
                                                                                                                                                                              }
                                                                                                                                                                            }
                                                                                                                                                                          }
                                                                                                                                                                        }
                                                                                                                                                                      }
                                                                                                                                                                    }
                                                                                                                                                                  }
                                                                                                                                                                }
                                                                                                                                                              }
                                                                                                                                                            }
                                                                                                                                                          }
                                                                                                                                                        }
                                                                                                                                                      }
                                                                                                                                                    }
                                                                                                                                                  }
                                                                                                                                                }
                                                                                                                                              }
                                                                                                                                            }
                                                                                                                                          }
                                                                                                                                        }
                                                                                                                                      }
                                                                                                                                    }
                                                                                                                                  }
                                                                                                                                }
                                                                                                                              }
                                                                                                                            }
                                                                                                                          }
                                                                                                                        }
                                                                                                                      }
                                                                                                                    }
                                                                                                                  }
                                                                                                                }
                                                                                                              }
                                                                                                            }
                                                                                                          }
                                                                                                        }
                                                                                                      }
                                                                                                    }
                                                                                                  }
                                                                                                }
                                                                                              }
                                                                                            }
                                                                                          }
                                                                                        }
                                                                                      }
                                                                                    }
                                                                                  }
                                                                                }
                                                                              }
                                                                            }
                                                                          }
                                                                        }
                                                                      }
                                                                    }
                                                                  }
                                                                }
                                                              }
                                                            }
                                                          }
                                                        }
                                                      }
                                                    }
                                                  }
                                                }
                                              }
                                            }
                                          }
                                        }
                                      }
                                    }
                                  }
                                }
                              }
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }
      }
    }
    return execute<T>(base, args, memory);
  }
}
