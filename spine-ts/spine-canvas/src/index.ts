export * from "./AssetManager";
export * from "./CanvasTexture";
export * from "./SkeletonRenderer";

export * from "spine-core";

// Before modularization, we would expose spine-core on the global
// `spine` object, and spine-canvas on the global `spine.canvas` object.
// This was used by clients when including spine-canvas via <script src="spine-canvas.js">
// 
// Now with modularization and using esbuild for bundling, we need to emulate this old
// behaviour as to not break old clients.
//
// We pass `--global-name=spine` to esbuild. This will create an object containing
// all exports and assign it to the global variable called `spine`.
//
// That solves half the issue. We also need to assign the exports object to 
// `spine.canvas`. esbuild creates a local variable called `scr_exports` pointing
// to the exports object. We get to it via eval, then assign it to itself, on a new
// property called `canvas`. The client can then access the APIs through `spine` and
// `spine.canvas` as before (with the caveat that both spine-core and spine-canvas are
// now in `spine` and `spine.canvas`).
//
// This will break if esbuild renames the variable `src_exports` pointing to
// the exports object.
declare global {
	var spine: any;
}

let exports = eval("src_exports");
exports.canvas = exports;