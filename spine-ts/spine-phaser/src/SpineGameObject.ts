/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

import { SPINE_GAME_OBJECT_TYPE } from "./keys.js";
import { SpinePlugin } from "./SpinePlugin.js";
import {
	ComputedSizeMixin,
	DepthMixin,
	FlipMixin,
	ScrollFactorMixin,
	TransformMixin,
	VisibleMixin,
	AlphaMixin,
	OriginMixin,
} from "./mixins.js";
import {
	AnimationState,
	AnimationStateData,
	Bone,
	MathUtils,
	Physics,
	Skeleton,
	Skin,
	Vector2,
} from "@esotericsoftware/spine-core";

class BaseSpineGameObject extends Phaser.GameObjects.GameObject {
	constructor (scene: Phaser.Scene, type: string) {
		super(scene, type);
	}
}

/** A bounds provider calculates the bounding box for a skeleton, which is then assigned as the size of the SpineGameObject. */
export interface SpineGameObjectBoundsProvider {
	// Returns the bounding box for the skeleton, in skeleton space.
	calculateBounds (gameObject: SpineGameObject): {
		x: number;
		y: number;
		width: number;
		height: number;
	};
}

/** A bounds provider that calculates the bounding box from the setup pose. */
export class SetupPoseBoundsProvider implements SpineGameObjectBoundsProvider {
	calculateBounds (gameObject: SpineGameObject) {
		if (!gameObject.skeleton) return { x: 0, y: 0, width: 0, height: 0 };
		// Make a copy of animation state and skeleton as this might be called while
		// the skeleton in the GameObject has already been heavily modified. We can not
		// reconstruct that state.
		const skeleton = new Skeleton(gameObject.skeleton.data);
		skeleton.setToSetupPose();
		skeleton.updateWorldTransform(Physics.update);
		const bounds = skeleton.getBoundsRect();
		return bounds.width == Number.NEGATIVE_INFINITY
			? { x: 0, y: 0, width: 0, height: 0 }
			: bounds;
	}
}

