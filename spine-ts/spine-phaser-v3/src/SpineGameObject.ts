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
	type Bone,
	MathUtils,
	Physics,
	type Skeleton,
	type SkeletonCoordinateConverter,
	SkeletonPhysicsMovement,
	type Vector2,
} from "@esotericsoftware/spine-core";
import * as Phaser from "phaser";
import { SPINE_GAME_OBJECT_TYPE } from "./keys.js";
import {
	AlphaMixin,
	ComputedSizeMixin,
	DepthMixin,
	FlipMixin,
	OriginMixin,
	ScrollFactorMixin,
	TransformMixin,
	VisibleMixin,
} from "./mixins.js";
import { SetupPoseBoundsProvider, type SpineGameObjectBoundsProvider } from "./SpineGameObjectBounds.js";
import type { SpinePlugin } from "./SpinePlugin.js";

export type { SpineGameObjectBoundsProvider } from "./SpineGameObjectBounds.js";
export { AABBRectangleBoundsProvider, SetupPoseBoundsProvider, SkinsAndAnimationBoundsProvider } from "./SpineGameObjectBounds.js";

class BaseSpineGameObject extends Phaser.GameObjects.GameObject {
	constructor (scene: Phaser.Scene, type: string) {
		super(scene, type);
	}
}

/** Options used to construct a {@link SpineGameObject}. */
export interface SpineGameObjectOptions {
	/** Initial x-position in Phaser coordinates. */
	x?: number;

	/** Initial y-position in Phaser coordinates. */
	y?: number;

	/** Phaser cache key for the loaded Spine skeleton data. */
	dataKey: string;

	/** Phaser cache key for the loaded Spine atlas. */
	atlasKey: string;

	/** Bounds provider used to calculate the Phaser GameObject size and display origin. */
	boundsProvider?: SpineGameObjectBoundsProvider;
}

/** Options accepted by the `this.add.spine(...)` factory after position and cache keys. */
export type SpineGameObjectFactoryOptions = Omit<SpineGameObjectOptions, "x" | "y" | "dataKey" | "atlasKey">;

/**
 * A SpineGameObject is a Phaser {@link GameObject} that can be added to a Phaser Scene and render a Spine skeleton.
 *
 * The Spine GameObject is a thin wrapper around a Spine {@link Skeleton}, {@link AnimationState} and {@link AnimationStateData}. It is responsible for:
 * - updating the animation state
 * - applying the animation state to the skeleton's bones, slots, attachments, and draw order.
 * - updating the skeleton's bone world transforms
 * - rendering the skeleton
 *
 * See the {@link SpinePlugin} class for more information on how to create a `SpineGameObject`.
 *
 * The skeleton, animation state, and animation state data can be accessed via the repsective fields. They can be manually updated via {@link updatePose}.
 *
 * To modify the bone hierarchy before the world transforms are computed, a callback can be set via the {@link beforeUpdateWorldTransforms} field.
 *
 * To modify the bone hierarchy after the world transforms are computed, a callback can be set via the {@link afterUpdateWorldTransforms} field.
 *
 * The class also features methods to convert between the skeleton coordinate system and the Phaser coordinate system.
 *
 * See {@link skeletonToGame}, {@link gameToSkeleton}, and {@link gameToBone}.
 */
