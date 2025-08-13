import { AnimationEventType, type AnimationState, AnimationStateListener, type AssetLoader, BlendingModeSpineToC3, type Event, EventType, type Skeleton, type SkeletonRendererCore, type SpineBoundsProvider, type TextureAtlas, TrackEntry } from "@esotericsoftware/spine-construct3-lib";

const C3 = globalThis.C3;
const spine = globalThis.spine;

spine.Skeleton.yDown = true;

class DrawingInstance extends globalThis.ISDKWorldInstanceBase {
	propAtlas = "";
	propSkel = "";
	propSkin: string[] = [];
	propAnimation?: string;
	propOffsetX = 0;
	propOffsetY = 0;
	propOffsetAngle = 0;
	propScaleX = 1;
	propScaleY = 1;

	textureAtlas?: TextureAtlas;
	renderer?: IRenderer;
	atlasLoaded = false;
	atlasLoading = false;
	skeletonLoaded = false;
	skeletonLoading = false;
	skeleton?: Skeleton;
	state?: AnimationState;

	private assetLoader: AssetLoader;
	private skeletonRenderer: SkeletonRendererCore;

	constructor () {
		super();

		const properties = this._getInitProperties();
		if (properties) {
			this.propAtlas = properties[0] as string;
			this.propSkel = properties[1] as string;
			const skinProp = properties[2] as string;
			this.propSkin = skinProp === "" ? [] : skinProp.split(",");
			this.propAnimation = properties[3] as string;

			this.propOffsetX = properties[7] as number;
			this.propOffsetY = properties[8] as number;
			this.propOffsetAngle = properties[9] as number;
			this.propScaleX = properties[10] as number;
			this.propScaleY = properties[11] as number;
			console.log(properties);
		}

		this.assetLoader = new spine.AssetLoader("runtime");
		this.skeletonRenderer = new spine.SkeletonRendererCore();

		this._setTicking(true);
	}

	_tick (): void {
		const { renderer } = this;

		if (!renderer) return;

		if (!this.atlasLoaded) {
			this._loadAtlas();
			return;
		}

		if (!this.skeletonLoaded) {
			this.loadSkeleton();
			return;
		}

		this.x++;
		this.x--;
		this.update(this.dt);
	}

	private async loadSkeleton () {
		if (this.skeletonLoading) return;
		if (!this.atlasLoaded) return;
		this.skeletonLoading = true;

		const propValue = this.propSkel;

		if (this.atlasLoaded && this.textureAtlas) {
			const skeletonData = await this.assetLoader.loadSkeletonRuntime(propValue, this.textureAtlas, 1, this.plugin.runtime);
			if (!skeletonData) return;

			this.skeleton = new spine.Skeleton(skeletonData);
			const animationStateData = new spine.AnimationStateData(skeletonData);
			this.state = new spine.AnimationState(animationStateData);

			if (this.propAnimation) {
				this.setAnimation(0, this.propAnimation, true);
			}

			this._setSkin();

			this.update(0);

			// Initially, width and height are values set on C3 Editor side that allows to determine the right scale
			this.skeleton.scaleX = this.propScaleX;
			this.skeleton.scaleY = this.propScaleY;

			// this.setSize(this._spineBounds.width * this.skeleton.scaleX, this._spineBounds.height * -this.skeleton.scaleY);
			// this.setOrigin(-this._spineBounds.x * this.skeleton.scaleX / this.width, this._spineBounds.y * this.skeleton.scaleY / this.height);

			this.skeletonLoaded = true;
			this._trigger(C3.Plugins.EsotericSoftware_SpineConstruct3.Cnds.OnSkeletonLoaded);
		}
	}

	public triggeredEventTrack = -1;
	public triggeredEventAnimation = "";
	public triggeredEventName = "";
	public triggeredEventData?: Event;

