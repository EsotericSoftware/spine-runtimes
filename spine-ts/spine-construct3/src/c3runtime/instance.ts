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

import type { AnimationState, AssetLoader, Bone, BoneLocal, C3Matrix, C3RendererRuntime, Event, NumberArrayLike, Skeleton, Skin, Slot, SpineBoundsProvider, SpineBoundsProviderType, TextureAtlas, } from "@esotericsoftware/spine-construct3-lib";

const C3 = globalThis.C3;
const spine = globalThis.spine;

spine.Skeleton.yDown = true;

type BoneOverride = Partial<BoneLocal> & { mode: "game" | "local" };
type BoneFollower = { uid: number, offsetX: number, offsetY: number, offsetAngle: number };

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
	propBoundsProvider: SpineBoundsProviderType = "setup";
	propEnableCollision = false;

	isMirrored = false;
	isFlipped = false;
	collisionSpriteInstance?: IWorldInstance;
	collisionSpriteClassName = "";
	isPlaying = true;
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
	public triggeredEventData?: Event & { track: number, animation: string };

	private assetLoader: AssetLoader;
	private skeletonRenderer?: C3RendererRuntime;
	private matrix: C3Matrix;
	private requestRedraw = false;

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
			this.propAnimation = properties[4] as string;
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
		}

		this.collisionSpriteClassName = `${this.objectType.name}_CollisionBody`;

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
			this.totalZ,
			this.angle + this.propOffsetAngle,
			this.width / this.spineBounds.width * this.propScaleX,
			this.height / this.spineBounds.height * this.propScaleY);

		this.updateCollisionSprite();

		if (this.isPlaying) this.update(this.dt);
	}

	private update (delta: number) {
		const { state, skeleton, animationSpeed, physicsMode, matrix } = this;

		if (!skeleton || !state) return;

		const adjustedDelta = delta * animationSpeed;
		state.update(adjustedDelta);
		skeleton.update(adjustedDelta);
		state.apply(skeleton);

		this.updateHandles(skeleton, matrix);

		this.updateBonesOverride();

		skeleton.updateWorldTransform(physicsMode);

		this.updateBoneFollowers(matrix);

		this.runtime.sdk.updateRender();
		this.requestRedraw = true;
	}

	_draw (renderer: IRenderer) {
		this.renderer ||= renderer;

		if (!this.isVisible) return;
		if (!this.isOnScreen) return;

		const { skeleton } = this;
		if (!skeleton) return;

		this.skeletonRenderer ||= new spine.C3RendererRuntime(renderer, this.matrix);
		this.skeletonRenderer.draw(skeleton, this.colorRgb, this.opacity, this.requestRedraw);
		this.requestRedraw = false;

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
	};

	private dragHandleUp = (event: ConstructPointerEvent) => {
		if (event.button === 0) this.touchDown = false;
	};

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
					pose.x = x;
					pose.y = -y * spine.Skeleton.yDir;
				}
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

		const attachment = slot.applied.attachment;
		if (!(attachment instanceof spine.RegionAttachment || attachment instanceof spine.MeshAttachment)) return false;

		const vertices = this.verticesTemp;
		let hullLength = 8;

		if (attachment instanceof spine.RegionAttachment) {
			attachment.computeWorldVertices(slot, vertices, 0, 2);
		} else if (attachment instanceof spine.MeshAttachment) {
			attachment.computeWorldVertices(this.skeleton as Skeleton, slot, 0, attachment.worldVerticesLength, vertices, 0, 2);
			hullLength = attachment.hullLength;
		}

		if (skeletonCoordinate) return this.isPointInPolygon(vertices, hullLength, x, y);

		const coords = this.matrix.gameToSkeleton(x, y);
		return this.isPointInPolygon(vertices, hullLength, coords.x, coords.y);
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

		const track = this.state.tracks[trackIndex];
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

		if (this.collisionSpriteInstance) {
			this.collisionSpriteInstance.destroy();
			this.collisionSpriteInstance = undefined;
		}
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
			this.state.addListener({
				start: (entry) => this.triggerAnimationEvent("start", entry.trackIndex, entry.animation?.name ?? ""),
				dispose: (entry) => this.triggerAnimationEvent("dispose", entry.trackIndex, entry.animation?.name ?? ""),
				event: (entry, event) => this.triggerAnimationEvent("event", entry.trackIndex, entry.animation?.name ?? "", event),
				interrupt: (entry) => this.triggerAnimationEvent("interrupt", entry.trackIndex, entry.animation?.name ?? ""),
				end: (entry) => this.triggerAnimationEvent("end", entry.trackIndex, entry.animation?.name ?? ""),
				complete: (entry) => this.triggerAnimationEvent("complete", entry.trackIndex, entry.animation?.name ?? ""),
			});

			if (this.propAnimation) this.setAnimation(0, this.propAnimation, true);

			this._setSkin();

			this.calculateBounds();

			this.update(0);

			this.createCollisionSprite();

			this.skeletonLoaded = true;
			this._trigger(C3.Plugins.EsotericSoftware_SpineConstruct3.Cnds.OnSkeletonLoaded);
		}
	}

	private createCollisionSprite () {
		if (!this.propEnableCollision) return;

		const objectType = (this.runtime.objects as Record<string, IObjectType<IWorldInstance>>)[this.collisionSpriteClassName];
		if (!objectType)
			throw new Error(`[Spine] Collision sprite object type "${this.collisionSpriteClassName}" not found`);

		this.collisionSpriteInstance = objectType.createInstance(this.layer.name, this.x, this.y);
		this.collisionSpriteInstance.setOrigin(this.originX, this.originY);
	}

	private updateCollisionSprite () {
		if (!this.collisionSpriteInstance) return;

		this.collisionSpriteInstance.x = this.x;
		this.collisionSpriteInstance.y = this.y;
		this.collisionSpriteInstance.width = this.width;
		this.collisionSpriteInstance.height = this.height;
		this.collisionSpriteInstance.angleDegrees = this.angleDegrees;
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

		this.spineBounds = boundsProvider.calculateBounds(this);
	}
	/**********/

	/*
	*  Animations
	*/

	public setAnimation (track: number, animation: string, loop = false) {
		const { state } = this;
		if (!state) return;
		state.setAnimation(track, animation, loop);
		this.isPlaying = true;
	}

	public addAnimation (track: number, animation: string, loop = false, delay = 0) {
		const { state } = this;
		if (!state) return;
		state.addAnimation(track, animation, loop, delay);
		this.isPlaying = true;
	}

	public addEmptyAnimation (track: number, mixDuration: number, delay: number) {
		this.state?.addEmptyAnimation(track, mixDuration, delay);
	}

	public setEmptyAnimation (track: number, mixDuration: number) {
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

	public clearTrack (track: number) {
		const { state } = this;
		if (!state) return;
		if (track === -1)
			state.clearTracks();
		else
			state.clearTrack(track);
	}

	private triggerAnimationEvent (eventName: string, track: number, animation: string, event?: Event) {
		this.triggeredEventTrack = track;
		this.triggeredEventAnimation = animation;
		this.triggeredEventName = eventName;
		if (event) this.triggeredEventData = { ...event, track, animation };
		this._trigger(C3.Plugins.EsotericSoftware_SpineConstruct3.Cnds.OnAnimationEvent);
	}

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
			if (!skin) throw new Error(`The given skin is not present in the skeleton data: ${skinName}`);
			skeleton.setSkin(skins[0]);
		} else {
			const customSkin = new spine.Skin(skins.join(","));
			for (const s of skins) {
				const skin = skeleton.data.findSkin(s);
				if (!skin) throw new Error(`The given skin is not present in the skeleton data: ${s}`);
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

		const follower = { uid, offsetX, offsetY, offsetAngle };
		const followers = this.boneFollowers.get(boneName);
		if (!followers) {
			this.boneFollowers.set(boneName, [follower]);
		} else {
			followers.push(follower);
		}

		this.isPlaying = true;
	}

	public detachInstanceFromBoneByUid (uid: number, boneName: string) {
		const followers = this.boneFollowers.get(boneName);
		if (!followers) return;

		const index = followers.findIndex(f => f.uid === uid);
		if (index !== -1) {
			followers.splice(index, 1);
			if (followers.length === 0) {
				this.boneFollowers.delete(boneName);
			}
		}
	}

	public detachAllFromBone (boneName: string) {
		this.boneFollowers.delete(boneName);
	}

	private mirrorFollower (instance: IWorldInstance) {
		instance.setSize(-instance.width, instance.height);
	}

	private flipFollower (instance: IWorldInstance) {
		instance.setSize(instance.width, -instance.height);
	}

	private updateBoneFollowers (matrix: C3Matrix) {
		if (this.boneFollowers.size === 0) return;

		for (const [boneName, followers] of this.boneFollowers) {
			const bone = this.skeleton?.findBone(boneName);
			if (!bone) continue;

			const { x, y } = matrix.boneToGame(bone);
			const boneRotation = bone.applied.getWorldRotationX();

			const rotationRadians = boneRotation * Math.PI / 180;
			const cos = Math.cos(rotationRadians);
			const sin = Math.sin(rotationRadians);

			for (const follower of followers) {
				const instance = this.runtime.getInstanceByUid(follower.uid) as IWorldInstance;
				if (!instance) {
					this.detachInstanceFromBoneByUid(follower.uid, boneName);
					continue;
				}

				const rotatedOffsetX = follower.offsetX * cos - follower.offsetY * sin;
				const rotatedOffsetY = follower.offsetX * sin + follower.offsetY * cos;

				instance.x = x + rotatedOffsetX * (this.isMirrored ? -1 : 1);
				instance.y = y + rotatedOffsetY * (this.isFlipped ? -1 : 1);
				instance.angleDegrees = boneRotation + follower.offsetAngle;
			}
		}
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
				bone.pose.x = locals.x;
				bone.pose.y = locals.y;
			}

			if (rotation !== undefined) bone.pose.rotation = this.matrix.gameToBoneRotation(rotation, bone);
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

	public setBonePose (boneName: string, mode: "game" | "local", applyMode: "once" | "hold", c3X?: number, c3Y?: number, c3Rotation?: number, scaleX?: number, scaleY?: number) {
		const bone = this.getBone(boneName);
		if (!bone) return;
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
			this.updateBonePoseOnce(bone, { mode, x: c3X, y: c3Y, rotation: c3Rotation, scaleX, scaleY });
		}
	}

	public releaseBoneHold (boneName: string, resetToSetup: boolean) {
		const bone = this.getBone(boneName);
		if (!bone) return;
		this.bonesOverride.delete(bone);
		if (resetToSetup) bone.setupPose();
	}

	public setupPose (target: 0 | 1 | 2) {
		const { skeleton } = this;
		if (!skeleton) return;
		if (target === 0) skeleton.setupPose();
		else if (target === 1) skeleton.setupPoseBones();
		else skeleton.setupPoseSlots();
	}

	public setupBoneSlotPose (type: "bone" | "slot", name: string) {
		if (type === "bone") {
			const bone = this.getBone(name);
			if (!bone) return;
			bone.setupPose();
		} else {
			const slot = this.getSlot(name);
			if (!slot) return;
			slot.setupPose();
		}
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

	public mirror (isMirrored: boolean) {
		if (isMirrored !== this.isMirrored) {
			this.isMirrored = isMirrored;
			this.width = -this.width

			for (const [, followers] of this.boneFollowers) {
				for (const follower of followers) {
					const instance = this.runtime.getInstanceByUid(follower.uid) as IWorldInstance;
					if (instance) this.mirrorFollower(instance);
				}
			}

		}
	}

	public flip (isFlipped: boolean) {
		if (isFlipped !== this.isFlipped) {
			this.isFlipped = isFlipped;
			this.height = -this.height;

			for (const [, followers] of this.boneFollowers) {
				for (const follower of followers) {
					const instance = this.runtime.getInstanceByUid(follower.uid) as IWorldInstance;
					if (instance) this.flipFollower(instance);
				}
			}

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
