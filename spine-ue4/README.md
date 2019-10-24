# spine-ue4
The spine-ue4 runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [Unreal Engine 4.21+](https://www.unrealengine.com/). spine-ue4 is based on [spine-cpp](../spine-cpp).

## Licensing

You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](http://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](http://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-ue4 works with data exported from Spine 3.8.xx.

spine-ue4 supports all Spine features.

spine-ue4 does not support multiply and screen blending. spine-ue4 does not support pre-multiplied alpha atlases. spine-ue4 does not support two color tinting.

## Usage
### [Please see the spine-ue4 guide for full documentation](http://esotericsoftware.com/spine-ue4)

1. Create a new Unreal Engine code project. You don't need to write C++, but the code project is needed for the plugin to compile. See the [Unreal Engine documentation](https://docs.unrealengine.com/latest/INT/) or have a look at the example in this repository.
2. Download the Spine Runtimes source using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it as a zip via the download button above.
3. Copy the `Plugins` folder from this directory to your new project's root directory.
4. Copy the folder `spine-runtimes/spine-cpp/spine-cpp` to your project's `Plugins/SpinePlugin/Source/SpinePlugin/Public/` folder.
5. Open the Unreal Project in the Unreal Editor

See the [Spine Runtimes documentation](http://esotericsoftware.com/spine-documentation#runtimesTitle) on how to use the APIs or check out the Spine UE4 example.

## Example
### [Please see the spine-ue4 guide for full documentation](http://esotericsoftware.com/spine-ue4)

The Spine UE4 example works on all platforms supported by Unreal Engine. The samples require Unreal Engine 4.23+.

1. Copy the `spine-cpp` folder from this repositories root directory to your `Plugins/SpinePlugin/Sources/SpinePlugin/Public/` directory.
2. Open the SpineUE4.uproject file with Unreal Editor
