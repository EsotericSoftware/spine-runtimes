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

module spine.threejs {
	export class SkeletonMesh extends THREE.Mesh {

		skeleton: Skeleton;
		state: AnimationState;
		zOffset: number = 0.1;

		private batcher: MeshBatcher;

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
			this.batcher = new MeshBatcher(this);			
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
			let batcher = this.batcher;
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
					// FIXME per slot blending would require multiple material support
					//let slotBlendMode = slot.data.blendMode;					
					//if (slotBlendMode != blendMode) {
					//	blendMode = slotBlendMode;
					//	batcher.setBlendMode(getSourceGLBlendMode(this._gl, blendMode, premultipliedAlpha), getDestGLBlendMode(this._gl, blendMode));
					//}
					
					this.batcher.batch(vertices, triangles, z);
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