# spine-love

The spine-love runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [LÖVE](https://love2d.org/). spine-love is based on [spine-lua](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-lua).

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
1. Copy the contents of `spine-lua` to `spine-love/spine-lua`.
1. Run the `main.lua` file using LÖVE.

Alternatively, the `spine-lua` and `spine-love/spine-love` directories can be copied into your project. Note that the require statements use `spine-lua.Xxx`, so the spine-lua files must be in a `spine-lua` directory in your project.

## Examples

[Simple Example](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-love/main.lua)
