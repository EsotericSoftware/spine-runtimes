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
	Physics,
	type Skeleton,
	type SkeletonCoordinateConverter,
	SkeletonPhysicsMovement,
	type Slot,
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
	TintMixin,
	TransformMixin,
	VisibleMixin,
} from "./mixins.js";
import { PhaserMesh2DRenderer } from "./renderers/PhaserMesh2DRenderer.js";
import { SpineCanvasRenderer } from "./renderers/SpineCanvasRenderer.js";
import type { SpineGameObjectRenderer, SpineGameObjectRendererType } from "./renderers/SpineGameObjectRenderer.js";
import { SpineWebGLRenderer } from "./renderers/SpineWebGLRenderer.js";
import { SetupPoseBoundsProvider, type SpineGameObjectBoundsProvider } from "./SpineGameObjectBounds.js";
import type { SpinePlugin } from "./SpinePlugin.js";

export type { SpineGameObjectRendererType } from "./renderers/SpineGameObjectRenderer.js";
export type { SpineGameObjectBoundsProvider } from "./SpineGameObjectBounds.js";
export { AABBRectangleBoundsProvider, SetupPoseBoundsProvider, SkinsAndAnimationBoundsProvider } from "./SpineGameObjectBounds.js";

class BaseSpineGameObject extends Phaser.GameObjects.GameObject {
	constructor (scene: Phaser.Scene, type: string) {
		super(scene, type);
	}
}

/** Options for attaching a Phaser GameObject to a Spine slot. */
export interface SpineSlotObjectOptions {
	/** If true, the attached GameObject is hidden when the slot has no attachment. */
	followAttachmentTimeline?: boolean;

	/** Whether to render the GameObject before or after the slot attachment. */
	placement?: "before" | "after";

	/** If true, active Spine clipping attachments also clip the attached GameObject. */
	clipping?: boolean;

	/** If true, keep the GameObject's current x/y as a local offset from the slot. Defaults to false. */
	preservePosition?: boolean;
}

/** Runtime state for a Phaser GameObject attached to a Spine slot. */
export interface SpineSlotObjectEntry extends Required<SpineSlotObjectOptions> {
	/** The Phaser GameObject attached to the slot. */
	gameObject: Phaser.GameObjects.GameObject;
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

	/** Renderer backend. Defaults to `"phaser"` in WebGL games and `"spine-canvas"` in Canvas games. */
	renderer?: SpineGameObjectRendererType;
}

/** Options accepted by the `this.add.spine(...)` factory after position and cache keys. */
export type SpineGameObjectFactoryOptions = Omit<SpineGameObjectOptions, "x" | "y" | "dataKey" | "atlasKey">;

/** Phaser GameObject that displays and updates a Spine skeleton. */
export class SpineGameObject extends DepthMixin(OriginMixin(ComputedSizeMixin(FlipMixin(ScrollFactorMixin(TintMixin(TransformMixin(VisibleMixin(AlphaMixin(BaseSpineGameObject))))))))) implements SkeletonCoordinateConverter {
	blendMode = -1;
	skeleton: Skeleton;
	animationStateData: AnimationStateData;
	animationState: AnimationState;
	/** Called after animation state is applied and before skeleton world transforms are updated. */
	beforeUpdateWorldTransforms: (object: SpineGameObject) => void = () => { };

	/** Called after skeleton world transforms are updated. */
	afterUpdateWorldTransforms: (object: SpineGameObject) => void = () => { };
	private readonly slotObjects = new Map<Slot, SpineSlotObjectEntry>();
	private offsetX = 0;
	private offsetY = 0;
	/** Tracks this GameObject's transform movement and applies it to skeleton physics constraints when enabled. */
	readonly skeletonPhysics: SkeletonPhysicsMovement;

