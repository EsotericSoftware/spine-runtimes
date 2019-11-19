 # spine-ts

The spine-ts runtime provides functionality to load and manipulate [Spine](http://esotericsoftware.com) skeletal animation data using TypeScript and JavaScript. spine-ts is split
up into multiple modules:

1. **Core**: `core/`, the core classes to load and process Spine models
1. **WebGL**: `webgl/`, a self-contained WebGL backend, build on the core classes
1. **Canvas**: `canvas/`, a self-contained Canvas backend, build on the core classes
1. **THREE.JS**: `threejs/`, a self-contained THREE.JS backend, build on the core classes
1. **Player**: `player/`, a self-contained player to easily display Spine animations on your website, build on core classes & WebGL backend.

While the source code for the core library and backends is written in TypeScript, all code is compiled to easily consumable JavaScript.

## Licensing
You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](http://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](http://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-ts works with data exported from Spine 3.8.xx.

spine-ts WebGL & players backends supports all Spine features.

spine-ts Canvas does not support white space stripped texture atlases, color tinting, mesh attachments and clipping. Only the alpha channel from tint colors is applied. Experimental support for mesh attachments can be enabled by setting `spine.canvas.SkeletonRenderer.useTriangleRendering` to true. Note that this method is slow and may lead to artifacts on some browsers.

spine-ts THREE.JS does not support two color tinting & blend modes. The THREE.JS backend provides `SkeletonMesh.zOffset` to avoid z-fighting. Adjust to your near/far plane settings.

spine-ts does not yet support loading the binary format.

## Usage
1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it as a zip via the download button above.
2. To use only the core library without rendering support, include the `build/spine-core.js` file in your project.
3. To use the WebGL backend, include the `build/spine-webgl.js` file in your project.
3. To use the Canvas backend, include the `build/spine-canvas.js` file in your project.
4. To use the Player, include `build/spine-player.js` and `player/css/spine-player.css` file in your project.
5. To use the THREE.JS backend, include the `build/spine-threejs.js` file in your project. THREE.JS must be loaded first.

All `*.js` files are self-contained and include both the core and respective backend classes.

If you write your app with TypeScript, additionally copy the corresponding `build/spine-*.d.ts` file to your project.

**Note:** If you are using the compiled `.js` files with ES6 or other module systems, you have to add

```
export { spine };
```

At the bottom of the `.js` file you are using. You can then import the module as usual, e.g.:

```
import { spine } from './spine-webgl.js';
```

## Examples
To run the examples, the image, atlas, and JSON files must be served by a webserver, they can't be loaded from your local disk. Spawn a light-weight web server in the root of spine-ts, then navigate to the `index.html` file for the example you want to view. E.g.:

```
cd spine-ts
python -m SimpleHTTPServer
```

Then open `http://localhost:8000/webgl/example`, `http://localhost:8000/canvas/example`, `https://localhost:8000/threejs/example` or `http://localhost:8000/player/example` in your browser.

## WebGL Demos
The spine-ts WebGL demos load their image, atlas, and JSON files from our webserver and so can be run directly, without needing a webserver. The demos can be viewed [all on one page](http://esotericsoftware.com/spine-demos/) or in individual, standalone pages which are easy for you to explore and edit. See the [standalone demos source code](webgl/demos) and view the pages here:

- [Spine vs sprite sheets](http://rawgit.com/EsotericSoftware/spine-runtimes/3.6/spine-ts/webgl/demos/spritesheets.html)
- [Image changes](http://rawgit.com/EsotericSoftware/spine-runtimes/3.6/spine-ts/webgl/demos/imagechanges.html)
- [Transitions](http://rawgit.com/EsotericSoftware/spine-runtimes/3.6/spine-ts/webgl/demos/transitions.html)
- [Meshes](http://rawgit.com/EsotericSoftware/spine-runtimes/3.6/spine-ts/webgl/demos/meshes.html)
- [Skins](http://rawgit.com/EsotericSoftware/spine-runtimes/3.6/spine-ts/webgl/demos/skins.html)
- [Hoverboard](http://rawgit.com/EsotericSoftware/spine-runtimes/3.6/spine-ts/webgl/demos/hoverboard.html)
- [Vine](http://rawgit.com/EsotericSoftware/spine-runtimes/3.6/spine-ts/webgl/demos/vine.html)
- [Clipping](http://rawgit.com/EsotericSoftware/spine-runtimes/3.6/spine-ts/webgl/demos/clipping.html)
- [Stretchyman](http://rawgit.com/EsotericSoftware/spine-runtimes/3.6/spine-ts/webgl/demos/stretchyman.html)
- [Tank](http://rawgit.com/EsotericSoftware/spine-runtimes/3.6/spine-ts/webgl/demos/tank.html)
- [Transform constraints](http://rawgit.com/EsotericSoftware/spine-runtimes/3.6/spine-ts/webgl/demos/transforms.html)

Please note that Chrome and possibly other browsers do not use the original CORS headers when loading cached resources. After the initial page load for a demo, you may need to forcefully refresh (hold `shift` and click refresh) or clear your browser cache.

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
  * **Canvas**: `tsc -w -p tsconfig.canvas.json`, builds `core/src` and `canvas/src`, outputs `build/spine-canvas.js|d.ts|js.map`
  * **THREE.JS**: `tsc -w -p tsconfig.threejs.json`, builds `core/src` and `threejs/src`, outputs `build/spine-threejs.js|d.ts|js.map`
  * **Player**: `tsc -w -p tsconfig.player.json`, builds `core/src` and `player/src`, outputs `build/spine-player.js|d.ts|js.map`
6. Open the `spine-ts` folder in Visual Studio Code. VS Code will use the `tsconfig.json` file all source files from core and all
backends for your development pleasure. The actual JavaScript output is still created by the command line TypeScript compiler process from the previous step.

Each backend contains an `example/` folder with an `index.html` file that demonstrates the respective backend. For development, we
suggest to run a HTTP server in the root of `spine-ts`, e.g.

```
cd spine-ts
python -m SimpleHTTPServer
```

Then navigate to `http://localhost:8000/webgl/example`, `http://localhost:8000/canvas/example`, `http://localhost:8000/threejs/example` or `http://localhost:8000/player/example`

### Spine-ts WebGL backend
By default, the spine-ts WebGL backend supports two-color tinting. This has a neglible effect on performance, as more per vertex data has to be submitted to the GPU, and the fragment shader has to do a few more arithmetic operations.

You can disable two-color tinting like this:

```javascript
// If you use SceneRenderer, disable two-color tinting via the last constructor argument
var sceneRenderer = new spine.SceneRenderer(canvas, gl, false);

// If you use SkeletonRenderer and PolygonBatcher directly,
// disable two-color tinting in the respective constructor
// and use the shader returned by Shader.newColoredTextured()
// instead of Shader.newTwoColoredTextured()
var batcher = new spine.PolygonBatcher(gl, false);
var skeletonRenderer = new spine.SkeletonRenderer(gl, false);
var shader = Shader.newColoredTextured();
```

### Using the Player
Please see the documentation for the [Spine Web Player](https://esotericsoftware.com/spine-player)
