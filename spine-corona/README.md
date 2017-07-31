# spine-corona

The spine-corona runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [Corona](http://coronalabs.com/products/corona-sdk/). spine-corona is based on [spine-lua](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-lua).

## Licensing

This Spine Runtime may only be used for personal or internal use, typically to evaluate Spine before purchasing. If you would like to incorporate a Spine Runtime into your applications, distribute software containing a Spine Runtime, or modify a Spine Runtime, then you will need a valid [Spine license](https://esotericsoftware.com/spine-purchase). Please see the [Spine Runtimes Software License](https://github.com/EsotericSoftware/spine-runtimes/blob/master/LICENSE) for detailed information.

The Spine Runtimes are developed with the intent to be used with data exported from Spine. By purchasing Spine, `Section 2` of the [Spine Software License](https://esotericsoftware.com/files/license.txt) grants the right to create and distribute derivative works of the Spine Runtimes.

## Spine version

spine-corona works with data exported from Spine 3.6.xx.

spine-corona supports all Spine features.

spine-corona does not yet support loading the binary format.

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/3.6.zip).
1. Copy the contents of `spine-lua` to `spine-corona/spine-lua`.
1. Run the `main.lua` file using Corona. Tap/click to switch between skeletons

Alternatively, the `spine-lua` and `spine-corona/spine-corona` directories can be copied into your project. Note that the require statements use `spine-lua.Xxx`, so the spine-lua files must be in a `spine-lua` directory in your project.
