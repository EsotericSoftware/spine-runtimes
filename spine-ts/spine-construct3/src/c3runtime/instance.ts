import type { AnimationState, AnimationStateListener, AssetLoader, Bone, C3Matrix, C3RendererRuntime, Event, NumberArrayLike, RegionAttachment, Skeleton, Skin, Slot, TextureAtlas, } from "@esotericsoftware/spine-construct3-lib";

const C3 = globalThis.C3;
const spine = globalThis.spine;

spine.Skeleton.yDown = true;

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
	propDebugSkeleton = false;

	isFlippedX = false;
	isPlaying = false;
	animationSpeed = 1.0;
	physicsMode = spine.Physics.update;
	customSkins: Record<string, Skin> = {};

	textureAtlas?: TextureAtlas;
	renderer?: IRenderer;
	atlasLoaded = false;
	atlasLoading = false;
	skeletonLoaded = false;
	skeletonLoading = false;
	skeleton?: Skeleton;
	state?: AnimationState;

	public triggeredEventTrack = -1;
	public triggeredEventAnimation = "";
	public triggeredEventName = "";
	public triggeredEventData?: Event;

	private assetLoader: AssetLoader;
	private skeletonRenderer?: C3RendererRuntime;
	private matrix: C3Matrix;

	private verticesTemp = spine.Utils.newFloatArray(2 * 1024);

	private boneFollowers = new Map<string, { uid: number, offsetX: number, offsetY: number, offsetAngle: number }>();

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
			console.log(properties);
			this.propAtlas = properties[0] as string;
			this.propSkel = properties[1] as string;
			this.propLoaderScale = properties[2] as number;
			const skinProp = properties[3] as string;
			this.propSkin = skinProp === "" ? [] : skinProp.split(",");
			this.propAnimation = properties[4] as string;
			this.propDebugSkeleton = properties[5] as boolean;

			this.propOffsetX = properties[8] as number;
			this.propOffsetY = properties[9] as number;
			this.propOffsetAngle = properties[10] as number;
			this.propScaleX = properties[11] as number;
			this.propScaleY = properties[12] as number;
		}

		this.assetLoader = new spine.AssetLoader();
		this.matrix = new spine.C3Matrix();

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

	_tick (): void {
		const { renderer } = this;

		if (!renderer) return;

		if (!this.atlasLoaded) {
			this.loadAtlas();
			return;
		}

		if (!this.skeletonLoaded) {
			this.loadSkeleton();
			return;
		}

		this.matrix.update(
			this.x + this.propOffsetX,
			this.y + this.propOffsetY,
			this.angle + this.propOffsetAngle);

		if (this.isPlaying) {
			this.update(this.dt);
			this.runtime.sdk.updateRender();
		}
	}

	private fromUpdate = false;

	private update (delta: number) {
		const { state, skeleton, animationSpeed, physicsMode, matrix } = this;

		if (!skeleton || !state) return;

		const adjustedDelta = delta * animationSpeed;
		state.update(adjustedDelta);
		skeleton.update(adjustedDelta);
		state.apply(skeleton);

		this.updateHandles(skeleton, matrix);

		skeleton.updateWorldTransform(physicsMode);

		this.updateBoneFollowers(matrix);

		this.fromUpdate = true;
	}

	_draw (renderer: IRenderer) {
		this.renderer ||= renderer;

		if (!this.isVisible) return;
		if (!this.isOnScreen) return;

		const { skeleton } = this;
		if (!skeleton) return;

		this.skeletonRenderer ||= new spine.C3RendererRuntime(renderer, this.matrix);
		this.skeletonRenderer.draw(skeleton, this.colorRgb, this.opacity, this.isPlaying, this.fromUpdate);
		this.fromUpdate = false;

		if (this.propDebugSkeleton) this.skeletonRenderer.drawDebug(skeleton, this.x, this.y, this.getBoundingQuad(false));
		this.renderDragHandles();
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
	public addDragHandle (type: 0 | 1, name: string, radius = 10, debug = false) {
		if (type === 0) {
			const bone = this.getBone(name);
			if (!bone) return;
			this.dragHandles.add({ bone, debug, radius, offsetX: 0, offsetY: 0 });
		} else {
			const slot = this.getSlot(name);
			if (!slot) return;
			this.dragHandles.add({ slot, bone: slot.bone, debug, radius, offsetX: 0, offsetY: 0 });
		}

		if (this.dragHandles.size === 1) {
			this.touchDown = false;
			this.runtime.addEventListener("pointerdown", this.dragHandleDown);
			this.runtime.addEventListener("pointermove", this.dragHandleMove);
			this.runtime.addEventListener("pointerup", this.dragHandleUp);
		}
		this.isPlaying = true;
	}

	private dragHandleDown = (event: ConstructPointerEvent) => {
		if (event.button !== 0) return;
		this.touchDown = true;
		this.touchX = event.clientX;
		this.touchY = event.clientY;
	};

	private dragHandleMove = (event: ConstructPointerEvent) => {
		if (!this.touchDown) return;
		this.touchX = event.clientX;
		this.touchY = event.clientY;
	}

	private dragHandleUp = (event: ConstructPointerEvent) => {
		if (event.button === 0) this.touchDown = false;
	}

	private dragHandleDispose () {
		this.runtime.removeEventListener("pointerdown", this.dragHandleDown);
		this.runtime.removeEventListener("pointermove", this.dragHandleMove);
		this.runtime.removeEventListener("pointerup", this.dragHandleUp);
	}

	public removeDragHandle (type: 0 | 1, name: string) {
		if (type === 0) {
			const bone = this.getBone(name);
			if (!bone) return;

			for (const handle of this.dragHandles) {
				if (handle.bone === bone && !handle.slot) {
					this.dragHandles.delete(handle);
					break;
				}
			}
		} else {
			const slot = this.getSlot(name);
			if (!slot) return;

			for (const handle of this.dragHandles) {
				if (handle.slot === slot) {
					this.dragHandles.delete(handle);
					break;
				}
			}
		}

		if (this.dragHandles.size === 0) this.dragHandleDispose();
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
				const pose = bone.pose;
				if (bone.parent) {
					const { x, y } = matrix.gameToBone(touchX - handleObject.offsetX, touchY - handleObject.offsetY, bone);
					pose.x = x;
					pose.y = y;
				} else {
					const { x, y } = matrix.gameToSkeleton(touchX - handleObject.offsetX, touchY - handleObject.offsetY);
					pose.x = x / skeleton.scaleX;
					pose.y = -y / skeleton.scaleY * spine.Skeleton.yDir;
				}
			} else if (!this.prevLeftClickDown) {
				const applied = bone.applied;
				const { x, y } = matrix.gameToSkeleton(touchX, touchY);
				const inside = handleObject.slot
					? this.isInsideSlot(x, y, handleObject.slot, true)
					: this.inRadius(x, y, applied.worldX, applied.worldY, handleObject.radius);
				if (inside) {
					handleObject.dragging = true;
					handleObject.offsetX = x - applied.worldX;
					handleObject.offsetY = y - applied.worldY;
				}
			}
		}

		this.prevLeftClickDown = true;
	}

	public isInsideSlot (x: number, y: number, slotName: string | Slot, skeletonCoordinate = false) {
		const slot = this.getSlot(slotName);
		if (!slot || !slot.bone.active) return false;

		const attachment = slot.applied.attachment;
		if (!(attachment instanceof spine.RegionAttachment || attachment instanceof spine.MeshAttachment)) return false;

		const vertices = this.verticesTemp;
		let hullLength = 8;

		if (attachment instanceof spine.RegionAttachment) {
			const regionAttachment = <RegionAttachment>attachment;
			regionAttachment.computeWorldVertices(slot, vertices, 0, 2);
		} else if (attachment instanceof spine.MeshAttachment) {
			attachment.computeWorldVertices(this.skeleton as Skeleton, slot, 0, attachment.worldVerticesLength, vertices, 0, 2);
			hullLength = attachment.hullLength;
		}

		if (skeletonCoordinate) return this.isPointInPolygon(vertices, hullLength, x, y);

		const coords = this.matrix.gameToSkeleton(x, y);
		return this.isPointInPolygon(vertices, hullLength, coords.x, coords.y);
	}

	private isPointInPolygon (vertices: NumberArrayLike, hullLength: number, px: number, py: number) {
		if (hullLength < 6) {
			throw new Error("A polygon must have at least 3 vertices (6 numbers in the array). ");
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
		return {
			// data to be saved for savegames
		};
	}

	_loadFromJson (o: JSONValue) {
		// load state for savegames
	}

	_release () {
		super._release();
		this.assetLoader.releaseInstanceResources(this.propSkel, this.propAtlas, this.propLoaderScale);
		this.textureAtlas = undefined;
		this.renderer = undefined;
		this.skeleton = undefined;
		this.state = undefined;
		this.dragHandleDispose();
	}

	/**********/

	/*
	*  Spine Internals
	*/

	private async loadAtlas () {
		if (this.atlasLoading || !this.renderer) return;
		this.atlasLoading = true;

		const textureAtlas = await this.assetLoader.loadAtlasRuntime(this.propAtlas, this.plugin.runtime, this.renderer);
		if (!textureAtlas) return;

		this.textureAtlas = textureAtlas;
		this.atlasLoaded = true;
	}

	private async loadSkeleton () {
		if (this.skeletonLoading || !this.atlasLoaded) return;
		this.skeletonLoading = true;

		const propValue = this.propSkel;

		if (this.atlasLoaded && this.textureAtlas) {
			const skeletonData = await this.assetLoader.loadSkeletonRuntime(propValue, this.textureAtlas, this.propLoaderScale, this.plugin.runtime);
			if (!skeletonData) return;

			this.skeleton = new spine.Skeleton(skeletonData);
			const animationStateData = new spine.AnimationStateData(skeletonData);
			this.state = new spine.AnimationState(animationStateData);

			if (this.propAnimation) {
				this.setAnimation(0, this.propAnimation, true);
				this.isPlaying = true;
			}

			this._setSkin();

			this.skeleton.scaleX = this.isFlippedX ? -this.propScaleX : this.propScaleX;
			this.skeleton.scaleY = this.propScaleY;

			this.update(0);
			this.runtime.sdk.updateRender();

			this.skeletonLoaded = true;
			this._trigger(C3.Plugins.EsotericSoftware_SpineConstruct3.Cnds.OnSkeletonLoaded);
		}
	}
	/**********/

	/*
	*  Animations
	*/

	public setAnimation (track: number, animation: string, loop = false) {
		const trackEntry = this.state?.setAnimation(track, animation, loop);
		if (!trackEntry) return;
		trackEntry.listener = this.makeTrackListener(track, animation);
	}

	public addAnimation (track: number, animation: string, loop = false, delay = 0) {
		const trackEntry = this.state?.addAnimation(track, animation, loop, delay);
		if (!trackEntry) return;
		trackEntry.listener = this.makeTrackListener(track, animation);
	}

	public setEmptyAnimation (track: number, mixDuration = 0) {
		this.state?.setEmptyAnimation(track, mixDuration);
	}

	public getCurrentAnimation (trackIndex: number): string {
		if (!this.skeleton) return "";

		const { state } = this;
		if (!state) return "";

		const track = state.tracks[trackIndex];
		if (!track || !track.animation) return "";

		return track.animation.name;
	}

	public setAnimationSpeed (speed: number) {
		this.animationSpeed = speed;
	}

	public setAnimationTime (units: 0 | 1, time: number, track: number) {
		if (!this.state) return;

		const trackEntry = this.state.tracks[track];
		if (!trackEntry) return;

		if (units === 0) {
			if (time < trackEntry.animationStart || time > trackEntry.animationEnd) {
				console.warn(`[Spine] Animation time ${time} is out of bounds [${trackEntry.animationStart}, ${trackEntry.animationEnd}]`);
				return;
			}
			trackEntry.trackTime = time;
		} else {
			if (time < 0 || time > 1) {
				console.warn(`[Spine] Animation time ratio ${time} is out of bounds [0, 1]`);
				return;
			}
			trackEntry.trackTime = time * (trackEntry.animationEnd - trackEntry.animationStart);
		}
	}

	public setAnimationMix (fromName: string, toName: string, duration: number) {
		const stateData = this.state?.data;
		if (!stateData) return;

		try {
			stateData.setMix(fromName, toName, duration);
		} catch (error) {
			console.error('[Spine] setAnimationMix error:', error);
		}
	}

	public setTrackAlpha (alpha: number, trackIndex: number) {
		const { state } = this;
		if (!state) {
			console.warn('[Spine] setAlpha: no state');
			return;
		}

		const track = state.tracks[trackIndex];
		if (!track) {
			console.warn(`[Spine] setAlpha: track ${trackIndex} not found`);
			return;
		}

		track.alpha = spine.MathUtils.clamp(0, 1, alpha);
	}

	public setTrackMixBlend (mixBlend: 0 | 1 | 2 | 3, trackIndex: number) {
		const { state } = this;
		if (!state) {
			console.warn('[Spine] setMixBlend: no state');
			return;
		}

		const track = state.tracks[trackIndex];
		if (!track) {
			console.warn(`[Spine] setMixBlend: track ${trackIndex} not found`);
			return;
		}

		switch (mixBlend) {
			case 0: track.mixBlend = spine.MixBlend.setup; break;
			case 1: track.mixBlend = spine.MixBlend.first; break;
			case 2: track.mixBlend = spine.MixBlend.replace; break;
			case 3: track.mixBlend = spine.MixBlend.add; break;
			default: console.warn('[Spine] Invalid mix blend mode:', mixBlend);
		}
	}

	private triggetAnimationEvent (eventName: string, track: number, animation: string, event?: Event) {
		this.triggeredEventTrack = track;
		this.triggeredEventAnimation = animation;
		this.triggeredEventName = eventName;
		this.triggeredEventData = event;
		this._trigger(C3.Plugins.EsotericSoftware_SpineConstruct3.Cnds.OnAnimationEvent);
	}

	private makeTrackListener = (track: number, animation: string): AnimationStateListener => ({
		start: () => this.triggetAnimationEvent("start", track, animation),
		dispose: () => this.triggetAnimationEvent("dispose", track, animation),
		event: (_, event) => this.triggetAnimationEvent("event", track, animation, event),
		interrupt: () => this.triggetAnimationEvent("interrupt", track, animation),
		end: () => this.triggetAnimationEvent("end", track, animation),
		complete: () => this.triggetAnimationEvent("complete", track, animation),
	})

	/**********/

	/*
	*  Skins
	*/

	public setSkin (skins: string[]) {
		this.propSkin = skins;
		this._setSkin();
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
			skeleton.skin = null;
		} else if (skins.length === 1) {
			const skinName = skins[0];
			const skin = skeleton.data.findSkin(skinName);
			if (!skin) {
				// TODO: signal error
				return;
			}
			skeleton.setSkin(skins[0]);
		} else {
			const customSkin = new spine.Skin(skins.join(","));
			for (const s of skins) {
				const skin = skeleton.data.findSkin(s)
				if (!skin) {
					// TODO: signal error
					return;
				}
				customSkin.addSkin(skin);
			}
			skeleton.setSkin(customSkin);
		}

		skeleton.setupPose();
	}

	public createCustomSkin (skinName: string) {
		if (!this.skeleton) return;

		if (this.customSkins[skinName])
			this.customSkins[skinName].clear();
		else
			this.customSkins[skinName] = new spine.Skin(skinName);
	}

	public addCustomSkin (customSkinName: string, skinToAddName: string) {
		if (!this.skeleton) return;

		if (!this.customSkins[customSkinName]) {
			console.warn(`[Spine] Custom skin "${customSkinName}" does not exist. Create it first.`);
			return;
		}

		const skinToAdd = this.skeleton.data.findSkin(skinToAddName);
		if (!skinToAdd) {
			console.warn(`[Spine] Skin "${skinToAddName}" not found in skeleton data.`);
			return;
		}

		this.customSkins[customSkinName].addSkin(skinToAdd);
	}

	public setCustomSkin (skinName: string) {
		if (!this.skeleton) return;

		if (!this.customSkins[skinName]) {
			console.warn(`[Spine] Custom skin "${skinName}" does not exist.`);
			return;
		}

		this.skeleton.setSkin(this.customSkins[skinName]);
		this.skeleton.setupPose();
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
	}

	public setSlotColor (slotName: string, color: string) {
		const { skeleton } = this;
		if (!skeleton) {
			console.warn('[Spine] setSlotColor: no skeleton');
			return;
		}

		const slot = skeleton.findSlot(slotName);
		if (!slot) {
			console.warn(`[Spine] setSlotColor: slot not found: ${slotName}`);
			return;
		}

		slot.pose.color.setFromString(color);
	}

	public resetSlotColors (slotName: string = "") {
		const { skeleton } = this;
		if (!skeleton) {
			console.warn('[Spine] resetSlotColors: no skeleton');
			return;
		}

		if (slotName === "") {
			for (const slot of skeleton.slots)
				slot.pose.color.setFromColor(slot.data.setup.color);
		} else {
			const slot = skeleton.findSlot(slotName);
			if (!slot) {
				console.warn(`[Spine] resetSlotColors: slot not found: ${slotName}`);
				return;
			}
			slot.pose.color.setFromColor(slot.data.setup.color);
		}
	}

	/**********/

	/*
	*  Bone follower
	*/

	public attachInstanceToBone (uid: number, boneName: string, offsetX = 0, offsetY = 0, offsetAngle = 0) {
		if (!this.skeleton) return;

		const bone = this.skeleton.findBone(boneName);
		if (!bone) {
			console.warn(`[Spine] attachInstanceToBone: bone not found: ${boneName}`);
			return;
		}

		this.boneFollowers.set(boneName, { uid, offsetX, offsetY, offsetAngle });
		this.isPlaying = true;
	}

	public detachInstanceFromBone (boneName: string) {
		this.boneFollowers.delete(boneName);
	}

	private updateBoneFollowers (matrix: C3Matrix) {
		if (this.boneFollowers.size === 0) return;

		for (const [boneName, follower] of this.boneFollowers) {
			const bone = this.skeleton?.findBone(boneName);
			if (!bone) continue;

			const instance = this.runtime.getInstanceByUid(follower.uid) as IWorldInstance;
			if (!instance) continue;

			const { x, y } = matrix.boneToGame(bone);
			const boneRotation = bone.applied.getWorldRotationX();

			// Apply rotation to offset
			const rotationRadians = boneRotation * Math.PI / 180;
			const cos = Math.cos(rotationRadians);
			const sin = Math.sin(rotationRadians);
			const rotatedOffsetX = follower.offsetX * cos - follower.offsetY * sin;
			const rotatedOffsetY = follower.offsetX * sin + follower.offsetY * cos;

			instance.x = x + rotatedOffsetX;
			instance.y = y + rotatedOffsetY;
			instance.angleDegrees = boneRotation + follower.offsetAngle;
		}
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

		const x = bone.applied.worldX;
		const y = bone.applied.worldY;
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

		const x = bone.applied.worldX;
		const y = bone.applied.worldY;
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

		const boneRotation = bone.applied.getWorldRotationX();
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

		const point = this.matrix.boneToGame(bone);
		return point.y;
	}

	public updateBonePose (c3X: number, c3Y: number, boneName: string) {
		const bone = this.getBone(boneName);
		if (!bone) return;

		const { x, y } = this.matrix.gameToBone(c3X, c3Y, bone);
		bone.applied.x = x;
		bone.applied.y = y;
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
		this.skeleton?.setAttachment(slotName, attachmentName);
	}

	/**********/

	/*
	*  Skeleton
	*/

	public flipX (isFlippedX: boolean) {
		this.isFlippedX = isFlippedX;

		const { skeleton } = this;
		if (skeleton) {
			skeleton.scaleX = isFlippedX ? -this.propScaleX : this.propScaleX;
		}
	}

	public setPhysicsMode (mode: 0 | 1 | 2 | 3) {
		switch (mode) {
			case 0: this.physicsMode = spine.Physics.none; break;
			case 1: this.physicsMode = spine.Physics.reset; break;
			case 2: this.physicsMode = spine.Physics.update; break;
			case 3: this.physicsMode = spine.Physics.pose; break;
			default: console.warn('[Spine] Invalid physics mode:', mode);
		}
	}

	/**********/

};

C3.Plugins.EsotericSoftware_SpineConstruct3.Instance = SpineC3Instance;

export type { SpineC3Instance as SDKInstanceClass };
