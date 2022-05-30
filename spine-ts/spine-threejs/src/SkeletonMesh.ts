/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

import { AnimationState, AnimationStateData, BlendMode, ClippingAttachment, Color, MeshAttachment, NumberArrayLike, RegionAttachment, Skeleton, SkeletonClipping, SkeletonData, TextureAtlasRegion, Utils, Vector2 } from "@esotericsoftware/spine-core";
import { MeshBatcher } from "./MeshBatcher";
import * as THREE from "three";
import { ThreeJsTexture } from "./ThreeJsTexture";

export type SkeletonMeshMaterialParametersCustomizer = (materialParameters: THREE.ShaderMaterialParameters) => void;

export class SkeletonMeshMaterial extends THREE.ShaderMaterial {
	constructor (customizer: SkeletonMeshMaterialParametersCustomizer) {
		let vertexShader = `
			attribute vec4 color;
			varying vec2 vUv;
			varying vec4 vColor;
			void main() {
				vUv = uv;
				vColor = color;
				gl_Position = projectionMatrix*modelViewMatrix*vec4(position,1.0);
			}
		`;
		let fragmentShader = `
			uniform sampler2D map;
			#ifdef USE_SPINE_ALPHATEST
			uniform float alphaTest;
			#endif
			varying vec2 vUv;
			varying vec4 vColor;
			void main(void) {
				gl_FragColor = texture2D(map, vUv)*vColor;
				#ifdef USE_SPINE_ALPHATEST
				if (gl_FragColor.a < alphaTest) discard;
				#endif
			}
		`;

		let parameters: THREE.ShaderMaterialParameters = {
			uniforms: {
				map: { value: null },
			},
			vertexShader: vertexShader,
			fragmentShader: fragmentShader,
			side: THREE.DoubleSide,
			transparent: true,
			depthWrite: false,
			alphaTest: 0.0
		};
		customizer(parameters);
		if (parameters.alphaTest && parameters.alphaTest > 0) {
			parameters.defines = { "USE_SPINE_ALPHATEST": 1 };
			if (!parameters.uniforms) parameters.uniforms = {};
			parameters.uniforms["alphaTest"] = { value: parameters.alphaTest };
		}
		super(parameters);
	};
}

export class SkeletonMesh extends THREE.Object3D {
	tempPos: Vector2 = new Vector2();
	tempUv: Vector2 = new Vector2();
	tempLight = new Color();
	tempDark = new Color();
	skeleton: Skeleton;
	state: AnimationState;
	zOffset: number = 0.1;

	private batches = new Array<MeshBatcher>();
	private nextBatchIndex = 0;
	private clipper: SkeletonClipping = new SkeletonClipping();

	static QUAD_TRIANGLES = [0, 1, 2, 2, 3, 0];
	static VERTEX_SIZE = 2 + 2 + 4;

	private vertices = Utils.newFloatArray(1024);
	private tempColor = new Color();

	constructor (skeletonData: SkeletonData, private materialCustomerizer: SkeletonMeshMaterialParametersCustomizer = (material) => { }) {
		super();

		this.skeleton = new Skeleton(skeletonData);
		let animData = new AnimationStateData(skeletonData);
		this.state = new AnimationState(animData);
	}

	update (deltaTime: number) {
		let state = this.state;
		let skeleton = this.skeleton;

		state.update(deltaTime);
		state.apply(skeleton);
		skeleton.updateWorldTransform();

		this.updateGeometry();
	}

	dispose () {
		for (var i = 0; i < this.batches.length; i++) {
			this.batches[i].dispose();
		}
	}

	private clearBatches () {
		for (var i = 0; i < this.batches.length; i++) {
			this.batches[i].clear();
			this.batches[i].visible = false;
		}
		this.nextBatchIndex = 0;
	}

	private nextBatch () {
		if (this.batches.length == this.nextBatchIndex) {
			let batch = new MeshBatcher(10920, this.materialCustomerizer);
			this.add(batch);
			this.batches.push(batch);
		}
		let batch = this.batches[this.nextBatchIndex++];
		batch.visible = true;
		return batch;
	}

