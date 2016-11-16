# spine-ts

The spine-ts runtime provides functionality to load and manipulate [Spine](http://esotericsoftware.com) skeletal animation data using TypeScript and JavaScript. spine-ts is split
up into multiple modules:

1. **Core**: `core/`, the core classes to load and process Spine models
1. **WebGL**: `webgl/`, a self-contained WebGL backend, build on the core classes
1. **Canvas**: `canvas/`, a self-contained Canvas backend, build on the core classes
1. **THREE.JS**: `threejs/`, a self-contained THREE.JS backend, build on the core classes
1. **Widget**: `widget/`, a self-contained widget to easily display Spine animations on your website, build on core classes & WebGL backend.

While the source code for the core library and backends is written in TypeScript, all code is compiled to easily consumable JavaScript.

## Licensing
This Spine Runtime may only be used for personal or internal use, typically to evaluate Spine before purchasing. If you would like to incorporate a Spine Runtime into your applications, distribute software containing a Spine Runtime, or modify a Spine Runtime, then you will need a valid [Spine license](https://esotericsoftware.com/spine-purchase). Please see the [Spine Runtimes Software License](https://github.com/EsotericSoftware/spine-runtimes/blob/master/LICENSE) for detailed information.

The Spine Runtimes are developed with the intent to be used with data exported from Spine. By purchasing Spine, `Section 2` of the [Spine Software License](https://esotericsoftware.com/files/license.txt) grants the right to create and distribute derivative works of the Spine Runtimes.

## Spine version

spine-ts works with data exported from Spine 3.5.xx.

spine-ts WebGL & Widget backends supports all Spine features. The spine-ts Canvas backend does not support color tinting, mesh attachments or shearing. Mesh attachments are supported by setting `spine.canvas.SkeletonRenderer.useTriangleRendering` to true. Note that this method is slow and may lead to artifacts on some browsers. The spine-ts THREE.JS backend does not support color tinting and blend modes. The THREE.JS backend provides `SkeletonMesh.zOffset` to avoid z-fighting. Adjust to your near/far plane settings.

spine-ts does not yet support loading the binary format.

## Usage
1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/master.zip).
2. To use only the core library without rendering support, include the `build/spine-core.js` file in your project.
3. To use the WebGL backend, include the `spine-webgl.js` file in your project.
3. To use the Canvas backend, include the `spine-canvas.js` file in your project.
4. To use the Widget, include `spine-widget.js` file in your project.
5. To use the THREE.JS backend, include the `spine-threejs.js` file in your project. THREE.JS must be loaded first.

All `*.js` files are self-contained and include both the core and respective backend classes.

If you write your app with TypeScript, additionally copy the corresponding `build/spine-*.d.ts` file to your project.

## Examples
To run the examples, the image, atlas, and JSON files must be served by a webserver, they can't be loaded from your local disk. Spawn a light-weight web server in the root of spine-ts, then navigate to the `index.html` file for the example you want to view. E.g.:

```
cd spine-ts
python -m SimpleHTTPServer
````

Then open `http://localhost:8000/webgl/example`, `http://localhost:8000/canvas/example`, `https://localhost:8000/threejs/example` or `http://localhost:8000/widget/example` in your browser.

## WebGL Demos
The spine-ts WebGL demos load their image, atlas, and JSON files from our webserver and so can be run directly, without needing a webserver. View the demos [all on one page](http://esotericsoftware.com/spine-demos/) or use the [standalone demos]() which are easy for you to explore and edit. The standalone demos can also be viewed here:

- [Spine vs sprite sheets](http://rawgit.com/EsotericSoftware/spine-runtimes/master/spine-ts/webgl/demos/spritesheets.html)
- [Image changes](http://rawgit.com/EsotericSoftware/spine-runtimes/master/spine-ts/webgl/demos/imagechanges.html)
- [Transitions](http://rawgit.com/EsotericSoftware/spine-runtimes/master/spine-ts/webgl/demos/transitions.html)
- [Meshes](http://rawgit.com/EsotericSoftware/spine-runtimes/master/spine-ts/webgl/demos/meshes.html)
- [Skins](http://rawgit.com/EsotericSoftware/spine-runtimes/master/spine-ts/webgl/demos/skins.html)
- [Hoverboard](http://rawgit.com/EsotericSoftware/spine-runtimes/master/spine-ts/webgl/demos/hoverboard.html)
- [Transform constraints](http://rawgit.com/EsotericSoftware/spine-runtimes/master/spine-ts/webgl/demos/transforms.html)
- [Tank](http://rawgit.com/EsotericSoftware/spine-runtimes/master/spine-ts/webgl/demos/tank.html)
- [Vine](http://rawgit.com/EsotericSoftware/spine-runtimes/master/spine-ts/webgl/demos/vine.html)
- [Stretchyman](http://rawgit.com/EsotericSoftware/spine-runtimes/master/spine-ts/webgl/demos/stretchyman.html)

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
  * **Widget**: `tsc -w -p tsconfig.widget.json`, builds `core/src` and `widget/src`, outputs `build/spine-widget.js|d.ts|js.map` 
6. Open the `spine-ts` folder in Visual Studio Code. VS Code will use the `tsconfig.json` file all source files from core and all 
backends for your development pleasure. The actual JavaScript output is still created by the command line TypeScript compiler process from the previous step.

Each backend contains an `example/` folder with an `index.html` file that demonstrates the respective backend. For development, we
suggest to run a HTTP server in the root of `spine-ts`, e.g.

```
cd spine-ts
python -m SimpleHTTPServer
```

Then navigate to `http://localhost:8000/webgl/example`, `http://localhost:8000/canvas/example`, `http://localhost:8000/threejs/example` or `http://localhost:8000/widget/example`

### Using the Widget
To easily display Spine animations on your website, you can use the spine-ts Widget backend. 

1. Export your Spine animation with a texture atlas and put the resulting `.json`, `.atlas` and `.png` files on your server.
2. Copy the `build/spine-widget.js` file to your server and include it on your website `<script src="spine-widget.js"></script>`, adjusting the src to match the location of the file on your server.
3. Add HTML elements, e.g. a `<div>`, with the class `spine-widget` to your website, specifying its configuration such as the location of the files, the animation to display, etc.

You can configure a HTML element either directly via HTML, or using JavaScript. At a minimum, you need to specify the location of the
`.json` and `.atlas` file as well as the name of the animation to play back.

#### HTML configuration
To specify the configuration of a Spine Widget via HTML, you can use these HTML element attributes:

  * `data-json`: required, path to the `.json` file, absolute or relative, e.g. "assets/animation.json"
  * `data-atlas`: required, path to the `.atlas` file, absolute or relative, e.g. "assets/animation.atlas"
  * `data-animation`: required, the name of the animation to play back
  * `data-images-path`: optional, the location of images on the server to load atlas pages from. If omitted, atlas `.png` page files are loaded relative to the `.atlas` file.
  * `data-skin`: optional, the name of the skin to use. Defaults to `default` if omitted.
  * `data-loop`: optional, whether to loop the animation or not. Defaults to `true` if omitted.
  * `data-scale`: optional, the scaling factor to apply when loading the `.json` file. Defaults to `1` if omitted. Irrelevant if `data-fit-to-canavs` is `true`.
  * `data-x`: optional, the x-coordinate to display the animation at. The origin is in the bottom left corner. Defaults to `0` if omitted. Irrelevant if `data-fit-to-canvas` is `true`.
  * `data-y`: optional, the y-coordinate to display the animation at. The origin is in the bottom left corner with the y-axis pointing upwards. Defaults to `0` if omitted. Irrelevant if `data-fit-to-canvas` is `true`.
  * `data-fit-to-canvas`: optional, whether to fit the animation to the canvas size or not. Defaults to `true` if omitted, in which case `data-scale`, `data-x` and `data-y` are irrelevant. This setting calculates the setup pose bounding box using the specified skin to center and scale the animation on the canvas.
  * `data-background-color`: optional, the background color to use. Defaults to `#000000` if omitted.
  * `data-premultiplied-alpha`: optional, whether the atlas pages use premultiplied alpha or not. Defaults to `false` if omitted.
  * `data-debug`: optional, whether to show debug information such as bones, attachments, etc. Defaults to `false` if omitted. 

You can specify these as attribuets on the HTML element like this:

```html
<div class="spine-widget" data-json="assets/animation.json" data-atlas="assets/animation.atlas" data-animation="walk"></div>
```

All HTML elements with class `spine-widget` will be automatically loaded when the website is finished loading by the browser. To add
widgets dynamically, use the JavaScript configuration described below.

#### JavaScript configuration
You can dynamically add Spine Widgets to your web page by using the JavaScript API.

Create a HTML element on your website, either statically or via JavaScript:

```html
<div id="my-widget"></div>
```

Then create a new `spine.SpineWidget`, providing a [`SpineWidgetConfiguration`](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-ts/widget/src/Widget.ts#L281) object, e.g.:

```JavaScript
new spine.SpineWidget("my-widget", {
	json: "assets/spineboy.json",
	atlas: "assets/spineboy.atlas",		
	animation: "run",	
	backgroundColor: "#000000",
	success: function (widget) {
		var animIndex = 0;
		widget.canvas.onclick = function () {				
			animIndex++;
			let animations = widget.skeleton.data.animations;
			if (animIndex >= animations.length) animIndex = 0;
			widget.setAnimation(animations[animIndex].name);
		}
	}
});
```

The configuration object has the following fields:

  * `json`: required, path to the `.json` file, absolute or relative, e.g. "assets/animation.json"
  * `atlas`: required, path to the `.atlas` file, absolute or relative, e.g. "assets/animation.atlas"
  * `animation`: required, the name of the animation to play back
  * `imagesPath`: optional, the location of images on the server to load atlas pages from. If omitted, atlas `.png` page files are loaded relative to the `.atlas` file.
  * `skin`: optional, the name of the skin to use. Defaults to `default` if omitted.
  * `loop`: optional, whether to loop the animation or not. Defaults to `true` if omitted.
  * `scale`: optional, the scaling factor to apply when loading the `.json` file. Defaults to `1` if omitted. Irrelevant if `data-fit-to-canavs` is `true`.
  * `x`: optional, the x-coordinate to display the animation at. The origin is in the bottom left corner. Defaults to `0` if omitted. Irrelevant if `data-fit-to-canvas` is `true`.
  * `y`: optional, the y-coordinate to display the animation at. The origin is in the bottom left corner with the y-axis pointing upwards. Defaults to `0` if omitted. Irrelevant if `data-fit-to-canvas` is `true`.
  * `fitToCanvas`: optional, whether to fit the animation to the canvas size or not. Defaults to `true` if omitted, in which case `data-scale`, `data-x` and `data-y` are irrelevant. This setting calculates the setup pose bounding box using the specified skin to center and scale the animation on the canvas.
  * `alpha`: optional, whether to allow the canvas to be transparent. Defaults to `true`. Set the alpha channel in ``backgroundColor` to 0 as well, e.g. `#00000000`.
  * `backgroundColor`: optional, the background color to use. Defaults to `#000000` if omitted.
  * `premultipliedAlpha`: optional, whether the atlas pages use premultiplied alpha or not. Defaults to `false` if omitted.
  * `debug`: optional, whether to show debug information such as bones, attachments, etc. Defaults to `false` if omitted.
  * `success`: optional, a callback taking a `SpineWidget` called when the animation has been loaded successfully
  * `error`: optional, a callback taking a `SpineWidget` and an error string called when the animation couldn't be loaded

You can also create a HTML element with class `spine-widget` and `data-` attributes as described above and call `spine.widget.SpineWidget.loadWidget(element)` to load
an element via JavaScript on demand.

The resulting `SpineWidget` has various fields that let you modify the animation programmatically. Most notably, the `skeleton` and `state` fields
let you modify all aspects of your animation as you wish. See the [example](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-ts/widget/example/index.html#L21).

You can also modify what debug information is shown by accessing `SpineWidget.debugRenderer` and set the various `drawXXX` fields to `true` or `false`.