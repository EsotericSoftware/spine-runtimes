module spine.threejs {
	export class SkeletonMesh extends THREE.Mesh {

		skeleton: Skeleton;
		state: AnimationState;

		private _vertexBuffer: THREE.InterleavedBuffer;

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
			geometry.drawRange.start = 0;
			geometry.drawRange.count = 0;
			this._vertexBuffer.needsUpdate = true;			
			geometry.getIndex().needsUpdate = true;
		}
	}
}