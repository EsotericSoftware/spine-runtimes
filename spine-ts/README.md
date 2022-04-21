# spine-ts

The spine-ts runtime provides functionality to load and manipulate [Spine](http://esotericsoftware.com) skeletal animation data using TypeScript and JavaScript. spine-ts is split
up into multiple modules:

1. `spine-core/`, the core classes to load and process Spine skeletons.
1. `spine-webgl/`, a self-contained WebGL backend, built on the core classes.
1. `spine-canvas/`, a self-contained Canvas backend, built on the core classes.
1. `spine-threejs/`, a self-contained THREE.JS backend, built on the core classes.
1. `spine-player/`, a self-contained player to easily display Spine animations on your website, built on the core classes and WebGL backend.

In most cases, the `spine-player` module is best suited for your needs. Please refer to the [Spine Web Player documentation](https://esotericsoftware.com/spine-player) for more information.

For documentation of the core API in `spine-core`, please refer to our [Spine Runtimes Guide](http://esotericsoftware.com/spine-runtimes-guide).

For module specific APIs in `spine-canvas`, `spine-webgl`, and `spine-threejs`, please refer to the [Examples](#examples) in the respecitve `spine-<modulename>/example` folder. For `spine-webgl` specifically, we have provided additional [demos](spine-webgl/demos), which you can also [view online](http://de.esotericsoftware.com/spine-demos).

## Licensing

You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](http://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](http://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-ts works with data exported from Spine 4.1.xx.

The spine-ts WebGL and Player backends support all Spine features.

spine-ts Canvas does not support mesh attachments, clipping attachments, or color tinting. Only the alpha channel from tint colors is applied. Experimental support for mesh attachments can be enabled by setting `spine.SkeletonRenderer.useTriangleRendering` to true. Note that this experimental mesh rendering is slow and render with artifacts on some browsers.

spine-ts THREE.JS does not support two color tinting. The THREE.JS backend provides `SkeletonMesh.zOffset` to avoid z-fighting. Adjust to your near/far plane settings.

## Usage

All spine-ts modules are published to [npm](http://npmjs.com) for consumption via vanilla JavaScript as well as 

## Usage in vanilla JavaScript
You can include a module in your project via a `<script>` tag from the [unpkg](https://unpkg.com/) CDN, specifying the version as part of the URL. In the examples below, the version is `4.0.*`, which fetches the latest patch release, and which will work with all exports from Spine Editor version `4.0.x`.

```
// spine-ts Core
<script src="https://unpkg.com/@esotericsoftware/spine-core@4.0.*/dist/iife/spine-core.js">

// spine-ts Canvas
<script src="https://unpkg.com/@esotericsoftware/spine-canvas@4.0.*/dist/iife/spine-canvas.js">

// spine-ts WebGL
<script src="https://unpkg.com/@esotericsoftware/spine-webgl@4.0.*/dist/iife/spine-webgl.js">

// spine-ts Player, which requires a spine-player.css as well
<script src="https://unpkg.com/@esotericsoftware/spine-player@4.0.*/dist/iife/spine-player.js">
<link rel="stylesheet" href="https://unpkg.com/@esotericsoftware/spine-player@4.0.*/dist/spine-player.css">

// spine-ts ThreeJS
<script src="https://unpkg.com/@esotericsoftware/spine-threejs@4.0.*/dist/iife/spine-threejs.js">
```

We also provide `js.map` source maps. They will be automatically fetched from unpkg when debugging code of a spine-module in Chrome, Firefox, or Safari, mapping the JavaScript code back to its original TypeScript sources.

We provide minified versions of each module, which can be used by replacing the `.js` file suffix with `.min.js` in the unpkg URLs.

## Usage via NPM or Yarn
If your project dependencies are managed through NPM or Yarn, you can add spine-ts modules the usual way:

```
npm install @esotericsoftware/spine-core
npm install @esotericsoftware/spine-canvas
npm install @esotericsoftware/spine-webgl
npm install @esotericsoftware/spine-player
npm install @esotericsoftware/spine-threejs
```

spine-ts modules are provided in the [ECMAScript format](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Modules), which can be consumed natively by all modern browsers, or bundled by tools like [webpack](https://webpack.js.org/), [Babel](https://babeljs.io/), [Parcel](https://parceljs.org/), or [esbuild](https://esbuild.github.io/). You can import functions and classes from a spine-ts module in your JavaScript or TypeScript code using the `import` syntax to get access to all exported constants, functions, and classes of a module:

```
import spine from "@esotericsoftware/spine-core"
```

Our module packages also contain `js.map` source maps as well as `d.ts` typings for easier debugging and development.

You can find all our published modules on the [npm registry](https://www.npmjs.com/search?q=%40esotericsoftware) via the `@esotericsoftware` scope.

## Examples

Every module except `spine-core` contains an `example/` folder demonstrating usage of that module's API. To run the examples, install [Node.js](https://nodejs.org/en/), then run the following command in the `spine-runtimes/spine-ts` folder:

```
npm run dev
```

This will compile the modules and start a server that serves the example pages at http://127.0.0.1:8080. When you make changes to the source code of either the modules and the examples, the source get recompiled, and the open page in the browser is reloaded automatically.

## Development setup
spine-ts is developed with TypeScript, we thus recommend the following development environment when working on its sources:

2. Install a [Git Client](https://git-fork.com/) and make sure it's available on the command line.
1. Install [NPM](https://nodejs.org/en/download/) and make sure it's available on the command line.
2. Install [Visual Studio Code](https://code.visualstudio.com/).
3. Open a terminal and execute
```
git clone https://github.com/esotericsoftware/spine-runtimes
cd spine-runtimes/spine-ts
npm install
npm run dev
```

The final command `npm run dev` will start a local web server at http://127.0.0.1:8080, which reloads any page it served automatically in case the source code has changed. The command will also start the build tools in watch mode, meaning they will recompile any source code changes in the background automatically.

You can then open Visual Studio Code to inspect, edit, and debug the source code. We also supply launch configurations to start examples and demos in debug mode, so you can debug them right inside Visual Studio code.

To build the artifacts as they are published to NPM, run `npm run build`.
