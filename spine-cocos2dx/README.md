# spine-cocos2dx v3.x & v4.x

The spine-cocos2dx runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [cocos2d-x](http://www.cocos2d-x.org/). spine-cocos2dx is based on [spine-cpp](../spine-cpp).

## Licensing

You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](http://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](http://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-cocos2dx works with data exported from Spine 3.8.xx.

spine-cocos2dx supports all Spine features.

## Setup

The setup for cocos2d-x differs from most other Spine Runtimes because the cocos2d-x distribution includes a copy of the Spine Runtime files. This is not ideal because these files may be old and fail to work with the latest Spine editor. Also it means if cocos2d-x is updated, you may get newer Spine Runtime files which can break your application if you are not using the latest Spine editor. For these reasons, we have requested cocos2d-x to cease distributing the Spine Runtime files, but they  continue to do so. The following instructions allow you to use the official Spine cocos2d-x runtime with your cocos2d-x project.

### Cocos2d-x v3.x
1. Create a new cocos2d-x project. See [the cocos2d-x documentation](https://docs.cocos2d-x.org/cocos2d-x/v3/en/installation/)
2. Delete the folder `cocos2d/cocos/editor-support/spine`. This will remove the outdated Spine cocos2d-x runtime shipped by cocos2d-x.
3. Open your project in your IDE of choice, then open the cocos2d_libs sub project and delete the `editor-support/spine` group. This will remove the outdated Spine cocos2d-x runtime shipped by cocos2d-x from your build.
3. Download the Spine Runtimes source using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it as a zip via the download button above.
4. Add the sources from `spine-cpp/spine-cpp/src/spine` and `spine-cocos2dx/src/spine` to your project
4. Add the folders `spine-cpp/spine-cpp/include` and `spine-cocos2dx/src` to your header search path. Note that includes are specified as `#inclue <spine/file.h>`, so the `spine` directory cannot be omitted when copying the source files.

### Cocos2d-x v4.x
TBD

1. Create a new Cocos2D-x project. See [the cocos2d-x documentation](https://docs.cocos2d-x.org/cocos2d-x/v4/en/installation/)
2. Delete the folder `cocos2d/cocos/editor-support/spine` in your project. This will remove the outdated Spine cocos2d-x runtime shipped by cocos2d-x.
3. TBD Integration in `CMakeLists.txt`

## Example
The Spine cocos2d-x example works on Windows, Mac OS X, iOS and Android.

### Cocos2d-x v3.x

#### Windows
1. Install [Visual Studio 2019 Community](https://www.visualstudio.com/en-us/downloads/download-visual-studio-vs.aspx)
2. Install CMake via the [Windows installer package](https://cmake.org/download/).
3. Download the Spine Runtimes repository using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it as a zip via the download button above.
4. Run CMake GUI from the start menu
5. Click `Browse Source` and select the directory `spine-runtimes/spine-cocos2dx/`
6. Click `Browse Build` and select the `spine-runtimes/spine-cocos2dx/build/` directory. You can create the `build` folder directly in the file dialog via `New Folder`.
7. Click `Configure` again. This will download the cocos2d-x dependency and wire it up with the example source code in `spine-runtimes/spine-cocos2dx/example`. The download is 400mb, so get yourself a cup of tea.
8. Open the file `spine-cocos2dx\example\cocos2d\cocos\2d\cocos2dx.props` and remove the `libSpine.lib` entry from the `<AdditionalDependencies>` tag.
8. Open the `spine-runtimes/spine-cocos2dx/example/proj.win32/spine-cocos2d-x.sln` file in Visual Studio 2019. Visual Studio may ask you to install the Windows XP/7 SDK, which you should install.
9. Expand `References` of the `libcocos2d` sub project, and remove the entry for `libSpine`, which should be marked with an error.
9. Right click the `spine-cocos2d-x` project in the solution explorer and select `Set as Startup Project` from the context menu
10. Click `Local Windows Debugger` to run the example

#### macOS/iOS
1. Install [Xcode](https://developer.apple.com/xcode/)
2. Install [Homebrew](http://brew.sh/)
3. Open a terminal and install CMake via `brew install cmake`
3. Download the Spine Runtimes repository using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it as a zip via the download button above.
4. Open a terminal, and `cd` into the `spine-runtimes/spine-cocos2dx` folder
5. Type `mkdir build && cd build && cmake ../..`. This will download the cocos2d-x dependency and wire it up with the example source code in `spine-runtimes/spine-cocos2dx/example`. The download is 400mb, so get yourself a cup of tea.
6. Open the Xcode project in `spine-runtimes/spine-cocos2dx/example/proj.ios_mac`
7. Expand the `cocos2d_libs.xcodeproj` sub project, delete the group `editor-support/spine`. This will remove the outdated Spine cocos2d-x runtime shipped by cocos2d-x.
8. Click the `Run` button or type `CMD+R` to run the example

#### Android (on macOS)
1. Install the prerequisits for [cocos2d-x Android development](http://www.cocos2d-x.org/docs/installation/Android-terminal/)
2. Install [Homebrew](http://brew.sh/)
3. Open a terminal and install CMake via `brew install cmake`
3. Download the Spine Runtimes repository using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it as a zip via the download button above.
4. Open a terminal, and `cd` into the `spine-runtimes/spine-cocos2dx` folder
5. Type `mkdir build && cd build && cmake ../..`. This will download the cocos2d-x dependency and wire it up with the example source code in `spine-runtimes/spine-cocos2dx/example`. The download is 400mb, so get yourself a cup of tea.
6. Delete `spine-runtimes/spine-cocos2dx/example/cocos2d/cocos/editor-support/spine`
7. Open `spine-runtimes/spine-cocos2dx/example/cocos2d/cocos/Android.mk` and remove the lines `LOCAL_STATIC_LIBRARIES += spine_static` and `$(call import-module,editor-support/spine)
8. Switch to `spine-runtimes/spine-cocos2dx/example/proj.android/jni` and execute `cocos compile -p android -m debug --ndk-mode debug` to compile the example for Android
9. In the same directory, execute `cocos run -p android -m debug` to deploy to the device
10. For debugging, run `ndk-debug` in the `proj.android/jni` folder. This will attach to the running app via GDB.

### Cocos2d-x v4.x

Please note the [new prerequisits to compile Cocos2d-x v4 projects on different platforms](https://docs.cocos2d-x.org/cocos2d-x/v4/en/installation/prerequisites.html). This includes an installation of Python 2.7.x!

#### Windows
1. Install [Visual Studio 2019 Community](https://www.visualstudio.com/en-us/downloads/download-visual-studio-vs.aspx)
2. Install CMake via the [Windows installer package](https://cmake.org/download/).
3. Install Python and make sure the `python.exe` is in your `PATH` environment variable. Python is required by cocos2d-x`s build system.
3. Download the Spine Runtimes repository using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it as a zip via the download button above.
4. Run CMake GUI from the start menu
5. Click `Browse Source` and select the directory `spine-runtimes`
6. Click `Browse Build` and select the `spine-runtimes/spine-cocos2dx/build-v4` directory. You can create the `build-v4` folder directly in the file dialog via `New Folder`.
7. Click `Configure`. Check `USE_COCOS2DX_V4`
8. Click `Configure` again. This will download the cocos2d-x dependency and wire it up with the example source code in `spine-runtimes/spine-cocos2dx/example`. The download is 400mb, so get yourself a cup of tea.
9. Click `Generate` this will create the Visual Studio solution in `spine-runtimes/spine-cocos2dx/build-v4`.
8. Open the `spine-runtimes/spine-cocos2dx/build-v4/spine-cocos2dx-example.sln` file in Visual Studio 2019. Visual Studio may ask you to install the Windows XP/7 SDK, which you should install.
9. Right click the `spine-cocos2dx-example` project in the solution explorer and select `Set as Startup Project` from the context menu
10. Click `Local Windows Debugger` to run the example

Make sure to build the example for Windows 32-bit!

#### macOS
1. Install [Xcode](https://developer.apple.com/xcode/)
2. Install [Homebrew](http://brew.sh/)
3. Open a terminal and install CMake via `brew install cmake`
3. Download the Spine Runtimes repository using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it as a zip via the download button above.
4. Open a terminal, and `cd` into the `spine-runtimes/spine-cocos2dx` folder
5. Type `mkdir build-v4 && cd build-v4 && cmake -GXcode -DUSE_COCOS2DX_V4=on ..`. This will download the cocos2d-x dependency and wire it up with the example source code in `spine-runtimes/spine-cocos2dx/example`. The download is 400mb, so get yourself a cup of tea.
6. Open the Xcode project in `spine-runtimes/spine-cocos2dx/build-v4`
7. Make sure you select `spine-cocos2dx-example > My Mac` as the target and click the `Run` button or type `CMD+R` to run the example.

#### iOS
1. Install [Xcode](https://developer.apple.com/xcode/)
2. Install [Homebrew](http://brew.sh/)
3. Open a terminal and install CMake via `brew install cmake`
3. Download the Spine Runtimes repository using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it as a zip via the download button above.
4. Open a terminal, and `cd` into the `spine-runtimes/spine-cocos2dx` folder
5. Type `mkdir build-v4 && cd build-v4 && cmake -GXcode -DUSE_COCOS2DX_V4=on -DCMAKE_SYSTEM_NAME=iOS -DCMAKE_OSX_SYSROOT=iphoneos ..`. This will download the cocos2d-x dependency and wire it up with the example source code in `spine-runtimes/spine-cocos2dx/example`. The download is 400mb, so get yourself a cup of tea.
6. Open the Xcode project in `spine-runtimes/spine-cocos2dx/build-v4`
7. Make sure you select `spine-cocos2dx-example > Device` as the target, where `Device` is either a simulator or a physically connected device. Click the `Run` button or type `CMD+R` to run the example.

#### Android (on macOS)
1. Install the prerequisits for [cocos2d-x Android development](http://www.cocos2d-x.org/docs/installation/Android-terminal/)
2. Install [Homebrew](http://brew.sh/)
3. Open a terminal and install CMake via `brew install cmake`
3. Download the Spine Runtimes repository using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it as a zip via the download button above.
4. Open a terminal, and `cd` into the `spine-runtimes/spine-cocos2dx` folder
5. Type `mkdir build-v4 && cd build-v4 && cmake -DUSE_COCOS2DX_V4=on ..`. This will download the cocos2d-x dependency and wire it up with the example source code in `spine-runtimes/spine-cocos2dx/example`. The download is 400mb, so get yourself a cup of tea.
6. Execute `cocos run -s . -p android`, this will build, deploy and run the APK on a connected device.

## Notes

* Images are premultiplied by cocos2d-x, so the Spine atlas images should *not* use premultiplied alpha.
* Two color tinting needs to be enabled on a per-skeleton basis. Call `SkeletonRenderer::setTwoColorTine(true)` or `SkeletonAnimation::setTwoColorTint(true)` after you created the skeleton instance. Note that two color tinting requires a custom shader and vertex format. Skeletons rendered with two color tinting can therefore not be batched with single color tinted skeletons or other 2D cocos2d-x elements like sprites. However, two-color tinted skeletons will be batched if possible when rendered after one another. Attaching a child to a two color tinted skeleton will also break the batch.
