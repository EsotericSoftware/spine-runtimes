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

import { Texture, TextureFilter, type TextureWrap } from "@esotericsoftware/spine-core";
import type * as Phaser from "phaser";

/** Spine core texture wrapper backed by a Phaser TextureSource. */
export class PhaserTexture extends Texture {
	private minFilter: TextureFilter | null = null;
	private magFilter: TextureFilter | null = null;
	private uWrap: TextureWrap | null = null;
	private vWrap: TextureWrap | null = null;

	constructor (
		public readonly phaserTexture: Phaser.Textures.Texture,
		public readonly sourceIndex = 0,
		public readonly uploadPremultiplyAlpha = true,
	) {
		super(phaserTexture.getSourceImage(sourceIndex));
		this.configureUploadPremultiplyAlpha();
	}

	get source (): Phaser.Textures.TextureSource {
		const source = this.phaserTexture.source[this.sourceIndex];
		if (!source) throw new Error(`Spine atlas page '${this.phaserTexture.key}' has no Phaser texture source at index ${this.sourceIndex}.`);
		return source;
	}

	get glTexture (): Phaser.Renderer.WebGL.Wrappers.WebGLTextureWrapper | null {
		return this.source.glTexture;
	}

	configureUploadPremultiplyAlpha (): void {
		const source = this.source;
		const glTexture = source.glTexture;
		if (!glTexture) return;

		if (glTexture.pma !== this.uploadPremultiplyAlpha) {
			glTexture.pma = this.uploadPremultiplyAlpha;
			source.update();
			this.applySamplerState(glTexture);
		}
	}

	setFilters (minFilter: TextureFilter, magFilter: TextureFilter): void {
		this.minFilter = minFilter;
		this.magFilter = PhaserTexture.validateMagFilter(magFilter);
		this.applySamplerState(this.glTexture);
	}

	setWraps (uWrap: TextureWrap, vWrap: TextureWrap): void {
		this.uWrap = uWrap;
		this.vWrap = vWrap;
		this.applySamplerState(this.glTexture);
	}

	bind (unit = 0): void {
		const glTexture = this.glTexture;
		if (!glTexture) throw new Error("Cannot bind Phaser-backed Spine texture without a WebGL renderer.");
		glTexture.renderer.glTextureUnits.bind(glTexture, unit);
	}

	unbind (): void {
		const glTexture = this.glTexture;
		if (glTexture) glTexture.renderer.glTextureUnits.unbindTexture(glTexture);
	}

	dispose (): void {
		// Phaser owns the backing texture. Nothing to delete here.
	}

	private applySamplerState (texture: Phaser.Renderer.WebGL.Wrappers.WebGLTextureWrapper | null): void {
		if (!texture) return;

		const renderer = texture.renderer;
		const gl = renderer.gl;
		const textureUnits = renderer.glTextureUnits;
		const previousTexture = textureUnits.units[0];
		const minFilter = this.minFilter ?? texture.minFilter;
		const magFilter = this.magFilter ?? texture.magFilter;
		const wrapS = this.uWrap ?? texture.wrapS;
		const wrapT = this.vWrap ?? texture.wrapT;

		textureUnits.bind(texture, 0);
		texture.minFilter = minFilter;
		texture.magFilter = magFilter;
		texture.wrapS = wrapS;
		texture.wrapT = wrapT;
		gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, minFilter);
		gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, magFilter);
		gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, wrapS);
		gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, wrapT);
		texture.generateMipmap();
		textureUnits.bind(previousTexture, 0);
	}

	private static validateMagFilter (magFilter: TextureFilter): TextureFilter {
		switch (magFilter) {
			case TextureFilter.MipMapLinearLinear:
			case TextureFilter.MipMapLinearNearest:
			case TextureFilter.MipMapNearestLinear:
			case TextureFilter.MipMapNearestNearest:
				return TextureFilter.Linear;
			default:
				return magFilter;
		}
	}
}
