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

import { BlendMode, Texture, type TextureAtlasPage, TextureFilter, TextureWrap } from "@esotericsoftware/spine-core";

export class C3TextureEditor extends Texture {
	texture: SDK.Gfx.IWebGLTexture;
	renderer: SDK.Gfx.IWebGLRenderer;

	constructor (image: HTMLImageElement | ImageBitmap, renderer: SDK.Gfx.IWebGLRenderer, page: TextureAtlasPage) {
		super(image);
		this.renderer = renderer;
		const options: TextureCreateOptions = {
			wrapX: toC3TextureWrap(page.uWrap),
			wrapY: toC3TextureWrap(page.vWrap),
			sampling: toC3Filter(page.minFilter),
			mipMap: toC3MipMap(page.minFilter),
		}
		this.texture = renderer.CreateDynamicTexture(image.width, image.height, options);
		this.renderer.UpdateTexture(image, this.texture, { premultiplyAlpha: !page.pma });
	}

	setFilters () {
		// cannot change filter after texture creation
	}

	setWraps () {
		// cannot change wraps after texture creation
	}


	dispose () {
		this.renderer.DeleteTexture(this.texture);
	}
}

export class C3Texture extends Texture {
	texture: ITexture;
	renderer: IRenderer;

	constructor (image: HTMLImageElement | ImageBitmap, renderer: IRenderer, page: TextureAtlasPage) {
		super(image);
		this.renderer = renderer;
		const options: TextureCreateOptions = {
			wrapX: toC3TextureWrap(page.uWrap),
			wrapY: toC3TextureWrap(page.vWrap),
			sampling: toC3Filter(page.minFilter),
			mipMap: toC3MipMap(page.minFilter),
		}
		this.texture = renderer.createDynamicTexture(image.width, image.height, options);
		this.renderer.updateTexture(image, this.texture, { premultiplyAlpha: !page.pma });
	}


	setFilters () {
		// cannot change filter after texture creation
	}

	setWraps () {
		// cannot change wraps after texture creation
	}

	dispose () {
		this.renderer.deleteTexture(this.texture);
	}
}

function toC3TextureWrap (wrap: TextureWrap): TextureWrapMode {
	if (wrap === TextureWrap.ClampToEdge) return "clamp-to-edge";
	else if (wrap === TextureWrap.MirroredRepeat) return "mirror-repeat";
	else if (wrap === TextureWrap.Repeat) return "repeat";
	else throw new Error(`Unknown texture wrap: ${wrap}`);
}

function toC3MipMap (filter: TextureFilter): boolean {
	switch (filter) {
		case TextureFilter.MipMap:
		case TextureFilter.MipMapLinearNearest:
		case TextureFilter.MipMapNearestLinear:
		case TextureFilter.MipMapNearestNearest:
			return true;

		case TextureFilter.Linear:
		case TextureFilter.Nearest:
			return false;

		default:
			throw new Error(`Unknown texture filter: ${filter}`);
	}
}

function toC3Filter (filter: TextureFilter): TextureSamplingMode {
	switch (filter) {
		case TextureFilter.Nearest:
		case TextureFilter.MipMapNearestNearest:
			return "nearest";

		case TextureFilter.Linear:
		case TextureFilter.MipMapLinearNearest:
		case TextureFilter.MipMapNearestLinear:
			return "bilinear";

		case TextureFilter.MipMap:
		case TextureFilter.MipMapLinearLinear:
			return "trilinear";
		default:
			throw new Error(`Unknown texture filter: ${filter}`);
	}
}

export const BlendingModeSpineToC3: Record<BlendMode, BlendModeParameter> = {
	[BlendMode.Normal]: "normal",
	[BlendMode.Additive]: "additive",
	[BlendMode.Multiply]: "normal",
	[BlendMode.Screen]: "normal",
}

