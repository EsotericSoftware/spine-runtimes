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
	export class SkeletonMesh extends THREE.Mesh {
		tempPos: Vector2 = new Vector2();
		tempUv: Vector2 = new Vector2();
		tempLight = new Color();
		tempDark = new Color();
		skeleton: Skeleton;
		state: AnimationState;
		zOffset: number = 0.1;
		vertexEffect: VertexEffect;

		private batcher: MeshBatcher;
		private clipper: SkeletonClipping = new SkeletonClipping();

		static QUAD_TRIANGLES = [0, 1, 2, 2, 3, 0];
		static VERTEX_SIZE = 2 + 2 + 4;

		private vertices = Utils.newFloatArray(1024);
		private tempColor = new Color();

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
			let tempPos = this.tempPos;
			let tempUv = this.tempUv;
			let tempLight = this.tempLight;
			let tempDark = this.tempDark;

			let geometry = <THREE.BufferGeometry>this.geometry;
			var numVertices = 0;
			var verticesLength = 0;
			var indicesLength = 0;

			let blendMode: BlendMode = null;
			let clipper = this.clipper;

			let vertices: ArrayLike<number> = this.vertices;
			let triangles: Array<number> = null;
			let uvs: ArrayLike<number> = null;
			let drawOrder = this.skeleton.drawOrder;
			let batcher = this.batcher;
			batcher.begin();
			let z = 0;
			let zOffset = this.zOffset;
			for (let i = 0, n = drawOrder.length; i < n; i++) {
				let vertexSize = clipper.isClipping() ? 2 : SkeletonMesh.VERTEX_SIZE;
				let slot = drawOrder[i];
				let attachment = slot.getAttachment();
				let attachmentColor: Color = null;
				let texture: ThreeJsTexture = null;
				let numFloats = 0;
				if (attachment instanceof RegionAttachment) {
					let region = <RegionAttachment>attachment;
					attachmentColor = region.color;
					vertices = this.vertices;
					numFloats = vertexSize * 4;
					region.computeWorldVertices(slot.bone, vertices, 0, vertexSize);
					triangles = SkeletonMesh.QUAD_TRIANGLES;
					uvs = region.uvs;
					texture = <ThreeJsTexture>(<TextureAtlasRegion>region.region.renderObject).texture;
				} else if (attachment instanceof MeshAttachment) {
					let mesh = <MeshAttachment>attachment;
					attachmentColor = mesh.color;
					vertices = this.vertices;
					numFloats = (mesh.worldVerticesLength >> 1) * vertexSize;
					if (numFloats > vertices.length) {
						vertices = this.vertices = spine.Utils.newFloatArray(numFloats);
					}
					mesh.computeWorldVertices(slot, 0, mesh.worldVerticesLength, vertices, 0, vertexSize);
					triangles = mesh.triangles;
					uvs = mesh.uvs;
					texture = <ThreeJsTexture>(<TextureAtlasRegion>mesh.region.renderObject).texture;
				} else if (attachment instanceof ClippingAttachment) {
					let clip = <ClippingAttachment>(attachment);
					clipper.clipStart(slot, clip);
					continue;
				} else continue;

				if (texture != null) {
					if (!(<THREE.MeshBasicMaterial>this.material).map) {
						let mat = <THREE.MeshBasicMaterial>this.material;
						mat.map = texture.texture;
						mat.needsUpdate = true;
					}

					let skeleton = slot.bone.skeleton;
					let skeletonColor = skeleton.color;
					let slotColor = slot.color;
					let alpha = skeletonColor.a * slotColor.a * attachmentColor.a;
					let color = this.tempColor;
					color.set(skeletonColor.r * slotColor.r * attachmentColor.r,
							skeletonColor.g * slotColor.g * attachmentColor.g,
							skeletonColor.b * slotColor.b * attachmentColor.b,
							alpha);
					// FIXME per slot blending would require multiple material support
					//let slotBlendMode = slot.data.blendMode;
					//if (slotBlendMode != blendMode) {
					//	blendMode = slotBlendMode;
					//	batcher.setBlendMode(getSourceGLBlendMode(this._gl, blendMode, premultipliedAlpha), getDestGLBlendMode(this._gl, blendMode));
					//}

					if (clipper.isClipping()) {
						clipper.clipTriangles(vertices, numFloats, triangles, triangles.length, uvs, color, null, false);
						let clippedVertices = clipper.clippedVertices;
						let clippedTriangles = clipper.clippedTriangles;
						if (this.vertexEffect != null) {
							let vertexEffect = this.vertexEffect;
							let verts = clippedVertices;
							for (let v = 0, n = clippedVertices.length; v < n; v += vertexSize) {
								tempPos.x = verts[v];
								tempPos.y = verts[v + 1];
								tempLight.setFromColor(color);
								tempDark.set(0, 0, 0, 0);
								tempUv.x = verts[v + 6];
								tempUv.y = verts[v + 7];
								vertexEffect.transform(tempPos, tempUv, tempLight, tempDark);
								verts[v] = tempPos.x;
								verts[v + 1] = tempPos.y;
								verts[v + 2] = tempLight.r;
								verts[v + 3] = tempLight.g;
								verts[v + 4] = tempLight.b;
								verts[v + 5] = tempLight.a;
								verts[v + 6] = tempUv.x;
								verts[v + 7] = tempUv.y;
							}
						}
						batcher.batch(clippedVertices, clippedVertices.length, clippedTriangles, clippedTriangles.length, z);
					} else {
						let verts = vertices;
						if (this.vertexEffect != null) {
							let vertexEffect = this.vertexEffect;
							for (let v = 0, u = 0, n = numFloats; v < n; v += vertexSize, u += 2) {
								tempPos.x = verts[v];
								tempPos.y = verts[v + 1];
								tempLight.setFromColor(color);
								tempDark.set(0, 0, 0, 0);
								tempUv.x = uvs[u];
								tempUv.y = uvs[u + 1];
								vertexEffect.transform(tempPos, tempUv, tempLight, tempDark);
								verts[v] = tempPos.x;
								verts[v + 1] = tempPos.y;
								verts[v + 2] = tempLight.r;
								verts[v + 3] = tempLight.g;
								verts[v + 4] = tempLight.b;
								verts[v + 5] = tempLight.a;
								verts[v + 6] = tempUv.x;
								verts[v + 7] = tempUv.y;
							}
						} else {
							for (let v = 2, u = 0, n = numFloats; v < n; v += vertexSize, u += 2) {
								verts[v] = color.r;
								verts[v + 1] = color.g;
								verts[v + 2] = color.b;
								verts[v + 3] = color.a;
								verts[v + 4] = uvs[u];
								verts[v + 5] = uvs[u + 1];
							}
						}
						batcher.batch(vertices, numFloats, triangles, triangles.length, z);
					}
					z += zOffset;
				}
			}

			batcher.end();
		}
	}
}
