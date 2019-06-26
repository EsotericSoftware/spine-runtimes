# spine-lua

The spine-lua runtime provides functionality to load and manipulate [Spine](http://esotericsoftware.com) skeletal animation data using [Lua](http://www.lua.org/). It does not perform rendering but can be extended to enable Spine animations for other Lua-based projects.

## Licensing

You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](http://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](http://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-lua works with data exported from Spine 3.8.xx.

spine-lua supports all Spine features.

spine-lua does not yet support loading the binary format.

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it as a zip via the download button above.
1. Copy the contents of the `spine-lua` directory into your project.

## Runtimes Extending spine-lua

- [spine-corona](../spine-corona)
- [spine-love](../spine-love)
