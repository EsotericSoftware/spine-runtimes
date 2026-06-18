/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

import {
	AnimationState,
	AnimationStateData,
	ClippingAttachment,
	Color,
	MeshAttachment,
	type NumberArrayLike,
	Physics,
	RegionAttachment,
	Skeleton,
	SkeletonBinary,
	SkeletonClipping,
	type SkeletonData,
	SkeletonJson,
	Utils,
	Vector2,
} from "@esotericsoftware/spine-core";
import * as THREE from "three";

import { type MaterialWithMap, MeshBatcher } from "./MeshBatcher.js";
import type { ThreeJsTexture } from "./ThreeJsTexture.js";

type SkeletonMeshMaterialParametersCustomizer = (materialParameters: THREE.MaterialParameters) => void;
type SkeletonMeshConfiguration = {

	/** The skeleton data object loaded by using {@link SkeletonJson} or {@link SkeletonBinary} */
	skeletonData: SkeletonData,

	/** Set it to true to enable tint black rendering */
	twoColorTint?: boolean,

	/**
	 * The function used to create the materials for the meshes composing this Object3D.
	 * The material used must have the `map` property.
	 * By default a MeshStandardMaterial is used, so no light and shadows are available.
	 * Use a MeshStandardMaterial
	 *
	 * @param parameters The default parameters with which this function is invoked.
	 * You should pass this parameters, once personalized, to the costructor of the material you want to use.
	 * Default values are defined in {@link SkeletonMesh.DEFAULT_MATERIAL_PARAMETERS}.
	 *
	 * @returns An instance of the material you want to be used for the meshes of this Object3D. The material must have the `map` property.
	 */
	materialFactory?: (parameters: THREE.MaterialParameters) => MaterialWithMap,
};

export class SkeletonMesh extends THREE.Object3D {
	// public static readonly DEFAULT_MATERIAL_PARAMETERS: THREE.MaterialParameters = {
	public static readonly DEFAULT_MATERIAL_PARAMETERS: THREE.MaterialParameters = {
		side: THREE.DoubleSide,
		depthWrite: true,
		depthTest: true,
		transparent: true,
		alphaTest: 0.001,
		vertexColors: true,
		premultipliedAlpha: true,
		forceSinglePass: true,
	}

	tempPos: Vector2 = new Vector2();
	tempUv: Vector2 = new Vector2();
	tempLight = new Color();
	tempDark = new Color();
	skeleton: Skeleton;
	state: AnimationState;
	zOffset: number = 0.1;

	private batches = [] as MeshBatcher[];
	private materialFactory: (parameters: THREE.MaterialParameters) => MaterialWithMap;
	private nextBatchIndex = 0;
	private clipper: SkeletonClipping = new SkeletonClipping();

	static QUAD_TRIANGLES = [0, 1, 2, 2, 3, 0];
	static VERTEX_SIZE = 2 + 2 + 4;
	private vertexSize = 2 + 2 + 4;
	private twoColorTint: boolean;

	private vertices = Utils.newFloatArray(1024);
	private tempColor = new Color();
	private tempDarkColor = new Color();

	private _castShadow = false;
	private _receiveShadow = false;
	private _physicsPositionInheritanceFactorX = 1;
	private _physicsPositionInheritanceFactorY = 1;
	private _physicsRotationInheritanceFactor = 1;
	private hasLastPhysicsTransform = false;
	private lastPhysicsX = 0;
	private lastPhysicsY = 0;
	private lastPhysicsRotation = 0;
	private readonly currentPhysicsPosition = new THREE.Vector3();
	private readonly lastPhysicsPosition = new THREE.Vector3();
	private readonly physicsWorldPosition = new THREE.Vector3();
	private readonly physicsWorldQuaternion = new THREE.Quaternion();
	private readonly physicsWorldScale = new THREE.Vector3();
	private readonly physicsEuler = new THREE.Euler();

	/** Scales how much horizontal translation of this Three.js object is inherited by skeleton physics constraints. */
	public get physicsPositionInheritanceFactorX (): number {
		return this._physicsPositionInheritanceFactorX;
	}

	/** Scales how much vertical translation of this Three.js object is inherited by skeleton physics constraints. */
	public get physicsPositionInheritanceFactorY (): number {
		return this._physicsPositionInheritanceFactorY;
	}

	/**
	 * Sets how much translation of this Three.js object is inherited by skeleton physics constraints.
	 * The default is (1, 1), which applies object translation normally. Use (0, 0)
	 * to prevent object translation from affecting physics constraints.
	 */
	public setPhysicsPositionInheritanceFactor (x: number, y: number): void {
		const wasDisabled = this._physicsPositionInheritanceFactorX === 0 && this._physicsPositionInheritanceFactorY === 0;
		const isEnabled = x !== 0 || y !== 0;

		this._physicsPositionInheritanceFactorX = x;
		this._physicsPositionInheritanceFactorY = y;
		if (wasDisabled && isEnabled) this.resetPhysicsPosition();
	}

