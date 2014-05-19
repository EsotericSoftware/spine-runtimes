# spine-unity

The spine-unity runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [Unity](http://unity3d.com/) directly, without any other plugins. spine-unity is based on [spine-csharp](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-csharp) and is very similar to [spine-tk2d](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-tk2d).

A Spine skeleton is a GameObject and can be used throughout Unity like any other GameObject. The `BoneComponent` class allows other GameObjects to follow a bone in a Spine skeleton.

## Setup

To run the examples:

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
1. Copy the contents of `spine-csharp/src` to `spine-unity/Assets/spine-csharp`.
1. Open an example scene file from `spine-unity/Assets/examples/` using Unity 4.3.4+.

To use spine-unity in your own Unity project:

1. Copy the contents of `spine-csharp/src` to `Assets/spine-csharp` in your project.
1. Copy the `spine-unity/Assets/spine-unity` to `Assets/spine-unity` in your project.

### Setup video

[![](http://i.imgur.com/cPxKK3S.png)](https://www.youtube.com/watch?v=-V84OIvZdQc)

## Examples

* **spineboy** This shows the spineboy skeleton. First an animation is played that shows the draw order changing and events firing, then spineboy jumps and walks. Click spineboy to jump again. Notice the walk and jump animations are mixed and transition smoothly. The white cube on spineboy's right hand is a separate GameObject that is positioned using a `BoneComponent`. This example uses images that are split across two atlas pages. This demonstrates a multi-page atlas, but of course has a high number of draw calls.
* **goblins*** This shows a male and female goblin that use the same skeleton and animations. Click to change the skin from male to female and back. It has a single atlas page, so is drawn with just 1 draw call. It uses the `Skeleton Lit` shader for vertex lighting. The [Goblins.cs](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-unity/Assets/examples/goblins/Goblins.cs) script manipulates the head bone after the animation is applied.
* **dragon*** This shows the dragon skeleton. The flying animation has many image changes. It also shows shadow rendering. This example uses a multi-page atlas so has a high number of draw calls.

## Notes

- Atlas images should use premultiplied alpha when using the shaders that come with spine-unity.
- This slightly outdated [spine-unity tutorial video](http://www.youtube.com/watch?v=x1umSQulghA) may still be useful.
- Unity scales large images down by default if they exceed 1024x1024, which causes atlas coordinates to be incorrect. To fix this, override the import settings in the Inspector for any large atlas image you have so Unity does not scale it down.
- Unity 4.3+'s 2D project defaults cause atlas images added to the project to be imported with the Texture Type "Sprite", which may cause artifacts when using Spine's Skeleton shader. To avoid these artifacts, make sure the Texture Type is set to "Texture".
