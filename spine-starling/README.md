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

spine-starling does not yet support loading the binary format.

# Usage
1. Create a new Starling 2.0 project as per the [documentation].
2. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it as a zip via the download button above.
3. Copy the sources in `spine-as3/spine-as3/src/` and `spine-starling/spine-starling/src/` into your project's source directory

## Example
The Spine AS3 example works on Windows, Linux and Mac OS X. This guide assumes you are using [FDT Free](http://fdt.powerflasher.com/) as your development environment.

1. Download [FDT free](http://fdt.powerflasher.com/buy-download/) for your operating system.
3. Download and install Adobe Flash Player 22 with debugging support](https://www.adobe.com/support/flashplayer/debug_downloads.html#fp15)
2. Download the latest [Flex SDK](http://www.adobe.com/devnet/flex/flex-sdk-download.html). We assume it will be installed to some folder on your disk called `flex_sdk`.
3. Download the latest [Adobe AIR SDK](http://www.adobe.com/devnet/air/air-sdk-download.html)
4. Extract the AIR SDK contents, and copy them to your `flex_sdk` folder. This will replace the Adobe AIR version shipped with Flex.
5. Open FDT, go to `Preferences -> FDT -> Installed SDKs`
6. Click `Add` and browse to `flex_sdk`
7. Go to `File -> Import -> General -> Existing Projects into Workspace`
6. Browse to `spine-as3/`. You should see both the `spine-as3` and `spine-as3-example` project in the import dialog. Click `Finish`
7. Go to `File -> Import -> General -> Existing Projects into Workspace`
6. Browse to `spine-starling/`. You should see both the `spine-starling` and `spine-starling-example` project in the import dialog. Click `Finish`
8. Right click the `Main.as` file in `spine-starling-example/src/spine` in the FDT explorer and select `Debug As -> FDT SWF Application`

**Note**: FDT Free does not allow project dependencies. If you modify the sources of `spine-as3` or `spine-starling`, you will have to compile the project to an `.swc` and place it in `spine-starling-example/libs`.

## Examples

- [Spine atlas example](spine-starling-example/src/AtlasExample.as#L21)
- [Starling atlas example](spine-starling-example/src/StarlingAtlasExample.as#L18)
- [Skin example](spine-starling-example/src/GoblinsExample.as#L21)
