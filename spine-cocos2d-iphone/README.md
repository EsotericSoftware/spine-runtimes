# spine-cocos2d-iphone

The spine-cocos2d-iphone runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [cocos2d-iphone](http://www.cocos2d-iphone.org/). spine-cocos2d-iphone is based on [spine-c](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-c).

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
1. Place the `cocos2d`, `CocosDenshion`, and `kazmath` directories from a cocos2d-iphone (v2.1-rc1 or later) distribution into the `spine-cocos2d-iphone/libs` directory.
1. Open the XCode project file for iOS or Mac from the `spine-cocos2d-iphone` directory.

Alternatively, the contents of the `spine-c/src`, `spine-c/include` and `spine-cocos2d-iphone/src` directories can be copied into your project. Be sure your header search path will find the contents of the `spine-c/include` and `spine-cocos2d-iphone/src` directories. Note that the includes use `spine/Xxx.h`, so the `spine` directory cannot be omitted when copying the files.

## Examples

[Simple example](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-cocos2d-iphone/example/ExampleLayer.m#L13)

## Links

[podspec](https://github.com/ldomaradzki/spine-runtimes/blob/master/Spine-Cocos2d-iPhone.podspec)
