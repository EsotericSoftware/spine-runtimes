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

import type { AnimationState, AssetLoader, Bone, BonePose, C3Matrix, C3RendererRuntime, Event, NumberArrayLike, Skeleton, SkeletonData, Skin, Slot, SpineBoundsProvider, SpineBoundsProviderType, TextureAtlas, } from "@esotericsoftware/spine-construct3-lib";

const C3 = globalThis.C3;
const spine = globalThis.spine;

spine.Skeleton.yDown = true;

type BoneOverride = Partial<BonePose> & { mode: "game" | "local" };
type BoneFollower = {
	uid: number,
	offsetX: number,
	offsetY: number,
	offsetAngle: number,
	offsetScaleX: number,
	offsetScaleY: number,
	originalWidth: number,
	originalHeight: number,
	refGameScaleX: number,
	refGameScaleY: number,
};

type BoneGameTransform = {
	x: number,
	y: number,
	xAxisX: number,
	xAxisY: number,
	yAxisX: number,
	yAxisY: number,
	reflectionSign: number,
};

class SpineC3Instance extends globalThis.ISDKWorldInstanceBase {
	propAtlas = "";
	propSkel = "";
	propLoaderScale = 1;
	propSkin: string[] = [];
	propAnimation?: string;
	propOffsetX = 0;
	propOffsetY = 0;
	propOffsetAngle = 0;
	propScaleX = 1;
	propScaleY = 1;
	propSlotZOffset = 0;
	propDebugSkeleton = false;
	propBoundsProvider: SpineBoundsProviderType = "setup";
	propEnableCollision = false;

	collisionSpriteInstance?: IWorldInstance;
	collisionSpriteClassName = "";
	private collisionBoundingBoxSlotName = "";
	private collisionBoundingBoxAttachmentName = "";
	private collisionBoundingBoxSlot?: Slot;
	private collisionBoundingBoxVertices = spine.Utils.newFloatArray(128);
	private collisionBoundingBoxGamePoints: number[] = [];
	private collisionBoundingBoxDebug = false;
	private collisionBoundingBoxMeshSize: [number, number] = [0, 0];
	private collisionBodyDrivesObject = false;
	private collisionBodyPrevX = 0;
	private collisionBodyPrevY = 0;
	private collisionBodyPrevAngle = 0;
	isPlaying = true;
	physicsMode = spine.Physics.update;
	customSkins = new Map<string, Skin>();

	textureAtlas?: TextureAtlas;
	renderer?: IRenderer;
	skeletonLoaded = false;
	private loadStarted = false;
	skeleton?: Skeleton;
	state?: AnimationState;
	public triggeredEventTrack = -1;
	public triggeredEventAnimation = "";
	public triggeredEventName = "";
	public triggeredEventData?: Event & { track: number, animation: string };
	private pendingAnimationEvents: { eventName: string, track: number, animation: string, event?: Event }[] = [];
	private assetLoader: AssetLoader;
	private skeletonRenderer?: C3RendererRuntime;
	private matrix: C3Matrix;
	private quaternion: Vec4Arr = [0, 0, 0, 1];
	private requestRedraw = false;
	private initializeSkeletonOnFirstTick = false;

	private spineBounds = {
		x: 0,
		y: 0,
		width: 200,
		height: 200,
	};

	private verticesTemp = spine.Utils.newFloatArray(2 * 1024);

	private boneFollowers = new Map<string, BoneFollower[]>();

	private bonesOverride: Map<Bone, BoneOverride> = new Map();

	private dragHandles = new Set<{
		slot?: Slot,
		bone: Bone,
		dragging?: boolean,
		debug: boolean,
		radius: number,
		offsetX: number,
		offsetY: number,
	}>();
	private prevLeftClickDown = false;

	constructor () {
		super();

		const properties = this._getInitProperties();
		if (properties) {
			this.propAtlas = properties[0] as string;
			this.propSkel = properties[1] as string;
			this.propLoaderScale = properties[2] as number;
			const skinProp = properties[3] as string;
			this.propSkin = skinProp === "" ? [] : skinProp.split(",");
			const animationProp = properties[4] as string;
			this.propAnimation = animationProp === "" ? undefined : animationProp;
			this.propDebugSkeleton = properties[5] as boolean;
			this.propEnableCollision = properties[6] as boolean;
			const boundsProviderIndex = properties[7] as number;
			this.propBoundsProvider = boundsProviderIndex === 0 ? "setup" : "animation-skin";
			// properties[8] is PROP_BOUNDS_PROVIDER_MOVE
			this.propOffsetX = properties[9] as number;
			this.propOffsetY = properties[10] as number;
			this.propOffsetAngle = properties[11] as number;
			this.propScaleX = properties[12] as number;
			this.propScaleY = properties[13] as number;
			this.propSlotZOffset = (properties[14] as number | undefined) ?? 0;
		}

		if (typeof this.propAtlas !== "string" || typeof this.propSkel !== "string" || !this.propAtlas || !this.propSkel)
			throw new Error("Spine atlas and skeleton files must be configured.");
		if (!this.propAtlas.toLowerCase().endsWith(".atlas"))
			throw new Error(`Invalid Spine atlas file: ${this.propAtlas}`);
		if (!/\.(json|skel)$/i.test(this.propSkel))
			throw new Error(`Invalid Spine skeleton file: ${this.propSkel}`);
		if (!Number.isFinite(this.propLoaderScale) || this.propLoaderScale <= 0)
			throw new Error("Spine loader scale must be a finite number greater than zero.");

		this.collisionSpriteClassName = `${this.objectType.name}_CollisionBody`;

		this.assetLoader = new spine.AssetLoader();
		this.matrix = new spine.C3Matrix();
		this.renderer ||= this.runtime.renderer;
		this.initializeCachedSpine(false);

		this._setTicking(true);
	}

	/*
	 *  Update, render, draw
	 */

	public play () {
		this.isPlaying = true;
	}

	public stop () {
		this.isPlaying = false;
	}

	private updateMatrix () {
		this.quaternion = this.getQuaternion();
		this.matrix.update(
			this.x + this.propOffsetX,
			this.y + this.propOffsetY,
			this.totalZ,
			this.angle,
			this.width / this.spineBounds.width * this.propScaleX,
			this.height / this.spineBounds.height * this.propScaleY,
			this.quaternion,
			this.propOffsetAngle);
	}

	_tick (): void {
		this.renderer ||= this.runtime.renderer;
		if (!this.renderer) return;

		if (this.initializeSkeletonOnFirstTick) {
			this.initializeSkeletonOnFirstTick = false;
			this.completeSkeletonInitialization();
		}

		if (!this.skeletonLoaded && !this.loadStarted) {
			if (!this.initializeCachedSpine(true)) {
				this.loadSpine();
				return;
			}
		}
		if (!this.skeletonLoaded) return;

		this.updateMatrix();
		this.updateObjectFromCollisionBody();

		if (this.isPlaying)
			this.update(this.dt);
		else
			this.updateCollisionSprite();
	}

	private update (delta: number) {
		const { state, skeleton, physicsMode, matrix } = this;

		if (!skeleton || !state) return;

		state.update(delta);
		skeleton.update(delta);
		state.apply(skeleton);

		this.updateHandles(skeleton, matrix);

		this.updateBonesOverride();

		skeleton.updateWorldTransform(physicsMode);

		this.updateCollisionSprite();
		this.updateBoneFollowers(matrix);

		this.invalidateRender();
	}

	private invalidateRender () {
		this.requestRedraw = true;
		this.runtime.sdk.updateRender();
	}

	private refreshPose (applyAnimationState = false) {
		const { skeleton, state, matrix } = this;
		if (!skeleton) return;

		if (applyAnimationState && state) {
			state.update(0);
			state.apply(skeleton);
		}
		this.updateMatrix();
		this.updateObjectFromCollisionBody();
		this.updateBonesOverride();
		skeleton.updateWorldTransform(this.physicsMode === spine.Physics.none ? spine.Physics.none : spine.Physics.pose);
		this.updateCollisionSprite();
		this.updateBoneFollowers(matrix);
		this.invalidateRender();
	}

