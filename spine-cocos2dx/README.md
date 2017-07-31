# spine-cocos2dx v3.x

The spine-cocos2dx runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [cocos2d-x](http://www.cocos2d-x.org/). spine-cocos2dx is based on [spine-c](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-c).

## Licensing

This Spine Runtime may only be used for personal or internal use, typically to evaluate Spine before purchasing. If you would like to incorporate a Spine Runtime into your applications, distribute software containing a Spine Runtime, or modify a Spine Runtime, then you will need a valid [Spine license](https://esotericsoftware.com/spine-purchase). Please see the [Spine Runtimes Software License](https://github.com/EsotericSoftware/spine-runtimes/blob/master/LICENSE) for detailed information.

The Spine Runtimes are developed with the intent to be used with data exported from Spine. By purchasing Spine, `Section 2` of the [Spine Software License](https://esotericsoftware.com/files/license.txt) grants the right to create and distribute derivative works of the Spine Runtimes.

## Spine version

spine-cocos2dx works with data exported from Spine 3.6.xx.

spine-cocos2dx supports all Spine features.

## Setup

The setup for cocos2d-x differs from most other Spine Runtimes because the cocos2d-x distribution includes a copy of the Spine Runtime files. This is not ideal because these files may be old and fail to work with the latest Spine editor. Also it means if cocos2d-x is updated, you may get newer Spine Runtime files which can break your application if you are not using the latest Spine editor. For these reasons, we have requested cocos2d-x to cease distributing the Spine Runtime files, but they  continue to do so. The following instructions allow you to use the official Spine cocos2d-x runtime with your cocos2d-x project.

1. Create a new cocos2d-x project. See [the cocos2d-x documentation](http://www.cocos2d-x.org/docs/static-pages/installation.html)
2. Delete the folder `cocos2d/cocos/editor-support/spine`. This will remove the outdated Spine cocos2d-x runtime shipped by cocos2d-x.
3. Open your project in your IDE of choice, then open the cocos2d_libs sub project and delete the `editor-support/spine` group. This will remove the outdated Spine cocos2d-x runtime shipped by cocos2d-x from your build.
3. Download the Spine Runtimes source using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/3.6.zip)
4. Add the sources from `spine-c/spine-c/src/spine` and `spine-cocos2dx/src/spine` to your project
4. Add the folders `spine-c/spine-c/include` and `spine-cocos2dx/src` to your header search path. Note that includes are specified as `#inclue <spine/file.h>`, so the `spine` directory cannot be omitted when copying the source files.

## Example
The Spine cocos2d-x example works on Windows and Mac OS X.

### Windows
1. Install [Visual Studio 2015 Community](https://www.visualstudio.com/en-us/downloads/download-visual-studio-vs.aspx)
2. Install CMake via the [Windows installer package](https://cmake.org/download/).
3. Download the Spine Runtimes repository using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/3.6.zip)
4. Run CMake GUI from the start menu
5. Click `Browse Source` and select the directory `spine-runtimes`
6. Click `Browse Build` and select the `spine-runtimes/spine-cocos2dx/build` directory. You can create the `build` folder directly in the file dialog via `New Folder`.
7. Click `Configure`. This will download the cocos2d-x dependency and wire it up with the example source code in `spine-runtimes/spine-cocos2dx/example`. The download is 400mb, so get yourself a cup of tea.
7. Open the file `spine-cocos2dx\example\cocos2d\cocos\2d\cocos2dx.props` and remove the `libSpine.lib` entry from the `<AdditionalDependencies>` tag.
8. Open the `spine-runtimes/spine-cocos2dx/example/proj.win32/spine-cocos2d-x.sln` file in Visual Studio 2015. Visual Studio may ask you to install the Windows XP/7 SDK, which you should install.
9. Expand the cocos2d_libs sub project and delete the `editor-support/spine` group. This will remove the outdated Spine cocos2d-x runtime shipped by cocos2d-x from your build.
9. Expand `References` of the cocos2d_libs sub project, and remove the entry for `libSpine`, which should be marked with an error.
9. Right click the `spine-cocos2d-x` project in the solution explorer and select `Set as Startup Project` from the context menu
10. Click `Local Windows Debugger` to run the example

### macOS/iOS
1. Install [Xcode](https://developer.apple.com/xcode/)
2. Install [Homebrew](http://brew.sh/)
3. Open a terminal and install CMake via `brew install cmake`
3. Download the Spine Runtimes repository using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/3.6.zip)
4. Open a terminal, and `cd` into the `spine-runtimes/spine-cocos2dx` folder
5. Type `mkdir build && cd build && cmake ../..`. This will download the cocos2d-x dependency and wire it up with the example source code in `spine-runtimes/spine-cocos2dx/example`. The download is 400mb, so get yourself a cup of tea.
6. Open the Xcode project in `spine-runtimes/spine-cocos2dx/example/proj.ios_mac`
7. Expand the `cocos2d_libs.xcodeproj` sub project, delete the group `editor-support/spine`. This will remove the outdated Spine cocos2d-x runtime shipped by cocos2d-x.
8. Click the `Run` button or type `CMD+R` to run the example

### Android
1. Install the prerequisits for [cocos2d-x Android development](http://www.cocos2d-x.org/docs/installation/Android-terminal/)
2. Install [Homebrew](http://brew.sh/)
3. Open a terminal and install CMake via `brew install cmake`
3. Download the Spine Runtimes repository using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/3.6.zip)
4. Open a terminal, and `cd` into the `spine-runtimes/spine-cocos2dx` folder
5. Type `mkdir build && cd build && cmake ../..`. This will download the cocos2d-x dependency and wire it up with the example source code in `spine-runtimes/spine-cocos2dx/example`. The download is 400mb, so get yourself a cup of tea.
6. Delete `spine-runtimes/spine-cocos2dx/example/cocos2d/cocos/editor-support/spine`
7. Open `spine-runtimes/spine-cocos2dx/example/cocos2d/cocos/Android.mk` and remove the lines `LOCAL_STATIC_LIBRARIES += spine_static` and `$(call import-module,editor-support/spine)
8. Switch to `spine-runtimes/spine-cocos2dx/example/proj.android/jni` and execute `cocos compile -p android -m debug --ndk-mode debug` to compile the example for Android
9. In the same directory, execute `cocos run -p android -m debug` to deploy to the device
10. For debugging, run `ndk-debug` in the `proj.android/jni` folder. This will attach to the running app via GDB.

## Notes

* Images are premultiplied by cocos2d-x, so the Spine atlas images should *not* use premultiplied alpha.
* Two color tinting needs to be enabled on a per-skeleton basis. Call `SkeletonRenderer::setTwoColorTine(true)` or `SkeletonAnimation::setTwoColorTint(true)` after you created the skeleton instance. Note that two color tinting requires a custom shader and vertex format. Skeletons rendered with two color tinting can therefore not be batched with single color tinted skeletons or other 2D cocos2d-x elements like sprites. However, two-color tinted skeletons will be batched if possible when rendered after one another. Attaching a child to a two color tinted skeleton will also break the batch.

## Examples

- [Raptor](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-cocos2dx/example/Classes/RaptorExample.cpp)
- [Spineboy](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-cocos2dx/example/Classes/SpineboyExample.cpp)
- [Golbins](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-cocos2dx/example/Classes/GoblinsExample.cpp)
