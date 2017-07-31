# spine-cocos2d-objc

The spine-cocos2d-objc runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using the latest [cocos2d-objc](http://cocos2d-objc.org/). spine-cocos2d-objc is based on [spine-c](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-c).

## Licensing

This Spine Runtime may only be used for personal or internal use, typically to evaluate Spine before purchasing. If you would like to incorporate a Spine Runtime into your applications, distribute software containing a Spine Runtime, or modify a Spine Runtime, then you will need a valid [Spine license](https://esotericsoftware.com/spine-purchase). Please see the [Spine Runtimes Software License](https://github.com/EsotericSoftware/spine-runtimes/blob/master/LICENSE) for detailed information.

The Spine Runtimes are developed with the intent to be used with data exported from Spine. By purchasing Spine, `Section 2` of the [Spine Software License](https://esotericsoftware.com/files/license.txt) grants the right to create and distribute derivative works of the Spine Runtimes.

## Spine version

spine-cocos2d-objc works with data exported from Spine 3.6.x.

spine-cocos2d-objc supports all Spine features.

spine-cocos2d-objc does not yet support loading the binary format.

## Usage

1. Create a new cocos2d-obj project. See the [cocos2d-objc documentation](http://cocos2d-objc.org/started/) or have a look at the example in this repository.
2. Download the Spine Runtimes source using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/3.6.zip)
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
3. Download the Spine Runtimes repository using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/3.6.zip)
4. Open a terminal, and `cd` into the `spine-runtimes/spine-cocos2d-objc` folder
5. Type `mkdir build && cd build && cmake ../..`, this will download the cocos2d-objc dependency
6. Open the Xcode project in `spine-runtimes/spine-cocos2d-objc/spine-cocos2d-objc.xcodeproj/`
7. In Xcode, click the `Run` button or type `CMD+R` to run the example on the simulator

## Links

[podspec](https://github.com/ldomaradzki/spine-runtimes/blob/master/Spine-Cocos2d-iPhone.podspec) (maintained externally)