	_draw (renderer: IRenderer) {
		this.renderer ||= renderer;

		if (!this.isVisible) return;
		if (!this.isOnScreen()) return;

		const { skeleton } = this;
		if (!skeleton) return;

		this.skeletonRenderer ||= new spine.C3RendererRuntime(renderer, this.matrix);
		const slotZOffset = this.layer.renderingMode === "3d" ? this.propSlotZOffset : 0;
		this.skeletonRenderer.draw(skeleton, this.colorRgb, this.opacity, this.requestRedraw, slotZOffset);
		this.requestRedraw = false;

		if (this.propDebugSkeleton) this.skeletonRenderer.drawDebug(skeleton, this.x, this.y, this.getBoundingQuad(false));
		this.renderCollisionBoundingBoxDebug(renderer);
		this.renderDragHandles();
	}

	private renderCollisionBoundingBoxDebug (renderer: IRenderer) {
		const points = this.collisionBoundingBoxGamePoints;
		if (!this.collisionBoundingBoxDebug || points.length < 6 || !this.collisionSpriteInstance?.isCollisionEnabled) return;

		renderer.setColorFillMode();
		renderer.setColorRgba(1, 0, 0, 1);
		renderer.pushLineWidth(2);
		for (let i = 0; i < points.length; i += 2) {
			const next = (i + 2) % points.length;
			renderer.line(points[i], points[i + 1], points[next], points[next + 1]);
		}
		renderer.popLineWidth();
		renderer.setTextureFillMode();
		renderer.setColorRgba(1, 1, 1, 1);
	}

	private renderDragHandles () {
		for (const { bone, radius, debug } of this.dragHandles) {
			if (!debug) continue;
			this.skeletonRenderer?.renderDragHandles(bone, radius);
		}
	}

	/**********/


	/*
	 *  Drag handles
	 */
	private touchDown = false;
	private touchX = 0;
	private touchY = 0;
	private activeDragPointerId?: number;
	public addDragHandle (type: 0 | 1, name: string, radius = 10, debug = false) {
		if (!Number.isFinite(radius) || radius <= 0)
			throw new Error("[Spine] Handle radius must be a finite number greater than zero.");
		if (!this.skeleton) return;
		if (type === 0) {
			const bone = this.getBone(name);
			if (!bone) throw new Error(`[Spine] Bone not found: ${name}`);
			this.dragHandles.add({ bone, debug, radius, offsetX: 0, offsetY: 0 });
		} else {
			const slot = this.getSlot(name);
			if (!slot) throw new Error(`[Spine] Slot not found: ${name}`);
			this.dragHandles.add({ slot, bone: slot.bone, debug, radius, offsetX: 0, offsetY: 0 });
		}

		if (this.dragHandles.size === 1) {
			this.endDragHandlePointer();
			this.runtime.addEventListener("pointerdown", this.dragHandleDown);
			this.runtime.addEventListener("pointermove", this.dragHandleMove);
			this.runtime.addEventListener("pointerup", this.dragHandleUp);
			this.runtime.addEventListener("pointercancel", this.dragHandleCancel);
		}
		this.isPlaying = true;
		this.invalidateRender();
	}

	private updateDragHandlePointerPosition (event: ConstructPointerEvent) {
		[this.touchX, this.touchY] = this.layer.cssPxToLayer(event.clientX, event.clientY, this.totalZ);
	}

	private dragHandleDown = (event: ConstructPointerEvent) => {
		if (event.button !== 0 || this.activeDragPointerId !== undefined) return;
		this.activeDragPointerId = event.pointerId;
		this.touchDown = true;
		this.updateDragHandlePointerPosition(event);
	};

	private dragHandleMove = (event: ConstructPointerEvent) => {
		if (event.pointerId !== this.activeDragPointerId) return;
		if ((event.buttons & 1) === 0) {
			this.endDragHandlePointer();
			return;
		}
		this.updateDragHandlePointerPosition(event);
	};

	private dragHandleUp = (event: ConstructPointerEvent) => {
		if (event.pointerId === this.activeDragPointerId) this.endDragHandlePointer();
	};

	private dragHandleCancel = (event: ConstructPointerEvent) => {
		if (event.pointerId === this.activeDragPointerId) this.endDragHandlePointer();
	};

	private endDragHandlePointer () {
		this.touchDown = false;
		this.activeDragPointerId = undefined;
		this.prevLeftClickDown = false;
		for (const handle of this.dragHandles) handle.dragging = false;
	}

	private dragHandleDispose () {
		this.runtime.removeEventListener("pointerdown", this.dragHandleDown);
		this.runtime.removeEventListener("pointermove", this.dragHandleMove);
		this.runtime.removeEventListener("pointerup", this.dragHandleUp);
		this.runtime.removeEventListener("pointercancel", this.dragHandleCancel);
		this.endDragHandlePointer();
	}

	public removeDragHandle (type: 0 | 1, name: string) {
		if (!this.skeleton) return;
		if (type === 0) {
			const bone = this.getBone(name);
			if (!bone) throw new Error(`[Spine] Bone not found: ${name}`);

			for (const handle of this.dragHandles) {
				if (handle.bone === bone && !handle.slot) {
					this.dragHandles.delete(handle);
					break;
				}
			}
		} else {
			const slot = this.getSlot(name);
			if (!slot) throw new Error(`[Spine] Slot not found: ${name}`);

			for (const handle of this.dragHandles) {
				if (handle.slot === slot) {
					this.dragHandles.delete(handle);
					break;
				}
			}
		}

		if (this.dragHandles.size === 0) this.dragHandleDispose();
		this.invalidateRender();
	}

	private updateHandles (skeleton: Skeleton, matrix: C3Matrix) {
		if (this.dragHandles.size === 0) return;

		const { touchDown } = this;
		if (!touchDown) {
			if (this.prevLeftClickDown) {
				this.prevLeftClickDown = false;
				for (const handleObject of this.dragHandles) handleObject.dragging = false;
			}
			return;
		}

		const { touchX, touchY } = this;
		for (const handleObject of this.dragHandles) {
			const bone = handleObject.bone;

			if (handleObject.dragging) {
				const position = matrix.gameToBone(touchX - handleObject.offsetX, touchY - handleObject.offsetY, bone);
				if (!Number.isFinite(position.x) || !Number.isFinite(position.y)) continue;
				bone.pose.x = position.x;
				bone.pose.y = position.y;
			} else if (!this.prevLeftClickDown) {
				const { x: boneGameX, y: boneGameY } = matrix.boneToGame(bone);
				const inside = handleObject.slot
					? this.isInsideSlot(touchX, touchY, handleObject.slot)
					: this.inRadius(touchX, touchY, boneGameX, boneGameY, handleObject.radius);
				if (inside) {
					handleObject.dragging = true;
					handleObject.offsetX = touchX - boneGameX;
					handleObject.offsetY = touchY - boneGameY;
				}
			}
		}

		this.prevLeftClickDown = true;
	}

	public isInsideSlot (x: number, y: number, slotName: string | Slot, skeletonCoordinate = false) {
		const slot = this.getSlot(slotName);
		if (!slot || !slot.bone.active) return false;

		const attachment = slot.appliedPose.attachment;
		if (!(attachment instanceof spine.RegionAttachment || attachment instanceof spine.MeshAttachment)) return false;

		const verticesLength = attachment instanceof spine.MeshAttachment ? attachment.worldVerticesLength : 8;
		if (this.verticesTemp.length < verticesLength) this.verticesTemp = spine.Utils.newFloatArray(verticesLength);
		const vertices = this.verticesTemp;
		let hullLength = 8;

		if (attachment instanceof spine.RegionAttachment) {
			attachment.computeWorldVertices(slot, attachment.getOffsets(slot.appliedPose), vertices, 0, 2);
		} else if (attachment instanceof spine.MeshAttachment) {
			attachment.computeWorldVertices(this.skeleton as Skeleton, slot, 0, attachment.worldVerticesLength, vertices, 0, 2);
			hullLength = attachment.hullLength;
		}

		if (skeletonCoordinate) return this.isPointInPolygon(vertices, hullLength, x, y);

		this.updateMatrix();
		const coords = this.matrix.gameToSkeleton(x, y);
		return Number.isFinite(coords.x) && Number.isFinite(coords.y)
			? this.isPointInPolygon(vertices, hullLength, coords.x, coords.y)
			: false;
	}

	public isInsideBone (x: number, y: number, boneName: string, radius: number) {
		const bone = this.getBone(boneName);
		if (!bone || !bone.active) return false;

		this.updateMatrix();
		const bonePos = this.matrix.boneToGame(bone);
		return this.inRadius(x, y, bonePos.x, bonePos.y, radius);
	}

