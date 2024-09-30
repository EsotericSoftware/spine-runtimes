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

import { Texture as PixiTexture } from 'pixi.js';
import { BlendMode, Texture, TextureFilter, TextureWrap } from '@esotericsoftware/spine-core';

import type { BLEND_MODES, SCALE_MODE, TextureSource, WRAP_MODE } from 'pixi.js';

export class SpineTexture extends Texture {
	private static readonly textureMap: Map<TextureSource, SpineTexture> = new Map<TextureSource, SpineTexture>();

	public static from (texture: TextureSource): SpineTexture {
		if (SpineTexture.textureMap.has(texture)) {
			return SpineTexture.textureMap.get(texture) as SpineTexture;
		}

		return new SpineTexture(texture);
	}

	public readonly texture: PixiTexture;

	private constructor (image: TextureSource) {
		// Todo: maybe add error handling if you feed a video texture to spine?
		super(image.resource);
		this.texture = PixiTexture.from(image);
	}

	public setFilters (minFilter: TextureFilter, magFilter: TextureFilter): void {
		const style = this.texture.source.style;

		style.minFilter = SpineTexture.toPixiTextureFilter(minFilter);
		style.magFilter = SpineTexture.toPixiTextureFilter(magFilter);
		this.texture.source.autoGenerateMipmaps = SpineTexture.toPixiMipMap(minFilter);
		this.texture.source.updateMipmaps();
	}

	public setWraps (uWrap: TextureWrap, vWrap: TextureWrap): void {
		const style = this.texture.source.style;

		style.addressModeU = SpineTexture.toPixiTextureWrap(uWrap);
		style.addressModeV = SpineTexture.toPixiTextureWrap(vWrap);
	}

	public dispose (): void {
		// I am not entirely sure about this...
		this.texture.destroy();
	}

	private static toPixiMipMap (filter: TextureFilter): boolean {
		switch (filter) {
			case TextureFilter.Nearest:
			case TextureFilter.Linear:
				return false;

			case TextureFilter.MipMapNearestLinear:
			case TextureFilter.MipMapNearestNearest:
			case TextureFilter.MipMapLinearLinear: // TextureFilter.MipMapLinearLinear == TextureFilter.MipMap
			case TextureFilter.MipMapLinearNearest:
				return true;

			default:
				throw new Error(`Unknown texture filter: ${String(filter)}`);
		}
	}

	private static toPixiTextureFilter (filter: TextureFilter): SCALE_MODE {
		switch (filter) {
			case TextureFilter.Nearest:
			case TextureFilter.MipMapNearestLinear:
			case TextureFilter.MipMapNearestNearest:
				return 'nearest';

			case TextureFilter.Linear:
			case TextureFilter.MipMapLinearLinear: // TextureFilter.MipMapLinearLinear == TextureFilter.MipMap
			case TextureFilter.MipMapLinearNearest:
				return 'linear';

			default:
				throw new Error(`Unknown texture filter: ${String(filter)}`);
		}
	}

	private static toPixiTextureWrap (wrap: TextureWrap): WRAP_MODE {
		switch (wrap) {
			case TextureWrap.ClampToEdge:
				return 'clamp-to-edge';

			case TextureWrap.MirroredRepeat:
				return 'mirror-repeat';

			case TextureWrap.Repeat:
				return 'repeat';

			default:
				throw new Error(`Unknown texture wrap: ${String(wrap)}`);
		}
	}

	public static toPixiBlending (blend: BlendMode): BLEND_MODES {
		switch (blend) {
			case BlendMode.Normal:
				return 'normal';

			case BlendMode.Additive:
				return 'add';

			case BlendMode.Multiply:
				return 'multiply';

			case BlendMode.Screen:
				return 'screen';

			default:
				throw new Error(`Unknown blendMode: ${String(blend)}`);
		}
	}
}
