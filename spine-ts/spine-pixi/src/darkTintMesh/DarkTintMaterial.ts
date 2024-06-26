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

import type { ColorSource } from "@pixi/core";
import { Shader, TextureMatrix, Color, Texture, Matrix, Program } from "@pixi/core";

const vertex = `
attribute vec2 aVertexPosition;
attribute vec2 aTextureCoord;

uniform mat3 projectionMatrix;
uniform mat3 translationMatrix;
uniform mat3 uTextureMatrix;

varying vec2 vTextureCoord;

void main(void)
{
    gl_Position = vec4((projectionMatrix * translationMatrix * vec3(aVertexPosition, 1.0)).xy, 0.0, 1.0);

    vTextureCoord = (uTextureMatrix * vec3(aTextureCoord, 1.0)).xy;
}
`;

const fragment = `
varying vec2 vTextureCoord;
uniform vec4 uColor;
uniform vec4 uDarkColor;

uniform sampler2D uSampler;

void main(void)
{
	vec4 texColor = texture2D(uSampler, vTextureCoord);
    gl_FragColor.a = texColor.a * uColor.a;
    gl_FragColor.rgb = ((texColor.a - 1.0) * uDarkColor.a + 1.0 - texColor.rgb) * uDarkColor.rgb + texColor.rgb * uColor.rgb;
}
`;

export interface IDarkTintMaterialOptions {
	alpha?: number;
	tint?: ColorSource;
	darkTint?: ColorSource;
	pluginName?: string;
	uniforms?: Record<string, unknown>;
}

export class DarkTintMaterial extends Shader {
	public readonly uvMatrix: TextureMatrix;

	public batchable: boolean;

	public pluginName: string;

	// eslint-disable-next-line @typescript-eslint/naming-convention
	public _tintRGB: number;
	// eslint-disable-next-line @typescript-eslint/naming-convention
	public _darkTintRGB: number;

	/**
	 * Only do update if tint or alpha changes.
	 * @private
	 * @default false
	 */
	private _colorDirty: boolean;
	private _alpha: number;

	private _tintColor: Color;
	private _darkTintColor: Color;

	constructor(texture?: Texture) {
		const uniforms = {
			uSampler: texture ?? Texture.EMPTY,
			alpha: 1,
			uTextureMatrix: Matrix.IDENTITY,
			uColor: new Float32Array([1, 1, 1, 1]),
			uDarkColor: new Float32Array([0, 0, 0, 0]),
		};

		// Set defaults
		const options = {
			tint: 0xffffff,
			darkTint: 0x0,
			alpha: 1,
			pluginName: "darkTintBatch",
		};

		super(Program.from(vertex, fragment), uniforms);

		this._colorDirty = false;

		this.uvMatrix = new TextureMatrix(uniforms.uSampler);
		this.batchable = true;
		this.pluginName = options.pluginName;

		this._tintColor = new Color(options.tint);
		this._darkTintColor = new Color(options.darkTint);
		this._tintRGB = this._tintColor.toLittleEndianNumber();
		this._darkTintRGB = this._darkTintColor.toLittleEndianNumber();
		this._alpha = options.alpha;
		this._colorDirty = true;
	}

	public get texture(): Texture {
		return this.uniforms.uSampler;
	}
	public set texture(value: Texture) {
		if (this.uniforms.uSampler !== value) {
			if (!this.uniforms.uSampler.baseTexture.alphaMode !== !value.baseTexture.alphaMode) {
				this._colorDirty = true;
			}

			this.uniforms.uSampler = value;
			this.uvMatrix.texture = value;
		}
	}

	public set alpha(value: number) {
		if (value === this._alpha) {
			return;
		}

		this._alpha = value;
		this._colorDirty = true;
	}
	public get alpha(): number {
		return this._alpha;
	}

	public set tint(value: ColorSource) {
		if (value === this.tint) {
			return;
		}

		this._tintColor.setValue(value);
		this._tintRGB = this._tintColor.toLittleEndianNumber();
		this._colorDirty = true;
	}
	public get tint(): ColorSource {
		return this._tintColor.value!;
	}

	public set darkTint(value: ColorSource) {
		if (value === this.darkTint) {
			return;
		}

		this._darkTintColor.setValue(value);
		this._darkTintRGB = this._darkTintColor.toLittleEndianNumber();
		this._colorDirty = true;
	}
	public get darkTint(): ColorSource {
		return this._darkTintColor.value!;
	}

	public get tintValue(): number {
		return this._tintColor.toNumber();
	}

	public get darkTintValue(): number {
		return this._darkTintColor.toNumber();
	}

	/** Gets called automatically by the Mesh. Intended to be overridden for custom {@link PIXI.MeshMaterial} objects. */
	public update(): void {
		if (this._colorDirty) {
			this._colorDirty = false;
			Color.shared.setValue(this._tintColor).premultiply(this._alpha, true).toArray(this.uniforms.uColor);
			Color.shared.setValue(this._darkTintColor).premultiply(this._alpha, true).premultiply(1, false).toArray(this.uniforms.uDarkColor);
		}
		if (this.uvMatrix.update()) {
			this.uniforms.uTextureMatrix = this.uvMatrix.mapCoord;
		}
	}
}
