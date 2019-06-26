# spine-cpp

The spine-cpp runtime provides basic functionality to load and manipulate [spine](http://esotericsoftware.com) skeletal animation data using C++. It does not perform rendering but can be extended to enable spine animations for other projects that utilize C++. Note, this library uses C++03 for maximum portability and therefore does not take advantage of any C++11 or newer features such as std::unique_ptr.

## Licensing

You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](http://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](http://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-cpp works with data exported from spine 3.8.xx.

spine-cpp supports all spine features.

## Setup

1. Download the spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it as a zip via the download button above.
2. Copy the contents of the `spine-cpp/spine-cpp/src` and `spine-cpp/spine-cpp/include` directories into your project. Be sure your header search is configured to find the contents of the `spine-cpp/spine-cpp/include` directory. Note that the includes use `spine/Xxx.h`, so the `spine` directory cannot be omitted when copying the files.

## Usage
### [Please see the spine-cpp guide for full documentation](http://esotericsoftware.com/spine-cpp)

## Extension

Extending spine-cpp requires implementing both the `SpineExtension` class and the TextureLoader class:

```
#include <spine/Extension.h>
void spine::SpineExtension *spine::getDefaultExtension() {
  return new spine::DefaultExtension();
}

class MyTextureLoader : public spine::TextureLoader
{
  virtual void load(spine::AtlasPage& page, const spine::String& path) {
    void* texture = ... load the texture based on path ...
    page->setRendererObject(texture); // use the texture later in your rendering code
  }

  virtual void unload(void* texture) { // TODO }
};
```

## Runtimes extending spine-cpp

- [spine-sfml](../spine-sfml/cpp)
- [spine-cocos2dx](../spine-cocos2dx)
- [spine-ue4](../spine-ue4)
