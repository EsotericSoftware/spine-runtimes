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

import type { Texture, ColorSource, Renderer, BLEND_MODES } from "@pixi/core";
import { Mesh } from "@pixi/mesh";
import { DarkTintGeometry } from "./DarkTintGeom.js";
import { DarkTintMaterial } from "./DarkTintMaterial.js";

export interface IDarkTintElement {
	// eslint-disable-next-line @typescript-eslint/naming-convention
	_texture: Texture;
	vertexData: Float32Array;
	indices: Uint16Array | Uint32Array | Array<number>;
	uvs: Float32Array;
	worldAlpha: number;
	// eslint-disable-next-line @typescript-eslint/naming-convention
	_tintRGB: number;
	// eslint-disable-next-line @typescript-eslint/naming-convention
	_darkTintRGB: number;
	alpha: number;
	blendMode: BLEND_MODES;
}

export class DarkTintMesh extends Mesh<DarkTintMaterial> {
	// eslint-disable-next-line @typescript-eslint/naming-convention
	public _darkTintRGB: number = 0;

	constructor(texture?: Texture) {
		super(new DarkTintGeometry(), new DarkTintMaterial(texture), undefined, undefined);
	}

	public get darkTint(): ColorSource | null {
		return "darkTint" in this.shader ? (this.shader as unknown as DarkTintMaterial).darkTint : null;
	}

	public set darkTint(value: ColorSource | null) {
		(this.shader as unknown as DarkTintMaterial).darkTint = value!;
	}

	public get darkTintValue(): number {
		return (this.shader as unknown as DarkTintMaterial).darkTintValue;
	}

	// eslint-disable-next-line @typescript-eslint/naming-convention
	protected override _renderToBatch(renderer: Renderer): void {
		const geometry = this.geometry;
		const shader = this.shader;

		if (shader.uvMatrix) {
			shader.uvMatrix.update();
			this.calculateUvs();
		}

		// set properties for batching..
		this.calculateVertices();
		this.indices = geometry.indexBuffer.data as Uint16Array;
		this._tintRGB = shader._tintRGB;
		this._darkTintRGB = shader._darkTintRGB;
		this._texture = shader.texture;

		const pluginName = this.material.pluginName;

		renderer.batch.setObjectRenderer(renderer.plugins[pluginName]);
		renderer.plugins[pluginName].render(this);
	}
}
