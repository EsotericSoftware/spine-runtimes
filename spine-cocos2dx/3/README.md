# spine-cocos2dx v3.x

The spine-cocos2dx runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [cocos2d-x](http://www.cocos2d-x.org/). spine-cocos2dx is based on [spine-c](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-c).

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
1. Place the contents of a cocos2d-x version 3.x distribution into the `spine-cocos2dx/3/cocos2dx` directory.
1. Run the `python download-deps.py` script in the `spine-cocos2dx/3/cocos2dx` directory.
1. Open the XCode (Mac) or Visual C++ 2012 Express (Windows) project file from the `spine-cocos2dx/3/example` directory. Build files are also provided for Android.

Alternatively, the contents of the `spine-c/src`, `spine-c/include` and `spine-cocos2dx/3/src` directories can be copied into your project. Be sure your header search path will find the contents of the `spine-c/include` and `spine-cocos2dx/3.1/src` directories. Note that the includes use `spine/Xxx.h`, so the `spine` directory cannot be omitted when copying the files.

## Notes

- Images are premultiplied by cocos2d-x, so the Spine atlas images should *not* use premultiplied alpha.

## Examples

- [Raptor](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-cocos2dx/3/example/Classes/RaptorExample.cpp)
- [Spineboy](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-cocos2dx/3/example/Classes/SpineboyExample.cpp)
- [Golbins](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-cocos2dx/3/example/Classes/GoblinsExample.cpp)
