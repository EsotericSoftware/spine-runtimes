# spine-unity

The **spine-unity** runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [Unity](http://unity3d.com/). spine-unity is based on [spine-csharp](../spine-csharp).

## Licensing

You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](http://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](http://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-unity works with data exported from Spine 3.8.xx.

spine-unity supports all Spine features.

Unity's physics components do not support dynamically assigned vertices so they cannot be used to mirror bone-weighted and deformed BoundingBoxAttachments. However, BoundingBoxAttachment vertices at runtime will still deform correctly and can be used to perform manual hit detection.

## Unity version

spine-unity is compatible with Unity 2017.1-2019.3.

## Usage

### [Please see the spine-unity guide for full documentation](http://esotericsoftware.com/spine-unity).

1. Create an empty Unity project (or use an existing project).
2. Download and import the [`spine-unity.unitypackage`](http://esotericsoftware.com/spine-unity-download/).

See the [Spine Runtimes documentation](http://esotericsoftware.com/spine-documentation#runtimesTitle) on how to use the APIs and check out the spine-unity examples for demonstrations of Unity specific features.

## Example

### [Please see the spine-unity guide for full documentation](http://esotericsoftware.com/spine-unity).

1. Create an empty Unity project
2. Download and import the [`spine-unity.unitypackage`](http://esotericsoftware.com/spine-unity-download/).
3. Explore the example scenes found in the `Assets/Spine Examples/Scenes` folder.

## Notes

- This slightly outdated [spine-unity tutorial video](http://www.youtube.com/watch?v=x1umSQulghA) may still be useful.
- Atlas images should use **Premultiplied Alpha** when using the shaders that come with spine-unity (`Spine/Skeleton` or `Spine/SkeletonLit`).
- Texture artifacts from compression: Unity's 2D project defaults import new images added to the project with the Texture Type "Sprite". This can cause artifacts when using the `Spine/Skeleton` shader. To avoid these artifacts, make sure the Texture Type is set to "Texture". spine-unity's automatic import will attempt to apply these settings but in the process of updating your textures, these settings may be reverted.
