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

module spine.threejs {
	export class MeshBatcher {
		mesh: THREE.Mesh;

		private static VERTEX_SIZE = 9;
		private vertexBuffer: THREE.InterleavedBuffer;
		private vertices: Float32Array;
		private verticesLength = 0;
		private indices: Uint16Array;
		private indicesLength = 0;

		constructor (mesh: THREE.Mesh, maxVertices: number = 10920) {
			if (maxVertices > 10920) throw new Error("Can't have more than 10920 triangles per batch: " + maxVertices);

			let vertices = this.vertices = new Float32Array(maxVertices * MeshBatcher.VERTEX_SIZE);
			let indices = this.indices = new Uint16Array(maxVertices * 3);
			this.mesh = mesh;
			let geo = new THREE.BufferGeometry();
			let vertexBuffer = this.vertexBuffer = new THREE.InterleavedBuffer(vertices, MeshBatcher.VERTEX_SIZE);
			vertexBuffer.dynamic = true;
			geo.addAttribute("position", new THREE.InterleavedBufferAttribute(vertexBuffer, 3, 0, false));
			geo.addAttribute("color", new THREE.InterleavedBufferAttribute(vertexBuffer, 4, 3, false));
			geo.addAttribute("uv", new THREE.InterleavedBufferAttribute(vertexBuffer, 2, 7, false));
			geo.setIndex(new THREE.BufferAttribute(indices, 1));
			geo.getIndex().dynamic = true;
			geo.drawRange.start = 0;
			geo.drawRange.count = 0;
			mesh.geometry = geo;
		}

		begin () {
			this.verticesLength = 0;
			this.indicesLength = 0;
		}

		batch (vertices: ArrayLike<number>, verticesLength: number, indices: ArrayLike<number>, indicesLength: number, z: number = 0) {
			let indexStart = this.verticesLength / MeshBatcher.VERTEX_SIZE;
			let vertexBuffer = this.vertices;
			let i = this.verticesLength;
			let j = 0;
			for (;j < verticesLength;) {
				vertexBuffer[i++] = vertices[j++];
				vertexBuffer[i++] = vertices[j++];
				vertexBuffer[i++] = z;
				vertexBuffer[i++] = vertices[j++];
				vertexBuffer[i++] = vertices[j++];
				vertexBuffer[i++] = vertices[j++];
				vertexBuffer[i++] = vertices[j++];
				vertexBuffer[i++] = vertices[j++];
				vertexBuffer[i++] = vertices[j++];
			}
			this.verticesLength = i;

			let indicesArray = this.indices;
			for (i = this.indicesLength, j = 0; j < indicesLength; i++, j++)
				indicesArray[i] = indices[j] + indexStart;
			this.indicesLength += indicesLength;
		}

		end () {
			this.vertexBuffer.needsUpdate = true;
			this.vertexBuffer.updateRange.offset = 0;
			this.vertexBuffer.updateRange.count = this.verticesLength;
			let geo = (<THREE.BufferGeometry>this.mesh.geometry);
			geo.getIndex().needsUpdate = true;
			geo.getIndex().updateRange.offset = 0;
			geo.getIndex().updateRange.count = this.indicesLength;
			geo.drawRange.start = 0;
			geo.drawRange.count = this.indicesLength;
		}
	}
}
