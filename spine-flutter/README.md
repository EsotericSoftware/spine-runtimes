# spine-flutter

The spine-godot runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [Flutter](https://flutter.dev/). spine-flutter is based on [spine-cpp](../spine-cpp) and supports desktop and mobile Flutter deployment targets. spine-flutter does not support Flutter's web deployment target.

# See the [spine-flutter documentation](http://esotericsoftware.com/spine-flutter) for in-depth information.

## Licensing

You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](http://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](http://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-flutter works with data exported from Spine 4.1.xx.

spine-flutter supports all Spine features, except two-color tinting and the screen blend mode.

## Setup

## Example
The example in this repository is directly depending on the spine-flutter sources. The sources include C++ code ([spine-cpp](../spine-cpp)) which needs to be compiled. To run the example project, install the [Flutter SDK](https://docs.flutter.dev/get-started/install), then run `flutter doctor`` which will instruct you what other dependencies to install.

Once installed, run the `setup.sh` script in the `spine-flutter` folder. This will copy [spine-cpp](../spine-cpp) to the appropriate locations.

You can then open `spine-flutter` in an IDE or editor of your choice that supports Flutter, like [IntelliJ IDEA/Android Studio](https://docs.flutter.dev/get-started/editor?tab=androidstudio) or [Visual Studio Code](https://docs.flutter.dev/get-started/editor?tab=vscode) to inspect and run the example. Alternatively, you can run the example from the [command line](https://docs.flutter.dev/get-started/test-drive?tab=terminal).

> **Note**: spine-flutter does not work on the web. spine-flutter uses spine-cpp underneath, which requires Dart FFI which is currently not supported on the web.  