	public isAnimationPlaying (animationName: string, trackIndex: number) {
		if (!this.state) return false;

		if (trackIndex === -1) {
			for (const track of this.state.tracks) {
				if (!track) continue;
				if (animationName === "" || track.animation?.name === animationName) return true;
			}
			return false;
		}

		const track = this.state.getTrack(trackIndex);
		if (!track) return false;

		if (animationName === "") return true;

		return track.animation?.name === animationName;
	}

	private isPointInPolygon (vertices: NumberArrayLike, hullLength: number, px: number, py: number) {
		if (hullLength < 6) {
			throw new Error("A polygon must have at least 3 vertices (6 numbers in the array).");
		}

		let isInside = false;

		for (let i = 0, j = hullLength - 2; i < hullLength; i += 2) {
			const xi = vertices[i], yi = vertices[i + 1];
			const xj = vertices[j], yj = vertices[j + 1];

			const intersects = ((yi > py) !== (yj > py)) &&
				(px < ((xj - xi) * (py - yi)) / (yj - yi) + xi);

			if (intersects) isInside = !isInside;

			j = i;
		}

		return isInside;
	}

	private inRadius (x1: number, y1: number, x2: number, y2: number, radius: number) {
		const dx = x1 - x2;
		const dy = y1 - y2;
		const distanceSquared = dx * dx + dy * dy;
		return distanceSquared <= radius * radius;
	}

	/**********/

	/*
	*  C3 Internals
	*/

	_saveToJson () {
		// Save/load support is deferred; Spine-specific mutable runtime state is not persisted yet.
		return {};
	}

	_loadFromJson (o: JSONValue) {
		// Save/load support is deferred; Spine-specific mutable runtime state is not persisted yet.
	}

	_release () {
		super._release();
		this.assetLoader.releaseInstanceResources(this);
		this.textureAtlas = undefined;
		this.renderer = undefined;
		this.skeleton = undefined;
		this.state = undefined;
		this.pendingAnimationEvents.length = 0;
		this.dragHandleDispose();

		const collisionSpriteInstance = this.collisionSpriteInstance;
		if (collisionSpriteInstance) {
			collisionSpriteInstance.removeEventListener("destroy", this.collisionSpriteDestroyed);
			this.clearCollisionSprite();
			collisionSpriteInstance.destroy();
		}
	}

	/**********/

	/*
	*  Spine Internals
	*/

	private initializeCachedSpine (triggerLoaded: boolean) {
		if (this.skeletonLoaded || !this.renderer) return false;

		const cached = this.assetLoader.getCachedRuntimeSkeletonAndAtlas(this.propSkel, this.propAtlas, this, this.propLoaderScale);
		if (!cached) return false;

		this.loadStarted = true;
		this.initializeSkeleton(cached.textureAtlas, cached.skeletonData, triggerLoaded);
		return true;
	}

	private async loadSpine () {
		const { renderer, propAtlas, propSkel, propLoaderScale } = this;
		if (this.loadStarted || this.skeletonLoaded || !renderer) return;

		this.loadStarted = true;
		if (!propAtlas || !propSkel) return;

		const textureAtlas = await this.assetLoader.loadAtlasRuntime(propAtlas, this, this.plugin.runtime, renderer);
		if (this.renderer !== renderer) return;

		this.textureAtlas = textureAtlas;
		const skeletonData = await this.assetLoader.loadSkeletonRuntime(propSkel, propAtlas, textureAtlas, this, propLoaderScale, this.plugin.runtime);
		if (this.renderer !== renderer) return;

		this.initializeSkeleton(textureAtlas, skeletonData, true);
	}

	private initializeSkeleton (textureAtlas: TextureAtlas, skeletonData: SkeletonData, triggerLoaded: boolean) {
		this.textureAtlas = textureAtlas;

		this.skeleton = new spine.Skeleton(skeletonData);
		const animationStateData = new spine.AnimationStateData(skeletonData);
		this.state = new spine.AnimationState(animationStateData);
		this.state.addListener({
			start: (entry) => this.triggerAnimationEvent("start", entry.trackIndex, entry.animation?.name ?? ""),
			dispose: (entry) => this.triggerAnimationEvent("dispose", entry.trackIndex, entry.animation?.name ?? ""),
			event: (entry, event) => this.triggerAnimationEvent("event", entry.trackIndex, entry.animation?.name ?? "", event),
			interrupt: (entry) => this.triggerAnimationEvent("interrupt", entry.trackIndex, entry.animation?.name ?? ""),
			end: (entry) => this.triggerAnimationEvent("end", entry.trackIndex, entry.animation?.name ?? ""),
			complete: (entry) => this.triggerAnimationEvent("complete", entry.trackIndex, entry.animation?.name ?? ""),
		});

		if (triggerLoaded)
			this.completeSkeletonInitialization();
		else
			this.initializeSkeletonOnFirstTick = true;
	}

	private completeSkeletonInitialization () {
		this.initializeInitialSkeletonState();
		this.skeletonLoaded = true;
		const pendingEvents = this.pendingAnimationEvents.splice(0);
		for (const { eventName, track, animation, event } of pendingEvents)
			this.triggerAnimationEvent(eventName, track, animation, event);
		this._trigger(C3.Plugins.EsotericSoftware_SpineConstruct3.Cnds.OnSkeletonLoaded);
	}

	private initializeInitialSkeletonState () {
		this._setSkin();
		if (this.propAnimation && this.state) {
			this.state.setAnimation(0, this.propAnimation, true);
			this.isPlaying = true;
		}
		this.calculateBounds();
		this.updateMatrix();
		this.createCollisionSprite();
		this.update(0);
	}

	private createCollisionSprite () {
		if (!this.propEnableCollision) return;

		const objectType = (this.runtime.objects as Record<string, IObjectType<IWorldInstance>>)[this.collisionSpriteClassName];
		if (!objectType)
			throw new Error(`[Spine] Collision sprite object type "${this.collisionSpriteClassName}" not found`);

		this.collisionSpriteInstance = objectType.createInstance(this.layer.name, this.x, this.y);
		this.collisionSpriteInstance.addEventListener("destroy", this.collisionSpriteDestroyed);
		this.syncCollisionSprite3DTransform(this.collisionSpriteInstance, this.x, this.y);
		this.collisionSpriteInstance.angle = this.angle;
		this.collisionSpriteInstance.setOrigin(this.originX, this.originY);
	}

	private collisionSpriteDestroyed = (event: InstanceDestroyEvent<IWorldInstance>) => {
		if (event.instance !== this.collisionSpriteInstance) return;
		this.clearCollisionSprite();
		this.invalidateRender();
	};

	private clearCollisionSprite () {
		this.collisionSpriteInstance = undefined;
		this.collisionBoundingBoxMeshSize = [0, 0];
		this.collisionBoundingBoxGamePoints.length = 0;
		this.collisionBodyDrivesObject = false;
	}

	private syncCollisionSprite3DTransform (collisionSpriteInstance: IWorldInstance, x: number, y: number, syncQuaternion = true) {
		collisionSpriteInstance.setPosition3d(x, y, this.z);
		if (syncQuaternion) {
			collisionSpriteInstance.setQuaternion(...this.quaternion);
		} else {
			collisionSpriteInstance.setQuaternion(0, 0, 0, 1);
		}
	}

	private captureCollisionBodyTransform (collisionSpriteInstance: IWorldInstance) {
		this.collisionBodyPrevX = collisionSpriteInstance.x;
		this.collisionBodyPrevY = collisionSpriteInstance.y;
		this.collisionBodyPrevAngle = collisionSpriteInstance.angle;
	}

	private updateCollisionSprite () {
		if (!this.collisionSpriteInstance) return;

		if (this.collisionBoundingBoxSlotName && this.collisionBoundingBoxAttachmentName) {
			this.updateCollisionBoundingBoxSprite();
			return;
		}

		if (this.collisionBoundingBoxMeshSize[0] !== 0) {
			this.collisionSpriteInstance.releaseMesh();
			this.collisionBoundingBoxMeshSize = [0, 0];
		}

		this.collisionSpriteInstance.isCollisionEnabled = true;
		this.syncCollisionSprite3DTransform(this.collisionSpriteInstance, this.x, this.y);
		this.collisionSpriteInstance.setSize(this.width, this.height);
		this.collisionSpriteInstance.angle = this.angle;
		this.collisionSpriteInstance.setOrigin(this.originX, this.originY);
		if (this.collisionBodyDrivesObject) this.captureCollisionBodyTransform(this.collisionSpriteInstance);
	}

