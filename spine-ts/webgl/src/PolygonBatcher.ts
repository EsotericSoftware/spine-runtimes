/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.5
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
	export class PolygonBatcher {
		private _gl: WebGLRenderingContext;
		private _drawCalls: number;
		private _drawing = false;
		private _mesh: Mesh;
		private _shader: Shader = null;
		private _lastTexture: GLTexture = null;
		private _verticesLength = 0;
		private _indicesLength = 0;
		private _srcBlend: number = WebGLRenderingContext.SRC_ALPHA;
		private _dstBlend: number = WebGLRenderingContext.ONE_MINUS_SRC_ALPHA;

		constructor (gl: WebGLRenderingContext, maxVertices: number = 10920) {
			if (maxVertices > 10920) throw new Error("Can't have more than 10920 triangles per batch: " + maxVertices);
			this._gl = gl;
			this._mesh = new Mesh(gl, [new Position2Attribute(), new ColorAttribute(), new TexCoordAttribute()], maxVertices, maxVertices * 3);
		}

		begin (shader: Shader) {
			let gl = this._gl;
			if (this._drawing) throw new Error("PolygonBatch is already drawing. Call PolygonBatch.end() before calling PolygonBatch.begin()");
			this._drawCalls = 0;
			this._shader = shader;
			this._lastTexture = null;
			this._drawing = true;

			gl.enable(gl.BLEND);
			gl.blendFunc(this._srcBlend, this._dstBlend);
		}

		setBlendMode (srcBlend: number, dstBlend: number) {
			let gl = this._gl;
			this._srcBlend = srcBlend;
			this._dstBlend = dstBlend;
			if (this._drawing) {
				this.flush();
				gl.blendFunc(this._srcBlend, this._dstBlend);
			}
		}

		draw (texture: GLTexture, vertices: ArrayLike<number>, indices: Array<number>) {
			if (texture != this._lastTexture) {
				this.flush();
				this._lastTexture = texture;
				texture.bind();
			} else if (this._verticesLength + vertices.length > this._mesh.vertices().length ||
					this._indicesLength + indices.length > this._mesh.indices().length) {
				this.flush();
			}

			let indexStart = this._mesh.numVertices();
			this._mesh.vertices().set(vertices, this._verticesLength);
			this._verticesLength += vertices.length;
			this._mesh.setVerticesLength(this._verticesLength)

			let indicesArray = this._mesh.indices();
			for (let i = this._indicesLength, j = 0; j < indices.length; i++, j++)
				indicesArray[i] = indices[j] + indexStart;
			this._indicesLength += indices.length;
			this._mesh.setIndicesLength(this._indicesLength);
		}

		private flush () {
			let gl = this._gl;
			if (this._verticesLength == 0) return;

			this._mesh.draw(this._shader, gl.TRIANGLES);

			this._verticesLength = 0;
			this._indicesLength = 0;
			this._mesh.setVerticesLength(0);
			this._mesh.setIndicesLength(0);
			this._drawCalls++;
		}

		end () {
			let gl = this._gl;
			if (!this._drawing) throw new Error("PolygonBatch is not drawing. Call PolygonBatch.begin() before calling PolygonBatch.end()");
			if (this._verticesLength > 0 || this._indicesLength > 0) this.flush();
			this._shader = null;
			this._lastTexture = null;
			this._drawing = false;

			gl.disable(gl.BLEND);
		}

		drawCalls () { return this._drawCalls; }
	}
}
