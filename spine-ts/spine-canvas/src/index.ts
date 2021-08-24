import { AssetManager } from "./AssetManager";
import { CanvasTexture } from "./CanvasTexture";
import { SkeletonRenderer } from "./SkeletonRenderer";

export * from "./AssetManager";
export * from "./CanvasTexture";
export * from "./SkeletonRenderer";
export * from "../../spine-core/dist/index"

// Needed for compatibility with the old way of how
// spine-canvas worked. We'd set all exported types
// on the global spine.canvas. However, with rollup
// we can only specify a single default object global
// name, which is spine, not spine.canvas. If spine-canvas.js
// is used in vanilla.js, we added a property spine.canvas
// and assign the types of spine-canvas to it. This way
// old code keeps on working.
declare global {
	var spine: any;
}

if (globalThis.spine) {
	globalThis.spine.canvas = {
		AssetManager: AssetManager,
		CanvasTexture: CanvasTexture,
		SkeletonRenderer: SkeletonRenderer
	};
}