# spine-cocos2dx v3.x & v4.x

The spine-cocos2dx runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [cocos2d-x](http://www.cocos2d-x.org/). spine-cocos2dx is based on [spine-cpp](../spine-cpp).

## Licensing

You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](http://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](http://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-cocos2dx works with data exported from Spine 4.1.xx.

spine-cocos2dx supports all Spine features.

## Setup

The setup for cocos2d-x differs from most other Spine Runtimes because the cocos2d-x distribution includes a copy of the Spine Runtime files. This is not ideal because these files may be old and fail to work with the latest Spine editor. Also it means if cocos2d-x is updated, you may get newer Spine Runtime files which can break your application if you are not using the latest Spine editor. For these reasons, we have requested cocos2d-x to cease distributing the Spine Runtime files, but they  continue to do so. The following instructions allow you to use the official Spine cocos2d-x runtime with your cocos2d-x project.

spine-cocos2dx works with both Cocos2d-x v3 and v4. The setup process is identical in both cases. The preferred way to integrate spine-cocos2dx into your Cocos2d-x project is to use the [Cocos2d-x CMake build system].


1. [Create a new C++ cocos2d-x project](https://docs.cocos2d-x.org/cocos2d-x/v4/en/editors_and_tools/cocosCLTool.html). Let's assume you created your project in a folder `/path/to/MyGame/` somewhere on your disk.
2. Download the Spine Runtimes source using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it as a zip via the download button above. Let's assume you cloned the Spine Runtimes to a folder `/path/to/spine-runtimes/` somewhere on your disk.
3. Open `MyGame/CMakeLists.txt` in a text editor and modify it as follows:
	* After the line `project(${APP_NAME})` add the following line:
	 	```
		include(/path/to/spine-runtimes/spine-cocos2dx/spine-cocos2dx.cmake)
		```	  
	* Before the line `target_link_libraries(${APP_NAME} cocos2d)`add the following line:
		```
		target_link_libraries(${APP_NAME} spine-cpp spine-cocos2dx)
		```
4. [Proceed with generating IDE files via CMake](https://docs.cocos2d-x.org/cocos2d-x/v4/en/installation/CMake-Guide.html) and build and run your project.

For reference, have a look at our spine-cocos2dx example project in this repository described below.

## Example
The spine-cocos2dx example works on Windows, Linux, macOS, iOS, Linux, and Android, for both cocos2d-x v3 and v4.

Please [install the reprequisit software](https://docs.cocos2d-x.org/cocos2d-x/v4/en/installation/prerequisites.html) as per the Cocos2d-x documentation. Ensure that the following programs are in your `PATH` environment variable and thus executable from the command line:

* `git`
* `cmake`
* `python`

Before you can compile and run the example project for a specific target platform, you need to clone the [Cocos2d-x repository](https://github.com/cocos2d/cocos2d-x) to `spine-runtimes/spine-cocos2dx/example/cocos2d` and download the dependencies:

```
cd spine-runtimes/spine-cocos2dx/example
git clone -b v4 --depth 1 https://github.com/cocos2d/cocos2d-x cocos2d
python cocos2d/download-deps.py -r yes
```

> **NOTE:** If you want to run the example with Cocos2d-x version 3, replace `-b v4` with `-b v3` in the `git clone` command.

> **NOTE:** On macOS Big Sur, replace `python` with `python3`.

You can now use CMake to create IDE projects for the target platform you want to compile and run the example on.

### macOS
Execute the following on the command line:

```
cd spine-runtimes/spine-cocos2dx/example
mkdir build-macos && cmake . -GXcode -Bbuild-macos
open build-macos/spine-cocos2dx-example.xcodeproj
```

This will generate an Xcode project in `build-macos/spine-cocos2dx-example.xcodeproj` and open it in Xcode. To build and run the example, select the `spine-cocos2dx-example` scheme and press `CMD + R`.

### iOS
Execute the following on the command line:

```
cd spine-runtimes/spine-cocos2dx/example
mkdir build-ios && cmake . -GXcode -DCMAKE_SYSTEM_NAME=iOS -DCMAKE_OSX_SYSROOT=iphoneos -Bbuild-ios
open build-ios/spine-cocos2dx-example.xcodeproj
```

This will generate an Xcode project in `build-ios/spine-cocos2dx-example.xcodeproj` and open it in Xcode. To build and run the example, select the `spine-cocos2dx-example` scheme and select a device or simulator to build for and run on. Finally, press `CMD + R` to build and run the example.

### Android
Open the project in `proj.android` in Android Studio. Make sure you have NDK version `24.0.8215888` installed via the SDK Manager. Alternatively, you can set the `ndkVersion` property in `proj.android/app/build.gradle` to the NDK version you have installed locally.

### Windows
Execute the following on the command line:

```
cd spine-runtimes/spine-cocos2dx/example
mkdir build-win
cmake . -G "Visual Studio 16 2019" -A Win32 -T host=x86 -Bbuild-win
```

You can then open the file `build-win/spine-cocos2dx-example.sln` with Visual Studio 2019. In the solution explorer, right click the `spine-cocos2dx-example` project, then click `Set as Startup Project` in the context menu. Finally, click the `Local Windows Debugger` button to build and run the example.

## Notes

* Images are premultiplied by cocos2d-x, so the Spine atlas images should *not* use premultiplied alpha.
* Two color tinting needs to be enabled on a per-skeleton basis. Call `SkeletonRenderer::setTwoColorTine(true)` or `SkeletonAnimation::setTwoColorTint(true)` after you created the skeleton instance. Note that two color tinting requires a custom shader and vertex format. Skeletons rendered with two color tinting can therefore not be batched with single color tinted skeletons or other 2D cocos2d-x elements like sprites. However, two-color tinted skeletons will be batched if possible when rendered after one another. Attaching a child to a two color tinted skeleton will also break the batch.

