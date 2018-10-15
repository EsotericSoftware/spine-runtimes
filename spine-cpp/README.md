# spine-cpp

The spine-cpp runtime provides basic functionality to load and manipulate [spine](http://esotericsoftware.com) skeletal animation data using C++. It does not perform rendering but can be extended to enable spine animations for other projects that utilize C++. Note, this library uses C++03 for maximum portability and therefore does not take advantage of any C++11 or newer features such as std::unique_ptr.

## Licensing

This spine Runtime may only be used for personal or internal use, typically to evaluate spine before purchasing. If you would like to incorporate a spine Runtime into your applications, distribute software containing a spine Runtime, or modify a spine Runtime, then you will need a valid [spine license](https://esotericsoftware.com/spine-purchase). Please see the [spine Runtimes Software License](http://esotericsoftware.com/git/spine-runtimes/blob/LICENSE) for detailed information.

The spine Runtimes are developed with the intent to be used with data exported from spine. By purchasing spine, `Section 2` of the [spine Software License](https://esotericsoftware.com/files/license.txt) grants the right to create and distribute derivative works of the spine Runtimes.

## spine version

spine-cpp works with data exported from spine 3.7.xx.

spine-cpp supports all spine features.

## Setup

1. Download the spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it as a zip via the download button above.
2. Copy the contents of the `spine-cpp/spine-cpp/src` and `spine-cpp/spine-cpp/include` directories into your project. Be sure your header search is configured to find the contents of the `spine-cpp/spine-cpp/include` directory. Note that the includes use `spine/Xxx.h`, so the `spine` directory cannot be omitted when copying the files.

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