	private updateGeometry () {
		this.clearBatches();

		let tempPos = this.tempPos;
		let tempUv = this.tempUv;
		let tempLight = this.tempLight;
		let tempDark = this.tempDark;
		let clipper = this.clipper;

		let vertices: NumberArrayLike = this.vertices;
		let triangles: Array<number> | null = null;
		let uvs: NumberArrayLike | null = null;
		let drawOrder = this.skeleton.drawOrder;
		let batch = this.nextBatch();
		batch.begin();
		let z = 0;
		let zOffset = this.zOffset;
		for (let i = 0, n = drawOrder.length; i < n; i++) {
			let vertexSize = clipper.isClipping() ? 2 : SkeletonMesh.VERTEX_SIZE;
			let slot = drawOrder[i];
			if (!slot.bone.active) {
				clipper.clipEndWithSlot(slot);
				continue;
			}
			let attachment = slot.getAttachment();
			let attachmentColor: Color | null;
			let texture: ThreeJsTexture | null;
			let numFloats = 0;
			if (attachment instanceof RegionAttachment) {
				let region = <RegionAttachment>attachment;
				attachmentColor = region.color;
				vertices = this.vertices;
				numFloats = vertexSize * 4;
				region.computeWorldVertices(slot, vertices, 0, vertexSize);
				triangles = SkeletonMesh.QUAD_TRIANGLES;
				uvs = region.uvs;
				texture = <ThreeJsTexture>(<TextureAtlasRegion>region.region!.renderObject).page.texture;
			} else if (attachment instanceof MeshAttachment) {
				let mesh = <MeshAttachment>attachment;
				attachmentColor = mesh.color;
				vertices = this.vertices;
				numFloats = (mesh.worldVerticesLength >> 1) * vertexSize;
				if (numFloats > vertices.length) {
					vertices = this.vertices = Utils.newFloatArray(numFloats);
				}
				mesh.computeWorldVertices(slot, 0, mesh.worldVerticesLength, vertices, 0, vertexSize);
				triangles = mesh.triangles;
				uvs = mesh.uvs;
				texture = <ThreeJsTexture>(<TextureAtlasRegion>mesh.region!.renderObject).page.texture;
			} else if (attachment instanceof ClippingAttachment) {
				let clip = <ClippingAttachment>(attachment);
				clipper.clipStart(slot, clip);
				continue;
			} else {
				clipper.clipEndWithSlot(slot);
				continue;
			}

			if (texture != null) {
				let skeleton = slot.bone.skeleton;
				let skeletonColor = skeleton.color;
				let slotColor = slot.color;
				let alpha = skeletonColor.a * slotColor.a * attachmentColor.a;
				let color = this.tempColor;
				color.set(skeletonColor.r * slotColor.r * attachmentColor.r,
					skeletonColor.g * slotColor.g * attachmentColor.g,
					skeletonColor.b * slotColor.b * attachmentColor.b,
					alpha);

				let finalVertices: NumberArrayLike;
				let finalVerticesLength: number;
				let finalIndices: NumberArrayLike;
				let finalIndicesLength: number;

				if (clipper.isClipping()) {
					clipper.clipTriangles(vertices, numFloats, triangles, triangles.length, uvs, color, tempLight, false);
					let clippedVertices = clipper.clippedVertices;
					let clippedTriangles = clipper.clippedTriangles;
					finalVertices = clippedVertices;
					finalVerticesLength = clippedVertices.length;
					finalIndices = clippedTriangles;
					finalIndicesLength = clippedTriangles.length;
				} else {
					let verts = vertices;
					for (let v = 2, u = 0, n = numFloats; v < n; v += vertexSize, u += 2) {
						verts[v] = color.r;
						verts[v + 1] = color.g;
						verts[v + 2] = color.b;
						verts[v + 3] = color.a;
						verts[v + 4] = uvs[u];
						verts[v + 5] = uvs[u + 1];
					}
					finalVertices = vertices;
					finalVerticesLength = numFloats;
					finalIndices = triangles;
					finalIndicesLength = triangles.length;
				}

				if (finalVerticesLength == 0 || finalIndicesLength == 0) {
					clipper.clipEndWithSlot(slot);
					continue;
				}

				// Start new batch if this one can't hold vertices/indices
				if (!batch.canBatch(finalVerticesLength, finalIndicesLength)) {
					batch.end();
					batch = this.nextBatch();
					batch.begin();
				}

				const slotBlendMode = slot.data.blendMode;
				const slotTexture = texture.texture;
				const materialGroup = batch.findMaterialGroup(slotTexture, slotBlendMode);

				batch.addMaterialGroup(finalIndicesLength, materialGroup);
				batch.batch(finalVertices, finalVerticesLength, finalIndices, finalIndicesLength, z);
				z += zOffset;
			}

			clipper.clipEndWithSlot(slot);
		}
		clipper.clipEnd();
		batch.end();
	}
}
