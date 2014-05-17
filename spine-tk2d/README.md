# spine-tk2d

The spine-tk2d runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [2D Toolkit](http://www.unikronsoftware.com/2dtoolkit/) for [Unity](http://unity3d.com/). spine-tk2d is based on [spine-csharp](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-csharp) and is very similar to [spine-unity](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-unity).

A Spine skeleton is a GameObject and can be used throughout Unity like any other GameObject. The `BoneComponent` class allows other GameObjects to follow a bone in a Spine skeleton.

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
1. Copy the contents of `spine-csharp/src` to `spine-tk2d/Assets/spine-csharp`.
1. Open the `spine-tk2d/Assets/examples/spineboy/spineboy.unity` scene file using Unity 4.3.4+.
1. Import 2D Toolkit into the example project.

To use spine-tk2d in your own Unity project:

1. Copy the contents of `spine-csharp/src` to `Assets/spine-csharp` in your project.
1. Copy the `spine-tk2d/Assets/spine-tk2d` to `Assets/spine-tk2d` in your project.
1. Import 2D Toolkit into your project.

## Examples

* **spineboy** This shows the spineboy skeleton with shadows. First an animation is played that shows the draw order changing and events firing, then spineboy jumps and walks. Click spineboy to jump again. Notice the walk and jump animations are mixed and transition smoothly. The white cube on spineboy's right hand is a separate GameObject that is positioned using a `BoneComponent`.

## Notes

- Atlas images should use premultiplied alpha when using the shaders that come with spine-tk2d.
- This slightly outdated [spine-tk2d setup video](http://www.youtube.com/watch?v=dnQbS9ap-i8) may still be useful.
