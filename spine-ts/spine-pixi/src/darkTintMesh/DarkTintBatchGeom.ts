import { Geometry, Buffer, TYPES } from "@pixi/core";

/**
 * Geometry used to batch standard PIXI content (e.g. Mesh, Sprite, Graphics objects).
 * @memberof PIXI
 */
export class DarkTintBatchGeometry extends Geometry {
	// eslint-disable-next-line @typescript-eslint/naming-convention
	public _buffer: Buffer;

	// eslint-disable-next-line @typescript-eslint/naming-convention
	public _indexBuffer: Buffer;

	/**
	 * @param {boolean} [_static=false] - Optimization flag, where `false`
	 *        is updated every frame, `true` doesn't change frame-to-frame.
	 */
	constructor(_static = false) {
		super();

		this._buffer = new Buffer(undefined, _static, false);

		this._indexBuffer = new Buffer(undefined, _static, true);

		this.addAttribute("aVertexPosition", this._buffer, 2, false, TYPES.FLOAT)
			.addAttribute("aTextureCoord", this._buffer, 2, false, TYPES.FLOAT)
			.addAttribute("aColor", this._buffer, 4, true, TYPES.UNSIGNED_BYTE)
			.addAttribute("aDarkColor", this._buffer, 4, true, TYPES.UNSIGNED_BYTE)
			.addAttribute("aTextureId", this._buffer, 1, true, TYPES.FLOAT)
			.addIndex(this._indexBuffer);
	}
}