	/** Renderer backend selected for this GameObject. */
	readonly rendererType: SpineGameObjectRendererType;
	private rendererBackend: SpineGameObjectRenderer;

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
		super(scene, window.SPINE_GAME_OBJECT_TYPE ?? SPINE_GAME_OBJECT_TYPE);
		let options: SpineGameObjectOptions;
		if (typeof optionsOrX === "number") {
			if (dataKey === undefined || atlasKey === undefined) throw new Error("Missing dataKey and atlasKey.");
			options = { x: optionsOrX, y: y ?? 0, dataKey, atlasKey, boundsProvider };
		} else {
			options = optionsOrX;
		}
		const rendererType = options.renderer ?? (plugin.isWebGL ? "phaser" : "spine-canvas");
		if (rendererType !== "phaser" && rendererType !== "spine-webgl" && rendererType !== "spine-canvas") throw new Error(`Unsupported SpineGameObject renderer '${rendererType}'. Use 'phaser', 'spine-webgl', or 'spine-canvas'.`);
		if (plugin.isWebGL && rendererType === "spine-canvas") throw new Error("SpineGameObject renderer 'spine-canvas' is only supported in Phaser Canvas games.");
		if (!plugin.isWebGL && rendererType !== "spine-canvas") throw new Error(`SpineGameObject renderer '${rendererType}' requires a Phaser WebGL game. Use renderer: 'spine-canvas' or omit the renderer option in Phaser Canvas games.`);
		this.rendererType = rendererType;
		this.rendererBackend = rendererType === "spine-webgl" ? new SpineWebGLRenderer() : rendererType === "spine-canvas" ? new SpineCanvasRenderer() : new PhaserMesh2DRenderer(this);
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

	/**
	 * Sets a uniform tint color for the skeleton.
	 *
	 * Spine skeletons do not support Phaser's four-corner gradient tint. Only the
	 * first color is used; the other arguments are accepted for Phaser API compatibility.
	 * @param topLeft Tint color applied to the whole skeleton.
	 * @param _topRight Ignored.
	 * @param _bottomLeft Ignored.
	 * @param _bottomRight Ignored.
	 */
	setTint (topLeft = 0xffffff, _topRight?: number, _bottomLeft?: number, _bottomRight?: number): this {
		this.tintTopLeft = topLeft;
		this.tintTopRight = topLeft;
		this.tintBottomLeft = topLeft;
		this.tintBottomRight = topLeft;
		this.skeleton.color.r = ((topLeft >> 16) & 0xff) / 255;
		this.skeleton.color.g = ((topLeft >> 8) & 0xff) / 255;
		this.skeleton.color.b = (topLeft & 0xff) / 255;
		return this;
	}

	clearTint (): this {
		this.setTint(0xffffff);
		this.setTint2(0x000000);
		this.setTintMode(Phaser.TintModes.MULTIPLY);
		return this;
	}

	/** Bounds provider used to calculate this GameObject's size and display origin. */
	boundsProvider: SpineGameObjectBoundsProvider = new SetupPoseBoundsProvider();

	/** Recalculates this GameObject's size and display origin from its bounds provider. */
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

	/**
	 * Converts `point` in-place from skeleton coordinates to Phaser game coordinates.
	 * @param point The point to convert.
	 */
	skeletonToGame (point: { x: number; y: number }) {
		const transform = this.getWorldTransformMatrix();
		const x = point.x + this.renderOffsetX;
		const y = point.y + this.renderOffsetY;
		point.x = x * transform.a + y * transform.c + transform.tx;
		point.y = x * transform.b + y * transform.d + transform.ty;
	}

	/**
	 * Converts `point` in-place from Phaser game coordinates to skeleton coordinates.
	 * @param point The point to convert.
	 */
	gameToSkeleton (point: { x: number; y: number }) {
		const transform = this.getWorldTransformMatrix().invert();
		const x = point.x, y = point.y;
		point.x = x * transform.a + y * transform.c + transform.tx - this.renderOffsetX;
		point.y = x * transform.b + y * transform.d + transform.ty - this.renderOffsetY;
	}

	/**
	 * Converts `point` in-place from Phaser game coordinates to a bone's local coordinates.
	 * @param point The point to convert.
	 * @param bone The bone whose local coordinates should receive the converted point.
	 */
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
	 * Attaches a Phaser GameObject to a Spine slot.
	 * While attached, the SpineGameObject controls the GameObject's scroll factors so it remains anchored to the slot.
	 * Removing it does not restore its previous scroll factors.
	 * @param slotRef Slot index, slot name, or Slot instance.
	 * @param gameObject Phaser GameObject to render at the slot.
	 * @param options Slot-object rendering options.
	 */
	addSlotObject (slotRef: number | string | Slot, gameObject: Phaser.GameObjects.GameObject, options: SpineSlotObjectOptions = {}): void {
		this.requireSlotObjectSupport();
		const slot = this.getSlot(slotRef);
		this.removeSlotObject(slot);
		this.removeGameObjectFromOtherSlots(gameObject);
		gameObject.parentContainer?.remove(gameObject);
		gameObject.removeFromDisplayList();
		if (!options.preservePosition) {
			const transform = gameObject as Phaser.GameObjects.GameObject & Phaser.GameObjects.Components.Transform;
			transform.x = 0;
			transform.y = 0;
		}
		this.slotObjects.set(slot, {
			gameObject,
			followAttachmentTimeline: options.followAttachmentTimeline ?? false,
			placement: options.placement ?? "after",
			clipping: options.clipping ?? true,
			preservePosition: options.preservePosition ?? false,
		});
	}

