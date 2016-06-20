# spine-cocos2dx v3.x

The spine-cocos2dx runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [cocos2d-x](http://www.cocos2d-x.org/). spine-cocos2dx is based on [spine-c](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-c).

## Licensing

This Spine Runtime may only be used for personal or internal use, typically to evaluate Spine before purchasing. If you would like to incorporate a Spine Runtime into your applications, distribute software containing a Spine Runtime, or modify a Spine Runtime, then you will need a valid [Spine license](https://esotericsoftware.com/spine-purchase). Please see the [Spine Runtimes Software License](https://github.com/EsotericSoftware/spine-runtimes/blob/master/LICENSE) for detailed information.

The Spine Runtimes are developed with the intent to be used with data exported from Spine. By purchasing Spine, `Section 2` of the [Spine Software License](https://esotericsoftware.com/files/license.txt) grants the right to create and distribute derivative works of the Spine Runtimes.

## Spine version

spine-cocos2dx v3 works with data exported from Spine 3.1.08. Updating spine-cocos2dx v3 to [v3.2](https://github.com/EsotericSoftware/spine-runtimes/issues/586) and [v3.3](https://github.com/EsotericSoftware/spine-runtimes/issues/613) is in progress.

spine-cocos2dx v3 supports all Spine features.

spine-cocos2dx v3 does not yet support loading the binary format.

## Setup

The setup for cocos2d-x differs from most other Spine Runtimes because the cocos2d-x distribution includes a copy of the Spine Runtime files. This is not ideal because these files may be old and fail to work with the latest Spine editor. Also it means if cocos2d-x is updated, you may get newer Spine Runtime files which can break your application if you are not using the latest Spine editor. For these reasons, we have requested cocos2d-x to cease distributing the Spine Runtime files, but they  continue to do so.

To replace the Spine Runtime files distributed with cocos2d-x, please follow these directions:

1. Download a [cocos2d-x](http://www.cocos2d-x.org/download) version 3.x distribution or get the latest from [GitHub](https://github.com/cocos2d/cocos2d-x) or [as a zip](https://github.com/cocos2d/cocos2d-x/archive/v3.zip).
1. Run the `cocos2dx/download-deps.py` Python script to download dependencies required by cocos2d-x.
1. Delete the `.c`, `.cpp`, and `.h` files in `cocos2dx/cocos/editor-support/spine`. These are the Spine Runtime files that cocos2d-x distributes.
1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
1. Copy `spine-c/src/spine`, `spine-c/include/spine`, and `spine-cocos2dx/3/src/spine` to `spine-cocos2dx/3/cocos2dx/cocos/editor-support/spine`. If any files were added or removed, you may need to edit the appropriate cocos2d-x project files.

Alternatively, you may delete the Spine Runtime files out of cocos2d-x and directly use the contents of the `spine-c/src`, `spine-c/include`, and `spine-cocos2dx/3/src` directories in your projects. Be sure your header search path will find the contents of these directories. Note that the includes use `spine/Xxx.h`, so the `spine` directory cannot be omitted when copying the files.

To run the cocos2d-x Spine examples on Windows:

1. Place cocos2d-x into `spine-cocos2dx/3/cocos2dx` and open `spine-cocos2dx/3/example/proj.win32/spine-cocos2dx.sln`. Alternatively, add `spine-cocos2dx/3/example/proj.win32/spine-cocos2dx.vcxproj` to your Visual Studio solution which contains cocos2d-x.
1. Copy the contents of `spine-cocos2dx/3/example/Resources` to `spine-cocos2dx/3/example/proj.win32/Debug.win32`. This step is required because cocos2d-x insists on looking for resources in the directory containing the executable.
1. Run the `spine-cocos2dx` project. Click to show debug bones, again for slow motion, and again to change scenes.

## Notes

- Images are premultiplied by cocos2d-x, so the Spine atlas images should *not* use premultiplied alpha.

## Examples

- [Raptor](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-cocos2dx/3/example/Classes/RaptorExample.cpp)
- [Spineboy](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-cocos2dx/3/example/Classes/SpineboyExample.cpp)
- [Golbins](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-cocos2dx/3/example/Classes/GoblinsExample.cpp)
