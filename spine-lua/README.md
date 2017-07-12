# spine-lua

The spine-lua runtime provides functionality to load and manipulate [Spine](http://esotericsoftware.com) skeletal animation data using [Lua](http://www.lua.org/). It does not perform rendering but can be extended to enable Spine animations for other Lua-based projects.

## Licensing

This Spine Runtime may only be used for personal or internal use, typically to evaluate Spine before purchasing. If you would like to incorporate a Spine Runtime into your applications, distribute software containing a Spine Runtime, or modify a Spine Runtime, then you will need a valid [Spine license](https://esotericsoftware.com/spine-purchase). Please see the [Spine Runtimes Software License](https://github.com/EsotericSoftware/spine-runtimes/blob/master/LICENSE) for detailed information.

The Spine Runtimes are developed with the intent to be used with data exported from Spine. By purchasing Spine, `Section 2` of the [Spine Software License](https://esotericsoftware.com/files/license.txt) grants the right to create and distribute derivative works of the Spine Runtimes.

## Spine version

spine-lua works with data exported from Spine 3.6.xx.

spine-lua supports all Spine features.

spine-lua does not yet support loading the binary format.

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/3.6.zip).
1. Copy the contents of the `spine-lua` directory into your project.

## Runtimes Extending spine-lua

- [spine-corona](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-corona)
- [spine-love](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-love)
