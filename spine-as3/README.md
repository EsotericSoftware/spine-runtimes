# spine-as3

The spine-as3 runtime provides functionality to load and manipulate [Spine](http://esotericsoftware.com) skeletal animation data using Adobe's ActionScript 3.0 (AS3). The [`spine.flash` package](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-as3/spine-as3/src/spine/flash) can be used to render Spine animations using Flash. spine-as3 can be extended to enable Spine animations for other AS3 projects, such as [Starling](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-starling).

## Licensing

This Spine Runtime may only be used for personal or internal use, typically to evaluate Spine before purchasing. If you would like to incorporate a Spine Runtime into your applications, distribute software containing a Spine Runtime, or modify a Spine Runtime, then you will need a valid [Spine license](https://esotericsoftware.com/spine-purchase). Please see the [Spine Runtimes Software License](https://github.com/EsotericSoftware/spine-runtimes/blob/master/LICENSE) for detailed information.

The Spine Runtimes are developed with the intent to be used with data exported from Spine. By purchasing Spine, `Section 2` of the [Spine Software License](https://esotericsoftware.com/files/license.txt) grants the right to create and distribute derivative works of the Spine Runtimes.

## Spine version

spine-as3 works with data exported from Spine 3.1.08. Updating spine-as3 to [v3.2](https://github.com/EsotericSoftware/spine-runtimes/issues/586) and [v3.3](https://github.com/EsotericSoftware/spine-runtimes/issues/613) is in progress.

spine-as3 supports all Spine features, including meshes. If using the `spine.flash` classes for rendering, meshes are not supported.

spine-as3 does not yet support loading the binary format.

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
1. Using [FDT](http://fdt.powerflasher.com/), import the spine-as3 project by choosing File -> Import -> Existing projects. For other IDEs you will need to create a new project and import the source.

Alternatively, the contents of the `spine-as3/src` directory can be copied into your project.

## Demos

* [Flash Demo](http://esotericsoftware.com/files/runtimes/spine-as3/spineboy/index.html)
  [Flash Demo source](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-as3/spine-as3-example/src/spine/Main.as#L43)

## Notes

- Atlas images should not use premultiplied alpha.
