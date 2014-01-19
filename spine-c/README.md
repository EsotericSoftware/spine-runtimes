# `spine-c`

The `spine-c` runtime provides basic functionality to load and manipulate [Spine](http://esotericsoftware.com) skeletal animation data using ANSI C. It does not perform rendering but can can be extended to enable Spine animations for any C-based language, such as C++ or Objective-C.

## Setup

Project files are provided for Visual C++ Express 2010.

If `SPINE_SHORT_NAMES` is defined, the `sp` prefix for all structs and functions is optional.

## Examples

[Loading data](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-c/example/main.c)

## Extension

Extending `spine-c` requires implementing three methods:

- **`_spAtlasPage_createTexture`** Loads a texture and stores it in the `void* rendererObject` field of an `spAtlasPage` struct.
- **`_spAtlasPage_disposeTexture`** Disposes of a texture loaded with `_spAtlasPage_createTexture`.
- **`_spUtil_readFile`** Reads a file. If this doesn't need to be customized, `_readFile` is provided which reads a file using `fopen`.

This allows the `spine-c` API to be used to load Spine animation data. Rendering is done by iterating the slots of a skeleton and rendering the attachment for each slot. [`spine-sfml`](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-sfml/src/spine/spine-sfml.cpp#L39) serves as a simple example of extending `spine-c`.

`spine-c` uses an OOP style of programming where each "class" is made up of a struct and a number of functions prefixed with the struct name. More detals about how this works are available in [extension.h](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-c/include/spine/extension.h#L2). This mechanism allows you to provide your own implementations for [spAttachmentLoader](http://esotericsoftware.com/spine-using-runtimes/#attachmentloader), `spAttachment` and `spTimeline`, if necessary.

## Runtimes Extending `spine-c`

- [`spine-cocos2d-iphone`](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-cocos2d-iphone)
- [`spine-cocos2dx`](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-cocos2dx)
- [`spine-sfml`](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-sfml)
- [`spine-torque2d`](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-torque2d)
