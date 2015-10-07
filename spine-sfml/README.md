# spine-sfml

The spine-sfml runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [SFML](http://www.sfml-dev.org/). spine-sfml is based on [spine-c](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-c).

## Licensing

This Spine Runtime may only be used for personal or internal use, typically to evaluate Spine before purchasing. If you would like to incorporate a Spine Runtime into your applications, distribute software containing a Spine Runtime, or modify a Spine Runtime, then you will need a valid [Spine license](https://esotericsoftware.com/spine-purchase). Please see the [Spine Runtimes Software License](https://github.com/EsotericSoftware/spine-runtimes/blob/master/LICENSE) for detailed information.

The Spine Runtimes are developed with the intent to be used with data exported from Spine. By purchasing Spine, `Section 2` of the [Spine Software License](https://esotericsoftware.com/files/license.txt) grants the right to create and distribute derivative works of the Spine Runtimes.

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
1. Using Eclipse CDT, import the project by choosing File -> Import -> Existing projects. For other IDEs you will need to create a new project and import the source.
1. Copy the SFML binaries into the `spine-sfml/Debug` directory so they can be found when the example is run.

Alternatively, the contents of the `spine-c/src`, `spine-c/include` and `spine-sfml/src` directories can be copied into your project. Be sure your header search path will find the contents of the `spine-c/include` and `spine-sfml/src` directories. Note that the includes use `spine/Xxx.h`, so the `spine` directory cannot be omitted when copying the files.

## Examples

- [Simple example](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-sfml/example/main.cpp#L61)

## Notes

- Atlas images should not use premultiplied alpha.
