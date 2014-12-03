# spine-as3

The spine-as3 runtime provides functionality to load and manipulate [Spine](http://esotericsoftware.com) skeletal animation data using Adobe's ActionScript 3.0 (AS3). The [`spine.flash` package](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-as3/spine-as3/src/spine/flash) can be used to render Spine animations using Flash. spine-as3 can be extended to enable Spine animations for other AS3 projects, such as [Starling](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-starling).

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
1. Using Adobe Flash Builder 4.6, import the spine-as3 project by choosing File -> Import -> Existing projects. For other IDEs you will need to create a new project and import the source.

Alternatively, the contents of the `spine-as3/src` directory can be copied into your project.

## Demos

* [Flash Demo](http://esotericsoftware.com/spine/files/demos/as3/spineboy/index.html)
*  [Flash Demo source](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-as3/spine-as3-example/src/Main.as#L55)

## Notes

- Atlas images should not use premultiplied alpha.
