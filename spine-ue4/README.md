# spine-ue4
The spine-ue4 runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [Unreal Engine 4](https://www.unrealengine.com/). spine-ue4 is based on [spine-c](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-c).

## Licensing

This Spine Runtime may only be used for personal or internal use, typically to evaluate Spine before purchasing. If you would like to incorporate a Spine Runtime into your applications, distribute software containing a Spine Runtime, or modify a Spine Runtime, then you will need a valid [Spine license](https://esotericsoftware.com/spine-purchase). Please see the [Spine Runtimes Software License](https://github.com/EsotericSoftware/spine-runtimes/blob/master/LICENSE) for detailed information.

The Spine Runtimes are developed with the intent to be used with data exported from Spine. By purchasing Spine, `Section 2` of the [Spine Software License](https://esotericsoftware.com/files/license.txt) grants the right to create and distribute derivative works of the Spine Runtimes.

## Spine version

spine-ue4 works with data exported from Spine 3.4.02.

spine-ue4 supports all Spine features.

## Usage
1. Create a new Unreal Engine project. See the [Unreal Engine documentation](https://docs.unrealengine.com/latest/INT/) or have a look at the example in this repository.
2. Download the Spine Runtimes source using git (`git clone https://github.com/esotericsoftware/spine-runtimes`) or download it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip)
3. Copy the `Plugins` folder from this directory to your new project's root directory.
4. Copy the `spine-c` folder from this repositories root directory to your project's `Plugins/SpinePlugin/Sources/SpinePlugin/Public/` directory.
5. Open the Unreal Project in the Unreal Editor

See the [Spine Runtimes documentation](http://esotericsoftware.com/spine-documentation#runtimesTitle) on how to use the APIs or check out the Spine UE4 example.

## Example
The Spine UE4 example works on all platforms supported by Unreal Engine.

1. Copy the `spine-c` folder from this repositories root directory to your `Plugins/SpinePlugin/Sources/SpinePlugin/Public/` directory.
2. Open the SpineUE4.uproject file with Unreal Editor
