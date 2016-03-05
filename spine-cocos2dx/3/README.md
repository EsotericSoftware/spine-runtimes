# spine-cocos2dx v3.x

The spine-cocos2dx runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [cocos2d-x](http://www.cocos2d-x.org/). spine-cocos2dx is based on [spine-c](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-c).

## Licensing

This Spine Runtime may only be used for personal or internal use, typically to evaluate Spine before purchasing. If you would like to incorporate a Spine Runtime into your applications, distribute software containing a Spine Runtime, or modify a Spine Runtime, then you will need a valid [Spine license](https://esotericsoftware.com/spine-purchase). Please see the [Spine Runtimes Software License](https://github.com/EsotericSoftware/spine-runtimes/blob/master/LICENSE) for detailed information.

The Spine Runtimes are developed with the intent to be used with data exported from Spine. By purchasing Spine, `Section 2` of the [Spine Software License](https://esotericsoftware.com/files/license.txt) grants the right to create and distribute derivative works of the Spine Runtimes.

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
1. Place the contents of a cocos2d-x version 3.x distribution into the `spine-cocos2dx/3/cocos2dx` directory.
1. Run the `python download-deps.py` script in the `spine-cocos2dx/3/cocos2dx` directory.
1. Open the XCode (Mac) or Visual C++ 2012 Express (Windows) project file from the `spine-cocos2dx/3/example` directory. Build files are also provided for Android.

Alternatively, the contents of the `spine-c/src`, `spine-c/include` and `spine-cocos2dx/3/src` directories can be copied into your project. Be sure your header search path will find the contents of the `spine-c/include` and `spine-cocos2dx/3.1/src` directories. Note that the includes use `spine/Xxx.h`, so the `spine` directory cannot be omitted when copying the files.

## Setup for Cocos Studio Users (cocos2d-x-3.10)
1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
	- Extract them somewhere you can find them. You will only need `spine-c` and `spine-cocos2dx`.
1. Download and install Cocos: http://www.cocos2d-x.org/download
	- (2016 March 4) Downloading and installing Cocos Studio and cocos2d-x-3.10 includes a Spine runtime in their source files. For instance, if the default install folder is `C:\Cocos\`, you will find an existing spine-c and spine-cocos2dx runtime inside `C:\Cocos\Cocos2d-x\cocos2d-x-3.10\cocos\editor-support\spine`.
		- Replace the contents of that folder with the files from the following Spine runtime folders:
			- `spine-c/src/spine`
			- `spine-c/include/spine`
			- `spine-cocos2dx/3/src/spine`
		- Since this is the shared version of the runtime Cocos Studio uses for all projects created through it, updating the Spine runtime here also updates the Spine runtime of all those projects.

### Examples
1. Create a **new C++ Cocos Studio project**. Make sure you can find that folder. 
1. The `spine-cocos2dx/3/` runtime folder contains a folder named `example`.
	- Copy the files in `spine-cocos2dx/3/example/Classes` folder (except `AppDelegate.cpp`, `AppDelegate.h` and `AppMacros.h` into the `Classes` folder of your Cocos Studio project.)
1. Add these files into your C++ Solution. (through your IDE, Visual Studio or Eclipse, etc...)
2. If this is a newly created project, edit the `AppDelegate.cpp`. For this example, we will use RaptorExample.
	1. Add `#include "RaptorExample.h"` to the includes near the top of the file.
	2. Look for the `bool AppDelegate::applicationDidFinishLaunching()` method and replace it with this. Notice the line where it defines the scene `auto scene = RaptorExample::scene()`:

```cpp
bool AppDelegate::applicationDidFinishLaunching() {
    // initialize director
    auto director = Director::getInstance();
    auto glview = director->getOpenGLView();
    if(!glview) {
        glview = GLViewImpl::createWithRect("Spine Test", Rect(0, 0, 960, 640));
        director->setOpenGLView(glview);
    }

    director->getOpenGLView()->setDesignResolutionSize(960, 640, ResolutionPolicy::SHOW_ALL);

    // turn on display FPS
    director->setDisplayStats(true);

    // set FPS. the default value is 1.0/60 if you don't call this
    director->setAnimationInterval(1.0f / 60);

    FileUtils::getInstance()->addSearchPath("res");

    // create a scene. it's an autorelease object    
    auto scene = RaptorExample::scene();

    // run
    director->runWithScene(scene);

    return true;
}
```

You can do the same with `GoblinsExample` and `SpineboyExample`.
	



## Notes

- Images are premultiplied by cocos2d-x, so the Spine atlas images should *not* use premultiplied alpha.

## Examples

- [Raptor](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-cocos2dx/3/example/Classes/RaptorExample.cpp)
- [Spineboy](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-cocos2dx/3/example/Classes/SpineboyExample.cpp)
- [Golbins](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-cocos2dx/3/example/Classes/GoblinsExample.cpp)
