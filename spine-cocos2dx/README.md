# spine-cocos2dx

The spine-cocos2dx runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [cocos2d-x](http://www.cocos2d-x.org/). spine-cocos2dx is based on [spine-c](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-c).

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
1. Place the `cocos2dx` directory from a cocos2d-x (cocos2d-2.1rc0-x-2.1.2 or later) distribution into the `spine-cocos2dx` directory.
1. Open the XCode (Mac) or Visual C++ 2012 Express (Windows) project file from the `spine-cocos2dx/example` directory. Build files are also provided for Android.

Alternatively, the contents of the `spine-c/src`, `spine-c/include` and `spine-cocos2dx/src` directories can be copied into your project. Be sure your header search path will find the contents of the `spine-c/include` and `spine-cocos2dx/src` directories. Note that the includes use `spine/Xxx.h`, so the `spine` directory cannot be omitted when copying the files.

## Examples

[Simple example](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-cocos2dx/example/Classes/ExampleLayer.cpp#L18)
