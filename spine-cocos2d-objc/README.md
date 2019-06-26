# spine-cocos2d-objc

The spine-cocos2d-objc runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using the latest [cocos2d-objc](http://cocos2d-objc.org/). spine-cocos2d-objc is based on [spine-c](../spine-c).

## Licensing

You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](http://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](http://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-cocos2d-objc works with data exported from Spine 3.8.x.

spine-cocos2d-objc supports all Spine features.

spine-cocos2d-objc does not yet support loading the binary format.

## Usage

1. Create a new cocos2d-obj project. See the [cocos2d-objc documentation](http://cocos2d-objc.org/started/) or have a look at the example in this repository.
2. Download the Spine Runtimes source using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it as a zip via the download button above.
3. Add the sources from `spine-c/spine-c/src/spine` and `spine-cocos2d-objc/src/spine` to your project
4. Add the folders `spine-c/spine-c/include` and `spine-cocos2d-objc/src` to your header search path. Note that includes are specified as `#inclue <spine/file.h>`, so the `spine` directory cannot be omitted when copying the source files.
5. If your project uses ARC, you have to exclude the `.m` files in `spine-cocos2d-objc/src` from ARC. See https://stackoverflow.com/questions/6646052/how-can-i-disable-arc-for-a-single-file-in-a-project for more information.

See the [Spine Runtimes documentation](http://esotericsoftware.com/spine-documentation#runtimesTitle) on how to use the APIs or check out the Spine cocos2d-objc example.

## Examples

The Spine cocos2d-objc example works on iOS simulators and devices.

## Notes
* To enable two color tinting, set `SkeletonRenderer.twoColorTint = true`. Note that skeletons rendered with this feature will not batch with other skeletons.

### iOS
1. Install [Xcode](https://developer.apple.com/xcode/)
2. Install [Homebrew](http://brew.sh/)
3. Open a terminal and install CMake via `brew install cmake`
3. Download the Spine Runtimes repository using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it as a zip via the download button above.
4. Open a terminal, and `cd` into the `spine-runtimes/spine-cocos2d-objc` folder
5. Type `mkdir build && cd build && cmake ../..`, this will download the cocos2d-objc dependency
6. Open the Xcode project in `spine-runtimes/spine-cocos2d-objc/spine-cocos2d-objc.xcodeproj/`
7. In Xcode, click the `Run` button or type `CMD+R` to run the example on the simulator

## Links

[podspec](https://github.com/ldomaradzki/spine-runtimes/blob/master/Spine-Cocos2d-iPhone.podspec) (maintained externally)
