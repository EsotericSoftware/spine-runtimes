# spine-ts

The spine-ts runtime provides functionality to load and manipulate [Spine](http://esotericsoftware.com) skeletal animation data using TypeScript and JavaScript. spine-ts is split
up into multiple modules:

1. **Core**: `core/`, the core classes to load and process Spine models
1. **WebGL**: `webgl/`, a self-contained WebGL backend, build on the core classes
1. **Canvas**: `canvas/`, a self-contained Canvas backend, build on the core classes
1. **Widget**: `widget/`, a self-contained widget to easily display Spine animations on your website, build on core classes & WebGL backend.

While the source code for the core library and backends is written in TypeScript, all code is compiled to easily consumable JavaScript.

## Licensing
This Spine Runtime may only be used for personal or internal use, typically to evaluate Spine before purchasing. If you would like to incorporate a Spine Runtime into your applications, distribute software containing a Spine Runtime, or modify a Spine Runtime, then you will need a valid [Spine license](https://esotericsoftware.com/spine-purchase). Please see the [Spine Runtimes Software License](https://github.com/EsotericSoftware/spine-runtimes/blob/master/LICENSE) for detailed information.

The Spine Runtimes are developed with the intent to be used with data exported from Spine. By purchasing Spine, `Section 2` of the [Spine Software License](https://esotericsoftware.com/files/license.txt) grants the right to create and distribute derivative works of the Spine Runtimes.

## Spine version

spine-ts works with data exported from the latest Spine version.

spine-ts WebGL & Widget backends supports all Spine features. The spine-ts Canvas backend does not support color tinting, mesh attachments or shearing. Mesh attachments are supported by setting `spine.canvas.SkeletonRenderer.useTriangleRendering` to true. Note that this method is slow and may lead to artifacts on some browsers.

spine-ts does not yet support loading the binary format.

## Usage
1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
2. To use only the core library without rendering support, include the `build/spine-core.js` file in your project.
3. To use the WebGL backend, include the `spine-webgl.js` file in your project.
3. To use the Canvas backend, include the `spine-canvas.js` file in your project.
4. To use the Widget, include `spine-widget.js` file in your project.

All `*.js` files are self-contained and include both the core and respective backend classes.

If you write your app with TypeScript, additionally copy the corresponding `build/spine-*.d.ts` file to your project.

## Example
To run the examples, spawn a light-weight web server in the root of spine-ts, then navigate to the respective
`index.html` file. E.g.:

```
cd spine-ts
python -m SimpleHTTPServer
````

Then open `http://localhost:8000/webgl/example`, `http://localhost:8000/canvas/example` or `http://localhost:8000/widget/example` in your browser.

## Development Setup
The spine-ts runtime and the various backends are implemented in TypeScript for greater maintainability and better tooling support. To
setup a development environment, follow these steps.

1. Install [NPM](https://nodejs.org/en/download/) and make sure it's available on the command line
2. On the command line, Install the TypeScript compiler via `npm install -g typescript`
3. Install [Visual Studio Code](https://code.visualstudio.com/)
4. On the command line, change into the `spine-ts` directory
5. Start the TypeScript compiler in watcher mode for the backend you want to work on:
  * **Core**: `tsc -w -p tsconfig.core.json`, builds `core/src`, outputs `build/spine-core.js|d.ts|js.map`
  * **WebGL**: `tsc -w -p tsconfig.webgl.json`, builds `core/src` and `webgl/src`, outputs `build/spine-webgl.js|d.ts|js.map`
  * **WebGL**: `tsc -w -p tsconfig.canvas.json`, builds `core/src` and `canvas/src`, outputs `build/spine-canvas.js|d.ts|js.map`
  * **Widget**: `tsc -w -p tsconfig.widget.json`, builds `core/src` and `widget/src`, outputs `build/spine-widget.js|d.ts|js.map`
6. Open the `spine-ts` folder in Visual Studio Code. VS Code will use the `tsconfig.json` file all source files from core and all 
backends for your development pleasure. The actual JavaScript output is still created by the command line TypeScript compiler process from the previous step.

Each backend contains an `example/` folder with an `index.html` file that demonstrates the respective backend. For development, we
suggest to run a HTTP server in the root of `spine-ts`, e.g.

```
cd spine-ts
python -m SimpleHTTPServer
```

Then navigate to `http://localhost:8000/webgl/example`, `http://localhost:8000/canvas/example` or `http://localhost:8000/widget/example`