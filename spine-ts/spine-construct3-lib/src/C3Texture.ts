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

import { BlendMode, Texture, type TextureFilter, type TextureWrap } from "@esotericsoftware/spine-core";

export class C3TextureEditor extends Texture {
	texture: SDK.Gfx.IWebGLTexture;
	renderer: SDK.Gfx.IWebGLRenderer;

	constructor (image: HTMLImageElement | ImageBitmap, renderer: SDK.Gfx.IWebGLRenderer) {
		super(image);
		this.renderer = renderer;
		this.texture = renderer.CreateDynamicTexture(image.width, image.height);
		this.renderer.UpdateTexture(image, this.texture);
	}

	setFilters (minFilter: TextureFilter, magFilter: TextureFilter) {
	}

	setWraps (uWrap: TextureWrap, vWrap: TextureWrap) {
	}

	dispose () {
		this.renderer.DeleteTexture(this.texture);
	}
}

export class C3Texture extends Texture {
	texture: ITexture;
	renderer: IRenderer;

	constructor (image: HTMLImageElement | ImageBitmap, renderer: IRenderer) {
		super(image);
		this.renderer = renderer;
		this.texture = renderer.createDynamicTexture(image.width, image.height);
		this.renderer.updateTexture(image, this.texture);
	}

	setFilters (minFilter: TextureFilter, magFilter: TextureFilter) {
	}

	setWraps (uWrap: TextureWrap, vWrap: TextureWrap) {
	}

	dispose () {
		this.renderer.deleteTexture(this.texture);
	}
}

export const BlendingModeSpineToC3: Record<BlendMode, BlendModeParameter> = {
	[BlendMode.Normal]: "normal",
	[BlendMode.Additive]: "additive",
	[BlendMode.Multiply]: "normal",
	[BlendMode.Screen]: "normal",
}

