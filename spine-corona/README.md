# spine-corona

The spine-corona runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [Corona](http://coronalabs.com/products/corona-sdk/). spine-corona is based on [spine-lua](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-lua).

## Licensing

This Spine Runtime may only be used for personal or internal use, typically to evaluate Spine before purchasing. If you would like to incorporate a Spine Runtime into your applications, distribute software containing a Spine Runtime, or modify a Spine Runtime, then you will need a valid [Spine license](https://esotericsoftware.com/spine-purchase). Please see the [Spine Runtimes Software License](https://github.com/EsotericSoftware/spine-runtimes/blob/master/LICENSE) for detailed information.

The Spine Runtimes are developed with the intent to be used with data exported from Spine. By purchasing Spine, `Section 2` of the [Spine Software License](https://esotericsoftware.com/files/license.txt) grants the right to create and distribute derivative works of the Spine Runtimes.

## Spine version

spine-corona works with data exported from Spine 2.1.27. Updating spine-corona to [v3.0](https://trello.com/c/tF8UykBM/72-update-runtimes-to-support-v3-0-skewing-scale), [v3.1](https://trello.com/c/bERJAFEq/73-update-runtimes-to-support-v3-1-linked-meshes), [v3.2](https://github.com/EsotericSoftware/spine-runtimes/issues/586), and [v3.3](https://github.com/EsotericSoftware/spine-runtimes/issues/613) is in progress.

spine-corona supports all Spine features except for rendering meshes due to Corona having a limited graphics API.

spine-corona does not yet support loading the binary format.

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
1. Copy the contents of `spine-lua` to `spine-corona/spine-lua`.
1. Run the `main.lua` file using Corona. There are multiple examples that can be enabled by editing this file.

Alternatively, the `spine-lua` and `spine-corona/spine-corona` directories can be copied into your project. Note that the require statements use `spine-lua.Xxx`, so the spine-lua files must be in a `spine-lua` directory in your project.

## Examples

[spineboy Example](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-corona/examples/spineboy.lua)
[goblins Example](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-corona/examples/goblins.lua)
[dragon Example](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-corona/examples/dragon.lua)
