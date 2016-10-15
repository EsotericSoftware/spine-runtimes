# spine-libgdx

The spine-libgdx runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [libgdx](http://www.libgdx.com/).

## Licensing

This Spine Runtime may only be used for personal or internal use, typically to evaluate Spine before purchasing. If you would like to incorporate a Spine Runtime into your applications, distribute software containing a Spine Runtime, or modify a Spine Runtime, then you will need a valid [Spine license](https://esotericsoftware.com/spine-purchase). Please see the [Spine Runtimes Software License](https://github.com/EsotericSoftware/spine-runtimes/blob/master/LICENSE) for detailed information.

The Spine Runtimes are developed with the intent to be used with data exported from Spine. By purchasing Spine, `Section 2` of the [Spine Software License](https://esotericsoftware.com/files/license.txt) grants the right to create and distribute derivative works of the Spine Runtimes.

## Spine version

spine-libgdx works with data exported from Spine 3.4.02.

spine-libgdx supports all Spine features and is the reference runtime implementation.

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
1. Using Eclipse, import the project by choosing File -> Import -> Existing projects. For other IDEs you will need to create a new project and import the source.

Alternatively, the contents of the `spine-libgdx/src` directory can be copied into your project.

## Notes

* The "test" source directory contains optional examples.
* spine-libgdx depends on the gdx-backend-lwjgl project so the tests can easily be run on the desktop. If the tests are excluded, spine-libgdx only needs to depend on the gdx project.
* spine-libgdx depends on the gdx-box2d extension project solely for the `Box2DExample` test.

## Examples

* [HTML5 example](http://esotericsoftware.com/files/runtimes/spine-libgdx/raptor/)
* [Super Spineboy](https://github.com/EsotericSoftware/spine-superspineboy) Full game example done with Spine Essential, includes source code.
* [Simple example 1](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-libgdx/spine-libgdx-tests/src/com/esotericsoftware/spine/SimpleTest1.java) Simplest possible example, fully commented.
* [Simple example 2](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-libgdx/spine-libgdx-tests/src/com/esotericsoftware/spine/SimpleTest2.java) Shows events and bounding box hit detection.
* [Simple example 3](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-libgdx/spine-libgdx-tests/src/com/esotericsoftware/spine/SimpleTest3.java) Shows mesh rendering and IK using the raptor example.
* [More examples](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-libgdx/spine-libgdx-tests/src/com/esotericsoftware/spine/)