	private updateCollisionBoundingBoxSprite () {
		const { skeleton, collisionSpriteInstance } = this;
		if (!skeleton || !collisionSpriteInstance) return;

		const slot = this.collisionBoundingBoxSlot ?? skeleton.findSlot(this.collisionBoundingBoxSlotName) ?? undefined;
		this.collisionBoundingBoxSlot = slot;
		if (!slot || !slot.bone.active) {
			collisionSpriteInstance.isCollisionEnabled = false;
			return;
		}

		const attachment = slot.appliedPose.attachment;
		if (!(attachment instanceof spine.BoundingBoxAttachment) || attachment.name !== this.collisionBoundingBoxAttachmentName) {
			collisionSpriteInstance.isCollisionEnabled = false;
			return;
		}

		const vertexCount = attachment.worldVerticesLength >> 1;
		if (vertexCount < 3) {
			collisionSpriteInstance.isCollisionEnabled = false;
			return;
		}

		if (this.collisionBoundingBoxVertices.length < attachment.worldVerticesLength)
			this.collisionBoundingBoxVertices = spine.Utils.newFloatArray(attachment.worldVerticesLength);

		attachment.computeWorldVertices(skeleton, slot, 0, attachment.worldVerticesLength, this.collisionBoundingBoxVertices, 0, 2);

		let minX = Number.POSITIVE_INFINITY;
		let minY = Number.POSITIVE_INFINITY;
		let maxX = Number.NEGATIVE_INFINITY;
		let maxY = Number.NEGATIVE_INFINITY;
		const gamePoints = this.collisionBoundingBoxGamePoints;
		gamePoints.length = 0;
		for (let i = 0; i < attachment.worldVerticesLength; i += 2) {
			const point = this.matrix.skeletonToGame(this.collisionBoundingBoxVertices[i], this.collisionBoundingBoxVertices[i + 1]);
			const x = point.x;
			const y = point.y;
			gamePoints.push(x, y);
			minX = Math.min(minX, x);
			minY = Math.min(minY, y);
			maxX = Math.max(maxX, x);
			maxY = Math.max(maxY, y);
		}

		const width = Math.max(maxX - minX, 1);
		const height = Math.max(maxY - minY, 1);
		const meshWidth = Math.max(2, Math.ceil(vertexCount / 2));
		const meshHeight = 2;
		if (this.collisionBoundingBoxMeshSize[0] !== meshWidth || this.collisionBoundingBoxMeshSize[1] !== meshHeight) {
			collisionSpriteInstance.createMesh(meshWidth, meshHeight);
			this.collisionBoundingBoxMeshSize = [meshWidth, meshHeight];
		}

		collisionSpriteInstance.isCollisionEnabled = true;
		// These mesh points are already transformed to game-space by the legacy
		// 2D matrix, so applying the quaternion again would double-transform them.
		this.syncCollisionSprite3DTransform(collisionSpriteInstance, minX, minY, false);
		collisionSpriteInstance.setSize(width, height);
		collisionSpriteInstance.angle = 0;
		collisionSpriteInstance.setOrigin(0, 0);
		if (this.collisionBodyDrivesObject) this.captureCollisionBodyTransform(collisionSpriteInstance);

		const perimeterPointCount = meshWidth * 2;
		for (let i = 0; i < perimeterPointCount; i++) {
			const sourceIndex = Math.min(i, vertexCount - 1) * 2;
			const meshX = (gamePoints[sourceIndex] - minX) / width;
			const meshY = (gamePoints[sourceIndex + 1] - minY) / height;
			if (i < meshWidth) {
				collisionSpriteInstance.setMeshPoint(i, 0, { mode: "absolute", x: meshX, y: meshY });
			} else {
				collisionSpriteInstance.setMeshPoint(perimeterPointCount - 1 - i, 1, { mode: "absolute", x: meshX, y: meshY });
			}
		}
	}

	public setCollisionBoundingBox (slotName: string, attachmentName: string) {
		const skeleton = this.skeleton;
		if (skeleton) {
			const slot = skeleton.findSlot(slotName);
			if (!slot) throw new Error(`[Spine] Slot not found: ${slotName}`);
			const attachment = skeleton.getAttachment(slot.data.index, attachmentName);
			if (!(attachment instanceof spine.BoundingBoxAttachment))
				throw new Error(`[Spine] Bounding box attachment not found: ${attachmentName}, for slot: ${slotName}`);
			skeleton.setAttachment(slotName, attachmentName);
			this.collisionBoundingBoxSlot = slot;
		}
		this.collisionBoundingBoxSlotName = slotName;
		this.collisionBoundingBoxAttachmentName = attachmentName;
		this.refreshPose();
	}

	public clearCollisionBoundingBox () {
		this.collisionBoundingBoxSlotName = "";
		this.collisionBoundingBoxAttachmentName = "";
		this.collisionBoundingBoxSlot = undefined;
		this.collisionBoundingBoxGamePoints.length = 0;
		this.refreshPose();
	}

	public setCollisionBoundingBoxDebug (enabled: boolean) {
		this.collisionBoundingBoxDebug = enabled;
		this.invalidateRender();
	}

	public hasCollisionBody () {
		return !!this.collisionSpriteInstance;
	}

	public getCollisionBodyUid () {
		return this.collisionSpriteInstance?.uid ?? -1;
	}

	private getBoundingBoxInfo (slotName: string, attachmentName: string) {
		const { skeleton } = this;
		if (!skeleton || !slotName || !attachmentName) return null;

		const slot = skeleton.findSlot(slotName);
		if (!slot || !slot.bone.active) return null;

		const attachment = skeleton.getAttachment(slot.data.index, attachmentName);
		if (!(attachment instanceof spine.BoundingBoxAttachment)) return null;

		const vertexCount = attachment.worldVerticesLength >> 1;
		if (vertexCount < 3) return null;

		if (this.collisionBoundingBoxVertices.length < attachment.worldVerticesLength)
			this.collisionBoundingBoxVertices = spine.Utils.newFloatArray(attachment.worldVerticesLength);

		attachment.computeWorldVertices(skeleton, slot, 0, attachment.worldVerticesLength, this.collisionBoundingBoxVertices, 0, 2);
		this.updateMatrix();

		let left = Number.POSITIVE_INFINITY;
		let top = Number.POSITIVE_INFINITY;
		let right = Number.NEGATIVE_INFINITY;
		let bottom = Number.NEGATIVE_INFINITY;
		const points: number[] = [];
		for (let i = 0; i < attachment.worldVerticesLength; i += 2) {
			const point = this.matrix.skeletonToGame(this.collisionBoundingBoxVertices[i], this.collisionBoundingBoxVertices[i + 1]);
			points.push(point.x, point.y);
			left = Math.min(left, point.x);
			top = Math.min(top, point.y);
			right = Math.max(right, point.x);
			bottom = Math.max(bottom, point.y);
		}

		return { points, left, top, right, bottom };
	}

	public getBoundingBoxPointCount (slotName: string, attachmentName: string) {
		return (this.getBoundingBoxInfo(slotName, attachmentName)?.points.length ?? 0) / 2;
	}

	public getBoundingBoxPointX (slotName: string, attachmentName: string, index: number) {
		const points = this.getBoundingBoxInfo(slotName, attachmentName)?.points;
		const pointIndex = Math.floor(index) * 2;
		return points && pointIndex >= 0 && pointIndex < points.length ? points[pointIndex] : 0;
	}

	public getBoundingBoxPointY (slotName: string, attachmentName: string, index: number) {
		const points = this.getBoundingBoxInfo(slotName, attachmentName)?.points;
		const pointIndex = Math.floor(index) * 2 + 1;
		return points && pointIndex >= 1 && pointIndex < points.length ? points[pointIndex] : 0;
	}

	public getBoundingBoxCenterX (slotName: string, attachmentName: string) {
		const info = this.getBoundingBoxInfo(slotName, attachmentName);
		return info ? (info.left + info.right) / 2 : 0;
	}

	public getBoundingBoxCenterY (slotName: string, attachmentName: string) {
		const info = this.getBoundingBoxInfo(slotName, attachmentName);
		return info ? (info.top + info.bottom) / 2 : 0;
	}

