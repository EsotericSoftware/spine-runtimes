/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

import { Texture, Disposable, Restorable, TextureFilter, TextureWrap } from "@esotericsoftware/spine-core";
import { ManagedWebGLRenderingContext } from "./WebGL";

export class GLTexture extends Texture implements Disposable, Restorable {
	context: ManagedWebGLRenderingContext;
	private texture: WebGLTexture | null = null;
	private boundUnit = 0;
	private useMipMaps = false;

	public static DISABLE_UNPACK_PREMULTIPLIED_ALPHA_WEBGL = false;

	constructor (context: ManagedWebGLRenderingContext | WebGLRenderingContext, image: HTMLImageElement | ImageBitmap, useMipMaps: boolean = false) {
		super(image);
		this.context = context instanceof ManagedWebGLRenderingContext ? context : new ManagedWebGLRenderingContext(context);
		this.useMipMaps = useMipMaps;
		this.restore();
		this.context.addRestorable(this);
	}

	setFilters (minFilter: TextureFilter, magFilter: TextureFilter) {
		let gl = this.context.gl;
		this.bind();
		gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, minFilter);
		gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, GLTexture.validateMagFilter(magFilter));
	}

	static validateMagFilter (magFilter: TextureFilter) {
		switch (magFilter) {
			case TextureFilter.MipMap:
			case TextureFilter.MipMapLinearLinear:
			case TextureFilter.MipMapLinearNearest:
			case TextureFilter.MipMapNearestLinear:
			case TextureFilter.MipMapNearestNearest:
				return TextureFilter.Linear;
			default:
				return magFilter;
		}
	}

	setWraps (uWrap: TextureWrap, vWrap: TextureWrap) {
		let gl = this.context.gl;
		this.bind();
		gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, uWrap);
		gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, vWrap);
	}

	update (useMipMaps: boolean) {
		let gl = this.context.gl;
		if (!this.texture) this.texture = this.context.gl.createTexture();
		this.bind();
		if (GLTexture.DISABLE_UNPACK_PREMULTIPLIED_ALPHA_WEBGL) gl.pixelStorei(gl.UNPACK_PREMULTIPLY_ALPHA_WEBGL, false);
		gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, this._image);
		gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
		gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, useMipMaps ? gl.LINEAR_MIPMAP_LINEAR : gl.LINEAR);
		gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
		gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
		if (useMipMaps) gl.generateMipmap(gl.TEXTURE_2D);
	}

	restore () {
		this.texture = null;
		this.update(this.useMipMaps);
	}

	bind (unit: number = 0) {
		let gl = this.context.gl;
		this.boundUnit = unit;
		gl.activeTexture(gl.TEXTURE0 + unit);
		gl.bindTexture(gl.TEXTURE_2D, this.texture);
	}

	unbind () {
		let gl = this.context.gl;
		gl.activeTexture(gl.TEXTURE0 + this.boundUnit);
		gl.bindTexture(gl.TEXTURE_2D, null);
	}

	dispose () {
		this.context.removeRestorable(this);
		let gl = this.context.gl;
		gl.deleteTexture(this.texture);
	}
}
