/** ****************************************************************************
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

import { AttachmentCacheData, Spine } from './Spine.js';

import type { Batch, Batcher, BLEND_MODES, DefaultBatchableMeshElement, Matrix, Texture, Topology } from 'pixi.js';

export class BatchableSpineSlot implements DefaultBatchableMeshElement {
	indexOffset = 0;
	attributeOffset = 0;

	indexSize!: number;
	attributeSize!: number;

	batcherName = 'darkTint';

	topology: Topology = 'triangle-list';

	readonly packAsQuad = false;

	renderable!: Spine;

	positions!: Float32Array;
	indices!: number[] | Uint16Array;
	uvs!: Float32Array;

	roundPixels!: 0 | 1;
	data!: AttachmentCacheData;
	blendMode!: BLEND_MODES;

	darkTint!: number;

	texture!: Texture;

	transform!: Matrix;

	// used internally by batcher specific. Stored for efficient updating.
	_textureId!: number;
	_attributeStart!: number;
	_indexStart!: number;
	_batcher!: Batcher;
	_batch!: Batch;


	get color () {
		const slotColor = this.data.color;

		const parentColor: number = this.renderable.groupColor;
		const parentAlpha: number = this.renderable.groupAlpha;
		let abgr: number;

		const mixedA = (slotColor.a * parentAlpha) * 255;

		if (parentColor !== 0xFFFFFF) {
			const parentB = (parentColor >> 16) & 0xFF;
			const parentG = (parentColor >> 8) & 0xFF;
			const parentR = parentColor & 0xFF;

			const mixedR = (slotColor.r * parentR);
			const mixedG = (slotColor.g * parentG);
			const mixedB = (slotColor.b * parentB);

			abgr = ((mixedA) << 24) | (mixedB << 16) | (mixedG << 8) | mixedR;
		}
		else {
			abgr = ((mixedA) << 24) | ((slotColor.b * 255) << 16) | ((slotColor.g * 255) << 8) | (slotColor.r * 255);
		}

		return abgr;
	}

	get darkColor () {
		const darkColor = this.data.darkColor;
		return ((darkColor.b * 255) << 16) | ((darkColor.g * 255) << 8) | (darkColor.r * 255);
	}

	get groupTransform () { return this.renderable.groupTransform; }

	setData (
		renderable: Spine,
		data: AttachmentCacheData,
		blendMode: BLEND_MODES,
		roundPixels: 0 | 1) {
		this.renderable = renderable;
		this.transform = renderable.groupTransform;
		this.data = data;

		if (data.clipped) {
			const clippedData = data.clippedData;

			this.indexSize = clippedData!.indicesCount;
			this.attributeSize = clippedData!.vertexCount;
			this.positions = clippedData!.vertices;
			this.indices = clippedData!.indices;
			this.uvs = clippedData!.uvs;
		}
		else {
			this.indexSize = data.indices.length;
			this.attributeSize = data.vertices.length / 2;
			this.positions = data.vertices;
			this.indices = data.indices;
			this.uvs = data.uvs;
		}

		this.texture = data.texture;
		this.roundPixels = roundPixels;

		this.blendMode = blendMode;

		this.batcherName = data.darkTint ? 'darkTint' : 'default';
	}
}
