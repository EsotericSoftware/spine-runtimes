export * from './require-shim';
export * from './AssetManager';
export * from './MeshBatcher';
export * from './SkeletonMesh';
export * from './ThreeJsTexture';

export * from "@esotericsoftware/spine-core";

// Before modularization, we would expose spine-core on the global
// `spine` object, and spine-threejs on the global `spine.threejs` object.
// This was used by clients when including spine-threejs via <script src="spine-threejs.js">
// 
// Now with modularization and using esbuild for bundling, we need to emulate this old
// behaviour as to not break old clients.
//
// We pass `--global-name=spine` to esbuild. This will create an object containing
// all exports and assign it to the global variable called `spine`.
//
// That solves half the issue. We also need to assign the exports object to 
// `spine.threejs`. esbuild creates a local variable called `scr_exports` pointing
// to the exports object. We get to it via eval, then assign it to itself, on a new
// property called `threejs`. The client can then access the APIs through `spine` and
// `spine.threejs` as before (with the caveat that both spine-core and spine-threejs are
// now in `spine` and `spine.threejs`).
//
// This will break if esbuild renames the variable `src_exports` pointing to
// the exports object.
let exports = eval("src_exports");
if (exports) exports.threejs = exports;