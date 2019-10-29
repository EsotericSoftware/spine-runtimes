# spine-as3

The spine-as3 runtime provides functionality to load and manipulate [Spine](http://esotericsoftware.com) skeletal animation data using Adobe's ActionScript 3.0 (AS3). The [`spine.flash` package](spine-as3/src/spine/flash) can be used to render Spine animations using Flash. spine-as3 can be extended to enable Spine animations for other AS3 projects, such as [Starling](../spine-starling).

## Licensing

You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](http://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](http://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-as3 works with data exported from Spine 3.8.xx.

spine-as3 supports all Spine features, including meshes. If using the `spine.flash` classes for rendering, meshes and two color tinting are not supported.

## Usage
1. Create a new Flex or Adobe AIR project in your preferred IDE.
2. Download the Spine Runtimes source using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it as a ZIP by clicking the download button above.
3. Add the sources from `spine-as3/spine-as3/src/` to your project

## Example
The Spine AS3 example works on Windows, Linux and Mac OS X. This guide assumes you are using [Visual Studio Code](https://code.visualstudio.com/) together with the [ActionScript & MXML extension for Visual Studio Code](https://github.com/BowlerHatLLC/vscode-as3mxml/wiki) as your development environment.

1. Install [Visual Studio Code](https://code.visualstudio.com/).
2. Install the [ActionScript & MXML extension for Visual Studio Code](https://github.com/BowlerHatLLC/vscode-as3mxml/wiki).
3. Install [Adobe Flash Player Projector version 32 with debugging support](https://www.adobe.com/support/flashplayer/debug_downloads.html#fp15).
4. Install the [Adobe AIR SDK 32](http://www.adobe.com/devnet/air/air-sdk-download.html) by simply extracting it to a known location.

To run the Flash example project `spine-as3-example`.

1. Open the `spine-as3-example/` folder in Visual Studio Code.
2. Set the AIR SDK location when prompted.
3. Launch the `Launch Spine AS3 Example` launch configuration.

Instead of directly adding the sources of from `spine-as3/src` to your project, you can also link the SWC file `spine-as3/lib/spine-as3.swc`. To (re-)compile this file yourself with Visual Studio Code:

1. Open the `spine-as3/` folder in Visual Studio Code.
2. Press `CTRL + SHIFT + B` (`CMD + SHIFT + B` on macOS) and select `ActionScript: compile release - asconfig.json`

## Notes

- Atlas images should not use premultiplied alpha.