	public getBoundingBoxLeft (slotName: string, attachmentName: string) {
		return this.getBoundingBoxInfo(slotName, attachmentName)?.left ?? 0;
	}

	public getBoundingBoxTop (slotName: string, attachmentName: string) {
		return this.getBoundingBoxInfo(slotName, attachmentName)?.top ?? 0;
	}

	public getBoundingBoxRight (slotName: string, attachmentName: string) {
		return this.getBoundingBoxInfo(slotName, attachmentName)?.right ?? 0;
	}

	public getBoundingBoxBottom (slotName: string, attachmentName: string) {
		return this.getBoundingBoxInfo(slotName, attachmentName)?.bottom ?? 0;
	}

	public getBoundingBoxPolygonJson (slotName: string, attachmentName: string) {
		return JSON.stringify(this.getBoundingBoxInfo(slotName, attachmentName)?.points ?? []);
	}

	public setRuntimeAssetCacheRetainedWhenUnused (enabled: boolean, scope: "object-type" | "all") {
		if (scope === "all") {
			this.assetLoader.setAllRuntimeResourcesRetained(enabled);
		} else {
			this.assetLoader.retainInstanceResources(this.propSkel, this.propAtlas, this.propLoaderScale, enabled);
		}
	}

	public releaseCachedSpineAssets () {
		this.assetLoader.releaseRetainedInstanceResources(this.propSkel, this.propAtlas, this.propLoaderScale);
	}

	public releaseAllCachedSpineAssets () {
		this.assetLoader.releaseAllUnusedRuntimeResources();
	}

	public setCollisionBodyDrivesObject (enabled: boolean) {
		if (enabled === this.collisionBodyDrivesObject) return;

		this.updateMatrix();
		if (enabled) {
			this.collisionBodyDrivesObject = false;
			this.updateCollisionSprite();
			this.collisionBodyDrivesObject = true;
			if (this.collisionSpriteInstance) {
				this.captureCollisionBodyTransform(this.collisionSpriteInstance);
			} else {
				this.collisionBodyPrevX = this.x;
				this.collisionBodyPrevY = this.y;
				this.collisionBodyPrevAngle = this.angle;
			}
		} else {
			this.collisionBodyDrivesObject = false;
			this.updateCollisionSprite();
		}
	}

	private updateObjectFromCollisionBody () {
		const collisionSpriteInstance = this.collisionSpriteInstance;
		if (!this.collisionBodyDrivesObject || !collisionSpriteInstance) return;

		const dx = collisionSpriteInstance.x - this.collisionBodyPrevX;
		const dy = collisionSpriteInstance.y - this.collisionBodyPrevY;
		const angle = collisionSpriteInstance.angle;
		const angleChanged = angle !== this.collisionBodyPrevAngle;
		this.captureCollisionBodyTransform(collisionSpriteInstance);
		if (dx === 0 && dy === 0 && !angleChanged) return;

		if (dx !== 0 || dy !== 0) this.offsetPosition(dx, dy);
		if (angleChanged) this.angle = angle;
		this.updateMatrix();
	}

	private calculateBounds () {
		const { skeleton } = this;
		if (!skeleton) return;

		let boundsProvider: SpineBoundsProvider;
		if (this.propBoundsProvider === "animation-skin") {
			const { propSkin, propAnimation } = this;
			if ((propSkin && propSkin.length > 0) || propAnimation) {
				boundsProvider = new spine.SkinsAndAnimationBoundsProvider(propAnimation, propSkin);
			} else {
				boundsProvider = new spine.SetupPoseBoundsProvider();
			}
		} else if (this.propBoundsProvider === "setup") {
			boundsProvider = new spine.SetupPoseBoundsProvider();
		} else {
			boundsProvider = new spine.AABBRectangleBoundsProvider(0, 0, 100, 100);
		}

		const bounds = boundsProvider.calculateBounds(this);
		if (spine.isValidBounds(bounds)) this.spineBounds = bounds;
	}
	/**********/

	/*
	*  Animations
	*/

	public getAnimations () {
		return this.skeleton?.data.animations.map(animation => animation.name).join("\n") ?? "";
	}

	public getAnimationsCount () {
		return this.skeleton?.data.animations.length ?? 0;
	}

	public getAnimationName (index: number) {
		return this.skeleton?.data.animations[Math.floor(index)]?.name ?? "";
	}

	public setAnimation (track: number, animation: string, loop = false, additive = false) {
		if (!Number.isInteger(track) || track < 0) throw new Error("[Spine] Track index must be a non-negative integer.");
		const { state } = this;
		if (!state) return;
		const animationData = state.data.skeletonData.findAnimation(animation);
		if (!animationData) throw new Error(`[Spine] Animation not found: ${animation}`);
		const entry = state.setAnimation(track, animationData, loop);
		entry.additive = additive;
		this.isPlaying = true;
		this.refreshPose(true);
	}

	public addAnimation (track: number, animation: string, loop = false, delay = 0, additive = false) {
		if (!Number.isInteger(track) || track < 0) throw new Error("[Spine] Track index must be a non-negative integer.");
		if (!Number.isFinite(delay)) throw new Error("[Spine] Animation delay must be finite.");
		const { state } = this;
		if (!state) return;
		const animationData = state.data.skeletonData.findAnimation(animation);
		if (!animationData) throw new Error(`[Spine] Animation not found: ${animation}`);
		const entry = state.addAnimation(track, animationData, loop, delay);
		entry.additive = additive;
		this.isPlaying = true;
		this.refreshPose(true);
	}

	public addEmptyAnimation (track: number, mixDuration: number, delay: number) {
		if (!Number.isInteger(track) || track < 0) throw new Error("[Spine] Track index must be a non-negative integer.");
		if (!Number.isFinite(mixDuration) || mixDuration < 0) throw new Error("[Spine] Mix duration must be finite and non-negative.");
		if (!Number.isFinite(delay)) throw new Error("[Spine] Animation delay must be finite.");
		if (!this.state) return;
		this.state.addEmptyAnimation(track, mixDuration, delay);
		this.isPlaying = true;
		this.refreshPose(true);
	}

	public setEmptyAnimation (track: number, mixDuration: number) {
		if (!Number.isInteger(track) || track < 0) throw new Error("[Spine] Track index must be a non-negative integer.");
		if (!Number.isFinite(mixDuration) || mixDuration < 0) throw new Error("[Spine] Mix duration must be finite and non-negative.");
		if (!this.state) return;
		this.state.setEmptyAnimation(track, mixDuration);
		this.isPlaying = true;
		this.refreshPose(true);
	}

	public getCurrentAnimation (trackIndex: number): string {
		if (!this.skeleton) return "";

		const { state } = this;
		if (!state) return "";

		const track = state.getTrack(trackIndex);
		if (!track || !track.animation) return "";

		return track.animation.name;
	}

	public setTimeScale (track: number, timeScale: number) {
		if (!Number.isInteger(track) || track < -1) throw new Error("[Spine] Track index must be -1 or a non-negative integer.");
		if (!Number.isFinite(timeScale)) throw new Error("[Spine] Time scale must be finite.");
		if (!this.state) return;
		if (track < 0) {
			this.state.timeScale = timeScale;
		} else {
			const entry = this.state.getTrack(track);
			if (entry) entry.timeScale = timeScale;
		}
	}

	public setAnimationTime (units: 0 | 1, time: number, track: number) {
		if (!Number.isInteger(track) || track < 0) throw new Error("[Spine] Track index must be a non-negative integer.");
		if (!Number.isFinite(time)) throw new Error("[Spine] Animation time must be finite.");
		if (!this.state) return;

		const trackEntry = this.state.getTrack(track);
		if (!trackEntry) return;

		if (units === 0) {
			if (time < trackEntry.animationStart || time > trackEntry.animationEnd)
				throw new Error(`[Spine] Animation time must be from ${trackEntry.animationStart} to ${trackEntry.animationEnd}.`);
			trackEntry.trackTime = time;
		} else {
			if (time < 0 || time > 1) throw new Error("[Spine] Animation time ratio must be from 0 to 1.");
			trackEntry.trackTime = trackEntry.animationStart + time * (trackEntry.animationEnd - trackEntry.animationStart);
		}
		this.refreshPose(true);
	}

	public setAnimationMix (fromName: string, toName: string, duration: number) {
		if (!Number.isFinite(duration) || duration < 0) throw new Error("[Spine] Mix duration must be finite and non-negative.");
		const stateData = this.state?.data;
		if (!stateData) return;
		stateData.setMix(fromName, toName, duration);
	}

