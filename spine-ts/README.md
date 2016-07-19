# spine-ts

The spine-ts runtime provides functionality to load and manipulate [Spine](http://esotericsoftware.com) skeletal animation data using TypeScript and JavaScript.

## Licensing
This Spine Runtime may only be used for personal or internal use, typically to evaluate Spine before purchasing. If you would like to incorporate a Spine Runtime into your applications, distribute software containing a Spine Runtime, or modify a Spine Runtime, then you will need a valid [Spine license](https://esotericsoftware.com/spine-purchase). Please see the [Spine Runtimes Software License](https://github.com/EsotericSoftware/spine-runtimes/blob/master/LICENSE) for detailed information.

The Spine Runtimes are developed with the intent to be used with data exported from Spine. By purchasing Spine, `Section 2` of the [Spine Software License](https://esotericsoftware.com/files/license.txt) grants the right to create and distribute derivative works of the Spine Runtimes.

## Spine version

spine-ts works with data exported from the latest Spine version.

spine-ts supports all Spine features. When using the canvas renderer, color tinting, mesh attachments, or nonuniform scaling are not supported.

spine-ts does not yet support loading the binary format.

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
1. Include the `spine.js` file in your project.

If you write your app with TypeScript, you can simply copy the contents of `src/` directory to your project and adjust `tsconfig.json` to include the files for compilation. Alternatively you can also
include `spine.js` and `spine.d.ts` in your project.

## Demos

- [spine-canvas](http://esotericsoftware.com/files/runtimes/spine-js/example/)

## Runtimes Extending spine-js

- [spine-turbulenz](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-turbulenz)
- [spine-threejs](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-threejs)
