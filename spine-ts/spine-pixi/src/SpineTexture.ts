import { BlendMode, Texture, TextureFilter, TextureWrap } from "@esotericsoftware/spine-core";
import type { BaseTexture as PixiBaseTexture, BaseImageResource } from "@pixi/core";
import { Texture as PixiTexture, SCALE_MODES, MIPMAP_MODES, WRAP_MODES, BLEND_MODES } from "@pixi/core";

export class SpineTexture extends Texture {
	private static textureMap: Map<PixiBaseTexture, SpineTexture> = new Map<PixiBaseTexture, SpineTexture>();

	public static from(texture: PixiBaseTexture): SpineTexture {
		if (SpineTexture.textureMap.has(texture)) {
			return SpineTexture.textureMap.get(texture)!;
		}
		return new SpineTexture(texture);
	}

	public readonly texture: PixiTexture;

	private constructor(image: PixiBaseTexture) {
		// Todo: maybe add error handling if you feed a video texture to spine?
		super((image.resource as BaseImageResource).source as any);
		this.texture = PixiTexture.from(image);
	}

	public setFilters(minFilter: TextureFilter, _magFilter: TextureFilter): void {
		this.texture.baseTexture.scaleMode = SpineTexture.toPixiTextureFilter(minFilter);
		this.texture.baseTexture.mipmap = SpineTexture.toPixiMipMap(minFilter);

		// pixi only has one filter for both min and mag, too bad
	}

	public setWraps(uWrap: TextureWrap, _vWrap: TextureWrap): void {
		this.texture.baseTexture.wrapMode = SpineTexture.toPixiTextureWrap(uWrap);

		// Pixi only has one setting
	}

	public dispose(): void {
		// I am not entirely sure about this...
		this.texture.destroy();
	}

	private static toPixiTextureFilter(filter: TextureFilter): SCALE_MODES {
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

	private static toPixiMipMap(filter: TextureFilter): MIPMAP_MODES {
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

	private static toPixiTextureWrap(wrap: TextureWrap): WRAP_MODES {
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

	public static toPixiBlending(blend: BlendMode): BLEND_MODES {
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
