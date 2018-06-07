# spine-unity

The **spine-unity** runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [Unity](http://unity3d.com/). spine-unity is based on [spine-csharp](../spine-csharp).

For more documentation, see [spine-unity Documentation](http://esotericsoftware.com/spine-unity).

While spine-unity can render directly with Unity, without the need for any other plugins, it also works with [2D Toolkit](http://www.2dtoolkit.com/) and can render skeletons using a TK2D texture atlas.

## Licensing

This Spine Runtime may only be used for personal or internal use, typically to evaluate Spine before purchasing. If you would like to incorporate a Spine Runtime into your applications, distribute software containing a Spine Runtime, or modify a Spine Runtime, then you will need a valid [Spine license](https://esotericsoftware.com/spine-purchase). Please see the [Spine Runtimes Software License](http://esotericsoftware.com/git/spine-runtimes/blob/LICENSE) for detailed information.

The Spine Runtimes are developed with the intent to be used with data exported from Spine. By purchasing Spine, `Section 2` of the [Spine Software License](https://esotericsoftware.com/files/license.txt) grants the right to create and distribute derivative works of the Spine Runtimes.

## Spine version

spine-unity works with data exported from Spine 3.7.xx.

spine-unity supports all Spine features.

Unity's physics components do not support dynamically assigned vertices so they cannot be used to mirror bone-weighted and deformed BoundingBoxAttachments. However, BoundingBoxAttachment vertices at runtime will still deform correctly and can be used to perform manual hit detection.

## Documentation

A Spine skeleton GameObject (a GameObject with a SkeletonAnimation component on it) can be used throughout Unity like any other GameObject. It renders through `MeshRenderer`.

See [spine-unity Documentation](http://esotericsoftware.com/spine-unity).

## Quick installation

Download the latest Spine-Unity unitypackage from the download page: http://esotericsoftware.com/spine-unity-download/

In the `Assets/Spine Examples/Scenes` folder you will find many example scenes that demonstrate various spine-unity features.

----------

> More resources:
- [Spine-Unity Documentation](http://esotericsoftware.com/spine-unity)
- [Importing Spine Skeletons into Unity](http://esotericsoftware.com/spine-unity#Importing-into-Unity)

----------

## Notes

- This slightly outdated [spine-unity tutorial video](http://www.youtube.com/watch?v=x1umSQulghA) may still be useful.
- Atlas images should use **Premultiplied Alpha** when using the shaders that come with spine-unity (`Spine/Skeleton` or `Spine/SkeletonLit`).
- Texture artifacts from compression: Unity's 2D project defaults import new images added to the project with the Texture Type "Sprite". This can cause artifacts when using the `Spine/Skeleton` shader. To avoid these artifacts, make sure the Texture Type is set to "Texture". spine-unity's automatic import will attempt to apply these settings but in the process of updating your textures, these settings may be reverted.
