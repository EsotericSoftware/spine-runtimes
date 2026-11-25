# spine-ts

The spine-ts runtime provides functionality to load and manipulate [Spine](http://esotericsoftware.com) skeletal animation data using TypeScript and JavaScript. spine-ts is split
up into multiple modules:

1. `spine-core/`, the core classes to load and process Spine skeletons.
1. `spine-webgl/`, a self-contained WebGL backend, built on the core classes.
1. `spine-canvas/`, a self-contained Canvas backend, built on the core classes.
1. `spine-canvaskit/`, a self-contained [CanvasKit](https://skia.org/docs/user/modules/canvaskit/) backend, built on the core classes for CanvasKit, supporting both NodeJS for headless rendering, and browsers.
1. `spine-player/`, a self-contained player to easily display Spine animations on your website, built on the core classes and WebGL backend.
1. `spine-webcomponents/`, web components to easily display Spine animations on your website, built on the core classes and WebGL backend.
1. `spine-threejs/`, a [THREE.JS](https://threejs.org/) backend, built on the core classes.
1. `spine-phaser-v3/`, a [Phaser v3](https://phaser.io/) backend, built on the core classes.
1. `spine-phaser-v4/`, a [Phaser v4](https://phaser.io/) backend, built on the core classes.
1. `spine-pixi-v7/`, a [PixiJS v7](https://pixijs.com/) backend, built on the core classes.
1. `spine-pixi-v8/`, a [PixiJS v8](https://pixijs.com/) backend, built on the core classes.
1. `spine-construct3/`, the official [Construct 3](https://www.construct.net/) plugin, built on the core classes.

In most cases, the `spine-player` module is best suited for your needs. Please refer to the [Spine Web Player documentation](https://esotericsoftware.com/spine-player) for more information.

For documentation of the core API in `spine-core`, please refer to our [Spine Runtimes Guide](http://esotericsoftware.com/spine-runtimes-guide).

For documentation of `spine-phaser-v3` and `spine-phaser-v4`, please refer to our [spine-phaser Guide](https://esotericsoftware.com/spine-phaser).

For documentation of `spine-pixi-v7` and `spine-pixi-v8`, please refer to our [spine-pixi Guide](https://esotericsoftware.com/spine-pixi).

For documentation of `spine-canvaskit`, please refer to our [spine-canvaskit Guide](https://esotericsoftware.com/spine-canvaskit).

For module specific APIs in `spine-canvas`, `spine-webgl`, and `spine-threejs`, please refer to the [Examples](#examples) in the respecitve `spine-<modulename>/example` folder. For `spine-webgl` specifically, we have provided additional [demos](spine-webgl/demos), which you can also [view online](http://esotericsoftware.com/spine-demos).

## Licensing

You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](http://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](http://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-ts works with data exported from Spine 4.3.xx.

spine-ts Canvas does not support mesh attachments, clipping attachments, or two-color tinting. Only the alpha channel from tint colors is applied. Experimental support for mesh attachments can be enabled by setting `spine.SkeletonRenderer.useTriangleRendering` to true. Note that this experimental mesh rendering is slow and render with artifacts on some browsers.

spine-canvaskit supports all Spine features except two-color tinting.

The spine-webgl and spine-player support all Spine features.

spine-ts THREE.JS does not support two color tinting. The THREE.JS backend provides `SkeletonMesh.zOffset` to avoid z-fighting. Adjust to your near/far plane settings.

## Usage

All spine-ts modules except the Construct 3 plugin are published to [npm](http://npmjs.com) for consumption via vanilla JavaScript as well as via NPM or Yarn. The Construct 3 plugin is packaged as a `.c3addon` and distributed separately from the Esoteric Software servers.

## Usage in vanilla JavaScript

You can include a module in your project via a `<script>` tag from the [unpkg](https://unpkg.com/) CDN, specifying the version as part of the URL. In the examples below, the version is `4.3.*`, which fetches the latest patch release, and which will work with all exports from Spine Editor version `4.3.x`.

```
// spine-core
<script src="https://unpkg.com/@esotericsoftware/spine-core@4.3.*/dist/iife/spine-core.js"></script>

// spine-canvas
<script src="https://unpkg.com/@esotericsoftware/spine-canvas@4.3.*/dist/iife/spine-canvas.js"></script>

// spine-canvaskit
<script src="https://unpkg.com/@esotericsoftware/spine-canvas@4.3.*/dist/iife/spine-canvaskit.js"></script>

// spine-webgl
<script src="https://unpkg.com/@esotericsoftware/spine-webgl@4.3.*/dist/iife/spine-webgl.js"></script>

// spine-player, which requires a spine-player.css as well
<script src="https://unpkg.com/@esotericsoftware/spine-player@4.3.*/dist/iife/spine-player.js"></script>
<link rel="stylesheet" href="https://unpkg.com/@esotericsoftware/spine-player@4.3.*/dist/spine-player.css">

// spine-threejs
<script src="https://unpkg.com/@esotericsoftware/spine-threejs@4.3.*/dist/iife/spine-threejs.js"></script>

// spine-phaser-v3
<script src="https://unpkg.com/@esotericsoftware/spine-phaser-v3@4.3.*/dist/iife/spine-phaser-v3.js"></script>

// spine-phaser-v4
<script src="https://unpkg.com/@esotericsoftware/spine-phaser-v4@4.3.*/dist/iife/spine-phaser-v4.js"></script>

// spine-pixi-v7
<script src="https://unpkg.com/@esotericsoftware/spine-pixi-v7@4.3.*/dist/iife/spine-pixi-v7.js"></script>

// spine-pixi-v8
<script src="https://unpkg.com/@esotericsoftware/spine-pixi-v8@4.3.*/dist/iife/spine-pixi-v8.js"></script>
```

We also provide `js.map` source maps. They will be automatically fetched from unpkg when debugging code of a spine-module in Chrome, Firefox, or Safari, mapping the JavaScript code back to its original TypeScript sources.

We provide minified versions of each module, which can be used by replacing the `.js` file suffix with `.min.js` in the unpkg URLs.

## Usage via NPM or Yarn

If your project dependencies are managed through NPM or Yarn, you can add spine-ts modules the usual way:

```
npm install @esotericsoftware/spine-core
npm install @esotericsoftware/spine-canvas
npm install @esotericsoftware/spine-canvaskit
npm install @esotericsoftware/spine-webgl
npm install @esotericsoftware/spine-player
npm install @esotericsoftware/spine-threejs
npm install @esotericsoftware/spine-phaser
npm install @esotericsoftware/spine-pixi-v7
npm install @esotericsoftware/spine-pixi-v8
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
1. Install [Visual Studio Code](https://code.visualstudio.com/).
1. Open a terminal and execute

```
git clone https://github.com/esotericsoftware/spine-runtimes
cd spine-runtimes/spine-ts
npm install
npm run dev
```

The final command `npm run dev` will start a local web server at http://127.0.0.1:8080, which reloads any page it served automatically in case the source code has changed. The command will also start the build tools in watch mode, meaning they will recompile any source code changes in the background automatically.

You can then open Visual Studio Code to inspect, edit, and debug the source code. We also supply launch configurations to start examples and demos in debug mode, so you can debug them right inside Visual Studio code.

To build the artifacts as they are published to NPM, run `npm run build`.

## Releasing

`spine-ts` is released using GitHub Actions. Public JavaScript runtimes are published to [npm](https://www.npmjs.com/search?q=%40esotericsoftware) and uploaded as a web artifacts zip. The Construct 3 plugin is a private workspace: it is not published to npm, and is instead packaged as `EsotericSoftware_SpineConstruct3.c3addon` and uploaded to the Esoteric Software servers as `spine-construct3.zip` and, for release builds, `spine-construct3-x.y.z.zip`.

The release workflow is triggered by tags from `EsotericSoftware/spine-runtimes` matching `spine-ts-x.y.z`, for example `spine-ts-4.3.8`. The same tag releases both the npm packages and the Construct 3 plugin.

The workflow runs the release helper scripts in this order:

1. `./scripts/build.sh` validates `TS_RELEASE_VERSION`, validates package/addon versions, installs dependencies with `npm ci`, and runs `npm run build`. This build includes `spine-construct3`.
2. `./scripts/deploy.sh` packages and uploads `spine-ts.zip` to the Esoteric Software server. It requires `TS_RELEASE_VERSION` and `TS_UPDATE_URL`; `TS_UPDATE_PATH` is optional and defaults to the major/minor release line, for example `4.3`.
3. `./scripts/deploy-construct3.sh` packages and uploads the Construct 3 artifacts. It requires `TS_RELEASE_VERSION` or `C3_RELEASE_VERSION`, and `C3_UPDATE_URL`; `C3_UPDATE_PATH` is optional and defaults to the same major/minor release line. This script expects `./scripts/build.sh` to have run first and intentionally does not rebuild before npm publishing.
4. The workflow publishes only the explicit public npm workspaces.

For a manual deploy, run the same scripts from the `spine-ts/` folder, for example:

```bash
TS_RELEASE_VERSION=4.3.8 ./scripts/build.sh
TS_RELEASE_VERSION=4.3.8 TS_UPDATE_URL=... ./scripts/deploy.sh
TS_RELEASE_VERSION=4.3.8 C3_UPDATE_URL=... ./scripts/deploy-construct3.sh
```

### One-time npm trusted publishing setup

Before the GitHub Actions workflow can publish to npm, each published package must be configured on npmjs.com to trust this repository workflow. For each `@esotericsoftware` spine-ts package, open the package settings on npmjs.com and configure **Trusted Publisher**:

- Publisher: `GitHub Actions`
- Organization or user: `EsotericSoftware`
- Repository: `spine-runtimes`
- Workflow filename: `spine-ts.yml`
- Environment name: leave empty unless the workflow is later changed to use a GitHub environment
- Allowed actions: `npm publish`

Configure this for:

- `@esotericsoftware/spine-core`
- `@esotericsoftware/spine-canvas`
- `@esotericsoftware/spine-canvaskit`
- `@esotericsoftware/spine-webgl`
- `@esotericsoftware/spine-player`
- `@esotericsoftware/spine-webcomponents`
- `@esotericsoftware/spine-threejs`
- `@esotericsoftware/spine-phaser-v3`
- `@esotericsoftware/spine-phaser-v4`
- `@esotericsoftware/spine-pixi-v7`
- `@esotericsoftware/spine-pixi-v8`

Trusted publishing lets the workflow publish using GitHub OIDC instead of a long-lived npm token. npm also automatically publishes provenance for public packages released this way, which should show the npm provenance/verified badge.

### Publishing npm packages manually

Use the workflow's manual dispatch to publish either one npm workspace or all release artifacts. Select the package and the branch containing the package contents to publish. The optional version input defaults to the version in `spine-ts/package.json` on that branch. Selecting one package skips the web and Construct 3 deployments; selecting `all` performs the complete release workflow, including those deployments.

For example, to publish `@esotericsoftware/spine-phaser-v4` version `4.3.11` from the `4.3` branch:

```bash
gh workflow run spine-ts.yml --ref 4.3 -f package=phaser-v4 -f version=4.3.11
```

Omit `version` to use the current package version:

```bash
gh workflow run spine-ts.yml --ref 4.3 -f package=phaser-v4
```

Valid package inputs are `core`, `canvas`, `canvaskit`, `webgl`, `player`, `webcomponents`, `threejs`, `phaser-v3`, `phaser-v4`, `pixi-v7`, `pixi-v8`, and `all`. The workflow always validates and builds all spine-ts packages. It then publishes only the selected npm workspace, or the complete workspace list when `all` is selected. Since npm package versions are immutable, select an individual package for recovery only when that package version has not already been published.

To manually run the complete release workflow:

```bash
gh workflow run spine-ts.yml --ref 4.3 -f package=all -f version=4.3.11
```

### Release process

The normal release process is performed by running `./scripts/publish.sh` from the `spine-ts/` folder on a release branch such as `4.3`:

```bash
cd spine-ts
./scripts/publish.sh
```

The script increments the patch version, optionally updates `CHANGELOG.md`, updates public workspace package versions, updates the private Construct 3 package/addon versions, refreshes `package-lock.json` in place with `npm install --workspaces`, commits the changes, creates the matching `spine-ts-x.y.z` tag, and pushes the branch and tag.

If doing the same steps manually, from the repository root:

1. Set the release version in the root `spine-ts/package.json`, public workspace `package.json` files, private Construct 3 workspace `package.json` files, and `spine-construct3/src/addon.json`. Public internal `@esotericsoftware/spine-*` dependencies must use the same version:

```json
"version": "4.3.8"
```

2. Refresh `spine-ts/package-lock.json`:

```bash
cd spine-ts
npm install --workspaces
cd ..
```

3. Add a matching entry to `spine-ts/CHANGELOG.md`. If previous commits already documented the changes under `## Unreleased`, usually all you need to do is add the release heading below `## Unreleased` and move those entries under it:

```md
## Unreleased

## 4.3.8 - 2026-06-05

### spine-ts

- Existing unreleased changelog entries...
```

4. Commit and push the release version:

```bash
git add spine-ts/package.json spine-ts/package-lock.json spine-ts/spine-*/package.json spine-ts/spine-construct3/spine-construct3-lib/package.json spine-ts/spine-construct3/src/addon.json spine-ts/CHANGELOG.md
git commit -m "[ts] Release 4.3.8"
git push origin 4.3
```

5. Tag that commit and push the tag:

```bash
git tag spine-ts-4.3.8
git push origin spine-ts-4.3.8
```

The tag triggers the GitHub Actions release workflow. It verifies the tag version matches the public packages and the Construct 3 package/addon versions, builds all packages, uploads `spine-ts.zip` to the Esoteric Software server for the matching release line, for example `4.3`, deploys the Construct 3 plugin zip artifacts to the Construct 3 update server for the same release line, and publishes the public npm workspaces using trusted publishing.

6. Check the workflow result, the uploaded `spine-ts.zip`, the uploaded Construct 3 zip artifacts, and the npm package pages.
