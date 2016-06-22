# spine-starling

The spine-starling runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [Starling](http://gamua.com/starling/). spine-starling is based on [spine-as3](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-as3).

## Licensing

This Spine Runtime may only be used for personal or internal use, typically to evaluate Spine before purchasing. If you would like to incorporate a Spine Runtime into your applications, distribute software containing a Spine Runtime, or modify a Spine Runtime, then you will need a valid [Spine license](https://esotericsoftware.com/spine-purchase). Please see the [Spine Runtimes Software License](https://github.com/EsotericSoftware/spine-runtimes/blob/master/LICENSE) for detailed information.

The Spine Runtimes are developed with the intent to be used with data exported from Spine. By purchasing Spine, `Section 2` of the [Spine Software License](https://esotericsoftware.com/files/license.txt) grants the right to create and distribute derivative works of the Spine Runtimes.

## Spine version

spine-starling works with data exported from Spine 3.1.08. Updating spine-starling to [v3.2](https://github.com/EsotericSoftware/spine-runtimes/issues/586) and [v3.3](https://github.com/EsotericSoftware/spine-runtimes/issues/613) is in progress.

spine-starling supports all Spine features.

spine-starling does not yet support loading the binary format.

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
1. Using [FDT](http://fdt.powerflasher.com/), import the spine-as3 and spine-starling projects by choosing File -> Import -> Existing projects. For other IDEs you will need to create new projects and import the source.

Alternatively, the contents of the `spine-as3/src` and `spine-starling/src` directories can be copied into your project.

## Examples

[Spine atlas example](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-starling/spine-starling-example/src/AtlasExample.as#L21)
[Starling atlas example](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-starling/spine-starling-example/src/StarlingAtlasExample.as#L18)
[Skin example](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-starling/spine-starling-example/src/GoblinsExample.as#L21)