/** A bounds provider that calculates the bounding box by taking the maximumg bounding box for a combination of skins and specific animation. */
export class SkinsAndAnimationBoundsProvider
	implements SpineGameObjectBoundsProvider {
	/**
	 * @param animation The animation to use for calculating the bounds. If null, the setup pose is used.
	 * @param skins The skins to use for calculating the bounds. If empty, the default skin is used.
	 * @param timeStep The time step to use for calculating the bounds. A smaller time step means more precision, but slower calculation.
	 */
	constructor (
		private animation: string | null,
		private skins: string[] = [],
		private timeStep: number = 0.05
	) { }

	calculateBounds (gameObject: SpineGameObject): {
		x: number;
		y: number;
		width: number;
		height: number;
	} {
		if (!gameObject.skeleton || !gameObject.animationState)
			return { x: 0, y: 0, width: 0, height: 0 };
		// Make a copy of animation state and skeleton as this might be called while
		// the skeleton in the GameObject has already been heavily modified. We can not
		// reconstruct that state.
		const animationState = new AnimationState(gameObject.animationState.data);
		const skeleton = new Skeleton(gameObject.skeleton.data);
		const data = skeleton.data;
		if (this.skins.length > 0) {
			let customSkin = new Skin("custom-skin");
			for (const skinName of this.skins) {
				const skin = data.findSkin(skinName);
				if (skin == null) continue;
				customSkin.addSkin(skin);
			}
			skeleton.setSkin(customSkin);
		}
		skeleton.setToSetupPose();

		const animation =
			this.animation != null ? data.findAnimation(this.animation!) : null;
		if (animation == null) {
			skeleton.updateWorldTransform(Physics.update);
			const bounds = skeleton.getBoundsRect();
			return bounds.width == Number.NEGATIVE_INFINITY
				? { x: 0, y: 0, width: 0, height: 0 }
				: bounds;
		} else {
			let minX = Number.POSITIVE_INFINITY,
				minY = Number.POSITIVE_INFINITY,
				maxX = Number.NEGATIVE_INFINITY,
				maxY = Number.NEGATIVE_INFINITY;
			animationState.clearTracks();
			animationState.setAnimationWith(0, animation, false);
			const steps = Math.max(animation.duration / this.timeStep, 1.0);
			for (let i = 0; i < steps; i++) {
				const delta = i > 0 ? this.timeStep : 0;
				animationState.update(delta);
				animationState.apply(skeleton);
				skeleton.update(delta);
				skeleton.updateWorldTransform(Physics.update);

				const bounds = skeleton.getBoundsRect();
				minX = Math.min(minX, bounds.x);
				minY = Math.min(minY, bounds.y);
				maxX = Math.max(maxX, minX + bounds.width);
				maxY = Math.max(maxY, minY + bounds.height);
			}
			const bounds = {
				x: minX,
				y: minY,
				width: maxX - minX,
				height: maxY - minY,
			};
			return bounds.width == Number.NEGATIVE_INFINITY
				? { x: 0, y: 0, width: 0, height: 0 }
				: bounds;
		}
	}
}

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
 * See {@link skeletonToPhaserWorldCoordinates}, {@link phaserWorldCoordinatesToSkeleton}, and {@link phaserWorldCoordinatesToBoneLocal.}
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
) {
	blendMode = -1;
	skeleton: Skeleton;
	animationStateData: AnimationStateData;
	animationState: AnimationState;
	beforeUpdateWorldTransforms: (object: SpineGameObject) => void = () => { };
	afterUpdateWorldTransforms: (object: SpineGameObject) => void = () => { };
	private premultipliedAlpha = false;

	constructor (
		scene: Phaser.Scene,
		private plugin: SpinePlugin,
		x: number,
		y: number,
		dataKey: string,
		atlasKey: string,
		public boundsProvider: SpineGameObjectBoundsProvider = new SetupPoseBoundsProvider()
	) {
		super(scene, (window as any).SPINE_GAME_OBJECT_TYPE ? (window as any).SPINE_GAME_OBJECT_TYPE : SPINE_GAME_OBJECT_TYPE);
		this.setPosition(x, y);

		this.premultipliedAlpha = this.plugin.isAtlasPremultiplied(atlasKey);
		this.skeleton = this.plugin.createSkeleton(dataKey, atlasKey);
		this.animationStateData = new AnimationStateData(this.skeleton.data);
		this.animationState = new AnimationState(this.animationStateData);
		this.skeleton.updateWorldTransform(Physics.update);
		this.updateSize();
	}

	updateSize () {
		if (!this.skeleton) return;
		let bounds = this.boundsProvider.calculateBounds(this);
		// For some reason the TS compiler and the ComputedSize mixin don't work well together and we have
		// to cast to any.
		let self = this as any;
		self.width = bounds.width;
		self.height = bounds.height;
		this.displayOriginX = -bounds.x;
		this.displayOriginY = -bounds.y;
	}

	/** Converts a point from the skeleton coordinate system to the Phaser world coordinate system. */
	skeletonToPhaserWorldCoordinates (point: { x: number; y: number }) {
		let transform = this.getWorldTransformMatrix();
		let a = transform.a,
			b = transform.b,
			c = transform.c,
			d = transform.d,
			tx = transform.tx,
			ty = transform.ty;
		let x = point.x;
		let y = point.y;
		point.x = x * a + y * c + tx;
		point.y = x * b + y * d + ty;
	}

	/** Converts a point from the Phaser world coordinate system to the skeleton coordinate system. */
	phaserWorldCoordinatesToSkeleton (point: { x: number; y: number }) {
		let transform = this.getWorldTransformMatrix();
		transform = transform.invert();
		let a = transform.a,
			b = transform.b,
			c = transform.c,
			d = transform.d,
			tx = transform.tx,
			ty = transform.ty;
		let x = point.x;
		let y = point.y;
		point.x = x * a + y * c + tx;
		point.y = x * b + y * d + ty;
	}

	/** Converts a point from the Phaser world coordinate system to the bone's local coordinate system. */
	phaserWorldCoordinatesToBone (point: { x: number; y: number }, bone: Bone) {
		this.phaserWorldCoordinatesToSkeleton(point);
		if (bone.parent) {
			bone.parent.worldToLocal(point as Vector2);
		} else {
			bone.worldToLocal(point as Vector2);
		}
	}

	/**
	 * Updates the {@link AnimationState}, applies it to the {@link Skeleton}, then updates the world transforms of all bones.
	 * @param delta The time delta in milliseconds
	 */
	updatePose (delta: number) {
		this.animationState.update(delta / 1000);
		this.animationState.apply(this.skeleton);
		this.beforeUpdateWorldTransforms(this);
		this.skeleton.update(delta / 1000);
		this.skeleton.updateWorldTransform(Physics.update);
		this.afterUpdateWorldTransforms(this);
	}

	preUpdate (time: number, delta: number) {
		if (!this.skeleton || !this.animationState) return;
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
			var sceneRenderer = this.plugin.webGLRenderer;

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

		let sceneRenderer = this.plugin.webGLRenderer;
		if (renderer.newType) {
			renderer.pipelines.clear();
			sceneRenderer.begin();
		}

		camera.addToRenderList(src);
		let transform = Phaser.GameObjects.GetCalcMatrix(
			src,
			camera,
			parentMatrix
		).calc;
		let a = transform.a,
			b = transform.b,
			c = transform.c,
			d = transform.d,
			tx = transform.tx,
			ty = transform.ty;
		sceneRenderer.drawSkeleton(
			this.skeleton,
			this.premultipliedAlpha,
			-1,
			-1,
			(vertices, numVertices, stride) => {
				for (let i = 0; i < numVertices; i += stride) {
					let vx = vertices[i];
					let vy = vertices[i + 1];
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

		let context = renderer.currentContext;
		let skeletonRenderer = this.plugin.canvasRenderer;
		(skeletonRenderer as any).ctx = context;

		camera.addToRenderList(src);
		let transform = Phaser.GameObjects.GetCalcMatrix(
			src,
			camera,
			parentMatrix
		).calc;
		let skeleton = this.skeleton;
		skeleton.x = transform.tx;
		skeleton.y = transform.ty;
		skeleton.scaleX = transform.scaleX;
		skeleton.scaleY = transform.scaleY;
		let root = skeleton.getRootBone()!;
		root.rotation = -MathUtils.radiansToDegrees * transform.rotationNormalized;
		this.skeleton.updateWorldTransform(Physics.update);

		context.save();
		skeletonRenderer.draw(skeleton);
		context.restore();
	}
}
