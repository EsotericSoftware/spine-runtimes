/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

module spine.threejs {
	export class MeshBatcher extends THREE.Mesh {
		private static VERTEX_SIZE = 9;
		private vertexBuffer: THREE.InterleavedBuffer;
		private vertices: Float32Array;
		private verticesLength = 0;
		private indices: Uint16Array;
		private indicesLength = 0;

		constructor (maxVertices: number = 10920) {
			super();
			if (maxVertices > 10920) throw new Error("Can't have more than 10920 triangles per batch: " + maxVertices);
			let vertices = this.vertices = new Float32Array(maxVertices * MeshBatcher.VERTEX_SIZE);
			let indices = this.indices = new Uint16Array(maxVertices * 3);
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
			this.geometry = geo;
			this.material = new SkeletonMeshMaterial();
		}

		dispose () {
			this.geometry.dispose();
			this.material.dispose();
		}

		clear () {
			let geo = (<THREE.BufferGeometry>this.geometry);
			geo.drawRange.start = 0;
			geo.drawRange.count = 0;
			(<SkeletonMeshMaterial>this.material).uniforms.map.value = null;
		}

		begin () {
			this.verticesLength = 0;
			this.indicesLength = 0;
		}

		canBatch(verticesLength: number, indicesLength: number) {
			if (this.indicesLength + indicesLength >= this.indices.byteLength / 2) return false;
			if (this.verticesLength + verticesLength >= this.vertices.byteLength / 2) return false;
			return true;
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
			this.vertexBuffer.needsUpdate = this.verticesLength > 0;
			this.vertexBuffer.updateRange.offset = 0;
			this.vertexBuffer.updateRange.count = this.verticesLength;
			let geo = (<THREE.BufferGeometry>this.geometry);
			geo.getIndex().needsUpdate = this.indicesLength > 0;
			geo.getIndex().updateRange.offset = 0;
			geo.getIndex().updateRange.count = this.indicesLength;
			geo.drawRange.start = 0;
			geo.drawRange.count = this.indicesLength;
		}
	}
}
