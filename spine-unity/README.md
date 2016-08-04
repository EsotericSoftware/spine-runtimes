# spine-unity

The **spine-unity** runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [Unity](http://unity3d.com/). spine-unity is based on [spine-csharp](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-csharp).

For more documentation, see [spine-unity Documentation](http://esotericsoftware.com/spine-unity).

While spine-unity can render directly with Unity, without the need for any other plugins, it also works with [2D Toolkit](http://www.2dtoolkit.com/) and can render skeletons using a TK2D texture atlas.

## Licensing

This Spine Runtime may only be used for personal or internal use, typically to evaluate Spine before purchasing. If you would like to incorporate a Spine Runtime into your applications, distribute software containing a Spine Runtime, or modify a Spine Runtime, then you will need a valid [Spine license](https://esotericsoftware.com/spine-purchase). Please see the [Spine Runtimes Software License](https://github.com/EsotericSoftware/spine-runtimes/blob/master/LICENSE) for detailed information.

The Spine Runtimes are developed with the intent to be used with data exported from Spine. By purchasing Spine, `Section 2` of the [Spine Software License](https://esotericsoftware.com/files/license.txt) grants the right to create and distribute derivative works of the Spine Runtimes.

## Spine version

spine-unity works with data exported from the latest version of Spine.

spine-unity supports all Spine features.

## Documentation

A Spine skeleton GameObject (a GameObject with a SkeletonAnimation component on it) can be used throughout Unity like any other GameObject. It renders through `MeshRenderer`.

`SkeletonUtility` allows other GameObjects to interact with the Spine skeleton, to control bones in the skeleton, be controlled by the skeleton, attach colliders, etc.

For advanced uses and specific optimization cases, Spine skeletons can be "baked" into native Unity animation assets. Since Unity's animation feature-set does not overlap with Spine's perfectly, baked assets have many limitations and removed features. For most uses, baking is not necessary.

The [Spine Unity Features Tutorial](http://esotericsoftware.com/forum/Unity-Feature-Tutorials-4839) forum thread has many videos on how to use spine-unity.

For more documentation, see [spine-unity Documentation](http://esotericsoftware.com/spine-unity).

## Quick installation

Download the latest Spine-Unity unitypackage from the download page: http://esotericsoftware.com/spine-unity-download/

In the `Assets/Examples/Scenes` folder you will find many example scenes that demonstrate various spine-unity features.

> Note: If you are still using Spine 2.1.xx, you'll need to use the older 2.1.xx compatible runtime. You can find it here: [spine-unity-v2.unitypackage](http://esotericsoftware.com/files/runtimes/unity/spine-unity-v2.unitypackage)

## Manual installation

You can also choose to setup and run from the Git files:

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
2. spine-unity requires both `spine-csharp` and `spine-unity`.
	- Copy the contents of `spine-csharp/src` to `Assets/spine-csharp` in your Unity project directory.
	- Copy the contents of `spine-unity/Assets` to `Assets` in your Unity project directory. Including `Gizmos` and `spine-unity` and `Examples` if you want them.

> - `Gizmos` is a [special folder](http://docs.unity3d.com/Manual/SpecialFolders.html) in Unity. It needs to be at the root of your assets folder to function correctly. (ie. `Assets/Gizmos`
- `spine-csharp` and `spine-unity` can be placed in any subfolder you want.

For more information on importing the runtime into your Unity project, see the documentation sections on  [installing](http://esotericsoftware.com/spine-unity#Installing) and [updating](http://esotericsoftware.com/spine-unity#Updating-Your-Projects-SpineUnity-Runtime),

----------

> More resources:
- [Spine-Unity Documentation](http://esotericsoftware.com/spine-unity)
- [Importing Spine Skeletons into Unity](http://esotericsoftware.com/spine-unity#Importing-into-Unity)

----------

## Notes

- This slightly outdated [spine-unity tutorial video](http://www.youtube.com/watch?v=x1umSQulghA) may still be useful.
- Atlas images should use **Premultiplied Alpha** when using the shaders that come with spine-unity (`Spine/Skeleton` or `Spine/SkeletonLit`).
- Texture sizes: Unity scales large images down by default if they exceed 1024x1024. This can cause atlas coordinates to be incorrect. To fix this, make sure to set import settings in the Inspector for any large atlas image you have so Unity does not scale it down.
- Texture artifacts from compression: Unity's 2D project defaults import new images added to the project with the Texture Type "Sprite". This can cause artifacts when using the `Spine/Skeleton` shader. To avoid these artifacts, make sure the Texture Type is set to "Texture". spine-unity's automatic import will attempt to apply these settings but in the process of updating your textures, these settings may be reverted.
