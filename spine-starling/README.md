# spine-starling

The spine-starling runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [Starling 2.0](http://gamua.com/starling/). spine-starling is based on [spine-as3](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-as3).

## Licensing

This Spine Runtime may only be used for personal or internal use, typically to evaluate Spine before purchasing. If you would like to incorporate a Spine Runtime into your applications, distribute software containing a Spine Runtime, or modify a Spine Runtime, then you will need a valid [Spine license](https://esotericsoftware.com/spine-purchase). Please see the [Spine Runtimes Software License](https://github.com/EsotericSoftware/spine-runtimes/blob/master/LICENSE) for detailed information.

The Spine Runtimes are developed with the intent to be used with data exported from Spine. By purchasing Spine, `Section 2` of the [Spine Software License](https://esotericsoftware.com/files/license.txt) grants the right to create and distribute derivative works of the Spine Runtimes.

## Spine version

spine-starling works with data exported from Spine 3.6.xx.

spine-starling supports all Spine features.

spine-starling does not yet support loading the binary format.

# Usage
1. Create a new Starling 2.0 project as per the [documentation].
2. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/3.6.zip).
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

[Spine atlas example](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-starling/spine-starling-example/src/AtlasExample.as#L21)
[Starling atlas example](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-starling/spine-starling-example/src/StarlingAtlasExample.as#L18)
[Skin example](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-starling/spine-starling-example/src/GoblinsExample.as#L21)
