# spine-solar2d

The spine-solar2d runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [Solar2D](https://solar2d.com/) (previously Corona). spine-solar2d is based on [spine-lua](../spine-lua).

## Licensing

You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](http://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](http://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-solar2d works with data exported from Spine 3.9.xx.

spine-solar2d supports all Spine features.

spine-solar2d does not yet support loading the binary format.

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it as a zip via the download button above.
2. Copy the `spine-lua/spine-lua` folder to `spine-solar2d`.
3. Run the `main.lua` file using Solar2D. Tap/click to switch between skeletons

Alternatively, the `spine-lua` and `spine-solar2d/spine-solar2d` directories can be copied into your project. Note that the spine-solar2d `require` statements use `spine-lua.Xxx`, so the spine-lua files must be in a `spine-lua` directory in your project.

When using the `EmmyLua` plugin for IntelliJ IDEA, create a launch configuration pointing at the `Corona Simulator` executable (e.g. ` /Applications/Corona/Corona Simulator.app/Contents/MacOS/Corona Simulator` on macOS), set the working directory to `spine-solar2d`, and set the parameters to `main.lua`.
