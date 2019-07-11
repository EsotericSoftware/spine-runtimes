# spine-as3

The spine-as3 runtime provides functionality to load and manipulate [Spine](http://esotericsoftware.com) skeletal animation data using Adobe's ActionScript 3.0 (AS3). The [`spine.flash` package](spine-as3/src/spine/flash) can be used to render Spine animations using Flash. spine-as3 can be extended to enable Spine animations for other AS3 projects, such as [Starling](../spine-starling).

## Licensing

You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](http://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](http://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-as3 works with data exported from Spine 3.8.xx.

spine-as3 supports all Spine features, including meshes. If using the `spine.flash` classes for rendering, meshes are not supported.

spine-as3 does not yet support loading the binary format.

## Usage
1. Create a new Flex or Adobe AIR project in your preferred IDE.
2. Download the Spine Runtimes source using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it as a ZIP by clicking the download button above.
3. Add the sources from `spine-as3/spine-as3/src/` to your project

## Example
The Spine AS3 example works on Windows, Linux and Mac OS X. This guide assumes you are using [FDT Free](http://fdt.powerflasher.com/) as your development environment.

1. Download [FDT free](http://fdt.powerflasher.com/buy-download/) for your operating system.
3. Download and install [Adobe Flash Player 23 with debugging support](https://www.adobe.com/support/flashplayer/debug_downloads.html#fp15)
2. Download the latest [Flex SDK](http://www.adobe.com/devnet/flex/flex-sdk-download.html). We assume it will be installed to some folder on your disk called `flex_sdk`.
3. Download the latest [Adobe AIR SDK](http://www.adobe.com/devnet/air/air-sdk-download.html)
4. Extract the AIR SDK contents, and copy them to your `flex_sdk` folder. This will replace the Adobe AIR version shipped with Flex.
5. Open FDT, go to `Preferences -> FDT -> Installed SDKs`
6. Click `Add` and browse to `flex_sdk`
7. Go to `File -> Import -> General -> Existing Projects into Workspace`
6. Browse to `spine-as3/`. You should see both the `spine-as3` and `spine-as3-example` project in the import dialog. Click `Finish`
8. Right click the `Main.as` file in `spine-as3-example/src/spine` in the FDT explorer and select `Debug As -> FDT SWF Application`

**Note**: FDT Free does not allow project dependencies. If you modify the sources of `spine-as3`, you will have to compile the project to an `.swc` and place it in `spine-as3-example/libs`.

## Demos

* [Flash Demo](http://esotericsoftware.com/files/runtimes/spine-as3/spineboy/index.html)
  [Flash Demo source](spine-as3-example/src/spine/Main.as#L43)

## Notes

- Atlas images should not use premultiplied alpha.
