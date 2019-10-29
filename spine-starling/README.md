# spine-starling

The spine-starling runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [Starling 2.0](http://gamua.com/starling/). spine-starling is based on [spine-as3](../spine-as3).

## Licensing

You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](http://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](http://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-starling works with data exported from Spine 3.8.xx.

spine-starling supports all Spine features.

# Usage
1. Create a new Starling 2.0 project as per the [documentation].
2. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it as a zip via the download button above.
3. Copy the sources in `spine-as3/spine-as3/src/` and `spine-starling/spine-starling/src/` into your project's source directory

## Example
The Spine Starling example works on Windows, Linux and Mac OS X. This guide assumes you are using [Visual Studio Code](https://code.visualstudio.com/) together with the [ActionScript & MXML extension for Visual Studio Code](https://github.com/BowlerHatLLC/vscode-as3mxml/wiki) as your development environment.

1. Install [Visual Studio Code](https://code.visualstudio.com/).
2. Install the [ActionScript & MXML extension for Visual Studio Code](https://github.com/BowlerHatLLC/vscode-as3mxml/wiki).
3. Install [Adobe Flash Player Projector version 32 with debugging support](https://www.adobe.com/support/flashplayer/debug_downloads.html#fp15).
4. Install the [Adobe AIR SDK 32](http://www.adobe.com/devnet/air/air-sdk-download.html) by simply extracting it to a known location.

To run the Flash example project `spine-starling-example`.

1. Open the `spine-starling-example/` folder in Visual Studio Code.
2. Set the AIR SDK location when prompted.
3. Launch the `Launch Spine Starling Example` launch configuration.

Instead of directly adding the sources of from `spine-starling/src` to your project, you can also link the SWC file `spine-starling/lib/spine-starling.swc`. To (re-)compile this file yourself with Visual Studio Code:

1. Open the `spine-starling/` folder in Visual Studio Code.
2. Press `CTRL + SHIFT + B` (`CMD + SHIFT + B` on macOS) and select `ActionScript: compile release - asconfig.json`

Note that `spine-starling` depends on the sources of the `spine-as3` project. See the `asconfig.json` file more information on dependencies.