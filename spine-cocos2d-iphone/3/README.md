# spine-cocos2d-iphone v3.0

The spine-cocos2d-iphone runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [cocos2d-iphone](http://www.cocos2d-iphone.org/). spine-cocos2d-iphone is based on [spine-c](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-c).

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
1. Place the contents of a cocos2d version 3.0.x distribution into the `spine-cocos2d-iphone/3.0/cocos2d` directory.
1. Open the XCode project file for iOS or Mac from the `spine-cocos2d-iphone/3.0` directory.

Alternatively, the contents of the `spine-c/src`, `spine-c/include` and `spine-cocos2d-iphone/3.0/src` directories can be copied into your project. Be sure your header search path will find the contents of the `spine-c/include` and `spine-cocos2d-iphone/3.0/src` directories. Note that the includes use `spine/Xxx.h`, so the `spine` directory cannot be omitted when copying the files.

## Examples

[Spineboy](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-cocos2d-iphone/3.0/example/SpineboyExample.cpp)
[Golbins](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-cocos2d-iphone/3.0/example/GoblinsExample.cpp)

## Links

[podspec](https://github.com/ldomaradzki/spine-runtimes/blob/master/Spine-Cocos2d-iPhone.podspec) (maintained externally)
