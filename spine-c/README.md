# spine-c

The spine-c runtime provides basic functionality to load and manipulate [Spine](http://esotericsoftware.com) skeletal animation data using ANSI C. It does not perform rendering but can be extended to enable Spine animations for other C-based projects, including C++ or Objective-C projects.

For a pure C++ API, you may consider the third party Spine runtime [Chobolabs/spine-cpp](https://github.com/Chobolabs/spine-cpp).

## Licensing

This Spine Runtime may only be used for personal or internal use, typically to evaluate Spine before purchasing. If you would like to incorporate a Spine Runtime into your applications, distribute software containing a Spine Runtime, or modify a Spine Runtime, then you will need a valid [Spine license](https://esotericsoftware.com/spine-purchase). Please see the [Spine Runtimes Software License](https://github.com/EsotericSoftware/spine-runtimes/blob/master/LICENSE) for detailed information.

The Spine Runtimes are developed with the intent to be used with data exported from Spine. By purchasing Spine, `Section 2` of the [Spine Software License](https://esotericsoftware.com/files/license.txt) grants the right to create and distribute derivative works of the Spine Runtimes.

## Spine version

spine-c works with data exported from Spine 3.4.02.

spine-c supports all Spine features.

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
1. Open the `spine-c.sln` Visual C++ 2010 Express project file. For other IDEs, you will need to create a new project and import the source.

Alternatively, the contents of the `spine-c/src` and `spine-c/include` directories can be copied into your project. Be sure your header search is configured to find the contents of the `spine-c/include` directory. Note that the includes use `spine/Xxx.h`, so the `spine` directory cannot be omitted when copying the files.

If `SPINE_SHORT_NAMES` is defined, the `sp` prefix for all structs and functions is optional. Only use this if the spine-c names won't cause a conflict.

## Extension

Extending spine-c requires implementing three methods:

- `_spAtlasPage_createTexture` Loads a texture and stores it and its size in the `void* rendererObject`, `width` and `height` fields of an `spAtlasPage` struct.
- `_spAtlasPage_disposeTexture` Disposes of a texture loaded with `_spAtlasPage_createTexture`.
- `_spUtil_readFile` Reads a file. If this doesn't need to be customized, `_readFile` is provided which reads a file using `fopen`.

With these implemented, the spine-c API can then be used to load Spine animation data. Rendering is done by enumerating the slots for a skeleton and rendering the attachment for each slot. Each attachment has a `rendererObject` field that is set when the attachment is loaded.

For example, `AtlasAttachmentLoader` is typically used to load attachments when using a Spine texture atlas. When `AtlasAttachmentLoader` loads a `RegionAttachment`, the attachment's `void* rendererObject` is set to an `AtlasRegion`. Rendering code can then obtain the `AtlasRegion` from the attachment, get the `AtlasPage` it belongs to, and get the page's `void* rendererObject`. This is the renderer specific texture object set by `_spAtlasPage_createTexture`. Attachment loading can be [customized](http://esotericsoftware.com/spine-using-runtimes/#attachmentloader) if not using `AtlasAttachmentLoader` or to provider different renderer specific data.

[spine-sfml](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-sfml/src/spine/spine-sfml.cpp#L39) serves as a simple example of extending spine-c.

spine-c uses an OOP style of programming where each "class" is made up of a struct and a number of functions prefixed with the struct name. More detals about how this works are available in [extension.h](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-c/include/spine/extension.h#L2). This mechanism allows you to provide your own implementations for `spAttachmentLoader`, `spAttachment` and `spTimeline`, if necessary.

## Runtimes extending spine-c

- [spine-cocos2d-iphone](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-cocos2d-iphone)
- [spine-cocos2dx](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-cocos2dx)
- [spine-sfml](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-sfml)
