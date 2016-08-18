module spine.threejs {
	export class SkeletonMesh extends THREE.Mesh {

		skeleton: Skeleton;
		state: AnimationState;
		zOffset: number = 0.1;

		private _batcher: MeshBatcher;

		static QUAD_TRIANGLES = [0, 1, 2, 2, 3, 0];

		constructor (skeletonData: SkeletonData) {
			super();

			this.skeleton = new Skeleton(skeletonData);
			let animData = new AnimationStateData(skeletonData);
			this.state = new AnimationState(animData);

			let material = this.material = new THREE.MeshBasicMaterial();
			material.side = THREE.DoubleSide;
			material.transparent = true;
			material.alphaTest = 0.5;									
			this._batcher = new MeshBatcher(this);			
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
			let batcher = this._batcher;
			batcher.begin();
			let z = 0;
			let zOffset = this.zOffset;
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
					if (!(<THREE.MeshBasicMaterial>this.material).map) {
						let mat = <THREE.MeshBasicMaterial>this.material;						
						mat.map = texture.texture;
						mat.needsUpdate = true;						
					}
					// FIXME
					//let slotBlendMode = slot.data.blendMode;					
					//if (slotBlendMode != blendMode) {
					//	blendMode = slotBlendMode;
					//	batcher.setBlendMode(getSourceGLBlendMode(this._gl, blendMode, premultipliedAlpha), getDestGLBlendMode(this._gl, blendMode));
					//}
					
					this._batcher.batch(vertices, triangles, z);
					z += zOffset;				
				}
			}

			batcher.end();
		}

		static createMesh(map: THREE.Texture) {
			let geo = new THREE.BufferGeometry();
			let vertices = new Float32Array(1024);
			vertices.set([
				-200, -200, 1, 0, 0, 1, 0, 0,
				200, -200, 0, 1, 0, 1, 1, 0,
				200, 200, 0, 0, 1, 1, 1, 1,
				-200, 200, 1, 1, 0, 0.1, 0, 1
			], 0);
			let vb = new THREE.InterleavedBuffer(vertices, 8);
			var positions = new THREE.InterleavedBufferAttribute(vb, 2, 0, false);
			geo.addAttribute("position", positions);
			var colors = new THREE.InterleavedBufferAttribute(vb, 4, 2, false);
			geo.addAttribute("color", colors);
			var uvs = new THREE.InterleavedBufferAttribute(vb, 2, 6, false);
			geo.addAttribute("uv", colors);

			var indices = new Uint16Array(1024);
			indices.set([0, 1, 2, 2, 3, 0], 0);
			geo.setIndex(new THREE.BufferAttribute(indices, 1));
			geo.drawRange.start = 0;
			geo.drawRange.count = 6;

			let mat = new THREE.MeshBasicMaterial();
			mat.vertexColors = THREE.VertexColors;
			mat.transparent = true;
			mat.map = map;		
			let mesh = new THREE.Mesh(geo, mat);
			return mesh; 
		}
	}
}