	/**
	 * Scales how much rotation of this Three.js object is inherited by skeleton physics constraints.
	 * The default is `1`, which applies object rotation normally. Use `0` to prevent object rotation
	 * from affecting physics constraints.
	 */
	public get physicsRotationInheritanceFactor (): number {
		return this._physicsRotationInheritanceFactor;
	}

	public set physicsRotationInheritanceFactor (value: number) {
		const wasDisabled = this._physicsRotationInheritanceFactor === 0;
		this._physicsRotationInheritanceFactor = value;
		if (wasDisabled && value !== 0) this.resetPhysicsRotation();
	}

	/**
	 * Create an Object3D containing meshes representing your Spine animation.
	 * Personalize your material providing a {@link SkeletonMeshConfiguration}
	 * @param skeletonData
	 */
	constructor (configuration: SkeletonMeshConfiguration)
	/**
	 * @deprecated This signature is deprecated, please use the one with a single {@link SkeletonMeshConfiguration} parameter
	 */
	constructor (
		skeletonData: SkeletonData,
		materialCustomizer: SkeletonMeshMaterialParametersCustomizer,
	)
	constructor (
		skeletonDataOrConfiguration: SkeletonData | SkeletonMeshConfiguration,
		materialCustomizer: SkeletonMeshMaterialParametersCustomizer = () => { }
	) {
		super();

		if (!('skeletonData' in skeletonDataOrConfiguration)) {
			const materialFactory = () => {
				const parameters: THREE.MaterialParameters = { ...SkeletonMesh.DEFAULT_MATERIAL_PARAMETERS };
				materialCustomizer(parameters);
				return new THREE.MeshBasicMaterial(parameters);
			};
			skeletonDataOrConfiguration = {
				skeletonData: skeletonDataOrConfiguration,
				materialFactory,
			}
		}

		this.twoColorTint = skeletonDataOrConfiguration.twoColorTint ?? true;
		if (this.twoColorTint) {
			this.vertexSize += 4;
		}

		this.materialFactory = skeletonDataOrConfiguration.materialFactory ?? (() => new THREE.MeshBasicMaterial(SkeletonMesh.DEFAULT_MATERIAL_PARAMETERS));
		this.skeleton = new Skeleton(skeletonDataOrConfiguration.skeletonData);
		const animData = new AnimationStateData(skeletonDataOrConfiguration.skeletonData);
		this.state = new AnimationState(animData);

		Object.defineProperty(this, 'castShadow', {
			get: () => this._castShadow,
			set: (value: boolean) => {
				this._castShadow = value;
				this.traverse((child) => {
					if (child instanceof MeshBatcher) {
						child.castShadow = value;
					}
				});
			},
		});

		Object.defineProperty(this, 'receiveShadow', {
			get: () => this._receiveShadow,
			set: (value: boolean) => {
				this._receiveShadow = value;
				// Propagate to children
				this.traverse((child) => {
					if (child instanceof MeshBatcher) {
						child.receiveShadow = value;
					}
				});
			},
		});
	}

	update (deltaTime: number) {
		const state = this.state;
		const skeleton = this.skeleton;

		state.update(deltaTime);
		state.apply(skeleton);
		this.applyTransformMovementToPhysics();
		skeleton.update(deltaTime);
		skeleton.updateWorldTransform(Physics.update);

		this.updateGeometry();
	}

	/** Resets the position used for calculating inherited physics translation. */
	public resetPhysicsPosition (): void {
		this.getWorldPosition(this.physicsWorldPosition);
		this.lastPhysicsX = this.physicsWorldPosition.x;
		this.lastPhysicsY = this.physicsWorldPosition.y;
		if (!this.hasLastPhysicsTransform) this.lastPhysicsRotation = this.getPhysicsRotation();
		this.hasLastPhysicsTransform = true;
	}

	/** Resets the rotation used for calculating inherited physics rotation. */
	public resetPhysicsRotation (): void {
		this.getWorldPosition(this.physicsWorldPosition);
		this.lastPhysicsRotation = this.getPhysicsRotation();
		if (!this.hasLastPhysicsTransform) {
			this.lastPhysicsX = this.physicsWorldPosition.x;
			this.lastPhysicsY = this.physicsWorldPosition.y;
		}
		this.hasLastPhysicsTransform = true;
	}