	public setDefaultMix (duration: number) {
		if (!Number.isFinite(duration) || duration < 0) throw new Error("[Spine] Default mix duration must be finite and non-negative.");
		const stateData = this.state?.data;
		if (!stateData) {
			console.warn('[Spine] setDefaultMix: no state data');
			return;
		}

		stateData.defaultMix = duration;
	}

	public setTrackAlpha (alpha: number, trackIndex: number) {
		if (!Number.isFinite(alpha) || alpha < 0 || alpha > 1)
			throw new Error("[Spine] Track alpha must be a finite number from 0 to 1.");
		if (!Number.isInteger(trackIndex) || trackIndex < 0) throw new Error("[Spine] Track index must be a non-negative integer.");
		const { state } = this;
		if (!state) {
			console.warn('[Spine] setAlpha: no state');
			return;
		}

		const track = state.getTrack(trackIndex);
		if (!track) {
			console.warn(`[Spine] setAlpha: track ${trackIndex} not found`);
			return;
		}

		track.alpha = spine.MathUtils.clamp(alpha, 0, 1);
		this.refreshPose(true);
	}


	public clearTrack (track: number) {
		if (!Number.isInteger(track) || track < -1) throw new Error("[Spine] Track index must be -1 or a non-negative integer.");
		const { state } = this;
		if (!state) return;
		if (track === -1)
			state.clearTracks();
		else
			state.clearTrack(track);
		this.refreshPose(true);
	}

	private triggerAnimationEvent (eventName: string, track: number, animation: string, event?: Event) {
		if (!this.skeletonLoaded) {
			this.pendingAnimationEvents.push({ eventName, track, animation, event });
			return;
		}
		this.triggeredEventTrack = track;
		this.triggeredEventAnimation = animation;
		this.triggeredEventName = eventName;
		this.triggeredEventData = event ? { ...event, track, animation } : undefined;
		this._trigger(C3.Plugins.EsotericSoftware_SpineConstruct3.Cnds.OnAnimationEvent);
	}

	/**********/

	/*
	*  Skins
	*/

	public setSkin (skins: string[]) {
		if (skins.some(skinName => skinName.length === 0)) throw new Error("[Spine] Skin names must not be empty.");
		if (this.skeleton)
			for (const skinName of skins)
				if (!this.skeleton.data.findSkin(skinName))
					throw new Error(`[Spine] Skin not found: ${skinName}`);
		this.propSkin = skins;
		this._setSkin();
		this.refreshPose(true);
	}

	public getCurrentSkin (): string {
		if (!this.skeleton) return "";

		const skin = this.skeleton.skin;
		if (!skin) return "";

		return skin.name;
	}

	private _setSkin () {
		const { skeleton } = this;
		if (!skeleton) return;

		const skins = this.propSkin;

		if (skins.length === 0) {
			skeleton.setSkin(null);
		} else if (skins.length === 1) {
			const skinName = skins[0];
			const skin = skeleton.data.findSkin(skinName);
			if (!skin) throw new Error(`[Spine] Skin not found: ${skinName}`);
			skeleton.setSkin(skins[0]);
		} else {
			const customSkin = new spine.Skin(skins.join(","));
			for (const s of skins) {
				const skin = skeleton.data.findSkin(s);
				if (!skin) throw new Error(`[Spine] Skin not found: ${s}`);
				customSkin.addSkin(skin);
			}
			skeleton.setSkin(customSkin);
		}

		skeleton.setupPose();
	}

	public createCustomSkin (skinName: string) {
		if (skinName.length === 0) throw new Error("[Spine] Custom skin name must not be empty.");
		if (!this.skeleton) return;

		const skin = this.customSkins.get(skinName);
		if (skin)
			skin.clear();
		else
			this.customSkins.set(skinName, new spine.Skin(skinName));
	}

	public addCustomSkin (customSkinName: string, skinToAddName: string) {
		if (!this.skeleton) return;

		const customSkin = this.customSkins.get(customSkinName);
		if (!customSkin) throw new Error(`[Spine] Custom skin not found: ${customSkinName}`);

		const skinToAdd = this.skeleton.data.findSkin(skinToAddName);
		if (!skinToAdd) throw new Error(`[Spine] Skin not found: ${skinToAddName}`);

		customSkin.addSkin(skinToAdd);
	}

	public setCustomSkin (skinName: string) {
		if (!this.skeleton) return;

		const skin = this.customSkins.get(skinName);
		if (!skin) throw new Error(`[Spine] Custom skin not found: ${skinName}`);

		this.skeleton.setSkin(skin);
		this.skeleton.setupPose();
		this.refreshPose(true);
	}

	/**********/

	/*
	*  Slot, skeleton color
	*/

	public setSkeletonColor (color: string) {
		const { skeleton } = this;
		if (!skeleton) {
			console.warn('[Spine] setSkeletonColor: no skeleton');
			return;
		}

		skeleton.color.setFromString(color);
		this.invalidateRender();
	}

	public setSlotColor (slotName: string, color: string) {
		const { skeleton } = this;
		if (!skeleton) {
			console.warn('[Spine] setSlotColor: no skeleton');
			return;
		}

		const slot = skeleton.findSlot(slotName);
		if (!slot) throw new Error(`[Spine] Slot not found: ${slotName}`);

		slot.pose.color.setFromString(color);
		this.refreshPose();
	}

	public resetSlotColors (slotName: string = "") {
		const { skeleton } = this;
		if (!skeleton) {
			console.warn('[Spine] resetSlotColors: no skeleton');
			return;
		}

		if (slotName === "") {
			for (const slot of skeleton.slots)
				slot.pose.color.setFromColor(slot.data.setupPose.color);
		} else {
			const slot = skeleton.findSlot(slotName);
			if (!slot) throw new Error(`[Spine] Slot not found: ${slotName}`);
			slot.pose.color.setFromColor(slot.data.setupPose.color);
		}
		this.refreshPose();
	}

	/**********/

	/*
	*  Bone follower
	*/

	public attachInstanceToBone (uid: number, boneName: string, offsetX = 0, offsetY = 0, offsetAngle = 0, offsetScaleX = 1, offsetScaleY = 1) {
		if (!Number.isSafeInteger(uid) || uid < 0) throw new Error("[Spine] UID must be a non-negative safe integer.");
		if (![offsetX, offsetY, offsetAngle, offsetScaleX, offsetScaleY].every(Number.isFinite))
			throw new Error("[Spine] Bone follower offsets and scales must be finite.");
		if (!this.skeleton) return;

		const bone = this.skeleton.findBone(boneName);
		if (!bone) throw new Error(`[Spine] Bone not found: ${boneName}`);

		const target = this.runtime.getInstanceByUid(uid);
		if (!target) throw new Error(`[Spine] Instance not found for UID: ${uid}`);
		if (!("width" in target) || !("height" in target) || !("setSize" in target) || typeof target.setSize !== "function")
			throw new Error(`[Spine] Instance for UID ${uid} is not a world instance.`);
		const instance = target as IWorldInstance;

		this.updateMatrix();
		this.updateBonesOverride();
		this.skeleton.updateWorldTransform(this.physicsMode === spine.Physics.none ? spine.Physics.none : spine.Physics.pose);

		const refGameScale = this.getSkeletonGameScale(this.matrix);
		const existingFollowers = this.boneFollowers.get(boneName);
		let existingFollower: BoneFollower | undefined;
		if (existingFollowers)
			for (const follower of existingFollowers)
				if (follower.uid === uid) existingFollower = follower;

		const follower: BoneFollower = {
			uid, offsetX, offsetY, offsetAngle, offsetScaleX, offsetScaleY,
			originalWidth: existingFollower?.originalWidth ?? Math.abs(instance.width),
			originalHeight: existingFollower?.originalHeight ?? Math.abs(instance.height),
			refGameScaleX: existingFollower?.refGameScaleX ?? refGameScale.scaleX,
			refGameScaleY: existingFollower?.refGameScaleY ?? refGameScale.scaleY,
		};
		this.detachInstanceFromBoneByUid(uid, boneName);
		const remainingFollowers = this.boneFollowers.get(boneName);
		if (remainingFollowers)
			remainingFollowers.push(follower);
		else
			this.boneFollowers.set(boneName, [follower]);

		this.updateBoneFollowers(this.matrix);
		this.isPlaying = true;
	}

