# spine-tk2d

The spine-tk2d runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [2D Toolkit](http://www.unikronsoftware.com/2dtoolkit/) for [Unity](http://unity3d.com/). spine-tk2d is based on [spine-csharp](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-csharp) and is very similar to [spine-unity](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-unity).

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
1. Copy the contents of `spine-csharp/src` to `spine-tk2d/Assets/Spine/spine-csharp`.
1. Open the `spine-tk2d/Assets/examples/spineboy/spineboy.unity` scene file using Unity 4.2+.
1. Import 2D Toolkit into the project.

# Notes

- Atlas images must use premultiplied alpha.
- This slightly outdated [setup tutorial video](http://www.youtube.com/watch?v=dnQbS9ap-i8) may still be useful to some.
