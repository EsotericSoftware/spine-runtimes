/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

module spine.webgl {
	export class PolygonBatcher implements Disposable {
		private context: ManagedWebGLRenderingContext;
		private drawCalls: number;
		private isDrawing = false;
		private mesh: Mesh;
		private shader: Shader = null;
		private lastTexture: GLTexture = null;
		private verticesLength = 0;
		private indicesLength = 0;
		private srcBlend: number;
		private dstBlend: number;

		constructor (context: ManagedWebGLRenderingContext | WebGLRenderingContext, twoColorTint: boolean = true, maxVertices: number = 10920) {
			if (maxVertices > 10920) throw new Error("Can't have more than 10920 triangles per batch: " + maxVertices);
			this.context = context instanceof ManagedWebGLRenderingContext? context : new ManagedWebGLRenderingContext(context);
			let attributes = twoColorTint ?
					[new Position2Attribute(), new ColorAttribute(), new TexCoordAttribute(), new Color2Attribute()] :
					[new Position2Attribute(), new ColorAttribute(), new TexCoordAttribute()];
			this.mesh = new Mesh(context, attributes, maxVertices, maxVertices * 3);
			this.srcBlend = this.context.gl.SRC_ALPHA;
			this.dstBlend = this.context.gl.ONE_MINUS_SRC_ALPHA;
		}

		begin (shader: Shader) {
			let gl = this.context.gl;
			if (this.isDrawing) throw new Error("PolygonBatch is already drawing. Call PolygonBatch.end() before calling PolygonBatch.begin()");
			this.drawCalls = 0;
			this.shader = shader;
			this.lastTexture = null;
			this.isDrawing = true;

			gl.enable(gl.BLEND);
			gl.blendFunc(this.srcBlend, this.dstBlend);
		}

		setBlendMode (srcBlend: number, dstBlend: number) {
			let gl = this.context.gl;
			this.srcBlend = srcBlend;
			this.dstBlend = dstBlend;
			if (this.isDrawing) {
				this.flush();
				gl.blendFunc(this.srcBlend, this.dstBlend);
			}
		}

		draw (texture: GLTexture, vertices: ArrayLike<number>, indices: Array<number>) {
			if (texture != this.lastTexture) {
				this.flush();
				this.lastTexture = texture;
			} else if (this.verticesLength + vertices.length > this.mesh.getVertices().length ||
					this.indicesLength + indices.length > this.mesh.getIndices().length) {
				this.flush();
			}

			let indexStart = this.mesh.numVertices();
			this.mesh.getVertices().set(vertices, this.verticesLength);
			this.verticesLength += vertices.length;
			this.mesh.setVerticesLength(this.verticesLength)

			let indicesArray = this.mesh.getIndices();
			for (let i = this.indicesLength, j = 0; j < indices.length; i++, j++)
				indicesArray[i] = indices[j] + indexStart;
			this.indicesLength += indices.length;
			this.mesh.setIndicesLength(this.indicesLength);
		}

		private flush () {
			let gl = this.context.gl;
			if (this.verticesLength == 0) return;

			this.lastTexture.bind();
			this.mesh.draw(this.shader, gl.TRIANGLES);

			this.verticesLength = 0;
			this.indicesLength = 0;
			this.mesh.setVerticesLength(0);
			this.mesh.setIndicesLength(0);
			this.drawCalls++;
		}

		end () {
			let gl = this.context.gl;
			if (!this.isDrawing) throw new Error("PolygonBatch is not drawing. Call PolygonBatch.begin() before calling PolygonBatch.end()");
			if (this.verticesLength > 0 || this.indicesLength > 0) this.flush();
			this.shader = null;
			this.lastTexture = null;
			this.isDrawing = false;

			gl.disable(gl.BLEND);
		}

		getDrawCalls () { return this.drawCalls; }

		dispose () {
			this.mesh.dispose();
		}
	}
}