	/** Resets the transform used for calculating inherited physics translation and rotation. */
	public resetPhysicsTransform (): void {
		this.resetPhysicsPosition();
		this.resetPhysicsRotation();
	}

	private applyTransformMovementToPhysics (): void {
		this.getWorldPosition(this.physicsWorldPosition);
		const { x, y } = this.physicsWorldPosition;
		const currentRotation = this.getPhysicsRotation();

		if (this.hasLastPhysicsTransform) {
			this.applyPositionMovementToPhysics(x, y);
			this.applyRotationMovementToPhysics(currentRotation);
		}

		this.setLastPhysicsTransform(x, y, currentRotation);
	}

	private applyPositionMovementToPhysics (currentX: number, currentY: number): void {
		if (this._physicsPositionInheritanceFactorX === 0 && this._physicsPositionInheritanceFactorY === 0) return;

		const currentPosition = this.currentPhysicsPosition;
		currentPosition.set(currentX, currentY, 0);
		this.worldToLocal(currentPosition);

		const lastPosition = this.lastPhysicsPosition;
		lastPosition.set(this.lastPhysicsX, this.lastPhysicsY, 0);
		this.worldToLocal(lastPosition);

		this.skeleton.physicsTranslate(
			(currentPosition.x - lastPosition.x) * this._physicsPositionInheritanceFactorX,
			(currentPosition.y - lastPosition.y) * this._physicsPositionInheritanceFactorY
		);
	}

	private applyRotationMovementToPhysics (currentRotation: number): void {
		const rotationFactor = this._physicsRotationInheritanceFactor;
		if (rotationFactor === 0) return;

		this.skeleton.physicsRotate(0, 0, this.getRotationDelta(currentRotation, this.lastPhysicsRotation) * rotationFactor);
	}

	private setLastPhysicsTransform (x: number, y: number, rotation: number): void {
		this.lastPhysicsX = x;
		this.lastPhysicsY = y;
		this.lastPhysicsRotation = rotation;
		this.hasLastPhysicsTransform = true;
	}

	private getPhysicsRotation (): number {
		this.updateWorldMatrix(true, false);
		this.matrixWorld.decompose(this.physicsWorldPosition, this.physicsWorldQuaternion, this.physicsWorldScale);
		this.physicsEuler.setFromQuaternion(this.physicsWorldQuaternion, 'XYZ');
		return this.physicsEuler.z * 180 / Math.PI;
	}

	private getRotationDelta (current: number, previous: number): number {
		let delta = current - previous;
		delta = (delta + 180) % 360 - 180;
		return delta < -180 ? delta + 360 : delta;
	}

	dispose () {
		for (let i = 0; i < this.batches.length; i++) {
			this.batches[i].dispose();
		}
	}

	private clearBatches () {
		for (let i = 0; i < this.batches.length; i++) {
			this.batches[i].clear();
			this.batches[i].visible = false;
		}
		this.nextBatchIndex = 0;
	}

	private nextBatch () {
		if (this.batches.length === this.nextBatchIndex) {
			const batch = new MeshBatcher(MeshBatcher.MAX_VERTICES, this.materialFactory, this.twoColorTint);
			batch.castShadow = this._castShadow;
			batch.receiveShadow = this._receiveShadow;
			this.add(batch);
			this.batches.push(batch);
		}
		const batch = this.batches[this.nextBatchIndex++];
		batch.visible = true;
		return batch;
	}

