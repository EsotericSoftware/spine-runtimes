import { AnimationEventType, AnimationStateListener, BlendingModeSpineToC3, EventType, TrackEntry, type AnimationState, type AssetLoader, type Event, type Skeleton, type SkeletonRendererCore, type SpineBoundsProvider, type TextureAtlas } from "@esotericsoftware/spine-construct3-lib";

const C3 = globalThis.C3;
const spine = globalThis.spine;

spine.Skeleton.yDown = true;

class DrawingInstance extends globalThis.ISDKWorldInstanceBase {
	atlasProp = "";
	skelProp = "";
	skinProp: string[] = [];
	animationProp?: string;

	textureAtlas?: TextureAtlas;
	renderer?: IRenderer;
	atlasLoaded = false;
	atlasLoading = false;
	skeletonLoaded = false;
	skeletonLoading = false;
	skeleton?: Skeleton;
	state?: AnimationState;

	private ratio: number;

	private assetLoader: AssetLoader;
	private skeletonRenderer: SkeletonRendererCore;
	private spineBoundsProvider: SpineBoundsProvider;
	private _spineBounds?: {
		x: number;
		y: number;
		width: number;
		height: number;
	};

	constructor () {
		super();

		const properties = this._getInitProperties();
		if (properties) {
			this.atlasProp = properties[0] as string;
			this.skelProp = properties[1] as string;
			const skinProp = properties[2] as string;
			this.skinProp = skinProp === "" ? [] : skinProp.split(",");
			this.animationProp = properties[3] as string;
			console.log(properties);
		}

		this.assetLoader = new spine.AssetLoader("runtime");
		this.skeletonRenderer = new spine.SkeletonRendererCore();

		if (this.animationProp || (this.skinProp && this.skinProp.length > 0)) {
			this.spineBoundsProvider = new spine.SkinsAndAnimationBoundsProvider(this.animationProp, this.skinProp);
		} else {
			this.spineBoundsProvider = new spine.SetupPoseBoundsProvider();
		}

		this.ratio = this.width / this.height;

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

		const propValue = this.skelProp;

		if (this.atlasLoaded && this.textureAtlas) {
			const skeletonData = await this.assetLoader.loadSkeletonRuntime(propValue, this.textureAtlas, 0.25, this.plugin.runtime);
			if (!skeletonData) return;

			this.skeleton = new spine.Skeleton(skeletonData);
			const animationStateData = new spine.AnimationStateData(skeletonData);
			this.state = new spine.AnimationState(animationStateData);

			if (this.animationProp) {
				this.setAnimation(0, this.animationProp, true);
			}

			this._setSkin();

			this.update(0);

			this._spineBounds = this.spineBoundsProvider.calculateBounds(this);

			// Initially, width and height are values set on C3 Editor side that allows to determine the right scale
			this.skeleton.scaleX = this.width / this._spineBounds.width;
			this.skeleton.scaleY = this.height / this._spineBounds.height;

			this.setSize(this._spineBounds.width * this.skeleton.scaleX, this._spineBounds.height * -this.skeleton.scaleY);
			this.setOrigin(-this._spineBounds.x * this.skeleton.scaleX / this.width, this._spineBounds.y * this.skeleton.scaleY / this.height);


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
		this.skinProp = skins;
		this._setSkin();
	}

	private _setSkin () {
		const { skeleton } = this;
		if (!skeleton) return;

		const skins = this.skinProp;

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

		const textureAtlas = await this.assetLoader.loadAtlasRuntime(this.atlasProp, this.plugin.runtime, this.renderer);
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
		while (command) {
			const { numVertices, positions, uvs, colors, indices, numIndices, blendMode } = command;

			const vertices = this.tempVertices;
			const c3colors = this.tempColors;
			for (let i = 0; i < numVertices; i++) {
				const srcIndex = i * 2;
				const dstIndex = i * 3;
				vertices[dstIndex] = positions[srcIndex] + this.x;
				vertices[dstIndex + 1] = positions[srcIndex + 1] + this.y;
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
