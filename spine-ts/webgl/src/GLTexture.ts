/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

module spine.webgl {
	export class GLTexture extends Texture implements Disposable, Restorable {
		private context: ManagedWebGLRenderingContext;
		private texture: WebGLTexture = null;
		private boundUnit = 0;
		private useMipMaps = false;

		constructor (context: ManagedWebGLRenderingContext | WebGLRenderingContext, image: HTMLImageElement, useMipMaps: boolean = false) {
			super(image);
			this.context = context instanceof ManagedWebGLRenderingContext? context : new ManagedWebGLRenderingContext(context);
			this.useMipMaps = useMipMaps;
			this.restore();
			this.context.addRestorable(this);
		}

		setFilters (minFilter: TextureFilter, magFilter: TextureFilter) {
			let gl = this.context.gl;
			this.bind();
			gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, minFilter);
			gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, magFilter);
		}

		setWraps (uWrap: TextureWrap, vWrap: TextureWrap) {
			let gl = this.context.gl;
			this.bind();
			gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, uWrap);
			gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, vWrap);
		}

		update (useMipMaps: boolean) {
			let gl = this.context.gl;
			if (!this.texture) {
				this.texture = this.context.gl.createTexture();
			}
			this.bind();
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
}
