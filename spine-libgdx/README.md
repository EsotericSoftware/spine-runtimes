# spine-libgdx

The spine-libgdx runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [libgdx](http://www.libgdx.com/).

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
1. Using Eclipse, import the project by choosing File -> Import -> Existing projects. For other IDEs you will need to create a new project and import the source.

Alternatively, the contents of the `spine-libgdx/src` directory can be copied into your project.

## Notes

* The "test" source directory contains optional examples.
* spine-libgdx depends on the gdx-backend-lwjgl project so the tests can easily be run on the desktop. If the tests are excluded, spine-libgdx only needs to depend on the gdx project.
* spine-libgdx depends on the gdx-box2d extension project solely for the `Box2DExample` test.

## Examples

* [Simple example 1](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-libgdx/spine-libgdx-tests/src/com/esotericsoftware/spine/SimpleTest1.java) Simplest possible example, fully commented.
* [Simple example 2](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-libgdx/spine-libgdx-tests/src/com/esotericsoftware/spine/SimpleTest2.java) Shows events and bounding box hit detection.
* [Simple example 3](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-libgdx/spine-libgdx-tests/src/com/esotericsoftware/spine/SimpleTest3.java) Shows mesh rendering and IK using the raptor example.
* [Box2D Example](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-libgdx/test/com/esotericsoftware/spine/Box2DExample.java#L56) (written before bounding boxes were available)
* [Mix Example](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-libgdx/test/com/esotericsoftware/spine/MixTest.java#L39)
* [Super Spineboy](https://github.com/EsotericSoftware/spine-superspineboy) Full game example done with Spine Essential.
