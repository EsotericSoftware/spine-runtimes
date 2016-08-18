module spine.threejs {
	export class MeshBatcher {
		mesh: THREE.Mesh;

		private static VERTEX_SIZE = 9;
		private _vertexBuffer: THREE.InterleavedBuffer;
		private _vertices: Float32Array;
		private _verticesLength = 0;		
		private _indices: Uint16Array;
		private _indicesLength = 0;

		constructor (mesh: THREE.Mesh, maxVertices: number = 10920) {
			if (maxVertices > 10920) throw new Error("Can't have more than 10920 triangles per batch: " + maxVertices);

			let vertices = this._vertices = new Float32Array(maxVertices * MeshBatcher.VERTEX_SIZE);
			let indices = this._indices = new Uint16Array(maxVertices * 3);
			this.mesh = mesh;			
			let geo = new THREE.BufferGeometry();
			let vertexBuffer = this._vertexBuffer = new THREE.InterleavedBuffer(vertices, MeshBatcher.VERTEX_SIZE);
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
			this._verticesLength = 0;
			this._indicesLength = 0;
		}

		batch (vertices: ArrayLike<number>, indices: ArrayLike<number>, z: number = 0) {
			let indexStart = this._verticesLength / MeshBatcher.VERTEX_SIZE;
			let vertexBuffer = this._vertices;
			let i = this._verticesLength;
			let j = 0;
			for (;j < vertices.length;) {
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
			this._verticesLength = i;			

			let indicesArray = this._indices;
			for (i = this._indicesLength, j = 0; j < indices.length; i++, j++)
				indicesArray[i] = indices[j] + indexStart;
			this._indicesLength += indices.length;			
		}

		end () {
			this._vertexBuffer.needsUpdate = true;
			this._vertexBuffer.updateRange.offset = 0;
			this._vertexBuffer.updateRange.count = this._verticesLength;
			let geo = (<THREE.BufferGeometry>this.mesh.geometry);
			geo.getIndex().needsUpdate = true;
			geo.getIndex().updateRange.offset = 0;
			geo.getIndex().updateRange.count = this._indicesLength;
			geo.drawRange.start = 0;
			geo.drawRange.count = this._indicesLength;				
		}
	}
}