export class SpineGameObject extends DepthMixin(
	OriginMixin(
		ComputedSizeMixin(
			FlipMixin(
				ScrollFactorMixin(
					TransformMixin(VisibleMixin(AlphaMixin(BaseSpineGameObject)))
				)
			)
		)
	)
) implements SkeletonCoordinateConverter {
	blendMode = -1;
	skeleton: Skeleton;
	animationStateData: AnimationStateData;
	animationState: AnimationState;
	beforeUpdateWorldTransforms: (object: SpineGameObject) => void = () => { };
	afterUpdateWorldTransforms: (object: SpineGameObject) => void = () => { };
	private offsetX = 0;
	private offsetY = 0;
	/** Tracks this GameObject's transform movement and applies it to skeleton physics constraints when enabled. */
	readonly skeletonPhysics: SkeletonPhysicsMovement;

	constructor (scene: Phaser.Scene, plugin: SpinePlugin, options: SpineGameObjectOptions);
	/** @deprecated Pass a {@link SpineGameObjectOptions} object as the third argument instead. */
	constructor (scene: Phaser.Scene, plugin: SpinePlugin, x: number, y: number, dataKey: string, atlasKey: string, boundsProvider?: SpineGameObjectBoundsProvider);
	constructor (
		scene: Phaser.Scene,
		public readonly plugin: SpinePlugin,
		optionsOrX: SpineGameObjectOptions | number,
		y?: number,
		dataKey?: string,
		atlasKey?: string,
		boundsProvider?: SpineGameObjectBoundsProvider,
	) {
		// biome-ignore lint/suspicious/noExplicitAny: necessary
		super(scene, (window as any).SPINE_GAME_OBJECT_TYPE ? (window as any).SPINE_GAME_OBJECT_TYPE : SPINE_GAME_OBJECT_TYPE);
		let options: SpineGameObjectOptions;
		if (typeof optionsOrX === "number") {
			if (dataKey === undefined || atlasKey === undefined) throw new Error("Missing dataKey and atlasKey.");
			options = { x: optionsOrX, y: y ?? 0, dataKey, atlasKey, boundsProvider };
		} else {
			options = optionsOrX;
		}
		this.boundsProvider = options.boundsProvider ?? new SetupPoseBoundsProvider();
		this.setPosition(options.x ?? 0, options.y ?? 0);
		this.skeleton = this.plugin.createSkeleton(options.dataKey, options.atlasKey);
		this.skeletonPhysics = new SkeletonPhysicsMovement(this.skeleton, {
			readTransform: (out, readRotation) => {
				const transform = this.getWorldTransformMatrix();
				out.x = transform.tx;
				out.y = transform.ty;
				out.z = 0;
				if (readRotation) out.rotation = -Math.atan2(transform.b, transform.a) * 180 / Math.PI;
			},
			worldToSkeleton: point => this.gameToSkeleton(point),
		});
		this.animationStateData = new AnimationStateData(this.skeleton.data);
		this.animationState = new AnimationState(this.animationStateData);
		this.skeleton.updateWorldTransform(Physics.update);
		this.updateSize();
	}

	/** Bounds provider used to calculate this GameObject's size and display origin. */
	boundsProvider: SpineGameObjectBoundsProvider = new SetupPoseBoundsProvider();

	updateSize () {
		const bounds = this.boundsProvider.calculateBounds(this);
		this.width = bounds.width;
		this.height = bounds.height;
		this.setDisplayOrigin(-bounds.x, -bounds.y);
		this.offsetX = -bounds.x;
		this.offsetY = -bounds.y;
	}

	/** Horizontal skeleton render offset from the Phaser GameObject origin. */
	get renderOffsetX (): number {
		return this.offsetX - this.displayOriginX;
	}

	/** Vertical skeleton render offset from the Phaser GameObject origin. */
	get renderOffsetY (): number {
		return this.offsetY - this.displayOriginY;
	}

	/** Converts `point` in-place from skeleton coordinates to Phaser game coordinates. */
	skeletonToGame (point: { x: number; y: number }) {
		const transform = this.getWorldTransformMatrix();
		const x = point.x + this.renderOffsetX;
		const y = point.y + this.renderOffsetY;
		point.x = x * transform.a + y * transform.c + transform.tx;
		point.y = x * transform.b + y * transform.d + transform.ty;
	}

	/** Converts `point` in-place from Phaser game coordinates to skeleton coordinates. */
	gameToSkeleton (point: { x: number; y: number }) {
		const transform = this.getWorldTransformMatrix().invert();
		const x = point.x, y = point.y;
		point.x = x * transform.a + y * transform.c + transform.tx - this.renderOffsetX;
		point.y = x * transform.b + y * transform.d + transform.ty - this.renderOffsetY;
	}

	/** Converts `point` in-place from Phaser game coordinates to a bone's local coordinates. */
	gameToBone (point: { x: number; y: number }, bone: Bone) {
		this.gameToSkeleton(point);
		(bone.parent ? bone.parent.appliedPose : bone.appliedPose).worldToLocal(point as Vector2);
	}

	/** @deprecated Use {@link skeletonToGame} instead. */
	skeletonToPhaserWorldCoordinates (point: { x: number; y: number }) {
		this.skeletonToGame(point);
	}

	/** @deprecated Use {@link gameToSkeleton} instead. */
	phaserWorldCoordinatesToSkeleton (point: { x: number; y: number }) {
		this.gameToSkeleton(point);
	}

	/** @deprecated Use {@link gameToBone} instead. */
	phaserWorldCoordinatesToBone (point: { x: number; y: number }, bone: Bone) {
		this.gameToBone(point, bone);
	}

	/**
	 * Updates the {@link AnimationState}, applies it to the {@link Skeleton}, then updates the world transforms of all bones.
	 * @param delta The time delta in milliseconds
	 */
	updatePose (delta: number) {
		const deltaSeconds = delta / 1000;
		this.animationState.update(deltaSeconds);
		this.animationState.apply(this.skeleton);
		this.skeletonPhysics.applyTransformMovement();
		this.beforeUpdateWorldTransforms(this);
		this.skeleton.update(deltaSeconds);
		this.skeleton.updateWorldTransform(Physics.update);
		this.afterUpdateWorldTransforms(this);
	}

	preUpdate (_time: number, delta: number) {
		this.updatePose(delta);
	}

	preDestroy () {
		// FIXME tear down any event emitters
	}

	willRender (camera: Phaser.Cameras.Scene2D.Camera) {
		var GameObjectRenderMask = 0xf;
		var result = !this.skeleton || !(GameObjectRenderMask !== this.renderFlags || (this.cameraFilter !== 0 && this.cameraFilter & camera.id));
		if (!this.visible) result = false;

		if (!result && this.parentContainer && this.plugin.webGLRenderer) {
			const sceneRenderer = this.plugin.webGLRenderer;

			if (this.plugin.gl && this.plugin.phaserRenderer instanceof Phaser.Renderer.WebGL.WebGLRenderer && sceneRenderer.batcher.isDrawing) {
				sceneRenderer.end();
				this.plugin.phaserRenderer.pipelines.rebind();
			}
		}

		return result;
	}

	renderWebGL (
		renderer: Phaser.Renderer.WebGL.WebGLRenderer,
		src: SpineGameObject,
		camera: Phaser.Cameras.Scene2D.Camera,
		parentMatrix: Phaser.GameObjects.Components.TransformMatrix
	) {
		if (!this.skeleton || !this.animationState || !this.plugin.webGLRenderer)
			return;

		const sceneRenderer = this.plugin.webGLRenderer;
		if (renderer.newType) {
			renderer.pipelines.clear();
			sceneRenderer.begin();
		}

		camera.addToRenderList(src);
		const transform = Phaser.GameObjects.GetCalcMatrix(
			src,
			camera,
			parentMatrix
		).calc;
		const a = transform.a,
			b = transform.b,
			c = transform.c,
			d = transform.d,
			tx = transform.tx,
			ty = transform.ty;

		const offsetX = src.renderOffsetX;
		const offsetY = src.renderOffsetY;

		sceneRenderer.drawSkeleton(
			src.skeleton,
			-1,
			-1,
			(vertices, numVertices, stride) => {
				for (let i = 0; i < numVertices; i += stride) {
					const vx = vertices[i] + offsetX;
					const vy = vertices[i + 1] + offsetY;
					vertices[i] = vx * a + vy * c + tx;
					vertices[i + 1] = vx * b + vy * d + ty;
				}
			}
		);

		if (!renderer.nextTypeMatch) {
			sceneRenderer.end();
			renderer.pipelines.rebind();
		}
	}

	renderCanvas (
		renderer: Phaser.Renderer.Canvas.CanvasRenderer,
		src: SpineGameObject,
		camera: Phaser.Cameras.Scene2D.Camera,
		parentMatrix: Phaser.GameObjects.Components.TransformMatrix
	) {
		if (!this.skeleton || !this.animationState || !this.plugin.canvasRenderer)
			return;

		const context = renderer.currentContext;
		const skeletonRenderer = this.plugin.canvasRenderer;
		// biome-ignore lint/suspicious/noExplicitAny: necessary for phaser
		(skeletonRenderer as any).ctx = context;

		camera.addToRenderList(src);
		const transform = Phaser.GameObjects.GetCalcMatrix(
			src,
			camera,
			parentMatrix
		).calc;
		const skeleton = this.skeleton;
		const offsetX = this.renderOffsetX;
		const offsetY = this.renderOffsetY;
		skeleton.x = transform.tx + offsetX * transform.a + offsetY * transform.c;
		skeleton.y = transform.ty + offsetX * transform.b + offsetY * transform.d;
		skeleton.scaleX = transform.scaleX;
		skeleton.scaleY = transform.scaleY;
		const root = skeleton.getRootBone() as Bone;
		root.appliedPose.rotation = -MathUtils.radiansToDegrees * transform.rotationNormalized;
		this.skeleton.updateWorldTransform(Physics.update);

		context.save();
		skeletonRenderer.draw(skeleton);
		context.restore();
	}
}