	/**
	 * Returns the Phaser GameObject attached to a Spine slot, if any.
	 * @param slotRef Slot index, slot name, or Slot instance.
	 * @returns The attached Phaser GameObject, or `undefined`.
	 */
	getSlotObject (slotRef: number | string | Slot): Phaser.GameObjects.GameObject | undefined {
		this.requireSlotObjectSupport();
		return this.slotObjects.get(this.getSlot(slotRef))?.gameObject;
	}

	/**
	 * Removes the Phaser GameObject attached to a Spine slot.
	 * @param slotRef Slot index, slot name, or Slot instance.
	 * @param gameObject Optional GameObject guard. If supplied, removal only occurs when it is the attached object.
	 */
	removeSlotObject (slotRef: number | string | Slot, gameObject?: Phaser.GameObjects.GameObject): void {
		this.requireSlotObjectSupport();
		const slot = this.getSlot(slotRef);
		const entry = this.slotObjects.get(slot);
		if (!entry) return;
		if (gameObject && entry.gameObject !== gameObject) return;
		this.slotObjects.delete(slot);
	}

	/** Removes all Phaser GameObjects attached to Spine slots. */
	removeSlotObjects (): void {
		this.requireSlotObjectSupport();
		this.slotObjects.clear();
	}

	/**
	 * Updates animation state, applies it to the skeleton, and updates skeleton world transforms.
	 * @param delta Time delta in milliseconds.
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

	willRender (camera: Phaser.Cameras.Scene2D.Camera) {
		const GameObjectRenderMask = 0xf;
		return this.visible && !(GameObjectRenderMask !== this.renderFlags || (this.cameraFilter !== 0 && (this.cameraFilter & camera.id) !== 0));
	}

	renderWebGL (
		renderer: Phaser.Renderer.WebGL.WebGLRenderer,
		src: SpineGameObject,
		drawingContext: Phaser.Renderer.WebGL.DrawingContext,
		parentMatrix?: Phaser.GameObjects.Components.TransformMatrix,
		renderStep?: number,
		displayList?: Phaser.GameObjects.GameObject[],
		displayListIndex?: number
	) {
		src.rendererBackend.renderWebGL(renderer, src, drawingContext, parentMatrix, renderStep, displayList, displayListIndex);
	}

	renderCanvas (
		renderer: Phaser.Renderer.Canvas.CanvasRenderer,
		src: SpineGameObject,
		camera: Phaser.Cameras.Scene2D.Camera,
		parentMatrix?: Phaser.GameObjects.Components.TransformMatrix
	) {
		src.rendererBackend.renderCanvas?.(renderer, src, camera, parentMatrix);
	}

	preDestroy () {
		this.rendererBackend.preDestroy();
		this.slotObjects.clear();
	}

	/**
	 * Returns the internal slot object entry for a slot.
	 * @param slot The slot to query.
	 * @returns The slot object entry, or `undefined`.
	 */
	getSlotObjectEntry (slot: Slot): SpineSlotObjectEntry | undefined {
		return this.slotObjects.get(slot);
	}

	/**
	 * Resolves a slot index, name, or Slot instance to a Slot.
	 * @param slotRef Slot index, slot name, or Slot instance.
	 * @returns The resolved Slot.
	 */
	getSlot (slotRef: number | string | Slot): Slot {
		if (typeof slotRef === "object") return slotRef;
		if (typeof slotRef === "number") {
			const slot = this.skeleton.slots[slotRef];
			if (!slot) throw new Error(`Spine slot index not found: ${slotRef}`);
			return slot;
		}
		const slot = this.skeleton.findSlot(slotRef);
		if (!slot) throw new Error(`Spine slot not found: ${slotRef}`);
		return slot;
	}

	private requireSlotObjectSupport (): void {
		if (this.rendererType !== "phaser") {
			throw new Error(`SpineGameObject slot-object APIs are not supported when renderer is '${this.rendererType}'. Use renderer: 'phaser' to enable slot objects.`);
		}
	}

	private removeGameObjectFromOtherSlots (gameObject: Phaser.GameObjects.GameObject): void {
		for (const [slot, entry] of this.slotObjects) {
			if (entry.gameObject === gameObject) this.slotObjects.delete(slot);
		}
	}
}
