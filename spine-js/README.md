# spine-js

The spine-js runtime provides functionality to load and manipulate [Spine](http://esotericsoftware.com) skeletal animation data using JavaScript. It does not perform rendering but can be extended to enable Spine animations for other JavaScript-based projects.

# spine-canvas

The spine-canvas runtime extends spine-js to perform rendering using an HTML5 canvas. Because it renders rectangular images, nonuniform scaling and mesh attachments are not supported.

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
1. Include the `spine.js` file (and optionally the `spine-canvas.js` file) in your project.

## Demos

- [spine-canvas](http://esotericsoftware.com/files/runtimes/spine-js/example/)

## Runtimes Extending spine-js

- [spine-turbulenz](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-turbulenz)
