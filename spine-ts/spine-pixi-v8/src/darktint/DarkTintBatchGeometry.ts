/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

import { Buffer, BufferUsage, Geometry } from 'pixi.js';

const placeHolderBufferData = new Float32Array(1);
const placeHolderIndexData = new Uint32Array(1);

export class DarkTintBatchGeometry extends Geometry {
	constructor () {
		const vertexSize = 7;

		const attributeBuffer = new Buffer({
			data: placeHolderBufferData,
			label: 'attribute-batch-buffer',
			usage: BufferUsage.VERTEX | BufferUsage.COPY_DST,
			shrinkToFit: false,
		});

		const indexBuffer = new Buffer({
			data: placeHolderIndexData,
			label: 'index-batch-buffer',
			usage: BufferUsage.INDEX | BufferUsage.COPY_DST, // | BufferUsage.STATIC,
			shrinkToFit: false,
		});

		const stride = vertexSize * 4;

		super({
			attributes: {
				aPosition: {
					buffer: attributeBuffer,
					format: 'float32x2',
					stride,
					offset: 0,
				},
				aUV: {
					buffer: attributeBuffer,
					format: 'float32x2',
					stride,
					offset: 2 * 4,
				},
				aColor: {
					buffer: attributeBuffer,
					format: 'unorm8x4',
					stride,
					offset: 4 * 4,
				},
				aDarkColor: {
					buffer: attributeBuffer,
					format: 'unorm8x4',
					stride,
					offset: 5 * 4,
				},
				aTextureIdAndRound: {
					buffer: attributeBuffer,
					format: 'uint16x2',
					stride,
					offset: 6 * 4,
				},
			},
			indexBuffer
		});
	}
}
