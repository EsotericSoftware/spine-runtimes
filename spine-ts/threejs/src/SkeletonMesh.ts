module spine.threejs {
	export class SkeletonMesh extends THREE.Mesh {

		skeleton: Skeleton;
		state: AnimationState;

		private _vertexBuffer: THREE.InterleavedBuffer;

		static QUAD_TRIANGLES = [0, 1, 2, 2, 3, 0];

		constructor (skeletonData: SkeletonData) {
			super();

			this.skeleton = new Skeleton(skeletonData);
			let animData = new AnimationStateData(skeletonData);
			this.state = new AnimationState(animData);

			this.material = new THREE.MeshBasicMaterial();
			this.material.vertexColors = THREE.VertexColors;

			let geometry: THREE.BufferGeometry = this.geometry = new THREE.BufferGeometry();
			let vertexBuffer = this._vertexBuffer = new THREE.InterleavedBuffer(new Float32Array(8 * 3 * 10920), 8);
			vertexBuffer.setDynamic(true);
			geometry.addAttribute("position", new THREE.InterleavedBufferAttribute(vertexBuffer, 2, 0, false));
			geometry.addAttribute("color", new THREE.InterleavedBufferAttribute(vertexBuffer, 4, 2, false));
			geometry.addAttribute("uv", new THREE.InterleavedBufferAttribute(vertexBuffer, 2, 6, false));

			let indexBuffer = new Uint16Array(3 * 10920);	
			geometry.setIndex(new THREE.BufferAttribute(indexBuffer, 1));
			geometry.getIndex().dynamic = true;			
			this.update(0);
		}

		update(deltaTime: number) {
			let state = this.state;
			let skeleton = this.skeleton;

			state.update(deltaTime);
			state.apply(skeleton);
			skeleton.updateWorldTransform();

			this.updateGeometry();
		}

		private updateGeometry() {
			let geometry = <THREE.BufferGeometry>this.geometry;
			var numVertices = 0;
			var verticesLength = 0;
			var indicesLength = 0;
			
			let blendMode: BlendMode = null;

			let vertices: ArrayLike<number> = null;
			let triangles: Array<number>  = null;
			let drawOrder = this.skeleton.drawOrder;
			for (let i = 0, n = drawOrder.length; i < n; i++) {
				let slot = drawOrder[i];
				let attachment = slot.getAttachment();
				let texture: ThreeJsTexture = null;
				if (attachment instanceof RegionAttachment) {
					let region = <RegionAttachment>attachment;
					vertices = region.updateWorldVertices(slot, false);
					triangles = SkeletonMesh.QUAD_TRIANGLES;
					texture = <ThreeJsTexture>(<TextureAtlasRegion>region.region.renderObject).texture;

				} else if (attachment instanceof MeshAttachment) {
					let mesh = <MeshAttachment>attachment;
					vertices = mesh.updateWorldVertices(slot, false);
					triangles = mesh.triangles;
					texture = <ThreeJsTexture>(<TextureAtlasRegion>mesh.region.renderObject).texture;
				} else continue;

				if (texture != null) {
					(<THREE.MeshBasicMaterial>this.material).map = texture.texture;
					// FIXME
					//let slotBlendMode = slot.data.blendMode;					
					//if (slotBlendMode != blendMode) {
					//	blendMode = slotBlendMode;
					//	batcher.setBlendMode(getSourceGLBlendMode(this._gl, blendMode, premultipliedAlpha), getDestGLBlendMode(this._gl, blendMode));
					//}
					
					let indexStart = verticesLength / 8;
					(<Float32Array>this._vertexBuffer.array).set(vertices, verticesLength);
					verticesLength += vertices.length;									

					let indicesArray = geometry.getIndex().array;
					for (let i = indicesLength, j = 0; j < triangles.length; i++, j++)
						indicesArray[i] = triangles[j] + indexStart;
					indicesLength += triangles.length;					
				}
			}

			geometry.drawRange.start = 0;
			geometry.drawRange.count = indicesLength;
			this._vertexBuffer.needsUpdate = true;
			this._vertexBuffer.updateRange.offset = 0;
			this._vertexBuffer.updateRange.count = verticesLength;		
			geometry.getIndex().needsUpdate = true;
			geometry.getIndex().updateRange.offset = 0;
			geometry.getIndex().updateRange.count = indicesLength;
		}
	}
}