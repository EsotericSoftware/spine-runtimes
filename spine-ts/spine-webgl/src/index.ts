export * from './AssetManager';
export * from './Camera';
export * from './GLTexture';
export * from './Input';
export * from './LoadingScreen';
export * from './Matrix4';
export * from './Mesh';
export * from './PolygonBatcher';
export * from './SceneRenderer';
export * from './Shader';
export * from './ShapeRenderer';
export * from './SkeletonDebugRenderer';
export * from './SkeletonRenderer';
export * from './Vector3';
export * from './WebGL';

export * from "spine-core";

// Before modularization, we would expose spine-core on the global
// `spine` object, and spine-webgl on the global `spine.webgl` object.
// This was used by clients when including spine-webgl via <script src="spine-webgl.js">
// 
// Now with modularization and using esbuild for bundling, we need to emulate this old
// behaviour as to not break old clients.
//
// We pass `--global-name=spine` to esbuild. This will create an object containing
// all exports and assign it to the global variable called `spine`.
//
// That solves half the issue. We also need to assign the exports object to 
// `spine.webgl`. esbuild creates a local variable called `scr_exports` pointing
// to the exports object. We get to it via eval, then assign it to itself, on a new
// property called `webgl`. The client can then access the APIs through `spine` and
// `spine.webgl` as before (with the caveat that both spine-core and spine-webgl are
// now in `spine` and `spine.webgl`).
//
// This will break if esbuild renames the variable `src_exports` pointing to
// the exports object.
declare global {
	var spine: any;
}

let exports = eval("src_exports");
exports.webgl = exports;