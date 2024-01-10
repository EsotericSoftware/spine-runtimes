# spine-flutter

The spine-flutter runtime provides functionality to load, manipulate and render [Spine](https://esotericsoftware.com) skeletal animation data using [Flutter](https://flutter.dev/). spine-flutter is based on [spine-cpp](../spine-cpp) and supports desktop and mobile Flutter deployment targets. spine-flutter does not support Flutter's web deployment target.

# See the [spine-flutter documentation](https://esotericsoftware.com/spine-flutter) for in-depth information.

The `spine_flutter` package name was previously used to publish the [Spine Flutter Runtime in plain Dart](https://github.com/jtakakura/spine_flutter/) by Junji Takakura. Junji has kindly transferred the package name to us and is now publishing his Dart-only Spine Flutter Runtime under the package name [spine_flutter_dart](https://pub.dev/packages/spine_flutter_dart).

## Licensing

You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](https://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](https://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-flutter works with data exported from Spine 4.2.xx.

spine-flutter supports all Spine features, except two-color tinting and the screen blend mode.

## Supported platforms
The spine-flutter runtime works on desktop, mobile and web. Web deployment requires canvaskit, which will add about 2mb of dependencies to your web deployment. You can compile your app for web with Canvaskit like this:

```
flutter build web --web-renderer canvaskit
```

## Setup
To add `spine_flutter` to your Flutter project, add the following dependency to your `pubspec.yaml` file:

```yaml
dependencies:
  ...
  spine_flutter: ^4.2.11
```

In your `main()`, add these two lines in the beginning to initialize the Spine Flutter runtime:

```dart
void main() {
    WidgetsFlutterBinding.ensureInitialized();
    await initSpineFlutter(enableMemoryDebugging: false);
    ...
}
```

## Example
If you have pulled the `spine_flutter` package from [pub.dev](https://pub.dev) directly, you can run the example in the `example/` folder as is:

```bash
cd path/to/downloaded/spine_flutter
cd example
flutter run
```

Otherwise you can run the example like this:

1. install the [Flutter SDK](https://docs.flutter.dev/get-started/install), then run `flutter doctor` which will instruct you what other dependencies to install.
2. Clone this repository `git clone https://github.com/esotericsoftware/spine-runtimes`
3. Run `setup.sh` in the `spine-flutter/` folder. On Windows, you can use [Git Bash](https://gitforwindows.org/) included in Git for Window to run the `setup.sh` Bash script.

You can then open `spine-flutter` in an IDE or editor of your choice that supports Flutter, like [IntelliJ IDEA/Android Studio](https://docs.flutter.dev/get-started/editor?tab=androidstudio) or [Visual Studio Code](https://docs.flutter.dev/get-started/editor?tab=vscode) to inspect and run the example. 

Alternatively, you can run the example from the [command line](https://docs.flutter.dev/get-started/test-drive?tab=terminal).

## Development
If all you modify are the Dart sources of the plugin, then the development setup is the same as the setup described under "Example" above.

If you need to work on the `dart:ffi` bindings for `spine-cpp`, you will also need to install [Emscripten](https://emscripten.org/docs/getting_started/downloads.html).

To generate the bindings based on the `src/spine_flutter.h` header, run `dart run ffigen --config ffigen.yaml`. After the bindings have been generated, you must replace the line `import 'dart:ffi' as ffi;` with `import 'ffi_proxy.dart' as ffi;` in the file `src/spine_flutter_bindings_generated.dart`. Otherwise the bindings will not compile for the web.

If you made changes to `spine-cpp` or the source files in `src/`, you must run `compile-wasm.sh`. This will compile `spine-cpp` and the bindings for the Web and place updated versions of `libspine_flutter.js` and `libspine_flutter.wasm` in the `lib/assets/` folder. For web builds, the `initSpineFlutterFFI()` function in `lib/init_web.dart` will load these files from the package's asset bundle.
