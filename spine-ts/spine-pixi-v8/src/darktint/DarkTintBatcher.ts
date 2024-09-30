import {
	Batcher,
	Color,
	DefaultBatchableMeshElement,
	DefaultBatchableQuadElement,
	extensions,
	ExtensionType,
	Shader
} from 'pixi.js';
import { DarkTintBatchGeometry } from './DarkTintBatchGeometry';
import { DarkTintShader } from './DarkTintShader';

let defaultShader: Shader | null = null;

/** The default batcher is used to batch quads and meshes. */
export class DarkTintBatcher extends Batcher {
	/** @ignore */
	public static extension = {
		type: [
			ExtensionType.Batcher,
		],
		name: 'darkTint',
	} as const;

	public geometry = new DarkTintBatchGeometry();
	public shader = defaultShader || (defaultShader = new DarkTintShader(this.maxTextures));
	public name = DarkTintBatcher.extension.name;

	/** The size of one attribute. 1 = 32 bit. x, y, u, v, color, darkColor, textureIdAndRound -> total = 7 */
	public vertexSize = 7;

	public packAttributes (
		element: DefaultBatchableMeshElement & { darkColor: number },
		float32View: Float32Array,
		uint32View: Uint32Array,
		index: number,
		textureId: number
	) {
		const textureIdAndRound = (textureId << 16) | (element.roundPixels & 0xFFFF);

		const wt = element.transform;

		const a = wt.a;
		const b = wt.b;
		const c = wt.c;
		const d = wt.d;
		const tx = wt.tx;
		const ty = wt.ty;

		const { positions, uvs } = element;

		const argb = element.color;
		const worldAlpha = ((argb >> 24) & 0xFF) / 255;
		const darkColor = Color.shared.setValue(element.darkColor).premultiply(worldAlpha, true).toPremultiplied(1, false);

		const offset = element.attributeOffset;
		const end = offset + element.attributeSize;

		for (let i = offset; i < end; i++) {
			const i2 = i * 2;

			const x = positions[i2];
			const y = positions[(i2) + 1];

			float32View[index++] = (a * x) + (c * y) + tx;
			float32View[index++] = (d * y) + (b * x) + ty;

			float32View[index++] = uvs[i2];
			float32View[index++] = uvs[(i2) + 1];

			uint32View[index++] = argb;
			uint32View[index++] = darkColor;

			uint32View[index++] = textureIdAndRound;
		}
	}

	public packQuadAttributes (
		element: DefaultBatchableQuadElement & { darkColor: number },
		float32View: Float32Array,
		uint32View: Uint32Array,
		index: number,
		textureId: number
	) {
		const texture = element.texture;

		const wt = element.transform;

		const a = wt.a;
		const b = wt.b;
		const c = wt.c;
		const d = wt.d;
		const tx = wt.tx;
		const ty = wt.ty;

		const bounds = element.bounds;

		const w0 = bounds.maxX;
		const w1 = bounds.minX;
		const h0 = bounds.maxY;
		const h1 = bounds.minY;

		const uvs = texture.uvs;

		// _ _ _ _
		// a b g r
		const argb = element.color;
		const darkColor = element.darkColor;

		const textureIdAndRound = (textureId << 16) | (element.roundPixels & 0xFFFF);

		float32View[index + 0] = (a * w1) + (c * h1) + tx;
		float32View[index + 1] = (d * h1) + (b * w1) + ty;

		float32View[index + 2] = uvs.x0;
		float32View[index + 3] = uvs.y0;

		uint32View[index + 4] = argb;
		uint32View[index + 5] = darkColor;
		uint32View[index + 6] = textureIdAndRound;

		// xy
		float32View[index + 7] = (a * w0) + (c * h1) + tx;
		float32View[index + 8] = (d * h1) + (b * w0) + ty;

		float32View[index + 9] = uvs.x1;
		float32View[index + 10] = uvs.y1;

		uint32View[index + 11] = argb;
		uint32View[index + 12] = darkColor;
		uint32View[index + 13] = textureIdAndRound;

		// xy
		float32View[index + 14] = (a * w0) + (c * h0) + tx;
		float32View[index + 15] = (d * h0) + (b * w0) + ty;

		float32View[index + 16] = uvs.x2;
		float32View[index + 17] = uvs.y2;

		uint32View[index + 18] = argb;
		uint32View[index + 19] = darkColor;
		uint32View[index + 20] = textureIdAndRound;

		// xy
		float32View[index + 21] = (a * w1) + (c * h0) + tx;
		float32View[index + 22] = (d * h0) + (b * w1) + ty;

		float32View[index + 23] = uvs.x3;
		float32View[index + 24] = uvs.y3;

		uint32View[index + 25] = argb;
		uint32View[index + 26] = darkColor;
		uint32View[index + 27] = textureIdAndRound;
	}
}

extensions.add(DarkTintBatcher);
