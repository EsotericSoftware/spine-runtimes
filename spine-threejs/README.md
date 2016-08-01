# spine-threejs

spine-threejs is a basic example of how to load and manipulate [Spine](http://esotericsoftware.com) skeletal animation data using [Three.js](http://threejs.org/). spine-threejs is based on [spine-js](https://github.com/EsotericSoftware/spine-runtimes/tree/master/spine-js).

## Licensing

This Spine Runtime may only be used for personal or internal use, typically to evaluate Spine before purchasing. If you would like to incorporate a Spine Runtime into your applications, distribute software containing a Spine Runtime, or modify a Spine Runtime, then you will need a valid [Spine license](https://esotericsoftware.com/spine-purchase). Please see the [Spine Runtimes Software License](https://github.com/EsotericSoftware/spine-runtimes/blob/master/LICENSE) for detailed information.

The Spine Runtimes are developed with the intent to be used with data exported from Spine. By purchasing Spine, `Section 2` of the [Spine Software License](https://esotericsoftware.com/files/license.txt) grants the right to create and distribute derivative works of the Spine Runtimes.

## Spine version

spine-threejs works with data exported from Spine 3.1.08. Updating spine-threejs to [v3.2](https://github.com/EsotericSoftware/spine-runtimes/issues/586) and [v3.3](https://github.com/EsotericSoftware/spine-runtimes/issues/613) is in progress.

spine-threejs supports all Spine features except for rendering meshes.

spine-threejs does not yet support loading the binary format.

## Setup

To run the example:

1. Copy the contents of `spine-js` to `spine-threejs/spine-js`.
1. Place the files on a webserver. Images can't be loaded when run from a local directory.
1. Open `spine-threejs/example/index.html` in a web browser.

## Demos

- [spine-threejs Demo](http://esotericsoftware.com/files/runtimes/spine-threejs/example/index.html)<br>
  [spine-turbulenz Demo source](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-threejs/example/index.html)

## Notes

- Atlas images should not use premultiplied alpha or rotation.
