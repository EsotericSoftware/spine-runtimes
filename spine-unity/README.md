# spine-unity

The **Spine-Unity** runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [Unity](http://unity3d.com/). spine-unity is based on [spine-csharp](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-csharp).

For more documentation, see [Spine-Unity Documentation](https://github.com/pharan/spine-unity-docs/blob/master/README.md).

While spine-unity can render directly with Unity, without the need for any other plugins, it also works with [2D Toolkit](http://www.unikronsoftware.com/2dtoolkit/) and can render skeletons using a TK2D texture atlas.

## Licensing

This Spine Runtime may only be used for personal or internal use, typically to evaluate Spine before purchasing. If you would like to incorporate a Spine Runtime into your applications, distribute software containing a Spine Runtime, or modify a Spine Runtime, then you will need a valid [Spine license](https://esotericsoftware.com/spine-purchase). Please see the [Spine Runtimes Software License](https://github.com/EsotericSoftware/spine-runtimes/blob/master/LICENSE) for detailed information.

The Spine Runtimes are developed with the intent to be used with data exported from Spine. By purchasing Spine, `Section 2` of the [Spine Software License](https://esotericsoftware.com/files/license.txt) grants the right to create and distribute derivative works of the Spine Runtimes.

## Documentation

A Spine skeleton GameObject (a GameObject with a SkeletonAnimation component on it) can be used throughout Unity like any other GameObject. It renders through `MeshRenderer`.

`SkeletonUtility` allows other GameObjects to interact with the Spine skeleton, to control bones in the skeleton, be controlled by the skeleton, attach colliders, etc.

For advanced uses and specific optimization cases, Spine skeletons can be "baked" into native Unity animation assets. Since Unity's animation feature-set does not overlap with Spine's perfectly, baked assets have many limitations and removed features. For most uses, baking is not necessary.

The [Spine Unity Features Tutorial](http://esotericsoftware.com/forum/Unity-Feature-Tutorials-4839) forum thread has many videos on how to use spine-unity.

For more documentation, see [Spine-Unity Documentation](https://github.com/pharan/spine-unity-docs/blob/master/README.md).

## Quick installation

Download and run this Unity package:

[spine-unity.unitypackage](http://esotericsoftware.com/files/runtimes/unity/spine-unity.unitypackage)

In the `Assets/Examples/Scenes` folder you will find many example scenes that demonstrate various spine-unity features.

> Note: If you are still using Spine 2.1.xx, you'll need to use the older 2.1.xx compatible runtime. You can find it here: [spine-unity-v2.unitypackage](http://esotericsoftware.com/files/runtimes/unity/spine-unity-v2.unitypackage)

## Manual installation

You can also choose to setup and run from the Git files:

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
2. Spine-Unity requires both `spine-csharp` and `spine-unity`.
	- Copy the contents of `spine-csharp/src` to `Assets/spine-csharp` in your Unity project directory.
	- Copy the contents of `spine-unity/Assets/` to `Assets/` in your Unity project directory. Including `Gizmos` and `spine-unity` and `Examples` if you want them.


> - `Gizmos` is a [special folder](http://docs.unity3d.com/Manual/SpecialFolders.html) in Unity. It needs to be at the root of your assets folder to function correctly. (ie. `Assets/Gizmos`
- `spine-csharp` and `spine-unity` can be placed in any subfolder you want.

----------

## Importing skeleton data

1. Add your `.json`, `.atlas.txt` and `.png` into your Unity project.
	- You can do this through Unity's Project View: Drag and drop a folder containing the `.json`, `.atlas.txt` and `.png` files exported from Spine directly into the Unity Project view.
	- ... or you can opt to do this through Windows File Explorer or OSX Finder. Move or copy your `.json`, `.atlas.txt` and `.png` files into your Unity project's `Assets` folder, ideally in its own subfolder.
2. Spine-Unity will automatically detect the `.json` and `.atlas.txt` and attempt to generate the necessary Spine-Unity assets.
3. To start using your Spine assets, right-click on the SkeletonDataAsset (the asset with the orange Spine logo on it) and choose `Spine > Instantiate(SkeletonAnimation)`. This will add a GameObject with a `SkeletonAnimation` component on it.
	-  If you are more familiar with Mecanim, you may choose `Spine > Instantiate(Mecanim)` instead.
4. For more info on how to control the animation, see the [Spine-Unity Animation Control documentation](https://github.com/pharan/spine-unity-docs/blob/master/Animation.md).



> The original [manual setup video](https://www.youtube.com/watch?v=-V84OIvZdQc) to shows which assets belong where and what Spine-Unity's automatic import actually does for you under the hood. In case you have a specialized asset setup, this video will be useful for understanding how assets fit together.

> More resources:
[Drag and drop video](http://www.youtube.com/watch?v=-Gk_zJsY1Ms)
[readme PDF](https://raw.githubusercontent.com/EsotericSoftware/spine-runtimes/master/spine-unity/README.pdf)

----------

## Notes

- This slightly outdated [spine-unity tutorial video](http://www.youtube.com/watch?v=x1umSQulghA) may still be useful.
- Atlas images should use **Premultiplied Alpha** when using the shaders that come with spine-unity (`Spine/Skeleton` or `Spine/SkeletonLit`).
- **TEXTURE SIZES.** Unity scales large images down by default if they exceed 1024x1024. This can cause atlas coordinates to be incorrect. To fix this, make sure to set import settings in the Inspector for any large atlas image you have so Unity does not scale it down.
- **TEXTURE ARTIFACTS FROM COMPRESSION.** Unity's 2D project defaults import new images added to the project with the Texture Type "Sprite". This can cause artifacts when using the `Spine/Skeleton` shader. To avoid these artifacts, make sure the Texture Type is set to "Texture". Spine-Unity's automatic import will attempt to apply these settings but in the process of updating your textures, these settings may be reverted.