	private triggetAnimationEvent (eventName: string, track: number, animation: string, event?: Event) {
		this.triggeredEventTrack = track;
		this.triggeredEventAnimation = animation;
		this.triggeredEventName = eventName;
		this.triggeredEventData = event;
		this._trigger(C3.Plugins.EsotericSoftware_SpineConstruct3.Cnds.OnAnimationEvent);
	}
	public setAnimation (track: number, animation: string, loop = false) {
		const trackEntry = this.state?.setAnimation(track, animation, loop);
		if (!trackEntry) return;

		trackEntry.listener = {
			start: () => {
				this.triggetAnimationEvent("start", track, animation);
			},
			dispose: () => {
				this.triggetAnimationEvent("dispose", track, animation);
			},
			event: (_, event) => {
				this.triggetAnimationEvent("event", track, animation, event);
			},
			interrupt: () => {
				this.triggetAnimationEvent("interrupt", track, animation);
			},
			end: () => {
				this.triggetAnimationEvent("end", track, animation);
			},
			complete: () => {
				this.triggetAnimationEvent("complete", track, animation);
			},
		}
	}

	public setSkin (skins: string[]) {
		this.propSkin = skins;
		this._setSkin();
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
		this.update(0);
	}

	private update (delta: number) {
		const { state, skeleton } = this;

		if (!skeleton || !state) return;

		state.update(delta);
		skeleton.update(delta);
		state.apply(skeleton);
		skeleton.updateWorldTransform(spine.Physics.update);
	}

	private async _loadAtlas () {
		if (this.atlasLoading || !this.renderer) return;
		this.atlasLoading = true;

		const textureAtlas = await this.assetLoader.loadAtlasRuntime(this.propAtlas, this.plugin.runtime, this.renderer);
		if (!textureAtlas) return;

		this.textureAtlas = textureAtlas;
		this.atlasLoaded = true;
	}

	_release () {
		super._release();
	}

	private tempVertices = new Float32Array(4096);
	private tempColors = new Float32Array(4096);
	_draw (renderer: IRenderer) {
		this.renderer ||= renderer;

		if (!this.isVisible) return;
		if (!this.isOnScreen) return;
		if (!this.skeleton) return;

		let command = this.skeletonRenderer.render(this.skeleton);
		const inv255 = 1 / 255;
		const offsetX = this.x + this.propOffsetX;
		const offsetY = this.y + this.propOffsetY;
		const offsetAngle = this.angle + this.propOffsetAngle;

		const cos = Math.cos(offsetAngle);
		const sin = Math.sin(offsetAngle);
		while (command) {
			const { numVertices, positions, uvs, colors, indices, numIndices, blendMode } = command;

			const vertices = this.tempVertices;
			const c3colors = this.tempColors;
			for (let i = 0; i < numVertices; i++) {
				const srcIndex = i * 2;
				const dstIndex = i * 3;
				const x = positions[srcIndex];
				const y = positions[srcIndex + 1];
				vertices[dstIndex] = x * cos - y * sin + offsetX;
				vertices[dstIndex + 1] = x * sin + y * cos + offsetY;
				vertices[dstIndex + 2] = 0;

				// there's something wrong with the hand after adding the colors on spineboy portal animation
				const color = colors[i];
				const colorDst = i * 4;
				c3colors[colorDst] = (color >>> 16 & 0xFF) * inv255;
				c3colors[colorDst + 1] = (color >>> 8 & 0xFF) * inv255;
				c3colors[colorDst + 2] = (color & 0xFF) * inv255;
				c3colors[colorDst + 3] = (color >>> 24 & 0xFF) * inv255 * this.opacity;
			}

			renderer.setTexture(command.texture.texture);
			renderer.setBlendMode(spine.BlendingModeSpineToC3[blendMode]);
			renderer.drawMesh(
				vertices.subarray(0, numVertices * 3),
				uvs.subarray(0, numVertices * 2),
				indices.subarray(0, numIndices),
				c3colors.subarray(0, numVertices * 4),
			);

			command = command.next;
		}

	}

	_saveToJson () {
		return {
			// data to be saved for savegames
		};
	}

	_loadFromJson (o: JSONValue) {
		// load state for savegames
	}

	_setTestProperty (n: number) {
		// this._testProperty = n;
	}

	_getTestProperty () {
		// return this._testProperty;
	}
};

C3.Plugins.EsotericSoftware_SpineConstruct3.Instance = DrawingInstance;

export type { DrawingInstance as SDKInstanceClass };
