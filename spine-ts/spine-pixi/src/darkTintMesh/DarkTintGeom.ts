import { Geometry, Buffer, TYPES } from "@pixi/core";

/**
 * Geometry used to batch standard PIXI content (e.g. Mesh, Sprite, Graphics objects).
 * @memberof PIXI
 */
export class DarkTintGeometry extends Geometry {
	/**
	 * @param {boolean} [_static=false] - Optimization flag, where `false`
	 *        is updated every frame, `true` doesn't change frame-to-frame.
	 */
	constructor(_static = false) {
		super();

		const verticesBuffer = new Buffer(undefined);
		const uvsBuffer = new Buffer(undefined, true);
		const indexBuffer = new Buffer(undefined, true, true);

		this.addAttribute("aVertexPosition", verticesBuffer, 2, false, TYPES.FLOAT);
		this.addAttribute("aTextureCoord", uvsBuffer, 2, false, TYPES.FLOAT);
		this.addIndex(indexBuffer);
	}
}