	public detachInstanceFromBoneByUid (uid: number, boneName: string) {
		const followers = this.boneFollowers.get(boneName);
		if (!followers) return;

		for (let i = followers.length - 1; i >= 0; i--)
			if (followers[i].uid === uid) followers.splice(i, 1);

		if (followers.length === 0) this.boneFollowers.delete(boneName);
	}

	public detachAllFromBone (boneName: string) {
		this.boneFollowers.delete(boneName);
	}

	private getSkeletonGameScale (matrix: C3Matrix) {
		return {
			scaleX: Math.hypot(matrix.a, matrix.b),
			scaleY: Math.hypot(matrix.c, matrix.d),
		};
	}

	private getBoneGameTransform (matrix: C3Matrix, bone: Bone): BoneGameTransform {
		const { appliedPose } = bone;

		const xAxisX = matrix.a * appliedPose.a + matrix.c * appliedPose.c;
		const xAxisY = matrix.b * appliedPose.a + matrix.d * appliedPose.c;
		const yAxisX = matrix.a * appliedPose.b + matrix.c * appliedPose.d;
		const yAxisY = matrix.b * appliedPose.b + matrix.d * appliedPose.d;
		const determinant = xAxisX * yAxisY - yAxisX * xAxisY;
		const reflectionSign = determinant > 0 ? -1 : 1;

		return {
			x: matrix.a * appliedPose.worldX + matrix.c * appliedPose.worldY + matrix.tx,
			y: matrix.b * appliedPose.worldX + matrix.d * appliedPose.worldY + matrix.ty,
			xAxisX,
			xAxisY,
			yAxisX,
			yAxisY,
			reflectionSign,
		};
	}

	private getBoneFollowerScaleRatio (scale: number, referenceScale: number) {
		return referenceScale === 0 ? 0 : scale / referenceScale;
	}

	private normalizeDegrees (degrees: number) {
		return ((degrees + 180) % 360 + 360) % 360 - 180;
	}

	private getBoneGameAngleDegrees (transform: BoneGameTransform) {
		if (transform.xAxisX !== 0 || transform.xAxisY !== 0) {
			return Math.atan2(transform.xAxisY, transform.xAxisX) * spine.MathUtils.radDeg;
		}

		if (transform.yAxisX !== 0 || transform.yAxisY !== 0) {
			return Math.atan2(transform.yAxisY, transform.yAxisX) * spine.MathUtils.radDeg + transform.reflectionSign * 90;
		}

		return 0;
	}

	private updateBoneFollowers (matrix: C3Matrix) {
		if (this.boneFollowers.size === 0) return;

		const skeletonGameScale = this.getSkeletonGameScale(matrix);
		const staleFollowers: { uid: number, boneName: string }[] = [];

		for (const [boneName, followers] of this.boneFollowers) {
			const bone = this.skeleton?.findBone(boneName);
			if (!bone) continue;

			const transform = this.getBoneGameTransform(matrix, bone);
			const boneGameAngleDegrees = this.getBoneGameAngleDegrees(transform);
			const offsetAngleSign = transform.reflectionSign;
			const boneWorldScaleX = bone.appliedPose.getWorldScaleX();
			const boneWorldScaleY = bone.appliedPose.getWorldScaleY();

			for (const follower of followers) {
				const instance = this.runtime.getInstanceByUid(follower.uid) as IWorldInstance;
				if (!instance) {
					staleFollowers.push({ uid: follower.uid, boneName });
					continue;
				}

				const sx = this.getBoneFollowerScaleRatio(skeletonGameScale.scaleX, follower.refGameScaleX) * boneWorldScaleX;
				const sy = this.getBoneFollowerScaleRatio(skeletonGameScale.scaleY, follower.refGameScaleY) * boneWorldScaleY * transform.reflectionSign;
				const followerAngleDegrees = this.normalizeDegrees(boneGameAngleDegrees + follower.offsetAngle * offsetAngleSign);
				const followerAngleRad = followerAngleDegrees * spine.MathUtils.degRad;
				const followerAngleCos = Math.cos(followerAngleRad);
				const followerAngleSin = Math.sin(followerAngleRad);

				instance.x = transform.x + follower.offsetX * followerAngleCos - follower.offsetY * followerAngleSin * offsetAngleSign;
				instance.y = transform.y + follower.offsetX * followerAngleSin + follower.offsetY * followerAngleCos * offsetAngleSign;

				instance.angleDegrees = followerAngleDegrees;

				const width = follower.originalWidth * sx * follower.offsetScaleX;
				const height = follower.originalHeight * sy * follower.offsetScaleY;
				instance.setSize(width, height);
			}
		}

		for (const { uid, boneName } of staleFollowers) this.detachInstanceFromBoneByUid(uid, boneName);
	}

	private updateBonesOverride () {
		for (const [bone, override] of this.bonesOverride) {
			this.updateBonePoseOnce(bone, override);
		}
	}

	private updateBonePoseOnce (bone: Bone, boneOverride: BoneOverride) {
		const { mode, x, y, rotation, scaleX, scaleY } = boneOverride;
		if (mode === "game") {
			if (x !== undefined || y !== undefined) {
				const locals = this.matrix.gameToBone(
					x ?? this.matrix.boneToGame(bone).x,
					y ?? this.matrix.boneToGame(bone).y,
					bone);
				if (Number.isFinite(locals.x) && Number.isFinite(locals.y)) {
					bone.pose.x = locals.x;
					bone.pose.y = locals.y;
				}
			}

			if (rotation !== undefined) {
				const localRotation = this.matrix.gameToBoneRotation(rotation, bone);
				if (Number.isFinite(localRotation)) bone.pose.rotation = localRotation;
			}
		}

		if (mode === "local") {
			if (x !== undefined) bone.pose.x = x;
			if (y !== undefined) bone.pose.y = y;
			if (rotation !== undefined) bone.pose.rotation = rotation;
		}

		if (scaleX !== undefined) bone.pose.scaleX = scaleX;
		if (scaleY !== undefined) bone.pose.scaleY = scaleY;
	}

	/**********/

	/*
	*  Bone
	*/

	public getBoneX (boneName: string): number {
		const { skeleton } = this;
		if (!skeleton) {
			console.warn('[Spine] getBoneX: no skeleton');
			return 0;
		}

		const bone = skeleton.findBone(boneName);
		if (!bone) {
			console.warn(`[Spine] getBoneX: bone not found: ${boneName}`);
			return 0;
		}

		const x = bone.appliedPose.worldX;
		const y = bone.appliedPose.worldY;
		const offsetX = this.x + this.propOffsetX;
		const offsetAngle = this.angle + this.propOffsetAngle;

		if (offsetAngle) {
			const cos = Math.cos(offsetAngle);
			const sin = Math.sin(offsetAngle);
			return x * cos - y * sin + offsetX;
		}
		return x + offsetX;
	}

	public getBoneY (boneName: string): number {
		const { skeleton } = this;
		if (!skeleton) {
			console.warn('[Spine] getBoneY: no skeleton');
			return 0;
		}

		const bone = skeleton.findBone(boneName);
		if (!bone) {
			console.warn(`[Spine] getBoneY: bone not found: ${boneName}`);
			return 0;
		}

		const x = bone.appliedPose.worldX;
		const y = bone.appliedPose.worldY;
		const offsetY = this.y + this.propOffsetY;
		const offsetAngle = this.angle + this.propOffsetAngle;

		if (offsetAngle) {
			const cos = Math.cos(offsetAngle);
			const sin = Math.sin(offsetAngle);
			return x * sin + y * cos + offsetY;
		}
		return y + offsetY;
	}

	public getBoneRotation (boneName: string): number {
		const { skeleton } = this;
		if (!skeleton) {
			console.warn('[Spine] getBoneRotation: no skeleton');
			return 0;
		}

		const bone = skeleton.findBone(boneName);
		if (!bone) {
			console.warn(`[Spine] getBoneRotation: bone not found: ${boneName}`);
			return 0;
		}

		const boneRotation = bone.appliedPose.getWorldRotationX();
		const offsetAngle = this.angle + this.propOffsetAngle;

		return boneRotation + (offsetAngle * 180 / Math.PI);
	}

