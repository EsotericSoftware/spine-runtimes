# spine-unity

The spine-unity runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [Unity](http://unity3d.com/). spine-unity is based on [spine-csharp](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-csharp).

While spine-unity can render directly with Unity, without the need for any other plugins, it also works with [2D Toolkit](http://www.unikronsoftware.com/2dtoolkit/) and render skeletons from a TK2D texture atlas.

A Spine skeleton is a GameObject and can be used throughout Unity like any other GameObject. It depends only on Unity's `MeshRenderer`, so it is close to the metal. `SkeletonUtility` allows other GameObjects to interact with the Spine skeleton, to control bones in the skeleton, be controlled by the skeleton, attach colliders, etc.

## Documentation

The [Spine Unity Features Tutorial](http://esotericsoftware.com/forum/Unity-Feature-Tutorials-4839) forum thread has many videos on how to use spine-unity.

## Quick installation

Download and run this Unity package:

[spine-unity.unitypackage](http://esotericsoftware.com/files/runtimes/unity/spine-unity.unitypackage)

In the `Assets/Examples/Scenes` folder you will find many example scenes that demonstrate various spine-unity features.

## Manual installation

You can also choose to setup and run from the Git files:

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
1. Copy the contents of `spine-csharp/src` to `Assets/spine-csharp` in your Unity project directory.
1. Copy the `spine-unity/Assets/spine-unity` to `Assets/spine-unity` in your Unity project directory.

## Importing skeleton data

There are a few options for importing Spine skeletons into your Unity project:

### Drag and drop import

1. Drag and drop a folder containing the JSON, atlas and PNG files exported from Spine directly into Unity.
1. Drag the prefab that was created into your scene.

The [drag and drop video](http://www.youtube.com/watch?v=-Gk_zJsY1Ms) shows how this works.

### Automated import

1. Place the JSON, atlas and PNG files exported from Spine into your Unity project directory.
1. Right click the JSON file and click `Spine`, `Ingest` to create the Unity assets.
1. Right click the SkeletonData asset that was created and click `Spine`, `Spawn`.

The [readme PDF](https://raw.githubusercontent.com/EsotericSoftware/spine-runtimes/master/spine-unity/README.pdf) illustrates these steps.

### Manual import

1. Follow the [setup video](https://www.youtube.com/watch?v=-V84OIvZdQc) to manually set up the assets for a skeleton in Unity. This video may prove useful to understand how the pieces fit together but the other import methods are much easier.

## Examples

To run the examples:

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
1. Copy the contents of `spine-csharp/src` to `spine-unity/Assets/spine-csharp`.
1. Open an example scene file from `spine-unity/Assets/examples/` using Unity 4.3.4+.

* **spineboy** This shows the spineboy skeleton. First an animation is played that shows the draw order changing and events firing, then spineboy jumps and walks. Click spineboy to jump again. Notice the walk and jump animations are mixed and transition smoothly. The white cube on spineboy's right hand is a separate GameObject that is positioned using a `BoneComponent`. This example uses images that are split across two atlas pages. This demonstrates a multi-page atlas, but of course has a high number of draw calls.
* **goblins*** This shows a male and female goblin that use the same skeleton and animations. Click to change the skin from male to female and back. It has a single atlas page, so is drawn with just 1 draw call. It uses the `Skeleton Lit` shader for vertex lighting. The [Goblins.cs](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-unity/Assets/examples/goblins/Goblins.cs) script manipulates the head bone after the animation is applied.
* **dragon*** This shows the dragon skeleton. The flying animation has many image changes. It also shows shadow rendering. This example uses a multi-page atlas so has a high number of draw calls.

## Notes

- Atlas images should use premultiplied alpha when using the shaders that come with spine-unity.
- This slightly outdated [spine-unity tutorial video](http://www.youtube.com/watch?v=x1umSQulghA) may still be useful.
- Unity scales large images down by default if they exceed 1024x1024, which causes atlas coordinates to be incorrect. To fix this, override the import settings in the Inspector for any large atlas image you have so Unity does not scale it down.
- Unity 4.3+'s 2D project defaults cause atlas images added to the project to be imported with the Texture Type "Sprite", which may cause artifacts when using Spine's Skeleton shader. To avoid these artifacts, make sure the Texture Type is set to "Texture".
