import type { Texture, ColorSource, Renderer, BLEND_MODES } from "@pixi/core";
import { Mesh } from "@pixi/mesh";
import { DarkTintGeometry } from "./DarkTintGeom";
import { DarkTintMaterial } from "./DarkTintMaterial";

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
