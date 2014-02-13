# spine-unity

The spine-unity runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [Unity](http://unity3d.com/) directly, without other 3rd party plugins. spine-unity is based on [spine-csharp](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-csharp) and is very similar to [spine-tk2d](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-tk2d).

A Spine skeleton is a single GameObject and can be used throughout Unity like any other GameObject. The `BoneComponent` class allows other GameObjects to follow a bone in a Spine skeleton.

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
1. Copy the contents of `spine-csharp/src` to `spine-unity/Assets/Spine/spine-csharp`.
1. Open the `spine-unity/Assets/examples/spineboy/spineboy.unity` scene file using Unity 4.2+.

# Notes

- Atlas images should use premultiplied alpha when using the shaders that come with spine-unity.
- This slightly outdated [spine-unity tutorial video](http://www.youtube.com/watch?v=x1umSQulghA) may still be useful.
- Unity scales large images down by default if they exceed 1024x1024, which causes atlas coordinates to be incorrect. To fix this, override the import settings in the Inspector for any large atlas image you have so Unity does not scale it down.
- Unity 4.3+'s 2D project defaults cause atlas images added to the project to be imported with the Texture Type "Sprite", which may cause artifacts when using Spine's Skeleton shader. To avoid these artifacts, make sure the Texture Type is set to "Texture".