	private updateGeometry () {
		this.clearBatches();

		const clipper = this.clipper;

		let vertices: NumberArrayLike = this.vertices;
		let triangles: Array<number> | null = null;
		let uvs: NumberArrayLike | null = null;
		const skeleton = this.skeleton;
		const drawOrder = skeleton.drawOrder.appliedPose;
		let batch = this.nextBatch();
		batch.begin();
		let z = 0;
		const zOffset = this.zOffset;
		const vertexSize = this.vertexSize;
		for (let i = 0, n = drawOrder.length; i < n; i++) {
			const slot = drawOrder[i];
			if (!slot.bone.active) {
				clipper.clipEnd(slot);
				continue;
			}
			const pose = slot.appliedPose;
			const attachment = pose.attachment;
			let attachmentColor: Color | null;
			let texture: ThreeJsTexture | null;
			let numFloats = 0;
			if (attachment instanceof RegionAttachment) {
				attachmentColor = attachment.color;
				vertices = this.vertices;
				numFloats = vertexSize * 4;

				const sequence = attachment.sequence;
				const sequenceIndex = sequence.resolveIndex(pose);
				attachment.computeWorldVertices(slot, attachment.getOffsets(pose), vertices, 0, vertexSize);

				triangles = SkeletonMesh.QUAD_TRIANGLES;
				uvs = sequence.getUVs(sequenceIndex);
				texture = sequence.regions[sequenceIndex]?.texture;
			} else if (attachment instanceof MeshAttachment) {
				attachmentColor = attachment.color;
				vertices = this.vertices;
				numFloats = (attachment.worldVerticesLength >> 1) * vertexSize;
				if (numFloats > vertices.length) {
					vertices = this.vertices = Utils.newFloatArray(numFloats);
				}
				attachment.computeWorldVertices(
					skeleton,
					slot,
					0,
					attachment.worldVerticesLength,
					vertices,
					0,
					vertexSize
				);
				triangles = attachment.triangles;

				const sequence = attachment.sequence;
				const sequenceIndex = sequence.resolveIndex(pose);

				uvs = sequence.getUVs(sequenceIndex);
				texture = sequence.regions[sequenceIndex]?.texture;
			} else if (attachment instanceof ClippingAttachment) {
				clipper.clipEnd(slot);
				clipper.clipStart(skeleton, slot, attachment);
				continue;
			} else {
				clipper.clipEnd(slot);
				continue;
			}

			if (texture != null) {
				const skeletonColor = skeleton.color;
				const slotColor = pose.color;
				const alpha = skeletonColor.a * slotColor.a * attachmentColor.a;
				const color = this.tempColor;
				color.set(
					skeletonColor.r * slotColor.r * attachmentColor.r * alpha,
					skeletonColor.g * slotColor.g * attachmentColor.g * alpha,
					skeletonColor.b * slotColor.b * attachmentColor.b * alpha,
					alpha
				);

				const darkColor = this.tempDarkColor;
				if (!pose.darkColor)
					darkColor.set(0, 0, 0, 1);
				else {
					darkColor.r = pose.darkColor.r * alpha;
					darkColor.g = pose.darkColor.g * alpha;
					darkColor.b = pose.darkColor.b * alpha;
					darkColor.a = 1;
				}

				let finalVertices: NumberArrayLike;
				let finalVerticesLength: number;
				let finalIndices: NumberArrayLike;
				let finalIndicesLength: number;

				if (clipper.isClipping() && clipper.clipTriangles(vertices, triangles, triangles.length, uvs, color, darkColor, this.twoColorTint, vertexSize)) {
					const clippedVertices = clipper.clippedVertices;
					const clippedTriangles = clipper.clippedTriangles;
					finalVertices = clippedVertices;
					finalVerticesLength = clippedVertices.length;
					finalIndices = clippedTriangles;
					finalIndicesLength = clippedTriangles.length;
				} else {
					const verts = vertices;
					if (!this.twoColorTint) {
						for (let v = 2, u = 0, n = numFloats; v < n; v += vertexSize, u += 2) {
							verts[v] = color.r;
							verts[v + 1] = color.g;
							verts[v + 2] = color.b;
							verts[v + 3] = color.a;
							verts[v + 4] = uvs[u];
							verts[v + 5] = uvs[u + 1];
						}
					} else {
						for (let v = 2, u = 0, n = numFloats; v < n; v += vertexSize, u += 2) {
							verts[v] = color.r;
							verts[v + 1] = color.g;
							verts[v + 2] = color.b;
							verts[v + 3] = color.a;
							verts[v + 4] = uvs[u];
							verts[v + 5] = uvs[u + 1];
							verts[v + 6] = darkColor.r;
							verts[v + 7] = darkColor.g;
							verts[v + 8] = darkColor.b;
							verts[v + 9] = darkColor.a;
						}
					}

					finalVertices = vertices;
					finalVerticesLength = numFloats;
					finalIndices = triangles;
					finalIndicesLength = triangles.length;
				}

				if (finalVerticesLength === 0 || finalIndicesLength === 0) {
					clipper.clipEnd(slot);
					continue;
				}

				// Start new batch if this one can't hold vertices/indices
				if (
					!batch.canBatch(
						finalVerticesLength / this.vertexSize,
						finalIndicesLength
					)
				) {
					batch.end();
					batch = this.nextBatch();
					batch.begin();
				}

				const slotBlendMode = slot.data.blendMode;
				const slotTexture = texture.texture;
				const materialGroup = batch.findMaterialGroup(
					slotTexture,
					slotBlendMode
				);

				batch.addMaterialGroup(finalIndicesLength, materialGroup);
				batch.batch(
					finalVertices,
					finalVerticesLength,
					finalIndices,
					finalIndicesLength,
					z
				);
				z += zOffset;
			}

			clipper.clipEnd(slot);
		}
		clipper.clipEnd();
		batch.end();
	}
}