	public getBoneWorldX (boneName: string): number {
		const { skeleton } = this;
		if (!skeleton) {
			console.warn('[Spine] getBoneWorldX: no skeleton');
			return 0;
		}

		const bone = skeleton.findBone(boneName);
		if (!bone) {
			console.warn(`[Spine] getBoneWorldX: bone not found: ${boneName}`);
			return 0;
		}

		this.updateMatrix();
		const point = this.matrix.boneToGame(bone);
		return point.x;
	}

	public getBoneWorldY (boneName: string): number {
		const { skeleton } = this;
		if (!skeleton) {
			console.warn('[Spine] getBoneWorldY: no skeleton');
			return 0;
		}

		const bone = skeleton.findBone(boneName);
		if (!bone) {
			console.warn(`[Spine] getBoneWorldY: bone not found: ${boneName}`);
			return 0;
		}

		this.updateMatrix();
		const point = this.matrix.boneToGame(bone);
		return point.y;
	}

	private getBoneAppliedPose (boneName: string) {
		const { skeleton } = this;
		if (!skeleton) {
			console.warn('[Spine] getBoneAppliedPose: no skeleton');
			return undefined;
		}

		const bone = skeleton.findBone(boneName);
		if (!bone) {
			console.warn(`[Spine] getBoneAppliedPose: bone not found: ${boneName}`);
			return undefined;
		}

		return bone.appliedPose;
	}

	public getBoneLocalX (boneName: string) {
		return this.getBoneAppliedPose(boneName)?.x ?? 0;
	}

	public getBoneLocalY (boneName: string) {
		return this.getBoneAppliedPose(boneName)?.y ?? 0;
	}

	public getBoneLocalRotation (boneName: string) {
		return this.getBoneAppliedPose(boneName)?.rotation ?? 0;
	}

	public getBoneLocalScaleX (boneName: string) {
		return this.getBoneAppliedPose(boneName)?.scaleX ?? 0;
	}

	public getBoneLocalScaleY (boneName: string) {
		return this.getBoneAppliedPose(boneName)?.scaleY ?? 0;
	}

	public getBoneLocalShearX (boneName: string) {
		return this.getBoneAppliedPose(boneName)?.shearX ?? 0;
	}

	public getBoneLocalShearY (boneName: string) {
		return this.getBoneAppliedPose(boneName)?.shearY ?? 0;
	}

	public setBonePose (boneName: string, mode: "game" | "local", applyMode: "once" | "hold", c3X?: number, c3Y?: number, c3Rotation?: number, scaleX?: number, scaleY?: number) {
		if (!this.skeleton) return;
		const bone = this.getBone(boneName);
		if (!bone) throw new Error(`[Spine] Bone not found: ${boneName}`);
		if (applyMode === "hold") {
			const existing = this.bonesOverride.get(bone);
			this.bonesOverride.set(bone, {
				mode,
				x: c3X ?? existing?.x,
				y: c3Y ?? existing?.y,
				rotation: c3Rotation ?? existing?.rotation,
				scaleX: scaleX ?? existing?.scaleX,
				scaleY: scaleY ?? existing?.scaleY,
			});
		} else {
			if (mode === "game") this.updateMatrix();
			this.updateBonePoseOnce(bone, { mode, x: c3X, y: c3Y, rotation: c3Rotation, scaleX, scaleY });
		}
		this.refreshPose();
	}

	public releaseBoneHold (boneName: string, resetToSetup: boolean) {
		if (!this.skeleton) return;
		const bone = this.getBone(boneName);
		if (!bone) throw new Error(`[Spine] Bone not found: ${boneName}`);
		this.bonesOverride.delete(bone);
		if (resetToSetup) bone.setupPose();
		this.refreshPose();
	}

	public setupPose (target: 0 | 1 | 2) {
		const { skeleton } = this;
		if (!skeleton) return;
		if (target === 0) skeleton.setupPose();
		else if (target === 1) skeleton.setupPoseBones();
		else skeleton.setupPoseSlots();
		this.refreshPose();
	}

	public setupBoneSlotPose (type: "bone" | "slot", name: string) {
		if (!this.skeleton) return;
		if (type === "bone") {
			const bone = this.getBone(name);
			if (!bone) throw new Error(`[Spine] Bone not found: ${name}`);
			bone.setupPose();
		} else {
			const slot = this.getSlot(name);
			if (!slot) throw new Error(`[Spine] Slot not found: ${name}`);
			slot.setupPose();
		}
		this.refreshPose();
	}

	private getBone (boneName: string | Bone) {
		if (boneName instanceof spine.Bone) return boneName;

		const { skeleton } = this;
		if (!skeleton) return;

		return skeleton.findBone(boneName);
	}

	/**********/


	/*
	*  Slot, attachments
	*/

	private getSlot (slotName: string | Slot) {
		if (slotName instanceof spine.Slot) return slotName;

		const { skeleton } = this;
		if (!skeleton) return;

		return skeleton.findSlot(slotName);
	}

	public setAttachment (slotName: string, attachmentName: string | null) {
		if (!this.skeleton) return;
		this.skeleton.setAttachment(slotName, attachmentName);
		this.refreshPose();
	}

	/**********/

	/*
	*  Skeleton
	*/

	public mirror (isMirrored: boolean) {
		if ((this.width < 0) !== isMirrored) {
			this.width = -this.width;
			this.refreshPose();
		}
	}

	public flip (isFlipped: boolean) {
		if ((this.height < 0) !== isFlipped) {
			this.height = -this.height;
			this.refreshPose();
		}
	}

	public setPhysicsMode (mode: 0 | 1 | 2 | 3) {
		switch (mode) {
			case 0: this.physicsMode = spine.Physics.none; break;
			case 1: this.physicsMode = spine.Physics.reset; break;
			case 2: this.physicsMode = spine.Physics.update; break;
			case 3: this.physicsMode = spine.Physics.pose; break;
			default: throw new Error(`[Spine] Invalid physics mode: ${mode}`);
		}
		this.refreshPose();
	}

	/**********/

	/*
	*  Bounds
	*/

	public setBounds (x: number, y: number, width: number, height: number) {
		if (!spine.isValidBounds({ x, y, width, height }))
			throw new Error("[Spine] Bounds values must be finite and dimensions must be positive.");

		const scaleX = (Math.abs(this.width) / this.spineBounds.width) * this.propScaleX;
		const scaleY = (Math.abs(this.height) / this.spineBounds.height) * this.propScaleY;

		this.spineBounds = { x, y, width, height };

		this.setOrigin(-x / width, 1 + y / height);

		this.width = width * scaleX * (this.width < 0 ? -1 : 1);
		this.height = height * scaleY * (this.height < 0 ? -1 : 1);
		this.propScaleX = 1;
		this.propScaleY = 1;

		this.refreshPose();
	}

	public getBounds () {
		return { ...this.spineBounds };
	}

	public setBoundsForSkinAnimation (skins: string[], animation: string) {
		if (!this.skeleton || !this.state) {
			console.warn('[Spine] setBoundsForSkinAnimation: skeleton or state not loaded');
			return;
		}
		for (const skinName of skins)
			if (!this.skeleton.data.findSkin(skinName)) throw new Error(`[Spine] Skin not found: ${skinName}`);
		if (animation && !this.skeleton.data.findAnimation(animation))
			throw new Error(`[Spine] Animation not found: ${animation}`);

		const boundsProvider = new spine.SkinsAndAnimationBoundsProvider(
			animation || undefined,
			skins
		);

		const bounds = boundsProvider.calculateBounds(this);

		if (!spine.isValidBounds(bounds)) {
			console.warn('[Spine] setBoundsForSkinAnimation: calculated bounds are invalid');
			return;
		}

		const yUp = -(bounds.y + bounds.height);

		this.setBounds(bounds.x, yUp, bounds.width, bounds.height);
	}

	public setBoundsForSetupPose () {
		if (!this.skeleton) {
			console.warn('[Spine] setBoundsForSetupPose: skeleton not loaded');
			return;
		}

		const boundsProvider = new spine.SetupPoseBoundsProvider();
		const bounds = boundsProvider.calculateBounds(this);

		if (!spine.isValidBounds(bounds)) {
			console.warn('[Spine] setBoundsForSetupPose: calculated bounds are invalid');
			return;
		}

		const yUp = -(bounds.y + bounds.height);

		this.setBounds(bounds.x, yUp, bounds.width, bounds.height);
	}

	/**********/

};

C3.Plugins.EsotericSoftware_SpineConstruct3.Instance = SpineC3Instance;

export type { SpineC3Instance as SDKInstanceClass };
