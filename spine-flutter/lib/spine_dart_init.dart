//
// Spine Runtimes License Agreement
// Last updated April 5, 2025. Replaces all prior versions.
//
// Copyright (c) 2013-2025, Esoteric Software LLC
//
// Integration of the Spine Runtimes into software or otherwise creating
// derivative works of the Spine Runtimes is permitted under the terms and
// conditions of Section 2 of the Spine Editor License Agreement:
// http://esotericsoftware.com/spine-editor-license
//
// Otherwise, it is permitted to integrate the Spine Runtimes into software
// or otherwise create derivative works of the Spine Runtimes (collectively,
// "Products"), provided that each user of the Products must obtain their own
// Spine Editor license and redistribution of the Products in any form must
// include this license and copyright notice.
//
// THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
// BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

import 'dart:io';

import 'package:universal_ffi/ffi.dart';
import 'package:universal_ffi/ffi_utils.dart';

const String _libName = 'spine_flutter';

class SpineDartFFI {
  DynamicLibrary dylib;
  Allocator allocator;

  SpineDartFFI(this.dylib, this.allocator);
}

Future<DynamicLibrary> initDynamicLibrary() async {
  if (Platform.isMacOS || Platform.isIOS) {
    try {
      return await DynamicLibrary.open('$_libName.framework/$_libName');
    } catch (_) {
      try {
        return await DynamicLibrary.open('spine-flutter.framework/spine-flutter');
      } catch (_) {
        // Fallback for macOS where the library might not be in a framework
        return await DynamicLibrary.open('$_libName.dylib');
      }
    }
  } else if (Platform.isAndroid || Platform.isLinux) {
    return await DynamicLibrary.open('lib$_libName.so');
  } else if (Platform.isWindows) {
    return await DynamicLibrary.open('$_libName.dll');
  } else {
    throw UnsupportedError('Unknown platform: ${Platform.operatingSystem}');
  }
}

Future<SpineDartFFI> initSpineDartFFI(bool useStaticLinkage) async {
  if (useStaticLinkage) {
    return SpineDartFFI(DynamicLibrary.process(), malloc);
  } else {
    return SpineDartFFI(await initDynamicLibrary(), malloc);
  }
}
