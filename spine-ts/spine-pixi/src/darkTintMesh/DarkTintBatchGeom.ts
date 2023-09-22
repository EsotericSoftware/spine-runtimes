/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

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
