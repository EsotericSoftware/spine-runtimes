# spine-love

The spine-love runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [LÖVE](https://love2d.org/). spine-love is based on [spine-lua](../spine-lua).

## Licensing
You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](http://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](http://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-love works with data exported from Spine 3.8.x.

spine-love supports all Spine features except for blending modes other than normal.

spine-love does not yet support loading the binary format.

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it as a zip via the download button above.
1. Copy the contents of `spine-lua` to `spine-love/spine-lua`.
1. Run the `main.lua` file using LÖVE.

Alternatively, the `spine-lua` and `spine-love/spine-love` directories can be copied into your project. Note that the require statements use `spine-lua.Xxx`, so the spine-lua files must be in a `spine-lua` directory in your project.

## Notes

 * To enable two color tinting, pass `true` to `SkeletonRenderer.new()`.

## Examples
If you want to run and debug the example project, use IntelliJ IDEA with the EmmyLua plugin.

1. Install IntelliJ IDEA and the EmmyLua plugin.
2. Install LÖVE.
3. Copy the contents of `spine-lua` to `spine-love/spine-lua`.
4. Open the `spine-love` folder in IntelliJ IDEA.
5. Create a new launch configuration of the type `Lua Application`, with the following settings
    1.  `Program` should point at the `love` or `love.exe` executable, e.g. `/Applications/love.app/Contents/MacOS/love` on macOS.
    2. `Working directory` should be the `spine-love/` directory.
    3. `Parameters` should be `./` so LÖVE loads the `main.lua` file from `spine-love/`.

[Simple Example](main.lua)
