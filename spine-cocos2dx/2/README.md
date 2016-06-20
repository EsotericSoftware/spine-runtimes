# spine-cocos2dx v2

The spine-cocos2dx runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [cocos2d-x](http://www.cocos2d-x.org/). spine-cocos2dx is based on [spine-c](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-c).

## Licensing

This Spine Runtime may only be used for personal or internal use, typically to evaluate Spine before purchasing. If you would like to incorporate a Spine Runtime into your applications, distribute software containing a Spine Runtime, or modify a Spine Runtime, then you will need a valid [Spine license](https://esotericsoftware.com/spine-purchase). Please see the [Spine Runtimes Software License](https://github.com/EsotericSoftware/spine-runtimes/blob/master/LICENSE) for detailed information.

The Spine Runtimes are developed with the intent to be used with data exported from Spine. By purchasing Spine, `Section 2` of the [Spine Software License](https://esotericsoftware.com/files/license.txt) grants the right to create and distribute derivative works of the Spine Runtimes.

## Spine version

spine-cocos2dx v2 works with data exported from Spine 3.1.08. Updating spine-cocos2dx v2 to [v3.2](https://github.com/EsotericSoftware/spine-runtimes/issues/586) and [v3.3](https://github.com/EsotericSoftware/spine-runtimes/issues/613) is in progress.

spine-cocos2dx v2 supports all Spine features.

spine-cocos2dx v2 does not yet support loading the binary format.

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
1. Place the contents of a cocos2d-x version 2.2.3 distribution into the `spine-cocos2dx/2/cocos2dx` directory.
1. Open the XCode (Mac) or Visual C++ 2012 Express (Windows) project file from the `spine-cocos2dx/2/example` directory. Build files are also provided for Android.

Alternatively, the contents of the `spine-c/src`, `spine-c/include` and `spine-cocos2dx/2/src` directories can be copied into your project. Be sure your header search path will find the contents of the `spine-c/include` and `spine-cocos2dx/2/src` directories. Note that the includes use `spine/Xxx.h`, so the `spine` directory cannot be omitted when copying the files.

## Notes

- Images are premultiplied by cocos2d-x, so the Spine atlas images should *not* use premultiplied alpha.

## Examples

- [Spineboy](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-cocos2dx/2/example/Classes/SpineboyExample.cpp)
- [Golbins](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-cocos2dx/2/example/Classes/GoblinsExample.cpp)
