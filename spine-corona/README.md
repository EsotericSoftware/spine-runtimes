# spine-corona

The spine-corona runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [Corona](http://coronalabs.com/products/corona-sdk/). spine-corona is based on [spine-lua](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-lua).

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
1. Copy the contents of `spine-lua` to `spine-corona/spine-lua`.
1. Run the `main.lua` file using Corona. There are multiple examples that can be enabled by editing this file.

Alternatively, the `spine-lua` and `spine-corona/spine-corona` directories can be copied into your project. Note that the require statements use `spine-lua.Xxx`, so the spine-lua files must be in a `spine-lua` directory in your project.

## Examples

[spineboy Example](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-corona/examples/spineboy.lua)
[goblins Example](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-corona/examples/goblins.lua)
[dragon Example](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-corona/examples/dragon.lua)
