# spine-c

The spine-c runtime provides basic functionality to load and manipulate [Spine](http://esotericsoftware.com) skeletal animation data using ANSI C89. It does not perform rendering but can be extended to enable Spine animations for other C-based projects, including C++ or Objective-C projects.

## Licensing

You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](http://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](http://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-c works with data exported from Spine 3.8.xx.

spine-c supports all Spine features.

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it as a zip via the download button above.
1. Copy the contents of the `spine-c/spine-c/src` and `spine-c/spine-c/include` directories into your project. Be sure your header search is configured to find the contents of the `spine-c/spine-c/include` directory. Note that the includes use `spine/Xxx.h`, so the `spine` directory cannot be omitted when copying the files.

If `SPINE_SHORT_NAMES` is defined, the `sp` prefix for all structs and functions is optional. Only use this if the spine-c names won't cause a conflict.

## Usage
### [Please see the spine-c guide for full documentation](http://esotericsoftware.com/spine-c)

## Extension

Extending spine-c requires implementing three methods:

- `_spAtlasPage_createTexture` Loads a texture and stores it and its size in the `void* rendererObject`, `width` and `height` fields of an `spAtlasPage` struct.
- `_spAtlasPage_disposeTexture` Disposes of a texture loaded with `_spAtlasPage_createTexture`.
- `_spUtil_readFile` Reads a file. If this doesn't need to be customized, `_readFile` is provided which reads a file using `fopen`.

With these implemented, the spine-c API can then be used to load Spine animation data. Rendering is done by enumerating the slots for a skeleton and rendering the attachment for each slot. Each attachment has a `rendererObject` field that is set when the attachment is loaded.

For example, `AtlasAttachmentLoader` is typically used to load attachments when using a Spine texture atlas. When `AtlasAttachmentLoader` loads a `RegionAttachment`, the attachment's `void* rendererObject` is set to an `AtlasRegion`. Rendering code can then obtain the `AtlasRegion` from the attachment, get the `AtlasPage` it belongs to, and get the page's `void* rendererObject`. This is the renderer specific texture object set by `_spAtlasPage_createTexture`. Attachment loading can be [customized](http://esotericsoftware.com/spine-using-runtimes/#attachmentloader) if not using `AtlasAttachmentLoader` or to provider different renderer specific data.

[spine-sfml](../spine-sfml/src/c/spine/spine-sfml.cpp#L39) serves as a simple example of extending spine-c.

spine-c uses an OOP style of programming where each "class" is made up of a struct and a number of functions prefixed with the struct name. More detals about how this works are available in [extension.h](spine-c/include/spine/extension.h#L2). This mechanism allows you to provide your own implementations for `spAttachmentLoader`, `spAttachment` and `spTimeline`, if necessary.

## Runtimes extending spine-c

- [spine-cocos2d-objc](../spine-cocos2d-objc)
- [spine-sfml](../spine-sfml)
