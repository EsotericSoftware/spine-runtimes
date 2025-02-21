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

import { BlendMode, Texture, TextureFilter, TextureWrap } from "@esotericsoftware/spine-core";
import type { BaseTexture as PixiBaseTexture, BaseImageResource } from "@pixi/core";
import { Texture as PixiTexture, SCALE_MODES, MIPMAP_MODES, WRAP_MODES, BLEND_MODES } from "@pixi/core";

export class SpineTexture extends Texture {
	private static textureMap: Map<PixiBaseTexture, SpineTexture> = new Map<PixiBaseTexture, SpineTexture>();

	public static from (texture: PixiBaseTexture): SpineTexture {
		if (SpineTexture.textureMap.has(texture)) {
			return SpineTexture.textureMap.get(texture)!;
		}
		return new SpineTexture(texture);
	}

	public readonly texture: PixiTexture;

	private constructor (image: PixiBaseTexture) {
		// Todo: maybe add error handling if you feed a video texture to spine?
		super((image.resource as BaseImageResource).source as any);
		this.texture = PixiTexture.from(image);
	}

	public setFilters (minFilter: TextureFilter, _magFilter: TextureFilter): void {
		this.texture.baseTexture.scaleMode = SpineTexture.toPixiTextureFilter(minFilter);
		this.texture.baseTexture.mipmap = SpineTexture.toPixiMipMap(minFilter);

		// pixi only has one filter for both min and mag, too bad
	}

	public setWraps (uWrap: TextureWrap, _vWrap: TextureWrap): void {
		this.texture.baseTexture.wrapMode = SpineTexture.toPixiTextureWrap(uWrap);

		// Pixi only has one setting
	}

	public dispose (): void {
		// I am not entirely sure about this...
		this.texture.destroy();
	}

	private static toPixiTextureFilter (filter: TextureFilter): SCALE_MODES {
		switch (filter) {
			case TextureFilter.Nearest:
			case TextureFilter.MipMapNearestLinear:
			case TextureFilter.MipMapNearestNearest:
				return SCALE_MODES.NEAREST;

			case TextureFilter.Linear:
			case TextureFilter.MipMapLinearLinear: // TextureFilter.MipMapLinearLinear == TextureFilter.MipMap
			case TextureFilter.MipMapLinearNearest:
				return SCALE_MODES.LINEAR;

			default:
				throw new Error(`Unknown texture filter: ${String(filter)}`);
		}
	}

	private static toPixiMipMap (filter: TextureFilter): MIPMAP_MODES {
		switch (filter) {
			case TextureFilter.Nearest:
			case TextureFilter.Linear:
				return MIPMAP_MODES.OFF;

			case TextureFilter.MipMapNearestLinear:
			case TextureFilter.MipMapNearestNearest:
			case TextureFilter.MipMapLinearLinear: // TextureFilter.MipMapLinearLinear == TextureFilter.MipMap
			case TextureFilter.MipMapLinearNearest:
				return MIPMAP_MODES.ON;

			default:
				throw new Error(`Unknown texture filter: ${String(filter)}`);
		}
	}

	private static toPixiTextureWrap (wrap: TextureWrap): WRAP_MODES {
		switch (wrap) {
			case TextureWrap.ClampToEdge:
				return WRAP_MODES.CLAMP;

			case TextureWrap.MirroredRepeat:
				return WRAP_MODES.MIRRORED_REPEAT;

			case TextureWrap.Repeat:
				return WRAP_MODES.REPEAT;

			default:
				throw new Error(`Unknown texture wrap: ${String(wrap)}`);
		}
	}

	public static toPixiBlending (blend: BlendMode): BLEND_MODES {
		switch (blend) {
			case BlendMode.Normal:
				return BLEND_MODES.NORMAL;

			case BlendMode.Additive:
				return BLEND_MODES.ADD;

			case BlendMode.Multiply:
				return BLEND_MODES.MULTIPLY;

			case BlendMode.Screen:
				return BLEND_MODES.SCREEN;

			default:
				throw new Error(`Unknown blendMode: ${String(blend)}`);
		}
	}
}
